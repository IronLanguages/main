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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Dynamic.Utils;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class ElementInit : IArgumentProvider {
        private MethodInfo _addMethod;
        private ReadOnlyCollection<Expression> _arguments;

        internal ElementInit(MethodInfo addMethod, ReadOnlyCollection<Expression> arguments) {
            _addMethod = addMethod;
            _arguments = arguments;
        }
        public MethodInfo AddMethod {
            get { return _addMethod; }
        }
        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }

        Expression IArgumentProvider.GetArgument(int index) {
            return _arguments[index];
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return _arguments.Count;
            }
        }

        public override string ToString() {
            return ExpressionStringBuilder.ElementInitBindingToString(this);
        }
    }


    public partial class Expression {
        //CONFORMING
        public static ElementInit ElementInit(MethodInfo addMethod, params Expression[] arguments) {
            return ElementInit(addMethod, arguments as IEnumerable<Expression>);
        }
        //CONFORMING
        public static ElementInit ElementInit(MethodInfo addMethod, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(addMethod, "addMethod");
            ContractUtils.RequiresNotNull(arguments, "arguments");
            RequiresCanRead(arguments, "arguments");
            ValidateElementInitAddMethodInfo(addMethod);
            ReadOnlyCollection<Expression> argumentsRO = arguments.ToReadOnly();
            ValidateArgumentTypes(addMethod, ExpressionType.Call, ref argumentsRO);
            return new ElementInit(addMethod, argumentsRO);
        }

        //CONFORMING
        private static void ValidateElementInitAddMethodInfo(MethodInfo addMethod) {
            ValidateMethodInfo(addMethod);
            ParameterInfo[] pis = addMethod.GetParametersCached();
            if (pis.Length == 0) {
                throw Error.ElementInitializerMethodWithZeroArgs();
            }
            if (!addMethod.Name.Equals("Add", StringComparison.OrdinalIgnoreCase)) {
                throw Error.ElementInitializerMethodNotAdd();
            }
            if (addMethod.IsStatic) {
                throw Error.ElementInitializerMethodStatic();
            }
            foreach (ParameterInfo pi in pis) {
                if (pi.ParameterType.IsByRef) {
                    throw Error.ElementInitializerMethodNoRefOutParam(pi.Name, addMethod.Name);
                }
            }
        }
    }
}
