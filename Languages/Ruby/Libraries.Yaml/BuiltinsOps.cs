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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Runtime.InteropServices;

namespace IronRuby.StandardLibrary.Yaml {

    [RubyClass(Extends = typeof(object))]
    public static class YamlObjectOps {
        /// <summary>
        /// Returns an array of sorted instance variable names.
        /// </summary>
        [RubyMethod("to_yaml_properties")]
        public static RubyArray/*!*/ ToYamlProperties(RubyContext/*!*/ context, object self) {
            string[] names = context.GetInstanceVariableNames(self);
            Array.Sort(names);
            return context.StringifyIdentifiers(names);
        }

        [RubyMethod("to_yaml_style")]
        public static object ToYamlStyle(object self) {
            return null;
        }

        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static object ToYamlProperties(object self, [NotNull]RubyRepresenter/*!*/ rep) {
            var map = new Dictionary<object, object>();
            rep.AddYamlProperties(map, self, true);
            return rep.Map(self, map);
        }

        [RubyMethod("to_yaml")]
        public static object ToYaml(object self, [NotNull]RubyRepresenter/*!*/ emitter) {
            if (emitter.Level > 0) {
                // return a node:
                return emitter.Represent(self);
            } else {
                // return a string:
                return RubyYaml.DumpAll(emitter, new object[] { self }, null);
            }
        }

        [RubyMethod("to_yaml")]
        public static object ToYaml(YamlCallSiteStorage/*!*/ siteStorage, object self, [DefaultParameterValue(null)]object options) {
            return RubyYaml.DumpAll(siteStorage, new object[] { self }, null);
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(RubyContext/*!*/ context, object self) {
            var result = MutableString.Create(Tags.RubyObject, context.GetIdentifierEncoding());
            var selfClass = context.GetClassOf(self);
            if (selfClass != context.ObjectClass) {
                return result.Append(':').Append(context.GetClassName(self));
            } else {
                return result;
            }
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
        public static MutableString/*!*/ TagUri(RubyContext/*!*/ context, object/*!*/ self) {
            return RubyYaml.GetTagUri(context, self, Tags.Map, typeof(Hash));
        }
    }

    [RubyModule(Extends = typeof(RubyArray))]
    public static class YamlArrayOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node ToYamlNode(RubyContext/*!*/ context, RubyArray/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Sequence(
                rep.GetTagUri(self, Tags.Seq, typeof(RubyArray)), 
                self, 
                rep.GetYamlStyle(self) != ScalarQuotingStyle.None ? FlowStyle.Inline : FlowStyle.Block
            );
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(RubyContext/*!*/ context, RubyArray/*!*/ self) {
            return RubyYaml.GetTagUri(context, self, Tags.Seq, typeof(RubyArray));
        }
    }

    [RubyModule(Extends = typeof(RubyStruct))]
    public static class YamlStructOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYamlNode(RubyStruct/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            var fieldNames = self.GetNames();

            var fields = new Dictionary<Node, Node>(fieldNames.Count);
            for (int i = 0; i < fieldNames.Count; i++) {
                fields[rep.Scalar(null, fieldNames[i], ScalarQuotingStyle.None)] = rep.RepresentItem(self.Values[i]);
            }

            var map = new Dictionary<object, object>();
            rep.AddYamlProperties(map, self, false);
            return rep.Map(fields, rep.GetTagUri(self), map, FlowStyle.Block);
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(RubyStruct/*!*/ self) {
            MutableString str = MutableString.CreateMutable("tag:ruby.yaml.org,2002:struct:", self.ImmediateClass.Context.GetIdentifierEncoding());
            string name = self.ImmediateClass.GetNonSingletonClass().Name;
            if (name != null) {
                string structPrefix = "Struct::";
                if (name.StartsWith(structPrefix, StringComparison.Ordinal)) {
                    name = name.Substring(structPrefix.Length);
                }
            }
            return str.Append(name);
        }
    }

    [RubyModule(Extends = typeof(Exception))]
    public static class YamlExceptionOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node ToYamlNode(UnaryOpStorage/*!*/ messageStorage, Exception/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            var site = messageStorage.GetCallSite("message", 0);
            var map = new Dictionary<object, object>();
            rep.AddYamlProperties(map, self, false);
            return rep.Map(
                new Dictionary<Node, Node> {
                    { rep.Scalar(null, "message", ScalarQuotingStyle.None), rep.RepresentItem(site.Target(site, self)) }
                },
                rep.GetTagUri(self),
                map, 
                FlowStyle.Block
            );
        }

