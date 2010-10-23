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

# ==, [], []=, clear, default, default=, default_proc, delete,
# delete_if, each, each_key, each_pair, each_value, empty?, fetch,
# has_key?, has_value?, include?, index, indexes, indices,
# initialize_copy, inspect, invert, key?, keys, length, member?,
# merge, merge!, rehash, reject, reject!, replace, select, shift,
# size, sort, store, to_a, to_hash, to_s, update, value?, values,
# values_at

class MyHash < Hash; end

class ToHashHash < Hash
  def to_hash() { "to_hash" => "was", "called!" => "duh." } end
end

# TODO: remove globals when we get instance variable support
class ToHash
  def initialize(hash)  $tohash_hash = hash end
  def to_hash() $tohash_hash end
end

class EqlTest
  def initialize (result)
    $eqltest_called_eql = false
    $eqltest_called_eq = false
    $eqltest_result = result
  end
  
  def called_eql() $eqltest_called_eql end  
  def called_eq() $eqltest_called_eq end
  
  def == (x)
    $eqltest_called_eq = true
    $eqltest_result
  end
  
  def eql?(x)
    $eqltest_called_eql = true
    $eqltest_result
  end
end

class IntHash0 < EqlTest
  def initialize (result)
    $eqltest_called_eql = false
    $eqltest_called_eq = false
    $eqltest_result = result
    $inthash0_called_hash == false
  end
  
  def called_hash() $inthash0_called_hash end
  def called_hash=(x) $inthash0_called_hash = x end
  
  def hash
    $inthash0_called_hash = true
    0
  end
end

class IntHash1 < EqlTest
  def initialize (result)
    $eqltest_called_eql = false
    $eqltest_called_eq = false
    $eqltest_result = result
    $inthash0_called_hash == false
  end
  
  def called_hash() $inthash1_called_hash end
  def called_hash=(x) $inthash1_called_hash = x end
  
  def hash
    $inthash1_called_hash = true
    1
  end
end

class IntHashX < EqlTest
  def initialize (val, result)
    $eqltest_called_eql = false
    $eqltest_called_eq = false
    $eqltest_result = result
    $inthashX_called_hash == false
    $inthashX_hash = val
  end
  
  def called_hash() $inthashX_called_hash end
  def called_hash=(x) $inthashX_called_hash = x end
  def newhash=(x) $inthashX_hash = x end
  
  def hash
    $inthashX_called_hash = true
    $inthashX_hash
  end
end

class NoHash
  def hash() raise("hash shouldn't be called here"); end
end

class ToHashUsingMissing
  def initialize(hash)  $tohashmm_hash = hash end
  def respond_to? (m)  m == :to_hash end
  def method_missing (m)
    if m == :to_hash
      $tohashmm_hash
    end
  end
end

context "Hash" do
  specify "includes Enumerable" do
    Hash.include?(Enumerable).should == true
  end
end

context "Hash#[] (class method)" do
  specify "[] creates a Hash; values can be provided as the argument list" do
    Hash[:a, 1, :b, 2].should == {:a => 1, :b => 2}
    Hash[].should == {}
  end

  specify "[] creates a Hash; values can be provided as one single hash" do
    Hash[:a => 1, :b => 2].should == {:a => 1, :b => 2} 
    Hash[{1 => 2, 3 => 4}].should == {1 => 2, 3 => 4}
    Hash[{}].should == {}
  end

  specify "[] raises on odd parameter list count" do
    should_raise(ArgumentError) { Hash[1, 2, 3] }
  end
 
  specify "[] raises when mixing argument styles" do
    should_raise(ArgumentError) { Hash[1, 2, {3 => 4}] }
    Hash[1, 2, 3, {3 => 4}].should == {1 => 2, 3 => {3 => 4}}
  end
  
  specify "[] shouldn't call to_hash" do
    should_raise(ArgumentError) { Hash[ToHash.new({1 => 2})] }
  end

  specify "[] should always return an instance of the class it's called on" do
    Hash[MyHash[1, 2]].class.should == Hash
    MyHash[Hash[1, 2]].class.should == MyHash
    MyHash[].class.should == MyHash
  end
end

context "Hash#new" do
  specify "new with default argument creates a new Hash with default object" do
    Hash.new(5).default.should == 5
    Hash.new({}).default.should == {}
  end

  specify "new with default argument shouldn't create a copy of the default" do
    str = "foo"
    Hash.new(str).default.equal?(str).should == true
  end
  
  specify "new with block creates a Hash with a default_proc" do
    Hash.new.default_proc.should == nil
    
    hash = Hash.new { |x| "Answer to #{x}" }
    hash[5].should == "Answer to 5"
    hash["x"].should == "Answer to x"
    hash.default_proc.call(5).should == "Answer to 5"
    hash.default_proc.call("x").should == "Answer to x"
  end
  
  specify "new with Proc sets the proc as the default value" do
    p = Proc.new { |x| "Answer to #{x}" }
    hash = Hash.new p
    hash.default_proc.should == nil
    hash.default.class.should == Proc
    hash.default.equal?(p).should == true
    hash[5].should == p
    hash["x"].should == p
    hash.default.call(5).should == "Answer to 5"
    hash.default.call("x").should == "Answer to x"
  end
  
  specify "new with default argument and default block should raise" do
    should_raise(ArgumentError) { Hash.new(5) { 0 } }
    should_raise(ArgumentError) { Hash.new(nil) { 0 } }
  end
end

