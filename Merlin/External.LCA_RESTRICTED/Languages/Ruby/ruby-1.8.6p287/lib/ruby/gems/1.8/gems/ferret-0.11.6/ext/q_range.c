#include <string.h>
#include "search.h"

/*****************************************************************************
 *
 * Range
 *
 *****************************************************************************/

typedef struct Range
{
    char *field;
    char *lower_term;
    char *upper_term;
    bool include_lower : 1;
    bool include_upper : 1;
} Range;

static char *range_to_s(Range *range, const char *field, float boost)
{
    char *buffer, *b;
    size_t flen, llen, ulen;

    flen = strlen(range->field);
    llen = range->lower_term ? strlen(range->lower_term) : 0;
    ulen = range->upper_term ? strlen(range->upper_term) : 0;
    buffer = ALLOC_N(char, flen + llen + ulen + 40);
    b = buffer;

    if (strcmp(field, range->field)) {
        memcpy(buffer, range->field, flen * sizeof(char));
        b += flen;
        *b = ':';
        b++;
    }

    if (range->lower_term) {
        *b = range->include_lower ? '[' : '{';
        b++;
        memcpy(b, range->lower_term, llen);
        b += llen;
    } else {
        *b = '<';
        b++;
    }

    if (range->upper_term && range->lower_term) {
        *b = ' '; b++;
    }

    if (range->upper_term) {
        memcpy(b, range->upper_term, ulen);
        b += ulen;
        *b = range->include_upper ? ']' : '}';
        b++;
    } else {
        *b = '>';
        b++;
    }

    *b = 0;
    if (boost != 1.0) {
        *b = '^';
        dbl_to_s(b + 1, boost);
    }
    return buffer;
}

static void range_destroy(Range *range)
{
    free(range->field);
    free(range->lower_term);
    free(range->upper_term);
    free(range);
}

static unsigned long range_hash(Range *filt)
{
    return filt->include_lower | (filt->include_upper << 1)
        | ((str_hash(filt->field)
            ^ (filt->lower_term ? str_hash(filt->lower_term) : 0)
            ^ (filt->upper_term ? str_hash(filt->upper_term) : 0)) << 2);
}

static int str_eq(char *s1, char *s2)
{
    return (s1 && s2 && (strcmp(s1, s2) == 0)) || (s1 == s2);
}

static int range_eq(Range *filt, Range *o)
{
    return (str_eq(filt->field, o->field)
            && str_eq(filt->lower_term, o->lower_term)
            && str_eq(filt->upper_term, o->upper_term)
            && (filt->include_lower == o->include_lower)
            && (filt->include_upper == o->include_upper));
}

Range *range_new(const char *field, const char *lower_term,
                 const char *upper_term, bool include_lower,
                 bool include_upper)
{
    Range *range; 

    if (!lower_term && !upper_term) {
        RAISE(ARG_ERROR, "Nil bounds for range. A range must include either "
              "lower bound or an upper bound");
    }
    if (include_lower && !lower_term) {
        RAISE(ARG_ERROR, "Lower bound must be non-nil to be inclusive. That "
              "is, if you specify :include_lower => true when you create a "
              "range you must include a :lower_term");
    }
    if (include_upper && !upper_term) {
        RAISE(ARG_ERROR, "Upper bound must be non-nil to be inclusive. That "
              "is, if you specify :include_upper => true when you create a "
              "range you must include a :upper_term");
    }
    if (upper_term && lower_term && (strcmp(upper_term, lower_term) < 0)) {
        RAISE(ARG_ERROR, "Upper bound must be greater than lower bound. "
              "\"%s\" < \"%s\"", upper_term, lower_term);
    }

    range = ALLOC(Range);

    range->field = estrdup((char *)field);
    range->lower_term = lower_term ? estrdup(lower_term) : NULL;
    range->upper_term = upper_term ? estrdup(upper_term) : NULL;
    range->include_lower = include_lower;
    range->include_upper = include_upper;
    return range;
}

/***************************************************************************
 *
 * RangeFilter
 *
 ***************************************************************************/

typedef struct RangeFilter
{
    Filter super;
    Range *range;
} RangeFilter;

#define RF(filt) ((RangeFilter *)(filt))

static void rfilt_destroy_i(Filter *filt)
{
    range_destroy(RF(filt)->range);
    filt_destroy_i(filt);
}

static char *rfilt_to_s(Filter *filt)
{
    char *rstr = range_to_s(RF(filt)->range, "", 1.0);
    char *rfstr = strfmt("RangeFilter< %s >", rstr);
    free(rstr);
    return rfstr;
}

