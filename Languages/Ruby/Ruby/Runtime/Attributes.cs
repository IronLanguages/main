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
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Builtins;

namespace IronRuby.Runtime {

    public abstract class RubyAttribute : Attribute {
        private string _buildConfig;
        private RubyCompatibility _compatibility = RubyCompatibility.Default;

        /// <summary>
        /// If set, indicates what build configurations this module should be available under
        /// Can be any string that is valid after a #if
        /// Default is to be available under all configurations
        /// 
        /// typical usage: BuildConfig = "!SILVERLIGHT"
        /// 
        /// TODO: is there a better way to do this?
        /// </summary>
        public string BuildConfig {
            get { return _buildConfig; }
            set { _buildConfig = value; }
        }

        /// <summary>
        /// Which language version supports the feature
        /// </summary>
        public RubyCompatibility Compatibility {
            get { return _compatibility; }
            set { _compatibility = value; }
        }
    }

    /// <summary>
    /// Applied to assemblies containing Ruby library methods.
    /// Specifies an initializer for the library, which is a type that publishes RubyModules and RubyClasses defined in the assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RubyLibraryAttribute : Attribute {
        private Type/*!*/ _initializer;

        public Type/*!*/ Initializer {
            get { return _initializer; }
        }

        public RubyLibraryAttribute(Type/*!*/ initializer) {
            ContractUtils.RequiresNotNull(initializer, "initializer");
            _initializer = initializer;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RubyConstantAttribute : RubyAttribute {
        private readonly string _name;

        public string Name { get { return _name; } }

        public RubyConstantAttribute() {
        }

        public RubyConstantAttribute(string/*!*/ name) {
            ContractUtils.RequiresNotNull(name, "name");
            _name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RubyMethodAttribute : RubyAttribute {
        private readonly string/*!*/ _name;
        private readonly RubyMethodAttributes _methodAttributes;

        /// <summary>
        /// RubyCompatibility is encoded along with the method attributes in the initialization code
        /// </summary>
        internal const int CompatibilityEncodingShift = 16;

        public string/*!*/ Name {
            get { return _name; }
        }

        public RubyMethodAttributes MethodAttributes {
            get { return _methodAttributes; }
        }
        
        public RubyMethodAttribute(string/*!*/ name)
            : this(name, RubyMethodAttributes.Default) {
        }

        public RubyMethodAttribute(string/*!*/ name, RubyMethodAttributes methodAttributes)
            : base() {
            ContractUtils.RequiresNotNull(name, "name");

            _name = name;
            _methodAttributes = methodAttributes;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RubyConstructorAttribute : RubyAttribute {
        public RubyConstructorAttribute() {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class DeclaresRubyModuleAttribute : RubyAttribute {
        private readonly string/*!*/ _name;
        private readonly Type/*!*/ _moduleDefinitionType;

        public string/*!*/ Name {
            get { return _name; }
        }

        public Type/*!*/ ModuleDefinitionType {
            get { return _moduleDefinitionType; }
        }

        public DeclaresRubyModuleAttribute(string/*!*/ name, Type/*!*/ moduleDefinitionType) {
            _name = name;
            _moduleDefinitionType = moduleDefinitionType;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class RubyModuleAttribute : RubyAttribute {
        private readonly string _name;
        private ModuleRestrictions? _restrictions; 

        /// <summary>
        /// If an extension type doesn't specify Ruby name its name is inferred from the CLR 
        /// non-generic qualified name of the type being extended. No constant is added to the Object class.
        /// 
        /// Otherwise, the qualified name is the qualified name of the outer Ruby type within which the type is nested
        /// or the simple name of the type only if it is not a nested type (non-Ruby types and namespaces are ignored).
        /// The constant defined for such module/class is set on the declaring Ruby type or on Object if it is not nested in a Ruby type.
        /// 
        /// Examples:
        /// 1)
        /// namespace Ruby.Extensions {
        ///     [RubyClass(Extends = System.Collections.Generic.IDictionary{K,V}]
        ///     public static class IDictionaryOps {}
        /// }
        /// 
        /// Ruby name is "System::Collections::Generic::IDictionary"
        /// 
        /// 2)
        /// namespace Ruby.Builtins {
        ///     [RubyClass("Exception", Extends = typeof(Exception))]
        ///     public static class ExceptionOps {}
        /// }
        /// 
        /// Ruby name is "Exception". Constant "Exception" is defined on Object.
        /// 
        /// 3) 
        /// namespace Ruby.MyLibrary {
        ///     public class MyType {
        ///         [RubyModule]
        ///         public class MyClass {
        ///             [RubyClass("Foo")]
        ///             public class Bar {
        ///                [RubyClass]
        ///                public class Baz {
        ///                }
        ///                
        ///                [RubyClass("Goo", Extends = typeof(int))]
        ///                public class Gaz {
        ///                }
        ///                
        ///                [RubyClass(Extends = typeof(int))]
        ///                public class IntOps {
        ///                  
        ///                    [RubyClass]
        ///                    public class C {
        ///                    }
        ///                }
        ///             }
        ///         }
        ///     }
        /// }
        /// 
        /// names are (full CLR name -> Ruby name):
        /// Ruby.MyLibrary.MyType.MyClass            -> "MyClass" 
        /// Ruby.MyLibrary.MyType.MyClass.Bar        -> "MyClass::Foo" 
        /// Ruby.MyLibrary.MyType.MyClass.Bar.Baz    -> "MyClass::Foo::Baz" 
        /// Ruby.MyLibrary.MyType.MyClass.Bar.Gaz    -> "MyClass::Foo::Goo" 
        /// Ruby.MyLibrary.MyType.MyClass.Bar.IntOps -> "System::Int32"
        /// Ruby.MyLibrary.MyType.MyClass.Bar.IntOps -> "System::Int32::C"
        /// </summary>
        public string Name {
            get { return _name; }
        }
        
        /// <summary>
        /// The CLR type extended by this RubyModule.
        /// </summary>
        /// <remarks>
        /// A Ruby class/module defined by this attribute can either <c>extend</c> an existing CLR type, specified by this property, 
        /// or be <c>self-contained</c>, in which case the value of <see cref="Extends"/> property is <c>null</c>. 
        /// A self-contained class/module has <see cref="ModuleRestriction.NoUnderlyingType"/> set by default.
        /// </remarks>
        public Type Extends { get; set; }

        public Type DefineIn { get; set; }

        public ModuleRestrictions Restrictions {
            get { return _restrictions ?? ModuleRestrictions.None; }
            set { _restrictions = value; }
        }

        public ModuleRestrictions GetRestrictions(bool builtin) {
            return _restrictions ?? (
                (builtin ? ModuleRestrictions.Builtin : ModuleRestrictions.None) | 
                (Extends == null ? ModuleRestrictions.NoUnderlyingType : ModuleRestrictions.None)
            );
        }

        public RubyModuleAttribute() {
        }

        public RubyModuleAttribute(string/*!*/ name) {
            ContractUtils.RequiresNotEmpty(name, "name");
            _name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RubySingletonAttribute : RubyModuleAttribute {
        public RubySingletonAttribute() : base() {
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RubyClassAttribute : RubyModuleAttribute {
        private Type/*!*/ _inherits;

        public Type/*!*/ Inherits {
            get { return _inherits; }
            set { ContractUtils.RequiresNotNull(value, "value"); _inherits = value; }
        }

        public RubyClassAttribute() {
        }

        public RubyClassAttribute(string/*!*/ name) 
            : base(name) {
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RubyExceptionAttribute : RubyClassAttribute {
        public RubyExceptionAttribute(string/*!*/ name) 
            : base(name) {
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IncludesAttribute : Attribute {
        private readonly Type/*!*/[]/*!*/ _types;
        private bool _copy; 

        public Type/*!*/[]/*!*/ Types {
            get { return _types; }
        }

        public bool Copy {
            get { return _copy; }
            set { _copy = value; }
        }

        public IncludesAttribute() {
            _types = Type.EmptyTypes;
        }

        public IncludesAttribute(params Type[]/*!*/ types) {
            ContractUtils.RequiresNotNullItems(types, "types");
            _types = types;
        }
    }

    /// <summary>
    /// Hides CLR method when called using give name. 
    /// Doesn't apply on calls using a different (mangled/unmangled) name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class HideMethodAttribute : Attribute {
        private readonly string/*!*/ _name;
        private bool _isStatic;

        public string/*!*/ Name { get { return _name; } }
        public bool IsStatic { get { return _isStatic; } set { _isStatic = value; } }
        
        public HideMethodAttribute(string/*!*/ name) {
            ContractUtils.RequiresNotNull(name, "name");
            _name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class UndefineMethodAttribute : Attribute {
        private readonly string/*!*/ _name;
        private bool _isStatic;

        public string/*!*/ Name { get { return _name; } }
        public bool IsStatic { get { return _isStatic; } set { _isStatic = value; } }

        public UndefineMethodAttribute(string/*!*/ name) {
            ContractUtils.RequiresNotNull(name, "name");
            _name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class AliasMethodAttribute : Attribute {
        private readonly string/*!*/ _oldName, _newName;
        private bool _isStatic;

        public string/*!*/ NewName { get { return _newName; } }
        public string/*!*/ OldName { get { return _oldName; } }
        public bool IsStatic { get { return _isStatic; } set { _isStatic = value; } }

        public AliasMethodAttribute(string/*!*/ newName, string/*!*/ oldName) {
            ContractUtils.RequiresNotNull(newName, "newName");
            ContractUtils.RequiresNotNull(oldName, "oldName");
            _newName = newName;
            _oldName = oldName;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RubyStackTraceHiddenAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class DefaultProtocolAttribute : Attribute {
    }

}