context "Hash#==" do
  specify "== is true if they have the same number of keys and each key-value pair matches" do
    Hash.new(5).should == Hash.new(1)
    h = Hash.new {|h, k| 1}
    h2 = Hash.new {}
    h.should == h2
    h = Hash.new {|h, k| 1}
    h.should == Hash.new(2)
    
    a = {:a => 5}
    b = {}

    a.should_not == b

    b[:a] = 5

    a.should == b

    c = Hash.new {|h, k| 1}
    d = Hash.new {}
    c[1] = 2
    d[1] = 2
    c.should == d
  end
  
  # Bad test? Ruby doesn't seem to call to_hash on its argument
  skip "(Fails on Ruby 1.8.6) == should call to_hash on its argument" do
    obj = ToHash.new({1 => 2, 3 => 4})
    obj.to_hash.should == {1 => 2, 3 => 4}
    
    {1 => 2, 3 => 4}.should == obj
  end
  
  specify "== shouldn't call to_hash on hash subclasses" do
    {5 => 6}.should == ToHashHash[5 => 6]
  end
  
  specify "== should be instantly false when the numbers of keys differ" do
    obj = EqlTest.new  true
    {}.should_not == { obj => obj }
    { obj => obj }.should_not == {}
    obj.called_eql.should == false
    obj.called_eq.should == false
  end
  
  specify "== should compare keys with eql? semantics" do
    { 1.0 => "x" }.should == { 1.0 => "x" }
    { 1 => "x" }.should_not == { 1.0 => "x" }
    { 1.0 => "x" }.should_not == { 1 => "x" }
  end

  specify "== should first compare keys via hash" do
    x = IntHash0.new false
    y = IntHash0.new false
    { x => 1 } == { y => 1 }
    x.called_hash.should == true
    y.called_hash.should == true
    x.called_eql.should == true
    y.called_eql.should == true
    x.called_eq.should == false
    y.called_eq.should == false
  end
  
  specify "== shouldn't compare keys with different hash codes via eql?" do
    x = IntHash0.new true
    y = IntHash1.new true
    { x => 1 }.should_not == { y => 1 }
    x.called_hash.should == true
    y.called_hash.should == true
    x.called_eql.should == false
    y.called_eql.should == false
    x.called_eq.should == false
    y.called_eq.should == false
  end    
    
  specify "== should compare keys with matching hash codes via eql?" do
    a = Array.new(2) { IntHash0.new(false) }

    { a[0] => 1 }.should_not == { a[1] => 1 }
    a[0].called_eql.should == true
    a[1].called_eql.should == true
    a[0].called_eq.should == false
    a[1].called_eq.should == false

    a = Array.new(2) { IntHash0.new(true) }

    { a[0] => 1 }.should == { a[1] => 1 }
    a[0].called_eql.should == true
    a[1].called_eql.should == true
    a[0].called_eq.should == false
    a[1].called_eq.should == false
  end
  
  specify "== should compare values with == semantics" do
    { "x" => 1.0 }.should == { "x" => 1 }
  end
  
  specify "== shouldn't compare values when keys don't match" do
    value = EqlTest.new true    
    { 1 => value }.should_not == { 2 => value }
    value.called_eql.should == false
    value.called_eq.should == false
  end
  
  specify "== should compare values when keys match" do
    x = EqlTest.new false
    y = EqlTest.new false
    { 1 => x }.should_not == { 1 => y }
    x.called_eq.should == true
    y.called_eq.should == true
    x.called_eql.should == false
    y.called_eql.should == false

    x = EqlTest.new true
    y = EqlTest.new true
    { 1 => x }.should == { 1 => y }
    x.called_eq.should == true
    y.called_eq.should == true
    x.called_eql.should == false
    y.called_eql.should == false
  end

  specify "== should ignore hash class differences" do
    h = { 1 => 2, 3 => 4 }
    MyHash[h].should == h
    MyHash[h].should == MyHash[h]
    h.should == MyHash[h]
  end
end

context "Hash#[] (instance method)" do  
  specify "[] should return the value for key" do
    obj = Object.new
    h = { 1 => 2, 3 => 4, "foo" => "bar", obj => obj }
    h[1].should == 2
    h[3].should == 4
    h["foo"].should == "bar"
    h[obj].should == obj
  end
  
  specify "[] should return the default (immediate) value for missing keys" do
    h = Hash.new(5)
    h[:a].should == 5
    h[:a] = 0
    h[:a].should == 0
    h[:b].should == 5
    
    # The default default is nil
    { 0 => 0 }[5].should == nil
  end

  specify "[] shouldn't create copies of the immediate default value" do
    str = "foo"
    h = Hash.new(str)
    a = h[:a]
    b = h[:b]
    a << "bar"

    a.equal?(b).should == true
    a.should == "foobar"
    b.should == "foobar"
  end

  specify "[] should return the default (dynamic) value for missing keys" do
    h = Hash.new { |hash, k| k.kind_of?(Numeric) ? hash[k] = k + 2 : hash[k] = k }
    h[1].should == 3
    h['this'].should == 'this'
    h.should == {1 => 3, 'this' => 'this'}
    
    i = 0
    h = Hash.new { |hash, key| i += 1 }
    h[:foo].should == 1
    h[:foo].should == 2
    h[:bar].should == 3
  end

  specify "[] shouldn't return default values for keys with nil values" do
    h = Hash.new(5)
    h[:a] = nil
    h[:a].should == nil
    
    h = Hash.new() { 5 }
    h[:a] = nil
    h[:a].should == nil
  end
  
  specify "[] should compare keys with eql? semantics" do
    { 1.0 => "x" }[1].should == nil
    { 1.0 => "x" }[1.0].should == "x"
    { 1 => "x" }[1.0].should == nil
    { 1 => "x" }[1].should == "x"
  end

  specify "[] should compare key via hash" do
    x = IntHash0.new true
    { }[x].should == nil
    x.called_hash.should == true
  end
  
  specify "[] shouldn't compare key with unknown hash codes via eql?" do
    x = IntHash0.new true
    y = IntHash1.new true
    { y => 1 }[x].should == nil
    x.called_hash.should == true
    x.called_eql.should == false
    y.called_eql.should == false
  end
    
  specify "[] should compare key with found hash code via eql?" do
    x = IntHash0.new false
    y = IntHash0.new false
    { y => 1 }[x].should == nil
    x.called_eql.should == true
    
    x = IntHash0.new true
    
    { y => 1 }[x].should == 1
    x.called_eql.should == true
  end  
end

