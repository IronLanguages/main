class UnitTestSetup
  def initialize
    @name = "Rake"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'rake', '= 0.8.4'
    require 'rake'
  end

  def gather_files
    @rake_tests_dir = File.expand_path("../External.LCA_RESTRICTED/Languages/IronRuby/tests/RakeTests", ENV["MERLIN_ROOT"])
    @all_test_files = Dir.glob("#{@rake_tests_dir}/test/test*.rb") + Dir.glob("#{@rake_tests_dir}/test/contrib/test*.rb")

    if RUBY_PLATFORM =~ /mswin/
      if true
        # The session gem uses "fork" by default, but can use popen3. However, it still does 
        # not work on Windows. It is currently very *nix oriented. For eg, it assumes that the
        # default shell is bash.
        puts "(Skipping functional tests on Windows)"
      else
        ENV['SESSION_USE_OPEN3'] = true
        require "open3"
        @all_test_files += Dir.glob("#{@rake_tests_dir}/test/fun*.rb")
      end
    else
      @all_test_files += Dir.glob("#{@rake_tests_dir}/test/fun*.rb")
    end
  end

  def sanity
    # Do some sanity checks
    sanity_size(25)
    abort("Did not find some expected files...") unless File.exist?(@rake_tests_dir + "/test/test_rake.rb")
    sanity_version('0.8.4', RAKEVERSION)

    # Some tests load data assuming the current folder
    Dir.chdir(@rake_tests_dir)
  end

  def disable_unstable_tests
    disable TestMultiTask,
      # monitor.rb uses Thread.critical= in a way that is not friendly with native threads
      # This can result in a deadlock
      :test_all_multitasks_wait_on_slow_prerequisites
  end
  
  def disable_tests
    # http://ironruby.codeplex.com/WorkItem/View.aspx?WorkItemId=1082
    # TypeError: can't convert Rake::EarlyTime into Time
    disable TestEarlyTime, :test_create    
    disable TestFileList, :test_cloned_items_stay_frozen
    #we're not exiting correctly from the ruby method. 
    disable TestFileUtils, :test_ruby
    # File.split("c:a") should return ["c:.", "a"], not ["c:", "a"]
    disable TestPathMapExplode, :test_explode
    # File.dirname("c:a") should be "c:.", not "c:"
    disable TestRake, :test_each_dir_parent
    # This failure does not happen with a later version (afer 0.8.4) of Rake tests with the same version (0.8.4) of the Rake gem.
    # So it might be a test issue
    disable TestTask, :test_investigation_output
  end
end
