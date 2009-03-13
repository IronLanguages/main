#include "global.h"
#include <stdarg.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <assert.h>
#include <math.h>
#include <ctype.h>

const char *EMPTY_STRING = "";

bool x_do_logging = false;

INLINE int min3(int a, int b, int c)
{
    return MIN3(a, b, c);
}

INLINE int min2(int a, int b)
{
    return MIN(a, b);
}

INLINE int max3(int a, int b, int c)
{
    return MAX3(a, b, c);
}

INLINE int max2(int a, int b)
{
    return MAX(a, b);
}

int scmp(const void *p1, const void *p2)
{
    return strcmp(*(char **) p1, *(char **) p2);
}

int icmp(const void *p1, const void *p2)
{
    int i1 = *(int *) p1;
    int i2 = *(int *) p2;

    if (i1 > i2) {
        return 1;
    }
    else if (i1 < i2) {
        return -1;
    }
    return 0;
}

int icmp_risky(const void *p1, const void *p2)
{
  return (*(int *)p1) - *((int *)p2);
}

unsigned int *imalloc(unsigned int value)
{
  unsigned int *p = ALLOC(unsigned int);
  *p = value;
  return p;
}

unsigned long *lmalloc(unsigned long value)
{
  unsigned long *p = ALLOC(unsigned long);
  *p = value;
  return p;
}

f_u32 *u32malloc(f_u32 value)
{
  f_u32 *p = ALLOC(f_u32);
  *p = value;
  return p;
}

f_u64 *u64malloc(f_u64 value)
{
  f_u64 *p = ALLOC(f_u64);
  *p = value;
  return p;
}


#ifndef RUBY_BINDINGS
/* frt_exit: print error message and exit */
# ifdef FRT_HAS_VARARGS
void vfrt_exit(const char *file, int line_num, const char *func,
               const char *err_type, const char *fmt, va_list args)
# else
void V_FRT_EXIT(const char *err_type, const char *fmt, va_list args)
# endif
{
    fflush(stdout);
    fprintf(stderr, "\n");
    if (progname() != NULL) {
        fprintf(stderr, "%s: ", progname());
    }

# ifdef FRT_HAS_VARARGS
    fprintf(stderr, "%s occured at <%s>:%d in %s\n",
            err_type, file, line_num, func);
# else
    fprintf(stderr, "%s occured:\n", err_type);
# endif
    vfprintf(stderr, fmt, args);

    if (fmt[0] != '\0' && fmt[strlen(fmt) - 1] == ':') {
        fprintf(stderr, " %s", strerror(errno));
    }

    fprintf(stderr, "\n");
    exit(2);                    /* conventional value for failed execution */
}


# ifdef FRT_HAS_VARARGS
void frt_exit(const char *file, int line_num, const char *func,
              const char *err_type, const char *fmt, ...)
# else
void FRT_EXIT(const char *err_type, const char *fmt, ...)
# endif
{
    va_list args;
    va_start(args, fmt);
# ifdef FRT_HAS_VARARGS
    vfrt_exit(file, line_num, func, err_type, fmt, args);
# else
    V_FRT_EXIT(err_type, fmt, args);
# endif
    va_end(args);
}
#endif


/* weprintf: print error message and don't exit */
void weprintf(const char *fmt, ...)
{
    va_list args;

    fflush(stdout);
    if (progname() != NULL)
        fprintf(stderr, "%s: ", progname());

    va_start(args, fmt);
    vfprintf(stderr, fmt, args);
    va_end(args);

    if (fmt[0] != '\0' && fmt[strlen(fmt) - 1] == ':')
        fprintf(stderr, " %s", strerror(errno));
    fprintf(stderr, "\n");
}

#define MAX_PROG_NAME 200
static char name[MAX_PROG_NAME]; /* program name for error msgs */

/* setprogname: set stored name of program */
void setprogname(const char *str)
{
    strncpy(name, str, MAX_PROG_NAME - 1);
}

char *progname()
{
    return name;
}

/* concatenate two strings freeing the second */
char *estrcat(char *str1, char *str2)
{
    size_t len1 = strlen(str1);
    size_t len2 = strlen(str2);
    REALLOC_N(str1, char, len1 + len2 + 3);     /* leave room for <CR> */
    memcpy(str1 + len1, str2, len2 + 1);        /* make sure '\0' copied too */
    free(str2);
    return str1;
}

/* epstrdup: duplicate a string with a format, report if error */
char *epstrdup(const char *fmt, int len, ...)
{
    char *string;
    va_list args;
    len += (int) strlen(fmt);

    string = ALLOC_N(char, len + 1);
    va_start(args, len);
    vsprintf(string, fmt, args);
    va_end(args);

    return string;
}

/* estrdup: duplicate a string, report if error */
char *estrdup(const char *s)
{
    char *t = (char *)malloc(strlen(s) + 1);

    if (t == NULL) {
        RAISE(MEM_ERROR, "failed to allocate %d bytes", (int)strlen(s) + 1);
    }

    strcpy(t, s);
    return t;
}

