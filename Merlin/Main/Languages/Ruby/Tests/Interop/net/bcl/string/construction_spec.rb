require File.dirname(__FILE__) + '/../../spec_helper'

describe "System::String construction" do
  before :each do
    @a = System::Char::Parse("a")
    @b = System::Char::Parse("b")
    @aa = System::String.new(@a, 2)
    @bbb = System::String.new(@b, 3)
    @str = System::String
  end

  it "can be parsed" do
    @a.should equal_clr_string("a")
  end

  it "can have it's methods used" do
    @str.concat(@aa, @bbb).should equal_clr_string("aabbb")
    @str.compare(@aa, @bbb).should == -1
    @str.compare(@bbb, @aa).should == 1
    @str.compare(@aa, @aa).should == 0
  end
end

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
    @aa = Klass.new.aa
  end

  it "returns a new System::String from the argument" do
    System::String.new("a").should == Klass.new.a
    System::String.new(System::Char.new("a")).should == Klass.new.a
    System::String.new(System::Char.new("a"), 2).should == Klass.new.aa
    System::String.new(System::Array.of(System::Char).new(2, System::Char.new("a"))).should == Klass.new.aa
  end
end

