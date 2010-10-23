require File.dirname(__FILE__) + '/../spec_helper'

describe "Interfaces" do
  it "map to modules" do
    [IEmptyInterface, IInterface].each do |iface|
      iface.should be_kind_of Module
      iface.should_not be_kind_of Class
    end
  end

  it "map to modules when a concrete instance of a generic interface" do
    System::IEquatable.of(Fixnum).should be_kind_of Module
    System::IEquatable.of(Fixnum).should_not be_kind_of Class
  end
end
