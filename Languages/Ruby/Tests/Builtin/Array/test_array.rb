# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

require "../../Util/simple_test.rb"

# adds init method so that we don't need send to test private initialize
class Array
  def init *a, &p
    initialize *a, &p
  end
end

class MyArray < Array; end

describe "Array#new" do
  it "creates an empty array via language syntactical sugar" do
    a = []
    a.length.should == 0
  end

  it "creates an empty array explicitly" do
    a = Array.new
    a.length.should == 0
  end

  it "creates an array of an explicit size" do
    a = Array.new(5)
    a.should == [nil, nil, nil, nil, nil]
  end

  it "creates calls to_int on the size argument" do
    a = Array.new(5.2)
    a.should == [nil, nil, nil, nil, nil]

    class Bob1
      def to_int
        5
      end
    end
    b = Array.new(Bob1.new)
    b.should == [nil, nil, nil, nil, nil]
  end

  it "creates an instance of a subclassed array type" do
    c = MyArray.new
    c.length.should == 0
  end

  it "creates and initializes an array" do
    a = [1,2,3]
    a.length.should == 3

    b = [1]
    b.length.should == 1
  end

  it "creates and initializes an array using a block" do
    a = Array.new(5) { |x| x + 1 }
    a.should == [1,2,3,4,5]
  end
  
  it "creates and initializes an array using a block, size is to_int convertible" do
    obj = Object.new
    class << obj
      def to_int; 3; end
    end
    a = Array.new(obj) { |x| x + 1 }
    a.should == [1,2,3]
  end
  
  it "returns result of the block break" do
    a = Array.new(5) { |x| if x < 2 then x else break 'foo' end }
    a.should == 'foo'
  end
end

describe "Array#initialize" do
  it "clears the array before adding items" do
    a = [1,2,3]
    r = a.init([3,4,5])
    r.object_id.should == a.object_id
    a.should == [3,4,5]
  end
  
  it "uses nil values when block is nil" do
    a = [1]
    r = a.init(3,&nil)
    r.object_id.should == a.object_id
    a.should == [nil, nil, nil]
  end
  
  it "is not atomic wrt break from block" do
    a = [1,2,3]
    r = a.init(5) { |x| if x < 2 then x else break 'foo' end }
    r.should == 'foo'
    a.should == [0,1]
  end
end

describe "Array#<=>" do
  skip "<=> should call <=> left to right and return first non-0 result" do
    [-1, +1, nil, "foobar"].each do |result|
      lhs = Array.new(3) { Object.new }
      rhs = Array.new(3) { Object.new }
    
      lhs[0].should_receive(:<=>, :with => [rhs[0]], :returning => 0)
      lhs[1].should_receive(:<=>, :with => [rhs[1]], :returning => result)
      lhs[2].should_not_receive(:<=>)

      (lhs <=> rhs).should == result
    end
  end
  
  it "<=> should be 0 if the arrays are equal" do
    ([] <=> []).should == 0
    ([1, 2, 3, 4, 5, 6] <=> [1, 2, 3, 4, 5, 6]).should == 0
    # TODO: once we have numerical comparisons working correctly we can
    # uncomment this test
    #([1, 2, 3, 4, 5, 6] <=> [1, 2, 3, 4, 5.0, 6.0]).should == 0
  end
  
  it "<=> should be -1 if the array is shorter than the other array" do
    ([] <=> [1]).should == -1
    ([1, 1] <=> [1, 1, 1]).should == -1
  end

  it "<=> should be +1 if the array is longer than the other array" do
    ([1] <=> []).should == +1
    ([1, 1, 1] <=> [1, 1]).should == +1
  end

  it "<=> should call to_ary on its argument" do
    class Bob100
      def to_ary
        [1, 2, 3]
      end
    end
    obj = Bob100.new
    ([4, 5] <=> obj).should == ([4, 5] <=> obj.to_ary)
  end
end

describe "Array#assoc" do
  # BUGBUG: this reveals the nested dynamic site bug as well
  skip "(assoc) should return the first contained array the first element of which is obj" do
    s1 = [ "colors", "red", "blue", "green" ] 
    s2 = [ "letters", "a", "b", "c" ] 
    a = [ s1, s2, "foo", [], [4] ] 
    a.assoc("letters").should == %w{letters a b c}
    a.assoc(4).should == [4]
    a.assoc("foo").should == nil
  end

  skip "(assoc) should call == on argument" do
    key = Object.new
    items = Array.new(3) { [Object.new, "foo"] }
    items[0][0].should_receive(:==, :with => [key], :returning => false)
    items[1][0].should_receive(:==, :with => [key], :returning => true)
    items[2][0].should_not_receive(:==, :with => [key])
    items.assoc(key).should == items[1]
  end
end

describe "Array#fetch" do
  it "(fetch) should return the element at index" do
    [[1, 2, 3].fetch(1), [1, 2, 3, 4].fetch(-1)].should == [2, 4]
  end
  
  it "(fetch) should raise if there is no element at index" do
    should_raise(IndexError) { [1, 2, 3].fetch(3) }
    should_raise(IndexError) { [1, 2, 3].fetch(-4) }
  end
  
  it "(fetch) with default should return default if there is no element at index" do
    [1, 2, 3].fetch(5, :not_found).should == :not_found
    [1, 2, 3].fetch(5, nil).should == nil
    [nil].fetch(0, :not_found).should == nil
  end

  it "(fetch) with block should return the value of block if there is no element at index" do
    [1, 2, 3].fetch(9) { |i| i * i }.should == 81
  end

  it "(fetch) default block takes precedence over its default argument" do
    [1, 2, 3].fetch(9, :foo) { |i| i * i }.should == 81
  end

  it "(fetch) should call to_int on its argument" do
    class Bob102
      def to_int
        0
      end
    end
    x = Bob102.new
    [1, 2, 3].fetch(x).should == 1
  end
end

describe "Array#include?" do
  # TODO: same bugs related to heterogeneous calls to RubySites.Equal()
  it "(include?) should return true if object is present, false otherwise" do
    [1, 2, "a", "b"].include?("c").should == false
    [1, 2, "a", "b"].include?("a").should == true
  end

  skip "(include?) calls == on elements from left to right until success" do
    key = "x"
    ary = Array.new(3) { Object.new }
    ary[0].should_receive(:==, :with => [key], :returning => false)
    ary[1].should_receive(:==, :with => [key], :returning => true)
    ary[2].should_not_receive(:==)
    
    ary.include?(key).should == true
  end
end

describe "Array#index" do
  it "(index) returns the index of the first element == object" do
    class Bob103
      def ==(obj)
        3 == obj
      end
    end
    x = Bob103.new

    [2, x, 3, 1, 3, 1].index(3).should == 1
  end

  it "(index) returns 0 if first element == object" do
    [2, 1, 3, 2, 5].index(2).should == 0
  end

  it "(index) returns size-1 if only last element == to object" do
    [2, 1, 3, 1, 5].index(5).should == 4
  end

  it "(index) returns nil if no element == to object" do
    [2, 1, 1, 1, 1].index(3).should == nil
  end
