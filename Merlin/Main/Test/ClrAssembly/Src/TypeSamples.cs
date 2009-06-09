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

namespace Merlin.Testing.TypeSample {
    public enum EnumByte : byte {
        A, B, C
    }
    public enum EnumSByte : sbyte {
        A, B, C
    }
    public enum EnumUInt16 : ushort {
        A, B, C
    }
    public enum EnumInt16 : short {
        A, B, C
    }
    public enum EnumUInt32 : uint {
        A, B, C
    }
    public enum EnumInt32 : int {
        A, B, C
    }
    public enum EnumUInt64 : ulong {
        A, B, C
    }
    public enum EnumInt64 : long {
        A, B, C
    }

    public interface SimpleInterface { }
    public class ClassImplementSimpleInterface : SimpleInterface {
        private int _flag;
        public ClassImplementSimpleInterface(int arg) {
            _flag = arg;
        }
        public int Flag {
            get { return _flag; }
        }

        public static int PublicStaticField = 500;
    }

    public class SimpleClass {
        private int _flag;
        public SimpleClass(int arg) {
            _flag = arg;
        }
        public int Flag {
            get { return _flag; }
        }
    }

    public struct SimpleStruct {
        private int _flag;
        public SimpleStruct(int arg) {
            _flag = arg;
        }
        public int Flag {
            get { return _flag; }
        }
    }

    public class SimpleGenericClass<T> {
        private T _flag;
        public SimpleGenericClass(T arg) {
            _flag = arg;
        }
        public T Flag {
            get { return _flag; }
        }
    }

    public struct SimpleGenericStruct<K> {
        private K _flag;
        public SimpleGenericStruct(K arg) {
            _flag = arg;
        }
        public K Flag {
            get { return _flag; }
        }
    }

    public class ClassWithDefaultCtor {
        int _flag;

        public ClassWithDefaultCtor() {
            _flag = 41;
        }

        public int Flag {
            get { return _flag; }
        }
    }
    public struct StructWithDefaultCtor {
        public int Flag {
            get { return 42; }
        }
    }

    public delegate void VoidVoidDelegate();
    public delegate Int32 Int32Int32Delegate(Int32 arg);
}
