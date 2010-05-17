require File.dirname(__FILE__) + "/../../spec_helper"

describe "IronPython interop" do
  before :each do
    @engine,@scope = SpecHelper.create_scoped_engine
    @py_module = IronRuby.require(fixture(__FILE__, "test.py"))

  end
  it "allows evaluation of IDOMPs" do
    load_assembly "IronPython"
    e = IronPython::Hosting::Python.CreateEngine
    lambda { e.execute "str" }.should_not raise_error
  end
  
  it "scopes constants" do
    lambda { ::Bar }.should raise_error 
  end

  it "has access to module functions" do
    @py_module.me("word").should == "word"
  end

  it "has access to classes" do
    #TODO: How can i get the class object?
    @py_module.bar.baz == @py_module.foo
    @py_module.bar.boo(1) == 1
  end

  it "has access to classes" do
    @pymodule.method(:bar).should be_kind_of Class
  end

  it "has access to top level variables" do
    @py_module.foo.should == 1
    @py_module.foo += 1
    @py_module.foo.should == 2
  end
end
