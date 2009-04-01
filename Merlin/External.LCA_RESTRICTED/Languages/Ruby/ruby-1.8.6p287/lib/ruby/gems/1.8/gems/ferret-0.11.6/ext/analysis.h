#ifndef FRT_ANALYSIS_H
#define FRT_ANALYSIS_H

#include "global.h"
#include "hash.h"
#include "multimapper.h"
#include <wchar.h>

/****************************************************************************
 *
 * Token
 *
 ****************************************************************************/

typedef struct Token
{
    char text[MAX_WORD_SIZE];
    int len;
    off_t start;
    off_t end;
    int pos_inc;
} Token;

extern Token *tk_new();
extern void tk_destroy(void *p);
extern Token *tk_set(Token *tk, char *text, int tlen, off_t start, off_t end,
                     int pos_inc);
extern Token *tk_set_no_len(Token *tk, char *text, off_t start, off_t end,
                            int pos_inc);
extern int tk_eq(Token *tk1, Token *tk2);
extern int tk_cmp(Token *tk1, Token *tk2);

/****************************************************************************
 *
 * TokenStream
 *
 ****************************************************************************/


typedef struct TokenStream TokenStream;
struct TokenStream
{
    char        *t;             /* ptr used to scan text */
    char        *text;
    Token       *(*next)(TokenStream *ts);
    TokenStream *(*reset)(TokenStream *ts, char *text);
    TokenStream *(*clone_i)(TokenStream *ts);
    void         (*destroy_i)(TokenStream *ts);
    int          ref_cnt;
};

#define ts_new(type) ts_new_i(sizeof(type))
extern TokenStream *ts_new_i(size_t size);
extern TokenStream *ts_clone_size(TokenStream *orig_ts, size_t size);

typedef struct CachedTokenStream
{
    TokenStream super;
    Token       token;
} CachedTokenStream;

typedef struct MultiByteTokenStream
{
    CachedTokenStream super;
    mbstate_t         state;
} MultiByteTokenStream;

typedef struct StandardTokenizer
{
    CachedTokenStream super;
    bool        (*advance_to_start)(TokenStream *ts);
    bool        (*is_tok_char)(char *c);
    int         (*get_alpha)(TokenStream *ts, char *token);
    int         (*get_apostrophe)(char *input);
} StandardTokenizer;

typedef struct TokenFilter
{
    TokenStream super;
    TokenStream *sub_ts;
} TokenFilter;

extern TokenStream *filter_clone_size(TokenStream *ts, size_t size);
#define tf_new(type, sub) tf_new_i(sizeof(type), sub)
extern TokenStream *tf_new_i(size_t size, TokenStream *sub_ts);

typedef struct StopFilter 
{
    TokenFilter super;
    HashTable  *words;
} StopFilter;

typedef struct MappingFilter 
{
    TokenFilter  super;
    MultiMapper *mapper;
} MappingFilter;

typedef struct HyphenFilter 
{
    TokenFilter super;
    char text[MAX_WORD_SIZE];
    int start;
    int pos;
    int len;
    Token *tk;
} HyphenFilter;

typedef struct StemFilter
{
    TokenFilter        super;
    struct sb_stemmer  *stemmer;
    char               *algorithm;
    char               *charenc;
} StemFilter;

#define ts_next(mts) mts->next(mts)
#define ts_clone(mts) mts->clone_i(mts)

extern void ts_deref(TokenStream *ts);

extern TokenStream *non_tokenizer_new();

extern TokenStream *whitespace_tokenizer_new();
extern TokenStream *mb_whitespace_tokenizer_new(bool lowercase);

extern TokenStream *letter_tokenizer_new();
extern TokenStream *mb_letter_tokenizer_new(bool lowercase);

extern TokenStream *standard_tokenizer_new();
extern TokenStream *mb_standard_tokenizer_new();

extern TokenStream *hyphen_filter_new(TokenStream *ts);
extern TokenStream *lowercase_filter_new(TokenStream *ts);
extern TokenStream *mb_lowercase_filter_new(TokenStream *ts);

extern const char *ENGLISH_STOP_WORDS[];
extern const char *FULL_ENGLISH_STOP_WORDS[];
extern const char *EXTENDED_ENGLISH_STOP_WORDS[];
extern const char *FULL_FRENCH_STOP_WORDS[];
extern const char *FULL_SPANISH_STOP_WORDS[];
extern const char *FULL_PORTUGUESE_STOP_WORDS[];
extern const char *FULL_ITALIAN_STOP_WORDS[];
extern const char *FULL_GERMAN_STOP_WORDS[];
extern const char *FULL_DUTCH_STOP_WORDS[];
extern const char *FULL_SWEDISH_STOP_WORDS[];
extern const char *FULL_NORWEGIAN_STOP_WORDS[];
extern const char *FULL_DANISH_STOP_WORDS[];
extern const char *FULL_RUSSIAN_STOP_WORDS[];
extern const char *FULL_FINNISH_STOP_WORDS[];

extern TokenStream *stop_filter_new_with_words_len(TokenStream *ts,
                                                   const char **words, int len);
extern TokenStream *stop_filter_new_with_words(TokenStream *ts,
                                               const char **words);
extern TokenStream *stop_filter_new(TokenStream *ts);
extern TokenStream *stem_filter_new(TokenStream *ts, const char *algorithm,
                                    const char *charenc);

extern TokenStream *mapping_filter_new(TokenStream *ts);
extern TokenStream *mapping_filter_add(TokenStream *ts, const char *pattern,
                                       const char *replacement);

/****************************************************************************
 *
 * Analyzer
 *
 ****************************************************************************/

typedef struct Analyzer
{
    TokenStream *current_ts;
    TokenStream *(*get_ts)(struct Analyzer *a, char *field, char *text);
    void (*destroy_i)(struct Analyzer *a);
    int ref_cnt;
} Analyzer;

extern void a_deref(Analyzer *a);

#define a_get_ts(ma, field, text) ma->get_ts(ma, field, text)

extern Analyzer *analyzer_new(TokenStream *ts,
                              void (*destroy)(Analyzer *a),
                              TokenStream *(*get_ts)(Analyzer *a,
                                                     char *field,
                                                     char *text));
extern void a_standard_destroy(Analyzer *a);
extern Analyzer *non_analyzer_new();

extern Analyzer *whitespace_analyzer_new(bool lowercase);
extern Analyzer *mb_whitespace_analyzer_new(bool lowercase);

extern Analyzer *letter_analyzer_new(bool lowercase);
extern Analyzer *mb_letter_analyzer_new(bool lowercase);

extern Analyzer *standard_analyzer_new(bool lowercase);
extern Analyzer *mb_standard_analyzer_new(bool lowercase);

extern Analyzer *standard_analyzer_new_with_words(const char **words,
                                                  bool lowercase);
extern Analyzer *standard_analyzer_new_with_words_len(const char **words, int len,
                                                      bool lowercase);
extern Analyzer *mb_standard_analyzer_new_with_words(const char **words,
                                                     bool lowercase);
extern Analyzer *mb_standard_analyzer_new_with_words_len(const char **words,
                                                  int len, bool lowercase);

#define PFA(analyzer) ((PerFieldAnalyzer *)(analyzer))
typedef struct PerFieldAnalyzer
{
    Analyzer    super;
    HashTable  *dict;
    Analyzer   *default_a;
} PerFieldAnalyzer;

extern Analyzer *per_field_analyzer_new(Analyzer *a);
extern void pfa_add_field(Analyzer *self, char *field, Analyzer *analyzer);

#endif
