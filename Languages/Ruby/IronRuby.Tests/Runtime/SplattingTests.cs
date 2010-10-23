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
using System.Reflection;
using System.Reflection.Emit;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using Microsoft.Scripting.Actions.Calls;

namespace IronRuby.Tests {

    public partial class Tests {
        internal static OverloadInfo/*!*/ CreateParamsArrayMethod(string/*!*/ name, Type/*!*/[]/*!*/ paramTypes, int paramsArrayIndex, int returnValue) {
            var tb = Snippets.Shared.DefineType("<T>", typeof(object), false, false).TypeBuilder;
            var mb = tb.DefineMethod(name, CompilerHelpers.PublicStatic, typeof(KeyValuePair<int, Array>), paramTypes);
            var pb = mb.DefineParameter(1 + paramsArrayIndex, ParameterAttributes.None, "ps");
            pb.SetCustomAttribute(new CustomAttributeBuilder(typeof(ParamArrayAttribute).GetConstructor(Type.EmptyTypes), ArrayUtils.EmptyObjects));

            var il = mb.GetILGenerator();
            il.Emit(OpCodes.Ldc_I4, returnValue);
            il.Emit(OpCodes.Ldarg, paramsArrayIndex);
            il.Emit(OpCodes.Newobj, typeof(KeyValuePair<int, Array>).GetConstructor(new[] { typeof(int), typeof(Array) }));
            il.Emit(OpCodes.Ret);
            return new ReflectionOverloadInfo(tb.CreateType().GetMethod(name, BindingFlags.Public | BindingFlags.Static));
        }

