require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::FilePath do
  before(:all) do
    class ::FilePathTest
      include DataMapper::Resource

      property :id, Integer, :serial => true
      property :path, FilePath
    end
    FilePathTest.auto_migrate!
  end

  it "should work" do
    repository(:default) do
      FilePathTest.create(:path => '/usr')
    end

    FilePathTest.first.path.should == Pathname.new('/usr')
  end

  it 'should immediately typecast supplied values' do
    FilePathTest.new(:path => '/usr').path.should == Pathname.new('/usr')
  end
end
