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
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using System.Globalization;

namespace IronRuby.StandardLibrary.Yaml {

    public delegate object RubyYamlConstructor(RubyConstructor self, Node node);
    public delegate object RubyYamlMultiConstructor(RubyConstructor self, string pref, Node node);

    public class RubyConstructor : BaseConstructor {
        private readonly static Dictionary<string, RubyYamlConstructor> _yamlConstructors = new Dictionary<string, RubyYamlConstructor>();
        private readonly static Dictionary<string, RubyYamlMultiConstructor> _yamlMultiConstructors = new Dictionary<string, RubyYamlMultiConstructor>();
        private readonly static Dictionary<string, Regex> _yamlMultiRegexps = new Dictionary<string, Regex>();                
        private readonly static Regex _regexPattern = new Regex("^/(?<expr>.+)/(?<opts>[eimnosux]*)$", RegexOptions.Compiled);

        private readonly CallSite<Func<CallSite, RubyModule, object, object, object, object>> _newSite;

        private readonly CallSite<Func<CallSite, object, object, Hash, object>> _yamlInitializeSite;
        
        public RubyConstructor(RubyGlobalScope/*!*/ scope, NodeProvider/*!*/ nodeProvider)
            : base(nodeProvider, scope) {
            
            _newSite = CallSite<Func<CallSite, RubyModule, object, object, object, object>>.Create(
                RubyCallAction.Make(scope.Context, "new", RubyCallSignature.WithImplicitSelf(3))
            ); 

            _yamlInitializeSite = CallSite<Func<CallSite, object, object, Hash, object>>.Create(
                RubyCallAction.Make(scope.Context, "yaml_initialize", RubyCallSignature.WithImplicitSelf(3))
            );
        }

        static RubyConstructor() {
            AddConstructor("tag:yaml.org,2002:str", ConstructRubyScalar);
            AddConstructor("tag:ruby.yaml.org,2002:range", ConstructRubyRange);
            AddConstructor("tag:ruby.yaml.org,2002:regexp", ConstructRubyRegexp);
            AddMultiConstructor("tag:ruby.yaml.org,2002:object:", ConstructPrivateObject);
            AddMultiConstructor("tag:ruby.yaml.org,2002:struct:", ConstructRubyStruct);
            AddConstructor("tag:yaml.org,2002:binary", ConstructRubyBinary);
            AddConstructor("tag:yaml.org,2002:timestamp#ymd", ConstructRubyTimestampYMD);

            //AddConstructor("tag:yaml.org,2002:omap", ConstructRubyOmap);
            //AddMultiConstructor("tag:yaml.org,2002:seq:", ConstructSpecializedRubySequence);
            //AddMultiConstructor("tag:yaml.org,2002:map:", ConstructSpecializedRubyMap);
        }

        public override YamlConstructor GetYamlConstructor(string key) {
            RubyYamlConstructor result;
            if (_yamlConstructors.TryGetValue(key, out result)) {
                return (self, node) => result(this, node);
            }
            return base.GetYamlConstructor(key);
        }

        public override YamlMultiConstructor GetYamlMultiConstructor(string key) {
            RubyYamlMultiConstructor result;
            if (_yamlMultiConstructors.TryGetValue(key, out result)) {
                return (self, pref, node) => result(this, pref, node);
            }
            return base.GetYamlMultiConstructor(key);
        }

        public override Regex GetYamlMultiRegexp(string key) {
            Regex result;
            if (_yamlMultiRegexps.TryGetValue(key, out result)) {
                return result;
            }
            return base.GetYamlMultiRegexp(key);
        }

        public override ICollection<string> GetYamlMultiRegexps() {
            return _yamlMultiRegexps.Keys;
        }

        public static void AddConstructor(string tag, RubyYamlConstructor ctor) {
            if (!_yamlConstructors.ContainsKey(tag)) {
                _yamlConstructors.Add(tag, ctor);
            }
        }

        public static void AddMultiConstructor(string tagPrefix, RubyYamlMultiConstructor ctor) {
            if (!_yamlMultiConstructors.ContainsKey(tagPrefix)) {
                _yamlMultiConstructors.Add(tagPrefix, ctor);
                _yamlMultiRegexps.Add(tagPrefix, new Regex("^" + tagPrefix, RegexOptions.Compiled));
            }
        }

