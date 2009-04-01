#include "bitvector.h"
#include <string.h>

BitVector *bv_new_capa(int capa)
{
    BitVector *bv = ALLOC(BitVector);

    /* The capacity passed by the user is number of bits allowed, however we
     * store capacity as the number of words (U32) allocated. */
    bv->capa = (capa >> 5) + 1;
    bv->bits = ALLOC_AND_ZERO_N(f_u32, bv->capa);

    bv->size = 0;
    bv->count = 0;
    bv->curr_bit = -1;
    bv->extends_as_ones = 0;
    bv->ref_cnt = 1;
    return bv;
}

BitVector *bv_new()
{
    return bv_new_capa(BV_INIT_CAPA);
}

void bv_destroy(BitVector * bv)
{
    if (--(bv->ref_cnt) == 0) {
        free(bv->bits);
        free(bv);
    }
}

void bv_set(BitVector * bv, int bit)
{
    f_u32 *word_p;
    int word = bit >> 5;
    f_u32 bitmask = 1 << (bit & 31);
    
    /* Check to see if we need to grow the BitVector */
    if (bit >= bv->size) {
        bv->size = bit + 1; /* size is max range of bits set */
        if (word >= bv->capa) {
            int capa = bv->capa << 1;
            while (capa <= word) {
                capa <<= 1;
            }
            REALLOC_N(bv->bits, f_u32, capa);
            memset(bv->bits + bv->capa, (bv->extends_as_ones ? 0xFF : 0),
                   sizeof(f_u32) * (capa - bv->capa));
            bv->capa = capa;
        }
    }
    
    /* Set the required bit */
    word_p = &(bv->bits[word]);
    if ((bitmask & *word_p) == 0) {
        bv->count++; /* update count */
        *word_p |= bitmask;
    }
}

/*
 * This method relies on the fact that enough space has been set for the bits
 * to be set. You need to create the BitVector using bv_new_capa(capa) with
 * a capacity larger than any bit being set.
 */
void bv_set_fast(BitVector * bv, int bit)
{
    bv->count++;
    bv->size = bit;
    bv->bits[bit >> 5] |= 1 << (bit & 31);
}

int bv_get(BitVector * bv, int bit)
{
    /* out of range so return 0 because it can't have been set */
    if (bit >= bv->size) {
        return bv->extends_as_ones;
    }
    return (bv->bits[bit >> 5] >> (bit & 31)) & 0x01;
}

void bv_clear(BitVector * bv)
{
    memset(bv->bits, 0, bv->capa * sizeof(f_u32));
    bv->extends_as_ones = 0;
    bv->count = 0;
    bv->size = 0;
}

/*
 * FIXME: if the top set bit is unset, size is not adjusted. This will not
 * cause any bugs in this code but could cause problems if users are relying
 * on the fact that size is accurate.
 */
void bv_unset(BitVector * bv, int bit)
{
    f_u32 *word_p;
    f_u32 bitmask;
    int word = bit >> 5;

    if (bit >= bv->size) {
        bv->size = bit + 1; /* size is max range of bits set */
        if (word >= bv->capa) {
            int capa = bv->capa << 1;

            while (capa <= word) {
                capa <<= 1;
            }
            REALLOC_N(bv->bits, f_u32, capa);
            memset(bv->bits + bv->capa, (bv->extends_as_ones ? 0xFF : 0),
                   sizeof(f_u32) * (capa - bv->capa));
            bv->capa = capa;
        }
    }
    
    word_p = &(bv->bits[word]);
    bitmask = 1 << (bit & 31);
    if ((bitmask & *word_p) > 0) {
        bv->count--; /* update count */
        *word_p &= ~bitmask;
    }
}

/* Table of bits per char. This table is used by the bv_recount method to
 * optimize the counting of bits */
static const uchar BYTE_COUNTS[] = {
    0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,
    1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
    1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
    2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
    1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
    2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
    2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
    3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
    1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
    2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
    2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
    3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
    2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
    3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
    3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
    4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8
};

int bv_recount(BitVector * bv)
{
    /* if the vector has been modified */
    int i, c = 0;
    uchar *bytes = (uchar *)bv->bits; /* count by character */
    const int num_bytes = (((bv->size >> 5) + 1) << 2);
    if (bv->extends_as_ones) {
        for (i = 0; i < num_bytes; i++) {
            c += BYTE_COUNTS[~(bytes[i]) & 0xFF];  /* sum bits per char */
        }
    }
    else {
        for (i = 0; i < num_bytes; i++) {
            c += BYTE_COUNTS[bytes[i]];     /* sum bits per char */
        }
    }
    bv->count = c;
    return c;
}

void bv_scan_reset(BitVector * bv)
{
    bv->curr_bit = -1;
}

/* Table showing the number of trailing 0s in a char. This is used to optimize
 * the bv_scan_next method.  */
const int NUM_TRAILING_ZEROS[] = {
    8, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0, 
    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    5, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    6, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    5, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    7, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    5, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    6, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    5, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0
};

/*
 * This method is highly optimized, hence the loop unrolling
 */
