require File.dirname(__FILE__) + "/../spec_helper"

describe "ADODB COM interop" do
  it "allows assignment and reading without name mangling" do
    obj = System::Activator.create_instance(System::Type.GetTypeFromProgID("ADODB.Connection"))
    # This fails with : 'GetNativeVariantForObject': Value does not fall within the expected range. (ArgumentError)
    # obj = WIN32OLE.new("ADODB.Connection") 
    obj.CommandTimeout = 40
    obj.CommandTimeout.should == 40
    obj = WIN32OLE.new("ADODB.Command")
    obj.CommandTimeout = 50
    obj.CommandTimeout.should == 50
  end
  
  it "allows assignment and reading with name mangling" do
    obj = WIN32OLE.new("ADODB.Connection")
    obj.command_timeout = 40
    obj.command_timeout.should == 40
    obj = WIN32OLE.new("ADODB.Command")
    obj.command_timeout = 50
    obj.command_timeout.should == 50
  end
end