        [RubyMethod("taguri")]
        public static MutableString TagUri(RubyContext/*!*/ context, object self) {
            return RubyYaml.GetTagUri(context, self, Tags.RubyException, typeof(Exception));
        }
    }

    [RubyModule(Extends = typeof(MutableString))]
    public static class YamlMutableStringOps {
        // True if has a newline & something is after it
        private static bool AfterNewLine(string/*!*/ str) {
            int i = str.IndexOf('\n');
            return i >= 0 && i < str.Length - 1;
        }

        /// <summary>
        /// Returns true if the string binary representation contains bytes from set: 0..0x1f + 0x7f..0xff - [0x0a, 0x0d].
        /// </summary>
        internal static bool ContainsBinaryData(MutableString/*!*/ str) {
            if (!str.IsAscii()) {
                return true;
            }

            // for ascii strings we can iterate over bytes or characters without converting the string repr:
            for (int i = 0; i < str.GetByteCount(); i++) {
                byte b = str.GetByte(i);
                if (b < 0x20 && b != 0x0a && b != 0x0d || b >= 0x7f) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// True if the string representation is multi-line, i.e. if
        /// - the string contains \n followed by data, 
        /// - any YAML properties has been attached to the string (to_yaml_properties returns non-null),
        /// - to_yaml_style returns true value (???)
        /// </summary>
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

        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYamlNode(MutableString/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            if (!self.IsEmpty && ContainsBinaryData(self)) {
                return rep.BaseCreateNode(self.ToByteArray());
            }

            Debug.Assert(self.IsAscii());
            string str = self.ToString();

            ScalarQuotingStyle style = ScalarQuotingStyle.None;
            if (str.StartsWith(":", StringComparison.Ordinal)) {
                style = ScalarQuotingStyle.Double;
            } else {
                style = rep.GetYamlStyle(self);
            }

            var tag = rep.GetTagUri(self, Tags.Str, typeof(MutableString));
            IList instanceVariableNames = rep.ToYamlProperties(self);
            if (instanceVariableNames.Count == 0) {
                return rep.Scalar(tag, str, style);
            }

            var map = new Dictionary<object, object>();
            rep.AddYamlProperties(map, self, instanceVariableNames, false);
            return rep.Map(
                new Dictionary<Node, Node> {
                    { rep.Scalar(null, "str", style), rep.Scalar(null, str, style) }
                },
                tag,
                map,
                FlowStyle.Block
            );
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(RubyContext/*!*/ context, MutableString/*!*/ self) {
            return RubyYaml.GetTagUri(context, self, Tags.Str, typeof(MutableString));
        }
    }

    [RubyModule(Extends = typeof(int))]
    public static class YamlFixnumOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(int self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(Tags.Int, self.ToString(), ScalarQuotingStyle.None);
        }          

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(int self) {
            return MutableString.CreateAscii(Tags.Fixnum);
        }
    }

    [RubyModule(Extends = typeof(BigInteger))]
    public static class YamlBigIntegerOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml([NotNull]BigInteger self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(Tags.Bignum, self.ToString(CultureInfo.InvariantCulture), ScalarQuotingStyle.None);
        } 

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri([NotNull]BigInteger self) {
            return MutableString.CreateAscii(Tags.Bignum);            
        }
    }

    [RubyModule(Extends = typeof(double))]
    public static class YamlDoubleOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(double self, [NotNull]RubyRepresenter/*!*/ rep) {
            if (Double.IsNaN(self)) {
                return rep.Scalar(Tags.Float, ".NaN", ScalarQuotingStyle.None);
            }

            if (Double.IsNegativeInfinity(self)) {
                return rep.Scalar(Tags.Float, "-.Inf", ScalarQuotingStyle.None);
            }

            if (Double.IsPositiveInfinity(self)) {
                return rep.Scalar(Tags.Float, ".Inf", ScalarQuotingStyle.None);
            }

            return rep.Scalar(Tags.Float, ClrFloat.ToS(rep.Context, self).ToString(), ScalarQuotingStyle.None);
        }    

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(double self) {
            return MutableString.CreateAscii(Tags.Float);
        }
    }

