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
// debug only:
// #define USE_SNIPPETS

#if !SILVERLIGHT
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Dynamic;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronRuby.Runtime.Conversions;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;

namespace IronRuby.StandardLibrary.Win32API {
    using Ast = Expression;
    using AstExpressions = ReadOnlyCollectionBuilder<Expression>;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    [RubyClass("Win32API", BuildConfig = "!SILVERLIGHT")]
    public class Win32API : RubyObject {
        private enum ArgType : byte {
            // ignored
            None,
            // to_str conversion
            Buffer,
            // to_int conversion
            Int32,
        }

        private static int _Version = 1;

        private int _version;
        private IntPtr _function;
        private ArgType[] _signature;  
        private ArgType _returnType;
        private MethodInfo _calliStub;
        private readonly RubyContext/*!*/ _context;

        public Win32API(RubyClass/*!*/ cls) 
            : base(cls) {
            // invalid:
            _context = cls.Context;
            _version = 0;
        }

        private Win32API/*!*/ Reinitialize(IntPtr function, ArgType[] signature, ArgType returnType) {
            if (IntPtr.Size != 4) {
                throw new NotSupportedException("Win32API is not supported in 64-bit process");
            }

            Debug.Assert(function != IntPtr.Zero && signature != null);
            _function = function;
            _signature = signature;
            _returnType = returnType;
            _version = Interlocked.Increment(ref _Version);
            return this;
        }

        #region Helpers

        [Emitted]
        public int Version {
            get { return _version; }
        }

        [Emitted]
        public IntPtr Function {
            get { return _function; }
        }

        private static readonly PropertyInfo VersionProperty = typeof(Win32API).GetProperty("Version");
        private static readonly PropertyInfo FunctionProperty = typeof(Win32API).GetProperty("Function");

        private static ArgType ToArgType(byte b) {
            switch ((char)b) {
                case 'i':
                case 'l':
                case 'n':
                case 'I':
                case 'L':
                case 'N':
                    return ArgType.Int32;

                case 'p':
                case 'P':
                    return ArgType.Buffer;

                default:
                    return ArgType.None;
            }
        }

        private static Type/*!*/ ToNativeType(ArgType argType) {
            switch (argType) {
                case ArgType.Buffer: return typeof(byte[]);
                case ArgType.Int32: return typeof(int);
                case ArgType.None: return typeof(void);
            }
            throw Assert.Unreachable;
        }

        private static ArgType[]/*!*/ MakeSignature(int size, Func<int, byte>/*!*/ getByte) {
            ArgType[] signature = new ArgType[size];
            int j = 0;
            for (int i = 0; i < size; i++) {
                var argType = ToArgType(getByte(i));
                if (argType != ArgType.None) {
                    signature[j++] = argType;
                }
            }

            if (j != signature.Length) {
                Array.Resize(ref signature, j);
            }
            return signature;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr module, string lpProcName);

        private static IntPtr GetFunction(MutableString/*!*/ libraryName, MutableString/*!*/ functionName) {
            IntPtr library = LoadLibrary(libraryName.ConvertToString());
            if (library == IntPtr.Zero) {
                throw new Win32Exception();
            }

            string procName = functionName.ConvertToString();
            IntPtr function = GetProcAddress(library, procName);
            if (function == IntPtr.Zero) {
                function = GetProcAddress(library, procName + "A");
                if (library == IntPtr.Zero) {
                    throw new Win32Exception();
                }
            }

            return function;
        }

        #endregion

        #region Ruby API

        [RubyConstructor]
        public static Win32API/*!*/ Create(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ libraryName, 
            [DefaultProtocol, NotNull]MutableString/*!*/ functionName, 
            [DefaultProtocol, NotNull]MutableString/*!*/ parameterTypes, 
            [DefaultProtocol, NotNull]MutableString/*!*/ returnType) {

            return Reinitialize(new Win32API(self), libraryName, functionName, parameterTypes, returnType);
        }

        [RubyConstructor(Compatibility=RubyCompatibility.Ruby19)]
        public static Win32API/*!*/ Create(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ libraryName,
            [DefaultProtocol, NotNull]MutableString/*!*/ functionName,
            [DefaultProtocol, NotNull]MutableString/*!*/ parameterTypes,
            [DefaultProtocol, NotNull]MutableString/*!*/ returnType,
            RubySymbol callingConvention) {

            Debug.Assert(callingConvention.ToString() == "stdcall");
            return Reinitialize(new Win32API(self), libraryName, functionName, parameterTypes, returnType);
        }

