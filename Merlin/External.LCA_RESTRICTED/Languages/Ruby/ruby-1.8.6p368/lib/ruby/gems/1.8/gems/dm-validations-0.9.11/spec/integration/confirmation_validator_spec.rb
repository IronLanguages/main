require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::Validate::ConfirmationValidator do
  before(:all) do
    class ::Canoe
      include DataMapper::Resource

      property :id,                Integer, :serial => true
      property :name,              String
      property :name_confirmation, String
      property :size,              Integer

      validates_is_confirmed :name
    end
  end

  it "should only validate if the attribute is dirty" do
    class ::Transformer
      include DataMapper::Resource

      property :id,                Integer, :serial => true
      property :name,              String
      property :assoc,             String

      validates_is_confirmed :name

      attr_accessor :name_confirmation
    end
    Transformer.auto_migrate!
    # attribute_dirty?
    tf = Transformer.new(:name => "Optimus Prime", :name_confirmation => "Optimus Prime", :assoc => "Autobot")
    tf.should be_valid
    tf.save.should == true
    tf = Transformer.first
    tf.update_attributes(:assoc => "Autobot!").should == true
  end

  it "should validate the confirmation of a value on an instance of a resource" do
    canoe = Canoe.new
    canoe.name = 'White Water'
    canoe.name_confirmation = 'Not confirmed'
    canoe.should_not be_valid
    canoe.errors.on(:name).should include('Name does not match the confirmation')
    canoe.errors.full_messages.first.should == 'Name does not match the confirmation'

    canoe.name_confirmation = 'White Water'
    canoe.should be_valid
  end

  it "should default the name of the confirmation field to <field>_confirmation
      if one is not specified" do
    canoe = Canoe.new
    canoe.name = 'White Water'
    canoe.name_confirmation = 'White Water'
    canoe.should be_valid
  end

  it "should default to allowing nil values on the fields if not specified to" do
    Canoe.new.should be_valid
  end

  it "should not pass validation with a nil value when specified to" do
    class ::Canoe
      validators.clear!
      validates_is_confirmed :name, :allow_nil => false
    end
    canoe = Canoe.new
    canoe.should_not be_valid
    canoe.errors.on(:name).should include('Name does not match the confirmation')
  end

  it "should allow the name of the confirmation field to be set" do
    class ::Canoe
      validators.clear!
      validates_is_confirmed :name, :confirm => :name_check
      def name_check=(value)
        @name_check = value
      end

      def name_check
        @name_confirmation ||= nil
      end
    end
    canoe = Canoe.new
    canoe.name = 'Float'
    canoe.name_check = 'Float'
    canoe.should be_valid
  end

  it "should not require that the confirmation field be a property" do
    class ::Raft
      include DataMapper::Resource
      attr_accessor :name, :name_confirmation

      property :id, Integer, :serial => true

      validates_is_confirmed :name
    end

    raft = Raft.new(:id => 10)
    raft.name = 'Lifeboat'
    raft.should_not be_valid
  end
end
