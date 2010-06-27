require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../fixtures/classes"

describe "Generic TypeGroups" do
  it "can instantiate all members" do
    [TypeGroup, TypeGroup[String], TypeGroup1[String], TypeGroup1[String,Fixnum]].each do |klass|
      klass.new.should be_kind_of klass
    end
  end
end
