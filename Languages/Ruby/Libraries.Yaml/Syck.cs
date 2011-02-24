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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronRuby.Runtime.Calls;

namespace IronRuby.StandardLibrary.Yaml {
    public static partial class RubyYaml {
        [RubyModule("Syck")]
        public static class Syck {
            [RubyClass("Emitter", Extends = typeof(RubyRepresenter), Inherits = typeof(Object), Restrictions = ModuleRestrictions.NoUnderlyingType)]
            public static class RepresenterOps {
                [RubyMethod("level")]
                public static int GetLevel(RubyRepresenter/*!*/ self) {
                    return self.Level;
                }

                [RubyMethod("level=")]
                public static int SetLevel(RubyRepresenter/*!*/ self, [DefaultProtocol]int level) {
                    return self.Level = level;
                }
            }

            [RubyClass("Out", Restrictions = ModuleRestrictions.NoUnderlyingType)]
            public sealed class Out {
                private RubyRepresenter/*!*/ _representer;

                internal Out(RubyRepresenter/*!*/ representer) {
                    Assert.NotNull(representer);
                    _representer = representer;
                }

                [RubyMethod("emitter")]
                public static RubyRepresenter/*!*/ GetEmitter(Out/*!*/ self) {
                    return self._representer;
                }

                [RubyMethod("emitter=")]
                public static RubyRepresenter/*!*/ SetEmitter(Out/*!*/ self, [NotNull]RubyRepresenter/*!*/ emitter) {
                    return self._representer = emitter;
                }

                [RubyMethod("map")]
                public static object CreateMap([NotNull]BlockParam/*!*/ block, Out/*!*/ self, [DefaultProtocol]MutableString taguri, object yamlStyle) {
                    var rep = self._representer;
                    var map = new MappingNode(rep.ToTag(taguri), new Dictionary<Node, Node>(), RubyYaml.ToYamlFlowStyle(yamlStyle));
                    
                    object blockResult;
                    if (block.Yield(map, out blockResult)) {
                        return blockResult;
                    }

                    return map;
                }

                [RubyMethod("seq")]
                public static object CreateSequence([NotNull]BlockParam/*!*/ block, Out/*!*/ self, [DefaultProtocol]MutableString taguri, object yamlStyle) {
                    var rep = self._representer;
                    var seq = new SequenceNode(rep.ToTag(taguri), new List<Node>(), RubyYaml.ToYamlFlowStyle(yamlStyle));

                    object blockResult;
                    if (block.Yield(seq, out blockResult)) {
                        return blockResult;
                    }

                    return seq;
                }

                // TODO: [RubyMethod("scalar")]
            }

            [RubyClass("Node", Extends = typeof(Node), Inherits = typeof(Object), Restrictions = ModuleRestrictions.NoUnderlyingType)]
            [Includes(typeof(BaseNode))]
            public sealed class NodeOps {
                // TODO: which of these we need to implement?
                // "resolver", "emitter=", "resolver=", "type_id", "kind", "type_id=", "emitter"

                [RubyMethod("transform")]
                public static object Transform(RubyScope/*!*/ scope, Node/*!*/ self) {
                    return new RubyConstructor(scope.GlobalScope, new SimpleNodeProvider(self, RubyYaml.GetEncoding(scope.RubyContext))).GetData();
                }
            }

            [RubyClass("Map", Extends = typeof(MappingNode), Inherits = typeof(Node), Restrictions = ModuleRestrictions.NoUnderlyingType)]
            public sealed class MapOps {
                [RubyMethod("style=")]
                public static object SetStyle(MappingNode/*!*/ self, object value) {
                    self.FlowStyle = RubyYaml.ToYamlFlowStyle(value);
                    return value;
                }

                [RubyMethod("value")]
                public static object GetValue(MappingNode/*!*/ self) {
                    return self.Nodes;
                }

                [RubyMethod("add")]
                public static void Add(YamlCallSiteStorage/*!*/ siteStorage, MappingNode/*!*/ self, object key, object value) {
                    RubyRepresenter rep = new RubyRepresenter(siteStorage);
                    self.Nodes.Add(rep.RepresentItem(key), rep.RepresentItem(value));
                }
            }

            [RubyClass("Seq", Extends = typeof(SequenceNode), Inherits = typeof(Node), Restrictions = ModuleRestrictions.NoUnderlyingType)]
            public sealed class SeqOps {
                // TODO: value=

                [RubyMethod("style=")]
                public static object SetStyle(SequenceNode/*!*/ self, object value) {
                    self.FlowStyle = RubyYaml.ToYamlFlowStyle(value);
                    return value;
                }

                [RubyMethod("value")]
                public static object GetValue(MappingNode/*!*/ self) {
                    return self.Nodes;
                }

                [RubyMethod("add")]
                public static void Add(YamlCallSiteStorage/*!*/ siteStorage, SequenceNode/*!*/ self, object value) {
                    RubyRepresenter rep = new RubyRepresenter(siteStorage);
                    self.Nodes.Add(rep.RepresentItem(value));
                }
            }

            [RubyClass("Scalar", Extends = typeof(ScalarNode), Inherits = typeof(Node), Restrictions = ModuleRestrictions.NoUnderlyingType)]
            public sealed class ScalarOps {
                // TODO: value=

                [RubyMethod("style=")]
                public static object SetStyle(RubyContext/*!*/ context, ScalarNode/*!*/ self, object value) {
                    self.Style = RubyYaml.ToYamlStyle(context, value);
                    return value;
                }

                [RubyMethod("value")]
                public static object GetValue(ScalarNode/*!*/ self) {
                    return MutableString.CreateAscii(self.Value);
                }
            }
        }
    }
}
