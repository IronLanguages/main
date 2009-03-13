#ifndef FRT_ARRAY_H
#define FRT_ARRAY_H
#include "global.h"

#if defined POSH_OS_SOLARIS || defined POSH_OS_SUNOS
# define ARY_META_CNT 4
#else
# define ARY_META_CNT 3
#endif

#define ARY_INIT_CAPA 8
#define ary_size(ary)      ary_sz(ary)
#define ary_sz(ary)        (((int *)ary)[-1])
#define ary_capa(ary)      (((int *)ary)[-2])
#define ary_type_size(ary) (((int *)ary)[-3])
#define ary_start(ary)     ((void **)&(((int *)ary)[-ARY_META_CNT]))
#define ary_free(ary)      free(ary_start(ary))

#define ary_new_type_capa(type, init_capa)\
                                (type *)ary_new_i(sizeof(type), init_capa)
#define ary_new_type(type)      (type *)ary_new_i(sizeof(type), 0)
#define ary_new_capa(init_capa) ary_new_i(sizeof(void *), init_capa)
#define ary_new()               ary_new_i(sizeof(void *), 0)
#define ary_resize(ary, size)   ary_resize_i(((void ***)(void *)&ary), size)
#define ary_set(ary, i, val)    ary_set_i(((void ***)(void *)&ary), i, val)
#define ary_get(ary, i)         ary_get_i(((void **)ary), i)
#define ary_push(ary, val)      ary_push_i(((void ***)(void *)&ary), val)
#define ary_pop(ary)            ary_pop_i(((void **)ary))
#define ary_unshift(ary, val)   ary_unshift_i(((void ***)(void *)&ary), val)
#define ary_shift(ary)          ary_shift_i(((void **)ary))
#define ary_remove(ary, i)      ary_remove_i(((void **)ary), i)
#define ary_delete(ary, i, f)   ary_delete_i(((void **)ary), i, (free_ft)f)
#define ary_destroy(ary, f)     ary_destroy_i(((void **)ary), (free_ft)f)
#define ary_rsz(ary, size)      ary_resize(ary, size)
#define ary_grow(ary)           ary_resize(ary, ary_sz(ary))
#define ary_last(ary)           ary[ary_sz(ary) - 1]
#define ary_sort(ary, cmp)      qsort(ary, ary_size(ary), ary_type_size(ary), cmp)
#define ary_each_rev(ary, i)    for (i = ary_size(ary) - 1; i >= 0; i--)
#define ary_each(ary, i)        for (i = 0; i < ary_size(ary); i++)

extern void   ary_resize_i(void ***ary, int size);
extern void **ary_new_i(int type_size, int init_capa);
extern void   ary_set_i(void ***ary, int index, void *value);
extern void  *ary_get_i(void **ary, int index);
extern void   ary_push_i(void ***ary, void *value);
extern void  *ary_pop_i(void **ary);
extern void   ary_unshift_i(void ***ary, void *value);
extern void  *ary_shift_i(void **ary);
extern void  *ary_remove_i(void **ary, int index);
extern void   ary_delete_i(void **ary, int index, void (*free_elem)(void *p));
extern void   ary_destroy_i(void **ary, void (*free_elem)(void *p));

#endif
