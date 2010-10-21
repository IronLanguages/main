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

using System;
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Runtime.Remoting;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.IronStudio.Repl {
    public struct Output {
        public string Text { get; set; }
        public object Object { get; set; }
    }

    public interface IReplEvaluator : IDisposable {
        void Start();
        void TextViewCreated(IReplWindow window, ITextView view);

        string/*!*/ Prompt { get; }
        string/*!*/ CommandPrefix { get; }
        bool DisplayPromptInMargin { get; }

        // Parsing and Execution
        bool CheckSyntax(string text, SourceCodeKind kind);
        bool CanExecuteText(string/*!*/ text);
        bool ExecuteText(string text, Action<bool, ObjectHandle> completion);
        void AbortCommand();
        string InsertData(object data, string prefix);
        string FormatException(ObjectHandle exception);
        void Reset();

        // IO
        void FlushOutput();
        void SetIO(Action<object, Output> outputDelegate, Func<string> inputDelegate);

        // Intellisense support
        /*
        string Language { get; }
        Member[] GetCompletions(string text);
        Member[] GetModules();
        ReplOverloadResult[] GetSignatures(string text);

        // Workspace support
        WorkspaceVariable[] GetVariablesInGlobalScope();*/
    }

    public interface IDlrEvaluator : IReplEvaluator {
        ICollection<MemberDoc> GetMemberNames(string expression);
        ICollection<OverloadDoc> GetSignatureDocumentation(string expression);
    }
}
