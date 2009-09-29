require File.dirname(__FILE__) + '/../../spec_helper'

describe "Converting Ruby arrays to .NET arrays" do
  before :each do
    @method = Klass.new.method(:array_accepting_method)
  end

  it "defaults to conversion to an object array" do
    @method.of(Object).call([1, "string"].to_clr_array).should == [1, "string"]
  end

  it "properly converts to object array" do
    @method.of(Object).call([1, "string"].to_clr_array(Object)).should == [1, "string"]
  end

  it "properly converts to typed array" do
    @method.of(Fixnum).call([1,2,3].to_clr_array(Fixnum)).should == [1,2,3]
  end
end
