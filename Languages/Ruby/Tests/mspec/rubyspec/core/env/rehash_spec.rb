require File.expand_path('../../../spec_helper', __FILE__)

describe "ENV.rehash" do
  it "returns nil" do
    ENV.rehash.should == nil
  end
end
