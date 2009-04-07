require File.join(File.dirname(__FILE__), "..", 'spec_helper.rb')

describe "Merb::Authentication.customizations" do
  
  before(:each) do
    Merb::Authentication.default_customizations.clear
  end
  
  it "should allow addition to the customizations" do
    Merb::Authentication.customize_default { "ONE" }
    Merb::Authentication.default_customizations.first.call.should == "ONE"
  end
  
  it "should allow multiple additions to the customizations" do
    Merb::Authentication.customize_default {"ONE"}
    Merb::Authentication.customize_default {"TWO"}
    
    Merb::Authentication.default_customizations.first.call.should == "ONE"
    Merb::Authentication.default_customizations.last.call.should  == "TWO"
  end
  
end