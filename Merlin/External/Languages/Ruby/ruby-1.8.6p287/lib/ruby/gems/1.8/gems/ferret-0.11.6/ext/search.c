#include <string.h>
#include <limits.h>
#include "search.h"
#include "array.h"

/***************************************************************************
 *
 * Explanation
 *
 ***************************************************************************/

Explanation *expl_new(float value, const char *description, ...)
{
    Explanation *expl = ALLOC(Explanation);

    va_list args;
    va_start(args, description);
    expl->description = vstrfmt(description, args);
    va_end(args);

    expl->value = value;
    expl->details = ary_new_type_capa(Explanation *,
                                      EXPLANATION_DETAILS_START_SIZE);
    return expl;
}

void expl_destroy(Explanation *expl)
{
    ary_destroy((void **)expl->details, (free_ft)expl_destroy);
    free(expl->description);
    free(expl);
}

Explanation *expl_add_detail(Explanation *expl, Explanation *detail)
{
    ary_push(expl->details, detail);
    return expl;
}

char *expl_to_s_depth(Explanation *expl, int depth)
{
    int i;
    char *buffer = ALLOC_N(char, depth * 2 + 1);
    const int num_details = ary_size(expl->details);

    memset(buffer, ' ', sizeof(char) * depth * 2);
    buffer[depth*2] = 0;

    buffer = estrcat(buffer, strfmt("%f = %s\n", expl->value, expl->description));
    for (i = 0; i < num_details; i++) {
        buffer = estrcat(buffer, expl_to_s_depth(expl->details[i], depth + 1));
    }

    return buffer;
}

char *expl_to_html(Explanation *expl)
{
    int i;
    char *buffer;
    const int num_details = ary_size(expl->details);

    buffer = strfmt("<ul>\n<li>%f = %s</li>\n", expl->value, expl->description);

    for (i = 0; i < num_details; i++) {
        estrcat(buffer, expl_to_html(expl->details[i]));
    }

    REALLOC_N(buffer, char, strlen(buffer) + 10);
    return strcat(buffer, "</ul>\n");
}

/***************************************************************************
 *
 * Hit
 *
 ***************************************************************************/

static bool hit_less_than(const Hit *hit1, const Hit *hit2)
{
    if (hit1->score == hit2->score) {
        return hit1->doc > hit2->doc;
    }
    else {
        return hit1->score < hit1->score;
    }
}

static bool hit_lt(Hit *hit1, Hit *hit2)
{
    if (hit1->score == hit2->score) {
        return hit1->doc > hit2->doc;
    }
    else {
        return hit1->score < hit2->score;
    }
}

static void hit_pq_down(PriorityQueue *pq)
{
    register int i = 1;
    register int j = 2;     /* i << 1; */
    register int k = 3;     /* j + 1;  */
    Hit **heap = (Hit **)pq->heap;
    Hit *node = heap[i];    /* save top node */

    if ((k <= pq->size) && hit_lt(heap[k], heap[j])) {
        j = k;
    }

    while ((j <= pq->size) && hit_lt(heap[j], node)) {
        heap[i] = heap[j];  /* shift up child */
        i = j;
        j = i << 1;
        k = j + 1;
        if ((k <= pq->size) && hit_lt(heap[k], heap[j])) {
            j = k;
        }
    }
    heap[i] = node;
}

static Hit *hit_pq_pop(PriorityQueue *pq)
{
    if (pq->size > 0) {
        Hit **heap = (Hit **)pq->heap;
        Hit *result = heap[1];    /* save first value */
        heap[1] = heap[pq->size]; /* move last to first */
        heap[pq->size] = NULL;
        pq->size--;
        hit_pq_down(pq);          /* adjust heap */
        return result;
    }
    else {
        return NULL;
    }
}

static void hit_pq_up(PriorityQueue *pq)
{
    Hit **heap = (Hit **)pq->heap;
    Hit *node;
    int i = pq->size;
    int j = i >> 1;
    node = heap[i];

    while ((j > 0) && hit_lt(node, heap[j])) {
        heap[i] = heap[j];
        i = j;
        j = j >> 1;
    }
    heap[i] = node;
}

static void hit_pq_insert(PriorityQueue *pq, Hit *hit) 
{
    if (pq->size < pq->capa) {
        Hit *new_hit = ALLOC(Hit);
        memcpy(new_hit, hit, sizeof(Hit));
        pq->size++;
        if (pq->size >= pq->mem_capa) {
            pq->mem_capa <<= 1;
            REALLOC_N(pq->heap, void *, pq->mem_capa);
        }
        pq->heap[pq->size] = new_hit;
        hit_pq_up(pq);
    }
    else if (pq->size > 0 && hit_lt((Hit *)pq->heap[1], hit)) {
        memcpy(pq->heap[1], hit, sizeof(Hit));
        hit_pq_down(pq);
    }
}

static void hit_pq_multi_insert(PriorityQueue *pq, Hit *hit) 
{
    hit_pq_insert(pq, hit);
    free(hit);
}

/***************************************************************************
 *
 * TopDocs
 *
 ***************************************************************************/

TopDocs *td_new(int total_hits, int size, Hit **hits, float max_score)
{
    TopDocs *td = ALLOC(TopDocs);
    td->total_hits = total_hits;
    td->size = size;
    td->hits = hits;
    td->max_score = max_score;
    return td;
}

void td_destroy(TopDocs *td)
{
    int i;

    for (i = 0; i < td->size; i++) {
        free(td->hits[i]);
    }
    free(td->hits);
    free(td);
}

char *td_to_s(TopDocs *td)
{
    int i;
    Hit *hit;
    char *buffer = strfmt("%d hits sorted by <score, doc_num>\n",
                          td->total_hits);
    for (i = 0; i < td->size; i++) {
        hit = td->hits[i];
        estrcat(buffer, strfmt("\t%d:%f\n", hit->doc, hit->score));
    }
    return buffer;
}

/***************************************************************************
 *
 * Weight
 *
 ***************************************************************************/

Query *w_get_query(Weight *self)
{
    return self->query;
}

float w_get_value(Weight *self)
{
    return self->value;
}

float w_sum_of_squared_weights(Weight *self)
{
    self->qweight = self->idf * self->query->boost;
    return self->qweight * self->qweight;   /* square it */
}

void w_normalize(Weight *self, float normalization_factor)
{
    self->qnorm = normalization_factor;
    self->qweight *= normalization_factor;  /* normalize query weight */
    self->value = self->qweight * self->idf;/* idf for document */
}

void w_destroy(Weight *self)
{
    q_deref(self->query);
    free(self);
}

