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

using Microsoft.Scripting;
namespace IronRuby.Tests {
    public partial class Tests {
        private bool PreciseTraces {
            get { return !_driver.PartialTrust || _driver.Interpret; }
        }

        public void Backtrace1() {
            AssertOutput(delegate() {
                Engine.CreateScriptSourceFromString(@"
def goo()
  foo()
end

def foo()
  bar()
end

def bar()  
  baz()
end

def baz()
  puts caller[0..3]
end

goo
", "Backtrace1.rb", SourceCodeKind.File).Execute();
            }, PreciseTraces ? @"
Backtrace1.rb:11:in `bar'
Backtrace1.rb:7:in `foo'
Backtrace1.rb:3:in `goo'
Backtrace1.rb:18
" : @"
Backtrace1.rb:10:in `bar'
Backtrace1.rb:6:in `foo'
Backtrace1.rb:2:in `goo'
:0
");
        }

        public void Backtrace2() {
            AssertOutput(delegate() {
                CompilerTest(@"
def goo()
  foo()
end

def foo()
  bar()
end

def bar()  
  baz()
end

def baz()
  raise
end

begin
  goo
rescue
  puts $@[0..4]
end
");
            }, PreciseTraces ? @"
Backtrace2.rb:15:in `baz'
Backtrace2.rb:11:in `bar'
Backtrace2.rb:7:in `foo'
Backtrace2.rb:3:in `goo'
Backtrace2.rb:19
" : @"
Backtrace2.rb:14:in `baz'
Backtrace2.rb:10:in `bar'
Backtrace2.rb:6:in `foo'
Backtrace2.rb:2:in `goo'
:0
");
        }

        public void Backtrace3() {
            AssertOutput(delegate() {
                CompilerTest(@"
def goo()
  foo()
rescue  
  raise
end

def foo()
  bar()
end

def bar()
  baz()
rescue  
  raise
end

def baz()
  raise
end

begin
  goo
rescue
  puts $@[0..4]
end
");
            }, PreciseTraces ? @"
Backtrace3.rb:19:in `baz'
Backtrace3.rb:13:in `bar'
Backtrace3.rb:9:in `foo'
Backtrace3.rb:3:in `goo'
Backtrace3.rb:23
" : @"
Backtrace3.rb:18:in `baz'
Backtrace3.rb:12:in `bar'
Backtrace3.rb:8:in `foo'
Backtrace3.rb:2:in `goo'
:0
");
        }
    }
}