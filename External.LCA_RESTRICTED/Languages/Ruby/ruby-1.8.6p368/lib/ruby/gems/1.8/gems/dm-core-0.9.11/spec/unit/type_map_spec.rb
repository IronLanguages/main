require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::TypeMap do

  before(:each) do
    @tm = DataMapper::TypeMap.new
  end

  describe "#map" do
    it "should return a type map chain" do
      @tm.map(String).should be_instance_of(DataMapper::TypeMap::TypeChain)
    end

    it "should return the original chain if the type has already been mapped" do
      tc = @tm.map(String)
      @tm.map(String).should == tc
    end
  end

  describe "#lookup" do
    it "should the primitive's mapping the class has a primitive type" do
      @tm.map(String).to(:varchar)

      lambda { @tm.lookup(DM::Text) }.should_not raise_error
    end

    it "should merge in the parent type map's translated match" do

    end

    describe "#lookup_from_map" do
      it "should merge the translated type match into the parent match" do
        @tm.map(String).to(:varchar)

        child = DataMapper::TypeMap.new(@tm)
        child.map(String).with(:size => 100)

        child.lookup_from_map(String).should == {:primitive => :varchar, :size => 100}
      end
    end

    describe "#lookup_by_type" do
      it "should raise an exception if the type is not mapped and does not have a primitive" do
        klass = Class.new
        lambda { @tm.lookup(klass) }.should raise_error("Type #{klass} must have a default primitive or type map entry")
      end
    end
  end

  describe "#map" do
    it "should create a new TypeChain if there is no match" do
      @tm.chains.should_not have_key(String)

      DataMapper::TypeMap::TypeChain.should_receive(:new)

      @tm.map(String)
    end

    it "should not create a new TypeChain if there is a match" do
      @tm.map(String)

      DataMapper::TypeMap::TypeChain.should_not_receive(:new)

      @tm.map(String)
    end
  end

  describe DataMapper::TypeMap::TypeChain do
    describe "#to" do
      it "should be a setter for @primitive" do
        tc = DataMapper::TypeMap::TypeChain.new

        lambda { tc.to(:primitive) }.should change { tc.primitive }.to(:primitive)
      end

      it "should return itself" do
        tc = DataMapper::TypeMap::TypeChain.new

        tc.to(:primitive).should == tc
      end
    end

    describe "#with" do
      it "should return itself" do
        tc = DataMapper::TypeMap::TypeChain.new

        tc.with(:key => :value).should == tc
      end

      it "should raise an error if the argument is not a hash" do
        tc = DataMapper::TypeMap::TypeChain.new

        lambda { tc.with(:key) }.should raise_error("method 'with' expects a hash")
      end

      it "should merge the argument hash into the attributes hash" do
        tc = DataMapper::TypeMap::TypeChain.new

        tc.with(:key => :value).with(:size => 10).attributes.should == {:key => :value, :size => 10}
      end
    end

    describe "#translate" do
      it "should merge the attributes hash with the primitive value" do
        DataMapper::TypeMap::TypeChain.new.to(:int).with(:size => 10).translate.should == {:primitive => :int, :size => 10}
      end

      it "should overwrite any :primitive entry set using the 'with' method" do
        DataMapper::TypeMap::TypeChain.new.to(:int).with(:primitive => :varchar).translate.should == {:primitive => :int}
      end
    end
  end
end