Weight *w_create(size_t size, Query *query)
{
    Weight *self                    = (Weight *)ecalloc(size);
#ifdef DEBUG
    if (size < sizeof(Weight)) {
        RAISE(FERRET_ERROR, "size of weight <%d> should be at least <%d>",
              (int)size, (int)sizeof(Weight));
    }
#endif
    REF(query);
    self->query                     = query;
    self->get_query                 = &w_get_query;
    self->get_value                 = &w_get_value;
    self->normalize                 = &w_normalize;
    self->destroy                   = &w_destroy;
    self->sum_of_squared_weights    = &w_sum_of_squared_weights;
    return self;
}

/***************************************************************************
 *
 * Query
 *
 ***************************************************************************/

static const char *QUERY_NAMES[] = {
    "TermQuery",
    "MultiTermQuery",
    "BooleanQuery",
    "PhraseQuery",
    "ConstantScoreQuery",
    "FilteredQuery",
    "MatchAllQuery",
    "RangeQuery",
    "WildCardQuery",
    "FuzzyQuery",
    "PrefixQuery",
    "SpanTermQuery",
    "SpanMultiTermQuery",
    "SpanPrefixQuery",
    "SpanFirstQuery",
    "SpanOrQuery",
    "SpanNotQuery",
    "SpanNearQuery"
};

static const char *UNKNOWN_QUERY_NAME = "UnkownQuery";
    
const char *q_get_query_name(enum QUERY_TYPE type) {
    if (type >= NELEMS(QUERY_NAMES)) {
        return UNKNOWN_QUERY_NAME;
    }
    else {
        return QUERY_NAMES[type];
    }
}

static Query *q_rewrite(Query *self, IndexReader *ir)
{
    (void)ir;
    self->ref_cnt++;
    return self;
}

static void q_extract_terms(Query *self, HashSet *terms)
{
    /* do nothing by default */
    (void)self;
    (void)terms;
}

Similarity *q_get_similarity_i(Query *self, Searcher *searcher)
{
    (void)self;
    return searcher->get_similarity(searcher);
}

void q_destroy_i(Query *self)
{
    free(self);
}

void q_deref(Query *self)
{  
    if (--(self->ref_cnt) == 0) {
        self->destroy_i(self);
    }
}

Weight *q_create_weight_unsup(Query *self, Searcher *searcher)
{
    (void)self;
    (void)searcher;
    RAISE(UNSUPPORTED_ERROR,
          "Create weight is unsupported for this type of query");
    return NULL;
}

Weight *q_weight(Query *self, Searcher *searcher)
{
    Query      *query   = searcher->rewrite(searcher, self);
    Weight     *weight  = query->create_weight_i(query, searcher);
    float       sum     = weight->sum_of_squared_weights(weight);
    Similarity *sim     = query->get_similarity(query, searcher);
    float       norm    = sim_query_norm(sim, sum);
    q_deref(query);

    weight->normalize(weight, norm);
    return self->weight = weight;
}

#define BQ(query) ((BooleanQuery *)(query))
Query *q_combine(Query **queries, int q_cnt)
{
    int i;
    Query *q, *ret_q;
    HashSet *uniques = hs_new((hash_ft)&q_hash, (eq_ft)&q_eq, NULL);

    for (i = 0; i < q_cnt; i++) {
        q = queries[i];
        if (q->type == BOOLEAN_QUERY) {
            int j;
            bool splittable = true;
            if (BQ(q)->coord_disabled == false) {
                splittable = false;
            }
            else {
                for (j = 0; j < BQ(q)->clause_cnt; j++) {
                    if (BQ(q)->clauses[j]->occur != BC_SHOULD) {
                        splittable = false;
                        break;
                    }
                }
            }
            if (splittable) {
                for (j = 0; j < BQ(q)->clause_cnt; j++) {
                    Query *sub_q = BQ(q)->clauses[j]->query;
                    hs_add(uniques, sub_q);
                }
            }
            else {
                hs_add(uniques, q);
            }
        }
        else {
            hs_add(uniques, q);
        }
    }
    if (uniques->size == 1) {
        ret_q = (Query *)uniques->elems[0]; 
        REF(ret_q);
    }
    else {
        ret_q = bq_new(true);
        for (i = 0; i < uniques->size; i++) {
            q = (Query *)uniques->elems[i];
            bq_add_query(ret_q, q, BC_SHOULD);
        }
    }
    hs_destroy(uniques);

    return ret_q;
}

unsigned long q_hash(Query *self)
{
    return (self->hash(self) << 5) | self->type;
}

int q_eq(Query *self, Query *o)
{
    return (self == o)
        || ((self->type == o->type)
            && (self->boost == o->boost)
            && self->eq(self, o));
}

static MatchVector *q_get_matchv_i(Query *self, MatchVector *mv, TermVector *tv)
{
    /* be default we don't add any matches */
    (void)self; (void)tv;
    return mv;
}

Query *q_create(size_t size)
{
    Query *self = (Query *)ecalloc(size);
#ifdef DEBUG
    if (size < sizeof(Query)) {
        RAISE(FERRET_ERROR, "Size of a query <%d> should never be smaller than the "
              "size of a Query struct <%d>", (int)size, (int)sizeof(Query));
    }
#endif
    self->boost             = 1.0;
    self->rewrite           = &q_rewrite;
    self->get_similarity    = &q_get_similarity_i;
    self->extract_terms     = &q_extract_terms;
    self->get_matchv_i      = &q_get_matchv_i;
    self->weight            = NULL;
    self->ref_cnt           = 1;
    return self;
}

/***************************************************************************
 *
 * Scorer
 *
 ***************************************************************************/

void scorer_destroy_i(Scorer *scorer)
{
    free(scorer);
}

Scorer *scorer_create(size_t size, Similarity *similarity)
{
    Scorer *self        = (Scorer *)ecalloc(size);
#ifdef DEBUG
    if (size < sizeof(Scorer)) {
        RAISE(FERRET_ERROR, "size of scorer <%d> should be at least <%d>",
              (int)size, (int)sizeof(Scorer));
    }
#endif
    self->destroy       = &scorer_destroy_i;
    self->similarity    = similarity;
    return self;
}

bool scorer_less_than(void *p1, void *p2)
{
    Scorer *s1 = (Scorer *)p1;
    Scorer *s2 = (Scorer *)p2;
    return s1->score(s1) < s2->score(s2);
}

bool scorer_doc_less_than(const Scorer *s1, const Scorer *s2)
{
    return s1->doc < s2->doc;
}

int scorer_doc_cmp(const void *p1, const void *p2)
{
    return (*(Scorer **)p1)->doc - (*(Scorer **)p2)->doc;
}

/***************************************************************************
 *
 * Highlighter
 *
 ***************************************************************************/

/* ** MatchRange ** */
static int match_range_cmp(const void *p1, const void *p2)
{
    int diff = ((MatchRange *)p1)->start - ((MatchRange *)p2)->start;
    if (diff != 0) {
        return diff;
    }
    else {
        return ((MatchRange *)p2)->end - ((MatchRange *)p1)->end;
    }
}



