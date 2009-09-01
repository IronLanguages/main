require File.dirname(__FILE__) + '/../spec_helper'

describe "Enums" do
  it "map to Ruby classes" do
    EnumInt.should be_kind_of Class
    CustomEnum.should be_kind_of Class
  end
end

describe "Enum values" do
  it "map to instances of the enum's class" do
    System::Enum.get_names(EnumInt.to_clr_type).each do |e|
      EnumInt.send(e.to_s).should be_kind_of EnumInt
    end
    System::Enum.get_names(Enum.to_clr_type).each do |e|
      CustomEnum.send(e.to_s).should be_kind_of CustomEnum
    end
  end
end
