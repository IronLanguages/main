/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// OperatorInfo provides a mapping from DLR ExpressionType to their associated .NET methods.
    /// </summary>
    internal class OperatorInfo {
        private static readonly OperatorInfo[] _infos = MakeOperatorTable(); // table of ExpressionType, names, and alt names for looking up methods.

        private readonly ExpressionType _operator;
        private readonly string _name;
        private readonly string _altName;

        private OperatorInfo(ExpressionType op, string name, string altName) {
            _operator = op;
            _name = name;
            _altName = altName;
        }

        /// <summary>
        /// Given an operator returns the OperatorInfo associated with the operator or null
        /// </summary>
        public static OperatorInfo GetOperatorInfo(ExpressionType op) {
            foreach (OperatorInfo info in _infos) {
                if (info._operator == op) return info;
            }
            return null;
        }

        public static OperatorInfo GetOperatorInfo(string name) {
            foreach (OperatorInfo info in _infos) {
                if (info.Name == name || info.AlternateName == name) {
                    return info;
                }
            }
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static Operators ExpressionTypeToOperator(ExpressionType et) {
            switch (et) {
                case ExpressionType.Add: return Operators.Add;
                case ExpressionType.And: return Operators.BitwiseAnd;
                case ExpressionType.Divide: return Operators.Divide;
                case ExpressionType.ExclusiveOr: return Operators.ExclusiveOr;
                case ExpressionType.Modulo: return Operators.Mod;
                case ExpressionType.Multiply: return Operators.Multiply;
                case ExpressionType.Or: return Operators.BitwiseOr;
                case ExpressionType.Power: return Operators.Power;
                case ExpressionType.RightShift: return Operators.RightShift;
                case ExpressionType.LeftShift: return Operators.LeftShift;
                case ExpressionType.Subtract: return Operators.Subtract;

                case ExpressionType.AddAssign: return Operators.InPlaceAdd;
                case ExpressionType.AndAssign: return Operators.InPlaceBitwiseAnd;
                case ExpressionType.DivideAssign: return Operators.InPlaceDivide;
                case ExpressionType.ExclusiveOrAssign: return Operators.InPlaceExclusiveOr;
                case ExpressionType.ModuloAssign: return Operators.InPlaceMod;
                case ExpressionType.MultiplyAssign: return Operators.InPlaceMultiply;
                case ExpressionType.OrAssign: return Operators.InPlaceBitwiseOr;
                case ExpressionType.PowerAssign: return Operators.InPlacePower;
                case ExpressionType.RightShiftAssign: return Operators.InPlaceRightShift;
                case ExpressionType.LeftShiftAssign: return Operators.InPlaceLeftShift;
                case ExpressionType.SubtractAssign: return Operators.InPlaceSubtract;

                case ExpressionType.Equal: return Operators.Equals;
                case ExpressionType.GreaterThan: return Operators.GreaterThan;
                case ExpressionType.GreaterThanOrEqual: return Operators.GreaterThanOrEqual;
                case ExpressionType.LessThan: return Operators.LessThan;
                case ExpressionType.LessThanOrEqual: return Operators.LessThanOrEqual;
                case ExpressionType.NotEqual: return Operators.NotEquals;
            }
            return Operators.None;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static ExpressionType? OperatorToExpressionType(Operators op) {
            switch (op) {
                case Operators.Add: return ExpressionType.Add;
                case Operators.BitwiseAnd: return ExpressionType.And;
                case Operators.Divide: return ExpressionType.Divide;
                case Operators.ExclusiveOr: return ExpressionType.ExclusiveOr;
                case Operators.Mod: return ExpressionType.Modulo;
                case Operators.Multiply: return ExpressionType.Multiply;
                case Operators.BitwiseOr: return ExpressionType.Or;
                case Operators.Power: return ExpressionType.Power;
                case Operators.RightShift: return ExpressionType.RightShift;
                case Operators.LeftShift: return ExpressionType.LeftShift;
                case Operators.Subtract: return ExpressionType.Subtract;
                
                case Operators.InPlaceAdd: return ExpressionType.AddAssign;
                case Operators.InPlaceBitwiseAnd: return ExpressionType.AndAssign;
                case Operators.InPlaceDivide: return ExpressionType.DivideAssign;
                case Operators.InPlaceExclusiveOr: return ExpressionType.ExclusiveOrAssign;
                case Operators.InPlaceMod: return ExpressionType.ModuloAssign;
                case Operators.InPlaceMultiply: return ExpressionType.MultiplyAssign;
                case Operators.InPlaceBitwiseOr: return ExpressionType.OrAssign;
                case Operators.InPlacePower: return ExpressionType.PowerAssign;
                case Operators.InPlaceRightShift: return ExpressionType.RightShiftAssign;
                case Operators.InPlaceLeftShift: return ExpressionType.LeftShiftAssign;
                case Operators.InPlaceSubtract: return ExpressionType.SubtractAssign;

                case Operators.Equals: return ExpressionType.Equal;
                case Operators.GreaterThan: return ExpressionType.GreaterThan;
                case Operators.GreaterThanOrEqual: return ExpressionType.GreaterThanOrEqual;
                case Operators.LessThan: return ExpressionType.LessThan;
                case Operators.LessThanOrEqual: return ExpressionType.LessThanOrEqual;
                case Operators.NotEquals: return ExpressionType.NotEqual;

                
            }
            return null;
        }

        /// <summary>
        /// The operator the OperatorInfo provides info for.
        /// </summary>
        public ExpressionType Operator {
            get { return _operator; }
        }
        
        /// <summary>
        /// The primary method name associated with the method.  This method name is
        /// usally in the form of op_Operator (e.g. op_Addition).
        /// </summary>
        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// The secondary method name associated with the method.  This method name is
        /// usually a standard .NET method name with pascal casing (e.g. Add).
        /// </summary>
        public string AlternateName {
            get { return _altName; }
        }

        private static OperatorInfo[] MakeOperatorTable() {
            List<OperatorInfo> res = new List<OperatorInfo>();

            // alternate names from: http://msdn2.microsoft.com/en-us/library/2sk3x8a7(vs.71).aspx
            //   different in:
            //    comparisons all support alternative names, Xor is "ExclusiveOr" not "Xor"

            // unary ExpressionType as defined in Partition I Architecture 9.3.1:
            res.Add(new OperatorInfo(ExpressionType.Decrement,           "op_Decrement",                 "Decrement"));      // --
            res.Add(new OperatorInfo(ExpressionType.Increment,           "op_Increment",                 "Increment"));      // ++
            res.Add(new OperatorInfo(ExpressionType.Negate,              "op_UnaryNegation",             "Negate"));         // - (unary)
            res.Add(new OperatorInfo(ExpressionType.UnaryPlus,           "op_UnaryPlus",                 "Plus"));           // + (unary)
            res.Add(new OperatorInfo(ExpressionType.Not,                 "op_LogicalNot",                null));             // !
            res.Add(new OperatorInfo(ExpressionType.IsTrue,              "op_True",                      null));             // not defined
            res.Add(new OperatorInfo(ExpressionType.IsFalse,             "op_False",                     null));             // not defined
            //res.Add(new OperatorInfo(ExpressionType.AddressOf,           "op_AddressOf",                 null));             // & (unary)
            res.Add(new OperatorInfo(ExpressionType.OnesComplement,      "op_OnesComplement",            "OnesComplement")); // ~
            //res.Add(new OperatorInfo(ExpressionType.PointerDereference,  "op_PointerDereference",        null));             // * (unary)

            // binary ExpressionType as defined in Partition I Architecture 9.3.2:
            res.Add(new OperatorInfo(ExpressionType.Add,                 "op_Addition",                  "Add"));            // +
            res.Add(new OperatorInfo(ExpressionType.Subtract,            "op_Subtraction",               "Subtract"));       // -
            res.Add(new OperatorInfo(ExpressionType.Multiply,            "op_Multiply",                  "Multiply"));       // *
            res.Add(new OperatorInfo(ExpressionType.Divide,              "op_Division",                  "Divide"));         // /
            res.Add(new OperatorInfo(ExpressionType.Modulo,              "op_Modulus",                   "Mod"));            // %
            res.Add(new OperatorInfo(ExpressionType.ExclusiveOr,         "op_ExclusiveOr",               "ExclusiveOr"));    // ^
            res.Add(new OperatorInfo(ExpressionType.And,                 "op_BitwiseAnd",                "BitwiseAnd"));     // &
            res.Add(new OperatorInfo(ExpressionType.Or,                  "op_BitwiseOr",                 "BitwiseOr"));      // |
            res.Add(new OperatorInfo(ExpressionType.And,                 "op_LogicalAnd",                "And"));            // &&
            res.Add(new OperatorInfo(ExpressionType.Or,                  "op_LogicalOr",                 "Or"));             // ||
            res.Add(new OperatorInfo(ExpressionType.LeftShift,           "op_LeftShift",                 "LeftShift"));      // <<
            res.Add(new OperatorInfo(ExpressionType.RightShift,          "op_RightShift",                "RightShift"));     // >>
            res.Add(new OperatorInfo(ExpressionType.Equal,               "op_Equality",                  "Equals"));         // ==   
            res.Add(new OperatorInfo(ExpressionType.GreaterThan,         "op_GreaterThan",               "GreaterThan"));    // >
            res.Add(new OperatorInfo(ExpressionType.LessThan,            "op_LessThan",                  "LessThan"));       // <
            res.Add(new OperatorInfo(ExpressionType.NotEqual,            "op_Inequality",                "NotEquals"));      // != 
            res.Add(new OperatorInfo(ExpressionType.GreaterThanOrEqual,  "op_GreaterThanOrEqual",        "GreaterThanOrEqual"));        // >=
            res.Add(new OperatorInfo(ExpressionType.LessThanOrEqual,     "op_LessThanOrEqual",           "LessThanOrEqual"));        // <=
            res.Add(new OperatorInfo(ExpressionType.MultiplyAssign,      "op_MultiplicationAssignment",  "InPlaceMultiply"));       // *=
            res.Add(new OperatorInfo(ExpressionType.SubtractAssign,      "op_SubtractionAssignment",     "InPlaceSubtract"));       // -=
            res.Add(new OperatorInfo(ExpressionType.ExclusiveOrAssign,   "op_ExclusiveOrAssignment",     "InPlaceExclusiveOr"));            // ^=
            res.Add(new OperatorInfo(ExpressionType.LeftShiftAssign,     "op_LeftShiftAssignment",       "InPlaceLeftShift"));      // <<=
            res.Add(new OperatorInfo(ExpressionType.RightShiftAssign,    "op_RightShiftAssignment",      "InPlaceRightShift"));     // >>=
            res.Add(new OperatorInfo(ExpressionType.ModuloAssign,        "op_ModulusAssignment",         "InPlaceMod"));            // %=
            res.Add(new OperatorInfo(ExpressionType.AddAssign,           "op_AdditionAssignment",        "InPlaceAdd"));            // += 
            res.Add(new OperatorInfo(ExpressionType.AndAssign,           "op_BitwiseAndAssignment",      "InPlaceBitwiseAnd"));     // &=
            res.Add(new OperatorInfo(ExpressionType.OrAssign,            "op_BitwiseOrAssignment",       "InPlaceBitwiseOr"));      // |=
            res.Add(new OperatorInfo(ExpressionType.DivideAssign,        "op_DivisionAssignment",        "InPlaceDivide"));         // /=

            return res.ToArray();
        }
    }
}
