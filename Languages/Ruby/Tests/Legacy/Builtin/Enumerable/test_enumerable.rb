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

# all?, any?, collect, detect, 
# each_with_index, entries,
# find, find_all, grep, include?, inject, map, max, member?, min,
# partition, reject, select, sort, sort_by, to_a, zip

# Note about "nil" tests:
# All of the methods on Enumerable should support self == nil.
# If, for some bizarre reason, NilClass is extended to implement
# "each" and mixin "Enumerable", all of the methods should work
# (basically this is testing that the C# methods on Enumerable accept null)

class EnumTest
  include Enumerable
    
  def initialize(*list)
    @list = list.empty? ? [2, 5, 3, 6, 1, 4] : list
  end
    
  def each
    @list.each { |i| yield i }
  end    
end

describe "Enumerable" do
  it "Some builtins should include Enumerable" do
    String.include?(Enumerable).should == true
    Array.include?(Enumerable).should == true
    Hash.include?(Enumerable).should == true
    Range.include?(Enumerable).should == true
    NilClass.include?(Enumerable).should == false
  end
  
  it "user types can mixin Enumerable" do
    # Verify that all enumerable methods work with a "nil" self
    class NilClass
      include Enumerable
      
      def each
        yield nil
      end
    end
    
    class EnumTestNoEach
      include Enumerable
    end
    
    EnumTest.include?(Enumerable).should == true
    EnumTestNoEach.include?(Enumerable).should == true
    NilClass.include?(Enumerable).should == true
  end
  
  it "Enumerable calls each" do
    $each_called = false
    class EnumTest2
      include Enumerable
      def each
        $each_called = true
        yield 123
      end
    end
    
    EnumTest2.new.to_a.should == [123]
    $each_called.should == true
    
    should_raise(NoMethodError) { EnumTestNoEach.new.to_a }    
  end
end

describe "Enumerable#all?" do  
  it "basic functionaltiy" do
    tests = [ [nil], [false], [true], [1,2,nil,true], [1,true,false], [1, 2, 3] ]
    expected = [ false, false, true, false, false, true ]
    
    tests.length.times do |i|
      tests[i].all?.should == expected[i]
      EnumTest.new(*tests[i]).all?.should == expected[i]
    end
    
    nil.all?.should == false
  end
  
  it "with block" do
    r0 = %w{ foo bar bazz }.all? { |w| w.length >= 3 }
    r1 = %w{ foo bar bazz }.all? { |w| w.length >= 4 }
    r2 = EnumTest.new(*%w{ foo bar bazz }).all? { |w| w.length >= 3 }
    r3 = [1, 2, 3, 4].all? { |e| e % 2 == 0 }
    r4 = EnumTest.new(2, 8, 6).all? { |e| e % 2 == 0 }
    r5 = nil.all? {|e| !e}
    
    [r0, r1, r2, r3, r4, r5].should == [true, false, true, false, true, true]
    
  end  
end

describe "Enumerable#any?" do  
  it "basic functionaltiy" do
    tests = [ [nil], [false], [true], [1,2,nil,true], [1,true,false], [1, 2, 3] ]
    expected = [ false, false, true, true, true, true ]
    
    tests.length.times do |i|
      tests[i].any?.should == expected[i]
      EnumTest.new(*tests[i]).any?.should == expected[i]
    end
    
    nil.any?.should == false
  end
  
  it "with block" do
    r0 = %w{ ant bear cat }.any? { |w| w.length >= 4 }
    r1 = %w{ ant bear cat }.any? { |w| w.length >= 5 }
    r2 = EnumTest.new(*%w{ ant bear cat }).any? { |w| w.length >= 4 }
    r3 = [1, 2, 3, 4].any? { |e| e % 2 == 1 }
    r4 = EnumTest.new(2, 8, 6).any? { |e| e % 2 == 1 }
    r5 = nil.any? {|e| !e}
    
    [r0, r1, r2, r3, r4, r5].should == [true, false, true, true, false, true]
  end
end

