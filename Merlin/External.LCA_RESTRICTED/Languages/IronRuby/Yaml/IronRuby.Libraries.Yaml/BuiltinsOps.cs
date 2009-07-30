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

using System;
using System.Runtime.CompilerServices;
using System.Dynamic;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Generation;
using System.Collections.Generic;

namespace IronRuby.StandardLibrary.Yaml {

    [RubyClass(Extends = typeof(object))]
    public static class YamlObjectOps {        

        [RubyMethod("to_yaml_properties")]
        public static RubyArray/*!*/ ToYamlProperties(
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            object self) {
            return ArrayOps.SortInPlace(comparisonStorage, lessThanStorage, greaterThanStorage,
                KernelOps.InstanceVariables(comparisonStorage.Context, self)
            );
        }

        [RubyMethod("to_yaml_style")]
        public static object ToYamlStyle(object self) {
            return null;
        }

        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static object ToYamlProperties(object self, [NotNull]RubyRepresenter/*!*/ rep) {
            var map = new Dictionary<MutableString, object>();
            rep.AddYamlProperties(self, map);
            return rep.Map(self, map);
        }

        [RubyMethod("to_yaml")]
        public static object ToYaml(RubyContext/*!*/ context, object self, params object[] args) {
            return RubyYaml.DumpAll(context, new object[] { self }, null);
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri(RubyContext/*!*/ context, object self) {
            MutableString str = MutableString.Create("!ruby/object:");
            str.Append(context.GetClassName(self));
            str.Append(' ');
            return str;
        }
    }

    [RubyClass(Extends = typeof(RubyClass))]
    public static class YamlClassOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node ToYamlNode(RubyContext/*!*/ context, object self, RubyRepresenter rep) {
            throw RubyExceptions.CreateTypeError("can't dump anonymous class " + context.GetClassDisplayName(self));
        }
    }

    [RubyClass(Extends = typeof(RubyModule))]
    public static class YamlModuleOps {
        [RubyMethod("yaml_as")]
        public static object YamlAs(RubyScope/*!*/ scope, RubyModule/*!*/ self, object tag) {
            RubyModule yamlModule;
            scope.RubyContext.TryGetModule(scope.GlobalScope, "YAML", out yamlModule);
            return RubyYaml.TagClass(yamlModule, tag, self);
        }
    }

    [RubyClass(Extends = typeof(Hash))]
    public static class YamlHashOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node ToYamlNode(Hash/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Map(self, self);
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri(RubyContext/*!*/ context, object self) {
            return MutableString.Create("tag:yaml.org,2002:map");
        }
    }

    [RubyModule(Extends = typeof(RubyArray))]
    public static class YamlArrayOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node ToYamlNode(RubyContext/*!*/ context, RubyArray/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Sequence(self, self);
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri(RubyContext/*!*/ context, object self) {
            return MutableString.Create("tag:yaml.org,2002:seq");
        }
    }

    [RubyModule(Extends = typeof(RubyStruct))]
    public static class YamlStructOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node ToYamlNode(RubyStruct/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            var fieldNames = self.GetNames();
            var map = new Dictionary<MutableString, object>(fieldNames.Count);
            for (int i = 0; i < fieldNames.Count; i++) {
                map[MutableString.Create(fieldNames[i])] = self.GetValue(i);
            }
            rep.AddYamlProperties(self, map);
            return rep.Map(self, map);
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri(RubyStruct/*!*/ self) {
            MutableString str = MutableString.Create("tag:ruby.yaml.org,2002:struct:");
            string name = self.ImmediateClass.GetNonSingletonClass().Name;
            if (name != null) {
                string structPrefix = "Struct::";
                if (name.StartsWith(structPrefix)) {
                    name = name.Substring(structPrefix.Length);
                }
            }
            return str.Append(name);
        }
    }

