#include "search.h"
#include <string.h>

/***************************************************************************
 *
 * ConstantScoreScorer
 *
 ***************************************************************************/

#define CScQ(query) ((ConstantScoreQuery *)(query))
#define CScSc(scorer) ((ConstantScoreScorer *)(scorer))

typedef struct ConstantScoreScorer
{
    Scorer      super;
    BitVector  *bv;
    float       score;
} ConstantScoreScorer;

static float cssc_score(Scorer *self)
{
    return CScSc(self)->score;
}

static bool cssc_next(Scorer *self)
{
    return ((self->doc = bv_scan_next(CScSc(self)->bv)) >= 0);
}

static bool cssc_skip_to(Scorer *self, int doc_num)
{
    return ((self->doc = bv_scan_next_from(CScSc(self)->bv, doc_num)) >= 0);
}

static Explanation *cssc_explain(Scorer *self, int doc_num)
{
    (void)self; (void)doc_num;
    return expl_new(1.0, "ConstantScoreScorer");
}

static Scorer *cssc_new(Weight *weight, IndexReader *ir)
{
    Scorer *self    = scorer_new(ConstantScoreScorer, weight->similarity);
    Filter *filter  = CScQ(weight->query)->filter;

    CScSc(self)->score  = weight->value;
    CScSc(self)->bv     = filt_get_bv(filter, ir);

    self->score     = &cssc_score;
    self->next      = &cssc_next;
    self->skip_to   = &cssc_skip_to;
    self->explain   = &cssc_explain;
    self->destroy   = &scorer_destroy_i;
    return self;
}

/***************************************************************************
 *
 * ConstantScoreWeight
 *
 ***************************************************************************/

static char *csw_to_s(Weight *self)
{
    return strfmt("ConstantScoreWeight(%f)", self->value);
}

static Explanation *csw_explain(Weight *self, IndexReader *ir, int doc_num)
{
    Filter *filter = CScQ(self->query)->filter;
    Explanation *expl;
    char *filter_str = filter->to_s(filter);
    BitVector *bv = filt_get_bv(filter, ir);

    if (bv_get(bv, doc_num)) {
        expl = expl_new(self->value,
                        "ConstantScoreQuery(%s), product of:", filter_str);
        expl_add_detail(expl, expl_new(self->query->boost, "boost"));
        expl_add_detail(expl, expl_new(self->qnorm, "query_norm"));
    }
    else {
        expl = expl_new(self->value,
                        "ConstantScoreQuery(%s), does not match id %d",
                        filter_str, doc_num);
    }
    free(filter_str);
    return expl;
}

static Weight *csw_new(Query *query, Searcher *searcher)
{
    Weight *self        = w_new(Weight, query);

    self->scorer        = &cssc_new;
    self->explain       = &csw_explain;
    self->to_s          = &csw_to_s;

    self->similarity    = query->get_similarity(query, searcher);
    self->idf           = 1.0;

    return self;
}

/***************************************************************************
 *
 * ConstantScoreQuery
 *
 ***************************************************************************/

static char *csq_to_s(Query *self, const char *field)
{
    Filter *filter = CScQ(self)->filter;
    char *filter_str = filter->to_s(filter);
    char *buffer;
    (void)field;
    if (self->boost == 1.0) {
        buffer = strfmt("ConstantScore(%s)", filter_str);
    }
    else {
        buffer = strfmt("ConstantScore(%s)^%f", filter_str, self->boost);
    }
    free(filter_str);
    return buffer;;
}

static void csq_destroy(Query *self)
{
    filt_deref(CScQ(self)->filter);
    q_destroy_i(self);
}

static unsigned long csq_hash(Query *self)
{
    return filt_hash(CScQ(self)->filter);
}

static int csq_eq(Query *self, Query *o)
{
    return filt_eq(CScQ(self)->filter, CScQ(o)->filter);
}

Query *csq_new_nr(Filter *filter)
{
    Query *self = q_new(ConstantScoreQuery);
    CScQ(self)->filter = filter;

    self->type              = CONSTANT_QUERY;
    self->to_s              = &csq_to_s;
    self->hash              = &csq_hash;
    self->eq                = &csq_eq;
    self->destroy_i         = &csq_destroy;
    self->create_weight_i   = &csw_new;

    return self;
}

Query *csq_new(Filter *filter)
{
    REF(filter);
    return csq_new_nr(filter);
}
