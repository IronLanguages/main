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

        protected RubyClass/*!*/ DefineGlobalClass(string/*!*/ name, Type/*!*/ type, int attributes, RubyClass/*!*/ super,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[]/*!*/ mixins, Delegate/*!*/ factory) {

            return DefineGlobalClass(name, type, attributes, super, instanceTrait, classTrait, constantsInitializer, mixins, new[] { factory });
        }

        protected RubyClass/*!*/ DefineGlobalClass(string/*!*/ name, Type/*!*/ type, int attributes, RubyClass/*!*/ super,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[]/*!*/ mixins, params Delegate[] factories) {

            RubyClass result = _context.DefineLibraryClass(name, type, instanceTrait, classTrait, constantsInitializer, super, mixins, factories, (RubyModuleAttributes)attributes, _builtin);
            _context.ObjectClass.SetConstant(result.Name, result);
            return result;
        }

        protected RubyClass/*!*/ DefineClass(string/*!*/ name, Type/*!*/ type, int attributes, RubyClass/*!*/ super,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[]/*!*/ mixins, params Delegate[] factories) {

            return _context.DefineLibraryClass(name, type, instanceTrait, classTrait, constantsInitializer, super, mixins, factories, (RubyModuleAttributes)attributes, _builtin);
        }

        protected RubyClass/*!*/ ExtendClass(Type/*!*/ type, RubyClass super, 
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[]/*!*/ mixins, params Delegate[] factories) {

            return _context.DefineLibraryClass(null, type, instanceTrait, classTrait, constantsInitializer, super, mixins, factories, RubyModuleAttributes.None, _builtin);
        }

        protected RubyModule/*!*/ DefineGlobalModule(string/*!*/ name, Type/*!*/ type, int attributes,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[]/*!*/ mixins) {

            RubyModule module = _context.DefineLibraryModule(name, type, instanceTrait, classTrait, constantsInitializer, mixins, (RubyModuleAttributes)attributes);
            _context.ObjectClass.SetConstant(module.Name, module);
            return module;
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
            return _context.DefineLibraryModule(name, type, instanceTrait, classTrait, constantsInitializer, mixins, (RubyModuleAttributes)attributes);
        }

        protected RubyModule/*!*/ ExtendModule(Type/*!*/ type,
            Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            params RubyModule/*!*/[]/*!*/ mixins) {
            return _context.DefineLibraryModule(null, type, instanceTrait, classTrait, constantsInitializer, mixins, RubyModuleAttributes.None);
        }

        protected object/*!*/ DefineSingleton(Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer,
            params RubyModule/*!*/[]/*!*/ mixins) {
            Assert.NotNullItems(mixins);
            Debug.Assert(_context.ObjectClass != null);
            
            RubyModule[] expandedMixins;
            using (_context.ClassHierarchyLocker()) {
                expandedMixins = RubyModule.ExpandMixinsNoLock(_context.ObjectClass, mixins);
            }

            object result = new object();
            RubyClass singleton = _context.CreateInstanceSingleton(result, instanceTrait, classTrait, constantsInitializer, expandedMixins);

            return result;
        }

        #endregion

        // TODO: dynamic library loading?
        protected RubyClass/*!*/ GetClass(Type/*!*/ type) {
            Debug.Assert(type != null && !type.IsInterface);
            return _context.GetOrCreateClass(type);
        }

        // TODO: dynamic library loading?
        protected RubyModule/*!*/ GetModule(Type/*!*/ type) {
            Debug.Assert(type != null && type.IsInterface);
            throw new NotImplementedException("TODO");
            // return _context.GetOrCreateInterfaceModule(type);
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
