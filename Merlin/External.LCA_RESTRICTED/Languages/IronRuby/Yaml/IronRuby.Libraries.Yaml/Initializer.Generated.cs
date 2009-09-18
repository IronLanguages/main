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
    using System;
    using Microsoft.Scripting.Utils;
    
    public sealed class YamlLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            IronRuby.Builtins.RubyClass classRef0 = GetClass(typeof(IronRuby.StandardLibrary.Yaml.Representer));
            IronRuby.Builtins.RubyClass classRef1 = GetClass(typeof(IronRuby.Builtins.RubyObject));
            
            
            ExtendModule(typeof(IronRuby.Builtins.FalseClass), LoadIronRuby__Builtins__FalseClass_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(IronRuby.Builtins.Hash), null, LoadIronRuby__Builtins__Hash_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.Integer), LoadIronRuby__Builtins__Integer_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.MutableString), LoadIronRuby__Builtins__MutableString_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.Range), LoadIronRuby__Builtins__Range_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.RubyArray), LoadIronRuby__Builtins__RubyArray_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(IronRuby.Builtins.RubyClass), null, LoadIronRuby__Builtins__RubyClass_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(IronRuby.Builtins.RubyModule), null, LoadIronRuby__Builtins__RubyModule_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.RubyRegex), LoadIronRuby__Builtins__RubyRegex_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.RubyStruct), LoadIronRuby__Builtins__RubyStruct_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.TrueClass), LoadIronRuby__Builtins__TrueClass_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(IronRuby.StandardLibrary.Yaml.Node), null, LoadIronRuby__StandardLibrary__Yaml__Node_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(Microsoft.Scripting.Math.BigInteger), LoadMicrosoft__Scripting__Math__BigInteger_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(Microsoft.Scripting.Runtime.DynamicNull), LoadMicrosoft__Scripting__Runtime__DynamicNull_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(Microsoft.Scripting.SymbolId), LoadMicrosoft__Scripting__SymbolId_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalClass("Out", typeof(IronRuby.StandardLibrary.Yaml.RubyRepresenter), 0x00000100, classRef0, LoadOut_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(System.DateTime), LoadSystem__DateTime_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(System.Double), LoadSystem__Double_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(System.Exception), LoadSystem__Exception_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(System.Object), null, LoadSystem__Object_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def1 = DefineGlobalModule("YAML", typeof(IronRuby.StandardLibrary.Yaml.RubyYaml), 0x00000100, null, LoadYAML_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def2 = DefineModule("YAML::BaseNode", typeof(IronRuby.StandardLibrary.Yaml.RubyYaml.BaseNode), 0x00000100, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def3 = DefineClass("YAML::Stream", typeof(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream), 0x00000100, classRef1, LoadYAML__Stream_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.Hash, IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.CreateStream)
            );
            def1.SetConstant("BaseNode", def2);
            def1.SetConstant("Stream", def3);
        }
        
        private static void LoadIronRuby__Builtins__FalseClass_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlFalseOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlFalseOps.ToYaml)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__Hash_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlHashOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<IronRuby.Builtins.Hash, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlHashOps.ToYamlNode)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__Integer_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlIntegerOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlIntegerOps.ToYaml)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__MutableString_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("is_binary_data?", 0x11, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Builtins.MutableString, System.Object>(IronRuby.StandardLibrary.Yaml.YamlStringOps.IsBinaryData)
            );
            
            module.DefineLibraryMethod("is_complex_yaml?", 0x11, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.MutableString>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.RubyArray>>, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.StandardLibrary.Yaml.YamlStringOps.IsComplexYaml)
            );
            
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlStringOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlStringOps.ToYamlNode)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__Range_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<IronRuby.Builtins.Range, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlRangeOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Builtins.Range, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlRangeOps.ToYaml)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__RubyArray_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlArrayOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyArray, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlArrayOps.ToYamlNode)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__RubyClass_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlClassOps.ToYamlNode)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__RubyModule_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("yaml_as", 0x11, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.YamlModuleOps.YamlAs)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__RubyRegex_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlRegexpOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<IronRuby.Builtins.RubyRegex, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlRegexpOps.ToYaml)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__RubyStruct_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlStructOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlStructOps.ToYamlNode)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__TrueClass_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlTrueOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlTrueOps.ToYaml)
            );
            
        }
        
        private static void LoadIronRuby__StandardLibrary__Yaml__Node_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("transform", 0x11, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.StandardLibrary.Yaml.Node, System.Object>(IronRuby.StandardLibrary.Yaml.YamlNodeOps.Transform)
            );
            
        }
        
        private static void LoadMicrosoft__Scripting__Math__BigInteger_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<Microsoft.Scripting.Math.BigInteger, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlBigIntegerOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, Microsoft.Scripting.Math.BigInteger, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlBigIntegerOps.ToYaml)
            );
            
        }
        
        private static void LoadMicrosoft__Scripting__Runtime__DynamicNull_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlNilOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlNilOps.ToYaml)
            );
            
        }
        
        private static void LoadMicrosoft__Scripting__SymbolId_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlSymbolOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlSymbolOps.ToYaml)
            );
            
        }
        
        private static void LoadOut_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("map", 0x11, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.Builtins.MutableString, System.Object, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.RubyRepresenter.Map)
            );
            
        }
        
        private static void LoadSystem__DateTime_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<System.DateTime, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.DateTimeOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<System.DateTime, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.DateTimeOps.ToYaml)
            );
            
        }
        
        private static void LoadSystem__Double_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<System.Double, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlDoubleOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Double, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlDoubleOps.ToYaml)
            );
            
        }
        
        private static void LoadSystem__Exception_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlExceptionOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Exception, System.Object>>, System.Exception, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlExceptionOps.ToYamlNode)
            );
            
        }
        
        private static void LoadSystem__Object_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("taguri", 0x11, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.TagUri)
            );
            
            module.DefineLibraryMethod("to_yaml", 0x11, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object[], System.Object>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.ToYaml)
            );
            
            module.DefineLibraryMethod("to_yaml_node", 0x12, 
                new Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, System.Object>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.ToYamlProperties)
            );
            
            module.DefineLibraryMethod("to_yaml_properties", 0x11, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.ToYamlProperties)
            );
            
            module.DefineLibraryMethod("to_yaml_style", 0x11, 
                new Func<System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.ToYamlStyle)
            );
            
        }
        
        private static void LoadYAML_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("add_domain_type", 0x21, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.AddDomainType), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.AddDomainType)
            );
            
            module.DefineLibraryMethod("dump", 0x21, 
                new Func<IronRuby.Builtins.RubyModule, System.Object, IronRuby.Builtins.RubyIO, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Dump)
            );
            
            module.DefineLibraryMethod("dump_all", 0x21, 
                new Func<IronRuby.Builtins.RubyModule, System.Collections.IEnumerable, IronRuby.Builtins.RubyIO, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.DumpAll)
            );
            
            module.DefineLibraryMethod("dump_stream", 0x21, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object[], System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.DumpStream)
            );
            
            module.DefineLibraryMethod("each_document", 0x21, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.EachDocument)
            );
            
            module.DefineLibraryMethod("each_node", 0x21, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.ParseDocuments)
            );
            
            module.DefineLibraryMethod("load", 0x21, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Load)
            );
            
            module.DefineLibraryMethod("load_documents", 0x21, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.EachDocument)
            );
            
            module.DefineLibraryMethod("load_file", 0x21, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.LoadFile)
            );
            
            module.DefineLibraryMethod("load_stream", 0x21, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.LoadStream)
            );
            
            module.DefineLibraryMethod("parse", 0x21, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Parse)
            );
            
            module.DefineLibraryMethod("parse_documents", 0x21, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.ParseDocuments)
            );
            
            module.DefineLibraryMethod("parse_file", 0x21, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.ParseFile)
            );
            
            module.DefineLibraryMethod("quick_emit", 0x21, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, IronRuby.Builtins.Hash, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.QuickEmit), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.RubyYaml.QuickEmit)
            );
            
            module.DefineLibraryMethod("quick_emit_node", 0x21, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object[], System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.QuickEmitNode)
            );
            
            module.DefineLibraryMethod("tag_class", 0x21, 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.TagClass)
            );
            
            module.DefineLibraryMethod("tagged_classes", 0x21, 
                new Func<IronRuby.Builtins.RubyModule, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.GetTaggedClasses)
            );
            
            module.DefineLibraryMethod("tagurize", 0x21, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Tagurize)
            );
            
        }
        
        private static void LoadYAML__Stream_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.DefineLibraryMethod("[]", 0x11, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, System.Int32, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.GetDocument)
            );
            
            module.DefineLibraryMethod("add", 0x11, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.Add)
            );
            
            module.DefineLibraryMethod("documents", 0x11, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.GetDocuments)
            );
            
            module.DefineLibraryMethod("documents=", 0x11, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.SetDocuments)
            );
            
            module.DefineLibraryMethod("edit", 0x11, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, System.Int32, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.EditDocument)
            );
            
            module.DefineLibraryMethod("emit", 0x11, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, IronRuby.Builtins.RubyIO, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.Emit)
            );
            
            module.DefineLibraryMethod("inspect", 0x11, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.Inspect)
            );
            
            module.DefineLibraryMethod("options", 0x11, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, IronRuby.Builtins.Hash>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.GetOptions)
            );
            
            module.DefineLibraryMethod("options=", 0x11, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, IronRuby.Builtins.Hash, IronRuby.Builtins.Hash>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.SetOptions)
            );
            
        }
        
    }
}