/* ** MatchVector ** */
MatchVector *matchv_new()
{
    MatchVector *matchv = ALLOC(MatchVector);

    matchv->size = 0;
    matchv->capa = MATCH_VECTOR_INIT_CAPA;
    matchv->matches = ALLOC_N(MatchRange, MATCH_VECTOR_INIT_CAPA);

    return matchv;
}

MatchVector *matchv_add(MatchVector *self, int start, int end)
{
    if (self->size >= self->capa) {
        self->capa <<= 1;
        REALLOC_N(self->matches, MatchRange, self->capa);
    }
    self->matches[self->size].start = start;
    self->matches[self->size].end = end;
    self->matches[self->size++].score = 1.0;
    return self;
}

MatchVector *matchv_sort(MatchVector *self)
{
    qsort(self->matches, self->size, sizeof(MatchRange), &match_range_cmp);
    return self;
}

MatchVector *matchv_compact(MatchVector *self)
{
    int left, right;
    matchv_sort(self);
    for (right = left = 0; right < self->size; right++) {
        /* Note the end + 1. This compacts a range 3:5 and 6:8 inleft 3:8 */
        if (self->matches[right].start > self->matches[left].end + 1) {
            left++;
            self->matches[left].start = self->matches[right].start;
            self->matches[left].end = self->matches[right].end;
            self->matches[left].score = self->matches[right].score;
        }
        else if (self->matches[right].end > self->matches[left].end) {
            self->matches[left].end = self->matches[right].end;
        }
        else {
            self->matches[left].score += self->matches[right].score;
        }
    }
    self->size = left + 1;
    return self;
}

MatchVector *matchv_compact_with_breaks(MatchVector *self)
{
    int left, right;
    matchv_sort(self);
    for (right = left = 0; right < self->size; right++) {
        /* Note: no end + 1. Unlike above won't compact ranges 3:5 and 6:8 */
        if (self->matches[right].start > self->matches[left].end) {
            left++;
            self->matches[left].start = self->matches[right].start;
            self->matches[left].end = self->matches[right].end;
            self->matches[left].score = self->matches[right].score;
        }
        else if (self->matches[right].end > self->matches[left].end) {
            self->matches[left].end = self->matches[right].end;
            self->matches[left].score += self->matches[right].score;
        }
        else if (right > left) {
            self->matches[left].score += self->matches[right].score;
        }
    }
    self->size = left + 1;
    return self;
}


static MatchVector *matchv_set_offsets(MatchVector *mv, Offset *offsets)
{
    int i;
    for (i = 0; i < mv->size; i++) {
        mv->matches[i].start_offset = offsets[mv->matches[i].start].start;
        mv->matches[i].end_offset = offsets[mv->matches[i].end].end;
    }
    return mv;
}

void matchv_destroy(MatchVector *self)
{
    free(self->matches);
    free(self);
}

/***************************************************************************
 *
 * Searcher
 *
 ***************************************************************************/

MatchVector *searcher_get_match_vector(Searcher *self,
                                       Query *query,
                                       const int doc_num,
                                       const char *field)
{
    MatchVector *mv = matchv_new();
    bool rewrite = query->get_matchv_i == q_get_matchv_i;
    TermVector *tv = self->get_term_vector(self, doc_num, field);
    if (rewrite) {
        query = self->rewrite(self, query);
    }
    if (tv && tv->term_cnt > 0 && tv->terms[0].positions != NULL) {
        mv = query->get_matchv_i(query, mv, tv);
        tv_destroy(tv);
    }
    if (rewrite) {
        q_deref(query);
    }
    return mv;
}

typedef struct Excerpt
{
    int start;
    int end;
    int start_pos;
    int end_pos;
    int start_offset;
    int end_offset;
    double score;
} Excerpt;

/*
static int excerpt_cmp(const void *p1, const void *p2)
{
    double score1 = (*((Excerpt **)p1))->score;
    double score2 = (*((Excerpt **)p2))->score;
    if (score1 > score2) return 1;
    if (score1 < score2) return -1;
    return 0;
}
*/

static int excerpt_start_cmp(const void *p1, const void *p2)
{
    return (*((Excerpt **)p1))->start - (*((Excerpt **)p2))->start;
}

static int excerpt_lt(Excerpt *e1, Excerpt *e2)
{
    return e1->score > e2->score; /* want the highest score at top */
}

static Excerpt *excerpt_new(int start, int end, double score)
{
    Excerpt *excerpt = ALLOC_AND_ZERO(Excerpt);
    excerpt->start = start;
    excerpt->end = end;
    excerpt->score = score;
    return excerpt;
}

static Excerpt *excerpt_recalc_score(Excerpt *e, MatchVector *mv)
{
    int i;
    double score = 0.0;
    for (i = e->start; i <= e->end; i++) {
        score += mv->matches[i].score;
    }
    e->score = score;
    return e;
}

/* expand an excerpt to it's largest possible size */
static Excerpt *excerpt_expand(Excerpt *e, const int len, TermVector *tv)
{
    Offset *offsets = tv->offsets;
    int offset_cnt = tv->offset_cnt;
    bool did_expansion = true;
    int i;
    /* fill in skipped offsets */
    for (i = 1; i < offset_cnt; i++) {
        if (offsets[i].start == 0) {
            offsets[i].start = offsets[i-1].start;
        }
        if (offsets[i].end == 0) {
            offsets[i].end = offsets[i-1].end;
        }
    }
    
    while (did_expansion) {
        did_expansion = false;
        if (e->start_pos > 0
            && (e->end_offset - offsets[e->start_pos - 1].start) < len) {
            e->start_pos--;
            e->start_offset = offsets[e->start_pos].start;
            did_expansion = true;
        }
        if (e->end_pos < (offset_cnt - 1)
            && (offsets[e->end_pos + 1].end - e->start_offset) < len) {
            e->end_pos++;
            e->end_offset = offsets[e->end_pos].end;
            did_expansion = true;
        }
    }
    return e;
}

