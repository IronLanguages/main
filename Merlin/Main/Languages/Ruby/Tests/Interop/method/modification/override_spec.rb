require File.dirname(__FILE__) + '/../../spec_helper'

describe "Overriding .NET methods" do 
  before(:each) do
    @obj = ClassWithMethods.new
  end

  it "is allowed via alias" do
    class << @obj
      alias _public_method public_method
    end

    @obj._public_method.to_s.should == "public"

    class << @obj
      alias public_method _public_method
    end
    @obj.public_method.to_s.should == "public"
  end

  it "is allowed via defining" do
    class << @obj
      alias _public_method public_method
      def public_method
        return :not_public
      end
    end

    @obj.public_method.should == :not_public
    @obj._public_method.to_s.should == "public"

    class << @obj
      alias public_method _public_method
    end

    @obj.public_method.to_s.should == "public"
  end
end

describe "Overriding virtual methods" do
  csc <<-EOL
    public class VirtualMethodBaseClass { 
      public virtual string VirtualMethod() { return "virtual"; } 
    }
    public class VirtualMethodOverrideNew : VirtualMethodBaseClass { 
      new public virtual string VirtualMethod() { return "new"; } 
    }
    public class VirtualMethodOverrideOverride : VirtualMethodBaseClass {
      public override string VirtualMethod() { return "override"; } 
    }
  EOL
  before(:each) do
    @meth = VirtualMethodBaseClass.instance_method("virtual_method")
  end

  it "call the correct method" do
    @meth.bind(VirtualMethodOverrideNew.new).call.to_s.should == "virtual"
  end

  it "call the correct method for overrides" do
    #override methods cannot be rebound
    @meth.bind(VirtualMethodOverrideOverride.new).call.to_s.should == "override"
  end
end
