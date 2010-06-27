#!/usr/local/bin/ruby -w

abort "rubinius does not support features required by zentest" if
  defined?(RUBY_ENGINE) && RUBY_ENGINE =~ /rbx/

$TESTING = true

require 'rubygems'
require 'minitest/autorun'

# I do this so I can still run ZenTest against the tests and itself...
require 'zentest' unless defined? $ZENTEST

# These are just classes set up for quick testing.
# TODO: need to test a compound class name Mod::Cls

class Cls1                  # ZenTest SKIP
  def meth1; end
  def self.meth2; end
end

class TestCls1              # ZenTest SKIP
  def setup; end
  def teardown; end
  def test_meth1; end
  def test_meth2; assert(true, "something"); end
end

class SuperDuper            # ZenTest SKIP
  def self.cls_inherited; end
  def inherited; end
  def overridden; end
end

class LowlyOne < SuperDuper # ZenTest SKIP
  def self.cls_extended; end
  def overridden; end
  def extended; end
  def pretty_print; end
  def pretty_print_cycle; end
end

# This is the good case where there are no missing methods on either side.

class Blah0
  def missingtest; end
  def notmissing1; end
  def notmissing2; end

  # found by zentest on testcase1.rb
  def missingimpl; end
end

class TestBlah0
  def setup; end
  def teardown; end

  def test_notmissing1
    assert(true, "a test")
  end
  def test_notmissing2_ext1
    assert(true, "a test")
  end
  def test_notmissing2_ext2
    flunk("a failed test")
  end
  def test_missingimpl; end
  def test_missingtest; end
end

class Blah1
  def missingtest; end
  def notmissing1; end
  def notmissing2; end
end

class TestBlah1
  def test_notmissing1; end
  def test_notmissing2_ext1; end
  def test_notmissing2_ext2; end
  def test_missingimpl; Blah1.new.missingimpl; end
  def test_integration_blah1; end
  def test_integration_blah2; end
  def test_integration_blah3; end
end

module Something2
  class Blah2
    def missingtest; end
    def notmissing1; end
    def notmissing2; end
  end
end

module TestSomething2
  class TestBlah2
    def test_notmissing1; end
    def test_notmissing2_ext1; end
    def test_notmissing2_ext2; end
    def test_missingimpl; end
  end
end

# only test classes
class TestBlah3
  def test_missingimpl; end
end
# only regular classes
class Blah4
  def missingtest1; end
  def missingtest2; end
end

# subclassing a builtin class
class MyHash5 < Hash
  def []; end
  def missingtest1; end
end

# nested class
module MyModule6
  class MyClass6
    def []; end
    def missingtest1; end
  end
end

# nested class
module MyModule7; end # in 1.9+ you'll not need this
class MyModule7::MyClass7
  def []; end
  def missingtest1; end
end

class MyClass8
  def self.foobar; end
  def MyClass8.foobaz; end
end

class TestTrueClass; end

class TestZenTest < MiniTest::Unit::TestCase
  def setup
    @tester = ZenTest.new()
  end

  ############################################################
  # Utility Methods

  def util_simple_setup
    @tester.klasses = {
      "Something" =>
        {
        "method1" => true,
        "method1!" => true,
        "method1=" => true,
        "method1?" => true,
        "attrib" => true,
        "attrib=" => true,
        "equal?" => true,
        "self.method3" => true,
        "self.[]" => true,
      },
    }
    @tester.test_klasses = {
      "TestSomething" =>
        {
        "test_class_method4" => true,
        "test_method2" => true,
        "setup" => true,
        "teardown" => true,
        "test_class_index" => true,
      },
    }
    @tester.inherited_methods = @tester.test_klasses.merge(@tester.klasses)
    @generated_code = "
require 'test/unit/testcase'
require 'test/unit' if $0 == __FILE__

class Something
  def self.method4(*args)
    raise NotImplementedError, 'Need to write self.method4'
  end

  def method2(*args)
    raise NotImplementedError, 'Need to write method2'
  end
end

class TestSomething < Test::Unit::TestCase
  def test_class_method3
    raise NotImplementedError, 'Need to write test_class_method3'
  end

  def test_attrib
    raise NotImplementedError, 'Need to write test_attrib'
  end

  def test_attrib_equals
    raise NotImplementedError, 'Need to write test_attrib_equals'
  end

  def test_equal_eh
    raise NotImplementedError, 'Need to write test_equal_eh'
  end

  def test_method1
    raise NotImplementedError, 'Need to write test_method1'
  end

  def test_method1_bang
    raise NotImplementedError, 'Need to write test_method1_bang'
  end

  def test_method1_eh
    raise NotImplementedError, 'Need to write test_method1_eh'
  end

  def test_method1_equals
    raise NotImplementedError, 'Need to write test_method1_equals'
  end
