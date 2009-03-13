#ifndef FRT_SEARCH_H
#define FRT_SEARCH_H

typedef struct Query Query;
typedef struct Weight Weight;
typedef struct Scorer Scorer;

#include "index.h"
#include "bitvector.h"
#include "similarity.h"

/***************************************************************************
 *
 * Explanation
 *
 ***************************************************************************/

#define EXPLANATION_DETAILS_START_SIZE 4
typedef struct Explanation
{
    float value;
    char *description;
    struct Explanation **details;
} Explanation;
 
extern Explanation *expl_new(float value, const char *description, ...);
extern void expl_destroy(Explanation *expl);
extern Explanation *expl_add_detail(Explanation *expl, Explanation *detail);
extern char *expl_to_s_depth(Explanation *expl, int depth);
extern char *expl_to_html(Explanation *expl);

#define expl_to_s(expl) expl_to_s_depth(expl, 0)

/***************************************************************************
 *
 * Highlighter
 *
 ***************************************************************************/

typedef struct MatchRange
{
    int start;
    int end;
    int start_offset;
    int end_offset;
    double score;
} MatchRange;

#define MATCH_VECTOR_INIT_CAPA 8
typedef struct MatchVector
{
    int size;
    int capa;
    MatchRange *matches;
} MatchVector;

extern MatchVector *matchv_new();
extern MatchVector *matchv_add(MatchVector *mp, int start, int end);
extern MatchVector *matchv_sort(MatchVector *self);
extern void matchv_destroy(MatchVector *self);
extern MatchVector *matchv_compact(MatchVector *self);
extern MatchVector *matchv_compact_with_breaks(MatchVector *self);

/***************************************************************************
 *
 * Hit
 *
 ***************************************************************************/

typedef struct Hit
{
    int doc;
    float score;
} Hit;

/***************************************************************************
 *
 * TopDocs
 *
 ***************************************************************************/

typedef struct TopDocs
{
    int total_hits;
    int size;
    Hit **hits;
    float max_score;
} TopDocs;

extern TopDocs *td_new(int total_hits, int size, Hit **hits, float max_score);
extern void td_destroy(TopDocs *td);
extern char *td_to_s(TopDocs *td);

/***************************************************************************
 *
 * Filter
 *
 ***************************************************************************/

typedef struct Filter
{
    char           *name;
    HashTable      *cache;
    BitVector      *(*get_bv_i)(struct Filter *self, IndexReader *ir);
    char           *(*to_s)(struct Filter *self);
    unsigned long   (*hash)(struct Filter *self);
    int             (*eq)(struct Filter *self, struct Filter *o);
    void            (*destroy_i)(struct Filter *self);
    int             ref_cnt;
} Filter;

#define filt_new(type) filt_create(sizeof(type), #type)
extern Filter *filt_create(size_t size, const char *name);
extern BitVector *filt_get_bv(Filter *filt, IndexReader *ir);
extern void filt_destroy_i(Filter *filt);
extern void filt_deref(Filter *filt);
extern unsigned long filt_hash(Filter *filt);
extern int filt_eq(Filter *filt, Filter *o);

/***************************************************************************
 *
 * RangeFilter
 *
 ***************************************************************************/

extern Filter *rfilt_new(const char *field,
                         const char *lower_term, const char *upper_term,
                         bool include_lower, bool include_upper);

/***************************************************************************
 *
 * QueryFilter
 *
 ***************************************************************************/

extern Filter *qfilt_new(Query *query);
extern Filter *qfilt_new_nr(Query *query);

/***************************************************************************
 *
 * Weight
 *
 ***************************************************************************/

struct Weight
{
    float        value;
    float        qweight;
    float        qnorm;
    float        idf;
    Query       *query;
    Similarity  *similarity;
    Query       *(*get_query)(Weight *self);
    float        (*get_value)(Weight *self);
    void         (*normalize)(Weight *self, float normalization_factor);
    Scorer      *(*scorer)(Weight *self, IndexReader *ir);
    Explanation *(*explain)(Weight *self, IndexReader *ir, int doc_num);
    float        (*sum_of_squared_weights)(Weight *self);
    char        *(*to_s)(Weight *self);
    void         (*destroy)(Weight *self);
};

