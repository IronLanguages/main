#include <string.h>
#include "priorityqueue.h"

#define START_CAPA 127

PriorityQueue *pq_new(int capa,
                      bool (*less_than)(const void *p1, const void *p2),
                      void (*free_elem)(void *elem))
{
    PriorityQueue *pq = ALLOC(PriorityQueue);
    pq->size = 0;
    pq->capa = capa;
    pq->mem_capa = (START_CAPA > capa ? capa : START_CAPA) + 1;
    pq->heap = ALLOC_N(void *, pq->mem_capa);
    pq->less_than_i = less_than;

    /* need to set this yourself if you want to change it */
    pq->free_elem_i = free_elem ? free_elem : &dummy_free;
    return pq;
}

PriorityQueue *pq_clone(PriorityQueue *pq)
{
    PriorityQueue *new_pq = ALLOC(PriorityQueue);
    memcpy(new_pq, pq, sizeof(PriorityQueue));
    new_pq->heap = ALLOC_N(void *, new_pq->mem_capa);
    memcpy(new_pq->heap, pq->heap, sizeof(void *) * (new_pq->size + 1));

    return new_pq;
}

void pq_clear(PriorityQueue *pq)
{
    int i;
    for (i = 1; i <= pq->size; i++) {
        pq->free_elem_i(pq->heap[i]);
        pq->heap[i] = NULL;
    }
    pq->size = 0;
}

void pq_free(PriorityQueue *pq)
{
    free(pq->heap);
    free(pq);
}

void pq_destroy(PriorityQueue *pq)
{
    pq_clear(pq);
    pq_free(pq);
}

/**
 * This method is used internally by pq_push. It is similar to pq_down except
 * that where pq_down reorders the elements from the top, pq_up reorders from
 * the bottom.
 *
 * @param pq the PriorityQueue to reorder
 */
static void pq_up(PriorityQueue *pq)
{
    void **heap = pq->heap;
    void *node;
    int i = pq->size;
    int j = i >> 1;

    node = heap[i];

    while ((j > 0) && pq->less_than_i(node, heap[j])) {
        heap[i] = heap[j];
        i = j;
        j = j >> 1;
    }
    heap[i] = node;
}

void pq_down(PriorityQueue *pq)
{
    register int i = 1;
    register int j = 2;         /* i << 1; */
    register int k = 3;         /* j + 1;  */
    register int size = pq->size;
    void **heap = pq->heap;
    void *node = heap[i];       /* save top node */

    if ((k <= size) && (pq->less_than_i(heap[k], heap[j]))) {
        j = k;
    }

    while ((j <= size) && pq->less_than_i(heap[j], node)) {
        heap[i] = heap[j];      /* shift up child */
        i = j;
        j = i << 1;
        k = j + 1;
        if ((k <= size) && pq->less_than_i(heap[k], heap[j])) {
            j = k;
        }
    }
    heap[i] = node;
}

void pq_push(PriorityQueue *pq, void *elem)
{
    pq->size++;
    if (pq->size >= pq->mem_capa) {
        pq->mem_capa <<= 1;
        REALLOC_N(pq->heap, void *, pq->mem_capa);
    }
    pq->heap[pq->size] = elem;
    pq_up(pq);
}

int pq_insert(PriorityQueue *pq, void *elem)
{
    if (pq->size < pq->capa) {
        pq_push(pq, elem);
        return PQ_ADDED;
    }
    else if (pq->size > 0 && pq->less_than_i(pq->heap[1], elem)) {
        pq->free_elem_i(pq->heap[1]);
        pq->heap[1] = elem;
        pq_down(pq);
        return PQ_INSERTED;
    }
    else {
        pq->free_elem_i(elem);
        return PQ_DROPPED;
    }
}

void *pq_top(PriorityQueue *pq)
{
    return pq->size ? pq->heap[1] : NULL;
}

void *pq_pop(PriorityQueue *pq)
{
    if (pq->size > 0) {
        void *result = pq->heap[1];       /* save first value */
        pq->heap[1] = pq->heap[pq->size]; /* move last to first */
        pq->heap[pq->size] = NULL;
        pq->size--;
        pq_down(pq);                      /* adjust heap */
        return result;
    }
    else {
        return NULL;
    }
}

