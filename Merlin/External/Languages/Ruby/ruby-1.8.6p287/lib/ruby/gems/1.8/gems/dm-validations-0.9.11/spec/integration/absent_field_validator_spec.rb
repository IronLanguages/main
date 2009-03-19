require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::Validate::AbsentFieldValidator do
  before(:all) do
    class ::Kayak
      include DataMapper::Resource
      property :id, Integer, :key => true
      property :salesman, String, :auto_validation => false
      validates_absent :salesman, :when => :sold
    end

    class ::Pirogue
      include DataMapper::Resource
      property :id, Integer, :key => true
      property :salesman, String, :default => 'Layfayette'
      validates_absent :salesman, :when => :sold
    end
  end

  it "should validate the absence of a value on an instance of a resource" do
    kayak = Kayak.new
    kayak.valid_for_sold?.should == true

    kayak.salesman = 'Joe'
    kayak.valid_for_sold?.should_not == true
    kayak.errors.on(:salesman).should include('Salesman must be absent')
  end

  it "should validate the absence of a value and ensure defaults" do
    pirogue = Pirogue.new
    pirogue.should_not be_valid_for_sold
    pirogue.errors.on(:salesman).should include('Salesman must be absent')
  end

end
