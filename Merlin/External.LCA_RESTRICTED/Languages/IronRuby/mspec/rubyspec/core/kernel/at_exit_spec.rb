require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Kernel.at_exit" do
  it "is a private method" do
    Kernel.should have_private_instance_method(:at_exit)
  end

  it "runs after all other code" do
    ruby_exe("at_exit {print 5}; print 6").should == "65"
  end

  it "runs in reverse order of registration" do
    code = "at_exit {print 4};at_exit {print 5}; print 6; at_exit {print 7}"
    ruby_exe(code).should == "6754"
  end
  
  it "runs after uncaught exception" do
    ruby_exe("at_exit { print 'at_exit:' + $!.message }; raise 'uncaught'").should == 'at_exit:uncaught'
  end

  it "runs after Kernel#exit" do
    ruby_exe("at_exit { print 5 }; exit").should == "5"
  end

  it "can call Kernel#exit" do
    ruby_exe("at_exit { exit(5) }")
    $?.exitstatus.should == 5
  end

  it "can raise uncaught exception" do
    ruby_exe("at_exit { raise 'bye' }", :args => "2>&1").should =~ /bye/
    $?.exitstatus.should == 1
  end

  it "can raise uncaught exception multiple times" do
    ruby_exe("at_exit { print 100; raise 'bye1' }; at_exit { raise 'bye2' }").should == "100"
  end

  it "can call exit multiple times" do
    ruby_exe("at_exit { exit(5) }; at_exit { exit(6) }")
    $?.exitstatus.should == 5
  end
  
  it "can call itself" do
    ruby_exe("at_exit { at_exit { print 100 } }").should == "100"
  end
end

describe "Kernel#at_exit" do
  it "needs to be reviewed for spec completeness"
end
