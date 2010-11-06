/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
 

public class PublicRefTypeWithoutNS {
    public class Nested { public static int A = 10;    }

    public static int A = 20;
    public int B;
    public static int SM() { return 30; }
    public int IM() { return 40; }
}

public static class PublicStaticRefTypeWithoutNS {
    public class Nested { public static int A = 10;    }

    public static readonly int A = 20;
    public const int B = 20;
    public static int C = 20;
    public static int SM() { return 30; }
}

public struct PublicValueTypeWithoutNS<T> {
    public static int A = 60;
}

class InternalRefTypeWithoutNS {
}

namespace NSwVarious.NestedNS {
    public class A { }
    public struct B { }
    public delegate void C();
    public interface D { }
    public enum E { }

    class F { }
}

namespace NSwGeneric {
    public class G1 { public static int A = 10;    }
    public class G1<K, V> { public static int A = 20;    }

    public class G2<T> { public static int A = 30;    }
    public class G2<K, V> { public static int A = 40;    }

    public class G3<T> where T : struct { public static int A = 50;    }

    public class G4 { public static int A = 60;}
}

//
// under one namespace, there is no "public" type
//
namespace NothingPublic {
    class PrivateClass { }
}

// IRONPYTHON SPECIFIC

// keyword
namespace pass { public class A { } }
namespace import { public class A { } }
namespace def { public class A { } }
namespace exec { public class A { } }
namespace except { public class A { } }

// builtin-func 
namespace abs { public class A { } }
namespace type { public class A { } }
namespace file { public class A { } }

// builtin-type
namespace complex { public class A { } }
namespace StandardError { public class A { } }

// constants
namespace None { public class A { } }
namespace False { public class A { } }

// builtin module
namespace __builtin__ { public class A { } }
namespace datetime { public class A { } }
namespace _collections { public class A { } }

// site
namespace site { public class A { } }

namespace NSwInterestingClassName {
    public class pass { public static int A = 10;}
    public class import { public static int A = 10; }
    public class def { public static int A = 10;}
    public class exec { public static int A = 10; }
    public class except { public static int A = 10;}

    public class abs { public static int A = 10; }
    public class type { public static int A = 10; }
    public class file { public static int A = 10;}

    public class complex { public static int A = 10; }
    public class StandardError { public static int A = 10;}

    public class None { public static int A = 10; }
    public class False { public static int A = 10; }

    public class __builtin__ { public static int A = 10;}
    public class datetime { public static int A = 10;}
    public class _collections { public static int A = 10;}

    public class site { public static int A = 10; }
}

namespace NSWithDigitsCase1 {
    public class Z0 { public static int  A = 0;}
    public class Z  { public static int  A = 10;}
}

namespace NSWithDigits.Case2 {
    public class Z0 { public static int A = 0;}
    public class Z { public static int A = 10;}
}

