#include "array.h"
#include <string.h>

#define META_CNT ARY_META_CNT
#define DATA_SZ sizeof(int) * META_CNT

void **ary_new_i(int type_size, int init_capa)
{
    void **ary;
    if (init_capa <= 0) {
        init_capa = ARY_INIT_CAPA;
    }
    ary = (void **)&(((int *)ecalloc(DATA_SZ + init_capa * type_size))[META_CNT]);
    ary_type_size(ary) = type_size;
    ary_capa(ary) = init_capa;
    return ary;
}

INLINE void ary_resize_i(void ***ary, int size)
{
    size++;
    if (size >= ary_sz(*ary)) {
        int capa = ary_capa(*ary);
        if (size >= capa) {
            int *ary_start = &((int *)*ary)[-META_CNT];
            while (size >= capa) {
                capa <<= 1;
            }

            ary_start = (int *)erealloc(ary_start,
                                        DATA_SZ + capa * ary_type_size(*ary));
            *ary = (void **)&(ary_start[META_CNT]);
            memset(((char *)*ary) + ary_type_size(*ary) * ary_sz(*ary), 0,
                   (capa - ary_sz(*ary)) * ary_type_size(*ary));
            ary_capa(*ary) = capa;
        }
        ary_sz(*ary) = size;
    }
}

void ary_set_i(void ***ary, int index, void *value)
{
    if (index < 0) {
        index += ary_sz(*ary);
        if (index < 0) {
            RAISE(INDEX_ERROR, "index %d out array", index);
        }
    }
    ary_resize_i(ary, index);
    (*ary)[index] = value;
}

void *ary_get_i(void **ary, int index)
{
    if (index < 0) {
        index += ary_sz(ary);
    }
    if (index >= 0 && index < ary_sz(ary)) {
        return ary[index];
    }
    else {
        return NULL;
    }
}

void ary_push_i(void ***ary, void *value)
{
    int size = ary_sz(*ary);
    ary_resize_i(ary, size);
    (*ary)[size] = value;
}

void *ary_pop_i(void **ary)
{
    void *val = ary[--ary_sz(ary)];
    ary[ary_sz(ary)] = NULL;
    return val;
}

void ary_unshift_i(void ***ary, void *value)
{
    int size = ary_sz(*ary);
    ary_resize_i(ary, size);
    memmove(*ary, *ary + 1, size * sizeof(void *));
    (*ary)[0] = value;
}

void *ary_shift_i(void **ary)
{
    void *val = ary[0];
    int size = --ary_sz(ary);
    memmove(ary, ary + 1, size * sizeof(void *));
    ary[size] = NULL;
    return val;
}

void *ary_remove_i(void **ary, int index)
{
    if (index >= 0 && index < ary_sz(ary)) {
        void *val = ary[index];
        memmove(ary + index, ary + index + 1,
                (ary_sz(ary) - index + 1) * sizeof(void *));
        ary_sz(ary)--;
        return val;
    }
    else {
        return NULL;
    }
}

void ary_delete_i(void **ary, int index, void (*free_elem)(void *p))
{
    free_elem(ary_remove(ary, index));
}

void ary_destroy_i(void **ary, void (*free_elem)(void *p))
{
    int i;
    for (i = ary_sz(ary) - 1; i >= 0; i--) {
        free_elem(ary[i]);
    }
    ary_free(ary);
}
