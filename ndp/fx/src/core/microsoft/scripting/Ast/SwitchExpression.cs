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

using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic.Utils;
using System.Runtime.InteropServices;

namespace System.Linq.Expressions {
    public sealed class SwitchExpression : Expression {
        private readonly Expression _testValue;
        private readonly ReadOnlyCollection<SwitchCase> _cases;
        private readonly LabelTarget _label;

        internal SwitchExpression(Expression testValue, LabelTarget label, ReadOnlyCollection<SwitchCase> cases) {            
            _label = label;
            _testValue = testValue;
            _cases = cases;
        }

        protected override Type GetExpressionType() {
            return typeof(void);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Switch;
        }

        public Expression Test {
            get { return _testValue; }
        }

        public ReadOnlyCollection<SwitchCase> SwitchCases {
            get { return _cases; }
        }

        public LabelTarget BreakLabel {
            get { return _label; }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitSwitch(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        public static SwitchExpression Switch(Expression value, params SwitchCase[] cases) {
            return Switch(value, null, (IEnumerable<SwitchCase>)cases);
        }
        public static SwitchExpression Switch(Expression value, LabelTarget label, params SwitchCase[] cases) {
            return Switch(value, label, (IEnumerable<SwitchCase>)cases);
        }
        public static SwitchExpression Switch(Expression value, LabelTarget label, IEnumerable<SwitchCase> cases) {
            RequiresCanRead(value, "value");
            ContractUtils.Requires(value.Type == typeof(int), "value", Strings.ValueMustBeInt);
            ContractUtils.RequiresNotNull(cases, "cases");
            var caseList = cases.ToReadOnly();
            ContractUtils.RequiresNotEmpty(caseList, "cases");
            ContractUtils.RequiresNotNullItems(caseList, "cases");
            ContractUtils.Requires(label == null || label.Type == typeof(void), "label", Strings.LabelTypeMustBeVoid);

            bool @default = false;
            int max = Int32.MinValue;
            int min = Int32.MaxValue;
            foreach (SwitchCase sc in caseList) {
                if (sc.IsDefault) {
                    ContractUtils.Requires(@default == false, "cases", Strings.OnlyDefaultIsAllowed);
                    @default = true;
                } else {
                    int val = sc.Value;
                    if (val > max) max = val;
                    if (val < min) min = val;
                }
            }

            ContractUtils.Requires(UniqueCaseValues(caseList, min, max), "cases", Strings.CaseValuesMustBeUnique);

            return new SwitchExpression(value, label, caseList);
        }

        // Below his threshold we'll use brute force N^2 algorithm
        private const int N2Threshold = 10;

        // If values are in a small range, we'll use bit array
        private const long BitArrayThreshold = 1024;

        private static bool UniqueCaseValues(ReadOnlyCollection<SwitchCase> cases, int min, int max) {
            int length = cases.Count;

            // If we have small number of cases, use straightforward N2 algorithm
            // which doesn't allocate memory
            if (length < N2Threshold) {
                for (int i = 0; i < length; i++) {
                    SwitchCase sci = cases[i];
                    if (sci.IsDefault) {
                        continue;
                    }
                    for (int j = i + 1; j < length; j++) {
                        SwitchCase scj = cases[j];
                        if (scj.IsDefault) {
                            continue;
                        }

                        if (sci.Value == scj.Value) {
                            // Duplicate value found
                            return false;
                        }
                    }
                }

                return true;
            }

            // We have at least N2Threshold items so the min and max values
            // are set to actual values and not the Int32.MaxValue and Int32.MaxValue
            Debug.Assert(min <= max);
            long delta = (long)max - (long)min;
            if (delta < BitArrayThreshold) {
                BitArray ba = new BitArray((int)delta + 1, false);

                for (int i = 0; i < length; i++) {
                    SwitchCase sc = cases[i];
                    if (sc.IsDefault) {
                        continue;
                    }
                    // normalize to 0 .. (max - min)
                    int val = sc.Value - min;
                    if (ba.Get(val)) {
                        // Duplicate value found
                        return false;
                    }
                    ba.Set(val, true);
                }

                return true;
            }

            // Too many values that are too spread around. Use dictionary
            // Using Dictionary<int, object> as it is used elsewhere to
            // minimize the impact of generic instantiation
            Dictionary<int, object> dict = new Dictionary<int, object>(length);
            for (int i = 0; i < length; i++) {
                SwitchCase sc = cases[i];
                if (sc.IsDefault) {
                    continue;
                }
                int val = sc.Value;
                if (dict.ContainsKey(val)) {
                    // Duplicate value found
                    return false;
                }
                dict[val] = null;
            }

            return true;
        }
    }
}
