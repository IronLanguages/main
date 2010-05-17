module DBI
  module DBD
    module MSSQL

      class Statement < DBI::BaseStatement

        def initialize(statement, db)
          @connection = db.current_connection;
          @command = @connection.create_command
          @previous_statement = @connection.instance_variable_get :@current_statement
          @connection.instance_variable_set :@current_statement, self
          @statement = statement.to_s
          @command.command_text = @statement.to_clr_string
          @command.transaction = db.current_transaction if db.has_transaction?
          @current_index = 0
          @db = db
        end

        def bind_param(name, value, attribs={})
          unless name.to_s.empty?
            parameter = @command.create_parameter
            parm_name = name.to_s.to_clr_string
            parameter.ParameterName = parm_name
            val =  value.is_a?(String) ? value.to_clr_string : value #(value || System::DBNull.value)
            parameter.Value = val
            if @command.parameters.contains(parm_name)
              @command.parameters[parm_name] = parameter
            else
              @command.parameters.add parameter
            end                                            
          end
        end

        def execute
          @previous_statement.finish if @previous_statement
          
          @current_index = 0
          @rows = []
          @schema = nil
          finish
          @reader = @command.execute_reader
          schema

          unless SQL.query?(@statement.to_s)
            do_finish true
          end
          @reader.records_affected
        rescue RuntimeError, System::Data::SqlClient::SqlException => err
          raise DBI::DatabaseError.new(err.message)
        end

        def fetch
          if @reader and @reader.is_closed
            if @pending_fetches
              return @pending_fetches.shift
            else
              return nil
            end
          end
          res = nil
          if  @reader.read
            res = read_row(@reader)
          end
          res
        rescue RuntimeError, System::Data::SqlClient::SqlException => err
          raise DBI::DatabaseError.new(err.message)
        end

        def finish
          do_finish false
        end
        
        def do_finish(check_pending_fetches)
          if @reader and not @reader.is_closed
            if check_pending_fetches
              @pending_fetches = []
              while (res = fetch) != nil
                @pending_fetches << res
              end
            else
              @pending_fetches = nil
            end
          @reader.close
        end
          
        rescue RuntimeError, System::Data::SqlClient::SqlException => err
          raise DBI::DatabaseError.new(err.message)
        end

        def cancel
          finish
          @command.cancel
        end

        def schema
          @schema ||= @reader.get_schema_table || System::Data::DataTable.new
        end

        def column_info
          infos = schema.rows.collect do |row|
            name = row["ColumnName"]
            def_val_col = row.table.columns[name]
            def_val = def_val_col.nil? ? nil : def_val_col.default_value
            dtn = row["DataTypeName"].to_s.upcase
            {

                'name' => name.to_s,
                'dbi_type' => MSSQL_TYPEMAP[dtn],
                'mssql_type_name' => dtn,
                'sql_type' =>MSSQL_TO_XOPEN[dtn][0],
                'type_name' => DBI::SQL_TYPE_NAMES[MSSQL_TO_XOPEN[dtn][0]],
                'precision' => %w(varchar nvarchar char nchar text ntext).include?(dtn.downcase) ? row["ColumnSize"] : row["NumericPrecision"],
                'default' => def_val,
                'scale' => %w(varchar nvarchar char nchar text ntext).include?(dtn.downcase) ? nil : row["NumericScale"]  ,
                'nullable' => row["AllowDBNull"],
                'primary' => schema.primary_key.select { |pk| pk.column_name == name }.size > 0,
                'unique' => row["IsUnique"]
            }
          end 
          infos
        rescue RuntimeError, System::Data::SqlClient::SqlException => err
          raise DBI::DatabaseError.new(err.message)
        end

        def rows
          return 0 if @reader.nil?
          res = @reader.records_affected
          res == -1 ? 0 : res
        end

        private

        def read_row(record)
          (0...record.visible_field_count).collect do |i|
            res = record.get_value(i)
            res.is_a?(System::DBNull) ? nil : res
          end
        end

      end
    end
  end
end