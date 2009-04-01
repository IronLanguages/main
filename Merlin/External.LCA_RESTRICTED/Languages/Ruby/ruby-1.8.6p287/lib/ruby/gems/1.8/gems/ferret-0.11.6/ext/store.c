#include "store.h"
#include <string.h>

#define VINT_MAX_LEN 10
#define VINT_END BUFFER_SIZE - VINT_MAX_LEN

/*
 * TODO: add try finally
 */
void with_lock(Lock *lock, void (*func)(void *arg), void *arg)
{
    if (!lock->obtain(lock)) {
        RAISE(IO_ERROR, "couldn't obtain lock \"%s\"", lock->name);
    }
    func(arg);
    lock->release(lock);
}

/*
 * TODO: add try finally
 */
void with_lock_name(Store *store, char *lock_name,
                    void (*func)(void *arg), void *arg)
{
    Lock *lock = store->open_lock_i(store, lock_name);
    if (!lock->obtain(lock)) {
        RAISE(LOCK_ERROR, "couldn't obtain lock \"%s\"", lock->name);
    }
    func(arg);
    lock->release(lock);
    store->close_lock_i(lock);
}

void store_deref(Store *store)
{
    mutex_lock(&store->mutex_i);
    if (--store->ref_cnt <= 0) {
        store->close_i(store);
    }
    else {
        mutex_unlock(&store->mutex_i);
    }
}

Lock *open_lock(Store *store, char *lockname)
{
    Lock *lock = store->open_lock_i(store, lockname);
    hs_add(store->locks, lock);
    return lock;
}

void close_lock(Lock *lock)
{
    hs_del(lock->store->locks, lock);
}

void close_lock_i(Lock *lock)
{
    lock->store->close_lock_i(lock);
}

/**
 * Create a store struct initializing the mutex.
 */
Store *store_new()
{
    Store *store = ALLOC(Store);
    store->ref_cnt = 1;
    mutex_init(&store->mutex_i, NULL);
    mutex_init(&store->mutex, NULL);
    store->locks = hs_new(ptr_hash, ptr_eq, (free_ft)&close_lock_i);
    return store;
}

/**
 * Destroy the store freeing allocated resources
 *
 * @param store the store struct to free
 */
void store_destroy(Store *store)
{
    mutex_destroy(&store->mutex_i);
    mutex_destroy(&store->mutex);
    hs_destroy(store->locks);
    free(store);
}

/**
 * Create a newly allocated and initialized OutStream object
 *
 * @return a newly allocated and initialized OutStream object
 */
OutStream *os_new()
{
    OutStream *os = ALLOC(OutStream);
    os->buf.start = 0;
    os->buf.pos = 0;
    os->buf.len = 0;
    return os;
}

/**
 * Flush the countents of the OutStream's buffers
 *
 * @param the OutStream to flush
 */
INLINE void os_flush(OutStream *os)
{
    os->m->flush_i(os, os->buf.buf, os->buf.pos);
    os->buf.start += os->buf.pos;
    os->buf.pos = 0;
}

void os_close(OutStream *os)
{
    os_flush(os);
    os->m->close_i(os);
    free(os);
}

off_t os_pos(OutStream *os)
{
    return os->buf.start + os->buf.pos;
}

void os_seek(OutStream *os, off_t new_pos)
{
    os_flush(os);
    os->buf.start = new_pos;
    os->m->seek_i(os, new_pos);
}

/**
 * Unsafe alternative to os_write_byte. Only use this method if you know there
 * is no chance of buffer overflow.
 */
#define write_byte(os, b) os->buf.buf[os->buf.pos++] = (uchar)b

/**
 * Write a single byte +b+ to the OutStream +os+
 *
 * @param os the OutStream to write to
 * @param b  the byte to write
 * @raise IO_ERROR if there is an IO error writing to the filesystem
 */
INLINE void os_write_byte(OutStream *os, uchar b)
{
    if (os->buf.pos >= BUFFER_SIZE) {
        os_flush(os);
    }
    write_byte(os, b);
}

