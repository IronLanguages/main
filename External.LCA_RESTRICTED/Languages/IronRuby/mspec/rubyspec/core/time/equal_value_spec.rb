require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/methods'

describe "Time#==" do
  ruby_version_is "" ... "1.9" do
    it "returns nil when comparing with another type" do
      (Time.now == "a string").should == nil
    end
  end

  ruby_version_is "1.9" do
    it "returns false when comparing with another type" do
      (Time.now == "a string").should == false
    end
  end
end
