#ifndef FRT_LANG_H
#define FRT_LANG_H

#define RUBY_BINDINGS 1

#include <stdarg.h>
#include <ruby.h>

#undef close
#undef rename
#undef read

#define frt_malloc xmalloc
#define frt_calloc(n) xcalloc(n, 1)
#define frt_realloc xrealloc


#ifdef FRT_HAS_ISO_VARARGS
/* C99-compliant compiler */

# define FRT_EXIT(...) frt_rb_raise(__FILE__, __LINE__, __func__, __VA_ARGS__)
extern void frt_rb_raise(const char *file, int line_num, const char *func,
                         const char *err_type, const char *fmt, ...);

# define V_FRT_EXIT(err_type, fmt, args) \
    vfrt_rb_raise(__FILE__, __LINE__, __func__, err_type, fmt, args)
extern void vfrt_rb_raise(const char *file, int line_num, const char *func,
                          const char *err_type, const char *fmt, va_list args);

#elif defined(FRT_HAS_GNUC_VARARGS)
/* gcc has an extension */

# define FRT_EXIT(args...) frt_rb_raise(__FILE__, __LINE__, __func__, ##args)
extern void frt_rb_raise(const char *file, int line_num, const char *func,
                         const char *err_type, const char *fmt, ...);

# define V_FRT_EXIT(err_type, fmt, args) \
    vfrt_rb_raise(__FILE__, __LINE__, __func__, err_type, fmt, args)
extern void vfrt_rb_raise(const char *file, int line_num, const char *func,
                          const char *err_type, const char *fmt, va_list args);
#else
/* Can't do VARARGS */

extern void FRT_EXIT(const char *err_type, const char *fmt, ...);
extern void V_FRT_EXIT(const char *err_type, const char *fmt, va_list args);
#endif

#endif
