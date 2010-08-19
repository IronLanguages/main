class UnitTestSetup
  def initialize
    @name = "ActiveResource"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'test-unit', "= #{TestUnitVersion}"
    gem 'activeresource', "= #{RailsVersion}"
    require 'active_resource/version'
  end

  def gather_files
    gather_rails_files
  end

  def sanity
    # Do some sanity checks
    sanity_size(5)
    sanity_version(RailsVersion, ActiveResource::VERSION::STRING)
  end
end
