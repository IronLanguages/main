#include "analysis.h"
#include "hash.h"
#include "libstemmer.h"
#include <string.h>
#include <ctype.h>
#include <wctype.h>
#include <wchar.h>

/****************************************************************************
 *
 * Token
 *
 ****************************************************************************/

INLINE Token *tk_set(Token *tk,
                     char *text, int tlen, off_t start, off_t end, int pos_inc)
{
    if (tlen >= MAX_WORD_SIZE) {
        tlen = MAX_WORD_SIZE - 1;
    }
    memcpy(tk->text, text, sizeof(char) * tlen);
    tk->text[tlen] = '\0';
    tk->len = tlen;
    tk->start = start;
    tk->end = end;
    tk->pos_inc = pos_inc;
    return tk;
}

INLINE Token *tk_set_ts(Token *tk,
                        char *start, char *end, char *text, int pos_inc)
{
    return tk_set(tk, start, (int)(end - start),
                  (off_t)(start - text), (off_t)(end - text), pos_inc);
}

INLINE Token *tk_set_no_len(Token *tk,
                            char *text, off_t start, off_t end, int pos_inc)
{
    return tk_set(tk, text, (int)strlen(text), start, end, pos_inc);
}

INLINE Token *w_tk_set(Token *tk, wchar_t *text, off_t start, off_t end,
                       int pos_inc)
{
    int len = wcstombs(tk->text, text, MAX_WORD_SIZE - 1);
    tk->text[len] = '\0';
    tk->len = len;
    tk->start = start;
    tk->end = end;
    tk->pos_inc = pos_inc;
    return tk;
}

int tk_eq(Token *tk1, Token *tk2)
{
    return (strcmp((char *)tk1->text, (char *)tk2->text) == 0 &&
            tk1->start == tk2->start && tk1->end == tk2->end &&
            tk1->pos_inc == tk2->pos_inc);
}

int tk_cmp(Token *tk1, Token *tk2)
{
    int cmp;
    if (tk1->start > tk2->start) {
        cmp = 1;
    }
    else if (tk1->start < tk2->start) {
        cmp = -1;
    }
    else {
        if (tk1->end > tk2->end) {
            cmp = 1;
        }
        else if (tk1->end < tk2->end) {
            cmp = -1;
        }
        else {
            cmp = strcmp((char *)tk1->text, (char *)tk2->text);
        }
    }
    return cmp;
}

void tk_destroy(void *p)
{
    free(p);
}

Token *tk_new()
{
    return ALLOC(Token);
}

/****************************************************************************
 *
 * TokenStream
 *
 ****************************************************************************/

void ts_deref(TokenStream *ts)
{
    if (--ts->ref_cnt <= 0) {
        ts->destroy_i(ts);
    }
}

static TokenStream *ts_reset(TokenStream *ts, char *text)
{
    ts->t = ts->text = text;
    return ts;
}

TokenStream *ts_clone_size(TokenStream *orig_ts, size_t size)
{
    TokenStream *ts = (TokenStream *)ecalloc(size);
    memcpy(ts, orig_ts, size);
    ts->ref_cnt = 1;
    return ts;
}

TokenStream *ts_new_i(size_t size)
{
    TokenStream *ts = ecalloc(size);

    ts->destroy_i = (void (*)(TokenStream *))&free;
    ts->reset = &ts_reset;
    ts->ref_cnt = 1;

    return ts;
}

/****************************************************************************
 * CachedTokenStream
 ****************************************************************************/

#define CTS(token_stream) ((CachedTokenStream *)(token_stream))

static TokenStream *cts_clone_i(TokenStream *orig_ts)
{
    return ts_clone_size(orig_ts, sizeof(CachedTokenStream));
}

static TokenStream *cts_new()
{
    TokenStream *ts = ts_new(CachedTokenStream);
    ts->clone_i = &cts_clone_i;
    return ts;
}

/* * Multi-byte TokenStream * */

#define MBTS(token_stream) ((MultiByteTokenStream *)(token_stream))

INLINE int mb_next_char(wchar_t *wchr, const char *s, mbstate_t *state)
{
    int num_bytes;
    if ((num_bytes = (int)mbrtowc(wchr, s, MB_CUR_MAX, state)) < 0) {
        const char *t = s;
        do {
            t++;
            ZEROSET(state, mbstate_t);
            num_bytes = (int)mbrtowc(wchr, t, MB_CUR_MAX, state);
        } while ((num_bytes < 0) && (*t != 0));
        num_bytes = t - s;
        if (*t == 0) *wchr = 0;
    }
    return num_bytes;
}

static TokenStream *mb_ts_reset(TokenStream *ts, char *text)
{
    ZEROSET(&(MBTS(ts)->state), mbstate_t);
    ts_reset(ts, text);
    return ts;
}

static TokenStream *mb_ts_clone_i(TokenStream *orig_ts)
{
    return ts_clone_size(orig_ts, sizeof(MultiByteTokenStream));
}

TokenStream *mb_ts_new()
{
    TokenStream *ts = ts_new(MultiByteTokenStream);
    ts->reset = &mb_ts_reset;
    ts->clone_i = &mb_ts_clone_i;
    ts->ref_cnt = 1;
    return ts;
}

/****************************************************************************
 *
 * Analyzer
 *
 ****************************************************************************/

void a_deref(Analyzer *a)
{
    if (--a->ref_cnt <= 0) {
        a->destroy_i(a);
    }
}