static char *excerpt_get_str(Excerpt *e, MatchVector *mv,
                             LazyDocField *lazy_df,
                             const char *pre_tag, 
                             const char *post_tag,
                             const char *ellipsis)
{
    int i, len;
    int last_offset = e->start_offset;
    const int num_matches = e->end - e->start + 1;
    const int pre_tag_len = (int)strlen(pre_tag);
    const int post_tag_len = (int)strlen(post_tag);
    const int ellipsis_len = (int)strlen(ellipsis);
    char *excerpt_str = ALLOC_N(char,
                                10 + e->end_offset - e->start_offset
                                + (num_matches * (pre_tag_len + post_tag_len))
                                + (2 * ellipsis_len));
    char *e_ptr = excerpt_str;
    if (e->start_offset > 0) {
        memcpy(e_ptr, ellipsis, ellipsis_len);
        e_ptr += ellipsis_len;
    }
    for (i = e->start; i <= e->end; i++) {
        MatchRange *mr = mv->matches + i;
        len = mr->start_offset - last_offset;
        if (len) {
            lazy_df_get_bytes(lazy_df, e_ptr, last_offset, len);
            e_ptr += len;
        }
        memcpy(e_ptr, pre_tag, pre_tag_len);
        e_ptr += pre_tag_len;
        len = mr->end_offset - mr->start_offset;
        if (len) {
            lazy_df_get_bytes(lazy_df, e_ptr, mr->start_offset, len);
            e_ptr += len;
        }
        memcpy(e_ptr, post_tag, post_tag_len);
        e_ptr += post_tag_len;
        last_offset = mr->end_offset;
    }
    if ((lazy_df->len - e->end_offset) <= ellipsis_len) {
        /* no point using ellipsis if it takes up more space */
        e->end_offset = lazy_df->len;
    }
    len = e->end_offset - last_offset;
    if (len) {
        lazy_df_get_bytes(lazy_df, e_ptr, last_offset, len);
        e_ptr += len;
    }
    if (e->end_offset < lazy_df->len) {
        memcpy(e_ptr, ellipsis, ellipsis_len);
        e_ptr += ellipsis_len;
    }
    *e_ptr = '\0';
    return excerpt_str;
}

static char *highlight_field(MatchVector *mv,
                             LazyDocField *lazy_df,
                             TermVector *tv,
                             const char *pre_tag, 
                             const char *post_tag)
{
    const int pre_len = (int)strlen(pre_tag);
    const int post_len = (int)strlen(post_tag);
    char *excerpt_str =
        ALLOC_N(char, 10 + lazy_df->len + (mv->size * (pre_len + post_len)));
    if (mv->size > 0) {
        int last_offset = 0;
        int i, len;
        char *e_ptr = excerpt_str;
        matchv_compact_with_breaks(mv);
        matchv_set_offsets(mv, tv->offsets);
        for (i = 0; i < mv->size; i++) {
            MatchRange *mr = mv->matches + i;
            len = mr->start_offset - last_offset;
            if (len) {
                lazy_df_get_bytes(lazy_df, e_ptr, last_offset, len);
                e_ptr += len;
            }
            memcpy(e_ptr, pre_tag, pre_len);
            e_ptr += pre_len;
            len = mr->end_offset - mr->start_offset;
            if (len) {
                lazy_df_get_bytes(lazy_df, e_ptr, mr->start_offset, len);
                e_ptr += len;
            }
            memcpy(e_ptr, post_tag, post_len);
            e_ptr += post_len;
            last_offset = mr->end_offset;
        }
        len = lazy_df->len - last_offset;
        if (len) {
            lazy_df_get_bytes(lazy_df, e_ptr, last_offset, len);
            e_ptr += len;
        }
        *e_ptr = '\0';
    }
    else {
        lazy_df_get_bytes(lazy_df, excerpt_str, 0, lazy_df->len);
        excerpt_str[lazy_df->len] = '\0';
    }
    return excerpt_str;
}

char **searcher_highlight(Searcher *self,
                          Query *query,
                          const int doc_num,
                          const char *field,
                          const int excerpt_len,
                          const int num_excerpts,
                          const char *pre_tag,
                          const char *post_tag,
                          const char *ellipsis)
{
    char **excerpt_strs = NULL;
    TermVector *tv = self->get_term_vector(self, doc_num, field);
    LazyDoc *lazy_doc = self->get_lazy_doc(self, doc_num);
    LazyDocField *lazy_df = NULL;
    if (lazy_doc) {
        lazy_df = h_get(lazy_doc->field_dict, field);
    }
    if (tv && lazy_df && tv->term_cnt > 0 && tv->terms[0].positions != NULL
        && tv->offsets != NULL) {
        MatchVector *mv;
        query = self->rewrite(self, query);
        mv = query->get_matchv_i(query, matchv_new(), tv);
        q_deref(query);
        if (lazy_df->len < (excerpt_len * num_excerpts)) {
            excerpt_strs = ary_new_type_capa(char *, 1);
            ary_push(excerpt_strs,
                     highlight_field(mv, lazy_df, tv, pre_tag, post_tag));
        }
        else if (mv->size > 0) {
            Excerpt **excerpts = ALLOC_AND_ZERO_N(Excerpt *, num_excerpts);
            int e_start, e_end, i, j;
            MatchRange *matches = mv->matches;
            double running_score = 0.0;
            Offset *offsets = tv->offsets;
            PriorityQueue *excerpt_pq;

            matchv_compact_with_breaks(mv);
            matchv_set_offsets(mv, offsets);
            excerpt_pq = pq_new(mv->size, (lt_ft)&excerpt_lt, &free);
            /* add all possible excerpts to the priority queue */
            
            for (e_start = e_end = 0; e_start < mv->size; e_start++) {
                const int start_offset = matches[e_start].start_offset;
                if (e_start > e_end) {
                    running_score = 0.0;
                    e_end = e_start;
                }
                while (e_end < mv->size && (matches[e_end].end_offset
                                             <= start_offset + excerpt_len)) {
                    running_score += matches[e_end].score;
                    e_end++;
                }
                pq_push(excerpt_pq,
                        excerpt_new(e_start, e_end - 1, running_score));
                /* - 0.1 so that earlier matches take priority */
                running_score -= matches[e_start].score;
            }

            for (i = 0; i < num_excerpts && excerpt_pq->size > 0; i++) {
                excerpts[i] = pq_pop(excerpt_pq);
                if (i < num_excerpts - 1) {
                    /* set match ranges alread included to 0 */
                    Excerpt *e = excerpts[i];
                    for (j = e->start; j <= e->end; j++) {
                        matches[j].score = 0.0;
                    }
                    e = NULL;
                    while (e != (Excerpt *)pq_top(excerpt_pq)) {
                        e = pq_top(excerpt_pq);
                        excerpt_recalc_score(e, mv);
                        pq_down(excerpt_pq);
                    }
                }
            }

            qsort(excerpts, i, sizeof(Excerpt *), &excerpt_start_cmp);
            for (j = 0; j < i; j++) {
                Excerpt *e = excerpts[j];
                e->start_pos = matches[e->start].start;
                e->end_pos = matches[e->end].end;
                e->start_offset = offsets[e->start_pos].start;
                e->end_offset = offsets[e->end_pos].end;
            }

            if (i < num_excerpts) {
                const int diff = num_excerpts - i;
                memmove(excerpts + (diff), excerpts,
                        i * sizeof(Excerpt *));
                for (j = 0; j < diff; j++) {
                    /* these new excerpts will grow into one long excerpt at
                     * the start */
                    excerpts[j] = ALLOC_AND_ZERO(Excerpt);
                    excerpts[j]->end = -1;
                }
            }

            excerpt_strs = ary_new_type_capa(char *, num_excerpts);
            /* merge excerpts where possible */
            for (i = 0; i < num_excerpts;) {
                Excerpt *ei = excerpts[i];
                int merged = 1; /* 1 means a single excerpt, ie no merges */
                for (j = i + 1; j < num_excerpts; j++) {
                    Excerpt *ej = excerpts[j];
                    if ((ej->end_offset - ei->start_offset)
                        < (j - i + 1) * excerpt_len) {
                        ei->end = ej->end;
                        ei->end_pos = ej->end_pos;
                        ei->end_offset = ej->end_offset;
                        merged = j - i + 1;
                    }
                }
                excerpt_expand(ei, merged * excerpt_len, tv);
                ary_push(excerpt_strs,
                         excerpt_get_str(ei, mv, lazy_df,
                                         pre_tag, post_tag, ellipsis));
                i += merged;
            }
            for (i = 0; i < num_excerpts; i++) {
                free(excerpts[i]);
            }
            free(excerpts);
            pq_destroy(excerpt_pq);
        }
        matchv_destroy(mv);
    }
    if (tv) tv_destroy(tv);
    if (lazy_doc) lazy_doc_close(lazy_doc);
    return excerpt_strs;
}