describe "Enumerable#collect" do
  it "without block" do
    (5..9).collect.should == [5,6,7,8,9]
    "abc\ndef".collect.should == ["abc\n","def"]
    [1,2,3].collect.should == [1,2,3]
    nil.collect.should == [nil]
  end
  
  it "with block" do
    r = (1..4).collect { |i| i*i }
    r.should == [1,4,9,16]
    
    r = EnumTest.new(5,3,2).collect { |i| i %2 }
    r.should == [1,1,0]
    
    r = EnumTest.new(*%w{ ant bear cat }).collect { |w| w.length }
    r.should == [3,4,3]
    
    r = [[:a,:b],[:c,:d],[:e,:f]].collect { |a| a.collect { |i| i.to_s } }
    r.should == [['a','b'],['c','d'],['e','f']]
    
    r = EnumTest.new(123, "567", :abc).collect { |e| e }
    r.should == [123, "567", :abc]
    
    r = EnumTest.new(123, "567", :abc).collect { |e| "foo" }    
    r.should == ["foo","foo","foo"]
    
    r = nil.collect {|e| "bar" }
    r.should == ["bar"]
  end  
end

describe "Enumerable#detect" do
  it "basic functionality" do
    def foo x
      x.detect {|i| i%5 + i%7 == 0 }
    end
    
    foo(1..34).should == nil
    foo(1..100).should == 35
    foo(35..100).should == 35
    foo(50..100).should == 70    
    
    r = nil.detect {|i| i == nil}
    r.should == nil    
  end
  
  it "detect calls its argument if the element is not found" do    
    # TODO: test with Procs, lambda expressions
    
    class TestDetect; def call; "not found"; end; end
    t = TestDetect.new
    
    r = (1...35).detect(t) {|i| i%5 + i%7 == 0 }
    r.should == "not found"

    r = (1...35).detect(nil) {|i| i%5 + i%7 == 0 }
    r.should == nil

    r = (1..35).detect(t) {|i| i%5 + i%7 == 0 }
    r.should == 35
    
    r = EnumTest.new("all", "any", "detect").detect(t) {|w| w.length == 6}
    r.should == "detect"

    r = EnumTest.new("all", "any", "detect").detect(t) {|w| w.length == 5}
    r.should == "not found"
    
    r = nil.detect(t) { |i| i }
    r.should == "not found"    
  end

  it "negative tests" do
    [].detect # no error because yield is not called
    should_raise(LocalJumpError, "no block given") { ["all", "any", "detect"].detect }
    should_raise(LocalJumpError, "no block given") { EnumTest.new("all", "any", "detect").detect(nil) }
    should_raise(LocalJumpError, "no block given") { ["all", "any", "detect"].detect(TestDetect.new) }
  end
end

describe "Enumerable#each_with_index" do
  it "basic functionality" do
    hash = Hash.new
    r = %w(foo bar baz).each_with_index { |i,x| hash[i] = x }
    hash.should == {"foo"=>0, "bar"=>1, "baz"=>2}
    r.should == %w(foo bar baz)
    
    array = []
    r = nil.each_with_index { |i,x| array << i << x }
    r.should == nil
    array.should == [nil, 0]
    
    # test order
    items = []
    r = [:a, :b, :c, :d].each_with_index { |i,x| items << [i,x]}
    items.should == [[:a,0], [:b,1], [:c,2], [:d,3]]
    r.should == [:a, :b, :c, :d]    
  end

  it "each_with_index calls each" do
    array = []
    enum = EnumTest.new("abc", 123, ["test"], :x123)
    r = enum.each_with_index do |i,x|
      if x % 2 == 0
        array << i
      end
    end
    array.should == ["abc", ["test"]]
    r.should == enum
  end

  it "negative tests" do
    should_raise(LocalJumpError, "no block given") { [].each_with_index }
    should_raise(LocalJumpError, "no block given") { [1].each_with_index }
  end
  
end

describe "Enumerable#entries" do
  it "entries returns all items in an array" do
    (7..10).entries.should == [7,8,9,10]
    EnumTest.new("foo", 1, "bar", :baz).entries.should == ["foo", 1, "bar", :baz]    
    {}.entries.should == []
    {1,2,3,4}.entries.should == {1,2,3,4}.to_a
    {1,2,3,4}.entries.should == [[1,2], [3,4]]
    "hello\nworld".entries.should == ["hello\n", "world"]
    nil.entries.should == [nil]
    
    r = [1,2,3].entries {|p| raise "dummy block" } # block is ignored
    r.should == [1,2,3]
  end
end

