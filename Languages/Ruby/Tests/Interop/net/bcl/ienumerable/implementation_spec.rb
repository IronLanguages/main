require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../fixtures/classes'

describe "Implementing IEnumerable from IronRuby" do
  it "uses the enumerator method defined" do
    list = TestList.new
    list << "a"
    Tester.new.test(list).should include "a"
  end

  it "uses the enumerator method defined in Array (via clr_member)" do
    list_a = TestListFromArray.new
    list_a << "b"
    Tester.new.test(list_a).should include "b"
  end

  it "doesn't use Array's get_enumerator" do
    list = TestListMissing.new
    list << "c"
    lambda {Tester.new.test(list)}.should raise_error NoMethodError
  end
end
