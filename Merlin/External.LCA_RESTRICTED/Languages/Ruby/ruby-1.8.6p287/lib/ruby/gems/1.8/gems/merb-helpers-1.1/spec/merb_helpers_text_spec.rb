require File.dirname(__FILE__) + '/spec_helper'

describe "cycle" do
  include Merb::Helpers::Text

  it "do basic cycling" do
    cycle("one", 2, "3").should == 'one'
    cycle("one", 2, "3").should == '2'
    cycle("one", 2, "3").should == '3'
    cycle("one", 2, "3").should == 'one'
    cycle("one", 2, "3").should == '2'
    cycle("one", 2, "3").should == '3'
  end

  it "should reset with new values" do
    cycle("even", "odd").should == 'even'
    cycle("even", "odd").should == 'odd'
    cycle("even", "odd").should == 'even'
    cycle("even", "odd").should == 'odd'
    cycle(1, 2, 3).should == '1'
    cycle(1, 2, 3).should == '2'
    cycle(1, 2, 3).should == '3'
    cycle(1, 2, 3).should == '1'
  end

  it "should support named cycles" do
    cycle(1, 2, 3, :name => "numbers").should == "1"
    cycle("red", "blue", :name => "colors").should == "red"
    cycle(1, 2, 3, :name => "numbers").should == "2"
    cycle("red", "blue", :name => "colors").should == "blue"
    cycle(1, 2, 3, :name => "numbers").should == "3"
    cycle("red", "blue", :name => "colors").should == "red"
  end

  it "should use a named cycle called 'default' by default" do
    cycle(1, 2, 3).should == "1"
    cycle(1, 2, 3, :name => "default").should == "2"
    cycle(1, 2, 3).should == "3"
  end

  it "should be able to be reset" do
    cycle(1, 2, 3).should == "1"
    cycle(1, 2, 3).should == "2"
    reset_cycle
    cycle(1, 2, 3).should == "1"
  end

  it "should be able to reset a named cycle" do
    cycle(1, 2, 3, :name => "numbers").should == "1"
    cycle("red", "blue", :name => "colors").should == "red"
    reset_cycle("numbers")
    cycle(1, 2, 3, :name => "numbers").should == "1"
    cycle("red", "blue", :name => "colors").should == "blue"
    cycle(1, 2, 3, :name => "numbers").should == "2"
    cycle("red", "blue", :name => "colors").should == "red"
  end

  it "should work with things other than strings" do
    class Red; def to_s; 'red'; end; end;
    class Blue; def to_s; 'blue'; end; end;
    red = Red.new
    blue = Blue.new
    cycle(red, blue).should == "red"
    cycle(red, blue).should == "blue"
    cycle(red, blue).should == "red"
  end
end
