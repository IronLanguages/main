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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting;

namespace IronRuby.Builtins {

    [RubyClass("Module", Extends = typeof(RubyModule), Inherits = typeof(Object))]
    public static class ModuleOps {

        #region CLR extensions

        [RubyMethod("to_clr_type")]
        public static Type ToClrType(RubyModule/*!*/ self) {
            return self.Tracker != null ? self.Tracker.Type : null;
        }

        #endregion

        #region Private Instance Methods

        #region class_variable_get, class_variable_set, remove_class_variable, remove_const

        [RubyMethod("class_variable_get", RubyMethodAttributes.PrivateInstance)]
        public static object GetClassVariable(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ variableName) {
            object value;
            if (self.TryResolveClassVariable(variableName, out value) == null) {
                RubyUtils.CheckClassVariableName(variableName);
                throw RubyExceptions.CreateNameError(String.Format("uninitialized class variable {0} in {1}", variableName, self.Name));
            }
            return value;
        }

        [RubyMethod("class_variable_set", RubyMethodAttributes.PrivateInstance)]
        public static object ClassVariableSet(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ variableName, object value) {
            RubyUtils.CheckClassVariableName(variableName);
            self.SetClassVariable(variableName, value);
            return value;
        }
        
        [RubyMethod("remove_class_variable", RubyMethodAttributes.PrivateInstance)]
        public static object RemoveClassVariable(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ variableName) {
            object value;
            if (!self.TryGetClassVariable(variableName, out value)) {
                RubyUtils.CheckClassVariableName(variableName);
                throw RubyExceptions.CreateNameError(String.Format("class variable {0} not defined for {1}", variableName, self.Name));
            }
            self.RemoveClassVariable(variableName);
            return value;
        }

        [RubyMethod("remove_const", RubyMethodAttributes.PrivateInstance)]
        public static object RemoveConstant(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ constantName) {
            object value;
            if (!self.TryGetConstantNoAutoload(constantName, out value)) {
                RubyUtils.CheckConstantName(constantName);
                throw RubyExceptions.CreateNameError(String.Format("constant {0}::{1} not defined", self.Name, constantName));
            }
            self.RemoveConstant(constantName);
            return value;
        }

        #endregion

        #region extend_object, extended, include, included