        private class ExternalConstructor {
            private BlockParam _block;

            public ExternalConstructor(BlockParam block) {
                _block = block;
            }

            public object Construct(BaseConstructor ctor, string tag, Node node) {                
                object result;
                _block.Yield(MutableString.Create(tag, RubyEncoding.UTF8), ctor.ConstructPrimitive(node), out result);
                return result;                
            }

            public object Construct(BaseConstructor ctor, Node node) {
                return Construct(ctor, node.Tag, node);
            }
        }

        public static void AddExternalConstructor(string tag, BlockParam block) {
            if (!_yamlConstructors.ContainsKey(tag)) {
                _yamlConstructors.Add(tag, new ExternalConstructor(block).Construct);
            }
        }

        public static void AddExternalMultiConstructor(string regex, BlockParam block) {
            if (!_yamlMultiConstructors.ContainsKey(regex)) {
                _yamlMultiConstructors.Add(regex, new ExternalConstructor(block).Construct);
                _yamlMultiRegexps.Add(regex, new Regex(regex, RegexOptions.Compiled));
            }
        }

        private static object ConstructRubyScalar(RubyConstructor/*!*/ ctor, Node node) {
            object value = ctor.ConstructScalar(node);
            if (value == null) {
                return value;
            }
            string str = value as string;
            if (str != null) {
                return MutableString.Create(str, RubyEncoding.UTF8);
            }
            return value;
        }

        private static object ParseObject(RubyConstructor/*!*/ ctor, string value) {
            Composer composer = RubyYaml.MakeComposer(new StringReader(value));
            if (composer.CheckNode()) {
                return ctor.ConstructObject(composer.GetNode());
            } else {
                throw new ConstructorException("Invalid YAML element: " + value);
            }
        }

        private static Range ConstructRubyRange(RubyConstructor/*!*/ ctor, Node node) {
            object begin = null;
            object end = null;
            bool excludeEnd = false;
            ScalarNode scalar = node as ScalarNode;                        
            if (scalar != null) {
                string value = scalar.Value;                
                int dotsIdx;
                if ((dotsIdx = value.IndexOf("...")) != -1) {
                    begin = ParseObject(ctor, value.Substring(0, dotsIdx));                
                    end = ParseObject(ctor, value.Substring(dotsIdx + 3));
                    excludeEnd = true;
                } else if ((dotsIdx = value.IndexOf("..")) != -1) {
                    begin = ParseObject(ctor, value.Substring(0, dotsIdx));
                    end = ParseObject(ctor, value.Substring(dotsIdx + 2));
                } else {
                    throw new ConstructorException("Invalid Range: " + value);
                }
            } else {
                MappingNode mapping = node as MappingNode;
                if (mapping == null) {
                    throw new ConstructorException("Invalid Range: " + node);    
                }
                foreach (KeyValuePair<Node, Node> n in mapping.Nodes) {
                    string key = ctor.ConstructScalar(n.Key).ToString();
                    switch (key) {
                        case "begin":
                            begin = ctor.ConstructObject(n.Value);
                            break;
                        case "end":
                            end = ctor.ConstructObject(n.Value);
                            break;
                        case "excl":
                            TryConstructYamlBool(ctor, n.Value, out excludeEnd);
                            break;
                        default:
                            throw new ConstructorException(string.Format("'{0}' is not allowed as an instance variable name for class Range", key));
                    }
                }                
            }

            var comparisonStorage = new BinaryOpStorage(ctor.GlobalScope.Context);
            return new Range(comparisonStorage, ctor.GlobalScope.Context, begin, end, excludeEnd);            
        }

