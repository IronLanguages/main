#include "multimapper.h"
#include "array.h"
#include "bitvector.h"
#include <string.h>

#define St(state) ((State *)(state))
#define UCtoI(val) ((int)(unsigned char)(val))

static void state_destroy(State *state)
{
    state->destroy_i(state);
}

typedef struct LetterState
{
    State super;
    int c;
    int val;
    char *mapping;
} LetterState;
#define LSt(state) ((LetterState *)(state))


static int lstate_next(LetterState *self, int c, int *states)
{
    if (c == self->c) {
        states[0] = self->val;
        return 1;
    }
    else {
        return 0;
    }
}

static int lstate_is_match(LetterState *self, char **mapping)
{
    if (self->val < 0) {
        *mapping = self->mapping;
        return self->val;
    }
    else {
        return 0;
    }
}

static LetterState *lstate_new(int c, int val)
{
    LetterState *self   = ALLOC(LetterState);
    self->c             = c;
    self->val           = val;
    self->mapping       = NULL;
    St(self)->next      = (int (*)(State *, int, int *))&lstate_next;
    St(self)->destroy_i = (void (*)(State *))&free;
    St(self)->is_match  = (int (*)(State *, char **))&lstate_is_match;
    return self;
}

typedef struct NonDeterministicState
{
    State super;
    int *states[256];
    int size[256];
    int capa[256];
} NonDeterministicState;

static int ndstate_next(NonDeterministicState *self, int c, int *states)
{
    int size = self->size[c];
    memcpy(states, self->states[c], size * sizeof(int));
    return size;
}

static void ndstate_add(NonDeterministicState *self, int c, int state)
{
    if (self->capa[c] <= self->size[c]) {
        if (self->capa[c] == 0) {
            self->capa[c] = 4;
        }
        else {
            self->capa[c] <<= 1;
        }
        REALLOC_N(self->states[c], int, self->capa[c]);
    }
    self->states[c][self->size[c]++] = state;
}

static void ndstate_destroy_i(NonDeterministicState *self)
{
    int i;
    for (i = 0; i < 256; i++) {
        free(self->states[i]);
    }
    free(self);
}

static int ndstate_is_match(State *self, char **mapping)
{
    (void)self; (void)mapping;
    return 0;
}

static NonDeterministicState *ndstate_new()
{
    NonDeterministicState *self = ALLOC_AND_ZERO(NonDeterministicState);
    St(self)->next              = (int (*)(State *, int, int *))&ndstate_next;
    St(self)->destroy_i         = (void (*)(State *))&ndstate_destroy_i;
    St(self)->is_match          = &ndstate_is_match;
    return self;
}

MultiMapper *mulmap_new()
{
    MultiMapper *self = ALLOC_AND_ZERO(MultiMapper);
    self->capa = 128;
    self->mappings = ALLOC_N(Mapping *, 128);
    self->d_capa = 128;
    self->dstates = ALLOC_N(DeterministicState *, 128);
    self->dstates_map = NULL;
    self->nstates = NULL;
    self->ref_cnt = 1;
    return self;
}

static INLINE void mulmap_free_dstates(MultiMapper *self)
{
    if (self->d_size > 0) {
        int i;
        for (i = self->d_size - 1; i >= 0; i--) {
            free(self->dstates[i]);
        }
        self->d_size = 0;
    }
}

void mulmap_add_mapping(MultiMapper *self, const char *pattern, const char *rep)
{
    if (pattern == NULL || pattern[0] == '\0') {
        RAISE(ARG_ERROR, "Tried to add empty pattern to multi_mapper");
    }
    else {
        Mapping *mapping = ALLOC(Mapping);
        if (self->size >= self->capa) {
            self->capa <<= 1;
            REALLOC_N(self->mappings, Mapping *, self->capa);
        }
        mapping->pattern = estrdup(pattern);
        mapping->replacement = estrdup(rep);
        self->mappings[self->size++] = mapping;
        mulmap_free_dstates(self);
    }
}


static INLINE void mulmap_bv_set_states(BitVector *bv, int *states, int cnt)
{
    int i;
    for (i = cnt - 1; i >= 0; i--) {
        bv_set(bv, states[i]);
    }
}

