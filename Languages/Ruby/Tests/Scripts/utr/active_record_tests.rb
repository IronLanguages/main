class UnitTestSetup
  def initialize
    @name = "ActiveRecord"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'test-unit', "= #{TestUnitVersion}"
    gem 'activerecord', "= #{RailsVersion}"
    gem 'activesupport', "= #{RailsVersion}"
    gem 'activerecord-sqlserver-adapter', "= #{SqlServerAdapterVersion}"
  end

  def ensure_database_exists(name)
    conn = ActiveRecord::Base.sqlserver_connection({
      :mode => 'ADONET',
      :adapter => 'sqlserver',
      :host => ENV['COMPUTERNAME'] + "\\SQLEXPRESS",
      :integrated_security => 'true',
      :database => ''
    })
    begin
      conn.execute "CREATE DATABASE #{name}"
      return
    rescue => e
      if e.message =~ /already exists/
        return
      end
    end
    
    warn "Could not create test databases #{name}"
    exit 0
  end

  def gather_files
    gather_rails_files
    
    # remove all adapter tests:
    @all_test_files.delete_if { |path| path.include?("cases/adapters") }
     
    if UnitTestRunner.ironruby?
      $LOAD_PATH << File.expand_path('Languages/Ruby/Tests/Scripts/adonet_sqlserver', ENV['DLR_ROOT'])
    else
      $LOAD_PATH << File.expand_path('Languages/Ruby/Tests/Scripts/native_sqlserver', ENV['DLR_ROOT'])
    end
    
    require 'active_record'
    require 'active_record/test_case'
    require 'active_record/fixtures'
    require 'active_record/connection_adapters/sqlserver_adapter'

    ensure_database_exists "activerecord_unittest"
    ensure_database_exists "activerecord_unittest2"
    
    sqlserver_adapter_root_dir = File.expand_path("Languages/Ruby/Tests/Libraries/activerecord-sqlserver-adapter-#{SqlServerAdapterVersion}", ENV['DLR_ROOT'])
    $LOAD_PATH << "#{sqlserver_adapter_root_dir}/test"
    ENV['RAILS_SOURCE'] = RAILS_TEST_DIR 
    
    @all_test_files += Dir.glob("#{sqlserver_adapter_root_dir}/test/cases/*_test_sqlserver.rb").sort
  end

  def sanity
    # Do some sanity checks
    sanity_size(85)
  end
  
  def disable_tests
    # many of the tests fail due to missing String#encode
    
    disable AdapterTestSqlserver, 
      "test: For chronic data types with a usec finding existing DB objects should find 003 millisecond in the DB with before and after casting. ",
      "test: For chronic data types with a usec finding existing DB objects should find 123 millisecond in the DB with before and after casting. ",
      "test: For views using @connection.views should allow the connection#view_information method to return meta data on the view. "

    disable SpecificSchemaTestSqlserver, 
      "test: Testing edge case schemas with uniqueidentifier column should allow a simple insert and read of a column without a default function. ",
      "test: Testing edge case schemas with uniqueidentifier column should record the default function name in the column definition but still show a nil real default, will use one day for insert/update. ",
      "test: Testing edge case schemas with uniqueidentifier column should use model callback to set get a new guid. "

    disable ExecuteProcedureTestSqlserver, 
      "test: ExecuteProcedureSqlserver should allow multiple result sets to be returned. ",
      "test: ExecuteProcedureSqlserver should execute a simple procedure. ",
      "test: ExecuteProcedureSqlserver should take parameter arguments. "
    
    disable ColumnTestSqlserver, 
      "test: For datetime columns which have coerced types should have column objects cast to time. "

    disable_by_name %w{
      test_add_limit_offset_should_sanitize_sql_injection_for_limit_with_comas(AdapterTest)
      test_add_limit_offset_should_sanitize_sql_injection_for_limit_without_comas(AdapterTest)
      test_marshalling_extensions(AssociationsExtensionsTest):
      test_marshalling_named_extensions(AssociationsExtensionsTest):
      test_add_to_self_referential_has_many_through(AssociationsJoinModelTest):    
      test_condition_local_time_interpolation_with_default_timezone_utc(FinderTest)
      test_using_limitable_reflections_helper(AssociationsTest)
      test_read_attributes_before_type_cast_on_datetime(AttributeMethodsTest):
      test_typecast_attribute_from_select_to_false(AttributeMethodsTest):
      test_typecast_attribute_from_select_to_true(AttributeMethodsTest):
      test_initialize_with_invalid_attribute(BasicsTest):
      test_preserving_time_objects_with_local_time_conversion_to_default_timezone_utc(BasicsTest)
      test_preserving_time_objects_with_time_with_zone_conversion_to_default_timezone_local(BasicsTest)
      test_preserving_time_objects_with_time_with_zone_conversion_to_default_timezone_utc(BasicsTest)
      test_preserving_time_objects_with_utc_time_conversion_to_default_timezone_local(BasicsTest)
      test_belongs_to_with_primary_key_joins_on_correct_column(BelongsToAssociationsTest)
      test_string_creates_string_column(ChangeTableMigrationsTest)
      test_includes_fetches_nth_level_associations(DatabaseConnectedJsonEncodingTest):
      test_includes_fetches_second_level_associations(DatabaseConnectedJsonEncodingTest):
      test_includes_uses_association_name(DatabaseConnectedJsonEncodingTest):
      test_includes_uses_association_name_and_applies_attribute_filters(DatabaseConnectedJsonEncodingTest):
      test_should_allow_except_option_for_list_of_authors(DatabaseConnectedJsonEncodingTest):
      test_should_allow_includes_for_list_of_authors(DatabaseConnectedJsonEncodingTest):
      test_should_allow_only_option_for_list_of_authors(DatabaseConnectedJsonEncodingTest):
      test_should_allow_options_for_hash_of_authors(DatabaseConnectedJsonEncodingTest):
      test_should_be_able_to_encode_relation(DatabaseConnectedJsonEncodingTest):
      test_should_not_call_methods_on_associations_that_dont_respond(DatabaseConnectedJsonEncodingTest):
      test_saves_both_date_and_time(DateTimeTest)
      test_partial_update(DirtyTest)
      test_count_with_include(EagerAssociationTest):
      test_eager_count_performed_on_a_has_many_association_with_multi_table_conditional(EagerAssociationTest):
      test_eager_count_performed_on_a_has_many_through_association_with_multi_table_conditional(EagerAssociationTest):
      test_eager_with_multi_table_conditional_properly_counts_the_records_when_using_size(EagerAssociationTest):
      test_condition_utc_time_interpolation_with_default_timezone_local(FinderTest)
      test_hash_condition_local_time_interpolation_with_default_timezone_utc(FinderTest)
      test_hash_condition_utc_time_interpolation_with_default_timezone_local(FinderTest)
      test_string_sanitation(FinderTest)
      test_empty_csv_fixtures(FixturesTest):
      test_inserts(FixturesTest)
      test_inserts_with_pre_and_suffix(FixturesTest)
      test_omap_fixtures(FixturesTest)
      test_count_with_finder_sql(HasAndBelongsToManyAssociationsTest):
      test_can_marshal_has_one_association_with_nil_target(HasOneAssociationsTest)
      test_eager_load_belongs_to_primary_key_quoting(InheritanceTest)
      test_assign_valid_dates(InvalidDateTest)
      test_methods_are_called_on_object(JsonSerializationTest):
      test_should_allow_attribute_filtering_with_except(JsonSerializationTest):
      test_should_allow_attribute_filtering_with_only(JsonSerializationTest):
      test_should_demodulize_root_in_json(JsonSerializationTest):
      test_should_encode_all_encodable_attributes(JsonSerializationTest):
      test_should_include_root_in_json(JsonSerializationTest):
      test_all_there(LoadAllFixturesTest):
      test_native_decimal_insert_manual_vs_automatic(MigrationTest):
      test_native_types(MigrationTest):
      test_merged_scoped_find(NestedScopingTest)
      test_cache_does_not_wrap_string_results_in_arrays(QueryCacheTest)
      test_schema_dump_keeps_large_precision_integer_columns_as_decimal(SchemaDumperTest)
      test_array_to_xml_including_belongs_to_association(SerializationTest)
      test_array_to_xml_including_has_one_association(SerializationTest)
      test_serialize_should_allow_attribute_except_filtering(SerializationTest):
      test_serialize_should_allow_attribute_only_filtering(SerializationTest):
      test_serialize_should_be_reversible(SerializationTest):
      test_should_rollback_any_changes_if_an_exception_occurred_while_saving(TestAutosaveAssociationOnAHasManyAssociation)
      test_should_rollback_destructions_if_an_exception_occurred_while_saving_parrots(TestDestroyAsPartOfAutosaveAssociation)
      test_should_assign_existing_children_if_parent_is_new(TestNestedAttributesOnAHasAndBelongsToManyAssociation)
      test_should_automatically_build_new_associated_models_for_each_entry_in_a_hash_where_the_id_is_missing(TestNestedAttributesOnAHasAndBelongsToManyAssociation)
      test_should_assign_existing_children_if_parent_is_new(TestNestedAttributesOnAHasManyAssociation)
      test_should_automatically_build_new_associated_models_for_each_entry_in_a_hash_where_the_id_is_missing(TestNestedAttributesOnAHasManyAssociation)
      test_to_yaml_with_time_with_zone_should_not_raise_exception(YamlSerializationTest):  
      test_attributes_on_dummy_time(BasicsTest):
      test_inspect_instance(BasicsTest):
      test_multiparameter_attributes_on_time_only_column_with_time_zone_aware_attributes_does_not_do_time_zone_conversion(BasicsTest):
      test_preserving_time_objects(BasicsTest)
      test_utc_as_time_zone(BasicsTest):
      test_array_to_xml_including_has_many_association(SerializationTest):
      test_array_to_xml_including_methods(SerializationTest):
      test_to_xml(SerializationTest):
      test_to_xml_including_has_many_association(SerializationTest):
      test_to_xml_skipping_attributes(SerializationTest):
    }
    
    # These tests pass but take more than 30 seconds each a and consume about 1GB of memory (in IronRuby debug build).
    # We should investigate if that's an IronRuby perf issue or the tests just operate on so much data.
    disable_by_name %w{ 
      test_eager_association_loading_of_stis_with_multiple_references(CascadedEagerLoadingTest)
      test_eager_association_loading_with_hmt_does_not_table_name_collide_when_joining_associations(CascadedEagerLoadingTest)
      test_eager_association_loading_with_multiple_stis_and_order(CascadedEagerLoadingTest)
      test_eager_association_loading_with_join_for_count(CascadedEagerLoadingTest)
      test_eager_loading_with_conditions_on_joined_table_preloads(EagerAssociationTest)
      test_including_duplicate_objects_from_has_many(EagerAssociationTest)
      test_limited_eager_with_numeric_in_association(EagerAssociationTest)
      test_missing_data_in_a_nested_include_should_not_cause_errors_when_constructing_objects(EagerLoadNestedIncludeWithMissingDataTest)
      test_find_with_order_on_included_associations_with_construct_finder_sql_for_association_limiting_and_is_distinct(FinderTest)
      test_join_table_alias(HasAndBelongsToManyAssociationsTest)
      test_join_with_group(HasAndBelongsToManyAssociationsTest)
      test_nested_scoped_find_merged_include(NestedScopingTest)
    }
  end
end