end

describe "Array#indexes and Array#indices are DEPRECATED synonyms for values_at" do
  it "returns a new array containing the elements indexed by its parameters" do
    a = [1,2,3,4,5]
    a.indexes(0,1,2,3,4).should == [1,2,3,4,5]
    a.indexes(0,0,0).should == [1,1,1]
  end

  it "works with negative offset indices" do
    a = [1,2,3,4,5]
    a.indexes(-1,-2,-3,-4,-5).should == [5,4,3,2,1]
    a.indexes(-5,0).should == [1, 1]
  end

  it "returns nil for elements that are beyond the indices of the array" do
    a = [1,2,3,4,5]
    a.indexes(4,5,-5,-6).should == [5,nil,1,nil]
  end

  skip "(indexes) and (indices) with integer indices are DEPRECATED synonyms for values_at" do
    array = [1, 2, 3, 4, 5]
    params = [1, 0, 5, -1, -8, 10]
    array.indexes(*params).should == array.values_at(*params)
    array.indices(*params).should == array.values_at(*params)
  end

  skip '(indexes) and (indices) can be given ranges which are returned as nested arrays (DEPRECATED)' do
    array = [1, 2, 3, 4, 5]
    params = [0..2, 1...3, 4..4]
    array.indexes(*params).should == [[1, 2, 3], [2, 3], [5]]
    array.indices(*params).should == [[1, 2, 3], [2, 3], [5]]
  end
end

describe "Array#rassoc" do
  it "(rassoc) should return the first contained array whose second element is == object" do
    ary = [[1, "a", 0.5], [2, "b"], [3, "b"], [4, "c"], [], [5], [6, "d"]]
    ary.rassoc("a").should == [1, "a", 0.5]
    ary.rassoc("b").should == [2, "b"]
    ary.rassoc("d").should == [6, "d"]
    ary.rassoc("z").should == nil
  end
  
  skip "(rassoc) should call == on argument" do
    key = Object.new
    items = Array.new(3) { ["foo", Object.new, "bar"] }
    items[0][1].should_receive(:==, :with => [key], :returning => false)
    items[1][1].should_receive(:==, :with => [key], :returning => true)
    items[2][1].should_not_receive(:==, :with => [key])
    items.rassoc(key).should == items[1]
  end
end

describe "Array#rindex" do
  skip "(rindex) returns the first index backwards from the end where element == to object" do
    key = 3    
    ary = Array.new(3) { Object.new }
    ary[2].should_receive(:==, :with => [key], :returning => false)
    ary[1].should_receive(:==, :with => [key], :returning => true)
    ary[0].should_not_receive(:==)

    ary.rindex(key).should == 1
  end
  
  it "(rindex) returns size-1 if last element == object" do
    [2, 1, 3, 2, 5].rindex(5).should == 4
  end

  it "(rindex) returns 0 if only first element == object" do
    [2, 1, 3, 1, 5].rindex(2).should == 0
  end

  it "(rindex) returns nil if no element == object" do
    [1, 1, 3, 2, 1, 3].rindex(4).should == nil
  end
end  

describe "Array#empty?" do
  it "tests for an empty array" do
    a = []
    b = [1,2,3]

    a.empty?.should == true
    b.empty?.should_not == true
  end
end

describe "Array#to_a and Array#to_ary" do
  it "to_a returns self" do
    a = [1, 2, 3]
    a.to_a.should == [1, 2, 3]
    a.equal?(a.to_a).should == true 
  end
  
  skip "to_a on array subclasses shouldn't return subclass instance" do
    e = MyArray.new
    e << 1
    e.to_a.class.should == Array
    e.to_a.should == [1]
  end
  
  it "to_ary returns self" do
    a = [1, 2, 3]
    a.equal?(a.to_ary).should == true
    #a = MyArray[1, 2, 3]
    #a.equal?(a.to_ary).should == true
  end
end

describe "Array#nitems" do
  it "counts the number of non-nil items" do
    a = [nil,1,2,nil,4,5,nil,7,8,nil]
    a.length.should == 10
    a.nitems.should == 6

    b = []
    b.nitems.should == 0

    c = [nil]
    c.nitems.should == 0
    c.length.should == 1
  end
end

describe "Array#length and Array#size" do
  it "retrieves the length/size of empty and non-empty arrays" do
    a = []
    a.length.should == 0
    a.size.should == 0

    b = [1,2,3]
    b.length.should == 3
    b.size.should == 3
  end
end

describe "Array#==(other)" do
  it "compares two arrays" do
    [].should == []
    [1,2].should == [1,2]
    [1,2].should_not == [3,4]
    [].should_not == [1]
  end
end

