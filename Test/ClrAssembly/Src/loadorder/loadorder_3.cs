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
 

namespace First {
    public class Generic1<K, V> {
        public static string Flag = typeof(Generic1<,>).FullName;
    }
}

//#region The above non-generic type will loaded followed by each type below

//// non-generic type, which has different namespace, same name from First.Generic1
//namespace Second {
//    public class Generic1 {
//        public static string Flag = typeof(Generic1).FullName;
//    }
//}

//// non-generic type, which has different namespace, different name from First.Generic1
//namespace Second {
//    public class Generic2 {
//        public static string Flag = typeof(Generic2).FullName;
//    }
//}

//// non-generic type, which has same namespace, same name from First.Generic1
//namespace First {
//    public class Generic1 {
//        public static string Flag = typeof(Generic1).Name;
//    }
//}

//// non-generic type, which has same namespace, different name from First.Generic1
//namespace First {
//    public class Generic2 {
//        public static string Flag = typeof(Generic2).FullName;
//    }
//}

//// generic type, which has different namespace, same name from First.Generic1
//namespace Second {
//    public class Generic1<K, V> {
//        public static string Flag = typeof(Generic1<,>).FullName;
//    }
//}

//// generic type, which has different namespace, different name from First.Generic1
//namespace Second {
//    public class Generic2<K, V> {
//        public static string Flag = typeof(Generic2<,>).FullName;
//    }
//}

//// generic type, which has same namespace, same name from First.Generic1
//namespace First {
//    public class Generic1<K, V> {
//        public static string Flag = typeof(Generic1<,>).FullName + "_Same";
//    }
//}

//// generic type, which has same namespace, same name from First.Generic1
//namespace First {
//    public class Generic1<T> {
//        public static string Flag = typeof(Generic1<>).FullName;
//    }
//}

//// generic type, which has same namespace, different name from First.Generic1
//namespace First {
//    public class Generic2<T> {
//        public static string Flag = typeof(Generic2<>).FullName;
//    }
//}

//#endregion