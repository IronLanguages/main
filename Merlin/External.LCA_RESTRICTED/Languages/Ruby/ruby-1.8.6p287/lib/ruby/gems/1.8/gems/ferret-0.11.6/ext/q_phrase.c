#include <string.h>
#include <limits.h>
#include "search.h"
#include "array.h"

#define PhQ(query) ((PhraseQuery *)(query))

static int phrase_pos_cmp(const void *p1, const void *p2)
{
    int pos1 = ((PhrasePosition *)p1)->pos;
    int pos2 = ((PhrasePosition *)p2)->pos;
    if (pos1 > pos2) {
        return 1;
    }
    if (pos1 < pos2) {
        return -1;
    }
    return strcmp(((PhrasePosition *)p1)->terms[0],
                  ((PhrasePosition *)p2)->terms[0]);
}


/***************************************************************************
 *
 * PhraseScorer
 *
 ***************************************************************************/

/***************************************************************************
 * PhPos
 ***************************************************************************/

#define PP(p) ((PhPos *)(p))
typedef struct PhPos
{
    TermDocEnum *tpe;
    int offset;
    int count;
    int doc;
    int position;
} PhPos;

static bool pp_next(PhPos *self)
{
    TermDocEnum *tpe = self->tpe;
    if (!tpe->next(tpe)) {
        tpe->close(tpe);            /* close stream */
        self->tpe = NULL;
        self->doc = INT_MAX;        /* sentinel value */
        return false;
    }
    self->doc = tpe->doc_num(tpe);
    self->position = 0;
    return true;
}

static bool pp_skip_to(PhPos *self, int doc_num)
{
    TermDocEnum *tpe = self->tpe;
    if (!tpe->skip_to(tpe, doc_num)) {
        tpe->close(tpe);            /* close stream */
        self->tpe = NULL;
        self->doc = INT_MAX;        /* sentinel value */
        return false;
    }
    self->doc = tpe->doc_num(tpe);
    self->position = 0;
    return true;
}

static bool pp_next_position(PhPos *self)
{
    TermDocEnum *tpe = self->tpe;
    self->count--;
    if (self->count >= 0) {         /* read subsequent pos's */
        self->position = tpe->next_position(tpe) - self->offset;
        return true;
    }
    else {
        return false;
    }
}

static bool pp_first_position(PhPos *self)
{
    TermDocEnum *tpe = self->tpe;
    self->count = tpe->freq(tpe);   /* read first pos */
    return pp_next_position(self);
}

/*
static char *pp_to_s(PhPos *self)
{
    return strfmt("pp->(doc => %d, position => %d)", self->doc, self->position);
}
*/

#define PP_pp(p) (*(PhPos **)p)
static int pp_cmp(const void *const p1, const void *const p2)
{
    int cmp = PP_pp(p1)->doc - PP_pp(p2)->doc;
    if (cmp == 0) {
        return PP_pp(p1)->position - PP_pp(p2)->position;
    }
    else {
        return cmp;
    }
}

static int pp_pos_cmp(const void *const p1, const void *const p2)
{
    return PP_pp(p1)->position - PP_pp(p2)->position;
}

static bool pp_less_than(const PhPos *pp1, const PhPos *pp2)
{
    /* docs will all be equal when this method is used */
    return pp1->position < pp2->position;
    /*
    if (PP(p)->doc == PP(p)->doc) {
        return PP(p)->position < PP(p)->position;
    }
    else {
        return PP(p)->doc < PP(p)->doc;
    }
    */
}

void pp_destroy(PhPos *pp)
{
    if (pp->tpe) {
        pp->tpe->close(pp->tpe);
    }
    free(pp);
}

PhPos *pp_new(TermDocEnum *tpe, int offset)
{
    PhPos *self = ALLOC(PhPos);

    self->tpe = tpe;
    self->count = self->doc = self->position = -1;
    self->offset = offset;

    return self;
}

/***************************************************************************
 * PhraseScorer
 ***************************************************************************/

#define PhSc(scorer) ((PhraseScorer *)(scorer))

