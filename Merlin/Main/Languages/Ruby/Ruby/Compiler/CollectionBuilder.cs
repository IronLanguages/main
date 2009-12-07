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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;

namespace IronRuby.Compiler {
    internal abstract class CollectionBuilder<TElement> : MutableTuple<TElement, TElement, TElement, TElement> {
        private int _count;
        private ReadOnlyCollectionBuilder<TElement> _items;

        public CollectionBuilder() {
        }

        public int Count {
            get { return _count; }
        }

        public ReadOnlyCollectionBuilder<TElement> Items {
            get { return _items; }
        }

        public void Add(TElement expression) {
            if (expression == null) {
                return;
            }

            switch (_count) {
                case 0: Item000 = expression; break;
                case 1: Item001 = expression; break;
                case 2: Item002 = expression; break;
                case 3: Item003 = expression; break;
                case 4:
                    _items = new ReadOnlyCollectionBuilder<TElement>();
                    _items.Add(Item000);
                    _items.Add(Item001);
                    _items.Add(Item002);
                    _items.Add(Item003);
                    _items.Add(expression);
                    break;

                default:
                    Debug.Assert(_items != null);
                    _items.Add(expression);
                    break;
            }

            _count++;
        }
    }
}
