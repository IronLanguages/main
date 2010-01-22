$: << File.join(File.dirname(__FILE__), "/utr")

class UnitTestRunner
  def self.ironruby?
    defined?(RUBY_ENGINE) and RUBY_ENGINE == "ironruby"
  end
  
  def parse_options(args)
    require "optparse"
    parser = OptionParser.new(args) do |opts|
      opts.banner = "USAGE: utr libname [-a] [-g] [-t TestClass#test_method]"
  
      opts.separator ""
  
      opts.on("-a", "--all", "Run all tests without ignoring the monkeypatched tests") do |a|
        @all = true
      end
      opts.on("-t", "--test TESTNAME", "Run specific test") do |t|
        @one_test = t
      end
      opts.on("-g", "--generate-tags", "Generate tags to disable failing tests") do |g|
        @generate_tags = true
      end
  
      opts.on_tail("-h", "--help", "Show this message") do |n|
        puts opts
        exit
      end
    end
        
    remaining_args = parser.parse!
    abort "Please specify the test suite to use" if remaining_args.empty?
    @lib = remaining_args.shift
    abort "Extra arguments: #{remaining_args}" if not remaining_args.empty?  
  end
    
  def initialize(args)
    parse_options(args)
    require "#{@lib}_tests"
    @setup = UnitTestSetup.new
  end

  def run
    @setup.require_files
    @setup.gather_files
    @setup.exclude_critical_files
    @setup.sanity
    @setup.require_tests

    if @one_test
      run_test
    else
      # Do not run tests with IronRuby that fail with MRI anyway
      @setup.disable_mri_failures if UnitTestRunner.ironruby?
    
      @setup.disable_critical_failures
      @setup.disable_unstable_tests      

      if @generate_tags
        require "generate_test-unit_tags"
      else
        @setup.disable_tests unless @all
      end
    end    
  end    
     
  class TestResultLogger
    def respond_to?(name, *args) true end
    def method_missing(name, *args)
      puts [name] + args
      if name == :add_error
        puts args[1].backtrace if args[1]
      end
    end
  end
  
  # Run just one test and exit
  def run_test()
    @one_test =~/(.*)#(test_.*)/
    class_name, test_name = $1, $2
    test_class = Object.const_get(class_name)

    test_class.new(test_name).run(TestResultLogger.new) {}

    # We do a hard exit. Otherwise all the tests will run as part of at_exit
    exit!(0)
  end

end        

class UnitTestSetup
  def require_files; end
  def gather_files; end
  def exclude_critical_files; end
  def disable_mri_failures; end
  def disable_critical_failures; end
  def disable_unstable_tests; end
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

  # Helpers for Rails tests
  
  def gather_rails_files
    rails_tests_dir = File.expand_path '..\External.LCA_RESTRICTED\Languages\IronRuby\tests\RailsTests-2.3.5', ENV['MERLIN_ROOT']
    @root_dir = File.expand_path @name, rails_tests_dir
    $LOAD_PATH << @root_dir + '/test'
    @all_test_files = Dir.glob("#{@root_dir}/test/**/*_test.rb").sort
  end
end         

UnitTestRunner.new(ARGV).run if $0 == __FILE__
