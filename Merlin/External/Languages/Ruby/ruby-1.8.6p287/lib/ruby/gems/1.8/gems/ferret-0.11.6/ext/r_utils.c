#include "ferret.h"
#include "bitvector.h"

/*****************
 *** BitVector ***
 *****************/
static VALUE cBitVector;

static void
frt_bv_free(void *p)
{
    object_del(p);
    bv_destroy((BitVector *)p);
}

static VALUE
frt_bv_alloc(VALUE klass)
{
    BitVector *bv = bv_new();
    VALUE rbv = Data_Wrap_Struct(klass, NULL, &frt_bv_free, bv);
    object_add(bv, rbv);
    return rbv;
}

#define GET_BV(bv, self) Data_Get_Struct(self, BitVector, bv)

VALUE
frt_get_bv(BitVector *bv)
{
    VALUE rbv;
    if ((rbv = object_get(bv)) == Qnil) {
        rbv = Data_Wrap_Struct(cBitVector, NULL, &frt_bv_free, bv);
        REF(bv);
        object_add(bv, rbv);
    }
    return rbv;
}

/*
 *  call-seq:
 *     BitVector.new() -> new_bv
 *  
 *  Returns a new empty bit vector object
 */
static VALUE 
frt_bv_init(VALUE self)
{
    return self;
}

/*
 *  call-seq:
 *     bv[i] = bool  -> bool
 *  
 *  Set the bit and _i_ to *val* (+true+ or 
 *  +false+).
 */
VALUE
frt_bv_set(VALUE self, VALUE rindex, VALUE rstate)
{
    BitVector *bv;
    int index = FIX2INT(rindex);
    GET_BV(bv, self);
    if (index < 0) {
        rb_raise(rb_eIndexError, "%d < 0", index);
    }
    if (RTEST(rstate)) {
        bv_set(bv, index);
    }
    else {
        bv_unset(bv, index);
    }

    return rstate;
}

/*
 *  call-seq:
 *     bv.set(i) -> self
 *  
 *  Set the bit at _i_ to *on* (+true+)
 */
VALUE
frt_bv_set_on(VALUE self, VALUE rindex)
{
    frt_bv_set(self, rindex, Qtrue);
    return self;
}

/*
 *  call-seq:
 *     bv.unset(i) -> self
 *  
 *  Set the bit at _i_ to *off* (+false+)
 */
VALUE
frt_bv_set_off(VALUE self, VALUE rindex)
{
    frt_bv_set(self, rindex, Qfalse);
    return self;
}

/*
 *  call-seq:
 *     bv.get(i) -> bool
 *     bv[i]     -> bool
 *  
 *  Get the bit value at _i_
 */
