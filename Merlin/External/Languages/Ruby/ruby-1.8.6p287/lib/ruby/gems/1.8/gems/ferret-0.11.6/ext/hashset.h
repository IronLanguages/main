#ifndef FRT_HASHSET_H
#define FRT_HASHSET_H

#include "hash.h"
#include "global.h"

#define HS_MIN_SIZE 4

typedef struct HashSet
{
    /* used internally to allocate space to elems */
    int capa;

    /* the number of elements in the HashSet */
    int size;

    /* the elements in the HashSet. The elements will be found in the order
     * they were added and can be iterated over from 0 to .size */ 
    void **elems;

    /* HashTable used internally */
    HashTable *ht;

    /* Internal: Frees elements added to the HashSet. Should never be NULL */
    void (*free_elem_i)(void *p);
} HashSet;

/**
 * Create a new HashSet. The function will allocate a HashSet Struct setting
 * the functions used to hash the objects it will contain and the eq function.
 * This should be used for non-string types.
 *
 * @param hash function to hash objects added to the HashSet
 * @param eq function to determine whether two items are equal
 * @param free_elem function used to free elements as added to the HashSet
 *   when the HashSet if destroyed or duplicate elements are added to the Set
 * @return a newly allocated HashSet structure
 */
extern HashSet *hs_new(unsigned long (*hash)(const void *p),
                       int (*eq)(const void *p1, const void *p2),
                       void (*free_elem)(void *p));

/**
 * Create a new HashSet specifically for strings. This will create a HashSet
 * as if you used hs_new with the standard string hash and eq functions.
 *
 * @param free_elem function used to free elements as added to the HashSet
 *   when the HashSet if destroyed or duplicate elements are added to the Set
 * @return a newly allocated HashSet structure
 */
extern HashSet *hs_new_str(void (*free_elem) (void *p));

/**
 * Free the memory allocated by the HashSet, but don't free the elements added
 * to the HashSet. If you'd like to free everything in the HashSet you should
 * use hs_destroy
 *
 * @param hs the HashSet to free
 */
extern void hs_free(HashSet *self);

/**
 * Destroy the HashSet including all elements added to the HashSet. If you'd
 * like to free the memory allocated to the HashSet without touching the
 * elements in the HashSet then use hs_free
 *
 * @param hs the HashSet to destroy
 */
extern void hs_destroy(HashSet *self);

/**
 * WARNING: this function may destroy some elements if you add them to a
 * HashSet were equivalent elements already exist, depending on how free_elem
 * was set.
 *
 * Add the element to the HashSet whether or not it was already in the
 * HashSet.
 *
 * When a element is added to the HashTable where it already exists, free_elem
 * is called on it, ie the element you tried to add might get destroyed.
 *
 * @param hs the HashSet to add the element to
 * @param elem the element to add to the HashSet
 * @return one of three values;
 *   <pre>
 *     HASH_KEY_DOES_NOT_EXIST  the element was not already in the HashSet.
 *                              This value is equal to 0 or false
 *     HASH_KEY_SAME            the element was identical (same memory
 *                              pointer) to an existing element so no freeing
 *                              was done
 *     HASH_KEY_EQUAL           the element was equal to an element already in
 *                              the HashSet so the new_elem was freed if
 *                              free_elem was set
 *   </pre>
 */
extern int hs_add(HashSet *self, void *elem);

/**
 * Add element to the HashSet. If the element already existed in the HashSet
 * and the new element was equal but not the same (same pointer/memory) then
 * don't add the element and return false, otherwise return true.
 *
 * @param hs the HashSet to add the element to
 * @param elem the element to add to the HashSet
 * @return true if the element was successfully added or false otherwise
 */
extern int hs_add_safe(HashSet *self, void *elem);

/**
 * Delete the element from the HashSet. Returns true if the item was
 * successfully deleted or false if the element never existed.
 *
 * @param hs the HashSet to delete from
 * @param elem the element to delete
 * @return true if the element was deleted or false if the element never
 *   existed
 */
extern int hs_del(HashSet *self, void *elem);

/**
 * Remove an item from the HashSet without actually freeing the item. This
 * function should return the item itself so that it can be freed later if
 * necessary.
 *
 * @param hs the HashSet to remove the element from.
 * @param elem the element to remove
 * @param the element that was removed or NULL otherwise
 */
extern void *hs_rem(HashSet *self, void *elem);

/**
 * Check if the element exists and return the appropriate value described
 * bellow.
 *
 * @param hs the HashSet to check in
 * @param elem the element to check for
 * @return one of the following values
 * <pre>
 *     HASH_KEY_DOES_NOT_EXIST  the element was not already in the HashSet.
 *                              This value is equal to 0 or false
 *     HASH_KEY_SAME            the element was identical (same memory
 *                              pointer) to an existing element so no freeing
 *                              was done
 *     HASH_KEY_EQUAL           the element was equal to an element already in
 *                              the HashSet so the new_elem was freed if
 *                              free_elem was set
 *   </pre>
 */
extern int hs_exists(HashSet *self, void *elem);

/**
 * Merge two HashSets. When a merge is done the merger (self) HashTable is
 * returned and the mergee is destroyed. All elements from mergee that were
 * not found in merger (self) will be added to self, otherwise they will be
 * destroyed.
 * 
 * @param self the HashSet to merge into
 * @param other HastSet to be merged into self
 * @return the merged HashSet
 */
extern HashSet *hs_merge(HashSet *self, HashSet *other);

/** 
 * Return the original version of +elem+. So if you allocate two elements
 * which are equal and add the first to the HashSet, calling this function
 * with the second element will return the first element from the HashSet.
 */
extern void *hs_orig(HashSet *self, void *elem);

/**
 * Clear all elements from the HashSet. If free_elem was set then use it to
 * free all elements as they are cleared. After the method is called, the
 * HashSets size will be 0.
 *
 * @param self the HashSet to clear
 */
extern void hs_clear(HashSet *self);

/* TODO: finish these functions.
int hs_osf(HashSet *hs, void *elem);
HashSet hs_or(HashSet *hs1, HashSet *h2);
HashSet hs_excl_or(HashSet *hs1, HashSet *h2);
HashSet hs_and(HashSet *hs1, HashSet *h2);
HashSet hs_mask(HashSet *hs1, HashSet *h2);
*/

#endif
