require File.dirname(__FILE__) + "/../spec_helper"

describe "AssemblyBuilder" do
  it "can be instantiatied and used" do
    name = System::Reflection::AssemblyName.new
    name.name = "Test"
    ab = System::AppDomain.current_domain.define_dynamic_assembly(name, System::Reflection::Emit::AssemblyBuilderAccess.run)
    ab.should have_method("add_resource_file")
    ab.should have_method("create_instance")
  end
end
