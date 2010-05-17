#!/usr/local/bin/ruby -w

$TESTING = true

require 'rubygems'
require 'minitest/autorun'

require 'stringio'
require 'autotest'

# NOT TESTED:
#   class_run
#   add_sigint_handler
#   all_good
#   get_to_green
#   reset
#   ruby
#   run
#   run_tests

class Autotest
  attr_reader :test_mappings, :exception_list

  def self.clear_hooks
    HOOKS.clear
  end
end

class TestAutotest < MiniTest::Unit::TestCase

  def deny test, msg=nil
    if msg then
      assert ! test, msg
    else
      assert ! test
    end
  end unless respond_to? :deny

  RUBY = File.join(Config::CONFIG['bindir'], Config::CONFIG['ruby_install_name']) unless defined? RUBY

  def setup
    @test_class = 'TestBlah'
    @test = 'test/test_blah.rb'
    @other_test = 'test/test_blah_other.rb'
    @impl = 'lib/blah.rb'
    @inner_test = 'test/outer/test_inner.rb'
    @outer_test = 'test/test_outer.rb'
    @inner_test_class = "TestOuter::TestInner"

    klassname = self.class.name.sub(/^Test/, '')
    klassname.sub!(/^(\w+)(Autotest)$/, '\2::\1') unless klassname == "Autotest"
    @a = klassname.split(/::/).inject(Object) { |k,n| k.const_get(n) }.new
    @a.output = StringIO.new
    @a.last_mtime = Time.at(2)

    @files = {}
    @files[@impl] = Time.at(1)
    @files[@test] = Time.at(2)

    @a.find_order = @files.keys.sort
  end

  def test_add_exception
    current = util_exceptions
    @a.add_exception 'blah'

    actual = util_exceptions
    expect = current + ["blah"]

    assert_equal expect, actual
  end

  def test_add_mapping
    current = util_mappings
    @a.add_mapping(/blah/) do 42 end

    actual = util_mappings
    expect = current + [/blah/]

    assert_equal expect, actual
  end

  def test_add_mapping_front
    current = util_mappings
    @a.add_mapping(/blah/, :front) do 42 end

    actual = util_mappings
    expect = [/blah/] + current

    assert_equal expect, actual
  end

  def test_clear_exceptions
    test_add_exception
    @a.clear_exceptions

    actual = util_exceptions
    expect = []

    assert_equal expect, actual
  end

  def test_clear_mapping
    @a.clear_mappings

    actual = util_mappings
    expect = []

    assert_equal expect, actual
  end

  def test_consolidate_failures_experiment
    @files.clear
    @files[@impl] = Time.at(1)
    @files[@test] = Time.at(2)

    @a.find_order = @files.keys.sort

    input = [['test_fail1', @test_class], ['test_fail2', @test_class], ['test_error1', @test_class], ['test_error2', @test_class]]
    result = @a.consolidate_failures input
    expected = { @test => %w( test_fail1 test_fail2 test_error1 test_error2 ) }
    assert_equal expected, result
  end

  def test_consolidate_failures_green
    result = @a.consolidate_failures([])
    expected = {}
    assert_equal expected, result
  end

  def test_consolidate_failures_multiple_possibilities
   @files[@other_test] = Time.at(42)
    result = @a.consolidate_failures([['test_unmatched', @test_class]])
    expected = { @test => ['test_unmatched']}
    assert_equal expected, result
    expected = ""
    assert_equal expected, @a.output.string
  end

  def test_consolidate_failures_nested_classes
    @files.clear
    @files['lib/outer.rb'] = Time.at(5)
    @files['lib/outer/inner.rb'] = Time.at(5)
    @files[@inner_test] = Time.at(5)
    @files[@outer_test] = Time.at(5)

    @a.find_order = @files.keys.sort

    result = @a.consolidate_failures([['test_blah1', @inner_test_class]])
    expected = { @inner_test => ['test_blah1'] }
    assert_equal expected, result
    expected = ""
    assert_equal expected, @a.output.string
  end

  def test_consolidate_failures_no_match
    result = @a.consolidate_failures([['test_blah1', @test_class], ['test_blah2', @test_class], ['test_blah1', 'TestUnknown']])
    expected = {@test => ['test_blah1', 'test_blah2']}
    assert_equal expected, result
    expected = "Unable to map class TestUnknown to a file\n"
    assert_equal expected, @a.output.string
  end

  def test_consolidate_failures_red
    result = @a.consolidate_failures([['test_blah1', @test_class], ['test_blah2', @test_class]])
    expected = {@test => ['test_blah1', 'test_blah2']}
    assert_equal expected, result
  end

  def test_exceptions
    @a.clear_exceptions
    test_add_exception
    assert_equal(/blah/, @a.exceptions)
  end

  def test_exceptions_nil
    @a.clear_exceptions
    assert_nil @a.exceptions
  end

  # TODO: lots of filename edgecases for find_files_to_test
  def test_find_files_to_test
    @a.last_mtime = Time.at(0)
    assert @a.find_files_to_test(@files)

    @a.last_mtime = @files.values.sort.last + 1
    deny @a.find_files_to_test(@files)
  end

  def test_find_files_to_test_dunno
    empty = {}

    files = { "fooby.rb" => Time.at(42) }
    assert @a.find_files_to_test(files)  # we find fooby,
    assert_equal empty, @a.files_to_test # but it isn't something to test
    assert_equal "No tests matched fooby.rb\n", @a.output.string
  end

  def test_find_files_to_test_lib
    # ensure we add test_blah.rb when blah.rb updates
    util_find_files_to_test(@impl, @test => [])
  end

  def test_find_files_to_test_no_change
    empty = {}

    # ensure world is virginal
    assert_equal empty, @a.files_to_test

    # ensure we do nothing when nothing changes...
    files = { @impl => @files[@impl] } # same time
    deny @a.find_files_to_test(files)
    assert_equal empty, @a.files_to_test
    assert_equal "", @a.output.string

    files = { @impl => @files[@impl] } # same time
    assert(! @a.find_files_to_test(files))
    assert_equal empty, @a.files_to_test
    assert_equal "", @a.output.string
  end

  def test_find_files_to_test_test
    # ensure we add test_blah.rb when test_blah.rb itself updates
    util_find_files_to_test(@test, @test => [])
  end

  def test_reorder_alpha
    @a.order = :alpha
    expected = @files.sort

    assert_equal expected, @a.reorder(@files)
  end

  def test_reorder_reverse
    @a.order = :reverse
    expected = @files.sort.reverse

    assert_equal expected, @a.reorder(@files)
  end

  def test_reorder_random
    @a.order = :random

    srand 42
    expected, size = @files.dup, @files.size
    expected = expected.sort_by { rand(size) }

    srand 42
    result = @a.reorder(@files.dup)

    assert_equal expected, result
  end

  def test_reorder_natural
    srand 42

    @files['lib/untested_blah.rb'] = Time.at(2)
    @a.find_order = @files.keys.sort_by { rand }

    @a.order = :natural
    expected = @a.find_order.map { |f| [f, @files[f]] }

    assert_equal expected, @a.reorder(@files)
  end

  def test_handle_results
    @a.files_to_test.clear
    @files.clear
    @files[@impl] = Time.at(1)
    @files[@test] = Time.at(2)

    @a.find_order = @files.keys.sort

    empty = {}
    assert_equal empty, @a.files_to_test, "must start empty"

    s1 = "Loaded suite -e