typedef struct PhraseScorer
{
    Scorer  super;
    float (*phrase_freq)(Scorer *self);
    float   freq;
    uchar  *norms;
    float   value;
    Weight *weight;
    PhPos **phrase_pos;
    int     pp_first_idx;
    int     pp_cnt;
    int     slop;
    bool    first_time : 1;
    bool    more : 1;
} PhraseScorer;

static void phsc_init(PhraseScorer *phsc)
{
    int i;
    for (i = phsc->pp_cnt - 1; i >= 0; i--) {
        if (!(phsc->more = pp_next(phsc->phrase_pos[i]))) break;
    }

    if (phsc->more) {
        qsort(phsc->phrase_pos, phsc->pp_cnt,
              sizeof(PhPos *), &pp_cmp);
        phsc->pp_first_idx = 0;
    }
}

static bool phsc_do_next(Scorer *self)
{
    PhraseScorer *phsc = PhSc(self);
    const int pp_cnt = phsc->pp_cnt;
    int pp_first_idx = phsc->pp_first_idx;
    PhPos **phrase_positions = phsc->phrase_pos;

    PhPos *first = phrase_positions[pp_first_idx];
    PhPos *last  = phrase_positions[PREV_NUM(pp_first_idx, pp_cnt)];

    while (phsc->more) {
        /* find doc with all the terms */
        while (phsc->more && first->doc < last->doc) {
            /* skip first upto last */
            phsc->more = pp_skip_to(first, last->doc);
            last = first;
            pp_first_idx = NEXT_NUM(pp_first_idx, pp_cnt);
            first = phrase_positions[pp_first_idx];
        }

        if (phsc->more) {
            /* pp_first_idx will be used by phrase_freq */
            phsc->pp_first_idx = pp_first_idx;

            /* found a doc with all of the terms */
            phsc->freq = phsc->phrase_freq(self);

            if (phsc->freq == 0.0) {            /* no match */
                /* continuing search so re-set first and last */
                pp_first_idx = phsc->pp_first_idx;
                first = phrase_positions[pp_first_idx];
                last =  phrase_positions[PREV_NUM(pp_first_idx, pp_cnt)];
                phsc->more = pp_next(last);     /* trigger further scanning */
            }
            else {
                self->doc = first->doc;
                return true;                    /* found a match */
            }

        }
    }
    return false;
}

static float phsc_score(Scorer *self)
{
    PhraseScorer *phsc = PhSc(self);
    float raw_score = sim_tf(self->similarity, phsc->freq) * phsc->value;
    /* normalize */
    return raw_score * sim_decode_norm(
        self->similarity,
        phsc->norms[phsc->phrase_pos[phsc->pp_first_idx]->doc]);
}

static bool phsc_next(Scorer *self)
{
    PhraseScorer *phsc = PhSc(self);
    if (phsc->first_time) {
        phsc_init(phsc);
        phsc->first_time = false;
    }
    else if (phsc->more) {
        /* trigger further scanning */
        phsc->more = pp_next(
            phsc->phrase_pos[PREV_NUM(phsc->pp_first_idx, phsc->pp_cnt)]);
    }

    return phsc_do_next(self);
}

static bool phsc_skip_to(Scorer *self, int doc_num)
{
    PhraseScorer *phsc = PhSc(self);
    int i;
    for (i = phsc->pp_cnt - 1; i >= 0; i--) {
        if (!(phsc->more = pp_skip_to(phsc->phrase_pos[i], doc_num))) {
            break;
        }
    }

    if (phsc->more) {
        qsort(phsc->phrase_pos, phsc->pp_cnt,
              sizeof(PhPos *), &pp_cmp);
        phsc->pp_first_idx = 0;
    }
    return phsc_do_next(self);
}

static Explanation *phsc_explain(Scorer *self, int doc_num)
{
    PhraseScorer *phsc = PhSc(self);
    float phrase_freq;

    phsc_skip_to(self, doc_num);

    phrase_freq = (self->doc == doc_num) ? phsc->freq : (float)0.0;
    return expl_new(sim_tf(self->similarity, phrase_freq), 
                    "tf(phrase_freq=%f)", phrase_freq);
}

static void phsc_destroy(Scorer *self)
{
    PhraseScorer *phsc = PhSc(self);
    int i;
    for (i = phsc->pp_cnt - 1; i >= 0; i--) {
        pp_destroy(phsc->phrase_pos[i]);
    }
    free(phsc->phrase_pos);
    scorer_destroy_i(self);
}

