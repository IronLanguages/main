require File.dirname(__FILE__) + '/19_helper'
class UnitTestSetup
  def initialize
    @name = "ActionPack"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'actionpack', '~> 3.0'
  end

  def gather_files
    test_dir = File.expand_path("../External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/actionpack", ENV["MERLIN_ROOT"])
    $LOAD_PATH << test_dir


    @all_test_files = Dir.glob("#{test_dir}/**/*_test.rb")
  end

  def sanity
    # Do some sanity checks
    sanity_size(24)
  end

  def disable_tests
  end
end
