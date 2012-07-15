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
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Tests {
    public partial class Tests {
        class TMC_A {
            public static void Foo(int a) { }
            public static void Foo() { }
            public void Bar(int b) { }
            public void Bar() { }
            public virtual void Bar(short d) { }
            protected void Bar(char c) { }
            private void Bar(double c) { }
        }

        class TMC_B : TMC_A {
            public static void Foo(string a) { }
            public new static void Foo() { }
            public virtual void Bar(string b) { }
            public override void Bar(short d) { }
            private new void Bar() { }
            internal static void Bar(bool b) {}
        }

        public void TypeMemberCache() {
            var cache = new TypeMemberCache<MethodInfo>(RuntimeReflectionExtensions.GetRuntimeMethods);

            var b_foos = cache.GetMembers(typeof(TMC_B), "Foo", inherited: true);
            var a_foos = cache.GetMembers(typeof(TMC_A), "Foo", inherited: true);

            var a_bars = cache.GetMembers(typeof(TMC_A), "Bar", inherited: true);
            var b_bars = cache.GetMembers(typeof(TMC_B), "Bar", inherited: true);

            var b_foos_declared = cache.GetMembers(typeof(TMC_B), "Foo", inherited: false);
            var b_bars_declared = cache.GetMembers(typeof(TMC_B), "Bar", inherited: false);

            AreBagsEqual(b_foos.Select(a => a.ToString()), new[] { "Void Foo(System.String)", "Void Foo()" });
            AreBagsEqual(a_foos.Select(a => a.ToString()), new[] { "Void Foo(Int32)", "Void Foo()" });
            AreBagsEqual(a_bars.Select(a => a.ToString()), new[] { "Void Bar(Int32)", "Void Bar()", "Void Bar(Char)", "Void Bar(Double)", "Void Bar(Int16)" });
            AreBagsEqual(b_bars.Select(a => a.ToString()), new[] { "Void Bar(System.String)", "Void Bar()", "Void Bar(Int32)", "Void Bar()", "Void Bar(Char)", "Void Bar(Boolean)", "Void Bar(Int16)" });
            AreBagsEqual(b_foos_declared.Select(a => a.ToString()), new[] { "Void Foo(System.String)", "Void Foo()" });
            AreBagsEqual(b_bars_declared.Select(a => a.ToString()), new[] { "Void Bar(System.String)", "Void Bar()", "Void Bar(Boolean)", "Void Bar(Int16)" });

            var m = b_bars.WithBindingFlags(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
            AreBagsEqual(m.Select(a => a.ToString()), new[] { "Void Bar()", "Void Bar(Char)", "Void Bar(Boolean)" });

            m = b_bars.WithBindingFlags(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            AreBagsEqual(m.Select(a => a.ToString()), new[] { "Void Bar(System.String)", "Void Bar(Int32)", "Void Bar()", "Void Bar(Int16)" });

            m = b_bars.WithBindingFlags(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            AreBagsEqual(m.Select(a => a.ToString()), new[] { "Void Bar(System.String)", "Void Bar()", "Void Bar(Int32)", "Void Bar()", "Void Bar(Char)", "Void Bar(Int16)" });

            m = b_bars.WithBindingFlags(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            AreBagsEqual(m.Select(a => a.ToString()), new string[] { "Void Bar(Boolean)" });
        }

        public abstract class GetMembers_A {
            // methods

            public static void A_PublicStaticMethod() { }
            protected static void A_ProtectedStaticMethod() { }
            internal static void A_InternalStaticMethod() { }
            private static void A_PrivateStaticMethod() { }
            public void A_PublicInstanceMethod() { }
            protected void A_ProtectedInstanceMethod() { }
            internal void A_InternalInstanceMethod() { }
            private void A_PrivateInstanceMethod() { }

            public abstract void PublicAbstractMethod();

            public virtual void PublicVirtualMethod() { }
            protected virtual void ProtectedVirtualMethod() { }
            internal virtual void InternalVirtualMethod() { }
            public virtual void PublicVirtualMethod2() { }

            // fields

            public static int A_PublicStaticField;
            private static int A_PrivateStaticField;
            protected static int A_ProtectedStaticField;
            internal static int A_InternalStaticField;

            public int A_PublicField;
            private int A_PrivateField;
            protected int A_ProtectedField;
            internal int A_InternalField;

            public int PublicField;

            // properties
         
            public static int A_PublicPublicStaticProperty { get { return 0; } set { } }
            public static int A_PublicPrivateStaticProperty { get { return 0; } private set { } }
            public static int A_PrivatePublicStaticProperty { private get { return 0; } set { } }
            private static int A_PrivatePrivateStaticProperty { get { return 0; } set { } }

            public int A_PublicPublicProperty { get { return 0; } set { } }
            public int A_PublicPrivateProperty { get { return 0; } private set { } }
            public int A_PrivatePublicProperty { private get { return 0; } set { } }
            private int A_PrivatePrivateProperty { get { return 0; } set { } }

            public int A_PublicReadOnlyProperty { get { return 0; } }
            private int A_PrivateReadOnlyProperty { get { return 0; } }
            public int A_PublicWriteOnlyProperty { set { } }
            private int A_PrivateWriteOnlyProperty { set { } }

            public virtual int PublicPublicVirtualProperty { get { return 0; } set { } }
            public virtual int PublicPrivateVirtualProperty { get { return 0; } private set { } }
            public virtual int PrivatePublicVirtualProperty { private get { return 0; } set { } }

            public int PublicPublicProperty { get { return 0; } set { } }
            public int PublicPublicProperty2 { get { return 0; } set { } }

            // events

            public static event Action A_PublicStaticEvent { add {  } remove { } }
            private static event Action A_PrivateStaticEvent { add {  } remove { } }

            public event Action A_PublicEvent { add {  } remove { } }
            private event Action A_PrivateEvent { add {  } remove { } }

            public virtual event Action PublicVirtualEvent { add {  } remove { } }

            public event Action PublicEvent { add {  } remove { } }
            public event Action<int> PublicEvent2 { add { } remove { } }

            // types

            public class A_PublicType { }
            protected class A_ProtectedType { }
            internal class A_InternalType { }
            private class A_PrivateType { }

            public class PublicType { } 
        }

        public abstract class GetMembers_B : GetMembers_A {
            static GetMembers_B() {
            }

            public GetMembers_B() {
            }

            private GetMembers_B(int a) {
            }

            // methods
            public void B_PublicInstanceMethod() { }
            protected void B_ProtectedInstanceMethod() { }
            internal void B_InternalInstanceMethod() { }
            private void B_PrivateInstanceMethod() { }

            public static void B_PublicStaticMethod() { }
            protected static void B_ProtectedStaticMethod() { }
            internal static void B_InternalStaticMethod() { }
            private static void B_PrivateStaticMethod() { }

            public override void PublicVirtualMethod() { }
            protected override void ProtectedVirtualMethod() { }
            internal override void InternalVirtualMethod() { }

            public new virtual void PublicVirtualMethod2() { }

            // fields

            public static int B_PublicStaticField;
            private static int B_PrivateStaticField;
            protected static int B_ProtectedStaticField;
            internal static int B_InternalStaticField;

            public int B_PublicField;
            private int B_PrivateField;
            protected int B_ProtectedField;
            internal int B_InternalField;
            
            public new int PublicField;

            // properties

            public static int B_PublicPublicStaticProperty { get { return 0; } set { } }
            public static int B_PublicPrivateStaticProperty { get { return 0; } private set { } }
            public static int B_PrivatePublicStaticProperty { private get { return 0; } set { } }
            private static int B_PrivatePrivateStaticProperty { get { return 0; } set { } }

            public int B_PublicPublicProperty { get { return 0; } set { } }
            public int B_PublicPrivateProperty { get { return 0; } private set { } }
            public int B_PrivatePublicProperty { private get { return 0; } set { } }
            private int B_PrivatePrivateProperty { get { return 0; } set { } }

            public int B_PublicReadOnlyProperty { get { return 0; } }
            private int B_PrivateReadOnlyProperty { get { return 0; } }
            public int B_PublicWriteOnlyProperty { set { } }
            private int B_PrivateWriteOnlyProperty { set { } }
        
            public override int PublicPublicVirtualProperty { get { return 0; } set { } }
            public override int PublicPrivateVirtualProperty { get { return 0; } }
            public override int PrivatePublicVirtualProperty { set { } }

            public new int PublicPublicProperty { get { return 0; } set { } }
            public new string PublicPublicProperty2 { get { return ""; } set { } }
   
            // events

            public static event Action B_PublicStaticEvent { add { } remove { } }
            private static event Action B_PrivateStaticEvent { add { } remove { } }

            public event Action B_PublicEvent { add { } remove { } }
            private event Action B_PrivateEvent { add { } remove { } }

            public override event Action PublicVirtualEvent { add { } remove { } }

            public new event Action PublicEvent { add { } remove { } }
            public new event Action<int> PublicEvent2 { add { } remove { } }

            // types

            public class B_PublicType { }
            protected class B_ProtectedType { }
            internal class B_IntenralType { }
            private class B_PrivateType { }

            public new class PublicType { } 
        }

        public abstract class GetMembers<T> where T : GetMembers_A {
        }

#if !WIN8
        public void ReflectionUtils_GetMembers() {
            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            
            var m_expected = typeof(GetMembers_B).GetMethods(all);
            var m_expectedFlat = typeof(GetMembers_B).GetMethods(BindingFlags.FlattenHierarchy | all);
            var m_actual = typeof(GetMembers_B).GetInheritedMethods(name: null, flattenHierarchy: false).ToArray();
            var m_actualFlat = typeof(GetMembers_B).GetInheritedMethods(name: null, flattenHierarchy: true).ToArray();

            // the results differ in ReflectedType - methods returned by our impl. have all DeclaringType == ReflectedType.
            AreBagsEqual(m_actual.Select(m => m.ToString()), m_expected.Select(m => m.ToString()));
            AreBagsEqual(m_actualFlat.Select(m => m.ToString()), m_expectedFlat.Select(m => m.ToString()));

            var f_expected = typeof(GetMembers_B).GetFields(all);
            var f_expectedFlat = typeof(GetMembers_B).GetFields(BindingFlags.FlattenHierarchy | all);
            var f_actual = typeof(GetMembers_B).GetInheritedFields(name: null, flattenHierarchy: false).ToArray();
            var f_actualFlat = typeof(GetMembers_B).GetInheritedFields(name: null, flattenHierarchy: true).ToArray();

            AreBagsEqual(f_actual.Select(m => m.ToString()), f_expected.Select(m => m.ToString()));
            AreBagsEqual(f_actualFlat.Select(m => m.ToString()), f_expectedFlat.Select(m => m.ToString()));

            var p_expected = typeof(GetMembers_B).GetProperties(all);
            var p_expectedFlat = typeof(GetMembers_B).GetProperties(BindingFlags.FlattenHierarchy | all);
            var p_actual = typeof(GetMembers_B).GetInheritedProperties(name: null, flattenHierarchy: false).ToArray();
            var p_actualFlat = typeof(GetMembers_B).GetInheritedProperties(name: null, flattenHierarchy: true).ToArray();

            // PublicPublicProperty isn't returned twice in the CLR implementation but we return it twice
            // to be consistent with fields and methods.
            // Note that PublicPublicProperty2 is returned twice in the CLR implementation because the two properties 
            // have different return type.
            AreSetsEqual(p_actual.Select(m => m.ToString()), p_expected.Select(m => m.ToString()));
            AreSetsEqual(p_actualFlat.Select(m => m.ToString()), p_expectedFlat.Select(m => m.ToString()));
            Assert(p_actual.Count() == p_expected.Count() + 1);
            Assert(p_actualFlat.Count() == p_expectedFlat.Count() + 1);

            var e_expected = typeof(GetMembers_B).GetEvents(all);
            var e_expectedFlat = typeof(GetMembers_B).GetEvents(BindingFlags.FlattenHierarchy | all);
            var e_actual = typeof(GetMembers_B).GetInheritedEvents(name: null, flattenHierarchy: false).ToArray();
            var e_actualFlat = typeof(GetMembers_B).GetInheritedEvents(name: null, flattenHierarchy: true).ToArray();

            // Both PublicEvent and PublicEvent2 are not returned twice by CLR - duplicate names are filtered out 
            // but unlike properties different types don't matter.
            AreSetsEqual(e_actual.Select(m => m.ToString()), e_expected.Select(m => m.ToString()));
            AreSetsEqual(e_actualFlat.Select(m => m.ToString()), e_expectedFlat.Select(m => m.ToString()));
            Assert(e_actual.Count() == e_expected.Count() + 2);
            Assert(e_actualFlat.Count() == e_expectedFlat.Count() + 2);

            // Types are not inherited.
            var t_expected = typeof(GetMembers_B).GetNestedTypes(all);
            var t_expectedFlat = typeof(GetMembers_B).GetNestedTypes(BindingFlags.FlattenHierarchy | all);
            var t_actual = typeof(GetMembers_B).GetDeclaredNestedTypes().ToArray();
            
            AreBagsEqual(t_actual.Select(m => m.ToString()), t_expected.Select(m => m.ToString()));
            AreBagsEqual(t_actual.Select(m => m.ToString()), t_expectedFlat.Select(m => m.ToString()));

            var gp = typeof(GetMembers<GetMembers_B>).GetGenericArguments()[0];
            var gp_expected = gp.GetMembers(all);
            var gp_actual = gp.GetInheritedMembers();
            AreSetsEqual(gp_actual.Select(m => m.ToString()), gp_expected.Select(m => m.ToString()));
        }
#endif
        public delegate int FRefIntIntOutInt(ref int a, int b, out int c);

        public delegate int FIntIntInt(int a, int b);

        public void DelegateInfo1() {
            object lambda = Engine.Execute("lambda { |a,b| a + b }");

            var creator = new DynamicDelegateCreator(Context);
            var d1a = (Func<int, int, int>)creator.GetDelegate(lambda, typeof(Func<int, int, int>));
            var d1b = (Func<int, int, int>)creator.GetDelegate(lambda, typeof(Func<int, int, int>));
            var d2 = (FIntIntInt)creator.GetDelegate(lambda, typeof(FIntIntInt));
            Assert(d1a == d1b);

            int r1 = d1a(1, 2);
            int r2 = d2(10, 20);

            Assert(r1 == 3);
            Assert(r2 == 30);
        }

        public void DelegateInfo2() {
            object lambda = Engine.Execute("lambda { |a,b,c| r = a.value + b; a.value = 1; c.value = 10; r }");

            var creator = new DynamicDelegateCreator(Context);
            var da = (FRefIntIntOutInt)creator.GetDelegate(lambda, typeof(FRefIntIntOutInt));
            var db = (FRefIntIntOutInt)creator.GetDelegate(lambda, typeof(FRefIntIntOutInt));
            Assert(da == db);

            int a = 2;
            int c = 3;
            int r = da(ref a, 3, out c);

            Assert(r == 5);
            Assert(a == 1);
            Assert(c == 10);
        }
    }
}
