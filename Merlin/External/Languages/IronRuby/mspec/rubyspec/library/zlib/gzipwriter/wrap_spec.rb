require "zlib"
require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../../../fixtures/class'

describe "Zlib::GzipWriter.wrap" do
  before :each do
    @io = ClassSpecs::StubWriterWithClose.new
  end
  
  it "can be called without a block" do
    @io.should_not_receive :write
    @io.should_not_receive :close
    Zlib::GzipWriter.wrap(@io).should be_kind_of(Zlib::GzipWriter)
  end
  
  ruby_bug "Causes segmentation fault", "1.8" do
    it "can be called without a block with a non-writable object" do
      Zlib::GzipWriter.wrap(nil).should be_kind_of(Zlib::GzipWriter)
    end
  end

  it "raises NoMethodError if argument does not have a write method" do
    lambda { Zlib::GzipWriter.wrap(mock("dummy")) { }  }.should raise_error(NoMethodError)
  end
  
  it "invokes the block with an instance of GzipWriter" do
    Zlib::GzipWriter.wrap(@io) do |gz|
      gz.should be_kind_of(Zlib::GzipWriter)
    end
  end

  it "returns the block result" do
    Zlib::GzipWriter.wrap(@io) do |gz|
      :end_of_block
    end.should == :end_of_block
  end

  it "allows the GzipWriter instance to be closed in the block" do
    ScratchPad.clear
    @io.should_receive :close
    Zlib::GzipWriter.wrap(@io) do |gz| 
      gz.close
      ScratchPad.record :after_gzipwriter_close
    end
    ScratchPad.recorded.should == :after_gzipwriter_close
  end

  it "calls io#close once for a trivial block" do
    ScratchPad.record []
    @io.should_receive :close
    Zlib::GzipWriter.wrap(@io) { }
  end

  it "allows IO objects without a close method" do
    io = ClassSpecs::StubWriter.new
    io.should_receive(:write).any_number_of_times
    Zlib::GzipWriter.wrap(io) { |gz| gz << "Hello" }
  end

  it "propagates Exceptions thrown from the block after calling close" do
    @io.should_receive :close
    lambda { Zlib::GzipWriter.wrap(@io) { raise "error from block" } }.should raise_error(RuntimeError)
  end
end
