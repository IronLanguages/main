require File.dirname(__FILE__) + '/../../spec_helper'
require 'thread'

describe "Thread.exclusive" do
  it "should implement a critical region" do
    counter = 0
    threads = []
    10.times { threads << Thread.new { 1000.times { Thread.exclusive { counter += 1 } } } }
    threads.each { |t| t.join }
    counter.should == (10 * 1000)
  end
  
  it "allows break" do
    result = Thread.exclusive { break 100 }
    result.should == 100
    Thread.critical.should be_false
  end

  it "propagates the block return value" do
    result = Thread.exclusive { 100 }
    result.should == 100
  end  

  it "should raise LocalJumpError if no block is given" do
    lambda { Thread.exclusive }.should raise_error(LocalJumpError)
  end
end