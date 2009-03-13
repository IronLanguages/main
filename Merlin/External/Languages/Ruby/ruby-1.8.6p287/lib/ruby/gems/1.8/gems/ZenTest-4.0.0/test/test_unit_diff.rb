#!/usr/local/bin/ruby -w

require 'rubygems'
require 'minitest/autorun'

require 'stringio'

$TESTING = true

require 'unit_diff'

class TestUnitDiff < MiniTest::Unit::TestCase

  def setup
    @diff = UnitDiff.new
  end

  def test_input
    header = "Loaded suite ./blah\nStarted\nFF\nFinished in 0.035332 seconds.\n\n"
    input = "#{header}  1) Failure:\ntest_test1(TestBlah) [./blah.rb:25]:\n<\"line1\\nline2\\nline3\\n\"> expected but was\n<\"line4\\nline5\\nline6\\n\">.\n\n  2) Failure:\ntest_test2(TestBlah) [./blah.rb:29]:\n<\"line1\"> expected but was\n<\"line2\\nline3\\n\\n\">.\n\n2 tests, 2 assertions, 2 failures, 0 errors\n"

    # TODO: I think I'd like a separate footer array as well
    expected = [[["  1) Failure:\n", "test_test1(TestBlah) [./blah.rb:25]:\n", "<\"line1\\nline2\\nline3\\n\"> expected but was\n", "<\"line4\\nline5\\nline6\\n\">.\n"],
                 ["  2) Failure:\n", "test_test2(TestBlah) [./blah.rb:29]:\n", "<\"line1\"> expected but was\n", "<\"line2\\nline3\\n\\n\">.\n"]],
                ["\n", "2 tests, 2 assertions, 2 failures, 0 errors\n"]]

    util_unit_diff(header, input, expected, :parse_input)
  end

  def test_input_miniunit
    header = "Loaded suite -e\nStarted\nF\nFinished in 0.035332 seconds.\n\n"
    input = "#{header}  1) Failure:
test_blah(TestBlah) [./blah.rb:25]:
Expected ['a', 'b', 'c'], not ['a', 'c', 'b'].

1 tests, 1 assertions, 1 failures, 0 errors
"

    expected = [[["  1) Failure:\n",
                  "test_blah(TestBlah) [./blah.rb:25]:\n",
                  "Expected ['a', 'b', 'c'], not ['a', 'c', 'b'].\n"]],
                ["\n", "1 tests, 1 assertions, 1 failures, 0 errors\n"]]

    util_unit_diff(header, input, expected, :parse_input)
  end

  def test_input_mspec
    header = <<-HEADER
Started
.......F
Finished in 0.1 seconds

    HEADER

    failure = <<-FAILURE
1)
The unless expression should fail FAILED
Expected nil to equal "baz":
    FAILURE

    backtrace = <<-BACKTRACE
      PositiveExpectation#== at spec/mspec.rb:217
          main.__script__ {} at spec/language/unless_spec.rb:49
                   Proc#call at kernel/core/proc.rb:127
               SpecRunner#it at spec/mspec.rb:368
                     main.it at spec/mspec.rb:412
          main.__script__ {} at spec/language/unless_spec.rb:48
                   Proc#call at kernel/core/proc.rb:127
         SpecRunner#describe at spec/mspec.rb:378
               main.describe at spec/mspec.rb:408
             main.__script__ at spec/language/unless_spec.rb:3
    CompiledMethod#as_script at kernel/bootstrap/primitives.rb:41
                   main.load at kernel/core/compile.rb:150
          main.__script__ {} at last_mspec.rb:11
               Array#each {} at kernel/core/array.rb:545
       Integer(Fixnum)#times at kernel/core/integer.rb:15
                  Array#each at kernel/core/array.rb:545
             main.__script__ at last_mspec.rb:16
    CompiledMethod#as_script at kernel/bootstrap/primitives.rb:41
                   main.load at kernel/core/compile.rb:150
             main.__script__ at kernel/loader.rb:145
    BACKTRACE

    footer = "\n8 examples, 1 failures\n"
    input = header + failure + backtrace + footer

    expected_backtrace = backtrace.split("\n").map {|l| "#{l}\n"}
    expected = [[["1)\n", "The unless expression should fail FAILED\n",
      "Expected nil to equal \"baz\":\n",
      *expected_backtrace]],
      ["\n", "8 examples, 1 failures\n"]]
    util_unit_diff(header, input, expected, :parse_input)
  end

  def test_input_mspec_multiline
    header = <<-HEADER
