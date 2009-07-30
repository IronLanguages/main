require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../shared/calling"

describe "Invoking an private .NET method on a internal class" do 
  csc <<-EOL
  internal partial class PartialClassWithMethods {
    internal int Foo(){ return 1; }
  }
  EOL

  if IronRuby.configuration.private_binding
    it "works with -X:PrivateBindings" do
      PartialClassWithMethods.new.foo.should == 1
    end
  end
end
