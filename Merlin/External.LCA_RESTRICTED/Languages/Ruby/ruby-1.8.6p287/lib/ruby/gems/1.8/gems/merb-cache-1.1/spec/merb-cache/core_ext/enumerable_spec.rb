require File.dirname(__FILE__) + '/../../spec_helper'

describe Enumerable do
  describe "#capture_first" do
    it "should return the result of the first block call that is non-nil, not the item sent to the block" do
      [1, 2, 3].capture_first {|i| i ** i if i % 2 == 0}.should == 4
    end

    it "should return nil if all block calls are nil" do
      [1, 2, 3].capture_first {|i| nil }.should be_nil
    end

    it "should stop calling the block once a block evaluates to non-nil" do
      lambda {
        [1, 2, 3].capture_first do |i|
          raise "#{i} is divisible by 3!" if i % 3 == 0
          i ** i if i % 2 == 0
        end
      }.should_not raise_error
    end
  end
end