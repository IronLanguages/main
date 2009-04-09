#include "ferret.h"
#include "index.h"
#include <st.h>

VALUE mIndex;

VALUE cFieldInfo;
VALUE cFieldInfos;

VALUE cTVOffsets;
VALUE cTVTerm;
VALUE cTermVector;

VALUE cTermEnum;
VALUE cTermDocEnum;

VALUE cLazyDoc;
VALUE cLazyDocData;
VALUE cIndexWriter;
VALUE cIndexReader;

VALUE sym_analyzer;
static VALUE sym_close_dir;
static VALUE sym_create;
static VALUE sym_create_if_missing;

static VALUE sym_chunk_size;
static VALUE sym_max_buffer_memory;
static VALUE sym_index_interval;
static VALUE sym_skip_interval;
static VALUE sym_merge_factor;
static VALUE sym_max_buffered_docs;
static VALUE sym_max_merge_docs;
static VALUE sym_max_field_length;
static VALUE sym_use_compound_file;

static VALUE sym_boost;
static VALUE sym_field_infos;

static VALUE sym_store;
static VALUE sym_index;
static VALUE sym_term_vector;

static VALUE sym_compress;
static VALUE sym_compressed;

static VALUE sym_untokenized;
static VALUE sym_omit_norms;
static VALUE sym_untokenized_omit_norms;

static VALUE sym_with_positions;
static VALUE sym_with_offsets;
static VALUE sym_with_positions_offsets;

static ID id_term;
static ID id_fields;
static ID id_fld_num_map;
static ID id_field_num;
static ID id_boost;

extern void frt_set_term(VALUE rterm, Term *t);
extern Analyzer *frt_get_cwrapped_analyzer(VALUE ranalyzer);
extern VALUE frt_get_analyzer(Analyzer *a);

/****************************************************************************
 *
 * FieldInfo Methods
 *
 ****************************************************************************/

static void
frt_fi_free(void *p)
{
    object_del(p);
    fi_deref((FieldInfo *)p);
}

static void
frt_fi_get_params(VALUE roptions,
                  enum StoreValues *store,
                  enum IndexValues *index,
                  enum TermVectorValues *term_vector,
                  float *boost)
{
    VALUE v;
    Check_Type(roptions, T_HASH);
    v = rb_hash_aref(roptions, sym_boost);
    if (Qnil != v) {
        *boost = (float)NUM2DBL(v);
    } else {
        *boost = 1.0f;
    }
    v = rb_hash_aref(roptions, sym_store);
    if (Qnil != v) Check_Type(v, T_SYMBOL);
    if (v == sym_no || v == sym_false || v == Qfalse) {
        *store = STORE_NO;
    } else if (v == sym_yes || v == sym_true || v == Qtrue) {
        *store = STORE_YES;
    } else if (v == sym_compress || v == sym_compressed) {
        *store = STORE_COMPRESS;
    } else if (v == Qnil) {
        /* leave as default */
    } else {
        rb_raise(rb_eArgError, ":%s isn't a valid argument for :store."
                 " Please choose from [:yes, :no, :compressed]",
                 rb_id2name(SYM2ID(v)));
    }

    v = rb_hash_aref(roptions, sym_index);
    if (Qnil != v) Check_Type(v, T_SYMBOL);
    if (v == sym_no || v == sym_false || v == Qfalse) {
        *index = INDEX_NO;
    } else if (v == sym_yes || v == sym_true || v == Qtrue) {
        *index = INDEX_YES;
    } else if (v == sym_untokenized) {
        *index = INDEX_UNTOKENIZED;
    } else if (v == sym_omit_norms) {
        *index = INDEX_YES_OMIT_NORMS;
    } else if (v == sym_untokenized_omit_norms) {
        *index = INDEX_UNTOKENIZED_OMIT_NORMS;
    } else if (v == Qnil) {
        /* leave as default */
    } else {
        rb_raise(rb_eArgError, ":%s isn't a valid argument for :index."
                 " Please choose from [:no, :yes, :untokenized, "
                 ":omit_norms, :untokenized_omit_norms]",
                 rb_id2name(SYM2ID(v)));
    }

    v = rb_hash_aref(roptions, sym_term_vector);
    if (Qnil != v) Check_Type(v, T_SYMBOL);
    if (v == sym_no || v == sym_false || v == Qfalse) {
        *term_vector = TERM_VECTOR_NO;
    } else if (v == sym_yes || v == sym_true || v == Qtrue) {
        *term_vector = TERM_VECTOR_YES;
    } else if (v == sym_with_positions) {
        *term_vector = TERM_VECTOR_WITH_POSITIONS;
    } else if (v == sym_with_offsets) {
        *term_vector = TERM_VECTOR_WITH_OFFSETS;
    } else if (v == sym_with_positions_offsets) {
        *term_vector = TERM_VECTOR_WITH_POSITIONS_OFFSETS;
    } else if (v == Qnil) {
        /* leave as default */
    } else {
        rb_raise(rb_eArgError, ":%s isn't a valid argument for "
                 ":term_vector. Please choose from [:no, :yes, "
                 ":with_positions, :with_offsets, "
                 ":with_positions_offsets]",
                 rb_id2name(SYM2ID(v)));
    }
}

static VALUE
frt_get_field_info(FieldInfo *fi)
{

    VALUE rfi = Qnil;
    if (fi) {
        rfi = object_get(fi);
        if (rfi == Qnil) {
            rfi = Data_Wrap_Struct(cFieldInfo, NULL, &frt_fi_free, fi);
            REF(fi);
            object_add(fi, rfi);
        }
    }
    return rfi;
}

/*
 *  call-seq:
 *     FieldInfo.new(name, options = {}) -> field_info
 *
 *  Create a new FieldInfo object with the name +name+ and the properties
 *  specified in +options+. The available options are [:store, :index,
 *  :term_vector, :boost]. See the description of FieldInfo for more
 *  information on these properties. 
 */
static VALUE
frt_fi_init(int argc, VALUE *argv, VALUE self)
{
    VALUE roptions, rname;
    FieldInfo *fi;
    enum StoreValues store = STORE_YES;
    enum IndexValues index = INDEX_YES;
    enum TermVectorValues term_vector = TERM_VECTOR_WITH_POSITIONS_OFFSETS;
    float boost = 1.0f;

    rb_scan_args(argc, argv, "11", &rname, &roptions);
    if (argc > 1) {
        frt_fi_get_params(roptions, &store, &index, &term_vector, &boost);
    }
    fi = fi_new(frt_field(rname), store, index, term_vector);
    fi->boost = boost;
    Frt_Wrap_Struct(self, NULL, &frt_fi_free, fi);
    object_add(fi, self);
    return self;
}

/*
 *  call-seq:
 *     fi.name -> symbol
 *
 *  Return the name of the field
 */
static VALUE
frt_fi_name(VALUE self)
{
    FieldInfo *fi = (FieldInfo *)DATA_PTR(self);
    return ID2SYM(rb_intern(fi->name));
}

/*
 *  call-seq:
 *     fi.stored? -> bool
 *
 *  Return true if the field is stored in the index.
 */
