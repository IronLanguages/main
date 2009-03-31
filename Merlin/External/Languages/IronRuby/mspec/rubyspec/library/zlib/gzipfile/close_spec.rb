require File.dirname(__FILE__) + '/../../../spec_helper'
require 'stringio'
require 'zlib'

describe 'Zlib::GzipFile#close' do
  before(:each) do
	@io = StringIO.new
  end
  
  it 'closes the io' do
    Zlib::GzipWriter.wrap @io do |gzio|
      gzio.close
      
      gzio.closed?.should == true
    end
  end
  
  it 'calls the close method of the associated IO object' do
    Zlib::GzipWriter.wrap @io do |gzio|
      gzio.close

      @io.closed?.should == true
    end
  end
  
  it 'returns the associated IO object' do
    Zlib::GzipWriter.wrap @io do |gzio|
      gzio.close.should eql(@io)
    end
  end
  
  after do
    @io.close unless @io.closed?
  end
end

