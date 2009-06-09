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

namespace Merlin.Testing.FieldTest.Literal {
    public struct StructWithLiterals {
        public const byte LiteralByteField = 1;
        public const sbyte LiteralSByteField = 2;
        public const ushort LiteralUInt16Field = 3;
        public const short LiteralInt16Field = 4;
        public const uint LiteralUInt32Field = 5;
        public const int LiteralInt32Field = 6;
        public const ulong LiteralUInt64Field = 7;
        public const long LiteralInt64Field = 8;
        public const double LiteralDoubleField = 9;
        public const float LiteralSingleField = 10;
        public const decimal LiteralDecimalField = 11;

        public const char LiteralCharField = 'K';
        public const bool LiteralBooleanField = true;

        public const EnumInt32 LiteralEnumField = EnumInt32.B;
        public const String LiteralStringField = "DLR";

        public const SimpleClass LiteralClassField = null; // have to be null
        public const SimpleInterface LiteralInterfaceField = null;
    }
    public struct GenericStructWithLiterals<T> {
        public const byte LiteralByteField = 1;
        public const sbyte LiteralSByteField = 2;
        public const ushort LiteralUInt16Field = 3;
        public const short LiteralInt16Field = 4;
        public const uint LiteralUInt32Field = 5;
        public const int LiteralInt32Field = 6;
        public const ulong LiteralUInt64Field = 7;
        public const long LiteralInt64Field = 8;
        public const double LiteralDoubleField = 9;
        public const float LiteralSingleField = 10;
        public const decimal LiteralDecimalField = 11;

        public const char LiteralCharField = 'K';
        public const bool LiteralBooleanField = true;

        public const EnumInt32 LiteralEnumField = EnumInt32.B;
        public const String LiteralStringField = "DLR";

        public const SimpleClass LiteralClassField = null; // have to be null
        public const SimpleInterface LiteralInterfaceField = null;
    }

    public class ClassWithLiterals {
        public const byte LiteralByteField = 1;
        public const sbyte LiteralSByteField = 2;
        public const ushort LiteralUInt16Field = 3;
        public const short LiteralInt16Field = 4;
        public const uint LiteralUInt32Field = 5;
        public const int LiteralInt32Field = 6;
        public const ulong LiteralUInt64Field = 7;
        public const long LiteralInt64Field = 8;
        public const double LiteralDoubleField = 9;
        public const float LiteralSingleField = 10;
        public const decimal LiteralDecimalField = 11;

        public const char LiteralCharField = 'K';
        public const bool LiteralBooleanField = true;

        public const EnumInt32 LiteralEnumField = EnumInt32.B;
        public const String LiteralStringField = "DLR";

        public const SimpleClass LiteralClassField = null; // have to be null
        public const SimpleInterface LiteralInterfaceField = null;
    }
    public class GenericClassWithLiterals<T> {
        public const byte LiteralByteField = 1;
        public const sbyte LiteralSByteField = 2;
        public const ushort LiteralUInt16Field = 3;
        public const short LiteralInt16Field = 4;
        public const uint LiteralUInt32Field = 5;
        public const int LiteralInt32Field = 6;
        public const ulong LiteralUInt64Field = 7;
        public const long LiteralInt64Field = 8;
        public const double LiteralDoubleField = 9;
        public const float LiteralSingleField = 10;
        public const decimal LiteralDecimalField = 11;

        public const char LiteralCharField = 'K';
        public const bool LiteralBooleanField = true;

        public const EnumInt32 LiteralEnumField = EnumInt32.B;
        public const String LiteralStringField = "DLR";

        public const SimpleClass LiteralClassField = null; // have to be null
        public const SimpleInterface LiteralInterfaceField = null;
    }

    public class DerivedClass : ClassWithLiterals { }
}