void os_write_bytes(OutStream *os, uchar *buf, int len)
{
    if (os->buf.pos > 0) {      /* flush buffer */
        os_flush(os);
    }

    if (len < BUFFER_SIZE) {
        os->m->flush_i(os, buf, len);
        os->buf.start += len;
    }
    else {
        int pos = 0;
        int size;
        while (pos < len) {
            if (len - pos < BUFFER_SIZE) {
                size = len - pos;
            }
            else {
                size = BUFFER_SIZE;
            }
            os->m->flush_i(os, buf + pos, size);
            pos += size;
            os->buf.start += size;
        }
    }
}

/**
 * Create a newly allocated and initialized InStream
 *
 * @return a newly allocated and initialized InStream
 */
InStream *is_new()
{
    InStream *is = ALLOC(InStream);
    is->buf.start = 0;
    is->buf.pos = 0;
    is->buf.len = 0;
    is->ref_cnt_ptr = ALLOC_AND_ZERO(int);
    return is;
}

/**
 * Refill the InStream's buffer from the store source (filesystem or memory).
 *
 * @param is the InStream to refill
 * @raise IO_ERROR if there is a error reading from the filesystem
 * @raise EOF_ERROR if there is an attempt to read past the end of the file
 */
void is_refill(InStream *is)
{
    off_t start = is->buf.start + is->buf.pos;
    off_t last = start + BUFFER_SIZE;
    off_t flen = is->m->length_i(is);

    if (last > flen) {          /* don't read past EOF */
        last = flen;
    }

    is->buf.len = last - start;
    if (is->buf.len <= 0) {
        RAISE(EOF_ERROR, "current pos = %"F_OFF_T_PFX"d, "
              "file length = %"F_OFF_T_PFX"d", start, flen);
    }

    is->m->read_i(is, is->buf.buf, is->buf.len);

    is->buf.start = start;
    is->buf.pos = 0;
}

/**
 * Unsafe alternative to is_read_byte. Only use this method when you know
 * there is no chance that you will read past the end of the InStream's
 * buffer.
 */
#define read_byte(is) is->buf.buf[is->buf.pos++]

/**
 * Read a singly byte (unsigned char) from the InStream +is+.
 *
 * @param is the Instream to read from
 * @return a single unsigned char read from the InStream +is+
 * @raise IO_ERROR if there is a error reading from the filesystem
 * @raise EOF_ERROR if there is an attempt to read past the end of the file
 */
INLINE uchar is_read_byte(InStream *is)
{
    if (is->buf.pos >= is->buf.len) {
        is_refill(is);
    }

    return read_byte(is);
}

off_t is_pos(InStream *is)
{
    return is->buf.start + is->buf.pos;
}

uchar *is_read_bytes(InStream *is, uchar *buf, int len)
{
    int i;
    off_t start;

    if ((is->buf.pos + len) < is->buf.len) {
        for (i = 0; i < len; i++) {
            buf[i] = read_byte(is);
        }
    }
    else {                              /* read all-at-once */
        start = is_pos(is);
        is->m->seek_i(is, start);
        is->m->read_i(is, buf, len);

        is->buf.start = start + len;    /* adjust stream variables */
        is->buf.pos = 0;
        is->buf.len = 0;                /* trigger refill on read */
    }
    return buf;
}

void is_seek(InStream *is, off_t pos)
{
    if (pos >= is->buf.start && pos < (is->buf.start + is->buf.len)) {
        is->buf.pos = pos - is->buf.start;  /* seek within buffer */
    }
    else {
        is->buf.start = pos;
        is->buf.pos = 0;
        is->buf.len = 0;                    /* trigger refill() on read() */
        is->m->seek_i(is, pos);
    }
}

void is_close(InStream *is)
{
    if (--(*(is->ref_cnt_ptr)) < 0) {
        is->m->close_i(is);
        free(is->ref_cnt_ptr);
    }
    free(is);
}

InStream *is_clone(InStream *is)
{
    InStream *new_index_i = ALLOC(InStream);
    memcpy(new_index_i, is, sizeof(InStream));
    (*(new_index_i->ref_cnt_ptr))++;
    return new_index_i;
}

