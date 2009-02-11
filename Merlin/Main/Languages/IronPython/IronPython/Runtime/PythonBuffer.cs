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
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Binding;

namespace IronPython.Runtime {
    [PythonType("buffer")]
    public sealed class PythonBuffer : ICodeFormattable {
        private object _object;
        private int _offset;
        private int _size;

        private bool _isbuffer;      /*buffer of buffer*/
        private bool _isstring;
        private bool _isarray;
        private readonly CodeContext/*!*/ _context;

        public PythonBuffer(CodeContext/*!*/ context, object @object)
            : this(context, @object, 0) {
        }

        public PythonBuffer(CodeContext/*!*/ context, object @object, int offset)
            : this(context, @object, offset, -1) {
        }

        public PythonBuffer(CodeContext/*!*/ context, object @object, int offset, int size) {
            if (!InitBufferObject(@object, offset, size)) {
                throw PythonOps.TypeError("expected buffer object");
            }
            _context = context;
        }

        private bool InitBufferObject(object o, int offset, int size) {
            //  we currently support only buffers, strings and arrays
            //  of primitives and strings
            if (o == null || (!(_isbuffer = o is PythonBuffer) && !(_isstring = o is string) && !(_isarray = o is Array)) && !(_isarray = o is IPythonArray)) {
                return false;
            }
            if (offset < 0) {
                throw PythonOps.ValueError("offset must be zero or positive");
            }
            //  -1 is the way to ask for the default size so we allow -1 as a size
            if (size < -1) {
                throw PythonOps.ValueError("size must be zero or positive");
            }
            if (_isbuffer) {
                PythonBuffer py = (PythonBuffer)o;
                o = py._object; // grab the internal object
                offset = py._offset + offset; // reset the offset based on the given buffer's original offset
                // reset the size based on the given buffer's original size
                if (size >= py._size - offset || size == -1) {
                    this._size = py._size - offset;
                } else {
                    this._size = size;
                }
            } else if (_isstring) {
                string strobj = ((string)o);
                if (size >= strobj.Length || size == -1) {
                    this._size = strobj.Length;
                } else {
                    this._size = size;
                }
            } else if (_isarray) { // has to be an array at this point
                Array arr = o as Array;
                if (arr != null) {
                    Type t = arr.GetType().GetElementType();
                    if (!t.IsPrimitive && t != typeof(string)) {
                        return false;
                    }
                    if (size >= arr.Length || size == -1) {
                        this._size = arr.Length;
                    } else {
                        this._size = size;
                    }
                } else {
                    IPythonArray pa = (IPythonArray)o;
                    _size = pa.__len__();
                }
            }
            this._object = o;
            this._offset = offset;

            return true;
        }

        public override string ToString() {
            return GetSelectedRange().ToString();
        }

        public override bool Equals(object obj) {
            PythonBuffer b = obj as PythonBuffer;
            if (b == null) return false;

            return this == b;
        }

        public override int GetHashCode() {
            return _object.GetHashCode() ^ _offset ^ (_size << 16 | (_size >> 16));
        }

        private object GetSlice() {
            object end = null;
            if (_size >= 0) {
                end = _offset + _size;
            }
            return new Slice(_offset, end);
        }

        public object __getslice__(object start, object stop) {
            return this[new Slice(start, stop)];
        }

        private Exception ReadOnlyError() {
            return PythonOps.TypeError("buffer is read-only");
        }

        public object __setslice__(object start, object stop, object value) {
            throw ReadOnlyError();
        }

        public void __delitem__(int index) {
            throw ReadOnlyError();
        }

        public void __delslice__(object start, object stop) {
           throw ReadOnlyError();
        }

        public object this[object s] {
            [SpecialName]
            get {
                return PythonOps.GetIndex(_context, GetSelectedRange(), s);
            }
            [SpecialName]
            set {
                throw ReadOnlyError();
            }
        }

        private object GetSelectedRange() {
            if (_isarray) {
                IPythonArray arr = _object as IPythonArray;
                if (arr != null) {
                    return arr.tostring();
                }
            }
            return PythonOps.GetIndex(_context, _object, GetSlice());
        }

        public static object operator +(PythonBuffer a, PythonBuffer b) {
            PythonContext context = PythonContext.GetContext(a._context);

            return context.Operation(
                PythonOperationKind.Add,
                PythonOps.GetIndex(a._context, a._object, a.GetSlice()), 
                PythonOps.GetIndex(a._context, b._object, b.GetSlice())
            );
        }

        public static object operator +(PythonBuffer a, string b) {
            return a.ToString() + b;
        }

        public static object operator *(PythonBuffer b, int n) {
            PythonContext context = PythonContext.GetContext(b._context);

            return context.Operation(
                PythonOperationKind.Multiply,
                PythonOps.GetIndex(b._context, b._object, b.GetSlice()),
                n
            );
        }

        public static object operator *(int n, PythonBuffer b) {
            PythonContext context = PythonContext.GetContext(b._context);

            return context.Operation(
                PythonOperationKind.Multiply,
                PythonOps.GetIndex(b._context, b._object, b.GetSlice()),
                n
            );                
        }

        public static bool operator ==(PythonBuffer a, PythonBuffer b) {
            if (Object.ReferenceEquals(a, b)) return true;
            if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null)) return false;

            return a._object.Equals(b._object) &&
                a._offset == b._offset &&
                a._size == b._size;
        }

        public static bool operator !=(PythonBuffer a, PythonBuffer b) {
            return !(a == b);
        }

        public int __len__() {
            return _size;
        }

        internal int Size {
            get {
                return _size;
            }
        }

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            return string.Format("<read-only buffer for 0x{0:X16}, size {1}, offset {2} at 0x{3:X16}>",
                PythonOps.Id(_object), _size, _offset, PythonOps.Id(this));
        }

        #endregion
    }

    /// <summary>
    /// A marker interface so we can recognize and access sequence members on our array objects.
    /// </summary>
    internal interface IPythonArray : ISequence {
        string tostring();
    }
}