static VALUE
frt_fi_is_stored(VALUE self)
{
    FieldInfo *fi = (FieldInfo *)DATA_PTR(self);
    return fi_is_stored(fi) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     fi.compressed? -> bool
 *
 *  Return true if the field is stored in the index in compressed format.
 */
static VALUE
frt_fi_is_compressed(VALUE self)
{
    FieldInfo *fi = (FieldInfo *)DATA_PTR(self);
    return fi_is_compressed(fi) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     fi.indexed? -> bool
 *
 *  Return true if the field is indexed, ie searchable in the index.
 */
static VALUE
frt_fi_is_indexed(VALUE self)
{
    FieldInfo *fi = (FieldInfo *)DATA_PTR(self);
    return fi_is_indexed(fi) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     fi.tokenized? -> bool
 *
 *  Return true if the field is tokenized. Tokenizing is the process of
 *  breaking the field up into tokens. That is "the quick brown fox" becomes:
 *
 *    ["the", "quick", "brown", "fox"]
 *
 *  A field can only be tokenized if it is indexed.
 */
static VALUE
frt_fi_is_tokenized(VALUE self)
{
    FieldInfo *fi = (FieldInfo *)DATA_PTR(self);
    return fi_is_tokenized(fi) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     fi.omit_norms? -> bool
 *
 *  Return true if the field omits the norm file. The norm file is the file
 *  used to store the field boosts for an indexed field. If you do not boost
 *  any fields, and you can live without scoring based on field length then
 *  you can omit the norms file. This will give the index a slight performance
 *  boost and it will use less memory, especially for indexes which have a
 *  large number of documents.
 */
static VALUE
frt_fi_omit_norms(VALUE self)
{
    FieldInfo *fi = (FieldInfo *)DATA_PTR(self);
    return fi_omit_norms(fi) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     fi.store_term_vector? -> bool
 *
 *  Return true if the term-vectors are stored for this field.
 */
static VALUE
frt_fi_store_term_vector(VALUE self)
{
    FieldInfo *fi = (FieldInfo *)DATA_PTR(self);
    return fi_store_term_vector(fi) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     fi.store_positions? -> bool
 *
 *  Return true if positions are stored with the term-vectors for this field.
 */
static VALUE
frt_fi_store_positions(VALUE self)
{
    FieldInfo *fi = (FieldInfo *)DATA_PTR(self);
    return fi_store_positions(fi) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     fi.store_offsets? -> bool
 *
 *  Return true if offsets are stored with the term-vectors for this field.
 */
static VALUE
frt_fi_store_offsets(VALUE self)
{
    FieldInfo *fi = (FieldInfo *)DATA_PTR(self);
    return fi_store_offsets(fi) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     fi.has_norms? -> bool
 *
 *  Return true if this field has a norms file. This is the same as calling;
 *    
 *    fi.indexed? and not fi.omit_norms?
 */
static VALUE
frt_fi_has_norms(VALUE self)
{
    FieldInfo *fi = (FieldInfo *)DATA_PTR(self);
    return fi_has_norms(fi) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     fi.boost -> boost
 *
 *  Return the default boost for this field
 */
static VALUE
frt_fi_boost(VALUE self)
{
    FieldInfo *fi = (FieldInfo *)DATA_PTR(self);
    return rb_float_new((double)fi->boost);
}

/*
 *  call-seq:
 *     fi.to_s -> string
 *
 *  Return a string representation of the FieldInfo object.
 */
static VALUE
frt_fi_to_s(VALUE self)
{
    FieldInfo *fi = (FieldInfo *)DATA_PTR(self);
    char *fi_s = fi_to_s(fi);
    VALUE rfi_s = rb_str_new2(fi_s);
    free(fi_s);
    return rfi_s;
}

/****************************************************************************
 *
 * FieldInfos Methods
 *
 ****************************************************************************/

static void
frt_fis_free(void *p)
{
    object_del(p);
    fis_deref((FieldInfos *)p);
}

static void
frt_fis_mark(void *p)
{
    int i;
    FieldInfos *fis = (FieldInfos *)p;

    for (i = 0; i < fis->size; i++) {
        frt_gc_mark(fis->fields[i]);
    }
}

static VALUE
frt_get_field_infos(FieldInfos *fis)
{

    VALUE rfis = Qnil;
    if (fis) {
        rfis = object_get(fis);
        if (rfis == Qnil) {
            rfis = Data_Wrap_Struct(cFieldInfos, &frt_fis_mark, &frt_fis_free,
                                    fis);
            REF(fis);
            object_add(fis, rfis);
        }
    }
    return rfis;
}

/*
 *  call-seq:
 *     FieldInfos.new(defaults = {}) -> field_infos
 *     
 *  Create a new FieldInfos object which uses the default values for fields
 *  specified in the +default+ hash parameter. See FieldInfo for available
 *  property values.
 */
static VALUE
frt_fis_init(int argc, VALUE *argv, VALUE self)
{
    VALUE roptions;
    FieldInfos *fis;
    enum StoreValues store = STORE_YES;
    enum IndexValues index = INDEX_YES;
    enum TermVectorValues term_vector = TERM_VECTOR_WITH_POSITIONS_OFFSETS;
    float boost;

    rb_scan_args(argc, argv, "01", &roptions);
    if (argc > 0) {
        frt_fi_get_params(roptions, &store, &index, &term_vector, &boost);
    }
    fis = fis_new(store, index, term_vector);
    Frt_Wrap_Struct(self, &frt_fis_mark, &frt_fis_free, fis);
    object_add(fis, self);
    return self;
}

/*
 *  call-seq:
 *     fis.to_a -> array
 *
 *  Return an array of the FieldInfo objects contained but this FieldInfos
 *  object.
 */
static VALUE
frt_fis_to_a(VALUE self)
{
    FieldInfos *fis = (FieldInfos *)DATA_PTR(self);
    VALUE rary = rb_ary_new();
    int i;

    for (i = 0; i < fis->size; i++) {
        rb_ary_push(rary, frt_get_field_info(fis->fields[i]));
    }
    return rary;
}

/*
 *  call-seq:
 *     fis[name] -> field_info
 *     fis[number] -> field_info
 *
 *  Get the FieldInfo object. FieldInfo objects can be referenced by either
 *  their field-number of the field-name (which must be a symbol). For
 *  example;
 *
 *    fi = fis[:name]
 *    fi = fis[2]
 */
static VALUE
frt_fis_get(VALUE self, VALUE ridx)
{
    FieldInfos *fis = (FieldInfos *)DATA_PTR(self);
    VALUE rfi = Qnil;
    switch (TYPE(ridx)) {
        case T_FIXNUM: {
            int index = FIX2INT(ridx);
            if (index < 0) index += fis->size;
            if (index < 0 || index >= fis->size) {
                rb_raise(rb_eArgError, "index of %d is out of range (0..%d)\n",
                         index, fis->size);
            }
            rfi = frt_get_field_info(fis->fields[index]);
            break;
                       }
        case T_SYMBOL:
            rfi = frt_get_field_info(fis_get_field(fis, frt_field(ridx)));
            break;
        case T_STRING:
            rfi = frt_get_field_info(fis_get_field(fis, StringValuePtr(ridx)));
            break;
        default:
            rb_raise(rb_eArgError, "Can't index FieldInfos with %s",
                     rs2s(rb_obj_as_string(ridx)));
            break;
    }
    return rfi;
}

/*
 *  call-seq:
 *     fis << fi -> fis
 *     fis.add(fi) -> fis
 *
 *  Add a FieldInfo object. Use the FieldInfos#add_field method where
 *  possible.
 */
static VALUE
frt_fis_add(VALUE self, VALUE rfi)
{
    FieldInfos *fis = (FieldInfos *)DATA_PTR(self);
    FieldInfo *fi = (FieldInfo *)frt_rb_data_ptr(rfi);
    fis_add_field(fis, fi);
    REF(fi);
    return self;
}

/*
 *  call-seq:
 *     fis.add_field(name, properties = {} -> fis
 *
 *  Add a new field to the FieldInfos object. See FieldInfo for a description
 *  of the available properties.
 */
static VALUE
frt_fis_add_field(int argc, VALUE *argv, VALUE self)
{
    FieldInfos *fis = (FieldInfos *)DATA_PTR(self);
    FieldInfo *fi;
    enum StoreValues store = fis->store;
    enum IndexValues index = fis->index;
    enum TermVectorValues term_vector = fis->term_vector;
    float boost = 1.0f;
    VALUE rname, roptions;

    rb_scan_args(argc, argv, "11", &rname, &roptions);
    if (argc > 1) {
        frt_fi_get_params(roptions, &store, &index, &term_vector, &boost);
    }
    fi = fi_new(frt_field(rname), store, index, term_vector);
    fi->boost = boost;
    fis_add_field(fis, fi);
    return self;
}

/*
 *  call-seq:
 *     fis.each {|fi| do_something } -> fis
 *
 *  Iterate through the FieldInfo objects.
 */
static VALUE
frt_fis_each(VALUE self)
{
    int i;
    FieldInfos *fis = (FieldInfos *)DATA_PTR(self);

    for (i = 0; i < fis->size; i++) {
        rb_yield(frt_get_field_info(fis->fields[i]));
    }
    return self;
}

/*
 *  call-seq:
 *     fis.to_s -> string
 *     
 *  Return a string representation of the FieldInfos object.
 */
static VALUE
frt_fis_to_s(VALUE self)
{
    FieldInfos *fis = (FieldInfos *)DATA_PTR(self);
    char *fis_s = fis_to_s(fis);
    VALUE rfis_s = rb_str_new2(fis_s);
    free(fis_s);
    return rfis_s;
}

/*
 *  call-seq:
 *     fis.size -> int
 *     
 *  Return the number of fields in the FieldInfos object.
 */
static VALUE
frt_fis_size(VALUE self)
{
    FieldInfos *fis = (FieldInfos *)DATA_PTR(self);
    return INT2FIX(fis->size);
}
 
/*
 *  call-seq:
 *     fis.create_index(dir) -> self
 *     
 *  Create a new index in the directory specified. The directory +dir+ can
 *  either be a string path representing a directory on the file-system or an
 *  actual directory object. Care should be taken when using this method. Any
 *  existing index (or other files for that matter) will be deleted from the
 *  directory and overwritten by the new index.
 */
static VALUE
frt_fis_create_index(VALUE self, VALUE rdir)
{
    FieldInfos *fis = (FieldInfos *)DATA_PTR(self);
    Store *store = NULL;
    if (TYPE(rdir) == T_DATA) {
        store = DATA_PTR(rdir);
        REF(store);
    } else {
        StringValue(rdir);
        frt_create_dir(rdir);
        store = open_fs_store(rs2s(rdir));
    }
    index_create(store, fis);
    store_deref(store);
    return self;
}
 
/*
 *  call-seq:
 *     fis.fields -> symbol array
 *
 *  Return a list of the field names (as symbols) of all the fields in the
 *  index.
 */
static VALUE
frt_fis_get_fields(VALUE self)
{
    FieldInfos *fis = (FieldInfos *)DATA_PTR(self);
    VALUE rfield_names = rb_ary_new();
    int i;
    for (i = 0; i < fis->size; i++) {
        rb_ary_push(rfield_names, ID2SYM(rb_intern(fis->fields[i]->name)));
    }
    return rfield_names;
}

/*
 *  call-seq:
 *     fis.tokenized_fields -> symbol array
 *
 *  Return a list of the field names (as symbols) of all the tokenized fields
 *  in the index.
 */
static VALUE
frt_fis_get_tk_fields(VALUE self)
{
    FieldInfos *fis = (FieldInfos *)DATA_PTR(self);
    VALUE rfield_names = rb_ary_new();
    int i;
    for (i = 0; i < fis->size; i++) {
        if (!fi_is_tokenized(fis->fields[i])) continue;
        rb_ary_push(rfield_names, ID2SYM(rb_intern(fis->fields[i]->name)));
    }
    return rfield_names;
}

/****************************************************************************
 *
 * TermEnum Methods
 *
 ****************************************************************************/

static void
frt_te_free(void *p)
{
    TermEnum *te = (TermEnum *)p;
    te->close(te);
}

static VALUE
frt_te_get_set_term(VALUE self, const char *term)
{
    TermEnum *te = (TermEnum *)DATA_PTR(self);
    VALUE str = term ? rb_str_new(term, te->curr_term_len) : Qnil;
    rb_ivar_set(self, id_term, str);
    return str;
}

static VALUE
frt_get_te(VALUE rir, TermEnum *te)
{
    VALUE self = Qnil;
    if (te != NULL) {
        self = Data_Wrap_Struct(cTermEnum, NULL, &frt_te_free, te);
        frt_te_get_set_term(self, te->curr_term);
        rb_ivar_set(self, id_fld_num_map, rb_ivar_get(rir, id_fld_num_map));
    }
    return self;
}

/*
 *  call-seq:
 *     term_enum.next -> term_string
 *
 *  Returns the next term in the enumeration or nil otherwise.
 */
static VALUE
frt_te_next(VALUE self)
{
    TermEnum *te = (TermEnum *)DATA_PTR(self);
    return frt_te_get_set_term(self, te->next(te));
}

/*
 *  call-seq:
 *     term_enum.term -> term_string
 *
 *  Returns the current term pointed to by the enum. This method should only
 *  be called after a successful call to TermEnum#next.
 */
static VALUE
frt_te_term(VALUE self)
{
    return rb_ivar_get(self, id_term);
}

/*
 *  call-seq:
 *     term_enum.doc_freq -> integer
 *
 *  Returns the document frequency of the current term pointed to by the enum.
 *  That is the number of documents that this term appears in. The method
 *  should only be called after a successful call to TermEnum#next.
 */
static VALUE
frt_te_doc_freq(VALUE self)
{
    TermEnum *te = (TermEnum *)DATA_PTR(self);
    return INT2FIX(te->curr_ti.doc_freq);
}

/*
 *  call-seq:
 *     term_enum.skip_to(target) -> term
 *
 *  Skip to term +target+. This method can skip forwards or backwards. If you
 *  want to skip back to the start, pass the empty string "". That is;
 *  
 *    term_enum.skip_to("")
 *
 *  Returns the first term greater than or equal to +target+
 */
static VALUE
frt_te_skip_to(VALUE self, VALUE rterm)
{
    TermEnum *te = (TermEnum *)DATA_PTR(self);
    return frt_te_get_set_term(self, te->skip_to(te, frt_field(rterm)));
}

/*
 *  call-seq:
 *     term_enum.each {|term, doc_freq| do_something() } -> term_count
 *
 *  Iterates through all the terms in the field, yielding the term and the
 *  document frequency. 
 */
static VALUE
frt_te_each(VALUE self)
{
    TermEnum *te = (TermEnum *)DATA_PTR(self);
    char *term;
    int term_cnt = 0;
    VALUE vals = rb_ary_new2(2);
    RARRAY(vals)->len = 2;
    rb_mem_clear(RARRAY(vals)->ptr, 2);


    /* each is being called so there will be no current term */
    rb_ivar_set(self, id_term, Qnil);

    
    while (NULL != (term = te->next(te))) {
        term_cnt++;
        RARRAY(vals)->ptr[0] = rb_str_new(term, te->curr_term_len);
        RARRAY(vals)->ptr[1] = INT2FIX(te->curr_ti.doc_freq);
        rb_yield(vals);
    }
    return INT2FIX(term_cnt);
}

/*
 *  call-seq:
 *     term_enum.set_field(field) -> self
 *
 *  Set the field for the term_enum. The field value should be a symbol as
 *  usual. For example, to scan all title terms you'd do this;
 *
 *    term_enum.set_field(:title).each do |term, doc_freq|
 *      do_something()
 *    end
 */
static VALUE
frt_te_set_field(VALUE self, VALUE rfield)
{
    TermEnum *te = (TermEnum *)DATA_PTR(self);
    int field_num = 0;
    VALUE rfnum_map = rb_ivar_get(self, id_fld_num_map);
    VALUE rfnum = rb_hash_aref(rfnum_map, rfield);
    if (rfnum != Qnil) {
        field_num = FIX2INT(rfnum);
        rb_ivar_set(self, id_field_num, rfnum);
    } else {
        Check_Type(rfield, T_SYMBOL);
        rb_raise(rb_eArgError, "field %s doesn't exist in the index",
                 frt_field(rfield));
    }
    te->set_field(te, field_num);

    return self;
}

/*
 *  call-seq:
 *     term_enum.to_json() -> string
 *
 *  Returns a JSON representation of the term enum. You can speed this up by
 *  having the method return arrays instead of objects, simply by passing an
 *  argument to the to_json method. For example;
 *
 *    term_enum.to_json() #=> 
 *    # [
 *    #   {"term":"apple","frequency":12},
 *    #   {"term":"banana","frequency":2},
 *    #   {"term":"cantaloupe","frequency":12}
 *    # ]
 *
 *    term_enum.to_json(:fast) #=> 
 *    # [
 *    #   ["apple",12],
 *    #   ["banana",2],
 *    #   ["cantaloupe",12]
 *    # ]
 */
static VALUE
frt_te_to_json(int argc, VALUE *argv, VALUE self)
{
    TermEnum *te = (TermEnum *)DATA_PTR(self);
    VALUE rjson;
    char *json, *jp;
    char *term;
    int capa = 65536;
    jp = json = ALLOC_N(char, capa);
    *(jp++) = '[';

    if (argc > 0) {
        while (NULL != (term = te->next(te))) {
            /* enough room for for term after converting " to '"' and frequency
             * plus some extra for good measure */
            *(jp++) = '[';
            if (te->curr_term_len * 3 + (jp - json) + 100 > capa) {
                capa <<= 1;
                REALLOC_N(json, char, capa);
            }
            jp = json_concat_string(jp, term);
            *(jp++) = ',';
            sprintf(jp, "%d", te->curr_ti.doc_freq);
            jp += strlen(jp);
            *(jp++) = ']';
            *(jp++) = ',';
        }
    }
    else {
        while (NULL != (term = te->next(te))) {
            /* enough room for for term after converting " to '"' and frequency
             * plus some extra for good measure */
            if (te->curr_term_len * 3 + (jp - json) + 100 > capa) {
                capa <<= 1;
                REALLOC_N(json, char, capa);
            }
            *(jp++) = '{';
            memcpy(jp, "\"term\":", 7);
            jp += 7;
            jp = json_concat_string(jp, term);
            *(jp++) = ',';
            memcpy(jp, "\"frequency\":", 12);
            jp += 12;
            sprintf(jp, "%d", te->curr_ti.doc_freq);
            jp += strlen(jp);
            *(jp++) = '}';
            *(jp++) = ',';
        }
    }
    if (*(jp-1) == ',') jp--;
    *(jp++) = ']';
    *jp = '\0';

    rjson = rb_str_new2(json);
    free(json);
    return rjson;
}

/****************************************************************************
 *
 * TermDocEnum Methods
 *
 ****************************************************************************/

static void
frt_tde_free(void *p)
{
    TermDocEnum *tde = (TermDocEnum *)p;
    tde->close(tde);
}

static VALUE
frt_get_tde(VALUE rir, TermDocEnum *tde)
{
    VALUE self = Data_Wrap_Struct(cTermDocEnum, NULL, &frt_tde_free, tde);
    rb_ivar_set(self, id_fld_num_map, rb_ivar_get(rir, id_fld_num_map));
    return self;
}

/*
 *  call-seq:
 *     term_doc_enum.seek(field, term) -> self
 *
 *  Seek the term +term+ in the index for +field+. After you call this method
 *  you can call next or each to skip through the documents and positions of
 *  this particular term.
 */
static VALUE
frt_tde_seek(VALUE self, VALUE rfield, VALUE rterm)
{
    TermDocEnum *tde = (TermDocEnum *)DATA_PTR(self);
    char *term;
    VALUE rfnum_map = rb_ivar_get(self, id_fld_num_map);
    VALUE rfnum = rb_hash_aref(rfnum_map, rfield);
    int field_num = -1;
    term = StringValuePtr(rterm);
    if (rfnum != Qnil) {
        field_num = FIX2INT(rfnum);
    } else {
        rb_raise(rb_eArgError, "field %s doesn't exist in the index",
                 frt_field(rfield));
    }
    tde->seek(tde, field_num, term);
    return self;
}

/*
 *  call-seq:
 *     term_doc_enum.seek_term_enum(term_enum) -> self
 *
 *  Seek the current term in +term_enum+. You could just use the standard seek
 *  method like this;
 *
 *    term_doc_enum.seek(term_enum.term)
 *
 *  However the +seek_term_enum+ method saves an index lookup so should offer
 *  a large performance improvement.
 */
static VALUE
frt_tde_seek_te(VALUE self, VALUE rterm_enum)
{
    TermDocEnum *tde = (TermDocEnum *)DATA_PTR(self);
    TermEnum *te = (TermEnum *)frt_rb_data_ptr(rterm_enum);
    tde->seek_te(tde, te);
    return self;
}

/*
 *  call-seq:
 *     term_doc_enum.doc -> doc_id
 *
 *  Returns the current document number pointed to by the +term_doc_enum+.
 */
static VALUE
frt_tde_doc(VALUE self)
{
    TermDocEnum *tde = (TermDocEnum *)DATA_PTR(self);
    return INT2FIX(tde->doc_num(tde));
}

/*
 *  call-seq:
 *     term_doc_enum.doc -> doc_id
 *
 *  Returns the frequency of the current document pointed to by the
 *  +term_doc_enum+.
 */
static VALUE
frt_tde_freq(VALUE self)
{
    TermDocEnum *tde = (TermDocEnum *)DATA_PTR(self);
    return INT2FIX(tde->freq(tde));
}

/*
 *  call-seq:
 *     term_doc_enum.doc -> doc_id
 *
 *  Move forward to the next document in the enumeration. Returns +true+ if
 *  there is another document or +false+ otherwise.
 */
static VALUE
frt_tde_next(VALUE self)
{
    TermDocEnum *tde = (TermDocEnum *)DATA_PTR(self);
    return tde->next(tde) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     term_doc_enum.doc -> doc_id
 *
 *  Move forward to the next document in the enumeration. Returns +true+ if
 *  there is another document or +false+ otherwise.
 */
static VALUE
frt_tde_next_position(VALUE self)
{
    TermDocEnum *tde = (TermDocEnum *)DATA_PTR(self);
    int pos;
    if (tde->next_position == NULL) {
        rb_raise(rb_eNotImpError, "to scan through positions you must create "
                 "the TermDocEnum with Index#term_positions method rather "
                 "than the Index#term_docs method");
    }
    pos = tde->next_position(tde);
    return pos >= 0 ? INT2FIX(pos) : Qnil;
}

/*
 *  call-seq:
 *     term_doc_enum.each {|doc_id, freq| do_something() } -> doc_count
 *
 *  Iterate through the documents and document frequencies in the
 *  +term_doc_enum+.
 *
 *  NOTE: this method can only be called once after each seek. If you need to
 *  call +#each+ again then you should call +#seek+ again too.
 */
static VALUE
frt_tde_each(VALUE self)
{
    int doc_cnt = 0;
    TermDocEnum *tde = (TermDocEnum *)DATA_PTR(self);
    VALUE vals = rb_ary_new2(2);
    RARRAY(vals)->len = 2;
    rb_mem_clear(RARRAY(vals)->ptr, 2);

    while (tde->next(tde)) {
        doc_cnt++;
        RARRAY(vals)->ptr[0] = INT2FIX(tde->doc_num(tde));
        RARRAY(vals)->ptr[1] = INT2FIX(tde->freq(tde));
        rb_yield(vals);

    }
    return INT2FIX(doc_cnt);
}

/*
 *  call-seq:
 *     term_doc_enum.to_json() -> string
 *
 *  Returns a json representation of the term doc enum. It will also add the
 *  term positions if they are available. You can speed this up by having the
 *  method return arrays instead of objects, simply by passing an argument to
 *  the to_json method. For example;
 *
 *    term_doc_enum.to_json() #=> 
 *    # [
 *    #   {"document":1,"frequency":12},
 *    #   {"document":11,"frequency":1},
 *    #   {"document":29,"frequency":120},
 *    #   {"document":30,"frequency":3}
 *    # ]
 *
 *    term_doc_enum.to_json(:fast) #=> 
 *    # [
 *    #   [1,12],
 *    #   [11,1],
 *    #   [29,120],
 *    #   [30,3]
 *    # ]
 */
static VALUE
frt_tde_to_json(int argc, VALUE *argv, VALUE self)
{
    TermDocEnum *tde = (TermDocEnum *)DATA_PTR(self);
    VALUE rjson;
    char *json, *jp;
    int capa = 65536;
    char *format;
    char close = (argc > 0) ? ']' : '}';
    bool do_positions = tde->next_position != NULL;
    jp = json = ALLOC_N(char, capa);
    *(jp++) = '[';

    if (do_positions) {
        if (argc == 0) {
            format = "{\"document\":%d,\"frequency\":%d,\"positions\":[";
        }
        else {
            format = "[%d,%d,[";
        }
    }
    else {
        if (argc == 0) {
            format = "{\"document\":%d,\"frequency\":%d},";
        }
        else {
            format = "[%d,%d],";
        }
    }
    while (tde->next(tde)) {
        /* 100 chars should be enough room for an extra entry */
        if ((jp - json) + 100 + tde->freq(tde) * 20 > capa) {
            capa <<= 1;
            REALLOC_N(json, char, capa);
        }
        sprintf(jp, format, tde->doc_num(tde), tde->freq(tde));
        jp += strlen(jp);
        if (do_positions) {
            int pos;
            while (0 <= (pos = tde->next_position(tde))) {
                sprintf(jp, "%d,", pos);
                jp += strlen(jp);
            }
            if (*(jp - 1) == ',') jp--;
            *(jp++) = ']';
            *(jp++) = close;
            *(jp++) = ',';
        }
    }
    if (*(jp - 1) == ',') jp--;
    *(jp++) = ']';
    *jp = '\0';

    rjson = rb_str_new2(json);
    free(json);
    return rjson;
}

/*
 *  call-seq:
 *     term_doc_enum.each_position {|pos| do_something } -> term_doc_enum
 *
 *  Iterate through each of the positions occupied by the current term in the
 *  current document. This can only be called once per document. It can be
 *  used within the each method. For example, to print the terms documents and
 *  positions;
 *
 *    tde.each do |doc_id, freq|
 *      puts "term appeared #{freq} times in document #{doc_id}:"
 *      positions = []
 *      tde.each_position {|pos| positions << pos}
 *      puts "  #{positions.join(', ')}"
 *    end
 */
static VALUE
frt_tde_each_position(VALUE self)
{
    TermDocEnum *tde = (TermDocEnum *)DATA_PTR(self);
    int pos;
    if (tde->next_position == NULL) {
        rb_raise(rb_eNotImpError, "to scan through positions you must create "
                 "the TermDocEnum with Index#term_positions method rather "
                 "than the Index#term_docs method");
    }
    while (0 <= (pos = tde->next_position(tde))) {
        rb_yield(INT2FIX(pos));
    }
    return self;
}

/*
 *  call-seq:
 *     term_doc_enum.skip_to(target) -> bool
 *
 *  Skip to the required document number +target+ and return true if there is
 *  a document >= +target+.
 */
static VALUE
frt_tde_skip_to(VALUE self, VALUE rtarget)
{
    TermDocEnum *tde = (TermDocEnum *)DATA_PTR(self);
    return tde->skip_to(tde, FIX2INT(rtarget)) ? Qtrue : Qfalse;
}

/****************************************************************************
 *
 * TVOffsets Methods
 *
 ****************************************************************************/

static VALUE
frt_get_tv_offsets(Offset *offset)
{
    return rb_struct_new(cTVOffsets,
                         ULL2NUM((unsigned long long)offset->start),
                         ULL2NUM((unsigned long long)offset->end),
                         NULL);
}

/****************************************************************************
 *
 * TVTerm Methods
 *
 ****************************************************************************/

static VALUE
frt_get_tv_term(TVTerm *tv_term)
{
    int i;
    const int freq = tv_term->freq;
    VALUE rtext;
    VALUE rpositions = Qnil;
    rtext = rb_str_new2(tv_term->text);
    if (tv_term->positions) {
        VALUE *rpos;
        int *positions = tv_term->positions;
        rpositions = rb_ary_new2(freq);
        rpos = RARRAY(rpositions)->ptr;
        for (i = 0; i < freq; i++) {
            rpos[i] = INT2FIX(positions[i]);
        }
        RARRAY(rpositions)->len = freq;
    }
    return rb_struct_new(cTVTerm, rtext, rpositions, NULL);
}

/****************************************************************************
 *
 * TermVector Methods
 *
 ****************************************************************************/

static VALUE
frt_get_tv(TermVector *tv)
{
    int i;
    TVTerm *terms = tv->terms;
    const int t_cnt = tv->term_cnt;
    const int o_cnt = tv->offset_cnt;
    VALUE rfield, rterms, *rts;
    VALUE roffsets = Qnil;
    rfield = ID2SYM(rb_intern(tv->field));

    rterms = rb_ary_new2(t_cnt);
    rts = RARRAY(rterms)->ptr;
    for (i = 0; i < t_cnt; i++) {
        rts[i] = frt_get_tv_term(&terms[i]);
        RARRAY(rterms)->len++;
    }

    if (tv->offsets) {
        VALUE *ros;
        Offset *offsets = tv->offsets;
        roffsets = rb_ary_new2(o_cnt);
        ros = RARRAY(roffsets)->ptr;
        for (i = 0; i < o_cnt; i++) {
            ros[i] = frt_get_tv_offsets(&offsets[i]);
            RARRAY(roffsets)->len++;
        }
    }

    return rb_struct_new(cTermVector, rfield, rterms, roffsets, NULL);
}

/****************************************************************************
 *
 * IndexWriter Methods
 *
 ****************************************************************************/

void
frt_iw_free(void *p)
{
    iw_close((IndexWriter *)p);
}

void
frt_iw_mark(void *p)
{
    IndexWriter *iw = (IndexWriter *)p;
    frt_gc_mark(iw->analyzer);
    frt_gc_mark(iw->store);
    frt_gc_mark(iw->fis);
}

/*
 *  call-seq:
 *     index_writer.close -> nil
 *
 *  Close the IndexWriter. This will close and free all resources used
 *  exclusively by the index writer. The garbage collector will do this
 *  automatically if not called explicitly.
 */
static VALUE
frt_iw_close(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    Frt_Unwrap_Struct(self);
    iw_close(iw);
    return Qnil;
}

#define SET_INT_ATTR(attr) \
    do {\
        if (RTEST(rval = rb_hash_aref(roptions, sym_##attr)))\
            config.attr = FIX2INT(rval);\
    } while (0)

/*
 *  call-seq:
 *     IndexWriter.new(options = {}) -> index_writer
 *
 *  Create a new IndexWriter. You should either pass a path or a directory to
 *  this constructor. For example, here are three ways you can create an
 *  IndexWriter;
 *
 *    dir = RAMDirectory.new()
 *    iw = IndexWriter.new(:dir => dir)
 *
 *    dir = FSDirectory.new("/path/to/index")
 *    iw = IndexWriter.new(:dir => dir)
 *
 *    iw = IndexWriter.new(:path => "/path/to/index")
 *
 * See IndexWriter for more options.
 */
static VALUE
frt_iw_init(int argc, VALUE *argv, VALUE self)
{
    VALUE roptions, rval;
    bool create = false;
    bool create_if_missing = true;
    Store *store = NULL;
    Analyzer *analyzer = NULL;
    IndexWriter *volatile iw = NULL;
    Config config = default_config;

    rb_scan_args(argc, argv, "01", &roptions);
    if (argc > 0) {
        Check_Type(roptions, T_HASH);

        if ((rval = rb_hash_aref(roptions, sym_dir)) != Qnil) {
            Check_Type(rval, T_DATA);
            store = DATA_PTR(rval);
        } else if ((rval = rb_hash_aref(roptions, sym_path)) != Qnil) {
            StringValue(rval);
            frt_create_dir(rval);
            store = open_fs_store(rs2s(rval));
            DEREF(store);
        }
        
        /* Let ruby's garbage collector handle the closing of the store
           if (!close_dir) {
           close_dir = RTEST(rb_hash_aref(roptions, sym_close_dir));
           }
           */
        /* use_compound_file defaults to true */
        config.use_compound_file = 
            (rb_hash_aref(roptions, sym_use_compound_file) == Qfalse)
            ? false
            : true;

        if ((rval = rb_hash_aref(roptions, sym_analyzer)) != Qnil) {
            analyzer = frt_get_cwrapped_analyzer(rval);
        }

        create = RTEST(rb_hash_aref(roptions, sym_create));
        if ((rval = rb_hash_aref(roptions, sym_create_if_missing)) != Qnil) {
            create_if_missing = RTEST(rval);
        }
        SET_INT_ATTR(chunk_size);
        SET_INT_ATTR(max_buffer_memory);
        SET_INT_ATTR(index_interval);
        SET_INT_ATTR(skip_interval);
        SET_INT_ATTR(merge_factor);
        SET_INT_ATTR(max_buffered_docs);
        SET_INT_ATTR(max_merge_docs);
        SET_INT_ATTR(max_field_length);
    }
    if (NULL == store) {
        store = open_ram_store();
        DEREF(store);
    }
    if (!create && create_if_missing && !store->exists(store, "segments")) {
        create = true;
    }
    if (create) {
        FieldInfos *fis;
        if ((rval = rb_hash_aref(roptions, sym_field_infos)) != Qnil) {
            Data_Get_Struct(rval, FieldInfos, fis);
            index_create(store, fis);
        } else {
            fis = fis_new(STORE_YES, INDEX_YES,
                          TERM_VECTOR_WITH_POSITIONS_OFFSETS);
            index_create(store, fis);
            fis_deref(fis);
        }
    }

    iw = iw_open(store, analyzer, &config);

    Frt_Wrap_Struct(self, &frt_iw_mark, &frt_iw_free, iw);

    if (rb_block_given_p()) {
        rb_yield(self);
        frt_iw_close(self);
        return Qnil;
    } else {
        return self;
    }
}

/*
 *  call-seq:
 *     iw.doc_count -> number
 *
 *  Returns the number of documents in the Index. Note that deletions won't be
 *  taken into account until the IndexWriter has been committed.
 */
static VALUE
frt_iw_get_doc_count(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    return INT2FIX(iw_doc_count(iw));
}

static int
frt_hash_to_doc_i(VALUE key, VALUE value, VALUE arg)
{
    if (key == Qundef) {
        return ST_CONTINUE;
    } else {
        Document *doc = (Document *)arg;
        char *field;
        VALUE val;
        DocField *df;
        switch (TYPE(key)) {
            case T_STRING:
                field = rs2s(key);
                break;
            case T_SYMBOL:
                field = rb_id2name(SYM2ID(key));
                break;
            default:
                rb_raise(rb_eArgError,
                         "%s cannot be a key to a field. Field keys must "
                         " be symbols.", rs2s(rb_obj_as_string(key)));
                break;
        }
        if (NULL == (df = doc_get_field(doc, field))) {
            df = df_new(field);
        }
        if (rb_respond_to(value, id_boost)) {
            df->boost = (float)NUM2DBL(rb_funcall(value, id_boost, 0));
        }
        switch (TYPE(value)) {
            case T_ARRAY:
                {
                    int i;
                    df->destroy_data = true;
                    for (i = 0; i < RARRAY(value)->len; i++) {
                        val = rb_obj_as_string(RARRAY(value)->ptr[i]);
                        df_add_data_len(df, nstrdup(val), RSTRING(val)->len);
                    }
                }
                break;
            case T_STRING:
                df_add_data_len(df, rs2s(value), RSTRING(value)->len);
                break;
            default:
                val = rb_obj_as_string(value);
                df->destroy_data = true;
                df_add_data_len(df, nstrdup(val), RSTRING(val)->len);
                break;
        }
        doc_add_field(doc, df);
    }
    return ST_CONTINUE;
}

static Document *
frt_get_doc(VALUE rdoc)
{
    VALUE val;
    Document *doc = doc_new();
    DocField *df;

    if (rb_respond_to(rdoc, id_boost)) {
        doc->boost = (float)NUM2DBL(rb_funcall(rdoc, id_boost, 0));
    }

    switch (TYPE(rdoc)) {
        case T_HASH:
            rb_hash_foreach(rdoc, frt_hash_to_doc_i, (VALUE)doc);
            break;
        case T_ARRAY:
            {
                int i;
                df = df_new("content");
                df->destroy_data = true;
                for (i = 0; i < RARRAY(rdoc)->len; i++) {
                    val = rb_obj_as_string(RARRAY(rdoc)->ptr[i]);
                    df_add_data_len(df, nstrdup(val), RSTRING(val)->len);
                }
                doc_add_field(doc, df);
            }
            break;
        case T_SYMBOL:
            df = df_add_data(df_new("content"), rb_id2name(SYM2ID(rdoc)));
            doc_add_field(doc, df);
            break;
        case T_STRING:
            df = df_add_data_len(df_new("content"), rs2s(rdoc),
                                 RSTRING(rdoc)->len);
            doc_add_field(doc, df);
            break;
        default:
            val = rb_obj_as_string(rdoc);
            df = df_add_data_len(df_new("content"), nstrdup(val),
                                 RSTRING(val)->len);
            df->destroy_data = true;
            doc_add_field(doc, df);
            break;
    }
    return doc;
}

/* 
 *  call-seq:
 *     iw << document -> iw
 *     iw.add_document(document) -> iw
 *
 *  Add a document to the index. See Document. A document can also be a simple
 *  hash object.
 */
static VALUE
frt_iw_add_doc(VALUE self, VALUE rdoc)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    Document *doc = frt_get_doc(rdoc);
    iw_add_doc(iw, doc);
    doc_destroy(doc);
    return self;
}

/*
 *  call-seq:
 *     iw.optimize -> iw
 *
 *  Optimize the index for searching. This commits any unwritten data to the
 *  index and optimizes the index into a single segment to improve search
 *  performance. This is an expensive operation and should not be called too
 *  often. The best time to call this is at the end of a long batch indexing
 *  process. Note that calling the optimize method do not in any way effect
 *  indexing speed (except for the time taken to complete the optimization
 *  process).
 */
static VALUE
frt_iw_optimize(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    iw_optimize(iw);
    return self;
}

/*
 *  call-seq:
 *     iw.commit -> iw
 *
 *  Explicitly commit any changes to the index that may be hanging around in
 *  memory. You should call this method if you want to read the latest index
 *  with an IndexWriter.
 */
static VALUE
frt_iw_commit(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    iw_commit(iw);
    return self;
}

/*
 *  call-seq:
 *     iw.add_readers(reader_array) -> iw
 *
 *  Use this method to merge other indexes into the one being written by
 *  IndexWriter. This is useful for parallel indexing. You can have several
 *  indexing processes running in parallel, possibly even on different
 *  machines. Then you can finish by merging all of the indexes into a single
 *  index.
 */
static VALUE
frt_iw_add_readers(VALUE self, VALUE rreaders)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    int i;
    IndexReader **irs;
    Check_Type(rreaders, T_ARRAY);

    irs = ALLOC_N(IndexReader *, RARRAY(rreaders)->len);
    i = RARRAY(rreaders)->len;
    while (i-- > 0) {
        IndexReader *ir;
        Data_Get_Struct(RARRAY(rreaders)->ptr[i], IndexReader, ir);
        irs[i] = ir;
    }
    iw_add_readers(iw, irs, RARRAY(rreaders)->len);
    free(irs);
    return self;
}

/*
 *  call-seq:
 *     iw.delete(field, term) -> iw
 *
 *  Delete all documents in the index with the term +term+ in the field
 *  +field+. You should usually have a unique document id which you use with
 *  this method, rather then deleting all documents with the word "the" in
 *  them. You may however use this method to delete spam.
 */
static VALUE
frt_iw_delete(VALUE self, VALUE rfield, VALUE rterm)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    iw_delete_term(iw, frt_field(rfield), StringValuePtr(rterm));
    return self;
}

/*
 *  call-seq:
 *     index_writer.field_infos -> FieldInfos
 *
 *  Get the FieldInfos object for this IndexWriter. This is useful if you need
 *  to dynamically add new fields to the index with specific properties.
 */
static VALUE
frt_iw_field_infos(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    return frt_get_field_infos(iw->fis);
}

/*
 *  call-seq:
 *     index_writer.analyzer -> Analyzer
 *
 *  Get the Analyzer for this IndexWriter. This is useful if you need
 *  to use the same analyzer in a QueryParser.
 */
static VALUE
frt_iw_get_analyzer(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    return frt_get_analyzer(iw->analyzer);
}

/*
 *  call-seq:
 *     index_writer.analyzer -> Analyzer
 *
 *  Set the Analyzer for this IndexWriter. This is useful if you need to
 *  change the analyzer for a special document. It is risky though as the
 *  same analyzer will be used for all documents during search.
 */
static VALUE
frt_iw_set_analyzer(VALUE self, VALUE ranalyzer)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);

    a_deref(iw->analyzer);
    iw->analyzer = frt_get_cwrapped_analyzer(ranalyzer);
    return ranalyzer;
}

/*
 *  call-seq:
 *     index_writer.version -> int
 *
 *  Returns the current version of the index writer.
 */
static VALUE
frt_iw_version(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    return ULL2NUM(iw->sis->version);
}

/*
 *  call-seq:
 *     iw.chunk_size -> number
 *
 *  Return the current value of chunk_size
 */
static VALUE
frt_iw_get_chunk_size(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    return INT2FIX(iw->config.chunk_size);
}

/*
 *  call-seq:
 *     iw.chunk_size = chunk_size -> chunk_size
 *
 *  Set the chunk_size parameter
 */
static VALUE
frt_iw_set_chunk_size(VALUE self, VALUE rval)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    iw->config.chunk_size = FIX2INT(rval);
    return rval;
}

/*
 *  call-seq:
 *     iw.max_buffer_memory -> number
 *
 *  Return the current value of max_buffer_memory
 */
static VALUE
frt_iw_get_max_buffer_memory(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    return INT2FIX(iw->config.max_buffer_memory);
}

/*
 *  call-seq:
 *     iw.max_buffer_memory = max_buffer_memory -> max_buffer_memory
 *
 *  Set the max_buffer_memory parameter
 */
static VALUE
frt_iw_set_max_buffer_memory(VALUE self, VALUE rval)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    iw->config.max_buffer_memory = FIX2INT(rval);
    return rval;
}

/*
 *  call-seq:
 *     iw.term_index_interval -> number
 *
 *  Return the current value of term_index_interval
 */
static VALUE
frt_iw_get_index_interval(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    return INT2FIX(iw->config.index_interval);
}

/*
 *  call-seq:
 *     iw.term_index_interval = term_index_interval -> term_index_interval
 *
 *  Set the term_index_interval parameter
 */
static VALUE
frt_iw_set_index_interval(VALUE self, VALUE rval)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    iw->config.index_interval = FIX2INT(rval);
    return rval;
}

/*
 *  call-seq:
 *     iw.doc_skip_interval -> number
 *
 *  Return the current value of doc_skip_interval
 */
static VALUE
frt_iw_get_skip_interval(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    return INT2FIX(iw->config.skip_interval);
}

/*
 *  call-seq:
 *     iw.doc_skip_interval = doc_skip_interval -> doc_skip_interval
 *
 *  Set the doc_skip_interval parameter
 */
static VALUE
frt_iw_set_skip_interval(VALUE self, VALUE rval)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    iw->config.skip_interval = FIX2INT(rval);
    return rval;
}

/*
 *  call-seq:
 *     iw.merge_factor -> number
 *
 *  Return the current value of merge_factor
 */
static VALUE
frt_iw_get_merge_factor(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    return INT2FIX(iw->config.merge_factor);
}

/*
 *  call-seq:
 *     iw.merge_factor = merge_factor -> merge_factor
 *
 *  Set the merge_factor parameter
 */
static VALUE
frt_iw_set_merge_factor(VALUE self, VALUE rval)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    iw->config.merge_factor = FIX2INT(rval);
    return rval;
}

/*
 *  call-seq:
 *     iw.max_buffered_docs -> number
 *
 *  Return the current value of max_buffered_docs
 */
static VALUE
frt_iw_get_max_buffered_docs(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    return INT2FIX(iw->config.max_buffered_docs);
}

/*
 *  call-seq:
 *     iw.max_buffered_docs = max_buffered_docs -> max_buffered_docs
 *
 *  Set the max_buffered_docs parameter
 */
static VALUE
frt_iw_set_max_buffered_docs(VALUE self, VALUE rval)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    iw->config.max_buffered_docs = FIX2INT(rval);
    return rval;
}

/*
 *  call-seq:
 *     iw.max_merge_docs -> number
 *
 *  Return the current value of max_merge_docs
 */
static VALUE
frt_iw_get_max_merge_docs(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    return INT2FIX(iw->config.max_merge_docs);
}

/*
 *  call-seq:
 *     iw.max_merge_docs = max_merge_docs -> max_merge_docs
 *
 *  Set the max_merge_docs parameter
 */
static VALUE
frt_iw_set_max_merge_docs(VALUE self, VALUE rval)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    iw->config.max_merge_docs = FIX2INT(rval);
    return rval;
}

/*
 *  call-seq:
 *     iw.max_field_length -> number
 *
 *  Return the current value of max_field_length
 */
static VALUE
frt_iw_get_max_field_length(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    return INT2FIX(iw->config.max_field_length);
}

/*
 *  call-seq:
 *     iw.max_field_length = max_field_length -> max_field_length
 *
 *  Set the max_field_length parameter
 */
static VALUE
frt_iw_set_max_field_length(VALUE self, VALUE rval)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    iw->config.max_field_length = FIX2INT(rval);
    return rval;
}

