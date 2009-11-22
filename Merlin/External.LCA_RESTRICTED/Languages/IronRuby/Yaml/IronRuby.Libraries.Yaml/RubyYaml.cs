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
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime.Conversions;
using System.Text;

namespace IronRuby.StandardLibrary.Yaml {

    [RubyModule("YAML")]    
    public static partial class RubyYaml {

        [RubyModule("BaseNode")]
        public static class BaseNode { }

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
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:array"), context.GetClass(typeof(RubyArray)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:exception"), context.GetClass(typeof(Exception)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:hash"), context.GetClass(typeof(Hash)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:object"), context.GetClass(typeof(object)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:range"), context.GetClass(typeof(Range)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:regexp"), context.GetClass(typeof(RubyRegex)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:string"), context.GetClass(typeof(MutableString)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:struct"), context.GetClass(typeof(RubyStruct)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:sym"), context.GetClass(typeof(SymbolId)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:symbol"), context.GetClass(typeof(SymbolId)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:time"), context.GetClass(typeof(Time)));
            taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:binary"), context.GetClass(typeof(MutableString)));
            taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:float"), context.GetClass(typeof(Double)));
            taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:int"), context.GetClass(typeof(Integer)));
            taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:map"), context.GetClass(typeof(Hash)));
            taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:seq"), context.GetClass(typeof(RubyArray)));
            taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:str"), context.GetClass(typeof(MutableString)));
            taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:timestamp"), context.GetClass(typeof(Time)));
                                                 
            taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:bool#no"), context.FalseClass);
            taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:bool#yes"), context.TrueClass);
            taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:null"), context.NilClass);
            //Currently not supported             
            //taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:omap"), ec.GetClass(typeof()));
            //taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:pairs"),//    ec.GetClass(typeof()));
            //taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:set"),//    ec.GetClass(typeof()));
            //taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:timestamp#ymd'"), );
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
                // TODO: encoding?
                writer = new MutableStringWriter(MutableString.CreateMutable(RubyEncoding.UTF8));
            }

            YamlOptions cfg = YamlOptions.DefaultOptions;
            using (Serializer s = new Serializer(new Emitter(writer, cfg), cfg)) {
                RubyRepresenter r = new RubyRepresenter(context, s, cfg);
                foreach (object obj in objs) {
                    r.Represent(obj);
                }
            }

