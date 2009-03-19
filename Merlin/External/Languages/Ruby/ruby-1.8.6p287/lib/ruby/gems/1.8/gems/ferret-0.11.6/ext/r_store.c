#include "ferret.h"
#include "store.h"

static ID id_ref_cnt;
VALUE cLock;
VALUE cLockError;
VALUE cDirectory;
VALUE cRAMDirectory;
VALUE cFSDirectory;

/****************************************************************************
 *
 * Lock Methods
 *
 ****************************************************************************/

void
frt_unwrap_locks(Store *store)
{
    int i;
    for (i = 0; i < store->locks->size; i++) {
        void *lock = store->locks->elems[i];
        VALUE rlock = object_get(lock);
        if (rlock != Qnil) {
            object_del(lock);
            Frt_Unwrap_Struct(rlock);
        }
    }
}

void
frt_lock_free(void *p)
{
    Lock *lock = (Lock *)p;
    object_del(p);
    close_lock(lock);
}

void
frt_lock_mark(void *p)
{
    Lock *lock = (Lock *)p;
    frt_gc_mark(lock->store);
}

#define GET_LOCK(lock, self) Data_Get_Struct(self, Lock, lock)

/*
 *  call-seq:
 *     lock.obtain(timeout = 1) -> bool
 *
 *  Obtain a lock. Returns true if lock was successfully obtained. Make sure
 *  the lock is released using Lock#release. Otherwise you'll be left with a
 *  stale lock file.
 *
 *  The timeout defaults to 1 second and 5 attempts are made to obtain the
 *  lock. If you're doing large batch updates on the index with multiple
 *  processes you may need to increase the lock timeout but 1 second will be
 *  substantial in most cases.
 *
 *  timeout:: seconds to wait to obtain lock before timing out and returning
 *            false
 *  return::  true if lock was successfully obtained. Raises a
 *            Lock::LockError otherwise.
 */
static VALUE
frt_lock_obtain(int argc, VALUE *argv, VALUE self)
{
    VALUE rtimeout;
    int timeout = 1;
    Lock *lock;
    GET_LOCK(lock, self);

    if (rb_scan_args(argc, argv, "01", &rtimeout) > 0) {
        timeout = FIX2INT(rtimeout);
    }
    /* TODO: use the lock timeout */
    if (!lock->obtain(lock)) {
        rb_raise(cLockError, "could not obtain lock: #%s", lock->name);
    }
    return Qtrue;
}

/*
 *  call-seq:
 *     lock.while_locked(timeout = 1) { do_something() } -> bool
 *
 *  Run the code in a block while a lock is obtained, automatically releasing
 *  the lock when the block returns.
 *
 *  See Lock#obtain for more information on lock timeout.
 *
 *  timeout:: seconds to wait to obtain lock before timing out and returning
 *            false
 *  return::  true if lock was successfully obtained. Raises a
 *            Lock::LockError otherwise.
 */
static VALUE
frt_lock_while_locked(int argc, VALUE *argv, VALUE self)
{
    VALUE rtimeout;
    int timeout = 1;
    Lock *lock;
    GET_LOCK(lock, self);
    if (rb_scan_args(argc, argv, "01", &rtimeout) > 0) {
        timeout = FIX2INT(rtimeout);
    }
    if (!lock->obtain(lock)) {
        rb_raise(cLockError, "could not obtain lock: #%s", lock->name);
    }
    rb_yield(Qnil);
    lock->release(lock);
    return Qtrue;
}

/*
 *  call-seq:
 *     lock.locked? -> bool
 *
 *  Returns true if the lock has been obtained.
 */