/*
 *  call-seq:
 *     iw.use_compound_file -> number
 *
 *  Return the current value of use_compound_file
 */
static VALUE
frt_iw_get_use_compound_file(VALUE self)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    return iw->config.use_compound_file ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     iw.use_compound_file = use_compound_file -> use_compound_file
 *
 *  Set the use_compound_file parameter
 */
static VALUE
frt_iw_set_use_compound_file(VALUE self, VALUE rval)
{
    IndexWriter *iw = (IndexWriter *)DATA_PTR(self);
    iw->config.use_compound_file = RTEST(rval);
    return rval;
}

/****************************************************************************
 *
 * LazyDoc Methods
 *
 ****************************************************************************/

static void
frt_lzd_date_free(void *p)
{
    lazy_doc_close((LazyDoc *)p);
}

static VALUE
frt_lazy_df_load(VALUE self, VALUE rkey, LazyDocField *lazy_df)
{
    VALUE rdata = Qnil;
    if (lazy_df) {
        if (lazy_df->size == 1) {
            char *data = lazy_df_get_data(lazy_df, 0);
            rdata = rb_str_new(data, lazy_df->len);
        } else {
            int i;
            rdata = rb_ary_new2(lazy_df->size);
            for (i = 0; i < lazy_df->size; i++) {
                char *data = lazy_df_get_data(lazy_df, i);
                RARRAY(rdata)->ptr[i] =
                    rb_str_new(data, lazy_df->data[i].length);
                RARRAY(rdata)->len++;
            }
        }
        rb_hash_aset(self, rkey, rdata);
    }
    return rdata;
}

