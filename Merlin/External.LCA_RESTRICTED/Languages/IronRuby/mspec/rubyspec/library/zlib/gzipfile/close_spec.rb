require File.dirname(__FILE__) + '/../../../spec_helper'
require 'stringio'
require 'zlib'

describe 'Zlib::GzipFile#close' do
  before(:each) do
	@io = StringIO.new
	@gzip_writer = Zlib::GzipWriter.new @io
  end
  
  it 'closes the GzipFile' do
    @gzip_writer.close      
    @gzip_writer.closed?.should be_true
  end
  
  it 'closes the IO object' do
    @gzip_writer.close
    @io.closed?.should be_true
  end
  
  it 'returns the associated IO object' do
    @gzip_writer.close.should eql(@io)
  end
  
  it 'raises Zlib::GzipFile::Error if called multiple times' do
    @gzip_writer.close
    lambda { @gzip_writer.close }.should raise_error(Zlib::GzipFile::Error)
  end

  it 'raises Zlib::GzipFile::Error if called after Zlib#finish' do
    @gzip_writer.finish
    lambda { @gzip_writer.close }.should raise_error(Zlib::GzipFile::Error)
  end
end

