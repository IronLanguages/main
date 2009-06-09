require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::Json do
  before(:all) do
    class ::JsonTest
      include DataMapper::Resource

      property :id, Serial
      property :json, Json
    end
    JsonTest.auto_migrate!
  end

  it "should work" do
    repository(:default) do
      JsonTest.create(:json => '[1, 2, 3]')
    end

    JsonTest.first.json.should == [1, 2, 3]
  end

  it 'should immediately typecast supplied values' do
    JsonTest.new(:json => '[1, 2, 3]').json.should == [1, 2, 3]
  end
end