describe "Array#[idx]" do
  it "reads elements in array using integer indices" do
    a = [1,2,3]
    a[0].should == 1
    a[1].should == 2
    a[2].should == 3
    a[-1].should == 3
    a[-2].should == 2
    a[-3].should == 1
    a[-4].should == nil
    a[4].should == nil
  end

  it "reads elements in array using offset, length parameters" do
    a = [1,2,3,4]
    a[0,2].should == [1,2]
  end

  it "uses slice method to read elements in array (just an alias for [])" do
    a = [1,2,3]
    a.slice(0).should == 1
    a.slice(0,2).should == [1,2]
    a.slice(-1).should == 3
    a.slice(-3).should == 1
  end

  it "returns element at index via slice" do
    a = [1, 2, 3, 4]

    a.slice(0).should == 1
    a.slice(1).should == 2
    a.slice(2).should == 3
    a.slice(3).should == 4
    a.slice(4).should == nil
    a.slice(10).should == nil

    a.slice(-1).should == 4
    a.slice(-2).should == 3
    a.slice(-3).should == 2
    a.slice(-4).should == 1
    a.slice(-5).should == nil
    a.slice(-10).should == nil

    a == [1, 2, 3, 4]
  end

  it "returns elements beginning at start when using slice with start, length" do
    a = [1, 2, 3, 4]

    a.slice(0, 0).should == []
    a.slice(0, 1).should == [1]
    a.slice(0, 2).should == [1, 2]
    a.slice(0, 4).should == [1, 2, 3, 4]
    a.slice(0, 6).should == [1, 2, 3, 4]

    a.slice(2, 0).should == []
    a.slice(2, 1).should == [3]
    a.slice(2, 2).should == [3, 4]
    a.slice(2, 4).should == [3, 4]
    a.slice(2, -1).should == nil
    
    a.slice(4, 0).should == []
    a.slice(4, 2).should == []
    a.slice(4, -1).should == nil

    a.slice(5, 0).should == nil
    a.slice(5, 2).should == nil
    a.slice(5, -1).should == nil

    a.slice(6, 0).should == nil
    a.slice(6, 2).should == nil
    a.slice(6, -1).should == nil

    a.slice(-1, 0).should == []
    a.slice(-1, 1).should == [4]
    a.slice(-1, 2).should == [4]
    a.slice(-1, -1).should == nil

    a.slice(-2, 0).should == []
    a.slice(-2, 1).should == [3]
    a.slice(-2, 2).should == [3, 4]
    a.slice(-2, 4).should == [3, 4]
    a.slice(-2, -1).should == nil

    a.slice(-4, 0).should == []
    a.slice(-4, 1).should == [1]
    a.slice(-4, 2).should == [1, 2]
    a.slice(-4, 4).should == [1, 2, 3, 4]
    a.slice(-4, 6).should == [1, 2, 3, 4]
    a.slice(-4, -1).should == nil

    a.slice(-5, 0).should == nil
    a.slice(-5, 1).should == nil
    a.slice(-5, 10).should == nil
    a.slice(-5, -1).should == nil

    a.should == [1, 2, 3, 4]
  end

  it "should return elements from array using slice and ranges" do
    a = [1, 2, 3, 4]

    a.slice(0..-10).should == []
    a.slice(0...-10).should == []
    a.slice(0..0).should == [1]
    a.slice(0...0).should == []
    a.slice(0..1).should == [1, 2]
    a.slice(0...1).should == [1]
    a.slice(0..2).should == [1, 2, 3]
    a.slice(0...2).should == [1, 2]
    a.slice(0..3).should == [1, 2, 3, 4]
    a.slice(0...3).should == [1, 2, 3]
    a.slice(0..4).should == [1, 2, 3, 4]
    a.slice(0...4).should == [1, 2, 3, 4]
    a.slice(0..10).should == [1, 2, 3, 4]
    a.slice(0...10).should == [1, 2, 3, 4]

    a.slice(2..-10).should == []
    a.slice(2...-10).should == []
    a.slice(2..0).should == []
    a.slice(2...0).should == []
    a.slice(2..2).should == [3]
    a.slice(2...2).should == []
    a.slice(2..3).should == [3, 4]
    a.slice(2...3).should == [3]
    a.slice(2..4).should == [3, 4]
    a.slice(2...4).should == [3, 4]

    a.slice(3..0).should == []
    a.slice(3...0).should == []
    a.slice(3..3).should == [4]
    a.slice(3...3).should == []
    a.slice(3..4).should == [4]
    a.slice(3...4).should == [4]

    a.slice(4..0).should == []
    a.slice(4...0).should == []
    a.slice(4..4).should == []
    a.slice(4...4).should == []
    a.slice(4..5).should == []
    a.slice(4...5).should == []

    a.slice(5..0).should == nil
    a.slice(5...0).should == nil
    a.slice(5..5).should == nil
    a.slice(5...5).should == nil
    a.slice(5..6).should == nil
    a.slice(5...6).should == nil

    a.slice(-1..-1).should == [4]
    a.slice(-1...-1).should == []
    a.slice(-1..3).should == [4]
    a.slice(-1...3).should == []
    a.slice(-1..4).should == [4]
    a.slice(-1...4).should == [4]
    a.slice(-1..10).should == [4]
    a.slice(-1...10).should == [4]
    a.slice(-1..0).should == []
    a.slice(-1..-4).should == []
    a.slice(-1...-4).should == []
    a.slice(-1..-6).should == []
    a.slice(-1...-6).should == []

    a.slice(-2..-2).should == [3]
    a.slice(-2...-2).should == []
    a.slice(-2..-1).should == [3, 4]
    a.slice(-2...-1).should == [3]
    a.slice(-2..10).should == [3, 4]
    a.slice(-2...10).should == [3, 4]

    a.slice(-4..-4).should == [1]
    a.slice(-4..-2).should == [1, 2, 3]
    a.slice(-4...-2).should == [1, 2]
    a.slice(-4..-1).should == [1, 2, 3, 4]
    a.slice(-4...-1).should == [1, 2, 3]
    a.slice(-4..3).should == [1, 2, 3, 4]
    a.slice(-4...3).should == [1, 2, 3]
    a.slice(-4..4).should == [1, 2, 3, 4]
    a.slice(-4...4).should == [1, 2, 3, 4]
    a.slice(-4...4).should == [1, 2, 3, 4]
    a.slice(-4..0).should == [1]
    a.slice(-4...0).should == []
    a.slice(-4..1).should == [1, 2]
    a.slice(-4...1).should == [1]

    a.slice(-5..-5).should == nil
    a.slice(-5...-5).should == nil
    a.slice(-5..-4).should == nil
    a.slice(-5..-1).should == nil
    a.slice(-5..10).should == nil

    a.should == [1,2,3,4]
  end

  it "should not expand array when slice called with indices outside of array" do
    a = [1, 2]
    a.slice(4).should == nil
    a.should == [1, 2]
    a.slice(4, 0).should == nil
    a.should == [1, 2]
    a.slice(6, 1).should == nil
    a.should == [1, 2]
    a.slice(8...8).should == nil
    a.should == [1, 2]
    a.slice(10..10).should == nil
    a.should == [1, 2]
  end

  it "(at) should return the element at index" do
    a = [1, 2, 3, 4, 5, 6]
    a.at(0).should  == 1
    a.at(-2).should == 5
    a.at(10).should == nil
  end

  skip "(at) should call to_int on its argument" do
    a = ["a", "b", "c"]
    a.at(0.5).should == "a"
    
    obj = Object.new
    obj.should_receive(:to_int, :returning => 2)
    a.at(obj).should == "c"
  end
  
  it "(first) should return the first element" do
    ['a', 'b', 'c'].first.should == 'a'
    [nil].first.should == nil
  end
  
  it "(first) should return nil if self is empty" do
    [].first.should == nil
  end
  
  it "(first) with count should return the first count elements" do
    [true, false, true, nil, false].first(2).should == [true, false]
  end
  
  it "(first) with count == 0 should return an empty array" do
    [1, 2, 3, 4, 5].first(0).should == []
  end
  
  it "(first) with count == 1 should return an array containing the first element" do
    [1, 2, 3, 4, 5].first(1).should == [1]
  end
  
  it "(first) should raise ArgumentError when count is negative" do
    should_raise(ArgumentError) { [1, 2].first(-1) }
  end
  
  it "(first) should return the entire array when count > length" do
    [1, 2, 3, 4, 5, 9].first(10).should == [1, 2, 3, 4, 5, 9]
  end

  skip "(first) should call to_int on count" do
    obj = Object.new
    def obj.to_int() 2 end
    [1, 2, 3, 4, 5].first(obj).should == [1, 2]
  end
  
  it "(last) should return last element" do
    [1, 1, 1, 1, 2].last.should == 2
  end
  
  it "(last) returns nil if self is empty" do
    [].last.should == nil
  end
  
  it "(last) returns the last count elements" do
    [1, 2, 3, 4, 5, 9].last(3).should == [4, 5, 9]
  end
  
  it "(last) returns an empty array when count == 0" do
    [1, 2, 3, 4, 5].last(0).should == []
  end
  
  it "(last) raises ArgumentError when count is negative" do
    should_raise(ArgumentError) { [1, 2].last(-1) }
  end
  
  it "(last) returns the entire array when count > length" do
    [1, 2, 3, 4, 5, 9].last(10).should == [1, 2, 3, 4, 5, 9]
  end

  it "(values_at) with indices should return an array of elements at the indexes" do
    [1, 2, 3, 4, 5].values_at().should == []
    [1, 2, 3, 4, 5].values_at(1, 0, 5, -1, -8, 10).should == [2, 1, nil, 5, nil, nil]
  end

  skip "values_at should call to_int on its indices" do
    obj = Object.new
    def obj.to_int() 1 end
    [1, 2].values_at(obj, obj, obj).should == [2, 2, 2]
  end
  
  skip "(values_at) with ranges should return an array of elements in the ranges" do
    # MRI (i think this is a bug)
    #[1, 2, 3, 4, 5].values_at(0..2, 1...3, 4..6).should == [1, 2, 3, 2, 3, 5, nil]
    # IronRuby
    [1, 2, 3, 4, 5].values_at(0..2, 1...3, 4..6).should == [1, 2, 3, 2, 3, 5]
    [1, 2, 3, 4, 5].values_at(6..4).should == []
  end

  skip "values_at with ranges should call to_int on arguments of ranges" do
    from = Object.new
    to = Object.new

    # So we can construct a range out of them...
    def from.<=>(o) 0 end
    def to.<=>(o) 0 end

    def from.to_int() 1 end
    def to.to_int() -2 end
      
    ary = [1, 2, 3, 4, 5]
    ary.values_at(from .. to, from ... to, to .. from).should == [2, 3, 4, 2, 3]
  end
  
  skip "values_at on array subclasses shouldn't return subclass instance" do
    MyArray[1, 2, 3].values_at(0, 1..2, 1).class.should == Array
  end
