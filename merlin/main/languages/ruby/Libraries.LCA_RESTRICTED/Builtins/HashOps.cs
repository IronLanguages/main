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

namespace IronRuby.Builtins {

    /// <summary>
    /// Dictionary inherits from Object, mixes in Enumerable.
    /// Ruby hash is a Dictionary{object, object}, but it adds default value/proc
    /// 
    /// TODO: Not all .NET types implement the right Equals, GetHashCode semantics (e.g. List{object})
    /// </summary>
    [RubyClass("Hash", Extends = typeof(Hash), Inherits = typeof(object)), Includes(typeof(IDictionary<object, object>), Copy = true)]
    public static class HashOps {

        [RubyConstructor]
        public static Hash/*!*/ Hash(RubyClass/*!*/ self) {
            return new Hash(self.Context.EqualityComparer);
        }

        [RubyConstructor]
        public static Hash/*!*/ Hash(BlockParam block, RubyClass/*!*/ self, object defaultValue) {
            if (block != null) {
                throw RubyExceptions.CreateArgumentError("wrong number of arguments");
            }
            return new Hash(self.Context.EqualityComparer, null, defaultValue);
        }

        [RubyConstructor]
        public static Hash/*!*/ Hash([NotNull]BlockParam/*!*/ defaultProc, RubyClass/*!*/ self) {
            return new Hash(self.Context.EqualityComparer, defaultProc.Proc, null);
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
            RubyUtils.RequiresNotFrozen(context, self);

            self.DefaultProc = source.DefaultProc;
            self.DefaultValue = source.DefaultValue;
            IDictionaryOps.ReplaceData(self, source);
            return self;
        }

        #region Singleton Methods

        private static readonly CallSite<Func<CallSite, RubyContext, RubyClass, Hash>>/*!*/
            _CreateHashSite = CallSite<Func<CallSite, RubyContext, RubyClass, Hash>>.Create(RubySites.InstanceCallAction("new"));

        [RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
        public static Hash/*!*/ CreateHash(RubyClass/*!*/ self, [NotNull]params object[] items) {
            // arg0: hash or first elem
            // argk: (k-1)th elem
            int itemCount = items.Length;

            IDictionary<object, object> hash = null;
            if (itemCount == 0 || itemCount == 1 && (hash = items[0] as IDictionary<object, object>) != null) {
                Hash newHash = _CreateHashSite.Target(_CreateHashSite, self.Context, self);
                return hash != null ? IDictionaryOps.ReplaceData(newHash, hash) : newHash;
            }

            if (itemCount % 2 != 0) {
                throw new ArgumentException("odd number of arguments for Hash");
            }

            return RubyUtils.MakeHash(self.Context, items);
        }

        #endregion


        #region Instance Methods
        
        private static readonly CallSite<Func<CallSite, RubyContext, Proc, Hash, object, object>>/*!*/ _DefaultProcSite =
            CallSite<Func<CallSite, RubyContext, Proc, Hash, object, object>>.Create(RubySites.InstanceCallAction("call", 2));

        private static readonly CallSite<Func<CallSite, RubyContext, Hash, object, object>>/*!*/ _DefaultSite =
            CallSite<Func<CallSite, RubyContext, Hash, object, object>>.Create(RubySites.InstanceCallAction("default", 1));

        [RubyMethod("[]")]
        public static object GetElement(RubyContext/*!*/ context, Hash/*!*/ self, object key) {
            object result;
            if (!self.TryGetValue(BaseSymbolDictionary.NullToObj(key), out result)) {
                return _DefaultSite.Target(_DefaultSite, context, self, key);
            }
            return result;
        }

        [RubyMethod("default")]
        public static object GetDefaultValue(RubyContext/*!*/ context, Hash/*!*/ self) {
            return self.DefaultValue;
        }

        [RubyMethod("default")]
        public static object GetDefaultValue(RubyContext/*!*/ context, Hash/*!*/ self, object key) {
            if (self.DefaultProc != null) {
                return _DefaultProcSite.Target(_DefaultProcSite, context, self.DefaultProc, self, key);
            }
            return self.DefaultValue;
        }

        [RubyMethod("default=")]
        public static object SetDefaultValue(RubyContext/*!*/ context, Hash/*!*/ self, object value) {
            RubyUtils.RequiresNotFrozen(context, self);
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
                    return MutableString.Create("{...}");
                }

                MutableString str = MutableString.Create("{");
                bool first = true;
                foreach (KeyValuePair<object, object> pair in self) {
                    if (first) {
                        first = false;
                    } else {
                        str.Append(", ");
                    }
                    str.Append(RubySites.Inspect(context, BaseSymbolDictionary.ObjToNull(pair.Key)));
                    str.Append("=>");
                    str.Append(RubySites.Inspect(context, pair.Value));
                }
                str.Append('}');
                return str;
            }
        }

        [RubyMethod("replace")]
        public static Hash/*!*/ Replace(RubyContext/*!*/ context, Hash/*!*/ self, object other) {
            if (Object.ReferenceEquals(self, other))
                return self;

            RubyUtils.RequiresNotFrozen(context, self);

            // If we are copying from another Hash, copy the default value/block, otherwise set to nil
            Hash otherHash = other as Hash;
            self.DefaultValue = (otherHash != null) ? otherHash.DefaultValue : null;
            self.DefaultProc = (otherHash != null) ? otherHash.DefaultProc : null;
            return IDictionaryOps.ReplaceData(self, IDictionaryOps.ConvertToHash(context, other));
        }

        [RubyMethod("shift")]
        public static object Shift(RubyContext/*!*/ context, Hash/*!*/ self) {
            RubyUtils.RequiresNotFrozen(context, self);

            if (self.Count == 0) {
                return _DefaultSite.Target(_DefaultSite, context, self, null);
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
