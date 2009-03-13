#include "global.h"
#include "mempool.h"
#include <string.h>

MemoryPool *mp_new_capa(int chuck_size, int init_buf_capa)
{
    MemoryPool *mp = ALLOC(MemoryPool);
    mp->chunk_size = chuck_size;
    mp->buf_capa = init_buf_capa;
    mp->buffers = ALLOC_N(char *, init_buf_capa);

    mp->buffers[0] = mp->curr_buffer = emalloc(mp->chunk_size);
    mp->buf_alloc = 1;
    mp->buf_pointer = 0;
    mp->pointer = 0;
    return mp;
}

MemoryPool *mp_new()
{
    return mp_new_capa(MP_BUF_SIZE, MP_INIT_CAPA);
}

INLINE void *mp_alloc(MemoryPool *mp, int size)
{
    char *p;
    p = mp->curr_buffer + mp->pointer;
#if defined POSH_OS_SOLARIS || defined POSH_OS_SUNOS
    size = (((size - 1) >> 3) + 1) << 3;
#endif
    mp->pointer += size;

    if (mp->pointer > mp->chunk_size) {
        mp->buf_pointer++;
        if (mp->buf_pointer >= mp->buf_alloc) {
            mp->buf_alloc++;
            if (mp->buf_alloc >= mp->buf_capa) {
                mp->buf_capa <<= 1;
                REALLOC_N(mp->buffers, char *, mp->buf_capa);
            }
            mp->buffers[mp->buf_pointer] = emalloc(mp->chunk_size);
        }
        p = mp->curr_buffer = mp->buffers[mp->buf_pointer];
        mp->pointer = size;
    }
    return p;
}

char *mp_strdup(MemoryPool *mp, const char *str)
{
    int len = strlen(str) + 1;
    return memcpy(mp_alloc(mp, len), str, len);
}

char *mp_strndup(MemoryPool *mp, const char *str, int len)
{
    char *s = memcpy(mp_alloc(mp, len + 1), str, len);
    s[len] = '\0';
    return s;
}

void *mp_memdup(MemoryPool *mp, const void *p, int len)
{
    return memcpy(mp_alloc(mp, len), p, len);
}

int mp_used(MemoryPool *mp)
{
    return mp->buf_pointer * mp->chunk_size + mp->pointer;
}

void mp_reset(MemoryPool *mp)
{
    mp->buf_pointer = 0;
    mp->pointer = 0;
    mp->curr_buffer = mp->buffers[0];
}

void mp_destroy(MemoryPool *mp)
{
    int i;
    for (i = 0; i < mp->buf_alloc; i++) {
        free(mp->buffers[i]);
    }
    free(mp->buffers);
    free(mp);
}