static Scorer *phsc_new(Weight *weight, TermDocEnum **term_pos_enum,
                        PhrasePosition *positions, int pos_cnt,
                        Similarity *similarity, uchar *norms)
{
    int i;
    Scorer *self                = scorer_new(PhraseScorer, similarity);

    PhSc(self)->weight          = weight;
    PhSc(self)->norms           = norms;
    PhSc(self)->value           = weight->value;
    PhSc(self)->phrase_pos      = ALLOC_N(PhPos *, pos_cnt);
    PhSc(self)->pp_first_idx    = 0;
    PhSc(self)->pp_cnt          = pos_cnt;
    PhSc(self)->slop            = 0;
    PhSc(self)->first_time      = true;
    PhSc(self)->more            = true;

    for (i = 0; i < pos_cnt; i++) {
        PhSc(self)->phrase_pos[i] = pp_new(term_pos_enum[i], positions[i].pos);
    }

    self->score     = &phsc_score;
    self->next      = &phsc_next;
    self->skip_to   = &phsc_skip_to;
    self->explain   = &phsc_explain;
    self->destroy   = &phsc_destroy;

    return self;
}

/***************************************************************************
 * ExactPhraseScorer
 ***************************************************************************/

static float ephsc_phrase_freq(Scorer *self)
{
    PhraseScorer *phsc = PhSc(self);
    int i;
    int pp_first_idx = 0;
    const int pp_cnt = phsc->pp_cnt;
    float freq = 0.0;
    PhPos **phrase_positions = phsc->phrase_pos;
    PhPos *first;
    PhPos *last;

    for (i = 0; i < pp_cnt; i++) {
        pp_first_position(phrase_positions[i]);
    }
    qsort(phrase_positions, pp_cnt, sizeof(PhPos *), &pp_pos_cmp);

    first = phrase_positions[0];
    last =  phrase_positions[pp_cnt - 1];

    /* scan to position with all terms */
    do {
        /* scan forward in first */
        while (first->position < last->position) {
            do {
                if (! pp_next_position(first)) {
                    /* maintain first position */
                    phsc->pp_first_idx = pp_first_idx;
                    return freq;
                }
            } while (first->position < last->position);
            last = first;
            pp_first_idx = NEXT_NUM(pp_first_idx, pp_cnt);
            first = phrase_positions[pp_first_idx];
        }
        freq += 1.0; /* all equal: a match */
    } while (pp_next_position(last));

    /* maintain first position */ 
    phsc->pp_first_idx = pp_first_idx;
    return freq;
}

static Scorer *exact_phrase_scorer_new(Weight *weight,
                                       TermDocEnum **term_pos_enum,
                                       PhrasePosition *positions, int pp_cnt,
                                       Similarity *similarity, uchar *norms)
{
    Scorer *self =
        phsc_new(weight, term_pos_enum, positions, pp_cnt, similarity, norms);

    PhSc(self)->phrase_freq = &ephsc_phrase_freq;
    return self;
}

/***************************************************************************
 * SloppyPhraseScorer
 ***************************************************************************/

static float sphsc_phrase_freq(Scorer *self)
{
    PhraseScorer *phsc = PhSc(self);
    PhPos *pp;
    PriorityQueue *pq = pq_new(phsc->pp_cnt, (lt_ft)&pp_less_than, NULL);
    const int pp_cnt = phsc->pp_cnt;

    int last_pos = 0, pos, next_pos, start, match_length, i;
    bool done = false;
    float freq = 0.0;

    for (i = 0; i < pp_cnt; i++) {
        pp = phsc->phrase_pos[i];
        pp_first_position(pp);
        if (pp->position > last_pos) {
            last_pos = pp->position;
        }
        pq_push(pq, pp);
    }

    do {
        pp = pq_pop(pq);
        pos = start = pp->position;
        next_pos = PP(pq_top(pq))->position;
        while (pos <= next_pos) {
            start = pos;        /* advance pp to min window */
            if (!pp_next_position(pp)) {
                done = true;    /* ran out of a positions for a term - done */
                break;
            }
            pos = pp->position;
        }

        match_length = last_pos - start;
        if (match_length <= phsc->slop) {
            /* score match */
            freq += sim_sloppy_freq(self->similarity, match_length);
        }

        if (pp->position > last_pos) {
            last_pos = pp->position;
        }
        pq_push(pq, pp);        /* restore pq */
    } while (!done);

    pq_destroy(pq);
    return freq;
}