        [RubyConstructor]
        public static Win32API/*!*/ Create(
            ConversionStorage<MutableString>/*!*/ toStr, 
            RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ libraryName,
            [DefaultProtocol, NotNull]MutableString/*!*/ functionName,
            [NotNull]IList/*!*/ parameterTypes,
            [DefaultProtocol, NotNull]MutableString/*!*/ returnType) {

            return Reinitialize(toStr, new Win32API(self), libraryName, functionName, parameterTypes, returnType);
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static Win32API/*!*/ Reinitialize(Win32API/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ libraryName,
            [DefaultProtocol, NotNull]MutableString/*!*/ functionName,
            [DefaultProtocol, NotNull]MutableString/*!*/ parameterTypes,
            [DefaultProtocol, NotNull]MutableString/*!*/ returnType) {

            return self.Reinitialize(
                GetFunction(libraryName, functionName),
                MakeSignature(parameterTypes.GetByteCount(), parameterTypes.GetByte),
                returnType.IsEmpty ? ArgType.None : ToArgType(returnType.GetByte(0))
            );
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static Win32API/*!*/ Reinitialize(
            ConversionStorage<MutableString>/*!*/ toStr,
            Win32API/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ libraryName,
            [DefaultProtocol, NotNull]MutableString/*!*/ functionName,
            [NotNull]IList/*!*/ parameterTypes,
            [DefaultProtocol, NotNull]MutableString/*!*/ returnType) {

            return self.Reinitialize(
                GetFunction(libraryName, functionName),
                MakeSignature(parameterTypes.Count, (i) => {
                    var str = Protocols.CastToString(toStr, parameterTypes[i]);
                    return str.IsEmpty ? (byte)0 : str.GetByte(0);
                }),
                returnType.IsEmpty ? ArgType.None : ToArgType(returnType.GetByte(0))
            );
        }

        [RubyMethod("call")]
        [RubyMethod("Call")]
        public static RuleGenerator/*!*/ Call() {
            return new RuleGenerator((metaBuilder, args, name) => ((Win32API)args.Target).BuildCall(metaBuilder, args, name));
        }

        #endregion

        #region Dynamic Call

        private void BuildCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            var actualArgs = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, Int32.MaxValue);
            if (metaBuilder.Error) {
                return;
            }
            
            metaBuilder.AddRestriction(
                Ast.Equal(
                    Ast.Property(Ast.Convert(args.TargetExpression, typeof(Win32API)), VersionProperty),
                    Ast.Constant(_version)
                )
            );
            
            if (_function == IntPtr.Zero) {
                metaBuilder.SetError(Ast.Throw(new Func<Exception>(UninitializedFunctionError).Method.OpCall(), typeof(object)));
                return;
            }

            if (_signature.Length != actualArgs.Count) {
                metaBuilder.SetError(Ast.Throw(new Func<int, int, Exception>(InvalidParameterCountError).Method.OpCall(
                    Ast.Constant(_signature.Length), Ast.Constant(actualArgs.Count)), typeof(object)
                ));
                return;
            }

            var calliArgs = new AstExpressions();
            calliArgs.Add(Ast.Property(Ast.Convert(args.TargetExpression, typeof(Win32API)), FunctionProperty));
            for (int i = 0; i < actualArgs.Count; i++) {
                calliArgs.Add(MarshalArgument(metaBuilder, actualArgs[i], _signature[i]));
            }

            metaBuilder.Result = Ast.Call(EmitCalliStub(), calliArgs);

            // MRI returns 0 if void return type is given:
            if (_returnType == ArgType.None) {
                metaBuilder.Result = Ast.Block(metaBuilder.Result, AstUtils.Constant(0));
            }
        }

