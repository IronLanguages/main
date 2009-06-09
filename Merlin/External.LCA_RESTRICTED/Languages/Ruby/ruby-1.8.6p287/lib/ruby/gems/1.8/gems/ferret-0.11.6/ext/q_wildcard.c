#include <string.h>
#include "search.h"

/****************************************************************************
 *
 * WildCardQuery
 *
 ****************************************************************************/

#define WCQ(query) ((WildCardQuery *)(query))

static char *wcq_to_s(Query *self, const char *current_field) 
{
    char *buffer, *bptr;
    const char *field = WCQ(self)->field;
    const char *pattern = WCQ(self)->pattern;
    size_t flen = strlen(field);
    size_t plen = strlen(pattern);
    bptr = buffer = ALLOC_N(char, plen + flen + 35);

    if (strcmp(field, current_field) != 0) {
        sprintf(bptr, "%s:", field);
        bptr += flen + 1;
    }
    sprintf(bptr, "%s", pattern);
    bptr += plen;

    if (self->boost != 1.0) {
        *bptr = '^';
        dbl_to_s(++bptr, self->boost);
    }

    return buffer;
}

bool wc_match(const char *pattern, const char *text)
{
    const char *p = pattern, *t = text, *xt; 

    /* include '\0' as we need to match empty string */
    const char *text_last = t + strlen(t);

    for (;; p++, t++) {

        /* end of text so make sure end of pattern doesn't matter */
        if (*t == '\0') {
            while (*p) {
                if (*p != WILD_STRING) {
                    return false;
                }
                p++;
            }
            return true;
        }

        /* If we've gone past the end of the pattern, return false. */
        if (*p == '\0') {
            return false;
        }

        /* Match a single character, so continue. */
        if (*p == WILD_CHAR) {
            continue;
        }

        if (*p == WILD_STRING) {
            /* Look at the character beyond the '*'. */
            p++;
            /* Examine the string, starting at the last character. */
            for (xt = text_last; xt >= t; xt--) {
                if (wc_match(p, xt)) return true;
            }
            return false;
        }
        if (*p != *t) {
            return false;
        }
    }

    return false;
}

static Query *wcq_rewrite(Query *self, IndexReader *ir)
{
    Query *q;
    const char *field = WCQ(self)->field;
    const char *pattern = WCQ(self)->pattern;
    const char *first_star = strchr(pattern, WILD_STRING);
    const char *first_ques = strchr(pattern, WILD_CHAR);

    if (NULL == first_star && NULL == first_ques) {
        q = tq_new(field, pattern);
        q->boost = self->boost;
    }
    else {
        const int field_num = fis_get_field_num(ir->fis, field);
        q = multi_tq_new_conf(field, MTQMaxTerms(self), 0.0);

        if (field_num >= 0) {
            TermEnum *te;
            char prefix[MAX_WORD_SIZE] = "";
            int prefix_len;

            pattern = (first_ques && (!first_star || first_star > first_ques))
                ? first_ques : first_star;

            prefix_len = (int)(pattern - WCQ(self)->pattern);

            if (prefix_len > 0) {
                memcpy(prefix, WCQ(self)->pattern, prefix_len);
                prefix[prefix_len] = '\0';
            }

            te = ir->terms_from(ir, field_num, prefix);

            if (te != NULL) {
                const char *term = te->curr_term;
                const char *pat_term = term + prefix_len;
                do { 
                    if (prefix && strncmp(term, prefix, prefix_len) != 0) {
                        break;
                    }

                    if (wc_match(pattern, pat_term)) {
                        multi_tq_add_term(q, term);
                    }
                } while (te->next(te) != NULL);
                te->close(te);
            }
        }
    }

    return q;
}

static void wcq_destroy(Query *self)
{
    free(WCQ(self)->field);
    free(WCQ(self)->pattern);
    q_destroy_i(self);
}

static unsigned long wcq_hash(Query *self)
{
    return str_hash(WCQ(self)->field) ^ str_hash(WCQ(self)->pattern);
}

static int wcq_eq(Query *self, Query *o)
{
    return (strcmp(WCQ(self)->pattern, WCQ(o)->pattern) == 0) 
        && (strcmp(WCQ(self)->field,   WCQ(o)->field) == 0);
}

Query *wcq_new(const char *field, const char *pattern)
{
    Query *self = q_new(WildCardQuery);

    WCQ(self)->field        = estrdup(field);
    WCQ(self)->pattern      = estrdup(pattern);
    MTQMaxTerms(self)       = WILD_CARD_QUERY_MAX_TERMS;

    self->type              = WILD_CARD_QUERY;
    self->rewrite           = &wcq_rewrite;
    self->to_s              = &wcq_to_s;
    self->hash              = &wcq_hash;
    self->eq                = &wcq_eq;
    self->destroy_i         = &wcq_destroy;
    self->create_weight_i   = &q_create_weight_unsup;

    return self;
}
