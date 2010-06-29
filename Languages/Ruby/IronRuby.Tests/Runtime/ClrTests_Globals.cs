using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Utils;

namespace InteropTests.Generics1 {
    public class C {
        public virtual int Arity { get { return 0; } }
    }

    public class C<T> {
        public virtual int Arity { get { return 1; } }
    }

    public class C<T, S> {
        public virtual int Arity { get { return 2; } }
    }

    public class D : C {
        public override int Arity { get { return 10; } }
    }

    public class D<T> : C<T> {
        public override int Arity { get { return 11; } }
    }

    public interface J<T> {
        void MethodOnJ();
    }

    public interface I<T> : J<T> {
        void MethodOnI();
    }

    public class E<T> : I<T>, J<T> {
        void I<T>.MethodOnI() { }
        void J<T>.MethodOnJ() { }
    }

    public static class Extensions {
        public static IEnumerable<R> Select<T, R>(this IEnumerable<T> a, Func<T, R> func) {
            foreach (var item in a) {
                yield return func(item);
            }
        }
    }
}

namespace InteropTests.Namespaces2 {
    public class C { }
    namespace N {
        public class D { }
    }
}

namespace IronRubyTests.ExtensionMethods1 {
    using IronRuby.Tests;

    public static class OverloadInheritanceExtensions2 {
        public static string f(this Tests.OverloadInheritance2.C self, int a, int b, int c, int d, int e) { return "e5"; }
    }

    public static class OverloadInheritanceExtensions3 {
        public static string f(this Tests.OverloadInheritance2.A self, int a, int b, int c, int d, int e, int f) { return "e6"; }
        public static string f(this Tests.OverloadInheritance2.F self, int a, int b, int c, int d) { return "e4-dup"; }
    }
}

namespace System.Linq {
    public class Dummy {
    }
}

// top level (no namespace)
public static class OverloadInheritanceExtensions4 {
    public static string f(this IronRuby.Tests.Tests.OverloadInheritance2.C self, int a, int b, int c, int d, int e, int f, int g) { return "e7"; }
}

namespace IronRubyTests.ExtensionMethods2 {
    public class A {
    }

    public class B : A {
    }

    public interface I {
    }

    public class X : I {
    }

    public class Y : X {
    }

    public struct S {
    }

    public static class EMs {
        public static string f1<T>(this List<T> self) where T : I { 
            return "f1"; 
        }

        public static string f2<S,T>(this Dictionary<S, T> self) where S : T {
            return "f2";
        }

        public static string f3<S, T>(this List<S> self, T t) 
            where S : T, I, new() 
        {
            return "f3";
        }

        public static string f4<T>(this T self) where T : struct {
            return "f4";
        }

        public static string f5<T>(this T self) where T : A {
            return "f5";
        }

        public static string f6<T>(this T[] self) where T : A {
            return "f6";
        }

        public static string f6<S, T>(this List<Dictionary<S, List<T>>[]> self) 
            where T : class
            where S : A 
        {
            return "f6";
        }
    }
}