/* emalloc: malloc and report if error */
void *emalloc(size_t size)
{
    void *p = malloc(size);

    if (p == NULL) {
        RAISE(MEM_ERROR, "failed to allocate %d bytes", (int)size);
    }

    return p;
}

/* ecalloc: malloc, zeroset and report if error */
void *ecalloc(size_t size)
{
    void *p = calloc(1, size);

    if (p == NULL) {
        RAISE(MEM_ERROR, "failed to allocate %d bytes", (int)size);
    }

    return p;
}

/* erealloc: realloc and report if error */
void *erealloc(void *ptr, size_t size)
{
    void *p = realloc(ptr, size);

    if (p == NULL) {
        RAISE(MEM_ERROR, "failed to reallocate %d bytes", (int)size);
    }

    return p;
}

/* Pretty print a float to the buffer. The buffer should have at least 32
 * bytes available.
 */
char *dbl_to_s(char *buf, double num)
{
    char *p, *e;

#ifdef FRT_IS_C99
    if (isinf(num)) {
        return estrdup(num < 0 ? "-Infinity" : "Infinity");
    }
    else if (isnan(num)) {
        return estrdup("NaN");
    }
#endif

    sprintf(buf, "%#.7g", num);
    if (!(e = strchr(buf, 'e'))) {
        e = buf + strlen(buf);
    }
    if (!isdigit(e[-1])) {
        /* reformat if ended with decimal point (ex 111111111111111.) */
        sprintf(buf, "%#.6e", num);
        if (!(e = strchr(buf, 'e'))) {
            e = buf + strlen(buf);
        }
    }
    p = e;
    while (p[-1] == '0' && isdigit(p[-2])) {
        p--;
    }

    memmove(p, e, strlen(e) + 1);
    return buf;
}

/* strfmt: like sprintf except that it allocates memory for the string */
char *vstrfmt(const char *fmt, va_list args)
{
    char *string;
    char *p = (char *) fmt, *q;
    int len = (int) strlen(fmt) + 1;
    int slen;
    char *s;
    long i;
    double d;

    q = string = ALLOC_N(char, len);

    while (*p) {
        if (*p == '%') {
            p++;
            switch (*p) {
            case 's':
                p++;
                s = va_arg(args, char *);
                if (s) {
                    slen = (int) strlen(s);
                    len += slen;
                    *q = 0;
                    REALLOC_N(string, char, len);
                    q = string + strlen(string);
                    sprintf(q, s);
                    q += slen;
                }
                continue;
            case 'f':
                p++;
                len += 32;
                *q = 0;
                REALLOC_N(string, char, len);
                q = string + strlen(string);
                d = va_arg(args, double);
                dbl_to_s(q, d);
                q += strlen(q);
                continue;
            case 'd':
                p++;
                len += 20;
                *q = 0;
                REALLOC_N(string, char, len);
                q = string + strlen(string);
                i = va_arg(args, long);
                sprintf(q, "%ld", i);
                q += strlen(q);
                continue;
            default:
                break;
            }
        }
        *q = *p;
        p++;
        q++;
    }
    *q = 0;

    return string;
}

char *strfmt(const char *fmt, ...)
{
    va_list args;
    char *str;
    va_start(args, fmt);
    str = vstrfmt(fmt, args);
    va_end(args);
    return str;
}

void dummy_free(void *p)
{
    (void)p; /* suppress unused argument warning */
}

#ifdef FRT_IS_C99
extern void usleep(unsigned long usec);
#else
# ifdef RUBY_BINDINGS
struct timeval rb_time_interval _((VALUE));
# else
#  include <unistd.h>
# endif
#endif

extern void micro_sleep(const int micro_seconds)
{
#ifdef RUBY_BINDINGS
    rb_thread_wait_for(rb_time_interval(rb_float_new((double)micro_seconds/1000000.0)));
#else
# ifdef POSH_OS_WIN32
    Sleep(micro_seconds / 1000);
# else
    usleep(micro_seconds);
# endif
#endif
}

typedef struct FreeMe
{
    void *p;
    free_ft free_func;
} FreeMe;

static FreeMe *free_mes = NULL;
static int free_mes_size = 0;
static int free_mes_capa = 0;

void register_for_cleanup(void *p, free_ft free_func)
{
    FreeMe *free_me;
    if (free_mes_capa == 0) {
        free_mes_capa = 16;
        free_mes = ALLOC_N(FreeMe, free_mes_capa);
    }
    else if (free_mes_capa <= free_mes_size) {
        free_mes_capa *= 2;
        REALLOC_N(free_mes, FreeMe, free_mes_capa);
    }
    free_me = free_mes + free_mes_size++;
    free_me->p = p;
    free_me->free_func = free_func;
}

void do_clean_up()
{
    int i;
    for (i = 0; i < free_mes_size; i++) {
        FreeMe *free_me = free_mes + i;
        free_me->free_func(free_me->p);
    }
    free(free_mes);
    free_mes = NULL;
    free_mes_size = free_mes_capa = 0;
}
