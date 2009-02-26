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
namespace Merlin.Testing {
    public class Helper {
        public static int Sum(int[] args) {
            int sum = 0;
            foreach (int x in args) sum += x;
            return sum;
        }
    }

    public class Flag {
        public static int Value;

        public static void SetOnly(int value) {
            Value = value;
        }
        public static void Set(int value) {
            Reset();
            SetOnly(value);
        }

        public static void CheckOnly(int expected) {
            if (Value != expected) {
                throw new Exception(string.Format("expected: {0}, actual: {1}", expected, Value));
            }
        }

        public static void Check(int expected) {
            CheckOnly(expected);
            Reset();
        }
        public static void Reset() {
            Value = 999;
        }

        public static void Add(int value) {
            Value += value;
        }
    }


    #region generated code
    public class Flag<T1> {
        public static T1 Value1;
        public static void Set(T1 value1) {
            Value1 = value1;
        }
        public static void Reset() {
            Value1 = default(T1);
        }
        public static void Check(T1 value1) {
            if (Value1 != null || value1 != null) {
                if (Value1 == null && value1 != null) throw new Exception(string.Format("#0 - expected {0}, actual: null", value1));
                else if (!Value1.Equals(value1)) throw new Exception(string.Format("#0 - expected: {0}, actual: {1}", value1, Value1));
            }
        }
    }

    public class Flag<T1, T2> {
        public static T1 Value1;
        public static T2 Value2;
        public static void Set(T1 value1, T2 value2) {
            Value1 = value1;
            Value2 = value2;
        }
        public static void Reset() {
            Value1 = default(T1);
            Value2 = default(T2);
        }
        public static void Check(T1 value1, T2 value2) {
            if (Value1 != null || value1 != null) {
                if (Value1 == null && value1 != null) throw new Exception(string.Format("#0 - expected {0}, actual: null", value1));
                else if (!Value1.Equals(value1)) throw new Exception(string.Format("#0 - expected: {0}, actual: {1}", value1, Value1));
            }
            if (Value2 != null || value2 != null) {
                if (Value2 == null && value2 != null) throw new Exception(string.Format("#1 - expected {0}, actual: null", value2));
                else if (!Value2.Equals(value2)) throw new Exception(string.Format("#1 - expected: {0}, actual: {1}", value2, Value2));
            }
        }
    }

    public class Flag<T1, T2, T3> {
        public static T1 Value1;
        public static T2 Value2;
        public static T3 Value3;
        public static void Set(T1 value1, T2 value2, T3 value3) {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
        }
        public static void Reset() {
            Value1 = default(T1);
            Value2 = default(T2);
            Value3 = default(T3);
        }
        public static void Check(T1 value1, T2 value2, T3 value3) {
            if (Value1 != null || value1 != null) {
                if (Value1 == null && value1 != null) throw new Exception(string.Format("#0 - expected {0}, actual: null", value1));
                else if (!Value1.Equals(value1)) throw new Exception(string.Format("#0 - expected: {0}, actual: {1}", value1, Value1));
            }
            if (Value2 != null || value2 != null) {
                if (Value2 == null && value2 != null) throw new Exception(string.Format("#1 - expected {0}, actual: null", value2));
                else if (!Value2.Equals(value2)) throw new Exception(string.Format("#1 - expected: {0}, actual: {1}", value2, Value2));
            }
            if (Value3 != null || value3 != null) {
                if (Value3 == null && value3 != null) throw new Exception(string.Format("#2 - expected {0}, actual: null", value3));
                else if (!Value3.Equals(value3)) throw new Exception(string.Format("#2 - expected: {0}, actual: {1}", value3, Value3));
            }
        }
    }

