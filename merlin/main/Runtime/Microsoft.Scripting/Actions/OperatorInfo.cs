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
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// OperatorInfo provides a mapping from DLR operators to their associated .NET methods.
    /// </summary>
    public class OperatorInfo {
        private static readonly OperatorInfo[] _infos = MakeOperatorTable(); // table of Operators, names, and alt names for looking up methods.
        
        private readonly Operators _operator;
        private readonly string _name;
        private readonly string _altName;

        private OperatorInfo(Operators op, string name, string altName) {
            _operator = op;
            _name = name;
            _altName = altName;
        }

        /// <summary>
        /// Given an operator returns the OperatorInfo associated with the operator or null
        /// </summary>
        public static OperatorInfo GetOperatorInfo(Operators op) {
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
        public Operators Operator {
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

            // unary operators as defined in Partition I Architecture 9.3.1:
            res.Add(new OperatorInfo(Operators.Decrement,           "op_Decrement",                 "Decrement"));      // --
            res.Add(new OperatorInfo(Operators.Increment,           "op_Increment",                 "Increment"));      // ++
            res.Add(new OperatorInfo(Operators.Negate,              "op_UnaryNegation",             "Negate"));         // - (unary)
            res.Add(new OperatorInfo(Operators.Positive,            "op_UnaryPlus",                 "Plus"));           // + (unary)
            res.Add(new OperatorInfo(Operators.Not,                 "op_LogicalNot",                null));             // !
            res.Add(new OperatorInfo(Operators.IsTrue,              "op_True",                      null));             // not defined
            res.Add(new OperatorInfo(Operators.IsFalse,             "op_False",                     null));             // not defined
            //res.Add(new OperatorInfo(Operators.AddressOf,           "op_AddressOf",                 null));             // & (unary)
            res.Add(new OperatorInfo(Operators.OnesComplement,      "op_OnesComplement",            "OnesComplement")); // ~
            //res.Add(new OperatorInfo(Operators.PointerDereference,  "op_PointerDereference",        null));             // * (unary)

            // binary operators as defined in Partition I Architecture 9.3.2:
            res.Add(new OperatorInfo(Operators.Add,                 "op_Addition",                  "Add"));            // +
            res.Add(new OperatorInfo(Operators.Subtract,            "op_Subtraction",               "Subtract"));       // -
            res.Add(new OperatorInfo(Operators.Multiply,            "op_Multiply",                  "Multiply"));       // *
            res.Add(new OperatorInfo(Operators.Divide,              "op_Division",                  "Divide"));         // /
            res.Add(new OperatorInfo(Operators.Mod,                 "op_Modulus",                   "Mod"));            // %
            res.Add(new OperatorInfo(Operators.ExclusiveOr,         "op_ExclusiveOr",               "ExclusiveOr"));    // ^
            res.Add(new OperatorInfo(Operators.BitwiseAnd,          "op_BitwiseAnd",                "BitwiseAnd"));     // &
            res.Add(new OperatorInfo(Operators.BitwiseOr,           "op_BitwiseOr",                 "BitwiseOr"));      // |
            res.Add(new OperatorInfo(Operators.And,                 "op_LogicalAnd",                "And"));            // &&
            res.Add(new OperatorInfo(Operators.Or,                  "op_LogicalOr",                 "Or"));             // ||
            res.Add(new OperatorInfo(Operators.Assign,              "op_Assign",                    "Assign"));         // =
            res.Add(new OperatorInfo(Operators.LeftShift,           "op_LeftShift",                 "LeftShift"));      // <<
            res.Add(new OperatorInfo(Operators.RightShift,          "op_RightShift",                "RightShift"));     // >>
            res.Add(new OperatorInfo(Operators.RightShiftSigned,    "op_SignedRightShift",          "RightShift"));     // not defined
            res.Add(new OperatorInfo(Operators.RightShiftUnsigned,  "op_UnsignedRightShift",        "RightShift"));     // not defined
            res.Add(new OperatorInfo(Operators.Equals,              "op_Equality",                  "Equals"));         // ==   
            res.Add(new OperatorInfo(Operators.GreaterThan,         "op_GreaterThan",               "GreaterThan"));    // >
            res.Add(new OperatorInfo(Operators.LessThan,            "op_LessThan",                  "LessThan"));       // <
            res.Add(new OperatorInfo(Operators.NotEquals,           "op_Inequality",                "NotEquals"));      // != 
            res.Add(new OperatorInfo(Operators.GreaterThanOrEqual,  "op_GreaterThanOrEqual",        "GreaterThanOrEqual"));        // >=
            res.Add(new OperatorInfo(Operators.LessThanOrEqual,     "op_LessThanOrEqual",           "LessThanOrEqual"));        // <=
            res.Add(new OperatorInfo(Operators.InPlaceMultiply,     "op_MultiplicationAssignment",  "InPlaceMultiply"));       // *=
            res.Add(new OperatorInfo(Operators.InPlaceSubtract,     "op_SubtractionAssignment",     "InPlaceSubtract"));       // -=
            res.Add(new OperatorInfo(Operators.InPlaceExclusiveOr,  "op_ExclusiveOrAssignment",     "InPlaceExclusiveOr"));            // ^=
            res.Add(new OperatorInfo(Operators.InPlaceLeftShift,    "op_LeftShiftAssignment",       "InPlaceLeftShift"));      // <<=
            res.Add(new OperatorInfo(Operators.InPlaceRightShift,   "op_RightShiftAssignment",      "InPlaceRightShift"));     // >>=
            res.Add(new OperatorInfo(Operators.InPlaceRightShiftUnsigned, "op_UnsignedRightShiftAssignment", "InPlaceUnsignedRightShift"));     // >>=
            res.Add(new OperatorInfo(Operators.InPlaceMod,          "op_ModulusAssignment",         "InPlaceMod"));            // %=
            res.Add(new OperatorInfo(Operators.InPlaceAdd,          "op_AdditionAssignment",        "InPlaceAdd"));            // += 
            res.Add(new OperatorInfo(Operators.InPlaceBitwiseAnd,   "op_BitwiseAndAssignment",      "InPlaceBitwiseAnd"));     // &=
            res.Add(new OperatorInfo(Operators.InPlaceBitwiseOr,    "op_BitwiseOrAssignment",       "InPlaceBitwiseOr"));      // |=
            res.Add(new OperatorInfo(Operators.InPlaceDivide,       "op_DivisionAssignment",        "InPlaceDivide"));         // /=
            res.Add(new OperatorInfo(Operators.Comma,               "op_Comma",                     null));             // ,

            // DLR Extended operators:
            res.Add(new OperatorInfo(Operators.Compare,             "op_Compare",                   "Compare"));        // not defined
            res.Add(new OperatorInfo(Operators.GetItem,             "get_Item",                     "GetItem"));        // x[y]
            res.Add(new OperatorInfo(Operators.SetItem,             "set_Item",                     "SetItem"));        // x[y] = z
            res.Add(new OperatorInfo(Operators.DeleteItem,          "del_Item",                     "DeleteItem"));     // not defined

            res.Add(new OperatorInfo(Operators.GetEnumerator,       "GetEnumerator",                null));
            res.Add(new OperatorInfo(Operators.Dispose,             "Dispose",                      null));

            res.Add(new OperatorInfo(Operators.MemberNames,         "GetMemberNames",               null));
            res.Add(new OperatorInfo(Operators.CodeRepresentation,  "ToCodeString",                 null));
            res.Add(new OperatorInfo(Operators.CallSignatures,      "GetCallSignatures",            null));
            res.Add(new OperatorInfo(Operators.Documentation,       "GetDocumentation",             null));
            res.Add(new OperatorInfo(Operators.IsCallable,          "IsCallable",                   null));

            return res.ToArray();
        }
    }
}
