/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting {
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class AmbiguousFileNameException : Exception {
        private readonly string _firstPath;
        private readonly string _secondPath;

        public string FirstPath {
            get { return _firstPath; }
        }

        public string SecondPath {
            get { return _secondPath; }
        }

        public AmbiguousFileNameException(string firstPath, string secondPath)
            : this(firstPath, secondPath, null, null) {
        }

        public AmbiguousFileNameException(string firstPath, string secondPath, string message)
            : this(firstPath, secondPath, message, null) {
        }

        public AmbiguousFileNameException(string firstPath, string secondPath, string message, Exception inner)
            : base(message ?? string.Format("File name is ambiguous; more files are matching the same name (including '{0}' and '{1}')", firstPath, secondPath), inner) {
            ContractUtils.RequiresNotNull(firstPath, "firstPath");
            ContractUtils.RequiresNotNull(secondPath, "secondPath");

            _firstPath = firstPath;
            _secondPath = secondPath;
        }


#if !SILVERLIGHT
        protected AmbiguousFileNameException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("firstPath", _firstPath);
            info.AddValue("secondPath", _secondPath);

            base.GetObjectData(info, context);
        }
#endif
    }
}