end

describe "generating arrays" do
  skip "& should create an array with elements common to both arrays (intersection)" do
    ([] & []).should == []
    ([1, 2] & []).should == []
    ([] & [1, 2]).should == []
    ([ 1, 1, 3, 5 ] & [ 1, 2, 3 ]).should == [1, 3]
  end
  
  skip "& should create an array with no duplicates" do
    ([] | []).should == []
    ([1, 2] | []).should == [1, 2]
    ([] | [1, 2]).should == [1, 2]
    ([ 1, 1, 3, 5 ] & [ 1, 2, 3 ]).uniq!.should == nil
  end

  skip "& should call to_ary on its argument" do
    obj = Object.new
    def obj.to_ary() [1, 2, 3] end
    ([1, 2] & obj).should == ([1, 2] & obj.to_ary)
  end

  # MRI doesn't actually call eql?() however. So you can't reimplement it.
  skip "& should act as if using eql?" do
    ([5.0, 4.0] & [5, 4]).should == []
    str = "x"
    ([str] & [str.dup]).should == [str]
  end
  
  skip "& with array subclasses shouldn't return subclass instance" do
    (MyArray[1, 2, 3] & []).class.should == Array
    (MyArray[1, 2, 3] & MyArray[1, 2, 3]).class.should == Array
    ([] & MyArray[1, 2, 3]).class.should == Array
  end
  
  it "| should return an array of elements that appear in either array (union) without duplicates" do
    ([1, 2, 3] | [1, 2, 3, 4, 5]).should == [1, 2, 3, 4, 5]
  end

  skip "| should call to_ary on its argument" do
    obj = Object.new
    def obj.to_ary() [1, 2, 3] end
    ([0] | obj).should == ([0] | obj.to_ary)
  end

  # MRI doesn't actually call eql?() however. So you can't reimplement it.
  skip "| should act as if using eql?" do
    ([5.0, 4.0] | [5, 4]).should == [5.0, 4.0, 5, 4]
    str = "x"
    ([str] | [str.dup]).should == [str]
  end
  
  skip "| with array subclasses shouldn't return subclass instance" do
    (MyArray[1, 2, 3] | []).class.should == Array
    (MyArray[1, 2, 3] | MyArray[1, 2, 3]).class.should == Array
    ([] | MyArray[1, 2, 3]).class.should == Array
  end
  
  it "appends elements to an array using << method" do
    a = []
    (a << 1).should == [1]
    (a << 2).should == [1,2]
    (a << 3).should == [1,2,3]
    a.length.should == 3
    a << 4 << 5 << 6
    a.length.should == 6
    a.should == [1,2,3,4,5,6]
  end

  it "<< should push the object onto the end of the array" do
    ([ 1, 2 ] << "c" << "d" << [ 3, 4 ]).should == [1, 2, "c", "d", [3, 4]]
  end
end  

describe "Array#*(count)" do
  it "generates count copies of an array" do
    a = [1,2,3] * 3
    a.length.should == 9
    a.should == [1,2,3,1,2,3,1,2,3]
  end

  it "(*) should concatenate n copies of the array" do
    ([ 1, 2, 3 ] * 0).should == []
    ([ 1, 2, 3 ] * 3).should == [1, 2, 3, 1, 2, 3, 1, 2, 3]
    ([] * 10).should == []
  end

  it "* with a negative int should raise an ArgumentError" do
    should_raise(ArgumentError) { [ 1, 2, 3 ] * -1 }
    should_raise(ArgumentError) { [] * -1 }
  end

  it "* should call to_int on its argument" do
    class Bob301
      def to_int
        2
      end
    end
    obj = Bob301.new
    #def obj.to_int() 2 end
    ([1, 2, 3] * obj).should == [1, 2, 3] * obj.to_int
  end

  skip "* on array subclass should return subclass instance" do
    (MyArray[1, 2, 3] * 0).class.should == MyArray
    (MyArray[1, 2, 3] * 1).class.should == MyArray
    (MyArray[1, 2, 3] * 2).class.should == MyArray
  end
end

describe "Array#*(string)" do
  it "should be equivalent to self.join(str)" do
    ([ 1, 2, 3 ] * ",").should == [1, 2, 3].join(",")
  end

  it "should call to_str on its argument" do
    class Bob300
      def to_str
        "x"
      end
    end
    obj = Bob300.new
    ([ 1, 2, 3 ] * obj).should == "1x2x3"
  end
  
  it "should call to_str on its argument before to_int" do
    class Bob301
      def to_int
        2
      end
      def to_str
        "x"
      end
    end
    obj = Bob301.new
    ([1, 2, 3] * obj).should == [1, 2, 3] * obj.to_str
  end
end

describe "Array#+(array)" do
  it "(+) should concatenate arrays" do
    a = [1,2,3]
    b = [4,5,6]
    c = a + b
    c.length.should == 6
  end

  it "(+) should concatenate two arrays" do
    ([ 1, 2, 3 ] + [ 3, 4, 5 ]).should == [1, 2, 3, 3, 4, 5]
    ([ 1, 2, 3 ] + []).should == [1, 2, 3]
    ([] + [ 1, 2, 3 ]).should == [1, 2, 3]
    ([] + []).should == []
  end

  skip "(+) should call to_ary on its argument" do
    obj = Object.new
    def obj.to_ary() ["x", "y"] end
    ([1, 2, 3] + obj).should == [1, 2, 3] + obj.to_ary
  end
