/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
 
public class TopLevelClass_ToBeRemoved {
    public static string Flag = typeof(TopLevelClass_ToBeRemoved).AssemblyQualifiedName;
}

public class TopLevelClass_ToBeRemained {
    public static string Flag = typeof(TopLevelClass_ToBeRemained).AssemblyQualifiedName;
}

namespace NormalNamespace {
    public class Class_ToBeRemoved {
        public static string Flag = typeof(Class_ToBeRemoved).AssemblyQualifiedName;
    }

    public class Class_ToBeRemained {
        public static string Flag = typeof(Class_ToBeRemained).AssemblyQualifiedName;
    }

    public class NormalClass {
        public class NestedClass_ToBeRemoved {
            public static string Flag = typeof(NestedClass_ToBeRemoved).AssemblyQualifiedName;
        }

        public class NestedClass_ToBeRemained {
            public static string Flag = typeof(NestedClass_ToBeRemained).AssemblyQualifiedName;
        }
    }
}

namespace Namespace_ToBeRemoved {
    public class C {
        public static string Flag = typeof(C).AssemblyQualifiedName;
    }
}

namespace Namespace_ToBeRemained {
    public class C {
        public static string Flag = typeof(C).AssemblyQualifiedName;
    }
}

