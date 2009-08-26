require File.dirname(__FILE__) + '/../../spec_helper'

describe "Creating a .NET array" do
  it "takes a generic parameter" do
    System::Array.of(Fixnum).new(1).should == [0]
  end

  it "takes a size parameter" do
    System::Array.of(Fixnum).new(5).should == [0,0,0,0,0]
  end

  it "takes a default value" do
    System::Array.of(Fixnum).new(3,3).should == [3,3,3]
  end

  it "can be done via create_instance" do
    System::Array.create_instance(Fixnum.to_clr_type, 2).should == [0,0]
  end

  describe "with multiple dimensions" do
    before :each do
      @array = System::Array.CreateInstance(Fixnum.to_clr_type, 2, 3)
      (@array.get_lower_bound(0)..@array.get_upper_bound(0)).each do |i|
        (@array.get_lower_bound(1)..@array.get_upper_bound(1)).each do |j|
          @array.set_value((i*100)+(j*10), i, j)
        end
      end
    end

    it "can be done with multi-dimnsion arrays via create_instance" do
      @array.class.should equal_clr_string("System::Int32[,]")
    end

    it "can be referenced" do
      @array[1, 1].should == 110
      @array.get_value(1,1).should == 110
    end
  end
end