static BitVector *rfilt_get_bv_i(Filter *filt, IndexReader *ir)
{
    BitVector *bv = bv_new_capa(ir->max_doc(ir));
    Range *range = RF(filt)->range;
    FieldInfo *fi = fis_get_field(ir->fis, range->field);
    /* the field info exists we need to add docs to the bit vector, otherwise
     * we just return an empty bit vector */
    if (fi) {
        const char *lower_term =
            range->lower_term ? range->lower_term : EMPTY_STRING;
        const char *upper_term = range->upper_term;
        const bool include_upper = range->include_upper;
        const int field_num = fi->number;
        char *term;
        TermEnum* te;
        TermDocEnum *tde;
        bool check_lower;

        te = ir->terms(ir, field_num);
        if (te->skip_to(te, lower_term) == NULL) {
            te->close(te);
            return bv;
        }

        check_lower = !(range->include_lower || (lower_term == EMPTY_STRING));

        tde = ir->term_docs(ir);
        term = te->curr_term;
        do {
            if (!check_lower
                || (strcmp(term, lower_term) > 0)) {
                check_lower = false;
                if (upper_term) {
                    int compare = strcmp(upper_term, term);
                    /* Break if upper term is greater than or equal to upper
                     * term and include_upper is false or ther term is fully
                     * greater than upper term. This is optimized so that only
                     * one check is done except in last check or two */
                    if ((compare <= 0)
                        && (!include_upper || (compare < 0))) {
                        break;
                    }
                }
                /* we have a good term, find the docs */
                /* text is already pointing to term buffer text */
                tde->seek_te(tde, te);
                while (tde->next(tde)) {
                    bv_set(bv, tde->doc_num(tde));
                    /* printf("Setting %d\n", tde->doc_num(tde)); */
                }
            }
        } while (te->next(te));

        tde->close(tde);
        te->close(te);
    }

    return bv;
}

static unsigned long rfilt_hash(Filter *filt)
{
    return range_hash(RF(filt)->range);
}

static int rfilt_eq(Filter *filt, Filter *o)
{
    return range_eq(RF(filt)->range, RF(o)->range);
}

Filter *rfilt_new(const char *field,
                  const char *lower_term, const char *upper_term,
                  bool include_lower, bool include_upper)
{
    Filter *filt = filt_new(RangeFilter);
    RF(filt)->range =  range_new(field, lower_term, upper_term,
                                 include_lower, include_upper); 

    filt->get_bv_i  = &rfilt_get_bv_i;
    filt->hash      = &rfilt_hash;
    filt->eq        = &rfilt_eq;
    filt->to_s      = &rfilt_to_s;
    filt->destroy_i = &rfilt_destroy_i;
    return filt;
}

/*****************************************************************************
 *
 * RangeQuery
 *
 *****************************************************************************/

#define RQ(query) ((RangeQuery *)(query))
typedef struct RangeQuery
{
    Query f;
    Range *range;
} RangeQuery;

static char *rq_to_s(Query *self, const char *field)
{
    return range_to_s(RQ(self)->range, field, self->boost);
}

static void rq_destroy(Query *self)
{
    range_destroy(RQ(self)->range);
    q_destroy_i(self);
}

static MatchVector *rq_get_matchv_i(Query *self, MatchVector *mv,
                                    TermVector *tv)
{
    Range *range = RQ(((ConstantScoreQuery *)self)->original)->range;
    if (strcmp(tv->field, range->field) == 0) {
        int i, j;
        char *upper_text = range->upper_term;
        char *lower_text = range->lower_term;
        int upper_limit = range->include_upper ? 1 : 0;
        int lower_limit = range->include_lower ? 1 : 0;

        for (i = tv->term_cnt - 1; i >= 0; i--) {
            TVTerm *tv_term = &(tv->terms[i]);
            char *text = tv_term->text;
            if ((!upper_text || strcmp(text, upper_text) < upper_limit) && 
                (!lower_text || strcmp(lower_text, text) < lower_limit)) {

                for (j = 0; j < tv_term->freq; j++) {
                    int pos = tv_term->positions[j];
                    matchv_add(mv, pos, pos);
                }
            }
        }
    }
    return mv;
}

static Query *rq_rewrite(Query *self, IndexReader *ir)
{
    Query *csq;
    Range *r = RQ(self)->range;
    Filter *filter = rfilt_new(r->field, r->lower_term, r->upper_term,
                               r->include_lower, r->include_upper);
    (void)ir;
    csq = csq_new_nr(filter);
    ((ConstantScoreQuery *)csq)->original = self;
    csq->get_matchv_i = &rq_get_matchv_i;
    return (Query *)csq;
}

static unsigned long rq_hash(Query *self)
{
    return range_hash(RQ(self)->range);
}

static int rq_eq(Query *self, Query *o)
{
    return range_eq(RQ(self)->range, RQ(o)->range);
}

Query *rq_new_less(const char *field, const char *upper_term,
                   bool include_upper)
{
    return rq_new(field, NULL, upper_term, false, include_upper);
}

Query *rq_new_more(const char *field, const char *lower_term,
                   bool include_lower)
{
    return rq_new(field, lower_term, NULL, include_lower, false);
}

Query *rq_new(const char *field, const char *lower_term,
              const char *upper_term, bool include_lower, bool include_upper)
{
    Query *self     = q_new(RangeQuery);

    RQ(self)->range = range_new(field, lower_term, upper_term,
                                include_lower, include_upper); 

    self->type              = RANGE_QUERY;
    self->rewrite           = &rq_rewrite;
    self->to_s              = &rq_to_s;
    self->hash              = &rq_hash;
    self->eq                = &rq_eq;
    self->destroy_i         = &rq_destroy;
    self->create_weight_i   = &q_create_weight_unsup;
    return self;
}
