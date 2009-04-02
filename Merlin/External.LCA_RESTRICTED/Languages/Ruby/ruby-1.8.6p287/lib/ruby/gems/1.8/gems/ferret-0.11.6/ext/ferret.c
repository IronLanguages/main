#include <errno.h>
#include "ferret.h"
#include "except.h"
#include "hash.h"
#include "hashset.h"
#include "threading.h"

/* Object Map */
static HashTable *object_map;

/* IDs */
ID id_new;
ID id_call;
ID id_eql;
ID id_hash;
ID id_capacity;
ID id_less_than;
ID id_lt;
ID id_is_directory;
ID id_close;
ID id_cclass;
ID id_data;

static ID id_mkdir_p;

/* Symbols */
VALUE sym_yes;
VALUE sym_no;
VALUE sym_true;
VALUE sym_false;
VALUE sym_path;
VALUE sym_dir;

/* Modules */
VALUE mFerret;
VALUE mStore;
VALUE mStringHelper;
VALUE mSpans;

/* Classes */
VALUE cTerm;
/*
*/

unsigned long value_hash(const void *key)
{
    return (unsigned long)key;
}

int value_eq(const void *key1, const void *key2)
{
    return key1 == key2;
}

VALUE object_get(void *key)
{
    VALUE val = (VALUE)h_get(object_map, key);
    if (!val) val = Qnil;
    return val;
}

//static int hash_cnt = 0;
void
//object_add(void *key, VALUE obj)
object_add2(void *key, VALUE obj, const char *file, int line)
{
    if (h_get(object_map, key))
        printf("failed adding %lx to %ld; already contains %lx. %s:%d\n",
               (long)obj, (long)key, (long)h_get(object_map, key), file, line);
    //printf("adding %ld. now contains %d %s:%d\n", (long)key, ++hash_cnt, file, line);
    h_set(object_map, key, (void *)obj);
}

void
//object_set(void *key, VALUE obj)
object_set2(void *key, VALUE obj, const char *file, int line)
{
    //if (!h_get(object_map, key))
      //printf("adding %ld. now contains %d %s:%d\n", (long)key, ++hash_cnt, file, line);
    h_set(object_map, key, (void *)obj);
}

void
//object_del(void *key)
object_del2(void *key, const char *file, int line)
{
    if (object_get(key) == Qnil) 
        printf("failed deleting %ld. %s:%d\n", (long)key, file, line);
    //printf("deleting %ld. now contains %ld, %s:%d\n", (long)key, --hash_cnt, file, line);
    h_del(object_map, key);
}

void frt_gc_mark(void *key)
{
    VALUE val = (VALUE)h_get(object_map, key);
    if (val)
        rb_gc_mark(val);
}

VALUE frt_data_alloc(VALUE klass)
{
    return Frt_Make_Struct(klass);
}

void frt_deref_free(void *p)
{
    object_del(p);
}

void frt_thread_once(int *once_control, void (*init_routine) (void))
{
    if (*once_control) {
        init_routine();
        *once_control = 0;
    }
}

void frt_thread_key_create(thread_key_t *key, void (*destr_function)(void *))
{
    *key = h_new(&value_hash, &value_eq, NULL, destr_function);
}

void frt_thread_key_delete(thread_key_t key)
{
    h_destroy(key);
}

void frt_thread_setspecific(thread_key_t key, const void *pointer)
{
    h_set(key, (void *)rb_thread_current(), (void *)pointer);
}

void *frt_thread_getspecific(thread_key_t key)
{
    return h_get(key, (void *)rb_thread_current());
}

void frt_create_dir(VALUE rpath)
{
    VALUE mFileUtils;
    rb_require("fileutils");
    mFileUtils = rb_define_module("FileUtils");
    rb_funcall(mFileUtils, id_mkdir_p, 1, rpath);
}

VALUE frt_hs_to_rb_ary(HashSet *hs)
{
    int i;
    VALUE ary = rb_ary_new();
    for (i = 0; i < hs->size; i++) {
        rb_ary_push(ary, rb_str_new2(hs->elems[i]));
    }
    return ary;
}

