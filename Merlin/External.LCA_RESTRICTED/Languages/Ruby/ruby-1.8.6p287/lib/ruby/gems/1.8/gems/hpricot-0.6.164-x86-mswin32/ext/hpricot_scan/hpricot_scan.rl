/*
 * hpricot_scan.rl
 *
 * $Author: why $
 * $Date: 2006-05-08 22:03:50 -0600 (Mon, 08 May 2006) $
 *
 * Copyright (C) 2006 why the lucky stiff
 */
#include <ruby.h>

#ifndef RARRAY_LEN
#define RARRAY_LEN(arr)  RARRAY(arr)->len
#define RSTRING_LEN(str) RSTRING(str)->len
#define RSTRING_PTR(str) RSTRING(str)->ptr
#endif

#define NO_WAY_SERIOUSLY "*** This should not happen, please send a bug report with the HTML you're parsing to why@whytheluckystiff.net.  So sorry!"

static VALUE sym_xmldecl, sym_doctype, sym_procins, sym_stag, sym_etag, sym_emptytag, sym_comment,
      sym_cdata, sym_text;
static VALUE rb_eHpricotParseError;
static ID s_read, s_to_str;

#define ELE(N) \
  if (te > ts || text == 1) { \
    VALUE raw_string = Qnil; \
    ele_open = 0; text = 0; \
    if (ts != 0 && sym_##N != sym_cdata && sym_##N != sym_text && sym_##N != sym_procins && sym_##N != sym_comment) { \
      raw_string = rb_str_new(ts, te-ts); \
    } \
    rb_yield_tokens(sym_##N, tag, attr, raw_string, taint); \
  }

#define SET(N, E) \
  if (mark_##N == NULL || E == mark_##N) \
    N = rb_str_new2(""); \
  else if (E > mark_##N) \
    N = rb_str_new(mark_##N, E - mark_##N);

#define CAT(N, E) if (NIL_P(N)) { SET(N, E); } else { rb_str_cat(N, mark_##N, E - mark_##N); }

#define SLIDE(N) if ( mark_##N > ts ) mark_##N = buf + (mark_##N - ts);

#define ATTR(K, V) \
    if (!NIL_P(K)) { \
      if (NIL_P(attr)) attr = rb_hash_new(); \
      rb_hash_aset(attr, K, V); \
    }

#define TEXT_PASS() \
    if (text == 0) \
    { \
      if (ele_open == 1) { \
        ele_open = 0; \
        if (ts > 0) { \
          mark_tag = ts; \
        } \
      } else { \
        mark_tag = p; \
      } \
      attr = Qnil; \
      tag = Qnil; \
      text = 1; \
    }

#define EBLK(N, T) CAT(tag, p - T + 1); ELE(N);

%%{
  machine hpricot_scan;

  action newEle {
    if (text == 1) {
      CAT(tag, p);
      ELE(text);
      text = 0;
    }
    attr = Qnil;
    tag = Qnil;
    mark_tag = NULL;
    ele_open = 1;
  }

  action _tag { mark_tag = p; }
  action _aval { mark_aval = p; }
  action _akey { mark_akey = p; }
  action tag { SET(tag, p); }
  action tagc { SET(tag, p-1); }
  action aval { SET(aval, p); }
  action aunq { 
    if (*(p-1) == '"' || *(p-1) == '\'') { SET(aval, p-1); }
    else { SET(aval, p); }
  }
  action akey { SET(akey, p); }
  action xmlver { SET(aval, p); ATTR(rb_str_new2("version"), aval); }
  action xmlenc { SET(aval, p); ATTR(rb_str_new2("encoding"), aval); }
  action xmlsd  { SET(aval, p); ATTR(rb_str_new2("standalone"), aval); }
  action pubid  { SET(aval, p); ATTR(rb_str_new2("public_id"), aval); }
  action sysid  { SET(aval, p); ATTR(rb_str_new2("system_id"), aval); }

  action new_attr { 
    akey = Qnil;
    aval = Qnil;
    mark_akey = NULL;
    mark_aval = NULL;
  }

  action save_attr { 
    ATTR(akey, aval);
  }

  include hpricot_common "hpricot_common.rl";

}%%

%% write data nofinal;

#define BUFSIZE 16384

void rb_yield_tokens(VALUE sym, VALUE tag, VALUE attr, VALUE raw, int taint)
{
  VALUE ary;
  if (sym == sym_text) {
    raw = tag;
  }
  ary = rb_ary_new3(4, sym, tag, attr, raw);
  if (taint) { 
    OBJ_TAINT(ary);
    OBJ_TAINT(tag);
    OBJ_TAINT(attr);
    OBJ_TAINT(raw);
  }
  rb_yield(ary);
}

VALUE hpricot_scan(VALUE self, VALUE port)
{
  int cs, act, have = 0, nread = 0, curline = 1, text = 0;
  char *ts = 0, *te = 0, *buf = NULL, *eof = NULL;

  VALUE attr = Qnil, tag = Qnil, akey = Qnil, aval = Qnil, bufsize = Qnil;
  char *mark_tag = 0, *mark_akey = 0, *mark_aval = 0;
  int done = 0, ele_open = 0, buffer_size = 0;

  int taint = OBJ_TAINTED( port );
  if ( !rb_respond_to( port, s_read ) )
  {
    if ( rb_respond_to( port, s_to_str ) )
    {
      port = rb_funcall( port, s_to_str, 0 );
      StringValue(port);
    }
    else
    {
      rb_raise( rb_eArgError, "bad Hpricot argument, String or IO only please." );
    }
  }

  buffer_size = BUFSIZE;
  if (rb_ivar_defined(self, rb_intern("@buffer_size")) == Qtrue) {
    bufsize = rb_ivar_get(self, rb_intern("@buffer_size"));
    if (!NIL_P(bufsize)) {
      buffer_size = NUM2INT(bufsize);
    }
  }
  buf = ALLOC_N(char, buffer_size);

  %% write init;
  
  while ( !done ) {
    VALUE str;
    char *p = buf + have, *pe;
    int len, space = buffer_size - have;

    if ( space == 0 ) {
      /* We've used up the entire buffer storing an already-parsed token
       * prefix that must be preserved.  Likely caused by super-long attributes.
       * See ticket #13. */
      rb_raise(rb_eHpricotParseError, "ran out of buffer space on element <%s>, starting on line %d.", RSTRING_PTR(tag), curline);
    }

    if ( rb_respond_to( port, s_read ) )
    {
      str = rb_funcall( port, s_read, 1, INT2FIX(space) );
    }
    else
    {
      str = rb_str_substr( port, nread, space );
    }

    StringValue(str);
    memcpy( p, RSTRING_PTR(str), RSTRING_LEN(str) );
    len = RSTRING_LEN(str);
    nread += len;

    /* If this is the last buffer, tack on an EOF. */
    if ( len < space ) {
      p[len++] = 0;
      done = 1;
    }

    pe = p + len;
    %% write exec;
    
    if ( cs == hpricot_scan_error ) {
      free(buf);
      if ( !NIL_P(tag) )
      {
        rb_raise(rb_eHpricotParseError, "parse error on element <%s>, starting on line %d.\n" NO_WAY_SERIOUSLY, RSTRING_PTR(tag), curline);
      }
      else
      {
        rb_raise(rb_eHpricotParseError, "parse error on line %d.\n" NO_WAY_SERIOUSLY, curline);
      }
    }
    
    if ( done && ele_open )
    {
      ele_open = 0;
      if (ts > 0) {
        mark_tag = ts;
        ts = 0;
        text = 1;
      }
    }

    if ( ts == 0 )
    {
      have = 0;
      /* text nodes have no ts because each byte is parsed alone */
      if ( mark_tag != NULL && text == 1 )
      {
        if (done)
        {
          if (mark_tag < p-1)
          {
            CAT(tag, p-1);
            ELE(text);
          }
        }
        else
        {
          CAT(tag, p);
        }
      }
      mark_tag = buf;
    }
    else
    {
      have = pe - ts;
      memmove( buf, ts, have );
      SLIDE(tag);
      SLIDE(akey);
      SLIDE(aval);
      te = buf + (te - ts);
      ts = buf;
    }
  }
  free(buf);
}

void Init_hpricot_scan()
{
  VALUE mHpricot = rb_define_module("Hpricot");
  rb_define_attr(rb_singleton_class(mHpricot), "buffer_size", 1, 1);
  rb_define_singleton_method(mHpricot, "scan", hpricot_scan, 1);
  rb_eHpricotParseError = rb_define_class_under(mHpricot, "ParseError", rb_eStandardError);

  s_read = rb_intern("read");
  s_to_str = rb_intern("to_str");
  sym_xmldecl = ID2SYM(rb_intern("xmldecl"));
  sym_doctype = ID2SYM(rb_intern("doctype"));
  sym_procins = ID2SYM(rb_intern("procins"));
  sym_stag = ID2SYM(rb_intern("stag"));
  sym_etag = ID2SYM(rb_intern("etag"));
  sym_emptytag = ID2SYM(rb_intern("emptytag"));
  sym_comment = ID2SYM(rb_intern("comment"));
  sym_cdata = ID2SYM(rb_intern("cdata"));
  sym_text = ID2SYM(rb_intern("text"));
}