context "Hash#[]=" do
  specify "[]= should associate the key with the value and return the value" do
    h = { :a => 1 }
    (h[:b] = 2).should == 2
    h.should == {:b=>2, :a=>1}
  end
  
  specify "[]= should duplicate and freeze string keys" do
    key = "foo"
    h = {}
    h[key] = 0
    key << "bar"

    h.should == { "foo" => 0 }
    skip "TODO: implement freeze" do
      h.keys[0].frozen?.should == true
    end
  end

  skip "TODO: []= should duplicate string keys using dup semantics" do
    # dup doesn't copy singleton methods
    key = "foo"
    def key.reverse() "bar" end
    h = {}
    h[key] = 0

    h.keys[0].reverse.should == "oof"
  end  
end

context "Hash#clear" do
  specify "clear should remove all key, value pairs" do
    h = { 1 => 2, 3 => 4 }
    h.clear.equal?(h).should == true
    h.should == {}
  end

  specify "clear shouldn't remove default values and procs" do
    h = Hash.new(5)
    h.clear
    h.default.should == 5

    h = Hash.new { 5 }
    h.clear
    h.default_proc.should_not == nil
  end
end

context "Hash#default" do
  specify "default should return the default value" do
    h = Hash.new(5)
    h.default.should == 5
    h.default(4).should == 5
    {}.default.should == nil
    {}.default(4).should == nil
  end

  specify "default should use the default proc to compute a default value, passing given key" do
    h = Hash.new { |*args| args }
    h.default(nil).should == [h, nil]
    h.default(5).should == [h, 5]
  end
  
  skip "(Fails on Ruby 1.8.6) default with default proc, but no arg should call default proc with nil arg" do
    h = Hash.new { |*args| args }
    h.default.should == [h, nil]
  end
end

context "Hash#default=" do
  specify "default= should set the default value" do
    h = Hash.new
    h.default = 99
    h.default.should == 99
  end

  specify "default= should unset the default proc" do
    p = Proc.new { 6 }
    [99, nil, p].each do |default|
      h = Hash.new { 5 }
      h.default_proc.should_not == nil
      h.default = default
      h.default.should == default
      h.default_proc.should == nil
    end
  end  
end

context "Hash#default_proc" do
  specify "default_proc should return the block passed to Hash.new" do
    h = Hash.new { |i| 'Paris' }
    p = h.default_proc
    p.call(1).should == 'Paris'
  end
  
  specify "default_proc should return nil if no block was passed to proc" do
    Hash.new.default_proc.should == nil
  end
end

context "Hash#delete" do
  it "delete should delete the item with the specified key" do
    h = {:a => 5, :b => 2}
    h.delete(:b).should == 2
    h.should == {:a => 5}    
  end
  
  skip "(BUG: Ruby's Hash respects an ordering constraint that we don't) delete should first entry (#keys order) whose key is == key and return the deleted value" do
    # So they end up in the same bucket
    class ArrayWithHash < Array
      def hash() 0 end
    end

    k1 = ArrayWithHash.new
    k1[0] = "x"
    k2 = ArrayWithHash.new
    k2[0] = "y"
    
    h = Hash.new
    h[k1] = 1
    h[k2] = 2
    k1.replace(k2)
    
    first_value = h.values.first
    first_key = h.keys.first
    h.delete(first_key).should == first_value
    h.size.should == 1
  end

  specify "delete should call supplied block if the key is not found" do
    d = {:a => 1, :b => 10, :c => 100 }.delete(:d) { 5 }
    d.should == 5
    d = Hash.new(:default).delete(:d) { 5 }
    d.should == 5
    h = Hash.new() { :default }
    d = h.delete(:d) { 5 }
    d.should == 5
  end
  
  specify "delete should return nil if the key is not found when no block is given" do
    {:a => 1, :b => 10, :c => 100 }.delete(:d).should == nil
    Hash.new(:default).delete(:d).should == nil
    h = Hash.new() { :defualt }
    h.delete(:d).should == nil
  end
end

context "Hash#delete_if" do
  specify "delete_if should yield two arguments: key and value" do
    all_args = []
    {1 => 2, 3 => 4}.delete_if { |*args| all_args << args }
    all_args.should == [[1, 2], [3, 4]]
  end
  
  specify "delete_if should remove every entry for which block is true and returns self" do
    h = {:a => 1, :b => 2, :c => 3, :d => 4}
    h2 = h.delete_if { |k,v| v % 2 == 1 }
    h2.equal?(h).should == true
    h.should == {:b => 2, :d => 4}
  end
  
  specify "delete_if should process entries with the same order as each()" do
    h = {:a => 1, :b => 2, :c => 3, :d => 4}

    each_pairs = []
    delete_pairs = []
    h.each { |pair| each_pairs << pair }
    h.delete_if { |*pair| delete_pairs << pair }

    each_pairs.should == delete_pairs
  end
end

context "Hash#each" do
  specify "each should yield one argument: [key, value]" do
    all_args = []
    {1 => 2, 3 => 4}.each { |*args| all_args << args }
    all_args.should == [[[1, 2]], [[3, 4]]]
  end
  
  specify "each should call block once for each entry, passing key, value" do
    r = {}
    h = {:a => 1, :b => 2, :c => 3, :d => 5}
    h2 = h.each { |p| r[p[0].to_s] = p[1].to_s }
    h2.equal?(h).should == true
    r.should == {"a" => "1", "b" => "2", "c" => "3", "d" => "5" }
  end

  specify "each should use the same order as keys() and values()" do
    h = {:a => 1, :b => 2, :c => 3, :d => 5}
    keys = []
    values = []

    h.each do |p|
      keys << p[0]
      values << p[1]
    end
    
    keys.should == h.keys
    values.should == h.values
  end
end

