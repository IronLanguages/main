#include <string.h>
#include <limits.h>
#include "search.h"
#include "hashset.h"

#define CLAUSE_INIT_CAPA 4

/*****************************************************************************
 *
 * SpanQuery
 *
 *****************************************************************************/

/***************************************************************************
 * SpanQuery
 ***************************************************************************/

#define SpQ(query) ((SpanQuery *)(query))

static unsigned long spanq_hash(Query *self)
{
    return str_hash(SpQ(self)->field);
}

static int spanq_eq(Query *self, Query *o)
{
    return strcmp(SpQ(self)->field, SpQ(o)->field) == 0;
}

static void spanq_destroy_i(Query *self)
{
    q_destroy_i(self);
}

static MatchVector *mv_to_term_mv(MatchVector *term_mv, MatchVector *full_mv,
                                  HashSet *terms, TermVector *tv)
{
    int i;
    for (i = 0; i < terms->size; i++) {
        char *term = (char *)terms->elems[i];
        TVTerm *tv_term = tv_get_tv_term(tv, term);
        if (tv_term) {
            int j;
            int m_idx = 0;
            for (j = 0; j < tv_term->freq; j++) {
                int pos = tv_term->positions[j];
                for (; m_idx < full_mv->size; m_idx++) {
                    if (pos <= full_mv->matches[m_idx].end) {
                        if (pos >= full_mv->matches[m_idx].start) {
                            matchv_add(term_mv, pos, pos);
                        }
                        break;
                    }
                }
            }
        }
    }

    return term_mv;
}

/***************************************************************************
 * TVTermDocEnum
 * dummy TermDocEnum used by the highlighter to find matches
 ***************************************************************************/

#define TV_TDE(tde) ((TVTermDocEnum *)(tde))

typedef struct TVTermDocEnum
{
    TermDocEnum super;
    int         doc;
    int         index;
    int         freq;
    int        *positions;
    TermVector *tv;
} TVTermDocEnum;

static void tv_tde_seek(TermDocEnum *tde, int field_num, const char *term)
{
    TVTermDocEnum *tv_tde = TV_TDE(tde);
    TVTerm *tv_term = tv_get_tv_term(tv_tde->tv, term);
    (void)field_num;
    if (tv_term) {
        tv_tde->doc = -1;
        tv_tde->index = 0;
        tv_tde->freq = tv_term->freq;
        tv_tde->positions = tv_term->positions;
    }
    else {
        tv_tde->doc = INT_MAX;
    }
}

static bool tv_tde_next(TermDocEnum *tde)
{
    if (TV_TDE(tde)->doc == -1) {
        TV_TDE(tde)->doc = 0;
        return true;
    }
    else {
        TV_TDE(tde)->doc = INT_MAX;
        return false;
    }
}

static bool tv_tde_skip_to(TermDocEnum *tde, int doc_num)
{
    if (doc_num == 0) {
        TV_TDE(tde)->doc = 0;
        return true;
    }
    else {
        TV_TDE(tde)->doc = INT_MAX;
        return false;
    }
}

static int tv_tde_next_position(TermDocEnum *tde)
{
    return TV_TDE(tde)->positions[TV_TDE(tde)->index++];
}

static int tv_tde_freq(TermDocEnum *tde)
{
    return TV_TDE(tde)->freq;
}

static int tv_tde_doc_num(TermDocEnum *tde)
{
    return TV_TDE(tde)->doc;
}

static TermDocEnum *spanq_ir_term_positions(IndexReader *ir)
{
    TVTermDocEnum *tv_tde = ALLOC(TVTermDocEnum);
    TermDocEnum *tde = (TermDocEnum *)tv_tde;
    tv_tde->tv = (TermVector *)ir->store;
    tde->seek           = &tv_tde_seek;
    tde->doc_num        = &tv_tde_doc_num;
    tde->freq           = &tv_tde_freq;
    tde->next           = &tv_tde_next;
    tde->skip_to        = &tv_tde_skip_to;
    tde->next_position  = &tv_tde_next_position;
    tde->close          = (void (*)(TermDocEnum *tde))&free;
    
    return tde;
}

static MatchVector *spanq_get_matchv_i(Query *self, MatchVector *mv,
                                       TermVector *tv)
{
    if (strcmp(SpQ(self)->field, tv->field) == 0) {
        SpanEnum *sp_enum;
        IndexReader *ir = ALLOC(IndexReader);
        MatchVector *full_mv = matchv_new();
        HashSet *terms = SpQ(self)->get_terms(self);
        ir->fis = fis_new(0, 0, 0);
        fis_add_field(ir->fis, fi_new(tv->field, 0, 0, 0));
        ir->store = (Store *)tv;
        ir->term_positions = &spanq_ir_term_positions;
        sp_enum = SpQ(self)->get_spans(self, ir);
        while (sp_enum->next(sp_enum)) {
            matchv_add(full_mv,
                       sp_enum->start(sp_enum),
                       sp_enum->end(sp_enum) - 1);
        }
        sp_enum->destroy(sp_enum);
        
        fis_deref(ir->fis);
        free(ir);

        matchv_compact(full_mv);
        mv_to_term_mv(mv, full_mv, terms, tv);
        matchv_destroy(full_mv);
        hs_destroy(terms);
    }
    return mv;
}

/***************************************************************************
 *
 * SpanScorer
 *
 ***************************************************************************/

#define SpSc(scorer) ((SpanScorer *)(scorer))
typedef struct SpanScorer
{
    Scorer          super;
    IndexReader    *ir;
    SpanEnum       *spans;
    Similarity     *sim;
    uchar          *norms;
    Weight         *weight;
    float           value;
    float           freq;
    bool            first_time : 1;
    bool            more : 1;
} SpanScorer;

static float spansc_score(Scorer *self)
{
    SpanScorer *spansc = SpSc(self);
    float raw = sim_tf(spansc->sim, spansc->freq) * spansc->value;

    /* normalize */
    return raw * sim_decode_norm(self->similarity, spansc->norms[self->doc]);
}

static bool spansc_next(Scorer *self)
{
    SpanScorer *spansc = SpSc(self);
    SpanEnum *se = spansc->spans;
    int match_length;

    if (spansc->first_time) {
        spansc->more = se->next(se);
        spansc->first_time = false;
    }

    if (!spansc->more) {
        return false;
    }

    spansc->freq = 0.0;
    self->doc = se->doc(se);

    while (spansc->more && (self->doc == se->doc(se))) {
        match_length = se->end(se) - se->start(se);
        spansc->freq += sim_sloppy_freq(spansc->sim, match_length);
        spansc->more = se->next(se);
    }

    return (spansc->more || (spansc->freq != 0.0));
}

static bool spansc_skip_to(Scorer *self, int target)
{
    SpanScorer *spansc = SpSc(self);
    SpanEnum *se = spansc->spans;

    spansc->more = se->skip_to(se, target);

    if (!spansc->more) {
        return false;
    }

    spansc->freq = 0.0;
    self->doc = se->doc(se);

    while (spansc->more && (se->doc(se) == target)) {
        spansc->freq += sim_sloppy_freq(spansc->sim, se->end(se) - se->start(se));
        spansc->more = se->next(se);
    }

    return (spansc->more || (spansc->freq != 0.0));
}

static Explanation *spansc_explain(Scorer *self, int target)
{
    Explanation *tf_explanation;
    SpanScorer *spansc = SpSc(self);
    float phrase_freq;
    self->skip_to(self, target);
    phrase_freq = (self->doc == target) ? spansc->freq : (float)0.0;

    tf_explanation = expl_new(sim_tf(self->similarity, phrase_freq),
                              "tf(phrase_freq(%f)", phrase_freq);

    return tf_explanation;
}

static void spansc_destroy(Scorer *self)
{
    SpanScorer *spansc = SpSc(self);
    if (spansc->spans) {
        spansc->spans->destroy(spansc->spans);
    }
    scorer_destroy_i(self);
}

Scorer *spansc_new(Weight *weight, IndexReader *ir)
{
    Scorer *self = NULL;
    const int field_num = fis_get_field_num(ir->fis, SpQ(weight->query)->field);
    if (field_num >= 0) {
        Query *spanq = weight->query;
        self = scorer_new(SpanScorer, weight->similarity);

        SpSc(self)->first_time  = true;
        SpSc(self)->more        = true;
        SpSc(self)->spans       = SpQ(spanq)->get_spans(spanq, ir);
        SpSc(self)->sim         = weight->similarity;
        SpSc(self)->norms       = ir->get_norms(ir, field_num);
        SpSc(self)->weight      = weight;
        SpSc(self)->value       = weight->value;
        SpSc(self)->freq        = 0.0;

        self->score             = &spansc_score;
        self->next              = &spansc_next;
        self->skip_to           = &spansc_skip_to;
        self->explain           = &spansc_explain;
        self->destroy           = &spansc_destroy;
    }
    return self;
}

/*****************************************************************************
 * SpanTermEnum
 *****************************************************************************/

