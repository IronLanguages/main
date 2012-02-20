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
using System.Reflection.Emit;

namespace IronRuby.Compiler.Generation {
    internal interface ITypeFeature {
        bool CanInherit { get; }
        bool IsImplementedBy(Type type);
#if FEATURE_REFEMIT
        IFeatureBuilder MakeBuilder(TypeBuilder tb);
#endif
    }

#if FEATURE_REFEMIT
    internal interface IFeatureBuilder {
        void Implement(ClsTypeEmitter emitter);
    }
#endif
}