#define w_new(type, query) w_create(sizeof(type), query)
extern Weight *w_create(size_t size, Query *query);
extern void w_destroy(Weight *self);
extern Query *w_get_query(Weight *self);
extern float w_get_value(Weight *self);
extern float w_sum_of_squared_weights(Weight *self);
extern void w_normalize(Weight *self, float normalization_factor);

/***************************************************************************
 *
 * Query
 *
 ***************************************************************************/

enum QUERY_TYPE
{
    TERM_QUERY,
    MULTI_TERM_QUERY,
    BOOLEAN_QUERY,
    PHRASE_QUERY,
    CONSTANT_QUERY,
    FILTERED_QUERY,
    MATCH_ALL_QUERY,
    RANGE_QUERY,
    WILD_CARD_QUERY,
    FUZZY_QUERY,
    PREFIX_QUERY,
    SPAN_TERM_QUERY,
    SPAN_MULTI_TERM_QUERY,
    SPAN_PREFIX_QUERY,
    SPAN_FIRST_QUERY,
    SPAN_OR_QUERY,
    SPAN_NOT_QUERY,
    SPAN_NEAR_QUERY
};

struct Query
{
    int           ref_cnt;
    float         boost;
    Weight        *weight;
    Query        *(*rewrite)(Query *self, IndexReader *ir);
    void          (*extract_terms)(Query *self, HashSet *terms);
    Similarity   *(*get_similarity)(Query *self, Searcher *searcher);
    char         *(*to_s)(Query *self, const char *field);
    unsigned long (*hash)(Query *self);
    int           (*eq)(Query *self, Query *o);
    void          (*destroy_i)(Query *self);
    Weight       *(*create_weight_i)(Query *self, Searcher *searcher);
    MatchVector  *(*get_matchv_i)(Query *self, MatchVector *mv, TermVector *tv);
    enum QUERY_TYPE type;
};

/* Internal Query Functions */
extern Similarity *q_get_similarity_i(Query *self, Searcher *searcher);
extern void q_destroy_i(Query *self);
extern Weight *q_create_weight_unsup(Query *self, Searcher *searcher);

extern void q_deref(Query *self);
extern const char *q_get_query_name(enum QUERY_TYPE type);
extern Weight *q_weight(Query *self, Searcher *searcher);
extern Query *q_combine(Query **queries, int q_cnt);
extern unsigned long q_hash(Query *self);
extern int q_eq(Query *self, Query *o);
extern Query *q_create(size_t size);
#define q_new(type) q_create(sizeof(type))

/***************************************************************************
 * TermQuery
 ***************************************************************************/

typedef struct TermQuery
{
    Query super;
    char *field;
    char *term;
} TermQuery;

Query *tq_new(const char *field, const char *term);

/***************************************************************************
 * BooleanQuery
 ***************************************************************************/

/* *** BooleanClause *** */

enum BC_TYPE
{
    BC_SHOULD,
    BC_MUST,
    BC_MUST_NOT
};

typedef struct BooleanClause
{
    int ref_cnt;
    Query *query;
    unsigned int occur : 4;
    bool is_prohibited : 1;
    bool is_required : 1;
} BooleanClause;

extern BooleanClause *bc_new(Query *query, enum BC_TYPE occur);
extern void bc_deref(BooleanClause *self);
extern void bc_set_occur(BooleanClause *self, enum BC_TYPE occur);

/* *** BooleanQuery *** */

#define DEFAULT_MAX_CLAUSE_COUNT 1024
#define BOOLEAN_CLAUSES_START_CAPA 4
#define QUERY_STRING_START_SIZE 64

typedef struct BooleanQuery
{
    Query           super;
    bool            coord_disabled;
    int             max_clause_cnt;
    int             clause_cnt;
    int             clause_capa;
    float           original_boost;
    BooleanClause **clauses;
    Similarity     *similarity;
} BooleanQuery;

extern Query *bq_new(bool coord_disabled);
extern Query *bq_new_max(bool coord_disabled, int max);
extern BooleanClause *bq_add_query(Query *self, Query *sub_query,
                                   enum BC_TYPE occur);
extern BooleanClause *bq_add_query_nr(Query *self, Query *sub_query,
                                      enum BC_TYPE occur);
extern BooleanClause *bq_add_clause(Query *self, BooleanClause *bc);
extern BooleanClause *bq_add_clause_nr(Query *self, BooleanClause *bc);

/***************************************************************************
 * PhraseQuery
 ***************************************************************************/

