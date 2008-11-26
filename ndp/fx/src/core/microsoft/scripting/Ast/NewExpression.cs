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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Dynamic.Binders;
using System.Dynamic.Utils;
using System.Text;

namespace System.Linq.Expressions {
    //CONFORMING
    public class NewExpression : Expression, IArgumentProvider {
        private readonly ConstructorInfo _constructor;
        private IList<Expression> _arguments;
        private readonly ReadOnlyCollection<MemberInfo> _members;

        internal NewExpression(ConstructorInfo constructor, IList<Expression> arguments, ReadOnlyCollection<MemberInfo> members) {
            _constructor = constructor;
            _arguments = arguments;
            _members = members;
        }

        protected override Type GetExpressionType() {
            return _constructor.DeclaringType;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.New;
        }

        public ConstructorInfo Constructor {
            get { return _constructor; }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return ReturnReadOnly(ref _arguments); }
        }

        Expression IArgumentProvider.GetArgument(int index) {
            return _arguments[index];
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return _arguments.Count;
            }
        }

        public ReadOnlyCollection<MemberInfo> Members {
            get { return _members; }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitNew(this);
        }
    }

    internal class NewValueTypeExpression : NewExpression {
        private readonly Type _valueType;

        internal NewValueTypeExpression(Type type, ReadOnlyCollection<Expression> arguments, ReadOnlyCollection<MemberInfo> members)
            : base(null, arguments, members) {
            _valueType = type;
        }

        protected override Type GetExpressionType() {
            return _valueType;
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        //CONFORMING
        public static NewExpression New(ConstructorInfo constructor) {
            return New(constructor, (IEnumerable<Expression>)null);
        }

        //CONFORMING
        public static NewExpression New(ConstructorInfo constructor, params Expression[] arguments) {
            return New(constructor, (IEnumerable<Expression>)arguments);
        }

        //CONFORMING
        public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(constructor, "constructor");
            ContractUtils.RequiresNotNull(constructor.DeclaringType, "constructor.DeclaringType");
            TypeUtils.ValidateType(constructor.DeclaringType);
            ReadOnlyCollection<Expression> argList = arguments.ToReadOnly();
            ValidateArgumentTypes(constructor, ExpressionType.New, ref argList);

            return new NewExpression(constructor, argList, null);
        }

        //CONFORMING
        public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, IEnumerable<MemberInfo> members) {
            ContractUtils.RequiresNotNull(constructor, "constructor");
            ReadOnlyCollection<MemberInfo> memberList = members.ToReadOnly();
            ReadOnlyCollection<Expression> argList = arguments.ToReadOnly();
            ValidateNewArgs(constructor, ref argList, ref memberList);
            return new NewExpression(constructor, argList, memberList);
        }

        //CONFORMING
        public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, params MemberInfo[] members) {
            return New(constructor, arguments, members.ToReadOnly());
        }

        //CONFORMING
        public static NewExpression New(Type type) {
            ContractUtils.RequiresNotNull(type, "type");
            if (type == typeof(void)) {
                throw Error.ArgumentCannotBeOfTypeVoid();
            }
            ConstructorInfo ci = null;
            if (!type.IsValueType) {
                ci = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, System.Type.EmptyTypes, null);
                if (ci == null) {
                    throw Error.TypeMissingDefaultConstructor(type);
                }
                return New(ci);
            }
            return new NewValueTypeExpression(type, EmptyReadOnlyCollection<Expression>.Instance, null);
        }


        //CONFORMING
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void ValidateNewArgs(ConstructorInfo constructor, ref ReadOnlyCollection<Expression> arguments, ref ReadOnlyCollection<MemberInfo> members) {
            ParameterInfo[] pis;
            if ((pis = constructor.GetParametersCached()).Length > 0) {
                if (arguments.Count != pis.Length) {
                    throw Error.IncorrectNumberOfConstructorArguments();
                }
                if (arguments.Count != members.Count) {
                    throw Error.IncorrectNumberOfArgumentsForMembers();
                }
                Expression[] newArguments = null;
                MemberInfo[] newMembers = null;
                for (int i = 0, n = arguments.Count; i < n; i++) {
                    Expression arg = arguments[i];
                    RequiresCanRead(arg, "argument");
                    MemberInfo member = members[i];
                    ContractUtils.RequiresNotNull(member, "member");
                    if (member.DeclaringType != constructor.DeclaringType) {
                        throw Error.ArgumentMemberNotDeclOnType(member.Name, constructor.DeclaringType.Name);
                    }
                    Type memberType;
                    ValidateAnonymousTypeMember(ref member, out memberType);
                    if (!TypeUtils.AreReferenceAssignable(memberType, arg.Type)) {
                        if (TypeUtils.IsSameOrSubclass(typeof(Expression), memberType) && memberType.IsAssignableFrom(arg.GetType())) {
                            arg = Expression.Quote(arg);
                        } else {
                            throw Error.ArgumentTypeDoesNotMatchMember(arg.Type, memberType);
                        }
                    }
                    ParameterInfo pi = pis[i];
                    Type pType = pi.ParameterType;
                    if (pType.IsByRef) {
                        pType = pType.GetElementType();
                    }
                    if (!TypeUtils.AreReferenceAssignable(pType, arg.Type)) {
                        if (TypeUtils.IsSameOrSubclass(typeof(Expression), pType) && pType.IsAssignableFrom(arg.Type)) {
                            arg = Expression.Quote(arg);
                        } else {
                            throw Error.ExpressionTypeDoesNotMatchConstructorParameter(arg.Type, pType);
                        }
                    }
                    if (newArguments == null && arg != arguments[i]) {
                        newArguments = new Expression[arguments.Count];
                        for (int j = 0; j < i; j++) {
                            newArguments[j] = arguments[j];
                        }
                    }
                    if (newArguments != null) {
                        newArguments[i] = arg;
                    }

                    if (newMembers == null && member != members[i]) {
                        newMembers = new MemberInfo[members.Count];
                        for (int j = 0; j < i; j++) {
                            newMembers[j] = members[j];
                        }
                    }
                    if (newMembers != null) {
                        newMembers[i] = member;
                    }
                }
                if (newArguments != null) {
                    arguments = new ReadOnlyCollection<Expression>(newArguments);
                }
                if (newMembers != null) {
                    members = new ReadOnlyCollection<MemberInfo>(newMembers);
                }
            } else if (arguments != null && arguments.Count > 0) {
                throw Error.IncorrectNumberOfConstructorArguments();
            } else if (members != null && members.Count > 0) {
                throw Error.IncorrectNumberOfMembersForGivenConstructor();
            }
        }

        //CONFORMING
        private static void ValidateAnonymousTypeMember(ref MemberInfo member, out Type memberType) {
            switch (member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo field = member as FieldInfo;
                    if (field.IsStatic) {
                        throw Error.ArgumentMustBeInstanceMember();
                    }
                    memberType = field.FieldType;
                    break;
                case MemberTypes.Property:
                    PropertyInfo pi = member as PropertyInfo;
                    if (!pi.CanRead) {
                        throw Error.PropertyDoesNotHaveGetter(pi);
                    }
                    if (pi.GetGetMethod().IsStatic) {
                        throw Error.ArgumentMustBeInstanceMember();
                    }
                    memberType = pi.PropertyType;
                    break;
                case MemberTypes.Method:
                    MethodInfo method = member as MethodInfo;
                    if (method.IsStatic) {
                        throw Error.ArgumentMustBeInstanceMember();
                    }

                    PropertyInfo prop = GetProperty(method);
                    member = prop;
                    memberType = prop.PropertyType;
                    break;
                default:
                    throw Error.ArgumentMustBeFieldInfoOrPropertInfoOrMethod();
            }
        }
    }
}