#define SpTEn(span_enum) ((SpanTermEnum *)(span_enum))
#define SpTQ(query) ((SpanTermQuery *)(query))

typedef struct SpanTermEnum
{
    SpanEnum     super;
    TermDocEnum *positions;
    int          position;
    int          doc;
    int          count;
    int          freq;
} SpanTermEnum;


static bool spante_next(SpanEnum *self)
{
    SpanTermEnum *ste = SpTEn(self);
    TermDocEnum *tde = ste->positions;

    if (ste->count == ste->freq) {
        if (! tde->next(tde)) {
            ste->doc = INT_MAX;
            return false;
        }
        ste->doc = tde->doc_num(tde);
        ste->freq = tde->freq(tde);
        ste->count = 0;
    }
    ste->position = tde->next_position(tde);
    ste->count++;
    return true;
}

static bool spante_skip_to(SpanEnum *self, int target)
{
    SpanTermEnum *ste = SpTEn(self);
    TermDocEnum *tde = ste->positions;

    /* are we already at the correct position? */
    if (ste->doc >= target) {
        return true;
    }

    if (! tde->skip_to(tde, target)) {
        ste->doc = INT_MAX;
        return false;
    }

    ste->doc = tde->doc_num(tde);
    ste->freq = tde->freq(tde);
    ste->count = 0;

    ste->position = tde->next_position(tde);
    ste->count++;
    return true;
}

static int spante_doc(SpanEnum *self)
{
    return SpTEn(self)->doc;
}

static int spante_start(SpanEnum *self)
{
    return SpTEn(self)->position;
}

static int spante_end(SpanEnum *self)
{
    return SpTEn(self)->position + 1;
}

static char *spante_to_s(SpanEnum *self) 
{
    char *field = SpQ(self->query)->field;
    char *query_str = self->query->to_s(self->query, field);
    char pos_str[20];
    size_t len = strlen(query_str);
    int pos;
    char *str = ALLOC_N(char, len + 40);

    if (self->doc(self) < 0) {
        sprintf(pos_str, "START");
    }
    else {
        if (self->doc(self) == INT_MAX) {
            sprintf(pos_str, "END");
        }
        else {
            pos = SpTEn(self)->position;
            sprintf(pos_str, "%d", self->doc(self) - pos);
        }
    }
    sprintf("SpanTermEnum(%s)@%s", query_str, pos_str);
    free(query_str);
    return str;
}

static void spante_destroy(SpanEnum *self)
{
    TermDocEnum *tde = SpTEn(self)->positions;
    tde->close(tde);
    free(self);
}

static SpanEnum *spante_new(Query *query, IndexReader *ir)
{
    char *term = SpTQ(query)->term;
    char *field = SpQ(query)->field;
    SpanEnum *self = (SpanEnum *)emalloc(sizeof(SpanTermEnum));

    SpTEn(self)->positions  = ir_term_positions_for(ir, field, term);
    SpTEn(self)->position   = -1;
    SpTEn(self)->doc        = -1;
    SpTEn(self)->count      = 0;
    SpTEn(self)->freq       = 0;

    self->query             = query;
    self->next              = &spante_next;
    self->skip_to           = &spante_skip_to;
    self->doc               = &spante_doc;
    self->start             = &spante_start;
    self->end               = &spante_end;
    self->destroy           = &spante_destroy;
    self->to_s              = &spante_to_s;

    return self;
}

/*****************************************************************************
 * SpanMultiTermEnum
 *****************************************************************************/

/* * TermPosEnumWrapper * */
#define TPE_READ_SIZE 16

typedef struct TermPosEnumWrapper
{
    const char  *term;
    TermDocEnum *tpe;
    int          doc;
    int          pos;
} TermPosEnumWrapper;

static bool tpew_less_than(const TermPosEnumWrapper *tpew1,
                           const TermPosEnumWrapper *tpew2)
{
    return (tpew1->doc < tpew2->doc)
        || (tpew1->doc == tpew2->doc && tpew1->pos < tpew2->pos);
}

static bool tpew_next(TermPosEnumWrapper *self)
{
    TermDocEnum *tpe = self->tpe;
    if (0 > (self->pos = tpe->next_position(tpe))) {
        if (!tpe->next(tpe)) return false;
        self->doc = tpe->doc_num(tpe);
        self->pos = tpe->next_position(tpe);
    }
    return true;
}

static bool tpew_skip_to(TermPosEnumWrapper *self, int doc_num)
{
    TermDocEnum *tpe = self->tpe;

    if (tpe->skip_to(tpe, doc_num)) {
        self->doc = tpe->doc_num(tpe);
        self->pos = tpe->next_position(tpe);
        return true;
    }
    else {
        return false;
    }
}

static void tpew_destroy(TermPosEnumWrapper *self)
{
    self->tpe->close(self->tpe);
    free(self);
}

static TermPosEnumWrapper *tpew_new(const char *term, TermDocEnum *tpe)
{
    TermPosEnumWrapper *self = ALLOC_AND_ZERO(TermPosEnumWrapper);
    self->term = term;
    self->tpe = tpe;
    self->doc = -1;
    self->pos = -1;
    return self;
}
#define SpMTEn(span_enum) ((SpanMultiTermEnum *)(span_enum))
#define SpMTQ(query) ((SpanMultiTermQuery *)(query))

typedef struct SpanMultiTermEnum
{
    SpanEnum             super;
    PriorityQueue       *tpew_pq;
    TermPosEnumWrapper **tpews;
    int                  tpew_cnt;
    int                  pos;
    int                  doc;
} SpanMultiTermEnum;

static bool spanmte_next(SpanEnum *self)
{
    int curr_doc, curr_pos;
    TermPosEnumWrapper *tpew;
    SpanMultiTermEnum *mte = SpMTEn(self);
    PriorityQueue *tpew_pq = mte->tpew_pq;
    if (tpew_pq == NULL) {
        TermPosEnumWrapper **tpews = mte->tpews;
        int i;
        tpew_pq = pq_new(mte->tpew_cnt, (lt_ft)tpew_less_than, (free_ft)NULL);
        for (i = mte->tpew_cnt - 1; i >= 0; i--) {
            if (tpew_next(tpews[i])) {
                pq_push(tpew_pq, tpews[i]);
            }
        }
        mte->tpew_pq = tpew_pq;
    }
    
    tpew = (TermPosEnumWrapper *)pq_top(tpew_pq);
    if (tpew == NULL) {
        return false;
    }

    mte->doc = curr_doc = tpew->doc;
    mte->pos = curr_pos = tpew->pos;

    do {
        if (tpew_next(tpew)) {
            pq_down(tpew_pq);
        }
        else {
            pq_pop(tpew_pq);
        }
    } while (((tpew = (TermPosEnumWrapper *)pq_top(tpew_pq)) != NULL)
             && tpew->doc == curr_doc && tpew->pos == curr_pos);
    return true;
}

static bool spanmte_skip_to(SpanEnum *self, int target)
{
    SpanMultiTermEnum *mte = SpMTEn(self);
    PriorityQueue *tpew_pq = mte->tpew_pq;
    TermPosEnumWrapper *tpew;
    if (tpew_pq == NULL) {
        TermPosEnumWrapper **tpews = mte->tpews;
        int i;
        tpew_pq = pq_new(mte->tpew_cnt, (lt_ft)tpew_less_than, (free_ft)NULL);
        for (i = mte->tpew_cnt - 1; i >= 0; i--) {
            tpew_skip_to(tpews[i], target);
            pq_push(tpew_pq, tpews[i]);
        }
        mte->tpew_pq = tpew_pq;
    }
    if (tpew_pq->size == 0) {
        mte->doc = -1;
        return false;
    }
    while ((tpew = (TermPosEnumWrapper *)pq_top(tpew_pq)) != NULL
           && (target > tpew->doc)) {
        if (tpew_skip_to(tpew, target)) {
            pq_down(tpew_pq);
        }
        else {
            pq_pop(tpew_pq);
        }
    }
    return spanmte_next(self);
}

static int spanmte_doc(SpanEnum *self)
{
    return SpMTEn(self)->doc;
}

static int spanmte_start(SpanEnum *self)
{
    return SpMTEn(self)->pos;
}

static int spanmte_end(SpanEnum *self)
{
    return SpMTEn(self)->pos + 1;
}

static void spanmte_destroy(SpanEnum *self)
{
    SpanMultiTermEnum *mte = SpMTEn(self);
    int i;
    if (mte->tpew_pq) pq_destroy(mte->tpew_pq);
    for (i = 0; i < mte->tpew_cnt; i++) {
        tpew_destroy(mte->tpews[i]);
    }
    free(mte->tpews);
    free(self);
}

