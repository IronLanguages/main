#line 2 "./fixtures/classes.rb"
public static class BindingTester {
  public static void MultipleOutArgs(this object arg1, object arg2, out object arg3, out string arg4) {
    arg3 = arg1;
    arg4 = "MultipleOutArgs";
  }
}