static Scorer *sloppy_phrase_scorer_new(Weight *weight,
                                        TermDocEnum **term_pos_enum,
                                        PhrasePosition *positions,
                                        int pp_cnt, Similarity *similarity,
                                        int slop, uchar *norms)
{
    Scorer *self =
        phsc_new(weight, term_pos_enum, positions, pp_cnt, similarity, norms);

    PhSc(self)->slop        = slop;
    PhSc(self)->phrase_freq = &sphsc_phrase_freq;
    return self;
}

/***************************************************************************
 *
 * PhraseWeight
 *
 ***************************************************************************/

static char *phw_to_s(Weight *self)
{
    return strfmt("PhraseWeight(%f)", self->value);
}

static Scorer *phw_scorer(Weight *self, IndexReader *ir)
{
    int i;
    Scorer *phsc = NULL;
    PhraseQuery *phq = PhQ(self->query);
    TermDocEnum **tps, *tpe;
    PhrasePosition *positions = phq->positions;
    const int pos_cnt = phq->pos_cnt;
    const int field_num = fis_get_field_num(ir->fis, phq->field);
    
    if (pos_cnt == 0 || field_num < 0) {
        return NULL;
    }

    tps = ALLOC_N(TermDocEnum *, pos_cnt);

    for (i = 0; i < pos_cnt; i++) {
        char **terms = positions[i].terms;
        const int t_cnt = ary_size(terms);
        if (t_cnt == 1) {
            tpe = tps[i] = ir->term_positions(ir);
            tpe->seek(tpe, field_num, terms[0]);
        }
        else {
            tps[i] = mtdpe_new(ir, field_num, terms, t_cnt);
        }
        if (tps[i] == NULL) {
            /* free everything we just created and return NULL */
            int j;
            for (j = 0; j < i; j++) {
                tps[i]->close(tps[i]);
            }
            free(tps);
            return NULL;
        }
    }

    if (phq->slop == 0) {       /* optimize exact (common) case */
        phsc = exact_phrase_scorer_new(self, tps, positions, pos_cnt,
                                       self->similarity,
                                       ir_get_norms_i(ir, field_num));
    }
    else {
        phsc = sloppy_phrase_scorer_new(self, tps, positions, pos_cnt,
                                        self->similarity, phq->slop,
                                        ir_get_norms_i(ir, field_num));
    }
    free(tps);
    return phsc;
}

Explanation *phw_explain(Weight *self, IndexReader *ir, int doc_num)
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
    PhraseQuery *phq = PhQ(self->query);
    const int pos_cnt = phq->pos_cnt;
    PhrasePosition *positions = phq->positions;
    int i, j;
    char *doc_freqs = NULL;
    size_t len = 0, pos = 0;
    const int field_num = fis_get_field_num(ir->fis, phq->field);

    if (field_num < 0) {
        return expl_new(0.0, "field \"%s\" does not exist in the index", phq->field);
    }
    
    query_str = self->query->to_s(self->query, "");

    expl = expl_new(0.0, "weight(%s in %d), product of:", query_str, doc_num);

    /* ensure the phrase positions are in order for explanation */
    qsort(positions, pos_cnt, sizeof(PhrasePosition), &phrase_pos_cmp);

    for (i = 0; i < phq->pos_cnt; i++) {
        char **terms = phq->positions[i].terms;
        for (j = ary_size(terms) - 1; j >= 0; j--) {
            len += strlen(terms[j]) + 30;
        }
    }
    doc_freqs = ALLOC_N(char, len);
    for (i = 0; i < phq->pos_cnt; i++) {
        char **terms = phq->positions[i].terms;
        const int t_cnt = ary_size(terms);
        for (j = 0; j < t_cnt; j++) {
            char *term = terms[j];
            sprintf(doc_freqs + pos, "%s=%d, ",
                    term, ir->doc_freq(ir, field_num, term));
            pos += strlen(doc_freqs + pos);
        }
    }
    pos -= 2; /* remove ", " from the end */
    doc_freqs[pos] = 0;

    idf_expl1 = expl_new(self->idf, "idf(%s:<%s>)", phq->field, doc_freqs);
    idf_expl2 = expl_new(self->idf, "idf(%s:<%s>)", phq->field, doc_freqs);
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

    scorer = self->scorer(self, ir);
    tf_expl = scorer->explain(scorer, doc_num);
    scorer->destroy(scorer);
    expl_add_detail(field_expl, tf_expl);
    expl_add_detail(field_expl, idf_expl2);

    field_norms = ir->get_norms(ir, field_num);
    field_norm = (field_norms != NULL)
        ? sim_decode_norm(self->similarity, field_norms[doc_num])
        : (float)0.0;
    field_norm_expl = expl_new(field_norm, "field_norm(field=%s, doc=%d)",
                               phq->field, doc_num);

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

