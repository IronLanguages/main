module DBI
  module DBD
    module MSSQL
       
      class Database < DBI::BaseDatabase


        def initialize(dbd_db, attr)
          super
          self['AutoCommit'] = true
        end

        def disconnect
          unless @trans.nil?
            @trans.rollback unless @attr['AutoCommit']
            @trans = nil
          end
          @handle.close
        rescue RuntimeError, System::Data::SqlClient::SqlException => err
          raise DBI::DatabaseError.new(err.message)
        end

        def prepare(statement)
          Statement.new(statement, self)
        end

        def ping
          cmd = @handle.create_command
          cmd.command_text = "Select 1"
          begin
            cmd.execute_non_query
            return true
          rescue
            return false
          end
        rescue RuntimeError, System::Data::SqlClient::SqlException => err
          raise DBI::DatabaseError.new(err.message)
        end

        def tables
          @handle.get_schema("Tables").rows.collect { |row| row["TABLE_NAME"].to_s }
        rescue RuntimeError, System::Data::SqlClient::SqlException => err
          raise DBI::DatabaseError.new(err.message)
        end

        def current_transaction
          @trans
        end

        def has_transaction?
          !@trans.nil?
        end

        def columns(table)
          sql = "select object_name(c.object_id) as table_name, c.column_id, c.name, type_name(system_type_id) as sql_type,
              max_length, is_nullable, precision, scale, object_definition(c.default_object_id) as default_value,
              convert(bit,(Select COUNT(*) from sys.indexes as i
                inner join sys.index_columns as ic
                  on ic.index_id = i.index_id and ic.object_id = i.object_id
                inner join sys.columns as c2 on ic.column_id = c2.column_id and i.object_id = c2.object_id
              WHERE i.is_primary_key = 1 and ic.column_id = c.column_id and i.object_id=c.object_id)) as is_primary_key,               
              convert(bit,(Select COUNT(*) from sys.indexes as i
                inner join sys.index_columns as ic
                  on ic.index_id = i.index_id and ic.object_id = i.object_id
                inner join sys.columns as c2 on ic.column_id = c2.column_id and i.object_id = c2.object_id
              WHERE i.is_primary_key = 0
                and i.is_unique_constraint = 0 and ic.column_id = c.column_id and i.object_id=c.object_id)) as is_index,
              convert(bit,(Select Count(*) from sys.indexes as i inner join sys.index_columns as ic
                  on ic.index_id = i.index_id and ic.object_id = i.object_id
                inner join sys.columns as c2 on ic.column_id = c2.column_id and i.object_id = c2.object_id
              WHERE (i.is_unique_constraint = 1) and ic.column_id = c.column_id and i.object_id=c.object_id)) as is_unique
              from sys.columns as c
              WHERE object_name(c.object_id) = @table_name
              order by table_name"
          stmt = prepare(sql)
          stmt.bind_param("table_name", table)
          stmt.execute
          ret = stmt.fetch_all.collect do |row|
            dtn = row[3].upcase
            ColumnInfo.new({
                    'name' => row[2].to_s,
                    'dbi_type' => MSSQL_TYPEMAP[dtn],
                    'mssql_type_name' => dtn,
                    'sql_type' =>MSSQL_TO_XOPEN[dtn][0],
                    'type_name' => DBI::SQL_TYPE_NAMES[MSSQL_TO_XOPEN[dtn][0]],
                    'precision' => row[6].zero? ? row[4] : row[6],
                    'default' => row[8],
                    'scale' => row[7],
                    'nullable' => row[5],
                    'primary' => row[9],
                    'indexed' => row[10],
                    'unique' => row[11]
            })
          end
          stmt.finish
          ret
        end

        def commit
          unless @attr['AutoCommit']
            @trans.commit if @trans
            @trans = @handle.begin_transaction
          end
        rescue RuntimeError, System::Data::SqlClient::SqlException => err
          raise DBI::DatabaseError.new(err.message)
        end

        def rollback
          unless @attr['AutoCommit']
            @trans.rollback if @trans
            @trans = @handle.begin_transaction
          end
        rescue RuntimeError, System::Data::SqlClient::SqlException => err
          raise DBI::DatabaseError.new(err.message)
        end

        def do(stmt, bindvars={})
          st = prepare(stmt)
          bindvars.each { |k, v| st.bind_param(k, v) }
          res = st.execute
          st.finish
          return res
        rescue RuntimeError, System::Data::SqlClient::SqlException => err
          raise DBI::DatabaseError.new(err.message)
        ensure
          st.finish if st
        end

        def current_connection
          @handle
        end

        def []=(attr, value)
          if attr == 'AutoCommit' and @attr[attr] != value
            @trans.commit if @trans
            unless value
              @trans = @handle.begin_transaction unless @trans
            else
              @trans = nil
            end
          end
          @attr[attr] = value
        end

      end # class Database
    end
  end
end