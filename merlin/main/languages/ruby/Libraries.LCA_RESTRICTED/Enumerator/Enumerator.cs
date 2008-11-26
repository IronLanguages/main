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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using EnumerableModule = IronRuby.Builtins.Enumerable;

namespace IronRuby.StandardLibrary.Enumerator {

    [RubyModule(Extends = typeof(EnumerableModule))]
    public static class Enumerable {
        
        // TODO: shouldn't be abstract class
        [RubyClass("Enumerator"), Includes(typeof(EnumerableModule))]
        public abstract class Enumerator {

            private static readonly Dictionary<string/*!*/, CallSite<Func<CallSite, RubyContext, object, Proc, object>>/*!*/> _siteCache =
                new Dictionary<string/*!*/, CallSite<Func<CallSite, RubyContext, object, Proc, object>>/*!*/>();

            private readonly object/*!*/ _obj;

            protected Enumerator(object/*!*/ obj) {
                _obj = obj;
            }

            protected CallSite<Func<CallSite, RubyContext, object, Proc, object>>/*!*/ GetSite(string/*!*/ name) {
                CallSite<Func<CallSite, RubyContext, object, Proc, object>> result;
                lock (_siteCache) {
                    if (!_siteCache.TryGetValue(name, out result)) {
                        result = CallSite<Func<CallSite, RubyContext, object, Proc, object>>.Create(RubySites.InstanceCallAction(name, RubyCallSignature.WithBlock(0)));
                        _siteCache[name] = result;
                    }
                }
                return result;
            }

            internal abstract CallSite<Func<CallSite, RubyContext, object, Proc, object>> GetSite();

            internal object Each(RubyContext/*!*/ context, Proc/*!*/ block) {
                var site = GetSite();
                return site.Target(site, context, _obj, block);
            }

            [RubyConstructor]
            public static Enumerator CreateForEach(RubyClass/*!*/ self, object/*!*/ obj) {
                return new EnumeratorWithSymbolName(obj, SymbolTable.StringToId("each"));
            }

            [RubyConstructor]
            public static Enumerator Create(RubyClass/*!*/ self, object/*!*/ obj, SymbolId enumerator) {
                return new EnumeratorWithSymbolName(obj, enumerator);
            }

            [RubyConstructor]
            public static Enumerator Create(RubyClass/*!*/ self, object/*!*/ obj, [DefaultProtocol, NotNull]MutableString/*!*/ enumerator) {
                return new EnumeratorWithStringName(obj, enumerator);
            }
        }

        internal class EnumeratorWithStringName : Enumerator {
            private readonly MutableString/*!*/ _name;

            internal override CallSite<Func<CallSite, RubyContext, object, Proc, object>>/*!*/ GetSite() {
                return GetSite(_name.ConvertToString());
            }

            internal EnumeratorWithStringName(object/*!*/ obj, MutableString/*!*/ name)
                : base(obj) {
                _name = name;
            }
        }
        internal class EnumeratorWithSymbolName : Enumerator {
            private readonly SymbolId _name;

            internal override CallSite<Func<CallSite, RubyContext, object, Proc, object>>/*!*/ GetSite() {
                return GetSite(SymbolTable.IdToString(_name));
            }

            internal EnumeratorWithSymbolName(object/*!*/ obj, SymbolId name)
                : base(obj) {
                _name = name;
            }
        }
    }
}
