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
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using Merlin.Testing;
using Merlin.Testing.TypeSample;

namespace Merlin.Testing.Call {
    public class AllOpsClass {
        int _value;
        public AllOpsClass(int val) {
            _value = val;
        }

        public int Value {
            get { return _value; }
        }

        // unary 
        public static bool operator true(AllOpsClass self) {
            Flag.Set(100);
            return self._value >= 0;
        }
        public static bool operator false(AllOpsClass self) {
            Flag.Set(110);
            return self._value < 0;
        }

        public static bool operator !(AllOpsClass self) {
            Flag.Set(130);
            return !(self._value >= 0);
        }

        public static AllOpsClass operator ~(AllOpsClass self) {
            Flag.Set(140);
            return new AllOpsClass(~self._value);
        }

        public static AllOpsClass operator ++(AllOpsClass self) {
            Flag.Set(150);
            return new AllOpsClass(self._value + 1);
        }
        public static AllOpsClass operator --(AllOpsClass self) {
            Flag.Set(160);
            return new AllOpsClass(self._value - 1);
        }

        [SpecialName]
        public static AllOpsClass op_UnaryNegation(AllOpsClass self) {
            Flag.Set(170);
            return new AllOpsClass(-self._value);
        }

        [SpecialName]
        public static AllOpsClass op_UnaryPlus(AllOpsClass self) {
            Flag.Set(180);
            return new AllOpsClass(self._value);
        }

        // binary
        public static AllOpsClass operator +(AllOpsClass self, AllOpsClass other) {
            Flag.Set(200);
            return new AllOpsClass(self._value + other._value);
        }
        public static AllOpsClass operator -(AllOpsClass self, AllOpsClass other) {
            Flag.Set(210);
            return new AllOpsClass(self._value - other._value);
        }
        public static AllOpsClass operator *(AllOpsClass self, AllOpsClass other) {
            Flag.Set(220);
            return new AllOpsClass(self._value * other._value);
        }
        public static AllOpsClass operator /(AllOpsClass self, AllOpsClass other) {
            Flag.Set(230);
            return new AllOpsClass(self._value / other._value);
        }
        public static AllOpsClass operator %(AllOpsClass self, AllOpsClass other) {
            Flag.Set(240);
            return new AllOpsClass(self._value % other._value);
        }
        public static AllOpsClass operator ^(AllOpsClass self, AllOpsClass other) {
            Flag.Set(250);
            return new AllOpsClass(self._value ^ other._value);
        }
        public static AllOpsClass operator &(AllOpsClass self, AllOpsClass other) {
            Flag.Set(260);
            return new AllOpsClass(self._value & other._value);
        }
        public static AllOpsClass operator |(AllOpsClass self, AllOpsClass other) {
            Flag.Set(270);
            return new AllOpsClass(self._value | other._value);
        }
        public static AllOpsClass operator <<(AllOpsClass self, int other) {
            Flag.Set(280);
            return new AllOpsClass(self._value << other);
        }
        public static AllOpsClass operator >>(AllOpsClass self, int other) {
            Flag.Set(290);
            return new AllOpsClass(self._value >> other);
        }

        public static bool operator ==(AllOpsClass self, AllOpsClass other) {
            Flag.Set(300);
            return self._value == other._value;
        }

        public static bool operator >(AllOpsClass self, AllOpsClass other) {
            Flag.Set(310);
            return self._value > other._value;
        }
        public static bool operator <(AllOpsClass self, AllOpsClass other) {
            Flag.Set(320);
            return self._value < other._value;
        }
        public static bool operator !=(AllOpsClass self, AllOpsClass other) {
            Flag.Set(330);
            return self._value != other._value;
        }

        public static bool operator >=(AllOpsClass self, AllOpsClass other) {
            Flag.Set(340);
            return self._value >= other._value;
        }
        public static bool operator <=(AllOpsClass self, AllOpsClass other) {
            Flag.Set(350);
            return self._value <= other._value;
        }

        [SpecialName]
        public static AllOpsClass op_SignedRightShift(AllOpsClass self, int other) {
            Flag.Set(360);
            return new AllOpsClass(self._value << other);
        }

        [SpecialName]
        public static AllOpsClass op_UnsignedRightShift(AllOpsClass self, int other) {
            Flag.Set(370);
            return new AllOpsClass(self._value << other);
        }

