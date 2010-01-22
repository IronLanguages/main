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
  
  def disable_tests
    disable ActionMailerTest, 
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_body_is_stored_as_an_ivar,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_cancelled_account,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_cc_bcc,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_custom_template,
      # Exception raised:
      # Class: <NoMethodError>
      # Message: <"undefined method `first' for nil:NilClass">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/actionmailer-2.3.5/lib/action_mailer/vendor/tmail-1.2.3/tmail/quoting.rb:99:in `convert_to'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/ge
      :test_decode_message_with_incorrect_charset,
      # Exception raised:
      # Class: <NoMethodError>
      # Message: <"undefined method `first' for nil:NilClass">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/actionmailer-2.3.5/lib/action_mailer/vendor/tmail-1.2.3/tmail/quoting.rb:99:in `convert_to'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/ge
      :test_decode_message_with_unknown_charset,
      # Exception raised:
      # Class: <NoMethodError>
      # Message: <"undefined method `first' for nil:NilClass">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/actionmailer-2.3.5/lib/action_mailer/vendor/tmail-1.2.3/tmail/quoting.rb:99:in `convert_to'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/ge
      :test_decode_part_without_content_type,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_delivery_logs_sent_mail,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_empty_header_values_omitted,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_extended_headers,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_headers_removed_on_smtp_delivery,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_html_mail_with_underscores,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_implicitly_multipart_with_utf8,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_iso_charset,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_multipart_with_utf8_subject,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_multiple_utf8_recipients,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_nested_parts_with_body,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_performs_delivery_via_sendmail,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_receive_decodes_base64_encoded_mail,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_recursive_multipart_processing,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_reply_to,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_return_path_with_deliver,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_signed_up,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_starttls_is_disabled_if_not_supported,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_starttls_is_enabled_if_supported,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_starttls_is_not_enabled,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_unencoded_subject,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_unquote_7bit_body,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_unquote_base64_body,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_unquote_quoted_printable_body,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_unquote_quoted_printable_subject,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_utf8_body_is_not_quoted,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_various_newlines,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_various_newlines_multipart

    disable ActionMailerUrlTest, 
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_signed_up_with_url

    disable AssetHostTest, 
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_asset_host_as_one_arguement_proc,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_asset_host_as_string,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_asset_host_as_two_arguement_proc

    disable FirstSecondHelperTest, 
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_ordering

    disable LayoutMailerTest, 
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_explicit_class_layout,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_explicit_layout_exceptions,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_should_fix_multipart_layout,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_should_pickup_default_layout,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_should_pickup_layout_given_to_render,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_should_pickup_multipart_layout,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_should_pickup_multipartmixed_layout,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_should_respect_layout_false

    disable MailerHelperTest, 
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_use_example_helper,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_use_helper,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_use_helper_method,
      # NameError: uninitialized constant TMail::Encoder::Encoding
      :test_use_mail_helper

    disable QuotingTest, 
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_email_with_partially_quoted_subject,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_unqoute_in_the_middle,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_unqoute_iso,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_unqoute_multiple,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_unquote_base64,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_unquote_quoted_printable

    disable RenderHelperTest, 
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_file_template,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_included_subtemplate,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_inline_template,
      # NoMethodError: undefined method `first' for nil:NilClass
      :test_rxml_template

  end
end