#define PHQ_INIT_CAPA 4
typedef struct PhraseQuery
{
    Query           super;
    int             slop;
    char           *field;
    PhrasePosition *positions;
    int             pos_cnt;
    int             pos_capa;
} PhraseQuery;

extern Query *phq_new(const char *field);
extern void phq_add_term(Query *self, const char *term, int pos_inc);
extern void phq_add_term_abs(Query *self, const char *term, int position);
extern void phq_append_multi_term(Query *self, const char *term);

/***************************************************************************
 * MultiTermQuery
 ***************************************************************************/

#define MULTI_TERM_QUERY_MAX_TERMS 256
typedef struct MultiTermQuery
{
    Query           super;
    char           *field;
    PriorityQueue  *boosted_terms;
    float           min_boost;
} MultiTermQuery;

extern void multi_tq_add_term(Query *self, const char *term);
extern void multi_tq_add_term_boost(Query *self, const char *term, float boost);
extern Query *multi_tq_new(const char *field);
extern Query *multi_tq_new_conf(const char *field, int max_terms,
                                          float min_boost);

#define MTQMaxTerms(query) (((MTQSubQuery *)(query))->max_terms)
typedef struct MTQSubQuery
{
    Query super;
    int   max_terms;
} MTQSubQuery;

/***************************************************************************
 * PrefixQuery
 ***************************************************************************/

#define PREFIX_QUERY_MAX_TERMS 256

typedef struct PrefixQuery
{
    MTQSubQuery super;
    char *field;
    char *prefix;
} PrefixQuery;

extern Query *prefixq_new(const char *field, const char *prefix);

/***************************************************************************
 * WildCardQuery
 ***************************************************************************/

#define WILD_CHAR '?'
#define WILD_STRING '*'
#define WILD_CARD_QUERY_MAX_TERMS 256

typedef struct WildCardQuery
{
    MTQSubQuery super;
    char *field;
    char *pattern;
} WildCardQuery;


extern Query *wcq_new(const char *field, const char *pattern);
extern bool wc_match(const char *pattern, const char *text);

/***************************************************************************
 * FuzzyQuery
 ***************************************************************************/

#define DEF_MIN_SIM 0.5f
#define DEF_PRE_LEN 0
#define DEF_MAX_TERMS 256
#define TYPICAL_LONGEST_WORD 20

typedef struct FuzzyQuery
{
    MTQSubQuery super;
    char       *field;
    char       *term;
    const char *text; /* term text after prefix */
    int         text_len;
    int         pre_len;
    float       min_sim;
    float       scale_factor;
    int         max_distances[TYPICAL_LONGEST_WORD];
    int        *da;
} FuzzyQuery;

extern Query *fuzq_new(const char *term, const char *field);
extern Query *fuzq_new_conf(const char *field, const char *term,
                            float min_sim, int pre_len, int max_terms);

/***************************************************************************
 * ConstantScoreQuery
 ***************************************************************************/

typedef struct ConstantScoreQuery
{
    Query   super;
    Filter *filter;
    Query  *original;
} ConstantScoreQuery;

extern Query *csq_new(Filter *filter);
extern Query *csq_new_nr(Filter *filter);

/***************************************************************************
 * FilteredQuery
 ***************************************************************************/

typedef struct FilteredQuery
{
    Query   super;
    Query  *query;
    Filter *filter;
} FilteredQuery;

extern Query *fq_new(Query *query, Filter *filter);

/***************************************************************************
 * MatchAllQuery
 ***************************************************************************/

extern Query *maq_new();

/***************************************************************************
 * RangeQuery
 ***************************************************************************/

extern Query *rq_new(const char *field, const char *lower_term,
                     const char *upper_term, bool include_lower,
                     bool include_upper);
extern Query *rq_new_less(const char *field, const char *upper_term,
                          bool include_upper);
extern Query *rq_new_more(const char *field, const char *lower_term,
                          bool include_lower);

/***************************************************************************
 * SpanQuery
 ***************************************************************************/

/* ** SpanEnum ** */
typedef struct SpanEnum SpanEnum;
struct SpanEnum
{
    Query *query;
    bool (*next)(SpanEnum *self);
    bool (*skip_to)(SpanEnum *self, int target_doc);
    int  (*doc)(SpanEnum *self);
    int  (*start)(SpanEnum *self);
    int  (*end)(SpanEnum *self);
    char *(*to_s)(SpanEnum *self);
    void (*destroy)(SpanEnum *self);
};

