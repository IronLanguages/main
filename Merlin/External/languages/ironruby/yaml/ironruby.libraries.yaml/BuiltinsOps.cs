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

namespace IronRuby.StandardLibrary.Yaml {

    [RubyClass(Extends = typeof(object))]
    public static class YamlObjectOps {        

        [RubyMethod("to_yaml_properties")]
        public static RubyArray/*!*/ ToYamlProperties(
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            RubyContext/*!*/ context, object self) {
            return ArrayOps.SortInPlace(comparisonStorage, lessThanStorage, greaterThanStorage, context, 
                null, KernelOps.InstanceVariables(context, self)
            );
        }

        [RubyMethod("to_yaml_style")]
        public static object ToYamlStyle(object self) {
            return null;
        }

        [RubyMethod("to_yaml_node")]
        public static object ToYamlProperties(object self, [NotNull]RubyRepresenter/*!*/ rep) {
            Hash map = new Hash(rep.Context);
            RubyRepresenter.AddYamlProperties(rep.Context, self, map);
            return rep.Map(self, map);
        }

        [RubyMethod("to_yaml")]
        public static object ToYaml(RubyContext/*!*/ context, object self, params object[] args) {
            return RubyYaml.DumpAll(context, new object[] { self }, null);
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri(RubyContext/*!*/ context, object self) {
            MutableString str = MutableString.Create("!ruby/object:");
            str.Append(RubyUtils.GetClassName(context, self));
            return str;
        }
    }

