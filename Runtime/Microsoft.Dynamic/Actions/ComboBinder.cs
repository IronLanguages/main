/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
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

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// A binder which can combine multiple binders into a single dynamic site.  The creator
    /// of this needs to perform the mapping of parameters, constants, and sub-site expressions
    /// and provide a List of BinderMappingInfo representing this data.  From there the ComboBinder
    /// just processes the list to create the resulting code.
    /// </summary>
    public class ComboBinder : DynamicMetaObjectBinder {
        private readonly BinderMappingInfo[] _metaBinders;

        public ComboBinder(params BinderMappingInfo[] binders)
            : this((ICollection<BinderMappingInfo>)binders) {
        }

        public ComboBinder(ICollection<BinderMappingInfo> binders) {
            Assert.NotNullItems(binders);

            _metaBinders = ArrayUtils.ToArray(binders);
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, params DynamicMetaObject[] args) {
            args = ArrayUtils.Insert(target, args);

            List<DynamicMetaObject> results = new List<DynamicMetaObject>(_metaBinders.Length);
            List<Expression> steps = new List<Expression>();
            List<ParameterExpression> temps = new List<ParameterExpression>();
            BindingRestrictions restrictions = BindingRestrictions.Empty;

            for (int i = 0; i < _metaBinders.Length; i++) {
                BinderMappingInfo curBinder = _metaBinders[i];

                DynamicMetaObject[] tmpargs = GetArguments(args, results, i);
                DynamicMetaObject next = curBinder.Binder.Bind(tmpargs[0], ArrayUtils.RemoveFirst(tmpargs));
                if (i != 0) {
                    // If the rule contains an embedded "update", replace it with a defer
                    var visitor = new ReplaceUpdateVisitor { Binder = curBinder.Binder, Arguments = tmpargs };
                    next = new DynamicMetaObject(visitor.Visit(next.Expression), next.Restrictions);
                }

                restrictions = restrictions.Merge(next.Restrictions);
                if (next.Expression.NodeType == ExpressionType.Throw) {
                    // end of the line... the expression is throwing, none of the other 
                    // binders will have an opportunity to run.
                    steps.Add(next.Expression);
                    break;
                }

                ParameterExpression tmp = Expression.Variable(next.Expression.Type, "comboTemp" + i.ToString());
                temps.Add(tmp);

                steps.Add(Expression.Assign(tmp, next.Expression));
                results.Add(new DynamicMetaObject(tmp, next.Restrictions));
            }

            return new DynamicMetaObject(
                Expression.Block(
                    temps.ToArray(),
                    steps.ToArray()
                ),
                restrictions
            );
        }

        public override Type ReturnType {
            get {
                return _metaBinders[_metaBinders.Length - 1].Binder.ReturnType;
            }
        }

        private sealed class ReplaceUpdateVisitor : ExpressionVisitor {
            internal DynamicMetaObjectBinder Binder;
            internal DynamicMetaObject[] Arguments;

            protected override Expression VisitGoto(GotoExpression node) {
                if (node.Target == CallSiteBinder.UpdateLabel) {
                    return Binder.Defer(Arguments).Expression;
                }
                return base.Visit(node);
            }
        }


        private DynamicMetaObject[] GetArguments(DynamicMetaObject[] args, IList<DynamicMetaObject> results, int metaBinderIndex) {
            BinderMappingInfo indices = _metaBinders[metaBinderIndex];

            DynamicMetaObject[] res = new DynamicMetaObject[indices.MappingInfo.Count];
            for (int i = 0; i < res.Length; i++) {
                ParameterMappingInfo mappingInfo = indices.MappingInfo[i];

                if (mappingInfo.IsAction) {
                    // input is the result of a previous bind
                    res[i] = results[mappingInfo.ActionIndex];
                } else if (mappingInfo.IsParameter) {
                    // input is one of the original arguments
                    res[i] = args[mappingInfo.ParameterIndex];
                } else {
                    // input is a constant
                    res[i] = new DynamicMetaObject(
                        mappingInfo.Constant,
                        BindingRestrictions.Empty,
                        mappingInfo.Constant.Value
                    );
                }
            }

            return res;
        }

        public override int GetHashCode() {
            int res = 6551;
            foreach (BinderMappingInfo metaBinder in _metaBinders) {
                res ^= metaBinder.Binder.GetHashCode();

                foreach (ParameterMappingInfo mapInfo in metaBinder.MappingInfo) {
                    res ^= mapInfo.GetHashCode();
                }
            }

            return res;
        }

        public override bool Equals(object obj) {
            ComboBinder other = obj as ComboBinder;
            if (other != null) {
                if (_metaBinders.Length != other._metaBinders.Length) {
                    return false;
                }

                for (int i = 0; i < _metaBinders.Length; i++) {
                    BinderMappingInfo self = _metaBinders[i];
                    BinderMappingInfo otherBinders = other._metaBinders[i];

                    if (!self.Binder.Equals(otherBinders.Binder) ||
                        self.MappingInfo.Count != otherBinders.MappingInfo.Count) {
                        return false;
                    }

                    for (int j = 0; j < self.MappingInfo.Count; j++) {
                        if (!self.MappingInfo[j].Equals(otherBinders.MappingInfo[j])) {
                            return false;
                        }
                    }
                }
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Provides a mapping for inputs of combo action expressions.  The input can map
    /// to either an input of the new dynamic site, an input of a previous DynamicExpression,
    /// or a ConstantExpression which has been pulled out of the dynamic site arguments.
    /// </summary>
    public class ParameterMappingInfo {
        private readonly int _parameterIndex;
        private readonly int _actionIndex;
        private ConstantExpression _fixedInput;

        private ParameterMappingInfo(int param, int action, ConstantExpression fixedInput) {
            _parameterIndex = param;
            _actionIndex = action;
            _fixedInput = fixedInput;
        }

        public static ParameterMappingInfo Parameter(int index) {
            return new ParameterMappingInfo(index, -1, null);
        }

        public static ParameterMappingInfo Action(int index) {
            return new ParameterMappingInfo(-1, index, null);
        }

        public static ParameterMappingInfo Fixed(ConstantExpression e) {
            return new ParameterMappingInfo(-1, -1, e);
        }

        public int ParameterIndex {
            get {
                return _parameterIndex;
            }
        }

        public int ActionIndex {
            get {
                return _actionIndex;
            }
        }

        public ConstantExpression Constant {
            get {
                return _fixedInput;
            }
        }

        public bool IsParameter {
            get {
                return _parameterIndex != -1;
            }
        }

        public bool IsAction {
            get {
                return _actionIndex != -1;
            }
        }

        public bool IsConstant {
            get {
                return _fixedInput != null;
            }
        }

        public override bool Equals(object obj) {
            ParameterMappingInfo pmi = obj as ParameterMappingInfo;
            if (pmi == null) {
                return false;
            }

            if (pmi.ParameterIndex == ParameterIndex && pmi.ActionIndex == ActionIndex) {
                if (Constant != null) {
                    if (pmi.Constant == null) {
                        return false;
                    }

                    return Constant.Value == pmi.Constant.Value;
                } else {
                    return pmi.Constant == null;
                }
            }

            return false;
        }

        public override int GetHashCode() {
            int res = ParameterIndex.GetHashCode() ^ ActionIndex.GetHashCode();
            if (Constant != null) {
                if (Constant.Value != null) {
                    res ^= Constant.Value.GetHashCode();
                }
            }
            return res;
        }

        public override string ToString() {
            if (IsAction) {
                return "Action" + ActionIndex.ToString();
            } else if (IsParameter) {
                return "Parameter" + ParameterIndex.ToString();
            } else {
                object value = Constant.Value;
                if (value == null) {
                    return "(null)";
                }

                return value.ToString();
            }
        }
    }

    /// <summary>
    /// Contains the mapping information for a single Combo Binder.  This includes the original
    /// meta-binder and the mapping of parameters, sub-sites, and constants into the binding.
    /// </summary>
    public class BinderMappingInfo {
        public DynamicMetaObjectBinder Binder;
        public IList<ParameterMappingInfo> MappingInfo;

        public BinderMappingInfo(DynamicMetaObjectBinder binder, IList<ParameterMappingInfo> mappingInfo) {
            Binder = binder;
            MappingInfo = mappingInfo;
        }

        public BinderMappingInfo(DynamicMetaObjectBinder binder, params ParameterMappingInfo[] mappingInfos)
            : this(binder, (IList<ParameterMappingInfo>)mappingInfos) {
        }

        public override string ToString() {
            StringBuilder res = new StringBuilder();
            res.Append(Binder.ToString());
            res.Append(" ");
            string comma = "";
            foreach (ParameterMappingInfo info in MappingInfo) {
                res.Append(comma);
                res.Append(info.ToString());
                comma = ", ";
            }
            return res.ToString();
        }
    }
}
