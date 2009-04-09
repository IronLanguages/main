#include "store.h"
#include <string.h>

extern Store *store_new();
extern void store_destroy(Store *store);
extern OutStream *os_new();
extern InStream *is_new();
extern int file_is_lock(char *filename);

static RAMFile *rf_new(const char *name)
{
    RAMFile *rf = ALLOC(RAMFile);
    rf->buffers = ALLOC(uchar *);
    rf->buffers[0] = ALLOC_N(uchar, BUFFER_SIZE);
    rf->name = estrdup(name);
    rf->len = 0;
    rf->bufcnt = 1;
    rf->ref_cnt = 1;
    return rf;
}

static void rf_extend_if_necessary(RAMFile *rf, int buf_num)
{
    while (rf->bufcnt <= buf_num) {
        REALLOC_N(rf->buffers, uchar *, (rf->bufcnt + 1));
        rf->buffers[rf->bufcnt++] = ALLOC_N(uchar, BUFFER_SIZE);
    }
}

static void rf_close(void *p)
{
    int i;
    RAMFile *rf = (RAMFile *)p;
    if (rf->ref_cnt > 0) {
        return;
    }
    free(rf->name);
    for (i = 0; i < rf->bufcnt; i++) {
        free(rf->buffers[i]);
    }
    free(rf->buffers);
    free(rf);
}

static void ram_touch(Store *store, char *filename)
{
    if (h_get(store->dir.ht, filename) == NULL) {
        h_set(store->dir.ht, filename, rf_new(filename));
    }
}

static int ram_exists(Store *store, char *filename)
{
    if (h_get(store->dir.ht, filename) != NULL) {
        return true;
    }
    else {
        return false;
    }
}

static int ram_remove(Store *store, char *filename)
{
    RAMFile *rf = h_rem(store->dir.ht, filename, false);
    if (rf != NULL) {
        DEREF(rf);
        rf_close(rf);
        return true;
    }
    else {
        return false;
    }
}

static void ram_rename(Store *store, char *from, char *to)
{
    RAMFile *rf = (RAMFile *)h_rem(store->dir.ht, from, false);
    RAMFile *tmp;

    if (rf == NULL) {
        RAISE(IO_ERROR, "couldn't rename \"%s\" to \"%s\". \"%s\""
              " doesn't exist", from, to, from);
    }

    free(rf->name);

    rf->name = estrdup(to);

    /* clean up the file we are overwriting */
    tmp = (RAMFile *)h_get(store->dir.ht, to);
    if (tmp != NULL) {
        DEREF(tmp);
    }

    h_set(store->dir.ht, rf->name, rf);
}

static int ram_count(Store *store)
{
    return store->dir.ht->size;
}

static void ram_each(Store *store,
                     void (*func)(char *fname, void *arg), void *arg)
{
    HashTable *ht = store->dir.ht;
    int i;
    for (i = 0; i <= ht->mask; i++) {
        RAMFile *rf = (RAMFile *)ht->table[i].value;
        if (rf) {
            if (strncmp(rf->name, LOCK_PREFIX, strlen(LOCK_PREFIX)) == 0) {
                continue;
            }
            func(rf->name, arg);
        }
    }
}

static void ram_close_i(Store *store)
{
    HashTable *ht = store->dir.ht;
    int i;
    for (i = 0; i <= ht->mask; i++) {
        RAMFile *rf = (RAMFile *)ht->table[i].value;
        if (rf) {
            DEREF(rf);
        }
    }
    h_destroy(store->dir.ht);
    store_destroy(store);
}

/*
 * Be sure to keep the locks
 */
static void ram_clear(Store *store)
{
    int i;
    HashTable *ht = store->dir.ht;
    for (i = 0; i <= ht->mask; i++) {
        RAMFile *rf = (RAMFile *)ht->table[i].value;
        if (rf && !file_is_lock(rf->name)) {
            DEREF(rf);
            h_del(ht, rf->name);
        }
    }
}

static void ram_clear_locks(Store *store)
{
    int i;
    HashTable *ht = store->dir.ht;
    for (i = 0; i <= ht->mask; i++) {
        RAMFile *rf = (RAMFile *)ht->table[i].value;
        if (rf && file_is_lock(rf->name)) {
            DEREF(rf);
            h_del(ht, rf->name);
        }
    }
}

