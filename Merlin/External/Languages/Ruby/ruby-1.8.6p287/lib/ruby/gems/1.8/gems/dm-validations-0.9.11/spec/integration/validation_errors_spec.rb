require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::Validate::ValidationErrors do

  before(:each) do
    @errors = DataMapper::Validate::ValidationErrors.new
  end

  it "should report that it is empty on first creation" do
    @errors.empty?.should == true
  end

  it "should continue to report that it is empty even after being checked" do
    @errors.on(:foo)
    @errors.empty?.should == true
  end
end
