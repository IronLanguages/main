#include "store.h"
#include <time.h>
#include <sys/types.h>
#include <fcntl.h>
#include <sys/stat.h>
#include <errno.h>
#include <string.h>
#include <stdio.h>
#ifdef POSH_OS_WIN32
# include <io.h>
# include "win32.h"
# ifndef sleep
#   define sleep _sleep
# endif
# ifndef DIR_SEPARATOR
#   define DIR_SEPARATOR "\\"
# endif
# ifndef S_IRUSR
#   define S_IRUSR _S_IREAD
# endif
# ifndef S_IWUSR
#   define S_IWUSR _S_IWRITE
# endif
#else
# define DIR_SEPARATOR "/"
# include <unistd.h>
# include <dirent.h>
#endif
#ifndef O_BINARY
# define O_BINARY 0
#endif

extern Store *store_new();
extern void store_destroy(Store *store);
extern OutStream *os_new();
extern InStream *is_new();
extern int file_is_lock(char *filename);

/**
 * Create a filepath for a file in the store using the operating systems
 * default file seperator.
 */
static char *join_path(char *buf, const char *base, const char *filename)
{
  snprintf(buf, MAX_FILE_PATH, "%s"DIR_SEPARATOR"%s", base, filename);
  return buf;
}

static void fs_touch(Store *store, char *filename)
{
    int f;
    char path[MAX_FILE_PATH];
    join_path(path, store->dir.path, filename);
    if ((f = creat(path, store->file_mode)) == 0) {
        RAISE(IO_ERROR, "couldn't create file %s: <%s>", path,
              strerror(errno));
    }
    close(f);
}

static int fs_exists(Store *store, char *filename)
{
    int fd;
    char path[MAX_FILE_PATH];
    join_path(path, store->dir.path, filename);
    fd = open(path, 0);
    if (fd < 0) {
        if (errno != ENOENT) {
            RAISE(IO_ERROR, "checking existance of %s: <%s>", path,
                  strerror(errno));
        }
        return false;
    }
    close(fd);
    return true;
}

static int fs_remove(Store *store, char *filename)
{
    char path[MAX_FILE_PATH];
    return remove(join_path(path, store->dir.path, filename));
}

static void fs_rename(Store *store, char *from, char *to)
{
    char path1[MAX_FILE_PATH], path2[MAX_FILE_PATH];

#ifdef POSH_OS_WIN32
    remove(join_path(path1, store->dir.path, to));
#endif

    if (rename(join_path(path1, store->dir.path, from),
               join_path(path2, store->dir.path, to)) < 0) {
        RAISE(IO_ERROR, "couldn't rename file \"%s\" to \"%s\": <%s>",
              path1, path2, strerror(errno));
    }
}

static int fs_count(Store *store)
{
    int cnt = 0;
    struct dirent *de;
    DIR *d = opendir(store->dir.path);

    if (!d) {
        RAISE(IO_ERROR, "counting files in %s: <%s>",
              store->dir.path, strerror(errno));
    }

    while ((de = readdir(d)) != NULL) {
        if (de->d_name[0] > '/') { /* skip ., .., / and '\0'*/
            cnt++;
        }
    }
    closedir(d);

    return cnt;
}

static void fs_each(Store *store, void (*func)(char *fname, void *arg), void *arg)
{
    struct dirent *de;
    DIR *d = opendir(store->dir.path);

    if (!d) {
        RAISE(IO_ERROR, "doing 'each' in %s: <%s>",
              store->dir.path, strerror(errno));
    }

    while ((de = readdir(d)) != NULL) {
        if (de->d_name[0] > '/' /* skip ., .., / and '\0'*/
                && !file_is_lock(de->d_name)) {
            func(de->d_name, arg);
        }
    }
    closedir(d);
}

static void fs_clear_locks(Store *store)
{
    struct dirent *de;
    DIR *d = opendir(store->dir.path);

    if (!d) {
        RAISE(IO_ERROR, "clearing locks in %s: <%s>",
              store->dir.path, strerror(errno));
    }

    while ((de = readdir(d)) != NULL) {
        if (file_is_lock(de->d_name)) {
            char path[MAX_FILE_PATH];
            remove(join_path(path, store->dir.path, de->d_name));
        }
    }
    closedir(d);
}

static void fs_clear(Store *store)
{
    struct dirent *de;
    DIR *d = opendir(store->dir.path);

    if (!d) {
        RAISE(IO_ERROR, "clearing files in %s: <%s>",
              store->dir.path, strerror(errno));
    }

    while ((de = readdir(d)) != NULL) {
        if (de->d_name[0] > '/' /* skip ., .., / and '\0'*/
                && !file_is_lock(de->d_name)) {
            char path[MAX_FILE_PATH];
            remove(join_path(path, store->dir.path, de->d_name));
        }
    }
    closedir(d);
}

