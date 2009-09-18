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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System.Collections.Generic;
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
