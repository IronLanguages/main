require File.dirname(__FILE__) + "/../../spec_helper"

describe :array_subtraction, :shared => true do
  before(:each) do
    @arr = (@method.new(0)..@method.new(4)).to_a
    @ne_values = [2,2.0]
    @ne_values << [System::Byte, System::SByte].map {|c| c.new(2)}
    @eq_value = @method.new(2)
    @ne_values = @ne_values - [@eq_value]
  end

  it "doesn't delete the value if the types don't match" do
    @ne_values.each {|val| (@arr - [val]).should include(@eq_value)}
  end

  it "deletes the value if the type matches" do
    (@arr - [@eq_value]).should_not include(@eq_value)
  end
end
describe :range_creation, :shared => true do
  it "creates a range of the given type" do
    arr = (@method.new(0)..@method.new(6))
    arr.each {|el| el.should be_kind_of(@method)}
  end

  it "converts to bignum or fixnum when it reaches maxvalue" do
    max = @method.max_value.to_i
    top = max + 6
    arr = (@method.new(max)..top).to_a
    arr[0].should be_kind_of(@method)
    arr[1..-1].each {|el| el.should be_kind_of((1+max.to_i).to_i.class)}
  end
end

#TODO: Float, Decimal, Bignum
[System::Byte, System::SByte, System::Int16, System::UInt16, System::UInt16, System::Int64, System::UInt64 ].each do |klass|
  describe "Array subtraction on #{klass}" do
    it_behaves_like :array_subtraction, klass
  end

  describe "Array range creation #{klass}" do
    it_behaves_like :range_creation, klass
  end
end