namespace Merlin.Testing.FieldTest.InitOnly {
    public struct StructWithInitOnlys {
        public static readonly byte InitOnlyByteField = 0;
        public static readonly sbyte InitOnlySByteField = 1;
        public static readonly ushort InitOnlyUInt16Field = 2;
        public static readonly short InitOnlyInt16Field = 3;
        public static readonly uint InitOnlyUInt32Field = 4;
        public static readonly int InitOnlyInt32Field = 5;
        public static readonly ulong InitOnlyUInt64Field = 6;
        public static readonly long InitOnlyInt64Field = 7;
        public static readonly double InitOnlyDoubleField = 8;
        public static readonly float InitOnlySingleField = 9;
        public static readonly decimal InitOnlyDecimalField = 10;

        public static readonly char InitOnlyCharField = 'P';
        public static readonly bool InitOnlyBooleanField = true;

        public static readonly EnumInt16 InitOnlyEnumField = EnumInt16.B;
        public static readonly string InitOnlyStringField = "ruby";

        public static readonly DateTime InitOnlyDateTimeField = new DateTime(5);

        public static readonly SimpleStruct InitOnlySimpleStructField = new SimpleStruct(10);
        public static readonly SimpleGenericStruct<UInt16> InitOnlySimpleGenericStructField = new SimpleGenericStruct<UInt16>(20);

        public static readonly Nullable<SimpleStruct> InitOnlyNullableStructField_NotNull = new SimpleStruct(30);
        public static readonly Nullable<SimpleStruct> InitOnlyNullableStructField_Null = null;

        public static readonly SimpleClass InitOnlySimpleClassField = new SimpleClass(40);
        public static readonly SimpleGenericClass<String> InitOnlySimpleGenericClassField = new SimpleGenericClass<String>("ironruby");

        public static readonly SimpleInterface InitOnlySimpleInterfaceField = new ClassImplementSimpleInterface(50);
    }
    public struct GenericStructWithInitOnlys<T> {
        public static readonly byte InitOnlyByteField = 0;
        public static readonly sbyte InitOnlySByteField = 1;
        public static readonly ushort InitOnlyUInt16Field = 2;
        public static readonly short InitOnlyInt16Field = 3;
        public static readonly uint InitOnlyUInt32Field = 4;
        public static readonly int InitOnlyInt32Field = 5;
        public static readonly ulong InitOnlyUInt64Field = 6;
        public static readonly long InitOnlyInt64Field = 7;
        public static readonly double InitOnlyDoubleField = 8;
        public static readonly float InitOnlySingleField = 9;
        public static readonly decimal InitOnlyDecimalField = 10;

        public static readonly char InitOnlyCharField = 'P';
        public static readonly bool InitOnlyBooleanField = true;

        public static readonly EnumInt16 InitOnlyEnumField = EnumInt16.B;
        public static readonly string InitOnlyStringField = "ruby";

        public static readonly DateTime InitOnlyDateTimeField = new DateTime(5);

        public static readonly SimpleStruct InitOnlySimpleStructField = new SimpleStruct(10);
        public static readonly SimpleGenericStruct<UInt16> InitOnlySimpleGenericStructField = new SimpleGenericStruct<UInt16>(20);

        public static readonly Nullable<SimpleStruct> InitOnlyNullableStructField_NotNull = new SimpleStruct(30);
        public static readonly Nullable<SimpleStruct> InitOnlyNullableStructField_Null = null;

        public static readonly SimpleClass InitOnlySimpleClassField = new SimpleClass(40);
        public static readonly SimpleGenericClass<String> InitOnlySimpleGenericClassField = new SimpleGenericClass<String>("ironruby");

        public static readonly SimpleInterface InitOnlySimpleInterfaceField = new ClassImplementSimpleInterface(50);

        // with T
        public static readonly T InitOnlyTField = default(T);
        public static readonly SimpleGenericClass<T> InitOnlyClassTField = new SimpleGenericClass<T>(default(T));
        public static readonly SimpleGenericStruct<T> InitOnlyStructTField = new SimpleGenericStruct<T>(default(T));
    }

    public class ClassWithInitOnlys {
        public static readonly byte InitOnlyByteField = 0;
        public static readonly sbyte InitOnlySByteField = 1;
        public static readonly ushort InitOnlyUInt16Field = 2;
        public static readonly short InitOnlyInt16Field = 3;
        public static readonly uint InitOnlyUInt32Field = 4;
        public static readonly int InitOnlyInt32Field = 5;
        public static readonly ulong InitOnlyUInt64Field = 6;
        public static readonly long InitOnlyInt64Field = 7;
        public static readonly double InitOnlyDoubleField = 8;
        public static readonly float InitOnlySingleField = 9;
        public static readonly decimal InitOnlyDecimalField = 10;

