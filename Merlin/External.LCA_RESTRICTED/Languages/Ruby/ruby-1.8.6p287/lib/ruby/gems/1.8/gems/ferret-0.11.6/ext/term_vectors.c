#include <string.h>
#include "index.h"
#include "array.h"
#include "helper.h"

/****************************************************************************
 *
 * TermVector
 *
 ****************************************************************************/

void tv_destroy(TermVector *tv)
{
    int i = tv->term_cnt;
    while (i > 0) {
        i--;
        free(tv->terms[i].text);
        free(tv->terms[i].positions);
    }
    free(tv->offsets);
    free(tv->field);
    free(tv->terms);
    free(tv);
}

int tv_get_tv_term_index(TermVector *tv, const char *term)
{
    int lo = 0;                 /* search starts array */
    int hi = tv->term_cnt - 1;  /* for 1st element < n, return its index */
    int mid;
    int cmp;
    char *mid_term;

    while (hi >= lo) {
        mid = (lo + hi) >> 1;
        mid_term = tv->terms[mid].text;
        cmp = strcmp(term, mid_term);
        if (cmp < 0) {
            hi = mid - 1;
        }
        else if (cmp > 0) {
            lo = mid + 1;
        }
        else {                  /* found a match */
            return mid;
        }
    }
    if (hi >= 0 && strcmp(term, tv->terms[hi].text) == 0) {
        return hi;
    }
    else {
        return -1;
    }
    return hi;
}

extern TVTerm *tv_get_tv_term(TermVector *tv, const char *term)
{
    int index = tv_get_tv_term_index(tv, term);
    if (index >= 0) {
        return &(tv->terms[index]);
    }
    else {
        return NULL;
    }
}

/****************************************************************************
 *
 * TermVectorsReader
 *
 ****************************************************************************/

TermVectorsReader *tvr_open(Store *store,
                            const char *segment,
                            FieldInfos *fis)
{
    TermVectorsReader *tvr = ALLOC(TermVectorsReader);
    char file_name[SEGMENT_NAME_MAX_LENGTH];

    tvr->fis = fis;
    sprintf(file_name, "%s.tvx", segment);
    tvr->tvx_in = store->open_input(store, file_name);
    tvr->size = is_length(tvr->tvx_in) / 12;

    sprintf(file_name, "%s.tvd", segment);
    tvr->tvd_in = store->open_input(store, file_name);
    return tvr;
}

TermVectorsReader *tvr_clone(TermVectorsReader *orig)
{
    TermVectorsReader *tvr = ALLOC(TermVectorsReader);

    memcpy(tvr, orig, sizeof(TermVectorsReader));
    tvr->tvx_in = is_clone(orig->tvx_in);
    tvr->tvd_in = is_clone(orig->tvd_in);
    
    return tvr;
}

void tvr_close(TermVectorsReader *tvr)
{
    is_close(tvr->tvx_in);
    is_close(tvr->tvd_in);
    free(tvr);
}

