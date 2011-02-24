/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
using CsBinder = Microsoft.CSharp.RuntimeBinder.Binder;
using CSharpBinderFlags = Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags;
#else
using Microsoft.Scripting.Ast;
using dynamic = System.Object;
#endif

using System;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Collections;
using IronRuby.Runtime;
using System.Collections.Generic;

namespace IronRuby.Tests {
    #region Custom binders

    class MyInvokeMemberBinder : InvokeMemberBinder {
        public MyInvokeMemberBinder(string name, CallInfo callInfo)
            : base(name, false, callInfo) {
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
            return new DynamicMetaObject(
                Expression.Constant("FallbackInvokeMember"),
                target.Restrictions.Merge(BindingRestrictions.Combine(args))
            );
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
            return new DynamicMetaObject(
                Expression.Dynamic(new MyInvokeBinder(CallInfo), typeof(object), DynamicUtils.GetExpressions(ArrayUtils.Insert(target, args))),
                target.Restrictions.Merge(BindingRestrictions.Combine(args))
            );
        }

        internal static object Invoke(object obj, string methodName) {
            var site = CallSite<Func<CallSite, object, object>>.Create(new MyInvokeMemberBinder(methodName, new CallInfo(0)));
            return site.Target(site, obj);
        }

        internal static object Invoke(object obj, string methodName, object arg) {
            var site = CallSite<Func<CallSite, object, object, object>>.Create(new MyInvokeMemberBinder(methodName, new CallInfo(1)));
            return site.Target(site, obj, arg);
        }
    }

    class MyInvokeBinder : InvokeBinder {
        public MyInvokeBinder(CallInfo callInfo)
            : base(callInfo) {
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
            return new DynamicMetaObject(
                Expression.Call(
                    typeof(String).GetMethod("Concat", new Type[] { typeof(object), typeof(object) }),
                    Expression.Constant("FallbackInvoke"),
                    target.Expression
                ),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target)
            );
        }

        internal static object Invoke(object obj) {
            var site = CallSite<Func<CallSite, object, object>>.Create(new MyInvokeBinder(new CallInfo(0)));
            return site.Target(site, obj);
        }

