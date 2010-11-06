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

namespace Merlin.Testing.Event {
    public delegate Int32 Int32Int32Delegate(Int32 arg);

    public class TargetClass {
        public static Int32 s_Double(Int32 arg) { Flag.Add(1); return arg * 2; }
        public static Int32 s_Negate(Int32 arg) { Flag.Add(10); return -arg; }
        public static Int32 s_Square(Int32 arg) { Flag.Add(100); return arg * arg; }
        public static Int32 s_Throw(Int32 arg) { throw new ApplicationException(); }

        public Int32 i_Double(Int32 arg) { Flag.Add(1); return arg * 2; }
        public Int32 i_Negate(Int32 arg) { Flag.Add(10); return -arg; }
        public Int32 i_Square(Int32 arg) { Flag.Add(100); return arg * arg; }
        public Int32 i_Throw(Int32 arg) { throw new ApplicationException(); }
    }

    public struct TargetStruct {
        public static Int32 s_Double(Int32 arg) { Flag.Add(1); return arg * 2; }
        public static Int32 s_Negate(Int32 arg) { Flag.Add(10); return -arg; }
        public static Int32 s_Square(Int32 arg) { Flag.Add(100); return arg * arg; }
        public static Int32 s_Throw(Int32 arg) { throw new ApplicationException(); }

        public Int32 i_Double(Int32 arg) { Flag.Add(1); return arg * 2; }
        public Int32 i_Negate(Int32 arg) { Flag.Add(10); return -arg; }
        public Int32 i_Square(Int32 arg) { Flag.Add(100); return arg * arg; }
        public Int32 i_Throw(Int32 arg) { throw new ApplicationException(); }
    }

    public interface IInterface {
        event Int32Int32Delegate OnAction;
    }

    public struct StructImplicitlyImplementInterface : IInterface {
        public event Int32Int32Delegate OnAction;

        public Int32 CallInside(Int32 arg) {
            if (OnAction != null) {
                return OnAction(arg);
            } else {
                return -1;
            }
        }
    }

    public class ClassImplicitlyImplementInterface : IInterface {
        public event Int32Int32Delegate OnAction;

        public Int32 CallInside(Int32 arg) {
            if (OnAction != null) {
                return OnAction(arg);
            } else {
                return -1;
            }
        }
    }

    public struct StructWithSimpleEvent {
        public event Int32Int32Delegate OnAction;

        public Int32 CallInside(Int32 arg) {
            if (OnAction != null) {
                return OnAction(arg);
            } else {
                return -1;
            }
        }
    }
    public class ClassWithSimpleEvent {
        public event Int32Int32Delegate OnAction;

        public Int32 CallInside(Int32 arg) {
            if (OnAction != null) {
                return OnAction(arg);
            } else {
                return -1;
            }
        }
    }

    public struct StructExplicitlyImplementInterface : IInterface {
        private Int32Int32Delegate _private;
        event Int32Int32Delegate IInterface.OnAction {
            add { _private += value; }
            remove { _private -= value; }
        }
    }

    public class ClassExplicitlyImplementInterface : IInterface {
        private Int32Int32Delegate _private;
        event Int32Int32Delegate IInterface.OnAction {
            add { _private += value; }
            remove { _private -= value; }
        }
    }

    public class ClassWithStaticEvent {
        public static event Int32Int32Delegate OnAction;

        public Int32 CallInside(Int32 arg) {
            if (OnAction != null) {
                return OnAction(arg);
            } else {
                return -1;
            }
        }
    }
    public struct StructWithStaticEvent {
        public static event Int32Int32Delegate OnAction;

        public Int32 CallInside(Int32 arg) {
            if (OnAction != null) {
                return OnAction(arg);
            } else {
                return -1;
            }
        }
    }

    public class DerivedClassWithStaticEvent : ClassWithStaticEvent { }

    //public class ClassWithAddOnlyEvent {
    //    private Int32Int32Delegate _private;
    //    public event Int32Int32Delegate OnAction {
    //        add { _private += value; }
    //    }
    //}

    //public class ClassWithRemoveOnlyEvent {
    //    private Int32Int32Delegate _private;
    //    public event Int32Int32Delegate OnAction {
    //        remove { _private -= value; }
    //    }
    //}

    //public class ClassWithPrivateAddPublicRemoveEvent {
    //    public event Int32Int32Delegate OnAction {
    //        add { }
    //        remove { }
    //    }
    //}
}