static Weight *phw_new(Query *query, Searcher *searcher)
{
    Weight *self        = w_new(Weight, query);

    self->scorer        = &phw_scorer;
    self->explain       = &phw_explain;
    self->to_s          = &phw_to_s;

    self->similarity    = query->get_similarity(query, searcher);
    self->value         = query->boost;
    self->idf           = sim_idf_phrase(self->similarity, PhQ(query)->field,
                                         PhQ(query)->positions,
                                         PhQ(query)->pos_cnt, searcher);
    return self;
}

/***************************************************************************
 *
 * PhraseQuery
 *
 ***************************************************************************/

/* ** TVPosEnum ** */
typedef struct TVPosEnum
{
    int index;
    int size;
    int offset;
    int pos;
    int positions[];
} TVPosEnum;

static bool tvpe_next(TVPosEnum *self)
{
    if (++(self->index) < self->size) {
        self->pos = self->positions[self->index] - self->offset;
        return true;
    }
    else {
        self->pos = -1;
        return false;
    }
}

static int tvpe_skip_to(TVPosEnum *self, int position)
{
    int i;
    int search_pos = position + self->offset;
    for (i = self->index + 1; i < self->size; i++) {
        if (self->positions[i] >= search_pos) {
            self->pos = self->positions[i] - self->offset;
            break;
        }
    }
    self->index = i;
    if (i == self->size) {
        self->pos = -1;
        return false;
    }
    return true;
}

static bool tvpe_lt(TVPosEnum *tvpe1, TVPosEnum *tvpe2)
{
    return tvpe1->pos < tvpe2->pos;
}

static TVPosEnum *tvpe_new(int *positions, int size, int offset)
{
    TVPosEnum *self = (TVPosEnum *)emalloc(sizeof(TVPosEnum)
                                           + size * sizeof(int));
    memcpy(self->positions, positions, size * sizeof(int));
    self->size = size;
    self->offset = offset;
    self->index = -1;
    self->pos = -1;
    return self;
}

static TVPosEnum *tvpe_new_merge(char **terms, int t_cnt, TermVector *tv,
                                 int offset)
{
    int i, total_positions = 0;
    PriorityQueue *tvpe_pq = pq_new(t_cnt, (lt_ft)tvpe_lt, &free);
    TVPosEnum *self = NULL;

    for (i = 0; i < t_cnt; i++) {
        TVTerm *tv_term = tv_get_tv_term(tv, terms[i]);
        if (tv_term) {
            TVPosEnum *tvpe = tvpe_new(tv_term->positions, tv_term->freq, 0);
            if (tvpe_next(tvpe)) {
                pq_push(tvpe_pq, tvpe);
                total_positions += tv_term->freq;
            }
            else {
                free(tvpe);
            }
        }
    }
    if (tvpe_pq->size == 0) {
        pq_destroy(tvpe_pq);
    }
    else {
        int index = 0;
        self = (TVPosEnum *)emalloc(sizeof(TVPosEnum)
                                    + total_positions * sizeof(int));
        self->size = total_positions;
        self->offset = offset;
        self->index = -1;
        self->pos = -1;
        while (tvpe_pq->size > 0) {
            TVPosEnum *top = (TVPosEnum *)pq_top(tvpe_pq);
            self->positions[index++] = top->pos;
            if (! tvpe_next(top)) {
                pq_pop(tvpe_pq);
                free(top);
            }
            else {
                pq_down(tvpe_pq);
            }
        }
        pq_destroy(tvpe_pq);
    }
    return self;
}

