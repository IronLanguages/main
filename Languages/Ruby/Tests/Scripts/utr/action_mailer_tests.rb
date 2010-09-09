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

  def disable_tests
    disable_by_name %w{
      test_extended_headers(ActionMailerTest)
      test_iso_charset(ActionMailerTest)
      test_multiple_utf8_recipients(ActionMailerTest)
      test_reply_to(ActionMailerTest)
      test_utf8_body_is_not_quoted(ActionMailerTest)
      test_implicit_multipart_with_attachments_creates_nested_parts(BaseTest)
      test_mail()_with_bcc,_cc,_content_type,_charset,_mime_version,_reply_to_and_date(BaseTest)
      test_assert_emails_too_few_sent(TestHelperMailerTest)
      test_assert_emails_too_many_sent(TestHelperMailerTest)
      test_assert_no_emails_failure(TestHelperMailerTest)
    }
  end
end
