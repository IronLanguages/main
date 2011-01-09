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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    using Debug = System.Diagnostics.Debug;

    /// <summary>
    /// Ruby threads are represented by CLR thread objects (System.Threading.Thread).
    /// Ruby 1.8.N has green threads where the language does the thread scheduling. We map the green threads 
    /// directly to CLR threads.
    /// 
    /// Ruby supports asynchronously manipulating of an arbitrary thread with methods like Thread#raise, Thread#exit, etc.
    /// For such methods, we use Thread.Abort which is unsafe. Howevever, Ruby 1.9 may not support green threads,
    /// and this will not be an issue then.
    /// </summary>
    [RubyClass("Thread", Extends = typeof(Thread), Inherits = typeof(object))]
    public static class ThreadOps {
        static bool _globalAbortOnException;

        /// <summary>
        /// The ThreadState enumeration is a flag, and multiple values could be set simultaneously. Also,
        /// there is other state that IronRuby tracks. RubyThreadStatus flattens out the different states
        /// into non-overlapping values.
        /// </summary>
        private enum RubyThreadStatus {
            /// <summary>
            /// Ruby does not expose such a state. However, since IronRuby uses CLR threads, this state can exist for
            /// threads that are not created directly from Ruby code
            /// </summary>
            Unstarted,

            Running,

            Sleeping,

            Completed,

            /// <summary>
            /// If Thread#kill has been called, and the thread is not sleeping
            /// </summary>
            Aborting,

            /// <summary>
            /// An unhandled exception was thrown by the thread
            /// </summary>
            Aborted
        }

        internal class RubyThreadInfo {
            private static readonly Dictionary<int, RubyThreadInfo> _mapping = new Dictionary<int, RubyThreadInfo>();
            private readonly Dictionary<RubySymbol, object> _threadLocalStorage;
            private ThreadGroup _group;
            private readonly Thread _thread;
            private bool _blocked;
            private bool _abortOnException;
            private AutoResetEvent _runSignal = new AutoResetEvent(false);
            private bool _isSleeping;

            private RubyThreadInfo(Thread thread) {
                _threadLocalStorage = new Dictionary<RubySymbol, object>();
                _group = ThreadGroup.Default;
                _thread = thread;
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

            internal object this[RubySymbol/*!*/ key] {
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

            internal bool HasKey(RubySymbol/*!*/ key) {
                lock (_threadLocalStorage) {
                    return _threadLocalStorage.ContainsKey(key);
                }
            }

            internal RubyArray GetKeys() {
                lock (_threadLocalStorage) {
                    RubyArray result = new RubyArray(_threadLocalStorage.Count);
                    foreach (RubySymbol key in _threadLocalStorage.Keys) {
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
            internal bool CreatedFromRuby { get; set; }
            internal bool ExitRequested { get; set; }
            
            internal bool Blocked {
                get {
                    return _blocked;
                }
                set {
                    System.Diagnostics.Debug.Assert(Thread.CurrentThread == _thread);
                    _blocked = value;
                }
            }

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

            /// <summary>
            /// We do not use Thread.Sleep here as another thread can call Thread#wakeup/Thread#run. Instead, we use our own
            /// lock which can be signalled from another thread.
            /// </summary>
            internal void Sleep() {
                try {
                    _isSleeping = true;
                    _runSignal.WaitOne();
                } finally {
                    _isSleeping = false;
                }
            }

            internal void Run() {
                if (_isSleeping) {
                    _runSignal.Set();
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
                // MRI calls RubyUtils.InspectObject, but this should be good enought as an error message:
                return RubyExceptions.CreateArgumentError("{0} is not a symbol", context.GetClassOf(key).Name);
            }
        }

        [RubyMethod("[]")]
        public static object GetElement(Thread/*!*/ self, [NotNull]RubySymbol/*!*/ key) {
            RubyThreadInfo info = RubyThreadInfo.FromThread(self);
            return info[key];
        }

        [RubyMethod("[]")]
        public static object GetElement(RubyContext/*!*/ context, Thread/*!*/ self, [NotNull]MutableString/*!*/ key) {
            return GetElement(self, context.CreateSymbol(key));
        }

        [RubyMethod("[]")]
        public static object GetElement(RubyContext/*!*/ context, Thread/*!*/ self, object key) {
            throw MakeKeyTypeException(context, key);
        }

        [RubyMethod("[]=")]
        public static object SetElement(Thread/*!*/ self, [NotNull]RubySymbol/*!*/ key, object value) {
            RubyThreadInfo info = RubyThreadInfo.FromThread(self);
            info[key] = value;
            return value;
        }

        [RubyMethod("[]=")]
        public static object SetElement(RubyContext/*!*/ context, Thread/*!*/ self, [NotNull]MutableString/*!*/ key, object value) {
            return SetElement(self, context.CreateSymbol(key), value);
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

            MutableString result = MutableString.CreateMutable(context.GetIdentifierEncoding());
            result.Append("#<");
            result.Append(context.GetClassDisplayName(self));
            result.Append(':');
            RubyUtils.AppendFormatHexObjectId(result, RubyUtils.GetObjectId(context, self));
            result.Append(' ');

            RubyThreadStatus status = GetStatus(self);
            switch (status) {
                case RubyThreadStatus.Unstarted:
                    result.Append("unstarted");
                    break;
                case RubyThreadStatus.Running:
                    result.Append("run");
                    break;
                case RubyThreadStatus.Sleeping:
                    result.Append("sleep");
                    break;
                case RubyThreadStatus.Aborting:
                    result.Append("aborting");
                    break;
                case RubyThreadStatus.Completed:
                case RubyThreadStatus.Aborted:
                    result.Append("dead");
                    break;
            }

            result.Append('>');
            return result;
        }

        [RubyMethod("join")]
        public static Thread/*!*/ Join(Thread/*!*/ self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);

            self.Join();

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
            RubyThreadInfo info = RubyThreadInfo.FromThread(self);
            if (GetStatus(self) == RubyThreadStatus.Sleeping && info.ExitRequested) {
                // Thread must be sleeping in an ensure clause. Wake up the thread and allow ensure clause to complete
                info.Run();
                return self;
            }

            info.ExitRequested = true;
            RubyUtils.ExitThread(self);
            return self;
        }

        [RubyMethod("key?")]
        public static object HasKey(Thread/*!*/ self, [NotNull]RubySymbol/*!*/ key) {
            RubyThreadInfo info = RubyThreadInfo.FromThread(self);
            return info.HasKey(key);
        }

        [RubyMethod("key?")]
        public static object HasKey(RubyContext/*!*/ context, Thread/*!*/ self, [NotNull]MutableString/*!*/ key) {
            return HasKey(self, context.CreateSymbol(key));
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

#if !SILVERLIGHT
        #region priority, priority=
        [RubyMethod("priority", BuildConfig = "!SILVERLIGHT")]
        public static object Priority(Thread/*!*/ self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            switch (self.Priority) {
                case ThreadPriority.Lowest:
                    return -2;
                case ThreadPriority.BelowNormal:
                    return -1;
                case ThreadPriority.Normal:
                    return 0;
                case ThreadPriority.AboveNormal:
                    return 1;
                case ThreadPriority.Highest:
                    return 2;
                default:
                    return 0;
            }
        }

        [RubyMethod("priority=", BuildConfig = "!SILVERLIGHT")]
        public static Thread Priority(Thread/*!*/ self, int priority) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            if (priority <= -2)
                self.Priority = ThreadPriority.Lowest;
            else if (priority == -1)
                self.Priority = ThreadPriority.BelowNormal;
            else if (priority == 0)
                self.Priority = ThreadPriority.Normal;
            else if (priority == 1)
                self.Priority = ThreadPriority.AboveNormal;
            else
                self.Priority = ThreadPriority.Highest;

            return self;
        }
        #endregion
#endif
        #region raise, fail

#if !SILVERLIGHT
        private static void RaiseAsyncException(Thread thread, Exception exception) {
            RubyThreadStatus status = GetStatus(thread);

            // rethrow semantics, preserves the backtrace associated with the exception:
            RubyUtils.RaiseAsyncException(thread, exception);

            if (status == RubyThreadStatus.Sleeping) {
                // Thread.Abort can interrupt a thread with ThreadState.WaitSleepJoin. However, Thread.Abort 
                // is deferred while the thread is in a catch block. If there is a Kernel.sleep in a catch block,
                // then that sleep will not be interrupted. 
                // TODO: We should call Run to nudge the thread if its CurrentException is not-null, and 
                // ThreadOps.Stop should have a checkpoint to see whether an async exception needs to be thrown

                // Run(thread);
            }
        }
#endif

        [RubyMethod("raise")]
        [RubyStackTraceHidden]
        public static void RaiseException(RubyContext/*!*/ context, Thread/*!*/ self) {
            if (self == Thread.CurrentThread) {
                KernelOps.RaiseException(context, self);
                return;
            }

#if SILVERLIGHT
            throw new NotImplementedError("Thread#raise is not implemented on Silverlight");
#else
            // TODO: RubyContext.CurrentException is a thread-local static, and cannot be accessed from other threads
            // To fix this, it would have to be stored somehow without using ThreadStaticAttribute
            // For now, we just throw a RuntimeError
            RaiseAsyncException(self, new RuntimeError());
#endif
        }

        [RubyMethod("raise")]
        [RubyStackTraceHidden]
        public static void RaiseException(Thread/*!*/ self, [NotNull]MutableString/*!*/ message) {
            if (self == Thread.CurrentThread) {
                KernelOps.RaiseException(self, message);
                return;
            }

#if SILVERLIGHT
            throw new NotImplementedError("Thread#raise is not implemented on Silverlight");
#else
            Exception e = RubyExceptionData.InitializeException(new RuntimeError(message.ToString()), message);
            RaiseAsyncException(self, e);
#endif
        }

        [RubyMethod("raise")]
        [RubyStackTraceHidden]
        public static void RaiseException(RespondToStorage/*!*/ respondToStorage, UnaryOpStorage/*!*/ storage0, BinaryOpStorage/*!*/ storage1, 
            CallSiteStorage<Action<CallSite, Exception, RubyArray>>/*!*/ setBackTraceStorage, 
            Thread/*!*/ self, object/*!*/ obj, [Optional]object arg, [Optional]RubyArray backtrace) {

            if (self == Thread.CurrentThread) {
                KernelOps.RaiseException(respondToStorage, storage0, storage1, setBackTraceStorage, self, obj, arg, backtrace);
                return;
            }

#if SILVERLIGHT
            throw new NotImplementedError("Thread#raise is not implemented on Silverlight");
#else
            Exception e = KernelOps.CreateExceptionToRaise(respondToStorage, storage0, storage1, setBackTraceStorage, obj, arg, backtrace);
            RaiseAsyncException(self, e);
#endif
        }

        #endregion

        //    safe_level

        // TODO: these two methods interrupt a sleeping thread via the Thread.Interrupt API.
        // Unfortunately, this API interrupts the sleeping thread by throwing a ThreadInterruptedException.
        // In many Ruby programs (eg the specs) this causes the thread to terminate, which is NOT the
        // expected behavior. This is tracked by Rubyforge bug # 21157

#if !SILVERLIGHT
        [RubyMethod("run", BuildConfig = "!SILVERLIGHT")]
        [RubyMethod("wakeup", BuildConfig = "!SILVERLIGHT")]
        public static Thread Run(Thread/*!*/ self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            RubyThreadInfo info = RubyThreadInfo.FromThread(self);
            info.Run();
            return self;
        }
#endif

        private static RubyThreadStatus GetStatus(Thread thread) {
            ThreadState state = thread.ThreadState;
            RubyThreadInfo info = RubyThreadInfo.FromThread(thread);

            if ((state & ThreadState.Unstarted) == ThreadState.Unstarted) {
                if (info.CreatedFromRuby) {
                    // Ruby threads do not have an unstarted status. We must be in the tiny window when ThreadOps.CreateThread
                    // created the thread, but has not called Thread.Start on it yet.
                    return RubyThreadStatus.Running;
                } else {
                    // This is a thread created from outside Ruby. In such a case, we do not know when Thread.Start
                    // will be called on it. So we report it as unstarted.
                    return RubyThreadStatus.Unstarted;
                }
            }

            if ((state & (ThreadState.Stopped|ThreadState.Aborted)) != 0) {
                if (RubyThreadInfo.FromThread(thread).Exception == null) {
                    return RubyThreadStatus.Completed;
                } else {
                    return RubyThreadStatus.Aborted;
                }
            }

            if ((state & ThreadState.WaitSleepJoin) == ThreadState.WaitSleepJoin) {
                // We will report a thread to be sleeping more often than in CRuby. This is because any "lock" statement
                // can potentially cause ThreadState.WaitSleepJoin. Also, "Thread.pass" does System.Threading.Thread.Sleep(0)
                // which also briefly changes the state to ThreadState.WaitSleepJoin
                return RubyThreadStatus.Sleeping;
            }

            if ((state & ThreadState.AbortRequested) != 0) {
                return RubyThreadStatus.Aborting;
            }

            if ((state & ThreadState.Running) == ThreadState.Running) {
                if (info.Blocked) {
                    return RubyThreadStatus.Sleeping;
                } else {
                    return RubyThreadStatus.Running;
                }
            }

#pragma warning disable 162 // msc: unreachable code
            throw new ArgumentException("unknown thread status: " + state);
#pragma warning restore 162
        }

        [RubyMethod("status")]
        public static object Status(Thread/*!*/ self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            switch (GetStatus(self)) {
                case RubyThreadStatus.Unstarted:
                    return MutableString.CreateAscii("unstarted");
                case RubyThreadStatus.Running:
                    return MutableString.CreateAscii("run");
                case RubyThreadStatus.Sleeping:
                    return MutableString.CreateAscii("sleep");
                case RubyThreadStatus.Aborting:
                    return MutableString.CreateAscii("aborting");
                case RubyThreadStatus.Completed:
                    return false;
                case RubyThreadStatus.Aborted:
                    return null;
                default:
                    throw new ArgumentException("unknown thread status");
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

        private static void SetCritical(RubyContext/*!*/ context, bool value) {
            // Debug.Assert(context.RubyOptions.Compatibility < RubyCompatibility.Ruby19);
            if (value) {
                bool lockTaken = false;
                try {
                    MonitorUtils.Enter(context.CriticalMonitor, ref lockTaken);
                } finally {
                    // thread could have been aborted just before/after Monitor.Enter acquired the lock:
                    if (lockTaken) {
                        context.CriticalThread = Thread.CurrentThread;
                    }
                }
            } else {
                Monitor.Exit(context.CriticalMonitor);
                context.CriticalThread = null;
            }
        }

        [RubyMethod("critical", RubyMethodAttributes.PublicSingleton)] // Compatibility <= RubyCompatibility.Ruby18
        public static bool Critical(RubyContext/*!*/ context, object self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            return context.CriticalThread != null;
        }

        [RubyMethod("critical=", RubyMethodAttributes.PublicSingleton)]
        public static void Critical(RubyContext/*!*/ context, object self, bool value) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            SetCritical(context, value);
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

        [RubyMethod("main", RubyMethodAttributes.PublicSingleton)]
        public static Thread/*!*/ GetMainThread(RubyContext/*!*/ context, RubyClass self) {
            return context.MainThread;
        }

        [RubyMethod("new", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("start", RubyMethodAttributes.PublicSingleton)]
        public static Thread/*!*/ CreateThread(RubyContext/*!*/ context, BlockParam startRoutine, object self, params object[]/*!*/ args) {
            if (startRoutine == null) {
                throw new ThreadError("must be called with a block");
            }
            ThreadGroup group = Group(Thread.CurrentThread);
            Thread result = new Thread(new ThreadStart(() => RubyThreadStart(context, startRoutine, args, group)));

            // Ruby exits when the main thread exits. So all other threads need to be marked as background threads
            result.IsBackground = true;

            result.Start();
            return result;
        }

        private static void RubyThreadStart(RubyContext/*!*/ context, BlockParam/*!*/ startRoutine, object[]/*!*/ args, ThreadGroup group) {
            RubyThreadInfo info = RubyThreadInfo.FromThread(Thread.CurrentThread);
            info.CreatedFromRuby = true;

            info.Group = group;

            try {
                object threadResult;
                // TODO: break/returns might throw LocalJumpError if the RFC that was created for startRoutine is not active anymore:
                if (startRoutine.Yield(args, out threadResult) && startRoutine.Returning(threadResult, out threadResult)) {
                    info.Exception = new ThreadError("return can't jump across threads");
                }
                info.Result = threadResult;
            } catch (MethodUnwinder) {
                info.Exception = new ThreadError("return can't jump across threads");
            } catch (Exception e) {
                if (info.ExitRequested) {
                    // Note that "e" may not be ThreadAbortException at this point If an exception was raised from a finally block,
                    // we will get that here instead
                    Utils.Log(String.Format("Thread {0} exited.", info.Thread.ManagedThreadId), "THREAD");
                    info.Result = false;
#if !SILVERLIGHT
                    Thread.ResetAbort();
#endif
                } else {
                    e = RubyUtils.GetVisibleException(e);
                    RubyExceptionData.ActiveExceptionHandled(e);
                    info.Exception = e;

                    StringBuilder trace = new StringBuilder();
                    trace.Append(e.Message);
                    trace.AppendLine();
                    trace.AppendLine();
                    trace.Append(e.StackTrace);
                    trace.AppendLine();
                    trace.AppendLine();
                    RubyExceptionData data = RubyExceptionData.GetInstance(e);
                    if (data.Backtrace != null) {
                        foreach (var frame in data.Backtrace) {
                            trace.Append(frame.ToString());
                        }
                    }

                    Utils.Log(trace.ToString(), "THREAD");

                    if (_globalAbortOnException || info.AbortOnException) {
                        throw;
                    }
                }
            } finally {
                // Its not a good idea to terminate a thread which has set Thread.critical=true, but its hard to predict
                // which thread will be scheduled next, even with green threads. However, ConditionVariable.create_timer 
                // in monitor.rb explicitly does "Thread.critical=true; other_thread.raise" before exiting, and expects
                // other_thread to be scheduled immediately.
                // To deal with such code, we release the critical monitor here if the current thread is holding it
                if (context.RubyOptions.Compatibility < RubyCompatibility.Ruby19 && context.CriticalThread == Thread.CurrentThread) {
                    SetCritical(context, false);
                }
            }
        }

        [RubyMethod("pass", RubyMethodAttributes.PublicSingleton)]
        public static void Yield(object self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            Thread.Sleep(0);
        }

        [RubyMethod("stop", RubyMethodAttributes.PublicSingleton)]
        public static void Stop(RubyContext/*!*/ context, object self) {
            if (context.CriticalThread == Thread.CurrentThread) {
                SetCritical(context, false);
            }
            DoSleep();
        }

        internal static void DoSleep() {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            // TODO: MRI throws an exception if you try to stop the main thread
            RubyThreadInfo info = RubyThreadInfo.FromThread(Thread.CurrentThread);
            info.Sleep();
        }

        [RubyMethod("stop?", RubyMethodAttributes.PublicInstance)]
        public static bool IsStopped(Thread self) {
            RubyThreadInfo.RegisterThread(Thread.CurrentThread);
            RubyThreadStatus status = GetStatus(self);
            return status == RubyThreadStatus.Sleeping || status == RubyThreadStatus.Completed || status == RubyThreadStatus.Aborted;
        }
    }
}
