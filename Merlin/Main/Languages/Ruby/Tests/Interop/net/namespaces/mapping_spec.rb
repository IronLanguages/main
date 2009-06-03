require File.dirname(__FILE__) + '/../spec_helper'

describe "Basic .NET namespaces" do
  csc <<-EOL
  namespace NotEmptyNamespace {
    public class Foo {
      public static int Bar() { return 1; }
    }
  }
  EOL
  it "map to Ruby modules" do
    [NotEmptyNamespace].each do |klass|
      klass.should be_kind_of Module
    end
  end
end
