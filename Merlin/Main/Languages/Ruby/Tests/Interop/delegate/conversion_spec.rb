require File.dirname(__FILE__) + '/../spec_helper'

describe "Converting procs to delegates" do
  bin = "#{ENV['MERLIN_ROOT']}\\bin\\debug"
  reference "#{bin}\\Microsoft.Scripting.ExtensionAttribute.dll"
  reference "#{bin}\\Microsoft.Scripting.dll"
  reference "#{bin}\\Microsoft.Scripting.Core.dll"
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
  EOL
  before :each do
    @klass = DelegateConversionClass.new("lambda {|a| a.to_i}")
  end

  #TODO: does this belong somewhere else?
  it "can directly invoke a lambda" do
    @klass.direct_invoke.should == 1
  end

  it "can convert to a lambda" do
    @klass.convert_to_delegate.should == 1
  end
end