describe "Enumerable#find" do
  it "find is the same as detect" do
    def foo x
      x.find {|i| i%5 + i%7 == 0 }
    end
    
    foo(1..34).should == nil
    foo(1..100).should == 35
    foo(35..100).should == 35
    foo(50..100).should == 70    

    r = nil.find {|i| i == nil}
    r.should == nil
  end
  
  it "find calls its argument if the element is not found" do
    t = TestDetect.new
    
    r = (1...35).find(t) {|i| i%5 + i%7 == 0 }
    r.should == "not found"

    r = (1...35).find(nil) {|i| i%5 + i%7 == 0 }
    r.should == nil

    r = (1..35).find(t) {|i| i%5 + i%7 == 0 }
    r.should == 35
    
    r = EnumTest.new("all", "any", "detect").find(t) {|w| w.length == 6}
    r.should == "detect"

    r = EnumTest.new("all", "any", "detect").find(t) {|w| w.length == 5}
    r.should == "not found" 
    
    r = nil.find(t) { |i| i }
    r.should == "not found"     
  end
  
  it "negative tests" do
    [].find # no error because yield is not called
    should_raise(LocalJumpError, "no block given") { ["all", "any", "detect"].find }
    should_raise(LocalJumpError, "no block given") { EnumTest.new("all", "any", "detect").find(nil) }
    should_raise(LocalJumpError, "no block given") { ["all", "any", "detect"].find(TestDetect.new) }
  end
  
end

describe "Enumerable#find_all" do
  it "basic functionality" do
    r = (1..10).find_all { |i| i % 3 == 0 }
    r.should == [3, 6, 9]
    
    test = [{1,"a",3,4,0,"b"},{0,5,1,"a"},["b","a"]]
    r = test.find_all { |e| e[1] == "a" }
    r.should == test
    r = test.find_all { |e| e[0] == "b" }
    r.should == [test[0], test[2]]
  
    r = nil.find_all { |e| e == nil }
    r.should == [nil]
    r = nil.find_all { |e| e }
    r.should == []
  
    r = EnumTest.new("a", :b, "c"[0]).find_all { |e| e == :b }
    r.should == [:b]
  
    [].find_all
    should_raise(LocalJumpError, "no block given") { [1].find_all }
  end
end

describe "Enumerable#grep" do
  it "basic functionality" do
    (1..100).grep(42..45).should == [42, 43, 44, 45]
    
    EnumTest.new("abc", "cab", "aaa", "def").grep("aaa").should == ["aaa"]

    should_raise(ArgumentError) { [].grep }
  end
  
  it "grep calls === on the argument" do
    $case_equal_called = 0
    class TestGrep
      def initialize data
        @data = data
      end
      
      def === other
        $case_equal_called += 1
        @data[0] == other[0]
      end
    end
    
    ["abc", "cab", "aaa", "def"].grep(TestGrep.new("abc")).should == ["abc", "aaa"]
    $case_equal_called.should == 4
    
    EnumTest.new("abc", "cab", "aaa", "def").grep(TestGrep.new("abc")).should == ["abc", "aaa"]
    $case_equal_called.should == 8
  end
end

describe "Enumerable#include?" do
  it "basic functionality" do
    a = ["abc", "cab", "aaa", "def"]
    ["abc", "cab", "aaa", "def"].each do |w|
      a.include?(w).should == true
      a.include?(w.dup + "a").should == false
    end

    (1...10).include?(5).should == true
    (1...10).include?(10).should == false

    nil.include?(nil).should == true
    
    should_raise(ArgumentError) { [].include? }
  end

  it "include? calls == on the argument" do
    $equal_called = 0
    class TestInclude
      def initialize data
        @data = data
      end
      
      def == other
        $equal_called += 1
        @data == other
      end
    end
  
    t = [TestInclude.new("foo"), TestInclude.new("bar")]
    t.include?("foo").should == true
    $equal_called.should == 1
    t.include?("bar").should == true
    $equal_called.should == 3
    t.include?("baz").should == false
    $equal_called.should == 5
  end
end

describe "Enumerable#inject" do
  it "basic functionality" do
    r = (1..10).inject { |sum, n| sum + n }
    r.should == 55
    r = (1..5).inject(2) { |prod, n| prod * n}
    r.should == 240
    
    r = (42..46).inject([]) { |a,i| a << i }
    r.should == [42, 43, 44, 45, 46]
    
    r = nil.inject(123) {|a,i| a}
    r.should == 123    
  end
  
  it "inject with no block" do
    [].inject.should == nil
    [].inject(nil).should == nil
    [].inject([]).should == []
    [].inject("foo").should == "foo"
    
    [1].inject.should == 1
    [:bar].inject.should == :bar
    
    should_raise(LocalJumpError, 'no block given') { [1].inject(2) }
    should_raise(LocalJumpError, 'no block given') { [1,2].inject }    
  end  
  
