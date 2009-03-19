require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::Flag do
  before(:all) do
    class ::Shirt
      include DataMapper::Resource
      property :id, Serial
      property :sizes, DM::Flag[:xs, :small, :medium, :large, :xl, :xxl]
    end
    Shirt.auto_migrate!
  end

  it "should save with create({:flag_field => [:flags]})" do
    lambda { Shirt.create(:sizes => [:medium, :large]) }.should_not raise_error
    repository do
      Shirt.get(1).sizes.should == [:medium, :large]
    end
  end

  it "should save with flag_field=[:flags]" do
    shirt = Shirt.new
    shirt.sizes = [:small, :xs]
    lambda { shirt.save }.should_not raise_error
    repository do
      Shirt.get(2).sizes.should == [:xs, :small]
    end
  end

  it 'should immediately typecast supplied values' do
    Shirt.new(:sizes => [:large, :xl]).sizes.should == [:large, :xl]
  end
end