/* ** SpanQuery ** */
typedef struct SpanQuery
{
    Query        super;
    char        *field;
    SpanEnum    *(*get_spans)(Query *self, IndexReader *ir);
    HashSet     *(*get_terms)(Query *self);
} SpanQuery;

/***************************************************************************
 * SpanTermQuery
 ***************************************************************************/

typedef struct SpanTermQuery
{
    SpanQuery super;
    char     *term;
} SpanTermQuery;
extern Query *spantq_new(const char *field, const char *term);

/***************************************************************************
 * SpanMultiTermQuery
 ***************************************************************************/

#define SPAN_MULTI_TERM_QUERY_CAPA 1024
typedef struct SpanMultiTermQuery
{
    SpanQuery super;
    char    **terms;
    int       term_cnt;
    int       term_capa;
} SpanMultiTermQuery;

extern Query *spanmtq_new(const char *field);
extern Query *spanmtq_new_conf(const char *field, int max_size);
extern void spanmtq_add_term(Query *self, const char *term);

/***************************************************************************
 * SpanFirstQuery
 ***************************************************************************/

typedef struct SpanFirstQuery
{
    SpanQuery   super;
    int         end;
    Query      *match;
} SpanFirstQuery;

extern Query *spanfq_new(Query *match, int end);
extern Query *spanfq_new_nr(Query *match, int end);

/***************************************************************************
 * SpanOrQuery
 ***************************************************************************/

typedef struct SpanOrQuery
{
    SpanQuery   super;
    Query     **clauses;
    int         c_cnt;
    int         c_capa;
} SpanOrQuery;

extern Query *spanoq_new();
extern Query *spanoq_add_clause(Query *self, Query *clause);
extern Query *spanoq_add_clause_nr(Query *self, Query *clause);

/***************************************************************************
 * SpanNearQuery
 ***************************************************************************/

typedef struct SpanNearQuery
{
    SpanQuery   super;
    Query     **clauses;
    int         c_cnt;
    int         c_capa;
    int         slop;
    bool        in_order : 1;
} SpanNearQuery;

extern Query *spannq_new(int slop, bool in_order);
extern Query *spannq_add_clause(Query *self, Query *clause);
extern Query *spannq_add_clause_nr(Query *self, Query *clause);

/***************************************************************************
 * SpanNotQuery
 ***************************************************************************/

typedef struct SpanNotQuery
{
    SpanQuery   super;
    Query      *inc;
    Query      *exc;
} SpanNotQuery;

extern Query *spanxq_new(Query *inc, Query *exc);
extern Query *spanxq_new_nr(Query *inc, Query *exc);


/***************************************************************************
 * SpanPrefixQuery
 ***************************************************************************/

#define SPAN_PREFIX_QUERY_MAX_TERMS 256

typedef struct SpanPrefixQuery
{
    SpanQuery   super;
    char       *prefix;
    int         max_terms;
} SpanPrefixQuery;

extern Query *spanprq_new(const char *field, const char *prefix);


/***************************************************************************
 *
 * Scorer
 *
 ***************************************************************************/

#define SCORER_NULLIFY(mscorer) do {\
    (mscorer)->destroy(mscorer);\
    (mscorer) = NULL;\
} while (0)

struct Scorer
{
    Similarity  *similarity;
    int          doc;
    float        (*score)(Scorer *self);
    bool         (*next)(Scorer *self);
    bool         (*skip_to)(Scorer *self, int doc_num);
    Explanation *(*explain)(Scorer *self, int doc_num);
    void         (*destroy)(Scorer *self);
};

#define scorer_new(type, similarity) scorer_create(sizeof(type), similarity)
/* Internal Scorer Function */
extern void scorer_destroy_i(Scorer *self);
extern Scorer *scorer_create(size_t size, Similarity *similarity);
extern bool scorer_less_than(void *p1, void *p2);
extern bool scorer_doc_less_than(const Scorer *s1, const Scorer *s2);
extern int scorer_doc_cmp(const void *p1, const void *p2);

/***************************************************************************
 *
 * Sort
 *
 ***************************************************************************/

enum SORT_TYPE
{
    SORT_TYPE_SCORE,
    SORT_TYPE_DOC,
    SORT_TYPE_BYTE,
    SORT_TYPE_INTEGER,
    SORT_TYPE_FLOAT,
    SORT_TYPE_STRING,
    SORT_TYPE_AUTO
};