        private Expression/*!*/ MarshalArgument(MetaObjectBuilder/*!*/ metaBuilder, DynamicMetaObject/*!*/ arg, ArgType parameterType) {
            object value = arg.Value;
            if (value == null) {
                metaBuilder.AddRestriction(Ast.Equal(arg.Expression, AstUtils.Constant(null)));
            } else {
                metaBuilder.AddTypeRestriction(value.GetType(), arg.Expression);
            }

            switch (parameterType) {
                case ArgType.Buffer:
                    if (value == null) {
                        return AstUtils.Constant(null, typeof(byte[]));
                    }

                    if (value is int && (int)value == 0) {
                        metaBuilder.AddRestriction(Ast.Equal(AstUtils.Convert(arg.Expression, typeof(int)), AstUtils.Constant(0)));
                        return AstUtils.Constant(null, typeof(byte[]));
                    }

                    if (value.GetType() == typeof(MutableString)) {
                        return Methods.GetMutableStringBytes.OpCall(
                            AstUtils.Convert(arg.Expression, typeof(MutableString))
                        );
                    } 
                    
                    return Methods.GetMutableStringBytes.OpCall(
                        AstUtils.LightDynamic(ConvertToStrAction.Make(_context), typeof(MutableString), arg.Expression)
                    );

                case ArgType.Int32:
                    if (value is int) {
                        return AstUtils.Convert(arg.Expression, typeof(int));
                    }

                    return Ast.Convert(
                        Ast.Call(
                            AstUtils.LightDynamic(ConvertToIntAction.Make(_context), typeof(IntegerValue), arg.Expression), 
                            Methods.IntegerValue_ToUInt32Unchecked
                        ), 
                        typeof(int)
                    );
            }
            throw Assert.Unreachable;
        }

        [Emitted]
        public static Exception/*!*/ UninitializedFunctionError() {
            return RubyExceptions.CreateRuntimeError("uninitialized Win32 function");
        }

        [Emitted]
        public static Exception/*!*/ InvalidParameterCountError(int expected, int actual) {
            return RubyExceptions.CreateRuntimeError("wrong number of parameters: expected {0}, got {1}", expected, actual);
        }

        #endregion

        #region Calli Stubs

        private MethodInfo/*!*/ EmitCalliStub() {
            if (_calliStub != null) {
                return _calliStub;
            }

            var returnType = ToNativeType(_returnType);
            var parameterTypes = new Type[1 + _signature.Length];

            // target function ptr:
            parameterTypes[0] = typeof(IntPtr);

            // calli args:
            for (int i = 0; i < _signature.Length; i++) {
                parameterTypes[1 + i] = ToNativeType(_signature[i]);
            }

#if USE_SNIPPETS
            TypeGen tg = Snippets.Shared.DefineType("calli", typeof(object), false, false);
            MethodBuilder dm = tg.TypeBuilder.DefineMethod("calli", CompilerHelpers.PublicStatic, returnType, parameterTypes);
#else
            DynamicMethod dm = new DynamicMethod("calli", returnType, parameterTypes, DynamicModule);
#endif

            var il = dm.GetILGenerator();
            var signature = SignatureHelper.GetMethodSigHelper(CallingConvention.Winapi, returnType);

            // calli args:
            for (int i = 1; i < parameterTypes.Length; i++) {
                il.Emit(OpCodes.Ldarg, i);
                signature.AddArgument(parameterTypes[i]);
            }

            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Calli, signature);
            il.Emit(OpCodes.Ret);

#if USE_SNIPPETS
            return _calliStub = tg.TypeBuilder.CreateType().GetMethod("calli");
#else
            return _calliStub = dm;
#endif
        }

        private static ModuleBuilder _dynamicModule;                                      // the dynamic module we generate unsafe code into
        private static readonly object _lock = new object();                              // lock for creating dynamic module for unsafe code

        /// <summary>
        /// Gets the ModuleBuilder used to generate our unsafe call stubs into.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.AppDomain.#DefineDynamicAssembly(System.Reflection.AssemblyName,System.Reflection.Emit.AssemblyBuilderAccess,System.Collections.Generic.IEnumerable`1<System.Reflection.Emit.CustomAttributeBuilder>)")]
        private static ModuleBuilder DynamicModule {
            get {
                if (_dynamicModule == null) {
                    lock (_lock) {
                        if (_dynamicModule == null) {
                            var attributes = new[] { 
                                new CustomAttributeBuilder(typeof(UnverifiableCodeAttribute).GetConstructor(Type.EmptyTypes), new object[0]),
                                //PermissionSet(SecurityAction.Demand, Unrestricted = true)
                                new CustomAttributeBuilder(typeof(PermissionSetAttribute).GetConstructor(new Type[] { typeof(SecurityAction) }), 
                                    new object[]{ SecurityAction.Demand },
                                    new PropertyInfo[] { typeof(PermissionSetAttribute).GetProperty("Unrestricted") }, 
                                    new object[] { true }
                                )
                            };

                            string name = typeof(Win32API).Namespace + ".DynamicAssembly";
                            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run, attributes);
                            assembly.DefineVersionInfoResource();
                            _dynamicModule = assembly.DefineDynamicModule(name);
                        }
                    }
                }

                return _dynamicModule;
            }
        }

        #endregion
    }
}
#endif