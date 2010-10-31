class UnitTestRunner
  def self.ironruby?
    defined?(RUBY_ENGINE) and RUBY_ENGINE == "ironruby"
  end
  
  def parse_options(args)
    require "optparse"
    pass_through_args = false
    parser = OptionParser.new(args) do |opts|
      opts.banner = "USAGE: utr libname [-a] [-l] [-g] [-i] [-t TestClass#test_method] [-s \"Spec Context#specify name\"] [-- <Test::Unit options>]"
  
      opts.separator ""
  
      opts.on("-a", "--all", "Run all tests without ignoring the monkeypatched tests") do |a|
        @all = true
      end
      opts.on('-l', '--list', "List test files being run") do |l|
        @list_test_files = true
      end
      opts.on('-s', '--spec SPECNAME', 'Run specific spec') do |t|
        @one_spec = t
      end
      opts.on("-t", "--test TESTNAME", "Run specific test") do |t|
        @one_test = t
      end
      opts.on("-g", "--generate-tags", "Generate tags to disable failing tests") do |g|
        @generate_tags = true
      end
      opts.on("-i", "--generate-incremental", "Generate tags to disable *additional* failing tests") do |g|
        @generate_tags = true
        @generate_incremental = true
      end
  
      opts.on_tail("-h", "--help", "Show this message") do |n|
        puts opts
        puts
        puts "Test::Unit help:"
        require "rubygems"
        require "test/unit"
        require "test/unit/autorunner"
        $0 = ""
        Test::Unit::AutoRunner.new(true).process_args(["-h"])
        exit
      end
      
      opts.on_tail("--", "Pass the remaining options to Test::Unit") do |n|
        pass_through_args = true
        opts.terminate
      end
    end

    remaining_args = parser.parse!
    abort "Please specify the test suite to use" if remaining_args.empty?
    test_suite = remaining_args.shift
    @lib = File.expand_path("utr/#{test_suite}_tests.rb", File.dirname(__FILE__)) 
    abort "Extra arguments: #{remaining_args}" if not remaining_args.empty? and not pass_through_args
  end
    
  def initialize(args)
    parse_options(args)
    require @lib
    @setup = UnitTestSetup.new
  end

  def run
    @setup.require_files
    @setup.gather_files
    @setup.exclude_critical_files
    @setup.sanity
    @setup.require_tests

    @setup.disable_mri_failures unless @all

    if UnitTestRunner.ironruby?
      @setup.disable_critical_failures
      @setup.disable_unstable_tests      

      if !@generate_tags
        @setup.disable_tests unless @all
      else
        @setup.disable_tests if @generate_incremental
        require "generate_test-unit_tags.rb"
        TagGenerator.test_file = @lib
        TagGenerator.initial_tag_generation = !@generate_incremental
      end
    else
      @setup.disable_mri_only_failures unless @all
    end

    if @one_test
      run_test
    elsif @one_spec
      run_spec
    else
      at_exit { 
        puts "Disabled #{@setup.disabled || 0} tests"
        p @setup.all_test_files if @list_test_files
      }
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
 
  def run_spec
    @one_spec =~ /(.*)#(.*)/
    context, specify = $1, $2
    klass = @setup.get_context_class(context)
    specify = '.*' if specify == '*' # wildcard allow for all specifies to be run in a context
    @setup.get_specify_methods(context, specify).each do |method|
      klass.new(method).run(TestResultLogger.new){}
    end
    exit!(0)
  end

  # Run just one test and exit
  def run_test()
    @one_test =~/(.*)#(test_.*)/
    class_name, test_name = $1, $2
    # Use class_eval instead of const_get in case of nested names like TestSuite::TestCase
    test_class = Object.class_eval(class_name)

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
  def disable_mri_only_failures; end
  def disable_critical_failures; end
  def disable_unstable_tests; end
  def disable_tests;end
  def sanity; end
  
  attr :disabled
  attr_reader :all_test_files


  def require_tests
    # Note that the tests are registered using Kernel#at_exit, and will run during shutdown
    # The "require" statement just registers the tests for being run later...
    @all_test_files.each {|f| require f}
  end       
 
  def valid_context?(context)
    not Test::Spec::CONTEXTS[context].nil?
  end

  def get_context_class(context)
    if not valid_context? context
      raise "'#{context}' is an invalid context; pick from #{Test::Spec::CONTEXTS.keys.sort.inspect}"
    end
    Test::Spec::CONTEXTS[context].testcase
  end

  def get_specify_methods(context, specify)
    klass = get_context_class(context)
    klass.instance_methods.grep(/^test_spec \{#{context}\} .*? \[#{specify.gsub('(', '\(').gsub(')', '\)')}\]$/)
  end

  private   
  def disable(klass, *methods)
    @disabled ||= 0
    @disabled += methods.size
    klass.class_eval do
      methods.each do |method| 
        undef_method method.to_sym rescue puts "Could not undef #{klass}##{method}"
      end
      # If all the test methods have been removed, test/unit complains saying "No tests were specified."
      # So we monkey-patch the offending method to be a noop.
      klass.class_eval { def default_test() end }
    end
  end
  
  def disable_by_name names
    names.each do |name|
      /(.*)[(](.*)[)][:]?/ =~ name
      disable Object.const_get($2), $1
    end
  end
  
  def disable_spec(context, *specifies)
    @disabled ||= 0
    @disabled += specifies.size
    specify_methods = specifies.inject([]) do |ss, specify|
      ss << get_specify_methods(context, specify)
    end.flatten.uniq
    get_context_class(context).instance_eval do |klass|
      specify_methods.each do |method|
        undef_method method.to_sym rescue puts "Could not undef #{klass}##{method}"
      end
      def default_test() end
    end
  end
            
  def sanity_size(size)
    abort("Did not find enough #{@name} tests files... \nFound #{@all_test_files.size}, expected #{size}.\n") unless @all_test_files.size >= size
  end

  def sanity_version(expected, actual)
    abort("Loaded the wrong version #{actual} of #{@name} instead of the expected #{expected}...") unless actual == expected
  end

  # Helpers for Rails tests
  
  RailsVersion = "3.0.0"
  TestUnitVersion = "2.1.1"
  SqlServerAdapterVersion = "3.0.0"
  
  RAILS_TEST_DIR = File.expand_path("Languages/Ruby/Tests/Libraries/Rails-#{RailsVersion}", ENV['DLR_ROOT'])
  
  def gather_rails_files
    @root_dir = File.join(File.expand_path(@name, RAILS_TEST_DIR), "test")
    $LOAD_PATH << @root_dir
    @all_test_files = Dir.glob("#{@root_dir}/**/*_test.rb").sort    
  end
end         

UnitTestRunner.new(ARGV).run if $0 == __FILE__
