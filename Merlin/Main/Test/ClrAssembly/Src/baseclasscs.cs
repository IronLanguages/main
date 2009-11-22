/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/


using System;
using Merlin.Testing.TypeSample;

namespace Merlin.Testing.BaseClass {
    public struct EmptyStruct { }
    public enum EmptyEnum { }

    public class EmptyClass { }
    public class EmptyGenericClass<T> { }

    public class EmptyTypeGroup1<T> { }
    public class EmptyTypeGroup1<K, V> { }

    public class EmptyTypeGroup2 { }
    public class EmptyTypeGroup2<T> { }

    public sealed class SealedClass { }

    public interface IEmpty { }
    public interface IGenericEmpty<T> { }

    public interface IInterfaceGroup1 { }
    public interface IInterfaceGroup1<T> { }

    public interface IInterfaceGroup2 { }
    public interface IInterfaceGroup2<T> { }

    public delegate void EmptyDelegate();

    public interface INotEmpty {
        event EmptyDelegate D;
        void M1();
        void M2(ref int arg);
        int P { get; set; }
        int this[int index] { get; set; }
    }

    public abstract class AbstractEmptyClass { }
    public abstract class AbstractNotEmptyClass {
        public virtual void M1() { }
        public abstract void M2();
        public void M3() { }

        public virtual int P {
            get { return 1; }
            set { }
        }
    }

    public interface IInterface21 { int m21(); }
    public interface IInterface22 { int m22(); }
    public interface IInterface23 { int m23(); }
    public interface IInterface24 : IInterface21 { int m24(); }
    public interface IInterface25 : IInterface24, IInterface21, IInterface22 { int m25(); }
    public interface IInterface26 { int m26(); }

    public class Class41 { public int m41() { return 41; } }
    public class Class42 { public virtual int m42() { return 42; } }
    public interface IInterface42 { int m42(); }

    public class Class43 : IInterface26 { public int m26() { return 43; } }
    public class Class44 : IInterface26 { public virtual int m26() { return 44; } }

    public interface IInterface110a { int m110(); }
    public interface IInterface110b { int m110(int arg); }
    public interface IInterface110c : IInterface110a, IInterface110b { }

    public abstract class Class200a { public abstract int m200(); }
    public class Class200b : Class200a {
        public override int m200() { return 200; }
    }

    public class Class210a { public virtual int m210() { return 210; } }
    public class Class210b : Class210a { new public virtual int m210() { return 211; } }
    public class Class210c : Class210a { public override int m210() { return 212; } }
    public class Class210d : Class210a { }
    public class Class210e : Class210a { public sealed override int m210() { return 214; } }

    public class Class215a { protected virtual int m215() { return 215; } }
    public class Class215b : Class215a { new protected virtual int m215() { return 216; } }
    public class Class215c : Class215a { protected override int m215() { return 217; } }
    public class Class215d : Class215a { }
    public class Class215e : Class215a { protected sealed override int m215() { return 218; } }

    public partial class Callback {
        public static int On(Class210a arg) { return arg.m210(); }
    }

    public interface IInterface250 { int m250<T>(T arg); }
    public interface IInterface251 {
        int m251(int arg);
        int m251<T>(T arg);
    }

    public interface IInterface260<T> { int m260(); }

    public abstract class Class300 { public abstract int m300<T>(); }

    public class Class310 {
        public virtual int m310a<T>(T arg) { return 1; }
        public int m310b<K, V>(K arg1, V arg2) { return 2; }

        public int m310c(int arg) { return 3; }
        public int m310c<T>(int arg) { return 4; }
    }

    public class Class320<T> {
        public virtual int m320(T arg) { return 11; }
    }

    public class Class500 {
        public static int m500a() { return 501; }
        public static int m500b<T>() { return 502; }

        public static int m500c() { return 503; }
        public static int m500c<T>() { return 504; }

        public static int m500d<T>() { return 505; }
        public static int m500d<K, V>() { return 506; }
    }

    public interface IInterface600 {
        void m_a(ref int arg);
        void m_b(out int arg);
        void m_c(int[] arg);
        void m_d(params int[] arg);

        int m_e();
        void m_f();

        int m_g(ref int arg1, int arg2);
        int m_h(out int arg1, ref int arg2);

        int m_l(ref int arg1, out int arg2, int arg3, params int[] arg4);
        int m_k(ref int arg1, out int arg2, int arg3, int[] arg4);
    }

    // property inheritance
    public interface IProperty10 {
        int IntProperty { get; set; }
    }
    public interface IProperty11 {
        string StrProperty { get; }
    }
    public interface IProperty12 {
        double DoubleProperty { set; }
    }
    public interface IIndexer20 {
        // overload
        string this[int index] { get; set; }
        string this[int index1, int index2] { get; set; }
    }
    public interface IIndexer21 {
        int this[int index] { get; }
    }
    public interface IIndexer22 {
        double this[int index] { set; }
    }

    public class CProperty30 {
        protected int _p;
        public virtual int Property {
            get { return _p; }
            set { _p = value + 20; }
        }
    }
    public abstract class CProperty31 {
        public abstract int Property { get; set; }
    }

    public class CProperty32 : CProperty30 {
        public sealed override int Property {
            get { return _p - 4; }
            set { _p = value + 40; }
        }
    }
    public class CProperty33 {
        static int _p;
        public static int Property {
            get { return _p; }
            set { _p = value + 60; }
        }
    }
    public class CIndexer40 {
        int[] array = new int[] { 1, 2, 3, 4, 5, 6, 7 };
        public virtual int this[int index] {
            get { return array[index]; }
            set { array[index] = value; }
        }
    }

