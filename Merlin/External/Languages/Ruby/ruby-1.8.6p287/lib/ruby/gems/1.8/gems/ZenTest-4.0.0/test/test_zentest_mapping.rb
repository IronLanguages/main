$TESTING = true

require 'rubygems'
require 'minitest/autorun'

require 'zentest_mapping' unless defined? $ZENTEST

class Dummy
  attr_accessor :inherited_methods
  include ZenTestMapping
end

class TestZentestMapping < MiniTest::Unit::TestCase
  def setup
    @tester = Dummy.new
  end

  def util_simple_setup
    klasses = {
      "Something" => {
        "method1"      => true,
        "method1!"     => true,
        "method1="     => true,
        "method1?"     => true,
        "attrib"       => true,
        "attrib="      => true,
        "equal?"       => true,
        "self.method3" => true,
        "self.[]"      => true,
      },
    }
    test_klasses = {
      "TestSomething" => {
        "test_class_method4" => true,
        "test_method2"       => true,
        "setup"              => true,
        "teardown"           => true,
        "test_class_index"   => true,
      },
    }
    @tester.inherited_methods = test_klasses.merge(klasses)
    @generated_code = "
require 'test/unit' unless defined? $ZENTEST and $ZENTEST

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

  def test_normal_to_test
    self.util_simple_setup
    assert_equal("test_method1",        @tester.normal_to_test("method1"))
    assert_equal("test_method1_bang",   @tester.normal_to_test("method1!"))
    assert_equal("test_method1_eh",     @tester.normal_to_test("method1?"))
    assert_equal("test_method1_equals", @tester.normal_to_test("method1="))
  end

  def test_normal_to_test_cls
    self.util_simple_setup
    assert_equal("test_class_method1",
                 @tester.normal_to_test("self.method1"))
    assert_equal("test_class_method1_bang",
                 @tester.normal_to_test("self.method1!"))
    assert_equal("test_class_method1_eh",
                 @tester.normal_to_test("self.method1?"))
    assert_equal("test_class_method1_equals",
                 @tester.normal_to_test("self.method1="))
  end

  def test_normal_to_test_operators
    self.util_simple_setup
    assert_equal("test_and",         @tester.normal_to_test("&"))
    assert_equal("test_bang",        @tester.normal_to_test("!"))
    assert_equal("test_carat",       @tester.normal_to_test("^"))
    assert_equal("test_div",         @tester.normal_to_test("/"))
    assert_equal("test_equalstilde", @tester.normal_to_test("=~"))
    assert_equal("test_minus",       @tester.normal_to_test("-"))
    assert_equal("test_or",          @tester.normal_to_test("|"))
    assert_equal("test_percent",     @tester.normal_to_test("%"))
    assert_equal("test_plus",        @tester.normal_to_test("+"))
    assert_equal("test_tilde",       @tester.normal_to_test("~"))
  end

  def test_normal_to_test_overlap
    self.util_simple_setup
    assert_equal("test_equals2",       @tester.normal_to_test("=="))
    assert_equal("test_equals3",       @tester.normal_to_test("==="))
    assert_equal("test_ge",            @tester.normal_to_test(">="))
    assert_equal("test_gt",            @tester.normal_to_test(">"))
    assert_equal("test_gt2",           @tester.normal_to_test(">>"))
    assert_equal("test_index",         @tester.normal_to_test("[]"))
    assert_equal("test_index_equals",  @tester.normal_to_test("[]="))
    assert_equal("test_lt",            @tester.normal_to_test("<"))
    assert_equal("test_lt2",           @tester.normal_to_test("<\<"))
    assert_equal("test_lte",           @tester.normal_to_test("<="))
    assert_equal("test_method",        @tester.normal_to_test("method"))
    assert_equal("test_method_equals", @tester.normal_to_test("method="))
    assert_equal("test_spaceship",     @tester.normal_to_test("<=>"))
    assert_equal("test_times",         @tester.normal_to_test("*"))
    assert_equal("test_times2",        @tester.normal_to_test("**"))
    assert_equal("test_unary_minus",   @tester.normal_to_test("-@"))
    assert_equal("test_unary_plus",    @tester.normal_to_test("+@"))
    assert_equal("test_class_index",   @tester.normal_to_test("self.[]"))
  end

  def test_test_to_normal
    self.util_simple_setup
    assert_equal("method1!",
                 @tester.test_to_normal("test_method1_bang", "Something"))
    assert_equal("method1",
                 @tester.test_to_normal("test_method1", "Something"))
    assert_equal("method1=",
                 @tester.test_to_normal("test_method1_equals", "Something"))
    assert_equal("method1?",
                 @tester.test_to_normal("test_method1_eh", "Something"))
  end

  def test_test_to_normal_cls
    self.util_simple_setup
    assert_equal("self.method1",
                 @tester.test_to_normal("test_class_method1"))
    assert_equal("self.method1!",
                 @tester.test_to_normal("test_class_method1_bang"))
    assert_equal("self.method1?",
                 @tester.test_to_normal("test_class_method1_eh"))
    assert_equal("self.method1=",
                 @tester.test_to_normal("test_class_method1_equals"))
    assert_equal("self.[]",
                 @tester.test_to_normal("test_class_index"))
  end

  def test_test_to_normal_extended
    self.util_simple_setup
    assert_equal("equal?",
                 @tester.test_to_normal("test_equal_eh_extension",
                                        "Something"))
    assert_equal("equal?",
                 @tester.test_to_normal("test_equal_eh_extension_again",
                                        "Something"))
    assert_equal("method1",
                 @tester.test_to_normal("test_method1_extension",
                                        "Something"))
    assert_equal("method1",
                 @tester.test_to_normal("test_method1_extension_again",
                                        "Something"))
  end

  def test_test_to_normal_mapped
    self.util_simple_setup
    assert_equal("*",   @tester.test_to_normal("test_times"))
    assert_equal("*",   @tester.test_to_normal("test_times_ext"))
    assert_equal("==",  @tester.test_to_normal("test_equals2"))
    assert_equal("==",  @tester.test_to_normal("test_equals2_ext"))
    assert_equal("===", @tester.test_to_normal("test_equals3"))
    assert_equal("===", @tester.test_to_normal("test_equals3_ext"))
    assert_equal("[]",  @tester.test_to_normal("test_index"))
    assert_equal("[]",  @tester.test_to_normal("test_index_ext"))
    assert_equal("[]=", @tester.test_to_normal("test_index_equals"))
    assert_equal("[]=", @tester.test_to_normal("test_index_equals_ext"))
  end

  def test_test_to_normal_operators
    self.util_simple_setup
    assert_equal("&",  @tester.test_to_normal("test_and"))
    assert_equal("!",  @tester.test_to_normal("test_bang"))
    assert_equal("^",  @tester.test_to_normal("test_carat"))
    assert_equal("/",  @tester.test_to_normal("test_div"))
    assert_equal("=~", @tester.test_to_normal("test_equalstilde"))
    assert_equal("-",  @tester.test_to_normal("test_minus"))
    assert_equal("|",  @tester.test_to_normal("test_or"))
    assert_equal("%",  @tester.test_to_normal("test_percent"))
    assert_equal("+",  @tester.test_to_normal("test_plus"))
    assert_equal("~",  @tester.test_to_normal("test_tilde"))
  end

  def test_test_to_normal_overlap
    self.util_simple_setup
    assert_equal("==",  @tester.test_to_normal("test_equals2"))
    assert_equal("===", @tester.test_to_normal("test_equals3"))
    assert_equal(">=",  @tester.test_to_normal("test_ge"))
    assert_equal(">",   @tester.test_to_normal("test_gt"))
    assert_equal(">>",  @tester.test_to_normal("test_gt2"))
    assert_equal("[]",  @tester.test_to_normal("test_index"))
    assert_equal("[]=", @tester.test_to_normal("test_index_equals"))
    assert_equal("<",   @tester.test_to_normal("test_lt"))
    assert_equal("<\<", @tester.test_to_normal("test_lt2"))
    assert_equal("<=",  @tester.test_to_normal("test_lte"))
    assert_equal("<=>", @tester.test_to_normal("test_spaceship"))
    assert_equal("*",   @tester.test_to_normal("test_times"))
    assert_equal("**",  @tester.test_to_normal("test_times2"))
    assert_equal("-@",  @tester.test_to_normal("test_unary_minus"))
    assert_equal("+@",  @tester.test_to_normal("test_unary_plus"))
  end

  def test_to_normal_subset
    self.util_simple_setup
    assert_equal("get_foo",  @tester.test_to_normal("test_get_foo"))
  end
end
