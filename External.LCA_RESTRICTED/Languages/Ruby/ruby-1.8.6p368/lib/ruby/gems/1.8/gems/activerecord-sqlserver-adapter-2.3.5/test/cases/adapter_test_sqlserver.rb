require 'cases/sqlserver_helper'
require 'models/task'
require 'models/reply'
require 'models/joke'
require 'models/subscriber'

class AdapterTestSqlserver < ActiveRecord::TestCase
  
  fixtures :tasks
    
  def setup
    @connection = ActiveRecord::Base.connection
    @basic_insert_sql = "INSERT INTO [funny_jokes] ([name]) VALUES('Knock knock')"
    @basic_update_sql = "UPDATE [customers] SET [address_street] = NULL WHERE [id] = 2"
    @basic_select_sql = "SELECT * FROM [customers] WHERE ([customers].[id] = 1)"
  end
  
  context 'For abstract behavior' do
    
    should 'have a 128 max #table_alias_length' do
      assert @connection.table_alias_length <= 128
    end
    
    should 'raise invalid statement error' do
      assert_raise(ActiveRecord::StatementInvalid) { Topic.connection.update("UPDATE XXX") }
    end
    
    should 'be our adapter_name' do
      assert_equal 'SQLServer', @connection.adapter_name
    end
    
    should 'include version in inspect' do
      assert_match(/version\: \d.\d/,@connection.inspect)
    end
    
    should 'support migrations' do
      assert @connection.supports_migrations?
    end
    
    should 'support DDL in transactions' do
      assert @connection.supports_ddl_transactions?
    end
    
    should 'allow owner table name prefixs like dbo. to still allow table_exists? to return true' do
      begin
        assert_equal 'tasks', Task.table_name
        assert Task.table_exists?
        Task.table_name = 'dbo.tasks'
        assert Task.table_exists?, 'Tasks table name of dbo.tasks should return true for exists.'
      ensure
        Task.table_name = 'tasks'
      end
    end
    
    context 'for database version' do
      
      setup do
        @version_regexp = ActiveRecord::ConnectionAdapters::SQLServerAdapter::DATABASE_VERSION_REGEXP
        @supported_version = ActiveRecord::ConnectionAdapters::SQLServerAdapter::SUPPORTED_VERSIONS
        @sqlserver_2000_string = "Microsoft SQL Server  2000 - 8.00.2039 (Intel X86)"
        @sqlserver_2005_string = "Microsoft SQL Server 2005 - 9.00.3215.00 (Intel X86)"
        @sqlserver_2008_string = "Microsoft SQL Server 2008 (RTM) - 10.0.1600.22 (Intel X86)"
      end
      
      should 'return a string from #database_version that matches class regexp' do
        assert_match @version_regexp, @connection.database_version
      end
      
      should 'return a 4 digit year fixnum for #database_year' do
        assert_instance_of Fixnum, @connection.database_year
        assert_contains @supported_version, @connection.database_year
      end
      
      should 'return true to #sqlserver_2000?' do
        @connection.stubs(:database_version).returns(@sqlserver_2000_string)
        assert @connection.sqlserver_2000?
      end
      
      should 'return true to #sqlserver_2005?' do
        @connection.stubs(:database_version).returns(@sqlserver_2005_string)
        assert @connection.sqlserver_2005?
      end
      
      should 'return true to #sqlserver_2008?' do
        @connection.stubs(:database_version).returns(@sqlserver_2008_string)
        assert @connection.sqlserver_2008?
      end
      
    end
    
    context 'for #unqualify_table_name and #unqualify_db_name' do

      setup do
        @expected_table_name = 'baz'
        @expected_db_name = 'foo'
        @first_second_table_names = ['[baz]','baz','[bar].[baz]','bar.baz']
        @third_table_names = ['[foo].[bar].[baz]','foo.bar.baz']
        @qualifed_table_names = @first_second_table_names + @third_table_names
      end
      
      should 'return clean table_name from #unqualify_table_name' do
        @qualifed_table_names.each do |qtn|
          assert_equal @expected_table_name, 
            @connection.send(:unqualify_table_name,qtn),
            "This qualifed_table_name #{qtn} did not unqualify correctly."
        end
      end
      
      should 'return nil from #unqualify_db_name when table_name is less than 2 qualified' do
        @first_second_table_names.each do |qtn|
          assert_equal nil, @connection.send(:unqualify_db_name,qtn),
            "This qualifed_table_name #{qtn} did not return nil."
        end
      end
      
      should 'return clean db_name from #unqualify_db_name when table is thrid level qualified' do
        @third_table_names.each do |qtn|
          assert_equal @expected_db_name, 
            @connection.send(:unqualify_db_name,qtn),
            "This qualifed_table_name #{qtn} did not unqualify the db_name correctly."
        end
      end

    end
    
    should 'return true to #insert_sql? for inserts only' do
      assert @connection.send(:insert_sql?,'INSERT...')
      assert !@connection.send(:insert_sql?,'UPDATE...')
      assert !@connection.send(:insert_sql?,'SELECT...')
    end
    
    context 'for #limited_update_conditions' do
    
      should 'only match up to the first WHERE' do
        where_sql = "TOP 1 WHERE ([posts].author_id = 1 and [posts].columnWHEREname = 2)  ORDER BY posts.id"
        assert_equal "WHERE bar IN (SELECT TOP 1  bar FROM foo WHERE ([posts].author_id = 1 and [posts].columnWHEREname = 2)  ORDER BY posts.id)", @connection.limited_update_conditions(where_sql, 'foo', 'bar')
      end
    
    end
        
    context 'for #sql_for_association_limiting?' do
      
      should 'return false for simple selects with no GROUP BY and ORDER BY' do
        assert !sql_for_association_limiting?("SELECT * FROM [posts]")
      end
      
      should 'return true to single SELECT, ideally a table/primarykey, that also has a GROUP BY and ORDER BY' do
        assert sql_for_association_limiting?("SELECT [posts].id FROM...GROUP BY [posts].id ORDER BY MIN(posts.id)")
      end
      
      should 'return false to single * wildcard SELECT that also has a GROUP BY and ORDER BY' do
        assert !sql_for_association_limiting?("SELECT * FROM...GROUP BY [posts].id ORDER BY MIN(posts.id)")
      end
      
      should 'return false to multiple columns in the select even when GROUP BY and ORDER BY are present' do
        sql = "SELECT [accounts].credit_limit, firm_id FROM...GROUP BY firm_id ORDER BY firm_id"
        assert !sql_for_association_limiting?(sql)
      end
      
    end
    
    context 'for #get_table_name' do

      should 'return quoted table name from basic INSERT, UPDATE and SELECT statements' do
        assert_equal '[funny_jokes]', @connection.send(:get_table_name,@basic_insert_sql)
        assert_equal '[customers]', @connection.send(:get_table_name,@basic_update_sql)
        assert_equal '[customers]', @connection.send(:get_table_name,@basic_select_sql)
      end

    end

    context "for add_limit! within a scoped method call" do
      setup do
        @connection.stubs(:select_value).with(regexp_matches(/TotalRows/)).returns '100000000'
      end

      should 'not add any ordering if the scope doesn\'t have an order' do
        assert_equal 'SELECT * FROM (SELECT TOP 10 * FROM (SELECT TOP 40 * FROM [developers]) AS tmp1) AS tmp2', add_limit!('SELECT * FROM [developers]', {:offset => 30, :limit => 10}, {})
      end

      should 'still add the default ordering if the scope doesn\'t have an order but the raw order option is there' do
        assert_equal 'SELECT * FROM (SELECT TOP 10 * FROM (SELECT TOP 40 * FROM [developers]) AS tmp1 ORDER BY [name] DESC) AS tmp2 ORDER BY [name]', add_limit!('SELECT * FROM [developers]', {:offset => 30, :limit => 10, :order => 'name'}, {})
      end

      should 'add scoped order options to the offset and limit sql' do
        assert_equal 'SELECT * FROM (SELECT TOP 10 * FROM (SELECT TOP 40 * FROM [developers]) AS tmp1 ORDER BY [id] DESC) AS tmp2 ORDER BY [id]', add_limit!('SELECT * FROM [developers]', {:offset => 30, :limit => 10}, {:order => 'id'})
      end

      should 'combine scoped order with raw order options in the offset and limit sql' do
        assert_equal 'SELECT * FROM (SELECT TOP 10 * FROM (SELECT TOP 40 * FROM [developers]) AS tmp1 ORDER BY [name] DESC, [id] DESC) AS tmp2 ORDER BY [name], [id]', add_limit!('SELECT * FROM [developers]', {:offset => 30, :limit => 10, :order => 'name'}, {:order => 'id'})
      end
    end
    
    context 'dealing with various orders SQL snippets' do
      
      setup do
        @single_order = 'comments.id'
        @single_order_with_desc = 'comments.id DESC'
        @two_orders = 'comments.id, comments.post_id'
        @two_orders_with_asc = 'comments.id, comments.post_id ASC'
        @two_orders_with_desc_and_asc = 'comments.id DESC, comments.post_id ASC'
        @two_duplicate_order_with_dif_dir = "id, id DESC"
      end
      
      should 'convert to an 2D array of column/direction arrays using #orders_and_dirs_set' do
        assert_equal [['comments.id',nil]], orders_and_dirs_set('ORDER BY comments.id'), 'Needs to remove ORDER BY'
        assert_equal [['comments.id',nil]], orders_and_dirs_set(@single_order)
        assert_equal [['comments.id',nil],['comments.post_id',nil]], orders_and_dirs_set(@two_orders)
        assert_equal [['comments.id',nil],['comments.post_id','ASC']], orders_and_dirs_set(@two_orders_with_asc)
        assert_equal [['id',nil],['id','DESC']], orders_and_dirs_set(@two_duplicate_order_with_dif_dir)
      end
      
      should 'remove duplicate or maintain the same order by statements giving precedence to first using #add_order! method chain extension' do
        assert_equal ' ORDER BY comments.id', add_order!(@single_order)
        assert_equal ' ORDER BY comments.id DESC', add_order!(@single_order_with_desc)
        assert_equal ' ORDER BY comments.id, comments.post_id', add_order!(@two_orders)
        assert_equal ' ORDER BY comments.id DESC, comments.post_id ASC', add_order!(@two_orders_with_desc_and_asc)
        assert_equal 'SELECT * FROM [developers] ORDER BY id', add_order!('id, developers.id DESC','SELECT * FROM [developers]')
        assert_equal 'SELECT * FROM [developers] ORDER BY [developers].[id] DESC', add_order!('[developers].[id] DESC, id','SELECT * FROM [developers]')
      end
      
      should 'take all types of order options and convert them to MIN functions using #order_to_min_set' do
        assert_equal 'MIN(comments.id)', order_to_min_set(@single_order)
        assert_equal 'MIN(comments.id), MIN(comments.post_id)', order_to_min_set(@two_orders)
        assert_equal 'MIN(comments.id) DESC', order_to_min_set(@single_order_with_desc)
        assert_equal 'MIN(comments.id), MIN(comments.post_id) ASC', order_to_min_set(@two_orders_with_asc)
        assert_equal 'MIN(comments.id) DESC, MIN(comments.post_id) ASC', order_to_min_set(@two_orders_with_desc_and_asc)
      end
      
      should 'leave order by alone when same column crosses two tables' do
        assert_equal ' ORDER BY developers.name, projects.name', add_order!('developers.name, projects.name')
      end
      
    end
    
    context 'with different language' do

      teardown do
        @connection.execute("SET LANGUAGE us_english") rescue nil
      end

      should_eventually 'do a date insertion when language is german' do
        @connection.execute("SET LANGUAGE deutsch")
        assert_nothing_raised do
          Task.create(:starting => Time.utc(2000, 1, 31, 5, 42, 0), :ending => Date.new(2006, 12, 31))
        end
      end

    end
    
    context 'testing #enable_default_unicode_types configuration' do

      should 'use non-unicode types when set to false' do
        with_enable_default_unicode_types(false) do
          if sqlserver_2000?
            assert_equal 'varchar', @connection.native_string_database_type
            assert_equal 'text', @connection.native_text_database_type
          elsif sqlserver_2005?
            assert_equal 'varchar', @connection.native_string_database_type
            assert_equal 'varchar(max)', @connection.native_text_database_type
          end
        end
      end
      
      should 'use unicode types when set to true' do
        with_enable_default_unicode_types(true) do
          if sqlserver_2000?
            assert_equal 'nvarchar', @connection.native_string_database_type
            assert_equal 'ntext', @connection.native_text_database_type
          elsif sqlserver_2005?
            assert_equal 'nvarchar', @connection.native_string_database_type
            assert_equal 'nvarchar(max)', @connection.native_text_database_type
          end
        end
      end

    end
    
    
  end
  
  context 'For chronic data types' do
    
    context 'with a usec' do
      
      setup do
        @time = Time.now
        @db_datetime_003 = '2012-11-08 10:24:36.003'
        @db_datetime_123 = '2012-11-08 10:24:36.123'
        @all_datetimes = [@db_datetime_003, @db_datetime_123]
        @all_datetimes.each do |datetime|
          @connection.execute("INSERT INTO [sql_server_chronics] ([datetime]) VALUES('#{datetime}')")
        end
      end
      
      teardown do
        @all_datetimes.each do |datetime|
          @connection.execute("DELETE FROM [sql_server_chronics] WHERE [datetime] = '#{datetime}'")
        end
      end
      
      context 'finding existing DB objects' do

        should 'find 003 millisecond in the DB with before and after casting' do
          existing_003 = SqlServerChronic.find_by_datetime!(@db_datetime_003)
          assert_equal @db_datetime_003, existing_003.datetime_before_type_cast
          assert_equal 3000, existing_003.datetime.usec, 'A 003 millisecond in SQL Server is 3000 microseconds'
        end

        should 'find 123 millisecond in the DB with before and after casting' do
          existing_123 = SqlServerChronic.find_by_datetime!(@db_datetime_123)
          assert_equal @db_datetime_123, existing_123.datetime_before_type_cast
          assert_equal 123000, existing_123.datetime.usec, 'A 123 millisecond in SQL Server is 123000 microseconds'
        end

      end
      
      context 'saving new datetime objects' do

        should 'truncate 123456 usec to just 123 in the DB cast back to 123000' do
          @time.stubs(:usec).returns(123456)
          saved = SqlServerChronic.create!(:datetime => @time).reload
          assert_equal '123', saved.datetime_before_type_cast.split('.')[1]
          assert_equal 123000, saved.datetime.usec
        end
        
        should 'truncate 3001 usec to just 003 in the DB cast back to 3000' do
          @time.stubs(:usec).returns(3001)
          saved = SqlServerChronic.create!(:datetime => @time).reload
          assert_equal '003', saved.datetime_before_type_cast.split('.')[1]
          assert_equal 3000, saved.datetime.usec
        end
        
      end
      
    end
    
  end
  
  context 'For identity inserts' do
    
    setup do
      @identity_insert_sql = "INSERT INTO [funny_jokes] ([id],[name]) VALUES(420,'Knock knock')"
      @identity_insert_sql_unquoted = "INSERT INTO funny_jokes (id, name) VALUES(420, 'Knock knock')"
      @identity_insert_sql_unordered = "INSERT INTO [funny_jokes] ([name],[id]) VALUES('Knock knock',420)"
    end
    
    should 'return quoted table_name to #query_requires_identity_insert? when INSERT sql contains id column' do
      assert_equal '[funny_jokes]', @connection.send(:query_requires_identity_insert?,@identity_insert_sql)
      assert_equal '[funny_jokes]', @connection.send(:query_requires_identity_insert?,@identity_insert_sql_unquoted)
      assert_equal '[funny_jokes]', @connection.send(:query_requires_identity_insert?,@identity_insert_sql_unordered)
    end
    
    should 'return false to #query_requires_identity_insert? for normal SQL' do
      [@basic_insert_sql, @basic_update_sql, @basic_select_sql].each do |sql|
        assert !@connection.send(:query_requires_identity_insert?,sql), "SQL was #{sql}"
      end
    end
    
    should 'find identity column using #identity_column' do
      joke_id_column = Joke.columns.detect { |c| c.name == 'id' }
      assert_equal joke_id_column, @connection.send(:identity_column,Joke.table_name)
    end
    
    should 'return nil when calling #identity_column for a table_name with no identity' do
      assert_nil @connection.send(:identity_column,Subscriber.table_name)
    end
    
  end
  
  context 'For Quoting' do
    
    should 'return 1 for #quoted_true' do
      assert_equal '1', @connection.quoted_true
    end
    
    should 'return 0 for #quoted_false' do
      assert_equal '0', @connection.quoted_false
    end
    
    should 'not escape backslash characters like abstract adapter' do
      string_with_backslashs = "\\n"
      assert_equal string_with_backslashs, @connection.quote_string(string_with_backslashs)
    end
    
    should 'quote column names with brackets' do
      assert_equal '[foo]', @connection.quote_column_name(:foo)
      assert_equal '[foo]', @connection.quote_column_name('foo')
      assert_equal '[foo].[bar]', @connection.quote_column_name('foo.bar')
    end
    
    should 'not quote already quoted column names with brackets' do
      assert_equal '[foo]', @connection.quote_column_name('[foo]')
      assert_equal '[foo].[bar]', @connection.quote_column_name('[foo].[bar]')
    end
    
    should 'quote table names like columns' do
      assert_equal '[foo].[bar]', @connection.quote_column_name('foo.bar')
      assert_equal '[foo].[bar].[baz]', @connection.quote_column_name('foo.bar.baz')
    end
    
  end
  
  context 'When disableing referential integrity' do
    
    setup do
      @parent = FkTestHasPk.create!
      @member = FkTestHasFk.create!(:fk_id => @parent.id)
    end
    
    should 'NOT ALLOW by default the deletion of a referenced parent' do
      FkTestHasPk.connection.disable_referential_integrity { }
      assert_raise(ActiveRecord::StatementInvalid) { @parent.destroy }
    end
    
    should 'ALLOW deletion of referenced parent using #disable_referential_integrity block' do
      FkTestHasPk.connection.disable_referential_integrity { @parent.destroy }
    end
    
    should 'again NOT ALLOW deletion of referenced parent after #disable_referential_integrity block' do
      assert_raise(ActiveRecord::StatementInvalid) do
        FkTestHasPk.connection.disable_referential_integrity { }
        @parent.destroy
      end
    end
    
  end
  
  context 'For DatabaseStatements' do
    
    context "finding out what user_options are available" do
      
      should "run the database consistency checker useroptions command" do
        @connection.expects(:select_rows).with(regexp_matches(/^dbcc\s+useroptions$/i)).returns []
        @connection.user_options
      end
      
      should "return a underscored key hash with indifferent access of the results" do
        @connection.expects(:select_rows).with(regexp_matches(/^dbcc\s+useroptions$/i)).returns [['some', 'thing'], ['isolation level', 'read uncommitted']]
        uo = @connection.user_options
        assert_equal 2, uo.keys.size
        assert_equal 'thing', uo['some']
        assert_equal 'thing', uo[:some]
        assert_equal 'read uncommitted', uo['isolation_level']
        assert_equal 'read uncommitted', uo[:isolation_level]
      end
      
    end

    context "altering isolation levels" do
      
      should "barf if the requested isolation level is not valid" do
        assert_raise(ArgumentError) do
          @connection.run_with_isolation_level 'INVALID ISOLATION LEVEL' do; end
        end
      end
      
      context "with a valid isolation level" do
        
        setup do
          @t1 = tasks(:first_task)
          @t2 = tasks(:another_task)
          assert @t1, 'Tasks :first_task should be in AR fixtures'
          assert @t2, 'Tasks :another_task should be in AR fixtures'
          good_isolation_level = @connection.user_options[:isolation_level].blank? || @connection.user_options[:isolation_level] =~ /read committed/i
          assert good_isolation_level, "User isolation level is not at a happy starting place: #{@connection.user_options[:isolation_level].inspect}"
        end
        
        should 'allow #run_with_isolation_level to not take a block to set it' do
          begin
            @connection.run_with_isolation_level 'READ UNCOMMITTED'
            assert_match %r|read uncommitted|i, @connection.user_options[:isolation_level]
          ensure
            @connection.run_with_isolation_level 'READ COMMITTED'
          end
        end
        
        should 'return block value using #run_with_isolation_level' do
          assert_same_elements Task.find(:all), @connection.run_with_isolation_level('READ UNCOMMITTED') { Task.find(:all) }
        end
        
        should 'pass a read uncommitted isolation level test' do
          assert_nil @t2.starting, 'Fixture should have this empty.'
          begin
            Task.transaction do
              @t2.starting = Time.now
              @t2.save
              @dirty_t2 = @connection.run_with_isolation_level('READ UNCOMMITTED') { Task.find(@t2.id) }
              raise ActiveRecord::ActiveRecordError
            end
          rescue
            'Do Nothing'
          end
          assert @dirty_t2, 'Should have a Task record from within block above.'
          assert @dirty_t2.starting, 'Should have a dirty date.'
          assert_nil Task.find(@t2.id).starting, 'Should be nil again from botched transaction above.'
        end unless active_record_2_point_2? # Transactions in tests are a bit screwy in 2.2.
        
      end
      
    end
    
  end
  
  context 'For SchemaStatements' do
    
    context 'returning from #type_to_sql' do
      
      should 'create integers when no limit supplied' do
        assert_equal 'integer', @connection.type_to_sql(:integer)
      end
      
      should 'create integers when limit is 4' do
        assert_equal 'integer', @connection.type_to_sql(:integer, 4)
      end
      
      should 'create integers when limit is 3' do
        assert_equal 'integer', @connection.type_to_sql(:integer, 3)
      end
      
      should 'create smallints when limit is less than 3' do
        assert_equal 'smallint', @connection.type_to_sql(:integer, 2)
        assert_equal 'smallint', @connection.type_to_sql(:integer, 1)
      end
      
      should 'create bigints when limit is greateer than 4' do
        assert_equal 'bigint', @connection.type_to_sql(:integer, 5)
        assert_equal 'bigint', @connection.type_to_sql(:integer, 6)
        assert_equal 'bigint', @connection.type_to_sql(:integer, 7)
        assert_equal 'bigint', @connection.type_to_sql(:integer, 8)
      end
      
    end
    
  end
  
  context 'For indexes' do
    
    setup do
      @desc_index_name = 'idx_credit_limit_test_desc'
      @connection.execute "CREATE INDEX #{@desc_index_name} ON accounts (credit_limit DESC)"
    end
    
    teardown do
      @connection.execute "DROP INDEX accounts.#{@desc_index_name}"
    end
    
    should 'have indexes with descending order' do
      assert @connection.indexes('accounts').detect { |i| i.name == @desc_index_name }
    end
    
  end
  
  context 'For views' do
    
    context 'using @connection.views' do

      should 'return an array' do
        assert_instance_of Array, @connection.views
      end
      
      should 'find CustomersView table name' do
        assert_contains @connection.views, 'customers_view'
      end
      
      should 'not contain system views' do
        systables = ['sysconstraints','syssegments']
        systables.each do |systable|
          assert !@connection.views.include?(systable), "This systable #{systable} should not be in the views array."
        end
      end
      
      should 'allow the connection.view_information method to return meta data on the view' do
        view_info = @connection.view_information('customers_view')
        assert_equal('customers_view', view_info['TABLE_NAME'])
        assert_match(/CREATE VIEW customers_view/, view_info['VIEW_DEFINITION'])
      end
      
      should 'allow the connection.view_table_name method to return true table_name for the view' do
        assert_equal 'customers', @connection.view_table_name('customers_view')
        assert_equal 'topics', @connection.view_table_name('topics'), 'No view here, the same table name should come back.'
      end
      
    end
    
    context 'used by a class for table_name' do
      
      context 'with same column names' do
        
        should 'have matching column objects' do
          columns = ['id','name','balance']
          assert !CustomersView.columns.blank?
          assert_equal columns.size, CustomersView.columns.size
          columns.each do |colname|
            assert_instance_of ActiveRecord::ConnectionAdapters::SQLServerColumn, 
              CustomersView.columns_hash[colname], 
              "Column name #{colname.inspect} was not found in these columns #{CustomersView.columns.map(&:name).inspect}"
          end
        end
        
        should 'find identity column' do
          assert CustomersView.columns_hash['id'].primary
          assert CustomersView.columns_hash['id'].is_identity?
        end
        
        should 'find default values' do
          assert_equal 0, CustomersView.new.balance
        end
        
        should 'respond true to table_exists?' do
          assert CustomersView.table_exists?
        end
        
        should 'have correct table name for all column objects' do
          assert CustomersView.columns.all?{ |c| c.table_name == 'customers_view' }, 
            CustomersView.columns.map(&:table_name).inspect
        end
        
      end
      
      context 'with aliased column names' do
        
        should 'have matching column objects' do
          columns = ['id','pretend_null']
          assert !StringDefaultsView.columns.blank?
          assert_equal columns.size, StringDefaultsView.columns.size
          columns.each do |colname|
            assert_instance_of ActiveRecord::ConnectionAdapters::SQLServerColumn, 
              StringDefaultsView.columns_hash[colname], 
              "Column name #{colname.inspect} was not found in these columns #{StringDefaultsView.columns.map(&:name).inspect}"
          end
        end
        
        should 'find identity column' do
          assert StringDefaultsView.columns_hash['id'].primary
          assert StringDefaultsView.columns_hash['id'].is_identity?
        end
        
        should 'find default values' do
          assert_equal 'null', StringDefaultsView.new.pretend_null, 
            StringDefaultsView.columns_hash['pretend_null'].inspect
        end
        
        should 'respond true to table_exists?' do
          assert StringDefaultsView.table_exists?
        end
        
        should 'have correct table name for all column objects' do
          assert StringDefaultsView.columns.all?{ |c| c.table_name == 'string_defaults_view' }, 
            StringDefaultsView.columns.map(&:table_name).inspect
        end
        
      end
      
    end
    
    context 'doing identity inserts' do

      setup do
        @view_insert_sql = "INSERT INTO [customers_view] ([id],[name],[balance]) VALUES (420,'Microsoft',0)"
      end
      
      should 'respond true/tablename to #query_requires_identity_insert?' do
        assert_equal '[customers_view]', @connection.send(:query_requires_identity_insert?,@view_insert_sql)
      end
      
      should 'be able to do an identity insert' do
        assert_nothing_raised { @connection.execute(@view_insert_sql) }
        assert CustomersView.find(420)
      end

    end
    
    context 'that have more than 4000 chars for their defintion' do

      should 'cope with null returned for the defintion' do
        assert_nothing_raised() { StringDefaultsBigView.columns }
      end
      
      should 'using alternate view defintion still be able to find real default' do
        assert_equal 'null', StringDefaultsBigView.new.pretend_null, 
          StringDefaultsBigView.columns_hash['pretend_null'].inspect
      end

    end
    
  end
  
  
  
  private
  
  def sql_for_association_limiting?(sql)
    @connection.send :sql_for_association_limiting?, sql
  end
  
  def orders_and_dirs_set(order)
    @connection.send :orders_and_dirs_set, order
  end
  
  def add_order!(order,sql='')
    ActiveRecord::Base.send :add_order!, sql, order, nil
    sql
  end
  
  def add_limit!(sql, options, scope = :auto)
    ActiveRecord::Base.send :add_limit!, sql, options, scope
    sql
  end

  def order_to_min_set(order)
    @connection.send :order_to_min_set, order
  end
  
  def with_enable_default_unicode_types(setting)
    old_setting = ActiveRecord::ConnectionAdapters::SQLServerAdapter.enable_default_unicode_types
    old_text = ActiveRecord::ConnectionAdapters::SQLServerAdapter.native_text_database_type
    old_string = ActiveRecord::ConnectionAdapters::SQLServerAdapter.native_string_database_type
    ActiveRecord::ConnectionAdapters::SQLServerAdapter.enable_default_unicode_types = setting
    ActiveRecord::ConnectionAdapters::SQLServerAdapter.native_text_database_type = nil
    ActiveRecord::ConnectionAdapters::SQLServerAdapter.native_string_database_type = nil
    yield
  ensure
    ActiveRecord::ConnectionAdapters::SQLServerAdapter.enable_default_unicode_types = old_setting
    ActiveRecord::ConnectionAdapters::SQLServerAdapter.native_text_database_type = old_text
    ActiveRecord::ConnectionAdapters::SQLServerAdapter.native_string_database_type = old_string
  end
  
