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

// C# test reference: 
//   \\vcslabsrv\Drops\orcas\VCS\Tst\current\qa\md\src\vcs\Compiler\csharp\Source\Conformance\conversions

namespace Merlin.Testing.Call {

    #region conversion between simple clr built-in type and class/struct
    public class ByteWrapperClass {
        public Byte Value;
        public ByteWrapperClass(Byte arg) { Value = arg; }
        public static implicit operator Byte(ByteWrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator ByteWrapperClass(Byte arg) { return new ByteWrapperClass(arg); }
    }
    public class SByteWrapperClass {
        public SByte Value;
        public SByteWrapperClass(SByte arg) { Value = arg; }
        public static implicit operator SByte(SByteWrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator SByteWrapperClass(SByte arg) { return new SByteWrapperClass(arg); }
    }
    public class UInt16WrapperClass {
        public UInt16 Value;
        public UInt16WrapperClass(UInt16 arg) { Value = arg; }
        public static implicit operator UInt16(UInt16WrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator UInt16WrapperClass(UInt16 arg) { return new UInt16WrapperClass(arg); }
    }
    public class Int16WrapperClass {
        public Int16 Value;
        public Int16WrapperClass(Int16 arg) { Value = arg; }
        public static implicit operator Int16(Int16WrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator Int16WrapperClass(Int16 arg) { return new Int16WrapperClass(arg); }
    }
    public class UInt32WrapperClass {
        public UInt32 Value;
        public UInt32WrapperClass(UInt32 arg) { Value = arg; }
        public static implicit operator UInt32(UInt32WrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator UInt32WrapperClass(UInt32 arg) { return new UInt32WrapperClass(arg); }
    }
    public class Int32WrapperClass {
        public Int32 Value;
        public Int32WrapperClass(Int32 arg) { Value = arg; }
        public static implicit operator Int32(Int32WrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator Int32WrapperClass(Int32 arg) { return new Int32WrapperClass(arg); }
    }
    public class UInt64WrapperClass {
        public UInt64 Value;
        public UInt64WrapperClass(UInt64 arg) { Value = arg; }
        public static implicit operator UInt64(UInt64WrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator UInt64WrapperClass(UInt64 arg) { return new UInt64WrapperClass(arg); }
    }
    public class Int64WrapperClass {
        public Int64 Value;
        public Int64WrapperClass(Int64 arg) { Value = arg; }
        public static implicit operator Int64(Int64WrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator Int64WrapperClass(Int64 arg) { return new Int64WrapperClass(arg); }
    }
    public class DoubleWrapperClass {
        public Double Value;
        public DoubleWrapperClass(Double arg) { Value = arg; }
        public static implicit operator Double(DoubleWrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator DoubleWrapperClass(Double arg) { return new DoubleWrapperClass(arg); }
    }
    public class SingleWrapperClass {
        public Single Value;
        public SingleWrapperClass(Single arg) { Value = arg; }
        public static implicit operator Single(SingleWrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator SingleWrapperClass(Single arg) { return new SingleWrapperClass(arg); }
    }
    public class DecimalWrapperClass {
        public Decimal Value;
        public DecimalWrapperClass(Decimal arg) { Value = arg; }
        public static implicit operator Decimal(DecimalWrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator DecimalWrapperClass(Decimal arg) { return new DecimalWrapperClass(arg); }
    }
    public class CharWrapperClass {
        public Char Value;
        public CharWrapperClass(Char arg) { Value = arg; }
        public static implicit operator Char(CharWrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator CharWrapperClass(Char arg) { return new CharWrapperClass(arg); }
    }
    public class BooleanWrapperClass {
        public Boolean Value;
        public BooleanWrapperClass(Boolean arg) { Value = arg; }
        public static implicit operator Boolean(BooleanWrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator BooleanWrapperClass(Boolean arg) { return new BooleanWrapperClass(arg); }
    }
    public class StringWrapperClass {
        public String Value;
        public StringWrapperClass(String arg) { Value = arg; }
        public static implicit operator String(StringWrapperClass wrapper) { return wrapper.Value; }
        public static implicit operator StringWrapperClass(String arg) { return new StringWrapperClass(arg); }
    }
    public struct ByteWrapperStruct {
        public Byte Value;
        public ByteWrapperStruct(Byte arg) { Value = arg; }
        public static implicit operator Byte(ByteWrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator ByteWrapperStruct(Byte arg) { return new ByteWrapperStruct(arg); }
    }
    public struct SByteWrapperStruct {
        public SByte Value;
        public SByteWrapperStruct(SByte arg) { Value = arg; }
        public static implicit operator SByte(SByteWrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator SByteWrapperStruct(SByte arg) { return new SByteWrapperStruct(arg); }
    }
    public struct UInt16WrapperStruct {
        public UInt16 Value;
        public UInt16WrapperStruct(UInt16 arg) { Value = arg; }
        public static implicit operator UInt16(UInt16WrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator UInt16WrapperStruct(UInt16 arg) { return new UInt16WrapperStruct(arg); }
    }
    public struct Int16WrapperStruct {
        public Int16 Value;
        public Int16WrapperStruct(Int16 arg) { Value = arg; }
        public static implicit operator Int16(Int16WrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator Int16WrapperStruct(Int16 arg) { return new Int16WrapperStruct(arg); }
    }
    public struct UInt32WrapperStruct {
        public UInt32 Value;
        public UInt32WrapperStruct(UInt32 arg) { Value = arg; }
        public static implicit operator UInt32(UInt32WrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator UInt32WrapperStruct(UInt32 arg) { return new UInt32WrapperStruct(arg); }
    }
    public struct Int32WrapperStruct {
        public Int32 Value;
        public Int32WrapperStruct(Int32 arg) { Value = arg; }
        public static implicit operator Int32(Int32WrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator Int32WrapperStruct(Int32 arg) { return new Int32WrapperStruct(arg); }
    }
    public struct UInt64WrapperStruct {
        public UInt64 Value;
        public UInt64WrapperStruct(UInt64 arg) { Value = arg; }
        public static implicit operator UInt64(UInt64WrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator UInt64WrapperStruct(UInt64 arg) { return new UInt64WrapperStruct(arg); }
    }
    public struct Int64WrapperStruct {
        public Int64 Value;
        public Int64WrapperStruct(Int64 arg) { Value = arg; }
        public static implicit operator Int64(Int64WrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator Int64WrapperStruct(Int64 arg) { return new Int64WrapperStruct(arg); }
    }
    public struct DoubleWrapperStruct {
        public Double Value;
        public DoubleWrapperStruct(Double arg) { Value = arg; }
        public static implicit operator Double(DoubleWrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator DoubleWrapperStruct(Double arg) { return new DoubleWrapperStruct(arg); }
    }
    public struct SingleWrapperStruct {
        public Single Value;
        public SingleWrapperStruct(Single arg) { Value = arg; }
        public static implicit operator Single(SingleWrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator SingleWrapperStruct(Single arg) { return new SingleWrapperStruct(arg); }
    }
    public struct DecimalWrapperStruct {
        public Decimal Value;
        public DecimalWrapperStruct(Decimal arg) { Value = arg; }
        public static implicit operator Decimal(DecimalWrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator DecimalWrapperStruct(Decimal arg) { return new DecimalWrapperStruct(arg); }
    }
    public struct CharWrapperStruct {
        public Char Value;
        public CharWrapperStruct(Char arg) { Value = arg; }
        public static implicit operator Char(CharWrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator CharWrapperStruct(Char arg) { return new CharWrapperStruct(arg); }
    }
    public struct BooleanWrapperStruct {
        public Boolean Value;
        public BooleanWrapperStruct(Boolean arg) { Value = arg; }
        public static implicit operator Boolean(BooleanWrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator BooleanWrapperStruct(Boolean arg) { return new BooleanWrapperStruct(arg); }
    }
    public struct StringWrapperStruct {
        public String Value;
        public StringWrapperStruct(String arg) { Value = arg; }
        public static implicit operator String(StringWrapperStruct wrapper) { return wrapper.Value; }
        public static implicit operator StringWrapperStruct(String arg) { return new StringWrapperStruct(arg); }
    }
    public class EnumWrapperClass {
        public EnumInt16 Value;
        public EnumWrapperClass(EnumInt16 arg) { Value = arg; }