            if (io != null) {
                return io;
            } else {
                return ((MutableStringWriter)writer).String;
            }
        }

        [RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
        public static object Load(ConversionStorage<MutableString>/*!*/ toStr, RespondToStorage/*!*/ respondTo,
            RubyScope/*!*/ scope, RubyModule/*!*/ self, object io) {

            try {
                foreach (object obj in MakeConstructor(scope.GlobalScope, CheckYamlPort(toStr, respondTo, io))) {
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
        public static object LoadFile(RubyScope/*!*/ scope, RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            using (RubyFile file = new RubyFile(self.Context, path.ConvertToString(), IOMode.Default)) {
                foreach (object obj in MakeConstructor(scope.GlobalScope, file.GetReadableStream())) {
                    return obj;
                }
            }
            return null;
        }

        [RubyMethod("each_document", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("load_documents", RubyMethodAttributes.PublicSingleton)]
        public static object EachDocument(ConversionStorage<MutableString>/*!*/ toStr, RespondToStorage/*!*/ respondTo, 
            RubyScope/*!*/ scope, BlockParam block, RubyModule/*!*/ self, object io) {
            RubyConstructor rc = MakeConstructor(scope.GlobalScope, CheckYamlPort(toStr, respondTo, io));
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
        public static object LoadStream(ConversionStorage<MutableString>/*!*/ toStr, RespondToStorage/*!*/ respondTo, 
            UnaryOpStorage/*!*/ newStorage, BinaryOpStorage/*!*/ addStorage, RubyScope/*!*/ scope, 
            RubyModule/*!*/ self, object io) {
            
            RubyConstructor rc = MakeConstructor(scope.GlobalScope, CheckYamlPort(toStr, respondTo, io));

            // TODO: only if io was converted to a string:
            io = CreateDefaultStream(newStorage, scope, self);

            AddDocumentsToStream(addStorage, rc, io);
            return io;
        }

        private static object CreateDefaultStream(UnaryOpStorage/*!*/ newStorage, RubyScope/*!*/ scope, RubyModule/*!*/ yamlModule) {
            object streamClass = RubyUtils.GetConstant(scope.GlobalScope, yamlModule, "Stream", false);
            var newSite = newStorage.GetCallSite("new");
            return newSite.Target(newSite, streamClass);
        }

        private static void AddDocumentsToStream(BinaryOpStorage/*!*/ addStorage, IEnumerable/*!*/ documents, object io) {
            var addSite = addStorage.GetCallSite("add");
            foreach (object doc in documents) {
                addSite.Target(addSite, io, doc);
            }
        }

        [RubyMethod("parse", RubyMethodAttributes.PublicSingleton)]
        public static object Parse(ConversionStorage<MutableString>/*!*/ toStr, RespondToStorage/*!*/ respondTo, RubyModule/*!*/ self, object io) {
            using (Stream stream = CheckYamlPort(toStr, respondTo, io)) {
                foreach (object obj in MakeComposer(stream)) {
                    return obj;
                }
            }
            return null;
        }

        [RubyMethod("parse_file", RubyMethodAttributes.PublicSingleton)]
        public static object ParseFile(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            using (Stream stream = new RubyFile(self.Context, path.ConvertToString(), IOMode.Default).GetReadableStream()) {
                foreach (object obj in MakeComposer(stream)) {
                    return obj;
                }
            }
            return null;
        }

        [RubyMethod("parse_documents", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("each_node", RubyMethodAttributes.PublicSingleton)]
        public static object ParseDocuments(ConversionStorage<MutableString>/*!*/ toStr, RespondToStorage/*!*/ respondTo,
            BlockParam block, RubyModule/*!*/ self, object io) {

            Composer c = MakeComposer(CheckYamlPort(toStr, respondTo, io));
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

        [RubyMethod("dump_stream", RubyMethodAttributes.PublicSingleton)]
        public static object DumpStream(UnaryOpStorage/*!*/ newStorage, BinaryOpStorage/*!*/ addStorage, UnaryOpStorage/*!*/ emitStorage,
            RubyScope/*!*/ scope, RubyModule/*!*/ self, [NotNull]params object[] args) {

            object io = CreateDefaultStream(newStorage, scope, self);
            AddDocumentsToStream(addStorage, args, io);

            var emitSite = emitStorage.GetCallSite("emit");
            return emitSite.Target(emitSite, io);
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
        public static object QuickEmit(RubyContext/*!*/ context, [NotNull]BlockParam/*!*/ block, RubyModule/*!*/ self, object objectId, [NotNull]Hash/*!*/ opts) {
            YamlOptions cfg = YamlOptions.DefaultOptions;
            
            // TODO: encoding
            MutableStringWriter writer = new MutableStringWriter(MutableString.CreateMutable(RubyEncoding.UTF8));
            Emitter emitter = new Emitter(writer, cfg);

            using (Serializer s = new Serializer(emitter, cfg)) {
                RubyRepresenter r = new RubyRepresenter(context, s, cfg);
                object result;

                if (block.Yield(r, out result)) {
                    return result;
                }

                s.Serialize(result as Node);
                return writer.String;
            }
        }

        [RubyMethod("quick_emit", RubyMethodAttributes.PublicSingleton)]
        public static Node QuickEmit(RubyContext/*!*/ context, [NotNull]BlockParam/*!*/ block, RubyModule/*!*/ self, object objectId, [NotNull]RubyRepresenter/*!*/ opts) {
            object result;
            block.Yield(opts, out result);
            return result as Node;
        }

        [RubyMethod("tagurize", RubyMethodAttributes.PublicSingleton)]
        public static object Tagurize(ConversionStorage<MutableString>/*!*/ stringTryCast, RubyModule/*!*/ self, object arg) {
            var str = Protocols.TryCastToString(stringTryCast, arg);
            return (str != null) ? MutableString.CreateMutable(str.Encoding).Append("tag:yaml.org,2002:").Append(str) : arg;
        }

        [RubyMethod("add_domain_type", RubyMethodAttributes.PublicSingleton)]
        public static object AddDomainType(RubyContext/*!*/ context, BlockParam/*!*/ block, RubyModule/*!*/ self, 
            MutableString/*!*/ domainAndDate, MutableString/*!*/ typeName) {
            if (block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            MutableString tag = MutableString.CreateMutable(typeName.Encoding).
                Append("tag:").
                Append(domainAndDate).
                Append(":").
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

            MutableString tag = MutableString.CreateMutable(typeRegex.Encoding).
                Append("tag:").
                Append(domainAndDate).
                Append(':').
                Append(typeRegex.Pattern);

            RubyConstructor.AddExternalMultiConstructor(tag.ConvertToString(), block);
            return null;
        }

        private static RubyConstructor/*!*/ MakeConstructor(RubyGlobalScope/*!*/ scope, Stream/*!*/ stream) {
            return new RubyConstructor(scope, MakeComposer(stream));
        }

        internal static Composer/*!*/ MakeComposer(Stream/*!*/ stream) {
            return MakeComposer(new StreamReader(stream, Encoding.UTF8));
        }

        internal static Composer/*!*/ MakeComposer(TextReader/*!*/ reader) {
            return new Composer(new Parser(new Scanner(reader), YamlOptions.DefaultOptions.Version));
        }

        private static Stream/*!*/ CheckYamlPort(ConversionStorage<MutableString>/*!*/ toStr, RespondToStorage/*!*/ respondTo, object port) {
            var toStrSite = toStr.GetSite(TryConvertToStrAction.Make(toStr.Context));
            MutableString str = toStrSite.Target(toStrSite, port);
            if (str != null) {
                return new MutableStringStream(str);
            }

            IOWrapper wrapper = RubyIOOps.CreateIOWrapper(respondTo, port, FileAccess.Read);
            if (!wrapper.CanRead) {
                throw RubyExceptions.CreateTypeError("instance of IO needed");
            }

            return wrapper;
        }
   
    }
}
