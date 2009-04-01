/**
 * Exception Handling Framework
 *
 * Exception Handling looks something like this;
 *
 * <pre>
 *   TRY
 *       RAISE(EXCEPTION, msg1);
 *       break;
 *   case EXCEPTION:
 *       // This should be called
 *       exception_handled = true;
 *       HANDLED();
 *       break;
 *   default:
 *       // shouldn't enter here
 *       break;
 *   XFINALLY
 *       // this code will always be run
 *       if (close_widget_one(arg) == 0) {
 *           RAISE(EXCEPTION_CODE, msg);
 *       }
 *       // this code will also always run, even if the above exception is
 *       // raised
 *       if (close_widget_two(arg) == 0) {
 *           RAISE(EXCEPTION_CODE, msg);
 *       }
 *   XENDTRY
 * </pre>
 *
 * Basically exception handling uses the following macros;
 *
 * TRY 
 *   Sets up the exception handler and need be placed before any expected
 *   exceptions would be raised.
 *
 * case <EXCEPTION_CODE>:
 *   Internally the exception handling uses a switch statement so use the case
 *   statement with the appropriate error code to catch Exceptions. Hence, if
 *   you want to catch all exceptions, use the default keyword.
 *
 * HANDLED
 *   If you catch and handle an exception you need to explicitely call
 *   HANDLED(); or the exeption will be re-raised once the current exception
 *   handling context is left.
 *
 * case FINALLY:
 *   Code in this block is always called. Use this block to close any
 *   resources opened in the Exception handling body.
 *
 * ENDTRY
 *   Must be placed at the end of all exception handling code.
 *
 * XFINALLY
 *   Similar to case FINALLY: except that it uses a fall through (ie, you must
 *   not use a break before it) instead of a jump to get to it. This saves a
 *   jump. It must be used in combination with XENDTRY and must not have any
 *   other catches. This is an optimization so should probably be not be used
 *   in most cases.
 *
 * XCATCHALL
 *   Like XFINALLY but the block is only called when an exception is raised.
 *   Must use in combination with XENDTRY and do not have any other FINALLY or
 *   catch block.
 *
 * XENDTRY
 *   Must use in combination with XFINALLY or XCATCHALL. Simply, it doesn't
 *   jump to FINALLY, making it more efficient.
 */
#ifndef FRT_EXCEPT_H
#define FRT_EXCEPT_H

#include <setjmp.h>
#include "config.h"

#define BODY 0
#define FINALLY 1
#define EXCEPTION 2
#define FERRET_ERROR 2
#define IO_ERROR 3
#define FILE_NOT_FOUND_ERROR 4
#define ARG_ERROR 5
#define EOF_ERROR 6
#define UNSUPPORTED_ERROR 7
#define STATE_ERROR 8
#define PARSE_ERROR 9
#define MEM_ERROR 10
#define INDEX_ERROR 11
#define LOCK_ERROR 12

extern char *const UNSUPPORTED_ERROR_MSG;
extern char *const EOF_ERROR_MSG;
extern bool except_show_pos;

typedef struct xcontext_t
{
    jmp_buf jbuf;
    struct xcontext_t *next;
    const char *msg;
    volatile int excode;
    unsigned int handled : 1;
    unsigned int in_finally : 1;
} xcontext_t;

#define TRY\
  do {\
    xcontext_t xcontext;\
    xpush_context(&xcontext);\
    switch (setjmp(xcontext.jbuf)) {\
      case BODY:


#define XENDTRY\
    }\
    xpop_context();\
  } while (0);

#define ENDTRY\
    }\
    if (!xcontext.in_finally) {\
      xpop_context();\
      xcontext.in_finally = 1;\
      longjmp(xcontext.jbuf, FINALLY);\
    }\
  } while (0);

#define RETURN_EARLY() xpop_context()


#define XFINALLY default: xcontext.in_finally = 1;

#define XCATCHALL break; default: xcontext.in_finally = 1;

#define HANDLED() xcontext.handled = 1; /* true */

#define XMSG_BUFFER_SIZE 2048

#ifdef FRT_HAS_ISO_VARARGS
# define RAISE(excode, ...) do {\
  snprintf(xmsg_buffer, XMSG_BUFFER_SIZE, __VA_ARGS__);\
  snprintf(xmsg_buffer_final, XMSG_BUFFER_SIZE,\
          "Error occured in %s:%d - %s\n\t%s\n",\
          __FILE__, __LINE__, __func__, xmsg_buffer);\
  xraise(excode, xmsg_buffer_final);\
} while (0)
#elif defined(FRT_HAS_GNUC_VARARGS)
# define RAISE(excode, args...) do {\
  snprintf(xmsg_buffer, XMSG_BUFFER_SIZE, ##args);\
  snprintf(xmsg_buffer_final, XMSG_BUFFER_SIZE,\
          "Error occured in %s:%d - %s\n\t%s\n",\
          __FILE__, __LINE__, __func__, xmsg_buffer);\
  xraise(excode, xmsg_buffer_final);\
} while (0)

#else
extern void RAISE(int excode, const char *fmt, ...);
#endif
#define RAISE_HELL() RAISE(FERRET_ERROR, "Hell")


extern void xraise(int excode, const char *const msg);
extern void xpush_context(xcontext_t *context);
extern void xpop_context();

extern char xmsg_buffer[XMSG_BUFFER_SIZE];
extern char xmsg_buffer_final[XMSG_BUFFER_SIZE];

#endif
