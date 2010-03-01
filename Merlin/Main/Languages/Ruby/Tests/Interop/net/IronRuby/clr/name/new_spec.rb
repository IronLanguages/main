require File.dirname(__FILE__) + '/../../../../spec_helper'

describe "IronRuby::Clr::Name.new" do
  it "returns a new IronRuby::Clr::Name object" do
    IronRuby::Clr::Name.new("foo").should be_kind_of IronRuby::Clr::Name
  end

  it "converts symbols" do
    name = IronRuby::Clr::Name.new(:a_sym)
    name.to_s.should == "a_sym"
  end

  it "converts ints that correspond to symbols" do
    id = :a_sym.to_i
    name = IronRuby::Clr::Name.new id
    name.to_s.should == "a_sym"
  end

  it "raises argument error if the int doesn't map to a symbol" do
    id = 1000
    while(id.to_sym)
      id += 1
    end
    lambda {IronRuby::Clr::Name.new id}.should raise_error(ArgumentError)
  end
  
  it "attempts to convert the argument with to_str" do
    obj = mock('obj')
    obj.should_receive(:to_str).and_return("obj")
    name = IronRuby::Clr::Name.new obj
    name.to_s.should == "obj"
  end

  it "allows empty strings" do
    name = IronRuby::Clr::Name.new ''
    name.to_s.should == ''
  end
  
  it "raises if given something else" do
    lambda {IronRuby::Clr::Name.new nil}.should raise_error
    lambda {IronRuby::Clr::Name.new 1.0}.should raise_error
    lambda {IronRuby::Clr::Name.new true}.should raise_error
  end
end

