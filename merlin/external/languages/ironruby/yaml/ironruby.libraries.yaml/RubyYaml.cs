/***** BEGIN LICENSE BLOCK *****
 * Version: CPL 1.0
 *
 * The contents of this file are subject to the Common Public
 * License Version 1.0 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.eclipse.org/legal/cpl-v10.html
 *
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 * Copyright (C) 2007 Ola Bini <ola.bini@gmail.com>
 * Copyright (c) Microsoft Corporation.
 * 
 ***** END LICENSE BLOCK *****/

using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.Yaml {

    [RubyModule("YAML")]    
    public static class RubyYaml {
        private static readonly CallSite<Func<CallSite, RubyContext, RubyModule, object, object>> _New = CallSite<Func<CallSite, RubyContext, RubyModule, object, object>>.Create(LibrarySites.InstanceCallAction("new", 1));
        private static readonly CallSite<Func<CallSite, RubyContext, object, object, object>> _Add = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(LibrarySites.InstanceCallAction("add", 1));
        private static readonly CallSite<Func<CallSite, RubyContext, object, object>> _Emit = CallSite<Func<CallSite, RubyContext, object, object>>.Create(LibrarySites.InstanceCallAction("emit"));
        private const string _Stream = "Stream";
        private const string _TaggedClasses = "tagged_classes";

        // TODO: missing public singleton methods:
        //add_builtin_type
        //add_private_type
        //add_ruby_type
        //detect_implicit
        //emitter
        //generic_parser
        //object_maker
        //read_type_class
        //resolver
        //transfer
        //try_implicit
        //yaml_tag_class_name
        //yaml_tag_read_class

        [RubyMethod("tagged_classes", RubyMethodAttributes.PublicSingleton)]
        public static object GetTaggedClasses(RubyModule/*!*/ self) {
            object taggedClasses;
            if (!self.TryGetClassVariable(_TaggedClasses, out taggedClasses)) {
                taggedClasses = CreateDefaultTagMapping(self.Context);
                self.SetClassVariable(_TaggedClasses, taggedClasses);                                
            }            
            return taggedClasses;
        }

        private static Hash CreateDefaultTagMapping(RubyContext/*!*/ context) {
            Hash taggedClasses = new Hash(context.EqualityComparer);
            taggedClasses.Add(MutableString.Create("tag:ruby.yaml.org,2002:array"), context.GetClass(typeof(RubyArray)));
            taggedClasses.Add(MutableString.Create("tag:ruby.yaml.org,2002:exception"), context.GetClass(typeof(Exception)));
            taggedClasses.Add(MutableString.Create("tag:ruby.yaml.org,2002:hash"), context.GetClass(typeof(Hash)));
            taggedClasses.Add(MutableString.Create("tag:ruby.yaml.org,2002:object"), context.GetClass(typeof(object)));
            taggedClasses.Add(MutableString.Create("tag:ruby.yaml.org,2002:range"), context.GetClass(typeof(Range)));
            taggedClasses.Add(MutableString.Create("tag:ruby.yaml.org,2002:regexp"), context.GetClass(typeof(RubyRegex)));
            taggedClasses.Add(MutableString.Create("tag:ruby.yaml.org,2002:string"), context.GetClass(typeof(MutableString)));
            taggedClasses.Add(MutableString.Create("tag:ruby.yaml.org,2002:struct"), context.GetClass(typeof(RubyStruct)));
            taggedClasses.Add(MutableString.Create("tag:ruby.yaml.org,2002:sym"), context.GetClass(typeof(SymbolId)));
            taggedClasses.Add(MutableString.Create("tag:ruby.yaml.org,2002:symbol"), context.GetClass(typeof(SymbolId)));
            taggedClasses.Add(MutableString.Create("tag:ruby.yaml.org,2002:time"), context.GetClass(typeof(DateTime)));
            taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:binary"), context.GetClass(typeof(MutableString)));
            taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:bool#no"), context.FalseClass);
            taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:bool#yes"), context.TrueClass);
            taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:float"), context.GetClass(typeof(Double)));
            taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:int"), context.GetClass(typeof(Integer)));
            taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:map"), context.GetClass(typeof(Hash)));
            taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:null"), context.NilClass);            
            taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:seq"), context.GetClass(typeof(RubyArray)));            
            taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:str"), context.GetClass(typeof(MutableString)));
            taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:timestamp"), context.GetClass(typeof(DateTime)));
            //Currently not supported
            //taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:omap"), ec.GetClass(typeof()));
            //taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:pairs"),//    ec.GetClass(typeof()));
            //taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:set"),//    ec.GetClass(typeof()));
            //taggedClasses.Add(MutableString.Create("tag:yaml.org,2002:timestamp#ymd'"), );
            return taggedClasses;
        }

        [RubyMethod("tag_class", RubyMethodAttributes.PublicSingleton)]
        public static object TagClass(RubyModule/*!*/ self, object tag, object clazz) {
            Hash tagged_classes = (Hash)GetTaggedClasses(self);
            return RubyUtils.SetHashElement(self.Context, tagged_classes, tag, clazz);
        }

        [RubyMethod("dump", RubyMethodAttributes.PublicSingleton)]
        public static object Dump(RubyModule/*!*/ self, object obj, [Optional]RubyIO io) {
            return DumpAll(self, new object[] { obj }, io);
        }

        [RubyMethod("dump_all", RubyMethodAttributes.PublicSingleton)]
        public static object DumpAll(RubyModule/*!*/ self, [NotNull]IEnumerable objs, [Optional]RubyIO io) {
            return DumpAll(self.Context, objs, io);
        }

        internal static object DumpAll(RubyContext/*!*/ context, [NotNull]IEnumerable objs, [Optional]RubyIO io) {
            TextWriter writer;
            if (io != null) {
                writer = new RubyIOWriter(io);
            } else {
                writer = new MutableStringWriter();
            }
            YamlOptions cfg = YamlOptions.DefaultOptions;
            using (Serializer s = new Serializer(new Emitter(writer, cfg), cfg)) {
                RubyRepresenter r = new RubyRepresenter(context, s, cfg);
                foreach (object obj in objs) {
                    r.Represent(obj);
                }
            }
            if (null != io) {
                return io;
            } else {
                return ((MutableStringWriter)writer).String;
            }
        }

        [RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
        public static object Load(RubyScope/*!*/ scope, RubyModule/*!*/ self, object io) {
            try {
                foreach (object obj in MakeConstructor(scope, CheckYamlPort(io))) {
                    return obj;
                }
                return null;
            } finally {
                RubyIO rio = io as RubyIO;
                if (rio != null) {
                    rio.Close();
                }
            }
        }

        [RubyMethod("load_file", RubyMethodAttributes.PublicSingleton)]
        public static object LoadFile(RubyScope/*!*/ scope, RubyModule/*!*/ self, object arg) {
            RubyClass file = self.Context.GetClass(typeof(RubyFile));
            object io = RubyFileOps.Open(null, file, arg, MutableString.Create("r"));
            return Load(scope, self, io as RubyIO);
        }

        [RubyMethod("each_document", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("load_documents", RubyMethodAttributes.PublicSingleton)]
        public static object EachDocument(RubyScope/*!*/ scope, BlockParam block, RubyModule/*!*/ self, object io) {
            RubyConstructor rc = MakeConstructor(scope, CheckYamlPort(io));
            if (block == null && rc.CheckData()) {
                throw RubyExceptions.NoBlockGiven();
            }

            foreach (object obj in rc) {
                object result;
                if (block.Yield(obj, out result)) {
                    return result;
                }
            }
            return null;
        }

        [RubyMethod("load_stream", RubyMethodAttributes.PublicSingleton)]
        public static object LoadStream(RubyScope/*!*/ scope, RubyModule/*!*/ self, object io) {
            RubyConstructor rc = MakeConstructor(scope, CheckYamlPort(io));
            object streamClass = RubyUtils.GetConstant(scope, self, _Stream, false);
            object stream = _New.Target(_New, scope.RubyContext, streamClass as RubyModule, null);
            foreach (object doc in rc) {
                _Add.Target(_Add, scope.RubyContext, stream, doc);
            }
            return stream;
        }

        [RubyMethod("parse", RubyMethodAttributes.PublicSingleton)]
        public static object Parse(RubyModule self, object io) {
            try {
                foreach (object obj in MakeComposer(CheckYamlPort(io))) {
                    return obj;
                }
                return null;
            } finally {
                RubyIO rio = io as RubyIO;
                if (rio != null) {
                    rio.Close();
                }
            }
        }

        [RubyMethod("parse_documents", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("each_node", RubyMethodAttributes.PublicSingleton)]
        public static object ParseDocuments(BlockParam block, RubyModule self, object io) {
            Composer c = MakeComposer(CheckYamlPort(io));
            if (block == null && c.CheckNode()) {
                throw RubyExceptions.NoBlockGiven();
            }
            foreach (object obj in c) {
                object result;
                if (block.Yield(obj, out result)) {
                    return result;
                }
            }
            return null;
        }

        [RubyMethod("parse_file", RubyMethodAttributes.PublicSingleton)]
        public static object ParseFile(RubyModule/*!*/ self, object arg) {
            RubyClass file = self.Context.GetClass(typeof(RubyFile));
            object io = RubyFileOps.Open(null, file, arg, MutableString.Create("r"));
            return Parse(self, io as RubyIO);
        }

        [RubyMethod("dump_stream", RubyMethodAttributes.PublicSingleton)]
        public static object DumpStream(RubyScope/*!*/ scope, RubyModule/*!*/ self, [NotNull]params object[] args) {
            object streamClass = RubyUtils.GetConstant(scope, self, _Stream, false);
            object stream = _New.Target(_New, scope.RubyContext, streamClass as RubyModule, null);
            foreach (object arg in args) {
                _Add.Target(_Add, scope.RubyContext, stream, arg);
            }
            return _Emit.Target(_Emit, scope.RubyContext, stream);
        }

        [RubyMethod("quick_emit_node", RubyMethodAttributes.PublicSingleton)]
        public static object QuickEmitNode(BlockParam block, RubyModule/*!*/ self, object arg, params object[] rest) {
            if (block != null) {
                object result;
                block.Yield(arg, out result);
                return result;
            }
            return null;
        }

        [RubyMethod("quick_emit", RubyMethodAttributes.PublicSingleton)]
        public static object QuickEmit(RubyContext/*!*/ context, BlockParam/*!*/ block, RubyModule/*!*/ self, object objectId, params object[] opts) {
            if (block == null) {
                throw RubyExceptions.NoBlockGiven();
            }
            MutableStringWriter writer = new MutableStringWriter();
            //We currently don't support serialization options, so we just ignore opts argument
            YamlOptions cfg = YamlOptions.DefaultOptions;
            using (Serializer s = new Serializer(new Emitter(writer, cfg), cfg)) {
                RubyRepresenter r = new RubyRepresenter(context, s, cfg);
                object result;
                block.Yield(r, out result);
                s.Serialize(result as Node);
                return writer.String;
            }
        }

        [RubyMethod("tagurize", RubyMethodAttributes.PublicSingleton)]
        public static object Tagurize(RubyContext context, RubyModule self, object arg) {
            if (arg == null) {
                return null;
            }
            if (RubySites.RespondTo(context, arg, "to_str")) {
                return MutableString.Create("tag:yaml.org,2002:").Append(Protocols.ConvertToString(context, arg));
            }
            return arg;
        }

        [RubyMethod("add_domain_type", RubyMethodAttributes.PublicSingleton)]
        public static object AddDomainType(RubyContext/*!*/ context, BlockParam/*!*/ block, RubyModule/*!*/ self, 
            MutableString/*!*/ domainAndDate, MutableString/*!*/ typeName) {
            if (block == null) {
                throw RubyExceptions.NoBlockGiven();
            }                        
            MutableString tag = MutableString.Create("tag:").
                                Append(domainAndDate).Append(":").
                                Append(typeName);
            RubyConstructor.AddExternalConstructor(tag.ConvertToString(), block);            
            return null;
        }

        [RubyMethod("add_domain_type", RubyMethodAttributes.PublicSingleton)]
        public static object AddDomainType(RubyContext/*!*/ context, BlockParam/*!*/ block, RubyModule/*!*/ self, 
            MutableString/*!*/ domainAndDate, RubyRegex/*!*/ typeRegex) {
            if (block == null) {
                throw RubyExceptions.NoBlockGiven();
            }                        
            MutableString tag = MutableString.Create("tag:").
                                Append(domainAndDate).Append(":").
                                Append(typeRegex.GetPattern());
            RubyConstructor.AddExternalMultiConstructor(tag.ConvertToString(), block);
            return null;
        }

        private static RubyConstructor/*!*/ MakeConstructor(RubyScope/*!*/ scope, TextReader/*!*/ reader) {
            return new RubyConstructor(scope, MakeComposer(reader));
        }

        internal static Composer/*!*/ MakeComposer(TextReader/*!*/ reader) {
            return new Composer(new Parser(new Scanner(reader), YamlOptions.DefaultOptions.Version));
        }

        private static TextReader CheckYamlPort(object port) {
            MutableString ms = port as MutableString;
            if (ms != null) {
                return new MutableStringReader(ms);
            }

            string str = port as string;
            if (str != null) {
                return new StringReader(str);
            }

            RubyIO io = port as RubyIO;
            if (io != null) {
                RubyIOOps.Binmode(io);
                return new RubyIOReader(io);
            }

            throw RubyExceptions.CreateTypeError("instance of IO needed");
        }

        /// <summary>
        /// YAML documents collection. Allows to collect and emit YAML documents.
        /// </summary>
        [RubyClass("Stream", Extends = typeof(YamlStream))]
        public static class YamlStreamOps {

            [RubyConstructor]
            public static YamlStream CreateStream(RubyClass/*!*/ self, [Optional]Hash options) {
                return new YamlStream(options ?? new Hash(self.Context.EqualityComparer));
            }

            [RubyMethod("add")]
            public static RubyArray Add(RubyContext/*!*/ context, YamlStream/*!*/ self, object document) {
                return IListOps.Append(context, self.Documents, document) as RubyArray;
            }

            [RubyMethod("[]")]
            public static object GetDocument(RubyContext/*!*/ context, YamlStream/*!*/ self, object index) {
                return IListOps.GetElement(self.Documents, Protocols.CastToFixnum(context, index));
            }

            [RubyMethod("edit")]
            public static object EditDocument(RubyContext/*!*/ context, YamlStream/*!*/ self, object index, object document) {
                return IListOps.SetElement(context, self.Documents, Protocols.CastToFixnum(context, index), document);
            }

            [RubyMethod("documents")]
            public static object GetDocuments(RubyContext/*!*/ context, YamlStream/*!*/ self) {
                return self.Documents;
            }

            [RubyMethod("documents=")]
            public static object SetDocuments(RubyContext/*!*/ context, YamlStream/*!*/ self, RubyArray value) {
                return self.Documents = value;
            }
            
            [RubyMethod("options")]
            public static object GetOptions(RubyContext/*!*/ context, YamlStream/*!*/ self) {
                return self.Options;
            }

            [RubyMethod("options=")]
            public static object SetOptions(RubyContext/*!*/ context, YamlStream/*!*/ self, Hash value) {
                return self.Options = value;
            }

            [RubyMethod("emit")]
            public static object Emit(RubyContext/*!*/ context, YamlStream/*!*/ self, [Optional]RubyIO io) {
                return RubyYaml.DumpAll(context, self.Documents, io);
            }

            [RubyMethod("inspect")]
            public static MutableString Inspect(RubyContext/*!*/ context, YamlStream/*!*/ self) {
                MutableString result = MutableString.CreateMutable("#<YAML::Stream:");
                RubyUtils.AppendFormatHexObjectId(result, RubyUtils.GetObjectId(context, self))
                .Append(" @documents=")
                .Append(RubySites.Inspect(context, self.Documents))
                .Append(", options=")
                .Append(RubySites.Inspect(context, self.Options))
                .Append('>');
                return result;
            }
        }        
    }
}
