/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using IronRuby.Runtime;

namespace IronRuby.Tests {
    public partial class Tests {
        public void Scenario_RubyNameMangling1() {
            Assert(RubyUtils.TryUnmangleName("ip_stack") == "IpStack");  // TODO
            Assert(RubyUtils.TryUnmangleName("stack") == "Stack");
            Assert(RubyUtils.TryUnmangleName("this_is_my_long_name") == "ThisIsMyLongName");
            Assert(RubyUtils.TryUnmangleName("") == null);
            Assert(RubyUtils.TryUnmangleName("f") == "F");
            Assert(RubyUtils.TryUnmangleName("fo") == "Fo");
            Assert(RubyUtils.TryUnmangleName("foo") == "Foo");
            Assert(RubyUtils.TryUnmangleName("foo_bar") == "FooBar");
            Assert(RubyUtils.TryUnmangleName("ma_m") == "MaM");
            Assert(RubyUtils.TryUnmangleName("initialize") == "initialize");
            Assert(RubyUtils.TryUnmangleName("Initialize") == "Initialize");
        }

        public void Scenario_RubyNameMangling2() {
            Assert(RubyUtils.MangleName("IPStack") == "ip_stack"); // TODO
            Assert(RubyUtils.MangleName("Stack") == "stack");
            Assert(RubyUtils.MangleName("ThisIsMyLongName") == "this_is_my_long_name");
            Assert(RubyUtils.MangleName("") == "");
            Assert(RubyUtils.MangleName("F") == "f");
            Assert(RubyUtils.MangleName("FO") == "fo");
            Assert(RubyUtils.MangleName("FOO") == "foo");
            Assert(RubyUtils.MangleName("FOOBar") == "foo_bar");
            Assert(RubyUtils.MangleName("MaM") == "ma_m");
            Assert(RubyUtils.MangleName("foo") == "foo");
            Assert(RubyUtils.MangleName("foo_bar=") == "foo_bar=");
            Assert(RubyUtils.MangleName("initialize") == "initialize");
            Assert(RubyUtils.MangleName("Initialize") == "Initialize");
        }
    }
}