end

# Number of errors detected: 10
"
  end

  ############################################################
  # Accessors & Adders:

  def test_initialize
    refute_nil(@tester, "Tester must be initialized")
    # TODO: should do more at this stage
  end

  ############################################################
  # Converters and Testers:

  def test_is_test_class
    # classes
    assert(@tester.is_test_class(TestCls1),
       "All test classes must start with Test")
    assert(!@tester.is_test_class(Cls1),
       "Classes not starting with Test must not be test classes")
    # strings
    assert(@tester.is_test_class("TestCls1"),
       "All test classes must start with Test")
    assert(@tester.is_test_class("TestMod::TestCls1"),
       "All test modules must start with test as well")
    assert(!@tester.is_test_class("Cls1"),
       "Classes not starting with Test must not be test classes")
    assert(!@tester.is_test_class("NotTestMod::TestCls1"),
       "Modules not starting with Test must not be test classes")
    assert(!@tester.is_test_class("NotTestMod::NotTestCls1"),
       "All names must start with Test to be test classes")
  end

  def test_is_test_class_reversed
    old = $r
    $r = true
    assert(@tester.is_test_class("Cls1Test"),
           "Reversed: All test classes must end with Test")
    assert(@tester.is_test_class("ModTest::Cls1Test"),
           "Reversed: All test classes must end with Test")
    assert(!@tester.is_test_class("TestMod::TestCls1"),
           "Reversed: All test classes must end with Test")
    $r = old
  end

  def test_convert_class_name

    assert_equal('Cls1', @tester.convert_class_name(TestCls1))
    assert_equal('TestCls1', @tester.convert_class_name(Cls1))

    assert_equal('Cls1', @tester.convert_class_name('TestCls1'))
    assert_equal('TestCls1', @tester.convert_class_name('Cls1'))

    assert_equal('TestModule::TestCls1',
         @tester.convert_class_name('Module::Cls1'))
    assert_equal('Module::Cls1',
         @tester.convert_class_name('TestModule::TestCls1'))
  end

  def test_convert_class_name_reversed
    old = $r
    $r = true

    assert_equal('Cls1', @tester.convert_class_name("Cls1Test"))
    assert_equal('Cls1Test', @tester.convert_class_name(Cls1))

    assert_equal('Cls1', @tester.convert_class_name('Cls1Test'))
    assert_equal('Cls1Test', @tester.convert_class_name('Cls1'))

    assert_equal('ModuleTest::Cls1Test',
         @tester.convert_class_name('Module::Cls1'))
    assert_equal('Module::Cls1',
         @tester.convert_class_name('ModuleTest::Cls1Test'))
    $r = old
  end

  ############################################################
  # Missing Classes and Methods:

  def test_missing_methods_empty
    missing = @tester.missing_methods
    assert_equal({}, missing)
  end

  def test_add_missing_method_normal
    @tester.add_missing_method("SomeClass", "some_method")
    missing = @tester.missing_methods
    assert_equal({"SomeClass" => { "some_method" => true } }, missing)
  end

  def test_add_missing_method_duplicates
    @tester.add_missing_method("SomeClass", "some_method")
    @tester.add_missing_method("SomeClass", "some_method")
    @tester.add_missing_method("SomeClass", "some_method")
    missing = @tester.missing_methods
    assert_equal({"SomeClass" => { "some_method" => true } }, missing)
  end

  def test_analyze_simple
    self.util_simple_setup

    @tester.analyze
    missing = @tester.missing_methods
    expected = {
      "Something" => {
        "method2" => true,
        "self.method4" => true,
      },
      "TestSomething" => {
        "test_class_method3" => true,
        "test_attrib" => true,
        "test_attrib_equals" => true,
        "test_equal_eh" => true,
        "test_method1" => true,
        "test_method1_eh"=>true,
        "test_method1_bang"=>true,
        "test_method1_equals"=>true,
      }
    }
    assert_equal(expected, missing)
  end

  def test_create_method
    list = @tester.create_method("  ", 1, "wobble")
    assert_equal(["  def wobble(*args)",
                  "    raise NotImplementedError, 'Need to write wobble'",
                  "  end"],list)
  end

  def test_methods_and_tests
    @tester.process_class("ZenTest")
    @tester.process_class("TestZenTest")
    m,t = @tester.methods_and_tests("ZenTest", "TestZenTest")
    assert(m.include?("methods_and_tests"))
    assert(t.include?("test_methods_and_tests"))
  end

  def test_generate_code_simple
    self.util_simple_setup

    @tester.analyze
    str = @tester.generate_code[1..-1].join("\n")
    exp = @generated_code

    assert_equal(exp, str)
  end

  def test_get_class_good
    assert_equal(Object, @tester.get_class("Object"))
  end

  def test_get_class_bad
    assert_nil(@tester.get_class("ZZZObject"))
  end

  def test_get_inherited_methods_for_subclass
    expect = { "inherited" => true, "overridden" => true }
    result = @tester.get_inherited_methods_for("LowlyOne", false)

    assert_equal(expect, result)
  end

  def test_get_inherited_methods_for_subclass_full
    expect = Object.instance_methods + %w( inherited overridden )
    expect.map! { |m| m.to_s }
    result = @tester.get_inherited_methods_for("LowlyOne", true)

    assert_equal(expect.sort, result.keys.sort)
  end

  def test_get_inherited_methods_for_superclass
    expect = { }
    result = @tester.get_inherited_methods_for("SuperDuper", false)

    assert_equal(expect.keys.sort, result.keys.sort)
  end

  def test_get_inherited_methods_for_superclass_full
    expect = Object.instance_methods.map { |m| m.to_s }
    result = @tester.get_inherited_methods_for("SuperDuper", true)

    assert_equal(expect.sort, result.keys.sort)
  end

  def test_get_methods_for_subclass
    expect = {
      "self.cls_extended" => true,
      "overridden" => true,
      "extended" => true
    }
    result = @tester.get_methods_for("LowlyOne")

    assert_equal(expect, result)
  end

  def test_get_methods_for_subclass_full
    expect = {
      "self.cls_inherited" => true,
      "self.cls_extended" => true,
      "overridden" => true,
      "extended" => true
   }
    result = @tester.get_methods_for("LowlyOne", true)

    assert_equal(expect, result)
  end

  def test_get_methods_for_superclass
    expect = {
      "self.cls_inherited" => true,
      "overridden" => true,
      "inherited" => true }
    result = @tester.get_methods_for("SuperDuper")

    assert_equal(expect, result)
  end

  def test_result
    self.util_simple_setup

    @tester.analyze
    @tester.generate_code
    str = @tester.result.split($/, 2).last
    exp = @generated_code

    assert_equal(exp, str)
  end

  def test_load_file
    # HACK raise NotImplementedError, 'Need to write test_load_file'
  end

  def test_scan_files
    # HACK raise NotImplementedError, 'Need to write test_scan_files'
  end

  def test_process_class
    assert_equal({}, @tester.klasses)
    assert_equal({}, @tester.test_klasses)
    assert_equal({}, @tester.inherited_methods["SuperDuper"])
    @tester.process_class("SuperDuper")
    assert_equal({"SuperDuper"=> {
                     "self.cls_inherited"=>true,
                     "inherited"=>true,
                     "overridden"=>true}},
                 @tester.klasses)
    assert_equal({}, @tester.test_klasses)
    assert_equal({}, @tester.inherited_methods["SuperDuper"])
  end

  def test_klasses_equals
    self.util_simple_setup
    assert_equal({"Something"=> {
                     "self.method3"=>true,
                     "equal?"=>true,
                     "attrib="=>true,
                     "self.[]"=>true,
                     "method1"=>true,
                     "method1="=>true,
                     "method1?"=>true,
                     "method1!"=>true,
                     "method1"=>true,
                     "attrib"=>true}}, @tester.klasses)
    @tester.klasses= {"whoopie" => {}}
    assert_equal({"whoopie"=> {}}, @tester.klasses)
  end

  # REFACTOR: this should probably be cleaned up and on ZenTest side
  def util_testcase(*klasses)
    zentest = ZenTest.new
    klasses.each do |klass|
      zentest.process_class(klass)
    end
    zentest.analyze
    zentest.generate_code
    return zentest.result.split("\n")[1..-1].join("\n")
  end

  def test_testcase0
    expected = '# Number of errors detected: 0'
    assert_equal expected, util_testcase("Blah0", "TestBlah0")
  end

  HEADER = "\nrequire 'test/unit/testcase'\nrequire 'test/unit' if $0 == __FILE__\n\n"

  def test_testcase1
    expected = "#{HEADER}class Blah1\n  def missingimpl(*args)\n    raise NotImplementedError, 'Need to write missingimpl'\n  end\nend\n\nclass TestBlah1 < Test::Unit::TestCase\n  def test_missingtest\n    raise NotImplementedError, 'Need to write test_missingtest'\n  end\nend\n\n# Number of errors detected: 2"

    assert_equal expected, util_testcase("Blah1", "TestBlah1")
  end

  def test_testcase2
    expected = "#{HEADER}module Something2\n  class Blah2\n    def missingimpl(*args)\n      raise NotImplementedError, 'Need to write missingimpl'\n    end\n  end\nend\n\nmodule TestSomething2\n  class TestBlah2 < Test::Unit::TestCase\n    def test_missingtest\n      raise NotImplementedError, 'Need to write test_missingtest'\n    end\n  end\nend\n\n# Number of errors detected: 2"

