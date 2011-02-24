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
using System.Diagnostics;

namespace IronRuby.StandardLibrary.Yaml {

    [RubyModule("YAML")]    
    public static partial class RubyYaml {
        [RubyModule("BaseNode")]
        public static class BaseNode {
            // TODO: which of these we need to implement?
            // "children_with_index", "[]", "select!", "children", "at", "search", "match_path", "match_segment", "select", "emit"
        }

        [RubyConstant("Emitter")]
        public static RubyClass/*!*/ Emitter(RubyModule/*!*/ module) {
            var result = module.Context.GetClass(typeof(RubyRepresenter));
            Debug.Assert(result != null, "All classes are loaded at the time the library is loaded");
            return result;
        }

        private const string _TaggedClasses = "tagged_classes";

        [RubyMethod("add_builtin_type", RubyMethodAttributes.PublicSingleton)]
        public static object AddBuiltinType(
            [NotNull]BlockParam/*!*/ block, 
            RubyModule/*!*/ self, 
            [DefaultProtocol, NotNull]MutableString/*!*/ typeTag) {
            // Add a stub implementation to unblock Rails 3
            Console.WriteLine("WARNING: YAML.add_builtin_type is not implemented");
            return null;
        }

        // TODO: missing public singleton methods:
        //add_private_type
        //add_ruby_type
        //detect_implicit
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
            taggedClasses.Add(MutableString.CreateAscii(Tags.RubyRange), context.GetClass(typeof(Range)));
            taggedClasses.Add(MutableString.CreateAscii(Tags.RubyRegexp), context.GetClass(typeof(RubyRegex)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:string"), context.GetClass(typeof(MutableString)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:struct"), context.GetClass(typeof(RubyStruct)));
            taggedClasses.Add(MutableString.CreateAscii(Tags.RubySymbol), context.GetClass(typeof(RubySymbol)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:symbol"), context.GetClass(typeof(RubySymbol)));
            taggedClasses.Add(MutableString.CreateAscii("tag:ruby.yaml.org,2002:time"), context.GetClass(typeof(RubyTime)));
            taggedClasses.Add(MutableString.CreateAscii(Tags.Binary), context.GetClass(typeof(MutableString)));
            taggedClasses.Add(MutableString.CreateAscii(Tags.Float), context.GetClass(typeof(Double)));
            taggedClasses.Add(MutableString.CreateAscii(Tags.Int), context.GetClass(typeof(Integer)));
            taggedClasses.Add(MutableString.CreateAscii(Tags.Map), context.GetClass(typeof(Hash)));
            taggedClasses.Add(MutableString.CreateAscii(Tags.Seq), context.GetClass(typeof(RubyArray)));
            taggedClasses.Add(MutableString.CreateAscii(Tags.Str), context.GetClass(typeof(MutableString)));
            taggedClasses.Add(MutableString.CreateAscii(Tags.Timestamp), context.GetClass(typeof(RubyTime)));
                                                 
            taggedClasses.Add(MutableString.CreateAscii(Tags.False), context.FalseClass);
            taggedClasses.Add(MutableString.CreateAscii(Tags.True), context.TrueClass);
            taggedClasses.Add(MutableString.CreateAscii(Tags.Null), context.NilClass);

            //if (ctor.GlobalScope.Context.TryGetModule(ctor.GlobalScope, "Date", out module)) {
            //    taggedClasses.Add(MutableString.CreateAscii(Tags.TimestampYmd), context.NilClass);
            //}

            //taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:timestamp#ymd'"), );
            //Currently not supported             
            //taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:omap"), ec.GetClass(typeof()));
            //taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:pairs"),//    ec.GetClass(typeof()));
            //taggedClasses.Add(MutableString.CreateAscii("tag:yaml.org,2002:set"),//    ec.GetClass(typeof()));
            return taggedClasses;
        }

        [RubyMethod("tag_class", RubyMethodAttributes.PublicSingleton)]
        public static object TagClass(RubyModule/*!*/ self, object tag, object clazz) {
            Hash tagged_classes = (Hash)GetTaggedClasses(self);
            return RubyUtils.SetHashElement(self.Context, tagged_classes, tag, clazz);
        }

        [RubyMethod("dump", RubyMethodAttributes.PublicSingleton)]
        public static object Dump(YamlCallSiteStorage/*!*/ siteStorage, RubyModule/*!*/ self, object obj, [Optional]RubyIO io) {
            return DumpAll(siteStorage, new object[] { obj }, io);
        }

        [RubyMethod("dump_all", RubyMethodAttributes.PublicSingleton)]
        public static object DumpAll(YamlCallSiteStorage/*!*/ siteStorage, RubyModule/*!*/ self, [NotNull]IList/*!*/ objs, [Optional]RubyIO io) {
            return DumpAll(siteStorage, objs, io);
        }

        internal static object DumpAll(YamlCallSiteStorage/*!*/ siteStorage, IEnumerable/*!*/ objs, RubyIO io) {
            return DumpAll(new RubyRepresenter(siteStorage), objs, io);
        }

        internal static object DumpAll(RubyRepresenter/*!*/ rep, IEnumerable/*!*/ objs, RubyIO io) {
            TextWriter writer;
            if (io != null) {
                writer = new RubyIOWriter(io);
            } else {
                // the output is ascii:
                writer = new MutableStringWriter(MutableString.CreateMutable(RubyEncoding.Binary));
            }

            YamlOptions cfg = YamlOptions.DefaultOptions;
            Serializer s = new Serializer(writer, cfg);
            foreach (object obj in objs) {
                s.Serialize(rep.Represent(obj));
                rep.ForgetObjects();
            }
            s.Close();

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
                foreach (object obj in MakeConstructor(scope.GlobalScope, GetStream(toStr, respondTo, io))) {
                    return obj;
                }
                return null;
            } catch (Exception e) {
                throw RubyExceptions.CreateArgumentError(e, e.Message);
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
            RubyConstructor rc = MakeConstructor(scope.GlobalScope, GetStream(toStr, respondTo, io));
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
            
            RubyConstructor rc = MakeConstructor(scope.GlobalScope, GetStream(toStr, respondTo, io));

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
            using (Stream stream = GetStream(toStr, respondTo, io)) {
                foreach (Node obj in MakeComposer(self.Context, stream)) {
                    // TODO: the enumerator shouldn't return null:
                    if (obj == null) {
                        break;
                    } 
                    
                    return obj;
                }
            }
            return ScriptingRuntimeHelpers.False;
        }

        [RubyMethod("parse_file", RubyMethodAttributes.PublicSingleton)]
        public static object ParseFile(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            using (Stream stream = new RubyFile(self.Context, path.ConvertToString(), IOMode.Default).GetReadableStream()) {
                foreach (Node obj in MakeComposer(self.Context, stream)) {
                    return obj;
                }
            }
            return ScriptingRuntimeHelpers.False;
        }

        [RubyMethod("parse_documents", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("each_node", RubyMethodAttributes.PublicSingleton)]
        public static object ParseDocuments(ConversionStorage<MutableString>/*!*/ toStr, RespondToStorage/*!*/ respondTo,
            BlockParam block, RubyModule/*!*/ self, object io) {

            foreach (Node obj in MakeComposer(self.Context, GetStream(toStr, respondTo, io))) {
                // TODO: the enumerator shouldn't return null:
                if (obj == null) {
                    return null;
                }

                if (block == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                object result;
                if (block.Yield(obj, out result)) {
                    return result;
                }
            }

            return null;
        }

        [RubyMethod("dump_stream", RubyMethodAttributes.PublicSingleton)]
        public static object DumpStream(UnaryOpStorage/*!*/ newStorage, BinaryOpStorage/*!*/ addStorage, UnaryOpStorage/*!*/ emitStorage,
            RubyScope/*!*/ scope, RubyModule/*!*/ self, params object[]/*!*/ args) {

            object io = CreateDefaultStream(newStorage, scope, self);
            AddDocumentsToStream(addStorage, args, io);

            var emitSite = emitStorage.GetCallSite("emit");
            return emitSite.Target(emitSite, io);
        }

        // TODO:
        [RubyMethod("quick_emit_node", RubyMethodAttributes.PublicSingleton)]
        public static object QuickEmitNode(BlockParam block, RubyModule/*!*/ self, object arg, params object[]/*!*/ rest) {
            if (block != null) {
                object result;
                block.Yield(arg, out result);
                return result;
            }
            return null;
        }

        [RubyMethod("quick_emit", RubyMethodAttributes.PublicSingleton)]
        public static object QuickEmit(YamlCallSiteStorage/*!*/ siteStorage, [NotNull]BlockParam/*!*/ block, RubyModule/*!*/ self, object objectId, [NotNull]Hash/*!*/ opts) {
            // TODO: load from opts
            YamlOptions cfg = YamlOptions.DefaultOptions;
            
            MutableStringWriter writer = new MutableStringWriter(MutableString.CreateMutable(RubyEncoding.Binary));
            Serializer s = new Serializer(writer, cfg);
            RubyRepresenter rep = new RubyRepresenter(siteStorage);
            object result;
            
            if (block.Yield(new Syck.Out(rep), out result)) {
                return result;
            }

            s.Serialize(rep.ToNode(result));
            s.Close();

            return writer.String;
        }

        [RubyMethod("quick_emit", RubyMethodAttributes.PublicSingleton)]
        public static object QuickEmit(RubyContext/*!*/ context, [NotNull]BlockParam/*!*/ block, RubyModule/*!*/ self, object objectId, [NotNull]RubyRepresenter/*!*/ emitter) {
            object result;
            block.Yield(new Syck.Out(emitter), out result);
            return result;
        }

        [RubyMethod("emitter", RubyMethodAttributes.PublicSingleton)]
        public static RubyRepresenter/*!*/ CreateEmitter(YamlCallSiteStorage/*!*/ siteStorage, RubyModule/*!*/ self) {
            return new RubyRepresenter(siteStorage);
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

        #region Helpers

        internal static Encoding/*!*/ GetEncoding(RubyContext/*!*/ context) {
            // MRI 1.9: UTF8 is used regardless of the string ending
            return (context.RubyOptions.Compatibility < RubyCompatibility.Ruby19 ? RubyEncoding.Binary : RubyEncoding.UTF8).Encoding;
        }

        private static RubyConstructor/*!*/ MakeConstructor(RubyGlobalScope/*!*/ scope, Stream/*!*/ stream) {
            return new RubyConstructor(scope, MakeComposer(scope.Context, stream));
        }

        internal static Composer/*!*/ MakeComposer(RubyContext/*!*/ context, Stream/*!*/ stream) {
            var encoding = GetEncoding(context);
            // Do not throw on invalid characters:
            // TODO: invalid characters are replaced by '?' while MRI keeps the bytes:
            return MakeComposer(new StreamReader(stream, encoding), encoding);
        }

        internal static Composer/*!*/ MakeComposer(TextReader/*!*/ reader, Encoding/*!*/ encoding) {
            return new Composer(new Parser(new Scanner(reader, encoding), YamlOptions.DefaultOptions.Version));
        }

        private static Stream/*!*/ GetStream(ConversionStorage<MutableString>/*!*/ toStr, RespondToStorage/*!*/ respondTo, object port) {
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

        internal static FlowStyle ToYamlFlowStyle(object styleObj) {
            return Protocols.IsTrue(styleObj) ? FlowStyle.Inline : FlowStyle.Block;
        }

        internal static ScalarQuotingStyle ToYamlStyle(RubyContext/*!*/ context, object styleObj) {
            if (styleObj == null) {
                return ScalarQuotingStyle.None;
            }

            // TODO: any conversions?
            var mstr = styleObj as MutableString;
            if (mstr == null) {
                throw RubyExceptions.CreateUnexpectedTypeError(context, styleObj, "String");
            }

            if (mstr.IsEmpty) {
                return ScalarQuotingStyle.None;
            }

            switch (mstr.GetChar(0)) {
                case '"': return ScalarQuotingStyle.Double;
                case '\'': return ScalarQuotingStyle.Single;

                // TODO: allow these???
                case '\0': return ScalarQuotingStyle.None;
                case '|': return ScalarQuotingStyle.Literal;
                case '>': return ScalarQuotingStyle.Folded;

                default:
                    // ??
                    throw new ArgumentException("Invalid style");
            }
        }

        internal static MutableString/*!*/ GetTagUri(RubyContext/*!*/ context, object/*!*/ obj, string/*!*/ baseTag, Type/*!*/ baseType) {
            var result = MutableString.Create(baseTag, context.GetIdentifierEncoding());
            if (obj.GetType() != baseType) {
                return result.Append(':').Append(context.GetClassName(obj));
            } else {
                return result;
            }
        }

        #endregion

    }
}
