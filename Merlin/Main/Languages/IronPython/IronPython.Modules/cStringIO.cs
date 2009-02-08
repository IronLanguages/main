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
using System.IO;
using System.Text;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonModule("cStringIO", typeof(IronPython.Modules.PythonStringIO))]
namespace IronPython.Modules {
    class StringStream {
        private string _data;
        private int _position;
        private int _length;

        public StringStream(string data) {
            this._data = data;
            this._position = 0;
            this._length = data == null ? 0 : data.Length;
        }

        public bool EOF {
            get { return _position >= _length; }
        }

        public int Position {
            get { return _position; }
        }

        public string Data {
            get {
                return _data;
            }
            set {
                _data = value;
                if (_data == null) {
                    _length = _position = 0;
                } else {
                    _length = _data.Length;
                    if (_position > _length) {
                        _position = _length;
                    }
                }
            }
        }

        public string Prefix {
            get {
                return _data.Substring(0, _position);
            }
        }

        public int Read() {
            if (_position < _length) {
                return _data[_position++];
            } else {
                return -1;
            }
        }

        public string Read(int i) {
            if (_position + i > _length) {
                i = _length - _position;
            }
            string ret = _data.Substring(_position, i);
            _position += i;
            return ret;
        }

        public string ReadLine(int size) {
            if (size < 0) {
                size = Int32.MaxValue;
            }
            int i = _position;
            int count = 0;
            while (i < _length && count < size) {
                char c = _data[i];
                if (c == '\n' || c == '\r') {
                    i++;
                    if (c == '\r' && _position < _length && _data[i] == '\n') {
                        i++;
                    }
                    // preserve newline character like StringIO

                    string res = _data.Substring(_position, i - _position);
                    _position = i;
                    return res;
                }
                i++;
                count++;
            }

            if (i > _position) {
                string res = _data.Substring(_position, i - _position);
                _position = i;
                return res;
            }

            return "";
        }

        public string ReadToEnd() {
            if (_position < _length) {
                string res = _data.Substring(_position);
                _position = _length;
                return res;
            } else return "";
        }

        public void Reset() {
            _position = 0;
        }

        public int Seek(int offset, SeekOrigin origin) {
            switch (origin) {
                case SeekOrigin.Begin:
                    _position = offset; break;
                case SeekOrigin.Current:
                    _position = _position + offset; break;
                case SeekOrigin.End:
                    _position = _length + offset; break;
                default:
                    throw new ArgumentException("origin");
            }
            return _position;
        }

        public void Truncate() {
            _data = _data.Substring(0, _position);
            _length = _data.Length;
        }

        public void Truncate(int size) {
            if (size > _data.Length) {
                size = _data.Length;
            } else if (size < 0) {
                size = 0;
            }
            _data = _data.Substring(0, size);
            _position = size;
            _length = _data.Length;
        }

        internal void Write(string s) {
            string newData;
            int newPosition;
            if (_position > 0) {
                newData = _data.Substring(0, _position) + s;
            } else {
                newData = s;
            }
            newPosition = newData.Length;
            if (_position + s.Length < _length) {
                newData = newData + _data.Substring(_position + s.Length);
            }

            _data = newData;
            _position = newPosition;
            _length = _data.Length;
        }
    }

    public static class PythonStringIO {
        public static PythonType InputType = DynamicHelpers.GetPythonTypeFromType(typeof(StringI));
        public static PythonType OutputType = DynamicHelpers.GetPythonTypeFromType(typeof(StringO));

        public class StringI : IEnumerable<string>, IEnumerable {
            private StringStream _sr;

            internal StringI(string data) {
                _sr = new StringStream(data);
            }

            public void close() {
                _sr = null;
            }

            public bool closed {
                get {
                    return _sr == null;
                }
            }

            public void flush() {
                ThrowIfClosed();
            }

            public string getvalue() {
                ThrowIfClosed();
                return _sr.Data;
            }

            public string getvalue(bool usePos) {
                return _sr.Prefix;
            }

            public object __iter__() {
                return this;
            }

            public string next() {
                ThrowIfClosed();
                if (_sr.EOF) {
                    throw PythonOps.StopIteration();
                }
                return readline();
            }

            public string read() {
                ThrowIfClosed();
                return _sr.ReadToEnd();
            }

            public string read(int s) {
                ThrowIfClosed();
                return (s < 0) ? _sr.ReadToEnd() : _sr.Read(s);
            }

            public string readline() {
                ThrowIfClosed();
                return _sr.ReadLine(-1);
            }

            public string readline(int size) {
                ThrowIfClosed();
                return _sr.ReadLine(size);
            }

            public List readlines() {
                ThrowIfClosed();
                List list = PythonOps.MakeList();
                while (!_sr.EOF) {
                    list.AddNoLock(readline());
                }
                return list;
            }

            public List readlines(int size) {
                ThrowIfClosed();
                List list = PythonOps.MakeList();
                while (!_sr.EOF) {
                    string line = readline();
                    list.AddNoLock(line);
                    if (line.Length >= size) break;
                    size -= line.Length;
                }
                return list;
            }

            public void reset() {
                ThrowIfClosed();
                _sr.Reset();
            }

