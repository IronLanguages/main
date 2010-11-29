/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#pragma warning disable 169 // mcs: unused private method
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.Yaml.YamlLibraryInitializer))]

namespace IronRuby.StandardLibrary.Yaml {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class YamlLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            IronRuby.Builtins.RubyClass classRef0 = GetClass(typeof(IronRuby.Builtins.RubyObject));
            
            
            ExtendModule(typeof(IronRuby.Builtins.FalseClass), 0x00000000, LoadIronRuby__Builtins__FalseClass_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(IronRuby.Builtins.Hash), 0x00000000, null, LoadIronRuby__Builtins__Hash_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.MutableString), 0x00000000, LoadIronRuby__Builtins__MutableString_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.Range), 0x00000000, LoadIronRuby__Builtins__Range_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.RubyArray), 0x00000000, LoadIronRuby__Builtins__RubyArray_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(IronRuby.Builtins.RubyClass), 0x00000000, null, LoadIronRuby__Builtins__RubyClass_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(IronRuby.Builtins.RubyModule), 0x00000000, null, LoadIronRuby__Builtins__RubyModule_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.RubyRegex), 0x00000000, LoadIronRuby__Builtins__RubyRegex_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.RubyStruct), 0x00000000, LoadIronRuby__Builtins__RubyStruct_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.RubySymbol), 0x00000000, LoadIronRuby__Builtins__RubySymbol_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.RubyTime), 0x00000000, LoadIronRuby__Builtins__RubyTime_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(IronRuby.Builtins.TrueClass), 0x00000000, LoadIronRuby__Builtins__TrueClass_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(Microsoft.Scripting.Math.BigInteger), 0x00000000, LoadMicrosoft__Scripting__Math__BigInteger_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(Microsoft.Scripting.Runtime.DynamicNull), 0x00000000, LoadMicrosoft__Scripting__Runtime__DynamicNull_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(System.Double), 0x00000000, LoadSystem__Double_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(System.Exception), 0x00000000, LoadSystem__Exception_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendModule(typeof(System.Int32), 0x00000000, LoadSystem__Int32_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(System.Object), 0x00000000, null, LoadSystem__Object_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def5 = DefineGlobalModule("YAML", typeof(IronRuby.StandardLibrary.Yaml.RubyYaml), 0x00000008, null, LoadYAML_Class, LoadYAML_Constants, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def6 = DefineModule("YAML::BaseNode", typeof(IronRuby.StandardLibrary.Yaml.RubyYaml.BaseNode), 0x00000008, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def8 = DefineClass("YAML::Stream", typeof(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream), 0x00000008, classRef0, LoadYAML__Stream_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.Hash, IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.CreateStream)
            );
            IronRuby.Builtins.RubyModule def1 = DefineModule("YAML::Syck", typeof(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck), 0x00000008, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalClass("Date", typeof(IronRuby.StandardLibrary.Yaml.YamlDateOps), 0x00000018, Context.ObjectClass, LoadDate_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def4 = DefineClass("YAML::Syck::Emitter", typeof(IronRuby.StandardLibrary.Yaml.RubyRepresenter), 0x00000008, Context.ObjectClass, LoadYAML__Syck__Emitter_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def3 = DefineClass("YAML::Syck::Node", typeof(IronRuby.StandardLibrary.Yaml.Node), 0x00000008, Context.ObjectClass, LoadYAML__Syck__Node_Instance, null, null, new IronRuby.Builtins.RubyModule[] {def6});
            IronRuby.Builtins.RubyClass def7 = DefineClass("YAML::Syck::Out", typeof(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.Out), 0x00000008, Context.ObjectClass, LoadYAML__Syck__Out_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def2 = DefineClass("YAML::Syck::Map", typeof(IronRuby.StandardLibrary.Yaml.MappingNode), 0x00000008, def3, LoadYAML__Syck__Map_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def9 = DefineClass("YAML::Syck::Scalar", typeof(IronRuby.StandardLibrary.Yaml.ScalarNode), 0x00000008, def3, LoadYAML__Syck__Scalar_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def10 = DefineClass("YAML::Syck::Seq", typeof(IronRuby.StandardLibrary.Yaml.SequenceNode), 0x00000008, def3, LoadYAML__Syck__Seq_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            SetConstant(def5, "BaseNode", def6);
            SetConstant(def5, "Stream", def8);
            SetConstant(def5, "Syck", def1);
            SetConstant(def1, "Emitter", def4);
            SetConstant(def1, "Node", def3);
            SetConstant(def1, "Out", def7);
            SetConstant(def1, "Map", def2);
            SetConstant(def1, "Scalar", def9);
            SetConstant(def1, "Seq", def10);
        }
        
        private static void LoadDate_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlDateOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000004U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlDateOps.ToYaml)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__FalseClass_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<System.Boolean, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlFalseOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000004U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Boolean, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlFalseOps.ToYaml)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__Hash_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlHashOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000002U, 
                new Func<IronRuby.Builtins.Hash, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlHashOps.ToYamlNode)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__MutableString_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "is_complex_yaml?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.MutableString>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.RubyArray>>, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.StandardLibrary.Yaml.YamlMutableStringOps.IsComplexYaml)
            );
            
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlMutableStringOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000002U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlMutableStringOps.ToYamlNode)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__Range_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.Range, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlRangeOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000010U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Builtins.Range, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlRangeOps.ToYaml)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__RubyArray_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyArray, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlArrayOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyArray, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlArrayOps.ToYamlNode)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__RubyClass_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlClassOps.ToYamlNode)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__RubyModule_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "yaml_as", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.YamlModuleOps.YamlAs)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__RubyRegex_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlRegexpOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000002U, 
                new Func<IronRuby.Builtins.RubyRegex, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlRegexpOps.ToYaml)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__RubyStruct_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlStructOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000002U, 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlStructOps.ToYamlNode)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__RubySymbol_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlSymbolOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000002U, 
                new Func<IronRuby.Builtins.RubySymbol, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlSymbolOps.ToYaml)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__RubyTime_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyTime, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlTimeOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000002U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlTimeOps.ToYaml)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__TrueClass_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<System.Boolean, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlTrueOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000004U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Boolean, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlTrueOps.ToYaml)
            );
            
        }
        
        private static void LoadMicrosoft__Scripting__Math__BigInteger_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000001U, 
                new Func<Microsoft.Scripting.Math.BigInteger, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlBigIntegerOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000003U, 
                new Func<Microsoft.Scripting.Math.BigInteger, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlBigIntegerOps.ToYaml)
            );
            
        }
        
        private static void LoadMicrosoft__Scripting__Runtime__DynamicNull_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlNilOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000002U, 
                new Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlNilOps.ToYaml)
            );
            
        }
        
        private static void LoadSystem__Double_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<System.Double, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlDoubleOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000002U, 
                new Func<System.Double, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlDoubleOps.ToYaml)
            );
            
        }
        
        private static void LoadSystem__Exception_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlExceptionOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000004U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, System.Exception, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlExceptionOps.ToYamlNode)
            );
            
        }
        
        private static void LoadSystem__Int32_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<System.Int32, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlFixnumOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000002U, 
                new Func<System.Int32, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.Node>(IronRuby.StandardLibrary.Yaml.YamlFixnumOps.ToYaml)
            );
            
        }
        
        private static void LoadSystem__Object_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "taguri", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.TagUri)
            );
            
            DefineLibraryMethod(module, "to_yaml", 0x11, 
                0x00000002U, 0x00000000U, 
                new Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, System.Object>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.ToYaml), 
                new Func<IronRuby.StandardLibrary.Yaml.YamlCallSiteStorage, System.Object, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.ToYaml)
            );
            
            DefineLibraryMethod(module, "to_yaml_node", 0x12, 
                0x00000002U, 
                new Func<System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, System.Object>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.ToYamlProperties)
            );
            
            DefineLibraryMethod(module, "to_yaml_properties", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.ToYamlProperties)
            );
            
            DefineLibraryMethod(module, "to_yaml_style", 0x11, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.YamlObjectOps.ToYamlStyle)
            );
            
        }
        
        private static void LoadYAML_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetConstant(module, "Emitter", IronRuby.StandardLibrary.Yaml.RubyYaml.Emitter(module));
            
        }
        
        private static void LoadYAML_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "add_builtin_type", 0x21, 
                0x00020005U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.AddBuiltinType)
            );
            
            DefineLibraryMethod(module, "add_domain_type", 0x21, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.AddDomainType), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.AddDomainType)
            );
            
            DefineLibraryMethod(module, "dump", 0x21, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.YamlCallSiteStorage, IronRuby.Builtins.RubyModule, System.Object, IronRuby.Builtins.RubyIO, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Dump)
            );
            
            DefineLibraryMethod(module, "dump_all", 0x21, 
                0x00000004U, 
                new Func<IronRuby.StandardLibrary.Yaml.YamlCallSiteStorage, IronRuby.Builtins.RubyModule, System.Collections.IList, IronRuby.Builtins.RubyIO, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.DumpAll)
            );
            
            DefineLibraryMethod(module, "dump_stream", 0x21, 
                0x80000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object[], System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.DumpStream)
            );
            
            DefineLibraryMethod(module, "each_document", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.EachDocument)
            );
            
            DefineLibraryMethod(module, "each_node", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.ParseDocuments)
            );
            
            DefineLibraryMethod(module, "emitter", 0x21, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.YamlCallSiteStorage, IronRuby.Builtins.RubyModule, IronRuby.StandardLibrary.Yaml.RubyRepresenter>(IronRuby.StandardLibrary.Yaml.RubyYaml.CreateEmitter)
            );
            
            DefineLibraryMethod(module, "load", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Load)
            );
            
            DefineLibraryMethod(module, "load_documents", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.EachDocument)
            );
            
            DefineLibraryMethod(module, "load_file", 0x21, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.LoadFile)
            );
            
            DefineLibraryMethod(module, "load_stream", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.LoadStream)
            );
            
            DefineLibraryMethod(module, "parse", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Parse)
            );
            
            DefineLibraryMethod(module, "parse_documents", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.ParseDocuments)
            );
            
            DefineLibraryMethod(module, "parse_file", 0x21, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.ParseFile)
            );
            
            DefineLibraryMethod(module, "quick_emit", 0x21, 
                0x00000012U, 0x00000012U, 
                new Func<IronRuby.StandardLibrary.Yaml.YamlCallSiteStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, IronRuby.Builtins.Hash, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.QuickEmit), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, IronRuby.StandardLibrary.Yaml.RubyRepresenter, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.QuickEmit)
            );
            
            DefineLibraryMethod(module, "quick_emit_node", 0x21, 
                0x80000000U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object, System.Object[], System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.QuickEmitNode)
            );
            
            DefineLibraryMethod(module, "tag_class", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.TagClass)
            );
            
            DefineLibraryMethod(module, "tagged_classes", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.GetTaggedClasses)
            );
            
            DefineLibraryMethod(module, "tagurize", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Tagurize)
            );
            
        }
        
        private static void LoadYAML__Stream_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "[]", 0x11, 
                0x00020000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, System.Int32, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.GetDocument)
            );
            
            DefineLibraryMethod(module, "add", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.Add)
            );
            
            DefineLibraryMethod(module, "documents", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.GetDocuments)
            );
            
            DefineLibraryMethod(module, "documents=", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.SetDocuments)
            );
            
            DefineLibraryMethod(module, "edit", 0x11, 
                0x00010000U, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, System.Int32, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.EditDocument)
            );
            
            DefineLibraryMethod(module, "emit", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.YamlCallSiteStorage, IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, IronRuby.Builtins.RubyIO, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.Emit)
            );
            
            DefineLibraryMethod(module, "inspect", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.Inspect)
            );
            
            DefineLibraryMethod(module, "options", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, IronRuby.Builtins.Hash>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.GetOptions)
            );
            
            DefineLibraryMethod(module, "options=", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream, IronRuby.Builtins.Hash, IronRuby.Builtins.Hash>(IronRuby.StandardLibrary.Yaml.RubyYaml.YamlStream.SetOptions)
            );
            
        }
        
        private static void LoadYAML__Syck__Emitter_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "level", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyRepresenter, System.Int32>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.RepresenterOps.GetLevel)
            );
            
            DefineLibraryMethod(module, "level=", 0x11, 
                0x00010000U, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyRepresenter, System.Int32, System.Int32>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.RepresenterOps.SetLevel)
            );
            
        }
        
        private static void LoadYAML__Syck__Map_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "add", 0x11, 
                0x00000000U, 
                new Action<IronRuby.StandardLibrary.Yaml.YamlCallSiteStorage, IronRuby.StandardLibrary.Yaml.MappingNode, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.MapOps.Add)
            );
            
            DefineLibraryMethod(module, "style=", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.MappingNode, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.MapOps.SetStyle)
            );
            
            DefineLibraryMethod(module, "value", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.MappingNode, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.MapOps.GetValue)
            );
            
        }
        
        private static void LoadYAML__Syck__Node_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "transform", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.StandardLibrary.Yaml.Node, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.NodeOps.Transform)
            );
            
        }
        
        private static void LoadYAML__Syck__Out_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "emitter", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.Out, IronRuby.StandardLibrary.Yaml.RubyRepresenter>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.Out.GetEmitter)
            );
            
            DefineLibraryMethod(module, "emitter=", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.Out, IronRuby.StandardLibrary.Yaml.RubyRepresenter, IronRuby.StandardLibrary.Yaml.RubyRepresenter>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.Out.SetEmitter)
            );
            
            DefineLibraryMethod(module, "map", 0x11, 
                0x00020001U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.Out, IronRuby.Builtins.MutableString, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.Out.CreateMap)
            );
            
            DefineLibraryMethod(module, "seq", 0x11, 
                0x00020001U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.Out, IronRuby.Builtins.MutableString, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.Out.CreateSequence)
            );
            
        }
        
        private static void LoadYAML__Syck__Scalar_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "style=", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Yaml.ScalarNode, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.ScalarOps.SetStyle)
            );
            
            DefineLibraryMethod(module, "value", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.ScalarNode, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.ScalarOps.GetValue)
            );
            
        }
        
        private static void LoadYAML__Syck__Seq_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "add", 0x11, 
                0x00000000U, 
                new Action<IronRuby.StandardLibrary.Yaml.YamlCallSiteStorage, IronRuby.StandardLibrary.Yaml.SequenceNode, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.SeqOps.Add)
            );
            
            DefineLibraryMethod(module, "style=", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.SequenceNode, System.Object, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.SeqOps.SetStyle)
            );
            
            DefineLibraryMethod(module, "value", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Yaml.MappingNode, System.Object>(IronRuby.StandardLibrary.Yaml.RubyYaml.Syck.SeqOps.GetValue)
            );
            
        }
        
    }
}

