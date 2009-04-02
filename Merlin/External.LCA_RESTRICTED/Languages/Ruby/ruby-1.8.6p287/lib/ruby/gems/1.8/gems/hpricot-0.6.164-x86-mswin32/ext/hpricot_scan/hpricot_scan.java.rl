
import java.io.IOException;

import org.jruby.Ruby;
import org.jruby.RubyClass;
import org.jruby.RubyHash;
import org.jruby.RubyModule;
import org.jruby.RubyNumeric;
import org.jruby.RubyObjectAdapter;
import org.jruby.RubyString;
import org.jruby.javasupport.JavaEmbedUtils;
import org.jruby.runtime.Block;
import org.jruby.runtime.CallbackFactory;
import org.jruby.runtime.builtin.IRubyObject;
import org.jruby.exceptions.RaiseException;
import org.jruby.runtime.load.BasicLibraryService;

public class HpricotScanService implements BasicLibraryService {
       public static String NO_WAY_SERIOUSLY="*** This should not happen, please send a bug report with the HTML you're parsing to why@whytheluckystiff.net.  So sorry!";
       private static RubyObjectAdapter rubyApi;

       public void ELE(IRubyObject N) {
         if (te > ts || text) {
           IRubyObject raw_string = runtime.getNil();
           ele_open = false; text = false;
           if (ts != -1 && N != cdata && N != sym_text && N != procins && N != comment) { 
             raw_string = runtime.newString(new String(buf,ts,te-ts));
           } 
           rb_yield_tokens(N, tag[0], attr, raw_string, taint);
         }
       }

       public void SET(IRubyObject[] N, int E) {
         int mark = 0;
         if(N == tag) { 
           if(mark_tag == -1 || E == mark_tag) {
             tag[0] = runtime.newString("");
           } else if(E > mark_tag) {
             tag[0] = runtime.newString(new String(buf,mark_tag, E-mark_tag));
           }
         } else if(N == akey) {
           if(mark_akey == -1 || E == mark_akey) {
             akey[0] = runtime.newString("");
           } else if(E > mark_akey) {
             akey[0] = runtime.newString(new String(buf,mark_akey, E-mark_akey));
           }
         } else if(N == aval) {
           if(mark_aval == -1 || E == mark_aval) {
             aval[0] = runtime.newString("");
           } else if(E > mark_aval) {
             aval[0] = runtime.newString(new String(buf,mark_aval, E-mark_aval));
           }
         }
       }

       public void CAT(IRubyObject[] N, int E) {
         if(N[0].isNil()) {
           SET(N,E);
         } else {
           int mark = 0;
           if(N == tag) {
             mark = mark_tag;
           } else if(N == akey) {
             mark = mark_akey;
           } else if(N == aval) {
             mark = mark_aval;
           }
           ((RubyString)(N[0])).append(runtime.newString(new String(buf, mark, E-mark)));
         }
       }

       public void SLIDE(Object N) {
           int mark = 0;
           if(N == tag) {
             mark = mark_tag;
           } else if(N == akey) {
             mark = mark_akey;
           } else if(N == aval) {
             mark = mark_aval;
           }
           if(mark > ts) {
             if(N == tag) {
               mark_tag  -= ts;
             } else if(N == akey) {
               mark_akey -= ts;
             } else if(N == aval) {
               mark_aval -= ts;
             }
           }
       }

       public void ATTR(IRubyObject K, IRubyObject V) {
         if(!K.isNil()) {
           if(attr.isNil()) {
             attr = RubyHash.newHash(runtime);
           }
           ((RubyHash)attr).op_aset(runtime.getCurrentContext(),K,V);
           // ((RubyHash)attr).aset(K,V);
         }
       }

       public void ATTR(IRubyObject[] K, IRubyObject V) {
         ATTR(K[0],V);
       }

       public void ATTR(IRubyObject K, IRubyObject[] V) {
         ATTR(K,V[0]);
       }

       public void ATTR(IRubyObject[] K, IRubyObject[] V) {
         ATTR(K[0],V[0]);
       }