        internal static object Invoke(object obj, object arg) {
            var site = CallSite<Func<CallSite, object, object, object>>.Create(new MyInvokeBinder(new CallInfo(1)));
            return site.Target(site, obj, arg);
        }
    }

    class MyGetIndexBinder : GetIndexBinder {
        public MyGetIndexBinder(CallInfo args)
            : base(args) {
        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion) {
            return new DynamicMetaObject(
                Expression.Call(
                    typeof(String).GetMethod("Concat", new Type[] { typeof(object), typeof(object) }),
                    Expression.Constant("FallbackGetIndex:"),
                    indexes[0].Expression
                ),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target)
            );
        }

        internal static object Invoke(object obj, object index) {
            var site = CallSite<Func<CallSite, object, object, object>>.Create(new MyGetIndexBinder(new CallInfo(1)));
            return site.Target(site, obj, index);
        }
    }

    class MySetIndexBinder : SetIndexBinder {
        public MySetIndexBinder(CallInfo args)
            : base(args) {
        }

        public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
            return new DynamicMetaObject(
                Expression.Call(
                    typeof(String).GetMethod("Concat", new Type[] { typeof(object), typeof(object), typeof(object) }),
                    Expression.Constant("FallbackSetIndex:"),
                    indexes[0].Expression,
                    value.Expression
                ),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target)
            );
        }

        internal static object Invoke(object obj, object index, object arg) {
            var site = CallSite<Func<CallSite, object, object, object, object>>.Create(new MySetIndexBinder(new CallInfo(1)));
            return site.Target(site, obj, index, arg);
        }
    }

    class MyGetMemberBinder : GetMemberBinder {
        public MyGetMemberBinder(string name)
            : base(name, false) {
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
            return new DynamicMetaObject(
                Expression.Constant("FallbackGetMember"),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target)
            );
        }

        internal static object Invoke(object obj, string memberName) {
            var site = CallSite<Func<CallSite, object, object>>.Create(new MyGetMemberBinder(memberName));
            return site.Target(site, obj);
        }
    }

    class MySetMemberBinder : SetMemberBinder {
        public MySetMemberBinder(string name)
            : base(name, false) {
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
            return new DynamicMetaObject(
                Expression.Constant("FallbackSetMember"),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target)
            );
        }

        internal static object Invoke(object obj, string memberName, object val) {
            var site = CallSite<Func<CallSite, object, object, object>>.Create(new MySetMemberBinder(memberName));
            return site.Target(site, obj, val);
        }
    }

    class MyInvokeBinder2 : InvokeBinder {
        public MyInvokeBinder2(CallInfo args)
            : base(args) {
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
            Expression[] exprs = new Expression[args.Length + 1];
            exprs[0] = Expression.Constant("FallbackInvoke");
            for (int i = 0; i < args.Length; i++) {
                exprs[i + 1] = args[i].Expression;
            }

            return new DynamicMetaObject(
                Expression.Call(
                    typeof(String).GetMethod("Concat", new Type[] { typeof(object[]) }),
                    Expression.NewArrayInit(
                        typeof(object),
                        exprs
                    )
                ),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target)
            );
        }

        internal static object Invoke(object obj, object arg) {
            var site = CallSite<Func<CallSite, object, object, object>>.Create(new MyInvokeBinder2(new CallInfo(1)));
            return site.Target(site, obj, arg);
        }
    }

    class MyConvertBinder : ConvertBinder {
        private object _result;
        public MyConvertBinder(Type type, object result)
            : base(type, true) {
            _result = result;
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
            return new DynamicMetaObject(
                Expression.Constant(_result, ReturnType),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target)
            );

        }

        internal static T Convert<T>(object obj, T fallbackResult) {
            var site = CallSite<Func<CallSite, object, T>>.Create(new MyConvertBinder(typeof(T), fallbackResult));
            return site.Target(site, obj);
        }
    }

    class MyBinaryOperationBinder : BinaryOperationBinder {
        public MyBinaryOperationBinder(ExpressionType operation)
            : base(operation) {
        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
            return new DynamicMetaObject(
                Expression.Call(
                    typeof(String).GetMethod("Concat", new Type[] { typeof(object), typeof(object) }),
                    Expression.Constant("FallbackInvoke:"),
                    arg.Expression
                ),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target)
            );
        }

        internal static object Invoke(ExpressionType operation, object obj, object arg) {
            var site = CallSite<Func<CallSite, object, object, object>>.Create(new MyBinaryOperationBinder(operation));
            return site.Target(site, obj, arg);
        }
    }

    class MyUnaryOperationBinder : UnaryOperationBinder {
        public MyUnaryOperationBinder(ExpressionType operation)
            : base(operation) {
        }

        public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
            return new DynamicMetaObject(
                Expression.Constant("FallbackInvoke"),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target)
            );
        }

        internal static object Invoke(ExpressionType operation, object obj) {
            var site = CallSite<Func<CallSite, object, object>>.Create(new MyUnaryOperationBinder(operation));
            return site.Target(site, obj);
        }
    }

    #endregion

    public partial class Tests {

        #region Ruby snippet

        const string RubySnippet = @"
ArrayList = System::Collections::ArrayList

#------------------------------------------------------------------------------
# Mixin which allows responding to arbitrary methods, properties, and indexing

module DynamicAttributes
    def initialize *args
        @attrs = Hash.new
        @elems = Hash.new
        super
    end
    
    def explicit_attribute
        'explicit_attribute'.to_clr_string
    end
       
    def method_missing *args
        case args.size
            when 1
                attr_name = args[0].to_s
                if @attrs.key? attr_name
                    @attrs[attr_name]
                else
                    ('dynamic_' + attr_name).to_clr_string
                end
            when 2
                if args[0] == :[] then
                    if @elems.key? args[1] then
                        @elems[args[1]]
                    else
                        ('dynamic_element_' + args[1].to_s).to_clr_string
                    end
                else
                    attr_name = args[0].to_s[0..-2] # Strip the trailing '='
                    @attrs[attr_name] = args[1]
                end
            when 3
                # args[0] will be :[]=
                @elems[args[1]] = args[2]
        end
    end
end

#------------------------------------------------------------------------------
# If the file is run from the command-line as 'ir.exe test.rb',
# some extra initialization needs to be done so that the rest of the code
# works both from the command-line and in a hosted ScriptRuntime.

if $0 == __FILE__ then
    # Mimic DynamicAttributes#initialize
    self.instance_variable_set(:@attrs, Hash.new)
    self.instance_variable_set(:@elems, Hash.new)
    class << self
        include DynamicAttributes
    end
end

#------------------------------------------------------------------------------
# Inherit from a CLR type

class RubyArrayList < ArrayList
    def initialize *args
        super(*args)
    end
    
    def ruby_method
        'Hi from Ruby'.to_clr_string
    end

    attr_accessor :ruby_attribute

    # override a CLR virtual method
    def IndexOf obj
        123456789
    end
end

self.ruby_array_list = RubyArrayList.new()
self.ruby_array_list.Add(100)
self.ruby_array_list.Add(200)

#------------------------------------------------------------------------------
class DynamicObject
    include DynamicAttributes
end

self.dynamic_object = DynamicObject.new

#------------------------------------------------------------------------------
class DynamicArrayList < RubyArrayList
    include DynamicAttributes
end

self.dynamic_array_list = DynamicArrayList.new

#------------------------------------------------------------------------------
class Miscellaneous
    def self.static_method
        'static_method'.to_clr_string
    end
    
    #self.class_instance_method = static_method
    
    attr :ruby_callable_called
    def get_a_ruby_callable
        @ruby_callable_called = false
        proc { @ruby_callable_called = true }
    end
    
    def to_s
        'to_s'
    end
end

self.misc = Miscellaneous.new

#------------------------------------------------------------------------------
class Indexable
    def initialize a=nil
        if a then
            @array = a
        else
            @array = []
        end
    end
    
    def [](index)
        @array[index]
    end

    def []=(index, value)
        @array[index] = value
    end
end

self.indexable = Indexable.new [0, 1, 2]

#------------------------------------------------------------------------------
class Methods
    def self.named_params(a, b)
        %Q(a:#{a} b:#{b}).to_clr_string
    end
    
    def self.default_values(a = 1, b = 2)
        %Q(a:#{a} b:#{b}).to_clr_string
    end

    def self.varargs(*args)
        args.collect {|elem| elem.to_s }.join(' ').to_clr_string
    end
    
    def self.multiple_return_values
        return 100, 200
    end
    
    def self.with_block
        yield 100
    end
end

#------------------------------------------------------------------------------

class SanityTest
    def self.assert_equal o1, o2
        if not o1 == o2 then raise %Q(Fail: Expected #{o1} to equal #{o2}) end
    end
    
    def self.assert_error l, exception_type
        begin
            l.call
            raise 'Unreachable'
        rescue Exception => e
            if not e.kind_of? exception_type then raise %Q(Expected #{e.class} to equal #{exception_type}) end
        end
    end

    def self.sanity_test main
        # $ruby_array_list
        assert_equal main.ruby_array_list.Count, 2
        main.ruby_array_list[0]
    
        assert_equal main.ruby_array_list.ruby_method, 'Hi from Ruby'.to_clr_string
        assert_equal main.ruby_array_list.IndexOf(nil), 123456789
        
        # main.dynamic_object
        assert_equal main.dynamic_object.foo, 'dynamic_foo'.to_clr_string
        main.dynamic_object.bar = 'my bar'
        assert_equal main.dynamic_object.bar, 'my bar'
        assert_equal main.dynamic_object.explicit_attribute, 'explicit_attribute'.to_clr_string
        assert_equal main.dynamic_object[:hello], 'dynamic_element_hello'.to_clr_string
        main.dynamic_object[:hello] = 1
        assert_equal main.dynamic_object[:hello], 1
        
        # main.dynamic_array_list
        assert_equal main.dynamic_array_list.foo, 'dynamic_foo'.to_clr_string
        main.dynamic_array_list.bar = 'my bar'
        assert_equal main.dynamic_array_list.bar, 'my bar'
        assert_equal main.dynamic_array_list.explicit_attribute, 'explicit_attribute'.to_clr_string
        main.dynamic_array_list.Count = 1
        assert_equal main.dynamic_array_list.Count, 0
        assert_equal main.dynamic_array_list.IndexOf(0), 123456789
        
        # main.misc
        assert_equal Miscellaneous.static_method, 'static_method'.to_clr_string
        assert_error lambda { main.misc.static_method }, NoMethodError
        c = main.misc.get_a_ruby_callable()
        assert_equal main.misc.ruby_callable_called, false
        c.call(nil)
        assert_equal main.misc.ruby_callable_called, true
        assert_equal main.misc.ToString(), 'to_s'
        
        # main.indexable
        assert_equal main.indexable[2], 2
        main.indexable[10] = 100
        assert_equal main.indexable[10], 100
        assert_equal main.indexable[9], nil
        
        # Methods
        assert_equal Methods.default_values(100), 'a:100 b:2'.to_clr_string
        assert_equal Methods.varargs(100, 200), '100 200'.to_clr_string
        assert_equal Methods.multiple_return_values, [100, 200]
        assert_equal Methods.with_block {|x| x + 1000}, 1100
        
        # Features to try from other languages
        # Pass in ref/out params
        # Named arguments
    end
end

if $0 == __FILE__ then
    SanityTest.sanity_test self
end
";
        #endregion

        private ScriptScope CreateInteropScope() {
            var scope = Runtime.CreateScope();
            Engine.Execute(RubySnippet, scope);
            return scope;
        }

        public void Dlr_RubySnippet() {
            var scope = CreateInteropScope();
            Engine.Execute("SanityTest.sanity_test self", scope);
        }

        public void Dlr_ClrSubtype() {
            var scope = CreateInteropScope();
            object ruby_array_list = scope.GetVariable("ruby_array_list");

            // CLR properties are accessible as methods
            AreEqual(MyInvokeMemberBinder.Invoke(ruby_array_list, "Count"), "FallbackInvokeMember");
            // CLR properties are accessible as members
            AreEqual(MyGetMemberBinder.Invoke(ruby_array_list, "Count"), "FallbackGetMember");
            // Overriden CLR member
            AreEqual(MyInvokeMemberBinder.Invoke(ruby_array_list, "IndexOf", null), 123456789);
            // CLR indexer
            AreEqual(MySetIndexBinder.Invoke(ruby_array_list, 10, 100), "FallbackSetIndex:10100");
            AreEqual(MyGetIndexBinder.Invoke(ruby_array_list, 10), "FallbackGetIndex:10");

            AreEqual(MyInvokeMemberBinder.Invoke(ruby_array_list, "ruby_method"), "Hi from Ruby");
            // CLR properties accessed with Ruby name. 
            AreEqual(MyInvokeMemberBinder.Invoke(ruby_array_list, "count"), "FallbackInvokeMember");
            // CLR methods accessed with Ruby name.
            AreEqual(MyInvokeMemberBinder.Invoke(ruby_array_list, "index_of", null), "FallbackInvokeMember");

            AreEqual(MyInvokeMemberBinder.Invoke(ruby_array_list, "non_existent"), "FallbackInvokeMember");
            AreEqual(MySetMemberBinder.Invoke(ruby_array_list, "Count", 100000), "FallbackSetMember");

            // Ruby attributes are invoked directly via SetMember/GetMember:
            AreEqual(MySetMemberBinder.Invoke(ruby_array_list, "ruby_attribute", 123), 123);
            AreEqual(MyGetMemberBinder.Invoke(ruby_array_list, "ruby_attribute"), 123);
#if !CLR2
            List<object> result = new List<object>();
            foreach (object item in (dynamic)ruby_array_list) {
                result.Add(item);
            }
            Assert(result.Count == 2 && (int)result[0] == 100 && (int)result[1] == 200);
#endif
        }

        public void Dlr_MethodMissing() {
            var scope = CreateInteropScope();
            object dynamic_object = scope.GetVariable("dynamic_object");

            AreEqual(MyInvokeMemberBinder.Invoke(dynamic_object, "non_existent_method"), "dynamic_non_existent_method");

            AreEqual(MySetMemberBinder.Invoke(dynamic_object, "non_existent_member", 100), 100);

            // Ruby doesn't have "mising_property" so we get a method, not the value:
            AreEqual(MyInvokeBinder.Invoke(MyGetMemberBinder.Invoke(dynamic_object, "non_existent_member")), 100);

            AreEqual(MyGetIndexBinder.Invoke(dynamic_object, "non_existent_index"), "dynamic_element_non_existent_index");
            AreEqual(MySetIndexBinder.Invoke(dynamic_object, "non_existent_index", 100), 100);
            AreEqual(MyGetIndexBinder.Invoke(dynamic_object, "non_existent_index"), 100);

            AreEqual(MyInvokeMemberBinder.Invoke(dynamic_object, "explicit_attribute"), "explicit_attribute");
        }

        public void Dlr_Miscellaneous() {
            var scope = CreateInteropScope();
            object misc_object = scope.GetVariable("misc");

            object misc_class = MyInvokeMemberBinder.Invoke(misc_object, "class");
            AreEqual(Engine.Runtime.Globals.GetVariable<object>("Miscellaneous"), misc_class);

            // singleton methods are only invokable on the class object, not the instance:
            AreEqual(MyInvokeMemberBinder.Invoke(misc_class, "static_method"), "static_method");
            AreEqual(MyInvokeMemberBinder.Invoke(misc_object, "static_method"), "FallbackInvokeMember");

            object callable = MyInvokeMemberBinder.Invoke(misc_object, "get_a_ruby_callable");
            AreEqual(MyInvokeMemberBinder.Invoke(misc_object, "ruby_callable_called"), false);
            MyInvokeBinder.Invoke(callable);
            AreEqual(MyInvokeMemberBinder.Invoke(misc_object, "ruby_callable_called"), true);

            // "ToString" is not handled in any special way by Ruby binder.
            // The call falls back to the caller's binder that should then call .NET ToString method.
            // ToString is overridden by all Ruby objects to call to_s.
            AreEqual(MyInvokeMemberBinder.Invoke(misc_class, "ToString"), "FallbackInvokeMember");
        }

        public void Dlr_Conversions1() {
            Engine.Execute(@"
class C
  def to_int
    1
  end
  
  def to_f
    2.0
  end
  
  def to_str
    'str'
  end
end
");
            object classC = Runtime.Globals.GetVariable("C");
            object c = Engine.Operations.CreateInstance(classC);

            AreEqual(MyConvertBinder.Convert<sbyte>(c, 10), (sbyte)1);
            AreEqual(MyConvertBinder.Convert<byte>(c, 10), (byte)1);
            AreEqual(MyConvertBinder.Convert<short>(c, 10), (short)1);
            AreEqual(MyConvertBinder.Convert<ushort>(c, 10), (ushort)1);
            AreEqual(MyConvertBinder.Convert<int>(c, 10), 1);
            AreEqual(MyConvertBinder.Convert<uint>(c, 10), (uint)1);
            AreEqual(MyConvertBinder.Convert<long>(c, 10), (long)1);
            AreEqual(MyConvertBinder.Convert<ulong>(c, 10), (ulong)1);
            AreEqual(MyConvertBinder.Convert<BigInteger>(c, 10), (BigInteger)1);
            AreEqual(MyConvertBinder.Convert<double>(c, -8.0), 2.0);
            AreEqual(MyConvertBinder.Convert<float>(c, -8.0f), 2.0f);
            AreEqual(MyConvertBinder.Convert<string>(c, "FallbackConvert"), "str");
        }

        public class DynamicList : IDynamicMetaObjectProvider {
            public DynamicMetaObject/*!*/ GetMetaObject(Expression/*!*/ parameter) {
                return new Meta(parameter, BindingRestrictions.Empty, this);
            }

            public class Meta : DynamicMetaObject {
                internal Meta(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, object/*!*/ value)
                    : base(expression, restrictions, value) {
                }

                public override DynamicMetaObject/*!*/ BindConvert(ConvertBinder/*!*/ binder) {
                    if (binder.Type != typeof(IList)) {
                        return binder.FallbackConvert(this);
                    }

                    return new DynamicMetaObject(
                        Expression.Constant(new object[] { 1, 2, 3}),
                        BindingRestrictions.GetTypeRestriction(Expression, Value.GetType())
                    );
                }
            }
        }

        public void Dlr_Splatting1() {
            var scope = Engine.CreateScope();
            scope.SetVariable("x", new DynamicList());

            var a = Engine.Execute<RubyArray>(@"a,b,c = *x; [a,b,c]", scope);
            Assert((int)a[0] == 1);
            Assert((int)a[1] == 2);
            Assert((int)a[2] == 3);

            a = Engine.Execute<RubyArray>(@"a,b,c = x; [a,b,c]", scope);
            Assert((int)a[0] == 1);
            Assert((int)a[1] == 2);
            Assert((int)a[2] == 3);

            var expando = new ExpandoObject();
            scope.SetVariable("x", expando);
            
            a = Engine.Execute<RubyArray>(@"a,b = *x; [a,b]", scope);
            Assert(a[0] == expando);
            Assert(a[1] == null);

            a = Engine.Execute<RubyArray>(@"a,b = x; [a,b]", scope);
            Assert(a[0] == expando);
            Assert(a[1] == null);
        }

        public void Dlr_Indexable() {
            var scope = CreateInteropScope();
            object indexable = scope.GetVariable("indexable");
            AreEqual(MyGetIndexBinder.Invoke(indexable, 2), 2);
            AreEqual(MySetIndexBinder.Invoke(indexable, 10, 100), 100);
            AreEqual(MyGetIndexBinder.Invoke(indexable, 10), 100);
            AreEqual(MyGetIndexBinder.Invoke(indexable, 9), null);
        }

        public void Dlr_Number() {
            object one_hundred = Engine.Execute(@"
class Number
    def initialize(v)
        @val = v
    end
    
    def +(other)
        @val + other
    end
    
    def -(other)
        @val - other
    end
    
    def *(other)
        @val * other
    end
    
    def /(other)
        @val / other
    end

    def -@
        -@val
    end  

    def ~
        ~@val
    end  
end

Number.new(100)
");

            AreEqual(MyBinaryOperationBinder.Invoke(ExpressionType.Add, one_hundred, 1), 100 + 1);
            AreEqual(MyBinaryOperationBinder.Invoke(ExpressionType.Subtract, one_hundred, 1), 100 - 1);
            AreEqual(MyBinaryOperationBinder.Invoke(ExpressionType.Multiply, one_hundred, 2), 2 * 100);
            AreEqual(MyBinaryOperationBinder.Invoke(ExpressionType.Divide, one_hundred, 2), 100/2);
            AreEqual(MyUnaryOperationBinder.Invoke(ExpressionType.Negate, one_hundred), -100);
            AreEqual(MyUnaryOperationBinder.Invoke(ExpressionType.OnesComplement, one_hundred), ~100);
            AreEqual(MyUnaryOperationBinder.Invoke(ExpressionType.Increment, one_hundred), 100 + 1);
            AreEqual(MyUnaryOperationBinder.Invoke(ExpressionType.Decrement, one_hundred), 100 - 1);

#if !CLR2
            dynamic number = one_hundred;
            number--;
            Assert(number == 99);
            number++;
            Assert(number == 100);
#endif
        }

        public void Dlr_Comparable() {
            object comparable = Engine.Execute(@"
class RubyComparable
    include Comparable
    def initialize val
        @val = val
    end
    
    def <=>(other)
        @val <=> other
    end
end

RubyComparable.new(100)
");

            AreEqual(MyBinaryOperationBinder.Invoke(ExpressionType.Equal, comparable, 100), true);
            AreEqual(MyBinaryOperationBinder.Invoke(ExpressionType.GreaterThan, comparable, 100), false);
            AreEqual(MyBinaryOperationBinder.Invoke(ExpressionType.LessThanOrEqual, comparable, 100), true);
        }

        public void Dlr_Equatable() {
            object equatable = Engine.Execute(@"
class RubyEquatable
    def initialize val
      @val = val
    end
    
    def ==(other)
      @val == other
    end
end

RubyEquatable.new(100)
");

            AreEqual(MyBinaryOperationBinder.Invoke(ExpressionType.Equal, equatable, 100), true);
            AreEqual(MyBinaryOperationBinder.Invoke(ExpressionType.Equal, equatable, 101), false);
            AreEqual(MyBinaryOperationBinder.Invoke(ExpressionType.NotEqual, equatable, 100), false);
            AreEqual(MyBinaryOperationBinder.Invoke(ExpressionType.NotEqual, equatable, 101), true);
#if CLR4
            dynamic dynamicEquatable = equatable;
            Assert((bool)(dynamicEquatable == 100));
            Assert(!(bool)(dynamicEquatable == 101));
            Assert(!(bool)(dynamicEquatable != 100));
            Assert((bool)(dynamicEquatable != 101));
#endif
        }

        public void Dlr_RubyObjects() {
            var scope = Engine.CreateScope();
            Engine.Execute(@"
scope.ruby_array = [100, 200]
scope.ruby_hash = { 1 => 3, 2 => 4 }

def inc(a)
  a + 1
end

scope.ruby_method = method(:inc)
scope.ruby_proc = proc { |a| a + 2 } 
", scope);

#if !CLR2
            dynamic s = scope;
            AreEqual(s.ruby_array[0], 100);
            AreEqual(s.ruby_hash[1], 3);
            AreEqual(s.ruby_method(1), 2);
            AreEqual(s.ruby_proc(1), 3);
#endif
            object method = scope.GetVariable("ruby_method");
            AreEqual(MyInvokeBinder.Invoke(method, 1), 2);

            object proc = scope.GetVariable("ruby_proc");
            AreEqual(MyInvokeBinder.Invoke(proc, 1), 3);
        }

        public void Dlr_Methods() {
            //assert_equal Methods.default_values(100), 'a:100 b:2'
            //assert_equal Methods.varargs(100, 200), '100 200'
            //assert_equal Methods.multiple_return_values, [100, 200]
            //assert_equal Methods.with_block {|x| x + 1000}, 1100
        }

        public void Dlr_Visibility() {
            Engine.Execute(@"
class D < Hash
end

class C
  def public_m
    0
  end

  private
  def private_m
    1
  end

  protected
  def protected_m
    2
  end
end
");
            var classC = Runtime.Globals.GetVariable("C");
            // TODO: CLR4 bug #772803 - c can't be dynamic:
            object c = Engine.Operations.CreateInstance(classC);

            AssertExceptionThrown<MissingMethodException>(() => MyInvokeMemberBinder.Invoke(c, "private_m"));
            AssertExceptionThrown<MissingMethodException>(() => MyInvokeMemberBinder.Invoke(c, "protected_m"));
            var r1 = MyInvokeMemberBinder.Invoke(c, "public_m");
            Assert(r1 is int && (int)r1 == 0);

            Engine.Execute(@"
class C
  def method_missing name
    3
  end
end");
            var r2 = MyInvokeMemberBinder.Invoke(c, "private_m");
            Assert(r2 is int && (int)r2 == 3);

            // private initialize method can be called if called via new:
            var classD = Runtime.Globals.GetVariable("D");
            var d = Engine.Operations.CreateInstance(classD);
            Assert(d is Hash);
        }

        public void Dlr_Languages() {
            //# Pass in ref/out params
            //# Named arguments
        }

        public class DynamicObject1 : DynamicObject {
            public string Test() {
                return "Hello, Test";
            }

            public override IEnumerable<string> GetDynamicMemberNames() {
                return new string[] { "Test2", "Test3" };
            }

            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
                result = "Invoke Member " + binder.Name;
                return true;
            }

            public override bool TryInvoke(InvokeBinder binder, object[] args, out object result) {
                result = "Invoke";
                return true;
            }
        }

        public void Dlr_DynamicObject1() {
            Context.ObjectClass.SetConstant("C", new DynamicObject1());
            TestOutput(@"
p C.Test2
p C.call
p C.methods(false)
", @"
'Invoke Member Test2'
'Invoke'
['test2', 'test3']
");
        }

        public class DynamicObject2 : DynamicObject {
            public override bool TryInvokeMember(InvokeMemberBinder binder, dynamic[] args, out dynamic result) {
                if (binder.Name == "Foo") {
                    result = 1;
                    return true;
                } else {
                    result = null;
                    return false;
                }
            }

            public override bool TryGetMember(GetMemberBinder binder, out dynamic result) {
                if (binder.Name == "Bar") {
                    result = 2;
                    return true;
                } else {
                    result = null;
                    return false;
                }
            }
        }

        public void Dlr_DynamicObject2() {
            Context.ObjectClass.SetConstant("C", new DynamicObject2());
            TestOutput(@"
p C.foo
p C.Foo
p C.bar
p C.Bar
C.xxx rescue puts $!.to_s[0, 22]
", @"
1
1
2
2
undefined method `xxx'
");
        }

        public class DynamicObject3 : DynamicObject {
            public List<string> Lookups = new List<string>();

            public override bool TryGetMember(GetMemberBinder binder, out object result) {
                Lookups.Add(binder.Name);
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Scope variable lookup uses InteropBinder.TryGetMemberExact which should use errorSuggestion correctly.
        /// </summary>
        public void Dlr_DynamicObject3() {
            var obj = new DynamicObject3();
            var s = Engine.CreateScope(obj);
            Assert(Engine.Execute<int>("foo_bar rescue 123", s) == 123);
            Assert(obj.Lookups.ToArray().ValueEquals(new[] { "foo_bar", "FooBar" }));
        }
    }
}

