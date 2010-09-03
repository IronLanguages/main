/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if CLR2
using Microsoft.Scripting.Math;
#else
using System.Numerics;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    [PythonType("memoryview")]
    public sealed class MemoryView : ICodeFormattable {
        private readonly IBufferProtocol _buffer;
        private readonly int _start;
        private readonly int? _end;

        public MemoryView(IBufferProtocol @object) {
            _buffer = @object;
        }

        private MemoryView(IBufferProtocol @object, int start, int end) {
            _buffer =@object;
            _start = start;
            _end = end;
        }

        public int __len__() {
            return _buffer.ItemCount;
        }

        public string format {
            get { return _buffer.Format; }
        }

        public BigInteger itemsize {
            get { return _buffer.ItemSize; }
        }

        public BigInteger ndim {
            get { return _buffer.NumberDimensions; }
        }

        public bool @readonly {
            get { return _buffer.ReadOnly; }
        }

        public PythonTuple shape {
            get {
                var shape = _buffer.Shape;
                if (shape == null) {
                    return null;
                }
                return new PythonTuple(shape); 
            }
        }

        public PythonTuple strides {
            get { return _buffer.Strides; }
        }

        public object suboffsets {
            get { return _buffer.SubOffsets; }
        }

        public Bytes tobytes() {
            return _buffer.ToBytes(_start, _end);
        }

        public List tolist() {
            return _buffer.ToList(_start, _end);
        }

        public object this[int index] {
            get {
                return _buffer.GetItem(index + _start);
            }
            set {
                if (_buffer.ReadOnly) {
                    throw PythonOps.TypeError("cannot modify read-only memory");
                }
                _buffer.SetItem(index + _start, value);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void __delitem__(int index) {
            // crashes CPython
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void __delitem__(Slice slice) {
            // crashes CPython
            throw new NotImplementedException();
        }

        public object this[Slice slice] {
            get {
                if (slice.step != null) {
                    throw PythonOps.NotImplementedError("");
                }

                return new MemoryView(
                    _buffer,
                    slice.start == null ? _start : Converter.ConvertToInt32(slice.start) + _start,
                    slice.stop == null ? -1 : Converter.ConvertToInt32(slice.stop)
                );
            }
            set {
                throw new NotImplementedException();
            }
        }

        public static bool operator >(MemoryView self, MemoryView other) {
            if ((object)self == null) {
                return (object)other != null;
            } else if ((object)other == null) {
                return true;
            }
            return self.tobytes() > other.tobytes();
        }

        public static bool operator <(MemoryView self, MemoryView other) {
            if ((object)self == null) {
                return (object)other == null;
            } else if ((object)other == null) {
                return false;
            }
            return self.tobytes() < other.tobytes();
        }
        
        public static bool operator >=(MemoryView self, MemoryView other) {
            if ((object)self == null) {
                return (object)other == null;
            } else if ((object)other == null) {
                return false;
            }
            return self.tobytes() >= other.tobytes();
        }

        public static bool operator <=(MemoryView self, MemoryView other) {
            if ((object)self == null) {
                return (object)other != null;
            } else if ((object)other == null) {
                return true;
            }
            return self.tobytes() <= other.tobytes();
        }
        
        public static bool operator ==(MemoryView self, MemoryView other) {
            if ((object)self == null) {
                return (object)other == null;
            } else if ((object)other == null) {
                return false;
            }
            return self.tobytes().Equals(other.tobytes());
        }

        public static bool operator !=(MemoryView self, MemoryView other) {
            if ((object)self == null) {
                return (object)other != null;
            } else if ((object)other == null) {
                return true;
            }
            return !self.tobytes().Equals(other.tobytes());
        }

        public const object __hash__ = null;

        public override bool Equals(object obj) {
            MemoryView mv = obj as MemoryView;
            if (mv != null) {
                return this == mv;
            }
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        #region ICodeFormattable Members

        public string __repr__(CodeContext context) {
            return String.Format("<memory at {0}>", PythonOps.Id(this));
        }

        #endregion
    }
}