    public class Flag<T1, T2, T3, T4> {
        public static T1 Value1;
        public static T2 Value2;
        public static T3 Value3;
        public static T4 Value4;
        public static void Set(T1 value1, T2 value2, T3 value3, T4 value4) {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
        }
        public static void Reset() {
            Value1 = default(T1);
            Value2 = default(T2);
            Value3 = default(T3);
            Value4 = default(T4);
        }
        public static void Check(T1 value1, T2 value2, T3 value3, T4 value4) {
            if (Value1 != null || value1 != null) {
                if (Value1 == null && value1 != null) throw new Exception(string.Format("#0 - expected {0}, actual: null", value1));
                else if (!Value1.Equals(value1)) throw new Exception(string.Format("#0 - expected: {0}, actual: {1}", value1, Value1));
            }
            if (Value2 != null || value2 != null) {
                if (Value2 == null && value2 != null) throw new Exception(string.Format("#1 - expected {0}, actual: null", value2));
                else if (!Value2.Equals(value2)) throw new Exception(string.Format("#1 - expected: {0}, actual: {1}", value2, Value2));
            }
            if (Value3 != null || value3 != null) {
                if (Value3 == null && value3 != null) throw new Exception(string.Format("#2 - expected {0}, actual: null", value3));
                else if (!Value3.Equals(value3)) throw new Exception(string.Format("#2 - expected: {0}, actual: {1}", value3, Value3));
            }
            if (Value4 != null || value4 != null) {
                if (Value4 == null && value4 != null) throw new Exception(string.Format("#3 - expected {0}, actual: null", value4));
                else if (!Value4.Equals(value4)) throw new Exception(string.Format("#3 - expected: {0}, actual: {1}", value4, Value4));
            }
        }
    }

    public class Flag<T1, T2, T3, T4, T5> {
        public static T1 Value1;
        public static T2 Value2;
        public static T3 Value3;
        public static T4 Value4;
        public static T5 Value5;
        public static void Set(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5) {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
            Value5 = value5;
        }
        public static void Reset() {
            Value1 = default(T1);
            Value2 = default(T2);
            Value3 = default(T3);
            Value4 = default(T4);
            Value5 = default(T5);
        }
        public static void Check(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5) {
            if (Value1 != null || value1 != null) {
                if (Value1 == null && value1 != null) throw new Exception(string.Format("#0 - expected {0}, actual: null", value1));
                else if (!Value1.Equals(value1)) throw new Exception(string.Format("#0 - expected: {0}, actual: {1}", value1, Value1));
            }
            if (Value2 != null || value2 != null) {
                if (Value2 == null && value2 != null) throw new Exception(string.Format("#1 - expected {0}, actual: null", value2));
                else if (!Value2.Equals(value2)) throw new Exception(string.Format("#1 - expected: {0}, actual: {1}", value2, Value2));
            }
            if (Value3 != null || value3 != null) {
                if (Value3 == null && value3 != null) throw new Exception(string.Format("#2 - expected {0}, actual: null", value3));
                else if (!Value3.Equals(value3)) throw new Exception(string.Format("#2 - expected: {0}, actual: {1}", value3, Value3));
            }
            if (Value4 != null || value4 != null) {
                if (Value4 == null && value4 != null) throw new Exception(string.Format("#3 - expected {0}, actual: null", value4));
                else if (!Value4.Equals(value4)) throw new Exception(string.Format("#3 - expected: {0}, actual: {1}", value4, Value4));
            }
            if (Value5 != null || value5 != null) {
                if (Value5 == null && value5 != null) throw new Exception(string.Format("#4 - expected {0}, actual: null", value5));
                else if (!Value5.Equals(value5)) throw new Exception(string.Format("#4 - expected: {0}, actual: {1}", value5, Value5));
            }
        }
    }

