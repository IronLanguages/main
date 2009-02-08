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
using System.Diagnostics;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix"), PythonType("generator")]
    public sealed class PythonGenerator : IEnumerator, IEnumerator<object>, IEnumerable, IEnumerable<object>, ICodeFormattable {
        /// <summary>
        /// The instance of the DLR generator.
        /// </summary>
        private IEnumerator/*!*/ _next;

        /// <summary>
        /// Current
        /// </summary>
        private object _current;

        /// <summary>
        /// Code context
        /// </summary>
        private readonly CodeContext _context;

        /// <summary> Flags capturing various state for the generator </summary>
        private GeneratorFlags _flags;

        /// <summary>
        /// Fields set by Throw() to communicate an exception to the yield point.
        /// These are plumbed through the generator to become parameters to Raise(...) invoked 
        /// at the yield suspenion point in the generator.
        /// </summary>
        private object[] _excInfo;
        /// <summary>
        /// Value sent by generator.send().
        /// Since send() could send an exception, we need to keep this different from throwable's value.
        /// </summary>
        private object _sendValue;

        private static object _notStarted = new object();

        public PythonGenerator(CodeContext context) {
            _context = context;
            _current = _notStarted;
        }

        private CodeContext Context {
            get {
                return _context;
            }
        }

        internal IEnumerator Next {
            set {
                _next = value;
            }
        }

        // Silverlight doesn't allow finalizers in user code.
#if !SILVERLIGHT
        // Pep 342 says generators now have finalizers (__del__) that call Close()
        ~PythonGenerator() {
            try {
                // This may run the users generator.
                close();
            } catch (Exception e) {
                // An unhandled exceptions on the finalizer could tear down the process, so catch it.

                // PEP says:
                //   If close() raises an exception, a traceback for the exception is printed to sys.stderr
                //   and further ignored; it is not propagated back to the place that
                //   triggered the garbage collection. 

                // Sample error message from CPython 2.5 looks like:
                //     Exception __main__.MyError: MyError() in <generator object at 0x00D7F6E8> ignored
                try {
                    string message = string.Format("Exception in generator {1} ignored", PythonOps.Repr(Context, this));

                    PythonOps.PrintWithDest(Context, PythonContext.GetContext(Context).SystemStandardError, message);
                    PythonOps.PrintWithDest(Context, PythonContext.GetContext(Context).SystemStandardError, Context.LanguageContext.FormatException(e));
                } catch {
                    // if stderr is closed then ignore any exceptions.
                }
            }
        }
#endif // !SILVERLIGHT

        bool IEnumerator.MoveNext() {
            try {
                object res = MoveNextWorker();
                if (res != OperationFailed.Value) {
                    return true;
                }

                return false;
            } catch (StopIterationException) {
                return false;
            }
        }

        private object MoveNextWorker() {
            // Python's language policy on generators is that attempting to access after it's closed (returned)
            // just continues to throw StopIteration exceptions.
            if (Closed) {
                throw new StopIterationException();
            }

            // Generators can not be called re-entrantly.
            CheckSetActive();

            // We need to save/restore the exception info if the generator
            // includes exception handling blocks.
            Exception save = SaveCurrentException();

            bool ret = false;
            object next = OperationFailed.Value;
            try {
                // This calls into the delegate that has the real body of the generator.
                // The generator body here may:
                // 1. return an item: _next() returns true and 'next' is set to the next item in the enumeration.
                // 2. Exit normally: _next returns false.
                // 3. Exit with a StopIteration exception: for-loops and other enumeration consumers will 
                //    catch this and terminate the loop without propogating the exception.
                // 4. Exit via some other unhandled exception: This will close the generator, but the exception still propogates.
                //    _next does not return, so ret is left assigned to false (closed), which we detect in the finally.
                if (ret = _next.MoveNext()) {
                    next = _next.Current;
                } else {
                    next = OperationFailed.Value;
                }
            } finally {
                // A generator restores the sys.exc_info() status after each yield point.
                RestoreCurrentException(save);
                Active = false;

                // If _next() returned false, or did not return (thus leavintg ret assigned to its initial value of false), then 
                // the body of the generator has exited and the generator is now closed.
                if (!ret) {
                    Close();
                }
            }
                        
            _current = next;
            return next;
        }

        private void RestoreCurrentException(Exception save) {
            if (CanSetSysExcInfo) {
                PythonOps.RestoreCurrentException(save);
            }
        }

        private Exception SaveCurrentException() {
            if (CanSetSysExcInfo) {
                return PythonOps.SaveCurrentException();
            }
            return null;
        }

        private void CheckSetActive() {
            if (Active) {
                // A generator could catch this exception and continue executing, so this does
                // not necessarily close the generator.
                throw PythonOps.ValueError("generator already executing");
            }
            Active = true;
        }

        public object next() {
            object res = MoveNextWorker();
            if (res == OperationFailed.Value) {
                throw new StopIterationException();
            }

            return res;
        }

        /// <summary>
        /// See PEP 342 (http://python.org/dev/peps/pep-0342/) for details of new methods on Generator.
        /// Full signature including default params for throw is:
        ///    throw(type, value=None, traceback=None)
        /// Use multiple overloads to resolve the default parameters.
        /// </summary>
        public object @throw(object type) {
            return @throw(type, null, null);
        }

        public object @throw(object type, object value) {
            return @throw(type, value, null);
        }

        /// <summary>
        /// Throw(...) is like Raise(...) being called from the yield point within the generator.
        /// Note it must come from inside the generator so that the traceback matches, and so that it can 
        /// properly cooperate with any try/catch/finallys inside the generator body.
        /// 
        /// If the generator catches the exception and yields another value, that is the return value of g.throw().
        /// </summary>
        public object @throw(object type, object value, object traceback) {
            // The Pep342 explicitly says "The type argument must not be None". 
            // According to CPython 2.5's implementation, a null type argument should:
            // - throw a TypeError exception (just as Raise(None) would) *outside* of the generator's body
            //   (so the generator can't catch it).
            // - not update any other generator state (so future calls to Next() will still work)
            if (type == null) {
                // Create the appropriate exception and throw it.
                throw PythonOps.MakeExceptionTypeError(null);
            }

            // Set fields which will then be used by CheckThrowable.
            // We create the actual exception from inside the generator so that if the exception's __init__ 
            // throws, the traceback matches that which we get from CPython2.5.
            _excInfo = new object[] { type, value, traceback };
            Debug.Assert(_sendValue == null);

            // Pep explicitly says that Throw on a closed generator throws the exception, 
            // and not a StopIteration exception. (This is different than Next()).
            if (Closed) {
                // this will throw the exception that we just set the fields for.
                CheckThrowable();
            }

            if (!((IEnumerator)this).MoveNext()) {
                throw PythonOps.StopIteration();
            }
            return _current;
        }

        /// <summary>
        /// send() was added in Pep342. It sends a result back into the generator, and the expression becomes
        /// the result of yield when used as an expression.
        /// </summary>
        public object send(object value) {
            Debug.Assert(_excInfo == null);

            // CPython2.5's behavior is that Send(non-null) on unstaretd generator should:
            // - throw a TypeError exception
            // - not change generator state. So leave as unstarted, and allow future calls to succeed.
            if (value != null && _current == _notStarted) {
                throw PythonOps.TypeErrorForIllegalSend();
            }

            _sendValue = value;
            return next();
        }
        
        /// <summary>
        /// Close introduced in Pep 342.
        /// </summary>
        public void close() {
            // This is nop if the generator is already closed.

            // Optimization to avoid throwing + catching an exception if we're already closed.
            if (Closed) {
                return;
            }

            // This function body is the psuedo code straight from Pep 342.
            try {
                @throw(new GeneratorExitException());

                // Generator should not have exited normally. 
                throw new RuntimeException("generator ignored GeneratorExit");
            } catch (StopIterationException) {
                // Ignore
            } catch (GeneratorExitException) {
                // Ignore
            }
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return this;
        }

        #endregion      

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return this;
        }

        #endregion

        #region Internal implementation details

        /// <summary>
        /// Helper called from PythonOps after the yield statement
        /// Keepin this in a helper method:
        /// - reduces generated code size
        /// - allows better coupling with PythonGenerator.Throw()
        /// - avoids throws from emitted code (which can be harder to debug).
        /// </summary>
        /// <returns></returns>
        internal object CheckThrowableAndReturnSendValue() {
            // Since this method is called from the generator body's execution, the generator must be running 
            // and not closed.
            Debug.Assert(!Closed);
            
            if (_sendValue != null) {
                // Can't Send() and Throw() at the same time.
                Debug.Assert(_excInfo == null);

                return SwapValues();
            }
            return CheckThrowable();
        }

        private object SwapValues() {
            object sendValueBackup = _sendValue;
            _sendValue = null;
            return sendValueBackup;
        }

        /// <summary>
        /// Called to throw an exception set by Throw().
        /// </summary>
        private object CheckThrowable() {
            if (_excInfo != null) {
                ThrowThrowable();
            }
            return null;
        }

        private void ThrowThrowable() {
            object[] throwableBackup = _excInfo;

            // Clear it so that any future Next()/MoveNext() call doesn't pick up the exception again.
            _excInfo = null;

            // This may invoke user code such as __init__, thus MakeException may throw. 
            // Since this is invoked from the generator's body, the generator can catch this exception. 
            throw PythonOps.MakeException(Context, throwableBackup[0], throwableBackup[1], throwableBackup[2]);
        }

        private void Close() {
            Closed = true;
            // if we're closed the finalizer won't do anything, so suppress it.
            GC.SuppressFinalize(this);
        }

        private bool Closed {
            get {
                return (_flags & GeneratorFlags.Closed) != 0;
            }
            set {
                if (value) _flags |= GeneratorFlags.Closed;
                else _flags &= ~GeneratorFlags.Closed;
            }
        }

        private bool Active {
            get {
                return (_flags & GeneratorFlags.Active) != 0;
            }
            set {
                if (value) _flags |= GeneratorFlags.Active;
                else _flags &= ~GeneratorFlags.Active;
            }
        }

        internal bool CanSetSysExcInfo {
            get {
                return (_flags & GeneratorFlags.CanSetSysExcInfo) != 0;
            }
            set {
                if (value) _flags |= GeneratorFlags.CanSetSysExcInfo;
                else _flags &= ~GeneratorFlags.CanSetSysExcInfo;
            }            
        }

        #endregion

        #region ICodeFormattable Members

        public string __repr__(CodeContext context) {
            return string.Format("<generator object at {0}>", PythonOps.HexId(this));
        }

        #endregion

        [Flags]
        private enum GeneratorFlags {
            None,
            /// <summary>
            /// True if the generator has finished (is "closed"), else false.
            /// Python language spec mandates that calling Next on a closed generator gracefully throws a StopIterationException.
            /// This can never be reset.
            /// </summary>
            Closed = 0x01,
            /// <summary>
            /// True iff the thread is currently inside the generator (ie, invoking the _next delegate).
            /// This can be used to enforce that a generator does not call back into itself. 
            /// Pep255 says that a generator should throw a ValueError if called reentrantly.
            /// </summary>
            Active = 0x02,
            /// <summary>
            /// True if the generator can set sys exc info and therefore needs exception save/restore.
            /// </summary>
            CanSetSysExcInfo = 0x04
        }


        #region IEnumerator Members

        object IEnumerator.Current {
            get { return _current; }
        }

        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerator<object> Members

        object IEnumerator<object>.Current {
            get { return _current; }
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose() {
            // nothing needed to dispose
            IDisposable dn = _next as IDisposable;
            if (dn != null) {
                dn.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
