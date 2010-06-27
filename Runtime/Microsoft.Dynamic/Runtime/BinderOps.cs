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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Dynamic;
using System.Text;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;


namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Helper methods that calls are generated to from the default DLR binders.
    /// </summary>
    public static class BinderOps {


        /// <summary>
        /// Helper function to combine an object array with a sequence of additional parameters that has been splatted for a function call.
        /// </summary>
        public static object[] GetCombinedParameters(object[] initialArgs, object additionalArgs) {
            IList listArgs = additionalArgs as IList;
            if (listArgs == null) {
                IEnumerable ie = additionalArgs as IEnumerable;
                if (ie == null) {
                    throw new InvalidOperationException("args must be iterable");
                }
                listArgs = new List<object>();
                foreach (object o in ie) {
                    listArgs.Add(o);
                }
            }

            object[] res = new object[initialArgs.Length + listArgs.Count];
            Array.Copy(initialArgs, res, initialArgs.Length);
            listArgs.CopyTo(res, initialArgs.Length);
            return res;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")] // TODO: fix
        public static object[] GetCombinedKeywordParameters(object[] initialArgs, IAttributesCollection additionalArgs, ref string[] extraNames) {
            List<object> args = new List<object>(initialArgs);
            List<string> newNames = extraNames == null ? new List<string>(additionalArgs.Count) : new List<string>(extraNames);
            foreach(KeyValuePair<object, object> kvp in additionalArgs) {
                if (kvp.Key is string) {
                    newNames.Add((string)kvp.Key);
                    args.Add(kvp.Value);
                }
            }
            extraNames = newNames.ToArray();
            return args.ToArray();
        }

        public static SymbolDictionary MakeSymbolDictionary(string[] names, object[] values) {
            SymbolDictionary res = new SymbolDictionary();
            for (int i = 0; i < names.Length; i++) {
                ((IAttributesCollection)res)[SymbolTable.StringToId(names[i])] = values[i];
            }
            return res;
        }

        public static Dictionary<TKey, TValue> MakeDictionary<TKey, TValue>(string[] names, object[] values) {
            Debug.Assert(typeof(TKey) == typeof(string) || typeof(TKey) == typeof(object));

            Dictionary<TKey, TValue> res = new Dictionary<TKey, TValue>();
            IDictionary id = (IDictionary)res;
            
            for (int i = 0; i < names.Length; i++) {
                id[names[i]] = values[i];
            }

            return res;
        }
        
        public static ArgumentTypeException BadArgumentsForOperation(ExpressionType op, params object[] args) {
            StringBuilder message = new StringBuilder("unsupported operand type(s) for operation ");
            message.Append(op.ToString());
            message.Append(": ");
            string comma = "";

            foreach (object o in args) {
                message.Append(comma);
                message.Append(CompilerHelpers.GetType(o));
                comma = ", ";
            }

            throw new ArgumentTypeException(message.ToString());
        }


        // formalNormalArgumentCount - does not include FuncDefFlags.ArgList and FuncDefFlags.KwDict
        // defaultArgumentCount - How many arguments in the method declaration have a default value?
        // providedArgumentCount - How many arguments are passed in at the call site?
        // hasArgList - Is the method declaration of the form "foo(*argList)"?
        // keywordArgumentsProvided - Does the call site specify keyword arguments?
        public static ArgumentTypeException TypeErrorForIncorrectArgumentCount(
            string methodName,
            int formalNormalArgumentCount,
            int defaultArgumentCount,
            int providedArgumentCount,
            bool hasArgList,
            bool keywordArgumentsProvided) {
            return TypeErrorForIncorrectArgumentCount(methodName, formalNormalArgumentCount, formalNormalArgumentCount, defaultArgumentCount, providedArgumentCount, hasArgList, keywordArgumentsProvided);
        }

        public static ArgumentTypeException TypeErrorForIncorrectArgumentCount(
            string methodName,
            int minFormalNormalArgumentCount,
            int maxFormalNormalArgumentCount,
            int defaultArgumentCount,
            int providedArgumentCount,
            bool hasArgList,
            bool keywordArgumentsProvided) {

            int formalCount;
            string formalCountQualifier;
            string nonKeyword = keywordArgumentsProvided ? "non-keyword " : "";

            if (defaultArgumentCount > 0 || hasArgList || minFormalNormalArgumentCount != maxFormalNormalArgumentCount) {
                if (providedArgumentCount < minFormalNormalArgumentCount || maxFormalNormalArgumentCount == Int32.MaxValue) {
                    formalCountQualifier = "at least";
                    formalCount = minFormalNormalArgumentCount - defaultArgumentCount;
                } else {
                    formalCountQualifier = "at most";
                    formalCount = maxFormalNormalArgumentCount;
                }
            } else if (minFormalNormalArgumentCount == 0) {
                return ScriptingRuntimeHelpers.SimpleTypeError(string.Format("{0}() takes no arguments ({1} given)", methodName, providedArgumentCount));
            } else {
                formalCountQualifier = "exactly";
                formalCount = minFormalNormalArgumentCount;
            }

            return new ArgumentTypeException(string.Format(
                "{0}() takes {1} {2} {3}argument{4} ({5} given)",
                                methodName, // 0
                                formalCountQualifier, // 1
                                formalCount, // 2
                                nonKeyword, // 3
                                formalCount == 1 ? "" : "s", // 4
                                providedArgumentCount)); // 5
        }

        public static ArgumentTypeException TypeErrorForIncorrectArgumentCount(string name, int formalNormalArgumentCount, int defaultArgumentCount, int providedArgumentCount) {
            return TypeErrorForIncorrectArgumentCount(name, formalNormalArgumentCount, defaultArgumentCount, providedArgumentCount, false, false);
        }

        public static ArgumentTypeException TypeErrorForIncorrectArgumentCount(string name, int expected, int received) {
            return TypeErrorForIncorrectArgumentCount(name, expected, 0, received);
        }

        public static ArgumentTypeException TypeErrorForExtraKeywordArgument(string name, string argumentName) {
            return new ArgumentTypeException(String.Format("{0}() got an unexpected keyword argument '{1}'", name, argumentName));
        }

        public static ArgumentTypeException TypeErrorForDuplicateKeywordArgument(string name, string argumentName) {
            return new ArgumentTypeException(String.Format("{0}() got multiple values for keyword argument '{1}'", name, argumentName));
        }

        public static ArgumentTypeException TypeErrorForNonInferrableMethod(string name) {
            return new ArgumentTypeException(String.Format("The type arguments for method '{0}' cannot be inferred from the usage. Try specifying the type arguments explicitly.", name));
        }

        public static ArgumentTypeException SimpleTypeError(string message) {
            return new ArgumentTypeException(message);
        }

        public static ArgumentTypeException InvalidSplatteeError(string name, string typeName) {
            return new ArgumentTypeException(String.Format("{0}() argument after * must be a sequence, not {1}", name, typeName));
        }

        public static object InvokeMethod(MethodBase mb, object obj, object[] args) {
            try {
                return mb.Invoke(obj, args);
            } catch (TargetInvocationException tie) {
                throw tie.InnerException;
            }
        }

        public static object InvokeConstructor(ConstructorInfo ci, object[] args) {
            try {
                return ci.Invoke(args);
            } catch (TargetInvocationException tie) {
                throw tie.InnerException;
            }
        }

        // TODO: just emit this in the generated code
        public static bool CheckDictionaryMembers(IDictionary dict, string[] names, Type[] types) {
            if (dict.Count != names.Length) return false;

            for (int i = 0; i < names.Length; i++) {
                string name = names[i];

                if (!dict.Contains(name)) {
                    return false;
                }

                if (types != null) {
                    if (CompilerHelpers.GetType(dict[name]) != types[i]) {
                        return false;
                    }
                }
            }
            return true;
        }

        public static IList<string> GetStringMembers(IList<object> members) {
            List<string> res = new List<string>();
            foreach (object o in members) {
                string str = o as string;
                if (str != null) {
                    res.Add(str);
                }
            }
            return res;
        }

        /// <summary>
        /// EventInfo.EventHandlerType getter is marked SecuritySafeCritical in CoreCLR
        /// This method is to get to the property without using Reflection
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <returns></returns>
        public static Type GetEventHandlerType(EventInfo eventInfo) {
            ContractUtils.RequiresNotNull(eventInfo, "eventInfo");
            return eventInfo.EventHandlerType;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        public static void SetEvent(EventTracker eventTracker, object value) {
            EventTracker et = value as EventTracker;
            if (et != null) {
                if (et != eventTracker) {
                    throw new ArgumentException(String.Format("expected event from {0}.{1}, got event from {2}.{3}",
                                                eventTracker.DeclaringType.Name,
                                                eventTracker.Name,
                                                et.DeclaringType.Name,
                                                et.Name));
                }
                return;
            }

            BoundMemberTracker bmt = value as BoundMemberTracker;
            if (bmt == null) throw new ArgumentTypeException("expected bound event, got " + CompilerHelpers.GetType(value).Name);
            if (bmt.BoundTo.MemberType != TrackerTypes.Event) throw new ArgumentTypeException("expected bound event, got " + bmt.BoundTo.MemberType.ToString());

            if (bmt.BoundTo != eventTracker) throw new ArgumentException(String.Format("expected event from {0}.{1}, got event from {2}.{3}",
                eventTracker.DeclaringType.Name,
                eventTracker.Name,
                bmt.BoundTo.DeclaringType.Name,
                bmt.BoundTo.Name));
        }
    }
}
