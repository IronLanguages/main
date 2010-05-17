require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "Inheriting from classes with" do
  describe "parameter attributes" do
    it "passes on optional constructors to subclasses" do
      RubyClassWithOptionalConstructor.new.arg.should == 0
    end


  end

  describe "method attributes that are abstract" do
    it "can call super" do
      lambda {class SubUnsafe < Unsafe
        def foo
          super
        end
      end}.should_not raise_error
    end
  end
end