static TVPosEnum *get_tvpe(TermVector *tv, char **terms, int t_cnt, int offset)
{
    TVPosEnum *tvpe = NULL;
    if (t_cnt == 1) {
        TVTerm *tv_term = tv_get_tv_term(tv, terms[0]);
        if (tv_term) {
            tvpe = tvpe_new(tv_term->positions, tv_term->freq, offset);
        }
    }
    else {
        tvpe = tvpe_new_merge(terms, t_cnt, tv, offset);
    }
    return tvpe;
}

static MatchVector *phq_get_matchv_i(Query *self, MatchVector *mv,
                                     TermVector *tv)
{
    if (strcmp(tv->field, PhQ(self)->field) == 0) {
        const int pos_cnt = PhQ(self)->pos_cnt;
        int i;
        int slop = PhQ(self)->slop;
        bool done = false;

        if (slop > 0) {
            PriorityQueue *tvpe_pq = pq_new(pos_cnt, (lt_ft)tvpe_lt, &free);
            int last_pos = 0;
            for (i = 0; i < pos_cnt; i++) {
                PhrasePosition *pp = &(PhQ(self)->positions[i]);
                const int t_cnt = ary_size(pp->terms);
                TVPosEnum *tvpe = get_tvpe(tv, pp->terms, t_cnt, pp->pos);
                if (tvpe && tvpe_next(tvpe)) {
                    if (tvpe->pos > last_pos) {
                        last_pos = tvpe->pos;
                    }
                    pq_push(tvpe_pq, tvpe);
                }
                else {
                    done = true;
                    free(tvpe);
                    break;
                }
            }
            while (! done) {
                TVPosEnum *tvpe = pq_pop(tvpe_pq);
                int pos;
                int start = pos = tvpe->pos;
                int next_pos = ((TVPosEnum *)pq_top(tvpe_pq))->pos;
                while (pos <= next_pos) {
                    start = pos;
                    if (!tvpe_next(tvpe)) {
                        done = true;
                        break;
                    }
                    pos = tvpe->pos;
                }

                if (last_pos - start <= slop) {
                    int min, max = min = start + tvpe->offset;
                    for (i = tvpe_pq->size; i > 0; i--) {
                        TVPosEnum *t = (TVPosEnum *)tvpe_pq->heap[i];
                        int p = t->pos + t->offset;
                        max = p > max ? p : max;
                        min = p < min ? p : min;
                    }
                    matchv_add(mv, min, max);
                }
                if (tvpe->pos > last_pos) {
                    last_pos = tvpe->pos;
                }
                pq_push(tvpe_pq, tvpe);
            }

            pq_destroy(tvpe_pq);
        }
        else { /* exact match */
            TVPosEnum **tvpe_a = ALLOC_AND_ZERO_N(TVPosEnum *, pos_cnt);
            TVPosEnum *first, *last;
            int first_index = 0;
            done = false;
            qsort(PhQ(self)->positions, pos_cnt, sizeof(PhrasePosition),
                  &phrase_pos_cmp);
            for (i = 0; i < pos_cnt; i++) {
                PhrasePosition *pp = &(PhQ(self)->positions[i]);
                const int t_cnt = ary_size(pp->terms);
                TVPosEnum *tvpe = get_tvpe(tv, pp->terms, t_cnt, pp->pos);
                if (tvpe && ((i == 0 && tvpe_next(tvpe))
                             || tvpe_skip_to(tvpe, tvpe_a[i-1]->pos))) {
                    tvpe_a[i] = tvpe;
                }
                else {
                    done = true;
                    free(tvpe);
                    break;
                }
            }

            first = tvpe_a[0];
            last = tvpe_a[pos_cnt - 1];
            
            while (!done) {
                while (first->pos < last->pos) {
                    if (tvpe_skip_to(first, last->pos)) {
                        last = first;
                        first_index = NEXT_NUM(first_index, pos_cnt);
                        first = tvpe_a[first_index];
                    }
                    else {
                        done = true;
                        break;
                    }
                }
                if (!done) {
                    matchv_add(mv, tvpe_a[0]->pos + tvpe_a[0]->offset,
                               tvpe_a[pos_cnt-1]->pos + tvpe_a[pos_cnt-1]->offset); 
                }
                if (!tvpe_next(last)) {
                    done = true;
                }
            }
            for (i = 0; i < pos_cnt; i++) {
                free(tvpe_a[i]);
            }
            free(tvpe_a);
        }
    }
    return mv;
}


