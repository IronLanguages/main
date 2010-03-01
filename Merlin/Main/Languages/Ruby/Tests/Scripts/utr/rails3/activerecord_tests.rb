require File.dirname(__FILE__) + '/19_helper'
class UnitTestSetup
  def initialize
    @name = "ActiveRecord"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'activerecord','~> 3.0'
  end

  def gather_files
    test_dir = File.expand_path("../External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-3.0.pre/activerecord", ENV["MERLIN_ROOT"])
    $LOAD_PATH << test_dir


    @all_test_files = Dir.glob("#{test_dir}/**/*_test.rb")
  end

  def sanity
    # Do some sanity checks
    sanity_size(80)
  end

  def disable_tests
  end
end
