require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe 'File::Stat#rdev' do
  it "returns the number of the device this file represents which the file exists" do
    File.stat(FileStatSpecs.null_device).rdev.should be_kind_of(Integer)
  end
end