assert_equal expected, util_testcase("Something2::Blah2", "TestSomething2::TestBlah2")
  end

  def test_testcase3
    expected = "#{HEADER}class Blah3\n  def missingimpl(*args)\n    raise NotImplementedError, 'Need to write missingimpl'\n  end\nend\n\n# Number of errors detected: 1"

    assert_equal expected, util_testcase("TestBlah3")
  end

  def test_testcase4
    expected = "#{HEADER}class TestBlah4 < Test::Unit::TestCase\n  def test_missingtest1\n    raise NotImplementedError, 'Need to write test_missingtest1'\n  end\n\n  def test_missingtest2\n    raise NotImplementedError, 'Need to write test_missingtest2'\n  end\nend\n\n# Number of errors detected: 3"

    assert_equal expected, util_testcase("Blah4")
  end

  def test_testcase5
    expected = "#{HEADER}class TestMyHash5 < Test::Unit::TestCase\n  def test_index\n    raise NotImplementedError, 'Need to write test_index'\n  end\n\n  def test_missingtest1\n    raise NotImplementedError, 'Need to write test_missingtest1'\n  end\nend\n\n# Number of errors detected: 3"

    assert_equal expected, util_testcase("MyHash5")
  end

  def test_testcase6
    expected = "#{HEADER}module TestMyModule6\n  class TestMyClass6 < Test::Unit::TestCase\n    def test_index\n      raise NotImplementedError, 'Need to write test_index'\n    end\n\n    def test_missingtest1\n      raise NotImplementedError, 'Need to write test_missingtest1'\n    end\n  end\nend\n\n# Number of errors detected: 3"

    assert_equal expected, util_testcase("MyModule6::MyClass6")
  end

  def test_testcase7
    expected = "#{HEADER}module TestMyModule7\n  class TestMyClass7 < Test::Unit::TestCase\n    def test_index\n      raise NotImplementedError, 'Need to write test_index'\n    end\n\n    def test_missingtest1\n      raise NotImplementedError, 'Need to write test_missingtest1'\n    end\n  end\nend\n\n# Number of errors detected: 3"

    assert_equal expected, util_testcase("MyModule7::MyClass7")
  end

  def test_testcase8
    expected = "#{HEADER}class TestMyClass8 < Test::Unit::TestCase\n  def test_class_foobar\n    raise NotImplementedError, 'Need to write test_class_foobar'\n  end\n\n  def test_class_foobaz\n    raise NotImplementedError, 'Need to write test_class_foobaz'\n  end\nend\n\n# Number of errors detected: 3"

    assert_equal expected, util_testcase("MyClass8")
  end

  def test_testcase9
    # stupid YAML is breaking my tests. Enters via Test::Rails. order dependent.
    TrueClass.send :remove_method, :taguri, :taguri=, :to_yaml if defined? YAML

    expected = "#{HEADER}class TestTrueClass < Test::Unit::TestCase\n  def test_and\n    raise NotImplementedError, 'Need to write test_and'\n  end\n\n  def test_carat\n    raise NotImplementedError, 'Need to write test_carat'\n  end\n\n  def test_or\n    raise NotImplementedError, 'Need to write test_or'\n  end\n\n  def test_to_s\n    raise NotImplementedError, 'Need to write test_to_s'\n  end\nend\n\n# Number of errors detected: 4"

    assert_equal expected, util_testcase("TestTrueClass")
  end
end
