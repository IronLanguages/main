require File.dirname(__FILE__) + '/../../../spec_helper'

describe "File::Stat#ino" do
  before :each do
    @file = tmp('i_exist')
    touch(@file) { |f| f.write "rubinius" }
  end

  after :each do
    rm_r @file
  end
  
  it "returns an Integer" do
    st = File.stat(@file)
    st.ino.is_a?(Integer).should == true
  end
  
  platform_is_not :windows do
    it "should be able to determine the ino on a File::Stat object" do
      File.stat(@file).ino.should > 0
    end
  end

  platform_is :windows do
    it "should be able to determine the ino on a File::Stat object" do
      File.stat(@file).ino.should == 0
    end
  end
end