static SpanEnum *spanmte_new(Query *query, IndexReader *ir)
{
    char *field = SpQ(query)->field;
    SpanEnum *self = (SpanEnum *)emalloc(sizeof(SpanMultiTermEnum));
    SpanMultiTermEnum *smte = SpMTEn(self);
    SpanMultiTermQuery *smtq = SpMTQ(query);
    int i;


    smte->tpews = ALLOC_N(TermPosEnumWrapper *, smtq->term_cnt);
    for (i = 0; i < smtq->term_cnt; i++) {
        char *term = smtq->terms[i];
        smte->tpews[i] = tpew_new(term, ir_term_positions_for(ir, field, term));
    }
    smte->tpew_cnt          = smtq->term_cnt;
    smte->tpew_pq           = NULL;
    smte->pos               = -1;
    smte->doc               = -1;

    self->query             = query;
    self->next              = &spanmte_next;
    self->skip_to           = &spanmte_skip_to;
    self->doc               = &spanmte_doc;
    self->start             = &spanmte_start;
    self->end               = &spanmte_end;
    self->destroy           = &spanmte_destroy;
    self->to_s              = &spante_to_s;

    return self;
}


/*****************************************************************************
 * SpanFirstEnum
 *****************************************************************************/

#define SpFEn(span_enum) ((SpanFirstEnum *)(span_enum))
#define SpFQ(query) ((SpanFirstQuery *)(query))

typedef struct SpanFirstEnum
{
    SpanEnum    super;
    SpanEnum   *sub_enum;
} SpanFirstEnum;


static bool spanfe_next(SpanEnum *self)
{
    SpanEnum *sub_enum = SpFEn(self)->sub_enum;
    int end = SpFQ(self->query)->end;
    while (sub_enum->next(sub_enum)) { /* scan to next match */
        if (sub_enum->end(sub_enum) <= end) {
            return true;
        }
    }
    return false;
}

static bool spanfe_skip_to(SpanEnum *self, int target)
{
    SpanEnum *sub_enum = SpFEn(self)->sub_enum;
    int end = SpFQ(self->query)->end;

    if (! sub_enum->skip_to(sub_enum, target)) {
        return false;
    }

    if (sub_enum->end(sub_enum) <= end) {   /* there is a match */
        return true;
    }

    return sub_enum->next(sub_enum);        /* scan to next match */
}

static int spanfe_doc(SpanEnum *self)
{
    SpanEnum *sub_enum = SpFEn(self)->sub_enum;
    return sub_enum->doc(sub_enum);
}

static int spanfe_start(SpanEnum *self)
{
    SpanEnum *sub_enum = SpFEn(self)->sub_enum;
    return sub_enum->start(sub_enum);
}

static int spanfe_end(SpanEnum *self)
{
    SpanEnum *sub_enum = SpFEn(self)->sub_enum;
    return sub_enum->end(sub_enum);
}

static char *spanfe_to_s(SpanEnum *self) 
{
    char *field = SpQ(self->query)->field;
    char *query_str = self->query->to_s(self->query, field);
    char *res = strfmt("SpanFirstEnum(%s)", query_str);
    free(query_str);
    return res;
}

static void spanfe_destroy(SpanEnum *self)
{
    SpanEnum *sub_enum = SpFEn(self)->sub_enum;
    sub_enum->destroy(sub_enum);
    free(self);
}

static SpanEnum *spanfe_new(Query *query, IndexReader *ir)
{
    SpanEnum *self          = (SpanEnum *)emalloc(sizeof(SpanFirstEnum));
    SpanFirstQuery *sfq     = SpFQ(query);

    SpFEn(self)->sub_enum   = SpQ(sfq->match)->get_spans(sfq->match, ir);

    self->query     = query;
    self->next      = &spanfe_next;
    self->skip_to   = &spanfe_skip_to;
    self->doc       = &spanfe_doc;
    self->start     = &spanfe_start;
    self->end       = &spanfe_end;
    self->destroy   = &spanfe_destroy;
    self->to_s      = &spanfe_to_s;

    return self;
}


/*****************************************************************************
 * SpanOrEnum
 *****************************************************************************/

#define SpOEn(span_enum) ((SpanOrEnum *)(span_enum))
#define SpOQ(query) ((SpanOrQuery *)(query))

typedef struct SpanOrEnum
{
    SpanEnum        super;
    PriorityQueue  *queue;
    SpanEnum      **span_enums;
    int             s_cnt;
    bool            first_time : 1;
} SpanOrEnum;


static bool span_less_than(SpanEnum *s1, SpanEnum *s2)
{
    int doc_diff, start_diff;
    doc_diff = s1->doc(s1) - s2->doc(s2);
    if (doc_diff == 0) {
        start_diff = s1->start(s1) - s2->start(s2);
        if (start_diff == 0) {
            return s1->end(s1) < s2->end(s2);
        }
        else {
            return start_diff < 0;
        }
    }
    else {
        return doc_diff < 0;
    }
}

static bool spanoe_next(SpanEnum *self)
{
    SpanOrEnum *soe = SpOEn(self);
    SpanEnum *se;
    int i;

    if (soe->first_time) { /* first time -- initialize */
        for (i = 0; i < soe->s_cnt; i++) {
            se = soe->span_enums[i];
            if (se->next(se)) { /* move to first entry */
                pq_push(soe->queue, se);
            }
        }
        soe->first_time = false;
        return soe->queue->size != 0;
    }

    if (soe->queue->size == 0) {
        return false; /* all done */
    }

    se = (SpanEnum *)pq_top(soe->queue);
    if (se->next(se)) { /* move to next */
        pq_down(soe->queue);
        return true;
    }

    pq_pop(soe->queue); /* exhausted a clause */

    return soe->queue->size != 0;
}

static bool spanoe_skip_to(SpanEnum *self, int target)
{
    SpanOrEnum *soe = SpOEn(self);
    SpanEnum *se;
    int i;

    if (soe->first_time) { /* first time -- initialize */
        for (i = 0; i < soe->s_cnt; i++) {
            se = soe->span_enums[i];
            if (se->skip_to(se, target)) {/* move to target */
                pq_push(soe->queue, se);
            }
        }
        soe->first_time = false;
    }
    else {
        while ((soe->queue->size != 0) &&
               ((se = (SpanEnum *)pq_top(soe->queue))->doc(se) < target)) {
            if (se->skip_to(se, target)) {
                pq_down(soe->queue);
            }
            else {
                pq_pop(soe->queue);
            }
        }
    }

    return soe->queue->size != 0;
}

#define SpOEn_Top_SE(self) (SpanEnum *)pq_top(SpOEn(self)->queue)

static int spanoe_doc(SpanEnum *self)
{
    SpanEnum *se = SpOEn_Top_SE(self);
    return se->doc(se);
}

static int spanoe_start(SpanEnum *self)
{
    SpanEnum *se = SpOEn_Top_SE(self);
    return se->start(se);
}

static int spanoe_end(SpanEnum *self)
{
    SpanEnum *se = SpOEn_Top_SE(self);
    return se->end(se);
}

static char *spanoe_to_s(SpanEnum *self) 
{
    SpanOrEnum *soe = SpOEn(self);
    char *field = SpQ(self->query)->field;
    char *query_str = self->query->to_s(self->query, field);
    char doc_str[62];
    size_t len = strlen(query_str);
    char *str = ALLOC_N(char, len + 80);

    if (soe->first_time) {
        sprintf(doc_str, "START");
    }
    else {
        if (soe->queue->size == 0) {
            sprintf(doc_str, "END");
        }
        else {
            sprintf(doc_str, "%d:%d-%d", self->doc(self),
                    self->start(self), self->end(self));
        }
    }
    sprintf("SpanOrEnum(%s)@%s", query_str, doc_str);
    free(query_str);
    return str;
}

static void spanoe_destroy(SpanEnum *self)
{
    SpanEnum *se;
    SpanOrEnum *soe = SpOEn(self);
    int i;
    pq_destroy(soe->queue);
    for (i = 0; i < soe->s_cnt; i++) {
        se = soe->span_enums[i];
        se->destroy(se);
    }
    free(soe->span_enums);
    free(self);
}

SpanEnum *spanoe_new(Query *query, IndexReader *ir)
{
    Query *clause;
    SpanEnum *self      = (SpanEnum *)emalloc(sizeof(SpanOrEnum));
    SpanOrQuery *soq    = SpOQ(query);
    int i;

    SpOEn(self)->first_time = true;
    SpOEn(self)->s_cnt      = soq->c_cnt;
    SpOEn(self)->span_enums = ALLOC_N(SpanEnum *, SpOEn(self)->s_cnt);

    for (i = 0; i < SpOEn(self)->s_cnt; i++) {
        clause = soq->clauses[i];
        SpOEn(self)->span_enums[i] = SpQ(clause)->get_spans(clause, ir);
    }

    SpOEn(self)->queue      = pq_new(SpOEn(self)->s_cnt, (lt_ft)&span_less_than,
                                     (free_ft)NULL);

    self->query             = query;
    self->next              = &spanoe_next;
    self->skip_to           = &spanoe_skip_to;
    self->doc               = &spanoe_doc;
    self->start             = &spanoe_start;
    self->end               = &spanoe_end;
    self->destroy           = &spanoe_destroy;
    self->to_s              = &spanoe_to_s;

    return self;
}