static Weight *sea_create_weight(Searcher *self, Query *query)
{
    return q_weight(query, self);
}

static void sea_check_args(int num_docs, int first_doc)
{
    if (num_docs <= 0) {
        RAISE(ARG_ERROR, ":num_docs was set to %d but should be greater "
              "than 0 : %d <= 0", num_docs, num_docs);
    }

    if (first_doc < 0) {
        RAISE(ARG_ERROR, ":first_doc was set to %d but should be greater "
              "than or equal to 0 : %d < 0", first_doc, first_doc);
    }
}

static Similarity *sea_get_similarity(Searcher *self)
{
    return self->similarity;
}

/***************************************************************************
 *
 * IndexSearcher
 *
 ***************************************************************************/

#define ISEA(searcher) ((IndexSearcher *)(searcher))

int isea_doc_freq(Searcher *self, const char *field, const char *term)
{
    return ir_doc_freq(ISEA(self)->ir, field, term);
}

static Document *isea_get_doc(Searcher *self, int doc_num)
{
    IndexReader *ir = ISEA(self)->ir;
    return ir->get_doc(ir, doc_num);
}

static LazyDoc *isea_get_lazy_doc(Searcher *self, int doc_num)
{
    IndexReader *ir = ISEA(self)->ir;
    return ir->get_lazy_doc(ir, doc_num);
}

static int isea_max_doc(Searcher *self)
{
    IndexReader *ir = ISEA(self)->ir;
    return ir->max_doc(ir);
}

#define IS_FILTERED(bits, filter_func, scorer, searcher) \
((bits && !bv_get(bits, scorer->doc))\
 || (filter_func \
     && !filter_func(scorer->doc, scorer->score(scorer), searcher)))

static TopDocs *isea_search_w(Searcher *self,
                              Weight *weight,
                              int first_doc,
                              int num_docs,
                              Filter *filter,
                              Sort *sort,
                              filter_ft filter_func,
                              bool load_fields)
{
    int max_size = num_docs + (num_docs == INT_MAX ? 0 : first_doc);
    int i;
    Scorer *scorer;
    Hit **score_docs = NULL;
    Hit hit;
    int total_hits = 0;
    float score, max_score = 0.0;
    BitVector *bits = (filter
                       ? filt_get_bv(filter, ISEA(self)->ir)
                       : NULL);
    Hit *(*hq_pop)(PriorityQueue *pq);
    void (*hq_insert)(PriorityQueue *pq, Hit *hit);
    void (*hq_destroy)(PriorityQueue *self);
    PriorityQueue *hq;

    sea_check_args(num_docs, first_doc);

    scorer = weight->scorer(weight, ISEA(self)->ir);
    if (!scorer || 0 == ISEA(self)->ir->num_docs(ISEA(self)->ir)) {
        if (scorer) scorer->destroy(scorer);
        return td_new(0, 0, NULL, 0.0);
    }

    if (sort) {
        hq = fshq_pq_new(max_size, sort, ISEA(self)->ir);
        hq_insert = &fshq_pq_insert;
        hq_destroy = &fshq_pq_destroy;
        if (load_fields) {
            hq_pop = &fshq_pq_pop_fd;
        }
        else {
            hq_pop = &fshq_pq_pop;
        }
    }
    else {
        hq = pq_new(max_size, (lt_ft)&hit_less_than, &free);
        hq_pop = &hit_pq_pop;
        hq_insert = &hit_pq_insert;
        hq_destroy = &pq_destroy;
    }

    while (scorer->next(scorer)) {
        if (IS_FILTERED(bits, filter_func, scorer, self)) {
            continue;
        }
        total_hits++;
        score = scorer->score(scorer);
        if (score > max_score) max_score = score;
        hit.doc = scorer->doc; hit.score = score;
        hq_insert(hq, &hit);
    }
    scorer->destroy(scorer);

    if (hq->size > first_doc) {
        if ((hq->size - first_doc) < num_docs) {
            num_docs = hq->size - first_doc;
        }
        score_docs = ALLOC_N(Hit *, num_docs);
        for (i = num_docs - 1; i >= 0; i--) {
            score_docs[i] = hq_pop(hq);
            /*
            printf("score_docs[i][%d] = [%ld] => %d-->%f\n", i,
                   score_docs[i], score_docs[i]->doc, score_docs[i]->score);
            */
        }
    }
    else {
        num_docs = 0;
    }
    pq_clear(hq);
    hq_destroy(hq);

    return td_new(total_hits, num_docs, score_docs, max_score);
}

static TopDocs *isea_search(Searcher *self,
                            Query *query,
                            int first_doc,
                            int num_docs,
                            Filter *filter,
                            Sort *sort,
                            filter_ft filter_func,
                            bool load_fields)
{
    TopDocs *td;
    Weight *weight = q_weight(query, self);
    td = isea_search_w(self, weight, first_doc, num_docs, filter,
                         sort, filter_func, load_fields);
    weight->destroy(weight);
    return td;
}