f_i32 is_read_i32(InStream *is)
{
    return ((f_i32)is_read_byte(is) << 24) |
        ((f_i32)is_read_byte(is) << 16) |
        ((f_i32)is_read_byte(is) << 8) |
        ((f_i32)is_read_byte(is));
}

f_i64 is_read_i64(InStream *is)
{
    return ((f_i64)is_read_byte(is) << 56) |
        ((f_i64)is_read_byte(is) << 48) |
        ((f_i64)is_read_byte(is) << 40) |
        ((f_i64)is_read_byte(is) << 32) |
        ((f_i64)is_read_byte(is) << 24) |
        ((f_i64)is_read_byte(is) << 16) |
        ((f_i64)is_read_byte(is) << 8) |
        ((f_i64)is_read_byte(is));
}

f_u32 is_read_u32(InStream *is)
{
    return ((f_u32)is_read_byte(is) << 24) |
        ((f_u32)is_read_byte(is) << 16) |
        ((f_u32)is_read_byte(is) << 8) |
        ((f_u32)is_read_byte(is));
}

f_u64 is_read_u64(InStream *is)
{
    return ((f_u64)is_read_byte(is) << 56) |
        ((f_u64)is_read_byte(is) << 48) |
        ((f_u64)is_read_byte(is) << 40) |
        ((f_u64)is_read_byte(is) << 32) |
        ((f_u64)is_read_byte(is) << 24) |
        ((f_u64)is_read_byte(is) << 16) |
        ((f_u64)is_read_byte(is) << 8) |
        ((f_u64)is_read_byte(is));
}

/* optimized to use unchecked read_byte if there is definitely space */
INLINE unsigned int is_read_vint(InStream *is)
{
    register unsigned int res, b;
    register int shift = 7;

    if (is->buf.pos > (is->buf.len - VINT_MAX_LEN)) {
        b = is_read_byte(is);
        res = b & 0x7F;                 /* 0x7F = 0b01111111 */

        while ((b & 0x80) != 0) {       /* 0x80 = 0b10000000 */
            b = is_read_byte(is);
            res |= (b & 0x7F) << shift;
            shift += 7;
        }
    }
    else {                              /* unchecked optimization */
        b = read_byte(is);
        res = b & 0x7F;                 /* 0x7F = 0b01111111 */

        while ((b & 0x80) != 0) {       /* 0x80 = 0b10000000 */
            b = read_byte(is);
            res |= (b & 0x7F) << shift;
            shift += 7;
        }
    }

    return res;
}

/* optimized to use unchecked read_byte if there is definitely space */
INLINE off_t is_read_voff_t(InStream *is)
{
    register off_t res, b;
    register int shift = 7;

    if (is->buf.pos > (is->buf.len - VINT_MAX_LEN)) {
        b = is_read_byte(is);
        res = b & 0x7F;                 /* 0x7F = 0b01111111 */

        while ((b & 0x80) != 0) {       /* 0x80 = 0b10000000 */
            b = is_read_byte(is);
            res |= (b & 0x7F) << shift;
            shift += 7;
        }
    }
    else {                              /* unchecked optimization */
        b = read_byte(is);
        res = b & 0x7F;                 /* 0x7F = 0b01111111 */

        while ((b & 0x80) != 0) {       /* 0x80 = 0b10000000 */
            b = read_byte(is);
            res |= (b & 0x7F) << shift;
            shift += 7;
        }
    }

    return res;
}

/* optimized to use unchecked read_byte if there is definitely space */
INLINE unsigned long long is_read_vll(InStream *is)
{
    register unsigned long long res, b;
    register int shift = 7;

    if (is->buf.pos > (is->buf.len - VINT_MAX_LEN)) {
        b = is_read_byte(is);
        res = b & 0x7F;                 /* 0x7F = 0b01111111 */

        while ((b & 0x80) != 0) {       /* 0x80 = 0b10000000 */
            b = is_read_byte(is);
            res |= (b & 0x7F) << shift;
            shift += 7;
        }
    }
    else {                              /* unchecked optimization */
        b = read_byte(is);
        res = b & 0x7F;                 /* 0x7F = 0b01111111 */

        while ((b & 0x80) != 0) {       /* 0x80 = 0b10000000 */
            b = read_byte(is);
            res |= (b & 0x7F) << shift;
            shift += 7;
        }
    }

    return res;
}

