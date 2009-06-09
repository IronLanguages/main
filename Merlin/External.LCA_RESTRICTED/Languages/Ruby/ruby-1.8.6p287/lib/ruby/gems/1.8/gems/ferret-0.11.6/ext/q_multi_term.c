#include <string.h>
#include "search.h"
#include "priorityqueue.h"
#include "helper.h"

#define MTQ(query) ((MultiTermQuery *)(query))

/***************************************************************************
 *
 * MultiTerm
 *
 ***************************************************************************/

/***************************************************************************
 * BoostedTerm
 ***************************************************************************/

typedef struct BoostedTerm
{
    char *term;
    float boost;
} BoostedTerm;

static bool boosted_term_less_than(const BoostedTerm *bt1,
                                  const BoostedTerm *bt2)
{
    if (bt1->boost == bt2->boost) {
        return (strcmp(bt1->term, bt2->term) < 0);
    }

    return (bt1->boost < bt2->boost);
}

static void boosted_term_destroy(BoostedTerm *self)
{
    free(self->term);
    free(self);
}

static BoostedTerm *boosted_term_new(const char *term, float boost)
{
    BoostedTerm *self = ALLOC(BoostedTerm);
    self->term = estrdup(term);
    self->boost = boost;
    return self;
}

/***************************************************************************
 * TermDocEnumWrapper
 ***************************************************************************/

#define TDE_READ_SIZE 16

typedef struct TermDocEnumWrapper
{
    const char  *term;
    TermDocEnum *tde;
    float        boost;
    int          doc;
    int          freq;
    int          docs[TDE_READ_SIZE];
    int          freqs[TDE_READ_SIZE];
    int          pointer;
    int          pointer_max;
} TermDocEnumWrapper;

static bool tdew_less_than(const TermDocEnumWrapper *tdew1,
                           const TermDocEnumWrapper *tdew2)
{
    return (tdew1->doc < tdew2->doc);
}

static bool tdew_next(TermDocEnumWrapper *self)
{
    self->pointer++;
    if (self->pointer >= self->pointer_max) {
        /* refill buffer */
        self->pointer_max = self->tde->read(self->tde, self->docs, self->freqs,
                                        TDE_READ_SIZE);
        if (self->pointer_max != 0) {
            self->pointer = 0;
        }
        else {
            return false;
        }
    }
    self->doc = self->docs[self->pointer];
    self->freq = self->freqs[self->pointer];
    return true;
}

static bool tdew_skip_to(TermDocEnumWrapper *self, int doc_num)
{
    TermDocEnum *tde = self->tde;

    while (++(self->pointer) < self->pointer_max) {
        if (self->docs[self->pointer] >= doc_num) {
            self->doc = self->docs[self->pointer];
            self->freq = self->freqs[self->pointer];
            return true;
        }
    }

    /* not found in cache, seek underlying stream */
    if (tde->skip_to(tde, doc_num)) {
        self->pointer_max = 1;
        self->pointer = 0;
        self->docs[0] = self->doc = tde->doc_num(tde);
        self->freqs[0] = self->freq = tde->freq(tde);
        return true;
    }
    else {
        return false;
    }
}

static void tdew_destroy(TermDocEnumWrapper *self)
{
    self->tde->close(self->tde);
    free(self);
}

static TermDocEnumWrapper *tdew_new(const char *term, TermDocEnum *tde,
                                    float boost)
{
    TermDocEnumWrapper *self = ALLOC_AND_ZERO(TermDocEnumWrapper);
    self->term = term;
    self->tde = tde;
    self->boost = boost;
    self->doc = -1;
    return self;
}

/***************************************************************************
 * MultiTermScorer
 ***************************************************************************/

#define SCORE_CACHE_SIZE 32
#define MTSc(scorer) ((MultiTermScorer *)(scorer))

typedef struct MultiTermScorer
{
    Scorer                super;
    const char           *field;
    uchar                *norms;
    Weight               *weight;
    TermDocEnumWrapper  **tdew_a;
    int                   tdew_cnt;
    PriorityQueue        *tdew_pq;
    float                 weight_value;
    float                 score_cache[SCORE_CACHE_SIZE];
    float                 total_score;
} MultiTermScorer;

static float multi_tsc_score(Scorer *self)
{
    return MTSc(self)->total_score * MTSc(self)->weight_value
        * sim_decode_norm(self->similarity, MTSc(self)->norms[self->doc]);
}

