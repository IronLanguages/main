#include <string.h>
#include "search.h"

#define TQ(query) ((TermQuery *)(query))
#define TSc(scorer) ((TermScorer *)(scorer))

/***************************************************************************
 *
 * TermScorer
 *
 ***************************************************************************/

#define SCORE_CACHE_SIZE 32
#define TDE_READ_SIZE 32

typedef struct TermScorer
{
    Scorer          super;
    int             docs[TDE_READ_SIZE];
    int             freqs[TDE_READ_SIZE];
    int             pointer;
    int             pointer_max;
    float           score_cache[SCORE_CACHE_SIZE];
    Weight         *weight;
    TermDocEnum    *tde;
    uchar          *norms;
    float           weight_value;
} TermScorer;

static float tsc_score(Scorer *self)
{
    TermScorer *ts = TSc(self);
    int freq = ts->freqs[ts->pointer];
    float score;
    /* compute tf(f)*weight */
    if (freq < SCORE_CACHE_SIZE) {    /* check cache */
        score = ts->score_cache[freq];  /* cache hit */
    }
    else {
        /* cache miss */
        score = sim_tf(self->similarity, (float)freq) * ts->weight_value;
    }
    /* normalize for field */
    score *= sim_decode_norm(self->similarity, ts->norms[self->doc]);
    return score;
}

static bool tsc_next(Scorer *self)
{
    TermScorer *ts = TSc(self);

    ts->pointer++;
    if (ts->pointer >= ts->pointer_max) {
        /* refill buffer */
        ts->pointer_max = ts->tde->read(ts->tde, ts->docs, ts->freqs,
                                        TDE_READ_SIZE);
        if (ts->pointer_max != 0) {
            ts->pointer = 0;
        }
        else {
            return false;
        }
    }
    self->doc = ts->docs[ts->pointer];
    return true;
}

static bool tsc_skip_to(Scorer *self, int doc_num)
{
    TermScorer *ts = TSc(self);
    TermDocEnum *tde = ts->tde;

    /* first scan in cache */
    while (++(ts->pointer) < ts->pointer_max) {
        if (ts->docs[ts->pointer] >= doc_num) {
            self->doc = ts->docs[ts->pointer];
            return true;
        }
    }

    /* not found in cache, seek underlying stream */
    if (tde->skip_to(tde, doc_num)) {
        ts->pointer_max = 1;
        ts->pointer = 0;
        ts->docs[0] = self->doc = tde->doc_num(tde);
        ts->freqs[0] = tde->freq(tde);
        return true;
    }
    else {
        return false;
    }
}

static Explanation *tsc_explain(Scorer *self, int doc_num)
{
    TermScorer *ts = TSc(self);
    Query *query = ts->weight->get_query(ts->weight);
    int tf = 0;

    tsc_skip_to(self, doc_num);
    if (self->doc == doc_num) {
        tf = ts->freqs[ts->pointer];
    }
    return expl_new(sim_tf(self->similarity, (float)tf),
                    "tf(term_freq(%s:%s)=%d)",
                    TQ(query)->field, TQ(query)->term, tf);
}

static void tsc_destroy(Scorer *self)
{
    TSc(self)->tde->close(TSc(self)->tde);
    scorer_destroy_i(self);
}

static Scorer *tsc_new(Weight *weight, TermDocEnum *tde, uchar *norms)
{
    int i;
    Scorer *self            = scorer_new(TermScorer, weight->similarity);
    TSc(self)->weight       = weight;
    TSc(self)->tde          = tde;
    TSc(self)->norms        = norms;
    TSc(self)->weight_value = weight->value;

    for (i = 0; i < SCORE_CACHE_SIZE; i++) {
        TSc(self)->score_cache[i]
            = sim_tf(self->similarity, (float)i) * TSc(self)->weight_value;
    }

    self->score             = &tsc_score;
    self->next              = &tsc_next;
    self->skip_to           = &tsc_skip_to;
    self->explain           = &tsc_explain;
    self->destroy           = &tsc_destroy;
    return self;
}

/***************************************************************************
 *
 * TermWeight
 *
 ***************************************************************************/

static Scorer *tw_scorer(Weight *self, IndexReader *ir)
{
    TermQuery *tq = TQ(self->query);
    TermDocEnum *tde = ir_term_docs_for(ir, tq->field, tq->term);
    if (!tde) {
        return NULL;
    }

    return tsc_new(self, tde, ir_get_norms(ir, tq->field));
}