/*****************************************************************************
 * SpanNearEnum
 *****************************************************************************/

#define SpNEn(span_enum) ((SpanNearEnum *)(span_enum))
#define SpNQ(query) ((SpanNearQuery *)(query))

typedef struct SpanNearEnum
{
    SpanEnum    super;
    SpanEnum  **span_enums;
    int         s_cnt;
    int         slop;
    int         current;
    int         doc;
    int         start;
    int         end;
    bool        first_time : 1;
    bool        in_order : 1;
} SpanNearEnum;


#define SpNEn_NEXT() do {\
    sne->current = (sne->current+1) % sne->s_cnt;\
    se = sne->span_enums[sne->current];\
} while (0);

static bool sne_init(SpanNearEnum *sne)
{
    SpanEnum *se = sne->span_enums[sne->current];
    int prev_doc = se->doc(se);
    int i;

    for (i = 1; i < sne->s_cnt; i++) {
        SpNEn_NEXT();
        if (!se->skip_to(se, prev_doc)) {
            return false;
        }
        prev_doc = se->doc(se);
    }
    return true;
}

static bool sne_goto_next_doc(SpanNearEnum *sne)
{
    SpanEnum *se = sne->span_enums[sne->current];
    int prev_doc = se->doc(se);

    SpNEn_NEXT();

    while (se->doc(se) < prev_doc) {
        if (! se->skip_to(se, prev_doc)) {
            return false;
        }
        prev_doc = se->doc(se);
        SpNEn_NEXT();
    }
    return true;
}

static bool sne_next_unordered_match(SpanEnum *self)
{
    SpanNearEnum *sne = SpNEn(self);
    SpanEnum *se, *min_se = NULL;
    int i;
    int max_end, end, min_start, start, doc;
    int lengths_sum;

    while (true) {
        max_end = 0;
        min_start = INT_MAX;
        lengths_sum = 0;

        for (i = 0; i < sne->s_cnt; i++) {
            se = sne->span_enums[i];
            if ((end=se->end(se)) > max_end) {
                max_end = end;
            }
            if ((start=se->start(se)) < min_start) {
                min_start = start;
                min_se = se;
                sne->current = i; /* current should point to the minimum span */
            }
            lengths_sum += end - start;
        }

        if ((max_end - min_start - lengths_sum) <= sne->slop) {
            /* we have a match */
            sne->start = min_start;
            sne->end = max_end;
            sne->doc = min_se->doc(min_se);
            return true;
        }

        /* increment the minimum span_enum and try again */
        doc = min_se->doc(min_se);
        if (!min_se->next(min_se)) {
            return false;
        }
        if (doc < min_se->doc(min_se)) {
            if (!sne_goto_next_doc(sne)) return false;
        }
    }
}

static bool sne_next_ordered_match(SpanEnum *self)
{
    SpanNearEnum *sne = SpNEn(self);
    SpanEnum *se;
    int i;
    int prev_doc, prev_start, prev_end;
    int doc=0, start=0, end=0;
    int lengths_sum;

    while (true) {
        se = sne->span_enums[0];

        prev_doc = se->doc(se);
        sne->start = prev_start = se->start(se);
        prev_end = se->end(se);

        i = 1;
        lengths_sum = prev_end - prev_start;

        while (i < sne->s_cnt) {
            se = sne->span_enums[i];
            doc = se->doc(se);
            start = se->start(se);
            end = se->end(se);
            while ((doc == prev_doc) && ((start < prev_start) ||
                                         ((start == prev_start) && (end < prev_end)))) {
                if (!se->next(se)) {
                    return false;
                }
                doc = se->doc(se);
                start = se->start(se);
                end = se->end(se);
            }
            if (doc != prev_doc) {
                sne->current = i;
                if (!sne_goto_next_doc(sne)) {
                    return false;
                }
                break;
            }
            i++;
            lengths_sum += end - start;
            prev_doc = doc;
            prev_start = start;
            prev_end = end;
        }
        if (i == sne->s_cnt) {
            if ((end - sne->start - lengths_sum) <= sne->slop) {
                /* we have a match */
                sne->end = end;
                sne->doc = doc;

                /* the minimum span is always the first span so it needs to be
                 * incremented next time around */
                sne->current = 0;
                return true;

            }
            else {
                se = sne->span_enums[0];
                if (!se->next(se)) {
                    return false;
                }
                if (se->doc(se) != prev_doc) {
                    sne->current = 0;
                    if (!sne_goto_next_doc(sne)) {
                        return false;
                    }
                }
            }
        }
    }
}

static bool sne_next_match(SpanEnum *self)
{
    SpanNearEnum *sne = SpNEn(self);
    SpanEnum *se_curr, *se_next;

    if (!sne->first_time) {
        if (!sne_init(sne)) {
            return false;
        }
        sne->first_time = false;
    }
    se_curr = sne->span_enums[sne->current];
    se_next = sne->span_enums[(sne->current+1)%sne->s_cnt];
    if (se_curr->doc(se_curr) > se_next->doc(se_next)) {
        if (!sne_goto_next_doc(sne)) {
            return false;
        }
    }

    if (sne->in_order) {
        return sne_next_ordered_match(self);
    }
    else {
        return sne_next_unordered_match(self);
    }
}

static bool spanne_next(SpanEnum *self)
{
    SpanNearEnum *sne = SpNEn(self);
    SpanEnum *se;

    se = sne->span_enums[sne->current];
    if (!se->next(se)) return false;

    return sne_next_match(self);
}

static bool spanne_skip_to(SpanEnum *self, int target)
{
    SpanEnum *se = SpNEn(self)->span_enums[SpNEn(self)->current];
    if (!se->skip_to(se, target)) {
        return false;
    }

    return sne_next_match(self);
}

static int spanne_doc(SpanEnum *self)
{
    return SpNEn(self)->doc;
}

static int spanne_start(SpanEnum *self)
{
    return SpNEn(self)->start;
}

static int spanne_end(SpanEnum *self)
{
    return SpNEn(self)->end;
}

static char *spanne_to_s(SpanEnum *self) 
{
    SpanNearEnum *sne = SpNEn(self);
    char *field = SpQ(self->query)->field;
    char *query_str = self->query->to_s(self->query, field);
    char doc_str[62];
    size_t len = strlen(query_str);
    char *str = ALLOC_N(char, len + 80);

    if (sne->first_time) {
        sprintf(doc_str, "START");
    }
    else {
        sprintf(doc_str, "%d:%d-%d", self->doc(self),
                self->start(self), self->end(self));
    }
    sprintf("SpanNearEnum(%s)@%s", query_str, doc_str);
    free(query_str);
    return str;
}

static void spanne_destroy(SpanEnum *self)
{
    SpanEnum *se;
    SpanNearEnum *sne = SpNEn(self);
    int i;
    for (i = 0; i < sne->s_cnt; i++) {
        se = sne->span_enums[i];
        se->destroy(se);
    }
    free(sne->span_enums);
    free(self);
}

static SpanEnum *spanne_new(Query *query, IndexReader *ir)
{
    int i;
    Query *clause;
    SpanEnum *self          = (SpanEnum *)emalloc(sizeof(SpanNearEnum));
    SpanNearQuery *snq      = SpNQ(query);

    SpNEn(self)->first_time = true;
    SpNEn(self)->in_order   = snq->in_order;
    SpNEn(self)->slop       = snq->slop;
    SpNEn(self)->s_cnt      = snq->c_cnt;
    SpNEn(self)->span_enums = ALLOC_N(SpanEnum *, SpNEn(self)->s_cnt);

    for (i = 0; i < SpNEn(self)->s_cnt; i++) {
        clause = snq->clauses[i];
        SpNEn(self)->span_enums[i] = SpQ(clause)->get_spans(clause, ir);
    }
    SpNEn(self)->current    = 0;

    SpNEn(self)->doc        = -1;
    SpNEn(self)->start      = -1;
    SpNEn(self)->end        = -1;

    self->query             = query;
    self->next              = &spanne_next;
    self->skip_to           = &spanne_skip_to;
    self->doc               = &spanne_doc;
    self->start             = &spanne_start;
    self->end               = &spanne_end;
    self->destroy           = &spanne_destroy;
    self->to_s              = &spanne_to_s;

    return self;
}

/*****************************************************************************
 *
 * SpanNotEnum
 *
 *****************************************************************************/

#define SpXEn(span_enum) ((SpanNotEnum *)(span_enum))
#define SpXQ(query) ((SpanNotQuery *)(query))

typedef struct SpanNotEnum
{
    SpanEnum    super;
    SpanEnum   *inc;
    SpanEnum   *exc;
    bool        more_inc : 1;
    bool        more_exc : 1;
} SpanNotEnum;


