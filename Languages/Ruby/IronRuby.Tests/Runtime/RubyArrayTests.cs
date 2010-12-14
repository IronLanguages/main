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
using IronRuby.Builtins;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using IronRuby.Runtime;
using Microsoft.Scripting;
using System.Runtime.CompilerServices;
using System.Collections;
using Microsoft.Scripting.Utils;

namespace IronRuby.Tests {
    public partial class Tests {
        private IEnumerable<int> Enumerable(int count) {
            for (int i = 0; i < count; i++) {
                yield return i + 1;
            }
        }

        private void AssertValueEquals(RubyArray/*!*/ array, params object[]/*!*/ expected) {
            Assert(ArrayUtils.ValueEquals(array.ToArray(), expected));
            array.RequireNullEmptySlots();
        }

        [Options(NoRuntime = true)]
        public void RubyArray_Ctors() {
            const int N = 10;
            var dict = new Dictionary<object, object>();
            for (int i = 0; i < N; i++) {
                dict.Add(i, i);
            }

            RubyArray a;

            a = RubyArray.Create(1);
            Assert(a.Count == 1 && (int)a[0] == 1);

            a = new RubyArray();
            Assert(a.Count == 0 && a.Capacity == 0);

            a = new RubyArray(1);
            Assert(a.Count == 0 && a.Capacity == Utils.MinListSize);

            a = new RubyArray(100);
            Assert(a.Count == 0 && a.Capacity == 100);
            
            a = new RubyArray((ICollection)dict.Values);
            Assert(a.Count == N);

            a = new RubyArray((IEnumerable)dict.Values);
            Assert(a.Count == N);

            a = new RubyArray((IList)new object[] { 1, 2, 3 });
            Assert(a.Count == 3);
            Assert((int)a[0] == 1 && (int)a[1] == 2 && (int)a[2] == 3);

            a = new RubyArray((IList)new object[] { 1, 2, 3 }, 1, 1);
            Assert(a.Count == 1);
            Assert((int)a[0] == 2);

            a = new RubyArray((IList)new object[] { 1, 2, 3 }, 1, 0);
            Assert(a.Count == 0);

            a = new RubyArray((ICollection)new object[] { 1, 2, 3 });
            Assert(a.Count == 3);
            Assert((int)a[0] == 1 && (int)a[1] == 2 && (int)a[2] == 3);

            a = new RubyArray((IEnumerable)new object[] { 1, 2, 3 });
            Assert(a.Count == 3);
            Assert((int)a[0] == 1 && (int)a[1] == 2 && (int)a[2] == 3);

            a = new RubyArray(Enumerable(3));
            Assert(a.Count == 3);
            Assert((int)a[0] == 1 && (int)a[1] == 2 && (int)a[2] == 3);

            a = new RubyArray(a);
            Assert(a.Count == 3);
            Assert((int)a[0] == 1 && (int)a[1] == 2 && (int)a[2] == 3);

            a = new RubyArray(a, 0, 2);
            Assert(a.Count == 2);
            Assert((int)a[0] == 1 && (int)a[1] == 2);

            // prepare array [nil, 1, 3, nil]
            RubyArray b = new RubyArray(new[] { 1, 2, 3 });
            b.RemoveAt(1);
            a = new RubyArray(b);
            Assert(a.Count == 2);
            Assert((int)a[0] == 1 && (int)a[1] == 3);

            a = new RubyArray(b, 2, 0);
            Assert(a.Count == 0);

            a = new RubyArray();
            a.AddRange(b);
            Assert(a.Count == 2);
            Assert((int)a[0] == 1 && (int)a[1] == 3);

            a = new RubyArray();
            a.AddRange(b, 1, 1);
            Assert(a.Count == 1);
            Assert((int)a[0] == 3);

            a = new RubyArray(new[] { 1, 2, 3 });
            AssertExceptionThrown<ArgumentNullException>(() => new RubyArray((RubyArray)null, 1, 2));
            AssertExceptionThrown<ArgumentOutOfRangeException>(() => new RubyArray(a, -1, 2));
            AssertExceptionThrown<ArgumentOutOfRangeException>(() => new RubyArray(a, 3, 1));
            AssertExceptionThrown<ArgumentOutOfRangeException>(() => new RubyArray(a, 0, 4));
        }

