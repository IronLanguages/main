/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;
using IronRuby.Builtins;
using System.IO;
using System.Linq.Expressions;
using System.Dynamic;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Hosting;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;

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
        public MyConvertBinder(Type type)
            : this(type, "FallbackConvert") {
        }
        public MyConvertBinder(Type type, object result)
            : base(type, true) {
            _result = result;
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
            return new DynamicMetaObject(
                Expression.Constant(_result),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target)
            );

        }

        internal static object Invoke(object obj, Type targetType) {
            var site = CallSite<Func<CallSite, object, object>>.Create(new MyConvertBinder(targetType));
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

    #endregion

    public partial class Tests {

        #region Ruby snippet

        const string RubySnippet = @"
require 'mscorlib'
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
    
    attr :callable_called
    def get_a_callable
        @callable_called = false
        System::Action[Object].new { @callable_called = true }
    end
    
    def to_s
        'to_s'.to_clr_string
    end
end

self.misc = Miscellaneous.new

#------------------------------------------------------------------------------
class Convertible
    def initialize v
        @val = v
    end
    
    def to_i
        @val.to_i
    end
    
    def to_f
        @val.to_f
    end
    
    def to_str
        @val.to_str.to_clr_string
    end
end

self.convertible = Convertible.new('0')

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
end

self.number = Number.new(100)

#------------------------------------------------------------------------------
class Helpers
    def self.get_ruby_array
        [100, 200]
    end
    
    def self.get_ruby_string
        'Ruby string'.to_clr_string
    end
    
    def self.get_ruby_hash
        { 'Ruby'.to_clr_string => 'keyed by Ruby'.to_clr_string, 'C#'.to_clr_string => 'keyed by C#'.to_clr_string }
    end
    
    def self._a_method
        'Ruby method'.to_clr_string
    end
    
    def self.get_ruby_callable
        self.method(:_a_method)
    end
    
    def self.get_singleton_string
        s = 'Hello'.to_clr_string
        class<<s
            def ToString
                'Singleton'.to_clr_string
            end
        end
        s
    end
end

#------------------------------------------------------------------------------
class RubyEnumerable
    include Enumerable
    def initialize a
        @array = a
    end
    
    def each
        @array.each {|elem| yield elem }
    end
end

self.ruby_enumerable = RubyEnumerable.new([1,2,3])

#------------------------------------------------------------------------------
class RubyComparable
    include Comparable
    def initialize val
        @val = val
    end
    
    def <=>(other)
        @val <=> other
    end
end

self.ruby_comparable = RubyComparable.new(100)

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
        assert_equal main.ruby_array_list[0], 100
        assert_error lambda { main.ruby_array_list.Count = 1 }, NoMethodError
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
        c = main.misc.get_a_callable()
        assert_equal main.misc.callable_called, false
        c.Invoke(nil)
        assert_equal main.misc.callable_called, true
        # assert_equal main.misc.ToString(), 'to_s' # Bug!!!!!!!!!!!!!!!!!
        
        # main.convertible
        assert_error lambda { System::GC.Collect(main.convertible) }, TypeError # convert to int - Bug!!!!!!!!!!
        assert_equal System::Exception.new(main.convertible).class, Exception
        # need to convert to float
        
        # main.indexable
        assert_equal main.indexable[2], 2
        main.indexable[10] = 100
        assert_equal main.indexable[10], 100
        assert_equal main.indexable[9], nil
        
        # main.number
        assert_equal((main.number + 1), 101)
        assert_equal((main.number - 1), 99)
        assert_equal((main.number * 2), 200)
        assert_equal((main.number / 2), 50)
        
        # RubyEnumerable
        assert_equal main.ruby_enumerable.min, 1
        
        # RubyComparable
        assert_equal((main.ruby_comparable == 100), true)
        assert_equal((main.ruby_comparable > 100), false)
        assert_equal((main.ruby_comparable >= 100), true)
        
        # Helpers
        assert_equal Helpers.get_ruby_array()[0], 100
        assert_equal System::Exception.new(Helpers.get_ruby_string).class, Exception
        assert_equal Helpers.get_ruby_hash()['Ruby'.to_clr_string], 'keyed by Ruby'.to_clr_string
        assert_equal Helpers.get_ruby_callable().call(), 'Ruby method'.to_clr_string
        # assert_equal Helpers.get_singleton_string.ToString, 'Singleton' # Bug!!! - this hangs the process
        
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

        private ScriptScope _rubyInteropScope;
        private ScriptScope RubyInteropScope {
            get {
                if (_rubyInteropScope == null) {
                    _rubyInteropScope = Runtime.CreateScope();
                    Engine.Execute(RubySnippet, _rubyInteropScope);
                }
                return _rubyInteropScope;
            }
        }

        public void Dlr_ClrSubtype() {
            object ruby_array_list = RubyInteropScope.GetVariable("ruby_array_list");

            // CLR properties are accessible as methods
            AreEqualBug(MyInvokeMemberBinder.Invoke(ruby_array_list, "Count"), "FallbackInvokeMember", 2);
            // CLR properties are not accessible as members : should this be allowed?
            AssertExceptionThrownBug<MissingMemberException>(delegate() { MyGetMemberBinder.Invoke(ruby_array_list, "Count"); }, 2);
            // Overriden CLR member
            AreEqualBug(MyInvokeMemberBinder.Invoke(ruby_array_list, "IndexOf", null), "FallbackInvokeMember", 123456789);
            // CLR indexer
            AreEqual(MySetIndexBinder.Invoke(ruby_array_list, 10, 100), "FallbackSetIndex:10100");
            AreEqual(MyGetIndexBinder.Invoke(ruby_array_list, 10), "FallbackGetIndex:10");

            AreEqual(MyInvokeMemberBinder.Invoke(ruby_array_list, "ruby_method"), "Hi from Ruby");
            // CLR properties accessed with Ruby-friendly name. This should not be allowed, right?
            AssertExceptionThrownBug<MissingMemberException>(delegate() { MyInvokeMemberBinder.Invoke(ruby_array_list, "count"); }, 2);
            // CLR methods accessed with Ruby-friendly name. This should not be allowed, right?
            AssertExceptionThrownBug<MissingMemberException>(delegate() { MyInvokeMemberBinder.Invoke(ruby_array_list, "index_of", null); }, -1);

            AreEqual(MyInvokeMemberBinder.Invoke(ruby_array_list, "non_existent"), "FallbackInvokeMember");
            AreEqual(MySetMemberBinder.Invoke(ruby_array_list, "Count", 100000), "FallbackSetMember");
        }

        public void Dlr_MethodMissing() {
            object dynamic_object = RubyInteropScope.GetVariable("dynamic_object");

            AreEqual(MyInvokeMemberBinder.Invoke(dynamic_object, "non_existent_method"), "dynamic_non_existent_method");

            AreEqualBug(MySetMemberBinder.Invoke(dynamic_object, "non_existent_member", 100), 100, "FallbackSetMember");
            AreEqualBug(MyGetMemberBinder.Invoke(dynamic_object, "non_existent_member"), 100, "FallbackGetMember");

            AreEqualBug(MyGetIndexBinder.Invoke(dynamic_object, "non_existent_index"), "dynamic_element_non_existent_index", "FallbackGetIndex:non_existent_index");
            AreEqual(MySetIndexBinder.Invoke(dynamic_object, "non_existent_index", 100), "FallbackSetIndex:non_existent_index100");
            AreEqualBug(MyGetIndexBinder.Invoke(dynamic_object, "non_existent_index"), 100, "FallbackGetIndex:non_existent_index");

            AreEqual(MyInvokeMemberBinder.Invoke(dynamic_object, "explicit_attribute"), "explicit_attribute");
        }

        public void Dlr_Miscellaneous() {
            object misc_object = RubyInteropScope.GetVariable("misc");

            object misc_class = MyInvokeMemberBinder.Invoke(misc_object, "class");
            AreEqualBug<MissingMemberException>(delegate() { RubyInteropScope.GetVariable("Miscellaneous"); }, misc_class);

            AreEqual(MyInvokeMemberBinder.Invoke(misc_class, "static_method"), "static_method");
            AssertExceptionThrownBug<MissingMethodException>(delegate() { MyInvokeMemberBinder.Invoke(misc_object, "static_method"); }, "static method");

            object callable = MyInvokeMemberBinder.Invoke(misc_object, "get_a_callable");
            AreEqual(MyInvokeMemberBinder.Invoke(misc_object, "callable_called"), false);
            MyInvokeBinder.Invoke(callable);
            AreEqualBug(MyInvokeMemberBinder.Invoke(misc_object, "callable_called"), true, false);

            AreEqualBug(MyInvokeMemberBinder.Invoke(misc_class, "ToString"), "to_s", "FallbackInvokeMember");
        }

        public void Dlr_Convertible() {
            object convertible = RubyInteropScope.GetVariable("convertible");
            AreEqualBug(MyConvertBinder.Invoke(convertible, typeof(int)), 0, "FallbackConvert");
            AreEqualBug(MyConvertBinder.Invoke(convertible, typeof(string)), "0", "FallbackConvert");
            AreEqualBug(MyConvertBinder.Invoke(convertible, typeof(float)), 0.0, "FallbackConvert");
        }

        public void Dlr_Indexable() {
            object indexable = RubyInteropScope.GetVariable("indexable");
            AreEqualBug(MyGetIndexBinder.Invoke(indexable, 2), 2, "FallbackGetIndex:2");
            AreEqualBug(MySetIndexBinder.Invoke(indexable, 10, 100), 100, "FallbackSetIndex:10100");
            AreEqualBug(MyGetIndexBinder.Invoke(indexable, 10), 100, "FallbackGetIndex:10");
            AreEqualBug(MyGetIndexBinder.Invoke(indexable, 9), null, "FallbackGetIndex:9");
        }

        public void Dlr_Number() {
            object number = RubyInteropScope.GetVariable("number");
            AreEqualBug(MyBinaryOperationBinder.Invoke(ExpressionType.Add, number, 1), 101, "FallbackInvoke:1");
            AreEqualBug(MyBinaryOperationBinder.Invoke(ExpressionType.Subtract, number, 1), 99, "FallbackInvoke:1");
            AreEqualBug(MyBinaryOperationBinder.Invoke(ExpressionType.Multiply, number, 2), 200, "FallbackInvoke:2");
            AreEqualBug(MyBinaryOperationBinder.Invoke(ExpressionType.Divide, number, 2), 50, "FallbackInvoke:2");
        }
        public void Dlr_Enumerable() {
            object ruby_enumerable = RubyInteropScope.GetVariable("ruby_enumerable");
            IEnumerable e = MyConvertBinder.Invoke(ruby_enumerable, typeof(IEnumerable)) as IEnumerable;
            AreEqual(e != null, true);
            IEnumerable<object> e2 = MyConvertBinder.Invoke(ruby_enumerable, typeof(IEnumerable<object>)) as IEnumerable<object>;
            AreEqualBug(e2 != null, true, false);
        }

        public void Dlr_Comparable() {
            object ruby_comparable = RubyInteropScope.GetVariable("ruby_comparable");
            AreEqualBug(MyBinaryOperationBinder.Invoke(ExpressionType.Equal, ruby_comparable, 100), true, "FallbackInvoke:100");
            AreEqualBug(MyBinaryOperationBinder.Invoke(ExpressionType.GreaterThan, ruby_comparable, 100), false, "FallbackInvoke:100");
            AreEqualBug(MyBinaryOperationBinder.Invoke(ExpressionType.LessThanOrEqual, ruby_comparable, 100), true, "FallbackInvoke:100");
        }

        public void Dlr_RubyObjects() {
            //assert_equal Helpers.get_ruby_array()[0], 100
            //assert_equal System::Exception.new(Helpers.get_ruby_string).class, Exception
            //assert_equal Helpers.get_ruby_hash()[:ruby], 'Ruby'
            //assert_equal Helpers.get_ruby_callable().call(), 'Ruby method'
            //# assert_equal Helpers.get_singleton_string.ToString, 'Singleton' # Bug!!! - this hangs the process
        }

        public void Dlr_Methods() {
            //assert_equal Methods.default_values(100), 'a:100 b:2'
            //assert_equal Methods.varargs(100, 200), '100 200'
            //assert_equal Methods.multiple_return_values, [100, 200]
            //assert_equal Methods.with_block {|x| x + 1000}, 1100
        }

        public void Dlr_Languages() {
            //# Pass in ref/out params
            //# Named arguments
        }
    }
}