static bool spanxe_next(SpanEnum *self)
{
    SpanNotEnum *sxe = SpXEn(self);
    SpanEnum *inc = sxe->inc, *exc = sxe->exc;
    if (sxe->more_inc) {                        /*  move to next incl */
        sxe->more_inc = inc->next(inc);
    }

    while (sxe->more_inc && sxe->more_exc) {
        if (inc->doc(inc) > exc->doc(exc)) {    /*  skip excl */
            sxe->more_exc = exc->skip_to(exc, inc->doc(inc));
        }

        while (sxe->more_exc                    /*  while excl is before */
               && (inc->doc(inc) == exc->doc(exc))
               && (exc->end(exc) <= inc->start(inc))) {
            sxe->more_exc = exc->next(exc);     /*  increment excl */
        }

        if (! sxe->more_exc ||                  /*  if no intersection */
            (inc->doc(inc) != exc->doc(exc)) ||
            inc->end(inc) <= exc->start(exc)) {
            break;                              /*  we found a match */
        }

        sxe->more_inc = inc->next(inc);         /*  intersected: keep scanning */
    }
    return sxe->more_inc;
}

static bool spanxe_skip_to(SpanEnum *self, int target)
{
    SpanNotEnum *sxe = SpXEn(self);
    SpanEnum *inc = sxe->inc, *exc = sxe->exc;
    int doc;

    if (sxe->more_inc) {                        /*  move to next incl */
        if (!(sxe->more_inc=sxe->inc->skip_to(sxe->inc, target))) return false;
    }

    if (sxe->more_inc && ((doc=inc->doc(inc)) > exc->doc(exc))) {
        sxe->more_exc = exc->skip_to(exc, doc);
    }

    while (sxe->more_exc                       /*  while excl is before */
           && inc->doc(inc) == exc->doc(exc)
           && exc->end(exc) <= inc->start(inc)) {
        sxe->more_exc = exc->next(exc);        /*  increment excl */
    }

    if (!sxe->more_exc ||                      /*  if no intersection */
        inc->doc(inc) != exc->doc(exc) ||
        inc->end(inc) <= exc->start(exc)) {
        return true;                           /*  we found a match */
    }

    return spanxe_next(self);                  /*  scan to next match */
}

static int spanxe_doc(SpanEnum *self)
{
    SpanEnum *inc = SpXEn(self)->inc;
    return inc->doc(inc);
}

static int spanxe_start(SpanEnum *self)
{
    SpanEnum *inc = SpXEn(self)->inc;
    return inc->start(inc);
}

static int spanxe_end(SpanEnum *self)
{
    SpanEnum *inc = SpXEn(self)->inc;
    return inc->end(inc);
}

static char *spanxe_to_s(SpanEnum *self) 
{
    char *field = SpQ(self->query)->field;
    char *query_str = self->query->to_s(self->query, field);
    char *res = strfmt("SpanNotEnum(%s)", query_str);
    free(query_str);
    return res;
}

static void spanxe_destroy(SpanEnum *self)
{
    SpanNotEnum *sxe = SpXEn(self);
    sxe->inc->destroy(sxe->inc);
    sxe->exc->destroy(sxe->exc);
    free(self);
}

static SpanEnum *spanxe_new(Query *query, IndexReader *ir)
{
    SpanEnum *self      = (SpanEnum *)emalloc(sizeof(SpanNotEnum));
    SpanNotEnum *sxe    = SpXEn(self);
    SpanNotQuery *sxq   = SpXQ(query);

    sxe->inc            = SpQ(sxq->inc)->get_spans(sxq->inc, ir);
    sxe->exc            = SpQ(sxq->exc)->get_spans(sxq->exc, ir);
    sxe->more_inc       = true;
    sxe->more_exc       = sxe->exc->next(sxe->exc);

    self->query         = query;
    self->next          = &spanxe_next;
    self->skip_to       = &spanxe_skip_to;
    self->doc           = &spanxe_doc;
    self->start         = &spanxe_start;
    self->end           = &spanxe_end;
    self->destroy       = &spanxe_destroy;
    self->to_s          = &spanxe_to_s;

    return self;
}

/*****************************************************************************
 *
 * SpanWeight
 *
 *****************************************************************************/

#define SpW(weight) ((SpanWeight *)(weight))
typedef struct SpanWeight
{
    Weight      super;
    HashSet    *terms;
} SpanWeight;

static Explanation *spanw_explain(Weight *self, IndexReader *ir, int target)
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
    HashSet *terms = SpW(self)->terms;
    char *field = SpQ(self->query)->field;
    const int field_num = fis_get_field_num(ir->fis, field);
    char *doc_freqs = NULL;
    size_t df_i = 0;
    int i;

    if (field_num < 0) {
        return expl_new(0.0, "field \"%s\" does not exist in the index", field);
    }

    query_str = self->query->to_s(self->query, "");

    for (i = 0; i < terms->size; i++) {
        char *term = (char *)terms->elems[i];
        REALLOC_N(doc_freqs, char, df_i + strlen(term) + 23);
        sprintf(doc_freqs + df_i, "%s=%d, ", term,
                ir->doc_freq(ir, field_num, term));
        df_i = strlen(doc_freqs);
    }
    /* remove the ',' at the end of the string if it exists */
    if (terms->size > 0) {
        df_i -= 2;
        doc_freqs[df_i] = '\0';
    }
    else {
        doc_freqs = "";
    }

    expl = expl_new(0.0, "weight(%s in %d), product of:", query_str, target);

    /* We need two of these as it's included in both the query explanation
     * and the field explanation */
    idf_expl1 = expl_new(self->idf, "idf(%s: %s)", field, doc_freqs);
    idf_expl2 = expl_new(self->idf, "idf(%s: %s)", field, doc_freqs);
    if (terms->size > 0) {
        free(doc_freqs); /* only free if allocated */
    }

    /* explain query weight */
    query_expl = expl_new(0.0, "query_weight(%s), product of:", query_str);

    if (self->query->boost != 1.0) {
        expl_add_detail(query_expl, expl_new(self->query->boost, "boost"));
    }

    expl_add_detail(query_expl, idf_expl1);

    qnorm_expl = expl_new(self->qnorm, "query_norm");
    expl_add_detail(query_expl, qnorm_expl);

    query_expl->value = self->query->boost * idf_expl1->value * qnorm_expl->value;

    expl_add_detail(expl, query_expl);

    /* explain field weight */
    field_expl = expl_new(0.0, "field_weight(%s:%s in %d), product of:",
                          field, query_str, target);
    free(query_str);

    scorer = self->scorer(self, ir);
    tf_expl = scorer->explain(scorer, target);
    scorer->destroy(scorer);
    expl_add_detail(field_expl, tf_expl);
    expl_add_detail(field_expl, idf_expl2);

    field_norms = ir->get_norms(ir, field_num);
    field_norm = (field_norms 
                  ? sim_decode_norm(self->similarity, field_norms[target]) 
                  : (float)0.0);
    field_norm_expl = expl_new(field_norm, "field_norm(field=%s, doc=%d)",
                               field, target);
    expl_add_detail(field_expl, field_norm_expl);

    field_expl->value = tf_expl->value * idf_expl2->value * field_norm_expl->value;

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

static char *spanw_to_s(Weight *self)
{
    return strfmt("SpanWeight(%f)", self->value);
}

static void spanw_destroy(Weight *self)
{
    hs_destroy(SpW(self)->terms);
    w_destroy(self);
}

static Weight *spanw_new(Query *query, Searcher *searcher)
{
    int i;
    Weight *self        = w_new(SpanWeight, query);
    HashSet *terms      = SpQ(query)->get_terms(query);

    SpW(self)->terms    = terms;
    self->scorer        = &spansc_new;
    self->explain       = &spanw_explain;
    self->to_s          = &spanw_to_s;
    self->destroy       = &spanw_destroy;

    self->similarity    = query->get_similarity(query, searcher);

    self->idf           = 0.0;
    
    for (i = terms->size - 1; i >= 0; i--) {
        self->idf += sim_idf_term(self->similarity, SpQ(query)->field, 
                                  (char *)terms->elems[i], searcher);
    }

    return self;
}

/*****************************************************************************
 * SpanTermQuery
 *****************************************************************************/

static char *spantq_to_s(Query *self, const char *field)
{
    if (field == SpQ(self)->field) {
        return strfmt("span_terms(%s)", SpTQ(self)->term);
    }
    else {
        return strfmt("span_terms(%s:%s)", SpQ(self)->field, SpTQ(self)->term);
    }
}

static void spantq_destroy_i(Query *self)
{
    free(SpTQ(self)->term);
    free(SpQ(self)->field);
    spanq_destroy_i(self);
}

static void spantq_extract_terms(Query *self, HashSet *terms)
{
    hs_add(terms, term_new(SpQ(self)->field, SpTQ(self)->term));
}

static HashSet *spantq_get_terms(Query *self)
{
    HashSet *terms = hs_new_str(&free);
    hs_add(terms, estrdup(SpTQ(self)->term));
    return terms;
}

static unsigned long spantq_hash(Query *self)
{
    return spanq_hash(self) ^ str_hash(SpTQ(self)->term);
}

