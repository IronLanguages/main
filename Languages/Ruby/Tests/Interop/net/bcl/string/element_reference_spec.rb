require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../../../../mspec/rubyspec/core/string/fixtures/classes.rb'
require File.dirname(__FILE__) + '/../../../../mspec/rubyspec/core/string/shared/slice.rb'

describe "System::String#[]" do
  before(:each) do
    @hello_there = "hello there".to_clr_string
    @hello = "hello".to_clr_string
    @hello_world = "hello world".to_clr_string
    @empty = "".to_clr_string
    @x = "x".to_clr_string
    @foo = "foo".to_clr_string
    @good = "GOOD".to_clr_string
  end
  it "returns the character at the given index" do
    @hello[0].should == "h"
    @hello[-1].should == "o"
  end

  it "returns nil if index is outside of self" do
    @hello[20].should == nil
    @hello[-20].should == nil

    @empty[0].should == nil
    @empty[-1].should == nil
  end

  it "calls to_int on the given index" do
    @hello[0.5].should == "h"

    obj = mock('1')
    obj.should_receive(:to_int).and_return(1)
    @hello[obj].should == "e"
  end

  it "raises a TypeError if the given index is nil" do
    lambda { @hello[nil] }.should raise_error(TypeError)
  end

  it "raises a TypeError if the given index can't be converted to an Integer" do
    lambda { @hello[mock('x')] }.should raise_error(TypeError)
    lambda { @hello[{}]        }.should raise_error(TypeError)
    lambda { @hello[[]]        }.should raise_error(TypeError)
  end
  
  it "returns a System::String" do
    @hello[1].should be_kind_of System::String
  end
end
describe "System::String#[] with index, length" do
  before(:each) do
    @hello_there = "hello there".to_clr_string
    @hello = "hello".to_clr_string
    @hello_world = "hello world".to_clr_string
    @empty = "".to_clr_string
    @x = "x".to_clr_string
    @foo = "foo".to_clr_string
    @good = "GOOD".to_clr_string
  end
  it_behaves_like :string_slice_index_length, :[]

  it "returns a System::String" do
    @hello[1,3].should be_kind_of System::String
  end
end

describe "System::String#[] with Range" do
  before(:each) do
    @hello_there = "hello there".to_clr_string
    @hello = "hello".to_clr_string
    @hello_world = "hello world".to_clr_string
    @empty = "".to_clr_string
    @x = "x".to_clr_string
    @foo = "foo".to_clr_string
    @good = "GOOD".to_clr_string
  end
  it_behaves_like :string_slice_range, :[]
  
  it "returns a System::String" do
    @hello[1..3].should be_kind_of System::String
  end
end

describe "System::String#[] with Regexp" do
  before(:each) do
    @hello_there = "hello there".to_clr_string
    @hello = "hello".to_clr_string
    @hello_world = "hello world".to_clr_string
    @empty = "".to_clr_string
    @x = "x".to_clr_string
    @foo = "foo".to_clr_string
    @good = "GOOD".to_clr_string
  end
  it_behaves_like :string_slice_regexp, :[]
  
  it "returns a System::String" do
    @hello[/e/].should be_kind_of System::String
  end
end

describe "System::String#[] with Regexp, index" do
  before(:each) do
    @hello_there = "hello there".to_clr_string
    @hello = "hello".to_clr_string
    @hello_world = "hello world".to_clr_string
    @empty = "".to_clr_string
    @x = "x".to_clr_string
    @foo = "foo".to_clr_string
    @good = "GOOD".to_clr_string
  end
  it_behaves_like :string_slice_regexp_index, :[]
  
  it "returns a System::String" do
    @hello[/e/, 2].should be_kind_of System::String
  end
end

describe "System::String#[] with String" do
  before(:each) do
    @hello_there = "hello there".to_clr_string
    @hello = "hello".to_clr_string
    @hello_world = "hello world".to_clr_string
    @empty = "".to_clr_string
    @x = "x".to_clr_string
    @foo = "foo".to_clr_string
    @good = "GOOD".to_clr_string
  end
  it_behaves_like :string_slice_string, :[]
  
  it "returns a System::String" do
    @hello["e"].should be_kind_of System::String
  end
end
describe "System::String#[] with System::String" do
  before(:each) do
    @hello_there = "hello there".to_clr_string
    @hello = "hello".to_clr_string
    @hello_world = "hello world".to_clr_string
    @empty = "".to_clr_string
    @x = "x".to_clr_string
    @foo = "foo".to_clr_string
    @good = "GOOD".to_clr_string
  end
  it "returns other_str if it occurs in self" do
    s = "lo".to_clr_string
    @hello_there.send(@method, s).should == s
  end

  it "taints resulting strings when other is tainted" do
    strs = [@empty, @hello_world, @hello]
    strs += strs.map { |s| s.dup.taint }

    strs.each do |str|
      strs.each do |other|
        r = str.send(@method, other)

        r.tainted?.should == !r.nil? & other.tainted?
      end
    end
  end

  it "doesn't set $~" do
    $~ = nil

    @hello.send(@method, 'll'.to_clr_string)
    $~.should == nil
  end

  it "returns nil if there is no match" do
    @hello_there.send(@method, "bye".to_clr_string).should == nil
  end

  it "doesn't call to_str on its argument" do
    o = mock('x')
    o.should_not_receive(:to_str)

    lambda { @hello.send(@method, o) }.should raise_error(TypeError)
  end

  it "returns a subclass instance when given a subclass instance" do
    s = StringSpecs::MyString.new("el")
    r = @hello.send(@method, s)
    r.should == "el"
    r.class.should == StringSpecs::MyString
  end
end