end

describe "Array#compact" do
  it "(compact) should return a copy of array with all nil elements removed" do
    a = [1, nil, 2, nil, 4, nil]
    a.compact.should == [1, 2, 4]
  end

  skip "(compact) on array subclasses should return subclass instance" do
    MyArray[1, 2, 3, nil].compact.class.should == MyArray
  end
end

describe "Array#flatten" do
  it "flatten should return a one-dimensional flattening recursively" do
    [[[1, [2, 3]],[2, 3, [4, [4, [5, 5]], [1, 2, 3]]], [4]], []].flatten.should == [1, 2, 3, 2, 3, 4, 4, 5, 5, 1, 2, 3, 4]
  end

  skip "flatten shouldn't call flatten on elements" do
    obj = Object.new
    def obj.flatten() [1, 2] end
    [obj, obj].flatten.should == [obj, obj]

    obj = [5, 4]
    def obj.flatten() [1, 2] end
    [obj, obj].flatten.should == [5, 4, 5, 4]
  end
  
  it "flatten should complain about recursive arrays" do
    x = []
    x << x
    should_raise(ArgumentError) { x.flatten }
    
    x = []
    y = []
    x << y
    y << x
    should_raise(ArgumentError) { x.flatten }
  end

  skip "flatten on array subclasses should return subclass instance" do
    MyArray[].flatten.class.should == MyArray
    MyArray[1, 2, 3].flatten.class.should == MyArray
    MyArray[1, [2], 3].flatten.class.should == MyArray
  end
end

describe "Array#reverse" do
  it "(reverse) creates a reversed array" do
    a = [1,2,3]
    b = a.reverse
    b.should == [3,2,1]
    a.object_id.should_not == b.object_id
  end
end

describe "Array#sort" do
  skip "(sort) should return a new array from sorting elements using <=> on the pivot" do
    # TODO: entering try with non-empty stack bug
    #[1, 1, 5, -5, 2, -10, 14, 6].sort.should == [-10, -5, 1, 1, 2, 5, 6, 14]
    #%w{z y x a e b d}.sort.should == ['a', 'b', 'd', 'e', 'x', 'y', 'z']
  end

  skip "(sort) raises an ArgumentError if the comparison cannot be completed" do
    # TODO: entering try with non-empty stack bug
    # d = D.new

    # Fails essentially because of d.<=>(e) whereas d.<=>(1) would work
    # should_raise(ArgumentError) { [1, d].sort.should == [1, d] }
  end
  
  skip "sort may take a block which is used to determine the order of objects a and b described as -1, 0 or +1" do
    # TODO: entering try with non-empty stack bug
    #a = [5, 1, 4, 3, 2]
    #a.sort.should == [1, 2, 3, 4, 5]
    #a.sort {|x, y| y <=> x}.should == [5, 4, 3, 2, 1]
  end
  
  skip "sort on array subclasses should return subclass instance" do
    # TODO: entering try with non-empty stack bug
    #ary = MyArray[1, 2, 3]
    #ary.sort.class.should == MyArray
  end
end

describe "Array#uniq" do
  it "uniq should return an array with no duplicates" do
    ["a", "a", "b", "b", "c"].uniq.should == ["a", "b", "c"]
    [1.0, 1].uniq.should == [1.0, 1]
  end
  
  skip "uniq on array subclasses should return subclass instance" do
    MyArray[1, 2, 3].uniq.class.should == MyArray
  end
end

describe "Array#uniq!" do
  it "uniq! modifies the array in place" do
    a = [ "a", "a", "b", "b", "c" ]
    a.uniq!
    a.should == ["a", "b", "c"]
  end
  
  it "uniq! should return self" do
    a = [ "a", "a", "b", "b", "c" ]
    a.equal?(a.uniq!).should == true
  end
  
  it "uniq! should return nil if no changes are made to the array" do
    [ "a", "b", "c" ].uniq!.should == nil
  end
end

describe "converting arrays" do
  it "converts an array to string" do
    [1,2,3].to_s.should == "123"
    ['hello','world'].to_s.should == 'helloworld'
  end

  it "converts an array containing nil elements to a string" do
    #[1,2,nil,4,5].to_s.should == "12nil45"
    [1,2,nil,4,5].inspect.should == "[1, 2, nil, 4, 5]"
  end

  it "converts an array to a string using a separator between elements" do
    ([1,2,3] * ',').should == '1,2,3'
    ([1,2,3] * '--').should == '1--2--3'
    ([1,2,3] * '').should == '123'
  end

  skip "(join) should return a string formed by concatenating each element.to_s separated by separator without trailing separator" do
    obj = Object.new
    def obj.to_s() 'foo' end

    [1, 2, 3, 4, obj].join(' | ').should == '1 | 2 | 3 | 4 | foo'
  end

  it "join's separator defaults to $, (which defaults to empty)" do
    [1, 2, 3].join.should == '123'
    old, $, = $,, '-'
    [1, 2, 3].join.should == '1-2-3'
    $, = old
  end
  
  skip "join should call to_str on its separator argument" do
    obj = Object.new
    def obj.to_str() '::' end    
    [1, 2, 3, 4].join(obj).should == '1::2::3::4'
  end

  it "(to_s) is equivalent to #joining without a separator string" do
    a = [1, 2, 3, 4]
    a.to_s.should == a.join
    $, = '-'
    a.to_s.should == a.join
    $, = ''
  end
end 

