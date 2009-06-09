require "zlib"
require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../../../fixtures/class'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Zlib::GzipReader.wrap" do
  before :each do
    @io = ClassSpecs::StubReaderWithClose.new GzipReaderSpecs::GzippedLoremIpsum
  end
  
  it "can be called without a block" do
    Zlib::GzipReader.wrap(@io).should be_kind_of(Zlib::GzipReader)
  end
  
  it "raises NoMethodError if argument does not have a write method" do
    lambda { Zlib::GzipReader.wrap(nil) { }  }.should raise_error(NoMethodError)
    lambda { Zlib::GzipReader.wrap(mock("dummy")) { }  }.should raise_error(NoMethodError)
  end
  
  it "invokes the block with an instance of GzipReader" do
    Zlib::GzipReader.wrap(@io) do |gz|
      gz.should be_kind_of Zlib::GzipReader
    end
  end

  it "returns the instance of GzipReader" do
    ret = Zlib::GzipReader.wrap(@io) do |gz|
      ScratchPad.record gz
    end
    ret.should equal(ScratchPad.recorded)
  end

  it "allows the GzipReader instance to be closed in the block" do
    ScratchPad.clear
    ret = Zlib::GzipReader.wrap(@io) do |gz| 
      gz.close
      ScratchPad.record :after_close
    end
    ScratchPad.recorded.should == :after_close
  end

  it "calls io#close once for a trivial block" do
    @io.should_receive(:close)
    Zlib::GzipReader.wrap(@io) { }
  end

  it "allows IO objects without a close method" do
    io = mock("io")
    io.should_receive(:read).any_number_of_times.and_return(GzipReaderSpecs::GzippedLoremIpsum, nil)
    Zlib::GzipReader.wrap(io) { |gz| gz.read }
  end

  it "propagates Exceptions thrown from the block after calling io#close" do
    @io.should_receive(:close)
    lambda { Zlib::GzipReader.wrap(@io) { raise "error from block" } }.should raise_error(RuntimeError)
  end
end
