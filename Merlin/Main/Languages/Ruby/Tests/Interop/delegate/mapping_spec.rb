require File.dirname(__FILE__) + '/../spec_helper'

describe "Delegates" do
  csc "public delegate void VoidVoidDelegate();"
  it "map to Ruby classes" do
    VoidVoidDelegate.should be_kind_of Class
  end
end
