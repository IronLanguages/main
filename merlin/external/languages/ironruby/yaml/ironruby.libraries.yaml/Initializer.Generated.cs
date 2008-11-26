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

[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.Yaml.YamlLibraryInitializer))]

namespace IronRuby.StandardLibrary.Yaml {
    public sealed class YamlLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            
            
            ExtendModule(typeof(IronRuby.Builtins.FalseClass), new System.Action<IronRuby.Builtins.RubyModule>(LoadIronRuby__Builtins__FalseClass_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(IronRuby.Builtins.Hash), new System.Action<IronRuby.Builtins.RubyModule>(LoadIronRuby__Builtins__Hash_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray, null);
            ExtendModule(typeof(IronRuby.Builtins.Integer), new System.Action<IronRuby.Builtins.RubyModule>(LoadIronRuby__Builtins__Integer_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.MutableString), new System.Action<IronRuby.Builtins.RubyModule>(LoadIronRuby__Builtins__MutableString_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.Range), new System.Action<IronRuby.Builtins.RubyModule>(LoadIronRuby__Builtins__Range_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.RubyArray), new System.Action<IronRuby.Builtins.RubyModule>(LoadIronRuby__Builtins__RubyArray_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(IronRuby.Builtins.RubyClass), new System.Action<IronRuby.Builtins.RubyModule>(LoadIronRuby__Builtins__RubyClass_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray, null);
            ExtendClass(typeof(IronRuby.Builtins.RubyModule), new System.Action<IronRuby.Builtins.RubyModule>(LoadIronRuby__Builtins__RubyModule_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray, null);
            ExtendModule(typeof(IronRuby.Builtins.RubyRegex), new System.Action<IronRuby.Builtins.RubyModule>(LoadIronRuby__Builtins__RubyRegex_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.RubyStruct), new System.Action<IronRuby.Builtins.RubyModule>(LoadIronRuby__Builtins__RubyStruct_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.TrueClass), new System.Action<IronRuby.Builtins.RubyModule>(LoadIronRuby__Builtins__TrueClass_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(IronRuby.StandardLibrary.Yaml.Node), new System.Action<IronRuby.Builtins.RubyModule>(LoadIronRuby__StandardLibrary__Yaml__Node_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray, null);
            ExtendModule(typeof(Microsoft.Scripting.Math.BigInteger), new System.Action<IronRuby.Builtins.RubyModule>(LoadMicrosoft__Scripting__Math__BigInteger_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(Microsoft.Scripting.SymbolId), new System.Action<IronRuby.Builtins.RubyModule>(LoadMicrosoft__Scripting__SymbolId_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(System.DateTime), new System.Action<IronRuby.Builtins.RubyModule>(LoadSystem__DateTime_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(System.Double), new System.Action<IronRuby.Builtins.RubyModule>(LoadSystem__Double_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(System.Dynamic.Null), new System.Action<IronRuby.Builtins.RubyModule>(LoadSystem__Dynamic__Null_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(System.Exception), new System.Action<IronRuby.Builtins.RubyModule>(LoadSystem__Exception_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(System.Object), new System.Action<IronRuby.Builtins.RubyModule>(LoadSystem__Object_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray, null);
            IronRuby.Builtins.RubyModule def1 = DefineGlobalModule("YAML", typeof(IronRuby.StandardLibrary.Yaml.RubyYaml), null, new System.Action<IronRuby.Builtins.RubyModule>(LoadYAML_Class), IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def2 = DefineClass("YAML::Stream", typeof(IronRuby.StandardLibrary.Yaml.YamlStream), Context.ObjectClass, new System.Action<IronRuby.Builtins.RubyModule>(LoadYAML__Stream_Instance), null, IronRuby.Builtins.RubyModule.EmptyArray, new System.Delegate[] {
                new System.Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.Hash, IronRuby.StandardLibrary.Yaml.YamlStream>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStreamOps.CreateStream),
            });
            def1.SetConstant("Stream", def2);
        }
        
        private void LoadIronRuby__Builtins__FalseClass_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlFalseOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlFalseOps.ToYaml),
            });
            
        }
        
        private void LoadIronRuby__Builtins__Hash_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlHashOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Builtins.Hash, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlHashOps.ToYamlNode),
            });
            
        }
        
        private void LoadIronRuby__Builtins__Integer_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlIntegerOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlIntegerOps.ToYaml),
            });
            
        }
        
        private void LoadIronRuby__Builtins__MutableString_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("is_binary_data?", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, System.Object>(IronRuby.StandardLibrary.Yaml.YamlStringOps.IsBinaryData),
            });
            