static void isea_search_each_w(Searcher *self, Weight *weight, Filter *filter,
                               filter_ft filter_func,
                               void (*fn)(Searcher *, int, float, void *),
                               void *arg)
{
    Scorer *scorer;
    BitVector *bits = (filter
                       ? filt_get_bv(filter, ISEA(self)->ir)
                       : NULL);

    scorer = weight->scorer(weight, ISEA(self)->ir);
    if (!scorer) {
        return;
    }

    while (scorer->next(scorer)) {
        if (IS_FILTERED(bits, filter_func, scorer, self)) {
            continue;
        }
        fn(self, scorer->doc, scorer->score(scorer), arg);
    }
    scorer->destroy(scorer);
}

static void isea_search_each(Searcher *self, Query *query, Filter *filter,
                             filter_ft filter_func,
                             void (*fn)(Searcher *, int, float, void *),
                             void *arg)
{
    Weight *weight = q_weight(query, self);
    isea_search_each_w(self, weight, filter, filter_func, fn, arg);
    weight->destroy(weight);
}

static Query *isea_rewrite(Searcher *self, Query *original)
{
    int q_is_destroyed = false;
    Query *query = original;
    Query *rewritten_query = query->rewrite(query, ISEA(self)->ir);
    while (q_is_destroyed || (query != rewritten_query)) {
        query = rewritten_query;
        rewritten_query = query->rewrite(query, ISEA(self)->ir);
        q_is_destroyed = (query->ref_cnt <= 1);
        q_deref(query); /* destroy intermediate queries */
    }
    return query;
}

static Explanation *isea_explain(Searcher *self, Query *query, int doc_num)
{
    Weight *weight = q_weight(query, self);
    Explanation *e =  weight->explain(weight, ISEA(self)->ir, doc_num);
    weight->destroy(weight);
    return e;
}

static Explanation *isea_explain_w(Searcher *self, Weight *w, int doc_num)
{
    return w->explain(w, ISEA(self)->ir, doc_num);
}

static TermVector *isea_get_term_vector(Searcher *self,
                                          const int doc_num,
                                          const char *field)
{
    IndexReader *ir = ISEA(self)->ir;
    return ir->term_vector(ir, doc_num, field);
}

static void isea_close(Searcher *self)
{
    if (ISEA(self)->ir && ISEA(self)->close_ir) {
        ir_close(ISEA(self)->ir);
    }
    free(self);
}

Searcher *isea_new(IndexReader *ir)
{
    Searcher *self          = (Searcher *)ecalloc(sizeof(IndexSearcher));

    ISEA(self)->ir          = ir;
    ISEA(self)->close_ir    = true;

    self->similarity        = sim_create_default();
    self->doc_freq          = &isea_doc_freq;
    self->get_doc           = &isea_get_doc;
    self->get_lazy_doc      = &isea_get_lazy_doc;
    self->max_doc           = &isea_max_doc;
    self->create_weight     = &sea_create_weight;
    self->search            = &isea_search;
    self->search_w          = &isea_search_w;
    self->search_each       = &isea_search_each;
    self->search_each_w     = &isea_search_each_w;
    self->rewrite           = &isea_rewrite;
    self->explain           = &isea_explain;
    self->explain_w         = &isea_explain_w;
    self->get_term_vector   = &isea_get_term_vector;
    self->get_similarity    = &sea_get_similarity;
    self->close             = &isea_close;

    return self;
}

/***************************************************************************
 *
 * CachedDFSearcher
 *
 ***************************************************************************/

#define CDFSEA(searcher) ((CachedDFSearcher *)(searcher))
typedef struct CachedDFSearcher
{
    Searcher    super;
    HashTable  *df_map;
    int         max_doc;
} CachedDFSearcher;

static int cdfsea_doc_freq(Searcher *self, const char *field, const char *text)
{
    Term term;
    int *df;
    term.field = (char *)field;
    term.text = (char *)text;
    df = (int *)h_get(CDFSEA(self)->df_map, &term);
    return df ? *df : 0;
}

static Document *cdfsea_get_doc(Searcher *self, int doc_num)
{
    (void)self; (void)doc_num;
    RAISE(UNSUPPORTED_ERROR, UNSUPPORTED_ERROR_MSG);
    return NULL;
}

static int cdfsea_max_doc(Searcher *self)
{
    (void)self;
    return CDFSEA(self)->max_doc;
}

static Weight *cdfsea_create_weight(Searcher *self, Query *query)
{
    (void)self; (void)query;
    RAISE(UNSUPPORTED_ERROR, UNSUPPORTED_ERROR_MSG);
    return NULL;
}

static TopDocs *cdfsea_search_w(Searcher *self, Weight *w, int fd, int nd,
                                Filter *f, Sort *s, filter_ft ff, bool load)
{
    (void)self; (void)w; (void)fd; (void)nd;
    (void)f; (void)s; (void)ff; (void)load;
    RAISE(UNSUPPORTED_ERROR, UNSUPPORTED_ERROR_MSG);
    return NULL;
}

static TopDocs *cdfsea_search(Searcher *self, Query *q, int fd, int nd,
                              Filter *f, Sort *s, filter_ft ff, bool load)
{
    (void)self; (void)q; (void)fd; (void)nd;
    (void)f; (void)s; (void)ff; (void)load;
    RAISE(UNSUPPORTED_ERROR, UNSUPPORTED_ERROR_MSG);
    return NULL;
}

static void cdfsea_search_each(Searcher *self, Query *query, Filter *filter,
                               filter_ft ff, 
                               void (*fn)(Searcher *, int, float, void *),
                               void *arg)
{
    (void)self; (void)query; (void)filter; (void)ff; (void)fn; (void)arg;
    RAISE(UNSUPPORTED_ERROR, UNSUPPORTED_ERROR_MSG);
}

static void cdfsea_search_each_w(Searcher *self, Weight *w, Filter *filter,
                                 filter_ft ff, 
                                 void (*fn)(Searcher *, int, float, void *),
                                 void *arg)
{
    (void)self; (void)w; (void)filter; (void)ff; (void)fn; (void)arg;
    RAISE(UNSUPPORTED_ERROR, UNSUPPORTED_ERROR_MSG);
}

static Query *cdfsea_rewrite(Searcher *self, Query *original)
{
    (void)self;
    original->ref_cnt++;
    return original;
}

static Explanation *cdfsea_explain(Searcher *self, Query *query, int doc_num)
{
    (void)self; (void)query; (void)doc_num;
    RAISE(UNSUPPORTED_ERROR, UNSUPPORTED_ERROR_MSG);
    return NULL;
}

static Explanation *cdfsea_explain_w(Searcher *self, Weight *w, int doc_num)
{
    (void)self; (void)w; (void)doc_num;
    RAISE(UNSUPPORTED_ERROR, UNSUPPORTED_ERROR_MSG);
    return NULL;
}

static TermVector *cdfsea_get_term_vector(Searcher *self, const int doc_num,
                                          const char *field)
{
    (void)self; (void)doc_num; (void)field;
    RAISE(UNSUPPORTED_ERROR, UNSUPPORTED_ERROR_MSG);
    return NULL;
}

