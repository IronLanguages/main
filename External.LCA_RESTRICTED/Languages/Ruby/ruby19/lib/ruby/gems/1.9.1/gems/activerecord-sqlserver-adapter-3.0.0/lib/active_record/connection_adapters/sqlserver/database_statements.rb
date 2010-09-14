module ActiveRecord
  module ConnectionAdapters
    module Sqlserver
      module DatabaseStatements
        
        def select_one(sql, name = nil)
          result = raw_select sql, name, :fetch => :one
          (result && result.first.present?) ? result.first : nil
        end
        
        def select_rows(sql, name = nil)
          raw_select sql, name, :fetch => :rows
        end

        def execute(sql, name = nil, skip_logging = false)
          if table_name = query_requires_identity_insert?(sql)
            with_identity_insert_enabled(table_name) { do_execute(sql,name) }
          else
            do_execute(sql,name)
          end
        end

        def outside_transaction?
          info_schema_query { select_value("SELECT @@TRANCOUNT") == 0 }
        end

        def begin_db_transaction
          do_execute "BEGIN TRANSACTION"
        end

        def commit_db_transaction
          do_execute "COMMIT TRANSACTION"
        end

        def rollback_db_transaction
          do_execute "ROLLBACK TRANSACTION" rescue nil
        end

        def create_savepoint
          do_execute "SAVE TRANSACTION #{current_savepoint_name}"
        end

        def release_savepoint
        end

        def rollback_to_savepoint
          do_execute "ROLLBACK TRANSACTION #{current_savepoint_name}"
        end

        def add_limit_offset!(sql, options)
          raise NotImplementedError, 'This has been moved to the SQLServerCompiler in Arel.'
        end

        def empty_insert_statement_value
          "DEFAULT VALUES"
        end

        def case_sensitive_equality_operator
          cs_equality_operator
        end

        def limited_update_conditions(where_sql, quoted_table_name, quoted_primary_key)
          match_data = where_sql.match(/^(.*?[\]\) ])WHERE[\[\( ]/)
          limit = match_data[1]
          where_sql.sub!(limit,'')
          "WHERE #{quoted_primary_key} IN (SELECT #{limit} #{quoted_primary_key} FROM #{quoted_table_name} #{where_sql})"
        end
        
        # === SQLServer Specific ======================================== #
        
        def execute_procedure(proc_name, *variables)
          vars = variables.map{ |v| quote(v) }.join(', ')
          sql = "EXEC #{proc_name} #{vars}".strip
          results = []
          log(sql,'Execute Procedure') do
            raw_connection_run(sql) do |handle|
              get_rows = lambda {
                rows = handle_to_names_and_values handle, :fetch => :all
                rows.each_with_index { |r,i| rows[i] = r.with_indifferent_access }
                results << rows
              }
              get_rows.call
              while handle_more_results?(handle)
                get_rows.call
              end
            end
          end
          results.many? ? results : results.first
        end
        
        def use_database(database=nil)
          database ||= @connection_options[:database]
          do_execute "USE #{quote_table_name(database)}" unless database.blank?
        end
        
        def user_options
          info_schema_query do
            select_rows("dbcc useroptions").inject(HashWithIndifferentAccess.new) do |values,row| 
              set_option = row[0].gsub(/\s+/,'_')
              user_value = row[1]
              values[set_option] = user_value
              values
            end
          end
        end

        def run_with_isolation_level(isolation_level)
          raise ArgumentError, "Invalid isolation level, #{isolation_level}. Supported levels include #{valid_isolation_levels.to_sentence}." if !valid_isolation_levels.include?(isolation_level.upcase)
          initial_isolation_level = user_options[:isolation_level] || "READ COMMITTED"
          do_execute "SET TRANSACTION ISOLATION LEVEL #{isolation_level}"
          begin
            yield 
          ensure
            do_execute "SET TRANSACTION ISOLATION LEVEL #{initial_isolation_level}"
          end if block_given?
        end
        
        def newid_function
          select_value "SELECT NEWID()"
        end
        
        def newsequentialid_function
          select_value "SELECT NEWSEQUENTIALID()"
        end
        
        # === SQLServer Specific (Rake/Test Helpers) ==================== #
        
        def recreate_database
          remove_database_connections_and_rollback do
            do_execute "EXEC sp_MSforeachtable 'DROP TABLE ?'"
          end
        end

        def recreate_database!(database=nil)
          current_db = current_database
          database ||= current_db
          this_db = database.to_s == current_db
          do_execute 'USE master' if this_db
          drop_database(database)
          create_database(database)
        ensure
          use_database(current_db) if this_db
        end

        def drop_database(database)
          retry_count = 0
          max_retries = 1
          begin
            do_execute "DROP DATABASE #{quote_table_name(database)}"
          rescue ActiveRecord::StatementInvalid => err
            if err.message =~ /because it is currently in use/i
              raise if retry_count >= max_retries
              retry_count += 1
              remove_database_connections_and_rollback(database)
              retry
            else
              raise
            end
          end
        end

        def create_database(database)
          do_execute "CREATE DATABASE #{quote_table_name(database)}"
        end

        def current_database
          select_value 'SELECT DB_NAME()'
        end
        
        def charset
          select_value "SELECT SERVERPROPERTY('SqlCharSetName')"
        end
        
        
        protected
        
        def select(sql, name = nil)
          raw_select sql, name, :fetch => :all
        end
        
        def insert_sql(sql, name = nil, pk = nil, id_value = nil, sequence_name = nil)
          super || select_value("SELECT SCOPE_IDENTITY() AS Ident")
        end
        
        def update_sql(sql, name = nil)
          execute(sql, name)
          select_value('SELECT @@ROWCOUNT AS AffectedRows')
        end
        
        # === SQLServer Specific ======================================== #
        
        def valid_isolation_levels
          ["READ COMMITTED", "READ UNCOMMITTED", "REPEATABLE READ", "SERIALIZABLE", "SNAPSHOT"]
        end
        
        # === SQLServer Specific (Executing) ============================ #

        def do_execute(sql, name = nil)
          name ||= 'EXECUTE'
          log(sql, name) do
            with_auto_reconnect { raw_connection_do(sql) }
          end
        end
        
        def raw_connection_do(sql)
          case connection_mode
          when :odbc
            raw_connection.do(sql)
          else :adonet
            raw_connection.create_command.tap{ |cmd| cmd.command_text = sql }.execute_non_query
          end
        end
        
        # === SQLServer Specific (Selecting) ============================ #

        def raw_select(sql, name=nil, options={})
          log(sql,name) do
            begin
              handle = raw_connection_run(sql)
              handle_to_names_and_values(handle, options)
            ensure
              finish_statement_handle(handle)
            end
          end
        end
        
        def raw_connection_run(sql)
          with_auto_reconnect do
            case connection_mode
            when :odbc
              block_given? ? raw_connection.run_block(sql) { |handle| yield(handle) } : raw_connection.run(sql)
            else :adonet
              raw_connection.create_command.tap{ |cmd| cmd.command_text = sql }.execute_reader
            end
          end
        end
        
        def handle_more_results?(handle)
          case connection_mode
          when :odbc
            handle.more_results
          when :adonet
            handle.next_result
          end
        end
        
        def handle_to_names_and_values(handle, options={})
          case connection_mode
          when :odbc
            handle_to_names_and_values_odbc(handle, options)
          when :adonet
            handle_to_names_and_values_adonet(handle, options)
          end
        end

        def handle_to_names_and_values_odbc(handle, options={})
          case options[:fetch]
          when :all, :one
            rows = if options[:fetch] == :all
                     handle.fetch_all || []
                   else
                     row = handle.fetch
                     row ? [row] : [[]]                     
                   end
            names = handle.columns(true).map{ |c| c.name }
            names_and_values = []
            rows.each do |row|
              h = {}
              i = 0
              while i < row.size
                v = row[i]
                h[names[i]] = v.respond_to?(:to_sqlserver_string) ? v.to_sqlserver_string : v
                i += 1
              end
              names_and_values << h
            end
            names_and_values
          when :rows
            rows = handle.fetch_all || []
            rows.each do |row|
              i = 0
              while i < row.size
                v = row[i]
                row[i] = v.to_sqlserver_string if v.respond_to?(:to_sqlserver_string)
                i += 1
              end
            end
            rows
          end
        end
        
        def handle_to_names_and_values_adonet(handle, options={})
          if handle.has_rows
            names = []
            rows = []
            fields_named = options[:fetch] == :rows
            one_row_only = options[:fetch] == :one
            while handle.read
              row = []
              handle.visible_field_count.times do |row_index|
                value = handle.get_value(row_index)
                value = case value
                  when System::String
                    value.to_s
                  when System::DBNull
                    nil
                  when System::DateTime
                    value.to_string("yyyy-MM-dd HH:mm:ss.fff").to_s
                  when @@array_of_bytes ||= System::Array[System::Byte]
                    String.new(value)
                  else
                    value
                end
                row << value
                names << handle.get_name(row_index).to_s unless fields_named
                break if one_row_only
              end
              rows << row
              fields_named = true
            end
          else
            rows = []
          end
          
          if options[:fetch] != :rows
            names_and_values = []
            rows.each do |row|
              h = {}
              i = 0
              while i < row.size
                h[names[i]] = row[i]
                i += 1
              end
              names_and_values << h
            end
            names_and_values
          else
            rows
          end
        end
        
        def finish_statement_handle(handle)
          case connection_mode
          when :odbc
            handle.drop if handle && handle.respond_to?(:drop) && !handle.finished?
          when :adonet
            handle.close if handle && handle.respond_to?(:close) && !handle.is_closed
            handle.dispose if handle && handle.respond_to?(:dispose)
          end
          handle
        end
        
      end
    end
  end
end
