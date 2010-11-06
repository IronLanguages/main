/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.Scripting.Metadata;

namespace Metadata {
    public class RubyContext {
        internal RubyModule ObjectModule;

        internal RubyContext() {
            ObjectModule = new RubyModule(this, new ClrTypeInfo());
        }

        internal void OnAssemblyLoaded(Assembly assembly) {
            foreach (var module in assembly.GetModules()) {
                NamespaceTree tree = new NamespaceTree();
                tree.Add(module.GetMetadataTables());
                ObjectModule.AddClrModules(tree.Root);
            }
        }
    }
    
    internal abstract class ClrModuleInfo {
        internal abstract void InitializeConstants(RubyContext context, Dictionary<string, object> constants);
    }

    internal sealed class ClrNamespaceInfo : ClrModuleInfo {
        internal NamespaceTreeNode _namespaceNode;

        internal ClrNamespaceInfo(NamespaceTreeNode namespaceNode) {
            _namespaceNode = namespaceNode;
        }

        internal override void InitializeConstants(RubyContext context, Dictionary<string, object> constants) {
            foreach (TypeDef typeDef in _namespaceNode.GetTypeDefs()) {
                constants[typeDef.Name.ToString()] = new RubyModule(context, new ClrTypeInfo(typeDef));
            }

            foreach (var ns in _namespaceNode.GetNamespaces()) {
                constants[ns.Name.ToString()] = new RubyModule(context, new ClrNamespaceInfo(ns));
            }

            // TODO: resolve namespace/type conflicts (C# doesn't allow, but there could be some)

            _namespaceNode = null;
        }
    }

    internal sealed class ClrTypeInfo : ClrModuleInfo {
        private Type _type;
        private TypeDef _typeDef;
        // TODO: private List<RubyModule> _genericOverloads;

        // object
        internal ClrTypeInfo() {
            _type = typeof(object);
        }

        internal ClrTypeInfo(TypeDef typeDef) {
            _typeDef = typeDef;
        }

        internal override void InitializeConstants(RubyContext context, Dictionary<string, object> constants) {
            if (_type != typeof(object)) {
                var typeNesting = ((MetadataRecord)_typeDef).Tables.GetTypeNesting();

                foreach (TypeDef typeDef in typeNesting.GetNestedTypes(_typeDef)) {
                    constants.Add(typeDef.Name.ToString(), new RubyModule(context, new ClrTypeInfo(typeDef)));
                }
            }
        }

        internal Type GetClrType() {
            if (_type != null) {
                Module module = ((MetadataRecord)_typeDef).Tables.Module;
                Debug.Assert(module != null, "We only work with loaded assemblies");
                _type = module.ResolveType(_typeDef.Record.Token.Value);
            }
            return _type;
        }
    }

    public class RubyModule {
        private readonly RubyContext _context;
        private ClrModuleInfo _clrModule;
        private Dictionary<string, object> _constants;

        internal RubyModule(RubyContext context, ClrModuleInfo clrModule) {
            _context = context;
            _clrModule = clrModule;
        }

        internal virtual void InitializeConstants() {
            if (_constants == null) {
                return;
            }

            _constants = new Dictionary<string, object>();
            if (_clrModule != null) {
                _clrModule.InitializeConstants(_context, _constants);
            }
        }

        internal void AddClrModules(NamespaceTreeNode treeNode) {
            foreach (var ns in treeNode.GetNamespaces()) {
                if (_constants == null) {
                    // not initialized yet:

                    if (_clrModule == null) {
                        // promote the Ruby module to a CLR namespace:

                    } else {
                        // TODO:
                        // if namespace => merge
                        // if type => ???
                    }
                } else {
                    // already initialized:
                    // TODO: MergeInitialized();
                }
            }

            foreach (var typeDef in treeNode.GetTypeDefs()) {

            }
        }
    }

    public static class RubyScenario {
        public static void Run() {
            var context = new RubyContext();
            context.OnAssemblyLoaded(typeof(object).Assembly);
            context.OnAssemblyLoaded(typeof(Expression).Assembly);
        }
    }
}
