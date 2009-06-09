require 'zlib'
require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe 'Zlib::Deflate#deflate' do

  before :each do
    @deflator = Zlib::Deflate.new
  end

  it 'deflates some data' do
    data = "\000" * 10

    zipped = @deflator.deflate data, Zlib::FINISH
    @deflator.finish

    zipped.should == "x\234c`\200\001\000\000\n\000\001"
  end

  it 'deflates lots of data' do
    data = "\000" * 32 * 1024

    zipped = @deflator.deflate data, Zlib::FINISH
    @deflator.finish

    zipped.should == "x\234\355\301\001\001\000\000\000\200\220\376\257\356\b\n#{"\000" * 31}\030\200\000\000\001"
  end

  # This example is useful for implementations which do generate valid compressed output, but which
  # do not compress to the exact same byte sequence as MRI
  it 'deflates data to an inflatable format' do
    zipped = @deflator.deflate DeflateSpecs::LoremIpsum, Zlib::FINISH
    Zlib::Inflate.inflate(zipped).should == DeflateSpecs::LoremIpsum
  end

end

describe 'Zlib::Deflate::deflate' do

  it 'deflates some data' do
    data = "\000" * 10

    zipped = Zlib::Deflate.deflate data

    zipped.should == "x\234c`\200\001\000\000\n\000\001"
  end

  it 'deflates lots of data' do
    data = "\000" * 32 * 1024

    zipped = Zlib::Deflate.deflate data

    zipped.should == "x\234\355\301\001\001\000\000\000\200\220\376\257\356\b\n#{"\000" * 31}\030\200\000\000\001"
  end
  
  # This example is useful for implementations which do generate valid compressed output, but which
  # do not compress to the exact same byte sequence as MRI
  it 'deflates data to an inflatable format' do
    zipped = Zlib::Deflate.deflate DeflateSpecs::LoremIpsum
    Zlib::Inflate.inflate(zipped).should == DeflateSpecs::LoremIpsum
  end

end

