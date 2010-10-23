/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
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
using System.Diagnostics;

namespace IronRuby.Builtins {

    [RubyClass("Module", Extends = typeof(RubyModule), Inherits = typeof(Object), Restrictions = ModuleRestrictions.Builtin | ModuleRestrictions.NoUnderlyingType)]
    public static class ModuleOps {

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

        #region extend_object, extended, include, included

        // thread-safe:
        [RubyMethod("extend_object", RubyMethodAttributes.PrivateInstance)]
        public static object ExtendObject(RubyModule/*!*/ self, object extendedObject) {
            // include self into extendedObject's singleton
            self.Context.GetOrCreateSingletonClass(extendedObject).IncludeModules(self);
            return extendedObject;
        }

        [RubyMethod("extended", RubyMethodAttributes.PrivateInstance)]
        public static void ObjectExtended(RubyModule/*!*/ self, object extendedObject) {
            // extendedObject has been extended by self, i.e. self has been included into extendedObject's singleton class
        }

        [RubyMethod("include", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ Include(
            CallSiteStorage<Func<CallSite, RubyModule, RubyModule, object>>/*!*/ appendFeaturesStorage,
            CallSiteStorage<Func<CallSite, RubyModule, RubyModule, object>>/*!*/ includedStorage,
            RubyModule/*!*/ self, [NotNullItems]params RubyModule/*!*/[]/*!*/ modules) {

            RubyUtils.RequireMixins(self, modules);

            var appendFeatures = appendFeaturesStorage.GetCallSite("append_features", 1);
            var included = includedStorage.GetCallSite("included", 1);

            // Kernel#append_features inserts the module at the beginning of ancestors list;
            // ancestors after include: [modules[0], modules[1], ..., modules[N-1], self, ...]
            for (int i = modules.Length - 1; i >= 0; i--) {
                appendFeatures.Target(appendFeatures, modules[i], self);
                included.Target(included, modules[i], self);
            }

            return self;
        }

        [RubyMethod("included", RubyMethodAttributes.PrivateInstance)]
        public static void Included(RubyModule/*!*/ self, RubyModule/*!*/ owner) {
            // self has been included into owner
        }

        // thread-safe:
        [RubyMethod("append_features", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ AppendFeatures(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ owner) {
            owner.IncludeModules(self);
            return self;
        }

        #endregion

        #region private, protected, public, private_class_method, public_class_method, module_function

        // thread-safe:
        [RubyMethod("private", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ SetPrivateVisibility(RubyScope/*!*/ scope, RubyModule/*!*/ self,
            [DefaultProtocol, NotNullItems]params string/*!*/[]/*!*/ methodNames) {

            // overwrites methods to instance:
            SetMethodAttributes(scope, self, methodNames, RubyMethodAttributes.PrivateInstance);
            return self;
        }

        // thread-safe:
        [RubyMethod("protected", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ SetProtectedVisibility(RubyScope/*!*/ scope, RubyModule/*!*/ self,
            [DefaultProtocol, NotNullItems]params string/*!*/[]/*!*/ methodNames) {
            // overwrites methods to instance:
            SetMethodAttributes(scope, self, methodNames, RubyMethodAttributes.ProtectedInstance);
            return self;
        }

        // thread-safe:
        [RubyMethod("public", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ SetPublicVisibility(RubyScope/*!*/ scope, RubyModule/*!*/ self,
            [DefaultProtocol, NotNullItems]params string/*!*/[]/*!*/ methodNames) {
            // overwrites methods to instance:
            SetMethodAttributes(scope, self, methodNames, RubyMethodAttributes.PublicInstance);
            return self;
        }

        // thread-safe:
        [RubyMethodAttribute("private_class_method")]
        public static RubyModule/*!*/ MakeClassMethodsPrivate(RubyModule/*!*/ self,
            [DefaultProtocol, NotNullItems]params string/*!*/[]/*!*/ methodNames) {
            SetMethodAttributes(self.GetOrCreateSingletonClass(), methodNames, RubyMethodAttributes.Private);
            return self;
        }

        // thread-safe:
        [RubyMethodAttribute("public_class_method")]
        public static RubyModule/*!*/ MakeClassMethodsPublic(RubyModule/*!*/ self,
            [DefaultProtocol, NotNullItems]params string/*!*/[]/*!*/ methodNames) {
            SetMethodAttributes(self.GetOrCreateSingletonClass(), methodNames, RubyMethodAttributes.Public);
            return self;
        }

        // thread-safe:
        [RubyMethod("module_function", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ CopyMethodsToModuleSingleton(RubyScope/*!*/ scope, RubyModule/*!*/ self,
            [DefaultProtocol, NotNullItems]params string/*!*/[]/*!*/ methodNames) {

            // This is an important restriction for correct super calls in module functions (see RubyOps.DefineMethod). 
            // MRI has it wrong. It checks just here and not in method definition.
            if (self.IsClass) {
                throw RubyExceptions.CreateTypeError("module_function must be called for modules");
            }
            
            // overwrites visibility to public:
            SetMethodAttributes(scope, self, methodNames, RubyMethodAttributes.ModuleFunction);
            return self;
        }

        internal static void SetMethodAttributes(RubyScope/*!*/ scope, RubyModule/*!*/ module, string/*!*/[]/*!*/ methodNames, RubyMethodAttributes attributes) {
            if (methodNames.Length == 0) {
                scope.GetMethodAttributesDefinitionScope().MethodAttributes = attributes;
            } else {
                SetMethodAttributes(module, methodNames, attributes);
            }
        }

        internal static void SetMethodAttributes(RubyModule/*!*/ module, string/*!*/[]/*!*/ methodNames, RubyMethodAttributes attributes) {
            var context = module.Context;

            bool isModuleFunction = (attributes & RubyMethodAttributes.ModuleFunction) == RubyMethodAttributes.ModuleFunction;
            var instanceVisibility = isModuleFunction ? RubyMethodVisibility.Private : 
                (RubyMethodVisibility)(attributes & RubyMethodAttributes.VisibilityMask);

            foreach (string methodName in methodNames) {
                RubyMemberInfo method;

                // we need to define new methods one by one since the method_added events can define a new method that might be used here:
                using (context.ClassHierarchyLocker()) {
                    MethodLookup options = MethodLookup.FallbackToObject;
                    if (!isModuleFunction) {
                        options |= MethodLookup.ReturnForwarder;
                    }

                    method = module.ResolveMethodNoLock(methodName, VisibilityContext.AllVisible, options).Info;
                    if (method == null) {
                        throw RubyExceptions.CreateUndefinedMethodError(module, methodName);
                    }

                    // MRI only adds method to the target module if visibility differs:
                    if (method.Visibility != instanceVisibility) {
                        module.SetVisibilityNoEventNoLock(context, methodName, method, instanceVisibility);
                    }

                    if (isModuleFunction) {
                        module.SetModuleFunctionNoEventNoLock(context, methodName, method);
                    }
                }

                if (method.Visibility != instanceVisibility) {
                    module.MethodAdded(methodName);
                }

                if (isModuleFunction) {
                    module.GetOrCreateSingletonClass().MethodAdded(methodName);
                }
            }
        }

        #endregion

        #region define_method (thread-safe)

        // thread-safe:
        [RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
        public static RubyMethod/*!*/ DefineMethod(RubyScope/*!*/ scope, RubyModule/*!*/ self, 
            [DefaultProtocol, NotNull]string/*!*/ methodName, [NotNull]RubyMethod/*!*/ method) {

            DefineMethod(scope, self, methodName, method.Info,  method.GetTargetClass());
            return method;
        }

        // thread-safe:
        // Defines method using mangled CLR name and aliases that method with the actual CLR name.
        [RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
        public static RubyMethod/*!*/ DefineMethod(RubyScope/*!*/ scope, RubyModule/*!*/ self,
            [NotNull]ClrName/*!*/ methodName, [NotNull]RubyMethod/*!*/ method) {
            var result = DefineMethod(scope, self, methodName.MangledName, method);
            if (methodName.HasMangledName) {
                self.AddMethodAlias(methodName.ActualName, methodName.MangledName);
            }
            return result;
        }

        // thread-safe:
        [RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
        public static UnboundMethod/*!*/ DefineMethod(RubyScope/*!*/ scope, RubyModule/*!*/ self, 
            [DefaultProtocol, NotNull]string/*!*/ methodName, [NotNull]UnboundMethod/*!*/ method) {

            DefineMethod(scope, self, methodName, method.Info, method.TargetConstraint);
            return method;
        }

        // thread-safe:
        // Defines method using mangled CLR name and aliases that method with the actual CLR name.
        [RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
        public static UnboundMethod/*!*/ DefineMethod(RubyScope/*!*/ scope, RubyModule/*!*/ self,
            [NotNull]ClrName/*!*/ methodName, [NotNull]UnboundMethod/*!*/ method) {
            var result = DefineMethod(scope, self, methodName.MangledName, method);
            if (methodName.HasMangledName) {
                self.AddMethodAlias(methodName.ActualName, methodName.MangledName);
            }
            return result;
        }

        private static void DefineMethod(RubyScope/*!*/ scope, RubyModule/*!*/ self, string/*!*/ methodName, RubyMemberInfo/*!*/ info,
            RubyModule/*!*/ targetConstraint) {

            var visibility = GetDefinedMethodVisibility(scope, self, methodName);
            using (self.Context.ClassHierarchyLocker()) {
                // MRI 1.8 does the check when the method is called, 1.9 checks it upfront as we do:
                if (!self.HasAncestorNoLock(targetConstraint)) {
                    throw RubyExceptions.CreateTypeError(
                        "bind argument must be a subclass of {0}", targetConstraint.GetName(scope.RubyContext)
                    );
                }

                self.SetDefinedMethodNoEventNoLock(self.Context, methodName, info, visibility);
            }

            self.MethodAdded(methodName);
        }

        // thread-safe:
        [RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
        public static Proc/*!*/ DefineMethod(RubyScope/*!*/ scope, [NotNull]BlockParam/*!*/ block, 
            RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ methodName) {

            return DefineMethod(scope, self, methodName, block.Proc);
        }

        // thread-safe:
        // Defines method using mangled CLR name and aliases that method with the actual CLR name.
        [RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
        public static Proc/*!*/ DefineMethod(RubyScope/*!*/ scope, [NotNull]BlockParam/*!*/ block,
            RubyModule/*!*/ self, [NotNull]ClrName/*!*/ methodName) {

            var result = DefineMethod(scope, block, self, methodName.MangledName);
            if (methodName.HasMangledName) {
                self.AddMethodAlias(methodName.ActualName, methodName.MangledName);
            }
            return result;
        }

        // thread-safe:
        [RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
        public static Proc/*!*/ DefineMethod(RubyScope/*!*/ scope, RubyModule/*!*/ self, 
            [DefaultProtocol, NotNull]string/*!*/ methodName, [NotNull]Proc/*!*/ block) {

            var visibility = GetDefinedMethodVisibility(scope, self, methodName);
            var info = Proc.ToLambdaMethodInfo(block, methodName, visibility, self);
            self.AddMethod(scope.RubyContext, methodName, info);
            return info.Lambda;
        }

        // thread-safe:
        [RubyMethod("define_method", RubyMethodAttributes.PrivateInstance)]
        public static Proc/*!*/ DefineMethod(RubyScope/*!*/ scope, RubyModule/*!*/ self,
            [NotNull]ClrName/*!*/ methodName, [NotNull]Proc/*!*/ block) {

            var result = DefineMethod(scope, self, methodName.MangledName, block);
            if (methodName.HasMangledName) {
                self.AddMethodAlias(methodName.ActualName, methodName.MangledName);
            }
            return result;
        }

        private static RubyMethodVisibility GetDefinedMethodVisibility(RubyScope/*!*/ scope, RubyModule/*!*/ module, string/*!*/ methodName) {
            // MRI: Special names are private.
            // MRI: Doesn't create a singleton method if module_function is used in the scope, however the private visibility is applied (bug?)
            // MRI 1.8: uses the current scope's visibility only if the target module is the same as the scope's module (bug?)
            // MFI 1.9: always uses public visibility (bug?)
            RubyMethodVisibility visibility;
            if (scope.RubyContext.RubyOptions.Compatibility < RubyCompatibility.Ruby19) {
                var attributesScope = scope.GetMethodAttributesDefinitionScope();
                if (attributesScope.GetInnerMostModuleForMethodLookup() == module) {
                    bool isModuleFunction = (attributesScope.MethodAttributes & RubyMethodAttributes.ModuleFunction) == RubyMethodAttributes.ModuleFunction;
                    visibility = (isModuleFunction) ? RubyMethodVisibility.Private : attributesScope.Visibility;
                } else {
                    visibility = RubyMethodVisibility.Public;
                }
            } else {
                visibility = RubyMethodVisibility.Public;
            }

            return RubyUtils.GetSpecialMethodVisibility(visibility, methodName);
        }

        #endregion

        #region method_(added|removed|undefined)

        [RubyMethod("method_added", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
        public static void MethodAdded(object/*!*/ self, object methodName) {
            // nop
        }

        [RubyMethod("method_removed", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
        public static void MethodRemoved(object/*!*/ self, object methodName) {
            // nop
        }

        [RubyMethod("method_undefined", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
        public static void MethodUndefined(object/*!*/ self, object methodName) {
            // nop
        }

        #endregion

        #region attr, attr_{reader|writer|accessor} (thread-safe)

        private static void DefineAccessor(RubyScope/*!*/ scope, RubyModule/*!*/ self, string/*!*/ name, bool readable, bool writable) {
            // MRI: ignores ModuleFunction scope flag (doesn't create singleton methods):

            if (!Tokenizer.IsVariableName(name)) {
                throw RubyExceptions.CreateNameError("invalid attribute name `{0}'", name);
            }

            var varName = "@" + name;
            var attributesScope = scope.GetMethodAttributesDefinitionScope();

            if (readable) {
                var flags = (RubyMemberFlags)RubyUtils.GetSpecialMethodVisibility(attributesScope.Visibility, name);
                self.AddMethod(scope.RubyContext, name, new RubyAttributeReaderInfo(flags, self, varName));
            }
            
            if (writable) {
                self.AddMethod(scope.RubyContext, name + "=", new RubyAttributeWriterInfo((RubyMemberFlags)attributesScope.Visibility, self, varName));
            }
        }

        // thread-safe:
        [RubyMethod("attr", RubyMethodAttributes.PrivateInstance)]
        public static void Attr(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ name, [Optional]bool writable) {
            DefineAccessor(scope, self, name, true, writable);
        }

        // thread-safe:
        [RubyMethod("attr", RubyMethodAttributes.PrivateInstance)]
        public static void Attr(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol, NotNullItems]params string/*!*/[]/*!*/ names) {
            foreach (string name in names) {
                DefineAccessor(scope, self, name, true, false);
            }
        }

        // thread-safe:
        [RubyMethod("attr_accessor", RubyMethodAttributes.PrivateInstance)]
        public static void AttrAccessor(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ name) {
            DefineAccessor(scope, self, name, true, true);
        }

        // thread-safe:
        [RubyMethod("attr_accessor", RubyMethodAttributes.PrivateInstance)]
        public static void AttrAccessor(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol, NotNullItems]params string/*!*/[]/*!*/ names) {
            foreach (string name in names) {
                DefineAccessor(scope, self, name, true, true);
            }
        }

        // thread-safe:
        [RubyMethod("attr_reader", RubyMethodAttributes.PrivateInstance)]
        public static void AttrReader(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ name) {
            DefineAccessor(scope, self, name, true, false);
        }

        // thread-safe:
        [RubyMethod("attr_reader", RubyMethodAttributes.PrivateInstance)]
        public static void AttrReader(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol, NotNullItems]params string/*!*/[]/*!*/ names) {
            foreach (string name in names) {
                DefineAccessor(scope, self, name, true, false);
            }
        }

        // thread-safe:
        [RubyMethod("attr_writer", RubyMethodAttributes.PrivateInstance)]
        public static void AttrWriter(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ name) {
            DefineAccessor(scope, self, name, false, true);
        }

        // thread-safe:
        [RubyMethod("attr_writer", RubyMethodAttributes.PrivateInstance)]
        public static void AttrWriter(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol, NotNullItems]params string/*!*/[]/*!*/ names) {
            foreach (string name in names) {
                DefineAccessor(scope, self, name, false, true);
            }
        }

        #endregion

        #region alias_method, remove_method, undef_method

        // thread-safe:
        [RubyMethod("alias_method", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ AliasMethod(RubyContext/*!*/ context, RubyModule/*!*/ self,
            [DefaultProtocol, NotNull]string/*!*/ newName, [DefaultProtocol, NotNull]string/*!*/ oldName) {

            self.AddMethodAlias(newName, oldName);
            return self;
        }

        // thread-safe:
        [RubyMethod("remove_method", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ RemoveMethod(RubyModule/*!*/ self, [DefaultProtocol, NotNullItems]params string[]/*!*/ methodNames) {
            foreach (var methodName in methodNames) {
                // MRI: reports a warning and allows removal
                if (self.IsBasicObjectClass && methodName == Symbols.Initialize) {
                    throw RubyExceptions.CreateNameError("Cannot remove BasicObject#initialize");
                }

                if (!self.RemoveMethod(methodName)) {
                    throw RubyExceptions.CreateUndefinedMethodError(self, methodName);
                }
            }
            return self;
        }

        // thread-safe:
        [RubyMethod("undef_method", RubyMethodAttributes.PrivateInstance)]
        public static RubyModule/*!*/ UndefineMethod(RubyModule/*!*/ self, [DefaultProtocol, NotNullItems]params string[]/*!*/ methodNames) {
            foreach (var methodName in methodNames) {
                if (!self.ResolveMethod(methodName, VisibilityContext.AllVisible).Found) {
                    throw RubyExceptions.CreateUndefinedMethodError(self, methodName);
                }
                self.UndefineMethod(methodName);
            }
            return self;
        }

        #endregion


        #region <, >, <=, >=, <=>, ==, ===, ancestors, included_modules, include? (thread-safe)

        // thread-safe:
        [RubyMethod("==")]
        public static bool Equals(RubyModule/*!*/ self, object other) {
            return ReferenceEquals(self, other);
        }

        // thread-safe:
        [RubyMethod("===")]
        public static bool CaseEquals(RubyModule/*!*/ self, object other) {
            return self.Context.IsKindOf(other, self);
        }

        // thread-safe:
        [RubyMethod("<")]
        public static object IsSubclassOrIncluded(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ module) {
            if (ReferenceEquals(self, module)) {
                return ScriptingRuntimeHelpers.False;
            }
            return self.HasAncestor(module) ? ScriptingRuntimeHelpers.True : null;
        }

        // thread-safe:
        [RubyMethod("<=")]
        public static object IsSubclassSameOrIncluded(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ module) {
            if (self.Context != module.Context) {
                return null;
            } 
            
            using (self.Context.ClassHierarchyLocker()) {
                if (self.HasAncestorNoLock(module)) {
                    return ScriptingRuntimeHelpers.True;
                }
                return module.HasAncestorNoLock(self) ? ScriptingRuntimeHelpers.False : null;
            }
        }

        // thread-safe:
        [RubyMethod(">")]
        public static object IsNotSubclassOrIncluded(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ module) {
            if (ReferenceEquals(self, module)) {
                return false;
            }
            return module.HasAncestor(self) ? ScriptingRuntimeHelpers.True : null;
        }

        // thread-safe:
        [RubyMethod(">=")]
        public static object IsNotSubclassSameOrIncluded(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ module) {
            if (self.Context != module.Context) {
                return null;
            } 
            
            using (self.Context.ClassHierarchyLocker()) {
                if (module.HasAncestorNoLock(self)) {
                    return ScriptingRuntimeHelpers.True;
                }
                return self.HasAncestorNoLock(module) ? ScriptingRuntimeHelpers.False : null;
            }
        }

        // thread-safe:
        [RubyMethod("<=>")]
        public static object Comparison(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ module) {
            if (ReferenceEquals(self, module)) {
                return ClrInteger.Zero;
            }

            if (self.Context != module.Context) {
                return null;
            }

            using (self.Context.ClassHierarchyLocker()) {
                if (self.HasAncestorNoLock(module)) {
                    return ClrInteger.MinusOne;
                }

                if (module.HasAncestorNoLock(self)) {
                    return ClrInteger.One;
                }
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

        // thread-safe:
        [RubyMethod("ancestors")]
        public static RubyArray/*!*/ Ancestors(RubyModule/*!*/ self) {
            RubyArray ancestors = new RubyArray();

            using (self.Context.ClassHierarchyLocker()) {
                self.ForEachAncestor(true, delegate(RubyModule/*!*/ module) {
                    if (!module.IsSingletonClass) {
                        ancestors.Add(module);
                    }
                    return false;
                });
            }
            return ancestors;
        }

        // thread-safe:
        [RubyMethod("included_modules")]
        public static RubyArray/*!*/ GetIncludedModules(RubyModule/*!*/ self) {
            RubyArray ancestorModules = new RubyArray();

            using (self.Context.ClassHierarchyLocker()) {
                self.ForEachAncestor(true, delegate(RubyModule/*!*/ module) {
                    if (module != self && !module.IsClass && !ancestorModules.Contains(module)) {
                        ancestorModules.Add(module);
                    }
                    return false;
                });
            }

            return ancestorModules;
        }

        // thread-safe:
        [RubyMethod("include?")]
        public static bool IncludesModule(RubyModule/*!*/ self, [NotNull]RubyModule/*!*/ other) {
            if (other.IsClass) {
                throw RubyExceptions.CreateTypeError("wrong argument type Class (expected Module)");
            }

            return other != self && self.HasAncestor(other);
        }

        #endregion

        #region (module|class)_(eval|exec)

        [RubyMethod("module_eval")]
        [RubyMethod("class_eval")]
        public static object Evaluate(RubyScope/*!*/ scope, BlockParam block, RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ code,
            [Optional, NotNull]MutableString file, [DefaultParameterValue(1)]int line) {

            if (block != null) {
                throw RubyExceptions.CreateArgumentError("wrong number of arguments");
            } 
            
            return RubyUtils.Evaluate(code, scope, self, self, file, line);
        }

        [RubyMethod("module_eval")]
        [RubyMethod("class_eval")]
        public static object Evaluate([NotNull]BlockParam/*!*/ block, RubyModule/*!*/ self) {
            return RubyUtils.EvaluateInModule(self, block, null);
        }

        // This method is not available in 1.8 so far, but since the usual workaround is very inefficient it is useful to have it in 1.8 as well.
        [RubyMethod("module_exec")]
        [RubyMethod("class_exec")]
        public static object Execute([NotNull]BlockParam/*!*/ block, RubyModule/*!*/ self, params object[]/*!*/ args) {
            return RubyUtils.EvaluateInModule(self, block, args);
        }

        #endregion

        #region class_variables, class_variable_defined?, class_variable_get, class_variable_set, remove_class_variable

        // not thread-safe
        [RubyMethod("class_variables")]
        public static RubyArray/*!*/ ClassVariables(RubyModule/*!*/ self) {
            var result = new RubyArray();
            self.EnumerateClassVariables((module, name, value) => {
                result.Add(self.Context.StringifyIdentifier(name));
                return false;
            });
            return result;
        }

        // not thread-safe:
        [RubyMethod("class_variable_defined?")]
        public static bool IsClassVariableDefined(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ variableName) {
            object value;
            if (self.TryResolveClassVariable(variableName, out value) == null) {
                RubyUtils.CheckClassVariableName(variableName);
                return false;
            }
            return true;
        }

        // not thread-safe:
        [RubyMethod("class_variable_get", RubyMethodAttributes.PrivateInstance)]
        public static object GetClassVariable(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ variableName) {
            object value;
            if (self.TryResolveClassVariable(variableName, out value) == null) {
                RubyUtils.CheckClassVariableName(variableName);
                throw RubyExceptions.CreateNameError("uninitialized class variable {0} in {1}", variableName, self.Name);
            }
            return value;
        }

        // not thread-safe:
        [RubyMethod("class_variable_set", RubyMethodAttributes.PrivateInstance)]
        public static object ClassVariableSet(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ variableName, object value) {
            RubyUtils.CheckClassVariableName(variableName);
            self.SetClassVariable(variableName, value);
            return value;
        }

        // not thread-safe:
        [RubyMethod("remove_class_variable", RubyMethodAttributes.PrivateInstance)]
        public static object RemoveClassVariable(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ variableName) {
            object value;
            if (!self.TryGetClassVariable(variableName, out value)) {
                RubyUtils.CheckClassVariableName(variableName);
                throw RubyExceptions.CreateNameError("class variable {0} not defined for {1}", variableName, self.Name);
            }
            self.RemoveClassVariable(variableName);
            return value;
        }

        #endregion

        #region constants, const_defined?, const_set, const_get, remove_const, const_missing

        // thread-safe:
        [RubyMethod("constants", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetGlobalConstants(RubyModule/*!*/ self, [DefaultParameterValue(true)]bool inherited) {
            return GetDefinedConstants(self.Context.ObjectClass, inherited);
        }

        // thread-safe:
        [RubyMethod("constants")]
        public static RubyArray/*!*/ GetDefinedConstants(RubyModule/*!*/ self, [DefaultParameterValue(true)]bool inherited) {
            var result = new RubyArray();
            if (inherited) {
                var visited = new Dictionary<string, bool>();

                bool hideGlobalConstants = !self.IsObjectClass;

                using (self.Context.ClassHierarchyLocker()) {
                    self.ForEachConstant(true, delegate(RubyModule/*!*/ module, string name, object value) {
                        if (name == null) {
                            // terminate enumeration when Object is reached
                            return hideGlobalConstants && module.IsObjectClass;
                        }

                        if (!visited.ContainsKey(name)) {
                            if (Tokenizer.IsConstantName(name)) {
                                result.Add(self.Context.StringifyIdentifier(name));
                            }
                            visited.Add(name, true);
                        }
                        return false;
                    });
                }

            } else {
                using (self.Context.ClassHierarchyLocker()) {
                    self.EnumerateConstants((module, name, value) => {
                        if (Tokenizer.IsConstantName(name)) {
                            result.Add(self.Context.StringifyIdentifier(name));
                        }
                        return false;
                    });
                }
            }
            return result;
        }

        // thread-safe:
        [RubyMethod("const_defined?")]
        public static bool IsConstantDefined(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ constantName) {
            RubyUtils.CheckConstantName(constantName);
            object constant;

            // MRI checks declared constans only and don't trigger autoload:
            return self.TryGetConstant(null, constantName, out constant);
        }

        // thread-safe:
        [RubyMethod("const_get")]
        public static object GetConstantValue(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ constantName) {
            return RubyUtils.GetConstant(scope.GlobalScope, self, constantName, true);
        }

        // thread-safe:
        [RubyMethod("const_set")]
        public static object SetConstantValue(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ constantName, object value) {
            RubyUtils.CheckConstantName(constantName);
            RubyUtils.SetConstant(self, constantName, value);
            return value;
        }

        // thread-safe:
        [RubyMethod("remove_const", RubyMethodAttributes.PrivateInstance)]
        public static object RemoveConstant(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ constantName) {
            object value;
            if (!self.TryRemoveConstant(constantName, out value)) {
                RubyUtils.CheckConstantName(constantName);
                throw RubyExceptions.CreateNameError("constant {0}::{1} not defined", self.Name, constantName);
            }
            return value;
        }

        [RubyMethod("const_missing")]
        public static object ConstantMissing(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ name) {
            return self.Context.ResolveMissingConstant(self, name);
        }

        #endregion

        #region autoload, autoload?

        // thread-safe:
        [RubyMethod("autoload")]
        public static void SetAutoloadedConstant(RubyModule/*!*/ self,
            [DefaultProtocol, NotNull]string/*!*/ constantName, [DefaultProtocol, NotNull]MutableString/*!*/ path) {

            RubyUtils.CheckConstantName(constantName);
            if (path.IsEmpty) {
                throw RubyExceptions.CreateArgumentError("empty file name");
            }

            self.SetAutoloadedConstant(constantName, path);
        }

        // thread-safe:
        [RubyMethod("autoload?")]
        public static MutableString GetAutoloadedConstantPath(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ constantName) {
            return self.GetAutoloadedConstantPath(constantName);
        }

        #endregion

        #region {private_|protected_|public_|}instance_methods (thread-safe)

        // thread-safe:
        [RubyMethod("instance_methods")]
        public static RubyArray/*!*/ GetInstanceMethods(RubyModule/*!*/ self) {
            return GetInstanceMethods(self, true);
        }

        // thread-safe:
        [RubyMethod("instance_methods")]
        public static RubyArray/*!*/ GetInstanceMethods(RubyModule/*!*/ self, bool inherited) {
            return GetMethods(self, inherited, RubyMethodAttributes.PublicInstance | RubyMethodAttributes.ProtectedInstance);
        }

        // thread-safe:
        [RubyMethod("private_instance_methods")]
        public static RubyArray/*!*/ GetPrivateInstanceMethods(RubyModule/*!*/ self) {
            return GetPrivateInstanceMethods(self, true);
        }

        // thread-safe:
        [RubyMethod("private_instance_methods")]
        public static RubyArray/*!*/ GetPrivateInstanceMethods(RubyModule/*!*/ self, bool inherited) {
            return GetMethods(self, inherited, RubyMethodAttributes.PrivateInstance);
        }

        // thread-safe:
        [RubyMethod("protected_instance_methods")]
        public static RubyArray/*!*/ GetProtectedInstanceMethods(RubyModule/*!*/ self) {
            return GetProtectedInstanceMethods(self, true);
        }

        // thread-safe:
        [RubyMethod("protected_instance_methods")]
        public static RubyArray/*!*/ GetProtectedInstanceMethods(RubyModule/*!*/ self, bool inherited) {
            return GetMethods(self, inherited, RubyMethodAttributes.ProtectedInstance);
        }

        // thread-safe:
        [RubyMethod("public_instance_methods")]
        public static RubyArray/*!*/ GetPublicInstanceMethods(RubyModule/*!*/ self) {
            return GetPublicInstanceMethods(self, true);
        }

        // thread-safe:
        [RubyMethod("public_instance_methods")]
        public static RubyArray/*!*/ GetPublicInstanceMethods(RubyModule/*!*/ self, bool inherited) {
            return GetMethods(self, inherited, RubyMethodAttributes.PublicInstance);
        }

        internal static RubyArray/*!*/ GetMethods(RubyModule/*!*/ self, bool inherited, RubyMethodAttributes attributes) {
            return GetMethods(self, inherited, attributes, null);
        }

        internal static RubyArray/*!*/ GetMethods(RubyModule/*!*/ self, bool inherited, RubyMethodAttributes attributes,
            IEnumerable<string> foreignMembers) {

            var result = new RubyArray();
            using (self.Context.ClassHierarchyLocker()) {
                self.ForEachMember(inherited, attributes, foreignMembers, (name, module, member) => {
                    if (member.IsInteropMember && (module.Restrictions & ModuleRestrictions.NoNameMapping) == 0 && RubyUtils.HasMangledName(name)) {
                        if (Tokenizer.IsMethodName(name) || Tokenizer.IsOperatorName(name)) {
                            result.Add(new ClrName(name));
                        }
                    } else {
                        result.Add(self.Context.StringifyIdentifier(name));
                    }
                });
            }
            return result;
        }

        #endregion

        #region {private_|protected_|public_|}method_defined? (thread-safe)

        // thread-safe:
        [RubyMethod("method_defined?")]
        public static bool MethodDefined(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ methodName) {
            RubyMemberInfo method = self.ResolveMethod(methodName, VisibilityContext.AllVisible).Info;
            return method != null && method.Visibility != RubyMethodVisibility.Private;
        }

        // thread-safe:
        [RubyMethod("private_method_defined?")]
        public static bool PrivateMethodDefined(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ methodName) {
            RubyMemberInfo method = self.ResolveMethod(methodName, VisibilityContext.AllVisible).Info;
            return method != null && method.Visibility == RubyMethodVisibility.Private;
        }

        // thread-safe:
        [RubyMethod("protected_method_defined?")]
        public static bool ProtectedMethodDefined(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ methodName) {
            RubyMemberInfo method = self.ResolveMethod(methodName, VisibilityContext.AllVisible).Info;
            return method != null && method.Visibility == RubyMethodVisibility.Protected;
        }

        // thread-safe:
        [RubyMethod("public_method_defined?")]
        public static bool PublicMethodDefined(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ methodName) {
            RubyMemberInfo method = self.ResolveMethod(methodName, VisibilityContext.AllVisible).Info;
            return method != null && method.Visibility == RubyMethodVisibility.Public;
        }

        #endregion

        #region instance_method, 1.9: public_instance_method

        // thread-safe:
        [RubyMethod("instance_method")]
        public static UnboundMethod/*!*/ GetInstanceMethod(RubyModule/*!*/ self, [DefaultProtocol, NotNull]string/*!*/ methodName) {
            RubyMemberInfo method = self.ResolveMethod(methodName, VisibilityContext.AllVisible).Info;
            if (method == null) {
                throw RubyExceptions.CreateUndefinedMethodError(self, methodName);
            }

            RubyModule constraint = self;
            if (self.IsSingletonClass && method.DeclaringModule != self) {
                constraint = ((RubyClass)self).SuperClass;
            }

            // unbound method binable to any class with "constraint" mixin:
            return new UnboundMethod(constraint, methodName, method);
        }

        #endregion

        #region to_s, name, freeze

        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToS(RubyContext/*!*/ context, RubyModule/*!*/ self) {
            return self.GetDisplayName(context, false);
        }

        [RubyMethod("name")]
        public static MutableString/*!*/ GetName(RubyContext/*!*/ context, RubyModule/*!*/ self) {
            return self.GetDisplayName(context, true);
        }

        [RubyMethod("freeze")]
        public static RubyModule/*!*/ Freeze(RubyContext/*!*/ context, RubyModule/*!*/ self) {
            self.Freeze();
            return self;
        }

        #endregion

        #region IronRuby: to_clr_type, of, []

        [RubyMethod("to_clr_type")]
        public static Type ToClrType(RubyModule/*!*/ self) {
            return self.TypeTracker != null ? self.TypeTracker.Type : null;
        }

        [RubyMethod("to_clr_ref")]
        public static RubyModule ToClrRef(RubyModule/*!*/ self) {
            try {
                return self.TypeTracker != null ? self.Context.GetClass(self.TypeTracker.Type.MakeByRefType()) : null;
            } catch (Exception) {
                throw RubyExceptions.CreateTypeError(
                    "Cannot create by-ref type for `{0}'", self.Context.GetTypeName(self.TypeTracker.Type, true)
                );
            }
        }

        [RubyMethod("of")]
        [RubyMethod("[]")]
        public static RubyModule/*!*/ Of(RubyModule/*!*/ self, [NotNullItems]params object/*!*/[]/*!*/ typeArgs) {
            if (self.TypeTracker == null) {
                throw RubyExceptions.CreateArgumentError("'{0}' is not a type", self.Name);
            }

            Type type = self.TypeTracker.Type;
            int provided = typeArgs.Length;

            if (provided == 1 && type == typeof(Array)) {
                Type elementType = Protocols.ToType(self.Context, typeArgs[0]);
                Type arrayType;
                try {
                    arrayType = elementType.MakeArrayType();
                } catch (Exception) {
                    throw RubyExceptions.CreateTypeError(
                        "Cannot create array type for `{0}'", self.Context.GetTypeName(elementType, true)
                    );
                }
                return self.Context.GetModule(arrayType);
            }

            if (!type.IsGenericTypeDefinition) {
                if (provided > 0) {
                    throw RubyExceptions.CreateArgumentError("`{0}' is not a generic type definition", self.Name);
                }
                return self;
            }

            int required = type.GetGenericArguments().Length;
            if (required != provided) {
                throw RubyExceptions.CreateArgumentError("Type `{0}' requires {1} generic type arguments, {2} provided", self.Name, required, provided);
            }

            Type concreteType = type.MakeGenericType(Protocols.ToTypes(self.Context, typeArgs));
            return self.Context.GetModule(concreteType);
        }

        [RubyMethod("of")]
        [RubyMethod("[]")]
        public static RubyModule/*!*/ Of(RubyModule/*!*/ self, int genericArity) {
            if (self.TypeTracker == null) {
                throw RubyExceptions.CreateArgumentError("`{0}' is not a type", self.Name);
            }

            Type type = self.TypeTracker.Type;

            if (!type.IsGenericTypeDefinition) {
                if (genericArity > 0) {
                    throw RubyExceptions.CreateArgumentError("`{0}' is not a generic type definition", self.Name);
                }
                return self;
            }
            
            if (type.GetGenericArguments().Length != genericArity) {
                throw RubyExceptions.CreateArgumentError("`{0}' does not have generic arity {1}", self.Name, genericArity);
            }

            return self;
        }

        #endregion

        
        #region nesting

        [RubyMethod("nesting", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetLexicalModuleNesting(RubyScope/*!*/ scope, RubyModule/*!*/ self) {
            RubyArray result = new RubyArray();
            do {
                // Ruby 1.9: the anonymous module doesn't show up
                if (scope.Module != null) {
                    result.Add(scope.Module);
                }
                scope = (RubyScope)scope.Parent;
            } while (scope != null);
            return result;
        }

        #endregion
    }
}
