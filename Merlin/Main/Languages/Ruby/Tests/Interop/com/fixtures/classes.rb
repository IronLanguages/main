reference "System.Core.dll"
csc <<-EOL
public static class BindingTester {
  public interface IFoo {
  }

  public static void WithIface(IFoo foo) {
  }
  
  public static void MultipleOutArgs(this object arg1, object arg2, out object arg3, out string arg4) {
    arg3 = arg1;
    arg4 = "MultipleOutArgs";
  }
}
EOL
