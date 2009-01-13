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

using System;
using System.Collections.Generic;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.Tests {

    public class DefaultProtocolTester : LibraryInitializer {
        protected override void LoadModules() {
            DefineGlobalModule("Tests", typeof(DefaultProtocolTester), true, null, (module) => {
                    module.DefineLibraryMethod("to_int_to_str", (int)RubyMethodAttributes.PublicSingleton, new System.Delegate[] {
                        new Func<RubyModule, Union<int, MutableString>, RubyArray>(ToIntToStr),
                    });
                    module.DefineLibraryMethod("to_str_to_int", (int)RubyMethodAttributes.PublicSingleton, new System.Delegate[] {
                        new Func<RubyModule, Union<MutableString, int>, RubyArray>(ToStrToInt),
                    });
                },
                RubyModule.EmptyArray);
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
            Context.DefineGlobalVariable("protocol_tester", MutableString.Create(typeof(DefaultProtocolTester).AssemblyQualifiedName));

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
    }
}
