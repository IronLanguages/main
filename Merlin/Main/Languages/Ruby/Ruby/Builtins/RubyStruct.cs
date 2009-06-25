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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Builtins {

    // TODO: freezing
    [ReflectionCached]
    public sealed class RubyStruct : RubyObject {
        /// <summary>
        /// This class represents the type information of a Struct. There is one
        /// instance of this per Struct RubyClass; all instances of that class shares
        /// one instance of StructInfo.
        /// </summary>
        internal sealed class Info {
            private readonly Dictionary<string, int>/*!*/ _nameIndices; // immutable
            private readonly string/*!*/[]/*!*/ _names;                 // immutable

            internal Info(string/*!*/[]/*!*/ names) {
                _names = ArrayUtils.Copy(names);
                _nameIndices = new Dictionary<string, int>(names.Length);
                for (int i = 0; i < names.Length; i++) {
                    // overwrites duplicates:
                    _nameIndices[names[i]] = i;
                }
            }
            
            internal int Length { 
                get { return _names.Length; } 
            }
            
            internal bool TryGetIndex(string/*!*/ name, out int index) {
                return _nameIndices.TryGetValue(name, out index);
            }

            internal string/*!*/ GetName(int index) {
                return _names[index];
            }

            internal RubyArray/*!*/ GetMembers() {
                RubyArray list = new RubyArray(_names.Length);
                foreach (string id in _names) {
                    list.Add(MutableString.Create(id));
                }
                return list;
            }

            internal ReadOnlyCollection<string>/*!*/ GetNames() {
                return new ReadOnlyCollection<string>(_names);
            }
        }

        private readonly object[]/*!*/ _data;

        #region Construction

#if !SILVERLIGHT
        public RubyStruct(SerializationInfo/*!*/ info, StreamingContext context) 
            : base(info, context) {
            // TODO: deserialize _data
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo/*!*/ info, StreamingContext context) {
            base.GetObjectData(info, context);
            // TODO: serialize _data
        }
#endif

        // This class should not have a default constructor (or one that takes RubyClass or RubyContext)!
        // Struct class provides its own allocator (Struct#new), 
        // The default allocator should never be called unless Struct#new is removed and a derived class is constructed.
        private RubyStruct(RubyClass/*!*/ rubyClass, bool dummy)
            : base(rubyClass) {
            Debug.Assert(rubyClass.StructInfo != null);
            _data = new object[rubyClass.StructInfo.Length];
        }

        // copy ctor:
        private RubyStruct(RubyClass/*!*/ rubyClass, object[]/*!*/ data) 
            : base(rubyClass) {
            Debug.Assert(rubyClass.StructInfo != null);
            Debug.Assert(!rubyClass.IsSingletonClass);
            _data = ArrayUtils.Copy(data);
        }

        protected override RubyObject/*!*/ CreateInstance() {
            return new RubyStruct(ImmediateClass.NominalClass, _data);
        }

        public static RubyStruct/*!*/ Create(RubyClass/*!*/ rubyClass) {
            Debug.Assert(!rubyClass.IsSingletonClass);
            return new RubyStruct(rubyClass, true);
        }

        // triggers "inherited" event, adds constant to the owner
        public static RubyClass/*!*/ DefineStruct(RubyClass/*!*/ owner, string className, string/*!*/[]/*!*/ attributeNames) {
            Assert.NotNullItems(attributeNames);
            
            // MRI: "inherited" event is triggered by DefineClass before the members are defined and the body is evaluated.
            // Any exception thrown by the event will cancel struct initialization.
            RubyClass result = owner.Context.DefineClass(owner, className, owner, new Info(attributeNames));

            AddClassMembers(result, attributeNames);

            return result;
        }

        // add methods to the generated class
        private static void AddClassMembers(RubyClass/*!*/ cls, string[]/*!*/ structMembers) {
            var newInstance = new RuleGenerator(RuleGenerators.InstanceConstructor);

            cls.SingletonClass.DefineRuleGenerator("[]", (int)RubyMethodAttributes.PublicSingleton, newInstance);
            cls.SingletonClass.DefineRuleGenerator("new", (int)RubyMethodAttributes.PublicSingleton, newInstance);

            cls.SingletonClass.DefineLibraryMethod("members", (int)RubyMethodAttributes.PublicSingleton,
                new Func<RubyClass, RubyArray>(GetMembers)
            );

            for (int i = 0; i < structMembers.Length; i++) {
                string getter = structMembers[i];
                cls.DefineRuleGenerator(getter, (int)RubyMethodAttributes.PublicInstance, CreateGetter(i));
                cls.DefineRuleGenerator(getter + '=', (int)RubyMethodAttributes.PublicInstance, CreateSetter(i));
            }
        }

        // Derived struct: [RubyMethod("members", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetMembers(RubyClass/*!*/ self) {
            Debug.Assert(self.StructInfo != null);
            return self.StructInfo.GetMembers();
        }

        public static RubyArray/*!*/ GetMembers(RubyStruct/*!*/ self) {
            Debug.Assert(self.StructInfo != null);
            return self.StructInfo.GetMembers();
        }

        private static RuleGenerator/*!*/ CreateGetter(int index) {
            return delegate(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {

                var actualArgs = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, 0);
                if (!metaBuilder.Error) {
                    metaBuilder.Result = Ast.Call(
                        Ast.Convert(args.TargetExpression, typeof(RubyStruct)),
                        Methods.RubyStruct_GetValue,
                        AstUtils.Constant(index)
                    );
                }
            };
        }

        private static RuleGenerator/*!*/ CreateSetter(int index) {
            return delegate(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {

                var actualArgs = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 1, 1);
                if (!metaBuilder.Error) {
                    metaBuilder.Result = Ast.Call(
                        Ast.Convert(args.TargetExpression, typeof(RubyStruct)),
                        Methods.RubyStruct_SetValue,
                        AstUtils.Constant(index),
                        AstFactory.Box(actualArgs[0].Expression)
                    );
                }
            };
        }

        #endregion

        // TODO: copy struct info reference to singletons?
        private Info/*!*/ StructInfo {
            get { return ImmediateClass.GetNonSingletonClass().StructInfo; }
        }
        
        public int GetIndex(string/*!*/ name) {
            int result;
            if (StructInfo.TryGetIndex(name, out result)) {
                return result;
            }
            throw RubyExceptions.CreateNameError(String.Format("no member `{0}' in struct", name));
        }

        public int GetHashCode(UnaryOpStorage/*!*/ hashStorage, ConversionStorage<int>/*!*/ fixnumCast) {
            // hash is: struct's hash, plus data hashes
            return StructInfo.GetHashCode() ^ RubyArray.GetHashCode(hashStorage, fixnumCast, _data);
        }

        public bool Equals(BinaryOpStorage/*!*/ eqlStorage, object obj) {
            var other = obj as RubyStruct;
            if (!StructReferenceEquals(other)) {
                return false;
            }
            
            return RubyArray.Equals(eqlStorage, _data, other._data);
        }

        public bool StructReferenceEquals(RubyStruct other) {
            // TODO: compare non-singleton classes?
            return ReferenceEquals(this, other) || (other != null && ImmediateClass.GetNonSingletonClass() == other.ImmediateClass.GetNonSingletonClass());
        }

        #region Emitted Helpers

        [Emitted] 
        public object GetValue(int index) {
            return _data[index];
        }

        [Emitted] 
        public object SetValue(int index, object value) {
            return _data[index] = value;
        }

        #endregion


        public IEnumerable<KeyValuePair<string, object>>/*!*/ GetItems() {
            for (int i = 0; i < _data.Length; i++) {
                yield return new KeyValuePair<string, object>(StructInfo.GetName(i), _data[i]);
            }
        }

        public object[]/*!*/ Values {
            get { return _data; }
        }

        public ReadOnlyCollection<string>/*!*/ GetNames() {
            return StructInfo.GetNames();
        }

        public void SetValues(object[]/*!*/ items) {
            ContractUtils.RequiresNotNull(items, "items");

            if (items.Length > _data.Length) {
                throw RubyExceptions.CreateArgumentError("struct size differs");
            }

            Array.Copy(items, _data, items.Length);
        }

        public object this[string name] {
            get { return _data[GetIndex(name)]; }
            set { _data[GetIndex(name)] = value; }
        }

        public object this[int index] {
            get { return _data[index]; }
            set { _data[index] = value; }
        }

        public int ItemCount {
            get { return _data.Length; }
        }
    }
}
