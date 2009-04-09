#include "hash.h"
#include "global.h"
#include <string.h>

/****************************************************************************
 *
 * HashTable
 *
 * This hash table is modeled after Python's dictobject and a description of
 * the algorithm can be found in the file dictobject.c in Python's src
 ****************************************************************************/

static char *dummy_key = "";

#define PERTURB_SHIFT 5
#define MAX_FREE_HASH_TABLES 80

static HashTable *free_hts[MAX_FREE_HASH_TABLES];
static int num_free_hts = 0;

unsigned long str_hash(const char *const str)
{
    register unsigned long h = 0;
    register unsigned char *p = (unsigned char *) str;

    for (; *p; p++) {
        h = 37 * h + *p;
    }

    return h;
}

unsigned long ptr_hash(const void *const ptr)
{
    return (unsigned long)ptr;
}

int ptr_eq(const void *q1, const void *q2)
{
    return q1 == q2;
}

static int int_eq(const void *q1, const void *q2)
{
    (void)q1;
    (void)q2;
    return true;
}

static unsigned long int_hash(const void *i)
{
    return *((unsigned long *)i);
}

typedef HashEntry *(*lookup_ft)(struct HashTable *ht, register const void *key);

/**
 * Fast lookup function for resizing as we know there are no equal elements or
 * deletes to worry about.
 *
 * @param ht the HashTable to do the fast lookup in
 * @param the hashkey we are looking for
 */
static INLINE HashEntry *h_resize_lookup(HashTable *ht,
                                           register const unsigned long hash)
{
    register unsigned long perturb;
    register int mask = ht->mask;
    register HashEntry *he0 = ht->table;
    register int i = hash & mask;
    register HashEntry *he = &he0[i];

    if (he->key == NULL) {
        he->hash = hash;
        return he;
    }

    for (perturb = hash;; perturb >>= PERTURB_SHIFT) {
        i = (i << 2) + i + perturb + 1;
        he = &he0[i & mask];
        if (he->key == NULL) {
            he->hash = hash;
            return he;
        }
    }
}

HashEntry *h_lookup_int(HashTable *ht, const void *key)
{
    register unsigned long hash = *((int *)key);
    register unsigned long perturb;
    register int mask = ht->mask;
    register HashEntry *he0 = ht->table;
    register int i = hash & mask;
    register HashEntry *he = &he0[i];
    register HashEntry *freeslot = NULL;

    if (he->key == NULL || he->hash == hash) {
        he->hash = hash;
        return he;
    }
    if (he->key == dummy_key) {
        freeslot = he;
    }

    for (perturb = hash;; perturb >>= PERTURB_SHIFT) {
        i = (i << 2) + i + perturb + 1;
        he = &he0[i & mask];
        if (he->key == NULL) {
            if (freeslot != NULL) {
                he = freeslot;
            }
            he->hash = hash;
            return he;
        }
        if (he->hash == hash) {
            return he;
        }
        if (he->key == dummy_key && freeslot == NULL) {
            freeslot = he;
        }
    }
}

HashEntry *h_lookup_str(HashTable *ht, register const char *key)
{
    register unsigned long hash = str_hash(key);
    register unsigned long perturb;
    register int mask = ht->mask;
    register HashEntry *he0 = ht->table;
    register int i = hash & mask;
    register HashEntry *he = &he0[i];
    register HashEntry *freeslot;

    if (he->key == NULL || he->key == key) {
        he->hash = hash;
        return he;
    }
    if (he->key == dummy_key) {
        freeslot = he;
    }
    else {
        if ((he->hash == hash) && (strcmp(he->key, key) == 0)) {
            return he;
        }
        freeslot = NULL;
    }

    for (perturb = hash;; perturb >>= PERTURB_SHIFT) {
        i = (i << 2) + i + perturb + 1;
        he = &he0[i & mask];
        if (he->key == NULL) {
            if (freeslot != NULL) {
                he = freeslot;
            }
            he->hash = hash;
            return he;
        }
        if (he->key == key
            || (he->hash == hash
                && he->key != dummy_key && strcmp(he->key, key) == 0)) {
            return he;
        }
        if (he->key == dummy_key && freeslot == NULL) {
            freeslot = he;
        }
    }
}

HashEntry *h_lookup(HashTable *ht, register const void *key)
{
    register unsigned int hash = ht->hash_i(key);
    register unsigned int perturb;
    register int mask = ht->mask;
    register HashEntry *he0 = ht->table;
    register int i = hash & mask;
    register HashEntry *he = &he0[i];
    register HashEntry *freeslot;
    eq_ft eq = ht->eq_i;

    if (he->key == NULL || he->key == key) {
        he->hash = hash;
        return he;
    }
    if (he->key == dummy_key) {
        freeslot = he;
    }
    else {
        if ((he->hash == hash) && eq(he->key, key)) {
            return he;
        }
        freeslot = NULL;
    }

    for (perturb = hash;; perturb >>= PERTURB_SHIFT) {
        i = (i << 2) + i + perturb + 1;
        he = &he0[i & mask];
        if (he->key == NULL) {
            if (freeslot != NULL) {
                he = freeslot;
            }
            he->hash = hash;
            return he;
        }
        if (he->key == key
            || (he->hash == hash
                && he->key != dummy_key && eq(he->key, key))) {
            return he;
        }
        if (he->key == dummy_key && freeslot == NULL) {
            freeslot = he;
        }
    }
}

