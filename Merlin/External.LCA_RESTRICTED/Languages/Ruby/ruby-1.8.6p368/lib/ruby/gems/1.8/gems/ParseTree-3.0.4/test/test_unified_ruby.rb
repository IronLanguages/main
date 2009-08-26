#!/usr/local/bin/ruby -w

$TESTING = true

require 'minitest/autorun' if $0 == __FILE__ unless defined? $ZENTEST and $ZENTEST
require 'test/unit/testcase'
require 'sexp'
require 'sexp_processor'
require 'unified_ruby'

class TestUnifier < MiniTest::Unit::TestCase
  def test_pre_fcall
    u = PreUnifier.new

    input  = [:fcall, :block_given?]
    expect = s(:fcall, :block_given?, s(:arglist))

    assert_equal expect, u.process(input)

    input  = [:fcall, :m, [:array, [:lit, 42]]]
    expect = s(:fcall, :m, s(:arglist, s(:lit, 42)))

    assert_equal expect, u.process(input)
  end

  def test_pre_call
    u = PreUnifier.new

    input  = [:call, [:self], :method]
    expect = s(:call, s(:self), :method, s(:arglist))

    assert_equal expect, u.process(input)

    input  = [:fcall, :m, [:array, [:lit, 42]]]
    expect = s(:fcall, :m, s(:arglist, s(:lit, 42)))

    assert_equal expect, u.process(input)
  end

  def test_process_bmethod
    u = Unifier.new

    raw = [:defn, :myproc3,
           [:bmethod,
            [:masgn, [:array,
                      [:dasgn_curr, :a],
                      [:dasgn_curr, :b],
                      [:dasgn_curr, :c]],
             nil, nil]]]

    s = s(:defn, :myproc3,
          s(:args, :a, :b, :c),
          s(:scope, s(:block)))

    assert_equal s, u.process(raw)
  end
end
