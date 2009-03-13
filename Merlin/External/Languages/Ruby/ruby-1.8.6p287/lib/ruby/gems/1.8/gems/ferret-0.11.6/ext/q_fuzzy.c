#include <string.h>
#include "search.h"
#include "helper.h"

/****************************************************************************
 *
 * FuzzyStuff
 *
 * The main method here is the fuzq_score method which scores a term against
 * another term. The other methods all act in support.
 *
 ****************************************************************************/

static INLINE int fuzq_calculate_max_distance(FuzzyQuery *fuzq, int m) 
{
    return (int)((1.0 - fuzq->min_sim) * (MIN(fuzq->text_len, m) + fuzq->pre_len));
}

static void fuzq_initialize_max_distances(FuzzyQuery *fuzq)
{
    int i;
    for (i = 0; i < TYPICAL_LONGEST_WORD; i++) {
        fuzq->max_distances[i] = fuzq_calculate_max_distance(fuzq, i);
    }
}

static INLINE int fuzq_get_max_distance(FuzzyQuery *fuzq, int m)
{
    return (m < TYPICAL_LONGEST_WORD) ? fuzq->max_distances[m]
        : fuzq_calculate_max_distance(fuzq, m);
}

/**
 * The following algorithm is taken from Bob Carpenter's FuzzyTermEnum
 * implentation here;
 *
 * http://mail-archives.apache.org/mod_mbox/lucene-java-dev/200606.mbox/%3c448F0E8C.3050901@alias-i.com%3e
 */
float fuzq_score(FuzzyQuery *fuzq, const char *target)
{
    const int m = (int)strlen(target);
    const int n = fuzq->text_len;

    if (n == 0)  {
        /* we don't have anything to compare.  That means if we just add
         * the letters for m we get the new word */
        return fuzq->pre_len == 0 ? 0.0f : 1.0f - ((float) m / fuzq->pre_len);
    }
    else if (m == 0) {
        return fuzq->pre_len == 0 ? 0.0f : 1.0f - ((float) n / fuzq->pre_len);
    }
    else {
        int i, j, prune;
        int *d_curr, *d_prev;
        const char *text = fuzq->text;
        const int max_distance = fuzq_get_max_distance(fuzq, m);

        /*
         printf("n%dm%dmd%ddiff%d<%s><%s>\n", n, m, max_distance, m-n,
               fuzq->text, target);
         */
        if (max_distance < ((m > n) ? (m-n) : (n-m))) { /* abs */
            /* Just adding the characters of m to n or vice-versa results in
             * too many edits for example "pre" length is 3 and "prefixes"
             * length is 8. We can see that given this optimal circumstance,
             * the edit distance cannot be less than 5 which is 8-3 or more
             * precisesly Math.abs(3-8). If our maximum edit distance is 4,
             * then we can discard this word without looking at it. */
            return 0.0f;
        }

        d_curr = fuzq->da;
        d_prev = d_curr + n + 1;

        /* init array */
        for (j = 0; j <= n; j++) {
            d_curr[j] = j;
        }

        /* start computing edit distance */
        for (i = 0; i < m;) {
           char s_i = target[i];
           /* swap d_current into d_prev */
           int *d_tmp = d_prev;
           d_prev = d_curr;
           d_curr = d_tmp;
           prune = (d_curr[0] = ++i) > max_distance;

           for (j = 0; j < n; j++) {
               d_curr[j + 1] = (s_i == text[j])
                   ? min3(d_prev[j + 1] + 1, d_curr[j] + 1, d_prev[j])
                   : min3(d_prev[j + 1], d_curr[j], d_prev[j]) + 1;
               if (prune && d_curr[j + 1] <= max_distance) {
                   prune = false;
               }
           }
           if (prune) {
               return 0.0f;
           }
        }

        /*
        printf("<%f, d_curr[n] = %d min_len = %d>",
               1.0f - ((float)d_curr[m] / (float) (fuzq->pre_len + min2(n, m))),
               d_curr[m], fuzq->pre_len + min2(n, m));
               */

        /* this will return less than 0.0 when the edit distance is greater
         * than the number of characters in the shorter word.  but this was
         * the formula that was previously used in FuzzyTermEnum, so it has
         * not been changed (even though min_sim must be greater than 0.0) */
        return 1.0f - ((float)d_curr[n] / (float) (fuzq->pre_len + min2(n, m)));
    }
}

/****************************************************************************
 *
 * FuzzyQuery
 *
 ****************************************************************************/

#define FzQ(query) ((FuzzyQuery *)(query))

