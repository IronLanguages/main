require 'zlib'
require File.dirname(__FILE__) + '/../../../spec_helper'

describe "Zlib::Deflate.new" do
  it "Can take compression and Window width" do
    lambda {Zlib::Deflate.new(Zlib::DEFAULT_COMPRESSION, -Zlib::MAX_WBITS)}.should_not raise_error
  end
  it "needs to be reviewed"
end