static bool multi_tsc_next(Scorer *self)
{
    int curr_doc;
    float total_score = 0.0;
    TermDocEnumWrapper *tdew;
    MultiTermScorer *mtsc = MTSc(self);
    PriorityQueue *tdew_pq = mtsc->tdew_pq;
    if (tdew_pq == NULL) {
        TermDocEnumWrapper **tdew_a = mtsc->tdew_a;
        int i;
        tdew_pq = pq_new(mtsc->tdew_cnt, (lt_ft)tdew_less_than, (free_ft)NULL);
        for (i = mtsc->tdew_cnt - 1; i >= 0; i--) {
            if (tdew_next(tdew_a[i])) {
                pq_push(tdew_pq, tdew_a[i]);
            }
        }
        mtsc->tdew_pq = tdew_pq;
    }
    
    tdew = (TermDocEnumWrapper *)pq_top(tdew_pq);
    if (tdew == NULL) {
        return false;
    }

    self->doc = curr_doc = tdew->doc;
    do {
        int freq = tdew->freq;
        if (freq < SCORE_CACHE_SIZE) {
            total_score += mtsc->score_cache[freq] * tdew->boost;
        }
        else {
            total_score += sim_tf(self->similarity, (float)freq) * tdew->boost;
        }

        if (tdew_next(tdew)) {
            pq_down(tdew_pq);
        }
        else {
            pq_pop(tdew_pq);
        }

    } while (((tdew = (TermDocEnumWrapper *)pq_top(tdew_pq)) != NULL)
             && tdew->doc == curr_doc);
    mtsc->total_score = total_score;
    return true;
}

static bool multi_tsc_advance_to(Scorer *self, int target_doc_num)
{
    PriorityQueue *tdew_pq = MTSc(self)->tdew_pq;
    TermDocEnumWrapper *tdew;
    if (tdew_pq == NULL) {
        MultiTermScorer *mtsc = MTSc(self);
        TermDocEnumWrapper **tdew_a = mtsc->tdew_a;
        int i;
        tdew_pq = pq_new(mtsc->tdew_cnt, (lt_ft)tdew_less_than, (free_ft)NULL);
        for (i = mtsc->tdew_cnt - 1; i >= 0; i--) {
            tdew_skip_to(tdew_a[i], target_doc_num);
            pq_push(tdew_pq, tdew_a[i]);
        }
        MTSc(self)->tdew_pq = tdew_pq;
    }
    if (tdew_pq->size == 0) {
        self->doc = -1;
        return false;
    }
    while ((tdew = (TermDocEnumWrapper *)pq_top(tdew_pq)) != NULL
           && (target_doc_num > tdew->doc)) {
        if (tdew_skip_to(tdew, target_doc_num)) {
            pq_down(tdew_pq);
        }
        else {
            pq_pop(tdew_pq);
        }
    }
    return (pq_top(tdew_pq) == NULL) ? false : true;
}

static INLINE bool multi_tsc_skip_to(Scorer *self, int target_doc_num)
{
    return multi_tsc_advance_to(self, target_doc_num) && multi_tsc_next(self);
}

static Explanation *multi_tsc_explain(Scorer *self, int doc_num)
{
    MultiTermScorer *mtsc = MTSc(self);
    TermDocEnumWrapper *tdew;

    if (multi_tsc_advance_to(self, doc_num) &&
        (tdew = (TermDocEnumWrapper *)pq_top(mtsc->tdew_pq))->doc == doc_num) {

        PriorityQueue *tdew_pq = MTSc(self)->tdew_pq;
        Explanation *expl = expl_new(0.0, "The sum of:");
        int curr_doc = self->doc = tdew->doc;
        float total_score = 0.0;

        do {
            int freq = tdew->freq;
            expl_add_detail(expl,
                expl_new(sim_tf(self->similarity, (float)freq) * tdew->boost,
                         "tf(term_freq(%s:%s)=%d)^%f",
                         mtsc->field, tdew->term, freq, tdew->boost));

            total_score += sim_tf(self->similarity, (float)freq) * tdew->boost;

            /* maintain tdew queue, even though it probably won't get used
             * again */
            if (tdew_next(tdew)) {
                pq_down(tdew_pq);
            }
            else {
                pq_pop(tdew_pq);
            }

        } while (((tdew = (TermDocEnumWrapper *)pq_top(tdew_pq)) != NULL)
                 && tdew->doc == curr_doc);
        expl->value = total_score;
        return expl;
    }
    else {
        return expl_new(0.0, "None of the required terms exist in the index");
    }
}

static void multi_tsc_destroy(Scorer *self)
{
    int i;
    TermDocEnumWrapper **tdew_a = MTSc(self)->tdew_a;
    for (i = MTSc(self)->tdew_cnt - 1; i >= 0; i--) {
        tdew_destroy(tdew_a[i]);
    }
    free(tdew_a);
    if (MTSc(self)->tdew_pq) pq_destroy(MTSc(self)->tdew_pq);
    scorer_destroy_i(self);
}

