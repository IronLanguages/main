require File.dirname(__FILE__) + '/../../spec_helper'

describe "ENV.each_value" do
  it "returns each value" do
    e = []
    orig = ENV.to_hash
    begin
      ENV.clear
      ENV["1"] = "3"
      ENV["2"] = "4"
      ENV.each_value { |v| e << v }
      e.should include("3")
      e.should include("4")
    ensure
      ENV.replace orig
    end
  end

  it "returns the value of break and stops execution of the loop if break is in the block" do
    e = []
    ENV.each_value {|k| break 1; e << k}.should == 1
    e.empty?.should == true
  end

  ruby_version_is "" ... "1.8.7" do
    it "raises LocalJumpError if no block given" do
      lambda { ENV.each_value }.should raise_error(LocalJumpError)
    end
  end

  ruby_version_is "1.8.7" do
    it "returns an Enumerator if called without a block" do
      ENV.each_value.should be_kind_of(Enumerable::Enumerator)
    end
  end

end