/* ** PhraseQuery besides highlighting stuff ** */

#define PhQ_INIT_CAPA 4

static void phq_extract_terms(Query *self, HashSet *term_set)
{
    PhraseQuery *phq = PhQ(self);
    int i, j;
    for (i = 0; i < phq->pos_cnt; i++) {
        char **terms = phq->positions[i].terms;
        for (j = ary_size(terms) - 1; j >= 0; j--) {
            hs_add(term_set, term_new(phq->field, terms[j]));
        }
    }
}

static char *phq_to_s(Query *self, const char *field)
{
    PhraseQuery *phq = PhQ(self);
    const int pos_cnt = phq->pos_cnt;
    PhrasePosition *positions = phq->positions;

    int i, j, buf_index = 0, pos, last_pos;
    size_t len = 0;
    char *buffer;

    if (phq->pos_cnt == 0) {
        if (strcmp(field, phq->field) != 0) {
            return strfmt("%s:\"\"", phq->field);
        }
        else {
            return estrdup("\"\"");
        }
    }

    /* sort the phrase positions by position */
    qsort(positions, pos_cnt, sizeof(PhrasePosition), &phrase_pos_cmp);

    len = strlen(phq->field) + 1;

    for (i = 0; i < pos_cnt; i++) {
        char **terms = phq->positions[i].terms;
        for (j = ary_size(terms) - 1; j >= 0; j--) {
            len += strlen(terms[j]) + 5;
        }
    }

    /* add space for extra <> characters and boost and slop */
    len += 100 + 3
        * (phq->positions[phq->pos_cnt - 1].pos - phq->positions[0].pos);

    buffer = ALLOC_N(char, len);

    if (strcmp(field, phq->field) != 0) {
        len = strlen(phq->field);
        memcpy(buffer, phq->field, len);
        buffer[len] = ':';
        buf_index += len + 1;
    }

    buffer[buf_index++] = '"';

    last_pos = positions[0].pos - 1;
    for (i = 0; i < pos_cnt; i++) {
        char **terms = positions[i].terms;
        const int t_cnt = ary_size(terms);

        pos = positions[i].pos;
        if (pos == last_pos) {
            buffer[buf_index - 1] = '&';
        }
        else {
            for (j = last_pos; j < pos - 1; j++) {
                memcpy(buffer + buf_index, "<> ", 3);
                buf_index += 3;
            }
        }

        last_pos = pos;
        for (j = 0; j < t_cnt; j++) {
            char *term = terms[j];
            len = strlen(term);
            memcpy(buffer + buf_index, term, len);
            buf_index += len;
            buffer[buf_index++] = '|';
        }
        buffer[buf_index-1] = ' '; /* change last '|' to ' ' */
    }

    if (buffer[buf_index-1] == ' ') {
        buf_index--;
    }

    buffer[buf_index++] = '"';
    buffer[buf_index] = 0;

    if (phq->slop != 0) {
        sprintf(buffer + buf_index, "~%d", phq->slop);
        buf_index += strlen(buffer + buf_index);
    }

    if (self->boost != 1.0) {
        buffer[buf_index++] = '^';
        dbl_to_s(buffer + buf_index, self->boost);
    }

    return buffer;
}

static void phq_destroy(Query *self)
{
    PhraseQuery *phq = PhQ(self);
    int i;
    free(phq->field);
    for (i = 0; i < phq->pos_cnt; i++) {
        ary_destroy(phq->positions[i].terms, &free);
    }
    free(phq->positions);
    q_destroy_i(self);
}