static char *fuzq_to_s(Query *self, const char *curr_field) 
{
    char *buffer, *bptr;
    char *term = FzQ(self)->term;
    char *field = FzQ(self)->field;
    int tlen = (int)strlen(term);
    int flen = (int)strlen(field);
    bptr = buffer = ALLOC_N(char, tlen + flen + 70);

    if (strcmp(curr_field, field) != 0) {
        sprintf(bptr, "%s:", field);
        bptr += flen + 1;
    }

    sprintf(bptr, "%s~", term);
    bptr += tlen + 1;
    if (FzQ(self)->min_sim != 0.5) {
        dbl_to_s(bptr, FzQ(self)->min_sim);
        bptr += strlen(bptr);
    }

    if (self->boost != 1.0) {
        *bptr = '^';
        dbl_to_s(++bptr, self->boost);
    }

    return buffer;
}

static Query *fuzq_rewrite(Query *self, IndexReader *ir)
{
    Query *q;
    FuzzyQuery *fuzq = FzQ(self);

    const char *term = fuzq->term;
    const char *field = fuzq->field;
    const int field_num = fis_get_field_num(ir->fis, field);

    if (field_num < 0) {
        q = bq_new(true);
    }
    else if (fuzq->pre_len >= (int)strlen(term)) {
        q = tq_new(field, term);
    }
    else {
        TermEnum *te;
        char *prefix = NULL;
        int pre_len = fuzq->pre_len;

        q = multi_tq_new_conf(fuzq->field, MTQMaxTerms(self), fuzq->min_sim);

        if (pre_len > 0) {
            prefix = ALLOC_N(char, pre_len + 1);
            strncpy(prefix, term, pre_len);
            prefix[pre_len] = '\0';
            te = ir->terms_from(ir, field_num, prefix);
        }
        else {
            te = ir->terms(ir, field_num);
        }

        fuzq->scale_factor = (float)(1.0 / (1.0 - fuzq->min_sim));
        fuzq->text = term + pre_len;
        fuzq->text_len = (int)strlen(fuzq->text);
        fuzq->da = REALLOC_N(fuzq->da, int, fuzq->text_len * 2 + 2);
        fuzq_initialize_max_distances(fuzq);

        if (te) {
            const char *curr_term = te->curr_term;
            const char *curr_suffix = curr_term + pre_len;
            float score = 0.0;


            do { 
                if ((prefix && strncmp(curr_term, prefix, pre_len) != 0)) {
                    break;
                }

                score = fuzq_score(fuzq, curr_suffix);
                /*
                 printf("%s:%s:%f < %f\n", curr_term, term, score, min_score);
                 */
                multi_tq_add_term_boost(q, curr_term, score);

            } while (te->next(te) != NULL);

            te->close(te);
        }
        free(prefix);
    }

    return q;
}

static void fuzq_destroy(Query *self)
{
    free(FzQ(self)->term);
    free(FzQ(self)->field);
    free(FzQ(self)->da);
    q_destroy_i(self);
}

static unsigned long fuzq_hash(Query *self)
{
    return str_hash(FzQ(self)->term) ^ str_hash(FzQ(self)->field)
        ^ float2int(FzQ(self)->min_sim) ^ FzQ(self)->pre_len;
}

static int fuzq_eq(Query *self, Query *o)
{
    FuzzyQuery *fq1 = FzQ(self);
    FuzzyQuery *fq2 = FzQ(o);

    return (strcmp(fq1->term, fq2->term) == 0)
        && (strcmp(fq1->field, fq2->field) == 0)
        && (fq1->pre_len == fq2->pre_len)
        && (fq1->min_sim == fq2->min_sim);
}

Query *fuzq_new_conf(const char *field, const char *term,
                     float min_sim, int pre_len, int max_terms)
{
    Query *self = q_new(FuzzyQuery);

    FzQ(self)->field      = estrdup(field);
    FzQ(self)->term       = estrdup(term);
    FzQ(self)->pre_len    = pre_len ? pre_len : DEF_PRE_LEN;
    FzQ(self)->min_sim    = min_sim ? min_sim : DEF_MIN_SIM;
    MTQMaxTerms(self)     = max_terms ? max_terms : DEF_MAX_TERMS;

    self->type            = FUZZY_QUERY;
    self->to_s            = &fuzq_to_s;
    self->hash            = &fuzq_hash;
    self->eq              = &fuzq_eq;
    self->rewrite         = &fuzq_rewrite;
    self->destroy_i       = &fuzq_destroy;
    self->create_weight_i = &q_create_weight_unsup;

    return self;
}

Query *fuzq_new(const char *field, const char *term)
{
    return fuzq_new_conf(field, term, 0.0f, 0, 0);
}
