require File.dirname(__FILE__) + '/../spec_helper'

describe "Delegates" do
  it "map to Ruby classes" do
    DelegateHolder::VoidVoidDelegate.should be_kind_of Class
  end
end
