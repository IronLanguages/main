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
using System.Linq.Expressions;
using Microsoft.Scripting;
using System.Dynamic.Binders;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Threading;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    [RubyClass("Thread", Extends = typeof(Thread), Inherits = typeof(object))]
    public static class ThreadOps {
        static bool _globalAbortOnException;

        internal class RubyThreadInfo {
            private static readonly Dictionary<int, RubyThreadInfo> _mapping = new Dictionary<int, RubyThreadInfo>();
            private readonly Dictionary<SymbolId, object> _threadLocalStorage;
            private readonly int _id;
            private ThreadGroup _group;
            private readonly Thread _thread;
            private bool _abortOnException;

            private RubyThreadInfo(Thread thread) {
                _threadLocalStorage = new Dictionary<SymbolId, object>();
                _group = ThreadGroup.Default;
                _thread = thread;
                _id = thread.ManagedThreadId;
            }

            internal static RubyThreadInfo FromThread(Thread t) {
                RubyThreadInfo result;
                lock (_mapping) {
                    int key = t.ManagedThreadId;
                    if (!_mapping.TryGetValue(key, out result)) {
                        result = new RubyThreadInfo(t);
                        _mapping[key] = result;
                    }
                }
                return result;
            }

            internal static void RegisterThread(Thread t) {
                FromThread(t);
            }

            internal object this[SymbolId key] {
                get {
                    lock (_threadLocalStorage) {
                        object result;
                        if (!_threadLocalStorage.TryGetValue(key, out result)) {
                            result = null;
                        }
                        return result;
                    }
                }
                set {
                    lock (_threadLocalStorage) {
                        if (value == null) {
                            _threadLocalStorage.Remove(key);
                        } else {
                            _threadLocalStorage[key] = value;
                        }
                    }
                }
            }

            internal bool HasKey(SymbolId key) {
                lock (_threadLocalStorage) {
                    return _threadLocalStorage.ContainsKey(key);
                }
            }

            internal RubyArray GetKeys() {
                lock (_threadLocalStorage) {
                    RubyArray result = new RubyArray(_threadLocalStorage.Count);
                    foreach (SymbolId key in _threadLocalStorage.Keys) {
                        result.Add(key);
                    }
                    return result;
                }
            }

            internal ThreadGroup Group {
                get {
                    return _group;
                }
                set {
                    Interlocked.Exchange(ref _group, value);
                }
            }

            internal Thread Thread {
                get {
                    return _thread;
                }
            }

            internal Exception Exception { get; set; }
            internal object Result { get; set; }

            internal bool AbortOnException {
                get {
                    return _abortOnException;
                }
                set {
                    _abortOnException = value;
                }
            }

            internal static RubyThreadInfo[] Threads {
                get {
                    lock (_mapping) {
                        List<RubyThreadInfo> result = new List<RubyThreadInfo>(_mapping.Count);
                        foreach (KeyValuePair<int, RubyThreadInfo> entry in _mapping) {
                            if (entry.Value.Thread.IsAlive) {
                                result.Add(entry.Value);
                            }
                        }
                        return result.ToArray();
                    }
                }
            }
        }

        //  declared private instance methods:
        //    initialize
        //  declared protected instance methods:
        //  declared public instance methods:

        private static Exception MakeKeyTypeException(RubyContext/*!*/ context, object key) {
            if (key == null) {
                return RubyExceptions.CreateTypeError("nil is not a symbol");
            } else {
                MutableString repr = Protocols.ConvertToString(context, key);
                string message = String.Format("{0} is not a symbol", repr.ToString());
                return RubyExceptions.CreateArgumentError(message);
            }
        }

        [RubyMethod("[]")]
        public static object GetElement(Thread/*!*/ self, SymbolId key) {
            RubyThreadInfo info = RubyThreadInfo.FromThread(self);
            return info[key];
        }

        [RubyMethod("[]")]
        public static object GetElement(Thread/*!*/ self, [NotNull]MutableString/*!*/ key) {
            return GetElement(self, SymbolTable.StringToId(key.ConvertToString()));
        }

        [RubyMethod("[]")]
        public static object GetElement(RubyContext/*!*/ context, Thread/*!*/ self, object key) {
            throw MakeKeyTypeException(context, key);
        }

        [RubyMethod("[]=")]
        public static object SetElement(Thread/*!*/ self, SymbolId key, object value) {
            RubyThreadInfo info = RubyThreadInfo.FromThread(self);
            info[key] = value;
            return value;
        }

        [RubyMethod("[]=")]
        public static object SetElement(Thread/*!*/ self, [NotNull]MutableString/*!*/ key, object value) {
            return SetElement(self, SymbolTable.StringToId(key.ConvertToString()), value);
        }

        [RubyMethod("[]=")]
        public static object SetElement(RubyContext/*!*/ context, Thread/*!*/ self, object key, object value) {
            throw MakeKeyTypeException(context, key);
        }

        [RubyMethod("abort_on_exception")]
        public static object AbortOnException(Thread/*!*/ self) {
            RubyThreadInfo info = RubyThreadInfo.FromThread(self);
            return info.AbortOnException;
        }

        [RubyMethod("abort_on_exception=")]
        public static object AbortOnException(Thread/*!*/ self, bool value) {
            RubyThreadInfo info = RubyThreadInfo.FromThread(self);
            info.AbortOnException = value;
            return value;
        }

        [RubyMethod("alive?")]
        public static bool IsAlive(Thread/*!*/ self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            return self.IsAlive;
        }

        [RubyMethod("group")]
        public static ThreadGroup Group(Thread/*!*/ self) {
            RubyThreadInfo info = RubyThreadInfo.FromThread(self);
            return info.Group;
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, Thread/*!*/ self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);

            MutableString result = MutableString.CreateMutable();
            result.Append("#<");
            result.Append(RubyUtils.GetClassName(context, self));
            result.Append(':');
            RubyUtils.AppendFormatHexObjectId(result, RubyUtils.GetObjectId(context, self));
            result.Append(' ');

            if ((self.ThreadState & ThreadState.WaitSleepJoin) != 0) {
                result.Append("sleep");
            } else if ((self.ThreadState & (ThreadState.Stopped | ThreadState.Aborted | ThreadState.AbortRequested)) != 0) {
                result.Append("dead");
            } else {
                result.Append("run");
            }

            result.Append('>');
            return result;
        }

        [RubyMethod("join")]
        public static Thread/*!*/ Join(Thread/*!*/ self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);

            if (!(self.ThreadState == ThreadState.AbortRequested || self.ThreadState == ThreadState.Aborted)) {
                self.Join();
            }

            Exception threadException = RubyThreadInfo.FromThread(self).Exception;
            if (threadException != null) {
                throw threadException;
            }

            return self;
        }

        [RubyMethod("join")]
        public static Thread/*!*/ Join(Thread/*!*/ self, double seconds) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);

            if (!(self.ThreadState == ThreadState.AbortRequested || self.ThreadState == ThreadState.Aborted)) {
                double ms = seconds * 1000;
                int timeout = (ms < Int32.MinValue || ms > Int32.MaxValue) ? Timeout.Infinite : (int)ms;
                if (!self.Join(timeout)) {
                    return null;
                }
            }

            Exception threadException = RubyThreadInfo.FromThread(self).Exception;
            if (threadException != null) {
                throw threadException;
            }

            return self;
        }

        [RubyMethod("kill")]
        [RubyMethod("exit")]
        [RubyMethod("terminate")]
        public static Thread Kill(Thread/*!*/ self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            self.Abort();
            return self;
        }

        [RubyMethod("key?")]
        public static object HasKey(Thread/*!*/ self, SymbolId key) {
            RubyThreadInfo info = RubyThreadInfo.FromThread(self);
            return info.HasKey(key);
        }

        [RubyMethod("key?")]
        public static object HasKey(Thread/*!*/ self, [NotNull]MutableString/*!*/ key) {
            return HasKey(self, SymbolTable.StringToId(key.ConvertToString()));
        }

        [RubyMethod("key?")]
        public static object HasKey(RubyContext/*!*/ context, Thread/*!*/ self, object key) {
            throw MakeKeyTypeException(context, key);
        }

        [RubyMethod("keys")]
        public static object Keys(RubyContext/*!*/ context, Thread/*!*/ self) {
            RubyThreadInfo info = RubyThreadInfo.FromThread(self);
            return info.GetKeys();
        }

        //    priority
        //    priority=
        //    raise
        //    safe_level

        // TODO: these two methods interrupt a sleeping thread via the Thread.Interrupt API.
        // Unfortunately, this API interrupts the sleeping thread by throwing a ThreadInterruptedException.
        // In many Ruby programs (eg the specs) this causes the thread to terminate, which is NOT the
        // expected behavior. This is tracked by Rubyforge bug # 21157