TermVector *tvr_read_term_vector(TermVectorsReader *tvr, int field_num)
{
    TermVector *tv = ALLOC_AND_ZERO(TermVector);
    InStream *tvd_in = tvr->tvd_in;
    FieldInfo *fi = tvr->fis->fields[field_num];
    const int num_terms = is_read_vint(tvd_in);
    
    tv->field_num = field_num;
    tv->field = estrdup(fi->name);

    if (num_terms > 0) {
        int i, j, delta_start, delta_len, total_len, freq;
        int store_positions = fi_store_positions(fi);
        int store_offsets = fi_store_offsets(fi);
        uchar buffer[MAX_WORD_SIZE];
        TVTerm *term;

        tv->term_cnt = num_terms;
        tv->terms = ALLOC_AND_ZERO_N(TVTerm, num_terms);

        for (i = 0; i < num_terms; i++) {
            term = &(tv->terms[i]);
            /* read delta encoded term */
            delta_start = is_read_vint(tvd_in);
            delta_len = is_read_vint(tvd_in);
            total_len = delta_start + delta_len;
            is_read_bytes(tvd_in, buffer + delta_start, delta_len);
            buffer[total_len++] = '\0';
            term->text = memcpy(ALLOC_N(char, total_len), buffer, total_len);

            /* read freq */
            freq = term->freq = is_read_vint(tvd_in);

            /* read positions if necessary */
            if (store_positions) {
                int *positions = term->positions = ALLOC_N(int, freq);
                int pos = 0;
                for (j = 0; j < freq; j++) {
                    positions[j] = pos += is_read_vint(tvd_in);
                }
            }

            /* read offsets if necessary */
        }
        if (store_offsets) {
            int num_positions = tv->offset_cnt = is_read_vint(tvd_in);
            Offset *offsets = tv->offsets = ALLOC_N(Offset, num_positions);
            int offset = 0;
            for (i = 0; i < num_positions; i++) {
                offsets[i].start = offset += is_read_vint(tvd_in);
                offsets[i].end = offset += is_read_vint(tvd_in);
            }
        }
    }
    return tv;
}

HashTable *tvr_get_tv(TermVectorsReader *tvr, int doc_num)
{
    HashTable *term_vectors = h_new_str((free_ft)NULL, (free_ft)&tv_destroy);
    int i;
    InStream *tvx_in = tvr->tvx_in;
    InStream *tvd_in = tvr->tvd_in;
    off_t data_ptr, field_index_ptr;
    int field_cnt;
    int *field_nums;

    if (doc_num >= 0 && doc_num < tvr->size) {
        is_seek(tvx_in, 12 * doc_num);

        data_ptr = (off_t)is_read_u64(tvx_in);
        field_index_ptr = data_ptr + (off_t)is_read_u32(tvx_in);

        /* scan fields to get position of field_num's term vector */
        is_seek(tvd_in, field_index_ptr);

        field_cnt = is_read_vint(tvd_in);
        field_nums = ALLOC_N(int, field_cnt);

        for (i = 0; i < field_cnt; i++) {
            field_nums[i] = is_read_vint(tvd_in);
            is_read_vint(tvd_in); /* skip space, we don't need it */
        }
        is_seek(tvd_in, data_ptr);

        for (i = 0; i < field_cnt; i++) {
            TermVector *tv = tvr_read_term_vector(tvr, field_nums[i]);
            h_set(term_vectors, tv->field, tv);
        }
        free(field_nums);
    }
    return term_vectors;
}

TermVector *tvr_get_field_tv(TermVectorsReader *tvr,
                             int doc_num,
                             int field_num)
{
    int i;
    InStream *tvx_in = tvr->tvx_in;
    InStream *tvd_in = tvr->tvd_in;
    off_t data_ptr, field_index_ptr;
    int field_cnt;
    int offset = 0;
    TermVector *tv = NULL;

    if (doc_num >= 0 && doc_num < tvr->size) {
        is_seek(tvx_in, 12 * doc_num);

        data_ptr = (off_t)is_read_u64(tvx_in);
        field_index_ptr = data_ptr + (off_t)is_read_u32(tvx_in);

        /* scan fields to get position of field_num's term vector */
        is_seek(tvd_in, field_index_ptr);

        field_cnt = is_read_vint(tvd_in);
        for (i = 0; i < field_cnt; i++) {
            if ((int)is_read_vint(tvd_in) == field_num) {
                break;
            }
            offset += is_read_vint(tvd_in); /* space taken by field */
        }
        if (i < field_cnt) {
            /* field was found */
            is_seek(tvd_in, data_ptr + offset);
            tv = tvr_read_term_vector(tvr, field_num);
        }
    }
    return tv;
}

/****************************************************************************
 *
 * TermVectorsWriter
 *
 ****************************************************************************/