static Query *phq_rewrite(Query *self, IndexReader *ir)
{
    PhraseQuery *phq = PhQ(self);
    (void)ir;
    if (phq->pos_cnt == 1) {
        /* optimize one-position case */
        char **terms = phq->positions[0].terms;
        const int t_cnt = ary_size(terms);
        if (t_cnt == 1) {
            Query *tq = tq_new(phq->field, terms[0]);
            tq->boost = self->boost;
            return tq;
        }
        else {
            Query *q = multi_tq_new(phq->field);
            int i;
            for (i = 0; i < t_cnt; i++) {
                multi_tq_add_term(q, terms[i]);
            }
            q->boost = self->boost;
            return q;
        }
    } else {
        self->ref_cnt++;
        return self;
    }
}

static unsigned long phq_hash(Query *self)
{
    int i, j;
    PhraseQuery *phq = PhQ(self);
    unsigned long hash = str_hash(phq->field);
    for (i = 0; i < phq->pos_cnt; i++) {
        char **terms = phq->positions[i].terms;
        for (j = ary_size(terms) - 1; j >= 0; j--) {
            hash = (hash << 1) ^ (str_hash(terms[j])
                                  ^ phq->positions[i].pos);
        }
    }
    return (hash ^ phq->slop);
}

static int phq_eq(Query *self, Query *o)
{
    int i, j;
    PhraseQuery *phq1 = PhQ(self);
    PhraseQuery *phq2 = PhQ(o);
    if (phq1->slop != phq2->slop
        || strcmp(phq1->field, phq2->field) != 0
        || phq1->pos_cnt != phq2->pos_cnt) {
        return false;
    }
    for (i = 0; i < phq1->pos_cnt; i++) {
        char **terms1 = phq1->positions[i].terms;
        char **terms2 = phq2->positions[i].terms;
        const int t_cnt = ary_size(terms1);
        if (t_cnt != ary_size(terms2) 
            || phq1->positions[i].pos != phq2->positions[i].pos) {
            return false;
        }
        for (j = 0; j < t_cnt; j++) {
            if (strcmp(terms1[j], terms2[j]) != 0) {
                return false;
            }
        }
    }
    return true;
}

Query *phq_new(const char *field)
{
    Query *self = q_new(PhraseQuery);

    PhQ(self)->field        = estrdup(field);
    PhQ(self)->pos_cnt      = 0;
    PhQ(self)->pos_capa     = PhQ_INIT_CAPA;
    PhQ(self)->positions    = ALLOC_N(PhrasePosition, PhQ_INIT_CAPA);

    self->type              = PHRASE_QUERY;
    self->rewrite           = &phq_rewrite;
    self->extract_terms     = &phq_extract_terms;
    self->to_s              = &phq_to_s;
    self->hash              = &phq_hash;
    self->eq                = &phq_eq;
    self->destroy_i         = &phq_destroy;
    self->create_weight_i   = &phw_new;
    self->get_matchv_i      = &phq_get_matchv_i;
    return self;
}

void phq_add_term_abs(Query *self, const char *term, int position)
{
    PhraseQuery *phq = PhQ(self);
    int index = phq->pos_cnt;
    PhrasePosition *pp;
    if (index >= phq->pos_capa) {
        phq->pos_capa <<= 1;
        REALLOC_N(phq->positions, PhrasePosition, phq->pos_capa);
    }
    pp = &(phq->positions[index]);
    pp->terms = ary_new_type_capa(char *, 2);
    ary_push(pp->terms, estrdup(term));
    pp->pos = position;
    phq->pos_cnt++;
}

void phq_add_term(Query *self, const char *term, int pos_inc)
{
    PhraseQuery *phq = PhQ(self);
    int position;
    if (phq->pos_cnt == 0) {
        position = 0;
    } 
    else {
        position = phq->positions[phq->pos_cnt - 1].pos + pos_inc;
    }
    phq_add_term_abs(self, term, position);
}

void phq_append_multi_term(Query *self, const char *term)
{
    PhraseQuery *phq = PhQ(self);
    int index = phq->pos_cnt - 1;

    if (index < 0) {
        phq_add_term(self, term, 0);
    }
    else {
        ary_push(phq->positions[index].terms, estrdup(term));
    }
}
