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
        private readonly static Regex _regexPattern = YamlUtils.CompiledRegex("^/(?<expr>.+)/(?<opts>[eimnosux]*)$");

        private readonly CallSite<Func<CallSite, RubyModule, object, object, object, object>> _newSite;
        private readonly CallSite<Func<CallSite, object, object, Hash, object>> _yamlInitializeSite;
        private readonly RubyEncoding/*!*/ _encoding;
        
        public RubyConstructor(RubyGlobalScope/*!*/ scope, NodeProvider/*!*/ nodeProvider)
            : base(nodeProvider, scope) {

            _encoding = RubyEncoding.GetRubyEncoding(nodeProvider.Encoding);

            _newSite = CallSite<Func<CallSite, RubyModule, object, object, object, object>>.Create(
                RubyCallAction.Make(scope.Context, "new", RubyCallSignature.WithImplicitSelf(3))
            ); 

            _yamlInitializeSite = CallSite<Func<CallSite, object, object, Hash, object>>.Create(
                RubyCallAction.Make(scope.Context, "yaml_initialize", RubyCallSignature.WithImplicitSelf(3))
            );
        }

        public RubyEncoding/*!*/ RubyEncoding {
            get { return _encoding; }
        }

        static RubyConstructor() {
            AddConstructor(Tags.Str, ConstructRubyString);
            AddConstructor(Tags.RubySymbol, ConstructRubySymbol);
            AddConstructor(Tags.RubyRange, ConstructRubyRange);
            AddConstructor(Tags.RubyRegexp, ConstructRubyRegexp);
            AddMultiConstructor("tag:ruby.yaml.org,2002:object:", ConstructPrivateObject);
            AddMultiConstructor("tag:ruby.yaml.org,2002:struct:", ConstructRubyStruct);
            AddConstructor(Tags.Binary, ConstructRubyBinary);
            AddConstructor(Tags.TimestampYmd, ConstructRubyDate);

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
                _yamlMultiRegexps.Add(tagPrefix, YamlUtils.CompiledRegex("^" + tagPrefix));
            }
        }

        private class ExternalConstructor {
            private BlockParam _block;

            public ExternalConstructor(BlockParam block) {
                _block = block;
            }

            public object Construct(BaseConstructor ctor, string tag, Node node) {                
                object result;
                _block.Yield(MutableString.Create(tag, RubyEncoding.GetRubyEncoding(ctor.Encoding)), ctor.ConstructPrimitive(node), out result);
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
                _yamlMultiRegexps.Add(regex, YamlUtils.CompiledRegex(regex));
            }
        }

        private static object ParseObject(RubyConstructor/*!*/ ctor, string/*!*/ value) {
            Composer composer = RubyYaml.MakeComposer(new StringReader(value), ctor.Encoding);
            if (composer.CheckNode()) {
                return ctor.ConstructObject(composer.GetNode());
            } else {
                throw new ConstructorException("Invalid YAML element: " + value);
            }
        }

        /// <summary>
        /// Returns MutableString or RubySymbol.
        /// </summary>
        private static object ConstructRubyString(RubyConstructor/*!*/ ctor, Node/*!*/ node) {
            ScalarNode scalar = (ScalarNode)node;
            string value = ctor.ConstructScalar(node);

            if (value == null) {
                return null;
            }

            if (value.Length > 1 && value[0] == ':' && scalar.Style == ScalarQuotingStyle.None) {
                return ctor.GlobalScope.Context.CreateAsciiSymbol(value.Substring(1));
            }

            return MutableString.CreateMutable(value, ctor.RubyEncoding);
        }

        private static object ConstructRubySymbol(RubyConstructor/*!*/ ctor, Node/*!*/ node) {
            return ctor.GlobalScope.Context.CreateAsciiSymbol(((ScalarNode)node).Value);
        }

        private static Range/*!*/ ConstructRubyRange(RubyConstructor/*!*/ ctor, Node/*!*/ node) {
            object begin = null;
            object end = null;
            bool excludeEnd = false;
            ScalarNode scalar = node as ScalarNode;                        
            if (scalar != null) {
                string value = scalar.Value;                
                int dotsIdx;
                if ((dotsIdx = value.IndexOf("...", StringComparison.Ordinal)) != -1) {
                    begin = ParseObject(ctor, value.Substring(0, dotsIdx));                
                    end = ParseObject(ctor, value.Substring(dotsIdx + 3));
                    excludeEnd = true;
                } else if ((dotsIdx = value.IndexOf("..", StringComparison.Ordinal)) != -1) {
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
                            if (!TryConstructYamlBool(ctor, n.Value, out excludeEnd)) {
                                throw new ConstructorException("Invalid Range: " + node);    
                            }
                            break;
                        default:
                            throw new ConstructorException(String.Format("'{0}' is not allowed as an instance variable name for class Range", key));
                    }
                }                
            }

            var comparisonStorage = new BinaryOpStorage(ctor.GlobalScope.Context);
            return new Range(comparisonStorage, ctor.GlobalScope.Context, begin, end, excludeEnd);            
        }

        private static RubyRegex/*!*/ ConstructRubyRegexp(RubyConstructor/*!*/ ctor, Node/*!*/ node) {
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
                    // TODO: call allocate here:
                    object result = RubyUtils.CreateObject((RubyClass)module);
                    ctor._yamlInitializeSite.Target(ctor._yamlInitializeSite, result, className, values);
                    return result;
                } else {
                    return RubyUtils.CreateObject((RubyClass)module, EnumerateAttributes(globalScope.Context, values));
                }
            } else {
                //TODO: YAML::Object
                throw new NotImplementedError("YAML::Object is not implemented yet");
            }
        }

        private static IEnumerable<KeyValuePair<string, object>>/*!*/ EnumerateAttributes(RubyContext/*!*/ context, Hash/*!*/ mapping) {
            foreach (var entry in mapping) {
                yield return new KeyValuePair<string, object>("@" + RubyRepresenter.ConvertToFieldName(context, entry.Key), entry.Value);
            }
        }

        private static object ConstructRubyStruct(RubyConstructor/*!*/ ctor, string/*!*/ structName, Node/*!*/ node) {
            MappingNode mapping = node as MappingNode;
            if (mapping == null) {
                throw new ConstructorException("can only construct struct from mapping node");
            }

            if (structName.Length == 0) {
                // TODO:
                throw new NotSupportedException("anonymous structs not supported");
            }

            RubyContext context = ctor.GlobalScope.Context;
            RubyModule module;

            // TODO: MRI calls "members" on an arbitrary object
            
            // MRI checks Struct first, then falls back to Object
            if (!context.TryGetModule(ctor.GlobalScope, "Struct::" + structName, out module) && 
                !context.TryGetModule(ctor.GlobalScope, structName, out module)) {
                throw RubyExceptions.CreateTypeError("Undefined struct `{0}'", structName);
            }

            RubyClass cls = module as RubyClass;
            if (cls == null) {
                throw RubyExceptions.CreateTypeError("`{0}' is not a class", structName);
            }

            RubyStruct newStruct = RubyStruct.Create(cls);
            foreach (var pair in ctor.ConstructMapping(mapping)) {
                var attributeName = pair.Key as MutableString;
                int index;

                // TODO: encoding
                if (attributeName != null && newStruct.TryGetIndex(attributeName.ToString(), out index)) {
                    newStruct[index] = pair.Value;
                }
            }
            return newStruct;
        }

        private static MutableString/*!*/ ConstructRubyBinary(RubyConstructor/*!*/ ctor, Node/*!*/ node) {
            return MutableString.CreateBinary(BaseConstructor.ConstructYamlBinary(ctor, node));
        }

        private static object ConstructRubyDate(RubyConstructor/*!*/ ctor, Node node) {
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