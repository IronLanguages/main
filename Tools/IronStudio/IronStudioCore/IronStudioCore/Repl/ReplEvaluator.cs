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
using System.Runtime.Remoting;
using Microsoft.IronStudio.Repl;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.IronStudio.Library.Repl {
    public abstract class ReplEvaluator : MarshalByRefObject, IDisposable, IReplEvaluator {
        private readonly string/*!*/ _language;
        protected readonly OutputBuffer/*!*/ _output;

        public Action<object, Output> WriteOutput;
        public Func<string> ReadInput;

        // Concrete subclasses constructed via reflection when deserialized from the registry.
        protected ReplEvaluator(string/*!*/ language) {
            ContractUtils.RequiresNotNull(language, "language");

            _language = language;
            _output = new OutputBuffer();
            _output.OutputText += OnFlushText;
        }

        public virtual void TextViewCreated(IReplWindow window, ITextView view) {
        }

        public virtual void Start() {
        }

        public virtual void Reset() {
        }

        public virtual string FormatException(ObjectHandle exception) {
            return exception.ToString();
        }

        public void SetIO(Action<object, Output> outputDelegate, Func<string> inputDelegate) {
            WriteOutput = outputDelegate;
            ReadInput = inputDelegate;
        }

        private void OnFlushText(string text) {
            var writeOutput = WriteOutput;
            if (writeOutput != null) {
                writeOutput(null, new Output { Text = text });
            }
        }

        public void FlushOutput() {
            _output.Flush();
        }

        protected internal void WriteObject(object obj, string text) {
            FlushOutput();

            var writeOutput = WriteOutput;
            if (writeOutput != null) {
                writeOutput(null, new Output { Text = text, Object = obj });
            }
        }

        protected void Write(string text) {
            _output.Write(text);
        }

        protected void WriteLine(string text) {
            _output.Write(text);
            _output.Write("\r\n");
        }

        protected void WriteException(ObjectHandle exception) {
            WriteObject(exception, FormatException(exception));
        }

        public string Language {
            get { return _language; }
        }

        public virtual bool CanExecuteText(string text) {
            return true;
        }

        abstract public bool ExecuteText(string text, Action<bool, ObjectHandle> completion);

        public virtual bool CheckSyntax(string text, SourceCodeKind kind) {
            return true;
        }

        public virtual void AbortCommand() {
        }

        public virtual void Dispose() {
            AbortCommand();
            _output.Dispose();
        }

        public virtual string InsertData(object data, string prefix) {
            return null;
        }
#if FALSE
        public virtual Member[] GetCompletions(string text) {
            string[] symbols = text.Split('.');
            object obj = GetRootObject();
            if (obj == null) {
                return Member.None;
            }
            for (int i = 0; i < symbols.Length - 1; i++) {
                obj = GetObjectMember(obj, symbols[i]);
                if (obj == null) {
                    return Member.None;
                }
            }

            string lastSymbol = symbols[symbols.Length - 1];
            var members = GetObjectMemberNames(obj, lastSymbol);
            var result = new Member[members.Length];
            for (int i = 0; i < members.Length; i++) {
                result[i] = new Member { Name = members[i] };
            }
            return result;
        }

        public virtual Member[] GetModules() {
            return Member.None;
        }

        public virtual ReplOverloadResult[] GetSignatures(string text) {
            return new ReplOverloadResult[0];
        }
#endif
        protected virtual object GetRootObject() {
            return null;
        }

        protected virtual string[] GetObjectMemberNames(ObjectHandle obj, string startsWith) {
            return ArrayUtils.EmptyStrings;
        }
        
        public virtual string/*!*/ Prompt {
            get { return "»"; }
        }

        public virtual string/*!*/ CommandPrefix {
            get { return "%"; }
        }

        public virtual bool DisplayPromptInMargin {
            get { return false; }
        }
    }
}
