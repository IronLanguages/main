/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace IronPython.Runtime {
    [PythonType("bytearray")]
    public class ByteArray : IList<byte>, ICodeFormattable, IValueEquality {
        private List<byte>/*!*/ _bytes;

        public ByteArray() {
            _bytes = new List<byte>(0);
        }

        internal ByteArray(List<byte> bytes) {
            _bytes = bytes;
        }

        public void __init__() {
            _bytes = new List<byte>();
        }

        public void __init__(IList<byte>/*!*/ bytes) {
            _bytes = new List<byte>(bytes);
        }

        public void __init__(CodeContext/*!*/ context, string unicode, string encoding) {
            _bytes = new List<byte>(StringOps.ToByteArray(StringOps.encode(context, unicode, encoding, "strict")));
        }

        #region Public Mutable Sequence API
        
        public void append(int item) {
            lock (this) {
                _bytes.Add(item.ToByteChecked());
            }
        }

        public void extend(IList<byte>/*!*/ seq) {
            using (new OrderedLocker(this, seq)) {
                // use the original count for if we're extending this w/ this
                _bytes.AddRange(seq);
            }
        }

        public void insert(int index, int value) {
            lock (this) {
                if (index >= Count) {
                    append(value);
                    return;
                }

                index = PythonOps.FixSliceIndex(index, Count);

                _bytes.Insert(index, value.ToByteChecked());
            }
        }

        public int pop() {
            lock (this) {
                if (Count == 0) {
                    throw PythonOps.OverflowError("pop off of empty bytearray");
                }

                int res = _bytes[_bytes.Count - 1];
                _bytes.RemoveAt(_bytes.Count - 1);
                return res;
            }
        }

        public int pop(int index) {
            lock (this) {
                if (Count == 0) {
                    throw PythonOps.OverflowError("pop off of empty bytearray");
                }

                index = PythonOps.FixIndex(index, Count);

                int ret = _bytes[index];
                _bytes.RemoveAt(index);
                return ret;
            }
        }

        public void remove(int value) {
            lock (this) {
                _bytes.RemoveAt(_bytes.IndexOfByte(value.ToByteChecked(), 0, _bytes.Count));
            }
        }

        public void reverse() {
            lock (this) {
                List<byte> reversed = new List<byte>();
                for (int i = _bytes.Count - 1; i >= 0; i--) {
                    reversed.Add(_bytes[i]);
                }
                _bytes = reversed;
            }
        }

        [SpecialName]
        public static ByteArray InPlaceAdd(ByteArray self, ByteArray other) {
            using (new OrderedLocker(self, other)) {
                self._bytes.AddRange(other._bytes);
                return self;
            }
        }

        [SpecialName]
        public static ByteArray InPlaceMultiply(ByteArray self, int len) {
            lock (self) {
                self._bytes = (self * len)._bytes;
                return self;
            }
        }

        #endregion

        #region Public Python API surface

        public ByteArray/*!*/ capitalize() {
            lock (this) {
                return new ByteArray(_bytes.Capitalize());
            }
        }

        public ByteArray/*!*/ center(int width) {
            return center(width, " ");
        }

        public ByteArray/*!*/ center(int width, [NotNull]string fillchar) {
            lock (this) {                
                List<byte> res = _bytes.TryCenter(width, fillchar.ToByte("center", 2));
                
                if (res == null) {
                    return CopyThis();
                }

                return new ByteArray(res);
            }
        }

        public ByteArray/*!*/ center(int width, IList<byte> fillchar) {
            lock (this) {
                List<byte> res = _bytes.TryCenter(width, fillchar.ToByte("center", 2));

                if (res == null) {
                    return CopyThis();
                }

                return new ByteArray(res);
            }
        }

        public int count(IList<byte>/*!*/ sub) {
            return count(sub, 0, _bytes.Count);
        }

        public int count(IList<byte>/*!*/ sub, int start) {
            return count(sub, start, _bytes.Count);
        }

        public int count(IList<byte>/*!*/ ssub, int start, int end) {
            lock (this) {
                IList<byte> bytes = _bytes;

                return _bytes.CountOf(ssub, start, end);
            }
        }

        public string decode(CodeContext/*!*/ context, [Optional]string encoding, [DefaultParameterValue("strict")]string errors) {
            return StringOps.decode(context, StringOps.FromByteArray(_bytes), encoding, errors);
        }

        public bool endswith(IList<byte>/*!*/ suffix) {
            lock (this) {
                return _bytes.EndsWith(suffix);
            }
        }

        public bool endswith(IList<byte>/*!*/ suffix, int start) {
            lock (this) {
                return _bytes.EndsWith(suffix, start);
            }
        }

        public bool endswith(IList<byte>/*!*/ suffix, int start, int end) {
            lock (this) {
                return _bytes.EndsWith(suffix, start, end);
            }
        }

        public bool endswith(PythonTuple/*!*/ suffix) {
            lock (this) {
                return _bytes.EndsWith(suffix);
            }
        }

        public bool endswith(PythonTuple/*!*/ suffix, int start) {
            lock (this) {
                return _bytes.EndsWith(suffix, start);
            }
        }

        public bool endswith(PythonTuple/*!*/ suffix, int start, int end) {
            lock (this) {
                return _bytes.EndsWith(suffix, start, end);
            }
        }

        public ByteArray/*!*/ expandtabs() {
            return expandtabs(8);
        }

        public ByteArray/*!*/ expandtabs(int tabsize) {
            lock (this) {
                return new ByteArray(_bytes.ExpandTabs(tabsize));
            }
        }

        public int find(IList<byte>/*!*/ sub) {
            lock (this) {
                return _bytes.Find(sub);
            }
        }

        public int find(IList<byte>/*!*/ sub, int start) {
            lock (this) {
                return _bytes.Find(sub, start);
            }
        }

        public int find(IList<byte>/*!*/ sub, int start, int end) {
            lock (this) {
                return _bytes.Find(sub, start, end);
            }
        }

        public static ByteArray/*!*/ fromhex(string/*!*/ @string) {
            return new ByteArray(IListOfByteOps.FromHex(@string));
        }

        public int index(IList<byte>/*!*/ item) {
            return index(item, 0, _bytes.Count);
        }

        public int index(IList<byte>/*!*/ item, int start) {
            return index(item, start, _bytes.Count);
        }

        public int index(IList<byte>/*!*/ item, int start, int stop) {
            lock (this) {
                return _bytes.IndexOfByte(item.ToByte("index", 1), start, stop);
            }
        }

        public bool isalnum() {
            lock (this) {
                return _bytes.IsAlphaNumeric();
            }
        }

        public bool isalpha() {
            lock (this) {
                return _bytes.IsLetter();
            }
        }

        public bool isdigit() {
            lock (this) {
                return _bytes.IsDigit();
            }
        }

        public bool islower() {
            lock (this) {
                return _bytes.IsLower();
            }
        }

        public bool isspace() {
            lock (this) {
                return _bytes.IsWhiteSpace();
            }
        }

        /// <summary>
        /// return true if self is a titlecased string and there is at least one
        /// character in self; also, uppercase characters may only follow uncased
        /// characters (e.g. whitespace) and lowercase characters only cased ones. 
        /// return false otherwise.
        /// </summary>
        public bool istitle() {
            lock (this) {
                return _bytes.IsTitle();
            }
        }

        public bool isupper() {
            lock (this) {
                return _bytes.IsUpper();
            }
        }

        /// <summary>
        /// Return a string which is the concatenation of the strings 
        /// in the sequence seq. The separator between elements is the 
        /// string providing this method
        /// </summary>
        public ByteArray/*!*/ join(object/*!*/ sequence) {
            IEnumerator seq = PythonOps.GetEnumerator(sequence);            
            if (!seq.MoveNext()) {
                return new ByteArray();
            }

            // check if we have just a sequnce of just one value - if so just
            // return that value.
            object curVal = seq.Current;
            if (!seq.MoveNext()) {
                return JoinOne(curVal);
            }

            List<byte> ret = new List<byte>();
            ByteOps.AppendJoin(curVal, 0, ret);

            int index = 1;
            do {
                ret.AddRange(this);

                ByteOps.AppendJoin(seq.Current, index, ret);

                index++;
            } while (seq.MoveNext());

            return new ByteArray(ret);
        }

        public ByteArray/*!*/ join([NotNull]List/*!*/ sequence) {
            if (sequence.__len__() == 0) {
                return new ByteArray();
            }

            lock (this) {
                if (sequence.__len__() == 1) {
                    return JoinOne(sequence[0]);
                }

                List<byte> ret = new List<byte>();
                ByteOps.AppendJoin(sequence._data[0], 0, ret);
                for (int i = 1; i < sequence._size; i++) {
                    ret.AddRange(this);
                    ByteOps.AppendJoin(sequence._data[i], i, ret);
                }

                return new ByteArray(ret);
            }
        }

        public ByteArray/*!*/ ljust(int width) {
            return ljust(width, (byte)' ');
        }

        public ByteArray/*!*/ ljust(int width, [NotNull]string/*!*/ fillchar) {
            return ljust(width, fillchar.ToByte("ljust", 2));
        }

        public ByteArray/*!*/ ljust(int width, IList<byte>/*!*/ fillchar) {
            return ljust(width, fillchar.ToByte("ljust", 2));
        }

        private ByteArray/*!*/ ljust(int width, byte fillchar) {
            lock (this) {
                int spaces = width - _bytes.Count;

                List<byte> ret = new List<byte>(width);
                ret.AddRange(_bytes);
                for (int i = 0; i < spaces; i++) {
                    ret.Add(fillchar);
                }
                return new ByteArray(ret);
            }
        }

        public ByteArray/*!*/ lower() {
            lock (this) {
                return new ByteArray(_bytes.ToLower());
            }
        }

        public ByteArray/*!*/ lstrip() {
            lock (this) {
                List<byte> res = _bytes.LeftStrip();
                if (res == null) {
                    return CopyThis();
                }

                return new ByteArray(res);
            }
        }

        public PythonTuple/*!*/ partition(IList<byte>/*!*/ sep) {
            if (sep == null) {
                throw PythonOps.TypeError("expected string, got NoneType");
            } else if (sep.Count == 0) {
                throw PythonOps.ValueError("empty separator");
            }

            object[] obj = new object[3] { new ByteArray(), new ByteArray(), new ByteArray() };

            if (_bytes.Count != 0) {
                int index = find(sep);
                if (index == -1) {
                    obj[0] = this;
                } else {
                    obj[0] = new ByteArray(_bytes.Substring(0, index));
                    obj[1] = sep;
                    obj[2] = new ByteArray(_bytes.Substring(index + sep.Count, _bytes.Count - index - sep.Count));
                }
            }

            return new PythonTuple(obj);
        }

        public ByteArray/*!*/ replace(IList<byte>/*!*/ old, IList<byte> new_) {
            if (old == null) {
                throw PythonOps.TypeError("expected bytes or bytearray, got NoneType");
            }

            return replace(old, new_, old.Count + 1);
        }

        public ByteArray/*!*/ replace(IList<byte>/*!*/ old, IList<byte>/*!*/ new_, int maxsplit) {
            if (old == null) {
                throw PythonOps.TypeError("expected bytes or bytearray, got NoneType");
            } else if (maxsplit == 0) {
                return CopyThis();
            }

            return new ByteArray(_bytes.Replace(old, new_, maxsplit));
        }


        public int rfind(IList<byte>/*!*/ sub) {
            return rfind(sub, 0, _bytes.Count);
        }

        public int rfind(IList<byte>/*!*/ sub, int start) {
            return rfind(sub, start, _bytes.Count);
        }

        public int rfind(IList<byte>/*!*/ sub, int start, int end) {
            lock (this) {
                return _bytes.ReverseFind(sub, start, end);
            }
        }

        public int rindex(IList<byte>/*!*/ sub) {
            return rindex(sub, 0, _bytes.Count);
        }

        public int rindex(IList<byte>/*!*/ sub, int start) {
            return rindex(sub, start, _bytes.Count);
        }

        public int rindex(IList<byte>/*!*/ sub, int start, int end) {
            int ret = rfind(sub, start, end);
            
            if (ret == -1) {
                throw PythonOps.ValueError("substring {0} not found in {1}", sub, this);
            }

            return ret;
        }

        public ByteArray/*!*/ rjust(int width) {
            return rjust(width, (byte)' ');
        }

        public ByteArray/*!*/ rjust(int width, [NotNull]string/*!*/ fillchar) {
            return rjust(width, fillchar.ToByte("rjust", 2));
        }

        public ByteArray/*!*/ rjust(int width, IList<byte>/*!*/ fillchar) {
            return rjust(width, fillchar.ToByte("rjust", 2));
        }

        private ByteArray/*!*/ rjust(int width, int fillchar) {
            byte fill = fillchar.ToByteChecked();

            lock (this) {
                int spaces = width - _bytes.Count;
                if (spaces <= 0) {
                    return CopyThis();
                }

                List<byte> ret = new List<byte>(width);
                for (int i = 0; i < spaces; i++) {
                    ret.Add(fill);
                }
                ret.AddRange(_bytes);
                return new ByteArray(ret);
            }
        }

        public PythonTuple/*!*/ rpartition(IList<byte>/*!*/ sep) {
            if (sep == null) {
                throw PythonOps.TypeError("expected string, got NoneType");
            } else if (sep.Count == 0) {
                throw PythonOps.ValueError("empty separator");
            }

            lock (this) {
                object[] obj = new object[3] { new ByteArray(), new ByteArray(), new ByteArray() };
                if (_bytes.Count != 0) {
                    int index = rfind(sep);
                    if (index == -1) {
                        obj[2] = this;
                    } else {
                        obj[0] = new ByteArray(_bytes.Substring(0, index));
                        obj[1] = sep;
                        obj[2] = new ByteArray(_bytes.Substring(index + sep.Count, Count - index - sep.Count));
                    }
                }
                return new PythonTuple(obj);
            }
        }

        public List/*!*/ rsplit() {
            lock (this) {
                return _bytes.SplitInternal((byte[])null, -1, x => new ByteArray(x));
            }
        }

        public List/*!*/ rsplit(IList<byte>/*!*/ sep) {
            return rsplit(sep, -1);
        }

        public List/*!*/ rsplit(IList<byte>/*!*/ sep, int maxsplit) {
            return _bytes.RightSplit(sep, maxsplit, x => new ByteArray(new List<byte>(x)));
        }

        public ByteArray rstrip() {
            lock (this) {
                List<byte> res = _bytes.RightStrip();
                if (res == null) {
                    return CopyThis();
                }

                return new ByteArray(res);
            }
        }

        public List/*!*/ split() {
            lock (this) {
                return _bytes.SplitInternal((byte[])null, -1, x => new ByteArray(x));
            }
        }

        public List/*!*/ split(IList<byte> sep) {
            return split(sep, -1);
        }

        public List/*!*/ split(IList<byte> sep, int maxsplit) {
            lock (this) {
                return _bytes.Split(sep, maxsplit, x => new ByteArray(x));
            }
        }

        public List/*!*/ splitlines() {
            return splitlines(false);
        }

        public List/*!*/ splitlines(bool keepends) {
            lock (this) {
                return _bytes.SplitLines(keepends, x => new ByteArray(x));
            }
        }

        public bool startswith(IList<byte>/*!*/ prefix) {
            lock (this) {
                return _bytes.StartsWith(prefix);
            }
        }

        public bool startswith(IList<byte>/*!*/ prefix, int start) {
            lock (this) {
                int len = Count;
                if (start > len) {
                    return false;
                } else if (start < 0) {
                    start += len;
                    if (start < 0) start = 0;
                }
                return _bytes.Substring(start).StartsWith(prefix);
            }
        }

        public bool startswith(IList<byte>/*!*/ prefix, int start, int end) {
            lock (this) {
                return _bytes.StartsWith(prefix, start, end);
            }
        }

        public bool startswith(PythonTuple/*!*/ prefix) {
            lock (this) {
                return _bytes.StartsWith(prefix);
            }
        }

        public bool startswith(PythonTuple/*!*/ prefix, int start) {
            lock (this) {
                return _bytes.StartsWith(prefix, start);
            }
        }

        public bool startswith(PythonTuple/*!*/ prefix, int start, int end) {
            lock (this) {
                return _bytes.StartsWith(prefix, start, end);
            }
        }

        public ByteArray/*!*/ strip() {
            lock (this) {
                List<byte> res = _bytes.Strip();
                if (res == null) {
                    return CopyThis();
                }

                return new ByteArray(res);
            }
        }

        public ByteArray/*!*/ swapcase() {
            lock (this) {
                return new ByteArray(_bytes.SwapCase());
            }
        }

        public ByteArray/*!*/ title() {
            lock (this) {
                List<byte> res = _bytes.Title();

                if (res == null) {
                    return CopyThis();
                }

                return new ByteArray(res);
            }
        }

        public ByteArray/*!*/ translate(IList<byte>/*!*/ table) {
            if (table == null) {
                throw PythonOps.TypeError("expected bytearray or bytes, got NoneType");
            }

            lock (this) {
                if (table.Count != 256) {
                    throw PythonOps.ValueError("translation table must be 256 characters long");
                } else if (Count == 0) {
                    return CopyThis();
                }

                return new ByteArray(_bytes.Translate(table, null));
            }
        }

        public ByteArray/*!*/ translate(IList<byte>/*!*/ table, IList<byte>/*!*/ deletechars) {
            if (table == null) {
                throw PythonOps.TypeError("expected bytearray or bytes, got NoneType");
            } else if (deletechars == null) {
                throw PythonOps.TypeError("expected bytes or bytearray, got None");
            }
            
            lock (this) {
                return new ByteArray(_bytes.Translate(table, deletechars));
            }
        }

        public ByteArray/*!*/ upper() {
            lock (this) {
                return new ByteArray(_bytes.ToUpper());
            }
        }

        public ByteArray/*!*/ zfill(int width) {
            lock (this) {
                int spaces = width - Count;
                if (spaces <= 0) {
                    return CopyThis();
                }

                return new ByteArray(_bytes.ZeroFill(width, spaces));
            }
        }

        public virtual string/*!*/ __repr__(CodeContext/*!*/ context) {
            lock (this) {
                return "bytearray(" + _bytes.BytesRepr() + ")";
            }
        }

        public static ByteArray operator +(ByteArray self, ByteArray other) {
            if (self == null) {
                throw PythonOps.TypeError("expected ByteArray, got None");
            }
            
            List<byte> bytes;

            lock (self) {
                bytes = new List<byte>(self._bytes);
            }
            lock (other) {
                bytes.AddRange(other._bytes);
            }

            return new ByteArray(bytes);
        }

        public static ByteArray operator +(ByteArray self, Bytes other) {
            List<byte> bytes;

            lock (self) {
                bytes = new List<byte>(self._bytes);
            }

            bytes.AddRange(other);

            return new ByteArray(bytes);
        }

        public static ByteArray operator *(ByteArray x, int y) {
            lock (x) {
                if (y == 1) {
                    return x.CopyThis();
                }

                return new ByteArray(x._bytes.Multiply(y));
            }
        }

        public static ByteArray operator *(int x, ByteArray y) {
            return y * x;
        }

        public static bool operator >(ByteArray/*!*/ x, ByteArray y) {
            if (y == null) {
                return true;
            }

            using (new OrderedLocker(x, y)) {
                return x._bytes.Compare(y._bytes) > 0;
            }
        }

        public static bool operator <(ByteArray/*!*/ x, ByteArray y) {
            if (y == null) {
                return false;
            }

            using (new OrderedLocker(x, y)) {
                return x._bytes.Compare(y._bytes) < 0;
            }
        }

        public static bool operator >=(ByteArray/*!*/ x, ByteArray y) {
            if (y == null) {
                return true;
            }
            using (new OrderedLocker(x, y)) {
                return x._bytes.Compare(y._bytes) >= 0;
            }
        }

        public static bool operator <=(ByteArray/*!*/ x, ByteArray y) {
            if (y == null) {
                return false;
            }
            using (new OrderedLocker(x, y)) {
                return x._bytes.Compare(y._bytes) <= 0;
            }
        }

        public static bool operator >(ByteArray/*!*/ x, Bytes y) {
            if (y == null) {
                return true;
            }
            lock (x) {
                return x._bytes.Compare(y) > 0;
            }
        }

        public static bool operator <(ByteArray/*!*/ x, Bytes y) {
            if (y == null) {
                return false;
            }
            lock (x) {
                return x._bytes.Compare(y) < 0;
            }
        }

        public static bool operator >=(ByteArray/*!*/ x, Bytes y) {
            if (y == null) {
                return true;
            }
            lock (x) {
                return x._bytes.Compare(y) >= 0;
            }
        }

        public static bool operator <=(ByteArray/*!*/ x, Bytes y) {
            if (y == null) {
                return false;
            }
            lock (x) {
                return x._bytes.Compare(y) <= 0;
            }
        }

        public int this[int index] {
            get {
                lock (this) {
                    return (int)_bytes[index];
                }
            }
            set {
                lock (this) {
                    _bytes[index] = value.ToByteChecked();
                }
            }
        }

        public IList<byte> this[Slice/*!*/ slice] {
            get {
                lock (this) {
                    List<byte> res = _bytes.Slice(slice);
                    if (res == null) {
                        return new ByteArray();
                    }

                    return new ByteArray(res);
                }
            }
            set {
                if (slice == null) {
                    throw PythonOps.TypeError("bytearray indices must be integer or slice, not None");
                }

                lock (this) {
                    if (slice.step != null) {
                        // try to assign back to self: make a copy first
                        if (this == value) {
                            value = CopyThis();
                        } else if (value.Count == 0) {
                            DeleteItem(slice);
                            return;
                        }

                        IList<byte> castedVal = GetBytes(value);

                        int start, stop, step;
                        slice.indices(_bytes.Count, out start, out stop, out step);

                        int n = (step > 0 ? (stop - start + step - 1) : (stop - start + step + 1)) / step;
                        
                        // we don't use slice.Assign* helpers here because bytearray has different assignment semantics.

                        if (value.Count < n) {
                            throw PythonOps.ValueError("too few items in the enumerator. need {0} have {1}", n, castedVal.Count);
                        }

                        for (int i = 0, index = start; i < castedVal.Count; i++, index += step) {
                            if (i >= n) {
                                if (index == _bytes.Count) {
                                    _bytes.Add(castedVal[i]);
                                } else {
                                    _bytes.Insert(index, castedVal[i]);
                                }
                            } else {
                                _bytes[index] = castedVal[i];
                            }
                        }
                    } else {
                        int start, stop, step;
                        slice.indices(_bytes.Count, out start, out stop, out step);
                        if (start > stop) {
                            return;
                        }

                        ByteArray lstVal = value as ByteArray;
                        if (lstVal != null) {
                            SliceNoStep(start, stop, lstVal);
                        } else {
                            SliceNoStep(start, stop, value);
                        }
                    }
                }
            }
        }
        
        [SpecialName]
        public void DeleteItem(int index) {
            _bytes.RemoveAt(index);
        }

        [SpecialName]
        public void DeleteItem(Slice/*!*/ slice) {
            if (slice == null) {
                throw PythonOps.TypeError("list indices must be integers or slices");
            }

            lock (this) {
                int start, stop, step;
                // slice is sealed, indices can't be user code...
                slice.indices(_bytes.Count, out start, out stop, out step);

                if (step > 0 && (start >= stop)) return;
                if (step < 0 && (start <= stop)) return;

                if (step == 1) {
                    int i = start;
                    for (int j = stop; j < _bytes.Count; j++, i++) {
                        _bytes[i] = _bytes[j];
                    }
                    _bytes.RemoveRange(i, stop - start);
                    return;
                } else if (step == -1) {
                    int i = stop + 1;
                    for (int j = start + 1; j < _bytes.Count; j++, i++) {
                        _bytes[i] = _bytes[j];
                    }
                    _bytes.RemoveRange(i, start - stop);
                    return;
                } else if (step < 0) {
                    // find "start" we will skip in the 1,2,3,... order
                    int i = start;
                    while (i > stop) {
                        i += step;
                    }
                    i -= step;

                    // swap start/stop, make step positive
                    stop = start + 1;
                    start = i;
                    step = -step;
                }

                int curr, skip, move;
                // skip: the next position we should skip
                // curr: the next position we should fill in data
                // move: the next position we will check
                curr = skip = move = start;

                while (curr < stop && move < stop) {
                    if (move != skip) {
                        _bytes[curr++] = _bytes[move];
                    } else
                        skip += step;
                    move++;
                }
                while (stop < _bytes.Count) {
                    _bytes[curr++] = _bytes[stop++];
                }
                _bytes.RemoveRange(curr, _bytes.Count - curr);
            }
        }

        #endregion

        #region Implementation Details

        private static ByteArray/*!*/ JoinOne(object/*!*/ curVal) {
            if (!(curVal is IList<byte>)) {
                throw PythonOps.TypeError("can only join an iterable of bytes");
            }

            return new ByteArray(new List<byte>(curVal as IList<byte>));
        }

        private ByteArray/*!*/ CopyThis() {
            return new ByteArray(new List<byte>(_bytes));
        }

        private static int GetIntValue(object/*!*/ value) {
            if (value is IList<byte>) {
                return ((IList<byte>)value).ToByte("__setitem__", 2);
            }
            if (!(value is int)) {
                throw PythonOps.TypeError("expected integer when assigning slice to bytearray, got {0}", DynamicHelpers.GetPythonType(value).Name);
            }
            return (int)value;
        }

        private void SliceNoStep(int start, int stop, ByteArray/*!*/ other) {
            Debug.Assert(other != null);

            // We don't lock other here - instead we read it's object array
            // and size therefore having a stable view even if it resizes.
            // This means if we had a multithreaded app like:
            // 
            //  T1                   T2                     T3
            //  l1[:] = [1] * 100    l1[:] = [2] * 100      l3[:] = l1[:]
            //
            // we can end up with both 1s and 2s in the array.  This is the
            // same as if our set was implemented on top of get/set item where
            // we'd take and release the locks repeatedly.
            int otherSize = other._bytes.Count;
            List<byte> otherData = other._bytes;

            lock (this) {
                if ((stop - start) == otherSize) {
                    // we are simply replacing values, this is fast...
                    for (int i = 0; i < otherSize; i++) {
                        _bytes[i + start] = otherData[i];
                    }
                } else {
                    // we are resizing the array (either bigger or smaller), we 
                    // will copy the data array and replace it all at once.
                    int newSize = Count - (stop - start) + otherSize;

                    List<byte> newData = new List<byte>();
                    for (int i = 0; i < start; i++) {
                        newData.Add(_bytes[i]);
                    }

                    for (int i = 0; i < otherSize; i++) {
                        newData.Add(otherData[i]);
                    }

                    for (int i = stop; i < Count; i++) {
                        newData.Add(_bytes[i]);
                    }

                    _bytes = newData;
                }
            }
        }

        private void SliceNoStep(int start, int stop, object/*!*/ value) {
            // always copy from a List object, even if it's a copy of some user defined enumerator.  This
            // makes it easy to hold the lock for the duration fo the copy.
            IList<byte> other = GetBytes(value);

            lock (this) {
                if ((stop - start) == other.Count) {
                    // we are simply replacing values, this is fast...
                    for (int i = 0; i < other.Count; i++) {
                        _bytes[i + start] = other[i];
                    }
                } else {
                    // we are resizing the array (either bigger or smaller), we 
                    // will copy the data array and replace it all at once.
                    int newSize = Count - (stop - start) + other.Count;

                    List<byte> newData = new List<byte>();
                    for (int i = 0; i < start; i++) {
                        newData.Add(_bytes[i]);
                    }

                    for (int i = 0; i < other.Count; i++) {
                        newData.Add(other[i]);
                    }

                    for (int i = stop; i < Count; i++) {
                        newData.Add(_bytes[i]);
                    }

                    _bytes = newData;
                }
            }
        }

        private static IList<byte>/*!*/ GetBytes(object/*!*/ value) {
            ListGenericWrapper<byte> genWrapper = value as ListGenericWrapper<byte>;
            if (genWrapper == null && value is IList<byte>) {
                return (IList<byte>)value;
            }

            List<byte> ret = new List<byte>();
            IEnumerator ie = PythonOps.GetEnumerator(value);
            while (ie.MoveNext()) {
                ret.Add(GetIntValue(ie.Current).ToByteChecked());
            }
            return ret;
        }

        #endregion

        #region IList<byte> Members

        [PythonHidden]
        public int IndexOf(byte item) {
            lock (this) {
                return _bytes.IndexOf(item);
            }
        }

        [PythonHidden]
        public void Insert(int index, byte item) {
            _bytes.Insert(index, item);
        }

        [PythonHidden]
        public void RemoveAt(int index) {
            _bytes.RemoveAt(index);
        }

        byte IList<byte>.this[int index] {
            get {
                return _bytes[index];
            }
            set {
                _bytes[index] = value;
            }
        }

        #endregion

        #region ICollection<byte> Members

        [PythonHidden]
        public void Add(byte item) {
            lock (this) {
                _bytes.Add(item);
            }
        }

        [PythonHidden]
        public void Clear() {
            lock (this) {
                _bytes.Clear();
            }
        }

        [PythonHidden]
        public bool Contains(byte item) {
            lock (this) {
                return _bytes.Contains(item);
            }
        }

        [PythonHidden]
        public void CopyTo(byte[]/*!*/ array, int arrayIndex) {
            lock (this) {
                _bytes.CopyTo(array, arrayIndex);
            }
        }

        public int Count {
            [PythonHidden]
            get { 
                lock (this) { 
                    return _bytes.Count; 
                } 
            }
        }

        public bool IsReadOnly {
            [PythonHidden]
            get { return false; }
        }

        [PythonHidden]
        public bool Remove(byte item) {
            lock (this) {
                return _bytes.Remove(item);
            }
        }

        #endregion

        #region IEnumerable<byte> Members

        [PythonHidden]
        public IEnumerator<byte>/*!*/ GetEnumerator() {
            return _bytes.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator/*!*/ System.Collections.IEnumerable.GetEnumerator() {
            return _bytes.GetEnumerator();
        }

        #endregion

        #region IValueEquality Members

        int IValueEquality.GetValueHashCode() {
            throw PythonOps.TypeError("bytearray object is unhashable");
        }

        bool IValueEquality.ValueEquals(object other) {
            IList<byte> bytes = other as IList<byte>;
            if (bytes == null) {
                return false;
            }

            using (new OrderedLocker(this, other)) {
                return _bytes.Compare(bytes) == 0;
            }
        }

        #endregion
    }
}