    public class Flag<T1, T2, T3, T4, T5, T6> {
        public static T1 Value1;
        public static T2 Value2;
        public static T3 Value3;
        public static T4 Value4;
        public static T5 Value5;
        public static T6 Value6;
        public static void Set(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6) {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
            Value5 = value5;
            Value6 = value6;
        }
        public static void Reset() {
            Value1 = default(T1);
            Value2 = default(T2);
            Value3 = default(T3);
            Value4 = default(T4);
            Value5 = default(T5);
            Value6 = default(T6);
        }
        public static void Check(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6) {
            if (Value1 != null || value1 != null) {
                if (Value1 == null && value1 != null) throw new Exception(string.Format("#0 - expected {0}, actual: null", value1));
                else if (!Value1.Equals(value1)) throw new Exception(string.Format("#0 - expected: {0}, actual: {1}", value1, Value1));
            }
            if (Value2 != null || value2 != null) {
                if (Value2 == null && value2 != null) throw new Exception(string.Format("#1 - expected {0}, actual: null", value2));
                else if (!Value2.Equals(value2)) throw new Exception(string.Format("#1 - expected: {0}, actual: {1}", value2, Value2));
            }
            if (Value3 != null || value3 != null) {
                if (Value3 == null && value3 != null) throw new Exception(string.Format("#2 - expected {0}, actual: null", value3));
                else if (!Value3.Equals(value3)) throw new Exception(string.Format("#2 - expected: {0}, actual: {1}", value3, Value3));
            }
            if (Value4 != null || value4 != null) {
                if (Value4 == null && value4 != null) throw new Exception(string.Format("#3 - expected {0}, actual: null", value4));
                else if (!Value4.Equals(value4)) throw new Exception(string.Format("#3 - expected: {0}, actual: {1}", value4, Value4));
            }
            if (Value5 != null || value5 != null) {
                if (Value5 == null && value5 != null) throw new Exception(string.Format("#4 - expected {0}, actual: null", value5));
                else if (!Value5.Equals(value5)) throw new Exception(string.Format("#4 - expected: {0}, actual: {1}", value5, Value5));
            }
            if (Value6 != null || value6 != null) {
                if (Value6 == null && value6 != null) throw new Exception(string.Format("#5 - expected {0}, actual: null", value6));
                else if (!Value6.Equals(value6)) throw new Exception(string.Format("#5 - expected: {0}, actual: {1}", value6, Value6));
            }
        }
    }

    public class Flag<T1, T2, T3, T4, T5, T6, T7> {
        public static T1 Value1;
        public static T2 Value2;
        public static T3 Value3;
        public static T4 Value4;
        public static T5 Value5;
        public static T6 Value6;
        public static T7 Value7;
        public static void Set(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7) {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
            Value5 = value5;
            Value6 = value6;
            Value7 = value7;
        }
        public static void Reset() {
            Value1 = default(T1);
            Value2 = default(T2);
            Value3 = default(T3);
            Value4 = default(T4);
            Value5 = default(T5);
            Value6 = default(T6);
            Value7 = default(T7);
        }
        public static void Check(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7) {
            if (Value1 != null || value1 != null) {
                if (Value1 == null && value1 != null) throw new Exception(string.Format("#0 - expected {0}, actual: null", value1));
                else if (!Value1.Equals(value1)) throw new Exception(string.Format("#0 - expected: {0}, actual: {1}", value1, Value1));
            }
            if (Value2 != null || value2 != null) {
                if (Value2 == null && value2 != null) throw new Exception(string.Format("#1 - expected {0}, actual: null", value2));
                else if (!Value2.Equals(value2)) throw new Exception(string.Format("#1 - expected: {0}, actual: {1}", value2, Value2));
            }
            if (Value3 != null || value3 != null) {
                if (Value3 == null && value3 != null) throw new Exception(string.Format("#2 - expected {0}, actual: null", value3));
                else if (!Value3.Equals(value3)) throw new Exception(string.Format("#2 - expected: {0}, actual: {1}", value3, Value3));
            }
            if (Value4 != null || value4 != null) {
                if (Value4 == null && value4 != null) throw new Exception(string.Format("#3 - expected {0}, actual: null", value4));
                else if (!Value4.Equals(value4)) throw new Exception(string.Format("#3 - expected: {0}, actual: {1}", value4, Value4));
            }
            if (Value5 != null || value5 != null) {
                if (Value5 == null && value5 != null) throw new Exception(string.Format("#4 - expected {0}, actual: null", value5));
                else if (!Value5.Equals(value5)) throw new Exception(string.Format("#4 - expected: {0}, actual: {1}", value5, Value5));
            }
            if (Value6 != null || value6 != null) {
                if (Value6 == null && value6 != null) throw new Exception(string.Format("#5 - expected {0}, actual: null", value6));
                else if (!Value6.Equals(value6)) throw new Exception(string.Format("#5 - expected: {0}, actual: {1}", value6, Value6));
            }
            if (Value7 != null || value7 != null) {
                if (Value7 == null && value7 != null) throw new Exception(string.Format("#6 - expected {0}, actual: null", value7));
                else if (!Value7.Equals(value7)) throw new Exception(string.Format("#6 - expected: {0}, actual: {1}", value7, Value7));
            }
        }
    }

