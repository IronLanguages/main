require File.dirname(__FILE__) + '/../../spec_helper'

describe "Modifying the typeof" do
  before(:each) do
    @klass = Class.new do
      include System::Collections::Generic::IList[Fixnum]
    end
  end

  it "is allowed before instantiation" do
    lambda { 
      @klass.class_eval do
        include System::IDisposable
      end
      @klass.new.should be_kind_of(System::IDisposable)
    }.should_not raise_error
  end

  it "is not allowed after instantiation" do
    @klass.new.should be_kind_of @klass
    lambda {
      @klass.class_eval do
        include System::IDisposable
      end
    }.should raise_error TypeError
  end
end