        [SpecialName]
        public static AllOpsClass op_MultiplicationAssignment(AllOpsClass self, AllOpsClass other) {
            Flag.Set(380);
            return new AllOpsClass(self._value * other._value);
        }
        [SpecialName]
        public static AllOpsClass op_SubtractionAssignment(AllOpsClass self, AllOpsClass other) {
            Flag.Set(390);
            return new AllOpsClass(self._value - other._value);
        }
        [SpecialName]
        public static AllOpsClass op_ExclusiveOrAssignment(AllOpsClass self, AllOpsClass other) {
            Flag.Set(400);
            return new AllOpsClass(self._value ^ other._value);
        }
        [SpecialName]
        public static AllOpsClass op_LeftShiftAssignment(AllOpsClass self, int other) {
            Flag.Set(410);
            return new AllOpsClass(self._value << other);
        }
        [SpecialName]
        public static AllOpsClass op_RightShiftAssignment(AllOpsClass self, int other) {
            Flag.Set(420);
            return new AllOpsClass(self._value >> other);
        }
        [SpecialName]
        public static AllOpsClass op_UnsignedRightShiftAssignment(AllOpsClass self, int other) {
            Flag.Set(430);
            return new AllOpsClass(self._value >> other);
        }
        [SpecialName]
        public static AllOpsClass op_ModulusAssignment(AllOpsClass self, AllOpsClass other) {
            Flag.Set(440);
            return new AllOpsClass(self._value % other._value);
        }
        [SpecialName]
        public static AllOpsClass op_AdditionAssignment(AllOpsClass self, AllOpsClass other) {
            Flag.Set(450);
            return new AllOpsClass(self._value + other._value);
        }

        [SpecialName]
        public static AllOpsClass op_BitwiseAndAssignment(AllOpsClass self, AllOpsClass other) {
            Flag.Set(460);
            return new AllOpsClass(self._value & other._value);
        }

        [SpecialName]
        public static AllOpsClass op_BitwiseOrAssignment(AllOpsClass self, AllOpsClass other) {
            Flag.Set(470);
            return new AllOpsClass(self._value | other._value);
        }

        [SpecialName]
        public static AllOpsClass op_DivisionAssignment(AllOpsClass self, AllOpsClass other) {
            Flag.Set(480);
            return new AllOpsClass(self._value / other._value);
        }
        [SpecialName]
        public static List<AllOpsClass> op_Comma(AllOpsClass self, AllOpsClass other) {
            Flag.Set(490);
            return new List<AllOpsClass>(new AllOpsClass[] { self, other });
        }

        public override bool Equals(object obj) {
            AllOpsClass other = obj as AllOpsClass;
            if (other == null) return false;

            return this._value == other._value;
        }

        public override int GetHashCode() {
            return _value.GetHashCode();
        }
    }

    public class InstanceOp {
        [SpecialName]
        public /*static*/ InstanceOp op_Addition(InstanceOp self, InstanceOp other) {
            return null;
        }
    }

    public class UnaryWithWrongParamOp {
        [SpecialName]
        public static UnaryWithWrongParamOp op_UnaryNegation() {
            return null;
        }
        [SpecialName]
        public static UnaryWithWrongParamOp op_UnaryPlus(UnaryWithWrongParamOp self, UnaryWithWrongParamOp other) {
            return null;
        }
        [SpecialName]
        public static UnaryWithWrongParamOp op_OnesComplement(UnaryWithWrongParamOp self, UnaryWithWrongParamOp other1, UnaryWithWrongParamOp other2) {
            return null;
        }
    }

    public class BinaryWithWrongParamOp {
        [SpecialName]
        public static BinaryWithWrongParamOp op_Subtraction() {
            return null;
        }
        [SpecialName]
        public static BinaryWithWrongParamOp op_Addition(BinaryWithWrongParamOp self) {
            return null;
        }
        [SpecialName]
        public static BinaryWithWrongParamOp op_Division(BinaryWithWrongParamOp self, BinaryWithWrongParamOp other1, BinaryWithWrongParamOp other2) {
            return null;
        }
    }

    public class FirstArgOp {
        private int _value;
        public int Value {
            get { return _value; }
        }

        public FirstArgOp(int value) {
            _value = value;
        }

        // return the second one
        public static FirstArgOp operator +(object self, FirstArgOp other) {
            Flag.Set(100);
            return new FirstArgOp(other._value);
        }
    }

    public class InstanceMethodOp {
        private int _value;
        public int Value {
            get { return _value; }
        }

        public InstanceMethodOp(int value) {
            _value = value;
        }

        [SpecialName]
        public InstanceMethodOp op_Addition(InstanceMethodOp other) {
            Flag.Set(123);
            return new InstanceMethodOp(this._value * other._value);
        }

        [SpecialName]
        public InstanceMethodOp op_OnesComplement() {
            Flag.Set(125);
            return new InstanceMethodOp(-this._value);
        }
    }

