class UnitTestSetup
  def initialize
    @name = "ActionMailer"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'test-unit', "= #{TestUnitVersion}"
    gem 'actionmailer', "= #{RailsVersion}"
    require 'action_mailer/version'
  end

  def gather_files
    gather_rails_files
  end
  
  def sanity
    # Do some sanity checks
    sanity_size(5)
    sanity_version(RailsVersion, ActionMailer::VERSION::STRING)
  end  
end
