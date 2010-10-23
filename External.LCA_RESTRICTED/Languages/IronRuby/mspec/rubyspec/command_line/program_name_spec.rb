require File.dirname(__FILE__) + '/../spec_helper'

describe 'The Program Name' do
  platform_is :windows do
    before :each do
      # does not use "fixture" method since tests
      # depend on path separators
      fixture_file = File.dirname(__FILE__) + '\\../fixtures/file.rb'

      @output = ruby_exe(fixture_file).split
      @base_file = @output[0] # file.rb's __FILE__
      @base_prgm = @output[1] # file.rb's $0
    end

    it 'should be set to $0 as a canonoicalized path' do
      @base_prgm.split(File::SEPARATOR).each{ |p| p.include?('\\').should == false }
    end

    it 'should be equivaliant to __FILE__' do
      @base_prgm.should == @base_file
    end
  end
end