        public void Scenario_RubyArgSplatting1() {
            AssertOutput(delegate() {
                CompilerTest(@"
def foo(a,b,c)
  print a,b,c
end

foo(*[1,2,3])
");
            }, @"123");
        }

        public void Scenario_RubyArgSplatting2() {
            AssertOutput(delegate() {
                CompilerTest(@"
class C
    def []=(a,b,c)
      print a,b,c
    end
end

x = [1,2]
C.new[*x] = 3
C.new[1, *[2]] = 3
");
            }, @"123123");
        }

        public void Scenario_RubyArgSplatting3() {
            TestOutput(@"
def foo(a,b,c)
  p [a,b,c]
end

foo(1,2,*3)
", @"
[1, 2, 3]
");
        }

        /// <summary>
        /// Splat anything that's IList (including arrays and values passed via out parameters).
        /// </summary>
        public void Scenario_RubyArgSplatting4() {
            AssertOutput(delegate() {
                CompilerTest(@"
a,b,c = System::Array[Fixnum].new([1,2,3])
p [a,b,c]

def y1; yield System::Array[Fixnum].new([4]); end
def y2; yield System::Array[Fixnum].new([4,5]); end
def y3; yield System::Array[Fixnum].new([4,5,6]); end
def y10; yield System::Array[Fixnum].new([1,2,3,4,5,6,7,8,9,10]); end

y1 { |x| p [x] }
y2 { |x,y| p [x,y] }
y3 { |x,y,z| p [x,y,z] }
y10 { |a1,a2,a3,a4,a5,a6,a7,a8,a9,a10| p [a1,a2,a3,a4,a5,a6,a7,a8,a9,a10] }

dict = System::Collections::Generic::Dictionary[Fixnum, Fixnum].new
dict.add(1,1)
has_value, value = dict.try_get_value(1)
p [has_value, value]
");
            }, @"
[1, 2, 3]
[[4]]
[4, 5]
[4, 5, 6]
[1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
[true, 1]
");
        }

        public void Scenario_RubyArgSplatting5() {
            var c = Context.GetClass(typeof(MethodsWithParamArrays));
            Runtime.Globals.SetVariable("C", new MethodsWithParamArrays());

            // The post-param-array arguments might decide which overload to call:
            c.SetMethodNoEvent(Context, "bar", new RubyMethodGroupInfo(new[] { 
                CreateParamsArrayMethod("B0", new[] { typeof(int), typeof(int), typeof(int[]), typeof(bool) }, 2, 0),
                CreateParamsArrayMethod("B1", new[] { typeof(int), typeof(int[]), typeof(int), typeof(int) }, 1, 1),
            }, c, true));

            AssertOutput(delegate() {
                CompilerTest(@"
x = C.bar(*[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,true])
p x.key, x.value
x = C.bar(*[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15])
p x.key, x.value
");
            }, @"
0
[2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]
1
[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]
");

            // Huge splattees.
            AssertOutput(delegate() {
                CompilerTest(@"
[
[1],
[1] * 2,
[1] * 3,
[1] * 4,
[1] * 5,
[1] * 10003 + [true],
[1] * 10003,
].each do |s| 
  begin
    x = C.bar(*s)
    puts ""B#{x.key} -> #{x.value.length}""
  rescue 
    p $!
  end
end
");
            }, @"
#<ArgumentError: wrong number of arguments (1 for 3)>
#<ArgumentError: wrong number of arguments (2 for 3)>
B1 -> 0
B1 -> 1
B1 -> 2
B0 -> 10001
B1 -> 10000
");

            // Overloads might differ only in the element types of params-array.
            // If binder decision is not based upon all splatted item types
            c.SetMethodNoEvent(Context, "baz", new RubyMethodGroupInfo(new[] { 
                CreateParamsArrayMethod("Z0", new[] { typeof(int), typeof(object[]) }, 1, 0),
                CreateParamsArrayMethod("Z1", new[] { typeof(int), typeof(MutableString[]) }, 1, 1),
                CreateParamsArrayMethod("Z2", new[] { typeof(int), typeof(int[]) }, 1, 2),
            }, c, true));

            AssertOutput(delegate() {
                CompilerTest(@"
[
[1] * 20 + ['x'] + [1] * 20,
[1] * 10001,
[1] * 10000 + [true],
[1] + ['x'] * 10000,
].each do |s| 
  x = C.baz(*s)
  puts ""Z#{x.key} -> #{x.value.length}""
end
");
            }, @"
Z0 -> 40
Z2 -> 10000
Z0 -> 10000
Z1 -> 10000
");

            // Tests error handling and caching.
            c.SetMethodNoEvent(Context, "error", new RubyMethodGroupInfo(new[] { 
                CreateParamsArrayMethod("E1", new[] { typeof(int), typeof(MutableString[]) }, 1, 1),
                CreateParamsArrayMethod("E2", new[] { typeof(int), typeof(int[]) }, 1, 2),
            }, c, true));

            AssertOutput(delegate() {
                CompilerTest(@"
[
[1] + [2] * 10000,
[1] * 20 + ['zzz'] + [1] * 20,
[1] + ['x'] * 10000,
].each do |s| 
  begin  
    x = C.error(*s)
    puts ""Z#{x.key} -> #{x.value.length}""
  rescue 
    p $!
  end
end
");
            }, @"
Z2 -> 10000
#<TypeError: can't convert String into Fixnum>
Z1 -> 10000
");

            // TODO: test GetPreferredParameters with collapsed arguments
        }

        public void Scenario_RubyArgSplatting6() {
            TestOutput(@"
class Array
  def to_a
    [:array]
  end
end

class NilClass
  def to_a
    [:nil]
  end
end

x = *[1, 2]
p x
x = *nil
p x
", @"
[1, 2]
[:nil]
");
        }

        public void Scenario_CaseSplatting1() {
            TestOutput(@"
[0,2,5,8,6,7,9,4].each do |x|
  case x
    when 0,1,*[2],*[3,4]; print 0
    when *[5]; print 1
    when *[6,7]; print 2
    when *8; print 3
    when *System::Array[Fixnum].new([9]); print 4
  end
end
", @"
00132240
");

            TestOutput(@"
def t(i)
  puts ""t#{i}""
  true
end

def f(i)
  puts ""f#{i}""
  false
end

case
  when *[f(1),f(2),f(3)], *[f(4), f(5)]; puts 'a'
  when *[t(6)]; puts 'b'
end
", @"
f1
f2
f3
f4
f5
t6
b
");
        }

        public void SplattingProtocol1() {
            TestOutput(@"
class C
  def respond_to? name
    p name
    false
  end
  
  def to_s
    'c'
  end
end

p [1,*C.new]
p(*C.new)

x,y = C.new
p x,y

proc {|a,b| p [a,b] }.call(C.new)

case
  when *C.new;
end

def foo
  yield 1,2,*C.new
end

foo do |a,b,c|
  p a,b,c
end
", @"
:to_a
[1, c]
:to_a
c
:to_ary
c
nil
:to_ary
[c, nil]
:to_a
:to_a
1
2
c
");
        }
        
    }
}
