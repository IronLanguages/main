require File.dirname(__FILE__) + '/../spec_helper'

# specs for __FILE__

describe "The __FILE__ constant" do
  it "equals the current filename" do
    File.basename(__FILE__).should == "file_spec.rb"
  end

  it "equals (eval) inside an eval" do
    eval("__FILE__").should == "(eval)"
  end

  platform_is :windows do
    it 'is not canonicalized via require or load' do
      # Does not use the "fixtures" method since the test
      # depends on the type of path separator used.
      sub_prgm = ruby_exe(File.dirname(__FILE__) + '\\../fixtures/file.rb').split[2]
      
      # ensures that the \\ separator is still there
      sub_prgm.split(File::SEPARATOR)[-2].should == 'fixtures\\file'
    end
  end
end