    public class Flag<T1, T2, T3, T4, T5, T6, T7, T8> {
        public static T1 Value1;
        public static T2 Value2;
        public static T3 Value3;
        public static T4 Value4;
        public static T5 Value5;
        public static T6 Value6;
        public static T7 Value7;
        public static T8 Value8;
        public static void Set(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8) {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
            Value5 = value5;
            Value6 = value6;
            Value7 = value7;
            Value8 = value8;
        }
        public static void Reset() {
            Value1 = default(T1);
            Value2 = default(T2);
            Value3 = default(T3);
            Value4 = default(T4);
            Value5 = default(T5);
            Value6 = default(T6);
            Value7 = default(T7);
            Value8 = default(T8);
        }
        public static void Check(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8) {
            if (Value1 != null || value1 != null) {
                if (Value1 == null && value1 != null) throw new Exception(string.Format("#0 - expected {0}, actual: null", value1));
                else if (!Value1.Equals(value1)) throw new Exception(string.Format("#0 - expected: {0}, actual: {1}", value1, Value1));
            }
            if (Value2 != null || value2 != null) {
                if (Value2 == null && value2 != null) throw new Exception(string.Format("#1 - expected {0}, actual: null", value2));
                else if (!Value2.Equals(value2)) throw new Exception(string.Format("#1 - expected: {0}, actual: {1}", value2, Value2));
            }
            if (Value3 != null || value3 != null) {
                if (Value3 == null && value3 != null) throw new Exception(string.Format("#2 - expected {0}, actual: null", value3));
                else if (!Value3.Equals(value3)) throw new Exception(string.Format("#2 - expected: {0}, actual: {1}", value3, Value3));
            }
            if (Value4 != null || value4 != null) {
                if (Value4 == null && value4 != null) throw new Exception(string.Format("#3 - expected {0}, actual: null", value4));
                else if (!Value4.Equals(value4)) throw new Exception(string.Format("#3 - expected: {0}, actual: {1}", value4, Value4));
            }
            if (Value5 != null || value5 != null) {
                if (Value5 == null && value5 != null) throw new Exception(string.Format("#4 - expected {0}, actual: null", value5));
                else if (!Value5.Equals(value5)) throw new Exception(string.Format("#4 - expected: {0}, actual: {1}", value5, Value5));
            }
            if (Value6 != null || value6 != null) {
                if (Value6 == null && value6 != null) throw new Exception(string.Format("#5 - expected {0}, actual: null", value6));
                else if (!Value6.Equals(value6)) throw new Exception(string.Format("#5 - expected: {0}, actual: {1}", value6, Value6));
            }
            if (Value7 != null || value7 != null) {
                if (Value7 == null && value7 != null) throw new Exception(string.Format("#6 - expected {0}, actual: null", value7));
                else if (!Value7.Equals(value7)) throw new Exception(string.Format("#6 - expected: {0}, actual: {1}", value7, Value7));
            }
            if (Value8 != null || value8 != null) {
                if (Value8 == null && value8 != null) throw new Exception(string.Format("#7 - expected {0}, actual: null", value8));
                else if (!Value8.Equals(value8)) throw new Exception(string.Format("#7 - expected: {0}, actual: {1}", value8, Value8));
            }
        }
    }

    #endregion
}
