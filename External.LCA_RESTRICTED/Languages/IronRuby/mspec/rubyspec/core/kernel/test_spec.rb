require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Kernel#test" do
  before :all do
    @file = File.dirname(__FILE__) + '/fixtures/classes.rb'
    @dir = File.dirname(__FILE__) + '/fixtures'
  end
  
  it "is a private method" do
    Kernel.should have_private_instance_method(:test)
  end
  
  it "returns true when passed ?f if the argument is a regular file" do
    Kernel.test(?f, @file).should == true
  end
  
  it "returns true when passed ?e if the argument is a file" do
    Kernel.test(?e, @file).should == true
  end
  
  it "returns true when passed ?d if the argument is a directory" do
    Kernel.test(?d, @dir).should == true
  end

  it "returns an object of type Time if the argument is a directory or file" do
    Kernel.test(?A, @file).kind_of?(Time).should == true
    Kernel.test(?A, @dir).kind_of?(Time).should == true
  end

  it "returns true when passed ?b if the argument is a block device" do
    Kernel.test(?b, @file).should == false
  end

  it "returns an object of type Time if the argument is a directory or file" do
    Kernel.test(?C, @file).kind_of?(Time).should == true
    Kernel.test(?C, @dir).kind_of?(Time).should == true
  end

  it "returns true when passed ?c if the argument is a character device" do
    Kernel.test(?c, @file).should == false
  end

  it "returns true when passed ?g if the argument has the GID set (false on NT and thus, .NET)" do
    Kernel.test(?g, @file).should == false
  end

  it "returns true when passed ?G if the argument exists and has a group ownership equal to the caller's group" do
    Kernel.test(?G, @file).should == false
  end

  it "returns true when passed ?k if the argument has the sticky bit set" do
    Kernel.test(?k, @file).should == nil
  end

  it "returns true when passed ?l if the argument is a symlink" do
    Kernel.test(?l, @file).should == false
  end

  ruby_version_is "1.9" do
    it "calls #to_path on second argument when passed ?f and a filename" do
      p = mock('path')
      p.should_receive(:to_path).and_return @file
      Kernel.test(?f, p)
    end
    
    it "calls #to_path on second argument when passed ?e and a filename" do
      p = mock('path')
      p.should_receive(:to_path).and_return @file
      Kernel.test(?e, p)
    end
    
    it "calls #to_path on second argument when passed ?d and a directory" do
      p = mock('path')
      p.should_receive(:to_path).and_return @dir
      Kernel.test(?d, p)
    end
  end
end

describe "Kernel.test" do
  it "needs to be reviewed for spec completeness"
end
