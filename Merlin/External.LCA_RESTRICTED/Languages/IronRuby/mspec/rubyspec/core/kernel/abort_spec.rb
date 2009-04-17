require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Kernel#abort" do
  it "is a private method" do
    Kernel.private_instance_methods.should include("abort")
  end
  
  it "prints the message to stderr" do
    ruby_exe("abort('abort message')").chomp.should == ''
    ruby_exe("abort('abort message')", :args => "2>&1").chomp.should == 'abort message'
  end
  
  it "does not allow the message to be nil or String-like object" do
    lambda { abort(nil) }.should raise_error(TypeError)
    
    m = mock('message')
    m.should_not_receive(:to_str)
    lambda { abort(m) }.should raise_error(TypeError)
  end
  
  it "sets the exit code to 1" do
    ruby_exe("abort")
    $?.exitstatus.should == 1
  end
end

describe "Kernel.abort" do
  it "needs to be reviewed for spec completeness"
end
