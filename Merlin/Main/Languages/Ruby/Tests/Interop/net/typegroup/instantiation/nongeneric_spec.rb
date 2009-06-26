require File.dirname(__FILE__) + '/../../spec_helper'

describe "Mixed TypeGroups (with non-generic member)" do
  it "don't cache the class information" do
    c = EmptyTypeGroup.new
    c2 = EmptyTypeGroup1.new
    c2.should_not be_kind_of EmptyTypeGroup
  end
end
