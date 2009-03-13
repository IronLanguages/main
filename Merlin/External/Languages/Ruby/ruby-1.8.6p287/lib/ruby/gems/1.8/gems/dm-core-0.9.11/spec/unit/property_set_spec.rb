require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

class Icon
      include DataMapper::Resource

      property :id, Serial
      property :name, String
      property :width, Integer, :lazy => true
      property :height, Integer, :lazy => true
end

class Boat
      include DataMapper::Resource
      property :name, String  #not lazy
      property :text, DataMapper::Types::Text    #Lazy by default
      property :notes, String, :lazy => true
      property :a1, String, :lazy => [:ctx_a,:ctx_c]
      property :a2, String, :lazy => [:ctx_a,:ctx_b]
      property :a3, String, :lazy => [:ctx_a]
      property :b1, String, :lazy => [:ctx_b]
      property :b2, String, :lazy => [:ctx_b]
      property :b3, String, :lazy => [:ctx_b]
end

describe DataMapper::PropertySet do
  before :each do
    @properties = Icon.properties(:default).dup
  end

  it "#slice should find properties" do
    @properties.slice(:name, 'width').should have(2).entries
  end

  it "#select should find properties" do
    @properties.select { |property| property.primitive == Integer }.should have(3).entries
  end

  it "#clear should clear out set" do
    @properties.clear
    @properties.key.should == []
    @properties.defaults.should == []
    @properties.length.should == 0
  end

  it "#[] should find properties by name (Symbol or String)" do
    default_properties = [ :id, 'name', :width, 'height' ]
    @properties.each_with_index do |property,i|
      property.should == @properties[default_properties[i]]
    end
  end

  it "should provide defaults" do
    @properties.defaults.should have(2).entries
    @properties.should have(4).entries
  end

  it 'should add a property for lazy loading  to the :default context if a context is not supplied' do
    Boat.properties(:default).lazy_context(:default).length.should == 2 # text & notes
  end

  it 'should return a list of contexts that a given field is in' do
    props = Boat.properties(:default)
    set = props.property_contexts(:a1)
    set.include?(:ctx_a).should == true
    set.include?(:ctx_c).should == true
    set.include?(:ctx_b).should == false
  end

  it 'should return a list of expanded fields that should be loaded with a given field' do
    props =  Boat.properties(:default)
    set = props.lazy_load_context(:a2)
    expect = [:a1,:a2,:a3,:b1,:b2,:b3]
    expect.should == set.sort! {|a,b| a.to_s <=> b.to_s}
  end

  describe 'when dup\'ed' do
    it 'should duplicate the @entries ivar' do
      @properties.dup.entries.should_not equal(@properties.entries)
    end

    it 'should reinitialize @properties_for' do
      # force @properties_for to hold a property
      Icon.properties(:default)[:name].should_not be_nil
      @properties = Icon.properties(:default)

      @properties.instance_variable_get("@property_for").should_not be_empty
      @properties.dup.instance_variable_get("@property_for").should be_empty
    end
  end
end
