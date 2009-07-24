require File.dirname(__FILE__) + '/../../spec_helper'

describe "System::String.new" do
  csc <<-EOL
  public partial class Klass {
    public string A(){
      return "a";
    }

    public string Aa(){
      return "aa";
    }
  }
  EOL

  before(:each) do
    @a = Klass.new.a
    @aa = Klass.new.Aa
  end

  it "returns a new System::String from the argument" do
    System::String.new("a").should == Klass.new.a
    System::String.new(System::Char.new("a")).should == Klass.new.a
    System::String.new(System::Char.new("a"), 2).should == Klass.new.Aa
    System::String.new(System::Array.of(System::Char).new(2, System::Char.new("a"))).should == Klass.new.Aa
  end
end

