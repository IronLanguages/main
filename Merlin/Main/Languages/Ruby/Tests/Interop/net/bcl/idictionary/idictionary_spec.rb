require File.dirname(__FILE__) + '/../../spec_helper'

describe "IDictionary support" do
  before(:each) do
    @dict = System::Collections::Generic::Dictionary[Object, Object].new
    @dict[:foo] = 'bar'
  end

  it "inspects as a hash" do
    @dict.inspect.should == '{:foo=>"bar"}'
  end
  
  it "allows indexer support for setting and getting values" do
    @dict[:baz] = 'raz'
    @dict[:baz].should == 'raz'
    @dict[:foo].should == 'bar'
  end
end