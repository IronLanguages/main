require "zlib"
require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../../../fixtures/class'
require "stringio"

describe "Zlib::GzipWriter#flush" do
  before :each do
    @io = StringIO.new("", "w")
    @gzip_writer = Zlib::GzipWriter.new @io
    @valid_arguments = [nil, Zlib::NO_FLUSH, Zlib::SYNC_FLUSH, Zlib::FULL_FLUSH, Zlib::FINISH]
  end
  
  after :each do 
    begin
      @gzip_writer.close
    rescue Zlib::BufError
      #noop, this will happen on a closed stream that has data waiting
    end
  end
  
  it "calls flush on underlying io object" do
    @io.should_receive(:flush).exactly(@valid_arguments.size)
    @valid_arguments.each do |f|
      @gzip_writer << "Hello"
      @gzip_writer.flush f
    end
  end
    
  it "returns self" do
    @gzip_writer.flush.should equal(@gzip_writer)
  end
    
  platform_is_not :windows do
    it "raises BufError if called multiple times without writing data" do
      @gzip_writer.flush
      lambda { @gzip_writer.flush }.should raise_error(Zlib::BufError)
    end
  end
    
  it "can be called multiple times if data is written" do
    @gzip_writer.flush
    @io.should_receive(:flush)
    @gzip_writer << "Hello"
    @gzip_writer.flush
  end
    
  it "can be called even if underlying io object does not have a flush method" do
    io = ClassSpecs::StubWriterWithClose.new
    gzip_writer = Zlib::GzipWriter.new io
    gzip_writer.flush.should equal(gzip_writer)
    gzip_writer.close
  end
    
  it "Zlib::FINISH closes the writer" do
    @gzip_writer.flush Zlib::FINISH
    lambda { @gzip_writer << "Hello" }.should raise_error(Zlib::StreamError)
    @gzip_writer.closed?.should be_false
  end
  
  it "throws StreamError for invalid parameter value" do
    lambda { @gzip_writer.flush 5 }.should raise_error(Zlib::StreamError)
    lambda { @gzip_writer.flush "1" }.should raise_error(TypeError)
  end
end
