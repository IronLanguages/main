
class UnitTestSetup
  Version = '0.8.7'
  
  def initialize
    @name = "Rake"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'rake', Version
    require 'rake'
  end

  def gather_files
    @rake_tests_dir = File.expand_path("Languages/Ruby/Tests/Libraries/Rake-#{Version}", ENV["DLR_ROOT"])
    @all_test_files = Dir.glob("#{@rake_tests_dir}/test/test_*.rb") + Dir.glob("#{@rake_tests_dir}/test/contrib/test_*.rb")
  end

  def sanity
    sanity_version(Version, RAKEVERSION)

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
    disable_by_name %w{
        test_finding_rakefile(TestApplication)
        test_help(TestApplicationOptions)
        test_file_utils_can_use_filelists(TestFileList)
        test_string_ext(TestFileList)
        test_ln(TestFileUtils)
        test_ruby_with_a_single_string_argument(TestFileUtils)
        test_sh_with_a_single_string_argument(TestFileUtils)
        test_create(TestPackageTask)
        test_x_returns_extension(TestPathMap)
        test_explode(TestPathMapExplode)
        test_each_dir_parent(TestRake)
        test_name_lookup_with_implicit_file_tasks(TestTaskManager)
        test_both_pattern_and_test_files(TestTestTask)
        test_pattern(TestTestTask)
        test_X_returns_everything_but_extension(TestPathMap)
        test_running_multitasks(TestMultiTask)
        test_array_comparisons(TestFileList)
    }
  end
end