end


class AdapterTest < ActiveRecord::TestCase
  
  COERCED_TESTS = [
    :test_add_limit_offset_should_sanitize_sql_injection_for_limit_without_comas,
    :test_add_limit_offset_should_sanitize_sql_injection_for_limit_with_comas
  ]
  
  include SqlserverCoercedTest
  
  def test_coerced_test_add_limit_offset_should_sanitize_sql_injection_for_limit_without_comas
    sql_inject = "1 select * from schema"
    connection = ActiveRecord::Base.connection
    assert_raise(ArgumentError) { connection.add_limit_offset!("", :limit=>sql_inject) }
    assert_raise(ArgumentError) { connection.add_limit_offset!("", :limit=>sql_inject, :offset=>7) }
  end

  def test_coerced_test_add_limit_offset_should_sanitize_sql_injection_for_limit_with_comas
    sql_inject = "1, 7 procedure help()"
    connection = ActiveRecord::Base.connection
    assert_raise(ArgumentError) { connection.add_limit_offset!("", :limit=>sql_inject) }
    assert_raise(ArgumentError) { connection.add_limit_offset!("", :limit=> '1 ; DROP TABLE USERS', :offset=>7) }
    assert_raise(ArgumentError) { connection.add_limit_offset!("", :limit=>sql_inject, :offset=>7) }
  end
  
end
