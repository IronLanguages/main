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

using IronRuby.Runtime;
namespace IronRuby.Tests {
    public partial class Tests {

        /// <summary>
        /// Tests whether user initializer is properly called (w/o block argument).
        /// If this fails all RSpec tests fails.
        /// </summary>
        public void Scenario_RubyInitializers0() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def initialize *a
    puts a.inspect
  end
end

C.new 1,2,3
");
            }, @"[1, 2, 3]");
        }

        public void Scenario_RubyInitializers1() {
            AssertOutput(delegate() {
                CompilerTest(@"
x = Array.new(5) { break 'foo' }
puts x
");
            }, @"foo");
        }

        public void Scenario_RubyInitializers2A() {
            AssertOutput(delegate() {
                CompilerTest(@"
class A
  def initialize
    yield
  end
end

x = A.new { break 'foo' }
puts x
");
            }, @"foo");
        }

        /// <summary>
        /// Block's proc-converter ("new") is not alive when the block breaks.
        /// This tests whether "new" frame is cleaned up correctly.
        /// </summary>
        public void Scenario_RubyInitializers2B() {
            AssertOutput(delegate() {
                CompilerTest(@"
class A
  def initialize &b
    $b = b
  end
end

x = A.new { break 'foo' }
1.times(&$b) rescue p $!
");
            }, @"#<LocalJumpError: break from proc-closure>");
        }

        /// <summary>
        /// Retry returns from the 'new' frame (skipping initialize frame) as that frame is its proc-converter.
        /// The block is not redefined however. Only the proc-converter frame is updated to the new instance of the 'new' frame.
        /// </summary>
        public void Scenario_RubyInitializers3() {
            AssertOutput(delegate() {
                CompilerTest(@"
class A
  def initialize *a
    yield
  end
end

$i = 0
y = A.new($i += 1) {
  puts $i
  if $i < 3 then
    retry
  end
}
");
            }, @"
1
2
3");
        }

        /// <summary>
        /// The same case as Scenario_RubyInitializers3, but with initializer defined in library.
        /// </summary>
        public void Scenario_RubyInitializers4A() {
            AssertOutput(delegate() {
                CompilerTest(@"
$i = 0
y = Array.new($i += 1) {
  puts $i
  if $i < 3 then
    retry
  end
}
puts y.inspect
");
            }, @"
1
2
3
3
3
[nil, nil, nil]");
        }

        /// <summary>
        /// A block is yielded to from "initialize", not from the factory.
        /// </summary>
        public void Scenario_RubyInitializers4B() {
            AssertOutput(delegate() {
                CompilerTest(@"
class Array
  def initialize *args
    puts 'init'
    p self
  end
end
Array.new(10) { puts 'unreachable' }
");
            }, @"
init
[]
");
        }

        public void Scenario_RubyInitializers4C() {
            AssertOutput(delegate() {
                CompilerTest(@"
p Array.new(10) { break 5 }
");
            }, @"5");
        }

        /// <summary>
        /// Test initializers with procs (instead of blocks - the proc-converter is different now).
        /// </summary>
        public void Scenario_RubyInitializers5() {
            AssertOutput(delegate() {
                CompilerTest(@"
def bar &p
  x = Array.new(5, &p)
  puts ""A: #{x}""
end

z = bar { |i| puts ""B: #{i}""; break 'foo' }
puts ""C: #{z}""
");
            }, @"
B: 0
C: foo
");
        }

        public void Scenario_RubyInitializers6() {
            TestOutput(@"
class C
  undef :initialize rescue p $!

  define_method(:initialize) { |*args| p args }
  new(1,2,3)

  def initialize; puts 'init'; end
  new
end
", @"
#<TypeError: Cannot undefine `initialize' method>
[1, 2, 3]
init
");
        }

        public void RubyInitializersCaching1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C; end
C.new

class C
  def initialize
    puts 'init'
  end
end
C.new
");
            }, @"init");
        }

        public void RubyInitializersCaching2() {
            AssertOutput(delegate() {
                CompilerTest(@"
class S < String; end
puts S.new('foo')

class S
  def initialize a
    super 'bar'
  end
end
puts S.new('baz')
");
            }, @"
foo
bar
");
        }

        public void RubyInitializersCaching3() {
            TestOutput(@"
class Object
  remove_method :initialize
end

class BasicObject
  remove_method :initialize rescue p $!
end
", @"
#<NameError: Cannot remove BasicObject#initialize>
");
        }

        public void RubyAllocators1() {
            AssertOutput(delegate() {
                CompilerTest(@"
p Array.allocate
p Hash.allocate
p Range.allocate
p Regexp.allocate
p String.allocate
p MatchData.allocate
p Object.allocate
p Module.allocate
p Class.allocate
p Struct.new(:f,:g).allocate
p Exception.allocate
p IO.allocate

p Binding.allocate rescue p $!
p Struct.allocate rescue p $!
p Method.allocate rescue p $!
p UnboundMethod.allocate rescue p $!
p Bignum.allocate rescue p $!
p Fixnum.allocate rescue p $!
p Float.allocate rescue p $!
p TrueClass.allocate rescue p $!
p FalseClass.allocate rescue p $!
p NilClass.allocate rescue p $!
p Proc.allocate rescue p $!
");
            }, @"
[]
{}
nil..nil
//
""""
#<MatchData:*>
#<Object:*>
#<Module:*>
#<Class:*>
#<struct #<Class:*> f=nil, g=nil>
#<Exception: Exception>
#<IO:*>
#<TypeError: allocator undefined for Binding>
#<TypeError: allocator undefined for Struct>
#<TypeError: allocator undefined for Method>
#<TypeError: allocator undefined for UnboundMethod>
#<TypeError: allocator undefined for Bignum>
#<TypeError: allocator undefined for Fixnum>
#<TypeError: allocator undefined for Float>
#<TypeError: allocator undefined for TrueClass>
#<TypeError: allocator undefined for FalseClass>
#<TypeError: allocator undefined for NilClass>
#<TypeError: allocator undefined for Proc>
", OutputFlags.Match);
        }
    }
}
