#ifndef FRT_SIMILARITY_H
#define FRT_SIMILARITY_H

typedef struct Searcher Searcher;

/****************************************************************************
 *
 * Term
 *
 ****************************************************************************/

#define term_set_new() \
    hs_new((hash_ft)&term_hash, (eq_ft)&term_eq, (free_ft)&term_destroy)

typedef struct Term
{
    char *field;
    char *text;
} Term;

extern Term *term_new(const char *field, const char *text);
extern void term_destroy(Term *self);
extern int term_eq(const void *t1, const void *t2);
extern unsigned long term_hash(const void *t);

/***************************************************************************
 *
 * PhrasePosition
 *
 ***************************************************************************/

typedef struct PhrasePosition
{
    int pos;
    char **terms;
} PhrasePosition;

/***************************************************************************
 *
 * Similarity
 *
 ***************************************************************************/

typedef struct Similarity Similarity;

struct Similarity
{
    void *data;
    float norm_table[256];
    float (*length_norm)(Similarity *self, const char *field, int num_terms);
    float (*query_norm)(Similarity *self, float sum_of_squared_weights);
    float (*tf)(Similarity *self, float freq);
    float (*sloppy_freq)(Similarity *self, int distance);
    float (*idf_term)(Similarity *self, const char *field, char *term,
                      Searcher *searcher);
    float (*idf_phrase)(Similarity *self, const char *field,
                        PhrasePosition *positions,
                        int pp_cnt, Searcher *searcher);
    float (*idf)(Similarity *self, int doc_freq, int num_docs);
    float (*coord)(Similarity *self, int overlap, int max_overlap);
    float (*decode_norm)(Similarity *self, unsigned char b);
    unsigned char (*encode_norm)(Similarity *self, float f);
    void  (*destroy)(Similarity *self);
};

#define sim_length_norm(msim, field, num_terms) msim->length_norm(msim, field, num_terms)
#define sim_query_norm(msim, sosw) msim->query_norm(msim, sosw)
#define sim_tf(msim, freq) msim->tf(msim, freq)
#define sim_sloppy_freq(msim, distance) msim->sloppy_freq(msim, distance)
#define sim_idf_term(msim, field, term, searcher)\
    msim->idf_term(msim, field, term, searcher)
#define sim_idf_phrase(msim, field, positions, pos_cnt, searcher)\
    msim->idf_phrase(msim, field, positions, pos_cnt, searcher)
#define sim_idf(msim, doc_freq, num_docs) msim->idf(msim, doc_freq, num_docs)
#define sim_coord(msim, overlap, max_overlap) msim->coord(msim, overlap, max_overlap)
#define sim_decode_norm(msim, b) msim->decode_norm(msim, b)
#define sim_encode_norm(msim, f) msim->encode_norm(msim, f)
#define sim_destroy(msim) msim->destroy(msim)

Similarity *sim_create_default();

#endif
