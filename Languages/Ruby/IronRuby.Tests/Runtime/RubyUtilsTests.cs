/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using IronRuby.Runtime;
using IronRuby.Builtins;
using System.Collections.Generic;
using System.Text;

namespace IronRuby.Tests {
    public partial class Tests {
        public void NameMangling1() {
            Assert(RubyUtils.TryUnmangleName("stack") == "Stack");
            Assert(RubyUtils.TryUnmangleName("this_is_my_long_name") == "ThisIsMyLongName");
            Assert(RubyUtils.TryUnmangleName("f") == "F");
            Assert(RubyUtils.TryUnmangleName("initialize") == "Initialize");

            // non-alpha characters are treated as lower-case letters: 
            Assert(RubyUtils.TryUnmangleName("foo_bar=") == "FooBar=");
            Assert(RubyUtils.TryUnmangleName("foo?") == "Foo?");
            Assert(RubyUtils.TryUnmangleName("???") == "???");
            Assert(RubyUtils.TryUnmangleName("1_2_3") == "123");
            
            // special cases:
            Assert(RubyUtils.TryUnmangleName("on") == "On");
            Assert(RubyUtils.TryUnmangleName("or") == "Or");
            Assert(RubyUtils.TryUnmangleName("up") == "Up");
            Assert(RubyUtils.TryUnmangleName("in") == "In");
            Assert(RubyUtils.TryUnmangleName("to") == "To");
            Assert(RubyUtils.TryUnmangleName("of") == "Of");
            Assert(RubyUtils.TryUnmangleName("it") == "It");
            Assert(RubyUtils.TryUnmangleName("is") == "Is");
            Assert(RubyUtils.TryUnmangleName("if") == "If");
            Assert(RubyUtils.TryUnmangleName("go") == "Go");
            Assert(RubyUtils.TryUnmangleName("do") == "Do");
            Assert(RubyUtils.TryUnmangleName("by") == "By");
            Assert(RubyUtils.TryUnmangleName("at") == "At");
            Assert(RubyUtils.TryUnmangleName("as") == "As");
            Assert(RubyUtils.TryUnmangleName("my") == "My");
            Assert(RubyUtils.TryUnmangleName("me") == "Me");
            Assert(RubyUtils.TryUnmangleName("no") == "No");
            
            Assert(RubyUtils.TryUnmangleName("ip") == "IP");
            Assert(RubyUtils.TryUnmangleName("rx") == "RX");
            Assert(RubyUtils.TryUnmangleName("pi") == "PI");

            Assert(RubyUtils.TryUnmangleName("na_n") == null);
            Assert(RubyUtils.TryUnmangleName("nan") == "Nan");
            Assert(RubyUtils.TryUnmangleName("ip_address") == "IPAddress");
            Assert(RubyUtils.TryUnmangleName("i_id_id") == "IIdId");
            Assert(RubyUtils.TryUnmangleName("i_ip_ip") == null);
            Assert(RubyUtils.TryUnmangleName("ip_foo_ip") == "IPFooIP");
            Assert(RubyUtils.TryUnmangleName("active_x") == "ActiveX");

            Assert(RubyUtils.TryUnmangleName("get_u_int16") == "GetUInt16");
            Assert(RubyUtils.TryUnmangleName("get_ui_parent_core") == "GetUIParentCore");
            
            // TODO: ???
            Assert(RubyUtils.TryUnmangleName("i_pv6") == "IPv6");
            
            // names that cannot be mangled:
            Assert(RubyUtils.TryUnmangleName("") == null);
            Assert(RubyUtils.TryUnmangleName("IPX") == null);
            Assert(RubyUtils.TryUnmangleName("FO") == null);
            Assert(RubyUtils.TryUnmangleName("FOOBar") == null);
            Assert(RubyUtils.TryUnmangleName("FooBAR") == null);
            Assert(RubyUtils.TryUnmangleName("foo__bar") == null);
            Assert(RubyUtils.TryUnmangleName("_foo") == null);
            Assert(RubyUtils.TryUnmangleName("foo_") == null);

            // special method names:
            Assert(RubyUtils.TryUnmangleMethodName("initialize") == null);
            Assert(RubyUtils.TryUnmangleMethodName("class") == null);
            Assert(RubyUtils.TryUnmangleMethodName("message") == "Message"); // we don't special case Exception.Message 
        }