context "Hash#each_key" do
  specify "each_key should call block once for each key, passing key" do
    r = {}
    h = {1 => -1, 2 => -2, 3 => -3, 4 => -4 }
    h2 = h.each_key { |k| r[k] = k }
    h2.equal?(h).should == true
    r.should == { 1 => 1, 2 => 2, 3 => 3, 4 => 4 }
  end

  specify "each_key should process keys in the same order as keys()" do
    keys = []
    h = {1 => -1, 2 => -2, 3 => -3, 4 => -4 }
    h.each_key { |k| keys << k }
    keys.should == h.keys
  end  
end

context "Hash#each_pair" do
  specify "each_pair should process all pairs, yielding two arguments: key and value" do
    all_args = []

    h = {1 => 2, 3 => 4}
    h2 = h.each_pair { |*args| all_args << args }
    h2.equal?(h).should == true

    all_args.should == [[1, 2], [3, 4]]
  end
end

context "Hash#each_value" do
  specify "each_value should call block once for each key, passing value" do
    r = []
    h = { :a => -5, :b => -3, :c => -2, :d => -1, :e => -1 }
    h2 = h.each_value { |v| r << v }
    h2.equal?(h).should == true
    r.sort.should == [-5, -3, -2, -1, -1]
  end

  specify "each_value should process values in the same order as values()" do
    values = []
    h = { :a => -5, :b => -3, :c => -2, :d => -1, :e => -1 }
    h.each_value { |v| values << v }
    values.should == h.values
  end
end

context "Hash#empty?" do
  specify "empty? should return true if the hash has no entries" do
    {}.empty?.should == true
    {1 => 1}.empty?.should == false
    Hash.new(5).empty?.should == true
    h = Hash.new { 5 }
    h.empty?.should == true
  end
end

context "Hash#fetch" do
  specify "fetch should return the value for key" do
    { :a => 1, :b => -1 }.fetch(:b).should == -1
  end
  
  specify "fetch should raise IndexError if key is not found" do
    should_raise(IndexError) { {}.fetch(:a) }
    should_raise(IndexError) { Hash.new(5).fetch(:a) }
    should_raise(IndexError) do
      h = Hash.new { 5 }
      h.fetch(:a)
    end
  end
  
  specify "fetch with default should return default if key is not found" do
    {}.fetch(:a, nil).should == nil
    {}.fetch(:a, 'not here!').should == "not here!"
    { :a => nil }.fetch(:a, 'not here!').should == nil
  end
  
  specify "fetch with block should return value of block if key is not found" do
    r = {}.fetch('a') { |k| k + '!' }
    r.should == "a!"
  end

  specify "fetch's default block should take precedence over its default argument" do
    r = {}.fetch(9, :foo) { |i| i * i }
    r.should == 81
  end
end

context "Hash#has_key?" do
  specify "has_key? should be a synonym for key?" do
    h = {:a => 1, :b => 2, :c => 3}
    h.has_key?(:a).should == h.key?(:a)
    h.has_key?(:b).should == h.key?(:b) 
    h.has_key?('b').should == h.key?('b') 
    h.has_key?(2).should == h.key?(2)
  end
end

context "Hash#has_value?" do
  specify "has_value? should be a synonym for value?" do
    {:a => :b}.has_value?(:a).should == {:a => :b}.value?(:a)
    {1 => 2}.has_value?(2).should == {1 => 2}.value?(2)
    h = Hash.new(5)
    h.has_value?(5).should == h.value?(5)
    h = Hash.new { 5 }
    h.has_value?(5).should == h.value?(5)
  end
end

context "Hash#include?" do
  specify "include? should be a synonym for key?" do
    h = {:a => 1, :b => 2, :c => 3}
    h.include?(:a).should   == h.key?(:a) 
    h.include?(:b).should   == h.key?(:b) 
    h.include?('b').should  == h.key?('b')
    h.include?(2).should    == h.key?(2)
  end
end

context "Hash#index" do
  specify "index should return the corresponding key for value" do
    {2 => 'a', 1 => 'b'}.index('b').should == 1
  end
  
  specify "index should return nil if the value is not found" do
    {:a => -1, :b => 3.14, :c => 2.718}.index(1).should == nil
    Hash.new(5).index(5).should == nil
  end

  specify "index should compare values using ==" do
    {1 => 0}.index(0.0).should == 1
    {1 => 0.0}.index(0).should == 1
    
    needle = EqlTest.new true
    inhash = EqlTest.new true
    
    {1 => inhash}.index(needle).should == 1
    needle.called_eql.should == false
    inhash.called_eql.should == false
    needle.called_eq.should == true
    inhash.called_eq.should == true
  end

  specify "index should compare values with same order as keys() and values()" do
    h = {1 => 0, 2 => 0, 3 => 0, 4 => 0, 5 => 0, 6 => 0}
    h.index(0).should == h.keys.first
    
    h = {1 => Object.new, 3 => Object.new, 4 => Object.new, 42 => Object.new }
    needle = h.values[2]
    h.index(needle).should == h.keys[2]
  end
end

context "Hash#indexes, Hash#indices" do
  specify "indexes and indices should be DEPRECATED synonyms for values_at" do
    h = {:a => 9, :b => 'a', :c => -10, :d => nil}
    h.indexes(:a, :d, :b).should == h.values_at(:a, :d, :b)
    h.indexes().should == h.values_at()
    h.indices(:a, :d, :b).should == h.values_at(:a, :d, :b)
    h.indices().should == h.values_at()
  end
end

skip "(TODO, testing private method) Hash#initialize" do
  specify "initialize should be private" do
    #{}.private_methods.map { |m| m.to_s }.include?("initialize").should == true
  end

  specify "initialize can be used to reset default_proc" do
    h = { "foo" => 1, "bar" => 2 }
    h.default_proc.should == nil
    h.instance_eval { initialize { |h, k| k * 2 } }
    h.default_proc.should_not == nil
    h["a"].should == "aa"
  end
end

