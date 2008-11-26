require File.dirname(__FILE__) + '/../spec_helper'
require 'rowantest.baseclasscs'

describe "Basic .NET classes" do
  it "map to Ruby classes" do
    Merlin::Testing::BaseClass::EmptyClass.class.should == Class
  end
end

