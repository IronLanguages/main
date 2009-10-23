require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Kernel#nil?" do
  it "returns nil unless self is an instance of NilClass or unless self has overridden nil?" do
    1.nil?.should == false
    Object.new.nil?.should == false
    false.nil?.should == false
    nil.nil?.should == true
    obj = Object.new
    class << obj
      def nil?
        true
      end
    end

    obj.nil?.should == true
  end
  it "needs to be reviewed for spec completeness"
end
