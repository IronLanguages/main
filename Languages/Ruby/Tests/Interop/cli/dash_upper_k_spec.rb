require File.dirname(__FILE__) + '/../spec_helper'

describe "The -K command line option" do
  it "doesn't cause errors" do
    ruby_exe(fixture(__FILE__, "kcode.rb"), :options => '-KS').chomp.should == "KCODE: shift_jis"
  end
end