#if !SILVERLIGHT
        [RubyMethod("run", BuildConfig = "!SILVERLIGHT")]
        [RubyMethod("wakeup", BuildConfig = "!SILVERLIGHT")]
        public static void Run(Thread/*!*/ self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            self.Interrupt();
        }
#endif

        [RubyMethod("status")]
        public static object Status(Thread/*!*/ self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            switch (self.ThreadState) {
                case ThreadState.WaitSleepJoin:
                    return MutableString.Create("sleep");
                case ThreadState.Running:
                    return MutableString.Create("run");
                case ThreadState.Aborted:
                case ThreadState.AbortRequested:
                    return null;
                case ThreadState.Stopped:
                case ThreadState.StopRequested:
                    return false;
                default:
                    throw new ArgumentException("unknown thread status: " + self.ThreadState.ToString());
            }
        }

        [RubyMethod("value")]
        public static object Value(Thread/*!*/ self) {
            Join(self);
            return RubyThreadInfo.FromThread(self).Result;
        }

        //    stop?

        //  declared singleton methods

        [RubyMethod("abort_on_exception", RubyMethodAttributes.PublicSingleton)]
        public static object GlobalAbortOnException(object self) {
            return _globalAbortOnException;
        }

        [RubyMethod("abort_on_exception=", RubyMethodAttributes.PublicSingleton)]
        public static object GlobalAbortOnException(object self, bool value) {
            _globalAbortOnException = value;
            return value;
        }

        //    critical
        //    critical=

        [RubyMethod("critical", RubyMethodAttributes.PublicSingleton)]
        public static bool Critical(object self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            return false;
        }

        [RubyMethod("critical=", RubyMethodAttributes.PublicSingleton)]
        public static bool Critical(object self, bool value) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            return false;
        }

        [RubyMethod("current", RubyMethodAttributes.PublicSingleton)]
        public static Thread/*!*/ Current(object self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            return Thread.CurrentThread;
        }

        //    exclusive
        //    fork
        [RubyMethod("list", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ List(object self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);

            RubyThreadInfo[] threads = RubyThreadInfo.Threads;
            RubyArray result = new RubyArray(threads.Length);
            foreach (RubyThreadInfo threadInfo in threads) {
                Thread thread = threadInfo.Thread;
                if (thread != null) {
                    result.Add(thread);
                }
            }

            return result;
        }

        //    main

        [RubyMethod("new", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("start", RubyMethodAttributes.PublicSingleton)]
        public static Thread/*!*/ CreateThread(RubyContext/*!*/ context, BlockParam startRoutine, object self, [NotNull]params object[]/*!*/ args) {
            if (startRoutine == null) {
                throw new ThreadError("must be called with a block");
            }
            ThreadGroup group = Group(Thread.CurrentThread);
            Thread result = new Thread(new ThreadStart(delegate() {
                RubyThreadInfo info = RubyThreadInfo.FromThread(Thread.CurrentThread);
                info.Group = group;

                try {
                    object threadResult;
                    // TODO: break?
                    startRoutine.Yield(args, out threadResult);
                    info.Result = threadResult;
#if !SILVERLIGHT
                } catch (ThreadInterruptedException) {
                    // Do nothing with this for now
#endif
                } catch (Exception e) {
                    info.Exception = e;

                    Utils.Log(
                        e.Message + "\r\n\r\n" + 
                        e.StackTrace + "\r\n\r\n" + 
                        IListOps.Join(context, RubyExceptionData.GetInstance(e).Backtrace).ToString(), 
                        "THREAD"
                    );

                    if (_globalAbortOnException || info.AbortOnException) {
                        throw;
                    }
                }
            }));

            result.Start();
            return result;
        }

        [RubyMethod("pass", RubyMethodAttributes.PublicSingleton)]
        public static void Yield(object self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            Thread.Sleep(0);
        }

        [RubyMethod("stop", RubyMethodAttributes.PublicSingleton)]
        public static void Stop(object self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            // TODO: MRI throws an exception if you try to stop the main thread
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