/*
 *  call-seq:
 *     lazy_doc.default(key) -> string
 *
 *  This method is used internally to lazily load fields. You should never
 *  really need to call it yourself.
 */
static VALUE
frt_lzd_default(VALUE self, VALUE rkey)
{
    LazyDoc *lazy_doc = (LazyDoc *)DATA_PTR(rb_ivar_get(self, id_data));
    char *field = NULL;
    switch (TYPE(rkey)) {
        case T_STRING:
            field = rs2s(rkey);
            rkey = ID2SYM(rb_intern(field));
            break;
        case T_SYMBOL:
            field = frt_field(rkey);
            break;
        default:
            rb_raise(rb_eArgError,
                     "%s cannot be a key to a field. Field keys must "
                     " be symbols.", rs2s(rb_obj_as_string(rkey)));
            break;
    }
    return frt_lazy_df_load(self, rkey, h_get(lazy_doc->field_dict, field));
}

/*
 *  call-seq:
 *     lazy_doc.fields -> array of available fields
 *
 *  Returns the list of fields stored for this particular document. If you try
 *  to access any of these fields in the document the field will be loaded.
 *  Try to access any other field an nil will be returned.
 */
static VALUE
frt_lzd_fields(VALUE self)
{
    return rb_ivar_get(self, id_fields);
}

