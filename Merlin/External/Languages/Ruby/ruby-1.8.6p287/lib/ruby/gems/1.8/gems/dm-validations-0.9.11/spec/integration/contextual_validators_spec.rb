require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::Validate::ContextualValidators do

  before :all do
    class ::Kayak
      include DataMapper::Resource
      property :id, Integer, :key => true
      property :salesman, String, :auto_validation => false
      validates_absent :salesman, :when => :sold
    end
  end

  it "should pass validation for a specific context" do
    k = Kayak.new
    k.valid?(:sold).should == true
    k.salesman = 'John Doe'
    k.valid?(:sold).should_not == true
    k.errors.on(:salesman).should include('Salesman must be absent')
  end

  it "should raise an exception if you provide an invalid context to save" do
    lambda { Kayak.new.save(:invalid_context) }.should raise_error
    lambda { Kayak.new.save(false) }.should raise_error
  end
end