static int spantq_eq(Query *self, Query *o)
{
    return spanq_eq(self, o) && strcmp(SpTQ(self)->term, SpTQ(o)->term) == 0;
}

Query *spantq_new(const char *field, const char *term)
{
    Query *self             = q_new(SpanTermQuery);

    SpTQ(self)->term        = estrdup(term);
    SpQ(self)->field        = estrdup(field);
    SpQ(self)->get_spans    = &spante_new;
    SpQ(self)->get_terms    = &spantq_get_terms;

    self->type              = SPAN_TERM_QUERY;
    self->extract_terms     = &spantq_extract_terms;
    self->to_s              = &spantq_to_s;
    self->hash              = &spantq_hash;
    self->eq                = &spantq_eq;
    self->destroy_i         = &spantq_destroy_i;
    self->create_weight_i   = &spanw_new;
    self->get_matchv_i      = &spanq_get_matchv_i;
    return self;
}

/*****************************************************************************
 * SpanMultiTermQuery
 *****************************************************************************/

static char *spanmtq_to_s(Query *self, const char *field)
{
    char *terms = NULL, *p;
    int len = 2, i;
    SpanMultiTermQuery *smtq = SpMTQ(self);
    for (i = 0; i < smtq->term_cnt; i++) {
        len += strlen(smtq->terms[i]) + 2;
    }
    p = terms = ALLOC_N(char, len);
    *(p++) = '[';
    for (i = 0; i < smtq->term_cnt; i++) {
        strcpy(p, smtq->terms[i]);
        p += strlen(smtq->terms[i]);
        *(p++) = ',';
    }
    if (p > terms) p--;
    *(p++) = ']';
    *p = '\0';

    if (field == SpQ(self)->field) {
        p = strfmt("span_terms(%s)", terms);
    }
    else {
        p = strfmt("span_terms(%s:%s)", SpQ(self)->field, terms);
    }
    free(terms);
    return p;
}

static void spanmtq_destroy_i(Query *self)
{
    SpanMultiTermQuery *smtq = SpMTQ(self);
    int i;
    for (i = 0; i < smtq->term_cnt; i++) {
        free(smtq->terms[i]);
    }
    free(smtq->terms);
    free(SpQ(self)->field);
    spanq_destroy_i(self);
}

static void spanmtq_extract_terms(Query *self, HashSet *terms)
{
    SpanMultiTermQuery *smtq = SpMTQ(self);
    int i;
    for (i = 0; i < smtq->term_cnt; i++) {
        hs_add(terms, term_new(SpQ(self)->field, smtq->terms[i]));
    }
}

static HashSet *spanmtq_get_terms(Query *self)
{
    HashSet *terms = hs_new_str(&free);
    SpanMultiTermQuery *smtq = SpMTQ(self);
    int i;
    for (i = 0; i < smtq->term_cnt; i++) {
        hs_add(terms, estrdup(smtq->terms[i]));
    }
    return terms;
}

static unsigned long spanmtq_hash(Query *self)
{
    unsigned long hash = spanq_hash(self);
    SpanMultiTermQuery *smtq = SpMTQ(self);
    int i;
    for (i = 0; i < smtq->term_cnt; i++) {
        hash ^= str_hash(smtq->terms[i]);
    }
    return hash;
}

static int spanmtq_eq(Query *self, Query *o)
{
    SpanMultiTermQuery *smtq = SpMTQ(self);
    SpanMultiTermQuery *smtqo = SpMTQ(o);
    int i;
    if (!spanq_eq(self, o)) return false;
    if (smtq->term_cnt != smtqo->term_cnt) return false;
    for (i = 0; i < smtq->term_cnt; i++) {
        if (strcmp(smtq->terms[i], smtqo->terms[i]) != 0) return false;
    }
    return true;;
}

Query *spanmtq_new_conf(const char *field, int max_terms)
{
    Query *self             = q_new(SpanMultiTermQuery);

    SpMTQ(self)->terms      = ALLOC_N(char *, max_terms);
    SpMTQ(self)->term_cnt   = 0;
    SpMTQ(self)->term_capa  = max_terms;

    SpQ(self)->field        = estrdup(field);
    SpQ(self)->get_spans    = &spanmte_new;
    SpQ(self)->get_terms    = &spanmtq_get_terms;

    self->type              = SPAN_MULTI_TERM_QUERY;
    self->extract_terms     = &spanmtq_extract_terms;
    self->to_s              = &spanmtq_to_s;
    self->hash              = &spanmtq_hash;
    self->eq                = &spanmtq_eq;
    self->destroy_i         = &spanmtq_destroy_i;
    self->create_weight_i   = &spanw_new;
    self->get_matchv_i      = &spanq_get_matchv_i;

    return self;
}

Query *spanmtq_new(const char *field)
{
    return spanmtq_new_conf(field, SPAN_MULTI_TERM_QUERY_CAPA);
}

void spanmtq_add_term(Query *self, const char *term)
{
    SpanMultiTermQuery *smtq = SpMTQ(self);
    if (smtq->term_cnt < smtq->term_capa) {
        smtq->terms[smtq->term_cnt++] = estrdup(term);
    }
}

/*****************************************************************************
 *
 * SpanFirstQuery
 *
 *****************************************************************************/

static char *spanfq_to_s(Query *self, const char *field)
{
    Query *match = SpFQ(self)->match;
    char *q_str = match->to_s(match, field);
    char *res = strfmt("span_first(%s, %d)", q_str, SpFQ(self)->end);
    free(q_str);
    return res;
}

static void spanfq_extract_terms(Query *self, HashSet *terms)
{
    SpFQ(self)->match->extract_terms(SpFQ(self)->match, terms);
}

static HashSet *spanfq_get_terms(Query *self)
{
    SpanFirstQuery *sfq = SpFQ(self);
    return SpQ(sfq->match)->get_terms(sfq->match);
}

static Query *spanfq_rewrite(Query *self, IndexReader *ir)
{
    Query *q, *rq;

    q = SpFQ(self)->match;
    rq = q->rewrite(q, ir);
    q_deref(q);
    SpFQ(self)->match = rq;

    self->ref_cnt++;
    return self;                    /* no clauses rewrote */
}

static void spanfq_destroy_i(Query *self)
{
    q_deref(SpFQ(self)->match);
    free(SpQ(self)->field);
    spanq_destroy_i(self);
}

static unsigned long spanfq_hash(Query *self)
{
    return spanq_hash(self) ^ SpFQ(self)->match->hash(SpFQ(self)->match)
        ^ SpFQ(self)->end;
}

static int spanfq_eq(Query *self, Query *o)
{
    SpanFirstQuery *sfq1 = SpFQ(self);
    SpanFirstQuery *sfq2 = SpFQ(o);
    return spanq_eq(self, o) && sfq1->match->eq(sfq1->match, sfq2->match)
        && (sfq1->end == sfq2->end);
}

Query *spanfq_new_nr(Query *match, int end)
{
    Query *self = q_new(SpanFirstQuery);

    SpFQ(self)->match       = match;
    SpFQ(self)->end         = end;

    SpQ(self)->field        = estrdup(SpQ(match)->field);
    SpQ(self)->get_spans    = &spanfe_new;
    SpQ(self)->get_terms    = &spanfq_get_terms;

    self->type              = SPAN_FIRST_QUERY;
    self->rewrite           = &spanfq_rewrite;
    self->extract_terms     = &spanfq_extract_terms;
    self->to_s              = &spanfq_to_s;
    self->hash              = &spanfq_hash;
    self->eq                = &spanfq_eq;
    self->destroy_i         = &spanfq_destroy_i;
    self->create_weight_i   = &spanw_new;
    self->get_matchv_i      = &spanq_get_matchv_i;

    return self;
}

Query *spanfq_new(Query *match, int end)
{
    REF(match);
    return spanfq_new_nr(match, end);
}

/*****************************************************************************
 *
 * SpanOrQuery
 *
 *****************************************************************************/

static char *spanoq_to_s(Query *self, const char *field)
{
    int i;
    SpanOrQuery *soq = SpOQ(self);
    char *res, *res_p;
    char **q_strs = ALLOC_N(char *, soq->c_cnt);
    int len = 50;
    for (i = 0; i < soq->c_cnt; i++) {
        Query *clause = soq->clauses[i];
        q_strs[i] = clause->to_s(clause, field);
        len += strlen(q_strs[i])  + 2;
    }

    res_p = res = ALLOC_N(char, len);
    sprintf(res_p, "span_or[ ");
    res_p += strlen(res_p);
    for (i = 0; i < soq->c_cnt; i++) {
        sprintf(res_p, "%s, ", q_strs[i]);
        free(q_strs[i]);
        res_p += strlen(res_p);
    }
    free(q_strs);

    sprintf(res_p - 2, " ]");
    return res;
}

