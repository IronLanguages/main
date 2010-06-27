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
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using Microsoft.Scripting.Generation;
using System.Threading;
using IronRuby.Runtime.Calls;

namespace IronRuby.Builtins {

    /// <summary>
    /// Dictionary inherits from Object, mixes in Enumerable.
    /// Ruby hash is a Dictionary{object, object}, but it adds default value/proc
    /// 
    /// TODO: Not all .NET types implement the right Equals, GetHashCode semantics (e.g. List{object})
    /// </summary>
    [RubyClass("Hash", Extends = typeof(Hash), Inherits = typeof(object)), Includes(typeof(IDictionary<object, object>), Copy = true)]
    public static class HashOps {

        #region Construction

        [RubyConstructor]
        public static Hash/*!*/ CreateHash(RubyClass/*!*/ self) {
            return new Hash(self.Context.EqualityComparer);
        }

        [RubyConstructor]
        public static Hash/*!*/ CreateHash(BlockParam block, RubyClass/*!*/ self, object defaultValue) {
            if (block != null) {
                throw RubyExceptions.CreateArgumentError("wrong number of arguments");
            }
            return new Hash(self.Context.EqualityComparer, null, defaultValue);
        }

        [RubyConstructor]
        public static Hash/*!*/ CreateHash([NotNull]BlockParam/*!*/ defaultProc, RubyClass/*!*/ self) {
            return new Hash(self.Context.EqualityComparer, defaultProc.Proc, null);
        }

        [RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
        public static Hash/*!*/ CreateSubclass(RubyClass/*!*/ self) {
            return Hash.CreateInstance(self);
        }
        
        [RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
        public static Hash/*!*/ CreateSubclass(RubyClass/*!*/ self, [NotNull]IDictionary<object, object>/*!*/ hash) {
            // creates a new hash and copies entries of the given hash into it (no other objects associated with the has are copied):
            return IDictionaryOps.ReplaceData(Hash.CreateInstance(self), hash);
        }

        [RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
        public static Hash/*!*/ CreateSubclass(RubyClass/*!*/ self, params object[]/*!*/ items) {
            Debug.Assert(items.Length > 0);
            if (items.Length % 2 != 0) {
                throw RubyExceptions.CreateArgumentError("odd number of arguments for Hash");
            }

            return RubyUtils.SetHashElements(self.Context, Hash.CreateInstance(self), items);
        }

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static Hash/*!*/ Initialize(Hash/*!*/ self) {
            Assert.NotNull(self);
            return self;
        }

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static Hash/*!*/ Initialize(BlockParam block, Hash/*!*/ self, object defaultValue) {
            Assert.NotNull(self);
            if (block != null) {
                throw RubyExceptions.CreateArgumentError("wrong number of arguments");
            }
            self.DefaultProc = null;
            self.DefaultValue = defaultValue;
            return self;
        }

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static Hash/*!*/ Initialize([NotNull]BlockParam/*!*/ defaultProc, Hash/*!*/ self) {
            Assert.NotNull(self, defaultProc);

            self.DefaultProc = defaultProc.Proc;
            self.DefaultValue = null;
            return self;
        }

        [RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
        public static Hash/*!*/ InitializeCopy(RubyContext/*!*/ context, Hash/*!*/ self, [NotNull]Hash/*!*/ source) {
            self.Mutate();
            self.DefaultProc = source.DefaultProc;
            self.DefaultValue = source.DefaultValue;
            IDictionaryOps.ReplaceData(self, source);
            return self;
        }

        #endregion
        
        #region Instance Methods
        
        [RubyMethod("[]")]
        public static object GetElement(CallSiteStorage<Func<CallSite, Hash, object, object>>/*!*/ storage, Hash/*!*/ self, object key) {
            object result;
            if (!self.TryGetValue(BaseSymbolDictionary.NullToObj(key), out result)) {
                var site = storage.GetCallSite("default", 1);
                return site.Target(site, self, key);
            }
            return result;
        }

        [RubyMethod("default")]
        public static object GetDefaultValue(RubyContext/*!*/ context, Hash/*!*/ self) {
            return self.DefaultValue;
        }

        [RubyMethod("default")]
        public static object GetDefaultValue(CallSiteStorage<Func<CallSite, Proc, Hash, object, object>>/*!*/ storage, Hash/*!*/ self, object key) {
            if (self.DefaultProc != null) {
                var site = storage.GetCallSite("call", 2);
                return site.Target(site, self.DefaultProc, self, key);
            }
            return self.DefaultValue;
        }

        [RubyMethod("default=")]
        public static object SetDefaultValue(RubyContext/*!*/ context, Hash/*!*/ self, object value) {
            self.Mutate();
            self.DefaultProc = null;
            return self.DefaultValue = value;
        }

        [RubyMethod("default_proc")]
        public static Proc GetDefaultProc(Hash/*!*/ self) {
            return self.DefaultProc;
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, Hash/*!*/ self) {

            using (IDisposable handle = RubyUtils.InfiniteInspectTracker.TrackObject(self)) {
                if (handle == null) {
                    return MutableString.CreateAscii("{...}");
                }

                MutableString str = MutableString.CreateMutable(RubyEncoding.Binary);
                str.Append('{');
                bool first = true;
                foreach (var entry in self) {
                    if (first) {
                        first = false;
                    } else {
                        str.Append(", ");
                    }
                    str.Append(context.Inspect(BaseSymbolDictionary.ObjToNull(entry.Key)));
                    str.Append("=>");
                    str.Append(context.Inspect(entry.Value));
                }
                str.Append('}');
                return str;
            }
        }
        
        [RubyMethod("replace")]
        public static Hash/*!*/ Replace(RubyContext/*!*/ context, Hash/*!*/ self, [DefaultProtocol, NotNull]IDictionary<object,object>/*!*/ other) {
            if (Object.ReferenceEquals(self, other)) {
                return self;
            }

            self.Mutate();
            
            Hash otherHash = other as Hash;
            if (otherHash != null) {
                self.DefaultValue = otherHash.DefaultValue;
                self.DefaultProc = otherHash.DefaultProc;
            }
            return IDictionaryOps.ReplaceData(self, other);
        }

        [RubyMethod("shift")]
        public static object Shift(CallSiteStorage<Func<CallSite, Hash, object, object>>/*!*/ storage, Hash/*!*/ self) {
            self.Mutate();
            
            if (self.Count == 0) {
                var site = storage.GetCallSite("default", 1);
                return site.Target(site, self, null);
            }

            IEnumerator<KeyValuePair<object, object>> e = self.GetEnumerator();
            e.MoveNext();
            KeyValuePair<object, object> pair = e.Current;
            self.Remove(pair.Key);

            return IDictionaryOps.MakeArray(pair);
        }

        #endregion
    }

}