    [RubyModule(Extends = typeof(Range))]
    public static class YamlRangeOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(UnaryOpStorage/*!*/ beginStorage, UnaryOpStorage/*!*/ endStorage, UnaryOpStorage/*!*/ exclStorage, 
            Range/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {

            var begin = beginStorage.GetCallSite("begin");
            var end = endStorage.GetCallSite("end");

            var map = new Dictionary<object, object>();
            rep.AddYamlProperties(map, self, false);
            return rep.Map(
                new Dictionary<Node, Node> {
                    { rep.Scalar(null, "begin", ScalarQuotingStyle.None), rep.RepresentItem(begin.Target(begin, self)) },
                    { rep.Scalar(null, "end", ScalarQuotingStyle.None), rep.RepresentItem(end.Target(end, self)) },
                    { rep.Scalar(null, "excl", ScalarQuotingStyle.None), rep.Scalar(self.ExcludeEnd) },
                },
                rep.GetTagUri(self),
                map,
                FlowStyle.Block
            );
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(RubyContext/*!*/ context, Range/*!*/ self) {
            return RubyYaml.GetTagUri(context, self, Tags.RubyRange, typeof(Range));
        }
    }

    [RubyModule(Extends = typeof(RubyRegex))]
    public static class YamlRegexpOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(RubyRegex/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(
                rep.GetTagUri(self), 
                self.Inspect().ToAsciiString(),
                ScalarQuotingStyle.None
            );
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(RubyContext/*!*/ context, RubyRegex/*!*/ self) {
            return RubyYaml.GetTagUri(context, self, Tags.RubyRegexp, typeof(RubyRegex));
        }
    }

    [RubyModule(Extends = typeof(RubyTime))]
    public static class YamlTimeOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(RubyTime/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            TimeSpan offset = self.GetCurrentZoneOffset();
            long fractional = self.Microseconds;
            string value = String.Format(CultureInfo.InvariantCulture,
                "{0:yyyy-MM-dd HH:mm:ss}" + (fractional == 0 ? "" : ".{1:D6}") + (self.Kind == DateTimeKind.Utc ? " Z" : " {2}{3:D2}:{4:D2}"),
                self.DateTime,
                fractional,
                offset.Hours >= 0 ? "+" : "",
                offset.Hours,
                offset.Minutes
            );

            return rep.Scalar(rep.GetTagUri(self), value, ScalarQuotingStyle.None);
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(RubyContext/*!*/ context, RubyTime/*!*/ self) {
            return RubyYaml.GetTagUri(context, self, Tags.Timestamp, typeof(RubyTime));
        }
    }

    [RubyClass("Date", Restrictions = ModuleRestrictions.AllowReopening)]
    public static class YamlDateOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(ConversionStorage<MutableString>/*!*/ tosConversion, object/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            string value = Protocols.ConvertToString(tosConversion, self).ToString();
            return rep.Scalar(rep.GetTagUri(self), value, ScalarQuotingStyle.None);
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(RubyContext/*!*/ context, object/*!*/ self) {
            var result = MutableString.Create(Tags.TimestampYmd, context.GetIdentifierEncoding());
            string className = context.GetClassName(self);
            if (className != "Date") {
                return result.Append(':').Append(className);
            } else {
                return result;
            }
        }
    }

    [RubyModule(Extends = typeof(RubySymbol))]
    public static class YamlSymbolOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(RubySymbol/*!*/ self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(
                // TODO: we should use RubySymbol but that would require ResolverScanner to recognize symbols in order not to emit a tag
                Tags.Str,
                SymbolOps.Inspect(rep.Context, self).ToAsciiString(), 
                ScalarQuotingStyle.None
            );
        }
        
        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(object self) {
            return MutableString.CreateAscii(Tags.RubySymbol);
        }
    }

    [RubyModule(Extends = typeof(TrueClass))]
    public static class YamlTrueOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(ConversionStorage<MutableString>/*!*/ tosConversion, bool self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(self);
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(bool self) {
            return MutableString.CreateAscii(Tags.True);
        }
    }

    [RubyModule(Extends = typeof(FalseClass))]
    public static class YamlFalseOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(ConversionStorage<MutableString>/*!*/ tosConversion, bool self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(self);
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(bool self) {
            return MutableString.CreateAscii(Tags.False);
        }
    }

    [RubyModule(Extends = typeof(DynamicNull))]
    public static class YamlNilOps {
        [RubyMethod("to_yaml_node", RubyMethodAttributes.PrivateInstance)]
        public static Node/*!*/ ToYaml(object self, [NotNull]RubyRepresenter/*!*/ rep) {
            return rep.Scalar(Tags.Null, null, ScalarQuotingStyle.None);
        }

        [RubyMethod("taguri")]
        public static MutableString/*!*/ TagUri(object self) {
            return MutableString.CreateAscii(Tags.Null);
        }
    }
}