INLINE void is_skip_vints(InStream *is, register int cnt)
{
    for (; cnt > 0; cnt--) {
        while ((is_read_byte(is) & 0x80) != 0) {
        }
    }
}

INLINE void is_read_chars(InStream *is, char *buffer,
                                  int off, int len)
{
    int end, i;

    end = off + len;

    for (i = off; i < end; i++) {
        buffer[i] = is_read_byte(is);
    }
}

char *is_read_string(InStream *is)
{
    register int length = (int) is_read_vint(is);
    char *str = ALLOC_N(char, length + 1);
    str[length] = '\0';

    if (is->buf.pos > (is->buf.len - length)) {
        register int i;
        for (i = 0; i < length; i++) {
            str[i] = is_read_byte(is);
        }
    }
    else {                      /* unchecked optimization */
        memcpy(str, is->buf.buf + is->buf.pos, length);
        is->buf.pos += length;
    }

    return str;
}

char *is_read_string_safe(InStream *is)
{
    register int length = (int) is_read_vint(is);
    char *str = ALLOC_N(char, length + 1);
    str[length] = '\0';

    TRY
        if (is->buf.pos > (is->buf.len - length)) {
            register int i;
            for (i = 0; i < length; i++) {
                str[i] = is_read_byte(is);
            }
        }
        else {                      /* unchecked optimization */
            memcpy(str, is->buf.buf + is->buf.pos, length);
            is->buf.pos += length;
        }
    XCATCHALL
        free(str);
    XENDTRY

    return str;
}

void os_write_i32(OutStream *os, f_i32 num)
{
    os_write_byte(os, (uchar)((num >> 24) & 0xFF));
    os_write_byte(os, (uchar)((num >> 16) & 0xFF));
    os_write_byte(os, (uchar)((num >> 8) & 0xFF));
    os_write_byte(os, (uchar)(num & 0xFF));
}

void os_write_i64(OutStream *os, f_i64 num)
{
    os_write_byte(os, (uchar)((num >> 56) & 0xFF));
    os_write_byte(os, (uchar)((num >> 48) & 0xFF));
    os_write_byte(os, (uchar)((num >> 40) & 0xFF));
    os_write_byte(os, (uchar)((num >> 32) & 0xFF));
    os_write_byte(os, (uchar)((num >> 24) & 0xFF));
    os_write_byte(os, (uchar)((num >> 16) & 0xFF));
    os_write_byte(os, (uchar)((num >> 8) & 0xFF));
    os_write_byte(os, (uchar)(num & 0xFF));
}

void os_write_u32(OutStream *os, f_u32 num)
{
    os_write_byte(os, (uchar)((num >> 24) & 0xFF));
    os_write_byte(os, (uchar)((num >> 16) & 0xFF));
    os_write_byte(os, (uchar)((num >> 8) & 0xFF));
    os_write_byte(os, (uchar)(num & 0xFF));
}

void os_write_u64(OutStream *os, f_u64 num)
{
    os_write_byte(os, (uchar)((num >> 56) & 0xFF));
    os_write_byte(os, (uchar)((num >> 48) & 0xFF));
    os_write_byte(os, (uchar)((num >> 40) & 0xFF));
    os_write_byte(os, (uchar)((num >> 32) & 0xFF));
    os_write_byte(os, (uchar)((num >> 24) & 0xFF));
    os_write_byte(os, (uchar)((num >> 16) & 0xFF));
    os_write_byte(os, (uchar)((num >> 8) & 0xFF));
    os_write_byte(os, (uchar)(num & 0xFF));
}