static INLINE int bv_get_1_offset(f_u32 word)
{
    if (word & 0xff) {
        return NUM_TRAILING_ZEROS[word & 0xff];
    }
    else {
        word >>= 8;
        if (word & 0xff) {
            return NUM_TRAILING_ZEROS[word & 0xff] + 8;
        }
        else {
            word >>= 8;
            if (word & 0xff) {
                return NUM_TRAILING_ZEROS[word & 0xff] + 16;
            }
            else {
                word >>= 8;
                return NUM_TRAILING_ZEROS[word & 0xff] + 24;
            }
        }
    }
}
    /*
     * second fastest;
     *
     *   while ((inc = NUM_TRAILING_ZEROS[word & 0xff]) == 8) {
     *       word >>= 8;
     *       bit_pos += 8;
     *   }
     *
     * third fastest;
     *
     *   bit_pos += inc;
     *   if ((word & 0xffff) == 0) {
     *     bit_pos += 16;
     *     word >>= 16;
     *   }
     *   if ((word & 0xff) == 0) {
     *     bit_pos += 8;
     *     word >>= 8;
     *   }
     *   bit_pos += NUM_TRAILING_ZEROS[word & 0xff];
     */

int bv_scan_next_from(BitVector * bv, register const int from)
{
    register const f_u32 *const bits = bv->bits;
    register const int word_size = (bv->size >> 5) + 1;
    register int word_pos = from >> 5;
    register int bit_pos = (from & 31);
    register f_u32 word = bits[word_pos] >> bit_pos;

    if (from >= bv->size) {
        return -1;
    }
    if (word == 0) {
        bit_pos = 0;
        do {
            word_pos++;
            if (word_pos >= word_size) {
                return -1;
            }
        } while (bits[word_pos] == 0);
        word = bits[word_pos];
    }

    /* check the word a byte at a time as the NUM_TRAILING_ZEROS table would
     * be too large for 32-bit integer or even a 16-bit integer */
    bit_pos += bv_get_1_offset(word);

    return bv->curr_bit = ((word_pos << 5) + bit_pos);
}

int bv_scan_next(BitVector * bv)
{
    return bv_scan_next_from(bv, bv->curr_bit + 1);
}

int bv_scan_next_unset_from(BitVector * bv, register const int from)
{
    register const f_u32 *const bits = bv->bits;
    register const int word_size = (bv->size >> 5) + 1;
    register int word_pos = from >> 5;
    register int bit_pos = (from & 31);
    register f_u32 word = ~(~(bits[word_pos]) >> bit_pos);

    if (from >= bv->size) {
        return -1;
    }
    if (word == 0xFFFFFFFF) {
        bit_pos = 0;
        do {
            word_pos++;
            if (word_pos >= word_size) {
                return -1;
            }
        } while (bits[word_pos] == 0xFFFFFFFF);
        word = bits[word_pos];
    }

    bit_pos += bv_get_1_offset(~word);

    return bv->curr_bit = ((word_pos << 5) + bit_pos);
}

int bv_scan_next_unset(BitVector * bv)
{
    return bv_scan_next_unset_from(bv, bv->curr_bit + 1);
}

int bv_eq(BitVector *bv1, BitVector *bv2)
{
    if (bv1 == bv2) {
        return true;
    }
    else if (bv1->extends_as_ones != bv2->extends_as_ones) {
        return false;
    }
    else {
        f_u32 *bits = bv1->bits;
        f_u32 *bits2 = bv2->bits;
        int min_size = min2(bv1->size, bv2->size);
        int word_size = (min_size >> 5) + 1;
        int ext_word_size = 0;

        int i;

        for (i = 0; i < word_size; i++) {
            if (bits[i] != bits2[i]) {
                return false;
            }
        }
        if (bv1->size > min_size) {
            bits = bv1->bits;
            ext_word_size = (bv1->size >> 5) + 1;
        }
        else if (bv2->size > min_size) {
            bits = bv2->bits;
            ext_word_size = (bv2->size >> 5) + 1;
        }
        if (ext_word_size) {
            const f_u32 expected = (bv1->extends_as_ones ? 0xFFFFFFFF : 0);
            for (i = word_size; i < ext_word_size; i++) {
                if (bits[i] != expected) {
                    return false;
                }
            }
        }
    }
    return true;
}

unsigned long bv_hash(BitVector *bv)
{
    unsigned long hash = 0;
    const f_u32 empty_word = bv->extends_as_ones ? 0xFFFFFFFF : 0;
    int i;
    for (i = (bv->size >> 5); i >= 0; i--) {
        const f_u32 word = bv->bits[i];
        if (word != empty_word) {
            hash = (hash << 1) ^ word;
        }
    }
    hash = (hash << 1) | bv->extends_as_ones;
    return hash;
}

static INLINE void bv_recapa(BitVector *bv, int new_capa)
{
    if (bv->capa < new_capa) {
        REALLOC_N(bv->bits, f_u32, new_capa);
        memset(bv->bits + bv->capa, (bv->extends_as_ones ? 0xFF : 0),
               sizeof(f_u32) * (new_capa - bv->capa)); 
        bv->capa = new_capa;
    }
}