static Similarity *cdfsea_get_similarity(Searcher *self)
{
    (void)self;
    RAISE(UNSUPPORTED_ERROR, UNSUPPORTED_ERROR_MSG);
    return NULL;
}

static void cdfsea_close(Searcher *self)
{
    h_destroy(CDFSEA(self)->df_map);
    free(self);
}

static Searcher *cdfsea_new(HashTable *df_map, int max_doc)
{
    Searcher *self          = (Searcher *)ecalloc(sizeof(CachedDFSearcher));

    CDFSEA(self)->df_map    = df_map;
    CDFSEA(self)->max_doc   = max_doc;

    self->doc_freq          = &cdfsea_doc_freq;
    self->get_doc           = &cdfsea_get_doc;
    self->max_doc           = &cdfsea_max_doc;
    self->create_weight     = &cdfsea_create_weight;
    self->search            = &cdfsea_search;
    self->search_w          = &cdfsea_search_w;
    self->search_each       = &cdfsea_search_each;
    self->search_each_w     = &cdfsea_search_each_w;
    self->rewrite           = &cdfsea_rewrite;
    self->explain           = &cdfsea_explain;
    self->explain_w         = &cdfsea_explain_w;
    self->get_term_vector   = &cdfsea_get_term_vector;
    self->get_similarity    = &cdfsea_get_similarity;
    self->close             = &cdfsea_close;
    return self;
}

/***************************************************************************
 *
 * MultiSearcher
 *
 ***************************************************************************/

#define MSEA(searcher) ((MultiSearcher *)(searcher))
static INLINE int msea_get_searcher_index(Searcher *self, int n)
{
    MultiSearcher *msea = MSEA(self);
    int lo = 0;                 /* search starts array */
    int hi = msea->s_cnt - 1;   /* for 1st element < n, return its index */
    int mid, mid_val;

    while (hi >= lo) {
        mid = (lo + hi) >> 1;
        mid_val = msea->starts[mid];
        if (n < mid_val) {
            hi = mid - 1;
        }
        else if (n > mid_val) {
            lo = mid + 1;
        }
        else {                  /* found a match */
            while (((mid+1) < msea->s_cnt)
                   && (msea->starts[mid+1] == mid_val)) {
                mid++;          /* scan to last match */
            }
            return mid;
        }
    }
    return hi;
}

static int msea_doc_freq(Searcher *self, const char *field, const char *term)
{
    int i;
    int doc_freq = 0;
    MultiSearcher *msea = MSEA(self);
    for (i = 0; i < msea->s_cnt; i++) {
        Searcher *s = msea->searchers[i];
        doc_freq += s->doc_freq(s, field, term);
    }

    return doc_freq;
}

static Document *msea_get_doc(Searcher *self, int doc_num)
{
    MultiSearcher *msea = MSEA(self);
    int i = msea_get_searcher_index(self, doc_num);
    Searcher *s = msea->searchers[i];
    return s->get_doc(s, doc_num - msea->starts[i]);
}

static LazyDoc *msea_get_lazy_doc(Searcher *self, int doc_num)
{
    MultiSearcher *msea = MSEA(self);
    int i = msea_get_searcher_index(self, doc_num);
    Searcher *s = msea->searchers[i];
    return s->get_lazy_doc(s, doc_num - msea->starts[i]);
}

static int msea_max_doc(Searcher *self)
{
    return MSEA(self)->max_doc;
}

static int *msea_get_doc_freqs(Searcher *self, HashSet *terms)
{
    int i;
    const int num_terms = terms->size;
    int *doc_freqs = ALLOC_N(int, num_terms);
    for (i = 0; i < num_terms; i++) {
        Term *t = (Term *)terms->elems[i];
        doc_freqs[i] = msea_doc_freq(self, t->field, t->text);
    }
    return doc_freqs;
}

static Weight *msea_create_weight(Searcher *self, Query *query)
{
    int i, *doc_freqs;
    Searcher *cdfsea;
    Weight *w;
    HashTable *df_map = h_new((hash_ft)&term_hash, (eq_ft)&term_eq,
                             (free_ft)NULL, free);
    Query *rewritten_query = self->rewrite(self, query);
    HashSet *terms = term_set_new();

    rewritten_query->extract_terms(rewritten_query, terms);
    doc_freqs = msea_get_doc_freqs(self, terms);

    for (i = 0; i < terms->size; i++) {
        h_set(df_map, terms->elems[i], imalloc(doc_freqs[i])); 
    }
    hs_destroy(terms);
    free(doc_freqs);

    cdfsea = cdfsea_new(df_map, MSEA(self)->max_doc);

    w = q_weight(rewritten_query, cdfsea);
    q_deref(rewritten_query);
    cdfsea->close(cdfsea);

    return w;
}

struct MultiSearchEachArg {
    int start;
    void *arg;
    void (*fn)(Searcher *, int, float, void *);
};

void msea_search_each_i(Searcher *self, int doc_num, float score, void *arg)
{
    struct MultiSearchEachArg *mse_arg = (struct MultiSearchEachArg *)arg;

    mse_arg->fn(self, doc_num + mse_arg->start, score, mse_arg->arg);
}

static void msea_search_each_w(Searcher *self, Weight *w, Filter *filter,
                               filter_ft filter_func,
                               void (*fn)(Searcher *, int, float, void *),
                               void *arg)
{
    int i;
    struct MultiSearchEachArg mse_arg;
    MultiSearcher *msea = MSEA(self);
    Searcher *s;

    mse_arg.fn = fn;
    mse_arg.arg = arg;
    for (i = 0; i < msea->s_cnt; i++) {
        s = msea->searchers[i];
        mse_arg.start = msea->starts[i];
        s->search_each_w(s, w, filter, filter_func,
                         &msea_search_each_i, &mse_arg);
    }
}

static void msea_search_each(Searcher *self, Query *query, Filter *filter,
                             filter_ft filter_func,
                             void (*fn)(Searcher *, int, float, void *), void *arg)
{
    Weight *w = q_weight(query, self);
    msea_search_each_w(self, w, filter, filter_func, fn, arg);
    w->destroy(w);
}

struct MultiSearchArg {
    int total_hits, max_size;
    PriorityQueue *hq;
    void (*hq_insert)(PriorityQueue *pq, Hit *hit);
};

void msea_search_i(Searcher *self, int doc_num, float score, void *arg)
{
    struct MultiSearchArg *ms_arg = (struct MultiSearchArg *)arg;
    Hit hit;
    (void)self;

    ms_arg->total_hits++;
    hit.doc = doc_num;
    hit.score = score;
    ms_arg->hq_insert(ms_arg->hq, &hit);
}