static Scorer *multi_tsc_new(Weight *weight, const char *field,
                             TermDocEnumWrapper **tdew_a, int tdew_cnt,
                             uchar *norms)
{
    int i;
    Scorer *self = scorer_new(MultiTermScorer, weight->similarity);

    MTSc(self)->weight          = weight;
    MTSc(self)->field           = field;
    MTSc(self)->weight_value    = weight->value;
    MTSc(self)->tdew_a          = tdew_a;
    MTSc(self)->tdew_cnt        = tdew_cnt;
    MTSc(self)->norms           = norms;

    for (i = 0; i < SCORE_CACHE_SIZE; i++) {
        MTSc(self)->score_cache[i] = sim_tf(self->similarity, (float)i);
    }

    self->score                 = &multi_tsc_score;
    self->next                  = &multi_tsc_next;
    self->skip_to               = &multi_tsc_skip_to;
    self->explain               = &multi_tsc_explain;
    self->destroy               = &multi_tsc_destroy;

    return self;
}

/***************************************************************************
 * MultiTermWeight
 ***************************************************************************/

static char *multi_tw_to_s(Weight *self)
{
    return strfmt("MultiTermWeight(%f)", self->value);
}

static Scorer *multi_tw_scorer(Weight *self, IndexReader *ir)
{
    Scorer *multi_tsc = NULL;
    PriorityQueue *boosted_terms = MTQ(self->query)->boosted_terms;
    const int field_num = fis_get_field_num(ir->fis, MTQ(self->query)->field);

    if (boosted_terms->size > 0 && field_num >= 0) {
        int i;
        TermDocEnum *tde;
        TermEnum *te = ir->terms(ir, field_num);
        TermDocEnumWrapper **tdew_a = ALLOC_N(TermDocEnumWrapper *,
                                             boosted_terms->size);
        int tdew_cnt = 0;
        /* Priority queues skip the first element */
        for (i = boosted_terms->size; i > 0; i--) {
            char *term;
            BoostedTerm *bt = (BoostedTerm *)boosted_terms->heap[i];
            if ((term = te->skip_to(te, bt->term)) != NULL
                && strcmp(term, bt->term) == 0) {
                tde = ir->term_docs(ir);
                tde->seek_te(tde, te);
                tdew_a[tdew_cnt++] = tdew_new(bt->term, tde, bt->boost);
            }
        }
        te->close(te);
        if (tdew_cnt) {
            multi_tsc = multi_tsc_new(self, MTQ(self->query)->field, tdew_a,
                                      tdew_cnt, ir_get_norms_i(ir, field_num));
        }
        else {
            free(tdew_a);
        }
    }

    return multi_tsc;
}