    #region One-side comparison
    public class LessThanOp {
        int _value;
        public int Value { get { return _value; } }
        public LessThanOp(int value) { _value = value; }

        [SpecialName]
        public static bool op_LessThan(LessThanOp self, LessThanOp other) {
            Flag.Set(328);
            return self._value < other._value;
        }
    }
    public class GreaterThanOp {
        int _value;
        public int Value { get { return _value; } }
        public GreaterThanOp(int value) { _value = value; }

        [SpecialName]
        public static bool op_GreaterThan(GreaterThanOp self, GreaterThanOp other) {
            Flag.Set(329);
            return self._value > other._value;
        }
    }
    public class LessThanOrEqualOp {
        int _value;
        public int Value { get { return _value; } }
        public LessThanOrEqualOp(int value) { _value = value; }

        [SpecialName]
        public static bool op_LessThanOrEqual(LessThanOrEqualOp self, LessThanOrEqualOp other) {
            Flag.Set(330);
            return self._value <= other._value;
        }
    }

    public class GreaterThanOrEqualOp {
        int _value;
        public int Value { get { return _value; } }
        public GreaterThanOrEqualOp(int value) { _value = value; }

        [SpecialName]
        public static bool op_GreaterThanOrEqual(GreaterThanOrEqualOp self, GreaterThanOrEqualOp other) {
            Flag.Set(331);
            return self._value >= other._value;
        }
    }

    public class EqualOp {
        int _value;
        public int Value { get { return _value; } }
        public EqualOp(int value) { _value = value; }

        [SpecialName]
        public static bool op_Equality(EqualOp self, EqualOp other) {
            Flag.Set(332);
            return self._value == other._value;
        }
    }
    public class NotEqualOp {
        int _value;
        public int Value { get { return _value; } }
        public NotEqualOp(int value) { _value = value; }

        [SpecialName]
        public static bool op_Inequality(NotEqualOp self, NotEqualOp other) {
            Flag.Set(333);
            return self._value != other._value;
        }
    }
    #endregion

    public class NoInPlaceOp {
        int _value;
        public int Value { get { return _value; } }

        public NoInPlaceOp(int value) { _value = value; }

        public static NoInPlaceOp operator +(NoInPlaceOp self, NoInPlaceOp other) {
            Flag.Set(493);
            return new NoInPlaceOp(self._value + other._value);
        }
        public static NoInPlaceOp operator -(NoInPlaceOp self, NoInPlaceOp other) {
            Flag.Set(494);
            return new NoInPlaceOp(self._value - other._value);
        }
        public static NoInPlaceOp operator *(NoInPlaceOp self, NoInPlaceOp other) {
            Flag.Set(495);
            return new NoInPlaceOp(self._value * other._value);
        }
        public static NoInPlaceOp operator /(NoInPlaceOp self, NoInPlaceOp other) {
            Flag.Set(496);
            return new NoInPlaceOp(self._value / other._value);
        }
        public static NoInPlaceOp operator ^(NoInPlaceOp self, NoInPlaceOp other) {
            Flag.Set(497);
            return new NoInPlaceOp(self._value ^ other._value);
        }
        public static NoInPlaceOp operator |(NoInPlaceOp self, NoInPlaceOp other) {
            Flag.Set(498);
            return new NoInPlaceOp(self._value | other._value);
        }
        public static NoInPlaceOp operator &(NoInPlaceOp self, NoInPlaceOp other) {
            Flag.Set(499);
            return new NoInPlaceOp(self._value & other._value);
        }
        public static NoInPlaceOp operator >>(NoInPlaceOp self, int other) {
            Flag.Set(500);
            return new NoInPlaceOp(self._value >> other);
        }
        public static NoInPlaceOp operator <<(NoInPlaceOp self, int other) {
            Flag.Set(501);
            return new NoInPlaceOp(self._value << other);
        }
        public static NoInPlaceOp operator %(NoInPlaceOp self, NoInPlaceOp other) {
            Flag.Set(502);
            return new NoInPlaceOp(self._value % other._value);
        }
    }

    // overloads
}

namespace Merlin.Testing.BaseClass {
    public class COperator10 {
        int _value;
        public COperator10(int val) { _value = val; }
        public int Value {
            get { return _value; }
        }

        [SpecialName]
        public virtual COperator10 op_Add(COperator10 other) {
            return new COperator10(this.Value + other.Value);
        }
    }

    public partial class Callback {
        public static COperator10 On(COperator10 arg1, COperator10 arg2) {
            return arg1.op_Add(arg2);
        }
    }
}