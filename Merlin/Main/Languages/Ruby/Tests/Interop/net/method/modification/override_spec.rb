require File.dirname(__FILE__) + '/../../spec_helper'

describe "Overriding .NET methods" do 
  before(:each) do
    @obj = ClassWithMethods.new
  end

  it "is allowed via alias" do
    @obj.metaclass_temp_alias("_public_method","public_method") do
      @obj._public_method.should equal_clr_string("public")
    end
    @obj.public_method.should equal_clr_string("public")
  end

  it "is allowed via defining" do
    @obj.metaclass_temp_alias("_public_method", "public_method") do
      @obj.metaclass_def("public_method") {:not_public}
      @obj.public_method.should == :not_public
      @obj._public_method.should equal_clr_string("public")
    end
    @obj.public_method.should equal_clr_string("public")
  end

  it "maintains super method" do
    @obj.metaclass_temp_alias("_summing_method","summing_method") do
      @obj.metaclass_eval do
        def summing_method(*args)
          super(args.inject(0) {|a,v| a+v},1)
        end
      end
      @obj.summing_method(1,2).should == 4
      @obj.summing_method(1,2,3,4).should == 11
    end
  end
end

describe "Overriding virtual methods" do
  before(:each) do
    @meth = VirtualMethodBaseClass.instance_method("virtual_method")
  end

  it "call the correct method" do
    @meth.bind(VirtualMethodOverrideNew.new).call.should equal_clr_string("virtual")
  end

  it "call the correct method for overrides" do
    #override methods cannot be rebound
    @meth.bind(VirtualMethodOverrideOverride.new).call.should equal_clr_string("override")
  end
end