static void ram_clear_all(Store *store)
{
    int i;
    HashTable *ht = store->dir.ht;
    for (i = 0; i <= ht->mask; i++) {
        RAMFile *rf = (RAMFile *)ht->table[i].value;
        if (rf) {
            DEREF(rf);
            h_del(ht, rf->name);
        }
    }
}

static off_t ram_length(Store *store, char *filename)
{
    RAMFile *rf = (RAMFile *)h_get(store->dir.ht, filename);
    if (rf != NULL) {
        return rf->len;
    }
    else {
        return 0;
    }
}

off_t ramo_length(OutStream *os)
{
    return os->file.rf->len;
}

static void ramo_flush_i(OutStream *os, uchar *src, int len)
{
    uchar *buffer;
    RAMFile *rf = os->file.rf;
    int buffer_number, buffer_offset, bytes_in_buffer, bytes_to_copy;
    int src_offset;
    off_t pointer = os->pointer;

    buffer_number = (int)(pointer / BUFFER_SIZE);
    buffer_offset = pointer % BUFFER_SIZE;
    bytes_in_buffer = BUFFER_SIZE - buffer_offset;
    bytes_to_copy = bytes_in_buffer < len ? bytes_in_buffer : len;

    rf_extend_if_necessary(rf, buffer_number);

    buffer = rf->buffers[buffer_number];
    memcpy(buffer + buffer_offset, src, bytes_to_copy);

    if (bytes_to_copy < len) {
        src_offset = bytes_to_copy;
        bytes_to_copy = len - bytes_to_copy;
        buffer_number += 1;
        rf_extend_if_necessary(rf, buffer_number);
        buffer = rf->buffers[buffer_number];

        memcpy(buffer, src + src_offset, bytes_to_copy);
    }
    os->pointer += len;

    if (os->pointer > rf->len) {
        rf->len = os->pointer;
    }
}

static void ramo_seek_i(OutStream *os, off_t pos)
{
    os->pointer = pos;
}

void ramo_reset(OutStream *os)
{
    os_seek(os, 0);
    os->file.rf->len = 0;
}

static void ramo_close_i(OutStream *os)
{
    RAMFile *rf = os->file.rf;
    DEREF(rf);
    rf_close(rf);
}

void ramo_write_to(OutStream *os, OutStream *other_o)
{
    int i, len;
    RAMFile *rf = os->file.rf;
    int last_buffer_number;
    int last_buffer_offset;

    os_flush(os);
    last_buffer_number = (int) (rf->len / BUFFER_SIZE);
    last_buffer_offset = rf->len % BUFFER_SIZE;
    for (i = 0; i <= last_buffer_number; i++) {
        len = (i == last_buffer_number ? last_buffer_offset : BUFFER_SIZE);
        os_write_bytes(other_o, rf->buffers[i], len);
    }
}

const struct OutStreamMethods RAM_OUT_STREAM_METHODS = {
    ramo_flush_i,
    ramo_seek_i,
    ramo_close_i
};

OutStream *ram_new_buffer()
{
    RAMFile *rf = rf_new("");
    OutStream *os = os_new();

    DEREF(rf);
    os->file.rf = rf;
    os->pointer = 0;
    os->m = &RAM_OUT_STREAM_METHODS;
    return os;
}

void ram_destroy_buffer(OutStream *os)
{
    rf_close(os->file.rf);
    free(os);
}

static OutStream *ram_new_output(Store *store, const char *filename)
{
    RAMFile *rf = (RAMFile *)h_get(store->dir.ht, filename);
    OutStream *os = os_new();

    if (rf == NULL) {
        rf = rf_new(filename);
        h_set(store->dir.ht, rf->name, rf);
    }
    REF(rf);
    os->pointer = 0;
    os->file.rf = rf;
    os->m = &RAM_OUT_STREAM_METHODS;
    return os;
}

static void rami_read_i(InStream *is, uchar *b, int len)
{
    RAMFile *rf = is->file.rf;

    int offset = 0;
    int buffer_number, buffer_offset, bytes_in_buffer, bytes_to_copy;
    int remainder = len;
    off_t start = is->d.pointer;
    uchar *buffer;

    while (remainder > 0) {
        buffer_number = (int) (start / BUFFER_SIZE);
        buffer_offset = start % BUFFER_SIZE;
        bytes_in_buffer = BUFFER_SIZE - buffer_offset;

        if (bytes_in_buffer >= remainder) {
            bytes_to_copy = remainder;
        }
        else {
            bytes_to_copy = bytes_in_buffer;
        }
        buffer = rf->buffers[buffer_number];
        memcpy(b + offset, buffer + buffer_offset, bytes_to_copy);
        offset += bytes_to_copy;
        start += bytes_to_copy;
        remainder -= bytes_to_copy;
    }

    is->d.pointer += len;
}