static DeterministicState *mulmap_process_state(MultiMapper *self, BitVector *bv)
{
    DeterministicState *current_state = h_get(self->dstates_map, bv);
    if (current_state == NULL) {
        int bit, i;
        int match_len = 0, max_match_len = 0;
        State *start = self->nstates[0];
        DeterministicState *start_ds;
        current_state = ALLOC_AND_ZERO(DeterministicState);
        h_set(self->dstates_map, bv, current_state);
        if (self->d_size >= self->d_capa) {
            self->d_capa <<= 1;
            REALLOC_N(self->dstates, DeterministicState *, self->d_capa);
        }
        self->dstates[self->d_size++] = current_state;
        start_ds = self->dstates[0];
        for (i = 0; i <= 256; i++) {
            current_state->next[i] = start_ds;
        }
        while ((bit = bv_scan_next(bv)) >= 0) {
            char *mapping;
            State *st = self->nstates[bit];
            if ((match_len = -st->is_match(st, &mapping)) > max_match_len) {
                current_state->longest_match = max_match_len = match_len;
                current_state->mapping = mapping;
                current_state->mapping_len = strlen(mapping);
            }
        }
        for (i = self->a_size - 1; i >= 0; i--) {
            unsigned char c = self->alphabet[i];
            BitVector *nxt_bv = bv_new_capa(self->nsize);
            mulmap_bv_set_states(nxt_bv, self->next_states,
                                 start->next(start, (int)c, self->next_states));
            bv_scan_reset(bv);
            while ((bit = bv_scan_next(bv)) >= 0) {
                State *state = self->nstates[bit];
                mulmap_bv_set_states(nxt_bv, self->next_states,
                                     state->next(state, (int)c, self->next_states));
            }
            current_state->next[(int)c] = mulmap_process_state(self, nxt_bv);
        }
    }
    else {
        bv_destroy(bv);
    }
    return current_state;
}

void mulmap_compile(MultiMapper *self)
{
    NonDeterministicState *start = ndstate_new();
    int i, j;
    int size = 1;
    int capa = 128;
    LetterState *ls;
    State **nstates = ALLOC_N(State *, capa);
    Mapping **mappings = self->mappings;
    unsigned char alphabet[256];
    nstates[0] = (State *)start;
    memset(alphabet, 0, 256);

    for (i = self->size - 1; i >= 0; i--) {
        const char *pattern = mappings[i]->pattern;
        const int plen = (int)strlen(pattern);
        ndstate_add(start, UCtoI(pattern[0]), size);
        if (size + plen + 1 >= capa) {
            capa <<= 2;
            REALLOC_N(nstates, State *, capa);
        }
        for (j = 0; j < plen; j++) {
            alphabet[UCtoI(pattern[j])] = 1;
            size += 1;
            nstates[size-1] = (State *)lstate_new(UCtoI(pattern[j+1]), size);
        }
        ls = LSt(nstates[size-1]);
        ls->mapping = mappings[i]->replacement;
        ls->val = -plen;
        ls->c = -1;
    }
    for (i = j = 0; i < 256; i++) {
        if (alphabet[i]) self->alphabet[j++] = i;
    }
    self->a_size = j;
    mulmap_free_dstates(self);
    self->nstates = nstates;
    self->nsize = size;
    self->next_states = ALLOC_N(int, size);
    self->dstates_map = h_new((hash_ft)&bv_hash, (eq_ft)&bv_eq,
                              (free_ft)&bv_destroy, (free_ft)NULL);
    mulmap_process_state(self, bv_new_capa(0));
    h_destroy(self->dstates_map);
    for (i = size - 1; i >= 0; i--) {
        state_destroy(nstates[i]);
    }
    free(self->next_states);
    free(nstates);
}

int mulmap_map_len(MultiMapper *self, char *to, char *from, int capa)
{
    DeterministicState *start = self->dstates[0];
    DeterministicState *state = start;
    char *s = from, *d = to, *end = to + capa - 1;
    if (self->d_size == 0) {
        RAISE(STATE_ERROR, "You forgot to compile your MultiMapper");
    }
    while (*s && d < end) {
        state = state->next[UCtoI(*s)];
        if (state->mapping) {
            int len = state->mapping_len;
            d -= (state->longest_match - 1);
            if ((d + len) > end) {
                len = end - d;
            }
            memcpy(d, state->mapping, len);
            d += len;
            state = start;
        }
        else {
            *(d++) = *s;
        }
        s++;
    }
    *d = '\0';
    return d - to;
}

char *mulmap_map(MultiMapper *self, char *to, char *from, int capa)
{
    mulmap_map_len(self, to, from, capa);
    return to;
}

void mulmap_destroy(MultiMapper *self)
{
    if (--(self->ref_cnt) <= 0) {
        int i;
        mulmap_free_dstates(self);
        for (i = self->size - 1; i >= 0; i--) {
            Mapping *mapping = self->mappings[i];
            free(mapping->pattern);
            free(mapping->replacement);
            free(mapping);
        }
        free(self->mappings);
        free(self->dstates);
        free(self);
    }
}