       public void TEXT_PASS() {
         if(!text) { 
           if(ele_open) { 
             ele_open = false; 
             if(ts > -1) { 
               mark_tag = ts; 
             } 
           } else {
             mark_tag = p; 
           } 
           attr = runtime.getNil(); 
           tag[0] = runtime.getNil(); 
           text = true; 
         }
       }

       public void EBLK(IRubyObject N, int T) {
         CAT(tag, p - T + 1);
         ELE(N);
       }


       public void rb_raise(RubyClass error, String message) {
              throw new RaiseException(runtime, error, message, true);
       }

       public IRubyObject rb_str_new2(String s) {
              return runtime.newString(s);
       }

%%{
  machine hpricot_scan;

  action newEle {
    if (text) {
      CAT(tag, p);
      ELE(sym_text);
      text = false;
    }
    attr = runtime.getNil();
    tag[0] = runtime.getNil();
    mark_tag = -1;
    ele_open = true;
  }

  action _tag { mark_tag = p; }
  action _aval { mark_aval = p; }
  action _akey { mark_akey = p; }
  action tag { SET(tag, p); }
  action tagc { SET(tag, p-1); }
  action aval { SET(aval, p); }
  action aunq { 
    if (buf[p-1] == '"' || buf[p-1] == '\'') { SET(aval, p-1); }
    else { SET(aval, p); }
  }
  action akey { SET(akey, p); }
  action xmlver { SET(aval, p); ATTR(rb_str_new2("version"), aval); }
  action xmlenc { SET(aval, p); ATTR(rb_str_new2("encoding"), aval); }
  action xmlsd  { SET(aval, p); ATTR(rb_str_new2("standalone"), aval); }
  action pubid  { SET(aval, p); ATTR(rb_str_new2("public_id"), aval); }
  action sysid  { SET(aval, p); ATTR(rb_str_new2("system_id"), aval); }

  action new_attr { 
    akey[0] = runtime.getNil();
    aval[0] = runtime.getNil();
    mark_akey = -1;
    mark_aval = -1;
  }

  action save_attr { 
    ATTR(akey, aval);
  }

  include hpricot_common "hpricot_common.rl";

}%%

%% write data nofinal;

public final static int BUFSIZE=16384;

private void rb_yield_tokens(IRubyObject sym, IRubyObject tag, IRubyObject attr, IRubyObject raw, boolean taint) {
  IRubyObject ary;
  if (sym == runtime.newSymbol("text")) {
    raw = tag;
  }
  ary = runtime.newArray(new IRubyObject[]{sym, tag, attr, raw});
  if (taint) { 
    ary.setTaint(true);
    tag.setTaint(true);
    attr.setTaint(true);
    raw.setTaint(true);
  }
  block.yield(runtime.getCurrentContext(), ary, null, null, false);
}


int cs, act, have = 0, nread = 0, curline = 1, p=-1;
boolean text = false;
int ts=-1, te;
int eof=-1;
char[] buf;
Ruby runtime;
IRubyObject attr, bufsize;
IRubyObject[] tag, akey, aval;
int mark_tag, mark_akey, mark_aval;
boolean done = false, ele_open = false;
int buffer_size = 0;        
boolean taint = false;
Block block = null;


IRubyObject xmldecl, doctype, procins, stag, etag, emptytag, comment,
      cdata, sym_text;

IRubyObject hpricot_scan(IRubyObject recv, IRubyObject port) {
  attr = bufsize = runtime.getNil();
  tag = new IRubyObject[]{runtime.getNil()};
  akey = new IRubyObject[]{runtime.getNil()};
  aval = new IRubyObject[]{runtime.getNil()};

  RubyClass rb_eHpricotParseError = runtime.getModule("Hpricot").getClass("ParseError");

  taint = port.isTaint();
  if ( !port.respondsTo("read")) {
    if ( port.respondsTo("to_str")) {
      port = port.callMethod(runtime.getCurrentContext(),"to_str");
    } else {
      throw runtime.newArgumentError("bad Hpricot argument, String or IO only please.");
    }
  }

  buffer_size = BUFSIZE;
  if (rubyApi.getInstanceVariable(recv, "@buffer_size") != null) {
    bufsize = rubyApi.getInstanceVariable(recv, "@buffer_size");
    if (!bufsize.isNil()) {
      buffer_size = RubyNumeric.fix2int(bufsize);
    }
  }
  buf = new char[buffer_size];

  %% write init;

  while( !done ) {
    IRubyObject str;
    p = have;
    int pe;
    int len, space = buffer_size - have;

    if ( space == 0 ) {
      /* We've used up the entire buffer storing an already-parsed token
       * prefix that must be preserved.  Likely caused by super-long attributes.
       * See ticket #13. */
      rb_raise(rb_eHpricotParseError, "ran out of buffer space on element <" + tag.toString() + ">, starting on line "+curline+".");
    }

    if (port.respondsTo("read")) {
      str = port.callMethod(runtime.getCurrentContext(),"read",runtime.newFixnum(space));
    } else {
      str = ((RubyString)port).substr(nread,space);
    }

    str = str.convertToString();
    String sss = str.toString();
    char[] chars = sss.toCharArray();
    System.arraycopy(chars,0,buf,p,chars.length);

    len = sss.length();
    nread += len;

    if ( len < space ) {
      len++;
      done = true;
    }

    pe = p + len;
    char[] data = buf;

    %% write exec;
    
    if ( cs == hpricot_scan_error ) {
      if(!tag[0].isNil()) {
        rb_raise(rb_eHpricotParseError, "parse error on element <"+tag.toString()+">, starting on line "+curline+".\n" + NO_WAY_SERIOUSLY);
      } else {
        rb_raise(rb_eHpricotParseError, "parse error on line "+curline+".\n" + NO_WAY_SERIOUSLY);
      }
    }
    
    if ( done && ele_open ) {
      ele_open = false;
      if(ts > -1) {
        mark_tag = ts;
        ts = -1;
        text = true;
      }
    }

    if(ts == -1) {
      have = 0;
      /* text nodes have no ts because each byte is parsed alone */
      if(mark_tag != -1 && text) {
        if (done) {
          if(mark_tag < p-1) {
            CAT(tag, p-1);
            ELE(sym_text);
          }
        } else {
          CAT(tag, p);
        }
      }
      mark_tag = 0;
    } else {
      have = pe - ts;
      System.arraycopy(buf,ts,buf,0,have);
      SLIDE(tag);
      SLIDE(akey);
      SLIDE(aval);
      te = (te - ts);
      ts = 0;
    }
  }
  return runtime.getNil();
}

public static IRubyObject __hpricot_scan(IRubyObject recv, IRubyObject port, Block block) {
  Ruby runtime = recv.getRuntime();
  HpricotScanService service = new HpricotScanService();
  service.runtime = runtime;
  service.xmldecl = runtime.newSymbol("xmldecl");
  service.doctype = runtime.newSymbol("doctype");
  service.procins = runtime.newSymbol("procins");
  service.stag = runtime.newSymbol("stag");
  service.etag = runtime.newSymbol("etag");
  service.emptytag = runtime.newSymbol("emptytag");
  service.comment = runtime.newSymbol("comment");
  service.cdata = runtime.newSymbol("cdata");
  service.sym_text = runtime.newSymbol("text");
  service.block = block;
  return service.hpricot_scan(recv, port);
}


public boolean basicLoad(final Ruby runtime) throws IOException {
       Init_hpricot_scan(runtime);
       return true;
}

public static void Init_hpricot_scan(Ruby runtime) {
  RubyModule mHpricot = runtime.defineModule("Hpricot");
  mHpricot.getMetaClass().attr_accessor(runtime.getCurrentContext(),new IRubyObject[]{runtime.newSymbol("buffer_size")});
  CallbackFactory fact = runtime.callbackFactory(HpricotScanService.class);
  mHpricot.getMetaClass().defineMethod("scan",fact.getSingletonMethod("__hpricot_scan",IRubyObject.class));
  mHpricot.defineClassUnder("ParseError",runtime.getClass("StandardError"),runtime.getClass("StandardError").getAllocator());
  rubyApi = JavaEmbedUtils.newObjectAdapter();
}
}
