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
    
    # Check if we should use a private version of ironruby-dbi (for ironruby-dbi development scenarios)
    ironruby_dbi_path = ENV['IRONRUBY_DBI']
    if ironruby_dbi_path
      abort "Could not find %IRONRUBY_DBI%/lib/dbd/mssql.rb" if not File.exist?(File.expand_path('lib/dbd/mssql.rb', ironruby_dbi_path))
      gem "dbi", "= 0.4.3"
      require 'dbi'
      $LOAD_PATH.unshift(File.expand_path('lib', ironruby_dbi_path))
      require "dbd/MSSQL"
      puts "Using ironruby-dbi from #{ironruby_dbi_path}"
    end
    
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

  def disable_critical_failures
    # If this test executes, all subsequent tests start failing during setup with an exception
    # saying "The server failed to resume the transaction", presumably because
    # teardown did not happen properly for this test, and the active transaction was not aborted.
    disable FinderTest, :test_exists
  end
  
  def disable_unstable_tests
    # These failures can be reproduced by running with "set IR_CULTURE=nl-BE".
    if System::Threading::Thread.CurrentThread.CurrentCulture != 'en-US'
      disable BasicsTest,
        # <#<BigDecimal:0x005a5e0,'0.158643E4',9(9)>> expected but was
        # <#<BigDecimal:0x005a5ec,'0.1586E4',9(9)>>.
        # diff:
        # - #<BigDecimal:0x005a5e0,'0.158643E4',9(9)>
        # ?                      ^        --
        # + #<BigDecimal:0x005a5ec,'0.1586E4',9(9)>
        # ?                      ^
        :test_numeric_fields
        
      disable CalculationsTest,
        # <19.83> expected but was
        # <#<BigDecimal:0x0062216,'0.19E2',9(9)>>.
        :test_sum_should_return_valid_values_for_decimals
        
      disable MigrationTest,
        # <#<BigDecimal:0x00ba56e,'0.158643E4',9(9)>> expected but was
        # <#<BigDecimal:0x00ba57a,'0.1586E4',9(9)>>.
        # diff:
        # - #<BigDecimal:0x00ba56e,'0.158643E4',9(9)>
        # ?                     ^^        --
        # + #<BigDecimal:0x00ba57a,'0.1586E4',9(9)>
        # ?                     ^^
        :test_add_table_with_decimals
    end
  end

  def disable_tests

    disable AdapterTestSqlserver, 
      # <"#<ActiveRecord::ConnectionAdapters::SQLServerAdapter version: 2.3, year: 2005, connection_options: [\"DBI:MSSQL:server=SBORDE1\\\\SQLEXPRESS;initial catalog=activerecord_unittest;integrated security=true\"]>"> expected to be =~
      # </version\: \d.\d.\d/>.
      "test: For abstract behavior should include version in inspect. ",
      # ActiveRecord::StatementInvalid: NoMethodError: undefined method `[]' for nil:NilClass: SELECT TOP 1 * FROM [sql_server_chronics] WHERE ([sql_server_chronics].[datetime] = '2012-11-08 10:24:36.003') 
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:862:in `raw_select'
      "test: For chronic data types with a usec finding existing DB objects should find 003 millisecond in the DB with before and after casting. ",
      # ActiveRecord::StatementInvalid: NoMethodError: undefined method `[]' for nil:NilClass: SELECT TOP 1 * FROM [sql_server_chronics] WHERE ([sql_server_chronics].[datetime] = '2012-11-08 10:24:36.123') 
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:862:in `raw_select'
      "test: For chronic data types with a usec finding existing DB objects should find 123 millisecond in the DB with before and after casting. ",
      # ActiveRecord::StatementInvalid: NoMethodError: undefined method `[]' for nil:NilClass: SELECT * FROM [sql_server_chronics] WHERE ([sql_server_chronics].[id] = 7) 
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:862:in `raw_select'
      "test: For chronic data types with a usec saving new datetime objects should truncate 123456 usec to just 123 in the DB cast back to 123000. ",
      # ActiveRecord::StatementInvalid: NoMethodError: undefined method `[]' for nil:NilClass: SELECT * FROM [sql_server_chronics] WHERE ([sql_server_chronics].[id] = 10) 
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:862:in `raw_select'
      "test: For chronic data types with a usec saving new datetime objects should truncate 3001 usec to just 003 in the DB cast back to 3000. "

    disable AssociationsExtensionsTest, 
      # ArgumentError: undefined class/module DeveloperProjectsAssociationExtensioo
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:718:in `ReadClassOrModule'
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:768:in `ReadExtended'
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:908:in `ReadAnObject'
      :test_marshalling_extensions,
      # ArgumentError: undefined class/module DeveloperProjectsAssociationExtensioo
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:718:in `ReadClassOrModule'
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:768:in `ReadExtended'
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:908:in `ReadAnObject'
      :test_marshalling_named_extensions

    disable AssociationsTest, 
      # NoMethodError: undefined method `flock' for #<File:C:/Users/sborde/AppData/Local/Temp/ar-pstore-association-test>
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/redist-libs/ruby/1.8/pstore.rb:296:in `transaction'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/associations_test.rb:78:in `test_storing_in_pstore'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/testsuite.rb:37:in `run'
      :test_storing_in_pstore

    disable BasicsTest, 
      # <false> is not true.
      :test_array_to_xml_including_belongs_to_association,
      # <false> is not true.
      :test_array_to_xml_including_has_one_association,
      # <"2010-02-08 00:06:37.000"> expected but was
      # <Mon, 08 Feb 2010 00:06:37 +0000>.
      :test_coerced_test_read_attributes_before_type_cast_on_datetime,
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
      # <"2004-04-15"> expected but was
      # <"2004-04-15 00:00:00">.
      :test_to_xml

    disable BinaryTest, 
      # Reloaded data differs from original.
      # <"\377\330\377\340\000\020JFIF\000\001\001\001\000H\000H\000\000\377\333\000C\000\r\t\t\n\n\n\016\v\v\016\024\r\v\r\024\027\021\016\016\021\027\e\025\025\025\025\025\e\e\025\027\027\027\027\025\e\032\036 ! \036\032''**''555556666666666\377\333\000C\001\016\r\r\021\021\021\027\021\021\027\027\023\024\023\027\035\031\032\032\031\035&\035\035\036\035\035&,$    $,(+
      :test_load_save

    disable CalculationsTest, 
      # NoMethodError: undefined method `to_d' for #<BigDecimal:0x005ee84,'0.0',9(9)>
      # calculations.rb:296:in `type_cast_calculated_value'
      # calculations.rb:236:in `execute_simple_calculation'
      # calculations.rb:130:in `calculate'
      :test_should_return_nil_as_average

    disable ColumnTestSqlserver, 
      # <"GIF89a\001\000\001\000\200\000\000\377\377\377\000\000\000!\371\004\000\000\000\000\000,\000\000\000\000\001\000\001\000\000\002\002D\001\000;"> expected but was
      # <"qspVW\227\020\020\022\200\002U%RU\000\0032I@\000\000D\000\000\020\020\002&\201\005\220">.
      "test: For binary columns should read and write binary data equally. ",
      # ActiveRecord::StatementInvalid: NoMethodError: undefined method `[]' for nil:NilClass: SELECT * FROM [sql_server_chronics] WHERE ([sql_server_chronics].[id] = 13) 
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:862:in `raw_select'
      "test: For datetime columns which have coerced types should have an inheritable attribute . ",
      # ActiveRecord::StatementInvalid: NoMethodError: undefined method `[]' for nil:NilClass: SELECT * FROM [sql_server_chronics] WHERE ([sql_server_chronics].[id] = 14) 
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:862:in `raw_select'
      "test: For datetime columns which have coerced types should have column and objects cast to date. ",
      # ActiveRecord::StatementInvalid: NoMethodError: undefined method `[]' for nil:NilClass: SELECT * FROM [sql_server_chronics] WHERE ([sql_server_chronics].[id] = 15) 
      # abstract_adapter.rb:201:in `log'
      # sqlserver_adapter.rb:839:in `raw_execute'
      # sqlserver_adapter.rb:862:in `raw_select'
      "test: For datetime columns which have coerced types should have column objects cast to time. "

    disable ConnectionTestSqlserver, 
      # RuntimeError: each_object only supported for objects of type Class or Module
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:117:in `assert_all_statements_used_are_closed'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:76
      "test: ConnectionSqlserver should active closes statement. ",
      # RuntimeError: each_object only supported for objects of type Class or Module
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:117:in `assert_all_statements_used_are_closed'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:56
      "test: ConnectionSqlserver should execute with block closes statement. ",
      # RuntimeError: each_object only supported for objects of type Class or Module
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:117:in `assert_all_statements_used_are_closed'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:50
      "test: ConnectionSqlserver should execute without block closes statement. ",
      # RuntimeError: each_object only supported for objects of type Class or Module
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:117:in `assert_all_statements_used_are_closed'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:22
      "test: ConnectionSqlserver should finish DBI statment handle from #execute with block. ",
      # RuntimeError: each_object only supported for objects of type Class or Module
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:117:in `assert_all_statements_used_are_closed'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:34
      "test: ConnectionSqlserver should finish connection from #raw_select. ",
      # RuntimeError: each_object only supported for objects of type Class or Module
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:117:in `assert_all_statements_used_are_closed'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.3/test/cases/connection_test_sqlserver.rb:64
      "test: ConnectionSqlserver should insert with identity closes statement. ",
      # RuntimeError: each_object only supported for objects of type Class or Module
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\ObjectSpace.cs:37:in `each_object'
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
      # <Sun Feb 07 00:00:00 -0800 2010> expected but was
      # <Sun, 07 Feb 2010 00:00:00 +0000>.
      # 
      # diff:
      # - Sun Feb 07 00:00:00 -0800 2010
      # ?          ^          ^^^  -- -
      # + Sun, 07 Feb 2010 00:00:00 +0000
      # ?    ++++     + ^^          ^
      :test_partial_update

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
      # <Wed, 16 Jul 2003 07:28:11 +0000> expected to be kind_of?
      # <Time> but was
      # <DateTime>.
      :test_named_bind_variables

    disable FixturesTest, 
      # <"\377\330\377\340\000\020JFIF\000\001\001\001\000H\000H\000\000\377\333\000C\000\r\t\t\n\n\n\016\v\v\016\024\r\v\r\024\027\021\016\016\021\027\e\025\025\025\025\025\e\e\025\027\027\027\027\025\e\032\036 ! \036\032''**''555556666666666\377\333\000C\001\016\r\r\021\021\021\027\021\021\027\027\023\024\023\027\035\031\032\032\031\035&\035\035\036\035\035&,$    $,(+&&&+(//,,//666666666666666\377\300\00
      :test_binary_in_fixtures,
      # Exception raised:
      # Class: <Fixture::FormatError>
      # Message: <"Bad data for Category fixture named IronRuby.StandardLibrary.Yaml.PrivateType">
      # ---Backtrace---
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/fixtures_test.rb:165:in `test_omap_fixtures'
      # fixtures.rb:707:in `read_yaml_fixture_files'
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\
      :test_omap_fixtures

    disable FoxyFixturesTest, 
      # <207281424> expected but was
      # <1014543642>.
      :test_identifies_consistently

    disable HasOneAssociationsTest, 
      # Exception raised:
      # Class: <TypeError>
      # Message: <"can't dump hash with default proc">
      # ---Backtrace---
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:246:in `WriteHash'
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marshal.cs:430:in `WriteAnObject'
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Builtins\Marsha
      :test_can_marshal_has_one_association_with_nil_target

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

    disable MigrationTest, 
      # System::OverflowException: Conversion overflows.
      # System.Data:0:in `get_Decimal'
      # System.Data:0:in `get_Value'
      # System.Data:0:in `GetValueInternal'
      :test_native_decimal_insert_manual_vs_automatic,
      # System::OverflowException: Conversion overflows.
      # System.Data:0:in `get_Decimal'
      # System.Data:0:in `get_Value'
      # System.Data:0:in `GetValueInternal'
      :test_native_types

    disable TestAutosaveAssociationOnAHasManyAssociation, 
      # <RuntimeError> exception expected but was
      # Class: <NoMethodError>
      # Message: <"undefined method `save' for #<Bird id: 19, name: \"Grace OMalley\", pirate_id: 521472015>">
      # ---Backtrace---
      # attribute_methods.rb:232:in `method_missing'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test/cases/autosave_association_test.rb:970:in `save'
      # (eval):1
      # autosav
      :test_should_rollback_any_changes_if_an_exception_occurred_while_saving

    disable TestDestroyAsPartOfAutosaveAssociation, 
      # <RuntimeError> exception expected but was
      # Class: <NoMethodError>
      # Message: <"undefined method `save' for #<Parrot id: 439063485, name: \"parrots_0\", parrot_sti_class: nil, killer_id: nil, created_at: \"2010-02-08 00:12:07\", created_on: \"2010-02-08 00:12:07\", updated_at: \"2010-02-08 00:12:07\", updated_on: \"2010-02-08 00:12:07\">">
      # ---Backtrace---
      # attribute_methods.rb:232:in `method_missing'
      # d:
      :test_should_rollback_destructions_if_an_exception_occurred_while_saving_parrots

    disable TestNestedAttributesOnAHasAndBelongsToManyAssociation, 
      # <"Grace OMalley"> expected but was
      # <"Privateers Greed">.
      :test_should_automatically_build_new_associated_models_for_each_entry_in_a_hash_where_the_id_is_missing

    disable TestNestedAttributesOnAHasManyAssociation, 
      # <"Grace OMalley"> expected but was
      # <"Privateers Greed">.
      :test_should_automatically_build_new_associated_models_for_each_entry_in_a_hash_where_the_id_is_missing

    disable UnicodeTestSqlserver, 
      # IronRuby::Builtins::EncodingCompatibilityError: incompatible character encodings: ASCII-8BIT and utf-8
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Extensions\IListOps.cs:1343:in `Join'
      # D:\vs_langs01_s\Merlin\Main\Languages\Ruby\Libraries.LCA_RESTRICTED\Extensions\IListOps.cs:1365:in `join'
      # d:/vs_langs01_s/Merlin/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/test-unit-2.0.5/lib/test/unit/diff.rb:720:in `diff'
      "test: Testing unicode data should insert into nvarchar field. "

    disable ValidationsTest, 
      # <false> is not true.
      :test_optionally_validates_length_of_using_within_on_create_utf8,
      # <false> is not true.
      :test_optionally_validates_length_of_using_within_on_update_utf8,
      # #<ActiveRecord::Errors:0x0122f6a @base=#<Topic id: nil, title: "一二三四五", author_name: nil, author_email_address: "test@test.com", written_on: nil, bonus_time: nil, last_read: nil, content: nil, approved: true, replies_count: 0, parent_id: nil, parent_title: nil, type: nil>, @errors=#<OrderedHash {"title"=>[#<ActiveRecord::Error:0x0122fa4 @base=#<Topic id: nil, title: "一二三四五", aut
      :test_optionally_validates_length_of_using_within_utf8,
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

  end

end
