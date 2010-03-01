class UnitTestSetup
  def initialize
    @name = "ActiveResource"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'test-unit', "= 2.0.5"
    gem 'activeresource', "= 2.3.5"
    require 'active_resource/version'
  end

  def gather_files
    gather_rails_files
  end

  def sanity
    # Do some sanity checks
    sanity_size(5)
    abort("Did not find some expected files...") unless File.exist?(@root_dir + "/test/connection_test.rb")
    sanity_version('2.3.5', ActiveResource::VERSION::STRING)
  end
end
