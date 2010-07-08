bin = "#{ENV['DLR_ROOT']}\\bin\\debug"
reference "#{bin}\\Microsoft.Scripting.dll"
reference "#{bin}\\Microsoft.Scripting.Metadata.dll"
reference "#{bin}\\Microsoft.Scripting.Core.dll"
reference "#{bin}\\Microsoft.Dynamic.dll"
reference "#{bin}\\IronRuby.dll"
reference "#{bin}\\IronRuby.Libraries.dll"
csc <<-EOL
using Microsoft.Scripting.Hosting;
using IronRuby.Runtime;
using IronRuby.Builtins;
EOL
csc <<-EOL
  public partial class DelegateConversionClass {
    public delegate int Delegate1(string str);
    private ScriptEngine _engine;
    private Proc _lambda;

    public DelegateConversionClass(string lambdaExpr)  {
      _engine = IronRuby.Ruby.CreateEngine();
      _lambda = (Proc) _engine.Execute(lambdaExpr);
    }

    public int DirectInvoke() {
      return (int) _engine.Operations.Invoke(_lambda, "1");
    }

    public int ConvertToDelegate() {
      Delegate1 d = _engine.Operations.ConvertTo<Delegate1>(_lambda);
      return d("1");
    }
  }

  public partial class DelegateHolder {
    public delegate string[] ARefVoidDelegate();
    public delegate string[] ARefRefDelegate(string foo);
    public delegate string[] ARefValDelegate(int foo);
    public delegate string[] ARefARefDelegate(string[] foo);
    public delegate string[] ARefAValDelegate(int[] foo);
    public delegate string[] ARefGenericDelegate<T>(T foo);

    public delegate void VoidVoidDelegate();
    public delegate void VoidRefDelegate(string foo);
    public delegate void VoidValDelegate(int foo);
    public delegate void VoidARefDelegate(string[] foo);
    public delegate void VoidAValDelegate(int[] foo);
    public delegate void VoidGenericDelegate<T>(T foo);

    public delegate int ValVoidDelegate();
    public delegate int ValRefDelegate(string foo);
    public delegate int ValValDelegate(int foo);
    public delegate int ValARefDelegate(string[] foo);
    public delegate int ValAValDelegate(int[] foo);
    public delegate int ValGenericDelegate<T>(T foo);

    public delegate string RefVoidDelegate();
    public delegate string RefRefDelegate(string foo);
    public delegate string RefValDelegate(int foo);
    public delegate string RefARefDelegate(string[] foo);
    public delegate string RefAValDelegate(int[] foo);
    public delegate string RefGenericDelegate<T>(T foo);

    public delegate U GenericVoidDelegate<U>();
    public delegate U GenericRefDelegate<U>(string foo);
    public delegate U GenericValDelegate<U>(int foo);
    public delegate U GenericARefDelegate<U>(string[] foo);
    public delegate U GenericAValDelegate<U>(int[] foo);
    public delegate U GenericGenericDelegate<T, U>(T foo);

    public delegate int[] AValVoidDelegate();
    public delegate int[] AValRefDelegate(string foo);
    public delegate int[] AValValDelegate(int foo);
    public delegate int[] AValARefDelegate(string[] foo);
    public delegate int[] AValAValDelegate(int[] foo);
    public delegate int[] AValGenericDelegate<T>(T foo);
  }
EOL

no_csc do
  class DelegateTester
    def self.bar
    end
  end
end