Started
.......F
Finished in 0.1 seconds

    HEADER

failure = <<-FAILURE
1)
Compiler compiles a case without an argument FAILED
Expected #<TestGenerator [[:push, :false], [:gif, #<Label 5>], [:push_literal, "foo"], [:string_dup], [:goto, #<Label 19>], [:set_label, #<Label 5>], [:push, :nil], [:gif, #<Label 10>], [:push_literal, "foo"], [:string_dup], [:goto, #<Label 19>], [:set_label, #<Label 10>], [:push, 2], [:push, 1], [:send, :==, 1, false], [:gif, #<Label 17>], [:push_literal, "bar"], [:string_dup], [:goto, #<Label 19>], [:set_label, #<Label 17>], [:push_literal, "baz"], [:string_dup], [:set_label, #<Label 19>]]
to equal #<TestGenerator [[:push, false], [:gif, #<Label 5>], [:push, "foo"], [:string_dup], [:goto, #<Label 6>], [:set_label, #<Label 5>], [:push, nil], [:set_label, #<Label 6>], [:pop], [:push, nil], [:gif, #<Label 12>], [:push, "foo"], [:string_dup], [:goto, #<Label 13>], [:set_label, #<Label 12>], [:push, nil], [:set_label, #<Label 13>], [:pop], [:push, 2], [:push, 1], [:send, :==, 1], [:gif, #<Label 21>], [:push, "bar"], [:string_dup], [:goto, #<Label 23>], [:set_label, #<Label 21>], [:push_literal, "baz"], [:string_dup], [:set_label, #<Label 23>], [:sret]]:
      FAILURE