VALUE
frt_bv_get(VALUE self, VALUE rindex)
{
    BitVector *bv;
    int index = FIX2INT(rindex);
    GET_BV(bv, self);
    if (index < 0) {
        rb_raise(rb_eIndexError, "%d < 0", index);
    }

    return bv_get(bv, index) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     bv.count -> bit_count
 *  
 *  Count the number of bits set in the bit vector. If the bit vector has been
 *  negated using +#not+ then count the number of unset bits
 *  instead.
 */
VALUE
frt_bv_count(VALUE self)
{
    BitVector *bv;
    GET_BV(bv, self);
    return INT2FIX(bv->count);
}

/*
 *  call-seq:
 *     bv.clear -> self
 *  
 *  Clears all set bits in the bit vector. Negated bit vectors will still have
 *  all bits set to *off*.
 */
VALUE
frt_bv_clear(VALUE self)
{
    BitVector *bv;
    GET_BV(bv, self);
    bv_clear(bv);
    bv_scan_reset(bv);
    return self;
}

/*
 *  call-seq:
 *     bv1 == bv2    -> bool
 *     bv1 != bv2    -> bool
 *     bv1.eql(bv2)  -> bool
 *  
 *  Compares two bit vectors and returns true if both bit vectors have the same
 *  bits set.
 */
VALUE
frt_bv_eql(VALUE self, VALUE other)
{
    BitVector *bv1, *bv2;
    GET_BV(bv1, self);
    GET_BV(bv2, other);
    return bv_eq(bv1, bv2) ? Qtrue : Qfalse;
}

/*
 *  call-seq:
 *     bv.hash -> int
 *  
 *  Used to store bit vectors in Hashes. Especially useful if you want to
 *  cache them.
 */
VALUE
frt_bv_hash(VALUE self)
{
    BitVector *bv;
    GET_BV(bv, self);
    return LONG2NUM(bv_hash(bv));
}

/*
 *  call-seq:
 *     bv1 & bv2    -> anded_bv
 *     bv1.and(bv2) -> anded_bv
 *  
 *  Perform a boolean _and_ operation on +bv1+ and
 *  +bv2+
 */
VALUE
frt_bv_and(VALUE self, VALUE other)
{
    BitVector *bv1, *bv2;
    GET_BV(bv1, self);
    GET_BV(bv2, other);
    return Data_Wrap_Struct(cBitVector, NULL, &bv_destroy, bv_and(bv1, bv2));
}

/*
 *  call-seq:
 *     bv1.and!(bv2) -> self
 *  
 *  Perform a boolean _and_ operation on +bv1+ and
 *  +bv2+ in place on +bv1+
 */
VALUE
frt_bv_and_x(VALUE self, VALUE other)
{
    BitVector *bv1, *bv2;
    GET_BV(bv1, self);
    GET_BV(bv2, other);
    bv_and_x(bv1, bv2);
    return self;
}

/*
 *  call-seq:
 *     bv1 | bv2   -> ored_bv
 *     bv1.or(bv2) -> ored_bv
 *  
 *  Perform a boolean _or_ operation on +bv1+ and
 *  +bv2+
 */
VALUE
frt_bv_or(VALUE self, VALUE other)
{
    BitVector *bv1, *bv2;
    GET_BV(bv1, self);
    GET_BV(bv2, other);
    return Data_Wrap_Struct(cBitVector, NULL, &bv_destroy, bv_or(bv1, bv2));
}

/*
 *  call-seq:
 *     bv1.or!(bv2) -> self
 *  
 *  Perform a boolean _or_ operation on +bv1+ and
 *  +bv2+ in place on +bv1+
 */
VALUE
frt_bv_or_x(VALUE self, VALUE other)
{
    BitVector *bv1, *bv2;
    GET_BV(bv1, self);
    GET_BV(bv2, other);
    bv_or_x(bv1, bv2);
    return self;
}

/*
 *  call-seq:
 *     bv1 ^ bv2    -> xored_bv
 *     bv1.xor(bv2) -> xored_bv
 *  
 *  Perform a boolean _xor_ operation on +bv1+ and
 *  +bv2+
 */
VALUE
frt_bv_xor(VALUE self, VALUE other)
{
    BitVector *bv1, *bv2;
    GET_BV(bv1, self);
    GET_BV(bv2, other);
    return Data_Wrap_Struct(cBitVector, NULL, &bv_destroy, bv_xor(bv1, bv2));
}

/*
 *  call-seq:
 *     bv1.xor!(bv2) -> self
 *  
 *  Perform a boolean _xor_ operation on +bv1+ and
 *  +bv2+ in place on +bv1+
 */
VALUE
frt_bv_xor_x(VALUE self, VALUE other)
{
    BitVector *bv1, *bv2;
    GET_BV(bv1, self);
    GET_BV(bv2, other);
    bv_xor_x(bv1, bv2);
    return self;
}

/*
 *  call-seq:
 *     ~bv -> bv
 *     bv.not -> bv
 *  
 *  Perform a boolean _not_ operation on +bv+
 *  */
VALUE
frt_bv_not(VALUE self)
{
    BitVector *bv;
    GET_BV(bv, self);
    return Data_Wrap_Struct(cBitVector, NULL, &bv_destroy, bv_not(bv));
}

/*
 *  call-seq:
 *     bv.not! -> self
 *  
 *  Perform a boolean _not_ operation on +bv+ in-place
 */
VALUE
frt_bv_not_x(VALUE self)
{
    BitVector *bv;
    GET_BV(bv, self);
    bv_not_x(bv);
    return self;
}

/*
 *  call-seq:
 *     bv.reset_scan -> self
 *  
 *  Resets the BitVector ready for scanning. You should call this method
 *  before calling +#next+ or +#next_unset+. It isn't
 *  necessary for the other scan methods or for the +#each+ method.
 */
VALUE
frt_bv_reset_scan(VALUE self)
{
    BitVector *bv;
    GET_BV(bv, self);
    bv_scan_reset(bv);
    return self;
}

/*
 *  call-seq:
 *     bv.next -> bit_num
 *  
 *  Returns the next set bit in the bit vector scanning from low order to high
 *  order. You should call +#reset_scan+ before calling this method
 *  if you want to scan from the beginning. It is automatically reset when you
 *  first create the bit vector.
 */
VALUE
frt_bv_next(VALUE self)
{
    BitVector *bv;
    GET_BV(bv, self);
    return INT2FIX(bv_scan_next(bv));
}

/*
 *  call-seq:
 *     bv.next_unset -> bit_num
 *  
 *  Returns the next unset bit in the bit vector scanning from low order to
 *  high order. This method should only be called on bit vectors which have
 *  been flipped (negated). You should call +#reset_scan+ before
 *  calling this method if you want to scan from the beginning. It is
 *  automatically reset when you first create the bit vector.
 */
VALUE
frt_bv_next_unset(VALUE self)
{
    BitVector *bv;
    GET_BV(bv, self);
    return INT2FIX(bv_scan_next_unset(bv));
}

/*
 *  call-seq:
 *     bv.next_from(from) -> bit_num
 *  
 *  Returns the next set bit in the bit vector scanning from low order to
 *  high order and starting at +from+. The scan is inclusive so if
 *  +from+ is equal to 10 and +bv[10]+ is set it will
 *  return the number 10. If the bit vector has been negated than you should
 *  use the +#next_unset_from+ method.
 */
VALUE
frt_bv_next_from(VALUE self, VALUE rfrom)
{
    BitVector *bv;
    int from = FIX2INT(rfrom);
    GET_BV(bv, self);
    if (from < 0) {
        from = 0;
    }
    return INT2FIX(bv_scan_next_from(bv, from));
}

/*
 *  call-seq:
 *     bv.next_unset_from(from) -> bit_num
 *  
 *  Returns the next unset bit in the bit vector scanning from low order to
 *  high order and starting at +from+. The scan is inclusive so if
 *  +from+ is equal to 10 and +bv[10]+ is unset it will
 *  return the number 10. If the bit vector has not been negated than you
 *  should use the +#next_from+ method.
 */
VALUE
frt_bv_next_unset_from(VALUE self, VALUE rfrom)
{
    BitVector *bv;
    int from = FIX2INT(rfrom);
    GET_BV(bv, self);
    if (from < 0) {
        from = 0;
    }
    return INT2FIX(bv_scan_next_unset_from(bv, from));
}

/*
 *  call-seq:
 *     bv.each { |bit_num| }
 *  
 *  Iterate through all the set bits in the bit vector yielding each one in
 *  order
 */
VALUE
frt_bv_each(VALUE self)
{
    BitVector *bv;
    int bit;
    GET_BV(bv, self);
    bv_scan_reset(bv);
    if (bv->extends_as_ones) {
        while ((bit = bv_scan_next_unset(bv)) >= 0) {
            rb_yield(INT2FIX(bit));
        }
    }
    else {
        while ((bit = bv_scan_next(bv)) >= 0) {
            rb_yield(INT2FIX(bit));
        }
    }
    return self;
}

/*
 *  call-seq:
 *     bv.to_a
 *  
 *  Iterate through all the set bits in the bit vector adding the index of
 *  each set bit to an array. This is useful if you want to perform array
 *  methods on the bit vector. If you want to convert an array to a bit_vector
 *  simply do this;
 *
 *    bv = [1, 12, 45, 367, 455].inject(BitVector.new) {|bv, i| bv.set(i)}
 */
VALUE
frt_bv_to_a(VALUE self)
{
    BitVector *bv;
    int bit;
    VALUE ary;
    GET_BV(bv, self);
    ary = rb_ary_new();
    bv_scan_reset(bv);
    if (bv->extends_as_ones) {
        while ((bit = bv_scan_next_unset(bv)) >= 0) {
            rb_ary_push(ary, INT2FIX(bit));
        }
    }
    else {
        while ((bit = bv_scan_next(bv)) >= 0) {
            rb_ary_push(ary, INT2FIX(bit));
        }
    }
    return ary;
}

static VALUE mUtils;

/*  
 * Document-class: Ferret::Utils::BitVector
 *
 * == Summary
 *
 * A BitVector is pretty easy to implement in Ruby using Ruby's BigNum class.
 * This BitVector however allows you to count the set bits with the
 * +#count+ method (or unset bits of flipped bit vectors) and also
 * to quickly scan the set bits.
 * 
 * == Boolean Operations
 *
 * BitVector handles four boolean operations;
 *
 * * +&+
 * * +|+
 * * +^+ 
 * * +~+
 *
 *    bv1 = BitVector.new
 *    bv2 = BitVector.new
 *    bv3 = BitVector.new
 *
 *    bv4 = (bv1 & bv2) | ~bv3
 *
 * You can also do the operations in-place;
 * 
 * * +and!+
 * * +or!+
 * * +xor!+
 * * +not!+
 *
 *    bv4.and!(bv5).not!
 *
 * == Set Bit Scanning
 *
 * Perhaps the most useful functionality in BitVector is the ability to
 * quickly scan for set bits. To print all set bits;
 *
 *    bv.each {|bit| puts bit }
 *   
 * Alternatively you could use the lower level +next+ or
 * +next_unset+ methods. Note that the +each+ method will
 * automatically scan unset bits if the BitVector has been flipped (using
 * +not+).
 */
static void
Init_BitVector(void)
{
    /* BitVector */
    cBitVector = rb_define_class_under(mUtils, "BitVector", rb_cObject);
    rb_define_alloc_func(cBitVector, frt_bv_alloc);

    rb_define_method(cBitVector, "initialize", frt_bv_init, 0);
    rb_define_method(cBitVector, "set", frt_bv_set_on, 1);
    rb_define_method(cBitVector, "unset", frt_bv_set_off, 1);
    rb_define_method(cBitVector, "[]=", frt_bv_set, 2);
    rb_define_method(cBitVector, "get", frt_bv_get, 1);
    rb_define_method(cBitVector, "[]", frt_bv_get, 1);
    rb_define_method(cBitVector, "count", frt_bv_count, 0);
    rb_define_method(cBitVector, "clear", frt_bv_clear, 0);
    rb_define_method(cBitVector, "eql?", frt_bv_eql, 1);
    rb_define_method(cBitVector, "==", frt_bv_eql, 1);
    rb_define_method(cBitVector, "hash", frt_bv_hash, 0);
    rb_define_method(cBitVector, "and!", frt_bv_and_x, 1);
    rb_define_method(cBitVector, "and", frt_bv_and, 1);
    rb_define_method(cBitVector, "&", frt_bv_and, 1);
    rb_define_method(cBitVector, "or!", frt_bv_or_x, 1);
    rb_define_method(cBitVector, "or", frt_bv_or, 1);
    rb_define_method(cBitVector, "|", frt_bv_or, 1);
    rb_define_method(cBitVector, "xor!", frt_bv_xor_x, 1);
    rb_define_method(cBitVector, "xor", frt_bv_xor, 1);
    rb_define_method(cBitVector, "^", frt_bv_xor, 1);
    rb_define_method(cBitVector, "not!", frt_bv_not_x, 0);
    rb_define_method(cBitVector, "not", frt_bv_not, 0);
    rb_define_method(cBitVector, "~", frt_bv_not, 0);
    rb_define_method(cBitVector, "reset_scan", frt_bv_reset_scan, 0);
    rb_define_method(cBitVector, "next", frt_bv_next, 0);
    rb_define_method(cBitVector, "next_unset", frt_bv_next_unset, 0);
    rb_define_method(cBitVector, "next_from", frt_bv_next_from, 1);
    rb_define_method(cBitVector, "next_unset_from", frt_bv_next_unset_from, 1);
    rb_define_method(cBitVector, "each", frt_bv_each, 0);
    rb_define_method(cBitVector, "to_a", frt_bv_to_a, 0);
}

/*********************
 *** PriorityQueue ***
 *********************/
typedef struct PriQ
{
    int size;
    int capa;
    int mem_capa;
    VALUE *heap;
    VALUE proc;
} PriQ;

#define PQ_START_CAPA 32

static bool frt_pq_lt(VALUE proc, VALUE v1, VALUE v2)
{
    if (proc == Qnil) {
        return RTEST(rb_funcall(v1, id_lt, 1, v2));
    }
    else {
        return RTEST(rb_funcall(proc, id_call, 2, v1, v2));
    }
}

static void pq_up(PriQ *pq)
{
    VALUE *heap = pq->heap;
    VALUE node;
    int i = pq->size;
    int j = i >> 1;

    node = heap[i];

    while ((j > 0) && frt_pq_lt(pq->proc, node, heap[j])) {
        heap[i] = heap[j];
        i = j;
        j = j >> 1;
    }
    heap[i] = node;
}

static void pq_down(PriQ *pq)
{
    register int i = 1;
    register int j = 2;         /* i << 1; */
    register int k = 3;         /* j + 1;  */
    register int size = pq->size;
    VALUE *heap = pq->heap;
    VALUE node = heap[i];       /* save top node */

    if ((k <= size) && (frt_pq_lt(pq->proc, heap[k], heap[j]))) {
        j = k;
    }

    while ((j <= size) && frt_pq_lt(pq->proc, heap[j], node)) {
        heap[i] = heap[j];      /* shift up child */
        i = j;
        j = i << 1;
        k = j + 1;
        if ((k <= size) && frt_pq_lt(pq->proc, heap[k], heap[j])) {
            j = k;
        }
    }
    heap[i] = node;
}

static void pq_push(PriQ *pq, VALUE elem)
{
    pq->size++;
    if (pq->size >= pq->mem_capa) {
        pq->mem_capa <<= 1;
        REALLOC_N(pq->heap, VALUE, pq->mem_capa);
    }
    pq->heap[pq->size] = elem;
    pq_up(pq);
}

static VALUE cPriorityQueue;

static void
frt_pq_mark(void *p)
{
    PriQ *pq = (PriQ *)p;
    int i;
    for (i = pq->size; i > 0; i--) {
        rb_gc_mark_maybe(pq->heap[i]);
    }
}

static void frt_pq_free(PriQ *pq)
{
    free(pq->heap);
    free(pq);
}

static VALUE
frt_pq_alloc(VALUE klass)
{
    PriQ *pq = ALLOC_AND_ZERO(PriQ);
    pq->capa = PQ_START_CAPA;
    pq->mem_capa = PQ_START_CAPA;
    pq->heap = ALLOC_N(VALUE, PQ_START_CAPA);
    pq->proc = Qnil;
    return Data_Wrap_Struct(klass, &frt_pq_mark, &frt_pq_free, pq);
}

#define GET_PQ(pq, self) Data_Get_Struct(self, PriQ, pq)
/*
 *  call-seq:
 *     PriorityQueue.new(capacity = 32) -> new_pq
 *     PriorityQueue.new({:capacity => 32,
 *                        :less_than_proc => lambda{|a, b| a < b}) -> new_pq
 *     PriorityQueue.new({:capacity => 32}) {|a, b| a < b} -> new_pq
 *  
 *  Returns a new empty priority queue object with an optional capacity.
 *  Once the capacity is filled, the lowest valued elements will be
 *  automatically popped off the top of the queue as more elements are
 *  inserted into the queue.
 */
static VALUE 
frt_pq_init(int argc, VALUE *argv, VALUE self)
{
    if (argc >= 1) {
        PriQ *pq;
        VALUE options = argv[0];
        VALUE param;
        int capa = PQ_START_CAPA;
        GET_PQ(pq, self);
        switch (TYPE(options)) {
            case T_FIXNUM:
                capa = FIX2INT(options);
                break;
            case T_HASH:
                if (!NIL_P(param = rb_hash_aref(options,
                                                ID2SYM(id_capacity)))) {
                    capa = FIX2INT(param);
                }
                if (!NIL_P(param = rb_hash_aref(options,
                                                ID2SYM(id_less_than)))) {
                    pq->proc = param;
                }
                break;
            default:
                rb_raise(rb_eArgError,
                         "PriorityQueue#initialize only takes a Hash or "
                         "an integer");
                
                break;
        }
        if (capa < 0) {
            rb_raise(rb_eIndexError,
                     "PriorityQueue must have a capacity > 0. %d < 0",
                     capa);
        }
        pq->capa = capa;
        if (rb_block_given_p()) {
            pq->proc = rb_block_proc();
        }
        if (argc > 1) {
            rb_raise(rb_eArgError,
                     "PriorityQueue#initialize only takes one parameter");
        }
    }

    return self;
}

/*
 *  call-seq:
 *     pq.clone -> pq_clone
 *  
 *  Returns a shallow clone of the priority queue. That is only the priority
 *  queue is cloned, its contents are not cloned.
 */
static VALUE
frt_pq_clone(VALUE self)
{
    PriQ *pq, *new_pq = ALLOC(PriQ);
    GET_PQ(pq, self);
    memcpy(new_pq, pq, sizeof(PriQ));
    new_pq->heap = ALLOC_N(VALUE, new_pq->mem_capa);
    memcpy(new_pq->heap, pq->heap, sizeof(VALUE) * (new_pq->size + 1));

    return Data_Wrap_Struct(cPriorityQueue, &frt_pq_mark, &frt_pq_free, new_pq);
}

/*
 *  call-seq:
 *     pq.clear -> self
 *  
 *  Clears all elements from the priority queue. The size will be reset to 0.
 */
static VALUE
frt_pq_clear(VALUE self)
{
    PriQ *pq;
    GET_PQ(pq, self);
    pq->size = 0;
    return self;
}

/*
 *  call-seq:
 *     pq.insert(elem) -> self
 *     pq << elem -> self
 *  
 *  Insert an element into a queue. It will be inserted into the correct
 *  position in the queue according to its priority.
 */
static VALUE
frt_pq_insert(VALUE self, VALUE elem)
{
    PriQ *pq;
    GET_PQ(pq, self);
    if (pq->size < pq->capa) {
        pq_push(pq, elem);
    }
    else if (pq->size > 0 && frt_pq_lt(pq->proc, pq->heap[1], elem)) {
        pq->heap[1] = elem;
        pq_down(pq);
    }
    /* else ignore the element */
    return self;
}

/*
 *  call-seq:
 *     pq.adjust -> self
 *  
 *  Sometimes you modify the top element in the priority queue so that its
 *  priority changes. When you do this you need to reorder the queue and you
 *  do this by calling the adjust method.
 */
static VALUE
frt_pq_adjust(VALUE self)
{
    PriQ *pq;
    GET_PQ(pq, self);
    pq_down(pq);
    return self;
}

/*
 *  call-seq:
 *     pq.top -> elem
 *  
 *  Returns the top element in the queue but does not remove it from the
 *  queue.
 */
static VALUE
frt_pq_top(VALUE self)
{
    PriQ *pq;
    GET_PQ(pq, self);
    return (pq->size > 0) ? pq->heap[1] : Qnil;
}

/*
 *  call-seq:
 *     pq.pop -> elem
 *  
 *  Returns the top element in the queue removing it from the queue.
 */
static VALUE
frt_pq_pop(VALUE self)
{
    PriQ *pq;
    GET_PQ(pq, self);
    if (pq->size > 0) {
        VALUE result = pq->heap[1];       /* save first value */
        pq->heap[1] = pq->heap[pq->size]; /* move last to first */
        pq->heap[pq->size] = Qnil;
        pq->size--;
        pq_down(pq);                      /* adjust heap */
        return result;
    }
    else {
        return Qnil;
    }
}

/*
 *  call-seq:
 *     pq.size -> integer
 *  
 *  Returns the size of the queue, ie. the number of elements currently stored
 *  in the queue. The _size_ of a PriorityQueue can never be greater than
 *  its _capacity_
 */
static VALUE
frt_pq_size(VALUE self)
{
    PriQ *pq;
    GET_PQ(pq, self);
    return INT2FIX(pq->size);
}

/*
 *  call-seq:
 *     pq.capacity -> integer
 *  
 *  Returns the capacity of the queue, ie. the number of elements that can be
 *  stored in a Priority queue before they start to drop off the end.  The
 *  _size_ of a PriorityQueue can never be greater than its
 *  _capacity_
 */
static VALUE
frt_pq_capa(VALUE self)
{
    PriQ *pq;
    GET_PQ(pq, self);
    return INT2FIX(pq->capa);
}

/*  
 *  Document-class: Ferret::Utils::PriorityQueue
 *
 *  == Summary
 *
 *  A PriorityQueue is a very useful data structure and one that needs a fast
 *  implementation. Hence this priority queue is implemented in C. It is
 *  pretty easy to use; basically you just insert elements into the queue and
 *  pop them off.
 *
 *  The elements are sorted with the lowest valued elements on the top of
 *  the heap, ie the first to be popped off. Elements are ordered using the
 *  less_than '<' method. To change the order of the queue you can either
 *  reimplement the '<' method pass a block when you initialize the queue.
 *
 *  You can also set the capacity of the PriorityQueue. Once you hit the
 *  capacity, the lowest values elements are automatically popped of the top
 *  of the queue as more elements are added.
 *
 *  == Example
 *  
 *  Here is a toy example that sorts strings by their length and has a capacity
 *  of 5;
 *    
 *    q = PriorityQueue.new(5) {|a, b| a.size < b.size}
 *    q << "x"
 *    q << "xxxxx"
 *    q << "xxx"
 *    q << "xxxx"
 *    q << "xxxxxx"
 *    q << "xx" # hit capacity so "x" will be popped off the top
 *
 *    puts q.size     #=> 5
 *    word = q.pop    #=> "xx"
 *    q.top << "yyyy" # "xxxyyyy" will still be at the top of the queue
 *    q.adjust        # move "xxxyyyy" to its correct location in queue
 *    word = q.pop    #=> "xxxx"
 *    word = q.pop    #=> "xxxxx"
 *    word = q.pop    #=> "xxxxxx"
 *    word = q.pop    #=> "xxxyyyy"
 *    word = q.pop    #=> nil
 */
static void
Init_PriorityQueue(void)
{
    /* PriorityQueue */
    cPriorityQueue = rb_define_class_under(mUtils, "PriorityQueue", rb_cObject);
    rb_define_alloc_func(cPriorityQueue, frt_pq_alloc);

    rb_define_method(cPriorityQueue, "initialize", frt_pq_init, -1);
    rb_define_method(cPriorityQueue, "clone", frt_pq_clone, 0);
    rb_define_method(cPriorityQueue, "clear", frt_pq_clear, 0);
    rb_define_method(cPriorityQueue, "insert", frt_pq_insert, 1);
    rb_define_method(cPriorityQueue, "<<", frt_pq_insert, 1);
    rb_define_method(cPriorityQueue, "top", frt_pq_top, 0);
    rb_define_method(cPriorityQueue, "pop", frt_pq_pop, 0);
    rb_define_method(cPriorityQueue, "size", frt_pq_size, 0);
    rb_define_method(cPriorityQueue, "capacity", frt_pq_capa, 0);
    rb_define_method(cPriorityQueue, "adjust", frt_pq_adjust, 0);
}

/* rdoc hack
extern VALUE mFerret = rb_define_module("Ferret");
*/

/*
 *  Document-module: Ferret::Utils
 *
 *  The Utils module contains a number of helper classes and modules that are
 *  useful when indexing with Ferret. They are;
 *
 *  * BitVector
 *  * PriorityQueue
 *  * => more to come
 *
 *  These helper classes could also be quite useful outside of Ferret and may
 *  one day find themselves in their own separate library.
 */
void
Init_Utils(void)
{
    mUtils = rb_define_module_under(mFerret, "Utils");

    Init_BitVector();
    Init_PriorityQueue();
}
