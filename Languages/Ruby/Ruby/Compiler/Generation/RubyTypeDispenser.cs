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
using System.Diagnostics;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;
using System.Globalization;

namespace IronRuby.Compiler.Generation {
    internal static class RubyTypeDispenser {

        private static readonly Publisher<TypeDescription/*!*/, Type/*!*/>/*!*/ _newTypes;
        private static readonly Dictionary<Type/*!*/, IList<ITypeFeature/*!*/>/*!*/>/*!*/ _typeFeatures;
        private static readonly ITypeFeature/*!*/[]/*!*/ _defaultFeatures = new ITypeFeature[] {
            RubyTypeFeature.Instance,
            InterfaceImplFeature.Create(Type.EmptyTypes)
        };

        static RubyTypeDispenser() {
            _newTypes = new Publisher<TypeDescription, Type>();
            _typeFeatures = new Dictionary<Type, IList<ITypeFeature>>();
            AddBuiltinType(typeof(object), typeof(RubyObject), false);
            AddBuiltinType(typeof(MutableString), typeof(MutableString.Subclass), true);
            AddBuiltinType(typeof(Proc), typeof(Proc.Subclass), true);
            AddBuiltinType(typeof(RubyRegex), typeof(RubyRegex.Subclass), true);
            AddBuiltinType(typeof(Range), typeof(Range.Subclass), true);
            AddBuiltinType(typeof(Hash), typeof(Hash.Subclass), true);
            AddBuiltinType(typeof(RubyArray), typeof(RubyArray.Subclass), true);
            AddBuiltinType(typeof(MatchData), typeof(MatchData.Subclass), true);
            AddBuiltinType(typeof(RubyIO), typeof(RubyIO.Subclass), true);
        }
      
        internal static Type/*!*/ GetOrCreateType(Type/*!*/ baseType, IList<Type/*!*/>/*!*/ interfaces, bool noOverrides) {
            Assert.NotNull(baseType);
            Assert.NotNull(interfaces);

            ITypeFeature[] features;
            if (interfaces.Count == 0) {
                features = _defaultFeatures;
            } else {
                features = new ITypeFeature[] {
                    RubyTypeFeature.Instance,
                    InterfaceImplFeature.Create(interfaces)
                };
            }
            noOverrides |= typeof(IRubyType).IsAssignableFrom(baseType);

            TypeDescription typeInfo = new TypeDescription(baseType, features, noOverrides);
            Type type = _newTypes.GetOrCreateValue(typeInfo,
                () => {
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
                throw new NotSupportedException(
                    String.Format(CultureInfo.InvariantCulture, "Can't inherit from a sealed type {0}.",
                    RubyContext.GetQualifiedNameNoLock(baseType, null, false))
                );
            }

            string typeName = GetName(baseType);
            TypeBuilder tb = Snippets.Shared.DefinePublicType(typeName, baseType);
            Utils.Log(typeName, "TYPE_BUILDER");

            IFeatureBuilder[] features = new IFeatureBuilder[typeInfo.Features.Count];
            RubyTypeEmitter emitter = new RubyTypeEmitter(tb);

            for (int i = 0; i < typeInfo.Features.Count; i++) {
                features[i] = typeInfo.Features[i].MakeBuilder(tb);
            }

            foreach (IFeatureBuilder feature in features) {
                feature.Implement(emitter);
            }

            if (!typeInfo.NoOverrides) {
                emitter.OverrideMethods(baseType);
            }

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

        private static void AddBuiltinType(Type/*!*/ clsBaseType, Type/*!*/ rubyType, bool noOverrides) {
            AddBuiltinType(clsBaseType, rubyType, _defaultFeatures, noOverrides);
        }

        private static void AddBuiltinType(Type/*!*/ clsBaseType, Type/*!*/ rubyType, ITypeFeature/*!*/[]/*!*/ features, bool noOverrides) {
            _newTypes.GetOrCreateValue(new TypeDescription(clsBaseType, features, noOverrides), () => rubyType);
            _typeFeatures[rubyType] = features;
        }
    }
}
