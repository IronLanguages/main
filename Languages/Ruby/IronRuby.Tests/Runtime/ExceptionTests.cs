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
using IronRuby.Builtins;

namespace IronRuby.Tests {
    public partial class Tests {
        public void Scenario_RubyExceptions1() {
            AssertExceptionThrown<RuntimeError>(delegate() {
                CompilerTest(@"raise");
            });
        }

        public void Scenario_RubyExceptions1A() {
            AssertExceptionThrown<RuntimeError>(delegate() {
                CompilerTest(@"raise 'foo'");
            });
        }

        public void Scenario_RubyExceptions2A() {
            AssertExceptionThrown<NotImplementedError>(delegate() {
                CompilerTest(@"
$! = NotImplementedError.new
raise");
            });
        }

        public void Scenario_RubyExceptions2B() {
            AssertExceptionThrown<InvalidOperationException>(delegate() {
                CompilerTest("$! = NotImplementedError");
            });
        }

        public void Scenario_RubyExceptions2C() {
            AssertOutput(delegate() {
                CompilerTest(@"
x = NotImplementedError.new 'hello'
puts x.message

x.message[0] = ?H
puts x.message

x.send :initialize
puts x.message

x.send :initialize, 'Bye'
puts x.message
");
            }, @"
hello
Hello
NotImplementedError
Bye
");
        }
        
        public void Scenario_RubyExceptions2D() {
            AssertOutput(delegate() {
                CompilerTest(@"
x = IOError.new 'hello'
puts x.message

x.message[0] = ?H
puts x.message

x.send :initialize
puts x.message

x.send :initialize,'Bye'
puts x.message
");
            }, @"
hello
Hello
IOError
Bye
");
        }

        public void Scenario_RubyExceptions3() {
            AssertOutput(delegate() {
                CompilerTest(@"
begin
  raise
  print 'U'
rescue
  print 'X'
end
");
            }, "X");
        }

        public void Scenario_RubyExceptions4() {
            AssertOutput(delegate() {
                CompilerTest(@"
begin
  raise
  print 'U'
rescue IOError
  print 'U'
rescue StandardError
  print 'X'
end
");
            }, "X");
        }

        public void Scenario_RubyExceptions5() {
            AssertOutput(delegate() {
                CompilerTest(@"
begin
  raise
rescue StandardError => $x
  puts 'Caught'
  puts ""$! = '#{$!.class.name}'""
  puts ""$x = '#{$x.class.name}'""
end
");
            }, @"
Caught
$! = 'RuntimeError'
$x = 'RuntimeError'
");
        }

        public void Scenario_RubyExceptions6() {
            AssertOutput(delegate() {
                CompilerTest(@"
x = StandardError
begin
  raise
rescue x => x
  puts 'Caught'
  puts ""$! = '#{$!.class.name}'""
  puts ""x = '#{x.class.name}'""
end
");
            }, @"
Caught
$! = 'RuntimeError'
x = 'RuntimeError'
");
        }

        public void Scenario_RubyExceptions7() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo
  raise
end

begin
  foo
rescue Exception => e
  $found = false
  e.backtrace.each { |frame| if frame.index('foo') != nil then $found = true end } 
  puts $found
end");
            }, @"true");
        }

        public void Scenario_RubyExceptions8() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts 'Begin'
x = begin
  puts 'Raise'
  1
  raise
  puts 'Unreachable'
  2
rescue IOError
  puts 'Rescue1'
  3
rescue
  puts 'Rescue2'
  4
else
  puts 'Else'
  5
ensure
  puts 'Ensure'
  6 
end
puts x
puts 'End'
");
            }, @"
Begin
Raise
Rescue2
Ensure
4
End
");
        }

        public void Scenario_RubyExceptions9() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts 'Begin'
x = class C
  puts 'Raise'
  1
rescue IOError
  puts 'Rescue1'
  3
else
  puts 'Else'
  5
ensure
  puts 'Ensure'
  6 
end
puts x
puts 'End'
");
            }, @"
Begin
Raise
Else
Ensure
5
End
");
        }

        public void Scenario_RubyExceptions10() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts 'Begin'
begin
  puts 'Class'
  class C
    puts 'NoRaise'
  rescue
    puts 'Rescue'
  else
    puts 'Else'
  ensure
    puts 'Ensure'
  end
  puts 'ClassEnd'
rescue
  puts 'OuterRescue'
end
puts 'End'
");
            }, @"
Begin
Class
NoRaise
Else
Ensure
ClassEnd
End
");
        }

        public void Scenario_RubyExceptions11() {
            AssertOutput(delegate() {
                CompilerTest(@"
puts 'Begin'
begin
  puts 'Class'
  class C
    puts 'NoRaise'
  rescue
    puts 'Rescue'
  else
    puts 'Else'
  ensure
    puts 'Ensure'
  end
  puts 'ClassEnd'
rescue
  puts 'OutterRescue'
end
puts 'End'
");
            }, @"
Begin
Class
NoRaise
Else
Ensure
ClassEnd
End
");
        }

        public void Scenario_RubyExceptions12() {
            AssertOutput(delegate() {
                CompilerTest(@"
class A < Exception; end
class B < Exception; end
class C < Exception; end

begin
  raise B
rescue A,B,C => e
  puts e.class
end
");
            }, @"B");
        }

        /// <summary>
        /// Order of evaluation inside rescue clauses.
        /// </summary>
        public void Scenario_RubyExceptions12A() {
            AssertOutput(delegate() {
                CompilerTest(@"
class Module
  alias old ===

  def ===(other)
    puts ""cmp(#{self}, #{other})""
    
    old other
  end
end

class A < Exception; end
class B < Exception; end
class C < Exception; end
class D < Exception; end
class E < Exception; end
class F < Exception; end
class G < Exception; end

def id(t)
  puts ""r(#{t})""
  t
end

def foo
  raise F
rescue id(A),id(B),id(C)
  puts 'rescued 1'  # unreachable
rescue id(E),id(F),id(G)
  puts 'rescued 2'
end

foo
");
            }, @"
r(A)
r(B)
r(C)
cmp(A, F)
cmp(B, F)
cmp(C, F)
r(E)
r(F)
r(G)
cmp(E, F)
cmp(F, F)
rescued 2
");
        }

        /// <summary>
        /// Retry try-catch block.
        /// </summary>
        public void Scenario_RubyExceptions13() {
            AssertOutput(delegate() {
                CompilerTest(@"
i = 0
begin
  puts i
  i = i + 1
  if i < 3 then raise end
rescue
  puts 'retrying'
  retry
else
  puts 'no exception'
ensure
  puts 'giving up'
end
");
            }, @"
0
retrying
1
retrying
2
no exception
giving up
");
        }

        public void Scenario_RubyExceptions14() {
            AssertOutput(delegate() {
                CompilerTest(@"
begin
  raise IOError
rescue IOError
  puts 'rescued'
end
");
            }, "rescued");
        }

        public void Scenario_RubyExceptions15() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def exception
    IOError.new
  end
end

begin
  raise C.new
rescue IOError
  puts 'rescued'
end
");
            }, @"rescued");
        }

        public void Scenario_RubyExceptions16() {
            AssertOutput(delegate() {
                CompilerTest(@"
begin
  raise Exception.new('foo')
rescue Exception => e
  puts e.message
end
");
            }, @"foo");
        }

        public void JumpFromEnsure1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo
ensure
  return 1
end

p foo
");
            }, @"1");
        }

        public void Scenario_RubyExceptions_Globals() {
            AssertOutput(delegate() {
                CompilerTest(@"
p $!
p $@

$! = Exception.new
p $@ = nil
p $@ = ['foo']
p $@ = class A < Array; new; end
p $@ = [class S < String; new; end]
");
            }, @"
nil
nil
nil
[""foo""]
[]
[""""]
");

            // $! must be non-null when assigning to $@
            AssertExceptionThrown<ArgumentException>(delegate() {
                CompilerTest(@"$! = nil; $@ = ['foo']");
            });

            // $! non-nullity checked before type of backtracce:
            AssertExceptionThrown<ArgumentException>(delegate() {
                CompilerTest(@"$! = nil; $@ = 1");
            });

            // backtrace needs to be an array
            AssertExceptionThrown<InvalidOperationException>(delegate() {
                CompilerTest(@"$! = Exception.new; $@ = 1");
            });

            // backtrace needs to be an array of strings
            AssertExceptionThrown<InvalidOperationException>(delegate() {
                CompilerTest(@"$! = Exception.new; $@ = ['foo', 1]");
            });

            // backtrace needs to be an array, no conversion take place:
            AssertExceptionThrown<InvalidOperationException>(delegate() {
                CompilerTest(@"
$! = Exception.new
class B; def to_ary; []; end; end
$@ = B.new
");
            });

            // backtrace needs to be an array of strings, no item conversion take place:
            AssertExceptionThrown<InvalidOperationException>(delegate() {
                CompilerTest(@"
$! = Exception.new
class T; def to_str; ''; end; end
$@ = [T.new]
");
            });
        }

        public void Scenario_RubyRescueStatement1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo
  raise IOError.new('1')
end

foo rescue puts $!
puts '1' rescue puts '2'
");
            }, @"
1
1");
        }

        public void Scenario_RubyRescueExpression1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo
  raise
end

x = foo rescue 2
puts x
");
            }, @"2");
        }

        public void Scenario_RubyRescueExpression2() {
            TestOutput(@"
def foo
  raise
end

def bar
  x = foo rescue return
  puts 'unreachable'
end

def baz
  foo rescue return
  puts 'unreachable'
end

def bazz
  foo rescue return 2
  puts 'unreachable'
end

bar
baz
bazz
", @"");
        }

        public void ExceptionArg1() {
            AssertOutput(delegate() {
                CompilerTest(@"
begin
  raise SystemExit, 42
rescue SystemExit
  puts $!.status
end
");
            }, @"42");
        }

        public void ExceptionArg2() {
            AssertOutput(delegate() {
                CompilerTest(@"
begin
  raise SystemExit, 'foo'
rescue SystemExit
  puts $!.message
end
");
            }, @"foo");
        }

        public void RescueSplat1() {
            AssertOutput(delegate() {
                CompilerTest(@"
begin
  raise IOError
rescue *[IOError]
  puts 'ok'
end
");
            }, @"ok");
        }

        public void RescueSplat2() {
            AssertOutput(delegate() {
                CompilerTest(@"
begin
  raise IOError
rescue SyntaxError, *[SyntaxError, SyntaxError, IOError, nil]
  puts 'ok'
end
");
            }, @"ok");
        }

        public void RescueSplat3() {
            TestOutput(@"
class C
  def respond_to? name
    puts '?' + name.to_s
    false
  end

  def to_ary
    puts 'to_ary'
    [Exception]
  end

  def to_a
    puts 'to_a'
    [SyntaxError, IOError]
  end
end

begin
  begin
    raise IOError
  rescue *C.new
    puts :a
  end
rescue
  puts :b
end
", @"
?to_a
b
");
        }

        public void RescueSplat4() {
            TestOutput(@"
class C
  def respond_to? name
    puts '?' + name.to_s
    true
  end

  def to_a
    puts 'to_a'
    1
  end
end

begin
  begin
    raise IOError
  rescue *C.new
    puts 'ok'
  end
rescue
  p $!
end
", @"
?to_a
to_a
#<TypeError: C#to_a should return Array>
");
        }

        public void RescueSplat5() {
            TestOutput(@"
class E < Exception
  
end

class C
  def ===(other)
    puts ""===#{other}""
    0
  end
end

def foo(i)
  puts ""foo(#{i})""
end

a = []
b = [1,2,3]
c = 1

begin
  raise E.new
rescue *a,*b,*[foo(0), C.new],foo(1),C.new,2 => x
  p x
end
", @"
foo(0)
foo(1)
===E
#<E: E>
");
        }

        public void ExceptionMapping1() {
            TestOutput(@"
class LoadError
  def self.new message
    ZeroDivisionError.new message
  end
end

class SecurityError                 # partial trust: current directory detection
  def self.new message
    ZeroDivisionError.new message
  end
end

begin
  require 'non-existent'
rescue ZeroDivisionError
  puts 'Caught ZeroDivisionError'
end
", @"
Caught ZeroDivisionError
");
        }

        public void ExceptionMapping2() {
            TestOutput(@"
class ZeroDivisionError
  def self.new(message)
    puts 'new ZeroDivError'
    'not an exception'
  end
end

module Any
  def self.===(other)
    puts ""?#{other.inspect}""
    true
  end
end

begin
  1/0
rescue Any
  puts 'rescue'
  p $!
end

puts 'Done'
", @"
new ZeroDivError
?#<TypeError: exception object expected>
rescue
#<TypeError: exception object expected>
Done
");
        }

        public void ExceptionMapping3() {
            TestOutput(@"
class LoadError
  def initialize *args
    puts 'initLE'
  end
end

class SecurityError
  def initialize *args
    puts 'initSE'
  end
end

begin
  require 'non-existent'
rescue SecurityError
  puts 'Caught SecurityError'  # partial trust: current directory detection
rescue LoadError
  puts 'Caught LoadError'
end
", _driver.PartialTrust ? @"
initSE
Caught SecurityError
" : @"
initLE
Caught LoadError
");
        }

    }
}
