require File.dirname(__FILE__) + '/spec_helper'

describe "#strftime_ordinalized" do
  
  before(:all) do
    @date = Date.parse('2008-5-3')
    @time = Time.parse('2008-5-3 14:00')
  end
  
  it "should ordinalize a date even without a locale passed" do
    @date.strftime_ordinalized('%b %d, %Y').should == "May 3rd, 2008"
  end
  
end

describe "to_ordinalized_s" do

  before(:each) do
    @date = Date.parse('2008-5-3')
    @time = Time.parse('2008-5-3 14:00')
  end

  it "should render a date using #to_s if no format is passed" do
    @date.to_ordinalized_s.should == @date.to_s
  end

  it "should render a time using #to_s if no format is passed" do
    @time.to_ordinalized_s.should == @time.to_s
  end

  it "should render a date or time using the db format" do
    @date.to_ordinalized_s(:db).should == "2008-05-03 00:00:00"
    @time.to_ordinalized_s(:db).should == "2008-05-03 14:00:00"
  end

  it "should render a date or time using the long format" do
    @date.to_ordinalized_s(:long).should == "May 3rd, 2008 00:00"
    @time.to_ordinalized_s(:long).should == "May 3rd, 2008 14:00"
  end

  it "should render a date or time using the time format" do
    @date.to_ordinalized_s(:time).should == "00:00"
    @time.to_ordinalized_s(:time).should == "14:00"
  end

  it "should render a date or a time using the short format" do
    @date.to_ordinalized_s(:short).should == "3rd May 00:00"
    @time.to_ordinalized_s(:short).should == "3rd May 14:00"
  end

end