        public static readonly char InitOnlyCharField = 'P';
        public static readonly bool InitOnlyBooleanField = true;

        public static readonly EnumInt16 InitOnlyEnumField = EnumInt16.B;
        public static readonly string InitOnlyStringField = "ruby";

        public static readonly DateTime InitOnlyDateTimeField = new DateTime(5);

        public static readonly SimpleStruct InitOnlySimpleStructField = new SimpleStruct(10);
        public static readonly SimpleGenericStruct<UInt16> InitOnlySimpleGenericStructField = new SimpleGenericStruct<UInt16>(20);

        public static readonly Nullable<SimpleStruct> InitOnlyNullableStructField_NotNull = new SimpleStruct(30);
        public static readonly Nullable<SimpleStruct> InitOnlyNullableStructField_Null = null;

        public static readonly SimpleClass InitOnlySimpleClassField = new SimpleClass(40);
        public static readonly SimpleGenericClass<String> InitOnlySimpleGenericClassField = new SimpleGenericClass<String>("ironruby");

        public static readonly SimpleInterface InitOnlySimpleInterfaceField = new ClassImplementSimpleInterface(50);
    }
    public class GenericClassWithInitOnlys<T> {
        public static readonly byte InitOnlyByteField = 0;
        public static readonly sbyte InitOnlySByteField = 1;
        public static readonly ushort InitOnlyUInt16Field = 2;
        public static readonly short InitOnlyInt16Field = 3;
        public static readonly uint InitOnlyUInt32Field = 4;
        public static readonly int InitOnlyInt32Field = 5;
        public static readonly ulong InitOnlyUInt64Field = 6;
        public static readonly long InitOnlyInt64Field = 7;
        public static readonly double InitOnlyDoubleField = 8;
        public static readonly float InitOnlySingleField = 9;
        public static readonly decimal InitOnlyDecimalField = 10;

        public static readonly char InitOnlyCharField = 'P';
        public static readonly bool InitOnlyBooleanField = true;

        public static readonly EnumInt16 InitOnlyEnumField = EnumInt16.B;
        public static readonly string InitOnlyStringField = "ruby";

        public static readonly DateTime InitOnlyDateTimeField = new DateTime(5);

        public static readonly SimpleStruct InitOnlySimpleStructField = new SimpleStruct(10);
        public static readonly SimpleGenericStruct<UInt16> InitOnlySimpleGenericStructField = new SimpleGenericStruct<UInt16>(20);

        public static readonly Nullable<SimpleStruct> InitOnlyNullableStructField_NotNull = new SimpleStruct(30);
        public static readonly Nullable<SimpleStruct> InitOnlyNullableStructField_Null = null;

        public static readonly SimpleClass InitOnlySimpleClassField = new SimpleClass(40);
        public static readonly SimpleGenericClass<String> InitOnlySimpleGenericClassField = new SimpleGenericClass<String>("ironruby");

        public static readonly SimpleInterface InitOnlySimpleInterfaceField = new ClassImplementSimpleInterface(50);

        // with T
        public static readonly T InitOnlyTField = default(T);
        public static readonly SimpleGenericClass<T> InitOnlyClassTField = new SimpleGenericClass<T>(default(T));
        public static readonly SimpleGenericStruct<T> InitOnlyStructTField = new SimpleGenericStruct<T>(default(T));
    }

    public class DerivedClass : ClassWithInitOnlys { }
}

namespace Merlin.Testing.FieldTest {
    public struct Struct {
        public static byte StaticByteField;
        public static sbyte StaticSByteField;
        public static ushort StaticUInt16Field;
        public static short StaticInt16Field;
        public static uint StaticUInt32Field;
        public static int StaticInt32Field;
        public static ulong StaticUInt64Field;
        public static long StaticInt64Field;
        public static double StaticDoubleField;
        public static float StaticSingleField;
        public static decimal StaticDecimalField;
        public static char StaticCharField;
        public static bool StaticBooleanField;
        public static string StaticStringField;