        public static implicit operator EnumInt16(EnumWrapperClass arg) { return arg.Value; }
        public static implicit operator EnumWrapperClass(EnumInt16 arg) { return new EnumWrapperClass(arg); }
    }
    public struct EnumWrapperStruct {
        public EnumInt16 Value;
        public EnumWrapperStruct(EnumInt16 arg) { Value = arg; }

        public static implicit operator EnumInt16(EnumWrapperStruct arg) { return arg.Value; }
        public static implicit operator EnumWrapperStruct(EnumInt16 arg) { return new EnumWrapperStruct(arg); }
    }
    public partial class Consumer {
        public static object EatByteClass(ByteWrapperClass arg) { Flag.Set(100); return arg.Value; }
        public static object EatByteStruct(ByteWrapperStruct arg) { Flag.Set(100); return arg.Value; }
        public static object EatSByteClass(SByteWrapperClass arg) { Flag.Set(110); return arg.Value; }
        public static object EatSByteStruct(SByteWrapperStruct arg) { Flag.Set(110); return arg.Value; }
        public static object EatUInt16Class(UInt16WrapperClass arg) { Flag.Set(120); return arg.Value; }
        public static object EatUInt16Struct(UInt16WrapperStruct arg) { Flag.Set(120); return arg.Value; }
        public static object EatInt16Class(Int16WrapperClass arg) { Flag.Set(130); return arg.Value; }
        public static object EatInt16Struct(Int16WrapperStruct arg) { Flag.Set(130); return arg.Value; }
        public static object EatUInt32Class(UInt32WrapperClass arg) { Flag.Set(140); return arg.Value; }
        public static object EatUInt32Struct(UInt32WrapperStruct arg) { Flag.Set(140); return arg.Value; }
        public static object EatInt32Class(Int32WrapperClass arg) { Flag.Set(150); return arg.Value; }
        public static object EatInt32Struct(Int32WrapperStruct arg) { Flag.Set(150); return arg.Value; }
        public static object EatUInt64Class(UInt64WrapperClass arg) { Flag.Set(160); return arg.Value; }
        public static object EatUInt64Struct(UInt64WrapperStruct arg) { Flag.Set(160); return arg.Value; }
        public static object EatInt64Class(Int64WrapperClass arg) { Flag.Set(170); return arg.Value; }
        public static object EatInt64Struct(Int64WrapperStruct arg) { Flag.Set(170); return arg.Value; }
        public static object EatDoubleClass(DoubleWrapperClass arg) { Flag.Set(180); return arg.Value; }
        public static object EatDoubleStruct(DoubleWrapperStruct arg) { Flag.Set(180); return arg.Value; }
        public static object EatSingleClass(SingleWrapperClass arg) { Flag.Set(190); return arg.Value; }
        public static object EatSingleStruct(SingleWrapperStruct arg) { Flag.Set(190); return arg.Value; }
        public static object EatDecimalClass(DecimalWrapperClass arg) { Flag.Set(200); return arg.Value; }
        public static object EatDecimalStruct(DecimalWrapperStruct arg) { Flag.Set(200); return arg.Value; }
        public static object EatCharClass(CharWrapperClass arg) { Flag.Set(210); return arg.Value; }
        public static object EatCharStruct(CharWrapperStruct arg) { Flag.Set(210); return arg.Value; }
        public static object EatBooleanClass(BooleanWrapperClass arg) { Flag.Set(220); return arg.Value; }
        public static object EatBooleanStruct(BooleanWrapperStruct arg) { Flag.Set(220); return arg.Value; }
        public static object EatStringClass(StringWrapperClass arg) { Flag.Set(230); return arg.Value; }
        public static object EatStringStruct(StringWrapperStruct arg) { Flag.Set(230); return arg.Value; }
        public static object EatEnumClass(EnumWrapperClass arg) { Flag.Set(240); return arg.Value; }
        public static object EatEnumStruct(EnumWrapperStruct arg) { Flag.Set(240); return arg.Value; }
    }

