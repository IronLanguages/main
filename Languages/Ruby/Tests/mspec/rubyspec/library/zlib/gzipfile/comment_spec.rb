require File.dirname(__FILE__) + '/../../../spec_helper'
require 'stringio'
require 'zlib'

describe 'Zlib::GzipFile#comment' do
  before :each do
    @io = StringIO.new
    @gzip_writer = Zlib::GzipWriter.new @io
  end

  after :each do
    @gzip_writer.close
  end

  it 'is nil by default' do
    @gzip_writer.comment.should be_nil
  end
  
  it 'returns the name' do
    @gzip_writer.comment = 'comment'
    @gzip_writer.comment.should == 'comment'
  end
end

describe 'Zlib::GzipFile#comment=' do
  before :each do
    @io = StringIO.new
    @gzip_writer = Zlib::GzipWriter.new @io
  end

  after :each do
    @gzip_writer.close rescue nil
  end
  
  it 'returns the argument' do
    c = 'comment'
    (@gzip_writer.comment = c).should equal(c)
  end
  
  it 'raises TypeError if argument is nil' do
    lambda { @gzip_writer.comment = nil }.should raise_error(TypeError)
  end
  
  it 'raises TypeError if argument is not a String' do
    m = mock("comment").should_not_receive(:to_s)
    lambda { @gzip_writer.comment = m }.should raise_error(TypeError)
  end
  
  it 'raises an error on a closed stream' do
    @gzip_writer.close
    lambda { @gzip_writer.comment = 'comment' }.should raise_error(Zlib::GzipFile::Error)
  end
end