static void fs_clear_all(Store *store)
{
    struct dirent *de;
    DIR *d = opendir(store->dir.path);

    if (!d) {
        RAISE(IO_ERROR, "clearing all files in %s: <%s>",
              store->dir.path, strerror(errno));
    }

    while ((de = readdir(d)) != NULL) {
        if (de->d_name[0] > '/') { /* skip ., .., / and '\0'*/
            char path[MAX_FILE_PATH];
            remove(join_path(path, store->dir.path, de->d_name));
        }
    }
    closedir(d);
}

/**
 * Destroy the store.
 *
 * @param p the store to destroy
 * @raise IO_ERROR if there is an error deleting the locks
 */
static void fs_destroy(Store *store)
{
    TRY
        fs_clear_locks(store);
    XCATCHALL
        HANDLED();
    XENDTRY
    free(store->dir.path);
    store_destroy(store);
}

static off_t fs_length(Store *store, char *filename)
{
    char path[MAX_FILE_PATH];
    struct stat stt;

    if (stat(join_path(path, store->dir.path, filename), &stt)) {
        RAISE(IO_ERROR, "getting lenth of %s: <%s>", path,
              strerror(errno));
    }

    return stt.st_size;
}

static void fso_flush_i(OutStream *os, uchar *src, int len)
{
    if (len != write(os->file.fd, src, len)) {
        RAISE(IO_ERROR, "flushing src of length %d, <%s>", len,
              strerror(errno));
    }
}

static void fso_seek_i(OutStream *os, off_t pos)
{
    if (lseek(os->file.fd, pos, SEEK_SET) < 0) {
        RAISE(IO_ERROR, "seeking position %"F_OFF_T_PFX"d: <%s>",
              pos, strerror(errno));
    }
}

static void fso_close_i(OutStream *os)
{
    if (close(os->file.fd)) {
        RAISE(IO_ERROR, "closing file: <%s>", strerror(errno));
    }
}

const struct OutStreamMethods FS_OUT_STREAM_METHODS = {
    fso_flush_i,
    fso_seek_i,
    fso_close_i
};

static OutStream *fs_new_output(Store *store, const char *filename)
{
    char path[MAX_FILE_PATH];
    int fd = open(join_path(path, store->dir.path, filename),
                  O_WRONLY | O_CREAT | O_BINARY, store->file_mode);
    OutStream *os;
    if (fd < 0) {
        RAISE(IO_ERROR, "couldn't create OutStream %s: <%s>",
              path, strerror(errno));
    }

    os = os_new();
    os->file.fd = fd;
    os->m = &FS_OUT_STREAM_METHODS;
    return os;
}

static void fsi_read_i(InStream *is, uchar *path, int len)
{
    int fd = is->file.fd;
    off_t pos = is_pos(is);
    if (pos != lseek(fd, 0, SEEK_CUR)) {
        lseek(fd, pos, SEEK_SET);
    }
    if (read(fd, path, len) != len) {
        /* win: the wrong value can be returned for some reason so double check */
        if (lseek(fd, 0, SEEK_CUR) != (pos + len)) {
            RAISE(IO_ERROR, "couldn't read %d chars from %s: <%s>",
                  len, path, strerror(errno));
        }
    }
}

static void fsi_seek_i(InStream *is, off_t pos)
{
    if (lseek(is->file.fd, pos, SEEK_SET) < 0) {
        RAISE(IO_ERROR, "seeking pos %"F_OFF_T_PFX"d: <%s>",
              pos, strerror(errno));
    }
}

static void fsi_close_i(InStream *is)
{
    if (close(is->file.fd)) {
        RAISE(IO_ERROR, strerror(errno));
    }
    free(is->d.path);
}

static off_t fsi_length_i(InStream *is)
{
    struct stat stt;
    if (fstat(is->file.fd, &stt)) {
        RAISE(IO_ERROR, "fstat failed: <%s>", strerror(errno));
    }
    return stt.st_size;
}

static const struct InStreamMethods FS_IN_STREAM_METHODS = {
    fsi_read_i,
    fsi_seek_i,
    fsi_length_i,
    fsi_close_i
};

static InStream *fs_open_input(Store *store, const char *filename)
{
    InStream *is;
    char path[MAX_FILE_PATH];
    int fd = open(join_path(path, store->dir.path, filename), O_RDONLY | O_BINARY);
    if (fd < 0) {
        RAISE(FILE_NOT_FOUND_ERROR,
              "tried to open \"%s\" but it doesn't exist: <%s>",
              path, strerror(errno));
    }
    is = is_new();
    is->file.fd = fd;
    is->d.path = estrdup(path);
    is->m = &FS_IN_STREAM_METHODS;
    return is;
}

