class UnitTestSetup
  def initialize
    @name = "ActionMailer"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'test-unit', "= 2.0.5"
    gem 'actionmailer', "= 2.3.5"
    require 'action_mailer/version'
  end

  def gather_files
    gather_rails_files
  end
  
  def sanity
    # Do some sanity checks
    sanity_size(5)
    abort("Did not find some expected files...") unless File.exist?(@root_dir + "/test/mail_helper_test.rb")
    sanity_version('2.3.5', ActionMailer::VERSION::STRING)
  end

  def disable_mri_failures
    disable QuotingTest,
      # RuntimeError: could not run test in sandbox
      :test_quote_multibyte_chars
  end
  
end
