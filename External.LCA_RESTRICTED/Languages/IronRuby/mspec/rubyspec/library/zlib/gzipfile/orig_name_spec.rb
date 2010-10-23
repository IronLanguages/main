require File.dirname(__FILE__) + '/../../../spec_helper'
require 'stringio'
require 'zlib'

describe 'Zlib::GzipFile#orig_name' do
  before :each do
    @io = StringIO.new
	@gzip_writer = Zlib::GzipWriter.new @io
  end

  before :all do
    GC.disable
  end

  after :each do
    @gzip_writer.close unless @gzip_writer.closed?
  end

  it 'is nil by default' do
    @gzip_writer.orig_name.should be_nil
  end
  
  it 'returns the name' do
    @gzip_writer.orig_name = 'name'
    @gzip_writer.orig_name.should == 'name'
  end
end

describe 'Zlib::GzipFile#orig_name=' do
  before :each do
    @io = StringIO.new
	@gzip_writer = Zlib::GzipWriter.new @io
  end

  after :each do
    @gzip_writer.close unless @gzip_writer.closed?
  end
  
  it 'returns the argument' do
    n = 'name'
    (@gzip_writer.orig_name = n).should equal(n)
  end
  
  it 'raises TypeError if argument is nil' do
    lambda { @gzip_writer.orig_name = nil }.should raise_error(TypeError)
  end
  
  it 'raises TypeError if argument is not a String' do
    m = mock("name").should_not_receive(:to_s)
    lambda { @gzip_writer.orig_name = m }.should raise_error(TypeError)
  end
  
  it 'raises an error on a closed stream' do
    @gzip_writer.close
    lambda { @gzip_writer.orig_name = 'name' }.should raise_error(Zlib::GzipFile::Error)
  end
end

