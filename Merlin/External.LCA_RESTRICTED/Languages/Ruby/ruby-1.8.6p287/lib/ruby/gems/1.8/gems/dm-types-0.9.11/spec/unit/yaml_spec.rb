require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::Yaml, ".load" do
  it 'should return nil if nil is provided' do
    DataMapper::Types::Yaml.load(nil, :property).should be_nil
  end

  it 'should parse the value if a string is provided' do
    YAML.should_receive(:load).with('yaml_string').once
    DataMapper::Types::Yaml.load('yaml_string', :property)
  end

  it 'should raise an ArgumentError if something else is given' do
    lambda {
      DataMapper::Types::Yaml.load(:sym, :property)
    }.should raise_error(ArgumentError, '+value+ must be nil or a String')
  end
end

describe DataMapper::Types::Yaml, ".dump" do
  it 'should return nil if the value is nil' do
    DataMapper::Types::Yaml.dump(nil, :property).should be_nil
  end

  it 'should do nothing if the value is a string which begins with ---' do
    YAML.should_not_receive(:dump)
    DataMapper::Types::Yaml.dump('--- str', :property).should be_kind_of(String)
  end

  it 'should dump to a YAML string if the value is a normal string' do
    YAML.should_receive(:dump).with('string').once
    DataMapper::Types::Yaml.dump('string', :property)
  end

  it 'should dump to a YAML string otherwise' do
    YAML.should_receive(:dump).with([]).once
    DataMapper::Types::Yaml.dump([], :property)
  end
end

describe DataMapper::Types::Yaml, ".typecast" do
  it 'should leave the value alone' do
    @type = DataMapper::Types::Yaml
    @type.typecast([1, 2, 3], :property).should == [1, 2, 3]

    class SerializeMe
      attr_accessor :name
    end

    obj = SerializeMe.new
    obj.name = 'Hello!'

    casted = @type.typecast(obj, :property)
    casted.should be_kind_of(SerializeMe)
    casted.name.should == 'Hello!'
  end
end