/*
 *  call-seq:
 *     lazy_doc.load -> lazy_doc
 *
 *  Load all unloaded fields in the document from the index.
 */
static VALUE
frt_lzd_load(VALUE self)
{
    LazyDoc *lazy_doc = (LazyDoc *)DATA_PTR(rb_ivar_get(self, id_data));
    int i;
    for (i = 0; i < lazy_doc->size; i++) {
        LazyDocField *lazy_df = lazy_doc->fields[i];
        frt_lazy_df_load(self, ID2SYM(rb_intern(lazy_df->name)), lazy_df);
    }
    return self;
}

VALUE
frt_get_lazy_doc(LazyDoc *lazy_doc)
{
    int i;
    VALUE rfields = rb_ary_new2(lazy_doc->size);

    VALUE self, rdata;
    self = rb_hash_new();
    OBJSETUP(self, cLazyDoc, T_HASH);

    rdata = Data_Wrap_Struct(cLazyDocData, NULL, &frt_lzd_date_free, lazy_doc);
    rb_ivar_set(self, id_data, rdata);

    for (i = 0; i < lazy_doc->size; i++) {
        RARRAY(rfields)->ptr[i] = ID2SYM(rb_intern(lazy_doc->fields[i]->name));
        RARRAY(rfields)->len++;
    }
    rb_ivar_set(self, id_fields, rfields);

    return self;
}

/****************************************************************************
 *
 * IndexReader Methods
 *
 ****************************************************************************/

void
frt_ir_free(void *p)
{
    object_del(p);
    ir_close((IndexReader *)p);
}

void
frt_ir_mark(void *p)
{
    IndexReader *ir = (IndexReader *)p;
    frt_gc_mark(ir->store);
}

static VALUE frt_ir_close(VALUE self);

void
frt_mr_mark(void *p)
{
    MultiReader *mr = (MultiReader *)p;
    int i;
    for (i = 0; i < mr->r_cnt; i++) {
        frt_gc_mark(mr->sub_readers[i]);
    }
}

/*
 *  call-seq:
 *     IndexReader.new(dir) -> index_reader
 *
 *  Create a new IndexReader. You can either pass a string path to a
 *  file-system directory or an actual Ferret::Store::Directory object. For
 *  example;
 *
 *    dir = RAMDirectory.new()
 *    iw = IndexReader.new(dir)
 *
 *    dir = FSDirectory.new("/path/to/index")
 *    iw = IndexReader.new(dir)
 *
 *    iw = IndexReader.new("/path/to/index")
 *
 *  You can also create a what used to be known as a MultiReader by passing an
 *  array of IndexReader objects, Ferret::Store::Directory objects or
 *  file-system paths;
 *
 *    iw = IndexReader.new([dir, dir2, dir3])
 *
 *    iw = IndexReader.new([reader1, reader2, reader3])
 *
 *    iw = IndexReader.new(["/path/to/index1", "/path/to/index2"])
 */
static VALUE
frt_ir_init(VALUE self, VALUE rdir)
{
    Store *store = NULL;
    IndexReader *ir;
    int i;
    FieldInfos *fis;
    VALUE rfield_num_map = rb_hash_new();

    if (TYPE(rdir) == T_ARRAY) {
        VALUE rdirs = rdir;
        const int reader_cnt = RARRAY(rdir)->len;
        IndexReader **sub_readers = ALLOC_N(IndexReader *, reader_cnt);
        int i;
        for (i = 0; i < reader_cnt; i++) {
            rdir = RARRAY(rdirs)->ptr[i];
            switch (TYPE(rdir)) {
                case T_DATA:
                    if (CLASS_OF(rdir) == cIndexReader) {
                        Data_Get_Struct(rdir, IndexReader, sub_readers[i]);
                        REF(sub_readers[i]);
                        continue;
                    } else if (RTEST(rb_obj_is_kind_of(rdir, cDirectory))) {
                        store = DATA_PTR(rdir);
                    } else {
                        rb_raise(rb_eArgError, "A Multi-IndexReader can only "
                                 "be created from other IndexReaders, "
                                 "Directory objects or file-system paths. "
                                 "Not %s",
                                 rs2s(rb_obj_as_string(rdir)));
                    }
                    break;
                case T_STRING:
                    frt_create_dir(rdir);
                    store = open_fs_store(rs2s(rdir));
                    DEREF(store);
                    break;
                default:
                    rb_raise(rb_eArgError, "%s isn't a valid directory "
                             "argument. You should use either a String or "
                             "a Directory",
                             rs2s(rb_obj_as_string(rdir)));
                    break;
            }
            sub_readers[i] = ir_open(store);
        }
        ir = mr_open(sub_readers, reader_cnt);
        Frt_Wrap_Struct(self, &frt_mr_mark, &frt_ir_free, ir);
    } else {
        switch (TYPE(rdir)) {
            case T_DATA:
                store = DATA_PTR(rdir);
                break;
            case T_STRING:
                frt_create_dir(rdir);
                store = open_fs_store(rs2s(rdir));
                DEREF(store);
                break;
            default:
                rb_raise(rb_eArgError, "%s isn't a valid directory argument. "
                         "You should use either a String or a Directory",
                         rs2s(rb_obj_as_string(rdir)));
                break;
        }
        ir = ir_open(store);
        Frt_Wrap_Struct(self, &frt_ir_mark, &frt_ir_free, ir);
    }
    object_add(ir, self);

    fis = ir->fis;
    for (i = 0; i < fis->size; i++) {
        FieldInfo *fi = fis->fields[i];
        rb_hash_aset(rfield_num_map,
                     ID2SYM(rb_intern(fi->name)),
                     INT2FIX(fi->number));
    }
    rb_ivar_set(self, id_fld_num_map, rfield_num_map);

    return self;
}

/*
 *  call-seq:
 *     index_reader.set_norm(doc_id, field, val)
 *
 *  Expert: change the boost value for a +field+ in document at +doc_id+.
 *  +val+ should be an integer in the range 0..255 which corresponds to an
 *  encoded float value.
 */
static VALUE
frt_ir_set_norm(VALUE self, VALUE rdoc_id, VALUE rfield, VALUE rval)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    ir_set_norm(ir, FIX2INT(rdoc_id), frt_field(rfield), (uchar)NUM2CHR(rval));
    return self;
}

/*
 *  call-seq:
 *     index_reader.norms(field) -> string
 *  
 *  Expert: Returns a string containing the norm values for a field. The
 *  string length will be equal to the number of documents in the index and it
 *  could have null bytes.
 */
static VALUE
frt_ir_norms(VALUE self, VALUE rfield)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    uchar *norms;
    norms = ir_get_norms(ir, frt_field(rfield));
    if (norms) {
        return rb_str_new((char *)norms, ir->max_doc(ir));
    } else {
        return Qnil;
    }
}

/*
 *  call-seq:
 *     index_reader.get_norms_into(field, buffer, offset) -> buffer
 *
 *  Expert: Get the norm values into a string +buffer+ starting at +offset+.
 */
static VALUE
frt_ir_get_norms_into(VALUE self, VALUE rfield, VALUE rnorms, VALUE roffset)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    int offset;
    offset = FIX2INT(roffset);
    Check_Type(rnorms, T_STRING);
    if (RSTRING(rnorms)->len < offset + ir->max_doc(ir)) {
        rb_raise(rb_eArgError, "supplied a string of length:%d to "
                 "IndexReader#get_norms_into but needed a string of length "
                 "offset:%d + maxdoc:%d",
                 RSTRING(rnorms)->len, offset, ir->max_doc(ir));
    }

    ir_get_norms_into(ir, frt_field(rfield),
                      (uchar *)rs2s(rnorms) + offset);
    return rnorms;
}

/*
 *  call-seq:
 *     index_reader.commit -> index_reader
 *
 *  Commit any deletes made by this particular IndexReader to the index. This
 *  will use open a Commit lock.
 */
static VALUE
frt_ir_commit(VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    ir_commit(ir);
    return self;
}

/*
 *  call-seq:
 *     index_reader.close -> index_reader
 *
 *  Close the IndexReader. This method also commits any deletions made by this
 *  IndexReader. This method will be called explicitly by the garbage
 *  collector but you should call it explicitly to commit any changes as soon
 *  as possible and to close any locks held by the object to prevent locking
 *  errors.
 */
static VALUE
frt_ir_close(VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    object_del(ir);
    Frt_Unwrap_Struct(self);
    ir_close(ir);
    return self;
}

/*
 *  call-seq:
 *     index_reader.has_deletions? -> bool
 *
 *  Return true if the index has any deletions, either uncommitted by this
 *  IndexReader or committed by any other IndexReader.
 */
static VALUE
frt_ir_has_deletions(VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return ir->has_deletions(ir) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     index_reader.delete(doc_id) -> index_reader
 *
 *  Delete document referenced internally by document id +doc_id+. The
 *  document_id is the number used to reference documents in the index and is
 *  returned by search methods.
 */
static VALUE
frt_ir_delete(VALUE self, VALUE rdoc_id)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    ir_delete_doc(ir, FIX2INT(rdoc_id));
    return self;
}

/*
 *  call-seq:
 *     index_reader.deleted?(doc_id) -> bool
 *
 *  Returns true if the document at +doc_id+ has been deleted.
 */
static VALUE
frt_ir_is_deleted(VALUE self, VALUE rdoc_id)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return ir->is_deleted(ir, FIX2INT(rdoc_id)) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     index_reader.max_doc -> number
 *
 *  Returns 1 + the maximum document id in the index. It is the
 *  document_id that will be used by the next document added to the index. If
 *  there are no deletions, this number also refers to the number of documents
 *  in the index.
 */
static VALUE
frt_ir_max_doc(VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return INT2FIX(ir->max_doc(ir));
}

/*
 *  call-seq:
 *     index_reader.num_docs -> number
 *
 *  Returns the number of accessible (not deleted) documents in the index.
 *  This will be equal to IndexReader#max_doc if there have been no documents
 *  deleted from the index.
 */
static VALUE
frt_ir_num_docs(VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return INT2FIX(ir->num_docs(ir));
}

/* 
 *  call-seq:
 *     index_reader.undelete_all -> index_reader
 *
 *  Undelete all deleted documents in the index. This is kind of like a
 *  rollback feature. Not that once an index is committed or a merge happens
 *  during index, deletions will be committed and undelete_all will have no
 *  effect on these documents.
 */
static VALUE
frt_ir_undelete_all(VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    ir_undelete_all(ir);
    return self;
}

static VALUE
frt_get_doc_range(IndexReader *ir, int pos, int len, int max)
{
    VALUE ary;
    int i;
    max = min2(max, pos+len);
    len = max - pos;
    ary = rb_ary_new2(len);
    for (i = 0; i < len; i++) {
        RARRAY(ary)->ptr[i] = frt_get_lazy_doc(ir->get_lazy_doc(ir, i + pos));
        RARRAY(ary)->len++;
    }
    return ary;
}

/*
 *  call-seq:
 *     index_reader.get_document(doc_id) -> LazyDoc
 *     index_reader[doc_id] -> LazyDoc
 *
 *  Retrieve a document from the index. See LazyDoc for more details on the
 *  document returned. Documents are referenced internally by document ids
 *  which are returned by the Searchers search methods.
 */
static VALUE
frt_ir_get_doc(int argc, VALUE *argv, VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    VALUE arg1, arg2;
    long pos, len;
    long max = ir->max_doc(ir);
    rb_scan_args(argc, argv, "11", &arg1, &arg2);
    if (argc == 1) {
        if (FIXNUM_P(arg1)) {
            pos = FIX2INT(arg1);
            pos = (pos < 0) ? (max + pos) : pos;
            if (pos < 0 || pos >= max) {
                rb_raise(rb_eArgError, ":%d is out of range [%d..%d] for "
                         "IndexReader#[]", pos, 0, max,
                         rb_id2name(SYM2ID(argv)));
            }
            return frt_get_lazy_doc(ir->get_lazy_doc(ir, pos));
        }

        /* check if idx is Range */
        switch (rb_range_beg_len(arg1, &pos, &len, max, 0)) {
            case Qfalse:
                rb_raise(rb_eArgError, ":%s isn't a valid argument for "
                         "IndexReader.get_document(index)",
                         rb_id2name(SYM2ID(argv)));
            case Qnil:
                return Qnil;
            default:
                return frt_get_doc_range(ir, pos, len, max);
        }
    }
    else {
        pos = FIX2LONG(arg1);
        len = FIX2LONG(arg2);
        return frt_get_doc_range(ir, pos, len, max);
    }
}

/*
 *  call-seq:
 *     index_reader.is_latest? -> bool
 *
 *  Return true if the index version referenced by this IndexReader is the
 *  latest version of the index. If it isn't you should close and reopen the
 *  index to search the latest documents added to the index.
 */