        [Options(NoRuntime = true)]
        public void RubyArray_Basic() {
            RubyArray a;

            a = new RubyArray(new object[] { 1, 2, 8 });
            Assert(!a.IsTainted && !a.IsFrozen && !a.IsReadOnly && !((IList)a).IsFixedSize && !((ICollection)a).IsSynchronized);
            Assert(ReferenceEquals(((ICollection)a).SyncRoot, a));

            a.IsTainted = true;
            Assert(a.IsTainted);
            a.IsTainted = false;
            Assert(!a.IsTainted);

            a[1] = 5;
            Assert((int)a[0] == 1 && (int)a[1] == 5 && (int)a[2] == 8);

            ((IRubyObjectState)a).Freeze();
            Assert(a.IsFrozen);

            AssertExceptionThrown<RuntimeError>(() => a.IsTainted = true);
            Assert(!a.IsTainted);

            AssertExceptionThrown<RuntimeError>(() => a[1] = 1);
            Assert((int)a[0] == 1 && (int)a[1] == 5 && (int)a[2] == 8);

            var values = new object[] { 1, 5, 8 };

            int i = 0;
            foreach (object item in a) {
                Assert(item.Equals(values[i]));
                i++;
            }
            Assert(i == values.Length);

            Assert(((IEnumerable)a).GetEnumerator() != null);

            AssertValueEquals(a, values);
            object[] array = new object[4];
            ((ICollection)a).CopyTo(array, 1);
            Assert(ArrayUtils.ValueEquals(ArrayUtils.ShiftLeft(array, 1), values));

            Assert(a.Contains(5));
            Assert(a.IndexOf(5) == 1);
            Assert(a.IndexOf(5, 2) == -1);
            Assert(a.IndexOf(5, 0, 1) == -1);

            Assert(a.FindIndex((x) => (int)x == 8) == 2);
            Assert(a.FindIndex(2, (x) => (int)x == 8) == 2);
            Assert(a.FindIndex(0, 2, (x) => (int)x == 8) == -1);
        }