skip "(TODO, testing private method) Hash#initialize_copy" do
  specify "initialize_copy should be private" do
    #{}.private_methods.map { |m| m.to_s }.include?("initialize_copy").should == true
  end
  
  specify "initialize_copy should be a synonym for replace" do
     # TODO: entering try with non-empty stack bug
    # init_hash = Hash.new
    # repl_hash = Hash.new
    # arg = { :a => 1, :b => 2 }
    
    # init_hash.instance_eval { initialize_copy(arg) }.should == repl_hash.replace(arg)
    # init_hash.should == repl_hash
  end
  
  specify "initialize_copy should have the same to_hash behaviour as replace" do
    init_hash = Hash.new
    repl_hash = Hash.new
    arg1 = Object.new
    def arg1.to_hash() {1 => 2} end
    arg2 = ToHashHash[1 => 2]
    
    # TODO: entering try with non-empty stack bug
    # [arg1, arg2].each do |arg|      
    #  init_hash.instance_eval { initialize_copy(arg) }.should == repl_hash.replace(arg)
    #  init_hash.should == repl_hash
    # end
  end
end

context "Hash#inspect" do
  specify "inspect should return a string representation with same order as each()" do
    h = {:a => [1, 2], :b => -2, :d => -6, nil => nil, "abc" => {7 => 8} }
    
    pairs = []
    h.each do |pair|
      pairs << pair[0].inspect + "=>" + pair[1].inspect
    end
    
    str = '{' + pairs.join(', ') + '}'
    h.inspect.should == str
  end

  specify "inspect should call inspect on keys and values" do
    class InspectKey
      def inspect() 'key' end
    end
    class InspectVal
      def inspect() 'val' end
    end
    
    key = InspectKey.new
    val = InspectVal.new
    
    { key => val }.inspect.should == '{key=>val}'
  end

  specify "inspect should handle recursive hashes" do
    x = {}
    x[0] = x
    x.inspect.should == '{0=>{...}}'

    x = {}
    x[x] = 0
    x.inspect.should == '{{...}=>0}'

    x = {}
    x[x] = x
    x.inspect.should == '{{...}=>{...}}'

    x = {}
    y = {}
    x[0] = y
    y[1] = x
    x.inspect.should == "{0=>{1=>{...}}}"
    y.inspect.should == "{1=>{0=>{...}}}"

    x = {}
    y = {}
    x[y] = 0
    y[x] = 1
    x.inspect.should == "{{{...}=>1}=>0}"
    y.inspect.should == "{{{...}=>0}=>1}"
    
    x = {}
    y = {}
    x[y] = x
    y[x] = y
    x.inspect.should == "{{{...}=>{...}}=>{...}}"
    y.inspect.should == "{{{...}=>{...}}=>{...}}"
  end
end

context "Hash#invert" do
  specify "invert should return a new hash where keys are values and vice versa" do
    { 1 => 'a', 2 => 'b', 3 => 'c' }.invert.should == { 'a' => 1, 'b' => 2, 'c' => 3 }
  end
  
  specify "invert should handle collisions by overriding with the key coming later in keys()" do
    h = { :a => 1, :b => 1 }
    override_key = h.keys.last
    h.invert[1].should == override_key
  end

  specify "invert should compare new keys with eql? semantics" do
    h = { :a => 1.0, :b => 1 }
    i = h.invert
    i[1.0].should == :a
    i[1].should == :b
  end
  
  specify "invert on hash subclasses shouldn't return subclass instances" do
    MyHash[1 => 2, 3 => 4].invert.class.should == Hash
    MyHash[].invert.class.should == Hash
  end
end

context "Hash#key?" do
  specify "key? should return true if argument is a key" do
    h = { :a => 1, :b => 2, :c => 3, 4 => 0 }
    h.key?(:a).should == true
    h.key?(:b).should == true
    h.key?('b').should == false
    h.key?(2).should == false
    h.key?(4).should == true
    h.key?(4.0).should == false
  end

  specify "key? should return true if the key's matching value was nil" do
    { :xyz => nil }.key?(:xyz).should == true
  end

  specify "key? should return true if the key's matching value was false" do
    { :xyz => false }.key?(:xyz).should == true
  end
end

context "Hash#keys" do
  specify "keys should return an array populated with keys" do
    {}.keys.should == []
    {}.keys.class.should == Array
    Hash.new(5).keys.should == []
    h = Hash.new { 5 }
    h.keys.should == []
    { 1 => 2, 2 => 4, 4 => 8 }.keys.should == [1, 2, 4]
    { 1 => 2, 2 => 4, 4 => 8 }.keys.class.should == Array
    { nil => nil }.keys.should == [nil]
  end

  specify "keys and values should use the same order" do
    h = { 1 => "1", 2 => "2", 3 => "3", 4 => "4" }
    
    h.size.times do |i|
      h[h.keys[i]].should == h.values[i]
    end
  end
end

context "Hash#length" do
  specify "length should return the number of entries" do
    {:a => 1, :b => 'c'}.length.should == 2
    {:a => 1, :b => 2, :a => 2}.length.should == 2
    {:a => 1, :b => 1, :c => 1}.length.should == 3
    {}.length.should == 0
    Hash.new(5).length.should == 0
    h = Hash.new { 5 }
    h.length.should == 0
  end
end

context "Hash#member?" do
  specify "member? should be a synonym for key?" do
    h = {:a => 1, :b => 2, :c => 3}
    h.member?(:a).should == h.key?(:a)
    h.member?(:b).should == h.key?(:b)
    h.member?('b').should == h.key?('b')
    h.member?(2).should == h.key?(2)
  end
end