static VALUE
frt_ir_is_latest(VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return ir_is_latest(ir) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     index_reader.term_vector(doc_id, field) -> TermVector
 *
 *  Return the TermVector for the field +field+ in the document at +doc_id+ in
 *  the index. Return nil of no such term_vector exists. See TermVector.
 */
static VALUE
frt_ir_term_vector(VALUE self, VALUE rdoc_id, VALUE rfield)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    TermVector *tv;
    VALUE rtv;
    tv = ir->term_vector(ir, FIX2INT(rdoc_id), frt_field(rfield));
    if (tv) {
        rtv = frt_get_tv(tv);
        tv_destroy(tv);
        return rtv;
    }
    else {
        return Qnil;
    }
}

static void
frt_add_each_tv(void *key, void *value, void *rtvs)
{
    rb_hash_aset((VALUE)rtvs, ID2SYM(rb_intern(key)), frt_get_tv(value));
}

/*
 *  call-seq:
 *     index_reader.term_vectors(doc_id) -> hash of TermVector
 *
 *  Return the TermVectors for the document at +doc_id+ in the index. The
 *  value returned is a hash of the TermVectors for each field in the document
 *  and they are referenced by field names (as symbols).
 */
static VALUE
frt_ir_term_vectors(VALUE self, VALUE rdoc_id)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    HashTable *tvs = ir->term_vectors(ir, FIX2INT(rdoc_id));
    VALUE rtvs = rb_hash_new();
    h_each(tvs, &frt_add_each_tv, (void *)rtvs);
    h_destroy(tvs);

    return rtvs;
}

/*
 *  call-seq:
 *     index_reader.term_docs -> TermDocEnum
 *
 *  Builds a TermDocEnum (term-document enumerator) for the index. You can use
 *  this object to iterate through the documents in which certain terms occur.
 *  See TermDocEnum for more info.
 */
static VALUE
frt_ir_term_docs(VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return frt_get_tde(self, ir->term_docs(ir));
}

/*
 *  call-seq:
 *     index_reader.term_docs_for(field, term) -> TermDocEnum
 *
 *  Builds a TermDocEnum to iterate through the documents that contain the
 *  term +term+ in the field +field+. See TermDocEnum for more info.
 */
static VALUE
frt_ir_term_docs_for(VALUE self, VALUE rfield, VALUE rterm)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return frt_get_tde(self, ir_term_docs_for(ir,
                                              frt_field(rfield),
                                              StringValuePtr(rterm)));
}

/*
 *  call-seq:
 *     index_reader.term_positions -> TermDocEnum
 *
 *  Same as IndexReader#term_docs except the TermDocEnum will also allow you
 *  to scan through the positions at which a term occurs. See TermDocEnum for
 *  more info.
 */
static VALUE
frt_ir_term_positions(VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return frt_get_tde(self, ir->term_positions(ir));
}

/*
 *  call-seq:
 *     index_reader.term_positions_for(field, term) -> TermDocEnum
 *
 *  Same as IndexReader#term_docs_for(field, term) except the TermDocEnum will
 *  also allow you to scan through the positions at which a term occurs. See
 *  TermDocEnum for more info.
 */
static VALUE
frt_ir_t_pos_for(VALUE self, VALUE rfield, VALUE rterm)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return frt_get_tde(self, ir_term_positions_for(ir,
                                                   frt_field(rfield),
                                                   StringValuePtr(rterm)));
}

/*
 *  call-seq:
 *     index_reader.doc_freq(field, term) -> integer
 *
 *  Return the number of documents in which the term +term+ appears in the
 *  field +field+.
 */
static VALUE
frt_ir_doc_freq(VALUE self, VALUE rfield, VALUE rterm)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return INT2FIX(ir_doc_freq(ir,
                               frt_field(rfield),
                               StringValuePtr(rterm)));
}

/*
 *  call-seq:
 *     index_reader.terms(field) -> TermEnum
 *
 *  Returns a term enumerator which allows you to iterate through all the
 *  terms in the field +field+ in the index.
 */
static VALUE
frt_ir_terms(VALUE self, VALUE rfield)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return frt_get_te(self, ir_terms(ir, frt_field(rfield)));
}

/*
 *  call-seq:
 *     index_reader.terms_from(field, term) -> TermEnum
 *
 *  Same as IndexReader#terms(fields) except that it starts the enumerator off
 *  at term +term+.
 */
static VALUE
frt_ir_terms_from(VALUE self, VALUE rfield, VALUE rterm)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return frt_get_te(self, ir_terms_from(ir,
                                          frt_field(rfield),
                                          StringValuePtr(rterm)));
}

/*
 *  call-seq:
 *     index_reader.term_count(field) -> int
 *
 *  Same return a count of the number of terms in the field
 */
static VALUE
frt_ir_term_count(VALUE self, VALUE rfield)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    TermEnum *te = ir_terms(ir, frt_field(rfield));
    int count = 0;
    while (te->next(te)) {
        count++;
    }
    te->close(te);
    return INT2FIX(count);
}

/*
 *  call-seq:
 *     index_reader.fields -> array of field-names
 *
 *  Returns an array of field names in the index. This can be used to pass to
 *  the QueryParser so that the QueryParser knows how to expand the "*"
 *  wild-card to all fields in the index. A list of field names can also be
 *  gathered from the FieldInfos object.
 */
static VALUE
frt_ir_fields(VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    FieldInfos *fis = ir->fis;
    VALUE rfield_names = rb_ary_new();
    int i;
    for (i = 0; i < fis->size; i++) {
        rb_ary_push(rfield_names, ID2SYM(rb_intern(fis->fields[i]->name)));
    }
    return rfield_names;
}

/*
 *  call-seq:
 *     index_reader.field_infos -> FieldInfos
 *
 *  Get the FieldInfos object for this IndexReader.
 */
static VALUE
frt_ir_field_infos(VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return frt_get_field_infos(ir->fis);
}

/*
 *  call-seq:
 *     index_reader.tokenized_fields -> array of field-names
 *
 *  Returns an array of field names of all of the tokenized fields in the
 *  index. This can be used to pass to the QueryParser so that the QueryParser
 *  knows how to expand the "*" wild-card to all fields in the index. A list
 *  of field names can also be gathered from the FieldInfos object.
 */
static VALUE
frt_ir_tk_fields(VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    FieldInfos *fis = ir->fis;
    VALUE rfield_names = rb_ary_new();
    int i;
    for (i = 0; i < fis->size; i++) {
        if (!fi_is_tokenized(fis->fields[i])) continue;
        rb_ary_push(rfield_names, ID2SYM(rb_intern(fis->fields[i]->name)));
    }
    return rfield_names;
}

/*
 *  call-seq:
 *     index_reader.version -> int
 *
 *  Returns the current version of the index reader.
 */
static VALUE
frt_ir_version(VALUE self)
{
    IndexReader *ir = (IndexReader *)DATA_PTR(self);
    return ULL2NUM(ir->sis->version);
}

/****************************************************************************
 *
 * Init Functions
 *
 ****************************************************************************/


/*
 *  Document-class: Ferret::Index::FieldInfo
 *
 *  == Summary
 *
 *  The FieldInfo class is the field descriptor for the index. It specifies
 *  whether a field is compressed or not or whether it should be indexed and
 *  tokenized. Every field has a name which must be a symbol. There are three
 *  properties that you can set, +:store+, +:index+ and +:term_vector+. You
 *  can also set the default +:boost+ for a field as well.
 *
 *  == Properties
 *
 *  === :store
 *
 *  The +:store+ property allows you to specify how a field is stored. You can
 *  leave a field unstored (+:no+), store it in it's original format (+:yes+)
 *  or store it in compressed format (+:compressed+). By default the document
 *  is stored in its original format. If the field is large and it is stored
 *  elsewhere where it is easily accessible you might want to leave it
 *  unstored. This will keep the index size a lot smaller and make the
 *  indexing process a lot faster. For example, you should probably leave the
 *  +:content+ field unstored when indexing all the documents in your
 *  file-system.
 *
 *  === :index
 *
 *  The +:index+ property allows you to specify how a field is indexed. A
 *  field must be indexed to be searchable. However, a field doesn't need to
 *  be indexed to be store in the Ferret index. You may want to use the index
 *  as a simple database and store things like images or MP3s in the index. By
 *  default each field is indexed and tokenized (split into tokens) (+:yes+).
 *  If you don't want to index the field use +:no+. If you want the field
 *  indexed but not tokenized, use +:untokenized+. Do this for the fields you
 *  wish to sort by. There are two other values for +:index+; +:omit_norms+
 *  and +:untokenized_omit_norms+. These values correspond to +:yes+ and
 *  +:untokenized+ respectively and are useful if you are not boosting any
 *  fields and you'd like to speed up the index. The norms file is the file
 *  which contains the boost values for each document for a particular field.
 *
 *  === :term_vector 
 *
 *  See TermVector for a description of term-vectors. You can specify whether
 *  or not you would like to store term-vectors. The available options are
 *  +:no+, +:yes+, +:with_positions+, +:with_offsets+ and
 *  +:with_positions_offsets+. Note that you need to store the positions to
 *  associate offsets with individual terms in the term_vector.
 *
 *  == Property Table
 *
 *    Property       Value                     Description
 *    ------------------------------------------------------------------------
 *     :store       | :no                     | Don't store field
 *                  |                         |
 *                  | :yes (default)          | Store field in its original
 *                  |                         | format. Use this value if you
 *                  |                         | want to highlight matches.
 *                  |                         | or print match excerpts a la
 *                  |                         | Google search.
 *                  |                         |
 *                  | :compressed             | Store field in compressed
 *                  |                         | format.
 *     -------------|-------------------------|------------------------------
 *     :index       | :no                     | Do not make this field
 *                  |                         | searchable.
 *                  |                         |
 *                  | :yes (default)          | Make this field searchable and
 *                  |                         | tokenized its contents.
 *                  |                         |
 *                  | :untokenized            | Make this field searchable but
 *                  |                         | do not tokenize its contents. 
 *                  |                         | use this value for fields you
 *                  |                         | wish to sort by.
 *                  |                         |
 *                  | :omit_norms             | Same as :yes except omit the
 *                  |                         | norms file. The norms file can
 *                  |                         | be omitted if you don't boost
 *                  |                         | any fields and you don't need
 *                  |                         | scoring based on field length.
 *                  |                         | 
 *                  | :untokenized_omit_norms | Same as :untokenized except omit
 *                  |                         | the norms file. Norms files can
 *                  |                         | be omitted if you don't boost
 *                  |                         | any fields and you don't need
 *                  |                         | scoring based on field length.
 *                  |                         |
 *     -------------|-------------------------|------------------------------
 *     :term_vector | :no                     | Don't store term-vectors
 *                  |                         |
 *                  | :yes                    | Store term-vectors without
 *                  |                         | storing positions or offsets.
 *                  |                         |
 *                  | :with_positions         | Store term-vectors with
 *                  |                         | positions.
 *                  |                         |
 *                  | :with_offsets           | Store term-vectors with
 *                  |                         | offsets.
 *                  |                         |
 *                  | :with_positions_offsets | Store term-vectors with
 *                  | (default)               | positions and offsets.
 *     -------------|-------------------------|------------------------------
 *     :boost       | Float                   | The boost property is used to
 *                  |                         | set the default boost for a
 *                  |                         | field. This boost value will
 *                  |                         | used for all instances of the
 *                  |                         | field in the index unless 
 *                  |                         | otherwise specified when you
 *                  |                         | create the field. All values
 *                  |                         | should be positive.
 *                  |                         | 
 *
 *  == Examples
 *
 *    fi = FieldInfo.new(:title, :index => :untokenized, :term_vector => :no,
 *                       :boost => 10.0)
 *
 *    fi = FieldInfo.new(:content)
 *
 *    fi = FieldInfo.new(:created_on, :index => :untokenized_omit_norms,
 *                       :term_vector => :no)
 *
 *    fi = FieldInfo.new(:image, :store => :compressed, :index => :no,
 *                       :term_vector => :no)
 */
static void
Init_FieldInfo(void)
{
    sym_store = ID2SYM(rb_intern("store"));
    sym_index = ID2SYM(rb_intern("index"));
    sym_term_vector = ID2SYM(rb_intern("term_vector"));

    sym_compress = ID2SYM(rb_intern("compress"));
    sym_compressed = ID2SYM(rb_intern("compressed"));

    sym_untokenized = ID2SYM(rb_intern("untokenized"));
    sym_omit_norms = ID2SYM(rb_intern("omit_norms"));
    sym_untokenized_omit_norms = ID2SYM(rb_intern("untokenized_omit_norms"));

    sym_with_positions = ID2SYM(rb_intern("with_positions"));
    sym_with_offsets = ID2SYM(rb_intern("with_offsets"));
    sym_with_positions_offsets = ID2SYM(rb_intern("with_positions_offsets"));

    cFieldInfo = rb_define_class_under(mIndex, "FieldInfo", rb_cObject);
    rb_define_alloc_func(cFieldInfo, frt_data_alloc);

    rb_define_method(cFieldInfo, "initialize",  frt_fi_init, -1);
    rb_define_method(cFieldInfo, "name",        frt_fi_name, 0);
    rb_define_method(cFieldInfo, "stored?",     frt_fi_is_stored, 0);
    rb_define_method(cFieldInfo, "compressed?", frt_fi_is_compressed, 0);
    rb_define_method(cFieldInfo, "indexed?",    frt_fi_is_indexed, 0);
    rb_define_method(cFieldInfo, "tokenized?",  frt_fi_is_tokenized, 0);
    rb_define_method(cFieldInfo, "omit_norms?", frt_fi_omit_norms, 0);
    rb_define_method(cFieldInfo, "store_term_vector?",
                                                frt_fi_store_term_vector, 0);
    rb_define_method(cFieldInfo, "store_positions?",
                                                frt_fi_store_positions, 0);
    rb_define_method(cFieldInfo, "store_offsets?",
                                                frt_fi_store_offsets, 0);
    rb_define_method(cFieldInfo, "has_norms?",  frt_fi_has_norms, 0);
    rb_define_method(cFieldInfo, "boost",       frt_fi_boost, 0);
    rb_define_method(cFieldInfo, "to_s",        frt_fi_to_s, 0);
}

