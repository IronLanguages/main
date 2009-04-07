#include "search.h"
#include <string.h>

/***************************************************************************
 *
 * FilteredQueryScorer
 *
 ***************************************************************************/

#define FQSc(scorer) ((FilteredQueryScorer *)(scorer))
#define FQQ(query) ((FilteredQuery *)(query))

typedef struct FilteredQueryScorer
{
    Scorer      super;
    Scorer     *sub_scorer;
    BitVector  *bv;
} FilteredQueryScorer;

float fqsc_score(Scorer *self)
{
    Scorer *sub_sc = FQSc(self)->sub_scorer;
    return sub_sc->score(sub_sc);
}

bool fqsc_next(Scorer *self)
{
    Scorer *sub_sc = FQSc(self)->sub_scorer;
    BitVector *bv = FQSc(self)->bv;
    while (sub_sc->next(sub_sc)) {
        self->doc = sub_sc->doc;
        if (bv_get(bv, self->doc)) return true;
    }
    return false;
}

bool fqsc_skip_to(Scorer *self, int doc_num)
{
    Scorer *sub_sc = FQSc(self)->sub_scorer;
    BitVector *bv = FQSc(self)->bv;
    if (sub_sc->skip_to(sub_sc, doc_num)) {
        self->doc = sub_sc->doc;
        do {
            if (bv_get(bv, self->doc)) return true;
        } while (sub_sc->next(sub_sc));
    }
    return false;
}

Explanation *fqsc_explain(Scorer *self, int doc_num)
{
    Scorer *sub_sc = FQSc(self)->sub_scorer;
    return sub_sc->explain(sub_sc, doc_num);
}

void fqsc_destroy(Scorer *self)
{
    FilteredQueryScorer *fqsc = FQSc(self);
    fqsc->sub_scorer->destroy(fqsc->sub_scorer);
    scorer_destroy_i(self);
}

Scorer *fqsc_new(Scorer *scorer, BitVector *bv, Similarity *sim)
{
    Scorer *self            = scorer_new(FilteredQueryScorer, sim);

    FQSc(self)->sub_scorer  = scorer;
    FQSc(self)->bv          = bv;

    self->score   = &fqsc_score;
    self->next    = &fqsc_next;
    self->skip_to = &fqsc_skip_to;
    self->explain = &fqsc_explain;
    self->destroy = &fqsc_destroy;

    return self;
}

/***************************************************************************
 *
 * Weight
 *
 ***************************************************************************/

#define FQW(weight) ((FilteredQueryWeight *)(weight))
typedef struct FilteredQueryWeight
{
    Weight  super;
    Weight *sub_weight;
} FilteredQueryWeight;

static char *fqw_to_s(Weight *self)
{
    return strfmt("FilteredQueryWeight(%f)", self->value);
}

static float fqw_sum_of_squared_weights(Weight *self)
{
    Weight *sub_weight = FQW(self)->sub_weight;
    return sub_weight->sum_of_squared_weights(sub_weight);
}

static void fqw_normalize(Weight *self, float normalization_factor)
{
    Weight *sub_weight = FQW(self)->sub_weight;
    sub_weight->normalize(sub_weight, normalization_factor);
}

static float fqw_get_value(Weight *self)
{
    Weight *sub_weight = FQW(self)->sub_weight;
    return sub_weight->get_value(sub_weight);
}

static Explanation *fqw_explain(Weight *self, IndexReader *ir, int doc_num)
{
    Weight *sub_weight = FQW(self)->sub_weight;
    return sub_weight->explain(sub_weight, ir, doc_num);
}

static Scorer *fqw_scorer(Weight *self, IndexReader *ir)
{
    Weight *sub_weight = FQW(self)->sub_weight;
    Scorer *scorer = sub_weight->scorer(sub_weight, ir);
    Filter *filter = FQQ(self->query)->filter;

    return fqsc_new(scorer, filt_get_bv(filter, ir), self->similarity);
}

static void fqw_destroy(Weight *self)
{
    Weight *sub_weight = FQW(self)->sub_weight;
    sub_weight->destroy(sub_weight);
    w_destroy(self);
}

static Weight *fqw_new(Query *query, Weight *sub_weight, Similarity *sim)
{
    Weight *self = w_new(FilteredQueryWeight, query);

    FQW(self)->sub_weight           = sub_weight;

    self->get_value                 = &fqw_get_value;
    self->normalize                 = &fqw_normalize;
    self->scorer                    = &fqw_scorer;
    self->explain                   = &fqw_explain;
    self->to_s                      = &fqw_to_s;
    self->destroy                   = &fqw_destroy;
    self->sum_of_squared_weights    = &fqw_sum_of_squared_weights;

    self->similarity                = sim;
    self->idf                       = 1.0;
    self->value                     = sub_weight->value;

    return self;
}

/***************************************************************************
 *
 * FilteredQuery
 *
 ***************************************************************************/

static char *fq_to_s(Query *self, const char *field)
{
    FilteredQuery *fq = FQQ(self);
    char *filter_str = fq->filter->to_s(fq->filter);
    char *query_str = fq->query->to_s(fq->query, field);
    char *buffer;
    if (self->boost == 1.0) {
        buffer = strfmt("FilteredQuery(query:%s, filter:%s)",
                        query_str, filter_str);
    } else {
        buffer = strfmt("FilteredQuery(query:%s, filter:%s)^%f",
                        query_str, filter_str, self->boost);
    }
    free(filter_str);
    free(query_str);
    return buffer;;
}

void fq_destroy(Query *self)
{
    filt_deref(FQQ(self)->filter);
    q_deref(FQQ(self)->query);
    q_destroy_i(self);
}

Weight *fq_new_weight(Query *self, Searcher *searcher)
{
    Query *sub_query = FQQ(self)->query;
    return fqw_new(self, q_weight(sub_query, searcher),
                      searcher->similarity);
}

Query *fq_new(Query *query, Filter *filter)
{
    Query *self = q_new(FilteredQuery);

    FQQ(self)->query        = query;
    FQQ(self)->filter       = filter;

    self->type              = FILTERED_QUERY;
    self->to_s              = &fq_to_s;
    self->destroy_i         = &fq_destroy;
    self->create_weight_i   = &fq_new_weight;

    return self;
}
