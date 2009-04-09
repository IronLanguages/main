require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::Enum do
  before(:all) do
    class ::YamlTest
      include DataMapper::Resource

      property :id, Serial
      property :yaml, Yaml
    end
    YamlTest.auto_migrate!

    class ::SerializeMe
      attr_accessor :name
    end
  end

  it "should work" do
    obj = SerializeMe.new
    obj.name = 'Hello!'

    repository(:default) do
      YamlTest.create(:yaml => [1, 2, 3])
      YamlTest.create(:yaml => obj)
    end

    tests = YamlTest.all
    tests.first.yaml.should == [1, 2, 3]
    tests.last.yaml.should be_kind_of(SerializeMe)
    tests.last.yaml.name.should == 'Hello!'
  end

  it 'should immediately typecast supplied values' do
    YamlTest.new(:yaml => [1, 2, 3]).yaml.should == [1, 2, 3]
  end
end
