require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Kernel#abort" do
  it "is a private method" do
    Kernel.should have_private_instance_method(:abort)
  end
  
  it "prints the message to stderr" do
    ruby_exe("abort('abort message')").chomp.should == ''
    ruby_exe("abort('abort message')", :args => "2>&1").chomp.should == 'abort message'
  end
  
  it "does not allow the message to be nil" do
    lambda { abort(nil) }.should raise_error(TypeError)
  end
  
  it "sets the exit code to 1" do
    ruby_exe("abort")
    $?.exitstatus.should == 1
  end
end

describe "Kernel.abort" do
  it "needs to be reviewed for spec completeness"
end