static void a_standard_destroy_i(Analyzer *a)
{
    if (a->current_ts) {
        ts_deref(a->current_ts);
    }
    free(a);
}

static TokenStream *a_standard_get_ts(Analyzer *a, char *field, char *text)
{
    TokenStream *ts;
    (void)field;
    ts = ts_clone(a->current_ts);
    return ts->reset(ts, text);
}

Analyzer *analyzer_new(TokenStream *ts,
                       void (*destroy_i)(Analyzer *a),
                       TokenStream *(*get_ts)(Analyzer *a, char *field,
                                              char *text))
{
    Analyzer *a = ALLOC(Analyzer);
    a->current_ts = ts;
    a->destroy_i = (destroy_i ? destroy_i : &a_standard_destroy_i);
    a->get_ts = (get_ts ? get_ts : &a_standard_get_ts);
    a->ref_cnt = 1;
    return a;
}

/****************************************************************************
 *
 * Non
 *
 ****************************************************************************/

/*
 * NonTokenizer
 */
static Token *nt_next(TokenStream *ts)
{
    if (ts->t) {
        size_t len = strlen(ts->t);
        ts->t = NULL;

        return tk_set(&(CTS(ts)->token), ts->text, len, 0, len, 1);
    }
    else {
        return NULL;
    }
}

TokenStream *non_tokenizer_new()
{
    TokenStream *ts = cts_new();
    ts->next = &nt_next;
    return ts;
}

/*
 * NonAnalyzer
 */
Analyzer *non_analyzer_new()
{
    return analyzer_new(non_tokenizer_new(), NULL, NULL);
}

/****************************************************************************
 *
 * Whitespace
 *
 ****************************************************************************/

/*
 * WhitespaceTokenizer
 */
static Token *wst_next(TokenStream *ts)
{
    char *t = ts->t;
    char *start;

    while (*t != '\0' && isspace(*t)) {
        t++;
    }

    if (*t == '\0') {
        return NULL;
    }

    start = t;
    while (*t != '\0' && !isspace(*t)) {
        t++;
    }

    ts->t = t;
    return tk_set_ts(&(CTS(ts)->token), start, t, ts->text, 1);
}

TokenStream *whitespace_tokenizer_new()
{
    TokenStream *ts = cts_new();
    ts->next = &wst_next;
    return ts;
}

/*
 * Multi-byte WhitespaceTokenizer
 */
static Token *mb_wst_next(TokenStream *ts)
{
    int i;
    char *start;
    char *t = ts->t;
    wchar_t wchr;
    mbstate_t *state = &(MBTS(ts)->state);

    i = mb_next_char(&wchr, t, state);
    while (wchr != 0 && iswspace(wchr)) {
        t += i;
        i = mb_next_char(&wchr, t, state);
    }
    if (wchr == 0) {
        return NULL;
    }

    start = t;
    t += i;
    i = mb_next_char(&wchr, t, state);
    while (wchr != 0 && !iswspace(wchr)) {
        t += i;
        i = mb_next_char(&wchr, t, state);
    }
    ts->t = t;
    return tk_set_ts(&(CTS(ts)->token), start, t, ts->text, 1);
}

/*
 * Lowercasing Multi-byte WhitespaceTokenizer
 */
static Token *mb_wst_next_lc(TokenStream *ts)
{
    int i;
    char *start;
    char *t = ts->t;
    wchar_t wchr;
    wchar_t wbuf[MAX_WORD_SIZE + 1], *w, *w_end;
    mbstate_t *state = &(MBTS(ts)->state);

    w = wbuf;
    w_end = &wbuf[MAX_WORD_SIZE];

    i = mb_next_char(&wchr, t, state);
    while (wchr != 0 && iswspace(wchr)) {
        t += i;
        i = mb_next_char(&wchr, t, state);
    }
    if (wchr == 0) {
        return NULL;
    }

    start = t;
    t += i;
    *w++ = towlower(wchr);
    i = mb_next_char(&wchr, t, state);
    while (wchr != 0 && !iswspace(wchr)) {
        if (w < w_end) {
            *w++ = towlower(wchr);
        }
        t += i;
        i = mb_next_char(&wchr, t, state);
    }
    *w = 0;
    ts->t = t;
    return w_tk_set(&(CTS(ts)->token), wbuf, (off_t)(start - ts->text),
                    (off_t)(t - ts->text), 1);
}

TokenStream *mb_whitespace_tokenizer_new(bool lowercase)
{
    TokenStream *ts = mb_ts_new();
    ts->next = lowercase ? &mb_wst_next_lc : &mb_wst_next;
    return ts;
}

/*
 * WhitespaceAnalyzers
 */
Analyzer *whitespace_analyzer_new(bool lowercase)
{
    TokenStream *ts;
    if (lowercase) {
        ts = lowercase_filter_new(whitespace_tokenizer_new());
    }
    else {
        ts = whitespace_tokenizer_new();
    }
    return analyzer_new(ts, NULL, NULL);
}

Analyzer *mb_whitespace_analyzer_new(bool lowercase)
{
    return analyzer_new(mb_whitespace_tokenizer_new(lowercase), NULL, NULL);
}

/****************************************************************************
 *
 * Letter
 *
 ****************************************************************************/

/*
 * LetterTokenizer
 */
Token *lt_next(TokenStream *ts)
{
    char *start;
    char *t = ts->t;

    while (*t != '\0' && !isalpha(*t)) {
        t++;
    }

    if (*t == '\0') {
        return NULL;
    }

    start = t;
    while (*t != '\0' && isalpha(*t)) {
        t++;
    }

    ts->t = t;
    return tk_set_ts(&(CTS(ts)->token), start, t, ts->text, 1);
}

