require File.dirname(__FILE__) + "/../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "Win32OLE COM interop" do
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
end

describe "Direct COM interop" do
  before(:each) do
    if !ComHelper.srv_registered?("ComTest.SimpleComObject")
      raise "ComTest.exe needs to be registered with Windows. Please run `ComTest.exe /regserver` before running this test, and run `ComTest.exe /unregserver` afterwords."
    end
    type = System::Type.GetTypeFromProgID("ComTest.SimpleComObject")
    @obj = System::Activator.create_instance type
  end
  
  it "allows to call a method" do
    @obj.HelloWorld.class.should == System::String
    @obj.HelloWorld.should == 'HelloWorld'
  end

  it "allows to call a method via mangled name" do
    @obj.hello_world.class.should == System::String
    @obj.hello_world.should == 'HelloWorld'
  end

  it "binds multiple out args" do
    # a regression for CP# 2609
    test, str = BindingTester.MultipleOutArgs(@obj, @obj)

    test.should == @obj
    str.should == "MultipleOutArgs"
  end
  
  it "allows conversion of a COM object to an arbitrary interface and throws if the COM object doesn't implement it" do
    lambda { BindingTester.with_iface(@obj) }.should raise_error(System::InvalidCastException)
  end

  it "can handle super" do
    begin
      class Object
        alias_method :mm, :method_missing
        def method_missing(meth, *args, &blk)
          super
        end
      end
      lambda {@obj.not_a_method}.should raise_error NoMethodError
    ensure
      class Object
        alias_method :method_missing, :mm
      end
    end
  end
end