    public partial class Callback {
        public static void On1(IIndexer20 arg) {
            arg[1] = arg[1].ToLower();
            arg[2] = arg[2].ToUpper();
        }
        public static void On2(IIndexer20 arg) {
            arg[1, 2] = arg[3, 4] + "inside clr";
        }
        public static void On(CProperty30 arg) {
            arg.Property += 200;
        }
        public static void On(CProperty31 arg) {
            arg.Property += 100;
        }
        public static void On(CProperty32 arg) {
            arg.Property += 400;
        }
        public static void On(CIndexer40 arg) {
            arg[0] = arg[1] + 10;
        }
    }

    // event inheritance

    public interface IEvent10 {
        event Int32Int32Delegate Act;
    }

#pragma warning disable 0067
    public class CEvent40 {
        public event Int32Int32Delegate Act;
    }

    public class C : IEvent10 {
        public event Int32Int32Delegate Act;
    }
#pragma warning restore

    public partial class Callback {
        public static void On(IEvent10 arg) {
            arg.Act += new Int32Int32Delegate(arg_Act);
        }

        static int arg_Act(int arg) {
            Console.WriteLine(arg);
            return arg;
        }
    }

    // ctor 
    public class CCtor10 {
        public CCtor10() { Flag.Set(42); }
    }
    public class CCtor20 {
        public CCtor20(int arg) { Flag.Set(arg); }
    }
    public class CCtor21 {
        public CCtor21(ref int arg) { Flag.Set(arg); arg *= 10; }
    }
    public class CCtor22 {
        public CCtor22(out int arg) { arg = 20; }
    }
    public class CCtor30 {
        public CCtor30(int[] args) { if (args == null) Flag.Set(-10); else Flag.Set(Helper.Sum(args)); }
    }
    public class CCtor31 {
        public CCtor31(params int[] args) {
            if (args == null) Flag.Set(-40);
            else if (args.Length == 0) Flag.Set(-20);  // explicit special casing here
            else Flag.Set(Helper.Sum(args));
        }
    }
    public class CCtor32 {
        public CCtor32(int arg1, params int[] args) { Flag.Set(arg1 + Helper.Sum(args)); }
    }

    public class CCtor40 {
        public CCtor40(int arg1, int arg2, int arg3, int arg4, int arg5) { Flag.Set(arg1 * 10000 + arg2 * 1000 + arg3 * 100 + arg4 * 10 + arg5); }
    }

    // ctor overloads
    public class CCtor50 {
        public CCtor50(int arg) { Flag.Set(arg); }
        public CCtor50(int arg1, int arg2) { Flag.Set(arg1 + arg2); }
    }

    public class CCtor51 {
        public CCtor51(int arg) { Flag.Set(10); }
        public CCtor51(params int[] args) { Flag.Set(20); }
    }

    // long hierarchy scenarios
    public class CType10 { public virtual void m1() { Flag.Set(10); } }
    public abstract class CType11 : CType10 { public abstract void m2(); }

    public interface IType20 { void m1();}
    public abstract class CType21 : IType20 {
        public abstract void m1();
        public abstract void m2();
    }

    public partial class Callback {
        public static void On(CType11 arg) { arg.m1(); }
        public static void On(CType21 arg) { arg.m1(); }
    }
}

namespace Merlin.Testing.Accessibility {
#pragma warning disable 0067, 0649
    public class CliClass {
        private static int private_static_field;
        private static int private_static_property {
            get { return private_static_field; }
            set { private_static_field = value; }
        }
        private static string private_static_method() { return "private_static"; }
        private static event System.EventHandler private_static_event;
        private static class private_static_nestedclass { }
        private int private_instance_field;
        private int private_instance_property {
            get { return private_instance_field; }
            set { private_instance_field = value; }
        }
        private string private_instance_method() { return "private_instance"; }
        private event System.EventHandler private_instance_event;
        private class private_instance_nestedclass { }
        internal static int internal_static_field;
        internal static int internal_static_property {
            get { return private_static_field; }
            set { private_static_field = value; }
        }
        internal static string internal_static_method() { return "internal_static"; }
        internal static event System.EventHandler internal_static_event;
        internal static class internal_static_nestedclass { }
        internal int internal_instance_field;
        internal int internal_instance_property {
            get { return private_instance_field; }
            set { private_instance_field = value; }
        }
        internal string internal_instance_method() { return "internal_instance"; }
        internal event System.EventHandler internal_instance_event;
        internal class internal_instance_nestedclass { }
        protected static int protected_static_field;
        protected static int protected_static_property {
            get { return private_static_field; }
            set { private_static_field = value; }
        }
        protected static string protected_static_method() { return "protected_static"; }
        protected static event System.EventHandler protected_static_event;
        protected static class protected_static_nestedclass { }
        protected int protected_instance_field;
        protected int protected_instance_property {
            get { return private_instance_field; }
            set { private_instance_field = value; }
        }
        protected string protected_instance_method() { return "protected_instance"; }
        protected event System.EventHandler protected_instance_event;
        protected class protected_instance_nestedclass { }
        public static int public_static_field;
        public static int public_static_property {
            get { return private_static_field; }
            set { private_static_field = value; }
        }
        public static string public_static_method() { return "public_static"; }
        public static event System.EventHandler public_static_event;
        public static class public_static_nestedclass { }
        public int public_instance_field;
        public int public_instance_property {
            get { return private_instance_field; }
            set { private_instance_field = value; }
        }
        public string public_instance_method() { return "public_instance"; }
        public event System.EventHandler public_instance_event;
        public class public_instance_nestedclass { }
    }

    public class DerivedCliClass : CliClass {
    }
#pragma warning restore
}
