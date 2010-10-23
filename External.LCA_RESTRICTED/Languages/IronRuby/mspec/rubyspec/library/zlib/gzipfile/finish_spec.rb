require File.dirname(__FILE__) + '/../../../spec_helper'
require 'stringio'
require 'zlib'

describe 'Zlib::GzipFile#finish' do
  before(:each) do
	@io = StringIO.new
	@gzip_writer = Zlib::GzipWriter.new @io
  end

  after :each do
    @gzip_writer.close unless @gzip_writer.closed?
  end
  
  it 'closes the GzipFile' do
    @gzip_writer.finish
    @gzip_writer.closed?.should be_true
  end
  
  it 'does not close the IO object' do
    @gzip_writer.finish
    @io.closed?.should be_false
  end
  
  it 'returns the associated IO object' do
    @gzip_writer.finish.should eql(@io)
  end
  
  it 'raises Zlib::GzipFile::Error if called multiple times' do
    @gzip_writer.finish
    lambda { @gzip_writer.finish }.should raise_error(Zlib::GzipFile::Error)
  end
end