end

describe "Enumerable#map" do
  it "map is the same as collect" do
    (5..9).map.should == [5,6,7,8,9]
    "abc\ndef".map.should == ["abc\n","def"]
    [1,2,3].map.should == [1,2,3]
    nil.map.should == [nil]
    
    r = (1..4).map { |i| i*i }
    r.should == [1,4,9,16]

    r = nil.map {|e| "bar" }
    r.map == ["bar"]
  end  
  
  it "map works on a user type with each" do
    r = EnumTest.new(5,3,2).map { |i| i %2 }
    r.should == [1,1,0]
    
    r = EnumTest.new(*%w{ ant bear cat }).map { |w| w.length }
    r.should == [3,4,3]
    
    r = [[:a,:b],[:c,:d],[:e,:f]].map { |a| a.map { |i| i.to_s } }
    r.should == [['a','b'],['c','d'],['e','f']]
    
    r = EnumTest.new(123, "567", :abc).map { |e| e }
    r.should == [123, "567", :abc]
    
    r = EnumTest.new(123, "567", :abc).map { |e| "foo" }    
    r.should == ["foo","foo","foo"]
  end  
end

describe "Enumerable#max" do
  it "basic functionality" do
    %w(foo bar baz).max.should == "foo"
    (1..100).max.should == 100
    
    # newer entries don't override older ones
    x = "abc"
    [x, "abc", "abc"].max.equal?(x).should == true
    
    nil.max.should == nil
  end
  
  it "block overrides <=>" do
    r = %w(foo bar baz).max { |x,y| x[2] <=> y[2] }
    r.should == "baz"
    
    r = (1..100).max { |x,y| (x % 60) <=> (y % 60) }
    r.should == 59
    
    r = (1..100).max { |x,y| (x % 16) <=> (y % 16) }
    r.should == 15    
  end
end

describe "Enumerable#member?" do
  it "member? is the same as member?" do
    a = ["abc", "cab", "aaa", "def"]
    ["abc", "cab", "aaa", "def"].each do |w|
      a.member?(w).should == true
      a.member?(w.dup + "a").should == false
    end

    (1...10).member?(5).should == true
    (1...10).member?(10).should == false

    nil.member?(nil).should == true
    
    should_raise(ArgumentError) { [].member? }
  end

  it "member? calls == on the argument" do
    $equal_called = 0
    t = [TestInclude.new("foo"), TestInclude.new("bar")]
    t.member?("foo").should == true
    $equal_called.should == 1
    t.member?("bar").should == true
    $equal_called.should == 3
    t.member?("baz").should == false
    $equal_called.should == 5
  end
end

describe "Enumerable#min" do
  it "basic functionality" do
    %w(foo bar baz).min.should == "bar"
    (1..100).min.should == 1
    
    # newer entries don't override older ones
    x = "abc"
    [x, "abc", "abc"].min.equal?(x).should == true
    
    nil.min.should == nil
  end
  
  it "block overrides <=>" do
    r = %w(foo bar baz).min { |x,y| x[2] <=> y[2] }
    r.should == "foo"
    
    r = (1..100).min { |x,y| (x % 60) <=> (y % 60) }
    r.should == 60
    
    r = (42..80).min { |x,y| (x % 42) <=> (y % 42) }
    r.should == 42
  end
end

describe "Enumerable#partition" do
  it "basic functionality" do
    r = (5..10).partition { |i| i%2 == 0 }
    r.should == [[6,8,10], [5,7,9]]
    
    # partition is non-destructive    
    a = [7, 4, 9, 3, 4, 8, 1, 2, 6, 9]
    r = a.partition { |i| i < 5 }
    a.should == [7, 4, 9, 3, 4, 8, 1, 2, 6, 9]
    r.should == [[4, 3, 4, 1, 2], [7, 9, 8, 6, 9]]
    
    r = a.partition { |i| i < 10 }
    r.should == [a, []]
    
    r = a.partition { |i| i >= 10 }
    r.should == [[], a]
    
    r = nil.partition { |i| i }
    r.should == [[], [nil]]
  end
  
  it "use partition to implement quicksort" do
    def qsort a
      if a.length <= 1
        return a
      end
      a = a.dup
      k = a.pop
      part = a.partition { |i| i < k }
      qsort(part[0]).push(k).concat(qsort(part[1]))
    end
    
    qsort([7, 4, 9, 3, 4, 8, 1, 2, 6, 9]).should == [1, 2, 3, 4, 4, 6, 7, 8, 9, 9]
    qsort(["foo", "bar", "baz"]).should == ["bar", "baz", "foo"]
  end
  
  it "negative tests" do
    [].partition.should == [[],[]]
    should_raise(LocalJumpError, 'no block given') { [1].partition }
    should_raise(ArgumentError) { [1].partition(1) }
  end  
