require File.dirname(__FILE__) + '/../../spec_helper'
require 'timeout'

describe "Timeout.timeout" do
  def sleep_for_a_long_time()
    # Implementations which cannot timeout a thread in sleeping state can change this
    # to sleep for a finite amount of time
    sleep
  end
  
  it "raises Timeout::Error when it times out" do
    lambda {
      Timeout::timeout(1) do
        sleep_for_a_long_time
      end
    }.should raise_error(Timeout::Error)
  end
  
  it "shouldn't wait too long" do
    before_time = Time.now
    begin
      Timeout::timeout(1) do
        sleep_for_a_long_time
        flunk # "shouldn't get here"
      end
      flunk # "shouldn't get here"
    rescue Timeout::Error
      (Time.now - before_time).should < 1.2
    end
  end

  it "shouldn't return too quickly" do
    before_time = Time.now
    begin
      Timeout::timeout(2) do
        sleep_for_a_long_time
        flunk # "shouldn't get here"
      end
      flunk # "shouldn't get here"
    rescue Timeout::Error
      (Time.now - before_time).should > 1.9
    end
  end

  it "should return back the last value in the block" do
    Timeout::timeout(1) do
      42
    end.should == 42
  end
end
