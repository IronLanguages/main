require File.dirname(__FILE__) + '/../spec_helper'

describe "merb-auth-core" do
  it "should ensure_authentication" do
    dispatch_to(Users, :index) do |controller|
      controller.should_receive(:ensure_authenticated)
    end
  end
  
  it "should not ensure_authenticated when skipped" do
    dispatch_to(Dingbats, :index) do |controller|
      controller.should_not_receive(:ensure_authenticated)
    end
  end
end