/***************************************************************************
 * Comparable
 ***************************************************************************/

typedef struct Comparable
{
    int type;
    union {
        int i;
        float f;
        char *s;
        void *p;
    } val;
    bool reverse : 1;
} Comparable;

/***************************************************************************
 * SortField
 ***************************************************************************/

typedef struct SortField
{
    mutex_t         mutex;
    char           *field;
    enum SORT_TYPE  type;
    bool            reverse : 1;
    void           *index;
    int             (*compare)(void *index_ptr, Hit *hit1, Hit *hit2);
    void            (*get_val)(void *index_ptr, Hit *hit1, Comparable *comparable);
    void           *(*create_index)(int size);
    void            (*destroy_index)(void *p);
    void            (*handle_term)(void *index, TermDocEnum *tde, char *text);
} SortField;

extern SortField *sort_field_new(char *field, enum SORT_TYPE type, bool reverse);
extern SortField *sort_field_score_new(bool reverse);
extern SortField *sort_field_doc_new(bool reverse);
extern SortField *sort_field_int_new(char *field, bool reverse);
extern SortField *sort_field_byte_new(char *field, bool reverse);
extern SortField *sort_field_float_new(char *field, bool reverse);
extern SortField *sort_field_string_new(char *field, bool reverse);
extern SortField *sort_field_auto_new(char *field, bool reverse);
extern void sort_field_destroy(void *p);
extern char *sort_field_to_s(SortField *self);

extern const SortField SORT_FIELD_SCORE; 
extern const SortField SORT_FIELD_SCORE_REV; 
extern const SortField SORT_FIELD_DOC; 
extern const SortField SORT_FIELD_DOC_REV; 

/***************************************************************************
 * Sort
 ***************************************************************************/

typedef struct Sort
{
    SortField **sort_fields;
    int size;
    int capa;
    int start;
    bool destroy_all : 1;
} Sort;

extern Sort *sort_new();
extern void sort_destroy(void *p);
extern void sort_add_sort_field(Sort *self, SortField *sf);
extern void sort_clear(Sort *self);
extern char *sort_to_s(Sort *self);

/***************************************************************************
 * FieldSortedHitQueue
 ***************************************************************************/

extern Hit *fshq_pq_pop(PriorityQueue *pq);
extern void fshq_pq_down(PriorityQueue *pq);
extern void fshq_pq_insert(PriorityQueue *pq, Hit *hit);
extern void fshq_pq_destroy(PriorityQueue *pq);
extern PriorityQueue *fshq_pq_new(int size, Sort *sort, IndexReader *ir);
extern Hit *fshq_pq_pop_fd(PriorityQueue *pq);

/***************************************************************************
 * FieldDoc
 ***************************************************************************/

typedef struct FieldDoc
{
    Hit hit;
    int size;
    Comparable comparables[1];
} FieldDoc;

extern void fd_destroy(FieldDoc *fd);

/***************************************************************************
 * FieldDocSortedHitQueue
 ***************************************************************************/

extern bool fdshq_lt(FieldDoc *fd1, FieldDoc *fd2);

/***************************************************************************
 *
 * Searcher
 *
 ***************************************************************************/

typedef bool (*filter_ft)(int doc_num, float score, Searcher *self);

struct Searcher
{
    Similarity  *similarity;
    int          (*doc_freq)(Searcher *self, const char *field,
                             const char *term);
    Document    *(*get_doc)(Searcher *self, int doc_num);
    LazyDoc     *(*get_lazy_doc)(Searcher *self, int doc_num);
    int          (*max_doc)(Searcher *self);
    Weight      *(*create_weight)(Searcher *self, Query *query);
    TopDocs     *(*search)(Searcher *self, Query *query, int first_doc,
                           int num_docs, Filter *filter, Sort *sort,
                           filter_ft filter_func,
                           bool load_fields);
    TopDocs     *(*search_w)(Searcher *self, Weight *weight, int first_doc,
                             int num_docs, Filter *filter, Sort *sort,
                             filter_ft filter_func,
                             bool load_fields);
    void         (*search_each)(Searcher *self, Query *query, Filter *filter,
                                filter_ft filter_func,
                                void (*fn)(Searcher *, int, float, void *),
                                void *arg);
    void         (*search_each_w)(Searcher *self, Weight *weight,
                                  Filter *filter,
                                  filter_ft filter_func,
                                  void (*fn)(Searcher *, int, float, void *),
                                  void *arg);
    Query       *(*rewrite)(Searcher *self, Query *original);
    Explanation *(*explain)(Searcher *self, Query *query, int doc_num);
    Explanation *(*explain_w)(Searcher *self, Weight *weight, int doc_num);
    TermVector  *(*get_term_vector)(Searcher *self, const int doc_num,
                                    const char *field);
    Similarity  *(*get_similarity)(Searcher *self);
    void         (*close)(Searcher *self);
    void        *arg; /* used to pass values to Searcher functions */
};

