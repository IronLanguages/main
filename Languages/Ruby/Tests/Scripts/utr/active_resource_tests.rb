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
  
  def disable_tests
    # most failures are due to missing String#encode, Time#subsec
  
    disable_by_name %w{
      default_test(BaseTest)
      test_authorization_header_if_credentials_supplied_and_auth_type_is_digest(AuthorizationTest):
      test_client_nonce_is_not_nil(AuthorizationTest):
      test_delete_with_digest_auth_handles_initial_401_response_and_retries(AuthorizationTest):
      test_get_with_digest_auth_caches_nonce(AuthorizationTest):
      test_get_with_digest_auth_handles_initial_401_response_and_retries(AuthorizationTest):
      test_head_with_digest_auth_handles_initial_401_response_and_retries(AuthorizationTest):
      test_post_with_digest_auth_handles_initial_401_response_and_retries(AuthorizationTest):
      test_put_with_digest_auth_handles_initial_401_response_and_retries(AuthorizationTest):
      test_should_format_full_errors(BaseErrorsTest):
      test_should_iterate_over_errors(BaseErrorsTest):
      test_should_iterate_over_full_errors(BaseErrorsTest):
      test_should_mark_as_invalid(BaseErrorsTest):
      test_should_mark_as_invalid_when_content_type_is_unavailable_in_response_header(BaseErrorsTest):
      test_should_parse_errors_to_individual_attributes(BaseErrorsTest):
      test_should_parse_json_and_xml_errors(BaseErrorsTest):
      test_should_parse_json_errors_when_no_errors_key(BaseErrorsTest):
      test_clone(BaseTest):
      test_collection_name(BaseTest):
      test_collection_path(BaseTest):
      test_collection_path_with_parameters(BaseTest):
      test_complex_clone(BaseTest):
      test_create(BaseTest):
      test_create_with_custom_prefix(BaseTest):
      test_create_without_location(BaseTest):
      test_credentials_from_site_are_decoded(BaseTest):
      test_custom_collection_name(BaseTest):
      test_custom_collection_path(BaseTest):
      test_custom_collection_path_with_parameters(BaseTest):
      test_custom_collection_path_with_prefix_and_parameters(BaseTest):
      test_custom_element_name(BaseTest):
      test_custom_element_path(BaseTest):
      test_custom_element_path_with_parameters(BaseTest):
      test_custom_element_path_with_prefix_and_parameters(BaseTest):
      test_custom_element_path_with_redefined_to_param(BaseTest):
      test_custom_header(BaseTest):
      test_custom_prefix(BaseTest):
      test_delete(BaseTest):
      test_delete_with_410_gone(BaseTest):
      test_delete_with_custom_prefix(BaseTest):
      test_destroy(BaseTest):
      test_destroy_with_410_gone(BaseTest):
      test_destroy_with_custom_prefix(BaseTest):
      test_exists(BaseTest):
      test_exists_with_410_gone(BaseTest):
      test_exists_with_redefined_to_param(BaseTest):
      test_exists_without_http_mock(BaseTest):
      test_id_from_response(BaseTest):
      test_id_from_response_without_location(BaseTest):
      test_load_preserves_prefix_options(BaseTest):
      test_load_yaml_array(BaseTest):
      test_module_element_path(BaseTest):
      test_nested_clone(BaseTest):
      test_parse_deep_nested_resources(BaseTest):
      test_password_reader_uses_superclass_password_until_written(BaseTest):
      test_password_variable_can_be_reset(BaseTest):
      test_prefix(BaseTest):
      test_proxy_accessor_accepts_uri_or_string_argument(BaseTest):
      test_proxy_reader_uses_superclass_site_until_written(BaseTest):
      test_proxy_variable_can_be_reset(BaseTest):
      test_reload_with_redefined_to_param(BaseTest):
      test_reload_works_with_prefix_options(BaseTest):
      test_reload_works_without_prefix_options(BaseTest):
      test_respond_to(BaseTest):
      test_save(BaseTest):
      test_save!(BaseTest):
      test_set_prefix(BaseTest):
      test_set_prefix_twice_should_clear_params(BaseTest):
      test_set_prefix_with_default_value(BaseTest):
      test_set_prefix_with_inline_keys(BaseTest):
      test_should_accept_setting_auth_type(BaseTest):
      test_should_accept_setting_password(BaseTest):
      test_should_accept_setting_ssl_options(BaseTest):
      test_should_accept_setting_timeout(BaseTest):
      test_should_accept_setting_user(BaseTest):
      test_should_use_proxy_prefix_and_credentials(BaseTest):
      test_should_use_site_prefix_and_credentials(BaseTest):
      test_site_accessor_accepts_uri_or_string_argument(BaseTest):
      test_site_reader_uses_superclass_site_until_written(BaseTest):
      test_site_variable_can_be_reset(BaseTest):
      test_ssl_options_hash_can_be_reset(BaseTest):
      test_ssl_options_reader_uses_superclass_ssl_options_until_written(BaseTest):
      test_timeout_reader_uses_superclass_timeout_until_written(BaseTest):
      test_timeout_variable_can_be_reset(BaseTest):
      test_to_json(BaseTest):
      test_to_json_with_element_name(BaseTest):
      test_to_key_quacks_like_active_record(BaseTest):
      test_to_param_quacks_like_active_record(BaseTest):
      test_to_xml(BaseTest):
      test_to_xml_with_element_name(BaseTest):
      test_update(BaseTest):
      test_update_attribute_as_string(BaseTest):
      test_update_attribute_as_symbol(BaseTest):
      test_update_attributes_as_strings(BaseTest):
      test_update_attributes_as_symbols(BaseTest):
      test_update_conflict(BaseTest):
      test_update_with_custom_prefix_with_specific_id(BaseTest):
      test_update_with_custom_prefix_without_specific_id(BaseTest):
      test_updating_baseclass_password_wipes_descendent_cached_connection_objects(BaseTest):
      test_updating_baseclass_site_object_wipes_descendent_cached_connection_objects(BaseTest):
      test_updating_baseclass_timeout_wipes_descendent_cached_connection_objects(BaseTest):
      test_updating_baseclass_user_wipes_descendent_cached_connection_objects(BaseTest):
      test_user_reader_uses_superclass_user_until_written(BaseTest):
      test_user_variable_can_be_reset(BaseTest):
      test_custom_collection_method(CustomMethodsTest):
      test_custom_element_method(CustomMethodsTest):
      test_custom_new_element_method(CustomMethodsTest):
      test_find_custom_resources(CustomMethodsTest):
      test_all(FinderTest):
      test_find_all(FinderTest):
      test_find_all_by_from(FinderTest):
      test_find_all_by_from_with_options(FinderTest):
      test_find_all_by_symbol_from(FinderTest):
      test_find_by_id(FinderTest):
      test_find_by_id_not_found(FinderTest)
      test_find_first(FinderTest):
      test_find_last(FinderTest):
      test_find_single_by_from(FinderTest):
      test_find_single_by_symbol_from(FinderTest):
      test_first(FinderTest):
      test_last(FinderTest):
      test_formats_on_collection(FormatTest):
      test_formats_on_custom_collection_method(FormatTest):
      test_formats_on_custom_element_method(FormatTest):
      test_formats_on_single_element(FormatTest):
      test_serialization_of_nested_resource(FormatTest):
      test_request_notification(LogSubscriberTest):
      test_create_fires_save_and_create_notifications(ObservingTest):
      test_destroy_fires_destroy_notifications(ObservingTest):
      test_update_fires_save_and_update_notifications(ObservingTest):
      test_defining_a_schema,_then_fetching_a_model_should_still_match_the_defined_schema(SchemaTest):
      test_known_attributes_on_a_fetched_resource_should_return_all_the_attributes_of_the_instance(SchemaTest):
      test_schema_on_a_fetched_resource_should_return_all_the_attributes_of_that_model_instance(SchemaTest):
      test_setting_schema_then_fetching_should_add_schema_attributes_to_the_intance_attributes(SchemaTest):
      test_with_two_instances,_default_schema_should_match_the_attributes_of_the_individual_instances_-_even_if_they_differ(SchemaTest):
      test_with_two_instances,_known_attributes_should_match_the_attributes_of_the_individual_instances_-_even_if_they_differ(SchemaTest):
    }
  end
end
