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

//TODO - Parts of this file should NOT be under ClrAssembly as it has dependencies on the DLR!

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Merlin.Testing.TypeSample;
using Microsoft.Scripting;

namespace Merlin.Testing.Call {
    public class VariousParameters {
        // no parameter
        public void M100() { Flag.Set(10); }

        // one parameter
        public void M200(int arg) { Flag.Set(arg); }
        public void M201([DefaultParameterValue(20)] int arg) { Flag.Set(arg); }
        public void M202(params int[] arg) { Flag.Set(arg.Length); }
        public void M203([ParamDictionaryAttribute] IDictionary<object, object> arg) { Flag.Set(arg.Count); }

        // optional (get missing value)
        // - check the value actually passed in
        public void M231([Optional] int arg) { Flag.Set(arg); }  // not reset any
        public void M232([Optional] bool arg) { Flag<bool>.Set(arg); }
        public void M233([Optional] object arg) { Flag<object>.Set(arg); }
        public void M234([Optional] string arg) { Flag<string>.Set(arg); }
        public void M235([Optional] EnumInt32 arg) { Flag<EnumInt32>.Set(arg); }
        public void M236([Optional] SimpleClass arg) { Flag<SimpleClass>.Set(arg); }
        public void M237([Optional] SimpleStruct arg) { Flag<SimpleStruct>.Set(arg); }

        // two parameters
        public void M300(int x, int y) { }
        public void M350(int x, params int[] y) { }
        public void M351(int x, [ParamDictionary] IDictionary<object, object> arg) { Flag<object>.Set(arg); }
        public void M352([ParamDictionary] IDictionary<object, object> arg, params int[] x) { Flag<object>.Set(arg); }

        public void M310(int x, [DefaultParameterValue(30)]int y) { Flag.Set(x + y); }
        public void M320([DefaultParameterValue(40)] int y, int x) { Flag.Set(x + y); }
        public void M330([DefaultParameterValue(50)] int x, [DefaultParameterValue(60)] int y) { Flag.Set(x + y); }

        public void M410(int x, [Optional]int y) { }
        public void M420([Optional] int y, int x) { }
        public void M430([Optional] int x, [Optional] int y) { }

        // three parameters
        public void M500(int x, int y, int z) { Flag.Set(x * 100 + y * 10 + z); }
        public void M510(int x, int y, [DefaultParameterValue(70)] int z) { Flag.Set(x * 100 + y * 10 + z); }
        public void M520(int x, [DefaultParameterValue(80)]int y, int z) { Flag.Set(x * 100 + y * 10 + z); }
        public void M530([DefaultParameterValue(90)]int x, int y, int z) { Flag.Set(x * 100 + y * 10 + z); }
        public void M550(int x, int y, params int[] z) { Flag.Set(x * 100 + y * 10 + z.Length); }

        // long paramater list
        public void M650(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10) {
            Flag.Reset();
            Flag<string>.Set(string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10));
        }

        // try this with keyword args, unpack tuple
        public void M700(int arg1, string arg2, bool arg3, object arg4, EnumInt16 arg5, SimpleClass arg6, SimpleStruct arg7) { }

