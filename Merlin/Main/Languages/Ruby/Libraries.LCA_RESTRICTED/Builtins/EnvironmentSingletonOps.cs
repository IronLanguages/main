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
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    /// <summary>
    /// ENV singleton trait.
    /// </summary>
    [RubyConstant("ENV")]
    [RubySingleton, Includes(typeof(Enumerable))]
    public static class EnvironmentSingletonOps {

        #region Public Instance Methods

        [RubyMethod("[]", RubyMethodAttributes.PublicInstance)]
        [RubyMethod("fetch", RubyMethodAttributes.PublicInstance)]
        public static MutableString GetVariable(RubyContext/*!*/ context, object/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ name) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            string value = pal.GetEnvironmentVariable(name.ConvertToString());
            return (value != null) ? MutableString.Create(value) : null;
        }

        [RubyMethod("[]=", RubyMethodAttributes.PublicInstance)]
        [RubyMethod("store", RubyMethodAttributes.PublicInstance)]
        public static MutableString SetVariable(RubyContext/*!*/ context, object/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ name, [DefaultProtocol]MutableString value) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            pal.SetEnvironmentVariable(name.ConvertToString(), (value != null) ? value.ConvertToString() : null);
            return value;
        }

        [RubyMethod("clear")]
        public static object Clear(RubyContext/*!*/ context, object/*!*/ self) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            foreach (DictionaryEntry entry in pal.GetEnvironmentVariables()) {
                pal.SetEnvironmentVariable(entry.Key.ToString(), null);
            }
            return self;
        }

        [RubyMethod("delete")]
        public static object Delete(RubyContext/*!*/ context, object/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ name) {
            MutableString result = GetVariable(context, self, name);
            if (result != null) {
                SetVariable(context, self, name, null);
            }
            return result;
        }

        [RubyMethod("delete_if")]
        [RubyMethod("reject!")]
        public static object DeleteIf(RubyContext/*!*/ context, BlockParam block, object/*!*/ self) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            IDictionary variables = pal.GetEnvironmentVariables();
            if (variables.Count > 0 && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            foreach (DictionaryEntry entry in variables) {
                MutableString key = MutableString.Create(entry.Key.ToString()).Freeze();
                MutableString value = MutableString.Create(entry.Value.ToString()).Freeze();
                object result;
                if (block.Yield(key, value, out result)) {
                    return result;
                }

                if (RubyOps.IsTrue(result)) {
                    SetVariable(context, self, key, null);
                }
            }
            return self;
        }

        [RubyMethod("each")]
        [RubyMethod("each_pair")]
        public static object Each(RubyContext/*!*/ context, BlockParam block, object/*!*/ self) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            IDictionary variables = pal.GetEnvironmentVariables();
            if (variables.Count > 0 && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            foreach (DictionaryEntry entry in variables) {
                RubyArray array = new RubyArray(2);
                array.Add(MutableString.Create(entry.Key.ToString()).Freeze());
                array.Add(MutableString.Create(entry.Value.ToString()).Freeze());
                object result;
                if (block.Yield(array, out result)) {
                    return result;
                }
            }
            return self;
        }

        [RubyMethod("each_key")]
        public static object EachKey(RubyContext/*!*/ context, BlockParam block, object/*!*/ self) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            IDictionary variables = pal.GetEnvironmentVariables();
            if (variables.Count > 0 && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            foreach (DictionaryEntry entry in variables) {
                MutableString name = MutableString.Create(entry.Key.ToString()).Freeze();
                object result;
                if (block.Yield(name, out result)) {
                    return result;
                }
            }
            return self;
        }

        [RubyMethod("each_value")]
        public static object EachValue(RubyContext/*!*/ context, BlockParam block, object/*!*/ self) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            IDictionary variables = pal.GetEnvironmentVariables();
            if (variables.Count > 0 && block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            foreach (DictionaryEntry entry in variables) {
                MutableString value = MutableString.Create(entry.Value.ToString()).Freeze();
                object result;
                if (block.Yield(value, out result)) {
                    return result;
                }
            }
            return self;
        }

        [RubyMethod("empty?")]
        public static bool IsEmpty(RubyContext/*!*/ context, object/*!*/ self) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            return (pal.GetEnvironmentVariables().Count == 0);
        }

        [RubyMethod("has_key?")]
        [RubyMethod("include?")]
        [RubyMethod("key?")]
        public static bool HasKey(RubyContext/*!*/ context, object/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ key) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            return pal.GetEnvironmentVariable(key.ConvertToString()) != null;
        }

        [RubyMethod("has_value?")]
        [RubyMethod("value?")]
        public static bool HasValue(RubyContext/*!*/ context, object/*!*/ self, object value) {
            // MRI doesn't use CastToString conversion here:
            var strValue = value as MutableString;
            if (value == null) {
                return false;
            }

            var clrStrValue = strValue.ConvertToString();
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            foreach (DictionaryEntry entry in pal.GetEnvironmentVariables()) {
                if (clrStrValue.Equals(entry.Value)) {
                    return true;
                }
            }
            return false;
        }

        [RubyMethod("index")]
        public static MutableString Index(RubyContext/*!*/ context, object/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ value) {
            string strValue = value.ConvertToString();
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            foreach (DictionaryEntry entry in pal.GetEnvironmentVariables()) {
                if (strValue.Equals(entry.Value)) {
                    return MutableString.Create(entry.Key.ToString()).Freeze();
                }
            }
            return null;
        }

        [RubyMethod("indices")]
        public static RubyArray/*!*/ Indices(RubyContext/*!*/ context, object/*!*/ self, 
            [DefaultProtocol, NotNull, NotNullItems]params MutableString[]/*!*/ keys) {
            context.ReportWarning("ENV.indices is deprecated; use ENV.values_at");
            return ValuesAt(context, self, keys);
        }

        [RubyMethod("indexes")]
        public static RubyArray/*!*/ Index(RubyContext/*!*/ context, object/*!*/ self,
            [DefaultProtocol, NotNull, NotNullItems]params MutableString[]/*!*/ keys) {
            context.ReportWarning("ENV.indexes is deprecated; use ENV.values_at");
            return ValuesAt(context, self, keys);
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, object/*!*/ self) {
            return HashOps.Inspect(context, ToHash(context, self));
        }

        [RubyMethod("invert")]
        public static Hash/*!*/ Invert(RubyContext/*!*/ context, object/*!*/ self) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            Hash result = new Hash(context);
            foreach (DictionaryEntry entry in pal.GetEnvironmentVariables()) {
                result.Add(MutableString.Create(entry.Value.ToString()).Freeze(), MutableString.Create(entry.Key.ToString()).Freeze());
            }
            return result;
        }

        [RubyMethod("keys")]
        public static RubyArray/*!*/ Keys(RubyContext/*!*/ context, object/*!*/ self) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            IDictionary variables = pal.GetEnvironmentVariables();
            RubyArray result = new RubyArray(variables.Count);
            foreach (DictionaryEntry entry in variables) {
                result.Add(MutableString.Create(entry.Key.ToString()).Freeze());
            }
            return result;
        }

        [RubyMethod("length")]
        [RubyMethod("size")]
        public static int Length(RubyContext/*!*/ context, object/*!*/ self) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            return pal.GetEnvironmentVariables().Count;
        }

        [RubyMethod("rehash")]
        public static object Rehash(object/*!*/ self) {
            return null;
        }

        private static void Update(ConversionStorage<MutableString>/*!*/ stringCast, Hash/*!*/ values) {
            PlatformAdaptationLayer pal = stringCast.Context.DomainManager.Platform;
            foreach (var pair in values) {
                var name = Protocols.CastToString(stringCast, pair.Key).ToString();
                var value = Protocols.CastToString(stringCast, pair.Value).ToString();
                
                pal.SetEnvironmentVariable(name, value);
            }
        }

        [RubyMethod("replace")]
        public static object/*!*/ Replace(ConversionStorage<MutableString>/*!*/ stringCast, object/*!*/ self, [NotNull]Hash/*!*/ values) {
            Clear(stringCast.Context, self);
            Update(stringCast, values);
            return self;
        }

        [RubyMethod("update")]
        public static object/*!*/ Update(ConversionStorage<MutableString>/*!*/ stringCast, object/*!*/ self, [NotNull]Hash/*!*/ values) {
            Update(stringCast, values);
            return self;
        }

        [RubyMethod("shift")]
        public static object Shift(RubyContext/*!*/ context, object/*!*/ self) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            IDictionary variables = pal.GetEnvironmentVariables();
            if (variables.Count == 0) {
                return null;
            }
            RubyArray result = new RubyArray(2);
            foreach (DictionaryEntry entry in pal.GetEnvironmentVariables()) {
                string key = entry.Key.ToString();
                result.Add(MutableString.Create(key).Freeze());
                result.Add(MutableString.Create(entry.Value.ToString()).Freeze());
                pal.SetEnvironmentVariable(key, null);
                break;
            }
            return result;
        }

        [RubyMethod("to_hash")]
        public static Hash/*!*/ ToHash(RubyContext/*!*/ context, object/*!*/ self) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            Hash result = new Hash(context);
            foreach (DictionaryEntry entry in pal.GetEnvironmentVariables()) {
                result.Add(MutableString.Create(entry.Key.ToString()).Freeze(), MutableString.Create(entry.Value.ToString()).Freeze());
            }
            return result;
        }

        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToString(object/*!*/ self) {
            return MutableString.Create("ENV");
        }

        [RubyMethod("values")]
        public static RubyArray/*!*/ Values(RubyContext/*!*/ context, object/*!*/ self) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            IDictionary variables = pal.GetEnvironmentVariables();
            RubyArray result = new RubyArray(variables.Count);
            foreach (DictionaryEntry entry in variables) {
                result.Add(MutableString.Create(entry.Value.ToString()).Freeze());
            }
            return result;
        }

        [RubyMethod("values_at")]
        public static RubyArray/*!*/ ValuesAt(RubyContext/*!*/ context, object/*!*/ self, 
            [DefaultProtocol, NotNull, NotNullItems]params MutableString/*!*/[]/*!*/ keys) {
            RubyArray result = new RubyArray(keys.Length);
            foreach (MutableString key in keys) {
                result.Add(GetVariable(context, self, key));
            }
            return result;
        }

        #endregion
    }
}