        public static object StaticObjectField;
        public static EnumInt64 StaticEnumField;
        public static DateTime StaticDateTimeField;

        public static SimpleStruct StaticSimpleStructField;
        public static SimpleGenericStruct<UInt16> StaticSimpleGenericStructField;
        public static Nullable<SimpleStruct> StaticNullableStructNotNullField;
        public static Nullable<SimpleStruct> StaticNullableStructNullField;

        public static SimpleClass StaticSimpleClassField;
        public static SimpleGenericClass<String> StaticSimpleGenericClassField;

        public static SimpleInterface StaticSimpleInterfaceField;

        public byte InstanceByteField;
        public sbyte InstanceSByteField;
        public ushort InstanceUInt16Field;
        public short InstanceInt16Field;
        public uint InstanceUInt32Field;
        public int InstanceInt32Field;
        public ulong InstanceUInt64Field;
        public long InstanceInt64Field;
        public double InstanceDoubleField;
        public float InstanceSingleField;
        public decimal InstanceDecimalField;
        public char InstanceCharField;
        public bool InstanceBooleanField;
        public string InstanceStringField;

        public object InstanceObjectField;
        public EnumInt64 InstanceEnumField;
        public DateTime InstanceDateTimeField;

        public SimpleStruct InstanceSimpleStructField;
        public SimpleGenericStruct<UInt16> InstanceSimpleGenericStructField;
        public Nullable<SimpleStruct> InstanceNullableStructNotNullField;
        public Nullable<SimpleStruct> InstanceNullableStructNullField;

        public SimpleClass InstanceSimpleClassField;
        public SimpleGenericClass<String> InstanceSimpleGenericClassField;

        public SimpleInterface InstanceSimpleInterfaceField;

        public static void SetStaticFields() {
            StaticByteField = 0;
            StaticSByteField = 1;
            StaticUInt16Field = 2;
            StaticInt16Field = 3;
            StaticUInt32Field = 4;
            StaticInt32Field = 5;
            StaticUInt64Field = 6;
            StaticInt64Field = 7;
            StaticDoubleField = 8;
            StaticSingleField = 9;
            StaticDecimalField = 10;
            StaticCharField = 'a';
            StaticBooleanField = true;
            StaticStringField = "testing";

            StaticObjectField = new SimpleStruct(1111);
            StaticEnumField = EnumInt64.B;
            StaticDateTimeField = new DateTime(50000);

            StaticSimpleStructField = new SimpleStruct(1234);
            StaticSimpleGenericStructField = new SimpleGenericStruct<ushort>(32);
            StaticNullableStructNotNullField = new SimpleStruct(56);
            StaticNullableStructNullField = null;

            StaticSimpleClassField = new SimpleClass(54);
            StaticSimpleGenericClassField = new SimpleGenericClass<string>("string");

            StaticSimpleInterfaceField = new ClassImplementSimpleInterface(87);
        }
        public void SetInstanceFields() {
            InstanceByteField = 0;
            InstanceSByteField = 1;
            InstanceUInt16Field = 2;
            InstanceInt16Field = 3;
            InstanceUInt32Field = 4;
            InstanceInt32Field = 5;
            InstanceUInt64Field = 6;
            InstanceInt64Field = 7;
            InstanceDoubleField = 8;
            InstanceSingleField = 9;
            InstanceDecimalField = 10;
            InstanceCharField = 'a';
            InstanceBooleanField = true;
            InstanceStringField = "testing";

            InstanceObjectField = new SimpleStruct(1111);
            InstanceEnumField = EnumInt64.B;
            InstanceDateTimeField = new DateTime(50000);

            InstanceSimpleStructField = new SimpleStruct(1234);
            InstanceSimpleGenericStructField = new SimpleGenericStruct<ushort>(32);
            InstanceNullableStructNotNullField = new SimpleStruct(56);
            InstanceNullableStructNullField = null;

            InstanceSimpleClassField = new SimpleClass(54);
            InstanceSimpleGenericClassField = new SimpleGenericClass<string>("string");

            InstanceSimpleInterfaceField = new ClassImplementSimpleInterface(87);
        }
    }
    public struct GenericStruct<T> {
        public static byte StaticByteField;
        public static sbyte StaticSByteField;
        public static ushort StaticUInt16Field;
        public static short StaticInt16Field;
        public static uint StaticUInt32Field;
        public static int StaticInt32Field;
        public static ulong StaticUInt64Field;
        public static long StaticInt64Field;
        public static double StaticDoubleField;
        public static float StaticSingleField;
        public static decimal StaticDecimalField;
        public static char StaticCharField;
        public static bool StaticBooleanField;
        public static string StaticStringField;

