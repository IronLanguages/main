require File.dirname(__FILE__) + '/../spec_helper'

describe "Interfaces" do
  csc <<-EOL
    public class ImplementsIInterface : IInterface {
      public void m() {
        return;
      }
    }
  EOL
  
  before(:all) do
    # class definition in a before block because csc.rb doesn't like the include
    class RubyImplementsIInterface
      include IInterface
      
      def m; end  
    end
  end
  
  it "be in ancestor list" do
    ImplementsIInterface.ancestors.should include(IInterface)
    RubyImplementsIInterface.ancestors.should include(IInterface)
  end
end