#ifndef __FERRET_H_
#define __FERRET_H_

#include "global.h"
#include "hashset.h"
#include "document.h"

/* IDs */
extern ID id_new;
extern ID id_call;
extern ID id_hash;
extern ID id_eql;
extern ID id_capacity;
extern ID id_less_than;
extern ID id_lt;
extern ID id_is_directory;
extern ID id_close;
extern ID id_cclass;
extern ID id_data;

/* Symbols */
extern VALUE sym_yes;
extern VALUE sym_no;
extern VALUE sym_true;
extern VALUE sym_false;
extern VALUE sym_path;
extern VALUE sym_dir;

/* Modules */
extern VALUE mFerret;
extern VALUE mIndex;
extern VALUE mSearch;
extern VALUE mStore;
extern VALUE mStringHelper;
extern VALUE mSpans;

/* Classes */
extern VALUE cDirectory;
extern VALUE cLockError;
extern VALUE cTerm;

/* Ferret Inits */
extern void Init_Utils();
extern void Init_Analysis();
extern void Init_Store();
extern void Init_Index();
extern void Init_Search();
extern void Init_QueryParser();

//extern void object_add(void *key, VALUE obj);
#define object_add(key, obj) object_add2(key, obj,  __FILE__, __LINE__)
extern void object_add2(void *key, VALUE obj, const char *file, int line);
//extern void object_set(void *key, VALUE obj);
#define object_set(key, obj) object_set2(key, obj,  __FILE__, __LINE__)
extern void object_set2(void *key, VALUE obj, const char *file, int line);
//extern void object_del(void *key);
#define object_del(key) object_del2(key,  __FILE__, __LINE__)
extern void object_del2(void *key, const char *file, int line);
extern void frt_gc_mark(void *key);
extern VALUE object_get(void *key);
extern VALUE frt_data_alloc(VALUE klass);
extern void frt_deref_free(void *p);
extern void frt_create_dir(VALUE rpath);
extern VALUE frt_hs_to_rb_ary(HashSet *hs);
extern void *frt_rb_data_ptr(VALUE val);
extern char * frt_field(VALUE rfield);
extern VALUE frt_get_term(const char *field, const char *term);
extern char *json_concat_string(char *s, char *field);
extern char *rs2s(VALUE rstr);
extern char *nstrdup(VALUE rstr);
#define Frt_Make_Struct(klass)\
  rb_data_object_alloc(klass,NULL,(RUBY_DATA_FUNC)NULL,(RUBY_DATA_FUNC)NULL)

#define Frt_Wrap_Struct(self,mmark,mfree,mdata)\
  do {\
    ((struct RData *)(self))->data = mdata;\
    ((struct RData *)(self))->dmark = (RUBY_DATA_FUNC)mmark;\
    ((struct RData *)(self))->dfree = (RUBY_DATA_FUNC)mfree;\
  } while (0)

#define Frt_Unwrap_Struct(self)\
  do {\
    ((struct RData *)(self))->data = NULL;\
    ((struct RData *)(self))->dmark = NULL;\
    ((struct RData *)(self))->dfree = NULL;\
  } while (0)

#endif

#define frt_mark_cclass(klass) rb_ivar_set(klass, id_cclass, Qtrue)
#define frt_is_cclass(obj) (rb_ivar_get(CLASS_OF(obj), id_cclass) == Qtrue)
