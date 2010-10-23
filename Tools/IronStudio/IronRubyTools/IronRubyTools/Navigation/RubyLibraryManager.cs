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
using System.Runtime.InteropServices;
using Microsoft.IronStudio.Navigation;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.IronRubyTools.Navigation {

    /// <summary>
    /// This interface defines the service that finds IronRuby files inside a hierarchy
    /// and builds the informations to expose to the class view or object browser.
    /// </summary>
    [Guid(RubyConstants.LibraryManagerServiceGuid)]
    public interface IRubyLibraryManager : ILibraryManager {        
    }

    /// <summary>
    /// Implementation of the service that builds the information to expose to the symbols
    /// navigation tools (class view or object browser) from the Ruby files inside a
    /// hierarchy.
    /// </summary>
    [Guid(RubyConstants.LibraryManagerGuid)]
    internal class RubyLibraryManager : LibraryManager, IRubyLibraryManager {
        private readonly IronRubyToolsPackage/*!*/ _package;

        public RubyLibraryManager(IronRubyToolsPackage/*!*/ package)
            : base(package) {
            _package = package;
        }

        protected override LibraryNode CreateLibraryNode(IScopeNode subItem, string namePrefix, IVsHierarchy hierarchy, uint itemid) {
            return new RubyLibraryNode(subItem, namePrefix, hierarchy, itemid);            
        }

        protected override void OnNewFile(LibraryTask task) {
            //AnalysisItem item;
            //if (task.TextBuffer != null) {
            //    item = task.TextBuffer.GetAnalysis();
            //} else {
            //    item = IronRubyToolsPackage.Instance.Analyzer.AnalyzeFile(task.FileName);
            //}

            // We subscribe to OnNewAnalysis here instead of OnNewParseTree so that 
            // in the future we can use the analysis to include type information in the
            // object browser (for example we could include base type information with
            // links elsewhere in the object browser).
            //item.OnNewAnalysis += (sender, args) => {
            //    FileParsed(task, new AstScopeNode(item.CurrentTree, item.Entry));
            //};
        }
    }
}