static TopDocs *msea_search_w(Searcher *self,
                              Weight *weight,
                              int first_doc,
                              int num_docs,
                              Filter *filter,
                              Sort *sort,
                              filter_ft filter_func,
                              bool load_fields)
{
    int max_size = num_docs + (num_docs == INT_MAX ? 0 : first_doc);
    int i;
    int total_hits = 0;
    Hit **score_docs = NULL;
    Hit *(*hq_pop)(PriorityQueue *pq);
    void (*hq_insert)(PriorityQueue *pq, Hit *hit);
    PriorityQueue *hq;
    float max_score = 0.0;
    (void)load_fields; /* does it automatically */

    sea_check_args(num_docs, first_doc);

    if (sort) {
        hq = pq_new(max_size, (lt_ft)fdshq_lt, &free);
        hq_insert = (void (*)(PriorityQueue *pq, Hit *hit))&pq_insert;
        hq_pop = (Hit *(*)(PriorityQueue *pq))&pq_pop;
    }
    else {
        hq = pq_new(max_size, (lt_ft)&hit_less_than, &free);
        hq_insert = &hit_pq_multi_insert;
        hq_pop = &hit_pq_pop;
    }

    /*if (sort) printf("sort = %s\n", sort_to_s(sort)); */
    for (i = 0; i < MSEA(self)->s_cnt; i++) {
        Searcher *s = MSEA(self)->searchers[i];
        TopDocs *td = s->search_w(s, weight, 0, max_size,
                                  filter, sort, filter_func, true);
        /*if (sort) printf("sort = %s\n", sort_to_s(sort)); */
        if (td->size > 0) {
            /*printf("td->size = %d %d\n", td->size, num_docs); */
            int j;
            int start = MSEA(self)->starts[i];
            for (j = 0; j < td->size; j++) {
                Hit *hit = td->hits[j];
                hit->doc += start;
                /*
                printf("adding hit = %d:%f\n", hit->doc, hit->score);
                */
                hq_insert(hq, hit);
            }
            td->size = 0;
            if (td->max_score > max_score) max_score = td->max_score;
        }
        total_hits += td->total_hits;
        td_destroy(td);
    }

    if (hq->size > first_doc) {
        if ((hq->size - first_doc) < num_docs) {
            num_docs = hq->size - first_doc;
        }
        score_docs = ALLOC_N(Hit *, num_docs);
        for (i = num_docs - 1; i >= 0; i--) {
            score_docs[i] = hq_pop(hq);
            /*
            Hit *hit = score_docs[i] = hq_pop(hq);
            printf("popped hit = %d-->%f\n", hit->doc, hit->score);
            */
        }
    }
    else {
        num_docs = 0;
    }
    pq_clear(hq);
    pq_destroy(hq);

    return td_new(total_hits, num_docs, score_docs, max_score);
}

static TopDocs *msea_search(Searcher *self,
                            Query *query,
                            int first_doc,
                            int num_docs,
                            Filter *filter,
                            Sort *sort,
                            filter_ft filter_func,
                            bool load_fields)
{
    TopDocs *td;
    Weight *weight = q_weight(query, self);
    td = msea_search_w(self, weight, first_doc, num_docs, filter,
                       sort, filter_func, load_fields);
    weight->destroy(weight);
    return td;
}

static Query *msea_rewrite(Searcher *self, Query *original)
{
    int i;
    Searcher *s;
    MultiSearcher *msea = MSEA(self);
    Query **queries = ALLOC_N(Query *, msea->s_cnt), *rewritten;

    for (i = 0; i < msea->s_cnt; i++) {
        s = msea->searchers[i];
        queries[i] = s->rewrite(s, original);
    }
    rewritten = q_combine(queries, msea->s_cnt);

    for (i = 0; i < msea->s_cnt; i++) {
        q_deref(queries[i]);
    }
    free(queries);
    return rewritten;
}

static Explanation *msea_explain(Searcher *self, Query *query, int doc_num)
{
    MultiSearcher *msea = MSEA(self);
    int i = msea_get_searcher_index(self, doc_num);
    Weight *w = q_weight(query, self);
    Searcher *s = msea->searchers[i];
    Explanation *e = s->explain_w(s, w, doc_num - msea->starts[i]);
    w->destroy(w);
    return e;
}

static Explanation *msea_explain_w(Searcher *self, Weight *w, int doc_num)
{
    MultiSearcher *msea = MSEA(self);
    int i = msea_get_searcher_index(self, doc_num);
    Searcher *s = msea->searchers[i];
    Explanation *e = s->explain_w(s, w, doc_num - msea->starts[i]);
    return e;
}

static TermVector *msea_get_term_vector(Searcher *self, const int doc_num,
                                        const char *field)
{
    MultiSearcher *msea = MSEA(self);
    int i = msea_get_searcher_index(self, doc_num);
    Searcher *s = msea->searchers[i];
    return s->get_term_vector(s, doc_num - msea->starts[i],
                              field);
}

static Similarity *msea_get_similarity(Searcher *self)
{
    return self->similarity;
}

static void msea_close(Searcher *self)
{
    int i;
    Searcher *s;
    MultiSearcher *msea = MSEA(self);
    if (msea->close_subs) {
        for (i = 0; i < msea->s_cnt; i++) {
            s = msea->searchers[i];
            s->close(s);
        }
    }
    free(msea->searchers);
    free(msea->starts);
    free(self);
}

Searcher *msea_new(Searcher **searchers, int s_cnt, bool close_subs)
{
    int i, max_doc = 0;
    Searcher *self = (Searcher *)ecalloc(sizeof(MultiSearcher));
    int *starts = ALLOC_N(int, s_cnt + 1);
    for (i = 0; i < s_cnt; i++) {
        starts[i] = max_doc;
        max_doc += searchers[i]->max_doc(searchers[i]);
    }
    starts[i] = max_doc;

    MSEA(self)->s_cnt           = s_cnt;
    MSEA(self)->searchers       = searchers;
    MSEA(self)->starts          = starts;
    MSEA(self)->max_doc         = max_doc;
    MSEA(self)->close_subs      = close_subs;

    self->similarity            = sim_create_default();
    self->doc_freq              = &msea_doc_freq;
    self->get_doc               = &msea_get_doc;
    self->get_lazy_doc          = &msea_get_lazy_doc;
    self->max_doc               = &msea_max_doc;
    self->create_weight         = &msea_create_weight;
    self->search                = &msea_search;
    self->search_w              = &msea_search_w;
    self->search_each           = &msea_search_each;
    self->search_each_w         = &msea_search_each_w;
    self->rewrite               = &msea_rewrite;
    self->explain               = &msea_explain;
    self->explain_w             = &msea_explain_w;
    self->get_term_vector       = &msea_get_term_vector;
    self->get_similarity        = &msea_get_similarity;
    self->close                 = &msea_close;
    return self;
}