describe "modifying arrays" do
  it "modifies elements in array using []=" do
    a = [1,2,3]
    a[0] = 42
    a[-1] = 1000
    a.first.should == 42
    a.last.should == 1000
    a[-3] = 10
    a.first.should == 10
  end

  it "should modify single elements / optionally expand array when []= called with index" do
    a = []
    a[4] = "e"
    a.should == [nil, nil, nil, nil, "e"]
    a[3] = "d"
    a.should == [nil, nil, nil, "d", "e"]
    a[0] = "a"
    a.should == ["a", nil, nil, "d", "e"]
    a[-3] = "C"
    a.should == ["a", nil, "C", "d", "e"]
    a[-1] = "E"
    a.should == ["a", nil, "C", "d", "E"]
    a[-5] = "A"
    a.should == ["A", nil, "C", "d", "E"]
    a[5] = "f"
    a.should == ["A", nil, "C", "d", "E", "f"]
    a[1] = []
    a.should == ["A", [], "C", "d", "E", "f"]
    a[-1] = nil
    a.should == ["A", [], "C", "d", "E", nil]
  end

  it "should raise if []= called with start and negative length" do
    a = [1, 2, 3, 4]
    should_raise(IndexError) { a[-2, -1] = "" }
    should_raise(IndexError) { a[0, -1] = "" }
    should_raise(IndexError) { a[2, -1] = "" }
    should_raise(IndexError) { a[4, -1] = "" }
    should_raise(IndexError) { a[10, -1] = "" }
  end

  # TODO: bust this baby up into multiple it
  it "[]= with start, length should set elements" do
    a = [];   a[0, 0] = nil;            a.should == []
    a = [];   a[2, 0] = nil;            a.should == [nil, nil]
    a = [];   a[0, 2] = nil;            a.should == []
    a = [];   a[2, 2] = nil;            a.should == [nil, nil]
    
    a = [];   a[0, 0] = [];             a.should == []
    a = [];   a[2, 0] = [];             a.should == [nil, nil]
    a = [];   a[0, 2] = [];             a.should == []
    a = [];   a[2, 2] = [];             a.should == [nil, nil]

    a = [];   a[0, 0] = ["a"];          a.should == ["a"]
    a = [];   a[2, 0] = ["a"];          a.should == [nil, nil, "a"]
    a = [];   a[0, 2] = ["a","b"];      a.should == ["a", "b"]
    a = [];   a[2, 2] = ["a","b"];      a.should == [nil, nil, "a", "b"]

    a = [];   a[0, 0] = ["a","b","c"];  a.should == ["a", "b", "c"]
    a = [];   a[2, 0] = ["a","b","c"];  a.should == [nil, nil, "a", "b", "c"]
    a = [];   a[0, 2] = ["a","b","c"];  a.should == ["a", "b", "c"]
    a = [];   a[2, 2] = ["a","b","c"];  a.should == [nil, nil, "a", "b", "c"]

    a = [1, 2, 3, 4]
    a[0, 0] = [];         a.should == [1, 2, 3, 4]
    a[1, 0] = [];         a.should == [1, 2, 3, 4]
    a[-1,0] = [];         a.should == [1, 2, 3, 4]

    a = [1, 2, 3, 4]
    a[0, 0] = [8, 9, 9];  a.should == [8, 9, 9, 1, 2, 3, 4]
    a = [1, 2, 3, 4]
    a[1, 0] = [8, 9, 9];  a.should == [1, 8, 9, 9, 2, 3, 4]
    a = [1, 2, 3, 4]
    a[-1,0] = [8, 9, 9];  a.should == [1, 2, 3, 8, 9, 9, 4]
    a = [1, 2, 3, 4]
    a[4, 0] = [8, 9, 9];  a.should == [1, 2, 3, 4, 8, 9, 9]

    a = [1, 2, 3, 4]
    a[0, 1] = [9];        a.should == [9, 2, 3, 4]
    a[1, 1] = [8];        a.should == [9, 8, 3, 4]
    a[-1,1] = [7];        a.should == [9, 8, 3, 7]
    a[4, 1] = [9];        a.should == [9, 8, 3, 7, 9]

    a = [1, 2, 3, 4]
    a[0, 1] = [8, 9];     a.should == [8, 9, 2, 3, 4]
    a = [1, 2, 3, 4]
    a[1, 1] = [8, 9];     a.should == [1, 8, 9, 3, 4]
    a = [1, 2, 3, 4]
    a[-1,1] = [8, 9];     a.should == [1, 2, 3, 8, 9]
    a = [1, 2, 3, 4]
    a[4, 1] = [8, 9];     a.should == [1, 2, 3, 4, 8, 9]

    a = [1, 2, 3, 4]
    a[0, 2] = [8, 9];     a.should == [8, 9, 3, 4]
    a = [1, 2, 3, 4]
    a[1, 2] = [8, 9];     a.should == [1, 8, 9, 4]
    a = [1, 2, 3, 4]
    a[-2,2] = [8, 9];     a.should == [1, 2, 8, 9]
    a = [1, 2, 3, 4]
    a[-1,2] = [8, 9];     a.should == [1, 2, 3, 8, 9]
    a = [1, 2, 3, 4]
    a[4, 2] = [8, 9];     a.should == [1, 2, 3, 4, 8, 9]

    a = [1, 2, 3, 4]
    a[0, 2] = [7, 8, 9];  a.should == [7, 8, 9, 3, 4]
    a = [1, 2, 3, 4]
    a[1, 2] = [7, 8, 9];  a.should == [1, 7, 8, 9, 4]
    a = [1, 2, 3, 4]
    a[-2,2] = [7, 8, 9];  a.should == [1, 2, 7, 8, 9]
    a = [1, 2, 3, 4]
    a[-1,2] = [7, 8, 9];  a.should == [1, 2, 3, 7, 8, 9]
    a = [1, 2, 3, 4]
    a[4, 2] = [7, 8, 9];  a.should == [1, 2, 3, 4, 7, 8, 9]
  end

  it "should assign multiple array elements with the [start, length]= syntax" do
    a = [1, 2, 3, 4]
    a[0, 2] = [1, 1.25, 1.5, 1.75, 2]
    a.should == [1, 1.25, 1.5, 1.75, 2, 3, 4]
    a[1, 1] = a[3, 1] = []
    a.should == [1, 1.5, 2, 3, 4]
    a[0, 2] = [1]
    a.should == [1, 2, 3, 4]
    a[5, 0] = [4, 3, 2, 1]
    a.should == [1, 2, 3, 4, nil, 4, 3, 2, 1]
    a[-2, 5] = nil
    a.should == [1, 2, 3, 4, nil, 4, 3]
    a[-2, 5] = []
    a.should == [1, 2, 3, 4, nil]
    a[0, 2] = nil
    a.should == [3, 4, nil]
    a[0, 100] = [1, 2, 3]
    a.should == [1, 2, 3]
    a[0, 2] *= 2
    a.should == [1, 2, 1, 2, 3]
  end

  it "[]= with negative index beyond array should raise" do
    a = [1, 2, 3, 4]
    should_raise(IndexError) { a[-5] = "" }
    should_raise(IndexError) { a[-5, -1] = "" }
    should_raise(IndexError) { a[-5, 0] = "" }
    should_raise(IndexError) { a[-5, 1] = "" }
    should_raise(IndexError) { a[-5, 2] = "" }
    should_raise(IndexError) { a[-5, 10] = "" }
    
    should_raise(RangeError) { a[-5..-5] = "" }
    should_raise(RangeError) { a[-5...-5] = "" }
    should_raise(RangeError) { a[-5..-4] = "" }
    should_raise(RangeError) { a[-5...-4] = "" }
    should_raise(RangeError) { a[-5..10] = "" }
    should_raise(RangeError) { a[-5...10] = "" }
    
    # ok
    a[0..-9] = [1]
    a.should == [1, 1, 2, 3, 4]
  end

  it "(compact!) should remove all nil elements" do
    a = ['a', nil, 'b', nil, nil, 'c']
    a.compact!.equal?(a).should == true
    a.should == ["a", "b", "c"]
  end
  
  it "(compact!) should return nil if there are no nil elements to remove" do
    [1, 2, 3].compact!.should == nil
  end

  it "concat should append the elements in the other array" do
    ary = [1, 2, 3]
    ary.concat([9, 10, 11]).equal?(ary).should == true
    ary.should == [1, 2, 3, 9, 10, 11]
    ary.concat([])
    ary.should == [1, 2, 3, 9, 10, 11]
  end
  
  it "concat shouldn't loop endlessly when argument is self" do
    ary = ["x", "y"]
    ary.concat(ary).should == ["x", "y", "x", "y"]
  end  

  it "concat should call to_ary on its argument" do
    class Bob104
      def to_ary
        ["x", "y"]
      end
    end
    obj = Bob104.new
    #def obj.to_ary() ["x", "y"] end
    [4, 5, 6].concat(obj).should == [4, 5, 6, "x", "y"]
  end
  
  skip "(delete) removes elements that are #== to object" do
    class Bob201
      def ==(other)
        3 == other
      end
    end
    x = Bob201.new
    #def x.==(other) 3 == other end

    a = [1, 2, 3, x, 4, 3, 5, x]
    a.delete Object.new
    a.should == [1, 2, 3, x, 4, 3, 5, x]

    a.delete 3
    a.should == [1, 2, 4, 5]
  end

  it "(delete) should return object or nil if no elements match object" do
    [1, 2, 4, 5].delete(1).should == 1
    [1, 2, 4, 5].delete(3).should == nil
    a = %w{a b b b c d}
    a.delete('b').should == 'b'
    a.should == ['a', 'c', 'd']
  end

  # TODO: entering try with non-empty stack bug
  skip '(delete) may be given a block that is executed if no element matches object' do
  #  [].delete('a') {:not_found}.should == :not_found
  end

  it '(delete) may be given a block that is executed if no element matches object' do
    r = [].delete('a') { :not_found }
    r.should == :not_found
  end