TokenStream *letter_tokenizer_new()
{
    TokenStream *ts = cts_new();
    ts->next = &lt_next;
    return ts;
}

/*
 * Multi-byte LetterTokenizer
 */
Token *mb_lt_next(TokenStream *ts)
{
    int i;
    char *start;
    char *t = ts->t;
    wchar_t wchr;
    mbstate_t *state = &(MBTS(ts)->state);

    i = mb_next_char(&wchr, t, state);
    while (wchr != 0 && !iswalpha(wchr)) {
        t += i;
        i = mb_next_char(&wchr, t, state);
    }

    if (wchr == 0) {
        return NULL;
    }

    start = t;
    t += i;
    i = mb_next_char(&wchr, t, state);
    while (wchr != 0 && iswalpha(wchr)) {
        t += i;
        i = mb_next_char(&wchr, t, state);
    }
    ts->t = t;
    return tk_set_ts(&(CTS(ts)->token), start, t, ts->text, 1);
}

/*
 * Lowercasing Multi-byte LetterTokenizer
 */
Token *mb_lt_next_lc(TokenStream *ts)
{
    int i;
    char *start;
    char *t = ts->t;
    wchar_t wchr;
    wchar_t wbuf[MAX_WORD_SIZE + 1], *w, *w_end;
    mbstate_t *state = &(MBTS(ts)->state);

    w = wbuf;
    w_end = &wbuf[MAX_WORD_SIZE];

    i = mb_next_char(&wchr, t, state);
    while (wchr != 0 && !iswalpha(wchr)) {
        t += i;
        i = mb_next_char(&wchr, t, state);
    }
    if (wchr == 0) {
        return NULL;
    }

    start = t;
    t += i;
    *w++ = towlower(wchr);
    i = mb_next_char(&wchr, t, state);
    while (wchr != 0 && iswalpha(wchr)) {
        if (w < w_end) {
            *w++ = towlower(wchr);
        }
        t += i;
        i = mb_next_char(&wchr, t, state);
    }
    *w = 0;
    ts->t = t;
    return w_tk_set(&(CTS(ts)->token), wbuf, (off_t)(start - ts->text),
                    (off_t)(t - ts->text), 1);
}

TokenStream *mb_letter_tokenizer_new(bool lowercase)
{
    TokenStream *ts = mb_ts_new();
    ts->next = lowercase ? &mb_lt_next_lc : &mb_lt_next;
    return ts;
}

/*
 * LetterAnalyzers
 */
Analyzer *letter_analyzer_new(bool lowercase)
{
    TokenStream *ts;
    if (lowercase) {
        ts = lowercase_filter_new(letter_tokenizer_new());
    }
    else {
        ts = letter_tokenizer_new();
    }
    return analyzer_new(ts, NULL, NULL);
}

Analyzer *mb_letter_analyzer_new(bool lowercase)
{
    return analyzer_new(mb_letter_tokenizer_new(lowercase), NULL, NULL);
}

/****************************************************************************
 *
 * Standard
 *
 ****************************************************************************/

#define STDTS(token_stream) ((StandardTokenizer *)(token_stream))

/*
 * StandardTokenizer
 */
static int std_get_alpha(TokenStream *ts, char *token)
{
    int i = 0;
    char *t = ts->t;
    while (t[i] != '\0' && isalnum(t[i])) {
        if (i < MAX_WORD_SIZE) {
            token[i] = t[i];
        }
        i++;
    }
    return i;
}

static int mb_std_get_alpha(TokenStream *ts, char *token)
{
    char *t = ts->t;
    wchar_t wchr;
    int i;
    mbstate_t state; ZEROSET(&state, mbstate_t);

    i = mb_next_char(&wchr, t, &state);

    while (wchr != 0 && iswalnum(wchr)) {
        t += i;
        i = mb_next_char(&wchr, t, &state);
    }

    i = (int)(t - ts->t);
    if (i > MAX_WORD_SIZE) {
        i = MAX_WORD_SIZE - 1;
    }
    memcpy(token, ts->t, i);
    return i;
}

/*
static int std_get_alnum(TokenStream *ts, char *token)
{
    int i = 0;
    char *t = ts->t;
    while (t[i] != '\0' && isalnum(t[i])) {
        if (i < MAX_WORD_SIZE) {
            token[i] = t[i];
        }
        i++;
    }
    return i;
}

static int mb_std_get_alnum(TokenStream *ts, char *token)
{
    char *t = ts->t;
    wchar_t wchr;
    int i;
    mbstate_t state; ZEROSET(&state, mbstate_t);

    i = mb_next_char(&wchr, t, &state);

    while (wchr != 0 && iswalnum(wchr)) {
        t += i;
        i = mb_next_char(&wchr, t, &state);
    }

    i = (int)(t - ts->t);
    if (i > MAX_WORD_SIZE) {
        i = MAX_WORD_SIZE - 1;
    }
    memcpy(token, ts->t, i);
    return i;
}
*/

static int isnumpunc(char c)
{
    return (c == '.' || c == ',' || c == '\\' || c == '/' || c == '_'
            || c == '-');
}

static int w_isnumpunc(wchar_t c)
{
    return (c == L'.' || c == L',' || c == L'\\' || c == L'/' || c == L'_'
            || c == L'-');
}