Explanation *multi_tw_explain(Weight *self, IndexReader *ir, int doc_num)
{
    Explanation *expl;
    Explanation *idf_expl1;
    Explanation *idf_expl2;
    Explanation *query_expl;
    Explanation *qnorm_expl;
    Explanation *field_expl;
    Explanation *tf_expl;
    Scorer *scorer;
    uchar *field_norms;
    float field_norm;
    Explanation *field_norm_expl;

    char *query_str;
    MultiTermQuery *mtq = MTQ(self->query);
    const char *field = mtq->field;
    PriorityQueue *bt_pq = mtq->boosted_terms;
    int i;
    int total_doc_freqs = 0;
    char *doc_freqs = NULL;
    size_t len = 0, pos = 0;
    const int field_num = fis_get_field_num(ir->fis, field);

    if (field_num < 0) {
        return expl_new(0.0, "field \"%s\" does not exist in the index", field);
    }
    
    query_str = self->query->to_s(self->query, "");

    expl = expl_new(0.0, "weight(%s in %d), product of:", query_str, doc_num);

    len = 30;
    for (i = bt_pq->size; i > 0; i--) {
        len += strlen(((BoostedTerm *)bt_pq->heap[i])->term) + 30;
    }
    doc_freqs = ALLOC_N(char, len);
    for (i = bt_pq->size; i > 0; i--) {
        char *term = ((BoostedTerm *)bt_pq->heap[i])->term;
        int doc_freq = ir->doc_freq(ir, field_num, term);
        sprintf(doc_freqs + pos, "(%s=%d) + ", term, doc_freq);
        pos += strlen(doc_freqs + pos);
        total_doc_freqs += doc_freq;
    }
    pos -= 2; /* remove " + " from the end */
    sprintf(doc_freqs + pos, "= %d", total_doc_freqs);

    idf_expl1 = expl_new(self->idf, "idf(%s:<%s>)", field, doc_freqs);
    idf_expl2 = expl_new(self->idf, "idf(%s:<%s>)", field, doc_freqs);
    free(doc_freqs);

    /* explain query weight */
    query_expl = expl_new(0.0, "query_weight(%s), product of:", query_str);

    if (self->query->boost != 1.0) {
        expl_add_detail(query_expl, expl_new(self->query->boost, "boost"));
    }
    expl_add_detail(query_expl, idf_expl1);

    qnorm_expl = expl_new(self->qnorm, "query_norm");
    expl_add_detail(query_expl, qnorm_expl);

    query_expl->value = self->query->boost * self->idf * self->qnorm;

    expl_add_detail(expl, query_expl);

    /* explain field weight */
    field_expl = expl_new(0.0, "field_weight(%s in %d), product of:",
                          query_str, doc_num);
    free(query_str);

    if ((scorer = self->scorer(self, ir)) != NULL) {
        tf_expl = scorer->explain(scorer, doc_num);
        scorer->destroy(scorer);
    }
    else {
        tf_expl = expl_new(0.0, "no terms were found");
    }
    expl_add_detail(field_expl, tf_expl);
    expl_add_detail(field_expl, idf_expl2);

    field_norms = ir->get_norms(ir, field_num);
    field_norm = (field_norms != NULL)
        ? sim_decode_norm(self->similarity, field_norms[doc_num])
        : (float)0.0;
    field_norm_expl = expl_new(field_norm, "field_norm(field=%s, doc=%d)",
                               field, doc_num);

    expl_add_detail(field_expl, field_norm_expl);

    field_expl->value = tf_expl->value * self->idf * field_norm;

    /* combine them */
    if (query_expl->value == 1.0) {
        expl_destroy(expl);
        return field_expl;
    }
    else {
        expl->value = (query_expl->value * field_expl->value);
        expl_add_detail(expl, field_expl);
        return expl;
    }
}

static Weight *multi_tw_new(Query *query, Searcher *searcher)
{
    int i;
    int doc_freq         = 0;
    Weight *self         = w_new(Weight, query);
    const char *field    = MTQ(query)->field;
    PriorityQueue *bt_pq = MTQ(query)->boosted_terms;

    self->scorer         = &multi_tw_scorer;
    self->explain        = &multi_tw_explain;
    self->to_s           = &multi_tw_to_s;

    self->similarity     = query->get_similarity(query, searcher);
    self->value          = query->boost;
    self->idf            = 0.0;

    for (i = bt_pq->size; i > 0; i--) {
        doc_freq += searcher->doc_freq(searcher, field,
                                       ((BoostedTerm *)bt_pq->heap[i])->term);
    }
    self->idf += sim_idf(self->similarity, doc_freq,
                         searcher->max_doc(searcher));

    return self;
}


/***************************************************************************
 * MultiTermQuery
 ***************************************************************************/

static char *multi_tq_to_s(Query *self, const char *curr_field) 
{
    int i;
    PriorityQueue *boosted_terms = MTQ(self)->boosted_terms, *bt_pq_clone;
    BoostedTerm *bt;
    char *buffer, *bptr;
    char *field = MTQ(self)->field;
    int flen = (int)strlen(field);
    int tlen = 0;

    /* Priority queues skip the first element */
    for (i = boosted_terms->size; i > 0; i--) {
        tlen += (int)strlen(((BoostedTerm *)boosted_terms->heap[i])->term) + 35;
    }

    bptr = buffer = ALLOC_N(char, tlen + flen + 35);

    if (strcmp(curr_field, field) != 0) {
        sprintf(bptr, "%s:", field);
        bptr += flen + 1;
    }

    *(bptr++) = '"';
    bt_pq_clone = pq_clone(boosted_terms);
    while ((bt = (BoostedTerm *)pq_pop(bt_pq_clone)) != NULL) {
        sprintf(bptr, "%s", bt->term);
        bptr += (int)strlen(bptr);

        if (bt->boost != 1.0) {
            *bptr = '^';
            dbl_to_s(++bptr, bt->boost);
            bptr += (int)strlen(bptr);
        }

        *(bptr++) = '|';
    }
    pq_destroy(bt_pq_clone);

    if (bptr[-1] == '"') {
        bptr++; /* handle zero term case */
    }
    bptr[-1] =  '"'; /* delete last '|' char */
    bptr[ 0] = '\0';
    
    if (self->boost != 1.0) {
        *bptr = '^';
        dbl_to_s(++bptr, self->boost);
    }

    return buffer;
}

