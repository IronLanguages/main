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
using System.Diagnostics;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Builtins {

    public abstract class LibraryInitializer {
        private RubyContext _context;
        private bool _builtin;

        protected RubyContext/*!*/ Context {
            get { return _context; }
        }

        protected LibraryInitializer() {
        }

        internal void LoadModules(RubyContext/*!*/ context, bool builtin) {
            Assert.NotNull(context);
            _context = context;
            _builtin = builtin;
            LoadModules();
        }

        #region Class/Module definitions/extensions

        private void PublishModule(string/*!*/ name, RubyModule/*!*/ module) {
            _context.ObjectClass.SetConstant(name, module);
            if ((module.Restrictions & ModuleRestrictions.NotPublished) == 0) {
                module.Publish(name);
            }
        }

        protected RubyClass/*!*/ DefineGlobalClass(string/*!*/ name, Type/*!*/ type, int attributes, RubyClass/*!*/ super,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[]/*!*/ mixins, Delegate/*!*/ factory) {

            return DefineGlobalClass(name, type, attributes, super, instanceTrait, classTrait, constantsInitializer, mixins, new[] { factory });
        }

        protected RubyClass/*!*/ DefineGlobalClass(string/*!*/ name, Type/*!*/ type, int attributes, RubyClass/*!*/ super,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[]/*!*/ mixins, params Delegate[] factories) {

            RubyClass result = _context.DefineLibraryClass(name, type, instanceTrait, classTrait, constantsInitializer, super, mixins, factories, (RubyModuleAttributes)attributes, _builtin);
            PublishModule(name, result);
            return result;
        }

        protected RubyClass/*!*/ DefineClass(string/*!*/ name, Type/*!*/ type, int attributes, RubyClass/*!*/ super,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[]/*!*/ mixins, params Delegate[] factories) {

            return _context.DefineLibraryClass(name, type, instanceTrait, classTrait, constantsInitializer, super, mixins, factories, (RubyModuleAttributes)attributes, _builtin);
        }

        protected RubyClass/*!*/ ExtendClass(Type/*!*/ type, int attributes, RubyClass super, 
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[]/*!*/ mixins, params Delegate[] factories) {

            return _context.DefineLibraryClass(null, type, instanceTrait, classTrait, constantsInitializer, super, mixins, factories, (RubyModuleAttributes)attributes, _builtin);
        }

        protected RubyModule/*!*/ DefineGlobalModule(string/*!*/ name, Type/*!*/ type, int attributes,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[]/*!*/ mixins) {

            RubyModule result = _context.DefineLibraryModule(name, type, instanceTrait, classTrait, constantsInitializer, mixins, (RubyModuleAttributes)attributes, _builtin);
            PublishModule(name, result);
            return result;
        }

        //
        // - Ruby module definitions: 
        //    "type" parameter specifies the primary type that identifies the module.
        //    If a library extends existing Ruby module it uses its primary type in Extends attribute parameter.
        //    E.g. Ruby.Builtins.Kernel is primary type for Kernel module, libraries maight add more methods by implementing
        //    types marked by [RubyModule(Extends = typeof(Ruby.Builtins.Kernel)]
        // - Interface modules:
        //    "type" parameter specifies the CLR interface being extended.
        // 
        protected RubyModule/*!*/ DefineModule(string/*!*/ name, Type/*!*/ type, int attributes,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            params RubyModule/*!*/[]/*!*/ mixins) {
            return _context.DefineLibraryModule(name, type, instanceTrait, classTrait, constantsInitializer, mixins, (RubyModuleAttributes)attributes, _builtin);
        }

        protected RubyModule/*!*/ ExtendModule(Type/*!*/ type, int attributes, 
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            params RubyModule/*!*/[]/*!*/ mixins) {
            return _context.DefineLibraryModule(null, type, instanceTrait, classTrait, constantsInitializer, mixins, (RubyModuleAttributes)attributes, _builtin);
        }

        protected object/*!*/ DefineSingleton(Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            params RubyModule/*!*/[]/*!*/ mixins) {
            Assert.NotNullItems(mixins);
            Debug.Assert(_context.ObjectClass != null);
            
            RubyModule[] expandedMixins;
            using (_context.ClassHierarchyLocker()) {
                expandedMixins = RubyModule.ExpandMixinsNoLock(_context.ObjectClass, mixins);
            }

            object result = new RubyObject(_context.ObjectClass);
            RubyClass singleton = _context.CreateInstanceSingleton(result, instanceTrait, classTrait, constantsInitializer, expandedMixins);

            return result;
        }

        #endregion

        #region Methods

        // thread-safe:
        public static void DefineLibraryMethod(RubyModule/*!*/ module, string/*!*/ name, int attributes, params Delegate[]/*!*/ overloads) {
            var flags = (RubyMemberFlags)(attributes & (int)RubyMethodAttributes.MemberFlagsMask);
            bool skipEvent = ((RubyMethodAttributes)attributes & RubyMethodAttributes.NoEvent) != 0;
            RubyCompatibility compatibility = (RubyCompatibility)(attributes >> RubyMethodAttribute.CompatibilityEncodingShift);
            if (compatibility > module.Context.RubyOptions.Compatibility) {
                return;
            }
            SetLibraryMethod(module, name, new RubyLibraryMethodInfo(overloads, flags, module), skipEvent);
        }

        // thread-safe:
        public static void DefineLibraryMethod(RubyModule/*!*/ module, string/*!*/ name, int attributes, Delegate/*!*/ overload) {
            DefineLibraryMethod(module, name, attributes, new[] { overload });
        }

        // thread-safe:
        public static void DefineLibraryMethod(RubyModule/*!*/ module, string/*!*/ name, int attributes, Delegate/*!*/ overload1, Delegate/*!*/ overload2) {
            DefineLibraryMethod(module, name, attributes, new[] { overload1, overload2 });
        }

        // thread-safe:
        public static void DefineLibraryMethod(RubyModule/*!*/ module, string/*!*/ name, int attributes, Delegate/*!*/ overload1, Delegate/*!*/ overload2, Delegate/*!*/ overload3) {
            DefineLibraryMethod(module, name, attributes, new[] { overload1, overload2, overload3 });
        }

        // thread-safe:
        public static void DefineLibraryMethod(RubyModule/*!*/ module, string/*!*/ name, int attributes, Delegate/*!*/ overload1, Delegate/*!*/ overload2, Delegate/*!*/ overload3, Delegate/*!*/ overload4) {
            DefineLibraryMethod(module, name, attributes, new[] { overload1, overload2, overload3, overload4 });
        }

        // thread-safe:
        public static void DefineRuleGenerator(RubyModule/*!*/ module, string/*!*/ name, int attributes, RuleGenerator/*!*/ generator) {
            Assert.NotNull(generator);
            var flags = (RubyMemberFlags)(attributes & (int)RubyMethodAttributes.VisibilityMask);
            bool skipEvent = ((RubyMethodAttributes)attributes & RubyMethodAttributes.NoEvent) != 0;
            SetLibraryMethod(module, name, new RubyCustomMethodInfo(generator, flags, module), skipEvent);
        }

        // thread-safe:
        private static void SetLibraryMethod(RubyModule/*!*/ module, string/*!*/ name, RubyMemberInfo/*!*/ method, bool noEvent) {
            var context = module.Context;
            // trigger event only for non-builtins:
            if (noEvent) {
                // TODO: hoist lock?
                using (context.ClassHierarchyLocker()) {
                    module.SetMethodNoMutateNoEventNoLock(context, name, method);
                }
            } else {
                module.AddMethod(context, name, method);
            }
        }

        #endregion

        #region Constants

        // thread-safe:
        public static void SetBuiltinConstant(RubyModule/*!*/ module, string/*!*/ name, object value) {
            // TODO: hoist the lock?
            using (module.Context.ClassHierarchyLocker()) {
                module.SetConstantNoMutateNoLock(name, value);
            }
        }

        // thread-safe:
        public static void SetConstant(RubyModule/*!*/ module, string/*!*/ name, object value) {
            module.SetConstant(name, value);
        }

        #endregion

        protected RubyClass/*!*/ GetClass(Type/*!*/ type) {
            Debug.Assert(type != null && !type.IsInterface);
            // TODO: CLR class vs library class:
            return _context.GetOrCreateClass(type);
        }

        protected RubyModule/*!*/ GetModule(Type/*!*/ type) {
            Debug.Assert(type != null);
            RubyModule result;
            if (!_context.TryGetModule(type, out result)) {
                // TODO: we should load the library that contains this type if it is not a CLR type
                throw new NotSupportedException(String.Format("Ruby library that contains type {0} hasn't been loaded yet.", type));
            }
            return result;
        }

        protected virtual void LoadModules() { 
            throw new NotImplementedException(); 
        }

        public static string/*!*/ GetTypeName(string/*!*/ libraryNamespace) {
            ContractUtils.RequiresNotNull(libraryNamespace, "libraryNamespace");
            return libraryNamespace.Substring(libraryNamespace.LastIndexOf(Type.Delimiter) + 1) + "LibraryInitializer";
        }

        public static string/*!*/ GetFullTypeName(string/*!*/ libraryNamespace) {
            return libraryNamespace + Type.Delimiter + GetTypeName(libraryNamespace);
        }

        public static string/*!*/ GetBuiltinsFullTypeName() {
            return GetFullTypeName(typeof(RubyClass).Namespace);
        }
    }
}
