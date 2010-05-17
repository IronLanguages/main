csc <<-EOL
  public class StaticMethodTypeGroup {
    public static int Return(int retval) { return retval; }
  }
  public class StaticMethodTypeGroup<T> {
    public static T Return(T retval) { return retval;}
  }
  public class EmptyTypeGroup { }
  public class EmptyTypeGroup<T> { }

  public class EmptyTypeGroup1<T> {}
  public class EmptyTypeGroup1<T,V> {}

  public class TypeGroup {int m1() {return 1;}}
  public class TypeGroup<T> {int m1() {return 1;}}

  public class TypeGroup1<T> {int m1() {return 1;}}
  public class TypeGroup1<T,V> {int m1() {return 1;}}
EOL
