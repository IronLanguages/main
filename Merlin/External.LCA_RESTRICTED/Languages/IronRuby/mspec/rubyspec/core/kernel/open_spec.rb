require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Kernel#open" do
  it "is a private method" do
    Kernel.should have_private_instance_method(:open)
  end
  
  before :each do
    @file = tmp("kernel_test.txt")
    @newfile = tmp("kernel_test.new")
    @fh = nil
    touch(@file) { |f| f.write "This is a test" }
  end
  
  after :each do
    @fh.close if @fh and not @fh.closed?
    rm_r @file
    rm_r @newfile
  end
  
  it "opens a file when given a valid filename" do
    @fh = open(@file)
    @fh.class.should == File
  end
  
  it "opens a file when called with a block" do
    open(@file, "r") { |f| f.gets }.should == "This is a test\n"
  end
  
  it "sets a default permission of writable" do
    @fh = open(@file)
    File.writable?(@file).should be_true  
  end
  
  it "sets permissions of newly created file" do
    open(@newfile, "w", 0444){ }
    File.writable?(@newfile).should be_false
  end
  
  it "sets the file as writable if perm is nil" do
    open(@newfile, "w", nil){ }
    File.writable?(@newfile).should be_true
  end
  
  it "ignores perm for existing file" do
    open(@file, "r", 0444) { }
    File.writable?(@file).should be_true  
  end
  
  platform_is_not :windows do
    
    it "opens an io when path starts with a pipe" do
      @io = open("|date")
      @io.should be_kind_of(IO)
    end
    
    it "opens an io when called with a block" do
      @output = open("|date") { |f| f.gets }
      @output.should_not == ''
    end
  
  end

  platform_is :windows do
    
    it "opens an io when path starts with a pipe" do
      @io = open("|date /t")
      @io.should be_kind_of(IO)
    end
    
    it "opens an io when called with a block" do
      @output = open("|date /t") { |f| f.gets }
      @output.should_not == ''
    end
    
    it "NotImplementedError when called with |-" do
      lambda { open("|-") }.should raise_error(NotImplementedError)
    end
  
  end
    
  it "returns block return value" do
    open(@file) { :end_of_block }.should == :end_of_block
  end
    
  it "raises an ArgumentError if not passed one argument" do
    lambda { open }.should raise_error(ArgumentError)
  end
  
  it "raises a TypeError if passed nil for path" do
    lambda { open(nil) }.should raise_error(TypeError)
  end
  
  it "accepts String-like objects for path" do
    file = mock('filename')
    file.should_receive(:to_str).and_return(@file)
    open(file) { }
  end
  
  it "allows nil for mode" do
    open(@file, nil) { |f| lambda { f << "some output" }.should raise_error(IOError) }
  end
  
  it "allows String-like objects for mode" do
    mode = mock('mode')
    mode.should_receive(:to_str).and_return("r")
    open(@file, mode) { }
  end
  
  it "allows nil for perm" do
    open(@newfile, "w", nil) { }
    File.writable?(@file).should be_true
  end
  
  it "allows Integer-like objects for perm" do
    perm = mock('perm')
    perm.should_receive(:to_int).and_return(0444)
    open(@newfile, "w", perm) { }
    File.writable?(@newfile).should be_false
  end

  ruby_version_is "1.9" do
    it "calls #to_open on argument" do
      obj = mock('fileish')
      obj.should_receive(:to_open).and_return(File.open(@file))
      @file = open(obj)
      @file.class.should == File
    end
    
    it "raises a TypeError if passed a non-String that does not respond to #to_open" do
      obj = mock('non-fileish')
      lambda { open(obj) }.should raise_error(TypeError)
      lambda { open(nil) }.should raise_error(TypeError)
      lambda { open(7)   }.should raise_error(TypeError)
    end 
  end

  ruby_version_is ""..."1.9" do
    it "raises a TypeError if not passed a String type" do
      lambda { open(nil)       }.should raise_error(TypeError)
      lambda { open(7)         }.should raise_error(TypeError)
      lambda { open(mock('x')) }.should raise_error(TypeError)
    end
  end
end

describe "Kernel.open" do
  it "needs to be reviewed for spec completeness"
end
