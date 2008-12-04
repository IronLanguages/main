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
using System.Diagnostics;
using Microsoft.Scripting;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Ast = System.Linq.Expressions.Expression;
using IronRuby.Compiler;
using System.Collections.ObjectModel;
using IronRuby.Compiler.Generation;

namespace IronRuby.Builtins {

    // TODO: freezing
    public sealed class RubyStruct : RubyObject {
        /// <summary>
        /// This class represents the type information of a Struct. There is one
        /// instance of this per Struct RubyClass; all instances of that class shares
        /// one instance of StructInfo.
        /// </summary>
        internal sealed class Info {
            internal readonly string[] Members;
            internal int Length { get { return Members.Length; } }

            // This dictionary is an immutable hash used for fast lookup on the SymbolId indexer
            internal readonly Dictionary<string, int> IndexMap;

            internal RubyArray/*!*/ GetMembers() {
                RubyArray list = new RubyArray(Members.Length);
                foreach (string id in Members) {
                    list.Add(MutableString.Create(id));
                }
                return list;
            }

            internal Info(string[]/*!*/ members) {
                Members = members;
                IndexMap = new Dictionary<string, int>(members.Length);
                for (int i = 0; i < members.Length; i++) {
                    // overwrites duplicates:
                    IndexMap[members[i]] = i;
                }
            }
        }

        private readonly object[]/*!*/ _data;

        #region Construction

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
            _data = ArrayUtils.Copy(data);
        }

        protected override RubyObject/*!*/ CreateInstance() {
            return new RubyStruct(Class, _data);
        }

        public static RubyStruct/*!*/ Create(RubyClass/*!*/ rubyClass) {
            return new RubyStruct(rubyClass, true);
        }

        // triggers "inherited" event, adds constant to the owner
        public static RubyClass/*!*/ DefineStruct(RubyClass/*!*/ owner, string className, string[]/*!*/ attributeNames) {
            // MRI: "inherited" event is triggered by DefineClass before the members are defined and the body is evaluated.
            // Any exception thrown by the event will cancel struct initialization.
            RubyClass result = owner.Context.DefineClass(owner, className, owner);
            result.StructInfo = new Info(attributeNames);

            AddClassMembers(result, attributeNames);

            return result;
        }

        // add methods to the generated class
        private static void AddClassMembers(RubyClass/*!*/ cls, string[]/*!*/ structMembers) {
            var newInstance = new RuleGenerator(RuleGenerators.InstanceConstructor);

            cls.SingletonClass.DefineRuleGenerator("[]", (int)RubyMethodAttributes.PublicSingleton, newInstance);
            cls.SingletonClass.DefineRuleGenerator("new", (int)RubyMethodAttributes.PublicSingleton, newInstance);

            cls.SingletonClass.DefineLibraryMethod("members", (int)RubyMethodAttributes.PublicSingleton,
                new Func<RubyClass, List<object>>(GetMembers)
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
            Debug.Assert(self.Class.StructInfo != null);
            return self.Class.StructInfo.GetMembers();
        }

        private static RuleGenerator/*!*/ CreateGetter(int index) {
            return delegate(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
                metaBuilder.Result = Ast.Call(
                    Ast.Convert(args.TargetExpression, typeof(RubyStruct)),
                    typeof(RubyStruct).GetMethod("GetValue"),
                    Ast.Constant(index)
                );
            };
        }

        private static RuleGenerator/*!*/ CreateSetter(int index) {
            return delegate(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {

                var actualArgs = RubyMethodGroupInfo.MakeActualArgs(metaBuilder, args, true, false, false, false);

                metaBuilder.Result = Ast.Call(
                    Ast.Convert(actualArgs[0], typeof(RubyStruct)),
                    typeof(RubyStruct).GetMethod("SetValue"),
                    Ast.Constant(index),
                    Ast.Convert(actualArgs[1], typeof(object))
                );
            };
        }

        #endregion 

        public int GetIndex(string/*!*/ name) {
            int result;
            if (Class.StructInfo.IndexMap.TryGetValue(name, out result)) {
                return result;
            }
            throw RubyExceptions.CreateNameError(String.Format("no member `{0}' in struct", name));
        }

        public override int GetHashCode() {
            // hash is: struct's hash, plus data hashes
            int hash = Class.StructInfo.GetHashCode();
            foreach (object obj in _data) {
                hash ^= RubyUtils.GetHashCode(obj);
            }
            return hash;
        }

        public override bool Equals(object obj) {
            var other = obj as RubyStruct;
            if (!StructReferenceEquals(other)) {
                return false;
            }
            
            Debug.Assert(_data.Length == other._data.Length);
            for (int i = 0; i < _data.Length; i++) {
                if (!RubyUtils.ValueEquals(_data[i], other._data[i])) {
                    return false;
                }
            }

            return true;
        }

        public bool StructReferenceEquals(RubyStruct other) {
            return ReferenceEquals(this, other) || (other != null && Class == other.Class);
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
                yield return new KeyValuePair<string, object>(Class.StructInfo.Members[i], _data[i]);
            }
        }

        public object[]/*!*/ Values {
            get { return _data; }
        }

        public ReadOnlyCollection<string>/*!*/ GetNames() {
            return new ReadOnlyCollection<string>(Class.StructInfo.Members);
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
