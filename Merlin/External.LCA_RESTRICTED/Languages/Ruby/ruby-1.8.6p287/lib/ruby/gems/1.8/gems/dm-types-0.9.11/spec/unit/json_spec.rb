require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::Json, ".load" do
  it 'should return nil if nil is provided' do
    DataMapper::Types::Json.load(nil, :property).should be_nil
  end

  it 'should parse the value if a string is provided' do
    JSON.should_receive(:load).with('json_string').once
    DataMapper::Types::Json.load('json_string', :property)
  end

  it 'should raise an ArgumentError if something else is given' do
    lambda {
      DataMapper::Types::Json.load(:sym, :property)
    }.should raise_error(ArgumentError, '+value+ must be nil or a String')
  end
end

describe DataMapper::Types::Json, ".dump" do
  it 'should return nil if the value is nil' do
    DataMapper::Types::Json.dump(nil, :property).should be_nil
  end

  it 'should do nothing if the value is a string' do
    JSON.should_not_receive(:dump)
    DataMapper::Types::Json.dump('', :property).should be_kind_of(String)
  end

  it 'should dump to a JSON string otherwise' do
    JSON.should_receive(:dump).with([]).once
    DataMapper::Types::Json.dump([], :property)
  end
end

describe DataMapper::Types::Json, ".typecast" do
  it 'should parse the value if a string is provided' do
    JSON.should_receive(:load).with('json_string')

    DataMapper::Types::Json.typecast('json_string', :property)
  end

  it 'should leave the value alone if an array is given' do
    JSON.should_not_receive(:load)
    DataMapper::Types::Json.typecast([], :property)
  end

  it 'should leave the value alone if a hash is given' do
    JSON.should_not_receive(:load)
    DataMapper::Types::Json.typecast({}, :property)
  end
end
