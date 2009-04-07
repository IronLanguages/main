require File.dirname(__FILE__) + "/spec_helper"

describe "Regression dev tests" do
  it "maps Ruby and CLR exceptions" do
    Errno::EACCES.should == System::UnauthorizedAccessException
  end
end