static int isurlpunc(char c)
{
    return (c == '.' || c == '/' || c == '-' || c == '_');
}

static int isurlc(char c)
{
    return (c == '.' || c == '/' || c == '-' || c == '_' || isalnum(c));
}

static int isurlxatpunc(char c)
{
    return (c == '.' || c == '/' || c == '-' || c == '_' || c == '@');
}

static int isurlxatc(char c)
{
    return (c == '.' || c == '/' || c == '-' || c == '_' || c == '@'
            || isalnum(c));
}

static bool std_is_tok_char(char *c)
{
    if (isspace(*c)) {
        return false;           /* most common so check first. */
    }
    if (isalnum(*c) || isnumpunc(*c) || *c == '&' ||
        *c == '@' || *c == '\'' || *c == ':') {
        return true;
    }
    return false;
}

static bool mb_std_is_tok_char(char *t)
{
    wchar_t c;
    mbstate_t state; ZEROSET(&state, mbstate_t);
    
    if (((int)mbrtowc(&c, t, MB_CUR_MAX, &state)) < 0) {
        /* error which we can handle next time round. For now just return
         * false so that we can return a token */
        return false;
    }
    if (iswspace(c)) {
        return false;           /* most common so check first. */
    }
    if (iswalnum(c) || w_isnumpunc(c) || c == L'&' || c == L'@' || c == L'\''
        || c == L':') {
        return true;
    }
    return false;
}

/* (alnum)((punc)(alnum))+ where every second sequence of alnum must contain at
 * least one digit.
 * (alnum) = [a-zA-Z0-9]
 * (punc) = [_\/.,-]
 */
static int std_get_number(char *input)
{
    int i = 0;
    int count = 0;
    int last_seen_digit = 2;
    int seen_digit = false;

    while (last_seen_digit >= 0) {
        while ((input[i] != '\0') && isalnum(input[i])) {
            if ((last_seen_digit < 2) && isdigit(input[i])) {
                last_seen_digit = 2;
            }
            if ((seen_digit == false) && isdigit(input[i])) {
                seen_digit = true;
            }
            i++;
        }
        last_seen_digit--;
        if (!isnumpunc(input[i]) || !isalnum(input[i + 1])) {

            if (last_seen_digit >= 0) {
                count = i;
            }
            break;
        }
        count = i;
        i++;
    }
    if (seen_digit) {
        return count;
    }
    else {
        return 0;
    }
}

static int std_get_apostrophe(char *input)
{
    char *t = input;

    while (isalpha(*t) || *t == '\'') {
        t++;
    }

    return (int)(t - input);
}

static int mb_std_get_apostrophe(char *input)
{
    char *t = input;
    wchar_t wchr;
    int i;
    mbstate_t state; ZEROSET(&state, mbstate_t);

    i = mb_next_char(&wchr, t, &state);

    while (iswalpha(wchr) || wchr == L'\'') {
        t += i;
        i = mb_next_char(&wchr, t, &state);
    }
    return (int)(t - input);
}

static int std_get_url(char *input, char *token, int i)
{
    while (isurlc(input[i])) {
        if (isurlpunc(input[i]) && isurlpunc(input[i - 1])) {
            break; /* can't have two puncs in a row */
        }
        if (i < MAX_WORD_SIZE) {
            token[i] = input[i];
        }
        i++;
    }

    /* strip trailing puncs */
    while (isurlpunc(input[i - 1])) {
        i--;
    }

    return i;
}

/* Company names can contain '@' and '&' like AT&T and Excite@Home. Let's
*/
static int std_get_company_name(char *input)
{
    int i = 0;
    while (isalpha(input[i]) || input[i] == '@' || input[i] == '&') {
        i++;
    }

    return i;
}

/*
static int mb_std_get_company_name(char *input, TokenStream *ts)
{
    char *t = input;
    wchar_t wchr;
    int i;
    mbstate_t state; ZEROSET(&state, mbstate_t);

    i = mb_next_char(&wchr, t, &state);
    while (iswalpha(wchr) || wchr == L'@' || wchr == L'&') {
        t += i;
        i = mb_next_char(&wchr, t, &state);
    }

    return (int)(t - input);
}
*/

static bool std_advance_to_start(TokenStream *ts)
{
    char *t = ts->t;
    while (*t != '\0' && !isalnum(*t)) {
        if (isnumpunc(*t) && isdigit(t[1])) break;
        t++;
    }

    ts->t = t;

    return (*t != '\0');
}

static bool mb_std_advance_to_start(TokenStream *ts)
{
    int i;
    wchar_t wchr;
    mbstate_t state; ZEROSET(&state, mbstate_t);

    i = mb_next_char(&wchr, ts->t, &state);

    while (wchr != 0 && !iswalnum(wchr)) {
        if (isnumpunc(*ts->t) && isdigit(ts->t[1])) break;
        ts->t += i;
        i = mb_next_char(&wchr, ts->t, &state);
    }

    return (wchr != 0);
}