    public partial class Consumer {
        public static object EatByte(Byte arg) { Flag.Set(500); return arg; }
        public static object EatSByte(SByte arg) { Flag.Set(510); return arg; }
        public static object EatUInt16(UInt16 arg) { Flag.Set(520); return arg; }
        public static object EatInt16(Int16 arg) { Flag.Set(530); return arg; }
        public static object EatUInt32(UInt32 arg) { Flag.Set(540); return arg; }
        public static object EatInt32(Int32 arg) { Flag.Set(550); return arg; }
        public static object EatUInt64(UInt64 arg) { Flag.Set(560); return arg; }
        public static object EatInt64(Int64 arg) { Flag.Set(570); return arg; }
        public static object EatDouble(Double arg) { Flag.Set(580); return arg; }
        public static object EatSingle(Single arg) { Flag.Set(590); return arg; }
        public static object EatDecimal(Decimal arg) { Flag.Set(600); return arg; }
        public static object EatChar(Char arg) { Flag.Set(610); return arg; }
        public static object EatBoolean(Boolean arg) { Flag.Set(620); return arg; }
        public static object EatString(String arg) { Flag.Set(630); return arg; }
        public static object EatEnum(EnumInt16 arg) { Flag.Set(640); return arg; }
    }
    #endregion

    #region conversion between class/struct, class/class, struct/struct
    public class MixedClass1 {
        public int Value;
        public MixedClass1(int arg) { Value = arg; }
        public static implicit operator MixedStruct1(MixedClass1 arg) { return new MixedStruct1(arg.Value); }
        public static implicit operator MixedClass1(MixedStruct1 arg) { return new MixedClass1(arg.Value); }
    }
    public struct MixedStruct1 {
        public int Value;
        public MixedStruct1(int arg) { Value = arg; }
    }

