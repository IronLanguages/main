require File.dirname(__FILE__) + '/../../../spec_helper'
require 'stringio'
require 'zlib'

describe 'Zlib::GzipFile#finish' do
  before(:each) do
	@io = StringIO.new
  end
  
  it 'closes the io' do
    Zlib::GzipWriter.wrap @io do |gzio|
      gzio.finish
      
      gzio.closed?.should == true
    end
  end
  
  it 'never calls the close method of the associated IO object' do
    Zlib::GzipWriter.wrap @io do |gzio|
      gzio.finish

      @io.closed?.should == false
    end
  end
  
  it 'returns the associated IO object' do
    Zlib::GzipWriter.wrap @io do |gzio|
      gzio.finish.should eql(@io)
    end
  end
  
  after do
    @io.close
  end
end
