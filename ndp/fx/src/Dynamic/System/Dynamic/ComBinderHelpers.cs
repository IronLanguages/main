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

#if !SILVERLIGHT

using System.Diagnostics;
using System.Dynamic;
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Linq.Expressions.Compiler;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace System.Dynamic {
    internal static class ComBinderHelpers {

        internal static bool PreferPut(Type type, bool holdsNull) {
            Debug.Assert(type != null);

            if (type.IsValueType || type.IsArray) return true;

            if (type == typeof(String) ||
                type == typeof(DBNull) ||
                holdsNull ||
                type == typeof(System.Reflection.Missing) ||
                type == typeof(CurrencyWrapper)) {

                return true;
            } else {
                return false;
            }
        }

        internal static bool IsByRef(DynamicMetaObject mo) {
            ParameterExpression pe = mo.Expression as ParameterExpression;
            return pe != null && pe.IsByRef;
        }

        internal static bool IsStrongBoxArg(DynamicMetaObject o, ArgumentInfo argInfo) {
            if (argInfo.IsByRef) {
                return false;
            }

            Type t = o.LimitType;
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(StrongBox<>);
        }

        // this helper checks if we have arginfos and verifies the assumptions that we make about them.
        private static bool HasArgInfos(DynamicMetaObject[] args, IList<ArgumentInfo> argInfos) {
            // We either have valid ArgInfos on the binder or the collection is empty.
            if (argInfos == null || argInfos.Count == 0) {
                return false;
            }

            // Number of arginfos matches number of metaobject arguments.
            if(args.Length != argInfos.Count){
                throw new InvalidOperationException();
            }

            // Named arguments go after positional ones.
            bool seenNonPositional = false;
            for (var i = 0; i < argInfos.Count; i++){
                ArgumentInfo curInfo = argInfos[i];

                PositionalArgumentInfo positional = curInfo as PositionalArgumentInfo;
                if (positional != null) {
                    if (seenNonPositional){
                        throw Error.NamedArgsShouldFollowPositional();
                    }
                    if (positional.Position != i) {
                        throw new InvalidOperationException();
                    }
                } else {
                    seenNonPositional = true;
                }
            }
            return true;
        }

        // this helper prepares arguments for COM binding by transforming ByVal StongBox arguments
        // into ByRef expressions that represent the argument's Value fields.
        internal static void ProcessArgumentsForCom(ref DynamicMetaObject[] args, ref IList<ArgumentInfo> argInfos) {
            Debug.Assert(args != null);
           
            DynamicMetaObject[] newArgs = new DynamicMetaObject[args.Length];
            ArgumentInfo[] newArgInfos = new ArgumentInfo[args.Length];

            bool hasArgInfos = HasArgInfos(args, argInfos);

            for (int i = 0; i < args.Length; i++) {
                DynamicMetaObject curArgument = args[i];

                // set new arg infos to their original values or set default ones
                // we will do this fixup early so that we can assume we always have
                // arginfos in COM binder.
                if (hasArgInfos) {
                    newArgInfos[i] = argInfos[i];
                } else {
                    // TODO: this fixup should not be needed once refness is expressed only by argInfos
                    if (IsByRef(curArgument)) {
                        newArgInfos[i] = Expression.ByRefPositionalArgument(i);
                    } else {
                        newArgInfos[i] = Expression.PositionalArg(i);
                    }
                }

                if (IsStrongBoxArg(curArgument, newArgInfos[i])) {


                    var restrictions = curArgument.Restrictions.Merge(
                        GetTypeRestrictionForDynamicMetaObject(curArgument)
                    );

                    // we have restricted this argument to LimitType so we can convert and conversion will be trivial cast.
                    Expression boxedValueAccessor = Expression.Field(
                        Helpers.Convert(
                            curArgument.Expression,
                            curArgument.LimitType
                        ),
                        curArgument.LimitType.GetField("Value")
                    );

                    IStrongBox value = curArgument.Value as IStrongBox;
                    object boxedValue = value != null ? value.Value : null;

                    newArgs[i] = new DynamicMetaObject(
                        boxedValueAccessor,
                        restrictions,
                        boxedValue
                    );

                    NamedArgumentInfo nai = newArgInfos[i] as NamedArgumentInfo;
                    if (nai != null) {
                        newArgInfos[i] = Expression.ByRefNamedArgument(nai.Name);
                    } else {
                        newArgInfos[i] = Expression.ByRefPositionalArgument(i);
                    }
                } else {
                    newArgs[i] = curArgument;
                }
            }

            args = newArgs;
            argInfos = newArgInfos;
        }

        internal static BindingRestrictions GetTypeRestrictionForDynamicMetaObject(DynamicMetaObject obj) {
            if (obj.Value == null && obj.HasValue) {
                //If the meta object holds a null value, create an instance restriction for checking null
                return BindingRestrictions.GetInstanceRestriction(obj.Expression, null);
            }
            return BindingRestrictions.GetTypeRestriction(obj.Expression, obj.LimitType);
        }
    }
}

#endif
