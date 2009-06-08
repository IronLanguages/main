require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Module#freeze" do
  before(:each) do
    @frozen_object_modified_error = TypeError  
    ruby_version_is "1.9" do
      @frozen_object_modified_error = RuntimeError
    end
  end
  
  it "prevents further modifications to self" do
    m = Module.new.freeze
    m.frozen?.should == true

    # Does not raise
    class << m; end

    lambda { m.module_eval { def f; end; } }.should raise_error(@frozen_object_modified_error)
    lambda { m.module_eval { include Enumerable } }.should raise_error(@frozen_object_modified_error)
    lambda { m.send(:define_method, :f) {} }.should raise_error(@frozen_object_modified_error)
    lambda { m.const_set(:C, 1) }.should raise_error(@frozen_object_modified_error)
    lambda { m.send :instance_variable_set, :@x, 1 }.should raise_error(@frozen_object_modified_error)
    lambda { m.send :class_variable_set, :@@x, 1 }.should raise_error(@frozen_object_modified_error)
    
    lambda { class << m; def f; end; end }.should raise_error(@frozen_object_modified_error)
    lambda { class << m; class << self; def f; end; end; end }.should raise_error(@frozen_object_modified_error)
  end
end