#define searcher_doc_freq(s, t)         s->doc_freq(s, t)
#define searcher_get_doc(s, dn)         s->get_doc(s, dn)
#define searcher_get_lazy_doc(s, dn)    s->get_lazy_doc(s, dn)
#define searcher_max_doc(s)             s->max_doc(s)
#define searcher_rewrite(s, q)          s->rewrite(s, q)
#define searcher_explain(s, q, dn)      s->explain(s, q, dn)
#define searcher_explain_w(s, q, dn)    s->explain_w(s, q, dn)
#define searcher_get_similarity(s)      s->get_similarity(s)
#define searcher_close(s)               s->close(s)
#define searcher_search(s, q, fd, nd, filt, sort, ff)\
    s->search(s, q, fd, nd, filt, sort, ff, false)
#define searcher_search_fd(s, q, fd, nd, filt, sort, ff)\
    s->search(s, q, fd, nd, filt, sort, ff, true)
#define searcher_search_each(s, q, filt, ff, fn, arg)\
    s->search_each(s, q, filt, ff, fn, arg)
#define searcher_search_each_w(s, q, filt, ff, fn, arg)\
    s->search_each_w(s, q, filt, ff, fn, arg)


extern MatchVector *searcher_get_match_vector(Searcher *self,
                                              Query *query,
                                              const int doc_num,
                                              const char *field);
extern char **searcher_highlight(Searcher *self,
                                 Query *query,
                                 const int doc_num,
                                 const char *field,
                                 const int excerpt_len,
                                 const int num_excerpts,
                                 const char *pre_tag,
                                 const char *post_tag,
                                 const char *ellipsis);

/***************************************************************************
 *
 * IndexSearcher
 *
 ***************************************************************************/

typedef struct IndexSearcher {
    Searcher        super;
    IndexReader    *ir;
    bool            close_ir : 1;
} IndexSearcher;

extern Searcher *isea_new(IndexReader *ir);
extern int isea_doc_freq(Searcher *self, const char *field, const char *term);

/***************************************************************************
 *
 * MultiSearcher
 *
 ***************************************************************************/

typedef struct MultiSearcher
{
    Searcher    super;
    int         s_cnt;
    Searcher  **searchers;
    int        *starts;
    int         max_doc;
    bool        close_subs : 1;
} MultiSearcher;

extern Searcher *msea_new(Searcher **searchers, int s_cnt, bool close_subs);

/***************************************************************************
 *
 * QParser
 *
 ***************************************************************************/

#define QP_CONC_WORDS 2
#define QP_MAX_CLAUSES 512

typedef struct QParser
{
    mutex_t mutex;
    int def_slop;
    int max_clauses;
    int phq_pos_inc;
    char *qstr;
    char *qstrp;
    char buf[QP_CONC_WORDS][MAX_WORD_SIZE];
    char *dynbuf;
    int  buf_index;
    HashTable *field_cache;
    HashSet *fields;
    HashSet *fields_buf;
    HashSet *def_fields;
    HashSet *all_fields;
    HashSet *tokenized_fields;
    Analyzer *analyzer;
    HashTable *ts_cache;
    Query *result;
    TokenStream *non_tokenizer;
    bool or_default : 1;
    bool wild_lower : 1;
    bool clean_str : 1;
    bool handle_parse_errors : 1;
    bool allow_any_fields : 1;
    bool close_def_fields : 1;
    bool destruct : 1;
    bool recovering : 1;
    bool use_keywords : 1;
} QParser;

extern QParser *qp_new(HashSet *all_fields, HashSet *def_fields,
                       HashSet *tokenized_fields, Analyzer *analyzer);
extern void qp_destroy(QParser *self);
extern Query *qp_parse(QParser *self, char *qstr);
extern char *qp_clean_str(char *str);

#endif
