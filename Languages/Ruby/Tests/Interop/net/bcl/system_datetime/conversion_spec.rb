require File.dirname(__FILE__) + '/../../spec_helper'

describe "System::DateTime conversion" do
  before :each do
    @dt = System::DateTime.new(2000, 1, 1)
    @t = Time.local(2000,1,1) 
  end
  
  it "supports conversion from Time" do
    #(@dt - @t).should be_kind_of(System::TimeSpan)
    System::DateTime.compare(@t, @t).should == 0
  end

  it "supports conversion to Time" do
    (@t - @dt).should be_kind_of(Float)
  end
end