static Token *std_next(TokenStream *ts)
{
    StandardTokenizer *std_tz = STDTS(ts);
    char *s;
    char *t;
    char *start = NULL;
    char *num_end = NULL;
    char token[MAX_WORD_SIZE + 1];
    int token_i = 0;
    int len;
    bool is_acronym;
    bool seen_at_symbol;


    if (!std_tz->advance_to_start(ts)) {
        return NULL;
    }

    start = t = ts->t;
    token_i = std_tz->get_alpha(ts, token);
    t += token_i;

    if (!std_tz->is_tok_char(t)) {
        /* very common case, ie a plain word, so check and return */
        ts->t = t;
        return tk_set_ts(&(CTS(ts)->token), start, t, ts->text, 1);
    }

    if (*t == '\'') {       /* apostrophe case. */
        t += std_tz->get_apostrophe(t);
        ts->t = t;
        len = (int)(t - start);
        /* strip possesive */
        if ((t[-1] == 's' || t[-1] == 'S') && t[-2] == '\'') {
            t -= 2;
            tk_set_ts(&(CTS(ts)->token), start, t, ts->text, 1);
            CTS(ts)->token.end += 2;
        }
        else if (t[-1] == '\'') {
            t -= 1;
            tk_set_ts(&(CTS(ts)->token), start, t, ts->text, 1);
            CTS(ts)->token.end += 1;
        }
        else {
            tk_set_ts(&(CTS(ts)->token), start, t, ts->text, 1);
        }

        return &(CTS(ts)->token);
    }

    if (*t == '&') {        /* apostrophe case. */
        t += std_get_company_name(t);
        ts->t = t;
        return tk_set_ts(&(CTS(ts)->token), start, t, ts->text, 1);
    }

    if ((isdigit(*t) || isnumpunc(*t))       /* possibly a number */
        && (len = std_get_number(t) > 0)) {
        num_end = start + len;
        if (!std_tz->is_tok_char(num_end)) { /* won't find a longer token */
            ts->t = num_end;
            return tk_set_ts(&(CTS(ts)->token), start, num_end, ts->text, 1);
        }
        /* else there may be a longer token so check */
    }

    if (t[0] == ':' && t[1] == '/' && t[2] == '/') {
        /* check for a known url start */
        token[token_i] = '\0';
        t += 3;
        while (*t == '/') {
            t++;
        }
        if (isalpha(*t) &&
            (memcmp(token, "ftp", 3) == 0 ||
             memcmp(token, "http", 4) == 0 ||
             memcmp(token, "https", 5) == 0 ||
             memcmp(token, "file", 4) == 0)) {
            len = std_get_url(t, token, 0); /* dispose of first part of the URL */
        }
        else {              /* still treat as url but keep the first part */
            token_i = (int)(t - start);
            memcpy(token, start, token_i * sizeof(char));
            len = token_i + std_get_url(t, token, token_i); /* keep start */
        }
        ts->t = t + len;
        token[len] = 0;
        return tk_set(&(CTS(ts)->token), token, len, (off_t)(start - ts->text),
               (off_t)(ts->t - ts->text), 1);
    }

    /* now see how long a url we can find. */
    is_acronym = true;
    seen_at_symbol = false;
    while (isurlxatc(*t)) {
        if (is_acronym && !isalpha(*t) && (*t != '.')) {
            is_acronym = false;
        }
        if (isurlxatpunc(*t) && isurlxatpunc(t[-1])) {
            break; /* can't have two punctuation characters in a row */
        }
        if (*t == '@') {
            if (seen_at_symbol) {
                break; /* we can only have one @ symbol */
            }
            else {
                seen_at_symbol = true;
            }
        }
        t++;
    }
    while (isurlxatpunc(t[-1]) && t > ts->t) {
        t--;                /* strip trailing punctuation */
    }

    if (t < ts->t || (num_end != NULL && num_end < ts->t)) {
        fprintf(stderr, "Warning: encoding error. Please check that you are using the correct locale for your input");
        return NULL;
    } else if (num_end == NULL || t > num_end) {
        ts->t = t;

        if (is_acronym) {   /* check it is one letter followed by one '.' */
            for (s = start; s < t - 1; s++) {
                if (isalpha(*s) && (s[1] != '.'))
                    is_acronym = false;
            }
        }
        if (is_acronym) {   /* strip '.'s */
            for (s = start + token_i; s < t; s++) {
                if (*s != '.') {
                    token[token_i] = *s;
                    token_i++;
                }
            }
            tk_set(&(CTS(ts)->token), token, token_i,
                   (off_t)(start - ts->text),
                   (off_t)(t - ts->text), 1);
        }
        else { /* just return the url as is */
            tk_set_ts(&(CTS(ts)->token), start, t, ts->text, 1);
        }
    }
    else {                  /* return the number */
        ts->t = num_end;
        tk_set_ts(&(CTS(ts)->token), start, num_end, ts->text, 1);
    }

    return &(CTS(ts)->token);
}

static TokenStream *std_ts_clone_i(TokenStream *orig_ts)
{
    return ts_clone_size(orig_ts, sizeof(StandardTokenizer));
}

static TokenStream *std_ts_new()
{
    TokenStream *ts = ts_new(StandardTokenizer);

    ts->clone_i     = &std_ts_clone_i;
    ts->next        = &std_next;

    return ts;
}

TokenStream *standard_tokenizer_new()
{
    TokenStream *ts = std_ts_new();

    STDTS(ts)->advance_to_start = &std_advance_to_start;
    STDTS(ts)->get_alpha        = &std_get_alpha;
    STDTS(ts)->is_tok_char      = &std_is_tok_char;
    STDTS(ts)->get_apostrophe   = &std_get_apostrophe;

    return ts;
}

TokenStream *mb_standard_tokenizer_new()
{
    TokenStream *ts = std_ts_new();

    STDTS(ts)->advance_to_start = &mb_std_advance_to_start;
    STDTS(ts)->get_alpha        = &mb_std_get_alpha;
    STDTS(ts)->is_tok_char      = &mb_std_is_tok_char;
    STDTS(ts)->get_apostrophe   = &mb_std_get_apostrophe;

    return ts;
}