    [RubyModule(Extends = typeof(Exception))]
    public static class YamlExceptionOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node ToYamlNode(CallSiteStorage<Func<CallSite, Exception, object>>/*!*/ messageStorage,
            Exception/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {

            var site = messageStorage.GetCallSite("message", 0);
            var map = new Dictionary<MutableString, object>() {
                { MutableString.Create("message"), site.Target(site, self) }
            };
            
            rep.AddYamlProperties(self, map);
            return rep.Map(self, map);
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri(RubyContext/*!*/ context, object self) {
            MutableString str = MutableString.Create("!ruby/exception:");
            str.Append(context.GetClassName(self));
            return str;
        }
    }

    [RubyModule(Extends = typeof(MutableString))]
    public static class YamlStringOps {
        [RubyMethod("is_complex_yaml?")]
        public static bool IsComplexYaml(
            CallSiteStorage<Func<CallSite, object, MutableString>>/*!*/ toYamlStyleStorage,
            CallSiteStorage<Func<CallSite, object, RubyArray>>/*!*/ toYamlPropertiesStorage,
            MutableString/*!*/ self) {

            var toYamlStyleSite = toYamlStyleStorage.GetCallSite("to_yaml_style", 0);
            var toYamlPropertiesSite = toYamlPropertiesStorage.GetCallSite("to_yaml_properties", 0);

            return RubyOps.IsTrue(toYamlStyleSite.Target(toYamlStyleSite, self)) ||
                   toYamlPropertiesSite.Target(toYamlPropertiesSite, self).Count == 0 ||
                   AfterNewLine(self.ConvertToString());
        }

        // True if has a newline & something is after it
        private static bool AfterNewLine(string str) {
            int i = str.IndexOf('\n');
            return i >= 0 && i < str.Length - 1;
        }

        [RubyMethod("is_binary_data?")]
        public static object IsBinaryData(UnaryOpStorage/*!*/ isEmptyStorage, MutableString/*!*/ self) {

            var site = isEmptyStorage.GetCallSite("empty?");
            if (RubyOps.IsTrue(site.Target(site, self))) {
                return null;
            }

            return ScriptingRuntimeHelpers.BooleanToObject((self.IsBinary ? self.IndexOf(0) : self.IndexOf('\0')) >= 0);
        }

        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYamlNode(UnaryOpStorage/*!*/ isBinaryDataStorage, MutableString/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {

            var site = isBinaryDataStorage.GetCallSite("is_binary_data?");
            if (RubyOps.IsTrue(site.Target(site, self))) {
                return rep.BaseCreateNode(self.ConvertToBytes());
            }

            string str = self.ConvertToString();
            RubyArray props = rep.ToYamlProperties(self);
            if (props.Count == 0) {
                MutableString taguri = rep.GetTagUri(self);

                char style = '\0';
                if (str.StartsWith(":")) {
                    style = '"';
                } else {
                    MutableString styleStr = rep.ToYamlStyle(self);
                    if (styleStr != null && styleStr.Length > 0) {
                        style = styleStr.GetChar(0);
                    }
                }

                return rep.Scalar(taguri != null ? taguri.ConvertToString() : "", str, style);
            }

            var map = new Dictionary<MutableString, object>() {
                { MutableString.Create("str"), str }
            };
            rep.AddYamlProperties(self, map, props);
            return rep.Map(self, map);
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(object self) {
            return MutableString.Create("tag:yaml.org,2002:str");
        }
    }

    [RubyModule(Extends = typeof(Integer))]
    public static class YamlIntegerOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(ConversionStorage<MutableString>/*!*/ tosConversion, object self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(self, Protocols.ConvertToString(tosConversion, self));
        }          

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(object self) {
            return MutableString.Create("tag:yaml.org,2002:int");
        }
    }

    [RubyModule(Extends = typeof(BigInteger))]
    public static class YamlBigIntegerOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(ConversionStorage<MutableString>/*!*/ tosConversion, [NotNull]BigInteger self, [NotNull]RubyRepresenter/*!*/ rep) {
            return YamlIntegerOps.ToYaml(tosConversion, self, rep);
        } 

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri([NotNull]BigInteger self) {     
            return MutableString.Create("tag:yaml.org,2002:int:Bignum");            
        }
    }

    [RubyModule(Extends = typeof(double))]
    public static class YamlDoubleOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(ConversionStorage<MutableString>/*!*/ tosConversion, double self, [NotNull]RubyRepresenter/*!*/ rep) {
            MutableString str = Protocols.ConvertToString(tosConversion, self);
            if (str != null) {
                if (str.Equals("Infinity")) {
                    str = MutableString.Create(".Inf");
                } else if (str.Equals("-Infinity")) {
                    str = MutableString.Create("-.Inf");
                } else if (str.Equals("NaN")) {
                    str = MutableString.Create(".NaN");
                }
            }
            return rep.Scalar(self, str);
        }    

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(double self) {
            return MutableString.Create("tag:yaml.org,2002:float");
        }
    }