end

describe "Array#delete_at" do
  it "(delete_at) should remove the element at the specified index" do
    a = [1, 2, 3, 4]
    a.delete_at(2)
    a.should == [1, 2, 4]
    a.delete_at(-1)
    a.should == [1, 2]
  end

  it "(delete_at) should return the removed element at the specified index" do
    a = [1, 2, 3, 4]
    a.delete_at(2).should == 3
    a.delete_at(-1).should == 4
  end
  
  it "(delete_at) should return nil if the index is out of range" do
    a = [1, 2]
    a.delete_at(3).should == nil
  end

  it "(delete_at) should call to_int on its argument" do
    class Bob200
      def to_int
        -1
      end
    end
    obj = Bob200.new
    [1, 2].delete_at(obj).should == 2
  end
end

describe "Array#flatten!" do
  it "(flatten!) should modify array to produce a one-dimensional flattening recursively" do
    a = [[[1, [2, 3]],[2, 3, [4, [4, [5, 5]], [1, 2, 3]]], [4]], []]
    a.flatten!.equal?(a).should == true
    a.should == [1, 2, 3, 2, 3, 4, 4, 5, 5, 1, 2, 3, 4]
  end
  
  it "(flatten!) should return nil if no modifications took place" do
    a = [1, 2, 3]
    a.flatten!.should == nil
  end

  it "(flatten!) should complain about recursive arrays" do
    x = []
    x << x
    should_raise(ArgumentError) { x.flatten! }
    
    x = []
    y = []
    x << y
    y << x
    should_raise(ArgumentError) { x.flatten! }
  end
end

describe "Array#insert" do
  it "(insert) with non-negative index should insert objects before the element at index" do
    ary = []
    ary.insert(0, 3).equal?(ary).should == true
    ary.should == [3]

    ary.insert(0, 1, 2).equal?(ary).should == true
    ary.should == [1, 2, 3]
    ary.insert(0)
    ary.should == [1, 2, 3]
    
    # Let's just assume insert() always modifies the array from now on.
    ary.insert(1, 'a').should == [1, 'a', 2, 3]
    ary.insert(0, 'b').should == ['b', 1, 'a', 2, 3]
    ary.insert(5, 'c').should == ['b', 1, 'a', 2, 3, 'c']
    ary.insert(7, 'd').should == ['b', 1, 'a', 2, 3, 'c', nil, 'd']
    ary.insert(10, 5, 4).should == ['b', 1, 'a', 2, 3, 'c', nil, 'd', nil, nil, 5, 4]
  end

  it "(insert) with index -1 should append objects to the end" do
    [1, 3, 3].insert(-1, 2, 'x', 0.5).should == [1, 3, 3, 2, 'x', 0.5]
  end

  it "(insert) with negative index should insert objects after the element at index" do
    ary = []
    ary.insert(-1, 3).should == [3]
    ary.insert(-2, 2).should == [2, 3]
    ary.insert(-3, 1).should == [1, 2, 3]
    ary.insert(-2, -3).should == [1, 2, -3, 3]
    ary.insert(-1, []).should == [1, 2, -3, 3, []]
    ary.insert(-2, 'x', 'y').should == [1, 2, -3, 3, 'x', 'y', []]
    ary = [1, 2, 3]
  end
  
  it "(insert) with negative index beyond array should raise" do
    should_raise(IndexError) { [].insert(-2, 1) }
    should_raise(IndexError) { [1].insert(-3, 2) }
  end

  it "(insert) without objects should do nothing" do
    [].insert(0).should == []
    [].insert(-1).should == []
    [].insert(10).should == []
    [].insert(-2).should == []
  end

  skip "(insert) should call to_int on position argument" do
    obj = Object.new
    def obj.to_int() 2 end
    [].insert(obj, 'x').should == [nil, nil, 'x']
  end
end

describe "Array#clear" do
  it "(clear) removes all elements from an array" do
    a = [1,2,3]
    a.clear.equal?(a).should == true
    a.length.should == 0
  end
end

describe "Array#collect!" do
  it "modifies elements in an array using collect!" do
    a = [1,2,3]
    a.collect! { |x| x + 1 }
    a.should == [2,3,4]
  end
end

describe "Array#replace" do
  it "(replace) should replace the elements with elements from other array" do
    a = [1, 2, 3, 4, 5]
    b = ['a', 'b', 'c']
    a.replace(b).equal?(a).should == true
    a.should == b
    a.equal?(b).should == false

    a.replace([4] * 10)
    a.should == [4] * 10
    
    a.replace([])
    a.should == []
  end
  
  # TODO: skip until site caching bug fixed
  skip "(replace) should call to_ary on its argument" do
    class Bob105
      def to_ary
        [1, 2, 3]
      end
    end
    obj = Bob105.new
      
    ary = []
    ary.replace(obj)
    ary.should == [1, 2, 3]
  end
end

describe "Array#replace!" do
  it "(reverse!) will reverse an array in-place" do
    a = [1,2,3]
    b = a.reverse!
    b.should == [3,2,1]
    a.object_id.should == b.object_id
  end
end

describe "Array#sort!" do
  it "(sort!) should sort array in place using <=>" do
    a = [1, 9, 7, 11, -1, -4]
    a.sort!
    a.should == [-4, -1, 1, 7, 9, 11]
  end
  
  skip "sort! should sort array in place using block value" do
    a = [1, 3, 2, 5, 4]
    a.sort! { |x, y| y <=> x }
    a.should == [5, 4, 3, 2, 1]
  end
end