/****************************************************************************
 *
 * Filters
 *
 ****************************************************************************/

#define TkFilt(filter) ((TokenFilter *)(filter))

TokenStream *filter_clone_size(TokenStream *ts, size_t size)
{
    TokenStream *ts_new = ts_clone_size(ts, size);
    TkFilt(ts_new)->sub_ts = TkFilt(ts)->sub_ts->clone_i(TkFilt(ts)->sub_ts);
    return ts_new;
}

static TokenStream *filter_clone_i(TokenStream *ts)
{
    return filter_clone_size(ts, sizeof(TokenFilter));
}

static TokenStream *filter_reset(TokenStream *ts, char *text)
{
    TkFilt(ts)->sub_ts->reset(TkFilt(ts)->sub_ts, text);
    return ts;
}

static void filter_destroy_i(TokenStream *ts)
{
    ts_deref(TkFilt(ts)->sub_ts);
    free(ts);
}

#define tf_new(type, sub) tf_new_i(sizeof(type), sub)
TokenStream *tf_new_i(size_t size, TokenStream *sub_ts)
{
    TokenStream *ts     = (TokenStream *)ecalloc(size);

    TkFilt(ts)->sub_ts  = sub_ts;

    ts->clone_i         = &filter_clone_i;
    ts->destroy_i       = &filter_destroy_i;
    ts->reset           = &filter_reset;
    ts->ref_cnt         = 1;

    return ts;
}

/****************************************************************************
 * StopFilter
 ****************************************************************************/

#define StopFilt(filter) ((StopFilter *)(filter))

static void sf_destroy_i(TokenStream *ts)
{
    h_destroy(StopFilt(ts)->words);
    filter_destroy_i(ts);
}

static TokenStream *sf_clone_i(TokenStream *orig_ts)
{
    TokenStream *new_ts = filter_clone_size(orig_ts, sizeof(MappingFilter));
    REF(StopFilt(new_ts)->words);
    return new_ts;
}

static Token *sf_next(TokenStream *ts)
{
    int pos_inc = 0;
    HashTable *words = StopFilt(ts)->words;
    TokenFilter *tf = TkFilt(ts);
    Token *tk = tf->sub_ts->next(tf->sub_ts);

    while ((tk != NULL) && (h_get(words, tk->text) != NULL)) {
        pos_inc += tk->pos_inc;
        tk = tf->sub_ts->next(tf->sub_ts);
    }

    if (tk != NULL) {
        tk->pos_inc += pos_inc;
    }

    return tk;
}

TokenStream *stop_filter_new_with_words_len(TokenStream *sub_ts,
                                            const char **words, int len)
{
    int i;
    char *word;
    HashTable *word_table = h_new_str(&free, (free_ft) NULL);
    TokenStream *ts = tf_new(StopFilter, sub_ts);

    for (i = 0; i < len; i++) {
        word = estrdup(words[i]);
        h_set(word_table, word, word);
    }
    StopFilt(ts)->words = word_table;
    ts->next            = &sf_next;
    ts->destroy_i       = &sf_destroy_i;
    ts->clone_i         = &sf_clone_i;
    return ts;
}

TokenStream *stop_filter_new_with_words(TokenStream *sub_ts,
                                        const char **words)
{
    char *word;
    HashTable *word_table = h_new_str(&free, (free_ft) NULL);
    TokenStream *ts = tf_new(StopFilter, sub_ts);

    while (*words) {
        word = estrdup(*words);
        h_set(word_table, word, word);
        words++;
    }

    StopFilt(ts)->words = word_table;
    ts->next            = &sf_next;
    ts->destroy_i       = &sf_destroy_i;
    ts->clone_i         = &sf_clone_i;
    return ts;
}

TokenStream *stop_filter_new(TokenStream *ts)
{
    return stop_filter_new_with_words(ts, FULL_ENGLISH_STOP_WORDS);
}

/****************************************************************************
 * MappingFilter
 ****************************************************************************/

#define MFilt(filter) ((MappingFilter *)(filter))

static void mf_destroy_i(TokenStream *ts)
{
    mulmap_destroy(MFilt(ts)->mapper);
    filter_destroy_i(ts);
}

static TokenStream *mf_clone_i(TokenStream *orig_ts)
{
    TokenStream *new_ts = filter_clone_size(orig_ts, sizeof(MappingFilter));
    REF(MFilt(new_ts)->mapper);
    return new_ts;
}

static Token *mf_next(TokenStream *ts)
{
    char buf[MAX_WORD_SIZE + 1];
    MultiMapper *mapper = MFilt(ts)->mapper;
    TokenFilter *tf = TkFilt(ts);
    Token *tk = tf->sub_ts->next(tf->sub_ts);
    if (tk != NULL) {
        tk->len = mulmap_map_len(mapper, buf, tk->text, MAX_WORD_SIZE);
        memcpy(tk->text, buf, tk->len + 1);
    }
    return tk;
}

static TokenStream *mf_reset(TokenStream *ts, char *text)
{
    MultiMapper *mm = MFilt(ts)->mapper;
    if (mm->d_size == 0) {
        mulmap_compile(MFilt(ts)->mapper);
    }
    filter_reset(ts, text);
    return ts;
}