/* optimized to use an unchecked write if there is space */
INLINE void os_write_vint(OutStream *os, register unsigned int num)
{
    if (os->buf.pos > VINT_END) {
        while (num > 127) {
            os_write_byte(os, (uchar)((num & 0x7f) | 0x80));
            num >>= 7;
        }
        os_write_byte(os, (uchar)(num));
    }
    else {
        while (num > 127) {
            write_byte(os, (uchar)((num & 0x7f) | 0x80));
            num >>= 7;
        }
        write_byte(os, (uchar)(num));
    }
}

/* optimized to use an unchecked write if there is space */
INLINE void os_write_voff_t(OutStream *os, register off_t num)
{
    if (os->buf.pos > VINT_END) {
        while (num > 127) {
            os_write_byte(os, (uchar)((num & 0x7f) | 0x80));
            num >>= 7;
        }
        os_write_byte(os, (uchar)num);
    }
    else {
        while (num > 127) {
            write_byte(os, (uchar)((num & 0x7f) | 0x80));
            num >>= 7;
        }
        write_byte(os, (uchar)num);
    }
}

/* optimized to use an unchecked write if there is space */
INLINE void os_write_vll(OutStream *os, register unsigned long long num)
{
    if (os->buf.pos > VINT_END) {
        while (num > 127) {
            os_write_byte(os, (uchar)((num & 0x7f) | 0x80));
            num >>= 7;
        }
        os_write_byte(os, (uchar)num);
    }
    else {
        while (num > 127) {
            write_byte(os, (uchar)((num & 0x7f) | 0x80));
            num >>= 7;
        }
        write_byte(os, (uchar)num);
    }
}

void os_write_string(OutStream *os, char *str)
{
    int len = (int)strlen(str);
    os_write_vint(os, len);

    os_write_bytes(os, (uchar *)str, len);
}

/**
 * Determine if the filename is the name of a lock file. Return 1 if it is, 0
 * otherwise.
 *
 * @param filename the name of the file to check
 * @return 1 (true) if the file is a lock file, 0 (false) otherwise
 */
int file_is_lock(char *filename)
{
    int start = (int) strlen(filename) - 4;
    return ((start > 0) && (strcmp(LOCK_EXT, &filename[start]) == 0));
}

void is2os_copy_bytes(InStream *is, OutStream *os, int cnt)
{
    int len;
    uchar buf[BUFFER_SIZE];

    for (; cnt > 0; cnt -= BUFFER_SIZE) {
        len = ((cnt > BUFFER_SIZE) ? BUFFER_SIZE : cnt);
        is_read_bytes(is, buf, len);
        os_write_bytes(os, buf, len);
    }
}

void is2os_copy_vints(InStream *is, OutStream *os, int cnt)
{
    uchar b;
    for (; cnt > 0; cnt--) {
        while (((b = is_read_byte(is)) & 0x80) != 0) {
            os_write_byte(os, b);
        }
        os_write_byte(os, b);
    }
}

/**
 * Test argument used to test the store->each function
 */
struct FileNameListArg
{
    int count;
    int size;
    int total_len;
    char **files;
};

/**
 * Test function used to test store->each function
 */
static void add_file_name(char *fname, void *arg)
{
    struct FileNameListArg *fnl = (struct FileNameListArg *)arg;
    if (fnl->count >= fnl->size) {
        fnl->size *= 2;
        REALLOC_N(fnl->files, char *, fnl->size);
    }
    fnl->files[fnl->count++] = estrdup(fname);
    fnl->total_len += strlen(fname) + 2;
}

char *store_to_s(Store *store)
{
    struct FileNameListArg fnl;
    char *buf, *b;
    int i;
    fnl.count = 0;
    fnl.size = 16;
    fnl.total_len = 10;
    fnl.files = ALLOC_N(char *, 16);

    store->each(store, &add_file_name, &fnl);
    qsort(fnl.files, fnl.count, sizeof(char *), &scmp);
    b = buf = ALLOC_N(char, fnl.total_len);

    for (i = 0; i < fnl.count; i++) {
        char *fn = fnl.files[i];
        int len = strlen(fn);
        memcpy(b, fn, len);
        b += len;
        *b++ = '\n';
        free(fn);
    }
    *b = '\0';
    free(fnl.files);

    return buf;
}
