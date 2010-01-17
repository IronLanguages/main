$: << File.join(File.dirname(__FILE__), "/utr")
class UnitTestRunner
  def initialize(args)
    if args.empty? || !args.grep(/(?:\/|\-)(?:h|\?)/).empty?
      puts "USAGE: utr libname [-all]"
      puts "passing in the -all parameter will run all tests without ignoring the monkeypatched tests."
      exit
    end
    
    if args.include?("-all")
      @all = true
      args.delete "-all"
    end
    
    @lib = args[0]
    require "#{@lib}_tests"
    @setup = UnitTestSetup.new
  end

  def run
    @setup.require_files
    @setup.gather_files
    @setup.sanity
    @setup.require_tests
    @setup.disable_tests unless @all
  end
end

class UnitTestSetup
  def require_files; end
  def gather_files; end
  def disable_tests;end
  def sanity; end

  def require_tests
    # Note that the tests are registered using Kernel#at_exit, and will run during shutdown
    # The "require" statement just registers the tests for being run later...
    @all_test_files.each {|f| require f}
  end
  
  private
  def disable(klass, *methods)
    klass.class_eval do
      def noop;end
      methods.each do |method| 
        alias_method method, :noop
      end
    end
  end

  def sanity_size(size)
    abort("Did not find enough #{@name} tests files... \nFound #{@all_test_files.size}, expected #{size}.\n") unless @all_test_files.size >= size
  end
  
  def sanity_version(expected, actual)
    abort("Loaded the wrong version #{actual} of #{@name} instead of the expected #{expected}...") unless actual == expected
  end
end

UnitTestRunner.new(ARGV).run if $0 == __FILE__
