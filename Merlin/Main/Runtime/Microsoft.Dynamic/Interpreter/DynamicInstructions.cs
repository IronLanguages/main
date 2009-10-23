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
using System.Runtime.CompilerServices;
using System.Reflection;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpreter {
    public static partial class DynamicInstructions {
        private static Dictionary<Type, Func<CallSiteBinder, Instruction>> _factories =
            new Dictionary<Type, Func<CallSiteBinder, Instruction>>();

        public static Instruction MakeInstruction(Type delegateType, CallSiteBinder binder) {
            Func<CallSiteBinder, Instruction> factory;
            lock (_factories) {
                if (!_factories.TryGetValue(delegateType, out factory)) {
                    if (delegateType.GetMethod("Invoke").ReturnType == typeof(void)) {
                        // TODO: We should generally support void returning binders but the only
                        // ones that exist are delete index/member who's perf isn't that critical.
                        return new DynamicInstructionN(delegateType, CallSite.Create(delegateType, binder), true);
                    }

                    Type instructionType = GetDynamicInstructionType(delegateType);
                    if (instructionType == null) {
                        return new DynamicInstructionN(delegateType, CallSite.Create(delegateType, binder));
                    }

                    factory = (Func<CallSiteBinder, Instruction>)Delegate.CreateDelegate(typeof(Func<CallSiteBinder, Instruction>),
                        instructionType.GetMethod("Factory"));
                    _factories[delegateType] = factory;
                }
            }
            return factory(binder);
        }
    }

    public class DynamicInstructionN : Instruction {
        private readonly CallInstruction _target;
        private readonly FieldInfo _targetField;
        private readonly CallSite _site;
        private readonly int _argCount;
        private readonly bool _isVoid;

        public DynamicInstructionN(Type delegateType, CallSite site) {
            var methodInfo = delegateType.GetMethod("Invoke");
            var parameters = methodInfo.GetParameters();

            _target = CallInstruction.Create(methodInfo, parameters);
            _targetField = site.GetType().GetField("Target");
            _site = site;
            _argCount = parameters.Length;
        }

        public DynamicInstructionN(Type delegateType, CallSite site, bool isVoid)
            : this(delegateType, site) {
            _isVoid = isVoid;
        }

        public override int ProducedStack { get { return _isVoid ? 0 : 1; } }
        public override int ConsumedStack { get { return _argCount-1; } }

        public override int Run(InterpretedFrame frame) {
            var targetDelegate = _targetField.GetValue(_site);

            object[] args = new object[_argCount];
            
            for (int i = _argCount - 1; i >= 1; i--) {
                args[i] = frame.Pop();
            }
            args[0] = _site;

            object ret = _target.InvokeInstance(targetDelegate, args);
            if (!_isVoid) frame.Push(ret);
            return +1;
        }

        public override string ToString() {
            return "DynamicInstructionN(" + _site + ")";
        }
    }
}