TermVectorsWriter *tvw_open(Store *store, const char *segment, FieldInfos *fis)
{
    TermVectorsWriter *tvw = ALLOC(TermVectorsWriter);
    char file_name[SEGMENT_NAME_MAX_LENGTH];
    tvw->fis = fis;
    tvw->fields = ary_new_type_capa(TVField, TV_FIELD_INIT_CAPA);

    snprintf(file_name, SEGMENT_NAME_MAX_LENGTH, "%s.tvx", segment);
    tvw->tvx_out = store->new_output(store, file_name);

    snprintf(file_name, SEGMENT_NAME_MAX_LENGTH, "%s.tvd", segment);
    tvw->tvd_out = store->new_output(store, file_name);

    return tvw;
}

void tvw_close(TermVectorsWriter *tvw)
{
    os_close(tvw->tvx_out);
    os_close(tvw->tvd_out);
    ary_free(tvw->fields);
    free(tvw);
}

void tvw_open_doc(TermVectorsWriter *tvw)
{
    ary_size(tvw->fields) = 0;
    tvw->tvd_ptr = os_pos(tvw->tvd_out);
    os_write_u64(tvw->tvx_out, tvw->tvd_ptr);
}

void tvw_close_doc(TermVectorsWriter *tvw)
{
    int i;
    OutStream *tvd_out = tvw->tvd_out;
    os_write_u32(tvw->tvx_out, (f_u32)(os_pos(tvw->tvd_out) - tvw->tvd_ptr));
    os_write_vint(tvd_out, ary_size(tvw->fields));
    for (i = 0; i < ary_size(tvw->fields); i++) {
        os_write_vint(tvd_out, tvw->fields[i].field_num);
        os_write_vint(tvd_out, tvw->fields[i].size);
    }
}

void tvw_add_postings(TermVectorsWriter *tvw,
                      int field_num,
                      PostingList **plists,
                      int posting_count,
                      Offset *offsets,
                      int offset_count)
{
    int i, delta_start, delta_length;
    const char *last_term = EMPTY_STRING;
    off_t tvd_start_pos = os_pos(tvw->tvd_out);
    OutStream *tvd_out = tvw->tvd_out;
    PostingList *plist;
    Posting *posting;
    Occurence *occ;
    FieldInfo *fi = tvw->fis->fields[field_num];
    int store_positions = fi_store_positions(fi);

    ary_grow(tvw->fields);
    ary_last(tvw->fields).field_num = field_num;

    os_write_vint(tvd_out, posting_count);
    for (i = 0; i < posting_count; i++) {
        plist = plists[i];
        posting = plist->last;
        delta_start = hlp_string_diff(last_term, plist->term);
        delta_length = plist->term_len - delta_start;

        os_write_vint(tvd_out, delta_start);  /* write shared prefix length */
        os_write_vint(tvd_out, delta_length); /* write delta length */
        /* write delta chars */
        os_write_bytes(tvd_out,
                       (uchar *)(plist->term + delta_start),
                       delta_length);
        os_write_vint(tvd_out, posting->freq);
        last_term = plist->term;

        if (store_positions) {
            /* use delta encoding for positions */
            int last_pos = 0;
            for (occ = posting->first_occ; occ; occ = occ->next) {
                os_write_vint(tvd_out, occ->pos - last_pos);
                last_pos = occ->pos;
            }
        }

    }

    if (fi_store_offsets(fi)) {
        /* use delta encoding for offsets */
        int last_end = 0;
        os_write_vint(tvd_out, offset_count);  /* write shared prefix length */
        for (i = 0; i < offset_count; i++) {
            int start = offsets[i].start;
            int end = offsets[i].end;
            os_write_vint(tvd_out, start - last_end);
            os_write_vint(tvd_out, end - start);
            last_end = end;
        }
    }

    ary_last(tvw->fields).size = os_pos(tvd_out) - tvd_start_pos;
}


