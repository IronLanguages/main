require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::Flag do

  describe ".new" do
    it "should create a Class" do
      DataMapper::Types::Flag.new.should be_instance_of(Class)
    end

    it "should create unique a Class each call" do
      DataMapper::Types::Flag.new.should_not == DataMapper::Types::Flag.new
    end

    it "should use the arguments as the values in the @flag_map hash" do
      DataMapper::Types::Flag.new(:first, :second, :third).flag_map.values.should == [:first, :second, :third]
    end

    it "should create keys by the 2 power series for the @flag_map hash, staring at 1" do
      DataMapper::Types::Flag.new(:one, :two, :three, :four, :five).flag_map.keys.should include(1, 2, 4, 8, 16)
    end
  end

  describe ".[]" do
    it "should be an alias for the new method" do
      DataMapper::Types::Flag.should_receive(:new).with(:uno, :dos, :tres)
      DataMapper::Types::Flag[:uno, :dos, :tres]
    end
  end

  describe ".dump" do
    before(:each) do
      @flag = DataMapper::Types::Flag[:first, :second, :third, :fourth, :fifth]
    end

    it "should return the key of the value match from the flag map" do
      @flag.dump(:first, :property).should == 1
      @flag.dump(:second, :property).should == 2
      @flag.dump(:third, :property).should == 4
      @flag.dump(:fourth, :property).should == 8
      @flag.dump(:fifth, :property).should == 16
    end

    it "should return a binary flag built from the key values of all matches" do
      @flag.dump([:first, :second], :property).should == 3
      @flag.dump([:second, :fourth], :property).should == 10
      @flag.dump([:first, :second, :third, :fourth, :fifth], :property).should == 31
    end

    it "should return a binary flag built from the key values of all matches even if strings" do
      @flag.dump(["first", "second"], :property).should == 3
      @flag.dump(["second", "fourth"], :property).should == 10
      @flag.dump(["first", "second", "third", "fourth", "fifth"], :property).should == 31
    end

    it "should return 0 if there is no match" do
      @flag.dump(:zero, :property).should == 0
    end
  end

  describe ".load" do
    before(:each) do
      @flag = DataMapper::Types::Flag[:uno, :dos, :tres, :cuatro, :cinco]
    end

    it "should return the value of the key match from the flag map" do
      @flag.load(1,  :property).should == [:uno]
      @flag.load(2,  :property).should == [:dos]
      @flag.load(4,  :property).should == [:tres]
      @flag.load(8,  :property).should == [:cuatro]
      @flag.load(16, :property).should == [:cinco]
    end

    it "should return an array of all flags matches" do
      @flag.load(3,  :property).should include(:uno, :dos)
      @flag.load(10, :property).should include(:dos, :cuatro)
      @flag.load(31, :property).should include(:uno, :dos, :tres, :cuatro, :cinco)
    end

    it "should return an empty array if there is no key" do
      @flag.load(-1,  :property).should == []
      @flag.load(nil, :property).should == []
      @flag.load(32,  :property).should == []
    end
  end
end