/*
 *  Document-class: Ferret::Index::FieldInfos
 *  
 *  == Summary
 *
 *  The FieldInfos class holds all the field descriptors for an index. It is
 *  this class that is used to create a new index using the
 *  FieldInfos#create_index method. If you are happy with the default
 *  properties for FieldInfo then you don't need to worry about this class.
 *  IndexWriter can create the index for you. Otherwise you should set up the
 *  index like in the example;
 *
 *  == Example
 *
 *    field_infos = FieldInfos.new(:term_vector => :no)
 *
 *    field_infos.add_field(:title, :index => :untokenized, :term_vector => :no,
 *                          :boost => 10.0)
 *
 *    field_infos.add_field(:content)
 *
 *    field_infos.add_field(:created_on, :index => :untokenized_omit_norms,
 *                          :term_vector => :no)
 *
 *    field_infos.add_field(:image, :store => :compressed, :index => :no,
 *                          :term_vector => :no)
 *
 *    field_infos.create_index("/path/to/index")
 *
 *  == Default Properties
 *
 *  See FieldInfo for the available field property values.
 *
 *  When you create the FieldInfos object you specify the default properties
 *  for the fields. Often you'll specify all of the fields in the index before
 *  you create the index so the default values won't come into play. However,
 *  it is possible to continue to dynamically add fields as indexing goes
 *  along. If you add a document to the index which has fields that the index
 *  doesn't know about then the default properties are used for the new field.
 */
static void
Init_FieldInfos(void)
{
    Init_FieldInfo();

    cFieldInfos = rb_define_class_under(mIndex, "FieldInfos", rb_cObject);
    rb_define_alloc_func(cFieldInfos, frt_data_alloc);

    rb_define_method(cFieldInfos, "initialize", frt_fis_init, -1);
    rb_define_method(cFieldInfos, "to_a",       frt_fis_to_a, 0);
    rb_define_method(cFieldInfos, "[]",         frt_fis_get, 1);
    rb_define_method(cFieldInfos, "add",        frt_fis_add, 1);
    rb_define_method(cFieldInfos, "<<",         frt_fis_add, 1);
    rb_define_method(cFieldInfos, "add_field",  frt_fis_add_field, -1);
    rb_define_method(cFieldInfos, "each",       frt_fis_each, 0);
    rb_define_method(cFieldInfos, "to_s",       frt_fis_to_s, 0);
    rb_define_method(cFieldInfos, "size",       frt_fis_size, 0);
    rb_define_method(cFieldInfos, "create_index",
                                                frt_fis_create_index, 1);
    rb_define_method(cFieldInfos, "fields",     frt_fis_get_fields, 0);
    rb_define_method(cFieldInfos, "tokenized_fields", frt_fis_get_tk_fields, 0);
}

/*
 *  Document-class: Ferret::Index::TermEnum
 *
 *  == Summary
 *
 *  The TermEnum object is used to iterate through the terms in a field. To
 *  get a TermEnum you need to use the IndexReader#terms(field) method. 
 *  
 *  == Example
 *
 *    te = index_reader.terms(:content)
 *
 *    te.each {|term, doc_freq| puts "#{term} occurred #{doc_freq} times" }
 *
 *    # or you could do it like this;
 *    te = index_reader.terms(:content)
 *
 *    while te.next?
 *      puts "#{te.term} occured in #{te.doc_freq} documents in the index"
 *    end
 */
static void
Init_TermEnum(void)
{
    id_term = rb_intern("@term");

    cTermEnum = rb_define_class_under(mIndex, "TermEnum", rb_cObject);
    rb_define_alloc_func(cTermEnum, frt_data_alloc);

    rb_define_method(cTermEnum, "next?",    frt_te_next, 0);
    rb_define_method(cTermEnum, "term",     frt_te_term, 0);
    rb_define_method(cTermEnum, "doc_freq", frt_te_doc_freq, 0);
    rb_define_method(cTermEnum, "skip_to",  frt_te_skip_to, 1);
    rb_define_method(cTermEnum, "each",     frt_te_each, 0);
    rb_define_method(cTermEnum, "field=",   frt_te_set_field, 1);
    rb_define_method(cTermEnum, "set_field",frt_te_set_field, 1);
    rb_define_method(cTermEnum, "to_json",  frt_te_to_json, -1);
}

/*
 *  Document-class: Ferret::Index::TermDocEnum
 *
 *  == Summary
 *
 *  Use a TermDocEnum to iterate through the documents that contain a
 *  particular term. You can also iterate through the positions which the term
 *  occurs in a document.
 *
 *
 *  == Example
 *
 *    tde = index_reader.term_docs_for(:content, "fox")
 *
 *    tde.each do |doc_id, freq|
 *      puts "fox appeared #{freq} times in document #{doc_id}:"
 *      positions = []
 *      tde.each_position {|pos| positions << pos}
 *      puts "  #{positions.join(', ')}"
 *    end
 *
 *    # or you can do it like this;
 *    tde.seek(:title, "red")
 *    while tde.next?
 *      puts "red appeared #{tde.freq} times in document #{tde.doc}:"
 *      positions = []
 *      while pos = tde.next_position
 *        positions << pos
 *      end
 *      puts "  #{positions.join(', ')}"
 *    end
 */
static void
Init_TermDocEnum(void)
{
    id_fld_num_map = rb_intern("@field_num_map");
    id_field_num = rb_intern("@field_num");

    cTermDocEnum = rb_define_class_under(mIndex, "TermDocEnum", rb_cObject);
    rb_define_alloc_func(cTermDocEnum, frt_data_alloc);
    rb_define_method(cTermDocEnum, "seek",           frt_tde_seek, 2);
    rb_define_method(cTermDocEnum, "seek_term_enum", frt_tde_seek_te, 1);
    rb_define_method(cTermDocEnum, "doc",            frt_tde_doc, 0);
    rb_define_method(cTermDocEnum, "freq",           frt_tde_freq, 0);
    rb_define_method(cTermDocEnum, "next?",          frt_tde_next, 0);
    rb_define_method(cTermDocEnum, "next_position",  frt_tde_next_position, 0);
    rb_define_method(cTermDocEnum, "each",           frt_tde_each, 0);
    rb_define_method(cTermDocEnum, "each_position",  frt_tde_each_position, 0);
    rb_define_method(cTermDocEnum, "skip_to",        frt_tde_skip_to, 1);
    rb_define_method(cTermDocEnum, "to_json",        frt_tde_to_json, -1);
}

/* rdochack
cTermVector = rb_define_class_under(mIndex, "TermVector", rb_cObject);
*/

/*
 *  Document-class: Ferret::Index::TermVector::TVOffsets
 *
 *  == Summary
 *
 *  Holds the start and end byte-offsets of a term in a field. For example, if
 *  the field was "the quick brown fox" then the start and end offsets of:
 *
 *    ["the", "quick", "brown", "fox"]
 *
 *  Would be:
 *
 *    [(0,3), (4,9), (10,15), (16,19)]
 *
 *  See the Analysis module for more information on setting the offsets.
 */
static void
Init_TVOffsets(void)
{
    const char *tv_offsets_class = "TVOffsets";
    /* rdochack
    cTVOffsets = rb_define_class_under(cTermVector, "TVOffsets", rb_cObject);
    */
    cTVOffsets = rb_struct_define(tv_offsets_class, "start", "end", NULL);
    rb_set_class_path(cTVOffsets, cTermVector, tv_offsets_class);
    rb_const_set(mIndex, rb_intern(tv_offsets_class), cTVOffsets);
}

/*
 *  Document-class: Ferret::Index::TermVector::TVTerm
 *
 *  == Summary
 *
 *  The TVTerm class holds the term information for each term in a TermVector.
 *  That is it holds the term's text and its positions in the document. You
 *  can use those positions to reference the offsets for the term.
 *
 *  == Example
 *
 *    tv = index_reader.term_vector(:content)
 *    tv_term = tv.find {|tvt| tvt.term = "fox"}
 *    offsets = tv_term.positions.collect {|pos| tv.offsets[pos]}
 */
static void
Init_TVTerm(void)
{
    const char *tv_term_class = "TVTerm";
    /* rdochack
    cTVTerm = rb_define_class_under(cTermVector, "TVTerm", rb_cObject);
    */
    cTVTerm = rb_struct_define(tv_term_class, "text", "positions", NULL);
    rb_set_class_path(cTVTerm, cTermVector, tv_term_class);
    rb_const_set(mIndex, rb_intern(tv_term_class), cTVTerm);
}

/*
 *  Document-class: Ferret::Index::TermVector
 *
 *  == Summary
 *
 *  TermVectors are most commonly used for creating search result excerpts and
 *  highlight search matches in results. This is all done internally so you
 *  won't need to worry about the TermVector object. There are some other
 *  reasons you may want to use the TermVectors object however. For example,
 *  you may wish to see which terms are the most commonly occurring terms in a
 *  document to implement a MoreLikeThis search.
 *
 *  == Example
 *
 *    tv = index_reader.term_vector(:content)
 *    tv_term = tv.find {|tvt| tvt.term = "fox"}
 *
 *    # get the term frequency
 *    term_freq = tv_term.positions.size
 *
 *    # get the offsets for a term
 *    offsets = tv_term.positions.collect {|pos| tv.offsets[pos]}
 *
 *  == Note
 *
 *  +positions+ and +offsets+ can be +nil+ depending on what you set the
 *  +:term_vector+ to when you set the FieldInfo object for the field. Note in
 *  particular that you need to store both positions and offsets if you want
 *  to associate offsets with particular terms.
 */
static void
Init_TermVector(void)
{
    const char *tv_class = "TermVector";
    /* rdochack
    cTermVector = rb_define_class_under(mIndex, "TermVector", rb_cObject);
    */
    cTermVector = rb_struct_define(tv_class,
                                   "field", "terms", "offsets", NULL);
    rb_set_class_path(cTermVector, mIndex, tv_class);
    rb_const_set(mIndex, rb_intern(tv_class), cTermVector);

    Init_TVOffsets();
    Init_TVTerm();
}

/*
 *  Document-class: Ferret::Index::IndexWriter
 *
 *  == Summary
 *
 *  The IndexWriter is the class used to add documents to an index. You can
 *  also delete documents from the index using this class. The indexing
 *  process is highly customizable and the IndexWriter has the following
 *  parameters;
 *
 *  dir::                 This is an Ferret::Store::Directory object. You
 *                        should either pass a +:dir+ or a +:path+ when
 *                        creating an index.
 *  path::                A string representing the path to the index
 *                        directory. If you are creating the index for the
 *                        first time the directory will be created if it's
 *                        missing. You should not choose a directory which
 *                        contains other files as they could be over-written.
 *                        To protect against this set +:create_if_missing+ to
 *                        false.
 *  create_if_missing::   Default: true. Create the index if no index is
 *                        found in the specified directory. Otherwise, use
 *                        the existing index.
 *  create::              Default: false. Creates the index, even if one
 *                        already exists.  That means any existing index will
 *                        be deleted. It is probably better to use the
 *                        create_if_missing option so that the index is only
 *                        created the first time when it doesn't exist.
 *  field_infos::         Default FieldInfos.new. The FieldInfos object to use
 *                        when creating a new index if +:create_if_missing+ or
 *                        +:create+ is set to true. If an existing index is
 *                        opened then this parameter is ignored.
 *  analyzer::            Default: Ferret::Analysis::StandardAnalyzer.
 *                        Sets the default analyzer for the index. This is
 *                        used by both the IndexWriter and the QueryParser
 *                        to tokenize the input. The default is the
 *                        StandardAnalyzer.
 *  chunk_size::          Default: 0x100000 or 1Mb. Memory performance tuning
 *                        parameter. Sets the default size of chunks of memory
 *                        malloced for use during indexing. You can usually
 *                        leave this parameter as is.
 *  max_buffer_memory::   Default: 0x1000000 or 16Mb. Memory performance
 *                        tuning parameter. Sets the amount of memory to be
 *                        used by the indexing process. Set to a larger value
 *                        to increase indexing speed. Note that this only
 *                        includes memory used by the indexing process, not
 *                        the rest of your ruby application.
 *  term_index_interval:: Default: 128. The skip interval between terms in the
 *                        term dictionary. A smaller value will possibly 
 *                        increase search performance while also increasing
 *                        memory usage and impacting negatively impacting
 *                        indexing performance.
 *  doc_skip_interval::   Default: 16. The skip interval for document numbers
 *                        in the index. As with +:term_index_interval+ you
 *                        have a trade-off. A smaller number may increase
 *                        search performance while also increasing memory
 *                        usage and impacting negatively impacting indexing
 *                        performance.
 *  merge_factor::        Default: 10. This must never be less than 2.
 *                        Specifies the number of segments of a certain size
 *                        that must exist before they are merged. A larger
 *                        value will improve indexing performance while
 *                        slowing search performance.
 *  max_buffered_docs::   Default: 10000. The maximum number of documents that
 *                        may be stored in memory before being written to the
 *                        index. If you have a lot of memory and are indexing
 *                        a large number of small documents (like products in
 *                        a product database for example) you may want to set
 *                        this to a much higher number (like
 *                        Ferret::FIX_INT_MAX). If you are worried about your
 *                        application crashing during the middle of index you
 *                        might set this to a smaller number so that the index
 *                        is committed more often. This is like having an
 *                        auto-save in a word processor application.
 *  max_merge_docs::      Set this value to limit the number of documents that
 *                        go into a single segment. Use this to avoid
 *                        extremely long merge times during indexing which can
 *                        make your application seem unresponsive. This is
 *                        only necessary for very large indexes (millions of
 *                        documents).
 *  max_field_length::    Default: 10000. The maximum number of terms added to
 *                        a single field.  This can be useful to protect the
 *                        indexer when indexing documents from the web for
 *                        example. Usually the most important terms will occur
 *                        early on in a document so you can often safely
 *                        ignore the terms in a field after a certain number
 *                        of them. If you wanted to speed up indexing and same
 *                        space in your index you may only want to index the
 *                        first 1000 terms in a field. On the other hand, if
 *                        you want to be more thorough and you are indexing
 *                        documents from your file-system you may set this
 *                        parameter to Ferret::FIX_INT_MAX.
 *  use_compound_file::   Default: true. Uses a compound file to store the
 *                        index. This prevents an error being raised for
 *                        having too many files open at the same time. The
 *                        default is true but performance is better if this is
 *                        set to false.
 *
 *
 *  === Deleting Documents
 *
 *  Both IndexReader and IndexWriter allow you to delete documents. You should
 *  use the IndexReader to delete documents by document id and IndexWriter to
 *  delete documents by term which we'll explain now. It is preferrable to
 *  delete documents from an index using IndexWriter for performance reasons.
 *  To delete documents using the IndexWriter you should give each document in
 *  the index a unique ID. If you are indexing documents from the file-system
 *  this unique ID will be the full file path. If indexing documents from the
 *  database you should use the primary key as the ID field. You can then
 *  use the delete method to delete a file referenced by the ID. For example;
 *
 *    index_writer.delete(:id, "/path/to/indexed/file")
 */
