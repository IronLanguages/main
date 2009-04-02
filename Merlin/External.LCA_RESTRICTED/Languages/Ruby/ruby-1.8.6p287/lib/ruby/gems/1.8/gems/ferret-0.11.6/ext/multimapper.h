#ifndef FRT_MAPPER_H
#define FRT_MAPPER_H

#include "hash.h"

typedef struct State
{
    int  (*next)(struct State *self, int c, int *states);
    void (*destroy_i)(struct State *self);
    int  (*is_match)(struct State *self, char **mapping);
} State;

typedef struct DeterministicState
{
    struct DeterministicState *next[256];
    int longest_match;
    char *mapping;
    int mapping_len;
} DeterministicState;

typedef struct Mapping
{
    char *pattern;
    char *replacement;
} Mapping;

typedef struct MultiMapper
{
    Mapping **mappings;
    int size;
    int capa;
    DeterministicState **dstates;
    int d_size;
    int d_capa;
    unsigned char alphabet[256];
    int a_size;
    HashTable *dstates_map;
    State **nstates;
    int nsize;
    int *next_states;
    int ref_cnt;
} MultiMapper;

extern MultiMapper *mulmap_new();
extern void mulmap_add_mapping(MultiMapper *self, const char *p, const char *r);
extern void mulmap_compile(MultiMapper *self);
extern char *mulmap_map(MultiMapper *self, char *to, char *from, int capa);
extern int mulmap_map_len(MultiMapper *self, char *to, char *from, int capa);
extern void mulmap_destroy(MultiMapper *self);

#endif