        [Options(NoRuntime = true)]
        public void RubyArray_Add() {
            RubyArray a;
            a = new RubyArray();

            for (int i = 0; i < Utils.MinListSize; i++) {
                a.Add(i);
                Assert((int)a[i] == i && a.Count == i + 1 && a.Capacity == Utils.MinListSize);
            }

            Assert(((IList)a).Add(Utils.MinListSize) == Utils.MinListSize);
            Assert(a.Count == Utils.MinListSize + 1);
            for (int i = 0; i < a.Count; i++) {
                Assert((int)a[i] == i);
            }

            a = new RubyArray(new[] { 1,2,3 });
            a.AddCapacity(0);
            Assert(a.Count == 3);
            a.AddCapacity(100);
            Assert(a.Count == 3 && a.Capacity >= 103);

            a = new RubyArray(new[] { 1, 2, 3 });
            a.AddMultiple(0, 4);
            AssertValueEquals(a, 1, 2, 3);
            a.AddMultiple(5, 4);
            AssertValueEquals(a, 1, 2, 3, 4, 4, 4, 4, 4);

            a = new RubyArray(new[] { 1, 2, 3 });
            a.AddRange(new object[0]);
            AssertValueEquals(a, 1, 2, 3);
            a.AddRange(new[] { 4 });
            AssertValueEquals(a, 1, 2, 3, 4);
            a.AddRange(new[] { 5, 6, 7, 8, 9, 10 });
            AssertValueEquals(a, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
            a.AddRange(new[] { 11 });
            AssertValueEquals(a, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);

            a = new RubyArray();
            a.AddRange((IEnumerable)new RubyArray(new[] { 1, 2, 3 }));
            a.AddRange((IList)new RubyArray(new[] { 1, 2, 3 }), 1, 2);
            AssertValueEquals(a, 1, 2, 3, 2, 3);

            a.Freeze();
            AssertExceptionThrown<RuntimeError>(() => a.Add(1));
            AssertExceptionThrown<RuntimeError>(() => a.AddCapacity(10));
            AssertExceptionThrown<RuntimeError>(() => a.AddMultiple(10, 10));
            AssertExceptionThrown<RuntimeError>(() => a.AddRange(new object[0]));
            AssertExceptionThrown<RuntimeError>(() => a.AddRange(Enumerable(0)));
        }

        [Options(NoRuntime = true)]
        public void RubyArray_Remove() {
            RubyArray a;
            a = new RubyArray(new[] { 1, 2, 3, 4, 5 });

            a.RemoveAt(1);
            AssertValueEquals(a, 1, 3, 4, 5);

            a.RemoveAt(1);
            AssertValueEquals(a, 1, 4, 5);

            a.RemoveAt(1);
            AssertValueEquals(a, 1, 5);

            a.RemoveAt(1);
            AssertValueEquals(a, 1);

            a.AddRange(new[] { 2, 3 });
            AssertValueEquals(a, 1, 2, 3);

            a.RemoveAt(0);
            AssertValueEquals(a, 2, 3);

            a.RemoveAt(0);
            AssertValueEquals(a, 3);

            a.RemoveAt(0);
            AssertValueEquals(a);

            a.AddRange(new[] { 1, 2, 3 });
            AssertValueEquals(a, 1, 2, 3);

            a = new RubyArray();
            a.AddMultiple(100, 1);
            a[0] = 0;
            a[99] = 99;
            a.RemoveRange(1, 98);
            AssertValueEquals(a, 0, 99);
            Assert(a.Capacity < 100, "array should shrink");

            ((IList)a).Remove(0);
            AssertValueEquals(a, 99);
            a.Clear();
            Assert(a.Count == 0);

            a = new RubyArray(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
            a.RemoveRange(0, 4);
            AssertValueEquals(a, 4, 5, 6, 7, 8, 9, 10, 11);
            a.AddMultiple(3, 1);
            AssertValueEquals(a, 4, 5, 6, 7, 8, 9, 10, 11, 1, 1, 1);
            a.RemoveRange(0, 6);
            AssertValueEquals(a, 10, 11, 1, 1, 1);
            a.AddMultiple(2, 2);
            AssertValueEquals(a, 10, 11, 1, 1, 1, 2, 2);

            a = new RubyArray();
            a = new RubyArray(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
            a.RemoveRange(0, 4);
            AssertValueEquals(a, 4, 5, 6, 7, 8, 9, 10, 11);
            a.AddMultiple(3, null);
            AssertValueEquals(a, 4, 5, 6, 7, 8, 9, 10, 11, null, null, null);

            var vector = new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            a = new RubyArray(vector);
            for (int i = 0, c = a.Count; i < c; i++) {
                a.RemoveAt(0);
                vector = ArrayUtils.ShiftLeft(vector, 1);
                AssertValueEquals(a, vector);
            }
            
            a = new RubyArray(new[] { 1, 2, 3 });
            a.Freeze();
            Assert(!a.Remove(0));
            AssertExceptionThrown<RuntimeError>(() => a.Remove(1));
            AssertExceptionThrown<RuntimeError>(() => a.Clear());
            AssertExceptionThrown<RuntimeError>(() => a.RemoveAt(0));
            AssertExceptionThrown<RuntimeError>(() => a.RemoveRange(0, 1));
        }

        [Options(NoRuntime = true)]
        public void RubyArray_Insert() {
            RubyArray a;
            a = new RubyArray(new[] { 1, 2, 3, 4, 5 });
            a.InsertRange(0, new[] { 10, 20, 30, 40 });
            AssertValueEquals(a, 10, 20, 30, 40, 1, 2, 3, 4, 5);
            a.InsertRange(2, new[] { 8 });
            AssertValueEquals(a,10, 20, 8, 30, 40, 1, 2, 3, 4, 5);
            a.InsertRange(10, new[] { 9 });
            AssertValueEquals(a, 10, 20, 8, 30, 40, 1, 2, 3, 4, 5, 9);

            a.RemoveRange(1, 5);
            AssertValueEquals(a, 10, 2, 3, 4, 5, 9);
            a.InsertRange(1, new[] { 0 });
            AssertValueEquals(a, 10, 0, 2, 3, 4, 5, 9);
            a.InsertRange(6, new[] { 8 });
            AssertValueEquals(a, 10, 0, 2, 3, 4, 5, 8, 9);
            a.RemoveRange(6, 2);
            AssertValueEquals(a, 10, 0, 2, 3, 4, 5);
            a.InsertRange(3, new[] { 11, 12, 13, 14 });
            AssertValueEquals(a, 10, 0, 2, 11, 12, 13, 14, 3, 4, 5);

            a = new RubyArray(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            // make enough space on both sides:
            a.RemoveRange(0, 2);
            AssertValueEquals(a, 2, 3, 4, 5, 6, 7, 8, 9);
            a.RemoveRange(6, 2);
            AssertValueEquals(a, 2, 3, 4, 5, 6, 7);

            // closer to start:
            a.InsertRange(1, new[] { 10 });
            AssertValueEquals(a, 2, 10, 3, 4, 5, 6, 7);

            // closer to end:
            a.InsertRange(5, new[] { 11 });
            AssertValueEquals(a, 2, 10, 3, 4, 5, 11, 6, 7);

            a = new RubyArray(new[] { 1, 2, 3 });
            a.Insert(0, 0);
            AssertValueEquals(a, 0, 1, 2, 3);
            a.InsertRange(0, Enumerable(2));
            AssertValueEquals(a, 1, 2, 0, 1, 2, 3);
            a.InsertRange(0, new RubyArray(new[] { 1, 0 }), 1, 1);
            a.InsertRange(0, new object[] { 0, -1 }, 1, 1);
            a.InsertRange(0, new[] { 0, -2 }, 1, 1);
            AssertValueEquals(a, -2, -1, 0, 1, 2, 0, 1, 2, 3);
            a.InsertRange(0, (IEnumerable)new RubyArray(new[] { 30 }));
            a.InsertRange(0, (IEnumerable)new object[] { 20 });
            a.InsertRange(0, (IEnumerable)new[] { 10 });
            AssertValueEquals(a, 10, 20, 30, -2, -1, 0, 1, 2, 0, 1, 2, 3);
            
            a.Freeze();
            AssertExceptionThrown<RuntimeError>(() => a.Insert(0, 1));
            AssertExceptionThrown<RuntimeError>(() => a.InsertRange(0, new object[0]));
            AssertExceptionThrown<RuntimeError>(() => a.InsertRange(0, Enumerable(0)));
        }

        [Options(NoRuntime = true)]
        public void RubyArray_Misc() {
            RubyArray a;

            a = new RubyArray(new[] { 3, 5, 2, 4, 1 });
            a.InsertRange(0, new[] {10, 20});
            a.RemoveRange(0, 2);

            a.Sort();
            Assert(ArrayUtils.ValueEquals(a.ToArray(), new object[] { 1, 2, 3, 4, 5 }));
            a.Sort((x, y) => (int)x == (int)y ? 0 : ((int)x < (int)y ? 1 : -1));
            Assert(ArrayUtils.ValueEquals(a.ToArray(), new object[] { 5, 4, 3, 2, 1 }));
            a.Reverse();
            Assert(ArrayUtils.ValueEquals(a.ToArray(), new object[] { 1, 2, 3, 4, 5 }));

            a.Freeze();
            AssertExceptionThrown<RuntimeError>(() => a.Reverse());
            AssertExceptionThrown<RuntimeError>(() => a.Sort());
        }

        [Options(NoRuntime = true)]
        public void RubyArray_Indexer() {
            RubyArray a;

            a = new RubyArray();
            a.Add(0);
            Assert(ArrayUtils.ValueEquals(a.ToArray(), new object[] { 0 }));
            a[1] = 1;
            Assert(ArrayUtils.ValueEquals(a.ToArray(), new object[] { 0, 1 }));
            a[4] = 4;
            Assert(ArrayUtils.ValueEquals(a.ToArray(), new object[] { 0, 1, null, null, 4 }));
            a[6] = 6;
            Assert(ArrayUtils.ValueEquals(a.ToArray(), new object[] { 0, 1, null, null, 4, null, 6 }));

            a = new RubyArray(new object[] { null, null, 2, 3}, 2, 2);
            Assert(ArrayUtils.ValueEquals(a.ToArray(), new object[] { 2, 3 }));
            a[3] = 4;
            Assert(ArrayUtils.ValueEquals(a.ToArray(), new object[] { 2, 3, null, 4 }));

            a = new RubyArray(new object[] { null, null, 2, 3 }, 2, 2);
            Assert((int)a[0] == 2);
            object x;
            AssertExceptionThrown<IndexOutOfRangeException>(() => x = a[2]);
        }
    }
}