        public static object StaticObjectField;
        public static EnumInt64 StaticEnumField;
        public static DateTime StaticDateTimeField;

        public static SimpleStruct StaticSimpleStructField;
        public static SimpleGenericStruct<UInt16> StaticSimpleGenericStructField;
        public static Nullable<SimpleStruct> StaticNullableStructNotNullField;
        public static Nullable<SimpleStruct> StaticNullableStructNullField;

        public static SimpleClass StaticSimpleClassField;
        public static SimpleGenericClass<String> StaticSimpleGenericClassField;

        public static SimpleInterface StaticSimpleInterfaceField;

        public byte InstanceByteField;
        public sbyte InstanceSByteField;
        public ushort InstanceUInt16Field;
        public short InstanceInt16Field;
        public uint InstanceUInt32Field;
        public int InstanceInt32Field;
        public ulong InstanceUInt64Field;
        public long InstanceInt64Field;
        public double InstanceDoubleField;
        public float InstanceSingleField;
        public decimal InstanceDecimalField;
        public char InstanceCharField;
        public bool InstanceBooleanField;
        public string InstanceStringField;

        public object InstanceObjectField;
        public EnumInt64 InstanceEnumField;
        public DateTime InstanceDateTimeField;

        public SimpleStruct InstanceSimpleStructField;
        public SimpleGenericStruct<UInt16> InstanceSimpleGenericStructField;
        public Nullable<SimpleStruct> InstanceNullableStructNotNullField;
        public Nullable<SimpleStruct> InstanceNullableStructNullField;

        public SimpleClass InstanceSimpleClassField;
        public SimpleGenericClass<String> InstanceSimpleGenericClassField;

        public SimpleInterface InstanceSimpleInterfaceField;

        public static void SetStaticFields() {
            StaticByteField = 0;
            StaticSByteField = 1;
            StaticUInt16Field = 2;
            StaticInt16Field = 3;
            StaticUInt32Field = 4;
            StaticInt32Field = 5;
            StaticUInt64Field = 6;
            StaticInt64Field = 7;
            StaticDoubleField = 8;
            StaticSingleField = 9;
            StaticDecimalField = 10;
            StaticCharField = 'a';
            StaticBooleanField = true;
            StaticStringField = "testing";

            StaticObjectField = new SimpleStruct(1111);
            StaticEnumField = EnumInt64.B;
            StaticDateTimeField = new DateTime(50000);

            StaticSimpleStructField = new SimpleStruct(1234);
            StaticSimpleGenericStructField = new SimpleGenericStruct<ushort>(32);
            StaticNullableStructNotNullField = new SimpleStruct(56);
            StaticNullableStructNullField = null;

            StaticSimpleClassField = new SimpleClass(54);
            StaticSimpleGenericClassField = new SimpleGenericClass<string>("string");

            StaticSimpleInterfaceField = new ClassImplementSimpleInterface(87);
        }
        public void SetInstanceFields() {
            InstanceByteField = 0;
            InstanceSByteField = 1;
            InstanceUInt16Field = 2;
            InstanceInt16Field = 3;
            InstanceUInt32Field = 4;
            InstanceInt32Field = 5;
            InstanceUInt64Field = 6;
            InstanceInt64Field = 7;
            InstanceDoubleField = 8;
            InstanceSingleField = 9;
            InstanceDecimalField = 10;
            InstanceCharField = 'a';
            InstanceBooleanField = true;
            InstanceStringField = "testing";

            InstanceObjectField = new SimpleStruct(1111);
            InstanceEnumField = EnumInt64.B;
            InstanceDateTimeField = new DateTime(50000);

            InstanceSimpleStructField = new SimpleStruct(1234);
            InstanceSimpleGenericStructField = new SimpleGenericStruct<ushort>(32);
            InstanceNullableStructNotNullField = new SimpleStruct(56);
            InstanceNullableStructNullField = null;

            InstanceSimpleClassField = new SimpleClass(54);
            InstanceSimpleGenericClassField = new SimpleGenericClass<string>("string");

            InstanceSimpleInterfaceField = new ClassImplementSimpleInterface(87);
        }
    }

