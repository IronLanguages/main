class UnitTestSetup
  def initialize
    @name = "TZInfo"
    super
  end
  
  def require_files
    require 'rubygems'
    require 'tzinfo'
  end

  def gather_files
    test_dir = File.expand_path("Languages/Ruby/Tests/Libraries/TZInfoTests", ENV["DLR_ROOT"])
    $LOAD_PATH << test_dir


    @all_test_files = Dir.glob("#{test_dir}/tc_*.rb")
  end

  def sanity
    # Do some sanity checks
    sanity_size(24)
  end

  def disable_tests
  end
end
