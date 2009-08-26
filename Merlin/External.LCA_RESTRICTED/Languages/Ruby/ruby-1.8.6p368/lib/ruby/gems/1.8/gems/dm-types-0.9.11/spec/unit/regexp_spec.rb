require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::Regexp, ".load" do
  it 'should make a Regexp from the value if a string is provided' do
    Regexp.should_receive(:new).with('regexp_string').once
    DataMapper::Types::Regexp.load('regexp_string', :property)
  end

  it 'should return nil otherwise' do
    DataMapper::Types::Regexp.load(nil, :property).should == nil
  end
end

describe DataMapper::Types::Regexp, ".dump" do
  it 'should dump to a string' do
    DataMapper::Types::Regexp.dump(/\d+/, :property).should == "\\d+"
  end

  it 'should return nil if the value is nil' do
    DataMapper::Types::Regexp.dump(nil, :property).should == nil
  end
end