    public class Class {
        public static byte StaticByteField;
        public static sbyte StaticSByteField;
        public static ushort StaticUInt16Field;
        public static short StaticInt16Field;
        public static uint StaticUInt32Field;
        public static int StaticInt32Field;
        public static ulong StaticUInt64Field;
        public static long StaticInt64Field;
        public static double StaticDoubleField;
        public static float StaticSingleField;
        public static decimal StaticDecimalField;
        public static char StaticCharField;
        public static bool StaticBooleanField;
        public static string StaticStringField;

        public static object StaticObjectField;
        public static EnumInt64 StaticEnumField;
        public static DateTime StaticDateTimeField;

        public static SimpleStruct StaticSimpleStructField;
        public static SimpleGenericStruct<UInt16> StaticSimpleGenericStructField;
        public static Nullable<SimpleStruct> StaticNullableStructNotNullField;
        public static Nullable<SimpleStruct> StaticNullableStructNullField;

        public static SimpleClass StaticSimpleClassField;
        public static SimpleGenericClass<String> StaticSimpleGenericClassField;

        public static SimpleInterface StaticSimpleInterfaceField;

        public byte InstanceByteField;
        public sbyte InstanceSByteField;
        public ushort InstanceUInt16Field;
        public short InstanceInt16Field;
        public uint InstanceUInt32Field;
        public int InstanceInt32Field;
        public ulong InstanceUInt64Field;
        public long InstanceInt64Field;
        public double InstanceDoubleField;
        public float InstanceSingleField;
        public decimal InstanceDecimalField;
        public char InstanceCharField;
        public bool InstanceBooleanField;
        public string InstanceStringField;

        public object InstanceObjectField;
        public EnumInt64 InstanceEnumField;
        public DateTime InstanceDateTimeField;

        public SimpleStruct InstanceSimpleStructField;
        public SimpleGenericStruct<UInt16> InstanceSimpleGenericStructField;
        public Nullable<SimpleStruct> InstanceNullableStructNotNullField;
        public Nullable<SimpleStruct> InstanceNullableStructNullField;

        public SimpleClass InstanceSimpleClassField;
        public SimpleGenericClass<String> InstanceSimpleGenericClassField;

        public SimpleInterface InstanceSimpleInterfaceField;

