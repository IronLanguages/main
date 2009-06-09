#--
#
# Author:: Nathaniel Talbott.
# Copyright:: Copyright (c) 2000-2003 Nathaniel Talbott. All rights reserved.
# License:: Ruby license.

require 'test/unit/attribute'
require 'test/unit/fixture'
require 'test/unit/exceptionhandler'
require 'test/unit/assertions'
require 'test/unit/failure'
require 'test/unit/error'
require 'test/unit/pending'
require 'test/unit/omission'
require 'test/unit/notification'
require 'test/unit/priority'
require 'test/unit/testsuite'
require 'test/unit/assertionfailederror'
require 'test/unit/util/backtracefilter'

module Test
  module Unit

    # Ties everything together. If you subclass and add your own
    # test methods, it takes care of making them into tests and
    # wrapping those tests into a suite. It also does the
    # nitty-gritty of actually running an individual test and
    # collecting its results into a Test::Unit::TestResult object.
    #
    # You can run two hooks before/after a TestCase run.
    #
    # Example:
    #   class TestMyClass < Test::Unit::TestCase
    #     class << self
    #       def startup
    #         ...
    #       end
    #
    #       def shutdown
    #         ...
    #       end
    #     end
    #
    #     def setup
    #       ...
    #     end
    #
    #     def teardown
    #       ...
    #     end
    #
    #     def test_my_method1
    #       ...
    #     end
    #
    #     def test_my_method2
    #       ...
    #     end
    #   end
    #
    # Here is a call order:
    #  * startup
    #  * setup
    #  * test_my_method1
    #  * teardown
    #  * setup
    #  * test_my_method2
    #  * teardown
    #  * shutdown
    class TestCase
      include Attribute
      include Fixture
      include ExceptionHandler
      include ErrorHandler
      include FailureHandler
      include TestCasePendingSupport
      include TestCaseOmissionSupport
      include TestCaseNotificationSupport
      include Priority
      include Assertions
      include Util::BacktraceFilter

      STARTED = name + "::STARTED"
      FINISHED = name + "::FINISHED"

      DESCENDANTS = []

      class << self
        def inherited(sub_class)
          DESCENDANTS << sub_class
        end

        # Rolls up all of the test* methods in the fixture into
        # one suite, creating a new instance of the fixture for
        # each method.
        def suite
          method_names = public_instance_methods(true).collect {|name| name.to_s}
          tests = method_names.delete_if {|method_name| method_name !~ /^test./}
          suite = TestSuite.new(name, self)
          tests.sort.each do |test|
            catch(:invalid_test) do
              suite << new(test)
            end
          end
          if suite.empty?
            catch(:invalid_test) do
              suite << new("default_test")
            end
          end
          suite
        end

        # Called before every test case runs. Can be used
        # to set up fixture information used in test case
        # scope.
        #
        # Here is an example test case:
        #   class TestMyClass < Test::Unit::TestCase
        #     class << self
        #       def startup
        #         ...
        #       end
        #     end
        #
        #     def setup
        #       ...
        #     end
        #
        #     def test_my_class1
        #       ...
        #     end
        #
        #     def test_my_class2
        #       ...
        #     end
        #   end
        #
        # Here is a call order:
        #   * startup
        #   * setup
        #   * test_my_class1 (or test_my_class2)
        #   * setup
        #   * test_my_class2 (or test_my_class1)
        #
        # Note that you should not assume test order. Tests
        # should be worked in any order.
        def startup
        end

        # Called after every test case runs. Can be used to tear
        # down fixture information used in test case scope.
        #
        # Here is an example test case:
        #   class TestMyClass < Test::Unit::TestCase
        #     class << self
        #       def shutdown
        #         ...
        #       end
        #     end
        #
        #     def teardown
        #       ...
        #     end
        #
        #     def test_my_class1
        #       ...
        #     end
        #
        #     def test_my_class2
        #       ...
        #     end
        #   end
        #
        # Here is a call order:
        #   * test_my_class1 (or test_my_class2)
        #   * teardown
        #   * test_my_class2 (or test_my_class1)
        #   * teardown
        #   * shutdown
        #
        # Note that you should not assume test order. Tests
        # should be worked in any order.
        def shutdown
        end
      end

      attr_reader :method_name

      # Creates a new instance of the fixture for running the
      # test represented by test_method_name.
      def initialize(test_method_name)
        throw :invalid_test unless respond_to?(test_method_name)
        test_method = method(test_method_name)
        throw :invalid_test if test_method.arity > 0
        if test_method.respond_to?(:owner)
          if test_method.owner.class != Module and
              self.class != test_method.owner
            throw :invalid_test
          end
        end
        @method_name = test_method_name
        @test_passed = true
        @interrupted = false
      end

      # Runs the individual test method represented by this
      # instance of the fixture, collecting statistics, failures
      # and errors in result.
      def run(result)
        begin
          @_result = result
          yield(STARTED, name)
          begin
            run_setup
            run_test
          rescue Exception
            @interrupted = true
            raise unless handle_exception($!)
          ensure
            begin
              run_teardown
            rescue Exception
              raise unless handle_exception($!)
            end
          end
          result.add_run
          yield(FINISHED, name)
        ensure
          @_result = nil
        end
      end

      # Called before every test method runs. Can be used
      # to set up fixture information.
      #
      # You can add additional setup tasks by the following
      # code:
      #   class TestMyClass < Test::Unit::TestCase
      #     def setup
      #       ...
      #     end
      #
      #     setup
      #     def my_setup1
      #       ...
      #     end
      #
      #     setup
      #     def my_setup2
      #       ...
      #     end
      #
      #     def test_my_class
      #       ...
      #     end
      #   end
      #
      # Here is a call order:
      #   * setup
      #   * my_setup1
      #   * my_setup2
      #   * test_my_class
      def setup
      end

      # Called after every test method runs. Can be used to tear
      # down fixture information.
      #
      # You can add additional teardown tasks by the following
      # code:
      #   class TestMyClass < Test::Unit::TestCase
      #     def teardown
      #       ...
      #     end
      #
      #     teardown
      #     def my_teardown1
      #       ...
      #     end
      #
      #     teardown
      #     def my_teardown2
      #       ...
      #     end
      #
      #     def test_my_class
      #       ...
      #     end
      #   end
      #
      # Here is a call order:
      #   * test_my_class
      #   * my_teardown2
      #   * my_teardown1
      #   * teardown
      def teardown
      end
      
      def default_test
        flunk("No tests were specified")
      end

      def size
        1
      end

      # Returns a human-readable name for the specific test that
      # this instance of TestCase represents.
      def name
        "#{@method_name}(#{self.class.name})"
      end

      # Overridden to return #name.
      def to_s
        name
      end

      # It's handy to be able to compare TestCase instances.
      def ==(other)
        return false unless(other.kind_of?(self.class))
        return false unless(@method_name == other.method_name)
        self.class == other.class
      end

      def interrupted?
        @interrupted
      end

      private
      def current_result
        @_result
      end

      def run_test
        __send__(@method_name)
      end

      def handle_exception(exception)
        self.class.exception_handlers.each do |handler|
          return true if send(handler, exception)
        end
        false
      end

      # Returns whether this individual test passed or
      # not. Primarily for use in teardown so that artifacts
      # can be left behind if the test fails.
      def passed?
        @test_passed
      end

      def problem_occurred
        @test_passed = false
      end

      def add_assertion
        current_result.add_assertion
      end
    end
  end
end
