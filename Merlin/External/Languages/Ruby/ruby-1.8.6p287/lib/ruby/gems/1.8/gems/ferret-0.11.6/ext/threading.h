#ifndef FRT_THREADING_H
#define FRT_THREADING_H

#include "hash.h"
#define UNTHREADED 1

typedef void * mutex_t;
typedef struct HashTable *thread_key_t;
typedef int thread_once_t;
#define MUTEX_INITIALIZER NULL
#define MUTEX_RECURSIVE_INITIALIZER NULL
#define THREAD_ONCE_INIT 1;
#define mutex_init(a, b)
#define mutex_lock(a)
#define mutex_trylock(a)
#define mutex_unlock(a)
#define mutex_destroy(a)
#define thread_key_create(a, b) frt_thread_key_create(a, b)
#define thread_key_delete(a) frt_thread_key_delete(a)
#define thread_setspecific(a, b) frt_thread_setspecific(a, b)
#define thread_getspecific(a) frt_thread_getspecific(a)
#define thread_exit(a)
#define thread_once(a, b) frt_thread_once(a, b)

void frt_thread_once(int *once_control, void (*init_routine)(void));
void frt_thread_key_create(thread_key_t *key, void (*destr_function)(void *));
void frt_thread_key_delete(thread_key_t key);
void frt_thread_setspecific(thread_key_t key, const void *pointer);
void *frt_thread_getspecific(thread_key_t key);

#endif
