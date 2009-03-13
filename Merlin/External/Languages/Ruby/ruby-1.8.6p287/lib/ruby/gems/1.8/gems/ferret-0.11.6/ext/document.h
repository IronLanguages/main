#ifndef FRT_DOCUMENT_H
#define FRT_DOCUMENT_H

#include "global.h"
#include "hash.h"

/****************************************************************************
 *
 * DocField
 *
 ****************************************************************************/

#define DF_INIT_CAPA 1
typedef struct DocField
{
    char *name;
    int size;
    int capa;
    int *lengths;
    char **data;
    float boost;
    bool destroy_data : 1;
} DocField;

extern DocField *df_new(const char *name);
extern DocField *df_add_data(DocField *df, char *data);
extern DocField *df_add_data_len(DocField *df, char *data, int len);
extern void df_destroy(DocField *df);
extern char *df_to_s(DocField *df);

/****************************************************************************
 *
 * Document
 *
 ****************************************************************************/

#define DOC_INIT_CAPA 8
typedef struct Document
{
    HashTable *field_dict;
    int size;
    int capa;
    DocField **fields;
    float boost;
} Document;

extern Document *doc_new();
extern DocField *doc_add_field(Document *doc, DocField *df);
extern DocField *doc_get_field(Document *doc, const char *fname);
extern char *doc_to_s(Document *doc);
extern void doc_destroy(Document *doc);

#endif
