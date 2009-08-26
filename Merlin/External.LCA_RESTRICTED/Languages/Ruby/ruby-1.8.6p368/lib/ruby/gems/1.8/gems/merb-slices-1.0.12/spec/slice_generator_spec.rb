require File.dirname(__FILE__) + '/spec_helper'

describe Merb::Generators::SliceGenerator do
  
  it "should invoke the full generator by default" do
    generator = Merb::Generators::SliceGenerator.new('/tmp', { :pretend => true }, 'testing')
    #generator.invoke!
    generator.invocations.first.class.should == Merb::Generators::FullSliceGenerator
  end
  
  it "should invoke the flat generator if --thin is set" do
    generator = Merb::Generators::SliceGenerator.new('/tmp', { :pretend => true, :thin => true }, 'testing')
    #generator.invoke!
    generator.invocations.first.class.should == Merb::Generators::ThinSliceGenerator
  end
  
  it "should invoke the very flat generator if --very-thin is set" do
    generator = Merb::Generators::SliceGenerator.new('/tmp', { :pretend => true, :very_thin => true }, 'testing')
    #generator.invoke!
    generator.invocations.first.class.should == Merb::Generators::VeryThinSliceGenerator
  end
  
end