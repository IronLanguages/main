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

namespace HostingTest {

    public class CodeSnippet {
        public string ID;
        public string Code;
        public string Description;

        public CodeSnippet(string id, string description, string code) {
            ID = id ; Code = code;
            Description = description;
        }
    }
    internal class CodeSnippetCollection {
        internal CodeSnippet[] AllSnippets = null;
        internal CodeSnippet GetCodeSnippetByID(string id) {
            foreach (CodeSnippet cs in AllSnippets) {
                if (cs.ID.ToLower() == id.ToLower()) {
                    return cs;
                }
            }
            return null;
        }

        internal string this[string id] {
            get {
                CodeSnippet cs = GetCodeSnippetByID( id );
                return cs == null ? null : cs.Code;
            }
        }
    }

    internal class PreDefinedCodeSnippets {

        internal CodeSnippetCollection langCollection;

        internal PreDefinedCodeSnippets() {
            string lang = GetLanguage();
            if (lang == "python") {//check if lang is python)
                langCollection = new PythonCodeSnippets();
            }
            else if (lang == "ironruby"){
                langCollection = new RubyCodeSnippets();
            }

        }

        private string GetLanguage() {
            return "python";
        }

        public string this[string id] {
            get {
                return langCollection[id];
            }
        }

        internal CodeSnippet[] AllSnippets {
            get {
                return langCollection.AllSnippets;
            }
        }
    }

}

