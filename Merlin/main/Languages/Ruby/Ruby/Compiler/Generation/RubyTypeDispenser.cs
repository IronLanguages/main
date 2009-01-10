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
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.Compiler.Generation {
    internal static class RubyTypeDispenser {
        private static readonly Publisher<TypeDescription/*!*/, Type/*!*/>/*!*/ _newTypes;
        private static readonly Dictionary<Type/*!*/, IList<ITypeFeature/*!*/>/*!*/>/*!*/ _typeFeatures;
        private static readonly ITypeFeature/*!*/[]/*!*/ _defaultFeatures = new ITypeFeature[2] {
            RubyTypeBuilder.Feature,
            InterfacesBuilder.MakeFeature(Type.EmptyTypes)
        };

        static RubyTypeDispenser() {
            _newTypes = new Publisher<TypeDescription, Type>();
            _typeFeatures = new Dictionary<Type, IList<ITypeFeature>>();

            AddSystemType(typeof(object), typeof(RubyObject));
            AddSystemType(typeof(RubyModule), typeof(RubyModule.Subclass));
            AddSystemType(typeof(MutableString), typeof(MutableString.Subclass));
            AddSystemType(typeof(Proc), typeof(Proc.Subclass));
            AddSystemType(typeof(RubyRegex), typeof(RubyRegex.Subclass));
            AddSystemType(typeof(Hash), typeof(Hash.Subclass));
            AddSystemType(typeof(RubyArray), typeof(RubyArray.Subclass));
            AddSystemType(typeof(MatchData), typeof(MatchData.Subclass));
        }

        internal static Type/*!*/ GetOrCreateType(Type/*!*/ baseType, IList<Type/*!*/>/*!*/ interfaces) {
            Assert.NotNull(baseType);
            Assert.NotNull(interfaces);

            ITypeFeature[] features;
            if (interfaces.Count == 0) {
                features = _defaultFeatures;
            } else {
                features = new ITypeFeature[2] {
                    RubyTypeBuilder.Feature,
                    InterfacesBuilder.MakeFeature(interfaces)
                };
            }

            TypeDescription typeInfo = new TypeDescription(baseType, features);
            Type type = _newTypes.GetOrCreateValue(typeInfo,
                delegate() {
                    if (TypeImplementsFeatures(baseType, features)) {
                        return baseType;
                    }
                    return CreateType(typeInfo);
                });

            Debug.Assert(typeof(IRubyObject).IsAssignableFrom(type));
            return type;
        }

        internal static bool TryGetFeatures(Type/*!*/ type, out IList<ITypeFeature/*!*/> features) {
            lock (_typeFeatures) {
                return _typeFeatures.TryGetValue(type, out features);
            }
        }

        private static bool TypeImplementsFeatures(Type/*!*/ type, IList<ITypeFeature/*!*/>/*!*/ features) {
            IList<ITypeFeature> featuresFound;
            if (TryGetFeatures(type, out featuresFound)) {
                return TypeDescription.FeatureSetsMatch(features, featuresFound);
            }

            foreach (ITypeFeature feature in features) {
                if (!feature.IsImplementedBy(type)) {
                    return false;
                }
            }
            return true;
        }

        private static Type CreateType(TypeDescription/*!*/ typeInfo) {
            Type baseType = typeInfo.BaseType;
            if (baseType.IsSealed) {
                throw new NotSupportedException("Can't inherit from a sealed type.");
            }

            string typeName = GetName(baseType);
            TypeBuilder tb = Snippets.Shared.DefinePublicType(typeName, baseType);
            Utils.Log(typeName, "TYPE_BUILDER");

            IFeatureBuilder[] _features = new IFeatureBuilder[typeInfo.Features.Count];
            ClsTypeEmitter emitter = new RubyTypeEmitter(tb);
            for (int i = 0; i < typeInfo.Features.Count; i++) {
                _features[i] = typeInfo.Features[i].MakeBuilder(tb);
            }

            foreach (IFeatureBuilder feature in _features) {
                feature.Implement(emitter);
            }
            emitter.OverrideMethods(baseType);
            Type result = emitter.FinishType();
            lock (_typeFeatures) {
                _typeFeatures.Add(result, typeInfo.Features);
            }
            return result;
        }

        private static string GetName(Type/*!*/ baseType) {
            // DLR appends a counter, so we don't need to
            // TODO: Reflect feature set in the name?
            StringBuilder name = new StringBuilder("IronRuby.Classes.");
            name.Append(baseType.Name);
            return name.ToString();
        }

        private static void AddSystemType(Type/*!*/ clsBaseType, Type/*!*/ rubyType) {
            AddSystemType(clsBaseType, rubyType, _defaultFeatures);
        }
        private static void AddSystemType(Type/*!*/ clsBaseType, Type/*!*/ rubyType, ITypeFeature/*!*/[]/*!*/ features) {
            _newTypes.GetOrCreateValue(new TypeDescription(clsBaseType, features), delegate() { return rubyType; });
            _typeFeatures[rubyType] = features;
        }
    }
}