static void spanoq_extract_terms(Query *self, HashSet *terms)
{
    SpanOrQuery *soq = SpOQ(self);
    int i;
    for (i = 0; i < soq->c_cnt; i++) {
        Query *clause = soq->clauses[i];
        clause->extract_terms(clause, terms);
    }
}

static HashSet *spanoq_get_terms(Query *self)
{
    SpanOrQuery *soq = SpOQ(self);
    HashSet *terms = hs_new_str(&free);
    int i;
    for (i = 0; i < soq->c_cnt; i++) {
        Query *clause = soq->clauses[i];
        HashSet *sub_terms = SpQ(clause)->get_terms(clause);
        hs_merge(terms, sub_terms);
    }

    return terms;
}

static SpanEnum *spanoq_get_spans(Query *self, IndexReader *ir)
{
    SpanOrQuery *soq = SpOQ(self);
    if (soq->c_cnt == 1) {
        Query *q = soq->clauses[0];
        return SpQ(q)->get_spans(q, ir);
    }

    return spanoe_new(self, ir);
}

static Query *spanoq_rewrite(Query *self, IndexReader *ir)
{
    SpanOrQuery *soq = SpOQ(self);
    int i;

    /* replace clauses with their rewritten queries */
    for (i = 0; i < soq->c_cnt; i++) {
        Query *clause = soq->clauses[i];
        Query *rewritten = clause->rewrite(clause, ir);
        q_deref(clause);
        soq->clauses[i] = rewritten;
    }

    self->ref_cnt++;
    return self;
}

static void spanoq_destroy_i(Query *self)
{
    SpanOrQuery *soq = SpOQ(self);

    int i;
    for (i = 0; i < soq->c_cnt; i++) {
        Query *clause = soq->clauses[i];
        q_deref(clause);
    }
    free(soq->clauses);
    free(SpQ(self)->field);

    spanq_destroy_i(self);
}

static unsigned long spanoq_hash(Query *self)
{
    int i;
    unsigned long hash = spanq_hash(self);
    SpanOrQuery *soq = SpOQ(self);

    for (i = 0; i < soq->c_cnt; i++) {
        Query *q = soq->clauses[i];
        hash ^= q->hash(q);
    }
    return hash;
}

static int spanoq_eq(Query *self, Query *o)
{
    int i;
    Query *q1, *q2;
    SpanOrQuery *soq1 = SpOQ(self);
    SpanOrQuery *soq2 = SpOQ(o);

    if (!spanq_eq(self, o) || soq1->c_cnt != soq2->c_cnt) {
        return false;
    }
    for (i = 0; i < soq1->c_cnt; i++) {
        q1 = soq1->clauses[i];
        q2 = soq2->clauses[i];
        if (!q1->eq(q1, q2)) {
            return false;
        }
    }
    return true;
}

Query *spanoq_new()
{
    Query *self             = q_new(SpanOrQuery);
    SpOQ(self)->clauses     = ALLOC_N(Query *, CLAUSE_INIT_CAPA);
    SpOQ(self)->c_capa      = CLAUSE_INIT_CAPA;

    SpQ(self)->field        = estrdup((char *)EMPTY_STRING);
    SpQ(self)->get_spans    = &spanoq_get_spans;
    SpQ(self)->get_terms    = &spanoq_get_terms;

    self->type              = SPAN_OR_QUERY;
    self->rewrite           = &spanoq_rewrite;
    self->extract_terms     = &spanoq_extract_terms;
    self->to_s              = &spanoq_to_s;
    self->hash              = &spanoq_hash;
    self->eq                = &spanoq_eq;
    self->destroy_i         = &spanoq_destroy_i;
    self->create_weight_i   = &spanw_new;
    self->get_matchv_i      = &spanq_get_matchv_i;

    return self;
}

Query *spanoq_add_clause_nr(Query *self, Query *clause)
{
    const int curr_index = SpOQ(self)->c_cnt++;
    if (clause->type < SPAN_TERM_QUERY || clause->type > SPAN_NEAR_QUERY) {
        RAISE(ARG_ERROR, "Tried to add a %s to a SpanOrQuery. This is not a "
              "SpanQuery.", q_get_query_name(clause->type));
    }
    if (curr_index == 0) {
        free(SpQ(self)->field);
        SpQ(self)->field = estrdup(SpQ(clause)->field);
    }
    else if (strcmp(SpQ(self)->field, SpQ(clause)->field) != 0) {
        RAISE(ARG_ERROR, "All clauses in a SpanQuery must have the same field. "
              "Attempted to add a SpanQuery with field \"%s\" to a SpanOrQuery "
              "with field \"%s\"", SpQ(clause)->field, SpQ(self)->field);
    }
    if (curr_index >= SpOQ(self)->c_capa) {
        SpOQ(self)->c_capa <<= 1;
        REALLOC_N(SpOQ(self)->clauses, Query *, SpOQ(self)->c_capa);
    }
    SpOQ(self)->clauses[curr_index] = clause;
    return clause;
}

Query *spanoq_add_clause(Query *self, Query *clause)
{
    REF(clause);
    return spanoq_add_clause_nr(self, clause);
}

/*****************************************************************************
 *
 * SpanNearQuery
 *
 *****************************************************************************/

static char *spannq_to_s(Query *self, const char *field)
{
    int i;
    SpanNearQuery *snq = SpNQ(self);
    char *res, *res_p;
    char **q_strs = ALLOC_N(char *, snq->c_cnt);
    int len = 50;
    for (i = 0; i < snq->c_cnt; i++) {
        Query *clause = snq->clauses[i];
        q_strs[i] = clause->to_s(clause, field);
        len += strlen(q_strs[i]);
    }

    res_p = res = ALLOC_N(char, len);
    sprintf(res_p, "span_near[ ");
    res_p += strlen(res_p);
    for (i = 0; i < snq->c_cnt; i++) {
        sprintf(res_p, "%s, ", q_strs[i]);
        free(q_strs[i]);
        res_p += strlen(res_p);
    }
    free(q_strs);

    sprintf(res_p - 2, " ]");
    return res;
}

static void spannq_extract_terms(Query *self, HashSet *terms)
{
    SpanNearQuery *snq = SpNQ(self);
    int i;
    for (i = 0; i < snq->c_cnt; i++) {
        Query *clause = snq->clauses[i];
        clause->extract_terms(clause, terms);
    }
}

static HashSet *spannq_get_terms(Query *self)
{
    SpanNearQuery *snq = SpNQ(self);
    HashSet *terms = hs_new_str(&free);
    int i;
    for (i = 0; i < snq->c_cnt; i++) {
        Query *clause = snq->clauses[i];
        HashSet *sub_terms = SpQ(clause)->get_terms(clause);
        hs_merge(terms, sub_terms);
    }

    return terms;
}

static SpanEnum *spannq_get_spans(Query *self, IndexReader *ir)
{
    SpanNearQuery *snq = SpNQ(self);

    if (snq->c_cnt == 1) {
        Query *q = snq->clauses[0];
        return SpQ(q)->get_spans(q, ir);
    }

    return spanne_new(self, ir);
}

static Query *spannq_rewrite(Query *self, IndexReader *ir)
{
    SpanNearQuery *snq = SpNQ(self);
    int i;
    for (i = 0; i < snq->c_cnt; i++) {
        Query *clause = snq->clauses[i];
        Query *rewritten = clause->rewrite(clause, ir);
        q_deref(clause);
        snq->clauses[i] = rewritten;
    }

    self->ref_cnt++;
    return self;
}

static void spannq_destroy(Query *self)
{
    SpanNearQuery *snq = SpNQ(self);

    int i;
    for (i = 0; i < snq->c_cnt; i++) {
        Query *clause = snq->clauses[i];
        q_deref(clause);
    }
    free(snq->clauses);
    free(SpQ(self)->field);

    spanq_destroy_i(self);
}

static unsigned long spannq_hash(Query *self)
{
    int i;
    unsigned long hash = spanq_hash(self);
    SpanNearQuery *snq = SpNQ(self);

    for (i = 0; i < snq->c_cnt; i++) {
        Query *q = snq->clauses[i];
        hash ^= q->hash(q);
    }
    return ((hash ^ snq->slop) << 1) | snq->in_order;
}

static int spannq_eq(Query *self, Query *o)
{
    int i;
    Query *q1, *q2;
    SpanNearQuery *snq1 = SpNQ(self);
    SpanNearQuery *snq2 = SpNQ(o);
    if (! spanq_eq(self, o)
        || (snq1->c_cnt != snq2->c_cnt)
        || (snq1->slop != snq2->slop)
        || (snq1->in_order != snq2->in_order)) {
        return false;
    }

    for (i = 0; i < snq1->c_cnt; i++) {
        q1 = snq1->clauses[i];
        q2 = snq2->clauses[i];
        if (!q1->eq(q1, q2)) {
            return false;
        }
    }

    return true;
}

