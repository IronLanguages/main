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

using System.Runtime.InteropServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace IronRuby.StandardLibrary.Yaml {

    public static partial class RubyYaml {

        /// <summary>
        /// YAML documents collection. Allows to collect and emit YAML documents.
        /// </summary>
        [RubyClass("Stream")]
        public class YamlStream : RubyObject {
            private Hash _options;
            private RubyArray _documents;

            public YamlStream(RubyClass/*!*/ rubyClass)
                : this(rubyClass, null) {
            }

            public YamlStream(RubyClass/*!*/ rubyClass, Hash options)
                : base(rubyClass) {
                _options = options ?? new Hash(rubyClass.Context.EqualityComparer);
                _documents = new RubyArray();
            }

#if !SILVERLIGHT
            protected YamlStream(SerializationInfo/*!*/ info, StreamingContext context) 
                : base(info, context) {
                // TODO: deserialize
            }

            public override void GetObjectData(SerializationInfo/*!*/ info, StreamingContext context) {
                base.GetObjectData(info, context);
                // TODO: serialize
            }
#endif

            [RubyConstructor]
            public static YamlStream/*!*/ CreateStream(RubyClass/*!*/ self, [Optional]Hash options) {
                return new YamlStream(self, options);
            }

            [RubyMethod("add")]
            public static RubyArray Add(YamlStream/*!*/ self, object document) {
                IListOps.Append(self._documents, document);
                return self._documents;
            }

            [RubyMethod("[]")]
            public static object GetDocument(RubyContext/*!*/ context, YamlStream/*!*/ self, [DefaultProtocol]int index) {
                return IListOps.GetElement(self._documents, index);
            }

            [RubyMethod("edit")]
            public static object EditDocument(YamlStream/*!*/ self, [DefaultProtocol]int index, object document) {
                return IListOps.SetElement(self._documents, index, document);
            }

            [RubyMethod("documents")]
            public static RubyArray GetDocuments(YamlStream/*!*/ self) {
                return self._documents;
            }

            [RubyMethod("documents=")]
            public static RubyArray SetDocuments(YamlStream/*!*/ self, RubyArray value) {
                return self._documents = value;
            }

            [RubyMethod("options")]
            public static Hash GetOptions(YamlStream/*!*/ self) {
                return self._options;
            }

            [RubyMethod("options=")]
            public static Hash SetOptions(YamlStream/*!*/ self, Hash value) {
                return self._options = value;
            }

            [RubyMethod("emit")]
            public static object Emit(YamlCallSiteStorage/*!*/ siteStorage, YamlStream/*!*/ self, [Optional]RubyIO io) {
                return RubyYaml.DumpAll(siteStorage, self._documents, io);
            }

            [RubyMethod("inspect")]
            public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, YamlStream/*!*/ self) {
                MutableString result = MutableString.CreateMutable("#<YAML::Stream:", RubyEncoding.Binary);
                RubyUtils.AppendFormatHexObjectId(result, RubyUtils.GetObjectId(context, self))
                .Append(" @documents=")
                .Append(context.Inspect(self._documents))
                .Append(", options=")
                .Append(context.Inspect(self._options))
                .Append('>');
                return result;
            }
        }     
    }
}