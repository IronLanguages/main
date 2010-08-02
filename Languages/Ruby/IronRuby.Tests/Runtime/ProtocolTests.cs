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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Tests {

    public class DefaultProtocolTester : LibraryInitializer {
        protected override void LoadModules() {
            DefineGlobalModule("Tests", typeof(DefaultProtocolTester), (int)ModuleRestrictions.NoUnderlyingType, null, (module) => {
                    DefineLibraryMethod(module, "to_int_to_str", (int)RubyMethodAttributes.PublicSingleton, new[] {
                        LibraryOverload.Reflect(new Func<RubyModule, Union<int, MutableString>, RubyArray>(ToIntToStr)),
                    });
                    DefineLibraryMethod(module, "to_str_to_int", (int)RubyMethodAttributes.PublicSingleton, new[] {
                        LibraryOverload.Reflect(new Func<RubyModule, Union<MutableString, int>, RubyArray>(ToStrToInt)),
                    });
                }, 
                null, RubyModule.EmptyArray);
        }

        public static RubyArray ToIntToStr(RubyModule/*!*/ self, [DefaultProtocol]Union<int, MutableString> value) {
            return RubyOps.MakeArray2(value.First, value.Second);
        }

        public static RubyArray ToStrToInt(RubyModule/*!*/ self, [DefaultProtocol]Union<MutableString, int> value) {
            return RubyOps.MakeArray2(value.First, value.Second);
        }
    }

    public partial class Tests {
        public void ToIntegerConversion1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def to_i
    1000000000000
  end
end

class D
  def to_int
    1
  end
end

class E
  def respond_to? name
    puts name
    false
  end
end

puts Integer(123)
puts Integer(1230000000000000)
puts Integer(C.new)
puts Integer(D.new)
puts Integer(E.new) rescue puts $!
");
            }, @"
123
1230000000000000
1000000000000
1
to_int
to_i
can't convert E into Integer
");
        }

        private const string StrIntDeclarations = @"
require $protocol_tester

class A
  def to_str
    'A'
  end
end

class B
  def to_int
    1
  end
end

class C
  def to_str
    'C'
  end

  def to_int
    2
  end
end

class E
  def respond_to? name
    puts name
    false
  end
end
";
        
        public void ToIntToStrConversion1() {
            Context.DefineGlobalVariable("protocol_tester", MutableString.CreateAscii(typeof(DefaultProtocolTester).AssemblyQualifiedName));

            Engine.Execute(StrIntDeclarations);

            AssertOutput(delegate() {
                CompilerTest(@"
p Tests.to_int_to_str(123)
p Tests.to_int_to_str('xxx')
p Tests.to_int_to_str(A.new)
p Tests.to_int_to_str(B.new)
p Tests.to_int_to_str(C.new)
p Tests.to_int_to_str(E.new) rescue puts $!
");
            }, @"
[123, nil]
[0, ""xxx""]
[0, ""A""]
[1, nil]
[2, nil]
to_int
to_str
can't convert E into String
");

            AssertOutput(delegate() {
                CompilerTest(@"
p Tests.to_str_to_int(123)
p Tests.to_str_to_int('xxx')
p Tests.to_str_to_int(A.new)
p Tests.to_str_to_int(B.new)
p Tests.to_str_to_int(C.new)
p Tests.to_str_to_int(E.new) rescue puts $!
");
            }, @"
[nil, 123]
[""xxx"", 0]
[""A"", 0]
[nil, 1]
[""C"", 0]
to_str
to_int
can't convert E into Fixnum
");
        }

        public void ConvertToFixnum1() {
            AssertOutput(() => CompilerTest(@"
class Float
  def to_int
    2
  end
end

p Array.new(4.3004, 1)                     # implicit conversion, to_int not called 
[][8e19] rescue p $!                       # overflow -> RangeError

include System
[Byte, SByte, Int16, UInt16, UInt32, Int64, UInt64, Single, Decimal].each_with_index do |n,i|
  p Array.new(n.new(i), i)
end
"), @"
[1, 1, 1, 1]
#<RangeError: float * out of range of Fixnum>
[]
[1]
[2, 2]
[3, 3, 3]
[4, 4, 4, 4]
[5, 5, 5, 5, 5]
[6, 6, 6, 6, 6, 6]
[7, 7, 7, 7, 7, 7, 7]
[8, 8, 8, 8, 8, 8, 8, 8]
", OutputFlags.Match);
        }

        /// <summary>
        /// Kernel#Array first tries to_ary and then to_a. We need to invalidate the cache when to_ary is added.
        /// </summary>
        public void ProtocolCaching1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def to_a
    puts 'to_a'
    [1]
  end
end

obj = C.new
Array(obj)

class C
  def to_ary
    puts 'to_ary'
    [2]
  end
end

Array(obj)
");
            }, @"
to_a
to_ary
");
        }

        /// <summary>
        /// We need to invalidate the cache when respond_to? is added.
        /// </summary>
        public void ProtocolCaching2() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def to_f
    puts 'C:to_f'
    1.0
  end
end

obj = C.new
Float(obj)

class C
  def respond_to? name
    puts name
    false
  end
end

Float(obj) rescue puts 'error'
");
            }, @"
C:to_f
to_f
error
");

        }

        /// <summary>
        /// We need to invalidate the cache when to_xxx is removed.
        /// </summary>
        public void ProtocolCaching3() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
  def to_f
    puts 'C:to_f'
    1.0
  end
end

obj = C.new
Float(obj)

class C
  remove_method(:to_f)
end

Float(obj) rescue puts 'error'
");
            }, @"
C:to_f
error
");

        }

        /// <summary>
        /// Caching of to_s conversion.
        /// </summary>
        public void ProtocolCaching4() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C; end
c = C.new

puts ""(#{c})""

module Kernel
  remove_method :to_s
  
  def method_missing name
    'mm:to_s'
  end
end

puts ""(#{c})""

class C; def to_s; 'C:to_s'; end; end

puts ""(#{c})""

class Array; def to_s; 'Array:to_s'; end; end
class C; def to_s; [1,2]; end; end

puts ""(#{c})""
");
            }, @"
(#<C:0x*>)
(mm:to_s)
(C:to_s)
(#<C:0x*>)
", OutputFlags.Match);

        }
    }
}