void *frt_rb_data_ptr(VALUE val)
{
    Check_Type(val, T_DATA);
    return DATA_PTR(val);
}

char *
rs2s(VALUE rstr)
{
    return (char *)(RSTRING(rstr)->ptr ? RSTRING(rstr)->ptr : EMPTY_STRING);
}

char *
nstrdup(VALUE rstr)
{
    char *old = rs2s(rstr);
    int len = RSTRING(rstr)->len;
    char *new = ALLOC_N(char, len + 1);
    memcpy(new, old, len + 1);
    return new;
}

char *
frt_field(VALUE rfield)
{
    switch (TYPE(rfield)) {
        case T_SYMBOL:
            return rb_id2name(SYM2ID(rfield));
        case T_STRING:
            return rs2s(rfield);
        default:
            rb_raise(rb_eArgError, "field name must be a symbol");
    }
    return NULL;
}

/*
 * Json Exportation - Loading each LazyDoc and formatting them into json
 * This code is designed to get a VERY FAST json string, the goal was speed,
 * not sexiness.
 * Jeremie 'ahFeel' BORDIER
 * ahFeel@rift.Fr
 */
char *
json_concat_string(char *s, char *field)
{
    *(s++) = '"';
	while (*field) {
		if (*field == '"') {
            *(s++) = '\'';
            *(s++) = *(field++);
            *(s++) = '\'';
        }
        else {
            *(s++) = *(field++);
        }
    }
    *(s++) = '"';
    return s;
}

static VALUE error_map;

VALUE frt_get_error(const char *err_type)
{
    VALUE error_class;
    if (Qnil != (error_class = rb_hash_aref(error_map, rb_intern(err_type)))) {
        return error_class;
    }
    return rb_eStandardError;
}

#define FRT_BUF_SIZ 2046
#ifdef FRT_HAS_VARARGS
void vfrt_rb_raise(const char *file, int line_num, const char *func,
                   const char *err_type, const char *fmt, va_list args)
#else
void V_FRT_EXIT(const char *err_type, const char *fmt, va_list args)
#endif
{
    char buf[FRT_BUF_SIZ];
    size_t so_far = 0;
#ifdef FRT_HAS_VARARGS
    snprintf(buf, FRT_BUF_SIZ, "%s occured at <%s>:%d in %s\n",
            err_type, file, line_num, func);
#else
    snprintf(buf, FRT_BUF_SIZ, "%s occured:\n", err_type);
#endif
    so_far = strlen(buf);
    vsnprintf(buf + so_far, FRT_BUF_SIZ - so_far, fmt, args);

    so_far = strlen(buf);
    if (fmt[0] != '\0' && fmt[strlen(fmt) - 1] == ':') {
        snprintf(buf + so_far, FRT_BUF_SIZ - so_far, " %s", strerror(errno));
        so_far = strlen(buf);
    }

    snprintf(buf + so_far, FRT_BUF_SIZ - so_far, "\n");
    rb_raise(frt_get_error(err_type), buf);
}

#ifdef FRT_HAS_VARARGS
void frt_rb_raise(const char *file, int line_num, const char *func,
                  const char *err_type, const char *fmt, ...)
#else
void FRT_EXIT(const char *err_type, const char *fmt, ...)
#endif
{
    va_list args;
    va_start(args, fmt);
#ifdef FRT_HAS_VARARGS
    vfrt_rb_raise(file, line_num, func, err_type, fmt, args);
#else
    V_FRT_EXIT(err_type, fmt, args);
#endif
    va_end(args);
}

/****************************************************************************
 *
 * Term Methods
 *
 ****************************************************************************/
static ID id_field;
static ID id_text;

VALUE frt_get_term(const char *field, const char *text)
{
    return rb_struct_new(cTerm,
                         ID2SYM(rb_intern(field)),
                         rb_str_new2(text),
                         NULL);
}