end

describe "Enumerable#reject" do
  it "reject is the opposite of find_all/select" do
    r = (1..10).reject { |i| i % 3 != 0 }
    r.should == [3, 6, 9]
    
    test = [{1,"a",3,4,0,"b"},{0,5,1,"a"},["b","a"]]
    r = test.reject { |e| e[1] != "a" }
    r.should == test
    r = test.reject { |e| e[0] != "b" }
    r.should == [test[0], test[2]]
  
    r = nil.reject { |e| e != nil }
    r.should == [nil]
    r = nil.reject { |e| !e }
    r.should == []
  
    r = EnumTest.new("a", :b, "c"[0]).reject { |e| e != :b }
    r.should == [:b]
  
    [].reject
    should_raise(LocalJumpError, "no block given") { [1].reject }
  end
end

describe "Enumerable#select" do
  it "select is the same as find_all" do
    r = (1..10).select { |i| i % 3 == 0 }
    r.should == [3, 6, 9]
    
    test = [{1,"a",3,4,0,"b"},{0,5,1,"a"},["b","a"]]
    r = test.select { |e| e[1] == "a" }
    r.should == test
    r = test.select { |e| e[0] == "b" }
    r.should == [test[0], test[2]]
  
    r = nil.select { |e| e == nil }
    r.should == [nil]
    r = nil.select { |e| e }
    r.should == []
  
    r = EnumTest.new("a", :b, "c"[0]).select { |e| e == :b }
    r.should == [:b]
  
    [].select
    should_raise(LocalJumpError, "no block given") { [1].select }
  end
end

describe "Enumerable#sort" do
  it "basic functionality" do
    unsorted = [7, 4, 9, 3, 4, 8, 1, 2, 6, 9]
    sorted = [1, 2, 3, 4, 4, 6, 7, 8, 9, 9]
    unsorted.sort.should == sorted
    unsorted.should == [7, 4, 9, 3, 4, 8, 1, 2, 6, 9]

    r = unsorted.sort { |x,y| y <=> x }
    r.should == sorted.reverse
    
    ["foo", "bar", "baz"].sort.should == ["bar", "baz", "foo"]

    nil.sort.should == [nil]
  end
  
  it "block is used for comparison" do    
    $words = %w(
      all? any? collect detect each_with_index entries find
      find_all grep include? inject map max member? min
      partition reject select sort sort_by to_a zip)
      
    backup = $words.dup
    $words.reverse.sort.should == $words
    $words.should == backup
    
    # sort by length, then by word order
    # (we want the resulting ordering to be total, because
    # sort isn't stable so the exact order wouldn't be guaranteed)
    r = $words.reverse.sort do |x,y|
      z = x.length <=> y.length
      if z == 0
        z = x <=> y
      end
      z
    end
    
    $words_sorted_by_length = %w(
      map max min zip
      all? any? find grep sort to_a
      detect inject reject select collect entries member? sort_by
      find_all include?
      partition
      each_with_index)
      
    r.should == $words_sorted_by_length
    $words.should == backup
  end  
end

describe "Enumerable#sort_by" do
  it "basic functionality" do
    a = [7, 4, 9, 3, 4, 8, 1, 2, 6, 9]
    r = a.sort_by { |i| -i }
    r.should == a.sort.reverse    
    
    backup = $words.dup
    r = $words.reverse.sort_by { |w| w }
    r.should == $words    
    
    r = $words.reverse.sort_by { |w| [w.length, w] }
    r.should == $words_sorted_by_length    
    $words.should == backup
    
    r = nil.sort_by { |e| e }
    r.should == [nil]
  end  
  
  it "negative tests" do
    [].sort_by.should == []
    should_raise(LocalJumpError, "no block given") { [1].sort_by }
  end  
