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

  def disable_mri_failures
  end
  
  def disable_critical_failures
    # NameError: uninitialized constant OpenSSL::SSL::SSLError
    disable BaseErrorsTest, :setup
  end
  
  def disable_tests
    disable AuthorizationTest, 
      # <ActiveResource::InvalidRequestError> exception expected but was
      # Class: <NameError>
      # Message: <"uninitialized constant OpenSSL::SSL::SSLError">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-2.3.5/lib/active_support/dependencies.rb:440:in `load_missing_constant'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages
      :test_raises_invalid_request_on_unauthorized_requests

    disable BaseErrorsTest, 
      # NameError: uninitialized constant OpenSSL::SSL::SSLError
      :test_should_format_full_errors,
      # NameError: uninitialized constant OpenSSL::SSL::SSLError
      :test_should_iterate_over_errors,
      # NameError: uninitialized constant OpenSSL::SSL::SSLError
      :test_should_iterate_over_full_errors,
      # NameError: uninitialized constant OpenSSL::SSL::SSLError
      :test_should_mark_as_invalid,
      # NameError: uninitialized constant OpenSSL::SSL::SSLError
      :test_should_parse_errors_to_individual_attributes,
      # NameError: uninitialized constant OpenSSL::SSL::SSLError
      :test_should_parse_xml_errors

    disable BaseTest, 
      # <ActiveResource::ResourceConflict> exception expected but was
      # Class: <NameError>
      # Message: <"uninitialized constant OpenSSL::SSL::SSLError">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-2.3.5/lib/active_support/dependencies.rb:440:in `load_missing_constant'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ru
      :test_create,
      # <ActiveResource::ResourceNotFound> exception expected but was
      # Class: <NameError>
      # Message: <"uninitialized constant OpenSSL::SSL::SSLError">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activesupport-2.3.5/lib/active_support/dependencies.rb:80:in `const_missing_with_dependencies'
      # dependencies.rb:398:in `load_missing_constant'
      # d:/
      :test_custom_header,
      # <ActiveResource::ResourceNotFound> exception expected but was
      # Class: <NameError>
      # Message: <"uninitialized constant OpenSSL::SSL::SSLError">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activeresource-2.3.5/lib/active_resource/connection.rb:175:in `request'
      # dependencies.rb:398:in `load_missing_constant'
      # dependencies.rb:79:in `con
      :test_delete,
      # <ActiveResource::ResourceGone> exception expected but was
      # Class: <NameError>
      # Message: <"uninitialized constant OpenSSL::SSL::SSLError">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activeresource-2.3.5/lib/active_resource/connection.rb:175:in `request'
      # dependencies.rb:398:in `load_missing_constant'
      # dependencies.rb:79:in `const_m
      :test_delete_with_410_gone,
      # <ActiveResource::ResourceNotFound> exception expected but was
      # Class: <NameError>
      # Message: <"uninitialized constant OpenSSL::SSL::SSLError">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activeresource-2.3.5/lib/active_resource/connection.rb:175:in `request'
      # dependencies.rb:398:in `load_missing_constant'
      # dependencies.rb:79:in `con
      :test_delete_with_custom_prefix,
      # <ActiveResource::ResourceNotFound> exception expected but was
      # Class: <NameError>
      # Message: <"uninitialized constant OpenSSL::SSL::SSLError">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activeresource-2.3.5/lib/active_resource/connection.rb:175:in `request'
      # dependencies.rb:398:in `load_missing_constant'
      # dependencies.rb:79:in `con
      :test_destroy,
      # <ActiveResource::ResourceGone> exception expected but was
      # Class: <NameError>
      # Message: <"uninitialized constant OpenSSL::SSL::SSLError">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activeresource-2.3.5/lib/active_resource/connection.rb:175:in `request'
      # dependencies.rb:398:in `load_missing_constant'
      # dependencies.rb:79:in `const_m
      :test_destroy_with_410_gone,
      # <ActiveResource::ResourceNotFound> exception expected but was
      # Class: <NameError>
      # Message: <"uninitialized constant OpenSSL::SSL::SSLError">
      # ---Backtrace---
      # dependencies.rb:398:in `load_missing_constant'
      # dependencies.rb:79:in `const_missing_with_dependencies'
      # connection.rb:168:in `request'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activeresou
      :test_destroy_with_custom_prefix,
      # NameError: uninitialized constant OpenSSL::SSL::SSLError
      :test_exists,
      # NameError: uninitialized constant OpenSSL::SSL::SSLError
      :test_exists_with_410_gone,
      # <ActiveResource::ResourceNotFound> exception expected but was
      # Class: <NameError>
      # Message: <"uninitialized constant OpenSSL::SSL::SSLError">
      # ---Backtrace---
      # dependencies.rb:398:in `load_missing_constant'
      # dependencies.rb:79:in `const_missing_with_dependencies'
      # connection.rb:168:in `request'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activeresou
      :test_find_by_id_not_found,
      # <ActiveResource::ResourceConflict> exception expected but was
      # Class: <NameError>
      # Message: <"uninitialized constant OpenSSL::SSL::SSLError">
      # ---Backtrace---
      # dependencies.rb:398:in `load_missing_constant'
      # dependencies.rb:79:in `const_missing_with_dependencies'
      # connection.rb:168:in `request'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activeresou
      :test_update_conflict

    disable ConnectionTest, 
      # NameError: uninitialized constant OpenSSL::SSL::SSLError
      :test_ssl_error

  end
end
