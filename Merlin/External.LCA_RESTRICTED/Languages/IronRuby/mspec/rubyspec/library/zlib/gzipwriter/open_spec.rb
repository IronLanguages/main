require "zlib"
require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../../../fixtures/class'

describe "Zlib::GzipWriter.open" do
  before :each do
    @gz = nil
    @filename = tmp("gzip_target")
  end
  
  after :each do
    @gz.close if @gz
    begin
      rm_r @filename
    rescue Errno::EACCES
      # Errno::EACCES is thrown occasionally for some reason
    end
    FileUtils.rm_rf(@filename) if File.exists?(@filename)
  end
  
  it "writes to the file" do
    Zlib::GzipWriter.open(@filename) { |gz| gz << "Hello" }
    Zlib::GzipReader.open(@filename) { |gz| gz.read.should == "Hello" }
  end
  
  it "returns block result" do
    Zlib::GzipWriter.open(@filename) { |gz| :end_of_block }.should == :end_of_block
  end
  
  it "returns an open GzipWriter without a block" do
    @gz = Zlib::GzipWriter.open(@filename)
    @gz.closed?.should be_false
  end
  
  it "raises Errno::EACCES if the file is not writable" do
    File.open(@filename, "w").close
    File.chmod(0555, @filename)
    lambda { Zlib::GzipWriter.open(@filename) }.should raise_error(Errno::EACCES)
    File.chmod(0666, @filename)
  end

  it "accepts nil for level and strategy" do
    Zlib::GzipWriter.open(@filename, nil, nil) {|gz| gz << "Hello" }.should_not be_nil
  end
  
  it "accepts Fixnums for level and strategy" do
    Zlib::GzipWriter.open(@filename, 0, 0) {|gz| gz << "Hello" }.should_not be_nil
  end
  
  it "passes an instance of GzipWriter to the block" do
    Zlib::GzipWriter.open(@filename) { |gz| gz.should be_kind_of(Zlib::GzipWriter) }
  end
  
  it "raises TypeError if filename is nil" do
    lambda { Zlib::GzipWriter.open(nil) }.should raise_error(TypeError)
  end
  
  it "raises TypeError unless the arguments are exactly a String, Fixnum and Fixnum" do
    filename = mock("filename")
    filename.should_not_receive(:to_s)
    lambda { Zlib::GzipWriter.open(filename) }.should raise_error(TypeError)

    level = mock("level")
    level.should_not_receive(:to_int)
    lambda { Zlib::GzipWriter.open(@filename, level) }.should raise_error(TypeError)
    
    strategy = mock("strategy")
    strategy.should_not_receive(:to_int)
    lambda { Zlib::GzipWriter.open(@filename, 0, strategy) }.should raise_error(TypeError)
  end
  
  it "propagates exceptions throw inside the block" do
    lambda { Zlib::GzipWriter.open(@filename) do |gz| 
      gz << "Hello"
      raise "error from block" 
    end }.should raise_error(RuntimeError)
  end

  it "closes file even if exceptions is thrown inside the block" do
    begin
      Zlib::GzipWriter.open(@filename) do |gz| 
        gz << "Hello"
        raise "error from block" 
      end
    rescue RuntimeError
    end
    Zlib::GzipReader.open(@filename) { |gz| gz.read.should == "Hello" }
  end
end
