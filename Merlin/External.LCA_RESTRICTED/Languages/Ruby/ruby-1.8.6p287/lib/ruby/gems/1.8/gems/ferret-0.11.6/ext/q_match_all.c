#include "search.h"
#include <string.h>

/***************************************************************************
 *
 * MatchAllScorer
 *
 ***************************************************************************/

#define MASc(scorer) ((MatchAllScorer *)(scorer))

typedef struct MatchAllScorer
{
    Scorer          super;
    IndexReader    *ir;
    int             max_doc;
    float           score;
} MatchAllScorer;

static float masc_score(Scorer *self)
{
    return MASc(self)->score;
}

static bool masc_next(Scorer *self)
{
    while (self->doc < (MASc(self)->max_doc - 1)) {
        self->doc++;
        if (!MASc(self)->ir->is_deleted(MASc(self)->ir, self->doc)) {
            return true;
        }
    }
    return false;
}

static bool masc_skip_to(Scorer *self, int doc_num)
{
    self->doc = doc_num - 1;
    return masc_next(self);
}

static Explanation *masc_explain(Scorer *self, int doc_num)
{
    (void)self;
    (void)doc_num;
    return expl_new(1.0, "MatchAllScorer");
}

static Scorer *masc_new(Weight *weight, IndexReader *ir)
{
    Scorer *self        = scorer_new(MatchAllScorer, weight->similarity);

    MASc(self)->ir      = ir;
    MASc(self)->max_doc = ir->max_doc(ir);
    MASc(self)->score   = weight->value;

    self->doc           = -1;
    self->score         = &masc_score;
    self->next          = &masc_next;
    self->skip_to       = &masc_skip_to;
    self->explain       = &masc_explain;
    self->destroy       = &scorer_destroy_i;

    return self;
}

/***************************************************************************
 *
 * Weight
 *
 ***************************************************************************/

static char *maw_to_s(Weight *self)
{
    return strfmt("MatchAllWeight(%f)", self->value);
}

static Explanation *maw_explain(Weight *self, IndexReader *ir, int doc_num)
{
    Explanation *expl;
    if (!ir->is_deleted(ir, doc_num)) {
        expl = expl_new(self->value, "MatchAllQuery: product of:");
        expl_add_detail(expl, expl_new(self->query->boost, "boost"));
        expl_add_detail(expl, expl_new(self->qnorm, "query_norm"));
    } else {
        expl = expl_new(self->value,
                        "MatchAllQuery: doc %d was deleted", doc_num);
    }

    return expl;
}

static Weight *maw_new(Query *query, Searcher *searcher)
{
    Weight *self        = w_new(Weight, query);

    self->scorer        = &masc_new;
    self->explain       = &maw_explain;
    self->to_s          = &maw_to_s;

    self->similarity    = query->get_similarity(query, searcher);
    self->idf           = 1.0;

    return self;
}

/***************************************************************************
 *
 * MatchAllQuery
 *
 ***************************************************************************/

char *maq_to_s(Query *self, const char *field)
{
    (void)field;
    if (self->boost == 1.0) {
        return estrdup("*");
    } else {
        return strfmt("*^%f", self->boost);
    }
}

static unsigned long maq_hash(Query *self)
{
    (void)self;
    return 0;
}

static int maq_eq(Query *self, Query *o)
{
    (void)self; (void)o;
    return true;
}

Query *maq_new()
{
    Query *self = q_new(Query);

    self->type = MATCH_ALL_QUERY;
    self->to_s = &maq_to_s;
    self->hash = &maq_hash;
    self->eq = &maq_eq;
    self->destroy_i = &q_destroy_i;
    self->create_weight_i = &maw_new;

    return self;
}

