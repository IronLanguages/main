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

#if !SILVERLIGHT // ComObject

using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace System.Dynamic {
    internal sealed class BoundDispEvent {
        private object _rcw;
        private Guid _sourceIid;
        private int _dispid;

        internal BoundDispEvent(object rcw, Guid sourceIid, int dispid) {
            _rcw = rcw;
            _sourceIid = sourceIid;
            _dispid = dispid;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        [SpecialName]
        public object op_AdditionAssignment(object func) {
            return InPlaceAdd(func);
        }

        [SpecialName]
        public object InPlaceAdd(object func) {
            ComEventSink comEventSink = ComEventSink.FromRuntimeCallableWrapper(_rcw, _sourceIid, true);
            comEventSink.AddHandler(_dispid, func);
            return this;
        }

        [SpecialName]
        public object InPlaceSubtract(object func) {
            ComEventSink comEventSink = ComEventSink.FromRuntimeCallableWrapper(_rcw, _sourceIid, false);
            if (comEventSink == null) {
                throw Error.RemovingUnregisteredEvent();
            }

            comEventSink.RemoveHandler(_dispid, func);
            return this;
        }
    }
}

#endif
