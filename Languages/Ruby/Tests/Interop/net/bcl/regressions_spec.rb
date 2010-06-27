require File.dirname(__FILE__) + "/../spec_helper"
require File.dirname(__FILE__) + "/fixtures/classes"

#These are specs that i'm not too sure where to place
describe "Regression test for" do
  it "CP# 2949" do
    module RegressionSpecs
      class D < C
      include I1
      end

      class E < D 
      include I2
      end
    end
    lambda {RegressionSpecs::E.new}.should_not raise_error
  end

  it "CP# 2686" do
    ruby_exe("a=System::Array[System::String].new(5);a[0];puts 'ok'").chomp.should == "ok"
  end
end