            public void seek(int position) {
                seek(position, 0);
            }

            public void seek(int position, int mode) {
                ThrowIfClosed();
                SeekOrigin so;
                switch (mode) {
                    case 1: so = SeekOrigin.Current; break;
                    case 2: so = SeekOrigin.End; break;
                    default: so = SeekOrigin.Begin; break;
                }
                _sr.Seek(position, so);
            }

            public int tell() {
                ThrowIfClosed();
                return _sr.Position;
            }

            public void truncate() {
                ThrowIfClosed();
                _sr.Truncate();
            }

            public void truncate(int size) {
                ThrowIfClosed();
                _sr.Truncate(size);
            }

            private void ThrowIfClosed() {
                if (closed) {
                    throw PythonOps.ValueError("I/O operation on closed file");
                }
            }

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator() {
                while (!_sr.EOF) {
                    yield return readline();
                }
            }

            #endregion

            #region IEnumerable<string> Members

            IEnumerator<string> IEnumerable<string>.GetEnumerator() {
                while (!_sr.EOF) {
                    yield return readline();
                }
            }

            #endregion
        }

        public class StringO : IEnumerable<string>, IEnumerable {
            private StringWriter _sw = new StringWriter();
            private StringStream _sr = new StringStream("");
            private int _softspace;

            internal StringO() {
            }

            public object __iter__() {
                return this;
            }

            public void close() {
                if (_sw != null) { _sw.Close(); _sw = null; }
                if (_sr != null) { _sr = null; }
            }

            public bool closed {
                get {
                    return _sw == null || _sr == null;
                }
            }

            public void flush() {
                FixStreams();
            }

            public string getvalue() {
                ThrowIfClosed();
                FixStreams();
                return _sr.Data;
            }

            public string getvalue(bool usePos) {
                ThrowIfClosed();
                FixStreams();
                return _sr.Prefix;
            }

            public string next() {
                ThrowIfClosed();
                FixStreams();
                if (_sr.EOF) {
                    throw PythonOps.StopIteration();
                }
                return readline();
            }

            public string read() {
                ThrowIfClosed();
                FixStreams();
                return _sr.ReadToEnd();
            }

            public string read(int i) {
                ThrowIfClosed();
                FixStreams();
                return (i < 0) ? _sr.ReadToEnd() : _sr.Read(i);
            }

            public string readline() {
                ThrowIfClosed();
                FixStreams();
                return _sr.ReadLine(-1);
            }

            public string readline(int size) {
                ThrowIfClosed();
                FixStreams();
                return _sr.ReadLine(size);
            }

            public List readlines() {
                ThrowIfClosed();
                List list = PythonOps.MakeList();
                while (!_sr.EOF) {
                    list.AddNoLock(readline());
                }
                return list;
            }

            public List readlines(int size) {
                ThrowIfClosed();
                List list = PythonOps.MakeList();
                while (!_sr.EOF) {
                    string line = readline();
                    list.AddNoLock(line);
                    if (line.Length >= size) break;
                    size -= line.Length;
                }
                return list;
            }

            public void reset() {
                ThrowIfClosed();
                FixStreams();
                _sr.Reset();
            }

            public void seek(int position) {
                seek(position, 0);
            }

            public void seek(int offset, int origin) {
                ThrowIfClosed();
                FixStreams();
                SeekOrigin so;
                switch (origin) {
                    case 1: so = SeekOrigin.Current; break;
                    case 2: so = SeekOrigin.End; break;
                    default: so = SeekOrigin.Begin; break;
                }
                _sr.Seek(offset, so);
            }

            public int softspace {
                get { return _softspace; }
                set { _softspace = value; }
            }

            public int tell() {
                ThrowIfClosed();
                FixStreams();
                return _sr.Position;
            }

            public void truncate() {
                ThrowIfClosed();
                FixStreams();
                _sr.Truncate();
            }

            public void truncate(int size) {
                ThrowIfClosed();
                FixStreams();
                _sr.Truncate(size);
            }

            public void write(string s) {
                ThrowIfClosed();
                _sw.Write(s);
            }

            public void writelines(object o) {
                ThrowIfClosed();
                IEnumerator e = PythonOps.GetEnumerator(o);
                while (e.MoveNext()) {
                    string s = e.Current as string;
                    if (s == null) {
                        throw PythonOps.ValueError("string expected");
                    }
                    write(s);
                }
            }

            private void FixStreams() {
                if (_sr != null) {
                    StringBuilder sb = _sw.GetStringBuilder();
                    if (sb != null && sb.Length > 0) {
                        _sr.Write(sb.ToString());
                        sb.Length = 0;
                    }
                }
            }

            private void ThrowIfClosed() {
                if (closed) {
                    throw PythonOps.ValueError("I/O operation on closed file");
                }
            }

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator() {
                while (!_sr.EOF) {
                    yield return readline();
                }
            }

            #endregion

            #region IEnumerable<string> Members

            IEnumerator<string> IEnumerable<string>.GetEnumerator() {
                while (!_sr.EOF) {
                    yield return readline();
                }
            }

            #endregion
        }

        public static object StringIO() {
            return new StringO();
        }

        public static object StringIO(string data) {
            return new StringI(data);
        }
    }
}