describe "Array#pop" do
  it "(pop) should remove and return the last element of the array" do
    a = ["a", 1, nil, true]
    
    a.pop.should == true
    a.should == ["a", 1, nil]

    a.pop.should == nil
    a.should == ["a", 1]

    a.pop.should == 1
    a.should == ["a"]

    a.pop.should == "a"
    a.should == []
  end
  
  it "(pop) should return nil if there are no more elements" do
    [].pop.should == nil
  end
end

describe "Array#push" do
  it "(push) should append the arguments to the array" do
    a = [ "a", "b", "c" ]
    a.push("d", "e", "f").equal?(a).should == true
    a.push().should == ["a", "b", "c", "d", "e", "f"]
    a.push(5)
    a.should == ["a", "b", "c", "d", "e", "f", 5]
  end

  it "(push) should append elements onto an array" do
    a = [1,2,3]
    a.push 4
    a.should == [1,2,3,4]
    a.push 5,6
    a.should == [1,2,3,4,5,6]
    a.push
    a.should == [1,2,3,4,5,6]
  end
end

describe "Array#shift" do
  it "(shift) should remove and return the first element" do
    a = [5, 1, 1, 5, 4]
    a.shift.should == 5
    a.should == [1, 1, 5, 4]
    a.shift.should == 1
    a.should == [1, 5, 4]
    a.shift.should == 1
    a.should == [5, 4]
    a.shift.should == 5
    a.should == [4]
    a.shift.should == 4
    a.should == []
  end
  
  it "(shift) should return nil when the array is empty" do
    [].shift.should == nil
  end
end

describe "Array#slice!" do
  it "(slice!) with index should remove and return the element at index" do
    a = [1, 2, 3, 4]
    a.slice!(10).should == nil
    a.should == [1, 2, 3, 4]
    a.slice!(-10).should == nil
    a.should == [1, 2, 3, 4]
    a.slice!(2).should == 3
    a.should == [1, 2, 4]
    a.slice!(-1).should == 4
    a.should == [1, 2]
    a.slice!(1).should == 2
    a.should == [1]
    a.slice!(-1).should == 1
    a.should == []
    a.slice!(-1).should == nil
    a.should == []
    a.slice!(0).should == nil
    a.should == []
  end
  
  it "(slice!) with start, length should remove and return length elements beginning at start" do
    a = [1, 2, 3, 4, 5, 6]
    a.slice!(2, 3).should == [3, 4, 5]
    a.should == [1, 2, 6]
    a.slice!(1, 1).should == [2]
    a.should == [1, 6]
    a.slice!(1, 0).should == []
    a.should == [1, 6]
    a.slice!(2, 0).should == []
    a.should == [1, 6]
    a.slice!(0, 4).should == [1, 6]
    a.should == []
    a.slice!(0, 4).should == []
    a.should == []
  end

  skip "(slice!) should call to_int on start and length arguments" do
    obj = Object.new
    def obj.to_int() 2 end
      
    a = [1, 2, 3, 4, 5]
    a.slice!(obj).should == 3
    a.should == [1, 2, 4, 5]
    a.slice!(obj, obj).should == [4, 5]
    a.should == [1, 2]
    a.slice!(0, obj).should == [1, 2]
    a.should == []
  end

  it "(slice!) with range should remove and return elements in range" do
    a = [1, 2, 3, 4, 5, 6, 7, 8]
    a.slice!(1..4).should == [2, 3, 4, 5]
    a.should == [1, 6, 7, 8]
    a.slice!(1...3).should == [6, 7]
    a.should == [1, 8]
    a.slice!(-1..-1).should == [8]
    a.should == [1]
    a.slice!(0...0).should == []
    a.should == [1]
    a.slice!(0..0).should == [1]
    a.should == []
  end
  
  skip "(slice!) with range should call to_int on range arguments" do
    from = Object.new
    to = Object.new
    
    # So we can construct a range out of them...
    def from.<=>(o) 0 end
    def to.<=>(o) 0 end

    def from.to_int() 1 end
    def to.to_int() -2 end
      
    a = [1, 2, 3, 4, 5]
      
    a.slice!(from .. to).should == [2, 3, 4]
    a.should == [1, 5]

    a.slice!(1..0).should == []
    a.should == [1, 5]
  
    should_raise(TypeError) { a.slice!("a" .. "b") }
    should_raise(TypeError) { a.slice!(from .. "b") }
  end
  
  # TODO: MRI behaves inconsistently here. I'm trying to find out what it should
  # do at ruby-core right now. -- flgr
  # See http://groups.google.com/group/ruby-core-google/t/af70e3d0e9b82f39
  skip "(slice!) with indices outside of array should (not?) expand array" do
    # This is the way MRI behaves -- subject to change
    a = [1, 2]
    a.slice!(4).should == nil
    a.should == [1, 2]
    a.slice!(4, 0).should == nil
    a.should == [1, 2, nil, nil]
    a.slice!(6, 1).should == nil
    a.should == [1, 2, nil, nil, nil, nil]
    a.slice!(8...8).should == nil
    a.should == [1, 2, nil, nil, nil, nil, nil, nil]
    a.slice!(10..10).should == nil
    a.should == [1, 2, nil, nil, nil, nil, nil, nil, nil, nil]
  end
end

describe "Array#unshift" do
  it "should prepend object to the original array" do
    a = [1, 2, 3]
    a.unshift("a").equal?(a).should == true
    a.should == ['a', 1, 2, 3]
    a.unshift().equal?(a).should == true
    a.should == ['a', 1, 2, 3]
    a.unshift(5, 4, 3)
    a.should == [5, 4, 3, 'a', 1, 2, 3]
  end
end

it "Array#each" do
  it "reads elements using each" do
    a = [1,2,3]
    b = []
    a.each { |x| b << x }
    b.should == [1,2,3]
  end

  it "reads no elements in an empty array" do
    a, b = [], []
    a.each { |x| b << x }
    b.should == []
  end

  it "each should yield each element to the block" do
    a = []
    x = [1, 2, 3]
    # BUG: This is the guilty line - chaining against the result of the call to
    # each() causes the generation of an invalid program - assigning to a temp
    # works fine.
    #x.each { |item| a << item }.equal?(x).should == true
    r = x.each { |item| a << item }
    # BUG: this line generates a system error as well ...
    #r.equal?(x).should == true
    a.should == [1, 2, 3]
  end
end

it "Array#each_index" do
  it "reads elements by index using each_index" do
    a = [1,2,3]
    b = []
    a.each_index { |index| b << index }
    b.should == [0,1,2]
  end

  it "reads no elements by index using each_index over an empty array" do
    a, b = [], []
    c = a.each_index { |x| b << x }
    b.should == []
  end
  
  it "each_index should pass the index of each element to the block" do
    a = []
    x = ['a', 'b', 'c', 'd']
    #x.each_index { |i| a << i }.equal?(x).should == true
    r = x.each_index { |i| a << i }
    r.equal?(x).should == true
    a.should == [0, 1, 2, 3]
  end
end

finished
