require File.dirname(__FILE__) + '/../spec_helper'

describe "The -K command line option" do
  it "doesn't cause errors" do
    ruby_exe(fixture(__FILE__, "kcode_error.rb"), :options => '-KS').chomp.should == "Pass"
  end

  it "sets the $KCODE to SJIS with S" do 
    ruby_exe(fixture(__FILE__, "kcode.rb"), :options => '-KS').chomp.should == "SJIS"
  end
  it "sets the $KCODE to SJIS with s" do 
    ruby_exe(fixture(__FILE__, "kcode.rb"), :options => '-Ks').chomp.should == "SJIS"
  end
  
  it "sets the $KCODE to EUC with E" do 
    ruby_exe(fixture(__FILE__, "kcode.rb"), :options => '-KE').chomp.should == "EUC"
  end
  it "sets the $KCODE to EUC with e" do 
    ruby_exe(fixture(__FILE__, "kcode.rb"), :options => '-Ke').chomp.should == "EUC"
  end
  
  it "sets the $KCODE to UTF8 with U" do 
    ruby_exe(fixture(__FILE__, "kcode.rb"), :options => '-KU').chomp.should == "UTF8"
  end
  it "sets the $KCODE to UTF8 with u" do 
    ruby_exe(fixture(__FILE__, "kcode.rb"), :options => '-Ku').chomp.should == "UTF8"
  end
  
  it "sets the $KCODE to NONE with A" do 
    ruby_exe(fixture(__FILE__, "kcode.rb"), :options => '-KA').chomp.should == "NONE"
  end
  it "sets the $KCODE to NONE with N" do 
    ruby_exe(fixture(__FILE__, "kcode.rb"), :options => '-Ka').chomp.should == "NONE"
  end
  it "sets the $KCODE to NONE with a" do 
    ruby_exe(fixture(__FILE__, "kcode.rb"), :options => '-KN').chomp.should == "NONE"
  end
  it "sets the $KCODE to NONE with n" do 
    ruby_exe(fixture(__FILE__, "kcode.rb"), :options => '-Kn').chomp.should == "NONE"
  end
end
