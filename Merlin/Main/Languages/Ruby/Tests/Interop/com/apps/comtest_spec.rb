require File.dirname(__FILE__) + "/../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "ComTest interop" do
  before(:each) do
    if !ComHelper.srv_registered?("ComTest.SimpleComObject")
      raise "ComTest.exe needs to be registered with Windows. Please run `ComTest.exe /regserver` before running this test, and run `ComTest.exe /unregserver` afterwords."
    end
    @obj = ComHelper.create_app("ComTest.SimpleComObject")
  end

  it "allows assignment and reading without name mangling" do
    @obj.should have_com_property("FloatProperty", 0, 1, 1.5)
  end

  it "allows assignment and reading with name mangling" do 
    @obj.should have_com_property("float_property", 0, 1, 1.5)
  end

  it "binds multiple out args" do
    #TODO: this should be a regression for CP# 2609, but it doesn't repro the
    #issue yet.
    test, str = BindingTester.MultipleOutArgs(@obj, @obj)

    test.should == @obj
    str.should == "MultipleOutArgs"
  end
end