context "Hash#merge" do
  specify "merge should return a new hash by combining self with the contents of other" do
    { 1 => :a, 2 => :b, 3 => :c }.merge(:a => 1, :c => 2).should == { :c => 2, 1 => :a, 2 => :b, :a => 1, 3 => :c }
  end

  specify "merge with block sets any duplicate key to the value of block" do
    h1 = { :a => 2, :b => 1, :d => 5}
    h2 = { :a => -2, :b => 4, :c => -3 }
    r = h1.merge(h2) { |k,x,y| nil }
    r.should == { :a => nil, :b => nil, :c => -3, :d => 5 }
      
    r = h1.merge(h2) { |k,x,y| "#{k}:#{x+2*y}" }
    r.should == { :a => "a:-2", :b => "b:9", :c => -3, :d => 5 }

    should_raise(IndexError) do
      h1.merge(h2) { |k, x, y| raise(IndexError) }
    end

    r = h1.merge(h1) { |k,x,y| :x }
    r.should == { :a => :x, :b => :x, :d => :x }
  end
  
  specify "merge should call to_hash on its argument" do
    obj = ToHash.new({1 => 2})
    {3 => 4}.merge(obj).should == {1 => 2, 3 => 4}
    
    obj = ToHashUsingMissing.new({1 => 2})
    
    {3 => 4}.merge(obj).should == {1 => 2, 3 => 4}
  end

  specify "merge shouldn't call to_hash on hash subclasses" do    
    {3 => 4}.merge(ToHashHash[1 => 2]).should == {1 => 2, 3 => 4}
  end

  specify "merge on hash subclasses should return subclass instance" do
    MyHash[1 => 2, 3 => 4].merge({1 => 2}).class.should == MyHash
    MyHash[].merge({1 => 2}).class.should == MyHash

    {1 => 2, 3 => 4}.merge(MyHash[1 => 2]).class.should == Hash
    {}.merge(MyHash[1 => 2]).class.should == Hash
  end
  
  specify "merge should process entries with same order as each()" do
    h = {1 => 2, 3 => 4, 5 => 6, "x" => nil, nil => 5, [] => []}
    merge_pairs = []
    each_pairs = []
    h.each { |pair| each_pairs << pair }
    h.merge(h) { |k, v1, v2| merge_pairs << [k, v1] }
    merge_pairs.should == each_pairs
  end
end

context "Hash#merge!" do
  specify "merge! should add the entries from other, overwriting duplicate keys. Returns self" do
    h = { :_1 => 'a', :_2 => '3' }
    h2 = h.merge!(:_1 => '9', :_9 => 2).equal?(h).should == true
    h.should == {:_1 => "9", :_2 => "3", :_9 => 2}
  end
  
  specify "merge! with block sets any duplicate key to the value of block" do
    h1 = { :a => 2, :b => -1 }
    h2 = { :a => -2, :c => 1 }
    h3 = h1.merge!(h2) { |k,x,y| 3.14 }
    h3.equal?(h1).should == true
    h1.should == {:c => 1, :b => -1, :a => 3.14}
    
    h1.merge!(h1) { nil }
    h1.should == { :a => nil, :b => nil, :c => nil }
  end
  
  specify "merge! should call to_hash on its argument" do
    obj = ToHash.new({1 => 2})
    {3 => 4}.merge!(obj).should == {1 => 2, 3 => 4}

    obj = ToHashUsingMissing.new({ 1 => 2 })
    {3 => 4}.merge!(obj).should == {1 => 2, 3 => 4}
  end

  specify "merge! shouldn't call to_hash on hash subclasses" do    
    {3 => 4}.merge!(ToHashHash[1 => 2]).should == {1 => 2, 3 => 4}
  end
  
  specify "merge! should process entries with same order as merge()" do
    h = {1 => 2, 3 => 4, 5 => 6, "x" => nil, nil => 5, [] => []}
    merge_bang_pairs = []
    merge_pairs = []
    h.merge(h) { |*arg| merge_pairs << arg }
    h.merge!(h) { |*arg| merge_bang_pairs << arg }
    merge_bang_pairs.should == merge_pairs
  end
end

context "Hash#rehash" do
  specify "rehash should reorganize the hash by recomputing all key hash codes" do
    k1 = [1]
    k2 = [2]
    h = {}
    h[k1] = 0
    h[k2] = 1

    k1 << 2

    h.key?(k1).should == false
    h.keys.include?(k1).should == true
    
    h.rehash.equal?(h).should == true
    h.key?(k1).should == true
    h[k1].should == 0
    
    k1 = IntHash0.new false
    k2 = IntHashX.new 1, false
    v1 = NoHash.new
    v2 = NoHash.new

    h = { k1 => v1, k2 => v2 }
    k1.called_hash.should == true
    k2.called_hash.should == true
    k1.called_hash = false
    k2.called_hash = false
    
    k2.newhash = 0
    
    h.rehash
    k1.called_hash.should == true
    k2.called_hash.should == true    
  end
  
  specify "rehash gives precedence to keys coming later in keys() on collisions" do
    k1 = [1]
    k2 = [2]
    h = {}
    h[k1] = 0
    h[k2] = 1

    k1.replace(k2)
    override_val = h[h.keys.last]
    h.rehash
    h[k1].should == override_val
  end
 
end

context "Hash#reject" do
  specify "reject should be equivalent to hsh.dup.delete_if" do
    h = { :a => 'a', :b => 'b', :c => 'd' }
    h2 = h.reject { |k,v| k == 'd' }
    h3 = h.dup.delete_if { |k, v| k == 'd' }
    h2.should == h3
    
    all_args_reject = []
    all_args_delete_if = []
    h = {1 => 2, 3 => 4}
    h.reject { |*args| all_args_reject << args }
    h.delete_if { |*args| all_args_delete_if << args }
    all_args_reject.should == all_args_delete_if
    
    skip "TODO: singleton method support" do
      h = { 1 => 2 }
      # dup doesn't copy singleton methods
      def h.to_a() end
      h2 = h.reject { false }
      h2.to_a.should == [[1, 2]]
    end
  end
  
  specify "reject on hash subclasses should return subclass instance" do
    h = MyHash[1 => 2, 3 => 4].reject { false }
    h.class.should == MyHash
    h = MyHash[1 => 2, 3 => 4].reject { true }
    h.class.should == MyHash
  end
  
  specify "reject should process entries with the same order as reject!" do
    h = {:a => 1, :b => 2, :c => 3, :d => 4}

    reject_pairs = []
    reject_bang_pairs = []
    h.reject { |*pair| reject_pairs << pair }
    h.reject! { |*pair| reject_bang_pairs << pair }

    reject_pairs.should == reject_bang_pairs
  end