end

describe "Enumerable#to_a" do
  it "to_a is the same as entries (returns all enumerated items in an array)" do
    (7..10).to_a.should == [7,8,9,10]
    EnumTest.new("foo", 1, "bar", :baz).to_a.should == ["foo", 1, "bar", :baz]    
    {}.to_a.should == []
    {1,2,3,4}.to_a.should == {1,2,3,4}.to_a
    {1,2,3,4}.to_a.should == [[1,2], [3,4]]
    "hello\nworld".to_a.should == ["hello\n", "world"]
    nil.to_a.should == [] # nil defines its own to_a method which takes precedence
    
    r = [1,2,3].to_a {|p| raise "dummy block" } # block is ignored
    r.should == [1,2,3]
  end
end

describe "Enumerable#zip" do
  it "basic functionality" do
    (1..3).zip.should == [[1],[2],[3]]
    (1..3).zip(4..6).should == [[1,4],[2,5],[3,6]]
    (1..3).zip(4..5).should == [[1,4],[2,5],[3,nil]]
    (1..3).zip(4..6, 7..9).should == [[1,4,7],[2,5,8],[3,6,9]]
    (1..3).zip(4..6, 7..9, 10..12).should == [[1,4,7,10],[2,5,8,11],[3,6,9,12]]
    (1..3).zip(4..7, 7..8, 10..10).should == [[1, 4, 7, 10], [2, 5, 8, nil], [3, 6, nil, nil]]
  end
  
  it "zip works with a block" do
    def ziptest *args
      a = []
      r = (1..3).zip(*args) { |i| a << i }
      r.should == nil
      a
    end

    ziptest.should == [[1],[2],[3]]    
    ziptest(4..6).should == [[1,4],[2,5],[3,6]]
    ziptest(4..6, 7..9).should == [[1,4,7],[2,5,8],[3,6,9]]
    ziptest(4..7, 7..8, 10..10).should == [[1, 4, 7, 10], [2, 5, 8, nil], [3, 6, nil, nil]]
    
    a = []
    r = (1..3).zip { |i| a << i }
    a.should == [[1],[2],[3]]
    r.should == nil

    a = []
    r = (1..3).zip(4..6) { |i| a << i }
    a.should == [[1,4],[2,5],[3,6]]
    r.should == nil

    a = []
    r = (1..3).zip(4..6, 7..9) { |i| a << i }
    a.should == [[1,4,7],[2,5,8],[3,6,9]]
    r.should == nil

    a = []
    r = (1..3).zip(4..7, 7..8, 10..10) { |i| a << i }
    a.should == [[1, 4, 7, 10], [2, 5, 8, nil], [3, 6, nil, nil]]
    r.should == nil

  end

  it "zip works with a type implementing each" do
    EnumTest.new(1, 2, 3).zip.should == [[1],[2],[3]]
    EnumTest.new(1, 2, 3).zip(4..6).should == [[1,4],[2,5],[3,6]]
    EnumTest.new(1, 2, 3).zip(4..6, EnumTest.new(7, 8, 9)).should == [[1,4,7],[2,5,8],[3,6,9]]
    EnumTest.new(1, 2, 3).zip(EnumTest.new(4, 5, 6), 7..9, 10..12).should == [[1,4,7,10],[2,5,8,11],[3,6,9,12]]    
    EnumTest.new(1, 2, 3).zip(EnumTest.new(4, 5, 6, 7), 7..8, 10..10).should == [[1, 4, 7, 10], [2, 5, 8, nil], [3, 6, nil, nil]]  
  end
    
  it "zip calls to_a on arguments" do
    $to_a_called = 0
    class TestZip
      def initialize data
        @data = data
      end
      
      def to_a
        $to_a_called += 1
        @data
      end
    end
    
    (1..3).zip(TestZip.new([4,5,6])).should == [[1,4],[2,5],[3,6]]
    $to_a_called.should == 1
    (1..3).zip(4..6, TestZip.new([7,8,9])).should == [[1,4,7],[2,5,8],[3,6,9]]
    $to_a_called.should == 2
    (1..3).zip(TestZip.new([4,5,6]), 7..9, TestZip.new([10,11,12])).should == [[1,4,7,10],[2,5,8,11],[3,6,9,12]]
    $to_a_called.should == 4    
  end
end

finished