        // keyword argument name, or **dict style
        public void M800(int True) { Flag.Set(True); }
        public void M801(int def) { Flag.Set(def); }
    }

    public class ByRefParameters {
        // 1 argument
        public void M100(ref int arg) { Flag.Set(arg); arg = 1; }
        public void M120(out int arg) { arg = 2; }

        // 2 arguments
        public void M200(int arg1, ref int arg2) { Flag.Set(arg1 * 10 + arg2); arg2 = 10; }
        public void M201(ref int arg1, int arg2) { Flag.Set(arg1 * 10 + arg2); arg1 = 20; }
        public void M202(ref int arg1, ref int arg2) { Flag.Set(arg1 * 10 + arg2); arg1 = 30; arg2 = 40; }

        public void M203(int arg1, out int arg2) { Flag.Set(arg1 * 10); arg2 = 50; }
        public void M204(out int arg1, int arg2) { Flag.Set(arg2); arg1 = 60; }
        public void M205(out int arg1, out int arg2) { arg1 = 70; arg2 = 80; }
        public void M206(ref int arg1, out int arg2) { Flag.Set(arg1 * 10); arg1 = 10; arg2 = 20; }
        public void M207(out int arg1, ref int arg2) { Flag.Set(arg2); arg1 = 30; arg2 = 40; }

        // 3 arguments
        public void M300(int arg1, ref int arg2, out int arg3) { arg3 = arg2 = 10; }
        public void M301(int arg1, out int arg2, ref int arg3) { arg3 = arg2 = 10; }
        public void M302(ref int arg1, int arg2, out int arg3) { arg1 = arg3 = 10; }
        public void M303(out int arg1, ref int arg2, int arg3) { arg1 = arg2 = 10; }

        // mixed 
        public void M400(ref int arg1, params int[] arg2) { arg1 = 10; }
        public void M401(out int arg1, params int[] arg2) { arg1 = 10; }

        public void M450(ref int arg1, [ParamDictionary] IDictionary<object, object> arg2) { arg1 = 10; }
        public void M451(ref int arg1, [ParamDictionary] IDictionary<object, object> arg2) { arg1 = 10; }

        public void M500(ref int arg1, [DefaultParameterValue(10)] int arg2) { arg1 = 10; }
        public void M501([DefaultParameterValue(10)] int arg2, ref int arg1) { arg1 = 10; }

        public void M510([DefaultParameterValue(10)] ref int arg1) { arg1 = 10; }
        public void M511([DefaultParameterValue(10)] out int arg1) { arg1 = 10; }
    }

    public class InOutParameters {
        public void M100([In] int arg) { }
        public void M110([Out] int arg) { }
        public void M120([In, Out] int arg) { }
    }

    // Constructor
    // 1 argument 
    public class Ctor100 {
        public Ctor100(int arg) { }
    }
    public class Ctor101 {
        public Ctor101([DefaultParameterValue(10)]int arg) { }
    }
    public class Ctor102 {
        public Ctor102([Optional]int arg) { }
    }
    public class Ctor103 {
        public Ctor103(params int[] arg) { }
    }
    public class Ctor104 {
        public Ctor104(object[] arg) { }
    }
    public class Ctor105 {
        public Ctor105([ParamDictionary] IDictionary<object, object> arg) { }
    }

    public class Ctor110 {
        public Ctor110(ref int arg) { arg = 10; }
    }
    public class Ctor111 {
        public Ctor111(out int arg) { arg = 10; }
    }

    // 2 arguments
    public class Ctor200 {
        public Ctor200(int x, int y) { }
    }

    public class Ctor210 {
        public Ctor210(int x, [DefaultParameterValue(10)]int y) { }
    }

    public class Ctor220 {
        public Ctor220(int x, params int[] y) { }
    }

    public class Ctor230 {
        public Ctor230([DefaultParameterValue(10)]int x, int y) { }
    }

    public class Ctor240 {
        public Ctor240([Optional]int x, int y) { }
    }

    public class Ctor250 {
        public Ctor250(int x, [ParamDictionary] IDictionary<object, object> y) { }
    }

    // argument types
    public class Ctor500 {
        public Ctor500(int arg1, bool arg2, string arg3, object arg4, EnumInt32 arg5, SimpleClass arg6, SimpleStruct arg7) { }
    }

    // keyword arguments for constructor call
    public class Ctor600 {
        public int IntField;

        private int _intProperty;
        public int IntProperty {
            get { return _intProperty; }
            set { _intProperty = value; }
        }

        public bool BoolField;
        private bool _boolProperty;

        public bool BoolProperty {
            get { return _boolProperty; }
            set { _boolProperty = value; }
        }

        public string StringField;

        private string _stringProperty;
        public string StringProperty {
            get { return _stringProperty; }
            set { _stringProperty = value; }
        }

        public object ObjectField;

        private object _objectProperty;

        public object ObjectProperty {
            get { return _objectProperty; }
            set { _objectProperty = value; }
        }

        public EnumInt64 EnumField;
        private EnumUInt64 _enumProperty;

        public EnumUInt64 EnumProperty {
            get { return _enumProperty; }
            set { _enumProperty = value; }
        }

        public SimpleClass ClassField;
        private SimpleClass _classProperty;

        public SimpleClass ClassProperty {
            get { return _classProperty; }
            set { _classProperty = value; }
        }

        public SimpleStruct StructField;

        private SimpleStruct _structProperty;

        public SimpleStruct StructProperty {
            get { return _structProperty; }
            set { _structProperty = value; }
        }
    }

    // use keyword arg for arg1/arg2 too
    public class Ctor610 {
        public Ctor610(int arg1, int arg2) {
            Flag<int, int, int>.Value1 = arg1;
            Flag<int, int, int>.Value2 = arg2;
        }

        private int _arg3;
        public int Arg3 {
            get { return _arg3; }
            set { Flag<int, int, int>.Value3 = _arg3 = value; }
        }

        public int Arg4;
    }

    // parameter name is same as property
    public class Ctor620 {
        public Ctor620(int arg1) {
            Flag<int, int, int, string>.Value1 = arg1;
        }

        public Ctor620(int arg1, int arg2) {
            Flag<int, int, int, string>.Value1 = arg1;
            Flag<int, int, int, string>.Value2 = arg2;
        }

        // different type
        private int _arg1;

        public int arg1 {
            get { return _arg1; }
            set { Flag<int, int, int, string>.Value3 = _arg1 = value; }
        }

        // same type
        private string _arg2;

        public string arg2 {
            get { return _arg2; }
            set { Flag<int, int, int, string>.Value4 = _arg2 = value; }
        }
    }

    // readonly instance property
    public class Ctor700 {
        public Ctor700(int arg1) {
        }

#pragma warning disable 649
        private int _readonlyProperty;
#pragma warning restore 

        public int ReadOnlyProperty {
            get { return _readonlyProperty; }
        }
    }

    // static field
    public class Ctor710 {
        public static int StaticField = 10;
    }

    public class Ctor720 {
        public static readonly int ReadOnlyField = 20;
    }

    public class Ctor730 {
        public const int LiteralField = 30;
    }

    // static property
    public class Ctor750 {
        private static int _staticProperty;

        public static int StaticProperty {
            get { return _staticProperty; }
            set { _staticProperty = value; }
        }
    }

    public class Ctor760 {
        public void InstanceMethod() { }
        public EventHandler MyEvent;
    }

    // python special
    public struct Struct {
        public int IntField;
        public string StringField;
        public object ObjectField;
    }
}