Started
............
Finished in 0.001655 seconds.

12 tests, 18 assertions, 0 failures, 0 errors
"

    @a.handle_results(s1)
    assert_equal empty, @a.files_to_test, "must stay empty"

    s2 = "
  1) Failure:
test_fail1(#{@test_class}) [#{@test}:59]:
  2) Failure:
test_fail2(#{@test_class}) [#{@test}:60]:
  3) Error:
test_error1(#{@test_class}):
  3) Error:
test_error2(#{@test_class}):

12 tests, 18 assertions, 2 failures, 2 errors
"

    @a.handle_results(s2)
    expected = { @test => %w( test_fail1 test_fail2 test_error1 test_error2 ) }
    assert_equal expected, @a.files_to_test
    assert @a.tainted

    @a.handle_results(s1)
    assert_equal empty, @a.files_to_test

    s3 = '
/opt/bin/ruby -I.:lib:test -rubygems -e "%w[test/unit #{@test}].each { |f| require f }" | unit_diff -u
-e:1:in `require\': ./#{@test}:23: parse error, unexpected tIDENTIFIER, expecting \'}\' (SyntaxError)
    settings_fields.each {|e| assert_equal e, version.send e.intern}
                                                            ^   from -e:1
        from -e:1:in `each\'
        from -e:1
'
    @a.files_to_test[@test] = Time.at(42)
    @files[@test] = []
    expected = { @test => Time.at(42) }
    assert_equal expected, @a.files_to_test
    @a.handle_results(s3)
    assert_equal expected, @a.files_to_test
    assert @a.tainted
    @a.tainted = false

    @a.handle_results(s1)
    assert_equal empty, @a.files_to_test
    deny @a.tainted
  end

  def test_hook_overlap_returning_false
    util_reset_hooks_returning false

    @a.hook :blah

    assert @a.instance_variable_get(:@blah1), "Hook1 should work on blah"
    assert @a.instance_variable_get(:@blah2), "Hook2 should work on blah"
    assert @a.instance_variable_get(:@blah3), "Hook3 should work on blah"
  end

  def test_hook_overlap_returning_true
    util_reset_hooks_returning true

    @a.hook :blah

    assert @a.instance_variable_get(:@blah1), "Hook1 should work on blah"
    deny @a.instance_variable_get(:@blah2), "Hook2 should NOT work on blah"
    deny @a.instance_variable_get(:@blah3), "Hook3 should NOT work on blah"
  end

  def test_hook_response
    Autotest.clear_hooks
    deny @a.hook(:blah)

    Autotest.add_hook(:blah) { false }
    deny @a.hook(:blah)

    Autotest.add_hook(:blah) { false }
    deny @a.hook(:blah)

    Autotest.add_hook(:blah) { true  }
    assert @a.hook(:blah)
  end

  def test_make_test_cmd
    f = {
      @test => [],
      'test/test_fooby.rb' => [ 'test_something1', 'test_something2' ]
    }

    expected = [ "#{RUBY} -I.:lib:test -rubygems -e \"%w[test/unit #{@test}].each { |f| require f }\" | unit_diff -u",
                 "#{RUBY} -I.:lib:test -rubygems test/test_fooby.rb -n \"/^(test_something1|test_something2)$/\" | unit_diff -u" ].join("; ")

    result = @a.make_test_cmd f
    assert_equal expected, result
  end

  def test_path_to_classname
    # non-rails
    util_path_to_classname 'TestBlah', 'test/test_blah.rb'
    util_path_to_classname 'TestOuter::TestInner', 'test/outer/test_inner.rb'
    util_path_to_classname 'TestRuby2Ruby', 'test/test_ruby2ruby.rb'
  end

  def test_remove_exception
    test_add_exception
    current = util_exceptions
    @a.remove_exception 'blah'

    actual = util_exceptions
    expect = current - ["blah"]

    assert_equal expect, actual
  end

  def test_remove_mapping
    current = util_mappings
    @a.remove_mapping(/^lib\/.*\.rb$/)

    actual = util_mappings
    expect = current - [/^lib\/.*\.rb$/]

    assert_equal expect, actual
  end

  def test_test_files_for
    assert_equal [@test], @a.test_files_for(@impl)
    assert_equal [@test], @a.test_files_for(@test)

    assert_equal [], @a.test_files_for('test/test_unknown.rb')
    assert_equal [], @a.test_files_for('lib/unknown.rb')
    assert_equal [], @a.test_files_for('unknown.rb')
    assert_equal [], @a.test_files_for('test_unknown.rb')
  end

  def test_testlib
    assert_equal "test/unit", @a.testlib

    @a.testlib = "MONKEY"
    assert_equal "MONKEY", @a.testlib

    f = { @test => [], "test/test_fooby.rb" => %w(first second) }
    assert_match @a.testlib, @a.make_test_cmd(f)
  end

  def util_exceptions
    @a.exception_list.sort_by { |r| r.to_s }
  end

  def util_find_files_to_test(f, expected)
    t = @a.last_mtime
    files = { f => t + 1 }

    assert @a.find_files_to_test(files)
    assert_equal expected, @a.files_to_test
    assert_equal t, @a.last_mtime
    assert_equal "", @a.output.string
  end

  def util_mappings
    @a.test_mappings.map { |k,v| k }
  end

  def util_path_to_classname(e,i)
    assert_equal e, @a.path_to_classname(i)
  end

  def util_reset_hooks_returning val
    Autotest.clear_hooks

    @a.instance_variable_set :@blah1, false
    @a.instance_variable_set :@blah2, false
    @a.instance_variable_set :@blah3, false

    Autotest.add_hook(:blah) do |at|
      at.instance_variable_set :@blah1, true
      val
    end

    Autotest.add_hook(:blah) do |at|
      at.instance_variable_set :@blah2, true
      val
    end

    Autotest.add_hook(:blah) do |at|
      at.instance_variable_set :@blah3, true
      val
    end
  end
end
