require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/methods'

describe "Time#zone" do
  platform_is_not :windows do
    # zone names not available on Windows w/o supplying zone offsets
    it "returns the time zone abbreviation used for time" do
      with_timezone("AST") do
        Time.now.zone.should == "AST"
      end
      
      with_timezone("Asia/Kuwait") do
        Time.now.zone.should == "AST"
      end
    end
  end  
  
  it "returns the time zone abbreviation used for time" do
    with_timezone("AST", 3) do
      Time.now.zone.should == "AST"
    end
  end
  
  it "returns UTC for utc times" do
    with_timezone("AST", 3) do
      Time.utc(2000).zone.should == "UTC"
    end
  end
end