    [RubyModule(Extends = typeof(Range))]
    public static class YamlRangeOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(UnaryOpStorage/*!*/ beginStorage, UnaryOpStorage/*!*/ endStorage, UnaryOpStorage/*!*/ exclStorage, 
            Range/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {

            var begin = beginStorage.GetCallSite("begin");
            var end = endStorage.GetCallSite("end");

            var map = new Dictionary<MutableString, object>() {
                { MutableString.Create("begin"), begin.Target(begin, self) },
                { MutableString.Create("end"), end.Target(end, self) },
                { MutableString.Create("excl"), self.ExcludeEnd },
            };

            rep.AddYamlProperties(self, map);
            return rep.Map(self, map);
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri([NotNull]Range self) {
            return MutableString.Create("tag:ruby.yaml.org,2002:range");
        }
    }

    [RubyModule(Extends = typeof(RubyRegex))]
    public static class YamlRegexpOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(RubyRegex/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(self, rep.Context.Inspect(self));
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(RubyRegex/*!*/ self) {
            return MutableString.Create("tag:ruby.yaml.org,2002:regexp");
        }
    }

    [RubyModule(Extends = typeof(DateTime))]
    public static class DateTimeOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(DateTime self, [NotNull]RubyRepresenter/*!*/ rep) {
            string format = (self.Millisecond != 0) ? "yyyy-MM-dd HH:mm:ss.fffffff K" : "yyyy-MM-dd HH:mm:ss K";
            return rep.Scalar(self, MutableString.Create(self.ToString(format)));
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(DateTime self) {
            return MutableString.Create("tag:yaml.org,2002:timestamp");
        }
    }

    [RubyModule(Extends = typeof(SymbolId))]
    public static class YamlSymbolOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(object self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(self, rep.Context.Inspect(self));
        }
        
        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(object self) {
            return MutableString.Create("tag:yaml.org,2002:str");
        }
    }

    [RubyModule(Extends = typeof(TrueClass))]
    public static class YamlTrueOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(ConversionStorage<MutableString>/*!*/ tosConversion, object self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(self, Protocols.ConvertToString(tosConversion, self));
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(object self) {
            return MutableString.Create("tag:yaml.org,2002:bool");
        }
    }

    [RubyModule(Extends = typeof(FalseClass))]
    public static class YamlFalseOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(ConversionStorage<MutableString>/*!*/ tosConversion, object self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(self, Protocols.ConvertToString(tosConversion, self));
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(object self) {
            return MutableString.Create("tag:yaml.org,2002:bool");
        }
    }

    [RubyModule(Extends = typeof(DynamicNull))]
    public static class YamlNilOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(object self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(self, null);
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(object self) {
            return MutableString.Create("tag:yaml.org,2002:null");
        }
    }

    [RubyClass(Extends = typeof(Node))]
    public static class YamlNodeOps {
        [RubyMethod("transform")]
        public static object Transform(RubyScope/*!*/ scope, Node/*!*/ self) {
            return new RubyConstructor(scope.GlobalScope, new SimpleNodeProvider(self)).GetData();
        }
    }
}