static VALUE frt_term_to_s(VALUE self)
{
    VALUE rstr;
    VALUE rfield = rb_funcall(self, id_field, 0); 
    VALUE rtext = rb_funcall(self, id_text, 0); 
    char *field = StringValuePtr(rfield);
    char *text = StringValuePtr(rtext);
    char *term_str = ALLOC_N(char,
                             5 + RSTRING(rfield)->len + RSTRING(rtext)->len);
    sprintf(term_str, "%s:%s", field, text);
    rstr = rb_str_new2(term_str);
    free(term_str);
    return rstr;
}
/*
 *  Document-class: Ferret::Term
 *
 *  == Summary
 *
 *  A Term holds a term from a document and its field name (as a Symbol).
 */
void Init_Term(void)
{
    const char *term_class = "Term";
    cTerm = rb_struct_define(term_class, "field", "text", NULL);
    rb_set_class_path(cTerm, mFerret, term_class);
    rb_const_set(mFerret, rb_intern(term_class), cTerm);
    rb_define_method(cTerm, "to_s", frt_term_to_s, 0);
    id_field = rb_intern("field");
    id_text = rb_intern("text");
}

/*
 *  Document-module: Ferret
 *
 *  See the README
 */
void Init_Ferret(void)
{
    mFerret = rb_define_module("Ferret");
    Init_Term();
}

void Init_ferret_ext(void)
{
    VALUE cParseError;
    VALUE cStateError;
    VALUE cFileNotFoundError;

    /* initialize object map */
    object_map = h_new(&value_hash, &value_eq, NULL, NULL);

    /* IDs */
    id_new = rb_intern("new");
    id_call = rb_intern("call");
    id_eql = rb_intern("eql?");
    id_hash = rb_intern("hash");

    id_capacity = rb_intern("capacity");
    id_less_than = rb_intern("less_than");
    id_lt = rb_intern("<");

    id_mkdir_p = rb_intern("mkdir_p");
    id_is_directory = rb_intern("directory?");
    id_close = rb_intern("close");

    id_cclass = rb_intern("cclass");

    id_data = rb_intern("@data");

    /* Symbols */
    sym_yes = ID2SYM(rb_intern("yes"));;
    sym_no = ID2SYM(rb_intern("no"));;
    sym_true = ID2SYM(rb_intern("true"));;
    sym_false = ID2SYM(rb_intern("false"));;
    sym_path = ID2SYM(rb_intern("path"));;
    sym_dir = ID2SYM(rb_intern("dir"));;

    /* Inits */
    Init_Ferret();
    Init_Utils();
    Init_Analysis();
    Init_Store();
    Init_Index();
    Init_Search();
    Init_QueryParser();

    /* Error Classes */
    cParseError =
        rb_define_class_under(mFerret, "ParseError", rb_eStandardError);
    cStateError =
        rb_define_class_under(mFerret, "StateError", rb_eStandardError);
    cFileNotFoundError =
        rb_define_class_under(mFerret, "FileNotFoundError", rb_eIOError);

    error_map = rb_hash_new();
    rb_hash_aset(error_map, rb_intern("Exception"),         rb_eStandardError);
    rb_hash_aset(error_map, rb_intern("IO Error"),          rb_eIOError);
    rb_hash_aset(error_map, rb_intern("File Not Found Error"),
                                                            cFileNotFoundError);
    rb_hash_aset(error_map, rb_intern("Argument Error"),    rb_eArgError);
    rb_hash_aset(error_map, rb_intern("End-of-File Error"), rb_eEOFError);
    rb_hash_aset(error_map, rb_intern("Unsupported Function Error"),
                                                            rb_eNotImpError);
    rb_hash_aset(error_map, rb_intern("State Error"),       cStateError);
    rb_hash_aset(error_map, rb_intern("ParseError"),        cParseError);
    rb_hash_aset(error_map, rb_intern("Memory Error"),      rb_eNoMemError);
    rb_hash_aset(error_map, rb_intern("Index Error"),       rb_eIndexError);
    rb_hash_aset(error_map, rb_intern("Lock Error"),        cLockError);

    rb_define_const(mFerret, "EXCEPTION_MAP", error_map);
    rb_define_const(mFerret, "FIX_INT_MAX", INT2FIX(INT_MAX >> 1));
}