        private static RubyRegex ConstructRubyRegexp(RubyConstructor/*!*/ ctor, Node node) {
            ScalarNode scalar = node as ScalarNode;
            if (node == null) {
                throw RubyExceptions.CreateTypeError("Can only create regex from scalar node");
            }                        
            Match match = _regexPattern.Match(scalar.Value);
            if (!match.Success) {
                throw new ConstructorException("Invalid Regular expression: \"" + scalar.Value + "\"");
            }
            RubyRegexOptions options = new RubyRegexOptions();
            foreach (char c in match.Groups["opts"].Value) {
                switch (c) {
                    case 'i': options |= RubyRegexOptions.IgnoreCase; break;
                    case 'x': options |= RubyRegexOptions.Extended; break;
                    case 'm': options |= RubyRegexOptions.Multiline; break;
                    case 'o': break;
                    case 'n': options |= RubyRegexOptions.FIXED; break;
                    case 'e': options |= RubyRegexOptions.EUC; break;
                    case 's': options |= RubyRegexOptions.SJIS; break;
                    case 'u': options |= RubyRegexOptions.UTF8; break;
                    default:
                        throw new ConstructorException("Unknown regular expression option: '" + c + "'");
                }
            }            
            // TODO: encoding (ignore kcode on 1.9, string enc?):
            return new RubyRegex(MutableString.CreateMutable(match.Groups["expr"].Value, RubyEncoding.UTF8), options);            
        }

        private static object ConstructPrivateObject(RubyConstructor/*!*/ ctor, string className, Node node) {
            MappingNode mapping = node as MappingNode;
            if (mapping == null) {
                throw new ConstructorException("can only construct private type from mapping node");
            }
            RubyModule module;
            RubyGlobalScope globalScope = ctor.GlobalScope;
            if (globalScope.Context.TryGetModule(globalScope, className, out module)) {
                if (!module.IsClass) {
                    throw new ConstructorException("Cannot construct module");
                }
                Hash values = ctor.ConstructMapping(mapping);
                RubyMethodInfo method = module.GetMethod("yaml_initialize") as RubyMethodInfo;
                if (method != null) {
                    object result = RubyUtils.CreateObject((RubyClass)module);
                    ctor._yamlInitializeSite.Target(ctor._yamlInitializeSite, result, className, values);
                    return result;
                } else {
                    return RubyUtils.CreateObject((RubyClass)module, values, true);
                }
            } else {
                //TODO: YAML::Object
                throw new NotImplementedError("YAML::Object is not implemented yet");
            }
        }

        private static object ConstructRubyStruct(RubyConstructor/*!*/ ctor, string className, Node node) {
            MappingNode mapping = node as MappingNode;
            if (mapping == null) {
                throw new ConstructorException("can only construct struct from mapping node");
            }

            RubyContext context = ctor.GlobalScope.Context;
            RubyModule module;
            RubyClass cls;
            if (context.TryGetModule(ctor.GlobalScope, className, out module)) {
                cls = module as RubyClass;
                if (cls == null) {
                    throw new ConstructorException("Struct type name must be Ruby class");
                }
            } else {
                RubyModule structModule = context.GetModule(typeof(RubyStruct));
                cls = RubyUtils.GetConstant(ctor.GlobalScope, structModule, className, false) as RubyClass;
                if (cls == null) {
                    throw new ConstructorException(String.Format("Cannot find struct class \"{0}\"", className));
                }
            }

            RubyStruct newStruct = RubyStruct.Create(cls);
            foreach (var pair in ctor.ConstructMapping(mapping)) {
                RubyStructOps.SetValue(newStruct, SymbolTable.StringToId(pair.Key.ToString()), pair.Value);        
            }
            return newStruct;
        }

        private static MutableString ConstructRubyBinary(RubyConstructor/*!*/ ctor, Node node) {
            return MutableString.CreateBinary(BaseConstructor.ConstructYamlBinary(ctor, node));
        }

        private static object ConstructRubyTimestampYMD(RubyConstructor/*!*/ ctor, Node node) {
            ScalarNode scalar = node as ScalarNode;
            if (scalar == null) {
                throw new ConstructorException("Can only contruct timestamp from scalar node.");
            }

            Match match = BaseConstructor.YmdRegex.Match(scalar.Value);
            if (match.Success) {
                int year_ymd = Int32.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                int month_ymd = Int32.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                int day_ymd = Int32.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

                RubyModule module;
                if (ctor.GlobalScope.Context.TryGetModule(ctor.GlobalScope, "Date", out module)) {
                    return ctor._newSite.Target(ctor._newSite, module, year_ymd, month_ymd, day_ymd);
                } else {
                    throw new ConstructorException("Date class not found.");
                }
            }
            throw new ConstructorException("Invalid tag:yaml.org,2002:timestamp#ymd value.");
        }
    }
}