            module.DefineLibraryMethod("is_complex_yaml?", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.StandardLibrary.Yaml.YamlStringOps.IsComplexYaml),
            });
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlStringOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlStringOps.ToYamlNode),
            });
            
        }
        
        private void LoadIronRuby__Builtins__Range_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Builtins.Range, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlRangeOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlRangeOps.ToYaml),
            });
            
        }
        
        private void LoadIronRuby__Builtins__RubyArray_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlArrayOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyArray, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlArrayOps.ToYamlNode),
            });
            
        }
        
        private void LoadIronRuby__Builtins__RubyClass_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlClassOps.ToYamlNode),
            });
            
        }
        
        private void LoadIronRuby__Builtins__RubyModule_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("yaml_as", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.YamlModuleOps.YamlAs),
            });
            
        }
        
        private void LoadIronRuby__Builtins__RubyRegex_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlRegexpOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlRegexpOps.ToYaml),
            });
            
        }
        
        private void LoadIronRuby__Builtins__RubyStruct_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Builtins.RubyStruct, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlStructOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Builtins.RubyStruct, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlStructOps.ToYamlNode),
            });
            
        }
        
        private void LoadIronRuby__Builtins__TrueClass_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlTrueOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlTrueOps.ToYaml),
            });
            
        }
        
        private void LoadIronRuby__StandardLibrary__Yaml__Node_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("transform", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyScope, IronRuby.StandardLibrary.Yaml.Node, System.Object>(IronRuby.StandardLibrary.Yaml.YamlNodeOps.Transform),
            });
            
        }
        
        private void LoadMicrosoft__Scripting__Math__BigInteger_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<Microsoft.Scripting.Math.BigInteger, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlBigIntegerOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<Microsoft.Scripting.Math.BigInteger, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlBigIntegerOps.ToYaml),
            });
            
        }
        
        private void LoadMicrosoft__Scripting__SymbolId_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlSymbolOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlSymbolOps.ToYaml),
            });
            
        }
        
        private void LoadSystem__DateTime_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<System.DateTime, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.DateTimeOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<System.DateTime, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.DateTimeOps.ToYaml),
            });
            
        }
        
        private void LoadSystem__Double_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<System.Double, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlDoubleOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<System.Double, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlDoubleOps.ToYaml),
            });
            
        }
        
        private void LoadSystem__Dynamic__Null_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlNilOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlNilOps.ToYaml),
            });
            
        }
        
        private void LoadSystem__Exception_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlExceptionOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<System.Exception, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlExceptionOps.ToYamlNode),
            });
            
        }
        
        private void LoadSystem__Object_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("taguri", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.TagUri),
            });
            
            module.DefineLibraryMethod("to_yaml", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, System.Object, System.Object[], System.Object>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.ToYaml),
            });
            
            module.DefineLibraryMethod("to_yaml_node", 0x11, new System.Delegate[] {
                new System.Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, System.Object>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.ToYamlProperties),
            });
            
            module.DefineLibraryMethod("to_yaml_properties", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.ToYamlProperties),
            });
            
            module.DefineLibraryMethod("to_yaml_style", 0x11, new System.Delegate[] {
                new System.Func<System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.ToYamlStyle),
            });
            
        }
        
        private void LoadYAML_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("add_domain_type", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.AddDomainType),
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.AddDomainType),
            });
            
            module.DefineLibraryMethod("dump", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Builtins.RubyModule, System.Object, IronRuby.Builtins.RubyIO, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Dump),
            });
            
            module.DefineLibraryMethod("dump_all", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Builtins.RubyModule, System.Collections.IEnumerable, IronRuby.Builtins.RubyIO, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.DumpAll),
            });
            
            module.DefineLibraryMethod("dump_stream", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object[], System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.DumpStream),
            });
            
            module.DefineLibraryMethod("each_document", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.EachDocument),
            });
            
            module.DefineLibraryMethod("each_node", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.ParseDocuments),
            });
            
            module.DefineLibraryMethod("load", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Load),
            });
            
            module.DefineLibraryMethod("load_documents", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.EachDocument),
            });
            
            module.DefineLibraryMethod("load_file", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.LoadFile),
            });
            
            module.DefineLibraryMethod("load_stream", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.LoadStream),
            });
            
            module.DefineLibraryMethod("parse", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Parse),
            });
            
            module.DefineLibraryMethod("parse_documents", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.ParseDocuments),
            });
            
            module.DefineLibraryMethod("parse_file", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.ParseFile),
            });
            
            module.DefineLibraryMethod("quick_emit", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object[], System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.QuickEmit),
            });
            
            module.DefineLibraryMethod("quick_emit_node", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object[], System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.QuickEmitNode),
            });
            
            module.DefineLibraryMethod("tag_class", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Builtins.RubyModule, System.Object, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.TagClass),
            });
            
            module.DefineLibraryMethod("tagged_classes", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Builtins.RubyModule, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.GetTaggedClasses),
            });
            
            module.DefineLibraryMethod("tagurize", 0x21, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Tagurize),
            });
            
        }
        
        private void LoadYAML__Stream_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            
            module.DefineLibraryMethod("[]", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.YamlStream, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStreamOps.GetDocument),
            });
            
            module.DefineLibraryMethod("add", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.YamlStream, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStreamOps.Add),
            });
            
            module.DefineLibraryMethod("documents", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.YamlStream, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStreamOps.GetDocuments),
            });
            
            module.DefineLibraryMethod("documents=", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.YamlStream, IronRuby.Builtins.RubyArray, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStreamOps.SetDocuments),
            });
            
            module.DefineLibraryMethod("edit", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.YamlStream, System.Object, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStreamOps.EditDocument),
            });
            
            module.DefineLibraryMethod("emit", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.YamlStream, IronRuby.Builtins.RubyIO, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStreamOps.Emit),
            });
            
            module.DefineLibraryMethod("inspect", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.YamlStream, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStreamOps.Inspect),
            });
            
            module.DefineLibraryMethod("options", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.YamlStream, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStreamOps.GetOptions),
            });
            
            module.DefineLibraryMethod("options=", 0x11, new System.Delegate[] {
                new System.Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.YamlStream, IronRuby.Builtins.Hash, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStreamOps.SetOptions),
            });
            
        }
        
    }
}

