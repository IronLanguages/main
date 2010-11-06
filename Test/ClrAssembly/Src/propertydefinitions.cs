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
using Merlin.Testing.TypeSample;


namespace Merlin.Testing.Property {

    #region Explicitly implemented property
    public interface IData {
        int Number { get; set; }
    }

    public class ClassExplicitlyImplement : IData {
        private int _value;

        int IData.Number {
            get { return _value; }
            set { _value = value; }
        }
    }

    public struct StructExplicitlyImplement : IData {
        private int _value;

        int IData.Number {
            get { return _value; }
            set { _value = value; }
        }
    }

    public interface IReadOnlyData {
        string Number { get; }
    }

    public interface IWriteOnlyData {
        SimpleStruct Number { set; }
    }

    public class ClassExplicitlyReadOnly : IReadOnlyData {
        private string _iv = "python";

        string IReadOnlyData.Number {
            get { return _iv; }
        }
    }

    public struct StructExplicitlyWriteOnly : IWriteOnlyData {
        private SimpleStruct _iv;

        SimpleStruct IWriteOnlyData.Number {
            set { Flag.Set(10 + value.Flag); _iv = value; }
        }
    }
    #endregion

    #region ReadOnly/WriteOnly static/instance properties
    public class ClassWithReadOnly {
        private int _iv = 9;
        private static string _sv = "dlr";

        public int InstanceProperty {
            get { return _iv; }
        }
        public static string StaticProperty {
            get { return _sv; }
        }
    }

    public class ClassWithWriteOnly {
        private int _iv;
        private static string _sv;

        public int InstanceProperty {
            set { Flag.Set(11); _iv = value; }
        }

        public static string StaticProperty {
            set { Flag.Set(12); _sv = value; }
            //get { return "abc"; }
        }
    }

    public class ReadOnlyBase {
        public int Number {
            get { return 21; }
        }
    }

    public class WriteOnlyDerived : ReadOnlyBase {
        public new int Number {
            set { Flag.Set(value); }
        }
    }
    #endregion

    public class ClassWithProperties {
        private int _iv1;
        public int InstanceInt32Property {
            get { return _iv1; }
            set { _iv1 = value; }
        }

        private SimpleStruct _iv2;
        public SimpleStruct InstanceSimpleStructProperty {
            get { return _iv2; }
            set { _iv2 = value; }
        }

        private SimpleClass _iv3;
        public SimpleClass InstanceSimpleClassProperty {
            get { return _iv3; }
            set { _iv3 = value; }
        }

        private static int _sv1;
        public static int StaticInt32Property {
            get { return _sv1; }
            set { _sv1 = value; }
        }

        private static SimpleStruct _sv2;
        public static SimpleStruct StaticSimpleStructProperty {
            get { return _sv2; }
            set { _sv2 = value; }
        }

        private static SimpleClass _sv3;
        public static SimpleClass StaticSimpleClassProperty {
            get { return _sv3; }
            set { _sv3 = value; }
        }
    }

    public class DerivedClass : ClassWithProperties { }

    public struct StructWithProperties {
        private int _iv1;
        public int InstanceInt32Property {
            get { return _iv1; }
            set { _iv1 = value; }
        }

        public SimpleStruct _iv2;
        public SimpleStruct InstanceSimpleStructProperty {
            get { return _iv2; }
            set { _iv2 = value; }
        }

        private SimpleClass _iv3;
        public SimpleClass InstanceSimpleClassProperty {
            get { return _iv3; }
            set { _iv3 = value; }
        }

        private static int _sv1;
        public static int StaticInt32Property {
            get { return _sv1; }
            set { _sv1 = value; }
        }

        private static SimpleStruct _sv2;
        public static SimpleStruct StaticSimpleStructProperty {
            get { return _sv2; }
            set { _sv2 = value; }
        }

        private static SimpleClass _sv3;
        public static SimpleClass StaticSimpleClassProperty {
            get { return _sv3; }
            set { _sv3 = value; }
        }
    }
}