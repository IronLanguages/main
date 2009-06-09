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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Dynamic;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// Provides a MetaObject for instances of Python's old-style classes.
    /// 
    /// TODO: Lots of CodeConetxt references, need to move CodeContext onto OldClass and pull it from there.
    /// </summary>
    class MetaOldInstance : MetaPythonObject, IPythonInvokable, IPythonGetable, IPythonOperable, IPythonConvertible {        
        public MetaOldInstance(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, OldInstance/*!*/ value)
            : base(expression, BindingRestrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region IPythonInvokable Members

        public DynamicMetaObject/*!*/ Invoke(PythonInvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, codeContext, args);
        }

        #endregion

        #region IPythonGetable Members

        public DynamicMetaObject GetMember(PythonGetMemberBinder member, DynamicMetaObject codeContext) {
            // no codeContext filtering but avoid an extra site by handling this action directly
            return MakeMemberAccess(member, member.Name, MemberAccess.Get, this);
        }

        #endregion

        #region MetaObject Overrides

        public override DynamicMetaObject/*!*/ BindInvokeMember(InvokeMemberBinder/*!*/ action, DynamicMetaObject/*!*/[]/*!*/ args) {
            return MakeMemberAccess(action, action.Name, MemberAccess.Invoke, args);
        }

        public override DynamicMetaObject/*!*/ BindGetMember(GetMemberBinder/*!*/ member) {
            return MakeMemberAccess(member, member.Name, MemberAccess.Get, this);
        }

        public override DynamicMetaObject/*!*/ BindSetMember(SetMemberBinder/*!*/ member, DynamicMetaObject/*!*/ value) {
            return MakeMemberAccess(member, member.Name, MemberAccess.Set, this, value);
        }

        public override DynamicMetaObject/*!*/ BindDeleteMember(DeleteMemberBinder/*!*/ member) {
            return MakeMemberAccess(member, member.Name, MemberAccess.Delete, this);
        }

        public override DynamicMetaObject/*!*/ BindBinaryOperation(BinaryOperationBinder/*!*/ binder, DynamicMetaObject/*!*/ arg) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "OldClass BinaryOperation" + binder.Operation);
            PerfTrack.NoteEvent(PerfTrack.Categories.BindingTarget, "OldClass BinaryOperation");
            return PythonProtocol.Operation(binder, this, arg, null);
        }

        public override DynamicMetaObject/*!*/ BindUnaryOperation(UnaryOperationBinder/*!*/ binder) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "OldClass UnaryOperation" + binder.Operation);
            PerfTrack.NoteEvent(PerfTrack.Categories.BindingTarget, "OldClass UnaryOperation");
            return PythonProtocol.Operation(binder, this);
        }

        public override DynamicMetaObject/*!*/ BindGetIndex(GetIndexBinder/*!*/ binder, DynamicMetaObject/*!*/[]/*!*/ indexes) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "OldClass GetIndex" + indexes.Length);
            PerfTrack.NoteEvent(PerfTrack.Categories.BindingTarget, "OldClass GetIndex");
            return PythonProtocol.Index(binder, PythonIndexType.GetItem, ArrayUtils.Insert(this, indexes));
        }

        public override DynamicMetaObject/*!*/ BindSetIndex(SetIndexBinder/*!*/ binder, DynamicMetaObject/*!*/[]/*!*/ indexes, DynamicMetaObject/*!*/ value) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "OldClass SetIndex" + indexes.Length);
            PerfTrack.NoteEvent(PerfTrack.Categories.BindingTarget, "OldClass SetIndex");
            return PythonProtocol.Index(binder, PythonIndexType.SetItem, ArrayUtils.Insert(this, ArrayUtils.Append(indexes, value)));
        }

        public override DynamicMetaObject/*!*/ BindDeleteIndex(DeleteIndexBinder/*!*/ binder, DynamicMetaObject/*!*/[]/*!*/ indexes) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "OldClass DeleteIndex" + indexes.Length);
            PerfTrack.NoteEvent(PerfTrack.Categories.BindingTarget, "OldClass DeleteIndex");
            return PythonProtocol.Index(binder, PythonIndexType.DeleteItem, ArrayUtils.Insert(this, indexes));
        }
        
        public override DynamicMetaObject BindConvert(ConvertBinder/*!*/ conversion) {
            return ConvertWorker(conversion, conversion.Type, conversion.Type, conversion.Explicit ? ConversionResultKind.ExplicitCast : ConversionResultKind.ImplicitCast);
        }

        public DynamicMetaObject BindConvert(PythonConversionBinder binder) {
            return ConvertWorker(binder, binder.Type, binder.ReturnType, binder.ResultKind);
        }

        public DynamicMetaObject ConvertWorker(DynamicMetaObjectBinder binder, Type type, Type retType, ConversionResultKind kind) {
            if (!type.IsEnum) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Boolean:
                        return MakeConvertToBool(binder);
                    case TypeCode.Int32:
                        return MakeConvertToCommon(binder, type, retType, Symbols.ConvertToInt);
                    case TypeCode.Double:
                        return MakeConvertToCommon(binder, type, retType, Symbols.ConvertToFloat);
                    case TypeCode.String:
                        return MakeConvertToCommon(binder, type, retType, Symbols.String);
                    case TypeCode.Object:
                        if (type == typeof(BigInteger)) {
                            return MakeConvertToCommon(binder, type, retType, Symbols.ConvertToLong);
                        } else if (type == typeof(Complex64)) {
                            return MakeConvertToCommon(binder, type, retType, Symbols.ConvertToComplex);
                        } else if (type == typeof(IEnumerable)) {
                            return MakeConvertToIEnumerable(binder);
                        } else if (type == typeof(IEnumerator)) {
                            return MakeConvertToIEnumerator(binder);
                        } else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                            return MakeConvertToIEnumerable(binder, type, type.GetGenericArguments()[0]);
                        } else if (type.IsSubclassOf(typeof(Delegate))) {
                            return MakeDelegateTarget(binder, type, Restrict(typeof(OldInstance)));
                        }

                        break;
                }
            }

            return FallbackConvert(binder);
        }

        public override DynamicMetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ invoke, params DynamicMetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(invoke, PythonContext.GetCodeContext(invoke), args);
        }

        public override System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames() {
            foreach (object o in ((IMembersList)Value).GetMemberNames(DefaultContext.Default)) {
                if (o is string) {
                    yield return (string)o;
                }
            }
        }

        #endregion

        #region Invoke Implementation

        private DynamicMetaObject/*!*/ InvokeWorker(DynamicMetaObjectBinder/*!*/ invoke, Expression/*!*/ codeContext, DynamicMetaObject/*!*/[] args) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "OldClass Invoke");

            DynamicMetaObject self = Restrict(typeof(OldInstance));

            Expression[] exprArgs = new Expression[args.Length + 1];
            for (int i = 0; i < args.Length; i++) {
                exprArgs[i + 1] = args[i].Expression;
            }

            ParameterExpression tmp = Ast.Variable(typeof(object), "callFunc");

            exprArgs[0] = tmp;
            return new DynamicMetaObject(
                // we could get better throughput w/ a more specific rule against our current custom old class but
                // this favors less code generation.

                Ast.Block(
                    new ParameterExpression[] { tmp },
                    Ast.Condition(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("OldInstanceTryGetBoundCustomMember"),
                            codeContext,
                            self.Expression,
                            AstUtils.Constant(Symbols.Call),
                            tmp
                        ),
                        Ast.Block(
                            Utils.Try(
                                Ast.Call(typeof(PythonOps).GetMethod("FunctionPushFrame")),
                                Ast.Assign(
                                    tmp,
                                    Ast.Dynamic(
                                        PythonContext.GetPythonContext(invoke).Invoke(
                                            BindingHelpers.GetCallSignature(invoke)
                                        ),
                                        typeof(object),
                                        ArrayUtils.Insert(codeContext, exprArgs)
                                    )
                                )
                            ).Finally(
                                Ast.Call(typeof(PythonOps).GetMethod("FunctionPopFrame"))
                            ),
                            tmp
                        ),
                        Utils.Convert(
                            BindingHelpers.InvokeFallback(invoke, codeContext, this, args).Expression,
                            typeof(object)
                        )
                    )
                ),
                self.Restrictions.Merge(BindingRestrictions.Combine(args))
            );
        }        

        #endregion

        #region Conversions

        private DynamicMetaObject/*!*/ MakeConvertToIEnumerable(DynamicMetaObjectBinder/*!*/ conversion) {
            ParameterExpression tmp = Ast.Variable(typeof(IEnumerable), "res");
            DynamicMetaObject self = Restrict(typeof(OldInstance));

            return new DynamicMetaObject(
                Ast.Block(
                    new ParameterExpression[] { tmp },
                    Ast.Condition(
                        Ast.NotEqual(
                            Ast.Assign(
                                tmp,
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("OldInstanceConvertToIEnumerableNonThrowing"),
                                    AstUtils.Constant(PythonContext.GetPythonContext(conversion).SharedContext),
                                    self.Expression
                                )
                            ),
                            AstUtils.Constant(null)
                        ),
                        tmp,
                        AstUtils.Convert(
                            AstUtils.Convert(  // first to object (incase it's a throw), then to IEnumerable
                                FallbackConvert(conversion).Expression,
                                typeof(object)
                            ),
                            typeof(IEnumerable)
                        )
                    )
                ),
                self.Restrictions
            );
        }

        private DynamicMetaObject/*!*/ MakeConvertToIEnumerator(DynamicMetaObjectBinder/*!*/ conversion) {
            ParameterExpression tmp = Ast.Variable(typeof(IEnumerator), "res");
            DynamicMetaObject self = Restrict(typeof(OldInstance));

            return new DynamicMetaObject(
                Ast.Block(
                    new ParameterExpression[] { tmp },
                    Ast.Condition(
                        Ast.NotEqual(
                            Ast.Assign(
                                tmp,
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("OldInstanceConvertToIEnumeratorNonThrowing"),
                                    AstUtils.Constant(PythonContext.GetPythonContext(conversion).SharedContext),
                                    self.Expression
                                )
                            ),
                            AstUtils.Constant(null)
                        ),
                        tmp,
                        AstUtils.Convert(
                            AstUtils.Convert(
                                FallbackConvert(conversion).Expression,
                                typeof(object)
                            ),
                            typeof(IEnumerator)
                        )
                    )
                ),
                self.Restrictions
            );            
        }

        private DynamicMetaObject/*!*/ MakeConvertToIEnumerable(DynamicMetaObjectBinder/*!*/ conversion, Type toType, Type genericType) {
            ParameterExpression tmp = Ast.Variable(toType, "res");
            DynamicMetaObject self = Restrict(typeof(OldInstance));

            return new DynamicMetaObject(
                Ast.Block(
                    new ParameterExpression[] { tmp },
                    Ast.Condition(
                        Ast.NotEqual(
                            Ast.Assign(
                                tmp,
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("OldInstanceConvertToIEnumerableOfTNonThrowing").MakeGenericMethod(genericType),
                                    AstUtils.Constant(PythonContext.GetPythonContext(conversion).SharedContext),
                                    self.Expression                                   
                                )
                            ),
                            AstUtils.Constant(null)
                        ),
                        tmp,
                        AstUtils.Convert(
                            AstUtils.Convert(
                                FallbackConvert(conversion).Expression,
                                typeof(object)
                            ),
                            toType
                        )
                    )
                ),
                self.Restrictions
            );                       
        }

        private DynamicMetaObject/*!*/ MakeConvertToCommon(DynamicMetaObjectBinder/*!*/ conversion, Type toType, Type retType, SymbolId symbolId) {
            // TODO: support trys
            ParameterExpression tmp = Ast.Variable(typeof(object), "convertResult");
            DynamicMetaObject self = Restrict(typeof(OldInstance));
            return new DynamicMetaObject(
                Ast.Block(
                    new ParameterExpression[] { tmp },
                    Ast.Condition(
                        MakeOneConvert(conversion, self, symbolId, tmp),
                        Expression.Convert(
                            tmp,
                            retType
                        ),
                        FallbackConvert(conversion).Expression
                    )
                ),
                self.Restrictions
            );
        }

        private static BinaryExpression/*!*/ MakeOneConvert(DynamicMetaObjectBinder/*!*/ conversion, DynamicMetaObject/*!*/ self, SymbolId symbolId, ParameterExpression/*!*/ tmp) {
            return Ast.NotEqual(
                Ast.Assign(
                    tmp,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceConvertNonThrowing"),
                        AstUtils.Constant(PythonContext.GetPythonContext(conversion).SharedContext),
                        self.Expression,
                        AstUtils.Constant(symbolId)
                    )
                ),
                AstUtils.Constant(null)
            );
        }

        private DynamicMetaObject/*!*/ MakeConvertToBool(DynamicMetaObjectBinder/*!*/ conversion) {
            DynamicMetaObject self = Restrict(typeof(OldInstance));

            ParameterExpression tmp = Ast.Variable(typeof(bool?), "tmp");
            DynamicMetaObject fallback = FallbackConvert(conversion);
            Type resType = BindingHelpers.GetCompatibleType(typeof(bool), fallback.Expression.Type);

            return new DynamicMetaObject(
                Ast.Block(
                    new ParameterExpression[] { tmp },
                    Ast.Condition(
                        Ast.NotEqual(
                            Ast.Assign(
                                tmp,
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("OldInstanceConvertToBoolNonThrowing"),
                                    AstUtils.Constant(PythonContext.GetPythonContext(conversion).SharedContext),
                                    self.Expression
                                )
                            ),
                            AstUtils.Constant(null)
                        ),
                        AstUtils.Convert(tmp, resType),
                        AstUtils.Convert(fallback.Expression, resType)
                    )
                ),
                self.Restrictions
            );
        }

        #endregion

        #region Member Access

        private DynamicMetaObject/*!*/ MakeMemberAccess(DynamicMetaObjectBinder/*!*/ member, string name, MemberAccess access, params DynamicMetaObject/*!*/[]/*!*/ args) {
            DynamicMetaObject self = Restrict(typeof(OldInstance));

            CustomOldClassDictionaryStorage dict;
            int key = GetCustomStorageSlot(name, out dict);
            if (key == -1) {
                PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "OldInstance " + access + " NoOptimized"); 
                return MakeDynamicMemberAccess(member, name, access, args);
            }

            ParameterExpression tmp = Ast.Variable(typeof(object), "dict");
            Expression target;

            ValidationInfo test = new ValidationInfo(
                Ast.NotEqual(
                    Ast.Assign(
                        tmp, 
                        Ast.Call(
                            typeof(PythonOps).GetMethod("OldInstanceGetOptimizedDictionary"),
                            self.Expression,
                            AstUtils.Constant(dict.KeyVersion)
                        )
                    ), 
                    AstUtils.Constant(null)
                )
            );
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "OldInstance " + access + " Optimized"); 
            switch (access) {
                case MemberAccess.Invoke:
                    ParameterExpression value = Ast.Variable(typeof(object), "value");
                    target = Ast.Block(
                        new[] { value },
                        Ast.Condition(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("TryOldInstanceDictionaryGetValueHelper"),
                                tmp,
                                Ast.Constant(key),
                                AstUtils.Convert(Expression, typeof(object)),
                                value
                            ),
                            AstUtils.Convert(
                                ((InvokeMemberBinder)member).FallbackInvoke(new DynamicMetaObject(value, BindingRestrictions.Empty), args, null).Expression,
                                typeof(object)
                            ),
                            AstUtils.Convert(
                                ((InvokeMemberBinder)member).FallbackInvokeMember(self, args).Expression,
                                typeof(object)
                            )
                        )
                    );
                    break;
                case MemberAccess.Get:
                    // BUG: There's a missing Fallback path here that's always been present even
                    // in the version that used rules.
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceDictionaryGetValueHelper"),
                        tmp,
                        AstUtils.Constant(key),
                        AstUtils.Convert(Expression, typeof(object))
                    );
                    break;
                case MemberAccess.Set:
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceDictionarySetExtraValue"),
                        tmp,
                        AstUtils.Constant(key),
                        AstUtils.Convert(args[1].Expression, typeof(object))
                    );
                    break;
                case MemberAccess.Delete:
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceDeleteCustomMember"),
                        AstUtils.Constant(PythonContext.GetPythonContext(member).SharedContext),
                        AstUtils.Convert(Expression, typeof(OldInstance)),
                        AstUtils.Constant(SymbolTable.StringToId(name))
                    );
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return BindingHelpers.AddDynamicTestAndDefer(
                member,
                new DynamicMetaObject(
                    target,
                    BindingRestrictions.Combine(args).Merge(self.Restrictions)
                ),
                args,
                test,
                tmp
            );                            
        }

        private int GetCustomStorageSlot(string name, out CustomOldClassDictionaryStorage dict) {
            dict = Value.Dictionary._storage as CustomOldClassDictionaryStorage;
            if (dict == null || Value._class.HasSetAttr) {
                return -1;
            }

            return dict.FindKey(name);
        }

        private enum MemberAccess {
            Get,
            Set,
            Delete,
            Invoke
        }
        
        private DynamicMetaObject/*!*/ MakeDynamicMemberAccess(DynamicMetaObjectBinder/*!*/ member, string/*!*/ name, MemberAccess access, DynamicMetaObject/*!*/[]/*!*/ args) {
            DynamicMetaObject self = Restrict(typeof(OldInstance));
            Expression target;
            SymbolId symName = SymbolTable.StringToId(name);

            ParameterExpression tmp = Ast.Variable(typeof(object), "result");

            switch (access) {
                case MemberAccess.Invoke:

                    target = Ast.Block(
                        new ParameterExpression[] { tmp },
                        Ast.Condition(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("OldInstanceTryGetBoundCustomMember"),
                                Ast.Constant(PythonContext.GetPythonContext(member).SharedContext),
                                self.Expression,
                                AstUtils.Constant(symName),
                                tmp
                            ),
                            ((InvokeMemberBinder)member).FallbackInvoke(new DynamicMetaObject(tmp, BindingRestrictions.Empty), args, null).Expression,
                            AstUtils.Convert(                            
                                ((InvokeMemberBinder)member).FallbackInvokeMember(this, args).Expression,
                                typeof(object)
                            )
                        )
                    );
                    break;
                case MemberAccess.Get:                    
                    target = Ast.Block(
                        new ParameterExpression[] { tmp },
                        Ast.Condition(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("OldInstanceTryGetBoundCustomMember"),
                                AstUtils.Constant(PythonContext.GetPythonContext(member).SharedContext),
                                self.Expression,
                                AstUtils.Constant(symName),
                                tmp
                            ),
                            tmp,
                            AstUtils.Convert(
                                FallbackGet(member, args),
                                typeof(object)
                            )
                        )
                    );                    
                    break;
                case MemberAccess.Set:
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceSetCustomMember"),
                        AstUtils.Constant(PythonContext.GetPythonContext(member).SharedContext),
                        self.Expression,
                        AstUtils.Constant(symName),
                        AstUtils.Convert(args[1].Expression, typeof(object))
                    );
                    break;
                case MemberAccess.Delete:
                    target = Ast.Call(
                        typeof(PythonOps).GetMethod("OldInstanceDeleteCustomMember"),
                        AstUtils.Constant(PythonContext.GetPythonContext(member).SharedContext),
                        self.Expression,
                        AstUtils.Constant(symName)
                    );
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return new DynamicMetaObject(
                target,
                self.Restrictions.Merge(BindingRestrictions.Combine(args))
            );
        }

        private Expression FallbackGet(DynamicMetaObjectBinder member, DynamicMetaObject[] args) {
            GetMemberBinder sa = member as GetMemberBinder;
            if (sa != null) {
                return sa.FallbackGetMember(args[0]).Expression;
            }

            return ((PythonGetMemberBinder)member).Fallback(args[0], PythonContext.GetCodeContextMO(member)).Expression;
        }

        #endregion

        #region Helpers

        public new OldInstance/*!*/ Value {
            get {
                return (OldInstance)base.Value;
            }
        }

        #endregion

        #region IPythonOperable Members

        DynamicMetaObject IPythonOperable.BindOperation(PythonOperationBinder action, DynamicMetaObject[] args) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "OldClass PythonOperation " + action.Operation);

            if (action.Operation == PythonOperationKind.IsCallable) {
                return MakeIsCallable(action);
            }

            return null;
        }

        private DynamicMetaObject/*!*/ MakeIsCallable(PythonOperationBinder/*!*/ operation) {
            DynamicMetaObject self = Restrict(typeof(OldInstance));

            return new DynamicMetaObject(
                Ast.Call(
                    typeof(PythonOps).GetMethod("OldInstanceIsCallable"),
                    AstUtils.Constant(PythonContext.GetPythonContext(operation).SharedContext),
                    self.Expression
                ),
                self.Restrictions
            );
        }


        #endregion
    }
}
