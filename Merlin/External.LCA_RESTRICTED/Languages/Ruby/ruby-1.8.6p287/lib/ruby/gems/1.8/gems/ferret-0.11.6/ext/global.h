#ifndef FRT_GLOBAL_H
#define FRT_GLOBAL_H

#include "config.h"
#include "except.h"
#include "lang.h"
#include <stdlib.h>
#include <stdio.h>
#include <stdarg.h>

#define MAX_WORD_SIZE 255
#define MAX_FILE_PATH 1024

#if defined(__GNUC__)
# define INLINE __inline__
#else
# define INLINE
#endif

typedef void (*free_ft)(void *key);

#define NELEMS(array) ((int)(sizeof(array)/sizeof(array[0])))


#define ZEROSET(ptr, type) memset(ptr, 0, sizeof(type))
#define ZEROSET_N(ptr, type, n) memset(ptr, 0, sizeof(type)*(n))

/*
#define ALLOC_AND_ZERO(type) (type*)memset(emalloc(sizeof(type)), 0, sizeof(type))
#define ALLOC_AND_ZERO_N(type,n) (type*)memset(emalloc(sizeof(type)*(n)), 0, sizeof(type)*(n))
*/
#define ALLOC_AND_ZERO(type) (type*)frt_calloc(sizeof(type))
#define ALLOC_AND_ZERO_N(type,n) (type*)frt_calloc(sizeof(type)*(n))

#define REF(a) (a)->ref_cnt++
#define DEREF(a) (a)->ref_cnt--

#define NEXT_NUM(index, size) (((index) + 1) % (size))
#define PREV_NUM(index, size) (((index) + (size) - 1) % (size))

#define MIN(a, b) ((a) < (b) ? (a) : (b))
#define MAX(a, b) ((a) > (b) ? (a) : (b))

#define MIN3(a, b, c) ((a) < (b) ? ((a) < (c) ? (a) : (c)) : ((b) < (c) ? (b) : (c)))
#define MAX3(a, b, c) ((a) > (b) ? ((a) > (c) ? (a) : (c)) : ((b) > (c) ? (b) : (c)))

#define RECAPA(self, len, capa, ptr, type) \
  do {\
    if (self->len >= self->capa) {\
      if (self->capa > 0) {\
        self->capa <<= 1;\
      } else {\
        self->capa = 4;\
      }\
      REALLOC_N(self->ptr, type, self->capa);\
    }\
  } while (0)

#ifdef POSH_OS_WIN32
# define Jx fprintf(stderr,"%s, %d\n", __FILE__, __LINE__);
# define Xj fprintf(stdout,"%s, %d\n", __FILE__, __LINE__);
#else
# define Jx fprintf(stderr,"%s, %d: %s\n", __FILE__, __LINE__, __func__);
# define Xj fprintf(stdout,"%s, %d: %s\n", __FILE__, __LINE__, __func__);
#endif

extern char *progname();
extern void setprogname(const char *str);

extern unsigned int *imalloc(unsigned int value);
extern unsigned long *lmalloc(unsigned long value);
extern f_u32 *u32malloc(f_u32 value);
extern f_u64 *u64malloc(f_u64 value);

extern void *emalloc(size_t n);
extern void *ecalloc(size_t n);
extern void *erealloc(void *ptr, size_t n);
extern char *estrdup(const char *s);
extern char *estrcat(char *str, char *str_cat);

extern const char *EMPTY_STRING;

extern int scmp(const void *p1, const void *p2);
extern int icmp(const void *p1, const void *p2);
extern int icmp_risky(const void *p1, const void *p2);

extern int min2(int a, int b);
extern int min3(int a, int b, int c);
extern int max2(int a, int b);
extern int max3(int a, int b, int c);

extern char *dbl_to_s(char *buf, double num);
extern char *strfmt(const char *fmt, ...);
extern char *vstrfmt(const char *fmt, va_list args);

extern void micro_sleep(const int micro_seconds);

extern void register_for_cleanup(void *p, free_ft free_func);
extern void do_clean_up();

/**
 * A dummy function which can be passed to functions which expect a free
 * function such as h_new() if you don't want the free functions to do anything.
 * This function will do nothing.
 *
 * @param p the object which this function will be called on.
 */
extern void dummy_free(void *p);

#ifdef DEBUG
extern bool x_do_logging;
#define xlog if (x_do_logging) printf
#else
#define xlog()
#endif

#endif
