require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../../fixtures/enumerator/classes'
require 'enumerator'

describe "Enumerator#each_cons" do
  it "iterates the block for each consecutive slice of n elements" do
    a = []
    EnumSpecs::Numerous.new.each_cons(3) { |e| a << e }
    a.should == [[2, 5, 3], [5, 3, 6], [3, 6, 1], [6, 1, 4]]
  end  

  it "iterates over last tail slice" do
    a = []
    EnumSpecs::Numerous.new.each_cons(100) { |e| a << e }
    a.should == []
  end  

  it "stops iteration on exception" do
    a = []
    lambda {
      EnumSpecs::Numerous.new.each_cons(3) do |e| 
        a << e
        raise "stop iteration"
      end
    }.should raise_error(RuntimeError)
    a.should == [[2, 5, 3]]
  end  

  it "walks Enumerable elements strictly in forward direction, in parallel with iteration" do
    enumerated_elements = []
    EnumSpecs::Numerous.new.each_cons(3) { |e| enumerated_elements << ScratchPad.recorded.clone }
    enumerated_elements.should == [[2, 5, 3], [2, 5, 3, 6], [2, 5, 3, 6, 1], [2, 5, 3, 6, 1, 4]]
  end  

  it "allows Enumerable to change during the iteration" do
    a = []
    enumerable = EnumSpecs::Numerous.new
    enumerable.each_cons(3) do |e|
      a << e
      if enumerable.list.size < 10 then enumerable.list << 100 end
    end
    a.should == [[2, 5, 3], [5, 3, 6], [3, 6, 1], [6, 1, 4], [1, 4, 100], [4, 100, 100], [100, 100, 100], [100, 100, 100]]
  end  

  it "raises LocalJumpError if no block is given" do
    lambda { EnumSpecs::Numerous.new.each_cons(1) }.should raise_error(LocalJumpError)
  end  

  it "raises ArgumentError for n <= 0 " do
    lambda { EnumSpecs::Numerous.new.each_cons(0) { |e| } }.should raise_error(ArgumentError)
    lambda { EnumSpecs::Numerous.new.each_cons(-1) { |e| } }.should raise_error(ArgumentError)
  end  

  it "raises RangeError for Bignum parameter" do
    lambda { EnumSpecs::Numerous.new.each_cons(bignum_value) { |e| } }.should raise_error(RangeError)
  end  
end