    [RubyClass(Extends = typeof(RubyClass))]
    public static class YamlClassOps {
        [RubyMethod("to_yaml_node")]
        public static Node ToYamlNode(RubyContext/*!*/ context, object self, RubyRepresenter rep) {
            throw RubyExceptions.CreateTypeError("can't dump anonymous class " + RubyUtils.GetClassName(context, self));
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
        [RubyMethod("to_yaml_node")]
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
        [RubyMethod("to_yaml_node")]
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
        private static readonly CallSite<Func<CallSite, RubyContext, RubyStruct, RubyArray>> _Members = CallSite<Func<CallSite, RubyContext, RubyStruct, RubyArray>>.Create(LibrarySites.InstanceCallAction("members"));
        private static readonly CallSite<Func<CallSite, RubyContext, RubyStruct, RubyArray>> _Values = CallSite<Func<CallSite, RubyContext, RubyStruct, RubyArray>>.Create(LibrarySites.InstanceCallAction("values"));

        [RubyMethod("to_yaml_node")]
        public static Node ToYamlNode(RubyStruct/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            RubyContext context = self.Class.Context;
            RubyArray members = _Members.Target(_Members, context, self);
            RubyArray values = _Values.Target(_Values, context, self);

            if (members.Count != values.Count) {
                throw new ArgumentException("Struct values and members returned arrays of different lengths");
            }

            Hash map = new Hash(self.Class.Context);
            for (int i = 0; i < members.Count; i++) {
                IDictionaryOps.SetElement(context, map, members[i], values[i]);
            }
            RubyRepresenter.AddYamlProperties(context, self, map);
            return rep.Map(self, map);
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri(RubyStruct/*!*/ self) {
            MutableString str = MutableString.Create("tag:ruby.yaml.org,2002:struct:");
            string name = self.Class.Name;
            string structPrefix = "Struct::";
            if (name.StartsWith(structPrefix)) {
                name = name.Substring(structPrefix.Length);
            }
            return str.Append(name);
        }
    }

    [RubyModule(Extends = typeof(Exception))]
    public static class YamlExceptionOps {
        private static readonly CallSite<Func<CallSite, RubyContext, Exception, object>> _Message = CallSite<Func<CallSite, RubyContext, Exception, object>>.Create(LibrarySites.InstanceCallAction("message"));

        [RubyMethod("to_yaml_node")]
        public static Node ToYamlNode(Exception/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            Hash map = new Hash(rep.Context);
            map.Add(MutableString.Create("message"), _Message.Target(_Message, rep.Context, self));
            RubyRepresenter.AddYamlProperties(rep.Context, self, map);
            return rep.Map(self, map);
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri(RubyContext/*!*/ context, object self) {
            MutableString str = MutableString.Create("!ruby/exception:");
            str.Append(RubyUtils.GetClassName(context, self));
            return str;
        }
    }

    [RubyModule(Extends = typeof(MutableString))]
    public static class YamlStringOps {
        [RubyMethod("is_complex_yaml?")]
        public static bool IsComplexYaml(RubyContext/*!*/ context, MutableString/*!*/ self) {
            return RubyOps.IsTrue(RubyRepresenter.ToYamlStyle(context, self)) ||
                   RubyRepresenter.ToYamlProperties(context, self).Count == 0 ||
                   AFTER_NEWLINE(self.ConvertToString());
        }

        // True if has a newline & something is after it
        private static bool AFTER_NEWLINE(string str) {
            int i = str.IndexOf('\n');
            return i >= 0 && i < str.Length - 1;
        }

        private static readonly CallSite<Func<CallSite, RubyContext, MutableString, object>> _Empty = CallSite<Func<CallSite, RubyContext, MutableString, object>>.Create(LibrarySites.InstanceCallAction("empty?"));
        private static readonly CallSite<Func<CallSite, RubyContext, MutableString, object>> _IsBinaryData = CallSite<Func<CallSite, RubyContext, MutableString, object>>.Create(LibrarySites.InstanceCallAction("is_binary_data?"));

        [RubyMethod("is_binary_data?")]
        public static object IsBinaryData(RubyContext/*!*/ context, MutableString/*!*/ self) {
            if (RubyOps.IsTrue(_Empty.Target(_Empty, context, self))) {
                return null;
            }
            // TODO: should be self.IndexOf(0)?
            return self.IndexOf('\0') != -1;
        }

        [RubyMethod("to_yaml_node")]
        public static Node ToYamlNode(MutableString/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            if (RubyOps.IsTrue(_IsBinaryData.Target(_IsBinaryData, rep.Context, self))) {
                return rep.BaseCreateNode(self.ConvertToBytes());
            }

            string str = self.ConvertToString();
            RubyArray props = RubyRepresenter.ToYamlProperties(rep.Context, self);
            if (props.Count == 0) {
                MutableString taguri = RubyRepresenter.TagUri(rep.Context, self);

                char style = (char)0;
                if (str.StartsWith(":")) {
                    style = '"';
                } else {
                    MutableString styleStr = RubyRepresenter.ToYamlStyle(rep.Context, self) as MutableString;
                    if (styleStr != null && styleStr.Length > 0) {
                        style = styleStr.GetChar(0);
                    }
                }

                return rep.Scalar(taguri != null ? taguri.ConvertToString() : "", str, style);
            }

            Hash map = new Hash(rep.Context);
            map.Add(MutableString.Create("str"), str);
            RubyRepresenter.AddYamlProperties(rep.Context, self, map, props);
            return rep.Map(self, map);
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri(object self) {
            return MutableString.Create("tag:yaml.org,2002:str");
        }
    }

    [RubyModule(Extends = typeof(Integer))]
    public static class YamlIntegerOps {
        [RubyMethod("to_yaml_node")]
        public static Node ToYaml(object self, [NotNull]RubyRepresenter/*!*/ rep) {            
            return rep.Scalar(self, RubySites.ToS(rep.Context, self));
        }          

        [RubyMethod("taguri")]
        public static MutableString TagUri(object self) {
            return MutableString.Create("tag:yaml.org,2002:int");
        }
    }

    [RubyModule(Extends = typeof(BigInteger))]
    public static class YamlBigIntegerOps {
        [RubyMethod("to_yaml_node")]
        public static Node ToYaml([NotNull]BigInteger self, [NotNull]RubyRepresenter/*!*/ rep) {
            return YamlIntegerOps.ToYaml(self, rep);
        } 

        [RubyMethod("taguri")]
        public static MutableString TagUri([NotNull]BigInteger self) {     
            return MutableString.Create("tag:yaml.org,2002:int:Bignum");            
        }
    }

    [RubyModule(Extends = typeof(double))]
    public static class YamlDoubleOps {
        [RubyMethod("to_yaml_node")]
        public static Node/*!*/ ToYaml(double self, [NotNull]RubyRepresenter/*!*/ rep) {
            MutableString str = RubySites.ToS(rep.Context, self);
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
        public static MutableString TagUri(double self) {
            return MutableString.Create("tag:yaml.org,2002:float");
        }
    }

    [RubyModule(Extends = typeof(Range))]
    public static class YamlRangeOps {
        private static readonly CallSite<Func<CallSite, RubyContext, object, object>> _Begin = CallSite<Func<CallSite, RubyContext, object, object>>.Create(LibrarySites.InstanceCallAction("begin"));
        private static readonly CallSite<Func<CallSite, RubyContext, object, object>> _End = CallSite<Func<CallSite, RubyContext, object, object>>.Create(LibrarySites.InstanceCallAction("end"));
        private static readonly CallSite<Func<CallSite, RubyContext, object, object>> _ExcludeEnd = CallSite<Func<CallSite, RubyContext, object, object>>.Create(LibrarySites.InstanceCallAction("exclude_end?"));

        [RubyMethod("to_yaml_node")]
        public static Node ToYaml(object self, [NotNull]RubyRepresenter/*!*/ rep) {
            Hash map = new Hash(rep.Context);
            map.Add(MutableString.Create("begin"), _Begin.Target(_Begin, rep.Context, self));
            map.Add(MutableString.Create("end"), _End.Target(_End, rep.Context, self));
            map.Add(MutableString.Create("excl"), _ExcludeEnd.Target(_ExcludeEnd, rep.Context, self));
            RubyRepresenter.AddYamlProperties(rep.Context, self, map);
            return rep.Map(self, map);
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri([NotNull]Range self) {
            return MutableString.Create("tag:ruby.yaml.org,2002:range");
        }
    }

    [RubyModule(Extends = typeof(RubyRegex))]
    public static class YamlRegexpOps {
        [RubyMethod("to_yaml_node")]
        public static Node ToYaml(object self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(self, RubySites.Inspect(rep.Context, self));
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri([NotNull]RubyRegex self) {
            return MutableString.Create("tag:ruby.yaml.org,2002:regexp");
        }
    }

    [RubyModule(Extends = typeof(DateTime))]
    public static class DateTimeOps {
        [RubyMethod("to_yaml_node")]
        public static Node ToYaml(DateTime self, [NotNull]RubyRepresenter/*!*/ rep) {
            string format = (self.Millisecond != 0) ? "yyyy-MM-dd HH:mm:ss.fffffff K" : "yyyy-MM-dd HH:mm:ss K";
            return rep.Scalar(self, MutableString.Create(self.ToString(format)));
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri(DateTime self) {
            return MutableString.Create("tag:yaml.org,2002:timestamp");
        }
    }

    [RubyModule(Extends = typeof(SymbolId))]
    public static class YamlSymbolOps {
        [RubyMethod("to_yaml_node")]
        public static Node ToYaml(object self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(self, RubySites.Inspect(rep.Context, self));
        }
        
        [RubyMethod("taguri")]
        public static MutableString TagUri(object self) {
            return MutableString.Create("tag:yaml.org,2002:str");
        }
    }

    [RubyModule(Extends = typeof(TrueClass))]
    public static class YamlTrueOps {
        [RubyMethod("to_yaml_node")]
        public static Node ToYaml(object self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(self, RubySites.ToS(rep.Context, self));
        }
        [RubyMethod("taguri")]
        public static MutableString TagUri(object self) {
            return MutableString.Create("tag:yaml.org,2002:bool");
        }
    }

    [RubyModule(Extends = typeof(FalseClass))]
    public static class YamlFalseOps {
        [RubyMethod("to_yaml_node")]
        public static Node ToYaml(object self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(self, RubySites.ToS(rep.Context, self));
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri(object self) {
            return MutableString.Create("tag:yaml.org,2002:bool");
        }
    }

    [RubyModule(Extends = typeof(DynamicNull))]
    public static class YamlNilOps {
        [RubyMethod("to_yaml_node")]
        public static Node ToYaml(object self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(self, null);
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri(object self) {
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
