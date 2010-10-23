require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe 'File::Stat#dev' do
  it "returns the number of the device on which the file exists" do
    File.stat(FileStatSpecs.null_device).dev.should be_kind_of(Integer)
  end
end
