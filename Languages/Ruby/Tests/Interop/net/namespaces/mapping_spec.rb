require File.dirname(__FILE__) + '/../spec_helper'

describe "Basic .NET namespaces" do
  it "map to Ruby modules" do
    [NotEmptyNamespace].each do |klass|
      klass.should be_kind_of Module
    end
  end
end