backtrace = <<-BACKTRACE
      PositiveExpectation#== at spec/mspec.rb:216
                    main.gen at ./compiler2/spec/helper.rb:125
          main.__script__ {} at compiler2/spec/control_spec.rb:448
    BACKTRACE

    footer = "\n8 examples, 1 failures\n"
    input = header + failure + backtrace + footer

    expected_backtrace = backtrace.split("\n").map {|l| "#{l}\n"}
    expected_failure = failure.split("\n").map {|l| "#{l}\n"}
    expected = [[[*(expected_failure + expected_backtrace)]],
      ["\n", "8 examples, 1 failures\n"]]
    util_unit_diff(header, input, expected, :parse_input)
  end

  def test_unit_diff_empty # simulates broken pipe at the least
    input = ""
    expected = ""
    util_unit_diff("", "", "")
  end

  def test_parse_diff_angles
    input = ["  1) Failure:\n",
             "test_test1(TestBlah) [./blah.rb:25]:\n",
             "<\"<html>\"> expected but was\n",
             "<\"<body>\">.\n"
            ]

    expected = [["  1) Failure:\n", "test_test1(TestBlah) [./blah.rb:25]:\n"],
                ["<html>"],
                ["<body>"],
                []]

    assert_equal expected, @diff.parse_diff(input)
  end

  def test_parse_diff_miniunit
    input = ["  1) Failure:\n",
             "test_blah(TestBlah) [./blah.rb:25]:\n",
             "Expected ['a', 'b', 'c'], not ['a', 'c', 'b'].\n"]

    expected = [["  1) Failure:\n", "test_blah(TestBlah) [./blah.rb:25]:\n"],
                ["['a', 'b', 'c']"],
                ["['a', 'c', 'b']"],
                []]

    assert_equal expected, @diff.parse_diff(input)
  end

  def test_parse_diff_miniunit_multiline
    input = ["  1) Failure:\n",
             "test_blah(TestBlah) [./blah.rb:25]:\n",
             "Expected ['a',\n'b',\n'c'], not ['a',\n'c',\n'b'].\n"]

    expected = [["  1) Failure:\n", "test_blah(TestBlah) [./blah.rb:25]:\n"],
                ["['a',\n'b',\n'c']"],
                ["['a',\n'c',\n'b']"],
                []]

    assert_equal expected, @diff.parse_diff(input)
  end
  def test_parse_diff1
    input = ["  1) Failure:\n",
             "test_test1(TestBlah) [./blah.rb:25]:\n",
             "<\"line1\\nline2\\nline3\\n\"> expected but was\n",
             "<\"line4\\nline5\\nline6\\n\">.\n"
            ]

    expected = [["  1) Failure:\n", "test_test1(TestBlah) [./blah.rb:25]:\n"], ["line1\\nline2\\nline3\\n"], ["line4\\nline5\\nline6\\n"], []]

    assert_equal expected, @diff.parse_diff(input)
  end

  def test_parse_diff2
    input = ["  2) Failure:\n",
             "test_test2(TestBlah) [./blah.rb:29]:\n",
             "<\"line1\"> expected but was\n",
             "<\"line2\\nline3\\n\\n\">.\n"
            ]

    expected = [["  2) Failure:\n",
                 "test_test2(TestBlah) [./blah.rb:29]:\n"],
                ["line1"],
                ["line2\\nline3\\n\\n"],
                []
               ]

    assert_equal expected, @diff.parse_diff(input)
  end

  def test_parse_diff3
    input = [" 13) Failure:\n",
             "test_case_stmt(TestRubyToRubyC) [./r2ctestcase.rb:1198]:\n",
             "Unknown expected data.\n",
             "<false> is not true.\n"]

    expected = [[" 13) Failure:\n", "test_case_stmt(TestRubyToRubyC) [./r2ctestcase.rb:1198]:\n", "Unknown expected data.\n"], ["<false> is not true.\n"], nil, []]

    assert_equal expected, @diff.parse_diff(input)
  end

  def test_parse_diff_suspect_equals
    input = ["1) Failure:\n",
             "test_util_capture(AssertionsTest) [test/test_zentest_assertions.rb:53]:\n",
             "<\"out\"> expected but was\n",
             "<\"out\">.\n"]
    expected = [["1) Failure:\n",
                 "test_util_capture(AssertionsTest) [test/test_zentest_assertions.rb:53]:\n"],
                ["out"],
                ["out"], []]

    assert_equal expected, @diff.parse_diff(input)
  end

  def test_parse_diff_NOT_suspect_equals
    input = ["1) Failure:\n",
             "test_util_capture(AssertionsTest) [test/test_zentest_assertions.rb:53]:\n",
             "<\"out\"> expected but was\n",
             "<\"out\\n\">.\n"]
    expected = [["1) Failure:\n",
                 "test_util_capture(AssertionsTest) [test/test_zentest_assertions.rb:53]:\n"],
                ["out"],
                ["out\\n"], []]

    assert_equal expected, @diff.parse_diff(input)
  end

  def test_parse_diff_mspec
    input = ["1)\n", "The unless expression should fail FAILED\n",
      "Expected nil to equal \"baz\":\n",
      "    PositiveExpectation#== at spec/mspec.rb:217\n"]

    expected = [["1)\n", "The unless expression should fail FAILED\n"],
      ["nil"],
      ["\"baz\""],
      ["    PositiveExpectation#== at spec/mspec.rb:217"]]

    assert_equal expected, @diff.parse_diff(input)
  end

  def test_parse_diff_mspec_multiline
    input = ["1)\n", "The unless expression should fail FAILED\n",
    "Expected #<TestGenerator [[:push, :true],\n", "  [:dup]\n", "]\n",
    "to equal #<TestGenerator [[:pop],\n", "  [:dup]\n", "]:\n",
    "    PositiveExpectation#== at spec/mspec.rb:217\n"]

    expected = [["1)\n", "The unless expression should fail FAILED\n"],
      ["#<TestGenerator [[:push, :true],\n", "  [:dup]\n", "]"],
      ["#<TestGenerator [[:pop],\n", "  [:dup]\n", "]"],
      ["    PositiveExpectation#== at spec/mspec.rb:217"]]

    assert_equal expected, @diff.parse_diff(input)
  end

  def test_unit_diff_angles
    header = "Loaded suite ./blah\nStarted\nF\nFinished in 0.035332 seconds.\n\n"
    input = "#{header}  1) Failure:\ntest_test1(TestBlah) [./blah.rb:25]:\n<\"<html>\"> expected but was\n<\"<body>\">.\n\n1 tests, 1 assertions, 1 failures, 0 errors\n"
    expected = "1) Failure:\ntest_test1(TestBlah) [./blah.rb:25]:\n1c1\n< <html>\n---\n> <body>\n\n1 tests, 1 assertions, 1 failures, 0 errors"

    util_unit_diff(header, input, expected)
  end

  def test_unit_diff1
    header = "Loaded suite ./blah\nStarted\nF\nFinished in 0.035332 seconds.\n\n"
    input = "#{header}  1) Failure:\ntest_test1(TestBlah) [./blah.rb:25]:\n<\"line1\\nline2\\nline3\\n\"> expected but was\n<\"line4\\nline5\\nline6\\n\">.\n\n1 tests, 1 assertions, 1 failures, 0 errors\n"
    expected = "1) Failure:\ntest_test1(TestBlah) [./blah.rb:25]:\n1,3c1,3\n< line1\n< line2\n< line3\n---\n> line4\n> line5\n> line6\n\n1 tests, 1 assertions, 1 failures, 0 errors"

    util_unit_diff(header, input, expected)
  end

  def test_unit_diff2
    header = "Loaded suite ./blah\nStarted\nFF\nFinished in 0.035332 seconds.\n\n"
    input = "#{header}  1) Failure:\ntest_test1(TestBlah) [./blah.rb:25]:\n<\"line1\\nline2\\nline3\\n\"> expected but was\n<\"line4\\nline5\\nline6\\n\">.\n\n  2) Failure:\ntest_test2(TestBlah) [./blah.rb:29]:\n<\"line1\"> expected but was\n<\"line2\\nline3\\n\\n\">.\n\n2 tests, 2 assertions, 2 failures, 0 errors\n"
    expected = "1) Failure:\ntest_test1(TestBlah) [./blah.rb:25]:\n1,3c1,3\n< line1\n< line2\n< line3\n---\n> line4\n> line5\n> line6\n\n2) Failure:\ntest_test2(TestBlah) [./blah.rb:29]:\n1c1,4\n< line1\n---\n> line2\n> line3\n> \n> \n\n2 tests, 2 assertions, 2 failures, 0 errors"

    util_unit_diff(header, input, expected)
  end

  def test_unit_diff3
    header = ""
    input = " 13) Failure:\ntest_case_stmt(TestRubyToRubyC) [./r2ctestcase.rb:1198]:\nUnknown expected data.\n<false> is not true.\n"
    expected = "13) Failure:\ntest_case_stmt(TestRubyToRubyC) [./r2ctestcase.rb:1198]:\nUnknown expected data.\n<false> is not true."

    util_unit_diff(header, input, expected)
  end

  def test_unit_diff_suspect_equals
    header   = "Loaded suite ./blah\nStarted\n.............................................F............................................\nFinished in 0.834671 seconds.\n\n"
    footer   = "90 tests, 241 assertions, 1 failures, 0 errors"
    input    = "#{header}  1) Failure:\ntest_unit_diff_suspect_equals(TestUnitDiff) [./test/test_unit_diff.rb:122]:\n<\"out\"> expected but was\n<\"out\">.\n\n#{footer}"
    expected = "1) Failure:\ntest_unit_diff_suspect_equals(TestUnitDiff) [./test/test_unit_diff.rb:122]:\n[no difference--suspect ==]\n\n#{footer}"

    util_unit_diff(header, input, expected)
  end

  def test_unit_diff_NOT_suspect_equals
    header   = "Loaded suite ./blah\nStarted\n.\nFinished in 0.0 seconds.\n\n"
    input    = "#{header}  1) Failure:\ntest_blah(TestBlah)\n<\"out\"> expected but was\n<\"out\\n\">.\n\n1 tests, 1 assertions, 1 failures, 0 errors"
    expected = "1) Failure:\ntest_blah(TestBlah)\n1a2\n> \n\n1 tests, 1 assertions, 1 failures, 0 errors"

    util_unit_diff(header, input, expected)
  end

  def util_unit_diff(header, input, expected, msg=:unit_diff)
    output = StringIO.new("")
    actual = @diff.send(msg, StringIO.new(input), output)
    assert_equal header, output.string, "header output"
    assert_equal expected, actual
  end
end