static void multi_tq_destroy_i(Query *self)
{
    free(MTQ(self)->field);
    pq_destroy(MTQ(self)->boosted_terms);
    q_destroy_i(self);
}

static void multi_tq_extract_terms(Query *self, HashSet *terms)
{
    int i;
    char *field = MTQ(self)->field;
    PriorityQueue *boosted_terms = MTQ(self)->boosted_terms;
    for (i = boosted_terms->size; i > 0; i--) {
        BoostedTerm *bt = (BoostedTerm *)boosted_terms->heap[i];
        hs_add(terms, term_new(field, bt->term));
    }
}

static unsigned long multi_tq_hash(Query *self)
{
    int i;
    unsigned long hash = str_hash(MTQ(self)->field);
    PriorityQueue *boosted_terms = MTQ(self)->boosted_terms;
    for (i = boosted_terms->size; i > 0; i--) {
        BoostedTerm *bt = (BoostedTerm *)boosted_terms->heap[i];
        hash ^= str_hash(bt->term) ^ float2int(bt->boost);
    }
    return hash;
}

static int multi_tq_eq(Query *self, Query *o)
{
    int i;
    PriorityQueue *boosted_terms1 = MTQ(self)->boosted_terms;
    PriorityQueue *boosted_terms2 = MTQ(o)->boosted_terms;

    if (strcmp(MTQ(self)->field, MTQ(o)->field) != 0
        || boosted_terms1->size != boosted_terms2->size) {
        return false;
    }
    for (i = boosted_terms1->size; i > 0; i--) {
        BoostedTerm *bt1 = (BoostedTerm *)boosted_terms1->heap[i];
        BoostedTerm *bt2 = (BoostedTerm *)boosted_terms2->heap[i];
        if ((strcmp(bt1->term, bt2->term) != 0) || (bt1->boost != bt2->boost)) {
            return false;
        }
    }
    return true;
}

static MatchVector *multi_tq_get_matchv_i(Query *self, MatchVector *mv,
                                          TermVector *tv)
{
    if (strcmp(tv->field, MTQ(self)->field) == 0) {
        int i;
        PriorityQueue *boosted_terms = MTQ(self)->boosted_terms;
        for (i = boosted_terms->size; i > 0; i--) {
            int j;
            BoostedTerm *bt = (BoostedTerm *)boosted_terms->heap[i];
            TVTerm *tv_term = tv_get_tv_term(tv, bt->term);
            if (tv_term) {
                for (j = 0; j < tv_term->freq; j++) {
                    int pos = tv_term->positions[j];
                    matchv_add(mv, pos, pos);
                }
            }
        }
    }
    return mv;
}

Query *multi_tq_new_conf(const char *field, int max_terms, float min_boost)
{
    Query *self;

    if (max_terms <= 0) {
        RAISE(ARG_ERROR, ":max_terms must be greater than or equal to zero. "
              "%d < 0. ", max_terms);
    }

    self                     = q_new(MultiTermQuery);

    MTQ(self)->field         = estrdup(field);
    MTQ(self)->boosted_terms = pq_new(max_terms,
                                      (lt_ft)&boosted_term_less_than,
                                      (free_ft)&boosted_term_destroy);
    MTQ(self)->min_boost     = min_boost;

    self->type               = MULTI_TERM_QUERY;
    self->to_s               = &multi_tq_to_s;
    self->extract_terms      = &multi_tq_extract_terms;
    self->hash               = &multi_tq_hash;
    self->eq                 = &multi_tq_eq;
    self->destroy_i          = &multi_tq_destroy_i;
    self->create_weight_i    = &multi_tw_new;
    self->get_matchv_i       = &multi_tq_get_matchv_i;

    return self;
}

Query *multi_tq_new(const char *field)
{
    return multi_tq_new_conf(field, MULTI_TERM_QUERY_MAX_TERMS, 0.0);
}

void multi_tq_add_term_boost(Query *self, const char *term, float boost)
{
    if (boost > MTQ(self)->min_boost && term && term[0]) {
        BoostedTerm *bt = boosted_term_new(term, boost);
        PriorityQueue *bt_pq = MTQ(self)->boosted_terms;
        pq_insert(bt_pq, bt);
        if (pq_full(bt_pq)) {
            MTQ(self)->min_boost = ((BoostedTerm *)pq_top(bt_pq))->boost;
        }
    }
}

void multi_tq_add_term(Query *self, const char *term)
{
    multi_tq_add_term_boost(self, term, 1.0);
}
