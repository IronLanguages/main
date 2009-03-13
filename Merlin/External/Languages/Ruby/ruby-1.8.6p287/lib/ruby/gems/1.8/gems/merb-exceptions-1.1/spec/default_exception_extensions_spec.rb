require File.dirname(__FILE__) + '/spec_helper'

describe MerbExceptions::DefaultExceptionExtensions do

  before(:each) do
    Merb::Router.prepare do
      default_routes
    end
  end

  it "should notify_of_exceptions" do
    MerbExceptions::Notification.should_receive(:new)
    request("/raise_error/index")
  end

end