end

context "Hash#reject!" do
  specify "reject! is equivalent to delete_if if changes are made" do
    h = {:a => 2}.reject! { |k,v| v > 1 }
    h2 = {:a => 2}.delete_if { |k, v| v > 1 }
    h.should == h2

    h = {1 => 2, 3 => 4}
    all_args_reject = []
    all_args_delete_if = []
    h.dup.reject! { |*args| all_args_reject << args }
    h.dup.delete_if { |*args| all_args_delete_if << args }
    all_args_reject.should == all_args_delete_if
  end
  
  specify "reject! should return nil if no changes were made" do
    r = { :a => 1 }.reject! { |k,v| v > 1 }
    r.should == nil
  end
  
  specify "reject! should process entries with the same order as delete_if" do
    h = {:a => 1, :b => 2, :c => 3, :d => 4}

    reject_bang_pairs = []
    delete_if_pairs = []
    h.dup.reject! { |*pair| reject_bang_pairs << pair }
    h.dup.delete_if { |*pair| delete_if_pairs << pair }

    reject_bang_pairs.should == delete_if_pairs
  end
end

context "Hash#replace" do
  specify "replace should replace the contents of self with other" do
    h = { :a => 1, :b => 2 }
    h.replace(:c => -1, :d => -2).equal?(h).should == true
    h.should == { :c => -1, :d => -2 }
  end

  specify "replace should call to_hash on its argument" do
    obj = ToHash.new({1 => 2, 3 => 4})
    
    h = {}
    h.replace(obj)
    h.should == {1 => 2, 3 => 4}
    
    obj = ToHashUsingMissing.new({})

    h.replace(obj)
    h.should == {}
  end

  specify "replace shouldn't call to_hash on hash subclasses" do
    h = {}
    h.replace(ToHashHash[1 => 2])
    h.should == {1 => 2}
  end
  
  specify "replace should transfer default values" do
    hash_a = {}
    hash_b = Hash.new(5)
    hash_a.replace(hash_b)
    hash_a.default.should == 5
    
    hash_a = {}
    hash_b = Hash.new { |h, k| k * 2 }
    hash_a.replace(hash_b)
    hash_a.default(5).should == 10
    
    skip "TODO: lambda not implemented" do
      hash_a = Hash.new { |h, k| k * 5 }
      p = lambda { raise "Should not invoke lambda" }
      hash_b = Hash.new(p)
      hash_a.replace(hash_b)
      hash_a.default.should == hash_b.default
    end
  end
end

context "Hash#select" do
  specify "select should yield two arguments: key and value" do
    all_args = []
    {1 => 2, 3 => 4}.select { |*args| all_args << args }
    all_args.should == [[1, 2], [3, 4]]
  end
  
  specify "select should return an array of entries for which block is true" do
    { :a => 9, :c => 4, :b => 5, :d => 2 }.select { |k,v| v % 2 == 0 }
  end

  specify "select should process entries with the same order as reject" do
    h = { :a => 9, :c => 4, :b => 5, :d => 2 }
    
    select_pairs = []
    reject_pairs = []
    h.select { |*pair| select_pairs << pair }
    h.reject { |*pair| reject_pairs << pair }
    
    select_pairs.should == reject_pairs
  end
end

context "Hash#shift" do
  specify "shift should remove a pair from hash and return it (same order as to_a)" do
    hash = { :a => 1, :b => 2, "c" => 3, nil => 4, [] => 5 }
    pairs = hash.to_a
    
    hash.size.times do
      r = hash.shift
      r.class.should == Array
      r.should == pairs.shift
      hash.size.should == pairs.size
    end
    
    hash.should == {}
    hash.shift.should == nil
  end
  
  specify "shift should return (computed) default for empty hashes" do
    Hash.new(5).shift.should == 5
    h = Hash.new { |*args| args }
    h.shift.should == [h, nil]
  end
end

context "Hash#size" do 
  specify "size should be a synonym for length" do
    { :a => 1, :b => 'c' }.size.should == {:a => 1, :b => 'c'}.length 
    {}.size.should == {}.length
  end
end

context "Hash#sort" do
  specify "sort should convert self to a nested array of [key, value] arrays and sort with Array#sort" do
    { 'a' => 'b', '1' => '2', 'b' => 'a' }.sort.should == [["1", "2"], ["a", "b"], ["b", "a"]]
  end
  
  specify "sort should work when some of the keys are themselves arrays" do
    { [1,2] => 5, [1,1] => 5 }.sort.should == [[[1,1],5], [[1,2],5]]
  end
  
  specify "sort with block should use block to sort array" do
    a = { 1 => 2, 2 => 9, 3 => 4 }.sort { |a,b| b <=> a }
    a.should == [[3, 4], [2, 9], [1, 2]]
  end
end

context "Hash#store" do
  specify "store should be a synonym for []=" do
    h1, h2 = {:a => 1}, {:a => 1}
    h1.store(:b, 2).should == (h2[:b] = 2)
    h1.should == h2
  end
end

context "Hash#to_a" do
  specify "to_a should return a list of [key, value] pairs with same order as each()" do
    h = {:a => 1, 1 => :a, 3 => :b, :b => 5}
    pairs = []

    h.each do |pair|
      pairs << pair
    end
    
    h.to_a.class.should == Array
    h.to_a.should == pairs
  end
end

context "Hash#to_hash" do
  specify "to_hash should return self" do
    h = {}
    h.to_hash.equal?(h).should == true
  end
end

context "Hash#to_s" do
  specify "to_s should return a string by calling Hash#to_a and using Array#join with default separator" do
    h = { :fun => 'x', 1 => 3, nil => "ok", [] => :y }
    h.to_a.to_s.should == h.to_s
    $, = ':'
    h.to_a.to_s.should == h.to_s
  end
end