TokenStream *mapping_filter_new(TokenStream *sub_ts)
{
    TokenStream *ts   = tf_new(MappingFilter, sub_ts);
    MFilt(ts)->mapper = mulmap_new();
    ts->next          = &mf_next;
    ts->destroy_i     = &mf_destroy_i;
    ts->clone_i       = &mf_clone_i;
    ts->reset         = &mf_reset;
    return ts;
}

TokenStream *mapping_filter_add(TokenStream *ts, const char *pattern,
                                const char *replacement)
{
    mulmap_add_mapping(MFilt(ts)->mapper, pattern, replacement);
    return ts;
}

/****************************************************************************
 * HyphenFilter
 ****************************************************************************/

#define HyphenFilt(filter) ((HyphenFilter *)(filter))

static TokenStream *hf_clone_i(TokenStream *orig_ts)
{
    TokenStream *new_ts = filter_clone_size(orig_ts, sizeof(HyphenFilter));
    return new_ts;
}

static Token *hf_next(TokenStream *ts)
{
    HyphenFilter *hf = HyphenFilt(ts);
    TokenFilter *tf = TkFilt(ts);
    Token *tk = hf->tk;
    
    if (hf->pos < hf->len) {
        const int pos = hf->pos;
        const int text_len = strlen(hf->text + pos);
        strcpy(tk->text, hf->text + pos);
        tk->pos_inc = ((pos != 0) ? 1 : 0);
        tk->start = hf->start + pos;
        tk->end = tk->start + text_len;
        hf->pos += text_len + 1;
        tk->len = text_len;
        return tk;
    }
    else {
        char *p;
        bool seen_hyphen = false;
        bool seen_other_punc = false;
        hf->tk = tk = tf->sub_ts->next(tf->sub_ts);
        if (NULL == tk) return NULL;
        p = tk->text + 1;
        while (*p) {
            if (*p == '-') {
                seen_hyphen = true;
            }
            else if (!isalpha(*p)) {
                seen_other_punc = true;
                break;
            }
            p++;
        }
        if (seen_hyphen && !seen_other_punc) {
            char *q = hf->text;
            char *r = tk->text;
            p = tk->text;
            while (*p) {
                if (*p == '-') {
                    *q = '\0';
                }
                else {
                    *r = *q = *p;
                    r++;
                }
                q++;
                p++;
            }
            *r = *q = '\0';
            hf->start = tk->start;
            hf->pos = 0;
            hf->len = q - hf->text;
            tk->len = r - tk->text;
        }
    }
    return tk;
}

TokenStream *hyphen_filter_new(TokenStream *sub_ts)
{
    TokenStream *ts = tf_new(HyphenFilter, sub_ts);
    ts->next        = &hf_next;
    ts->clone_i     = &hf_clone_i;
    return ts;
}

/****************************************************************************
 * LowerCaseFilter
 ****************************************************************************/


Token *mb_lcf_next(TokenStream *ts)
{
    wchar_t wbuf[MAX_WORD_SIZE + 1], *wchr;
    Token *tk = TkFilt(ts)->sub_ts->next(TkFilt(ts)->sub_ts);
    int x;
    wbuf[MAX_WORD_SIZE] = 0;

    if (tk == NULL) {
        return tk;
    }

    if ((x=mbstowcs(wbuf, tk->text, MAX_WORD_SIZE)) <= 0) return tk;
    wchr = wbuf;
    while (*wchr != 0) {
        *wchr = towlower(*wchr);
        wchr++;
    }
    tk->len = wcstombs(tk->text, wbuf, MAX_WORD_SIZE);
    if (tk->len <= 0) {
        strcpy(tk->text, "BAD_DATA");
        tk->len = 8;
    }
    tk->text[tk->len] = '\0';
    return tk;
}

TokenStream *mb_lowercase_filter_new(TokenStream *sub_ts)
{
    TokenStream *ts = tf_new(TokenFilter, sub_ts);
    ts->next = &mb_lcf_next;
    return ts;
}

Token *lcf_next(TokenStream *ts)
{
    int i = 0;
    Token *tk = TkFilt(ts)->sub_ts->next(TkFilt(ts)->sub_ts);
    if (tk == NULL) {
        return tk;
    }
    while (tk->text[i] != '\0') {
        tk->text[i] = tolower(tk->text[i]);
        i++;
    }
    return tk;
}

TokenStream *lowercase_filter_new(TokenStream *sub_ts)
{
    TokenStream *ts = tf_new(TokenFilter, sub_ts);
    ts->next = &lcf_next;
    return ts;
}

/****************************************************************************
 * StemFilter
 ****************************************************************************/

#define StemFilt(filter) ((StemFilter *)(filter))

void stemf_destroy_i(TokenStream *ts)
{
    sb_stemmer_delete(StemFilt(ts)->stemmer);
    free(StemFilt(ts)->algorithm);
    free(StemFilt(ts)->charenc);
    filter_destroy_i(ts);
}

Token *stemf_next(TokenStream *ts)
{
    int len;
    const sb_symbol *stemmed;
    struct sb_stemmer *stemmer = StemFilt(ts)->stemmer;
    TokenFilter *tf = TkFilt(ts);
    Token *tk = tf->sub_ts->next(tf->sub_ts);
    if (tk == NULL) {
        return tk;
    }
    stemmed = sb_stemmer_stem(stemmer, (sb_symbol *)tk->text, tk->len);
    len = sb_stemmer_length(stemmer);
    if (len >= MAX_WORD_SIZE) {
        len = MAX_WORD_SIZE - 1;
    }

    memcpy(tk->text, stemmed, len);
    tk->text[len] = '\0';
    tk->len = len;
    return tk;
}