HashTable *h_new_str(free_ft free_key, free_ft free_value)
{
    HashTable *ht;
    if (num_free_hts > 0) {
        ht = free_hts[--num_free_hts];
    }
    else {
        ht = ALLOC(HashTable);
    }
    ht->fill = 0;
    ht->size = 0;
    ht->mask = HASH_MINSIZE - 1;
    ht->table = ht->smalltable;
    memset(ht->smalltable, 0, sizeof(ht->smalltable));
    ht->lookup_i = (lookup_ft)&h_lookup_str;

    ht->free_key_i = free_key != NULL ? free_key : &dummy_free;
    ht->free_value_i = free_value != NULL ? free_value : &dummy_free;
    ht->ref_cnt = 1;
    return ht;
}

HashTable *h_new_int(free_ft free_value)
{
    HashTable *ht = h_new_str(NULL, free_value);
    ht->lookup_i = &h_lookup_int;
    ht->eq_i = int_eq;
    ht->hash_i = int_hash;
    return ht;
}

HashTable *h_new(hash_ft hash, eq_ft eq, free_ft free_key, free_ft free_value)
{
    HashTable *ht = h_new_str(free_key, free_value);

    ht->lookup_i = &h_lookup;
    ht->eq_i = eq;
    ht->hash_i = hash;
    return ht;
}

void h_clear(HashTable *ht)
{
    int i;
    HashEntry *he;
    free_ft free_key = ht->free_key_i;
    free_ft free_value = ht->free_value_i;

    /* Clear all the hash values and keys as necessary */
    if (free_key != dummy_free || free_value != dummy_free) {
        for (i = 0; i <= ht->mask; i++) {
            he = &ht->table[i];
            if (he->key != NULL && he->key != dummy_key) {
                free_value(he->value);
                free_key(he->key);
            }
            he->key = NULL;
        }
    }
    ZEROSET_N(ht->table, HashEntry, ht->mask + 1);
    ht->size = 0;
    ht->fill = 0;
}

void h_destroy(HashTable *ht)
{
    if (--(ht->ref_cnt) <= 0) {
        h_clear(ht);

        /* if a new table was created, be sure to free it */
        if (ht->table != ht->smalltable) {
            free(ht->table);
        }

#ifdef DEBUG
        free(ht);
#else
        if (num_free_hts < MAX_FREE_HASH_TABLES) {
            free_hts[num_free_hts++] = ht;
        }
        else {
            free(ht);
        }
#endif
    }
}

void *h_get(HashTable *ht, const void *key)
{
    /* Note: lookup_i will never return NULL. */
    return ht->lookup_i(ht, key)->value;
}

int h_del(HashTable *ht, const void *key)
{
    HashEntry *he = ht->lookup_i(ht, key);

    if (he->key != NULL && he->key != dummy_key) {
        ht->free_key_i(he->key);
        ht->free_value_i(he->value);
        he->key = dummy_key;
        he->value = NULL;
        ht->size--;
        return true;
    }
    else {
        return false;
    }
}

void *h_rem(HashTable *ht, const void *key, bool destroy_key)
{
    void *val;
    HashEntry *he = ht->lookup_i(ht, key);

    if (he->key != NULL && he->key != dummy_key) {
        if (destroy_key) {
            ht->free_key_i(he->key);
        }

        he->key = dummy_key;
        val = he->value;
        he->value = NULL;
        ht->size--;
        return val;
    }
    else {
        return NULL;
    }
}

static int h_resize(HashTable *ht, int min_newsize)
{
    HashEntry smallcopy[HASH_MINSIZE];
    HashEntry *oldtable;
    HashEntry *he_old, *he_new;
    int newsize, num_active;

    /* newsize will be a power of two */
    for (newsize = HASH_MINSIZE; newsize < min_newsize; newsize <<= 1) {
    }

    oldtable = ht->table;
    if (newsize == HASH_MINSIZE) {
        if (ht->table == ht->smalltable) {
            /* need to copy the d*(int *)ata out so we can rebuild the table into
             * the same space */
            memcpy(smallcopy, ht->smalltable, sizeof(smallcopy));
            oldtable = smallcopy;
        }
        else {
            ht->table = ht->smalltable;
        }
    }
    else {
        ht->table = ALLOC_N(HashEntry, newsize);
    }
    memset(ht->table, 0, sizeof(HashEntry) * newsize);
    ht->fill = ht->size;
    ht->mask = newsize - 1;

    for (num_active = ht->size, he_old = oldtable; num_active > 0; he_old++) {
        if (he_old->key && he_old->key != dummy_key) {    /* active entry */
            /*he_new = ht->lookup_i(ht, he_old->key); */
            he_new = h_resize_lookup(ht, he_old->hash);
            he_new->key = he_old->key;
            he_new->value = he_old->value;
            num_active--;
        }                       /* else empty entry so nothing to do */
    }
    if (oldtable != smallcopy && oldtable != ht->smalltable) {
        free(oldtable);
    }
    return 0;
}