        public void NameMangling2() {
            Assert(RubyUtils.TryMangleName("Stack") == "stack");
            Assert(RubyUtils.TryMangleName("ThisIsMyLongName") == "this_is_my_long_name");
            Assert(RubyUtils.TryMangleName("F") == "f");
            Assert(RubyUtils.TryMangleName("Initialize") == "initialize");
            Assert(RubyUtils.TryMangleName("fooBar") == "foo_bar");

            // characters that are not upper case letters are treated as lower-case:
            Assert(RubyUtils.TryMangleName("Foo123bar") == "foo123bar");
            Assert(RubyUtils.TryMangleName("123Bar") == "123_bar");
            Assert(RubyUtils.TryMangleName("?Bar") == "?_bar");

            // special cases:
            Assert(RubyUtils.TryUnmangleName("ON") == null);
            Assert(RubyUtils.TryUnmangleName("OR") == null);
            Assert(RubyUtils.TryUnmangleName("UP") == null);
            Assert(RubyUtils.TryUnmangleName("IN") == null);
            Assert(RubyUtils.TryUnmangleName("TO") == null);
            Assert(RubyUtils.TryUnmangleName("OF") == null);
            Assert(RubyUtils.TryUnmangleName("IT") == null);
            Assert(RubyUtils.TryUnmangleName("IF") == null);
            Assert(RubyUtils.TryUnmangleName("IS") == null);
            Assert(RubyUtils.TryUnmangleName("GO") == null);
            Assert(RubyUtils.TryUnmangleName("DO") == null);
            Assert(RubyUtils.TryUnmangleName("BY") == null);
            Assert(RubyUtils.TryUnmangleName("AT") == null);
            Assert(RubyUtils.TryUnmangleName("AS") == null);
            Assert(RubyUtils.TryUnmangleName("MY") == null);
            Assert(RubyUtils.TryUnmangleName("ME") == null);
            Assert(RubyUtils.TryUnmangleName("ID") == null);
            Assert(RubyUtils.TryUnmangleName("OK") == null);
            Assert(RubyUtils.TryUnmangleName("NO") == null);


            Assert(RubyUtils.TryMangleName("NaN") == null);
            Assert(RubyUtils.TryMangleName("NaNValue") == null);
            Assert(RubyUtils.TryMangleName("At") == "at");
            Assert(RubyUtils.TryMangleName("IP") == "ip");
            Assert(RubyUtils.TryMangleName("FO") == "fo");
            Assert(RubyUtils.TryMangleName("PI") == "pi");
            Assert(RubyUtils.TryMangleName("IPAddress") == "ip_address");
            Assert(RubyUtils.TryMangleName("MyDB") == "my_db");
            Assert(RubyUtils.TryMangleName("PyPy") == "py_py");
            Assert(RubyUtils.TryMangleName("IPFooIP") == "ip_foo_ip");
            Assert(RubyUtils.TryMangleName("ActiveX") == "active_x");

            Assert(RubyUtils.TryMangleName("GetUInt16") == "get_u_int16");
            Assert(RubyUtils.TryMangleName("GetUIParentCore") == "get_ui_parent_core");

            // TODO: ???
            Assert(RubyUtils.TryMangleName("IPv6") == "i_pv6");

            // names that cannot be mangled:
            Assert(RubyUtils.TryMangleName("") == null);
            Assert(RubyUtils.TryMangleName("IPX") == null);
            Assert(RubyUtils.TryMangleName("FOO") == null);
            Assert(RubyUtils.TryMangleName("FOOBar") == null);
            Assert(RubyUtils.TryMangleName("FooBAR") == null);
            Assert(RubyUtils.TryMangleName("foo") == null);
            Assert(RubyUtils.TryMangleName("initialize") == null);

            // name containing underscore(s) cannot be mangled:
            Assert(RubyUtils.TryMangleName("a_b") == null);
            Assert(RubyUtils.TryMangleName("add_Foo") == null);
            Assert(RubyUtils.TryMangleName("B__") == null);
            Assert(RubyUtils.TryMangleName("foo_bar=") == null);
            Assert(RubyUtils.TryMangleName("foo__bar") == null);
            Assert(RubyUtils.TryMangleName("_foo") == null);
            Assert(RubyUtils.TryMangleName("foo_") == null);

            // special method names:
            Assert(RubyUtils.TryMangleMethodName("Initialize") == null);
            Assert(RubyUtils.TryMangleMethodName("Class") == null);
            Assert(RubyUtils.TryMangleMethodName("Message") == "message"); // we don't special case Exception.Message
        }

        [Options(NoRuntime = true)]
        public void DelegateChainClone1() {
            StringBuilder sb = new StringBuilder();
            Action<RubyModule> f = (_) => { sb.Append('1'); };
            f += (_) => { sb.Append('2'); };
            f += (_) => { sb.Append('3'); };

            f(null);
            Assert(sb.ToString() == "123");
            sb.Length = 0;

            Action<RubyModule> g = Utils.CloneInvocationChain(f);
            g += (_) => { sb.Append('4'); };
            
            g(null);
            Assert(sb.ToString() == "1234");
            sb.Length = 0;

            f(null);
            Assert(sb.ToString() == "123");
        }
    }
}
