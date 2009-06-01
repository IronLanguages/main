/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Dynamic;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Builtins {
    [RubyClass(Extends = typeof(TypeGroup), Restrictions = ModuleRestrictions.None)]
    [Includes(typeof(Enumerable))]
    public class TypeGroupOps {
        
        [RubyMethod("new")]
        public static RuleGenerator/*!*/ GetInstanceConstructor() {
            return new RuleGenerator(RuleGenerators.InstanceConstructorForGroup);
        }

        [RubyMethod("of")]
        [RubyMethod("[]")]
        public static RubyModule/*!*/ Of(RubyContext/*!*/ context, TypeGroup/*!*/ self, [NotNull]params object[]/*!*/ typeArgs) {
            TypeTracker tracker = self.GetTypeForArity(typeArgs.Length);

            if (tracker == null) {
                throw RubyExceptions.CreateArgumentError(String.Format("Invalid number of type arguments for `{0}'", self.Name));
            }

            Type concreteType;
            if (typeArgs.Length > 0) {
                concreteType = tracker.Type.MakeGenericType(Protocols.ToTypes(context, typeArgs));
            } else {
                concreteType = tracker.Type;
            }

            return context.GetModule(concreteType);
        }

        [RubyMethod("each")]
        public static object EachType(RubyContext/*!*/ context, BlockParam/*!*/ block, TypeGroup/*!*/ self) {
            if (block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            foreach (Type type in self.Types) {
                RubyModule module = context.GetModule(type);
                object result;
                if (block.Yield(module, out result)) {
                    return result;
                }
            }

            return self;
        }

        [RubyMethod("name")]
        [RubyMethod("to_s")]
        public static MutableString/*!*/ GetName(TypeGroup/*!*/ self) {
            return MutableString.Create(self.Name);
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, TypeGroup/*!*/ self) {
            var result = MutableString.CreateMutable();
            result.Append("#<TypeGroup: ");

            bool isFirst = true;
            foreach (Type type in self.Types) {
                if (!isFirst) {
                    result.Append(", ");
                } else {
                    isFirst = false;
                }

                result.Append(context.GetTypeName(type, true));
            }
            result.Append(">");

            return result;
        }
    }
}