#define LOCK_OBTAIN_TIMEOUT 10

#ifdef RUBY_BINDINGS
struct timeval rb_time_interval _((VALUE));
#endif

static int fs_lock_obtain(Lock *lock)
{
    int f;
    int trys = LOCK_OBTAIN_TIMEOUT;
    while (((f =
             open(lock->name, O_CREAT | O_EXCL | O_RDWR,
                   S_IRUSR | S_IWUSR)) < 0) && (trys > 0)) {

        /* sleep for 10 milliseconds */
        micro_sleep(10000);
        trys--;
    }
    if (f >= 0) {
        close(f);
        return true;
    }
    else {
        return false;
    }
}

static int fs_lock_is_locked(Lock *lock)
{
    int f = open(lock->name, O_CREAT | O_EXCL | O_WRONLY, S_IRUSR | S_IWUSR);
    if (f >= 0) {
        if (close(f) || remove(lock->name)) {
            RAISE(IO_ERROR, "couldn't close lock \"%s\": <%s>", lock->name,
                  strerror(errno));
        }
        return false;
    }
    else {
        return true;
    }
}

void fs_lock_release(Lock *lock)
{
    remove(lock->name);
}

static Lock *fs_open_lock_i(Store *store, char *lockname)
{
    Lock *lock = ALLOC(Lock);
    char lname[100];
    char path[MAX_FILE_PATH];
    snprintf(lname, 100, "%s%s.lck", LOCK_PREFIX, lockname);
    lock->name = estrdup(join_path(path, store->dir.path, lname));
    lock->store = store;
    lock->obtain = &fs_lock_obtain;
    lock->release = &fs_lock_release;
    lock->is_locked = &fs_lock_is_locked;
    return lock;
}

static void fs_close_lock_i(Lock *lock)
{
    remove(lock->name);
    free(lock->name);
    free(lock);
}

static HashTable stores = {
    /* fill */       0,
    /* used */       0,
    /* mask */       HASH_MINSIZE - 1,
    /* ref_cnt */    1,
    /* table */      stores.smalltable,
    /* smalltable */ {{0, NULL, NULL}},
    /* lookup */     (h_lookup_ft)&h_lookup_str,
    /* hash */       NULL,
    /* eq */         NULL,
    /* free_key */   (free_ft)&dummy_free,
    /* free_value */ (free_ft)&fs_destroy
};

#ifndef UNTHREADED
static mutex_t stores_mutex = MUTEX_INITIALIZER;
#endif

static void fs_close_i(Store *store)
{
    mutex_lock(&stores_mutex);
    h_del(&stores, store->dir.path);
    mutex_unlock(&stores_mutex);
}

static Store *fs_store_new(const char *pathname)
{
    struct stat stt;
    Store *new_store = store_new();

    new_store->file_mode = S_IRUSR | S_IWUSR;
#ifndef POSH_OS_WIN32
    if (!stat(pathname, &stt) && stt.st_gid == getgid()) {
        if (stt.st_mode & S_IWGRP) {
            umask(S_IWOTH);
        }
        new_store->file_mode |= stt.st_mode & (S_IRGRP | S_IWGRP);
    }
#endif

    new_store->dir.path      = estrdup(pathname);
    new_store->touch         = &fs_touch;
    new_store->exists        = &fs_exists;
    new_store->remove        = &fs_remove;
    new_store->rename        = &fs_rename;
    new_store->count         = &fs_count;
    new_store->close_i       = &fs_close_i;
    new_store->clear         = &fs_clear;
    new_store->clear_all     = &fs_clear_all;
    new_store->clear_locks   = &fs_clear_locks;
    new_store->length        = &fs_length;
    new_store->each          = &fs_each;
    new_store->new_output    = &fs_new_output;
    new_store->open_input    = &fs_open_input;
    new_store->open_lock_i   = &fs_open_lock_i;
    new_store->close_lock_i  = &fs_close_lock_i;
    return new_store;
}

Store *open_fs_store(const char *pathname)
{
    Store *store = NULL;

    mutex_lock(&stores_mutex);
    store = h_get(&stores, pathname);
    if (store) {
        mutex_lock(&store->mutex);
        store->ref_cnt++;
        mutex_unlock(&store->mutex);
    }
    else {
        store = fs_store_new(pathname);
        h_set(&stores, store->dir.path, store);
    }
    mutex_unlock(&stores_mutex);

    return store;
}
