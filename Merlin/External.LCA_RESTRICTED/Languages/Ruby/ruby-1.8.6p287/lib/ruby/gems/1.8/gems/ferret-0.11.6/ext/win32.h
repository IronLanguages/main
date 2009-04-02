#include "global.h"

#ifndef FRT_WIN32_H
#define FRT_WIN32_H

#include <io.h>

struct dirent
{
    char *d_name;
};

typedef struct DIR
{
    struct _finddata_t find_data;
    struct dirent de;
    long handle;
} DIR;

DIR *opendir(const char *dirname)
{
    DIR *d = ALLOC_AND_ZERO(DIR);
    char dirname_buf[MAX_FILE_PATH];
    long ff_res;
    sprintf(dirname_buf, "%s\\*", dirname);
    ff_res = _findfirst(dirname_buf, &d->find_data);
    if (ff_res < 0) {
        free(d);
        d = NULL;
    } else {
        d->de.d_name = NULL;
        d->handle = ff_res;
    }
    return d;
}

struct dirent *readdir(DIR *d)
{
    /* _findfirst already returned so do _findnext */
    if (d->de.d_name != NULL) {
        if (_findnext(d->handle, &d->find_data) < 0) {
            return NULL;
        }
    }
    d->de.d_name = d->find_data.name;
    return &d->de;
}

void closedir(DIR *d)
{
    _findclose(d->handle);
    free(d);
}
#endif