        [RubyMethod("extend_object", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ ExtendObject(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ extendedModule) {
            // include self into extendedModule's singleton class
            extendedModule.SingletonClass.IncludeModules(self);
            return self;
        }
        
        [RubyMethod("extend_object", RubyMethodAttributes.PrivateInstance)]
        public static object ExtendObject(RubyModule/*!*/ self, object extendedObject) {
            // include self into extendedObject's singleton
            self.Context.CreateSingletonClass(extendedObject).IncludeModules(self);
            return extendedObject;
        }

        [RubyMethod("extended", RubyMethodAttributes.PrivateInstance)]
        public static void ObjectExtended(RubyModule/*!*/ self, object extendedObject) {
            // extendedObject has been extended by self, i.e. self has been included into extendedObject's singleton class
        }

        [RubyMethod("include", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ Include(
            CallSiteStorage<Func<CallSite, RubyContext, RubyModule, RubyModule, object>>/*!*/ appendFeaturesStorage,
            CallSiteStorage<Func<CallSite, RubyContext, RubyModule, RubyModule, object>>/*!*/ includedStorage,
            RubyModule/*!*/ self, [NotNull]params RubyModule[]/*!*/ modules) {

            RubyUtils.RequireMixins(self, modules);

            var appendFeatures = appendFeaturesStorage.GetCallSite("append_features", 1);
            var included = includedStorage.GetCallSite("included", 1);
            
            // Kernel#append_features inserts the module at the beginning of ancestors list;
            // ancestors after include: [modules[0], modules[1], ..., modules[N-1], self, ...]
            for (int i = modules.Length - 1; i >= 0; i--) {
                appendFeatures.Target(appendFeatures, self.Context, modules[i], self);
                included.Target(included, self.Context, modules[i], self);
            }

            return self;
        }

        [RubyMethod("included", RubyMethodAttributes.PrivateInstance)]
        public static void Included(RubyModule/*!*/ self, RubyModule/*!*/ owner) {
            // self has been included into owner
        }

        [RubyMethod("append_features", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ AppendFeatures(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ owner) {
            owner.IncludeModules(self);
            return self;
        }
        
        #endregion

        #region initialize, initialize_copy

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static object Reinitialize(BlockParam block, RubyModule/*!*/ self) {
            // no class can be reinitialized:
            if (self.IsClass) {
                throw RubyExceptions.CreateTypeError("already initialized class");
            }

            return (block != null) ? RubyUtils.EvaluateInModule(self, block, null) : null;
        }

        [RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ InitializeCopy(RubyModule/*!*/ self, object other) {
            // no class can be reinitialized:
            if (self.IsClass) {
                throw RubyExceptions.CreateTypeError("already initialized class");
            }
            
            // self could be a meta-module:
            RubyClass selfClass = self.Context.GetClassOf(self);
            RubyClass otherClass = self.Context.GetClassOf(other);
            if (otherClass != selfClass) {
                throw RubyExceptions.CreateTypeError("initialize_copy should take same class object");
            }

            self.InitializeModuleCopy((RubyModule)other);
            return self;
        }
        
        #endregion

        #region private, protected, public, module_function

        [RubyMethod("private", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ SetPrivateVisibility(RubyScope/*!*/ scope, RubyModule/*!*/ self, [NotNull]params object[]/*!*/ methodNames) {
            // overwrites methods to instance:
            SetMethodAttributes(scope, self, methodNames, RubyMethodAttributes.PrivateInstance);
            return self;
        }

        [RubyMethod("protected", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ SetProtectedVisibility(RubyScope/*!*/ scope, RubyModule/*!*/ self, [NotNull]params object[]/*!*/ methodNames) {
            // overwrites methods to instance:
            SetMethodAttributes(scope, self, methodNames, RubyMethodAttributes.ProtectedInstance);
            return self;
        }

        [RubyMethod("public", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ SetPublicVisibility(RubyScope/*!*/ scope, RubyModule/*!*/ self, [NotNull]params object[]/*!*/ methodNames) {
            // overwrites methods to instance:
            SetMethodAttributes(scope, self, methodNames, RubyMethodAttributes.PublicInstance);
            return self;
        }

        [RubyMethod("module_function", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ CopyMethodsToModuleSingleton(RubyScope/*!*/ scope, RubyModule/*!*/ self, [NotNull]params object[]/*!*/ methodNames) {
            // This is an important restriction for correct super calls in module functions (see RubyOps.DefineMethod). 
            // MRI has it wrong. It checks just here and not in method definition.
            if (self.IsClass) {
                throw RubyExceptions.CreateTypeError("module_function must be called for modules");
            }
            
            // overwrites visibility to public:
            SetMethodAttributes(scope, self, methodNames, RubyMethodAttributes.ModuleFunction);
            return self;
        }

        internal static void SetMethodAttributes(RubyScope/*!*/ scope, RubyModule/*!*/ module, object[]/*!*/ methodNames, RubyMethodAttributes attributes) {
            ContractUtils.RequiresNotNull(scope, "scope");
            ContractUtils.RequiresNotNull(methodNames, "methodNames");

            if (methodNames.Length == 0) {
                scope.GetMethodAttributesDefinitionScope().MethodAttributes = attributes;
            } else {
                foreach (string methodName in Protocols.CastToSymbols(scope.RubyContext, methodNames)) {
                    RubyMemberInfo method = module.ResolveMethodFallbackToObject(methodName, true);
                    if (method == null) {
                        throw RubyExceptions.CreateNameError(RubyExceptions.FormatMethodMissingMessage(scope.RubyContext, module, methodName));
                    }

                    if ((attributes & RubyMethodAttributes.ModuleFunction) == RubyMethodAttributes.ModuleFunction) {
                        module.AddModuleFunction(scope.RubyContext, methodName, method);
                    } else {
                        module.SetMethodVisibility(scope.RubyContext, methodName, method, (RubyMethodVisibility)(attributes & RubyMethodAttributes.VisibilityMask));
                    }
                }
            }
        }

        #endregion

        #region define_method

        [RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
        public static RubyMethod/*!*/ DefineMethod(RubyContext/*!*/ context, RubyModule/*!*/ self, 
            [DefaultProtocol]string/*!*/ methodName, [NotNull]RubyMethod/*!*/ method) {

            // MRI 1.8 does the check when the method is called, 1.9 checks it upfront as we do:
            var targetClass = method.GetTargetClass();
            if (!self.HasAncestor(targetClass)) {
                throw RubyExceptions.CreateTypeError(
                    String.Format("bind argument must be a subclass of {0}", targetClass.GetName(context))
                );
            }

            self.AddDefinedMethod(context, methodName, method.Info);
            return method;
        }

        [RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
        public static UnboundMethod/*!*/ DefineMethod(RubyContext/*!*/ context, RubyModule/*!*/ self, 
            [DefaultProtocol]string/*!*/ methodName, [NotNull]UnboundMethod/*!*/ method) {

            // MRI 1.8 does the check when the method is called, 1.9 checks it upfront as we do:
            if (!self.HasAncestor(method.TargetConstraint)) {
                throw RubyExceptions.CreateTypeError(
                    String.Format("bind argument must be a subclass of {0}", method.TargetConstraint.GetName(context))
                );
            }

            self.AddDefinedMethod(context, methodName, method.Info);
            return method;
        }

        [RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
        public static Proc/*!*/ DefineMethod(RubyScope/*!*/ scope, [NotNull]BlockParam/*!*/ block, 
            RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ methodName) {

            return DefineMethod(scope, self, methodName, block.Proc);
        }

        [RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
        public static Proc/*!*/ DefineMethod(RubyScope/*!*/ scope, RubyModule/*!*/ self, 
            [DefaultProtocol]string/*!*/ methodName, [NotNull]Proc/*!*/ method) {

            // MRI: ignores ModuleFunction scope flag (doesn't create singleton method).
            // MRI 1.8: uses private visibility if module_function is applied (bug).
            // MFI 1.9: uses public visibility as we do, unless the name is special.
            var visibility = RubyUtils.GetSpecialMethodVisibility(scope.Visibility, methodName);

            self.AddMethod(scope.RubyContext, methodName, Proc.ToLambdaMethodInfo(method.ToLambda(), methodName, visibility, self));
            return method;
        }

        #endregion

        #region method_(added|removed|undefined)

        [RubyMethod("method_added", RubyMethodAttributes.PrivateInstance)]
        public static void MethodAdded(RubyModule/*!*/ self, object methodName) {
            // nop
        }

        [RubyMethod("method_removed", RubyMethodAttributes.PrivateInstance)]
        public static void MethodRemoved(RubyModule/*!*/ self, object methodName) {
            // nop
        }

        [RubyMethod("method_undefined", RubyMethodAttributes.PrivateInstance)]
        public static void MethodUndefined(RubyModule/*!*/ self, object methodName) {
            // nop
        }

        #endregion

        #region attr, attr_(reader,writer,accessor)

        private static void DefineAccessor(RubyScope/*!*/ scope, RubyModule/*!*/ self, string/*!*/ name, bool readable, bool writable) {
            // MRI: ignores ModuleFunction scope flag (doesn't create singleton methods):

            var varName = "@" + name;

            if (readable) {
                var flags = (RubyMemberFlags)RubyUtils.GetSpecialMethodVisibility(scope.Visibility, name);
                self.SetLibraryMethod(name, new RubyAttributeReaderInfo(flags, self, varName), false);
            }
            
            if (writable) {
                self.SetLibraryMethod(name + "=", new RubyAttributeWriterInfo((RubyMemberFlags)scope.Visibility, self, varName), false);
            }
        }

        [RubyMethod("attr", RubyMethodAttributes.PrivateInstance)]
        public static void Attr(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ name, [Optional]bool writable) {
            DefineAccessor(scope, self, name, true, writable);
        }

        [RubyMethod("attr_accessor", RubyMethodAttributes.PrivateInstance)]
        public static void AttrAccessor(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ name) {
            DefineAccessor(scope, self, name, true, true);
        }

        [RubyMethod("attr_accessor", RubyMethodAttributes.PrivateInstance)]
        public static void AttrAccessor(RubyScope/*!*/ scope, RubyModule/*!*/ self, [NotNull]params object[] names) {
            foreach (string name in Protocols.CastToSymbols(scope.RubyContext, names)) {
                DefineAccessor(scope, self, name, true, true);
            }
        }

        [RubyMethod("attr_reader", RubyMethodAttributes.PrivateInstance)]
        public static void AttrReader(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ name) {
            DefineAccessor(scope, self, name, true, false);
        }

        [RubyMethod("attr_reader", RubyMethodAttributes.PrivateInstance)]
        public static void AttrReader(RubyScope/*!*/ scope, RubyModule/*!*/ self, [NotNull]params object[] names) {
            foreach (string name in Protocols.CastToSymbols(scope.RubyContext, names)) {
                DefineAccessor(scope, self, name, true, false);
            }
        }

        [RubyMethod("attr_writer", RubyMethodAttributes.PrivateInstance)]
        public static void AttrWriter(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ name) {
            DefineAccessor(scope, self, name, false, true);
        }

        [RubyMethod("attr_writer", RubyMethodAttributes.PrivateInstance)]
        public static void AttrWriter(RubyScope/*!*/ scope, RubyModule/*!*/ self, [NotNull]params object[] names) {
            foreach (string name in Protocols.CastToSymbols(scope.RubyContext, names)) {
                DefineAccessor(scope, self, name, false, true);
            }
        }

        #endregion

        #region alias_method, remove_method, undef_method

        [RubyMethod("alias_method", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ AliasMethod(RubyContext/*!*/ context, RubyModule/*!*/ self,
            [DefaultProtocol]string/*!*/ newName, [DefaultProtocol]string/*!*/ oldName) {

            RubyMemberInfo method = self.ResolveMethodFallbackToObject(oldName, true);
            if (method == null) {
                throw RubyExceptions.CreateUndefinedMethodError(self, oldName);
            }

            self.AddMethodAlias(context, newName, method);
            return self;
        }

        [RubyMethod("remove_method", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ RemoveMethod(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ methodName) {
            if (!self.RemoveMethod(methodName)) {
                throw RubyExceptions.CreateUndefinedMethodError(self, methodName);
            }
            return self;
        }

        [RubyMethod("undef_method", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ UndefineMethod(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ methodName) {
            RubyMemberInfo method = self.ResolveMethod(methodName, true);
            if (method == null) {
                throw RubyExceptions.CreateUndefinedMethodError(self, methodName);
            }
            self.UndefineMethod(methodName);
            return self;
        }

        #endregion

        #endregion

        #region Public Instance Methods

        #region <, >, <=, >=, <=>, ==, ===, ancestors, included_modules, include? 

        [RubyMethod("==")]
        public static bool Equals(RubyModule/*!*/ self, object other) {
            return ReferenceEquals(self, other);
        }
        
        [RubyMethod("===")]
        public static bool CaseEquals(RubyModule/*!*/ self, object other) {
            return self.Context.GetClassOf(other).HasAncestor(self);
        }

        [RubyMethod("<")]
        public static object IsSubclassOrIncluded(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ module) {
            if (ReferenceEquals(self, module)) {
                return false;
            }
            return self.HasAncestor(module) ? ScriptingRuntimeHelpers.True : null;
        }

        [RubyMethod("<=")]
        public static object IsSubclassSameOrIncluded(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ module) {
            if (self.HasAncestor(module)) {
                return true;
            }
            return module.HasAncestor(self) ? ScriptingRuntimeHelpers.False : null;
        }

        [RubyMethod(">")]
        public static object IsNotSubclassOrIncluded(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ module) {
            if (ReferenceEquals(self, module)) {
                return false;
            }
            return module.HasAncestor(self) ? ScriptingRuntimeHelpers.True : null;
        }

        [RubyMethod(">=")]
        public static object IsNotSubclassSameOrIncluded(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ module) {
            if (module.HasAncestor(self)) {
                return true;
            }
            return self.HasAncestor(module) ? ScriptingRuntimeHelpers.False : null;
        }

        [RubyMethod("<=>")]
        public static object Comparison(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ module) {
            if (ReferenceEquals(self, module)) {
                return ScriptingRuntimeHelpers.Int32ToObject(0);
            }

            if (self.HasAncestor(module)) {
                return ScriptingRuntimeHelpers.Int32ToObject(-1);
            }

            if (module.HasAncestor(self)) {
                return ScriptingRuntimeHelpers.Int32ToObject(1);
            }

            return null;
        }

        [RubyMethod("<=>")]
        public static object Comparison(RubyModule/*!*/ self, object module) {
            return null;
        }

        [RubyMethod("<")]
        [RubyMethod(">")]
        [RubyMethod("<=")]
        [RubyMethod(">=")]
        public static object InvalidComparison(RubyModule/*!*/ self, object module) {
            throw RubyExceptions.CreateTypeError("compared with non class/module");
        }

        [RubyMethod("ancestors")]
        public static RubyArray/*!*/ Ancestors(RubyModule/*!*/ self) {
            RubyArray ancestors = new RubyArray();
            self.ForEachAncestor(true, delegate(RubyModule/*!*/ module) {
                if (!module.IsSingletonClass) {
                    ancestors.Add(module);
                }
                return false;
            });
            return ancestors;
        }

        [RubyMethod("included_modules")]
        public static RubyArray/*!*/ GetIncludedModules(RubyModule/*!*/ self) {
            RubyArray ancestorModules = new RubyArray();
            self.ForEachAncestor(true, delegate(RubyModule/*!*/ module) {
                if (module != self && !module.IsClass && !ancestorModules.Contains(module)) {
                    ancestorModules.Add(module);
                }
                return false;
            });
            return ancestorModules;
        }

        [RubyMethod("include?")]
        public static bool IncludesModule(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ other) {
            if (other.IsClass) {
                throw RubyExceptions.CreateTypeError("wrong argument type Class (expected Module)");
            }

            return other != self && self.HasAncestor(other);
        }

        #endregion

        #region module_eval, class_eval

        [RubyMethod("module_eval")]
        [RubyMethod("class_eval")]
        public static object Evaluate(RubyScope/*!*/ scope, BlockParam block, RubyModule/*!*/ self, [NotNull]MutableString/*!*/ code,
            [Optional, NotNull]MutableString file, [DefaultParameterValue(1)]int line) {

            if (block != null) {
                throw RubyExceptions.CreateArgumentError("wrong number of arguments");
            } 
            
            return RubyUtils.Evaluate(code, scope, self, self, file, line);
        }

        [RubyMethod("module_eval")]
        [RubyMethod("class_eval")]
        public static object Evaluate([NotNull]BlockParam/*!*/ block, RubyModule/*!*/ self) {
            return RubyUtils.EvaluateInModule(self, block);
        }

        #endregion

        #region class_variables, class_variable_defined?

        [RubyMethod("class_variables", RubyMethodAttributes.PublicInstance)]
        public static RubyArray/*!*/ ClassVariables(RubyModule/*!*/ self) {
            var visited = new Dictionary<string, bool>();
            var result = new RubyArray();

            self.ForEachClassVariable(true, delegate(RubyModule/*!*/ module, string name, object value) {
                if (name != null && !visited.ContainsKey(name)) {
                    result.Add(MutableString.Create(name));
                    visited.Add(name, true);
                }
                return false;
            });
            return result;
        }

        [RubyMethod("class_variable_defined?", RubyMethodAttributes.PublicInstance)]
        public static bool ClassVariableDefined(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ variableName) {
            RubyUtils.CheckClassVariableName(variableName);
            object value;
            return self.TryResolveClassVariable(variableName, out value) != null;
        }

        #endregion

        #region const_defined?, const_set, const_get, constants, const_missing

        [RubyMethod("const_defined?")]
        public static bool IsConstantDefined(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ constantName) {
            RubyUtils.CheckConstantName(constantName);

            object constant;
            return self.TryResolveConstantNoAutoload(constantName, out constant);
        }

        [RubyMethod("const_get")]
        public static object GetConstantValue(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ constantName) {
            return RubyUtils.GetConstant(scope.GlobalScope, self, constantName, true);
        }

        [RubyMethod("const_set")]
        public static object SetConstantValue(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ constantName, object value) {
            RubyUtils.CheckConstantName(constantName);
            RubyUtils.SetConstant(self, constantName, value);
            return value;
        }

        [RubyMethod("constants")]
        public static RubyArray/*!*/ GetDefinedConstants(RubyModule/*!*/ self) {
            var visited = new Dictionary<string, bool>();
            var result = new RubyArray();
            
            bool hideGlobalConstants = !ReferenceEquals(self, self.Context.ObjectClass);
            self.ForEachConstant(true, delegate(RubyModule/*!*/ module, string name, object value) {
                if (name == null) {
                    // terminate enumeration when Object is reached
                    return hideGlobalConstants && ReferenceEquals(module, module.Context.ObjectClass);
                }

                if (!visited.ContainsKey(name)) {
                    visited.Add(name, true);
                    result.Add(MutableString.Create(name));
                }
                return false;
            });

            return result;
        }

        [RubyMethod("const_missing")]
        public static void ConstantMissing(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ name) {
            throw RubyExceptions.CreateNameError(String.Format("uninitialized constant {0}::{1}", self.Name, name));
        }

        #endregion

        #region {private_|protected_|public_|}instance_methods

        [RubyMethod("instance_methods")]
        public static RubyArray/*!*/ GetInstanceMethods(RubyModule/*!*/ self) {
            return GetInstanceMethods(self, true);
        }

        [RubyMethod("instance_methods")]
        public static RubyArray/*!*/ GetInstanceMethods(RubyModule/*!*/ self, bool inherited) {
            return GetMethods(self, inherited, RubyMethodAttributes.PublicInstance | RubyMethodAttributes.ProtectedInstance);
        }

        [RubyMethod("private_instance_methods")]
        public static RubyArray/*!*/ GetPrivateInstanceMethods(RubyModule/*!*/ self) {
            return GetPrivateInstanceMethods(self, true);
        }

        [RubyMethod("private_instance_methods")]
        public static RubyArray/*!*/ GetPrivateInstanceMethods(RubyModule/*!*/ self, bool inherited) {
            return GetMethods(self, inherited, RubyMethodAttributes.PrivateInstance);
        }

        [RubyMethod("protected_instance_methods")]
        public static RubyArray/*!*/ GetProtectedInstanceMethods(RubyModule/*!*/ self) {
            return GetProtectedInstanceMethods(self, true);
        }

        [RubyMethod("protected_instance_methods")]
        public static RubyArray/*!*/ GetProtectedInstanceMethods(RubyModule/*!*/ self, bool inherited) {
            return GetMethods(self, inherited, RubyMethodAttributes.ProtectedInstance);
        }

        [RubyMethod("public_instance_methods")]
        public static RubyArray/*!*/ GetPublicInstanceMethods(RubyModule/*!*/ self) {
            return GetPublicInstanceMethods(self, true);
        }

        [RubyMethod("public_instance_methods")]
        public static RubyArray/*!*/ GetPublicInstanceMethods(RubyModule/*!*/ self, bool inherited) {
            return GetMethods(self, inherited, RubyMethodAttributes.PublicInstance);
        }

        internal static RubyArray/*!*/ GetMethods(RubyModule/*!*/ self, bool inherited, RubyMethodAttributes attributes) {
            var result = new RubyArray();
            var symbolicNames = self.Context.RubyOptions.Compatibility > RubyCompatibility.Ruby18;
            self.ForEachMember(inherited, attributes, delegate(string/*!*/ name, RubyMemberInfo/*!*/ member) {
                if (symbolicNames) {
                    result.Add(SymbolTable.StringToId(name));
                } else {
                    result.Add(MutableString.Create(name));
                }
            });
            return result;
        }

        #endregion

        #region {private_|protected_|public_|}method_defined?

        [RubyMethod("method_defined?")]
        public static bool MethodDefined(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ methodName) {
            RubyMemberInfo method = self.ResolveMethod(methodName, true);
            return method != null && method.Visibility != RubyMethodVisibility.Private;
        }

        [RubyMethod("private_method_defined?")]
        public static bool PrivateMethodDefined(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ methodName) {
            RubyMemberInfo method = self.ResolveMethod(methodName, true);
            return method != null && method.Visibility == RubyMethodVisibility.Private;
        }

        [RubyMethod("protected_method_defined?")]
        public static bool ProtectedMethodDefined(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ methodName) {
            RubyMemberInfo method = self.ResolveMethod(methodName, true);
            return method != null && method.Visibility == RubyMethodVisibility.Protected;
        }

        [RubyMethod("public_method_defined?")]
        public static bool PublicMethodDefined(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ methodName) {
            RubyMemberInfo method = self.ResolveMethod(methodName, true);
            return method != null && method.Visibility == RubyMethodVisibility.Public;
        }

        #endregion

        #region instance_method, private_class_method, public_class_method

        [RubyMethod("instance_method")]
        public static UnboundMethod/*!*/ GetInstanceMethod(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ methodName) {
            RubyMemberInfo method = self.ResolveMethod(methodName, true);
            if (method == null) {
                throw RubyExceptions.CreateUndefinedMethodError(self, methodName);
            }

            // unbound method binable to any class with "self" mixin:
            return new UnboundMethod(self, methodName, method);
        }

        private static void SetClassMethodsVisibility(RubyContext/*!*/ context, RubyModule/*!*/ module, string[]/*!*/ methodNames, RubyMethodVisibility visibility) {
            var methods = new RubyMemberInfo[methodNames.Length];
            for (int i = 0; i < methods.Length; i++) {
                RubyMemberInfo method = module.SingletonClass.ResolveMethod(methodNames[i], true);
                if (method == null) {
                    throw RubyExceptions.CreateUndefinedMethodError(module, methodNames[i]);
                }
                methods[i] = method;
            }

            for (int i = 0; i < methods.Length; i++) {
                module.SingletonClass.SetMethodVisibility(context, methodNames[i], methods[i], visibility);
            }
        }

        [RubyMethodAttribute("private_class_method")]
        public static RubyModule/*!*/ MakeClassMethodsPrivate(RubyContext/*!*/ context, RubyModule/*!*/ self, [NotNull]params object[]/*!*/ methodNames) {
            SetClassMethodsVisibility(context, self, Protocols.CastToSymbols(context, methodNames), RubyMethodVisibility.Private);
            return self;
        }

        [RubyMethodAttribute("public_class_method")]
        public static RubyModule/*!*/ MakeClassMethodsPublic(RubyContext/*!*/ context, RubyModule/*!*/ self, [NotNull]params object[]/*!*/ methodNames) {
            SetClassMethodsVisibility(context, self, Protocols.CastToSymbols(context, methodNames), RubyMethodVisibility.Public);
            return self;
        }

        #endregion

        [RubyMethod("freeze")]
        public static RubyModule/*!*/ Freeze(RubyContext/*!*/ context, RubyModule/*!*/ self) {
            // TODO:
            context.FreezeObject(self);
            return self;            
        }

        [RubyMethod("autoload")]
        public static void SetAutoloadedConstant(RubyModule/*!*/ self,
            [DefaultProtocol]string/*!*/ constantName, [DefaultProtocol, NotNull]MutableString/*!*/ path) {

            RubyUtils.CheckConstantName(constantName);
            if (path.IsEmpty) {
                throw RubyExceptions.CreateArgumentError("empty file name");
            }

            self.SetAutoloadedConstant(constantName, path);
        }

        [RubyMethod("autoload?")]
        public static MutableString GetAutoloadedConstantPath(RubyModule/*!*/ self, [DefaultProtocol]string/*!*/ constantName) {
            return self.GetAutoloadedConstantPath(constantName);
        }

        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToS(RubyContext/*!*/ context, RubyModule/*!*/ self) {
            return self.GetDisplayName(context, false);
        }

        [RubyMethod("name")]
        public static MutableString/*!*/ GetName(RubyContext/*!*/ context, RubyModule/*!*/ self) {
            return self.GetDisplayName(context, true);
        }

        [RubyMethod("of")]
        [RubyMethod("[]")]
        public static RubyModule/*!*/ Of(RubyModule/*!*/ self, [NotNull]params object[]/*!*/ typeArgs) {
            if (self.Tracker == null) {
                throw new NotImplementedException("TODO");
            }

            Type type = self.Tracker.Type;
            int provided = typeArgs.Length;

            if (provided == 1 && type == typeof(Array)) {
                return self.Context.GetModule(Protocols.ToType(self.Context, typeArgs[0]).MakeArrayType());
            } 

            int required = type.GetGenericArguments().Length;
            if (required == 0 && provided > 0) {
                throw RubyExceptions.CreateArgumentError(String.Format("'{0}' is not a generic type", type.FullName));
            }

            if (required != provided) {
                throw RubyExceptions.CreateArgumentError(String.Format("Type '{0}' requires {1} generic type arguments, {2} provided", type.FullName, required, provided));
            }

            if (typeArgs.Length > 0) {
                Type concreteType = type.MakeGenericType(Protocols.ToTypes(self.Context, typeArgs));
                return self.Context.GetModule(concreteType);
            } else {
                return self;
            }
        }

        #endregion

        #region Singleton Methods

        [RubyMethod("constants", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetGlobalConstants(RubyModule/*!*/ self) {
            return ModuleOps.GetDefinedConstants(self.Context.ObjectClass);
        }

        [RubyMethod("nesting", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetLexicalModuleNesting(RubyScope/*!*/ scope, RubyModule/*!*/ self) {
            RubyArray result = new RubyArray();
            while (scope != null) {
                // Ruby 1.9: the anonymous module doesn't show up
                if (scope.Module != null) {
                    result.Add(scope.Module);
                }
                scope = (RubyScope)scope.Parent;
            }
            return result;
        }

        #endregion
    }
}
