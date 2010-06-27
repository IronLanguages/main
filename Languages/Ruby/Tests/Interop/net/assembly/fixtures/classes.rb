#CSharp fixtures
assembly("B\\b.generated.cs", :out => "B\\b.generated.dll") do
  csc <<-EOL
  public class B {
    public static int Main() {
      return 1;
    }
  }
  EOL
end
assembly("A\\a.generated.cs", :out => "A\\a.generated.dll", :references => "B\\b.generated.dll") do
  csc <<-EOL
  public class A {
    public static int Main() {
      return B.Main();
    }
  }
  EOL
end

