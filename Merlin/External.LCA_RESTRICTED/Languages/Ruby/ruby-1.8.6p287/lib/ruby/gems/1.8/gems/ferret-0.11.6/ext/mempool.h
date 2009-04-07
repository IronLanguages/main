#ifndef FRT_MEM_POOL_H
#define FRT_MEM_POOL_H

#define MP_BUF_SIZE 65536
#define MP_INIT_CAPA 4

typedef struct MemoryPool {
    int buf_alloc;
    int buf_capa;
    int buf_pointer;
    int pointer;
    int chunk_size;
    char *curr_buffer;
    char **buffers;
} MemoryPool;

extern MemoryPool *mp_new();
extern MemoryPool *mp_new_capa(int chunk_size, int init_capa);
extern INLINE void *mp_alloc(MemoryPool *mp, int size);
extern void mp_reset(MemoryPool *mp);
extern void mp_destroy(MemoryPool *mp);
extern char *mp_strdup(MemoryPool *mp, const char *str);
extern char *mp_strndup(MemoryPool *mp, const char *str, int len);
extern void *mp_memdup(MemoryPool *mp, const void *p, int len);
extern int mp_used(MemoryPool *mp);

#define MP_ALLOC_N(mp,type,n) (type *)mp_alloc(mp, sizeof(type)*(n))
#define MP_ALLOC(mp,type) (type *)mp_alloc(mp, sizeof(type))

#define MP_ALLOC_AND_ZERO(mp,type)\
    (type*)memset(mp_alloc(mp, sizeof(type)), 0, sizeof(type))
#define MP_ALLOC_AND_ZERO_N(mp,type,n)\
    (type*)ZEROSET_N(mp_alloc(mp, sizeof(type)*(n)), type, n)

#endif