static off_t rami_length_i(InStream *is)
{
    return is->file.rf->len;
}

static void rami_seek_i(InStream *is, off_t pos)
{
    is->d.pointer = pos;
}

static void rami_close_i(InStream *is)
{
    RAMFile *rf = is->file.rf;
    DEREF(rf);
    rf_close(rf);
}

static const struct InStreamMethods RAM_IN_STREAM_METHODS = {
    rami_read_i,
    rami_seek_i,
    rami_length_i,
    rami_close_i
};

static InStream *ram_open_input(Store *store, const char *filename)
{
    RAMFile *rf = (RAMFile *)h_get(store->dir.ht, filename);
    InStream *is = NULL;

    if (rf == NULL) {
        RAISE(FILE_NOT_FOUND_ERROR,
              "tried to open \"%s\" but it doesn't exist", filename);
    }
    REF(rf);
    is = is_new();
    is->file.rf = rf;
    is->d.pointer = 0;
    is->m = &RAM_IN_STREAM_METHODS;

    return is;
}

#define LOCK_OBTAIN_TIMEOUT 5

static int ram_lock_obtain(Lock *lock)
{
    int ret = true;
    if (ram_exists(lock->store, lock->name))
        ret = false;
    ram_touch(lock->store, lock->name);
    return ret;
}

static int ram_lock_is_locked(Lock *lock)
{
    return ram_exists(lock->store, lock->name);
}

static void ram_lock_release(Lock *lock)
{
    ram_remove(lock->store, lock->name);
}

static Lock *ram_open_lock_i(Store *store, char *lockname)
{
    Lock *lock = ALLOC(Lock);
    char lname[100];
    snprintf(lname, 100, "%s%s.lck", LOCK_PREFIX, lockname);
    lock->name = estrdup(lname);
    lock->store = store;
    lock->obtain = &ram_lock_obtain;
    lock->release = &ram_lock_release;
    lock->is_locked = &ram_lock_is_locked;
    return lock;
}

static void ram_close_lock_i(Lock *lock)
{
    free(lock->name);
    free(lock);
}


Store *open_ram_store()
{
    Store *new_store = store_new();

    new_store->dir.ht       = h_new_str(NULL, rf_close);
    new_store->touch        = &ram_touch;
    new_store->exists       = &ram_exists;
    new_store->remove       = &ram_remove;
    new_store->rename       = &ram_rename;
    new_store->count        = &ram_count;
    new_store->clear        = &ram_clear;
    new_store->clear_all    = &ram_clear_all;
    new_store->clear_locks  = &ram_clear_locks;
    new_store->length       = &ram_length;
    new_store->each         = &ram_each;
    new_store->new_output   = &ram_new_output;
    new_store->open_input   = &ram_open_input;
    new_store->open_lock_i  = &ram_open_lock_i;
    new_store->close_lock_i = &ram_close_lock_i;
    new_store->close_i      = &ram_close_i;
    return new_store;
}

struct CopyFileArg
{
    Store *to_store, *from_store;
};

static void copy_files(char *fname, void *arg)
{
    struct CopyFileArg *cfa = (struct CopyFileArg *)arg;
    OutStream *os = cfa->to_store->new_output(cfa->to_store, fname);
    InStream *is = cfa->from_store->open_input(cfa->from_store, fname);
    int len = (int)is_length(is);
    uchar *buffer = ALLOC_N(uchar, len + 1);

    is_read_bytes(is, buffer, len);
    os_write_bytes(os, buffer, len);

    is_close(is);
    os_close(os);
    free(buffer);
}

Store *open_ram_store_and_copy(Store *from_store, bool close_dir)
{
    Store *store = open_ram_store();
    struct CopyFileArg cfa;
    cfa.to_store = store;
    cfa.from_store = from_store;

    from_store->each(from_store, &copy_files, &cfa);

    if (close_dir) {
        store_deref(from_store);
    }

    return store;
}
