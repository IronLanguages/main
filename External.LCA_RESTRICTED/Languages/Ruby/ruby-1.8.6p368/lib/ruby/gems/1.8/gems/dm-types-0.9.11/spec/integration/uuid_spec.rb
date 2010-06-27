require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

begin
  gem 'uuidtools', '~>1.0.7'
  require 'uuidtools'
rescue LoadError
  skip_tests = true
end

describe 'DataMapper::Types::UUID' do
  unless skip_tests
    before(:all) do
      class UUIDTest
        include DataMapper::Resource

        property :id, Serial
        property :uuid, ::DataMapper::Types::UUID
      end

      UUIDTest.auto_migrate!
    end

    it "should be settable as a uuid" do
      u = UUIDTest.create(:uuid => UUID.parse('b0fc632e-d744-4821-afe3-4ea0701859ee'))

      u.uuid.should be_kind_of(UUID)
      u.uuid.to_s.should == 'b0fc632e-d744-4821-afe3-4ea0701859ee'
    end

    it "should be settable as a string" do
      u = UUIDTest.create(:uuid => 'b0fc632e-d744-4821-afe3-4ea0701859ee')

      u.uuid.should be_kind_of(UUID)
      u.uuid.to_s.should == 'b0fc632e-d744-4821-afe3-4ea0701859ee'
    end

    it "should be allowed to be null" do
      u = UUIDTest.create()

      u.uuid.should be_nil
    end
  else
    it 'requires the uuidtools gem to test'
  end
end
