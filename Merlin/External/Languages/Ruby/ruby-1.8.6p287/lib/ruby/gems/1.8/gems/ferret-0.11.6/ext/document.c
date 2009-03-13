#include "document.h"
#include <string.h>

/****************************************************************************
 *
 * DocField
 *
 ****************************************************************************/

DocField *df_new(const char *name)
{
    DocField *df = ALLOC(DocField);
    df->name = estrdup(name);
    df->size = 0;
    df->capa = DF_INIT_CAPA;
    df->data = ALLOC_N(char *, df->capa);
    df->lengths = ALLOC_N(int, df->capa);
    df->destroy_data = false;
    df->boost = 1.0;
    return df;
}

DocField *df_add_data_len(DocField *df, char *data, int len)
{
    if (df->size >= df->capa) {
        df->capa <<= 2;
        REALLOC_N(df->data, char *, df->capa);
        REALLOC_N(df->lengths, int, df->capa);
    }
    df->data[df->size] = data;
    df->lengths[df->size] = len;
    df->size++;
    return df;
}

DocField *df_add_data(DocField *df, char *data)
{
    return df_add_data_len(df, data, strlen(data));
}

void df_destroy(DocField *df)
{
    if (df->destroy_data) {
        int i;
        for (i = 0; i < df->size; i++) {
            free(df->data[i]);
        }
    }
    free(df->data);
    free(df->lengths);
    free(df->name);
    free(df);
}

char *df_to_s(DocField *df)
{
    int i;
    int len = strlen(df->name) + 10;
    char *str, *s;
    for (i = 0; i < df->size; i++) {
        len += df->lengths[i] + 5;
    }
    s = str = ALLOC_N(char, len);
    sprintf(str, "%s: ", df->name);
    s += strlen(str);
    if (df->size == 1) {
        *(s++) = '"';
        strncpy(s, df->data[0], df->lengths[0]);
        s += df->lengths[0];
        *(s++) = '"';
        *(s++) = '\0';
    }
    else {
        *(s++) = '[';
        *(s++) = '"';
        strncpy(s, df->data[0], df->lengths[0]);
        s += df->lengths[0];
        *(s++) = '"';
        for (i = 1; i < df->size; i++) {
            *(s++) = ',';
            *(s++) = ' ';
            *(s++) = '"';
            strncpy(s, df->data[i], df->lengths[i]);
            s += df->lengths[i];
            *(s++) = '"';
        }
        sprintf(s, "]");
    }
    return str;
}

/****************************************************************************
 *
 * Document
 *
 ****************************************************************************/

Document *doc_new()
{
    Document *doc = ALLOC(Document);
    doc->field_dict = h_new_str(NULL, (free_ft)&df_destroy);
    doc->size = 0;
    doc->capa = DOC_INIT_CAPA;
    doc->fields = ALLOC_N(DocField *, doc->capa);
    doc->boost = 1.0;
    return doc;
}

DocField *doc_add_field(Document *doc, DocField *df)
{
    if (!h_set_safe(doc->field_dict, df->name, df)) {
        RAISE(EXCEPTION, "tried to add %s field which alread existed\n",
              df->name);
    }
    if (doc->size >= doc->capa) {
        doc->capa <<= 1;
        REALLOC_N(doc->fields, DocField *, doc->capa);
    }
    doc->fields[doc->size] = df;
    doc->size++;
    return df;
}

DocField *doc_get_field(Document *doc, const char *name)
{
    return h_get(doc->field_dict, name);
}

char *doc_to_s(Document *doc)
{
    int i;
    int len = 100;
    char **fields = ALLOC_N(char *, doc->size);
    char *buf, *s;
    for (i = 0; i < doc->size; i++) {
        fields[i] = df_to_s(doc->fields[i]);
        len += strlen(fields[i]) + 10;
    }
    s = buf = ALLOC_N(char, len);
    sprintf(buf, "Document [\n");
    s += strlen(buf);
    for (i = 0; i < doc->size; i++) {
        sprintf(s, "  =>%s\n", fields[i]);
        free(fields[i]);
        s += strlen(s);
    }
    return buf;
}

void doc_destroy(Document *doc)
{
    h_destroy(doc->field_dict);
    free(doc->fields);
    free(doc);
}