static Explanation *tw_explain(Weight *self, IndexReader *ir, int doc_num)
{
    Explanation *qnorm_expl;
    Explanation *field_expl;
    Scorer *scorer;
    Explanation *tf_expl;
    uchar *field_norms;
    float field_norm;
    Explanation *field_norm_expl;

    char *query_str = self->query->to_s(self->query, "");
    TermQuery *tq = TQ(self->query);
    char *term = tq->term;
    char *field = tq->field;

    Explanation *expl = expl_new(0.0, "weight(%s in %d), product of:",
                                 query_str, doc_num);

    /* We need two of these as it's included in both the query explanation
     * and the field explanation */
    Explanation *idf_expl1 = expl_new(self->idf, "idf(doc_freq=%d)",
                                      ir_doc_freq(ir, field, term));
    Explanation *idf_expl2 = expl_new(self->idf, "idf(doc_freq=%d)",
                                      ir_doc_freq(ir, field, term));

    /* explain query weight */
    Explanation *query_expl = expl_new(0.0, "query_weight(%s), product of:",
                                       query_str);
    free(query_str);

    if (self->query->boost != 1.0) {
        expl_add_detail(query_expl, expl_new(self->query->boost, "boost"));
    }

    expl_add_detail(query_expl, idf_expl1);

    qnorm_expl = expl_new(self->qnorm, "query_norm");
    expl_add_detail(query_expl, qnorm_expl);

    query_expl->value = self->query->boost
        * idf_expl1->value * qnorm_expl->value;

    expl_add_detail(expl, query_expl);

    /* explain field weight */
    field_expl = expl_new(0.0, "field_weight(%s:%s in %d), product of:",
                          field, term, doc_num);

    scorer = self->scorer(self, ir);
    tf_expl = scorer->explain(scorer, doc_num);
    scorer->destroy(scorer);
    expl_add_detail(field_expl, tf_expl);
    expl_add_detail(field_expl, idf_expl2);

    field_norms = ir_get_norms(ir, field);
    field_norm = (field_norms 
                  ? sim_decode_norm(self->similarity, field_norms[doc_num]) 
                  : (float)0.0);
    field_norm_expl = expl_new(field_norm, "field_norm(field=%s, doc=%d)",
                               field, doc_num);

    expl_add_detail(field_expl, field_norm_expl);

    field_expl->value = tf_expl->value * idf_expl2->value
        * field_norm_expl->value;

    /* combine them */
    if (query_expl->value == 1.0) {
        expl_destroy(expl);
        return field_expl;
    } else {
        expl->value = (query_expl->value * field_expl->value);
        expl_add_detail(expl, field_expl);
        return expl;
    }
}

static char *tw_to_s(Weight *self)
{
    return strfmt("TermWeight(%f)", self->value);
}

static Weight *tw_new(Query *query, Searcher *searcher)
{
    Weight *self    = w_new(Weight, query);
    self->scorer    = &tw_scorer;
    self->explain   = &tw_explain;
    self->to_s      = &tw_to_s;

    self->similarity = query->get_similarity(query, searcher);
    self->idf = sim_idf(self->similarity,
                        searcher->doc_freq(searcher,
                                           TQ(query)->field,
                                           TQ(query)->term),
                        searcher->max_doc(searcher)); /* compute idf */

    return self;
}

/***************************************************************************
 *
 * TermQuery
 *
 ***************************************************************************/

static void tq_destroy(Query *self)
{
    free(TQ(self)->term);
    free(TQ(self)->field);
    q_destroy_i(self);
}

static char *tq_to_s(Query *self, const char *field)
{
    size_t flen = strlen(TQ(self)->field);
    size_t tlen = strlen(TQ(self)->term);
    char *buffer = ALLOC_N(char, 34 + flen + tlen);
    char *b = buffer;
    if (strcmp(field, TQ(self)->field) != 0) {
        memcpy(b, TQ(self)->field, sizeof(char) * flen);
        b[flen] = ':';
        b += flen + 1;
    }
    memcpy(b, TQ(self)->term, tlen);
    b += tlen;
    *b = 0;
    if (self->boost != 1.0) {
        *b = '^';
        dbl_to_s(b+1, self->boost);
    }
    return buffer;
}

static void tq_extract_terms(Query *self, HashSet *terms)
{
    hs_add(terms, term_new(TQ(self)->field, TQ(self)->term));
}

static unsigned long tq_hash(Query *self)
{
    return str_hash(TQ(self)->term) ^ str_hash(TQ(self)->field);
}

static int tq_eq(Query *self, Query *o)
{
    return (strcmp(TQ(self)->term, TQ(o)->term) == 0) 
        && (strcmp(TQ(self)->field, TQ(o)->field) == 0);
}

static MatchVector *tq_get_matchv_i(Query *self, MatchVector *mv,
                                    TermVector *tv)
{
    if (strcmp(tv->field, TQ(self)->field) == 0) {
        int i;
        TVTerm *tv_term = tv_get_tv_term(tv, TQ(self)->term);
        if (tv_term) {
            for (i = 0; i < tv_term->freq; i++) {
                int pos = tv_term->positions[i];
                matchv_add(mv, pos, pos);
            }
        }
    }
    return mv;
}

Query *tq_new(const char *field, const char *term)
{
    Query *self             = q_new(TermQuery);

    TQ(self)->field         = estrdup(field);
    TQ(self)->term          = estrdup(term);

    self->type              = TERM_QUERY;
    self->extract_terms     = &tq_extract_terms;
    self->to_s              = &tq_to_s;
    self->hash              = &tq_hash;
    self->eq                = &tq_eq;

    self->destroy_i         = &tq_destroy;
    self->create_weight_i   = &tw_new;
    self->get_matchv_i      = &tq_get_matchv_i;

    return self;
}
