require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Kernel#freeze" do
  before(:each) do
    @frozen_object_modified_error = TypeError  
    ruby_version_is "1.9" do
      @frozen_object_modified_error = RuntimeError
    end
  end

  it "prevents self from being further modified" do
    o = mock('o')
    o.frozen?.should == false
    o.freeze
    o.frozen?.should == true
  end

  # bug in 1.9?
  ruby_version_is "" ... "1.9" do
    it "has no effect on immediate values" do
      a = nil
      b = true
      c = false
      d = 1
      a.freeze
      b.freeze
      c.freeze
      d.freeze
      a.frozen?.should == false
      b.frozen?.should == false
      c.frozen?.should == false
      d.frozen?.should == false
    end
  end
  
  it "causes mutative calls to raise TypeError" do
    o = Class.new do
      def mutate; @foo = 1; end
    end.new
    o.freeze
    lambda {o.mutate}.should raise_error(@frozen_object_modified_error)
  end
  
  it "freezes singleton classes" do
    o = Object.new
    o.freeze
    lambda { class << o; def f; end; end }.should raise_error(@frozen_object_modified_error)
    lambda { class << o; class << self; def f; end; end; end }.should raise_error(@frozen_object_modified_error)
    lambda { class << o; class << self; class << self; def f; end; end; end; end }.should raise_error(@frozen_object_modified_error)
  end
end