static BitVector *bv_and_i(BitVector *bv, BitVector *bv1, BitVector *bv2)
{
    int i;
    int size;
    int word_size;
    int capa = 4;

    if (bv1->extends_as_ones && bv2->extends_as_ones) {
        size = max2(bv1->size, bv2->size);
        bv->extends_as_ones = true;
    }
    else if (bv1->extends_as_ones || bv2->extends_as_ones) {
        size = max2(bv1->size, bv2->size);
        bv->extends_as_ones = false;
    }
    else {
        size = min2(bv1->size, bv2->size);
        bv->extends_as_ones = false;
    }

    word_size = (size >> 5) + 1;
    while (capa < word_size) {
        capa <<= 1;
    }
    bv_recapa(bv1, capa);
    bv_recapa(bv2, capa);
    REALLOC_N(bv->bits, f_u32, capa);
    bv->capa = capa;
    bv->size = size;

    memset(bv->bits + word_size, (bv->extends_as_ones ? 0xFF : 0),
           sizeof(f_u32) * (capa - word_size)); 

    for (i = 0; i < word_size; i++) {
        bv->bits[i] = bv1->bits[i] & bv2->bits[i];
    }

    bv_recount(bv);
    return bv;
}

BitVector *bv_and(BitVector *bv1, BitVector *bv2)
{
    return bv_and_i(bv_new(), bv1, bv2);
}

BitVector *bv_and_x(BitVector *bv1, BitVector *bv2)
{
    return bv_and_i(bv1, bv1, bv2);
}

static BitVector *bv_or_i(BitVector *bv, BitVector *bv1, BitVector *bv2)
{
    int i;
    int max_size = max2(bv1->size, bv2->size);
    int word_size = (max_size >> 5) + 1;
    int capa = 4;
    while (capa < word_size) {
        capa <<= 1;
    }
    REALLOC_N(bv->bits, f_u32, capa);
    bv->capa = capa;
    bv->size = max_size;

    bv_recapa(bv1, capa);
    bv_recapa(bv2, capa);

    if (bv1->extends_as_ones || bv2->extends_as_ones) {
        bv->extends_as_ones = true;
    }
    else {
        bv->extends_as_ones = false;
    }

    memset(bv->bits + word_size, (bv->extends_as_ones ? 0xFF : 0),
           sizeof(f_u32) * (capa - word_size)); 

    for (i = 0; i < word_size; i++) {
        bv->bits[i] = bv1->bits[i] | bv2->bits[i];
    }
    bv_recount(bv);
    return bv;
}

BitVector *bv_or(BitVector *bv1, BitVector *bv2)
{
    return bv_or_i(bv_new(), bv1, bv2);
}

BitVector *bv_or_x(BitVector *bv1, BitVector *bv2)
{
    return bv_or_i(bv1, bv1, bv2);
}

static BitVector *bv_xor_i(BitVector *bv, BitVector *bv1, BitVector *bv2)
{
    int i;
    int max_size = max2(bv1->size, bv2->size);
    int word_size = (max_size >> 5) + 1;
    int capa = 4;
    while (capa < word_size) {
        capa <<= 1;
    }
    REALLOC_N(bv->bits, f_u32, capa);
    bv->capa = capa;
    bv->size = max_size;

    bv_recapa(bv1, capa);
    bv_recapa(bv2, capa);

    if (bv1->extends_as_ones != bv2->extends_as_ones) {
        bv->extends_as_ones = true;
    }
    else {
        bv->extends_as_ones = false;
    }

    memset(bv->bits + word_size, (bv->extends_as_ones ? 0xFF : 0),
           sizeof(f_u32) * (capa - word_size)); 

    for (i = 0; i < word_size; i++) {
        bv->bits[i] = bv1->bits[i] ^ bv2->bits[i];
    }
    bv_recount(bv);
    return bv;
}

BitVector *bv_xor(BitVector *bv1, BitVector *bv2)
{
    return bv_xor_i(bv_new(), bv1, bv2);
}

BitVector *bv_xor_x(BitVector *bv1, BitVector *bv2)
{
    return bv_xor_i(bv1, bv1, bv2);
}

static BitVector *bv_not_i(BitVector *bv, BitVector *bv1)
{
    int i;
    int word_size = (bv1->size >> 5) + 1;
    int capa = 4;
    while (capa < word_size) {
        capa <<= 1;
    }
    REALLOC_N(bv->bits, f_u32, capa);
    bv->capa = capa;
    bv->size = bv1->size;
    bv->extends_as_ones = 1 - bv1->extends_as_ones;
    memset(bv->bits + word_size, (bv->extends_as_ones ? 0xFF : 0),
           sizeof(f_u32) * (capa - word_size)); 

    for (i = 0; i < word_size; i++) {
        bv->bits[i] = ~(bv1->bits[i]);
    }
    bv_recount(bv);
    return bv;
}

BitVector *bv_not(BitVector *bv1)
{
    return bv_not_i(bv_new(), bv1);
}

BitVector *bv_not_x(BitVector *bv1)
{
    return bv_not_i(bv1, bv1);
}
