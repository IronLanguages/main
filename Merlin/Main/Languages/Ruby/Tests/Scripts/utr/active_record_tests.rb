class UnitTestSetup
  def initialize
    @name = "ActiveRecord"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'test-unit', "= 2.0.5"
    gem 'activerecord', "= 2.3.5"
    gem 'activesupport', "= 2.3.5"
    require 'ironruby_sqlserver' if UnitTestRunner.ironruby?
  end

  def ensure_database_exists(name)
    conn = DBI.connect("DBI:MSSQL:server=#{ENV["COMPUTERNAME"]}\\SQLEXPRESS;integrated security=true")
    begin
      conn.execute "CREATE DATABASE #{name}"
      return
    rescue DBI::DatabaseError => e
      if e.message =~ /already exists/
        return
      end
    end
    
    warn "Could not create test databases #{name}"
    exit 0
  end

  def gather_files
    sqlserver_adapter_root_dir = File.expand_path '../External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3', ENV['MERLIN_ROOT']
    activerecord_tests_dir = File.expand_path '../External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test', ENV['MERLIN_ROOT']
    $LOAD_PATH << sqlserver_adapter_root_dir + '/test'
    $LOAD_PATH << activerecord_tests_dir

    if UnitTestRunner.ironruby?
      $LOAD_PATH << File.expand_path('Languages/Ruby/Tests/Scripts/adonet_sqlserver', ENV['MERLIN_ROOT'])
    else
      dbi_0_2_2 = 'c:/bugs/rails/dbi-0.2.2' # change this for your computer
      $LOAD_PATH << "#{dbi_0_2_2}/lib"
      require "#{dbi_0_2_2}/lib/dbi.rb"
      require "#{dbi_0_2_2}/lib/dbd/ado.rb"
      $LOAD_PATH << File.expand_path('Languages/Ruby/Tests/Scripts/native_sqlserver', ENV['MERLIN_ROOT'])
    end

    require 'active_record'
    require 'active_record/test_case'
    require 'active_record/fixtures'

    ensure_database_exists "activerecord_unittest"
    ensure_database_exists "activerecord_unittest2"

    # Load helper files
    require "#{sqlserver_adapter_root_dir}/test/cases/aaaa_create_tables_test_sqlserver"
    # Overwrite ACTIVERECORD_TEST_ROOT since aaaa_create_tables_test_sqlserver assumes a specific folder layout
    Object.const_set "ACTIVERECORD_TEST_ROOT", activerecord_tests_dir

    @all_test_files = Dir.glob("#{sqlserver_adapter_root_dir}/test/cases/*_test_sqlserver.rb").sort
    # Rails ActiveRecord tests
    @all_test_files += Dir.glob("#{activerecord_tests_dir}/**/*_test.rb")
  end
  
  def require_tests
    # Note that the tests are registered using Kernel#at_exit, and will run during shutdown
    # The "require" statement just registers the tests for being run later...
    @all_test_files.each { |f| 
      begin
        require f
      rescue NameError => e # TODO - This should not be needed ideally...
        abort if not /eager_association_test_sqlserver/ =~ f
        warn "Error while loading #{f}: #{e}"
      end
    }
  end
  
  def sanity
    # Do some sanity checks
    sanity_size(85)
  end

  # Provide dummy implementation for the missing Zlib.crc32 for now
  require "zlib"
  Zlib.class_eval { def self.crc32(o) h = o.to_str.hash; h < 0 ? -h : h end }

  def disable_critical_failures
    # If this test executes, all subsequent tests start failing during setup with an exception
    # saying "The server failed to resume the transaction", presumably because
    # teardown did not happen properly for this test, and the active transaction was not aborted.
    disable FinderTest, :test_exists
  end
  
  def disable_unstable_tests
    disable AggregationsTest, 
      # ActiveRecord::RecordNotFound: Couldn't find Customer with ID=1
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1586:in `find_one'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1569:in `find_from_ids'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:616:in `find'
      :test_nil_assignment_results_in_nil,
      # <NoMethodError> exception expected but was
      # Class: <ActiveRecord::RecordNotFound>
      # Message: <"Couldn't find Customer with ID=1">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1586:in `find_one'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8
      :test_nil_raises_error_when_allow_nil_is_false,
      # ActiveRecord::RecordNotFound: Couldn't find Customer with ID=1
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1586:in `find_one'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1569:in `find_from_ids'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:616:in `find'
      :test_reloaded_instance_refreshes_aggregations

    disable AssociationCallbacksTest, 
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1586:in `find_one'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1569:in `find_from_ids'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:616:in `find'
      :test_adding_macro_callbacks,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1586:in `find_one'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1569:in `find_from_ids'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:616:in `find'
      :test_adding_with_proc_callbacks,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1586:in `find_one'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1569:in `find_from_ids'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:616:in `find'
      :test_dont_add_if_before_callback_raises_exception,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1586:in `find_one'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1569:in `find_from_ids'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:616:in `find'
      :test_has_and_belongs_to_many_add_callback,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1586:in `find_one'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1569:in `find_from_ids'
      # base.rb:607:in `find'
      :test_has_and_belongs_to_many_after_add_called_after_save,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_has_and_belongs_to_many_remove_callback,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_has_and_belongs_to_many_remove_callback_on_clear,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_has_many_and_belongs_to_many_callbacks_for_save_on_parent,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_has_many_callbacks_for_save_on_parent,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_has_many_callbacks_with_create,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_has_many_callbacks_with_create!,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_multiple_callbacks,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_removing_with_macro_callbacks,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=2
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_removing_with_proc_callbacks

    disable AssociationProxyTest, 
      # ActiveRecord::RecordNotFound: Couldn't find Author with ID=1
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_create_via_association_with_block,
      # ActiveRecord::RecordNotFound: Couldn't find Author with ID=1
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_create_with_bang_via_association_with_block,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=1
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_proxy_accessors,
      # ActiveRecord::RecordNotFound: Couldn't find Author with ID=1
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_push_has_many_through_does_not_load_target,
      # ActiveRecord::RecordNotFound: Couldn't find Developer with ID=1
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_reload_returns_assocition,
      # ActiveRecord::RecordNotFound: Couldn't find Developer with ID=1
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_save_on_parent_does_not_load_target

    disable AssociationsExtensionsTest, 
      # ActiveRecord::RecordNotFound: Couldn't find Project with ID=2
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_extension_on_habtm,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=1
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_extension_on_has_many,
      # ActiveRecord::RecordNotFound: Couldn't find Project with ID=2
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_named_extension_and_block_on_habtm,
      # ActiveRecord::RecordNotFound: Couldn't find Project with ID=2
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_named_extension_on_habtm,
      # ActiveRecord::RecordNotFound: Couldn't find Project with ID=2
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_named_two_extensions_on_habtm

    disable DatabaseConnectedJsonEncodingTest, 
      # TypeError: There is already an open DataReader associated with this Command which must be closed first.
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateCommand'
      :test_includes_fetches_second_level_associations,
      # TypeError: There is already an open DataReader associated with this Command which must be closed first.
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateCommand'
      :test_includes_uses_association_name

    disable EagerAssociationTest, 
      # <[]> expected but was
      # <[#<Post id: 1, author_id: 1, title: "Welcome to the weblog", body: "Such a lovely day", type: "Post", comments_count: 2, taggings_count: 1>]>.
      :test_eager_with_multiple_associations_with_same_table_has_many_and_habtm,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=1
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/eager_test.rb:136:in `test_finding_with_includes_on_belongs_to_association_with_same_include_includes_only_once'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_finding_with_includes_on_belongs_to_association_with_same_include_includes_only_once,
      # ActiveRecord::UnknownAttributeError: unknown attribute: author
      # base.rb:2742:in `attributes='
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Extensions\IDictionaryOps.cs:196:in `each'
      # base.rb:2734:in `attributes='
      :test_finding_with_includes_on_null_belongs_to_association_with_same_include_includes_only_once

    disable HasManyThroughAssociationsTest, 
      # <false> is not true.
      :test_associate_with_create,
      # <1> expected but was
      # <0>.
      :test_associate_with_create_and_no_options

  end
  
  def disable_tests
    disable AdapterTest, 
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_indexes

    disable AdapterTestSqlserver, 
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      "test: For DatabaseStatements altering isolation levels with a valid isolation level should allow #run_with_isolation_level to not take a block to set it. ",
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      "test: For DatabaseStatements altering isolation levels with a valid isolation level should pass a read uncommitted isolation level test. ",
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      "test: For DatabaseStatements altering isolation levels with a valid isolation level should return block value using #run_with_isolation_level. ",
      # <"#<ActiveRecord::ConnectionAdapters::SQLServerAdapter version: 2.3, year: 2005, connection_options: [\"DBI:MSSQL:server=SBORDE1\\\\SQLEXPRESS;initial catalog=activerecord_unittest;integrated security=true\"]>"> expected to be =~
      # </version\: \d.\d.\d/>.
      "test: For abstract behavior should include version in inspect. ",
      # ActiveRecord::StatementInvalid: TypeError: There is already an open DataReader associated with this Command which must be closed first.: DELETE FROM [sql_server_chronics] WHERE [datetime] = '2012-11-08 10:24:36.003'
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      "test: For chronic data types with a usec finding existing DB objects should find 003 millisecond in the DB with before and after casting. ",
      # TypeError: There is already an open DataReader associated with this Command which must be closed first.
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateCommand'
      "test: For chronic data types with a usec finding existing DB objects should find 123 millisecond in the DB with before and after casting. ",
      # TypeError: There is already an open DataReader associated with this Command which must be closed first.
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateCommand'
      "test: For chronic data types with a usec saving new datetime objects should truncate 123456 usec to just 123 in the DB cast back to 123000. ",
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: The transaction operation cannot be performed because there are pending requests working on this transaction.: SAVE TRANSACTION active_record_1
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:856:in `do_execute'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/lib/active_record/connection_adapters/sqlserver_adapter.rb:437:in `create_savepoint'
      "test: For chronic data types with a usec saving new datetime objects should truncate 3001 usec to just 003 in the DB cast back to 3000. ",
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      "test: For indexes should have indexes with descending order. ",
      # Exception raised:
      # Class: <TypeError>
      # Message: <"Invalid attempt to call Read when reader is closed.">
      # ---Backtrace---
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      # statement.rb:207:in `fetch'
      # statement.rb:236:in `each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Enumerable.cs:35:in `Each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Rub
      "test: For views that have more than 4000 chars for their defintion should cope with null returned for the defintion. ",
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      "test: For views that have more than 4000 chars for their defintion should using alternate view defintion still be able to find real default. "

    disable AssociationProxyTest, 
      # <false> is not true.
      :test_push_does_not_load_target,
      # <false> is not true.
      :test_push_followed_by_save_does_not_load_target,
      # ActiveRecord::RecordNotFound: Couldn't find Developer with ID=150
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_save_on_parent_saves_children

    disable AssociationsExtensionsTest, 
      # ArgumentError: undefined class/module DeveloperProjectsAssociationExtensioo
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:689:in `ReadClassOrModule'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:739:in `ReadExtended'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:879:in `ReadAnObject'
      :test_marshalling_extensions,
      # ArgumentError: undefined class/module DeveloperProjectsAssociationExtensioo
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:689:in `ReadClassOrModule'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:739:in `ReadExtended'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:879:in `ReadAnObject'
      :test_marshalling_named_extensions

    disable AssociationsJoinModelTest, 
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=170
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_associating_unsaved_records_with_has_many_through,
      # <[9, 10, 120]> expected but was
      # <[9, 10]>.
      :test_has_many_through_goes_through_all_sti_classes

    disable AssociationsTest, 
      # NoMethodError: undefined method `flock' for #<File:C:/Users/sborde/AppData/Local/Temp/ar-pstore-association-test>
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/redist-libs/ruby/1.8/pstore.rb:296:in `transaction'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations_test.rb:78:in `test_storing_in_pstore'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      :test_storing_in_pstore

    disable AttributeMethodsTest, 
      # <NoMethodError> exception expected but was
      # Class: <ArgumentError>
      # Message: <"wrong number of arguments (3 for 1)">
      # ---Backtrace---
      # attribute_methods.rb:232:in `method_missing'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/attribute_methods_test.rb:268:in `test_question_attributes_respect_access_control'
      # assertions.rb:49:in `assert_b
      :test_question_attributes_respect_access_control,
      # <NoMethodError> exception expected but was
      # Class: <ArgumentError>
      # Message: <"wrong number of arguments (3 for 1)">
      # ---Backtrace---
      # attribute_methods.rb:232:in `method_missing'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/attribute_methods_test.rb:248:in `test_read_attributes_respect_access_control'
      # assertions.rb:49:in `assert_block
      :test_read_attributes_respect_access_control,
      # <NoMethodError> exception expected but was
      # Class: <ArgumentError>
      # Message: <"wrong number of arguments (3 for 1)">
      # ---Backtrace---
      # attribute_methods.rb:232:in `method_missing'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/attribute_methods_test.rb:258:in `test_write_attributes_respect_access_control'
      # assertions.rb:49:in `assert_bloc
      :test_write_attributes_respect_access_control

    disable BasicsTest, 
      # <false> is not true.
      :test_array_to_xml_including_belongs_to_association,
      # <false> is not true.
      :test_array_to_xml_including_has_one_association,
      # Exception raised:
      # Class: <ActiveRecord::RecordNotFound>
      # Message: <"Couldn't find Reply with ID=110">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:1559:in `test_class_level_delete'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      # assertions.rb:812:in `_wrap_assertion'
      # d:/v
      :test_class_level_delete,
      # <"2010-01-24 01:50:35.000"> expected but was
      # <Sun, 24 Jan 2010 01:50:35 +0000>.
      :test_coerced_test_read_attributes_before_type_cast_on_datetime,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=140
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:190:in `test_create'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_create,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=180
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_create_many_through_factory_with_block,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=210
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_create_through_factory_with_block,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=220
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:773:in `test_default_values'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_default_values,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=230
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:819:in `test_default_values_on_empty_strings'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_default_values_on_empty_strings,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=240
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:156:in `test_hash_content'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      # base.rb:1576:in `find_one'
      :test_hash_content,
      # NoMethodError: undefined method `size' for nil:NilClass
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:511:in `test_initialize_with_invalid_attribute'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      :test_initialize_with_invalid_attribute,
      # The bonus_time attribute should be of the Time class.
      # <Sun, 30 Jan 2005 06:28:00 +0000> expected to be kind_of?
      # <Time> but was
      # <DateTime>.
      :test_preserving_time_objects,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=270
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:1525:in `test_quote'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_quote,
      # ActiveRecord::RecordNotFound: Couldn't find ReadonlyTitlePost with ID=210
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:2696:in `reload'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_readonly_attributes,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=310
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_serialized_attribute_with_class_constraint,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=320
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_serialized_string_attribute,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=330
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_serialized_time_attribute,
      # <ActiveRecord::SerializationTypeMismatch> exception expected but was
      # Class: <ActiveRecord::RecordNotFound>
      # Message: <"Couldn't find Topic with ID=340">
      # ---Backtrace---
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:1507:in `test_should_raise
      :test_should_raise_exception_on_serialized_attribute_with_type_mismatch,
      # <"2004-04-15"> expected but was
      # <"2004-04-15 00:00:00">.
      :test_to_xml,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=350
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:284:in `test_update'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_update,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=360
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_update_array_content,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=370
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:300:in `test_update_columns_not_equal_attributes'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_update_columns_not_equal_attributes

    disable BelongsToAssociationsTest, 
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=380
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/belongs_to_associations_test.rb:144:in `test_belongs_to_counter'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_belongs_to_counter,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=420
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/belongs_to_associations_test.rb:246:in `test_belongs_to_counter_after_update_attributes'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_belongs_to_counter_after_update_attributes,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=440
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/belongs_to_associations_test.rb:192:in `test_belongs_to_counter_with_reassigning'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_belongs_to_counter_with_reassigning,
      # <NoMethodError> exception expected but was
      # Class: <ArgumentError>
      # Message: <"wrong number of arguments (3 for 1)">
      # ---Backtrace---
      # attribute_methods.rb:232:in `method_missing'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/belongs_to_associations_test.rb:401:in `test_belongs_to_proxy_should_not_respond_to_private_methods
      :test_belongs_to_proxy_should_not_respond_to_private_methods,
      # ActiveRecord::RecordNotFound: Couldn't find Web::Topic with ID=470
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/belongs_to_associations_test.rb:224:in `test_belongs_to_reassign_with_namespaced_models_and_counters'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_belongs_to_reassign_with_namespaced_models_and_counters,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=520
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/belongs_to_associations_test.rb:178:in `test_belongs_to_with_primary_key_counter_with_assigning_nil'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_belongs_to_with_primary_key_counter_with_assigning_nil,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=540
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_counter_cache,
      # ActiveRecord::RecordNotFound: Couldn't find Client with ID=210
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_creating_the_belonging_object_with_primary_key,
      # ActiveRecord::RecordNotFound: Couldn't find Reply with ID=560
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_custom_counter_cache

    disable BinaryTest, 
      # Reloaded data differs from original.
      # <"\377\330\377\340\000\020JFIF\000\001\001\001\000H\000H\000\000\377\333\000C\000\r\t\t\n\n\n\016\v\v\016\024\r\v\r\024\027\021\016\016\021\027\e\025\025\025\025\025\e\e\025\027\027\027\027\025\e\032\036 ! \036\032''**''555556666666666\377\333\000C\001\016\r\r\021\021\021\027\021\021\027\027\023\024\023\027\035\031\032\032\031\035&\035\035\036\035\035&,$    $,(+
      :test_load_save

    disable CalculationsTest, 
      # NoMethodError: undefined method `to_d' for #<BigDecimal:0x005d4bc,'0.0',9(9)>
      # calculations.rb:296:in `type_cast_calculated_value'
      # calculations.rb:236:in `execute_simple_calculation'
      # calculations.rb:130:in `calculate'
      :test_should_return_nil_as_average

    disable ColumnTestSqlserver, 
      # <"GIF89a\001\000\001\000\200\000\000\377\377\377\000\000\000!\371\004\000\000\000\000\000,\000\000\000\000\001\000\001\000\000\002\002D\001\000;"> expected but was
      # <"qspVW\227\020\020\022\200\002U%RU\000\0032I@\000\000D\000\000\020\020\002&\201\005\220">.
      "test: For binary columns should read and write binary data equally. ",
      # ActiveRecord::StatementInvalid: NoMethodError: undefined method `[]' for nil:NilClass: SELECT * FROM [sql_server_chronics] WHERE ([sql_server_chronics].[id] = 7) 
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:862:in `raw_select'
      "test: For datetime columns which have coerced types should have an inheritable attribute . ",
      # TypeError: There is already an open DataReader associated with this Command which must be closed first.
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateCommand'
      "test: For datetime columns which have coerced types should have column and objects cast to date. ",
      # TypeError: There is already an open DataReader associated with this Command which must be closed first.
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateCommand'
      "test: For datetime columns which have coerced types should have column objects cast to time. "

    disable ConnectionTestSqlserver, 
      # RuntimeError: each_object only supported for objects of type Class or Module
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:117:in `assert_all_statements_used_are_closed'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:76
      "test: ConnectionSqlserver should active closes statement. ",
      # RuntimeError: each_object only supported for objects of type Class or Module
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:117:in `assert_all_statements_used_are_closed'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:56
      "test: ConnectionSqlserver should execute with block closes statement. ",
      # RuntimeError: each_object only supported for objects of type Class or Module
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:117:in `assert_all_statements_used_are_closed'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:50
      "test: ConnectionSqlserver should execute without block closes statement. ",
      # RuntimeError: each_object only supported for objects of type Class or Module
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:117:in `assert_all_statements_used_are_closed'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:22
      "test: ConnectionSqlserver should finish DBI statment handle from #execute with block. ",
      # RuntimeError: each_object only supported for objects of type Class or Module
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:117:in `assert_all_statements_used_are_closed'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:34
      "test: ConnectionSqlserver should finish connection from #raw_select. ",
      # RuntimeError: each_object only supported for objects of type Class or Module
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:117:in `assert_all_statements_used_are_closed'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:64
      "test: ConnectionSqlserver should insert with identity closes statement. ",
      # RuntimeError: each_object only supported for objects of type Class or Module
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:117:in `assert_all_statements_used_are_closed'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:70
      "test: ConnectionSqlserver should insert without identity closes statement. "

    disable DateTimeTest, 
      # <Tue Feb 10 15:30:45 -0800 1807> expected but was
      # <Tue, 10 Feb 1807 15:30:45 +0000>.
      # 
      # diff:
      # - Tue Feb 10 15:30:45 -0800 1807
      # ?                     ^^^  --- ^
      # + Tue, 10 Feb 1807 15:30:45 +0000
      # ?    ++++      + +          ^   ^
      :test_saves_both_date_and_time

    disable DirtyTest, 
      # <Sun Jan 24 00:00:00 -0800 2010> expected but was
      # <Sun, 24 Jan 2010 00:00:00 +0000>.
      # 
      # diff:
      # - Sun Jan 24 00:00:00 -0800 2010
      # ?          ^          ^^^  -- -
      # + Sun, 24 Jan 2010 00:00:00 +0000
      # ?    ++++      ^^^          ^
      :test_partial_update

    disable EagerAssociationTest, 
      # NoMethodError: undefined method `comments' for nil:NilClass
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/eager_test.rb:98:in `test_including_duplicate_objects_from_belongs_to'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/eager_test.rb:97:in `test_including_duplicate_objects_from_belongs_to'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      :test_including_duplicate_objects_from_belongs_to,
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_limited_eager_with_multiple_order_columns,
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_limited_eager_with_order

    disable EagerLoadIncludeFullStiClassNamesTest, 
      # <"Tagging"> expected but was
      # <"NilClass">.
      :test_class_names

    disable EagerLoadPolyAssocsTest, 
      # NoMethodError: undefined method `non_poly' for nil:NilClass
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/eager_load_nested_include_test.rb:101:in `test_include_query'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/eager_load_nested_include_test.rb:100:in `test_include_query'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Extensions\IListOps.cs:834:in `each'
      :test_include_query

    disable ExecuteProcedureTestSqlserver, 
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      "test: ExecuteProcedureSqlserver should execute a simple procedure. ",
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      "test: ExecuteProcedureSqlserver should quote bind vars correctly. ",
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      "test: ExecuteProcedureSqlserver should take parameter arguments. "

    disable FinderTest, 
      # <Wed, 16 Jul 2003 07:28:11 +0000> expected to be kind_of?
      # <Time> but was
      # <DateTime>.
      :test_bind_variables,
      # <Wed, 16 Jul 2003 07:28:11 +0000> expected to be kind_of?
      # <Time> but was
      # <DateTime>.
      :test_condition_array_interpolation,
      # <Wed, 16 Jul 2003 07:28:11 +0000> expected to be kind_of?
      # <Time> but was
      # <DateTime>.
      :test_condition_hash_interpolation,
      # <Wed, 16 Jul 2003 07:28:11 +0000> expected to be kind_of?
      # <Time> but was
      # <DateTime>.
      :test_condition_interpolation,
      # <#<Customer id: 42100, name: nil, balance: 123, address_street: nil, address_city: nil, address_country: nil, gps_location: nil>> expected but was
      # <#<Customer id: 421, name: nil, balance: 123, address_street: nil, address_city: nil, address_country: nil, gps_location: nil>>.
      # 
      # diff:
      # - #<Customer id: 42100, name: nil, balance: 123, address_street: nil, address_city: nil, address_country: nil, gps_loc
      :test_find_or_create_from_one_aggregate_attribute,
      # <#<Customer id: 42200, name: "Elizabeth", balance: 123, address_street: nil, address_city: nil, address_country: nil, gps_location: nil>> expected but was
      # <#<Customer id: 422, name: "Elizabeth", balance: 123, address_street: nil, address_city: nil, address_country: nil, gps_location: nil>>.
      # 
      # diff:
      # - #<Customer id: 42200, name: "Elizabeth", balance: 123, address_street: nil, address_city: nil, addre
      :test_find_or_create_from_one_aggregate_attribute_and_hash,
      # <#<Company id: 280, type: nil, ruby_type: nil, firm_id: nil, firm_name: nil, name: "38signals", client_of: nil, rating: 1>> expected but was
      # <#<Company id: 28, type: nil, ruby_type: nil, firm_id: nil, firm_name: nil, name: "38signals", client_of: nil, rating: 1>>.
      # 
      # diff:
      # - #<Company id: 280, type: nil, ruby_type: nil, firm_id: nil, firm_name: nil, name: "38signals", client_of: nil, rating: 1>
      # ?    
      :test_find_or_create_from_one_attribute,
      # <#<Company id: 290, type: nil, ruby_type: nil, firm_id: 17, firm_name: nil, name: "38signals", client_of: 23, rating: 1>> expected but was
      # <#<Company id: 29, type: nil, ruby_type: nil, firm_id: 17, firm_name: nil, name: "38signals", client_of: 23, rating: 1>>.
      # 
      # diff:
      # - #<Company id: 290, type: nil, ruby_type: nil, firm_id: 17, firm_name: nil, name: "38signals", client_of: 23, rating: 1>
      # ?          
      :test_find_or_create_from_one_attribute_and_hash,
      # <#<Topic id: 620, title: "Another topic", author_name: "John", author_email_address: "test@test.com", written_on: "2010-01-24 01:52:36", bonus_time: nil, last_read: nil, content: nil, approved: true, replies_count: 0, parent_id: nil, parent_title: nil, type: nil>> expected but was
      # <#<Topic id: 62, title: "Another topic", author_name: "John", author_email_address: "test@test.com", written_on: "2010-
      :test_find_or_create_from_two_attributes,
      # <#<Customer id: 42300, name: "Elizabeth", balance: 123, address_street: nil, address_city: nil, address_country: nil, gps_location: nil>> expected but was
      # <#<Customer id: 423, name: "Elizabeth", balance: 123, address_street: nil, address_city: nil, address_country: nil, gps_location: nil>>.
      # 
      # diff:
      # - #<Customer id: 42300, name: "Elizabeth", balance: 123, address_street: nil, address_city: nil, addre
      :test_find_or_create_from_two_attributes_with_one_being_an_aggregate,
      # <Wed, 16 Jul 2003 07:28:11 +0000> expected to be kind_of?
      # <Time> but was
      # <DateTime>.
      :test_named_bind_variables

    disable FixturesTest, 
      # <"\377\330\377\340\000\020JFIF\000\001\001\001\000H\000H\000\000\377\333\000C\000\r\t\t\n\n\n\016\v\v\016\024\r\v\r\024\027\021\016\016\021\027\e\025\025\025\025\025\e\e\025\027\027\027\027\025\e\032\036 ! \036\032''**''555556666666666\377\333\000C\001\016\r\r\021\021\021\027\021\021\027\027\023\024\023\027\035\031\032\032\031\035&\035\035\036\035\035&,$    $,(+&&&+(//,,//666666666666666\377\300\00
      :test_binary_in_fixtures,
      # Exception raised:
      # Class: <Fixture::FormatError>
      # Message: <"Bad data for Category fixture named #<PrivateType Tag=\"omap\" Value=\"IronRuby.Builtins.RubyArray\">">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/fixtures_test.rb:165:in `test_omap_fixtures'
      # fixtures.rb:707:in `read_yaml_fixture_files'
      # d:\vs_langs01_s\Mer
      :test_omap_fixtures

    disable FoxyFixturesTest, 
      # <207281424> expected but was
      # <1014543642>.
      :test_identifies_consistently

    disable HasAndBelongsToManyAssociationsTest, 
      # ActiveRecord::RecordNotFound: Couldn't find Developer with ID=240
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_assign_ids,
      # ActiveRecord::RecordNotFound: Couldn't find Developer with ID=250
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_assign_ids_ignoring_blanks,
      # <nil> expected but was
      # <#<Project id: 6, name: "Lie in it", type: nil>>.
      :test_build_by_new_record,
      # ActiveRecord::RecordNotFound: Couldn't find DeveloperWithCounterSQL with ID=270
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_count_with_counter_sql,
      # <nil> expected but was
      # <#<Project id: 10, name: "Lie in it", type: nil>>.
      :test_create_by_new_record,
      # <[#<Developer id: 2, name: "Jamis", salary: 150000, created_at: "2010-01-24 01:53:09", updated_at: "2010-01-24 01:53:09">,
      #  #<Developer id: 11, name: "Jamis", salary: 9000, created_at: "2010-01-24 01:53:09", updated_at: "2010-01-24 01:53:09">,
      #  #<Developer id: 290, name: "Jamis", salary: 70000, created_at: "2010-01-24 01:53:18", updated_at: "2010-01-24 01:53:18">]> expected but was
      # <[#<Developer id
      :test_dynamic_find_all_order_should_override_association_order,
      # <#<Developer id: 320, name: "Jamis", salary: 70000, created_at: "2010-01-24 01:53:19", updated_at: "2010-01-24 01:53:19">> expected but was
      # <#<Developer id: 11, name: "Jamis", salary: 9000, created_at: "2010-01-24 01:53:09", updated_at: "2010-01-24 01:53:09">>.
      # 
      # diff:
      # - #<Developer id: 320, name: "Jamis", salary: 70000, created_at: "2010-01-24 01:53:19", updated_at: "2010-01-24 01:53:19">
      # ?        
      :test_dynamic_find_should_respect_association_order,
      # <2> expected but was
      # <1>.
      :test_habtm_adding_before_save,
      # ActiveRecord::RecordNotFound: Couldn't find all Developers with IDs (370,360) (found 0 results, but was looking for 2)
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/base.rb:1613:in `find_some'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/associations.rb:1331:in `collection_accessor_methods'
      # base.rb:1559:in `find_from_ids'
      :test_habtm_saving_multiple_relationships,
      # ActiveRecord::RecordNotFound: Couldn't find ProjectWithAfterCreateHook with ID=120
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_new_with_values_in_collection,
      # <1> expected but was
      # <0>.
      :test_symbols_as_keys

    disable HasManyAssociationsTest, 
      # <#<Client id: 360, type: "Client", ruby_type: nil, firm_id: nil, firm_name: nil, name: "Natural Company", client_of: 1, rating: 1>> expected but was
      # <#<Client id: 36, type: "Client", ruby_type: nil, firm_id: nil, firm_name: nil, name: "Natural Company", client_of: 1, rating: 1>>.
      # 
      # diff:
      # - #<Client id: 360, type: "Client", ruby_type: nil, firm_id: nil, firm_name: nil, name: "Natural Company", client
      :test_adding,
      # <#<Client id: 430, type: "Client", ruby_type: nil, firm_id: nil, firm_name: nil, name: "Another Client", client_of: 1, rating: 1>> expected but was
      # <#<Client id: 43, type: "Client", ruby_type: nil, firm_id: nil, firm_name: nil, name: "Another Client", client_of: 1, rating: 1>>.
      # 
      # diff:
      # - #<Client id: 430, type: "Client", ruby_type: nil, firm_id: nil, firm_name: nil, name: "Another Client", client_of
      :test_create,
      # <0> expected but was
      # <1>.
      :test_delete_all,
      # <0> expected but was
      # <1>.
      :test_deleting_a_collection,
      # <#<Client id: 610, type: "Client", ruby_type: nil, firm_id: 1, firm_name: nil, name: "Yet another client", client_of: nil, rating: 1>> expected but was
      # <#<Client id: 61, type: "Client", ruby_type: nil, firm_id: 1, firm_name: nil, name: "Yet another client", client_of: nil, rating: 1>>.
      # 
      # diff:
      # - #<Client id: 610, type: "Client", ruby_type: nil, firm_id: 1, firm_name: nil, name: "Yet another client",
      :test_find_or_create,
      # ActiveRecord::RecordNotFound: Couldn't find Namespaced::Firm with ID=630
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/has_many_associations_test.rb:1099:in `test_joins_with_namespaced_model_should_use_correct_type'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_joins_with_namespaced_model_should_use_correct_type

    disable HasManyThroughAssociationsTest, 
      # <1> expected but was
      # <0>.
      :test_associate_with_create_and_valid_options,
      # <1> expected but was
      # <0>.
      :test_associate_with_create_bang_and_valid_options,
      # <1> expected but was
      # <0>.
      :test_associate_with_create_exclamation_and_no_options,
      # <false> is not true.
      :test_associating_new,
      # <["Bob", "Bob", "Lary", "Lary", "Sam", "Sam", "Ted", "Ted"]> expected but was
      # <["Lary", "Lary", "Sam", "Sam", "Ted", "Ted", "Ted", "Ted"]>.
      # 
      # diff:
      # - ["Bob", "Bob", "Lary", "Lary", "Sam", "Sam", "Ted", "Ted"]
      # ?  --------------
      # + ["Lary", "Lary", "Sam", "Sam", "Ted", "Ted", "Ted", "Ted"]
      # ?                                       ++++++++++++++
      :test_association_callback_ordering

    disable HasOneAssociationsTest, 
      # <#<Account id: 120, firm_id: 1, firm_name: nil, credit_limit: 1000>> expected but was
      # <#<Account id: 12, firm_id: 1, firm_name: nil, credit_limit: 1000>>.
      # 
      # diff:
      # - #<Account id: 120, firm_id: 1, firm_name: nil, credit_limit: 1000>
      # ?                 -
      # + #<Account id: 12, firm_id: 1, firm_name: nil, credit_limit: 1000>
      :test_assignment_before_child_saved,
      # Exception raised:
      # Class: <TypeError>
      # Message: <"can't dump hash with default proc">
      # ---Backtrace---
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:245:in `WriteHash'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:428:in `WriteAnObject'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marsha
      :test_can_marshal_has_one_association_with_nil_target,
      # <#<Account id: 210, firm_id: 70, firm_name: nil, credit_limit: 1000>> expected but was
      # <#<Account id: 21, firm_id: 70, firm_name: nil, credit_limit: 1000>>.
      # 
      # diff:
      # - #<Account id: 210, firm_id: 70, firm_name: nil, credit_limit: 1000>
      # ?                 -
      # + #<Account id: 21, firm_id: 70, firm_name: nil, credit_limit: 1000>
      :test_create_association,
      # NoMethodError: undefined method `rating' for nil:NilClass
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/has_one_associations_test.rb:266:in `test_finding_with_interpolated_condition'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_finding_with_interpolated_condition,
      # <NoMethodError> exception expected but was
      # Class: <ArgumentError>
      # Message: <"wrong number of arguments (3 for 1)">
      # ---Backtrace---
      # attribute_methods.rb:232:in `method_missing'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/has_one_associations_test.rb:293:in `test_has_one_proxy_should_not_respond_to_private_methods'
      # asse
      :test_has_one_proxy_should_not_respond_to_private_methods

    disable HasOneThroughAssociationsTest, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [memberships] ([joined_on], [club_id], [member_id], [favourite], [type]) VALUES(NULL, 1054009214000000000, 102717546000000000, 0, 'CurrentMembership')
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_assigning_association_correctly_assigns_target,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [memberships] ([joined_on], [club_id], [member_id], [favourite], [type]) VALUES(NULL, 1054009213, 1027175461000000000, 0, 'CurrentMembership')
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_creating_association_builds_through_record_for_new,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [memberships] ([joined_on], [club_id], [member_id], [favourite], [type]) VALUES(NULL, 1054009215000000000, 1027175462000000000, 0, 'CurrentMembership')
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_creating_association_creates_through_record,
      # <NoMethodError> exception expected but was
      # Class: <ArgumentError>
      # Message: <"wrong number of arguments (3 for 1)">
      # ---Backtrace---
      # attribute_methods.rb:232:in `method_missing'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/has_one_through_associations_test.rb:135:in `test_has_one_through_proxy_should_not_respond_to_priva
      :test_has_one_through_proxy_should_not_respond_to_private_methods,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: UPDATE [memberships] SET [club_id] = 1054009216000000000 WHERE [id] = 224856423
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_replace_target_record,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: UPDATE [memberships] SET [club_id] = 1054009217000000000 WHERE [id] = 224856423
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_replacing_target_record_deletes_old_association

    disable InheritanceComputeTypeTest, 
      # <ActiveRecord::SubclassNotFound> exception expected but was
      # Class: <ActiveRecord::RecordNotFound>
      # Message: <"Couldn't find Firm with ID=770">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/inheritance_test.rb:252:in `test_instantiation_doesnt_try_to_require_corresponding_file'
      # base.rb:1576:in `find_one'
      # base.rb:1559:i
      :test_instantiation_doesnt_try_to_require_corresponding_file

    disable InheritanceTest, 
      # <#<VerySpecialClient id: 780, type: nil, ruby_type: "VerySpecialClient", firm_id: nil, firm_name: nil, name: "veryspecial", client_of: nil, rating: 1>> expected but was
      # <#<VerySpecialClient id: 78, type: nil, ruby_type: "VerySpecialClient", firm_id: nil, firm_name: nil, name: "veryspecial", client_of: nil, rating: 1>>.
      # 
      # diff:
      # - #<VerySpecialClient id: 780, type: nil, ruby_type: "VerySpecialClient",
      :test_alt_complex_inheritance,
      # ActiveRecord::RecordNotFound: Couldn't find Company with ID=790
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/inheritance_test.rb:104:in `test_inheritance_save'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_alt_inheritance_save,
      # <#<VerySpecialClient id: 10100, type: "VerySpecialClient", ruby_type: nil, firm_id: nil, firm_name: nil, name: "veryspecial", client_of: nil, rating: 1>> expected but was
      # <#<VerySpecialClient id: 101, type: "VerySpecialClient", ruby_type: nil, firm_id: nil, firm_name: nil, name: "veryspecial", client_of: nil, rating: 1>>.
      # 
      # diff:
      # - #<VerySpecialClient id: 10100, type: "VerySpecialClient", ruby_type:
      :test_complex_inheritance,
      # ActiveRecord::RecordNotFound: Couldn't find Company with ID=10200
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/inheritance_test.rb:47:in `test_different_namespace_subclass_should_load_correctly_with_store_full_sti_class_option'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_different_namespace_subclass_should_load_correctly_with_store_full_sti_class_option,
      # ActiveRecord::RecordNotFound: Couldn't find Company with ID=10300
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_inheritance_save

    disable InvalidDateTest, 
      # Exception raised:
      # Class: <ActiveRecord::MultiparameterAssignmentErrors>
      # Message: <"1 error(s) on assignment of multiparameter attributes">
      # ---Backtrace---
      # base.rb:3040:in `execute_callstack_for_multiparameter_attributes'
      # base.rb:3026:in `assign_multiparameter_attributes'
      # base.rb:2734:in `attributes='
      # base.rb:2433:in `initialize'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/test
      :test_assign_valid_dates

    disable MigrationTableAndIndexTest, 
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_add_schema_info_respects_prefix_and_suffix

    disable MigrationTest, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_add_column_not_null_with_default,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_add_column_with_precision_and_scale,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_add_column_with_primary_key_attribute,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_add_drop_table_with_prefix_and_suffix,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_add_index,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_add_remove_single_field_using_string_arguments,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_add_remove_single_field_using_symbol_arguments,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Cannot insert the value NULL into column 'first_name', table 'activerecord_unittest.dbo.people'; column does not allow nulls. INSERT fails.
      # The statement has been terminated.: INSERT INTO [people] ([first_name], [primary_contact_id], [gender], [lock_version], [wealth], [last_name], [key], [administrator], [girlfriend]) VALUES(NULL, NULL, NULL, 0,
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_add_rename,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_add_table,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_add_table_with_decimals,
      # Exception raised:
      # Class: <TypeError>
      # Message: <"Invalid attempt to call Read when reader is closed.">
      # ---Backtrace---
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      # statement.rb:207:in `fetch'
      # statement.rb:236:in `each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Enumerable.cs:35:in `Each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Rub
      :test_change_column,
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_change_column_default,
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_change_column_default_to_null,
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_change_column_nullability,
      # Exception raised:
      # Class: <TypeError>
      # Message: <"Invalid attempt to call Read when reader is closed.">
      # ---Backtrace---
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      # statement.rb:207:in `fetch'
      # statement.rb:236:in `each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Enumerable.cs:35:in `Each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Rub
      :test_change_column_quotes_column_names,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'administrator' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [administrator] bit DEFAULT 1
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_change_column_with_new_default,
      # Exception raised:
      # Class: <TypeError>
      # Message: <"Invalid attempt to call Read when reader is closed.">
      # ---Backtrace---
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      # statement.rb:207:in `fetch'
      # statement.rb:236:in `each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Enumerable.cs:35:in `Each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Rub
      :test_change_column_with_nil_default,
      # Exception raised:
      # Class: <TypeError>
      # Message: <"Invalid attempt to call Read when reader is closed.">
      # ---Backtrace---
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      # statement.rb:207:in `fetch'
      # statement.rb:236:in `each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Enumerable.cs:35:in `Each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Rub
      :test_change_type_of_not_null_column,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_coerced_test_add_column_not_null_without_default,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_create_table_adds_id,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_create_table_with_binary_column,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_create_table_with_custom_sequence_name,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_create_table_with_defaults,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_create_table_with_force_true_does_not_drop_nonexisting_table,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_create_table_with_limits,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_create_table_with_not_null_column,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_create_table_with_primary_key_prefix_as_table_name,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_create_table_with_primary_key_prefix_as_table_name_with_underscore,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_create_table_with_timestamps_should_create_datetime_columns,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_create_table_with_timestamps_should_create_datetime_columns_with_options,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_create_table_without_id,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_finds_migrations,
      # StandardError: An error has occurred, this and all later migrations canceled:
      # 
      # DBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_finds_pending_migrations,
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_keeping_default_and_notnull_constaint_on_change,
      # <false> is not true.
      :test_migrator,
      # Exception raised:
      # Class: <StandardError>
      # Message: <"An error has occurred, this and all later migrations canceled:\n\nDBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)">
      # ---Backtrace---
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlse
      :test_migrator_db_has_no_schema_migrations_table,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_migrator_double_down,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_migrator_double_up,
      # StandardError: An error has occurred, this and all later migrations canceled:
      # 
      # DBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_migrator_going_down_due_to_version_target,
      # Exception raised:
      # Class: <StandardError>
      # Message: <"An error has occurred, this and all later migrations canceled:\n\nDBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)">
      # ---Backtrace---
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlse
      :test_migrator_interleaved_migrations,
      # StandardError: An error has occurred, this and all later migrations canceled:
      # 
      # DBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_migrator_one_down,
      # <false> is not true.
      :test_migrator_one_up,
      # StandardError: An error has occurred, this and all later migrations canceled:
      # 
      # DBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_migrator_one_up_one_down,
      # <false> is not true.
      :test_migrator_one_up_with_exception_and_rollback,
      # StandardError: An error has occurred, this and all later migrations canceled:
      # 
      # DBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_migrator_rollback,
      # StandardError: An error has occurred, this and all later migrations canceled:
      # 
      # DBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_migrator_verbosity,
      # StandardError: An error has occurred, this and all later migrations canceled:
      # 
      # DBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_migrator_verbosity_off,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_migrator_with_duplicate_names,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_migrator_with_duplicates,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_migrator_with_missing_version_numbers,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'wealth' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [wealth] decimal(30,10)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_native_decimal_insert_manual_vs_automatic,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_native_types,
      # StandardError: An error has occurred, this and all later migrations canceled:
      # 
      # DBI::DatabaseError: Column names in each table must be unique. Column name 'last_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [last_name] varchar(255)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_only_loads_pending_migrations,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_proper_table_name,
      # Exception raised:
      # Class: <TypeError>
      # Message: <"Invalid attempt to call Read when reader is closed.">
      # ---Backtrace---
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      # statement.rb:207:in `fetch'
      # statement.rb:236:in `each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Enumerable.cs:35:in `Each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Rub
      :test_remove_column_with_index,
      # Exception raised:
      # Class: <TypeError>
      # Message: <"Invalid attempt to call Read when reader is closed.">
      # ---Backtrace---
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      # statement.rb:207:in `fetch'
      # statement.rb:236:in `each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Enumerable.cs:35:in `Each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Rub
      :test_remove_column_with_multi_column_index,
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_rename_column,
      # ActiveRecord::ActiveRecordError: No such column: developers.anual_salary
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/lib/active_record/connection_adapters/sqlserver_adapter.rb:1089:in `column_for'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/lib/active_record/connection_adapters/sqlserver_adapter.rb:673:in `rename_column'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/migration_test.rb:576:in `test_rename_column_preserves_default_value_not_null'
      :test_rename_column_preserves_default_value_not_null,
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_rename_column_using_symbol_arguments,
      # Exception raised:
      # Class: <ActiveRecord::StatementInvalid>
      # Message: <"DBI::DatabaseError: The transaction operation cannot be performed because there are pending requests working on this transaction.: EXEC sp_rename 'hats.hat_name', 'name', 'COLUMN'">
      # ---Backtrace---
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:856:in `do_execute'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Rub
      :test_rename_column_with_an_index,
      # Exception raised:
      # Class: <ActiveRecord::StatementInvalid>
      # Message: <"DBI::DatabaseError: The transaction operation cannot be performed because there are pending requests working on this transaction.: EXEC sp_rename 'people.first_name', 'group', 'COLUMN'">
      # ---Backtrace---
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:856:in `do_execute'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Language
      :test_rename_column_with_sql_reserved_word,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_rename_nonexistent_column,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: The transaction operation cannot be performed because there are pending requests working on this transaction.: EXEC sp_rename 'octopuses', 'octopi'
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:856:in `do_execute'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/lib/active_record/connection_adapters/sqlserver_adapter.rb:630:in `rename_table'
      :test_rename_table,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: The transaction operation cannot be performed because there are pending requests working on this transaction.: EXEC sp_rename 'octopuses', 'octopi'
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:856:in `do_execute'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/lib/active_record/connection_adapters/sqlserver_adapter.rb:630:in `rename_table'
      :test_rename_table_with_an_index,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Column names in each table must be unique. Column name 'first_name' in table 'people' is specified more than once.: ALTER TABLE [people] ADD [first_name] varchar(40)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_schema_migrations_table_name

    disable MigrationTestSqlserver, 
      # Exception raised:
      # Class: <TypeError>
      # Message: <"Invalid attempt to call Read when reader is closed.">
      # ---Backtrace---
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      # statement.rb:207:in `fetch'
      # statement.rb:236:in `each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Enumerable.cs:35:in `Each'
      # d:\vs_langs01_s\Merlin\Main\Languages\Rub
      "test: For changing column should not raise exception when column contains default constraint. "

    disable NamedScopeTest, 
      # <false> is not true.
      :test_reload_expires_cache_of_found_items

    disable OptimisticLockingTest, 
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_decrement_counter_updates_custom_lock_version,
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_decrement_counter_updates_lock_version,
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_increment_counter_updates_custom_lock_version,
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_increment_counter_updates_lock_version,
      # ActiveRecord::StaleObjectError: Attempted to update a stale object
      # optimistic.rb:69:in `update_with_lock'
      # dirty.rb:142:in `update_with_dirty'
      # timestamp.rb:56:in `update_with_timestamps'
      :test_lock_column_is_mass_assignable,
      # ActiveRecord::StaleObjectError: Attempted to update a stale object
      # optimistic.rb:69:in `update_with_lock'
      # dirty.rb:142:in `update_with_dirty'
      # timestamp.rb:56:in `update_with_timestamps'
      :test_lock_new_with_nil,
      # ActiveRecord::RecordNotFound: Couldn't find ReadonlyFirstNamePerson with ID=220
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_readonly_attributes,
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_update_counters_updates_custom_lock_version,
      # TypeError: Invalid attempt to call Read when reader is closed.
      # System.Data:0:in `ReadInternal'
      # System.Data:0:in `Read'
      # statement.rb:48:in `fetch'
      :test_update_counters_updates_lock_version

    disable PrimaryKeysTest, 
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=690
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/pk_test.rb:24:in `test_integer_key'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_integer_key

    disable ReflectionTest, 
      # NameError: uninitialized constant ReflectionTest::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/reflection_test.rb:105:in `test_has_many_reflection'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_has_many_reflection,
      # NameError: uninitialized constant ReflectionTest::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/reflection_test.rb:117:in `test_has_one_reflection'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_has_one_reflection,
      # NameError: uninitialized constant ReflectionTest::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/reflection_test.rb:173:in `test_reflection_of_all_associations'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_reflection_of_all_associations,
      # Exception raised:
      # Class: <NameError>
      # Message: <"uninitialized constant ReflectionTest::Firm">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/reflection_test.rb:180:in `test_reflection_should_not_raise_error_when_compared_to_other_object'
      # dependencies.rb:90:in `const_missing'
      # assertions.rb:358:in `assert_nothing_raised
      :test_reflection_should_not_raise_error_when_compared_to_other_object

    disable SchemaDumperTest, 
      # </\# Could not dump table/> expected to not match
      # <"# This file is auto-generated from the current state of the database. Instead of editing this file, \n# please use the migrations feature of Active Record to incrementally modify your database, and\n# then regenerate this schema definition.\n#\n# Note that this schema.rb definition is the authoritative source for your database schema. If you need\
      :test_no_dump_errors,
      # <"# This file is auto-generated from the current state of the database. Instead of editing this file, \n# please use the migrations feature of Active Record to incrementally modify your database, and\n# then regenerate this schema definition.\n#\n# Note that this schema.rb definition is the authoritative source for your database schema. If you need\n# to create the application database on another s
      :test_schema_dump,
      # <"# This file is auto-generated from the current state of the database. Instead of editing this file, \n# please use the migrations feature of Active Record to incrementally modify your database, and\n# then regenerate this schema definition.\n#\n# Note that this schema.rb definition is the authoritative source for your database schema. If you need\n# to create the application database on another s
      :test_schema_dump_includes_camelcase_table_name,
      # <"# This file is auto-generated from the current state of the database. Instead of editing this file, \n# please use the migrations feature of Active Record to incrementally modify your database, and\n# then regenerate this schema definition.\n#\n# Note that this schema.rb definition is the authoritative source for your database schema. If you need\n# to create the application database on another s
      :test_schema_dump_includes_decimal_options,
      # <"# This file is auto-generated from the current state of the database. Instead of editing this file, \n# please use the migrations feature of Active Record to incrementally modify your database, and\n# then regenerate this schema definition.\n#\n# Note that this schema.rb definition is the authoritative source for your database schema. If you need\n# to create the application database on another s
      :test_schema_dump_includes_limit_constraint_for_integer_columns,
      # <"# This file is auto-generated from the current state of the database. Instead of editing this file, \n# please use the migrations feature of Active Record to incrementally modify your database, and\n# then regenerate this schema definition.\n#\n# Note that this schema.rb definition is the authoritative source for your database schema. If you need\n# to create the application database on another s
      :test_schema_dump_includes_not_null_columns,
      # goofy_string_id table not found.
      # <nil> expected to not be nil.
      :test_schema_dump_keeps_id_column_when_id_is_false_and_id_column_added,
      # nonstandardpk table not found.
      # <nil> expected to not be nil.
      :test_schema_dump_should_honor_nonstandard_primary_keys,
      # <"# This file is auto-generated from the current state of the database. Instead of editing this file, \n# please use the migrations feature of Active Record to incrementally modify your database, and\n# then regenerate this schema definition.\n#\n# Note that this schema.rb definition is the authoritative source for your database schema. If you need\n# to create the application database on another s
      :test_schema_dump_with_regexp_ignored_table,
      # <"# This file is auto-generated from the current state of the database. Instead of editing this file, \n# please use the migrations feature of Active Record to incrementally modify your database, and\n# then regenerate this schema definition.\n#\n# Note that this schema.rb definition is the authoritative source for your database schema. If you need\n# to create the application database on another s
      :test_schema_dump_with_string_ignored_table,
      # NoMethodError: undefined method `strip' for nil:NilClass
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/schema_dumper_test.rb:156:in `test_schema_dumps_index_columns_in_right_order'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_schema_dumps_index_columns_in_right_order

    disable SchemaDumperTestSqlserver, 
      # <"# This file is auto-generated from the current state of the database. Instead of editing this file, \n# please use the migrations feature of Active Record to incrementally modify your database, and\n# then regenerate this schema definition.\n#\n# Note that this schema.rb definition is the authoritative source for your database schema. If you need\n# to create the application database on another s
      "test: For integers should include limit constraint that match logic for smallint and bigint in #extract_limit. ",
      # nonstandardpk table not found.
      # <nil> expected to not be nil.
      "test: For primary keys should honor nonstandards. ",
      # <"# This file is auto-generated from the current state of the database. Instead of editing this file, \n# please use the migrations feature of Active Record to incrementally modify your database, and\n# then regenerate this schema definition.\n#\n# Note that this schema.rb definition is the authoritative source for your database schema. If you need\n# to create the application database on another s
      "test: For strings should have varchar(max) dumped as text. "

    disable TestAutosaveAssociationOnABelongsToAssociation, 
      # ActiveRecord::RecordNotFound: Couldn't find Ship with ID=67406967500000000
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_should_automatically_save_bang_the_associated_model,
      # ActiveRecord::RecordNotFound: Couldn't find Ship with ID=67406967600000000
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_should_automatically_save_the_associated_model,
      # NoMethodError: undefined method `catchphrase' for nil:NilClass
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:884:in `test_should_rollback_any_changes_if_an_exception_occurred_while_saving'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_should_rollback_any_changes_if_an_exception_occurred_while_saving,
      # ActiveRecord::RecordNotFound: Couldn't find Ship with ID=67406968100000000
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_should_still_allow_to_bypass_validations_on_the_associated_model,
      # ActiveRecord::RecordNotFound: Couldn't find Ship with ID=67406968300000000
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_should_still_work_without_an_associated_model

    disable TestAutosaveAssociationOnAHasAndBelongsToManyAssociation, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906345600000000, 52147199600000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_allow_to_bypass_validations_on_the_associated_models_on_create,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906345700000000, 52147199700000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_allow_to_bypass_validations_on_the_associated_models_on_update,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906345800000000, 52147199800000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_save_bang_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906345900000000, 52147199900000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_save_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (4390634600000000, 52147200000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_validate_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906346100000000, 52147200100000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_merge_errors_on_the_associated_models_onto_the_parent_even_if_it_is_not_valid,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906346200000000, 52147200200000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_load_the_associated_models_if_they_were_not_loaded_yet,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906346300000000, 52147200300000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_use_default_invalid_error_on_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906346400000000, 52147200400000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_rollback_any_changes_if_an_exception_occurred_while_saving,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906346500000000, 52147200500000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_still_raise_an_ActiveRecordRecord_Invalid_exception_if_we_want_that,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906346600000000, 52147200600000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_validation_the_associated_models_on_create

    disable TestAutosaveAssociationOnAHasManyAssociation, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147200700000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_allow_to_bypass_validations_on_the_associated_models_on_create,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147200800000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_allow_to_bypass_validations_on_the_associated_models_on_update,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147200900000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_save_bang_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 5214720100000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_save_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147201100000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_validate_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147201200000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_merge_errors_on_the_associated_models_onto_the_parent_even_if_it_is_not_valid,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147201300000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_load_the_associated_models_if_they_were_not_loaded_yet,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147201400000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_use_default_invalid_error_on_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147201500000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_rollback_any_changes_if_an_exception_occurred_while_saving,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147201600000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_still_raise_an_ActiveRecordRecord_Invalid_exception_if_we_want_that,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147201700000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_validation_the_associated_models_on_create

    disable TestAutosaveAssociationOnAHasOneAssociation, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147201800000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_allow_to_bypass_validations_on_associated_models_at_any_depth,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147201900000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_save_bang_the_associated_model,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 5214720200000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_save_the_associated_model,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147202100000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_validate_the_associated_model,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147202200000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_merge_errors_on_the_associated_models_onto_the_parent_even_if_it_is_not_valid,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147202300000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_load_the_associated_model,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147202400000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_rollback_any_changes_if_an_exception_occurred_while_saving,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147202500000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_still_allow_to_bypass_validations_on_the_associated_model,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147202600000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_still_raise_an_ActiveRecordRecord_Invalid_exception_if_we_want_that,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147202700000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_still_work_without_an_associated_model

    disable TestAutosaveAssociationValidationsOnAHABTMAssocication, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906346700000000, 5214720300000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      "test_should_automatically_validate_associations_with_:validate_=>_true",
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906346800000000, 52147203100000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      "test_should_not_automatically_validate_associations_without_:validate_=>_true"

    disable TestAutosaveAssociationValidationsOnAHasManyAssocication, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('cookoo', 52147203200000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_validate_associations

    disable TestAutosaveAssociationValidationsOnAHasOneAssocication, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('titanic', 52147203300000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      "test_should_automatically_validate_associations_with_:validate_=>_true",
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('titanic', 52147203400000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      "test_should_not_automatically_validate_associations_without_:validate_=>_true"

    disable TestDefaultAutosaveAssociationOnABelongsToAssociation, 
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnABelongsToAssociation::Client
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:184:in `test_assignment_before_either_saved'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_assignment_before_either_saved,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnABelongsToAssociation::Client
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:171:in `test_assignment_before_parent_saved'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_assignment_before_parent_saved,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnABelongsToAssociation::Client
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:141:in `test_should_save_parent_but_not_invalid_child'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_should_save_parent_but_not_invalid_child,
      # <#<Customer id: 42400, name: nil, balance: 0, address_street: nil, address_city: nil, address_country: nil, gps_location: nil>> expected but was
      # <nil>.
      :test_store_association_in_two_relations_with_one_save,
      # <#<Customer id: 42500, name: nil, balance: 0, address_street: nil, address_city: nil, address_country: nil, gps_location: nil>> expected but was
      # <nil>.
      :test_store_association_in_two_relations_with_one_save_in_existing_object,
      # <#<Customer id: 42700, name: nil, balance: 0, address_street: nil, address_city: nil, address_country: nil, gps_location: nil>> expected but was
      # <nil>.
      :test_store_association_in_two_relations_with_one_save_in_existing_object_with_values,
      # <#<Customer id: 42800, name: nil, balance: 0, address_street: nil, address_city: nil, address_country: nil, gps_location: nil>> expected but was
      # <nil>.
      :test_store_two_association_with_one_save

    disable TestDefaultAutosaveAssociationOnAHasManyAssociation, 
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasManyAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:343:in `test_adding_before_save'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_adding_before_save,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasManyAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:368:in `test_assign_ids'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_assign_ids,
      # ActiveRecord::RecordNotFound: Couldn't find Post with ID=320
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_assign_ids_for_through_a_belongs_to,
      # ActiveRecord::SubclassNotFound: The single-table inheritance mechanism failed to locate the subclass: 'Firm'. This error is raised because the column 'type' is reserved for storing the class in case of inheritance. Please rename this column if you didn't intend it to be used for storing the inheritance class or overwrite Company.inheritance_column to use another column for that information.
      # base.rb:1620:in `instantiate'
      # base.rb:661:in `find_by_sql'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Extensions\IListOps.cs:666:in `collect!'
      :test_build_before_save,
      # ActiveRecord::SubclassNotFound: The single-table inheritance mechanism failed to locate the subclass: 'Firm'. This error is raised because the column 'type' is reserved for storing the class in case of inheritance. Please rename this column if you didn't intend it to be used for storing the inheritance class or overwrite Company.inheritance_column to use another column for that information.
      # base.rb:1620:in `instantiate'
      # base.rb:661:in `find_by_sql'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Extensions\IListOps.cs:666:in `collect!'
      :test_build_many_before_save,
      # ActiveRecord::SubclassNotFound: The single-table inheritance mechanism failed to locate the subclass: 'Firm'. This error is raised because the column 'type' is reserved for storing the class in case of inheritance. Please rename this column if you didn't intend it to be used for storing the inheritance class or overwrite Company.inheritance_column to use another column for that information.
      # base.rb:1620:in `instantiate'
      # base.rb:661:in `find_by_sql'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Extensions\IListOps.cs:666:in `collect!'
      :test_build_many_via_block_before_save,
      # ActiveRecord::SubclassNotFound: The single-table inheritance mechanism failed to locate the subclass: 'Firm'. This error is raised because the column 'type' is reserved for storing the class in case of inheritance. Please rename this column if you didn't intend it to be used for storing the inheritance class or overwrite Company.inheritance_column to use another column for that information.
      # base.rb:1620:in `instantiate'
      # base.rb:661:in `find_by_sql'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Extensions\IListOps.cs:666:in `collect!'
      :test_build_via_block_before_save,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasManyAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:283:in `test_invalid_adding'
      # dependencies.rb:90:in `const_missing'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      :test_invalid_adding,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasManyAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:292:in `test_invalid_adding_before_save'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_invalid_adding_before_save,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasManyAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:305:in `test_invalid_adding_with_validate_false'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_invalid_adding_with_validate_false,
      # ActiveRecord::SubclassNotFound: The single-table inheritance mechanism failed to locate the subclass: 'Firm'. This error is raised because the column 'type' is reserved for storing the class in case of inheritance. Please rename this column if you didn't intend it to be used for storing the inheritance class or overwrite Company.inheritance_column to use another column for that information.
      # base.rb:1620:in `instantiate'
      # base.rb:661:in `find_by_sql'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Extensions\IListOps.cs:666:in `collect!'
      :test_invalid_build,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasManyAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:430:in `test_replace_on_new_object'
      # dependencies.rb:90:in `const_missing'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      :test_replace_on_new_object,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasManyAssociation::Client
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:316:in `test_valid_adding_with_validate_false'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_valid_adding_with_validate_false

    disable TestDefaultAutosaveAssociationOnAHasOneAssociation, 
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasOneAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:108:in `test_assignment_before_either_saved'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_assignment_before_either_saved,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasOneAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:98:in `test_assignment_before_parent_saved'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_assignment_before_parent_saved,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasOneAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:76:in `test_build_before_child_saved'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_build_before_child_saved,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasOneAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:87:in `test_build_before_either_saved'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_build_before_either_saved,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasOneAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:121:in `test_not_resaved_when_unchanged'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_not_resaved_when_unchanged,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasOneAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:53:in `test_save_fails_for_invalid_has_one'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_save_fails_for_invalid_has_one,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasOneAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:65:in `test_save_succeeds_for_invalid_has_one_with_validate_false'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_save_succeeds_for_invalid_has_one_with_validate_false,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnAHasOneAssociation::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:42:in `test_should_save_parent_but_not_invalid_child'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_should_save_parent_but_not_invalid_child

    disable TestDefaultAutosaveAssociationOnNewRecord, 
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnNewRecord::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:442:in `test_autosave_new_record_on_belongs_to_can_be_disabled_per_relationship'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_autosave_new_record_on_belongs_to_can_be_disabled_per_relationship,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnNewRecord::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:483:in `test_autosave_new_record_on_has_many_can_be_disabled_per_relationship'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_autosave_new_record_on_has_many_can_be_disabled_per_relationship,
      # NameError: uninitialized constant TestDefaultAutosaveAssociationOnNewRecord::Firm
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:461:in `test_autosave_new_record_on_has_one_can_be_disabled_per_relationship'
      # dependencies.rb:90:in `const_missing'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_autosave_new_record_on_has_one_can_be_disabled_per_relationship

    disable TestDestroyAsPartOfAutosaveAssociation, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147203500000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_a_child_marked_for_destruction_should_not_be_destroyed_twice,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147203600000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_a_child_marked_for_destruction_should_not_be_destroyed_twice_while_saving_birds,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147203700000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_a_child_marked_for_destruction_should_not_be_destroyed_twice_while_saving_parrots,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147203800000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_a_marked_for_destruction_record_should_not_be_be_marked_after_reload,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147203900000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_a_parent_marked_for_destruction_should_not_be_destroyed_twice,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 5214720400000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_destroy_a_child_association_as_part_of_the_save_transaction_if_it_was_marked_for_destroyal,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147204100000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_destroy_a_parent_association_as_part_of_the_save_transaction_if_it_was_marked_for_destroyal,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147204200000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_destroy_birds_as_part_of_the_save_transaction_if_they_were_marked_for_destroyal,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147204300000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_destroy_parrots_as_part_of_the_save_transaction_if_they_were_marked_for_destroyal,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147204400000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_rollback_destructions_if_an_exception_occurred_while_saving_a_child,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147204500000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_rollback_destructions_if_an_exception_occurred_while_saving_a_parent,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147204600000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_rollback_destructions_if_an_exception_occurred_while_saving_birds,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147204700000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_rollback_destructions_if_an_exception_occurred_while_saving_parrots,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147204800000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_run_add_callback_methods_for_birds,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147204900000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_run_add_callback_methods_for_parrots,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 5214720500000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_run_add_callback_procs_for_birds,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147205100000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_run_add_callback_procs_for_parrots,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147205200000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_run_remove_callback_methods_for_birds,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147205300000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_run_remove_callback_methods_for_parrots,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147205400000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_run_remove_callback_procs_for_birds,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147205500000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_run_remove_callback_procs_for_parrots,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147205600000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_skip_validation_on_a_child_association_if_marked_for_destruction,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147205700000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_skip_validation_on_a_parent_association_if_marked_for_destruction,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147205800000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_skip_validation_on_the_birds_association_if_destroyed,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147205900000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_skip_validation_on_the_birds_association_if_marked_for_destruction,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 5214720600000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_skip_validation_on_the_parrots_association_if_destroyed,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147206100000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_skip_validation_on_the_parrots_association_if_marked_for_destruction

    disable TestNestedAttributesInGeneral, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Red Pearl', 52147206200000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_reject_if_method_with_arguments,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Hello Pearl', 52147206400000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_reject_if_with_indifferent_keys,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147206500000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_disable_allow_destroy_by_default

    disable TestNestedAttributesLimit, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906346900000000, 52147206700000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_limit_with_less_records,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (4390634700000000, 52147206800000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_limit_with_number_exact_records

    disable TestNestedAttributesOnABelongsToAssociation, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147206900000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_enable_autosave_on_the_association,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 5214720700000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_build_a_new_record_if_there_is_no_id,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147207100000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_define_an_attribute_writer_method_for_the_association,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147207200000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_destroy_an_existing_record_if_there_is_a_matching_id_and_destroy_is_truthy,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147207300000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_modify_an_existing_record_if_there_is_a_matching_composite_id,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147207400000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_modify_an_existing_record_if_there_is_a_matching_id,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147207500000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_build_a_new_record_if_a_reject_if_proc_returns_false,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147207600000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_build_a_new_record_if_there_is_no_id_and_destroy_is_truthy,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147207700000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_destroy_an_existing_record_if_allow_destroy_is_false,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147207800000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_destroy_an_existing_record_if_destroy_is_not_truthy,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147207900000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_destroy_the_associated_model_until_the_parent_is_saved,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 5214720800000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_replace_an_existing_record_if_there_is_no_id_and_destroy_is_truthy,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147208100000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_replace_an_existing_record_if_there_is_no_id,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147208200000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_take_a_hash_with_string_keys_and_update_the_associated_model,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147208300000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_work_with_update_attributes_as_well

    disable TestNestedAttributesOnAHasAndBelongsToManyAssociation, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906347100000000, 52147208400000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_also_work_with_a_HashWithIndifferentAccess,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906347200000000, 52147208500000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_build_new_associated_models_for_each_entry_in_a_hash_where_the_id_is_missing,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906347300000000, 52147208600000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_enable_autosave_on_the_association,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906347400000000, 52147208700000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_be_possible_to_destroy_a_record,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906347500000000, 52147208800000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_define_an_attribute_writer_method_for_the_association,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906347600000000, 52147208900000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_ignore_new_associated_records_if_a_reject_if_proc_returns_false,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906347700000000, 5214720900000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_ignore_new_associated_records_with_truthy_destroy_attribute,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906347800000000, 52147209100000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_assign_destroy_key_to_a_record,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906347900000000, 52147209200000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_destroy_the_associated_model_until_the_parent_is_saved,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (4390634800000000, 52147209300000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_destroy_the_associated_model_with_a_non_truthy_argument,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906348100000000, 52147209400000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_raise_an_argument_error_if_something_else_than_a_hash_is_passed,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906348200000000, 52147209500000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_sort_the_hash_by_the_keys_before_building_new_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906348300000000, 52147209600000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_take_a_hash_and_assign_the_attributes_to_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906348400000000, 52147209700000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_take_a_hash_with_composite_id_keys_and_assign_the_attributes_to_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906348500000000, 52147209800000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_take_a_hash_with_string_keys_and_assign_the_attributes_to_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906348600000000, 52147209900000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_take_an_array_and_assign_the_attributes_to_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906348700000000, 521472100000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_update_existing_records_and_add_new_ones_that_have_no_id,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [parrots_pirates] ([parrot_id], [pirate_id]) VALUES (43906348800000000, 52147210100000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_work_with_update_attributes_as_well

    disable TestNestedAttributesOnAHasManyAssociation, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147210200000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_also_work_with_a_HashWithIndifferentAccess,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147210300000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_build_new_associated_models_for_each_entry_in_a_hash_where_the_id_is_missing,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147210400000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_enable_autosave_on_the_association,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147210500000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_be_possible_to_destroy_a_record,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147210600000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_define_an_attribute_writer_method_for_the_association,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147210700000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_ignore_new_associated_records_if_a_reject_if_proc_returns_false,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147210800000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_ignore_new_associated_records_with_truthy_destroy_attribute,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147210900000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_assign_destroy_key_to_a_record,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 5214721100000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_destroy_the_associated_model_until_the_parent_is_saved,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147211100000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_destroy_the_associated_model_with_a_non_truthy_argument,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147211200000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_raise_an_argument_error_if_something_else_than_a_hash_is_passed,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147211300000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_sort_the_hash_by_the_keys_before_building_new_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147211400000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_take_a_hash_and_assign_the_attributes_to_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147211500000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_take_a_hash_with_composite_id_keys_and_assign_the_attributes_to_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147211600000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_take_a_hash_with_string_keys_and_assign_the_attributes_to_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147211700000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_take_an_array_and_assign_the_attributes_to_the_associated_models,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147211800000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_update_existing_records_and_add_new_ones_that_have_no_id,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [birds] ([name], [pirate_id]) VALUES('Posideons Killer', 52147211900000000)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_work_with_update_attributes_as_well

    disable TestNestedAttributesOnAHasOneAssociation, 
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 5214721200000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_also_work_with_a_HashWithIndifferentAccess,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147212100000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_automatically_enable_autosave_on_the_association,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147212200000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_build_a_new_record_if_there_is_no_id,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147212300000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_define_an_attribute_writer_method_for_the_association,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147212400000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_destroy_an_existing_record_if_there_is_a_matching_id_and_destroy_is_truthy,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147212500000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_modify_an_existing_record_if_there_is_a_matching_composite_id,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147212600000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_modify_an_existing_record_if_there_is_a_matching_id,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147212700000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_build_a_new_record_if_a_reject_if_proc_returns_false,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147212800000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_build_a_new_record_if_there_is_no_id_and_destroy_is_truthy,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147212900000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_destroy_an_existing_record_if_allow_destroy_is_false,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 5214721300000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_destroy_an_existing_record_if_destroy_is_not_truthy,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147213100000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_destroy_the_associated_model_until_the_parent_is_saved,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147213200000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_not_replace_an_existing_record_if_there_is_no_id_and_destroy_is_truthy,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147213300000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_raise_argument_error_if_trying_to_build_polymorphic_belongs_to,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147213400000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_replace_an_existing_record_if_there_is_no_id,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147213500000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_take_a_hash_with_string_keys_and_update_the_associated_model,
      # ActiveRecord::StatementInvalid: DBI::DatabaseError: Arithmetic overflow error converting expression to data type int.
      # The statement has been terminated.: INSERT INTO [ships] ([name], [pirate_id], [created_at], [created_on], [updated_at], [updated_on]) VALUES('Nights Dirty Lightning', 52147213600000000, NULL, NULL, NULL, NULL)
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:403:in `execute'
      :test_should_work_with_update_attributes_as_well

    disable UnicodeTestSqlserver, 
      # IronRuby::Builtins::EncodingCompatibilityError: incompatible character encodings: ASCII-8BIT and utf-8
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Extensions\IListOps.cs:1343:in `Join'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Extensions\IListOps.cs:1365:in `join'
      # diff.rb:718:in `diff'
      "test: Testing unicode data should insert into nvarchar field. "

    disable ValidationsTest, 
      # <false> is not true.
      :test_optionally_validates_length_of_using_within_on_create_utf8,
      # <false> is not true.
      :test_optionally_validates_length_of_using_within_on_update_utf8,
      # #<ActiveRecord::Errors:0x010264a @base=#<Topic id: nil, title: "?????", author_name: nil, author_email_address: "test@test.com", written_on: nil, bonus_time: nil, last_read: nil, content: nil, approved: true, replies_count: 0, parent_id: nil, parent_title: nil, type: nil>, @errors=#<OrderedHash {"title"=>[#<ActiveRecord::Error:0x0102684 @base=#<Topic id: nil, title: "?????", aut
      :test_optionally_validates_length_of_using_within_utf8,
      # Should still save t as unique.
      # <false> is not true.
      :test_validate_case_insensitive_uniqueness,
      # Should still save t as unique.
      # <false> is not true.
      :test_validate_case_sensitive_uniqueness,
      # Should still save t as unique.
      # <false> is not true.
      :test_validate_uniqueness,
      # Saving r1.
      # <false> is not true.
      :test_validate_uniqueness_scoped_to_defining_class,
      # Saving r1.
      # <false> is not true.
      :test_validate_uniqueness_with_scope,
      # Saving r1.
      # <false> is not true.
      :test_validate_uniqueness_with_scope_array,
      # <false> is not true.
      :test_validates_length_of_using_is_utf8,
      # <false> is not true.
      :test_validates_length_of_using_maximum_utf8,
      # <false> is not true.
      :test_validates_length_of_using_minimum_utf8,
      # <"is too short (minimum is 3 characters)"> expected but was
      # <"is too long (maximum is 5 characters)">.
      # 
      # diff:
      # - is too short (minimum is 3 characters)
      # ?        ^^ ^^   ^^        ^
      # + is too long (maximum is 5 characters)
      # ?        ^ ^^   ^^        ^
      :test_validates_length_of_using_within_utf8

    disable_incremental
  end
  
  def disable_incremental
    disable BasicsTest, 
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=160
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:254:in `test_create_through_factory'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_create_through_factory,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2755:in `attributes'
      :test_set_attributes,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/attribute_methods.rb:211:in `content'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/serializers/xml_serializer.rb:199:in `serializable_attributes'
      :test_to_xml_including_has_many_association,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/attribute_methods.rb:211:in `content'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/serializers/xml_serializer.rb:199:in `serializable_attributes'
      :test_to_xml_skipping_attributes,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2755:in `attributes'
      :test_toggle_attribute,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/attribute_methods.rb:211:in `content'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:655:in `test_update_all'
      :test_update_all,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/attribute_methods.rb:211:in `content'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:669:in `test_update_all_with_hash'
      :test_update_all_with_hash,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2755:in `attributes'
      :test_update_attribute,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2755:in `attributes'
      :test_update_attributes,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/attribute_methods.rb:211:in `content'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/base_test.rb:721:in `test_update_by_condition'
      :test_update_by_condition,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:804:in `__send__'
      :test_update_many

    disable BelongsToAssociationsTest, 
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=220
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      # base.rb:607:in `find'
      :test_belongs_to_counter_after_save,
      # ActiveRecord::RecordNotFound: Couldn't find Topic with ID=240
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations/belongs_to_associations_test.rb:155:in `test_belongs_to_with_primary_key_counter'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_belongs_to_with_primary_key_counter,
      # <#<Firm id: 180, type: "Firm", ruby_type: nil, firm_id: nil, firm_name: nil, name: "Apple", client_of: nil, rating: 1>> expected but was
      # <nil>.
      :test_creating_the_belonging_object

    disable ConnectionTestSqlserver, 
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:804:in `__send__'
      "test: ConnectionSqlserver should affect rows. ",
      # TypeError: There is already an open DataReader associated with this Command which must be closed first.
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateCommand'
      "test: ConnectionSqlserver should return finished DBI statment handle from #execute without block. "

    disable CustomConnectionFixturesTest, 
      # TypeError: There is already an open DataReader associated with this Command which must be closed first.
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateCommand'
      :test_connection

    disable DatabaseConnectedJsonEncodingTest, 
      # TypeError: There is already an open DataReader associated with this Command which must be closed first.
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateConnectionForExecute'
      # System.Data:0:in `ValidateCommand'
      :test_includes_fetches_nth_level_associations

    disable DirtyTest, 
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a Hash
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2983:in `attributes_with_quotes'
      :test_save_should_not_save_serialized_attribute_with_partial_updates_if_not_present,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a Hash
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2983:in `attributes_with_quotes'
      :test_save_should_store_serialized_attributes_even_with_partial_updates

    disable HasAndBelongsToManyAssociationsTest, 
      # <[#<Developer id: 230, name: "Jamis", salary: 70000, created_at: "2010-01-24 06:56:39", updated_at: "2010-01-24 06:56:39">,
      #  #<Developer id: 11, name: "Jamis", salary: 9000, created_at: "2010-01-24 06:56:32", updated_at: "2010-01-24 06:56:32">,
      #  #<Developer id: 2, name: "Jamis", salary: 150000, created_at: "2010-01-24 06:56:32", updated_at: "2010-01-24 06:56:32">]> expected but was
      # <[#<Developer id
      :test_dynamic_find_all_should_respect_association_order

    disable LifecycleTest, 
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2755:in `attributes'
      :test_after_save

    disable MethodScopingTest, 
      # <false> is not true.
      :test_scoped_create

    disable OptimisticLockingTest, 
      # ActiveRecord::RecordNotFound: Couldn't find Person with ID=110
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/locking_test.rb:64:in `test_lock_new'
      # base.rb:1576:in `find_one'
      # base.rb:1559:in `find_from_ids'
      :test_lock_new

    disable TransactionTest, 
      # <"Make the transaction rollback"> expected but was
      # <"content was supposed to be a BasicsTest::MyObject, but was a String">.
      :test_callback_rollback_in_create,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2755:in `attributes'
      :test_force_savepoint_in_nested_transaction,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2755:in `attributes'
      :test_manually_rolling_back_a_transaction,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:804:in `__send__'
      :test_many_savepoints,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2755:in `attributes'
      :test_nested_explicit_transactions,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2755:in `attributes'
      :test_no_savepoint_in_nested_transaction_without_force,
      # <"Make the transaction rollback"> expected but was
      # <"content was supposed to be a BasicsTest::MyObject, but was a String">.
      :test_raising_exception_in_callback_rollbacks_in_save,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2755:in `attributes'
      :test_successful,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2755:in `attributes'
      :test_successful_with_instance_method,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2755:in `attributes'
      :test_successful_with_return

    disable TransactionsWithTransactionalFixturesTest, 
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2755:in `attributes'
      :test_no_automatic_savepoint_for_inner_transaction

    disable ValidationsTest, 
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2983:in `attributes_with_quotes'
      :test_if_validation_using_block_false,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-2.3.5/lib/active_record/attribute_methods.rb:211:in `content'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/validations_test.rb:1396:in `test_if_validation_using_block_true'
      :test_if_validation_using_block_true,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2983:in `attributes_with_quotes'
      :test_if_validation_using_method_false,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2983:in `attributes_with_quotes'
      :test_if_validation_using_string_false,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2983:in `attributes_with_quotes'
      :test_optionally_validates_length_of_using_is,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2983:in `attributes_with_quotes'
      :test_optionally_validates_length_of_using_maximum,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2983:in `attributes_with_quotes'
      :test_optionally_validates_length_of_using_minimum,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:211:in `content'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_optionally_validates_length_of_using_within,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:211:in `content'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_optionally_validates_length_of_using_within_on_create,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2983:in `attributes_with_quotes'
      :test_optionally_validates_length_of_using_within_on_update,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/validations_test.rb:1406:in `test_unless_validation_using_block_true'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:211:in `content'
      :test_unless_validation_using_block_true,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2983:in `attributes_with_quotes'
      :test_unless_validation_using_method_true,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2983:in `attributes_with_quotes'
      :test_unless_validation_using_string_true,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:211:in `content'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_validate_format,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:211:in `content'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_validate_format_numeric,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:211:in `content'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_validate_presences,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2983:in `attributes_with_quotes'
      :test_validates_associated_many,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:211:in `content'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_validates_associated_one,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:211:in `content'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_validates_each,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      # base.rb:2983:in `attributes_with_quotes'
      :test_validates_exclusion_of,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/validations_test.rb:741:in `test_validates_exclusion_of_with_formatted_message'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      :test_validates_exclusion_of_with_formatted_message,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/validations_test.rb:661:in `test_validates_inclusion_of'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      :test_validates_inclusion_of,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/validations_test.rb:681:in `test_validates_inclusion_of_with_allow_nil'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      :test_validates_inclusion_of_with_allow_nil,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/validations_test.rb:714:in `test_validates_inclusion_of_with_formatted_message'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      :test_validates_inclusion_of_with_formatted_message,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/validations_test.rb:883:in `test_validates_length_of_using_is'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      :test_validates_length_of_using_is,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/validations_test.rb:784:in `test_validates_length_of_using_maximum'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      :test_validates_length_of_using_maximum,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/validations_test.rb:752:in `test_validates_length_of_using_minimum'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:266:in `read_attribute'
      :test_validates_length_of_using_minimum,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/validations_test.rb:813:in `test_validates_length_of_using_within'
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:211:in `content'
      :test_validates_length_of_using_within,
      # ActiveRecord::SerializationTypeMismatch: content was supposed to be a BasicsTest::MyObject, but was a String
      # attribute_methods.rb:293:in `unserialize_attribute'
      # attribute_methods.rb:211:in `content'
      # d:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\KernelOps.cs:783:in `__send__'
      :test_validates_length_of_with_block

    disable HasManyAssociationsTest,
      # "Client.count" didn't change by -2.
      # <2> expected but was
      # <3>.
      :test_destroying_a_collection
      
  end
end
