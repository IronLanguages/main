require File.dirname(__FILE__) + '/../../spec_helper'
require 'timeout'

describe "Timeout.timeout" do
  it "raises Timeout::Error when it times out with no specified error type" do
    lambda {
      Timeout::timeout(1) do
        sleep 3
      end
    }.should raise_error(Timeout::Error)
  end

  it "raises specified error type when it times out" do
    lambda do
      Timeout.timeout(0.1, StandardError) do
        sleep 1
      end
    end.should raise_error(StandardError)
  end
  
  it "does not wait too long" do
    before_time = Time.now
    begin
      Timeout::timeout(1) do
        sleep 3
      end
    rescue Timeout::Error
      (Time.now - before_time).should < 1.2
    else
      flunk
    end
  end

  it "does not return too quickly" do
    before_time = Time.now
    begin
      Timeout::timeout(2) do
        sleep 3
      end
    rescue Timeout::Error
      (Time.now - before_time).should > 1.9
    else
      flunk
    end
  end

  it "returns back the last value in the block" do
    Timeout::timeout(1) do
      42
    end.should == 42
  end
  
  it "cancels the timeout if an exception is raised" do
    lambda { Timeout::timeout(2) { 1/0 } }.should raise_error(ZeroDivisionError)
  end
  
  it "accepts Float arguments" do
    Timeout::timeout(123.456) { 42 }.should == 42
  end  
end