    public struct MixedStruct2 {
        public int Value;
        public MixedStruct2(int arg) { Value = arg; }
        public static implicit operator MixedStruct2(MixedClass2 arg) { return new MixedStruct2(arg.Value); }
        public static implicit operator MixedClass2(MixedStruct2 arg) { return new MixedClass2(arg.Value); }
    }
    public class MixedClass2 {
        public int Value;
        public MixedClass2(int arg) { Value = arg; }
    }

    public class ClassOne {
        public int Value;
        public ClassOne(int arg) { Value = arg; }
        public static implicit operator ClassTwo(ClassOne arg) { return new ClassTwo(arg.Value); }
        public static implicit operator ClassOne(ClassTwo arg) { return new ClassOne(arg.Value); }
    }

    public class ClassTwo {
        public int Value;
        public ClassTwo(int arg) { Value = arg; }
    }

    public struct StructOne {
        public int Value;
        public StructOne(int arg) { Value = arg; }
        public static implicit operator StructTwo(StructOne arg) { return new StructTwo(arg.Value); }
        public static implicit operator StructOne(StructTwo arg) { return new StructOne(arg.Value); }
    }

    public struct StructTwo {
        public int Value;
        public StructTwo(int arg) { Value = arg; }
    }

    public partial class Consumer {
        public static object EatClassOne(ClassOne arg) { Flag.Set(100); return arg.Value; }
        public static object EatClassTwo(ClassTwo arg) { Flag.Set(110); return arg.Value; }

        public static object EatStructOne(StructOne arg) { Flag.Set(120); return arg.Value; }
        public static object EatStructTwo(StructTwo arg) { Flag.Set(130); return arg.Value; }

        public static object EatMixedClass1(MixedClass1 arg) { Flag.Set(140); return arg.Value; }
        public static object EatMixedStruct1(MixedStruct1 arg) { Flag.Set(150); return arg.Value; }

        public static object EatMixedClass2(MixedClass2 arg) { Flag.Set(160); return arg.Value; }
        public static object EatMixedStruct2(MixedStruct2 arg) { Flag.Set(170); return arg.Value; }
    }
    #endregion

    #region generics
    public class G1<T> {
        public T Value;
        public G1(T arg) { Value = arg; }

        public static implicit operator G3<T, T>(G1<T> arg) { return new G3<T, T>(arg.Value, arg.Value); }
    }

    public class GInt {
        public int Value;
        public GInt(int arg) { Value = arg; }
        public static implicit operator GInt(G1<int> arg) { return new GInt(arg.Value); }
        public static implicit operator G1<int>(GInt arg) { return new G1<int>(arg.Value); }
    }

    public struct G2<T> {
        public T Value;
        public G2(T arg) { Value = arg; }
        public static implicit operator G2<T>(G1<T> arg) { return new G2<T>(arg.Value); }
        public static implicit operator G1<T>(G2<T> arg) { return new G1<T>(arg.Value); }
    }

    public class G3<K, V> {
        public K Value1;
        public V Value2;

        public G3(K arg1, V arg2) { Value1 = arg1; Value2 = arg2; }
        public static implicit operator G1<K>(G3<K, V> arg) { return new G1<K>(arg.Value1); }
        public static implicit operator G1<V>(G3<K, V> arg) { return new G1<V>(arg.Value2); }
    }