static VALUE
frt_lock_is_locked(VALUE self)
{
    Lock *lock;
    GET_LOCK(lock, self);
    return lock->is_locked(lock) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     lock.release() -> self
 *
 *  Release the lock. This should only be called by the process which obtains
 *  the lock.
 */
static VALUE
frt_lock_release(VALUE self)
{
    Lock *lock;
    GET_LOCK(lock, self);
    lock->release(lock);
    return self;
}

/****************************************************************************
 *
 * Directory Methods
 *
 ****************************************************************************/

void
frt_dir_free(Store *store)
{
    frt_unwrap_locks(store);
    object_del(store);
    store_deref(store);
}

/*
 *  call-seq:
 *     dir.close() -> nil
 *
 *  It is a good idea to close a directory when you have finished using it.
 *  Although the garbage collector will currently handle this for you, this
 *  behaviour may change in future.
 */
static VALUE
frt_dir_close(VALUE self)
{
    Store *store = DATA_PTR(self);
    int ref_cnt = FIX2INT(rb_ivar_get(self, id_ref_cnt)) - 1;
    rb_ivar_set(self, id_ref_cnt, INT2FIX(ref_cnt));
    if (ref_cnt < 0) {
        Frt_Unwrap_Struct(self);
        object_del(store);
        frt_unwrap_locks(store);
        store_deref(store);
    }
    return Qnil;
}

/*
 *  call-seq:
 *     dir.exists?(file_name) -> nil
 *
 *  Return true if a file with the name +file_name+ exists in the directory.
 */
static VALUE
frt_dir_exists(VALUE self, VALUE rfname)
{
    Store *store = DATA_PTR(self);
    StringValue(rfname);
    return store->exists(store, rs2s(rfname)) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     dir.touch(file_name) -> nil
 *
 *  Create an empty file in the directory with the name +file_name+.
 */
static VALUE
frt_dir_touch(VALUE self, VALUE rfname)
{
    Store *store = DATA_PTR(self);
    StringValue(rfname);
    store->touch(store, rs2s(rfname));
    return Qnil;
}

/*
 *  call-seq:
 *     dir.delete(file_name) -> nil
 *
 *  Remove file +file_name+ from the directory. Returns true if successful.
 */
static VALUE
frt_dir_delete(VALUE self, VALUE rfname)
{
    Store *store = DATA_PTR(self);
    StringValue(rfname);
    return (store->remove(store, rs2s(rfname)) == 0) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     dir.count -> integer
 *
 *  Return a count of the number of files in the directory.
 */
static VALUE
frt_dir_file_count(VALUE self)
{
    Store *store = DATA_PTR(self);
    return INT2FIX(store->count(store));
}

/*
 *  call-seq:
 *     dir.refresh -> self
 *
 *  Delete all files in the directory. It gives you a clean slate.
 */
static VALUE
frt_dir_refresh(VALUE self)
{
    Store *store = DATA_PTR(self);
    store->clear_all(store);
    return self;
}

/*
 *  call-seq:
 *     dir.rename(from, to) -> self
 *
 *  Rename a file from +from+ to +to+. An error will be raised if the file
 *  doesn't exist or there is some other type of IOError.
 */
static VALUE
frt_dir_rename(VALUE self, VALUE rfrom, VALUE rto)
{
    Store *store = DATA_PTR(self);
    StringValue(rfrom);
    StringValue(rto);
    store->rename(store, rs2s(rfrom), rs2s(rto));
    return self;
}

/*
 *  call-seq:
 *     dir.make_lock(lock_name) -> self
 *
 *  Make a lock with the name +lock_name+. Note that lockfiles will be stored
 *  in the directory with other files but they won't be visible to you. You
 *  should avoid using files with a .lck extension as this extension is
 *  reserved for lock files
 */
static VALUE
frt_dir_make_lock(VALUE self, VALUE rlock_name)
{
    VALUE rlock;
    Lock *lock;
    Store *store = DATA_PTR(self);
    StringValue(rlock_name);
    lock = open_lock(store, rs2s(rlock_name));
    rlock = Data_Wrap_Struct(cLock, &frt_lock_mark, &frt_lock_free, lock);
    object_add(lock, rlock);
    return rlock;
}

/****************************************************************************
 *
 * RAMDirectory Methods
 *
 ****************************************************************************/

/*
 *  call-seq:
 *     RAMDirectory.new(dir = nil)
 *
 *  Create a new RAMDirectory.
 *
 *  You can optionally load another Directory (usually a FSDirectory) into
 *  memory. This may be useful to speed up search performance but usually the
 *  speedup won't be worth the trouble. Be sure to benchmark.
 *
 *  dir:: Directory to load into memory
 */
static VALUE
frt_ramdir_init(int argc, VALUE *argv, VALUE self) 
{
    VALUE rdir;
    Store *store;
    switch (rb_scan_args(argc, argv, "01", &rdir)) {
        case 1: {
                    Store *ostore;
                    Data_Get_Struct(rdir, Store, ostore);
                    store = open_ram_store_and_copy(ostore, false);
                    break;
                }
        default: store = open_ram_store();
    }
    Frt_Wrap_Struct(self, NULL, &frt_dir_free, store);
    object_add(store, self);
    rb_ivar_set(self, id_ref_cnt, INT2FIX(0));
    return self;
}

/****************************************************************************
 *
 * FSDirectory Methods
 *
 ****************************************************************************/

/*
 *  call-seq:
 *     FSDirectory.new(/path/to/index/, create = false)
 *
 *  Create a new FSDirectory at +/path/to/index/+ which must be a valid path
 *  on your file system. If it doesn't exist it will be created. You can also
 *  specify the +create+ parameter. If +create+ is true the FSDirectory will
 *  be refreshed as new. That is to say, any existing files in the directory
 *  will be deleted. The default value for +create+ is false.
 *
 *  path::   path to index directory. Must be a valid path on your system
 *  create:: set to true if you want any existing files in the directory to be
 *           deleted
 */
static VALUE
frt_fsdir_new(int argc, VALUE *argv, VALUE klass) 
{
    VALUE self, rpath, rcreate;
    Store *store;
    bool create;

    rb_scan_args(argc, argv, "11", &rpath, &rcreate);
    StringValue(rpath);
    create = RTEST(rcreate);
    if (create) {
        frt_create_dir(rpath);
    }
    if (!rb_funcall(rb_cFile, id_is_directory, 1, rpath)) {
        rb_raise(rb_eIOError, "No directory <%s> found. Use :create => true"
                 " to create one.", rs2s(rpath));
    }
    store = open_fs_store(rs2s(rpath));
    if (create) store->clear_all(store);
    if ((self = object_get(store)) == Qnil) {
        self = Data_Wrap_Struct(klass, NULL, &frt_dir_free, store);
        object_add(store, self);
        rb_ivar_set(self, id_ref_cnt, INT2FIX(0));
    }
    else {
        int ref_cnt = FIX2INT(rb_ivar_get(self, id_ref_cnt)) + 1;
        rb_ivar_set(self, id_ref_cnt, INT2FIX(ref_cnt));
        DEREF(store);
    }
    return self;
}

/****************************************************************************
 *
 * Init Function
 *
 ****************************************************************************/

/*
 *  Document-class: Ferret::Store::Directory
 *
 *  A Directory is an object which is used to access the index storage.
 *  Ruby's IO API is not used so that we can use different storage
 *  mechanisms to store the index. Some examples are;
 *  
 *  * File system based storage (currently implemented as FSDirectory)
 *  * RAM based storage (currently implemented as RAMDirectory)
 *  * Database based storage
 * 
 *  NOTE: Once a file has been written and closed, it can no longer be
 *  modified. To make any changes to the file it must be deleted and
 *  rewritten. For this reason, the method to open a file for writing is
 *  called _create_output_, while the method to open a file for reading is
 *  called _open_input_ If there is a risk of simultaneous modifications of
 *  the files then locks should be used. See Lock to find out how.
 */
void
Init_Directory(void)
{
    cDirectory = rb_define_class_under(mStore, "Directory", rb_cObject);
    rb_define_const(cDirectory, "LOCK_PREFIX", rb_str_new2(LOCK_PREFIX));
    rb_define_method(cDirectory, "close", frt_dir_close, 0);
    rb_define_method(cDirectory, "exists?", frt_dir_exists, 1);
    rb_define_method(cDirectory, "touch", frt_dir_touch, 1);
    rb_define_method(cDirectory, "delete", frt_dir_delete, 1);
    rb_define_method(cDirectory, "file_count", frt_dir_file_count, 0);
    rb_define_method(cDirectory, "refresh", frt_dir_refresh, 0);
    rb_define_method(cDirectory, "rename", frt_dir_rename, 2);
    rb_define_method(cDirectory, "make_lock", frt_dir_make_lock, 1);
}

/*
 *  Document-class: Ferret::Store::Lock
 *
 *  A Lock is used to lock a data source so that not more than one 
 *  output stream can access a data source at one time. It is possible
 *  that locks could be disabled. For example a read only index stored
 *  on a CDROM would have no need for a lock.
 * 
 *  You can use a lock in two ways. Firstly:
 * 
 *    write_lock = @directory.make_lock(LOCK_NAME)
 *    write_lock.obtain(WRITE_LOCK_TIME_OUT)
 *      ... # Do your file modifications # ...
 *    write_lock.release()
 * 
 *  Alternatively you could use the while locked method. This ensures that
 *  the lock will be released once processing has finished.
 * 
 *    write_lock = @directory.make_lock(LOCK_NAME)
 *    write_lock.while_locked(WRITE_LOCK_TIME_OUT) do
 *      ... # Do your file modifications # ...
 *    end
 */
void
Init_Lock(void)
{
    cLock = rb_define_class_under(mStore, "Lock", rb_cObject);
    rb_define_method(cLock, "obtain", frt_lock_obtain, -1);
    rb_define_method(cLock, "while_locked", frt_lock_while_locked, -1);
    rb_define_method(cLock, "release", frt_lock_release, 0);
    rb_define_method(cLock, "locked?", frt_lock_is_locked, 0);

    cLockError = rb_define_class_under(cLock, "LockError", rb_eStandardError);
}

/*
 *  Document-class: Ferret::Store::RAMDirectory
 *
 *  Memory resident Directory implementation. You should use a RAMDirectory
 *  during testing but otherwise you should stick with FSDirectory. While
 *  loading an index into memory may slightly speed things up, on most
 *  operating systems there won't be much difference so it wouldn't be worth
 *  your trouble.
 */
void
Init_RAMDirectory(void)
{
    cRAMDirectory = rb_define_class_under(mStore, "RAMDirectory", cDirectory);
    rb_define_alloc_func(cRAMDirectory, frt_data_alloc);
    rb_define_method(cRAMDirectory, "initialize", frt_ramdir_init, -1);
}

/*
 *  Document-class: Ferret::Store::RAMDirectory
 *
 *  File-system resident Directory implementation. The FSDirectory will use a
 *  single directory to store all of it's files. You should not otherwise
 *  touch this directory. Modifying the files in the directory will corrupt
 *  the index. The one exception to this rule is you may need to delete stale
 *  lock files which have a ".lck" extension.
 */
void
Init_FSDirectory(void)
{
    cFSDirectory = rb_define_class_under(mStore, "FSDirectory", cDirectory);
    rb_define_alloc_func(cFSDirectory, frt_data_alloc);
    rb_define_singleton_method(cFSDirectory, "new", frt_fsdir_new, -1);
}

/* rdoc hack
extern VALUE mFerret = rb_define_module("Ferret");
*/

/*
 *  Document-module: Ferret::Store
 *
 *  The Store module contains all the classes required to handle the storing
 *  of an index. 
 *
 *  NOTE: You can currently store an index on a file-system or in memory. If
 *  you want to add a different type of Directory, like a database Directory
 *  for instance, you will to implement it in C.
 */
void
Init_Store(void)
{
    id_ref_cnt = rb_intern("@id_ref_cnt");
    mStore = rb_define_module_under(mFerret, "Store");
    Init_Directory();
    Init_Lock();
    Init_RAMDirectory();
    Init_FSDirectory();
}
