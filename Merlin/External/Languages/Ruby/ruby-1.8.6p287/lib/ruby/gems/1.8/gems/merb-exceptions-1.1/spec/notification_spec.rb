require File.dirname(__FILE__) + '/spec_helper'

include MerbExceptions
include NotificationSpecHelper

describe MerbExceptions::Notification do
  describe "#new" do
    it "should create a new notification without errors" do
      lambda { Notification.new(mock_details) }.should_not raise_error
    end

    it "should set the detail values to those provided" do
      Notification.new(mock_details).details.should == mock_details
    end
  end

  describe ".deliver!" do
    before :each do
      @notification = Notification.new(mock_details)
      @notification.stub!('deliver_web_hooks!')
    end

    after :each do
      Notification::Mailer.deliveries.clear
    end

    it "should deliver web hooks" do
      @notification.should_receive('deliver_web_hooks!')
      @notification.deliver!
    end

    it "should deliver emails" do
      Notification::Mailer.deliveries.length.should == 0
      @notification.deliver!
      Notification::Mailer.deliveries.length.should == 2
    end
  end

  describe ".deliver_web_hooks!" do
    before :each do
      @notification = Notification.new(mock_details)
      @notification.stub!(:post_hook)
    end

    it "should call post_hook for each url" do
      @notification.should_receive(:post_hook).
        once.with('http://www.test1.com')
      @notification.should_receive(:post_hook).
        once.with('http://www.test2.com')
      @notification.deliver_web_hooks!
    end
  end

  describe ".deliver_emails!" do
    before :each do
      @notification = Notification.new(mock_details)
      Notification::Mailer.deliveries.clear
    end

    it "should call send_notification_email for each address" do
      @notification.deliver_emails!
      Notification::Mailer.deliveries.first.to.should include("user1@test.com")
      Notification::Mailer.deliveries.first.from.should include("exceptions@myapp.com")
      Notification::Mailer.deliveries.last.to.should include("user2@test.com")
      Notification::Mailer.deliveries.first.text.should == Notification::Mailer.deliveries.last.text
    end
  end

end