        public static void SetStaticFields() {
            StaticByteField = 0;
            StaticSByteField = 1;
            StaticUInt16Field = 2;
            StaticInt16Field = 3;
            StaticUInt32Field = 4;
            StaticInt32Field = 5;
            StaticUInt64Field = 6;
            StaticInt64Field = 7;
            StaticDoubleField = 8;
            StaticSingleField = 9;
            StaticDecimalField = 10;
            StaticCharField = 'a';
            StaticBooleanField = true;
            StaticStringField = "testing";

            StaticObjectField = new SimpleStruct(1111);
            StaticEnumField = EnumInt64.B;
            StaticDateTimeField = new DateTime(50000);

            StaticSimpleStructField = new SimpleStruct(1234);
            StaticSimpleGenericStructField = new SimpleGenericStruct<ushort>(32);
            StaticNullableStructNotNullField = new SimpleStruct(56);
            StaticNullableStructNullField = null;

            StaticSimpleClassField = new SimpleClass(54);
            StaticSimpleGenericClassField = new SimpleGenericClass<string>("string");

            StaticSimpleInterfaceField = new ClassImplementSimpleInterface(87);
        }
        public void SetInstanceFields() {
            InstanceByteField = 0;
            InstanceSByteField = 1;
            InstanceUInt16Field = 2;
            InstanceInt16Field = 3;
            InstanceUInt32Field = 4;
            InstanceInt32Field = 5;
            InstanceUInt64Field = 6;
            InstanceInt64Field = 7;
            InstanceDoubleField = 8;
            InstanceSingleField = 9;
            InstanceDecimalField = 10;
            InstanceCharField = 'a';
            InstanceBooleanField = true;
            InstanceStringField = "testing";

            InstanceObjectField = new SimpleStruct(1111);
            InstanceEnumField = EnumInt64.B;
            InstanceDateTimeField = new DateTime(50000);

            InstanceSimpleStructField = new SimpleStruct(1234);
            InstanceSimpleGenericStructField = new SimpleGenericStruct<ushort>(32);
            InstanceNullableStructNotNullField = new SimpleStruct(56);
            InstanceNullableStructNullField = null;

            InstanceSimpleClassField = new SimpleClass(54);
            InstanceSimpleGenericClassField = new SimpleGenericClass<string>("string");

            InstanceSimpleInterfaceField = new ClassImplementSimpleInterface(87);
        }
    }
    public class GenericClass<T> {
        public static byte StaticByteField;
        public static sbyte StaticSByteField;
        public static ushort StaticUInt16Field;
        public static short StaticInt16Field;
        public static uint StaticUInt32Field;
        public static int StaticInt32Field;
        public static ulong StaticUInt64Field;
        public static long StaticInt64Field;
        public static double StaticDoubleField;
        public static float StaticSingleField;
        public static decimal StaticDecimalField;
        public static char StaticCharField;
        public static bool StaticBooleanField;
        public static string StaticStringField;

        public static object StaticObjectField;
        public static EnumInt64 StaticEnumField;
        public static DateTime StaticDateTimeField;

        public static SimpleStruct StaticSimpleStructField;
        public static SimpleGenericStruct<UInt16> StaticSimpleGenericStructField;
        public static Nullable<SimpleStruct> StaticNullableStructNotNullField;
        public static Nullable<SimpleStruct> StaticNullableStructNullField;

        public static SimpleClass StaticSimpleClassField;
        public static SimpleGenericClass<String> StaticSimpleGenericClassField;

        public static SimpleInterface StaticSimpleInterfaceField;

        public byte InstanceByteField;
        public sbyte InstanceSByteField;
        public ushort InstanceUInt16Field;
        public short InstanceInt16Field;
        public uint InstanceUInt32Field;
        public int InstanceInt32Field;
        public ulong InstanceUInt64Field;
        public long InstanceInt64Field;
        public double InstanceDoubleField;
        public float InstanceSingleField;
        public decimal InstanceDecimalField;
        public char InstanceCharField;
        public bool InstanceBooleanField;
        public string InstanceStringField;

        public object InstanceObjectField;
        public EnumInt64 InstanceEnumField;
        public DateTime InstanceDateTimeField;

        public SimpleStruct InstanceSimpleStructField;
        public SimpleGenericStruct<UInt16> InstanceSimpleGenericStructField;
        public Nullable<SimpleStruct> InstanceNullableStructNotNullField;
        public Nullable<SimpleStruct> InstanceNullableStructNullField;

        public SimpleClass InstanceSimpleClassField;
        public SimpleGenericClass<String> InstanceSimpleGenericClassField;

        public SimpleInterface InstanceSimpleInterfaceField;

