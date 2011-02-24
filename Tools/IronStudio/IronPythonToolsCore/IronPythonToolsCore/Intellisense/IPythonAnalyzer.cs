/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using Microsoft.IronPythonTools.Intellisense;
using Microsoft.PyAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.IronPythonTools.Intellisense {

    /// <summary>
    /// Provides access to the Python analysis of files and text buffers.
    /// 
    /// The analysis can be queried for information about expressions and new files can be added for the analysis to consider.
    /// </summary>
    public interface IPythonAnalyzer {
        /// <summary>
        /// Analyzes the specified text view and wires up support for tracking changes to the text view.
        /// </summary>
        IProjectEntry AnalyzeTextView(ITextView textView);

        /// <summary>
        /// Analyzes the specified file on disk.
        /// </summary>
        IProjectEntry AnalyzeFile(string path);

        /// <summary>
        /// Returns the project entry for the given file name or null if the file is not being analyzed.
        /// </summary>
        IProjectEntry GetAnalysisFromFile(string filename);

        /// <summary>
        /// Gets a ExpressionAnalysis for the expression at the provided span.  If the span is in
        /// part of an identifier then the expression is extended to complete the identifier.
        /// </summary>
        ExpressionAnalysis AnalyzeExpression(ITextSnapshot snapshot, ITextBuffer buffer, ITrackingSpan span);

        /// <summary>
        /// Gets a list of signatuers available for the expression at the provided location in the snapshot.
        /// </summary>
        SignatureAnalysis GetSignatures(ITextSnapshot snapshot, ITextBuffer buffer, ITrackingSpan span);

        /// <summary>
        /// Gets a CompletionAnalysis providing a list of possible members the user can dot through.
        /// </summary>
        CompletionAnalysis GetCompletions(ITextSnapshot snapshot, ITextBuffer buffer, ITrackingSpan span, bool intersectMembers = true, bool hideAdvancedMembers = false);

        /// <summary>
        /// Returns true if there are currently items being analyzed.
        /// </summary>
        bool IsAnalyzing { get; }

        /// <summary>
        /// Gets or sets whether the implicit project support is enabled and loose files shouldbe included.
        /// </summary>
        bool ImplicitProject { get; set; }
    }
}
