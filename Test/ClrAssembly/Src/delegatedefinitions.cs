/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/


using System;
using System.Runtime.InteropServices;
using Merlin.Testing.TypeSample;


namespace Merlin.Testing.Delegate {

    // return values
    public delegate void VoidVoidDelegate();
    public delegate Byte ByteVoidDelegate();
    public delegate SByte SByteVoidDelegate();
    public delegate UInt16 UInt16VoidDelegate();
    public delegate Int16 Int16VoidDelegate();
    public delegate UInt32 UInt32VoidDelegate();
    public delegate Int32 Int32VoidDelegate();
    public delegate UInt64 UInt64VoidDelegate();
    public delegate Int64 Int64VoidDelegate();
    public delegate Double DoubleVoidDelegate();
    public delegate Single SingleVoidDelegate();
    public delegate Decimal DecimalVoidDelegate();
    public delegate Char CharVoidDelegate();
    public delegate Boolean BooleanVoidDelegate();
    public delegate String StringVoidDelegate();

    public delegate EnumInt16 EnumVoidDelegate();
    public delegate SimpleInterface InterfaceVoidDelegate();
    public delegate SimpleStruct StructVoidDelegate();
    public delegate SimpleClass ClassVoidDelegate();

    public delegate SimpleGenericClass<Int32> ClosedGenericClassVoidDelegate();
    public delegate SimpleGenericStruct<Int32> ClosedGenericStructVoidDelegate();

    public delegate SimpleGenericClass<T> OpenGenericClassVoidDelegate<T>();
    public delegate SimpleGenericStruct<T> OpenGenericStructVoidDelegate<T>();

    public partial class ClassWithTargetMethods {
        // instance 
        public void MVoidVoid() { }
        public Byte MByteVoid() { return 0; }

        // static 
    }

    public partial struct StructWithTargetMethods {
    }

    public class GenericClassWithTargetMethods<T> {
        public T MTVoid() { return default(T); }
        public Int32 MInt32Void() { return 1; }

        public SimpleGenericClass<T> MOpenGenericClassVoid() { return null; }
    }

    //
    // delegate with different signatures
    //
    public delegate void VoidInt32Delegate(Int32 arg);
    public delegate void VoidInt32Int32Delegate(Int32 arg1, Int32 arg2);

    public delegate void VoidInt32ArrayDelegate(Int32[] args);
    public delegate void VoidInt32ParamsArrayDelegate(params Int32[] args);
    public delegate void VoidInt32Int32ParamsArrayDelegate(Int32 arg1, params Int32[] args);

    public delegate void VoidRefInt32Delegate(ref Int32 arg);
    public delegate void VoidOutInt32Delegate(out Int32 arg);

    public delegate void VoidSelfInt32Delegate(ClassWithTargetMethods self, Int32 arg);

    public partial class ClassWithTargetMethods {
        public Int32 MInt32Void() { return 3; }

        public void MVoidInt32(Int32 arg) { /*Console.WriteLine(arg);*/ Flag.Set(arg); }
        public static void SMVoidInt32(Int32 arg) { Flag.Set(10 * arg); }

        public void MVoidByte(Byte arg) { Flag.Set(100); }
        public void MVoidDouble(Double arg) { /*Console.WriteLine(arg);*/  Flag.Set(200); }
        public void MVoidInt32Int32(Int32 arg1, Int32 arg2) { Flag.Set(arg1 + arg2); }
        public void MVoidRefInt32(ref Int32 arg) { Flag.Set(arg); arg = -100; }
        public void MVoidOutInt32(out Int32 arg) { Flag.Set(300); arg = 200; }
    }

    public partial class ClassWithTargetMethods {
        public void MOverload1() { Flag.Set(100); }
        public void MOverload1(Int32 arg) { Flag.Set(110); }
        public void MOverload1(Int32 arg1, Int32 arg2) { Flag.Set(120); }

        public void MOverload2(Int32 arg, params Int32[] args) { Flag.Set(210); }
        public void MOverload2(Int32 arg) { Flag.Set(200); }

        public void MOverload3(ref Int32 arg) { Flag.Set(310); arg = 1; }
        public void MOverload3(Int32 arg) { Flag.Set(300); }
    }

    // \\vcslabsrv\Drops\orcas\VCS\Tst\current\qa\md\src\vcs\Compiler\csharp\Source\Conformance\delegates\variance\basic003.cs

    public delegate void VoidB100Delegate(B100 arg);
    public class A100 { }
    public class B100 : A100 { }
    public class C100 : B100 { }

    public partial class ClassWithTargetMethods {
        public void MOverload8(A100 arg) { Flag.Set(800); }
        public void MOverload8(B100 arg) { Flag.Set(810); }
        public void MOverload8(C100 arg) { Flag.Set(820); }
    }

    // generic delegate
    public delegate void VoidTDelegate(int arg);
    public delegate void VoidTDelegate<T>(T arg);

    // Relaxed Rules for Delegate Binding
    // http://msdn2.microsoft.com/en-us/library/96b1ayy4.aspx

    public class Base { }
    public class Derived : Base { }
    public delegate Base BaseDerivedDelegate(Derived arg);
    public delegate Derived DerivedBaseDelegate(Base arg);

    public partial class ClassWithTargetMethods {
        public Derived MDerivedBase(Base arg) { Flag.Set(345); return new Derived(); }
        public Base MBaseDerived(Derived arg) { Flag.Set(456); return new Base(); }

        public Derived MDerivedBaseReturnNull(Base arg) { Flag.Set(567); return null; }
        public Base MBaseDerivedReturnNull(Derived arg) { Flag.Set(678); return null; }

        class Test {
            void M() {
                ClassWithTargetMethods x = new ClassWithTargetMethods();
                BaseDerivedDelegate d = new BaseDerivedDelegate(x.MDerivedBase);

                // error CS0123: No overload for 'MBaseDerived' matches delegate 'Merlin.Testing.Delegate.DerivedBaseDelegate
                // DerivedBaseDelegate d2 = new DerivedBaseDelegate(x.MBaseDerived);
            }
        }
    }

    public delegate void InterfaceInt32Delegate(InterfaceWithTargetMethods self, Int32 arg);

    public interface InterfaceWithTargetMethods {
        void MVoidInt32(Int32 arg);
    }

    public partial struct StructWithTargetMethods {
        public void MVoidInt32(Int32 arg) { Flag.Set(arg * 2); }
        public static void SMVoidInt32(Int32 arg) { Flag.Set(arg * 3); }
    }

}