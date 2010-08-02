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
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Microsoft.Scripting.Utils {
    [Serializable]
    internal struct AssemblyQualifiedTypeName : IEquatable<AssemblyQualifiedTypeName> {
        public readonly string TypeName;
        public readonly AssemblyName AssemblyName;

        public AssemblyQualifiedTypeName(string typeName, AssemblyName assemblyName) {
            ContractUtils.RequiresNotNull(typeName, "typeName");
            ContractUtils.RequiresNotNull(assemblyName, "assemblyName");

            TypeName = typeName;
            AssemblyName = assemblyName;
        }

        public AssemblyQualifiedTypeName(Type type) {
            TypeName = type.FullName;
            AssemblyName = type.Assembly.GetName();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public AssemblyQualifiedTypeName(string assemblyQualifiedTypeName) {
            ContractUtils.RequiresNotNull(assemblyQualifiedTypeName, "assemblyQualifiedTypeName");

            int firstColon = assemblyQualifiedTypeName.IndexOf(",");
            if (firstColon != -1) {
                TypeName = assemblyQualifiedTypeName.Substring(0, firstColon).Trim();
                var assemblyNameStr = assemblyQualifiedTypeName.Substring(firstColon + 1).Trim();
                if (TypeName.Length > 0 && assemblyNameStr.Length > 0) {
                    try {
                        AssemblyName = new AssemblyName(assemblyNameStr);
                        return;
                    } catch (Exception e) {
                        throw new ArgumentException(String.Format("Invalid assembly qualified name '{0}': {1}", assemblyQualifiedTypeName, e.Message), e);
                    }
                }
            }

            throw new ArgumentException(String.Format("Invalid assembly qualified name '{0}'", assemblyQualifiedTypeName));
        }

        internal static AssemblyQualifiedTypeName ParseArgument(string str, string argumentName) {
            Assert.NotEmpty(argumentName);           
            try {
                return new AssemblyQualifiedTypeName(str);
            } catch (ArgumentException e) {
#if SILVERLIGHT
                throw new ArgumentException(e.Message, argumentName);
#else
                throw new ArgumentException(e.Message, argumentName, e.InnerException);
#endif
            }
        }

        public bool Equals(AssemblyQualifiedTypeName other) {
            return TypeName == other.TypeName && AssemblyName.FullName == other.AssemblyName.FullName;
        }

        public override bool Equals(object obj) {
            return obj is AssemblyQualifiedTypeName && Equals((AssemblyQualifiedTypeName)obj);
        }

        public override int GetHashCode() {
            return TypeName.GetHashCode() ^ AssemblyName.FullName.GetHashCode();
        }

        public override string ToString() {
            return TypeName + ", " + AssemblyName.FullName;
        }

        public static bool operator ==(AssemblyQualifiedTypeName name, AssemblyQualifiedTypeName other) {
            return name.Equals(other);
        }

        public static bool operator !=(AssemblyQualifiedTypeName name, AssemblyQualifiedTypeName other) {
            return !name.Equals(other);
        }
    }
}