context "Hash#update" do
  specify "update should be a synonym for merge!" do
    h1 = { :_1 => 'a', :_2 => '3' }
    h2 = h1.dup

    h1.update(:_1 => '9', :_9 => 2).should == h2.merge!(:_1 => '9', :_9 => 2)
    h1.should == h2
  end

  specify "update with block should be a synonym for merge!" do
    h1 = { :a => 2, :b => -1 }
    h2 = h1.dup

    h3 = h1.update(:a => -2, :c => 1) { |k,v| 3.14 }
    h4 = h2.merge!(:a => -2, :c => 1) { |k,v| 3.14 }
    h3.should == h4
    h1.should == h2
  end
  
  specify "update should have the same to_hash behaviour as merge!" do
    update_hash = Hash.new
    merge_hash = Hash.new

    arg1 = ToHash.new({1 => 2})
    arg2 = ToHashHash[1 => 2]
    
    [arg1, arg2].each do |arg|      
      update_hash.update(arg).should == merge_hash.merge!(arg)
      update_hash.should == merge_hash
    end    
  end
end

context "Hash#value?" do
  specify "value? returns true if the value exists in the hash" do
    {:a => :b}.value?(:a).should == false
    {1 => 2}.value?(2).should == true
    h = Hash.new(5)
    h.value?(5).should == false
    h = Hash.new { 5 }
    h.value?(5).should == false
  end

  specify "value? uses == semantics for comparing values" do
    { 5 => 2.0 }.value?(2).should == true
  end
end

context "Hash#values" do
  specify "values should return an array of values" do
    h = { 1 => :a, 'a' => :a, 'the' => 'lang'}
    h.values.class.should == Array
    h2 = h.values.sort {|a, b| a.to_s <=> b.to_s}
    h2.should == [:a, :a, 'lang']
  end
end

context "Hash#values_at" do
  specify "values_at should return an array of values for the given keys" do
    h = {:a => 9, :b => 'a', :c => -10, :d => nil}
    h.values_at().class.should == Array
    h.values_at().should == []
    h.values_at(:a, :d, :b).class.should == Array
    h.values_at(:a, :d, :b).should == [9, nil, 'a']
  end
end

skip "BUG: we convert InvalidOperationException into TypeError, but it should be RuntimeError" do
  # These are the only ones that actually have the exceptions on MRI 1.8.
  # sort and reject don't raise!
  %w(
    delete_if each each_key each_pair each_value merge merge! reject!
    select update
  ).each do |cmd|
    hash = {1 => 2, 3 => 4, 5 => 6}  
    big_hash = {}
    100.times { |k| big_hash[k.to_s] = k }    
       
    specify "#{cmd} should raise if rehash() is called from block" do
      h = hash.dup
      args = cmd[/merge|update/] ? [h] : []
      
      should_raise(RuntimeError, "rehash occurred during iteration") do
        h.send(cmd, *args) { h.rehash }
      end
    end

    specify "#{cmd} should raise if lots of new entries are added from block" do
      h = hash.dup
      args = cmd[/merge|update/] ? [h] : []

      should_raise(RuntimeError, "hash modified during iteration") do
        h.send(cmd, *args) { |*x| h.merge!(big_hash) }
      end
    end

    skip "(Test failure on Ruby 1.8.6) #{cmd}'s yielded items shouldn't be affected by removing current element" do
      n = 3
      
      h = Array.new(n) { hash.dup }
      args = Array.new(n) { |i| cmd[/merge|update/] ? [h[i]] : [] }
      r = Array.new(n) { [] }
      
      h[0].send(cmd, *args[0]) { |*x| r[0] << x; true }
      h[1].send(cmd, *args[1]) { |*x| r[1] << x; h[1].shift; true }
      h[2].send(cmd, *args[2]) { |*x| r[2] << x; h[2].delete(h[2].keys.first); true }
      
      r[1..-1].each do |yielded|
        yielded.should == r[0]
      end
    end
  end
end

skip "(TODO) On a frozen hash" do
  empty = {}
  empty.freeze

  hash = {1 => 2, 3 => 4}
  hash.freeze
  
  specify "[]= should raise" do
    should_raise(TypeError) { hash[1] = 2 }
  end

  specify "clear should raise" do
    should_raise(TypeError) { hash.clear }
    should_raise(TypeError) { empty.clear }
  end

  specify "default= should raise" do
    should_raise(TypeError) { hash.default = nil }
  end

  specify "delete should raise" do
    should_raise(TypeError) { hash.delete("foo") }
    should_raise(TypeError) { empty.delete("foo") }
  end

  specify "delete_if should raise" do
    should_raise(TypeError) { hash.delete_if { false } }
    should_raise(TypeError) { empty.delete_if { true } }
  end
  
  specify "initialize should raise" do
    hash.instance_eval do
      should_raise(TypeError) { initialize() }
      should_raise(TypeError) { initialize(nil) }
      should_raise(TypeError) { initialize(5) }
      should_raise(TypeError) { initialize { 5 } }
    end
  end

  specify "merge! should raise" do
    hash.merge!(empty) # ok, empty
    should_raise(TypeError) { hash.merge!(1 => 2) }
  end

  specify "rehash should raise" do
    should_raise(TypeError) { hash.rehash }
    should_raise(TypeError) { empty.rehash }
  end
  
  specify "reject! should raise" do
    should_raise(TypeError) { hash.reject! { false } }
    should_raise(TypeError) { empty.reject! { true } }
  end  

  specify "replace should raise" do
    hash.replace(hash) # ok, nothing changed
    should_raise(TypeError) { hash.replace(empty) }
  end  

  specify "shift should raise" do
    should_raise(TypeError) { hash.shift }
    should_raise(TypeError) { empty.shift }
  end

  specify "store should raise" do
    should_raise(TypeError) { hash.store(1, 2) }
    should_raise(TypeError) { empty.shift }
  end

  specify "update should raise" do
    hash.update(empty) # ok, empty
    should_raise(TypeError) { hash.update(1 => 2) }
  end
end

finished