        public static void SetStaticFields() {
            StaticByteField = 0;
            StaticSByteField = 1;
            StaticUInt16Field = 2;
            StaticInt16Field = 3;
            StaticUInt32Field = 4;
            StaticInt32Field = 5;
            StaticUInt64Field = 6;
            StaticInt64Field = 7;
            StaticDoubleField = 8;
            StaticSingleField = 9;
            StaticDecimalField = 10;
            StaticCharField = 'a';
            StaticBooleanField = true;
            StaticStringField = "testing";

            StaticObjectField = new SimpleStruct(1111);
            StaticEnumField = EnumInt64.B;
            StaticDateTimeField = new DateTime(50000);

            StaticSimpleStructField = new SimpleStruct(1234);
            StaticSimpleGenericStructField = new SimpleGenericStruct<ushort>(32);
            StaticNullableStructNotNullField = new SimpleStruct(56);
            StaticNullableStructNullField = null;

            StaticSimpleClassField = new SimpleClass(54);
            StaticSimpleGenericClassField = new SimpleGenericClass<string>("string");

            StaticSimpleInterfaceField = new ClassImplementSimpleInterface(87);
        }
        public void SetInstanceFields() {
            InstanceByteField = 0;
            InstanceSByteField = 1;
            InstanceUInt16Field = 2;
            InstanceInt16Field = 3;
            InstanceUInt32Field = 4;
            InstanceInt32Field = 5;
            InstanceUInt64Field = 6;
            InstanceInt64Field = 7;
            InstanceDoubleField = 8;
            InstanceSingleField = 9;
            InstanceDecimalField = 10;
            InstanceCharField = 'a';
            InstanceBooleanField = true;
            InstanceStringField = "testing";

            InstanceObjectField = new SimpleStruct(1111);
            InstanceEnumField = EnumInt64.B;
            InstanceDateTimeField = new DateTime(50000);

            InstanceSimpleStructField = new SimpleStruct(1234);
            InstanceSimpleGenericStructField = new SimpleGenericStruct<ushort>(32);
            InstanceNullableStructNotNullField = new SimpleStruct(56);
            InstanceNullableStructNullField = null;

            InstanceSimpleClassField = new SimpleClass(54);
            InstanceSimpleGenericClassField = new SimpleGenericClass<string>("string");

            InstanceSimpleInterfaceField = new ClassImplementSimpleInterface(87);
        }
    }

    public class DerivedClass : Class { }
    public class DerivedOpenGenericClass<T> : GenericClass<T> { }
    public class DerivedGenericClassOfInt32 : GenericClass<int> { }
    public class DerivedGenericClassOfObject : GenericClass<object> { }

    public struct Struct2 {
        public static int StaticField = 10;
        public static Struct2 StaticNextField;

        // this is not allowed 
        // public Struct2 InstanceNextField;
    }

    public class Class2 {
        public static int StaticField = 10;
        public static Class2 StaticNextField;

        public int InstanceField;
        public Class2 InstanceNextField;
    }

    public struct GenericStruct2<T> {
        public static T StaticTField;
        public static SimpleGenericClass<T> StaticClassTField;
        public static SimpleGenericStruct<T> StaticStructTField;

        public T InstanceTField;
        public SimpleGenericClass<T> InstanceClassTField;
        public SimpleGenericStruct<T> InstanceStructTField;

        public static int StaticField = 10;
        public static GenericStruct2<T> StaticNextField;

    }
    public class GenericClass2<T> {
        public static T StaticTField;
        public static SimpleGenericClass<T> StaticClassTField;
        public static SimpleGenericStruct<T> StaticStructTField;

        public T InstanceTField;
        public SimpleGenericClass<T> InstanceClassTField;
        public SimpleGenericStruct<T> InstanceStructTField;

        public static int StaticField = 10;
        public static GenericClass2<T> StaticNextField;

        public int InstanceField;
        public GenericClass2<T> InstanceNextField;
    }

    public class Misc {
        public int PublicField;
        protected int ProtectedField;
#pragma warning disable 414
        private int PrivateField;
#pragma warning restore

        public SimpleInterface InterfaceField;
        public SimpleInterface InternalInterfaceField;

        public void Set() {
            PublicField = 100;
            ProtectedField = 200;
            PrivateField = 300;
            InterfaceField = new ClassImplementSimpleInterface(0);
            InternalInterfaceField = new InternalClassImplementSimpleInterface();
        }
    }

    public class DerivedMisc : Misc {
        public new int PublicField = 400;
    }

    class InternalClassImplementSimpleInterface : SimpleInterface {
        public static int Flag = 500;
    }

}

