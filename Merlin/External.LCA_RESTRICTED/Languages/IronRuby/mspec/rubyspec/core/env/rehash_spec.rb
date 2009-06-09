require File.dirname(__FILE__) + '/../../spec_helper'

describe "ENV.rehash" do
  it "returns nil" do
    ENV.rehash.should == nil
  end
end