TokenStream *stemf_clone_i(TokenStream *orig_ts)
{
    TokenStream *new_ts      = filter_clone_size(orig_ts, sizeof(StemFilter));
    StemFilter *stemf        = StemFilt(new_ts);
    StemFilter *orig_stemf   = StemFilt(orig_ts);
    stemf->stemmer =
        sb_stemmer_new(orig_stemf->algorithm, orig_stemf->charenc);
    stemf->algorithm =
        orig_stemf->algorithm ? estrdup(orig_stemf->algorithm) : NULL;
    stemf->charenc =
        orig_stemf->charenc ? estrdup(orig_stemf->charenc) : NULL;
    return new_ts;
}

TokenStream *stem_filter_new(TokenStream *ts, const char *algorithm,
                             const char *charenc)
{
    TokenStream *tf = tf_new(StemFilter, ts);

    StemFilt(tf)->stemmer   = sb_stemmer_new(algorithm, charenc);
    StemFilt(tf)->algorithm = algorithm ? estrdup(algorithm) : NULL;
    StemFilt(tf)->charenc   = charenc ? estrdup(charenc) : NULL;

    tf->next = &stemf_next;
    tf->destroy_i = &stemf_destroy_i;
    tf->clone_i = &stemf_clone_i;
    return tf;
}

/****************************************************************************
 *
 * Analyzers
 *
 ****************************************************************************/

/****************************************************************************
 * Standard
 ****************************************************************************/

Analyzer *standard_analyzer_new_with_words_len(const char **words, int len,
                                               bool lowercase)
{
    TokenStream *ts = standard_tokenizer_new();
    if (lowercase) {
        ts = lowercase_filter_new(ts);
    }
    ts = hyphen_filter_new(stop_filter_new_with_words_len(ts, words, len));
    return analyzer_new(ts, NULL, NULL);
}

Analyzer *standard_analyzer_new_with_words(const char **words,
                                           bool lowercase)
{
    TokenStream *ts = standard_tokenizer_new();
    if (lowercase) {
        ts = lowercase_filter_new(ts);
    }
    ts = hyphen_filter_new(stop_filter_new_with_words(ts, words));
    return analyzer_new(ts, NULL, NULL);
}

Analyzer *mb_standard_analyzer_new_with_words_len(const char **words,
                                                  int len, bool lowercase)
{
    TokenStream *ts = mb_standard_tokenizer_new();
    if (lowercase) {
        ts = mb_lowercase_filter_new(ts);
    }
    ts = hyphen_filter_new(stop_filter_new_with_words_len(ts, words, len));
    return analyzer_new(ts, NULL, NULL);
}

Analyzer *mb_standard_analyzer_new_with_words(const char **words,
                                              bool lowercase)
{
    TokenStream *ts = mb_standard_tokenizer_new();
    if (lowercase) {
        ts = mb_lowercase_filter_new(ts);
    }
    ts = hyphen_filter_new(stop_filter_new_with_words(ts, words));
    return analyzer_new(ts, NULL, NULL);
}

Analyzer *standard_analyzer_new(bool lowercase)
{
    return standard_analyzer_new_with_words(FULL_ENGLISH_STOP_WORDS,
                                            lowercase);
}

Analyzer *mb_standard_analyzer_new(bool lowercase)
{
    return mb_standard_analyzer_new_with_words(FULL_ENGLISH_STOP_WORDS,
                                               lowercase);
}

/****************************************************************************
 *
 * PerFieldAnalyzer
 *
 ****************************************************************************/

#define PFA(analyzer) ((PerFieldAnalyzer *)(analyzer))
void pfa_destroy_i(Analyzer *self)
{
    h_destroy(PFA(self)->dict);

    a_deref(PFA(self)->default_a);
    free(self);
}

TokenStream *pfa_get_ts(Analyzer *self, char *field, char *text)
{
    Analyzer *a = h_get(PFA(self)->dict, field);
    if (a == NULL) {
        a = PFA(self)->default_a;
    }
    return a_get_ts(a, field, text);
}

void pfa_sub_a_destroy_i(void *p)
{
    Analyzer *a = (Analyzer *) p;
    a_deref(a);
}

void pfa_add_field(Analyzer *self, char *field, Analyzer *analyzer)
{
    h_set(PFA(self)->dict, estrdup(field), analyzer);
}

Analyzer *per_field_analyzer_new(Analyzer *default_a)
{
    Analyzer *a = (Analyzer *)ecalloc(sizeof(PerFieldAnalyzer));

    PFA(a)->default_a = default_a;
    PFA(a)->dict = h_new_str(&free, &pfa_sub_a_destroy_i);

    a->destroy_i = &pfa_destroy_i;
    a->get_ts    = pfa_get_ts;
    a->ref_cnt   = 1;
    
    return a;
}

#ifdef ALONE
int main(int argc, char **argv)
{
    char buf[10000];
    Analyzer *a = standard_analyzer_new(true);
    TokenStream *ts;
    Token *tk;
    while (fgets(buf, 9999, stdin) != NULL) {
        ts = a_get_ts(a, "hello", buf);
        while ((tk = ts->next(ts)) != NULL) {
            printf("<%s:%ld:%ld> ", tk->text, tk->start, tk->end);
        }
        printf("\n");
        ts_deref(ts);
    }
    return 0;
}
#endif
