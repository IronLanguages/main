/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Text;
using IronRuby.Compiler.Ast;
using IronRuby.Runtime;
using Microsoft.IronStudio.Navigation;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.IronRubyTools.Navigation {
    public class RubyLibraryNode : CommonLibraryNode {
        public RubyLibraryNode(IScopeNode scope, string namePrefix, IVsHierarchy hierarchy, uint itemId)
            : base(scope, namePrefix, hierarchy, itemId) { }

        protected RubyLibraryNode(RubyLibraryNode node) : base(node) { }

        protected override LibraryNode Clone() {
            return new RubyLibraryNode(this);
        }

        public override StandardGlyphGroup GlyphType {
            get {
                //if (ScopeNode is FunctionScopeNode) {
                //    return StandardGlyphGroup.GlyphGroupMethod;
                //}

                return StandardGlyphGroup.GlyphGroupClass;
            }
        }

        public override string GetTextRepresentation(VSTREETEXTOPTIONS options) {
            // TODO:
            //FunctionScopeNode funcScope = ScopeNode as FunctionScopeNode;
            //if (funcScope != null) {
            //    StringBuilder sb = new StringBuilder();
            //    GetFunctionDescription(funcScope.Definition, (text, kind, arg) => {
            //        sb.Append(text);
            //    });
            //    return sb.ToString();
            //}

            return Name;
        }

        public override void FillDescription(_VSOBJDESCOPTIONS flags, IVsObjectBrowserDescription3 description) {
            description.ClearDescriptionText();
            // TODO:
            //FunctionScopeNode funcScope = ScopeNode as FunctionScopeNode;
            //if (funcScope != null) {
            //    description.AddDescriptionText3("def ", VSOBDESCRIPTIONSECTION.OBDS_MISC, null);
            //    var def = funcScope.Definition;
            //    GetFunctionDescription(def, (text, kind, arg) => {
            //        description.AddDescriptionText3(text, kind, arg);
            //    });
            //    description.AddDescriptionText3(null, VSOBDESCRIPTIONSECTION.OBDS_ENDDECL, null);
            //    if (def.Body.Documentation != null) {
            //        description.AddDescriptionText3("    " + def.Body.Documentation, VSOBDESCRIPTIONSECTION.OBDS_MISC, null);
            //    }
            //} else {
                description.AddDescriptionText3("class ", VSOBDESCRIPTIONSECTION.OBDS_MISC, null);
                description.AddDescriptionText3(ScopeNode.Name, VSOBDESCRIPTIONSECTION.OBDS_NAME, null);
            // }
        }

        // TODO:
        //private void GetFunctionDescription(FunctionDefinition def, Action<string, VSOBDESCRIPTIONSECTION, IVsNavInfo> addDescription) {
        //    addDescription(ScopeNode.Name, VSOBDESCRIPTIONSECTION.OBDS_NAME, null);
        //    addDescription("(", VSOBDESCRIPTIONSECTION.OBDS_MISC, null);

        //    for (int i = 0; i < def.Parameters.Count; i++) {
        //        if (i != 0) {
        //            addDescription(", ", VSOBDESCRIPTIONSECTION.OBDS_MISC, null);
        //        }

        //        var curParam = def.Parameters[i];

        //        string name = curParam.Name;
        //        if (curParam.IsDictionary) {
        //            name = "**" + name;
        //        } else if (curParam.IsList) {
        //            name = "*" + curParam.Name;
        //        }

        //        if (curParam.DefaultValue != null) {
        //            // TODO: Support all possible expressions for default values, we should
        //            // probably have a RubyAst walker for expressions or we should add ToCodeString()
        //            // onto Ruby ASTs so they can round trip
        //            ConstantExpression defaultValue = curParam.DefaultValue as ConstantExpression;
        //            if (defaultValue != null) {
        //                name = name + " = " + RubyOps.Repr(DefaultContext.Default, defaultValue.Value);
        //            }
        //        }

        //        addDescription(name, VSOBDESCRIPTIONSECTION.OBDS_PARAM, null);
        //    }
        //    addDescription(")\n", VSOBDESCRIPTIONSECTION.OBDS_MISC, null);
        //}
    }
}