    public partial class Consumer {
        public static object EatG1OfInt(G1<int> arg) { Flag.Set(180); return arg.Value; }
        public static object EatGInt(GInt arg) { Flag.Set(190); return arg.Value; }
        public static object EatG2OfInt(G2<int> arg) { Flag.Set(200); return arg.Value; }
        public static object EatG3OfIntInt(G3<int, int> arg) { Flag.Set(210); return arg.Value1; }
    }
    #endregion

    #region many things can be converted to me
    public class OmniTarget {
        public int Value;
        public OmniTarget(int arg) { Value = arg; }
        public static implicit operator OmniTarget(double arg) { return new OmniTarget(10); }
        public static implicit operator OmniTarget(int arg) { return new OmniTarget(20); }
        public static implicit operator OmniTarget(EnumInt16 arg) { return new OmniTarget(30); }
        public static implicit operator OmniTarget(SimpleStruct arg) { return new OmniTarget(40); }
    }

    public partial class Consumer {
        public static object EatOmniTarget(OmniTarget arg) { return arg.Value; }
    }
    #endregion

    // derivation: too many combinations, here are some samples

    public class SBase1 { }
    public class S1 : SBase1 {
        public static implicit operator T1(S1 arg) { return new T1(); }
    }
    public class SDerived1 : S1 { }

    public class TBase1 { }
    public class T1 : TBase1 { }
    public class TDerived1 : T1 { }

    // TODO: more scenarios, 
    //       -- including value type
    //       -- defined the operator in Tx

    public partial class Consumer {
        public static void EatTBase1(TBase1 arg) { Flag.Set(701); }
        public static void EatT1(T1 arg) { Flag.Set(702); }
        public static void EatTDerived1(TDerived1 arg) { Flag.Set(703); }

        // ...
    }

    //
    // implicit reference conversion
    //
    public struct AnyStruct { }
    public class AnyReference { }

    public class First { }
    public class Second : First { }
    public class Third : Second { }

    public interface IBase { }
    public interface IDerived : IBase { }

    public struct StructBase : IBase { }
    public class ClassBase : IBase { }

    public struct StructDerived : IDerived { }
    public class ClassDerived : First, IDerived { }

    public class Target1 { }
    public struct Target2 { }
    public class Source1 {
        public static implicit operator Target1(Source1 arg) { return new Target1(); }
        public static implicit operator Target2(Source1 arg) { return new Target2(); }
    }

    public partial class Consumer {
        public static void EatObject(object arg) { Flag.Set(801); } // try boxing conversion
        public static void EatIBase(IBase arg) { Flag.Set(802); }
        public static void EatIDerived(IDerived arg) { Flag.Set(803); }

        public static void EatInt32WrapperClassArray(Int32WrapperClass[] args) { Flag.Set(810); }
        public static void EatInt32WrapperStructArray(Int32WrapperStruct[] args) { Flag.Set(811); }
        public static void EatInt32Array(Int32[] args) { Flag.Set(812); }
        public static void EatTarget1Array(Target1[] arg) { Flag.Set(813); }
        public static void EatTarget2Array(Target2[] arg) { Flag.Set(814); }
        public static void EatFirstArray(First[] arg) { Flag.Set(815); }

        public static void EatArray(Array arg) { Flag.Set(806); }
        public static void EatDelegate(Delegate arg) { Flag.Set(807); }

        public static void MVoidVoid() { }
        public static Int32 MInt32Int32(Int32 arg) { return 1; }

        public static void EatFirst(First arg) { Flag.Set(808); }
        public static void EatValueType(ValueType arg) { Flag.Set(809); } // boxing conversion
        public static void EatEnumType(Enum arg) { Flag.Set(810); } // boxing conversion 

        public static void EatAnyStruct(AnyStruct arg) { Flag.Set(850); }
        public static void EatNullableAnyStruct(Nullable<AnyStruct> arg) { Flag.Set(851); }
    }

    // to be removed
    public class X {
        public static void M() {
            Consumer.EatT1(new SDerived1());
            Consumer.EatTBase1(new S1());

            Consumer.EatDelegate(new VoidVoidDelegate(Consumer.MVoidVoid));
            Consumer.EatEnum(0);
        }
    }
}