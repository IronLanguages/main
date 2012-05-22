using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

[assembly: PythonModule("pyexpat", typeof(IronPython.Modules.PythonExpatModule))]
namespace IronPython.Modules {
    public static class PythonExpatModule {

        public const int XML_PARAM_ENTITY_PARSING_UNLESS_STANDALONE = 1;
        public const int XML_PARAM_ENTITY_PARSING_ALWAYS = 2;

        [SpecialName]
        public static void PerformModuleReload(PythonContext context, PythonDictionary dict) {
            InitModuleExceptions(context, dict);
        }

        public static xmlparser ParserCreate(CodeContext/*!*/ context, [DefaultParameterValue(null)] object encoding, [DefaultParameterValue(null)] object namespace_separator) {
            return new xmlparser(namespace_separator);
        }

        public static string ErrorString(CodeContext/*!*/ context, int errno) {
            return "unknown error";
        }

        [PythonType]
        public class xmlparser {
            private StringBuilder _data = new StringBuilder();
            private Stack<string> _ns_stack = new Stack<string>();
            private object _separator;
            private PythonDictionary _intern = new PythonDictionary();
            private XmlReader _reader;

            private delegate void ElementHandlerDelegate(CodeContext/*!*/ context);
            private Dictionary<string, ElementHandlerDelegate> _handlers = new Dictionary<string, ElementHandlerDelegate>();

            /*
             // Stub for xml.dom
        "buffer_text",
        "specified_attributes",


        // Stub for ElementTree
        "DefaultHandlerExpand",
        // Stub for xml.sax
        "ProcessingInstructionHandler",
        "UnparsedEntityDeclHandler",
        "NotationDeclHandler",
        "ExternalEntityRefHandler",
        "EndDoctypeDeclHandler",
        // Stub for xml.dom
        "StartDoctypeDeclHandler",
        "EntityDeclHandler",
        "CommentHandler",
        "StartCdataSectionHandler",
        "EndCdataSectionHandler",
        "XmlDeclHandler",
        "ElementDeclHandler",
        "AttlistDeclHandler",
        // Stub for Kid
        "DefaultHandler",
             * */

            public object StartNamespaceDeclHandler;
            public object StartElementHandler;
            public object EndNamespaceDeclHandler;
            public object EndElementHandler;
            public object CharacterDataHandler;

            public xmlparser(object namespace_separator) {
                _separator = namespace_separator;
                _handlers.Add("Element", HandleElement);
                _handlers.Add("EndElement", HandleEndElement);
                _handlers.Add("Text", HandleText);
                _handlers.Add("CDATA", HandleText);
                _handlers.Add("Whitespace", HandleText);
                ordered_attributes = false;
                namespace_prefixes = false;
                returns_unicode = true;

                StartNamespaceDeclHandler = null;
                StartElementHandler = null;
                EndNamespaceDeclHandler = null;
                EndElementHandler = null;
                CharacterDataHandler = null;
            }

            public bool ordered_attributes {
                get;
                set;
            }

            public bool namespace_prefixes {
                get;
                set;
            }

            public bool returns_unicode {
                get;
                set;
            }

            public void Parse(CodeContext/*!*/ context, string data, [DefaultParameterValue(false)] bool isFinal) {
                _data.Append(data);
                if (isFinal) {
                    string content = _data.ToString();
                    _data = new StringBuilder();
                    Parse(context, content);
                }
            }

            public void ParseFile(CodeContext/*!*/ context, PythonFile file) {
                Parse(context, file);
            }

            private void Parse(CodeContext/*!*/ context, string content) {
                XmlReaderSettings settings = new XmlReaderSettings() { DtdProcessing = DtdProcessing.Parse };
                _reader = XmlTextReader.Create(new StringReader(content), settings);
                while (_reader.Read()) {
                    string typename = Enum.GetName(typeof(XmlNodeType), _reader.NodeType);
                    if (_handlers.ContainsKey(typename)) {
                        _handlers[typename](context);
                    }
                }
            }

            private void Parse(CodeContext/*!*/ context, IEnumerator<string> content) {

            }

            public int CurrentLineNumber {
                get {
                    return ((IXmlLineInfo)_reader).LineNumber;
                }
            }

            public int CurrentColumnNumber {
                get {
                    return ((IXmlLineInfo)_reader).LinePosition;
                }
            }

            private string qname() {
                if (_separator == null)
                    return _reader.Name;

                if (!string.IsNullOrWhiteSpace(_reader.NamespaceURI)) {
                    string temp = _reader.NamespaceURI + (char)_separator + _reader.LocalName;
                    if (namespace_prefixes && !string.IsNullOrWhiteSpace(_reader.Prefix))
                        return temp + (char)_separator + _reader.Prefix;
                    return temp;
                }
                return _reader.LocalName;
            }

            private void HandleElement(CodeContext/*!*/ context) {
                string name = qname();
                _ns_stack.Push(null);
                PythonDictionary attributes_dict = new PythonDictionary();
                List attributes_list = new List();

                while (_reader.MoveToNextAttribute()) {
                    if (_reader.Prefix == "xmlns") {
                        string prefix = _reader.LocalName;
                        string uri = _reader.Value;
                        _ns_stack.Push(prefix);
                        if (StartNamespaceDeclHandler != null) {
                            PythonCalls.Call(context, StartNamespaceDeclHandler, prefix, uri);
                        }
                        continue;
                    }

                    string key = qname();
                    string value = _reader.Value;
                    if (ordered_attributes) {
                        attributes_list.Add(key);
                        attributes_list.Add(value);
                    } else {
                        attributes_dict[key] = value;
                    }
                }
                _reader.MoveToElement();
                if (StartElementHandler != null) {
                    PythonCalls.Call(context, StartElementHandler, name, ordered_attributes ? (object)attributes_list : (object)attributes_dict);
                }

                // EndElement node is not generated for empty elements.
                // Call its handler here.
                if (_reader.IsEmptyElement) {
                    HandleEndElement(context);
                }
            }

            private void HandleEndElement(CodeContext/*!*/ context) {
                string name = qname();
                if (EndElementHandler != null) {
                    PythonCalls.Call(context, EndElementHandler, name);
                }

                while (true) {
                    string prefix = _ns_stack.Pop();
                    if (prefix == null) {
                        break;
                    }

                    if (EndNamespaceDeclHandler != null) {
                        PythonCalls.Call(context, EndNamespaceDeclHandler, prefix);
                    }
                }
            }

            private void HandleText(CodeContext/*!*/ context) {
                string data = _reader.Value;
                if (CharacterDataHandler != null) {
                    PythonCalls.Call(context, CharacterDataHandler, data);
                }
            }

            /// <summary>
            ///  Stub for xml.sax
            /// </summary>
            /// <param name="base"></param>
            public void SetBase(object @base) {

            }

            public bool SetParamEntiryParsing(bool flag) {
                return true;
            }

            /// <summary>
            /// Stub for Kid
            /// </summary>
            public void UseForeignDTD() {

            }
        }

        public static PythonType ExpatError;
        public static PythonType error;

        internal static Exception MakeError(params object[] args) {
            return PythonOps.CreateThrowable(ExpatError, args);
        }

        private static void InitModuleExceptions(PythonContext context, PythonDictionary dict) {
            error = ExpatError = context.EnsureModuleException("pyexpat.ExpatError",
                PythonExceptions.StandardError, dict, "ExpatError", "pyexpat");
        }
    }
}