Query *spannq_new(int slop, bool in_order)
{
    Query *self             = q_new(SpanNearQuery);

    SpNQ(self)->clauses     = ALLOC_N(Query *, CLAUSE_INIT_CAPA);
    SpNQ(self)->c_capa      = CLAUSE_INIT_CAPA;
    SpNQ(self)->slop        = slop;
    SpNQ(self)->in_order    = in_order;

    SpQ(self)->get_spans    = &spannq_get_spans;
    SpQ(self)->get_terms    = &spannq_get_terms;
    SpQ(self)->field        = estrdup((char *)EMPTY_STRING);

    self->type              = SPAN_NEAR_QUERY;
    self->rewrite           = &spannq_rewrite;
    self->extract_terms     = &spannq_extract_terms;
    self->to_s              = &spannq_to_s;
    self->hash              = &spannq_hash;
    self->eq                = &spannq_eq;
    self->destroy_i         = &spannq_destroy;
    self->create_weight_i   = &spanw_new;
    self->get_matchv_i      = &spanq_get_matchv_i;

    return self;
}

Query *spannq_add_clause_nr(Query *self, Query *clause)
{
    const int curr_index = SpNQ(self)->c_cnt++;
    if (clause->type < SPAN_TERM_QUERY || clause->type > SPAN_NEAR_QUERY) {
        RAISE(ARG_ERROR, "Tried to add a %s to a SpanNearQuery. This is not a "
              "SpanQuery.", q_get_query_name(clause->type));
    }
    if (curr_index == 0) {
        free(SpQ(self)->field);
        SpQ(self)->field = estrdup(SpQ(clause)->field);
    }
    else if (strcmp(SpQ(self)->field, SpQ(clause)->field) != 0) {
        RAISE(ARG_ERROR, "All clauses in a SpanQuery must have the same field. "
              "Attempted to add a SpanQuery with field \"%s\" to SpanNearQuery "
              "with field \"%s\"", SpQ(clause)->field, SpQ(self)->field);
    }
    if (curr_index >= SpNQ(self)->c_capa) {
        SpNQ(self)->c_capa <<= 1;
        REALLOC_N(SpNQ(self)->clauses, Query *, SpNQ(self)->c_capa);
    }
    SpNQ(self)->clauses[curr_index] = clause;
    return clause;
}

Query *spannq_add_clause(Query *self, Query *clause)
{
    REF(clause);
    return spannq_add_clause_nr(self, clause);
}

/*****************************************************************************
 *
 * SpanNotQuery
 *
 *****************************************************************************/

static char *spanxq_to_s(Query *self, const char *field)
{
    SpanNotQuery *sxq = SpXQ(self);
    char *inc_s = sxq->inc->to_s(sxq->inc, field);
    char *exc_s = sxq->exc->to_s(sxq->exc, field);
    char *res = strfmt("span_not(inc:<%s>, exc:<%s>)", inc_s, exc_s);

    free(inc_s);
    free(exc_s);
    return res;
}

static void spanxq_extract_terms(Query *self, HashSet *terms)
{
    SpXQ(self)->inc->extract_terms(SpXQ(self)->inc, terms);
}

static HashSet *spanxq_get_terms(Query *self)
{
    return SpQ(SpXQ(self)->inc)->get_terms(SpXQ(self)->inc);
}

static Query *spanxq_rewrite(Query *self, IndexReader *ir)
{
    SpanNotQuery *sxq = SpXQ(self);
    Query *q, *rq;

    /* rewrite inclusive query */
    q = sxq->inc;
    rq = q->rewrite(q, ir);
    q_deref(q);
    sxq->inc = rq;

    /* rewrite exclusive query */
    q = sxq->exc;
    rq = q->rewrite(q, ir);
    q_deref(q);
    sxq->exc = rq;

    self->ref_cnt++;
    return self;
}

static void spanxq_destroy(Query *self)
{
    SpanNotQuery *sxq = SpXQ(self);

    q_deref(sxq->inc);
    q_deref(sxq->exc);

    free(SpQ(self)->field);

    spanq_destroy_i(self);
}

static unsigned long spanxq_hash(Query *self)
{
    SpanNotQuery *sxq = SpXQ(self);
    return spanq_hash(self) ^ sxq->inc->hash(sxq->inc)
        ^ sxq->exc->hash(sxq->exc);
}

static int spanxq_eq(Query *self, Query *o)
{
    SpanNotQuery *sxq1 = SpXQ(self);
    SpanNotQuery *sxq2 = SpXQ(o);
    return spanq_eq(self, o) && sxq1->inc->eq(sxq1->inc, sxq2->inc)
        && sxq1->exc->eq(sxq1->exc, sxq2->exc);
}


Query *spanxq_new_nr(Query *inc, Query *exc)
{
    Query *self;
    if (strcmp(SpQ(inc)->field, SpQ(inc)->field) != 0) {
        RAISE(ARG_ERROR, "All clauses in a SpanQuery must have the same field. "
              "Attempted to add a SpanQuery with field \"%s\" along with a "
              "SpanQuery with field \"%s\" to an SpanNotQuery",
              SpQ(inc)->field, SpQ(exc)->field);
    }
    self = q_new(SpanNotQuery);

    SpXQ(self)->inc         = inc;
    SpXQ(self)->exc         = exc;

    SpQ(self)->field        = estrdup(SpQ(inc)->field);
    SpQ(self)->get_spans    = &spanxe_new;
    SpQ(self)->get_terms    = &spanxq_get_terms;

    self->type              = SPAN_NOT_QUERY;
    self->rewrite           = &spanxq_rewrite;
    self->extract_terms     = &spanxq_extract_terms;
    self->to_s              = &spanxq_to_s;
    self->hash              = &spanxq_hash;
    self->eq                = &spanxq_eq;
    self->destroy_i         = &spanxq_destroy;
    self->create_weight_i   = &spanw_new;
    self->get_matchv_i      = &spanq_get_matchv_i;

    return self;
}

Query *spanxq_new(Query *inc, Query *exc)
{
    REF(inc);
    REF(exc);
    return spanxq_new_nr(inc, exc);
}


/*****************************************************************************
 *
 * Rewritables
 *
 *****************************************************************************/

/*****************************************************************************
 *
 * SpanPrefixQuery
 *
 *****************************************************************************/

#define SpPfxQ(query) ((SpanPrefixQuery *)(query))

static char *spanprq_to_s(Query *self, const char *current_field) 
{
    char *buffer, *bptr;
    const char *prefix = SpPfxQ(self)->prefix;
    const char *field = SpQ(self)->field;
    size_t plen = strlen(prefix);
    size_t flen = strlen(field);

    bptr = buffer = ALLOC_N(char, plen + flen + 35);

    if (strcmp(field, current_field) != 0) {
        sprintf(bptr, "%s:", field);
        bptr += flen + 1;
    }

    sprintf(bptr, "%s*", prefix);
    bptr += plen + 1;
    if (self->boost != 1.0) {
        *bptr = '^';
        dbl_to_s(++bptr, self->boost);
    }

    return buffer;
}

static Query *spanprq_rewrite(Query *self, IndexReader *ir)
{
    const char *field = SpQ(self)->field;
    const int field_num = fis_get_field_num(ir->fis, field);
    Query *volatile q = spanmtq_new_conf(field, SpPfxQ(self)->max_terms);
    q->boost = self->boost;        /* set the boost */

    if (field_num >= 0) {
        const char *prefix = SpPfxQ(self)->prefix;
        TermEnum *te = ir->terms_from(ir, field_num, prefix);
        const char *term = te->curr_term;
        size_t prefix_len = strlen(prefix);

        TRY
            do { 
                if (strncmp(term, prefix, prefix_len) != 0) {
                    break;
                }
                spanmtq_add_term(q, term);       /* found a match */
            } while (te->next(te));
        XFINALLY
            te->close(te);
        XENDTRY
    }

    return q;
}

static void spanprq_destroy(Query *self)
{
    free(SpQ(self)->field);
    free(SpPfxQ(self)->prefix);
    spanq_destroy_i(self);
}

static unsigned long spanprq_hash(Query *self)
{
    return str_hash(SpQ(self)->field) ^ str_hash(SpPfxQ(self)->prefix);
}

static int spanprq_eq(Query *self, Query *o)
{
    return (strcmp(SpPfxQ(self)->prefix, SpPfxQ(o)->prefix) == 0) 
        && (strcmp(SpQ(self)->field,  SpQ(o)->field) == 0);
}

Query *spanprq_new(const char *field, const char *prefix)
{
    Query *self = q_new(SpanPrefixQuery);

    SpQ(self)->field        = estrdup(field);
    SpPfxQ(self)->prefix    = estrdup(prefix);
    SpPfxQ(self)->max_terms = SPAN_PREFIX_QUERY_MAX_TERMS;

    self->type              = SPAN_PREFIX_QUERY;
    self->rewrite           = &spanprq_rewrite;
    self->to_s              = &spanprq_to_s;
    self->hash              = &spanprq_hash;
    self->eq                = &spanprq_eq;
    self->destroy_i         = &spanprq_destroy;
    self->create_weight_i   = &q_create_weight_unsup;

    return self;
}