void
Init_IndexWriter(void)
{
    id_boost = rb_intern("boost");

    sym_create              = ID2SYM(rb_intern("create"));
    sym_create_if_missing   = ID2SYM(rb_intern("create_if_missing"));
    sym_field_infos         = ID2SYM(rb_intern("field_infos"));

    sym_chunk_size          = ID2SYM(rb_intern("chunk_size"));
    sym_max_buffer_memory   = ID2SYM(rb_intern("max_buffer_memory"));
    sym_index_interval      = ID2SYM(rb_intern("term_index_interval"));
    sym_skip_interval       = ID2SYM(rb_intern("doc_skip_interval"));
    sym_merge_factor        = ID2SYM(rb_intern("merge_factor"));
    sym_max_buffered_docs   = ID2SYM(rb_intern("max_buffered_docs"));
    sym_max_merge_docs      = ID2SYM(rb_intern("max_merge_docs"));
    sym_max_field_length    = ID2SYM(rb_intern("max_field_length"));
    sym_use_compound_file   = ID2SYM(rb_intern("use_compound_file"));

    cIndexWriter = rb_define_class_under(mIndex, "IndexWriter", rb_cObject);
    rb_define_alloc_func(cIndexWriter, frt_data_alloc);

    rb_define_const(cIndexWriter, "WRITE_LOCK_TIMEOUT", INT2FIX(1));
    rb_define_const(cIndexWriter, "COMMIT_LOCK_TIMEOUT", INT2FIX(10));
    rb_define_const(cIndexWriter, "WRITE_LOCK_NAME",
                    rb_str_new2(WRITE_LOCK_NAME));
    rb_define_const(cIndexWriter, "COMMIT_LOCK_NAME",
                    rb_str_new2(COMMIT_LOCK_NAME));
    rb_define_const(cIndexWriter, "DEFAULT_CHUNK_SIZE",
                    INT2FIX(default_config.chunk_size)); 
    rb_define_const(cIndexWriter, "DEFAULT_MAX_BUFFER_MEMORY",
                    INT2FIX(default_config.max_buffer_memory)); 
    rb_define_const(cIndexWriter, "DEFAULT_TERM_INDEX_INTERVAL",
                    INT2FIX(default_config.index_interval));
    rb_define_const(cIndexWriter, "DEFAULT_DOC_SKIP_INTERVAL",
                    INT2FIX(default_config.skip_interval));
    rb_define_const(cIndexWriter, "DEFAULT_MERGE_FACTOR",
                    INT2FIX(default_config.merge_factor)); 
    rb_define_const(cIndexWriter, "DEFAULT_MAX_BUFFERED_DOCS",
                    INT2FIX(default_config.max_buffered_docs));
    rb_define_const(cIndexWriter, "DEFAULT_MAX_MERGE_DOCS",
                    INT2FIX(default_config.max_merge_docs));
    rb_define_const(cIndexWriter, "DEFAULT_MAX_FIELD_LENGTH",
                    INT2FIX(default_config.max_field_length));
    rb_define_const(cIndexWriter, "DEFAULT_USE_COMPOUND_FILE",
                    default_config.use_compound_file ? Qtrue : Qfalse);

    rb_define_method(cIndexWriter, "initialize",    frt_iw_init, -1);
    rb_define_method(cIndexWriter, "doc_count",     frt_iw_get_doc_count, 0);
    rb_define_method(cIndexWriter, "close",         frt_iw_close, 0);
    rb_define_method(cIndexWriter, "add_document",  frt_iw_add_doc, 1);
    rb_define_method(cIndexWriter, "<<",            frt_iw_add_doc, 1);
    rb_define_method(cIndexWriter, "optimize",      frt_iw_optimize, 0);
    rb_define_method(cIndexWriter, "commit",        frt_iw_commit, 0);
    rb_define_method(cIndexWriter, "add_readers",   frt_iw_add_readers, 1);
    rb_define_method(cIndexWriter, "delete",        frt_iw_delete, 2);
    rb_define_method(cIndexWriter, "field_infos",   frt_iw_field_infos, 0);
    rb_define_method(cIndexWriter, "analyzer",      frt_iw_get_analyzer, 0);
    rb_define_method(cIndexWriter, "analyzer=",     frt_iw_set_analyzer, 1);
    rb_define_method(cIndexWriter, "version",       frt_iw_version, 0);

    rb_define_method(cIndexWriter, "chunk_size", 
                     frt_iw_get_chunk_size, 0);
    rb_define_method(cIndexWriter, "chunk_size=",
                     frt_iw_set_chunk_size, 1);

    rb_define_method(cIndexWriter, "max_buffer_memory", 
                     frt_iw_get_max_buffer_memory, 0);
    rb_define_method(cIndexWriter, "max_buffer_memory=",
                     frt_iw_set_max_buffer_memory, 1);

    rb_define_method(cIndexWriter, "term_index_interval",
                     frt_iw_get_index_interval, 0);
    rb_define_method(cIndexWriter, "term_index_interval=",
                     frt_iw_set_index_interval, 1);

    rb_define_method(cIndexWriter, "doc_skip_interval",
                     frt_iw_get_skip_interval, 0);
    rb_define_method(cIndexWriter, "doc_skip_interval=",
                     frt_iw_set_skip_interval, 1);

    rb_define_method(cIndexWriter, "merge_factor", 
                     frt_iw_get_merge_factor, 0);
    rb_define_method(cIndexWriter, "merge_factor=",
                     frt_iw_set_merge_factor, 1);

    rb_define_method(cIndexWriter, "max_buffered_docs", 
                     frt_iw_get_max_buffered_docs, 0);
    rb_define_method(cIndexWriter, "max_buffered_docs=",
                     frt_iw_set_max_buffered_docs, 1);

    rb_define_method(cIndexWriter, "max_merge_docs",
                     frt_iw_get_max_merge_docs, 0);
    rb_define_method(cIndexWriter, "max_merge_docs=",
                     frt_iw_set_max_merge_docs, 1);

    rb_define_method(cIndexWriter, "max_field_length",
                     frt_iw_get_max_field_length, 0);
    rb_define_method(cIndexWriter, "max_field_length=",
                     frt_iw_set_max_field_length, 1);

    rb_define_method(cIndexWriter, "use_compound_file",
                     frt_iw_get_use_compound_file, 0);
    rb_define_method(cIndexWriter, "use_compound_file=",
                     frt_iw_set_use_compound_file, 1);

}

/*
 *  Document-class: Ferret::Index::LazyDoc
 *
 *  == Summary
 *
 *  When a document is retrieved from the index a LazyDoc is returned.
 *  Actually, LazyDoc is just a modified Hash object which lazily adds fields
 *  to itself when they are accessed. You should not that they keys method
 *  will return nothing until you actually access one of the fields. To see
 *  what fields are available use LazyDoc#fields rather than LazyDoc#keys. To
 *  load all fields use the LazyDoc#load method.
 *
 *  == Example
 *
 *    doc = index_reader[0]
 *
 *    doc.keys     #=> []
 *    doc.values   #=> []
 *    doc.fields   #=> [:title, :content]
 *
 *    title = doc[:title] #=> "the title"
 *    doc.keys     #=> [:title]
 *    doc.values   #=> ["the title"]
 *    doc.fields   #=> [:title, :content]
 *
 *    doc.load
 *    doc.keys     #=> [:title, :content]
 *    doc.values   #=> ["the title", "the content"]
 *    doc.fields   #=> [:title, :content]
 */
void
Init_LazyDoc(void)
{
    id_fields = rb_intern("@fields");


    cLazyDoc = rb_define_class_under(mIndex, "LazyDoc", rb_cHash);
    rb_define_method(cLazyDoc, "default",   frt_lzd_default, 1);
    rb_define_method(cLazyDoc, "load",      frt_lzd_load, 0);
    rb_define_method(cLazyDoc, "fields",    frt_lzd_fields, 0);

    cLazyDocData = rb_define_class_under(cLazyDoc, "LazyDocData", rb_cObject);
    rb_define_alloc_func(cLazyDocData, frt_data_alloc);
}

/*
 *  Document-class: Ferret::Index::IndexReader
 *
 *  == Summary
 *
 *  IndexReader is used for reading data from the index. This class is usually
 *  used directly for more advanced tasks like iterating through terms in an
 *  index, accessing term-vectors or deleting documents by document id. It is
 *  also used internally by IndexSearcher.
 */
void
Init_IndexReader(void)
{
    cIndexReader = rb_define_class_under(mIndex, "IndexReader", rb_cObject);
    rb_define_alloc_func(cIndexReader, frt_data_alloc);
    rb_define_method(cIndexReader, "initialize",    frt_ir_init, 1);
    rb_define_method(cIndexReader, "set_norm",      frt_ir_set_norm, 3);
    rb_define_method(cIndexReader, "norms",         frt_ir_norms, 1);
    rb_define_method(cIndexReader, "get_norms_into",frt_ir_get_norms_into, 3);
    rb_define_method(cIndexReader, "commit",        frt_ir_commit, 0);
    rb_define_method(cIndexReader, "close",         frt_ir_close, 0);
    rb_define_method(cIndexReader, "has_deletions?",frt_ir_has_deletions, 0);
    rb_define_method(cIndexReader, "delete",        frt_ir_delete, 1);
    rb_define_method(cIndexReader, "deleted?",      frt_ir_is_deleted, 1);
    rb_define_method(cIndexReader, "max_doc",       frt_ir_max_doc, 0);
    rb_define_method(cIndexReader, "num_docs",      frt_ir_num_docs, 0);
    rb_define_method(cIndexReader, "undelete_all",  frt_ir_undelete_all, 0);
    rb_define_method(cIndexReader, "latest?",       frt_ir_is_latest, 0);
    rb_define_method(cIndexReader, "get_document",  frt_ir_get_doc, -1);
    rb_define_method(cIndexReader, "[]",            frt_ir_get_doc, -1);
    rb_define_method(cIndexReader, "term_vector",   frt_ir_term_vector, 2);
    rb_define_method(cIndexReader, "term_vectors",  frt_ir_term_vectors, 1);
    rb_define_method(cIndexReader, "term_docs",     frt_ir_term_docs, 0);
    rb_define_method(cIndexReader, "term_positions",frt_ir_term_positions, 0);
    rb_define_method(cIndexReader, "term_docs_for", frt_ir_term_docs_for, 2);
    rb_define_method(cIndexReader, "term_positions_for", frt_ir_t_pos_for, 2);
    rb_define_method(cIndexReader, "doc_freq",      frt_ir_doc_freq, 2);
    rb_define_method(cIndexReader, "terms",         frt_ir_terms, 1);
    rb_define_method(cIndexReader, "terms_from",    frt_ir_terms_from, 2);
    rb_define_method(cIndexReader, "term_count",    frt_ir_term_count, 1);
    rb_define_method(cIndexReader, "fields",        frt_ir_fields, 0);
    rb_define_method(cIndexReader, "field_names",   frt_ir_fields, 0);
    rb_define_method(cIndexReader, "field_infos",   frt_ir_field_infos, 0);
    rb_define_method(cIndexReader, "tokenized_fields", frt_ir_tk_fields, 0);
    rb_define_method(cIndexReader, "version",       frt_ir_version, 0);
}

/* rdoc hack
extern VALUE mFerret = rb_define_module("Ferret");
*/

/*
 *  Document-module: Ferret::Index
 *
 *  == Summary
 *
 *  The Index module contains all the classes used for adding to and
 *  retrieving from the index. The important classes to know about are;
 *
 *  * FieldInfo
 *  * FieldInfos
 *  * IndexWriter
 *  * IndexReader
 *  * LazyDoc
 *
 *  The other classes in this module are useful for more advanced uses like
 *  building tag clouds, creating more-like-this queries, custom highlighting
 *  etc. They are also useful for index browsers.
 */
void
Init_Index(void)
{
    mIndex = rb_define_module_under(mFerret, "Index");

    sym_boost     = ID2SYM(rb_intern("boost"));
    sym_analyzer  = ID2SYM(rb_intern("analyzer"));
    sym_close_dir = ID2SYM(rb_intern("close_dir"));

    Init_TermVector();
    Init_TermEnum();
    Init_TermDocEnum();

    Init_FieldInfos();

    Init_LazyDoc();
    Init_IndexWriter();
    Init_IndexReader();
}