int h_set(HashTable *ht, const void *key, void *value)
{
    int ret_val = HASH_KEY_DOES_NOT_EXIST;
    HashEntry *he = ht->lookup_i(ht, key);
    if (he->key == NULL) {
        if (ht->fill * 3 > ht->mask * 2) {
            h_resize(ht, ht->size * ((ht->size > SLOW_DOWN) ? 4 : 2));
            he = ht->lookup_i(ht, key);
        }
        ht->fill++;
        ht->size++;
    }
    else if (he->key == dummy_key) {
        ht->size++;
    }
    else if (he->key != key) {
        ht->free_key_i(he->key);
        if (he->value != value) {
            ht->free_value_i(he->value);
        }
        ret_val = HASH_KEY_EQUAL;
    }
    else {
        /* safety check. Only free old value if it isn't the new value */
        if (he->value != value) {
            ht->free_value_i(he->value);
        }
        ret_val = HASH_KEY_SAME;
    }
    he->key = (void *)key;
    he->value = value;

    /*
    if ((ht->fill > fill) && (ht->fill * 3 > ht->mask * 2)) {
        h_resize(ht, ht->size * ((ht->size > SLOW_DOWN) ? 4 : 2));
    }
    */
    return ret_val;
}

HashEntry *h_set_ext(HashTable *ht, const void *key)
{
    HashEntry *he = ht->lookup_i(ht, key);
    if (he->key == NULL) {
        if (ht->fill * 3 > ht->mask * 2) {
            h_resize(ht, ht->size * ((ht->size > SLOW_DOWN) ? 4 : 2));
            he = ht->lookup_i(ht, key);
        }
        ht->fill++;
        ht->size++;
    }
    else if (he->key == dummy_key) {
        ht->size++;
    }

    return he;
}

int h_set_safe(HashTable *ht, const void *key, void *value)
{
    HashEntry *he = ht->lookup_i(ht, key);
    int fill = ht->fill;
    if (he->key == NULL) {
        ht->fill++;
        ht->size++;
    }
    else if (he->key == dummy_key) {
        ht->size++;
    }
    else {
        return false;
    }
    he->key = (void *)key;
    he->value = value;

    if ((ht->fill > fill) && (ht->fill * 3 > ht->mask * 2)) {
        h_resize(ht, ht->size * ((ht->size > SLOW_DOWN) ? 4 : 2));
    }
    return true;
}

int h_has_key(HashTable *ht, const void *key)
{
    HashEntry *he = ht->lookup_i(ht, key);
    if (he->key == NULL || he->key == dummy_key) {
        return HASH_KEY_DOES_NOT_EXIST;
    }
    else if (he->key == key) {
        return HASH_KEY_SAME;
    }
    else {
        return HASH_KEY_EQUAL;
    }
}

void *h_get_int(HashTable *self, const unsigned long key)
{
  return h_get(self, &key);
}

int h_del_int(HashTable *self, const unsigned long key)
{
  return h_del(self, &key);
}

void *h_rem_int(HashTable *self, const unsigned long key)
{
  return h_rem(self, &key, false);
}

int h_set_int(HashTable *self, const unsigned long key, void *value)
{
  return h_set(self, &key, value);
}

int h_set_safe_int(HashTable *self, const unsigned long key, void *value)
{
  return h_set_safe(self, &key, value);
}

int h_has_key_int(HashTable *self, const unsigned long key)
{
  return h_has_key(self, &key);
}

void h_each(HashTable *ht,
            void (*each_kv) (void *key, void *value, void *arg), void *arg)
{
    HashEntry *he;
    int i = ht->size;
    for (he = ht->table; i > 0; he++) {
        if (he->key && he->key != dummy_key) {        /* active entry */
            each_kv(he->key, he->value, arg);
            i--;
        }
    }
}

HashTable *h_clone(HashTable *ht,
                   h_clone_func_t clone_key, h_clone_func_t clone_value)
{
    void *key, *value;
    HashEntry *he;
    int i = ht->size;
    HashTable *ht_clone;

    if (ht->lookup_i == (lookup_ft)&h_lookup_str) {
        ht_clone = h_new_str(ht->free_key_i, ht->free_value_i);
    }
    else {
        ht_clone = h_new(ht->hash_i, ht->eq_i, ht->free_key_i, ht->free_value_i);
    }

    for (he = ht->table; i > 0; he++) {
        if (he->key && he->key != dummy_key) {        /* active entry */
            key = clone_key ? clone_key(he->key) : he->key;
            value = clone_value ? clone_value(he->value) : he->value;
            h_set(ht_clone, key, value);
            i--;
        }
    }
    return ht_clone;
}

void h_str_print_keys(HashTable *ht)
{
    HashEntry *he;
    int i = ht->size;
    printf("keys:\n");
    for (he = ht->table; i > 0; he++) {
        if (he->key && he->key != dummy_key) {        /* active entry */
            printf("\t%s\n", (char *)he->key);
            i--;
        }
    }
}
