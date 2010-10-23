module ActiveRecord
  module ConnectionAdapters
    module Sqlserver
      module SchemaStatements
        
        def native_database_types
          {
            :primary_key  => "int NOT NULL IDENTITY(1,1) PRIMARY KEY",
            :string       => { :name => native_string_database_type, :limit => 255  },
            :text         => { :name => native_text_database_type },
            :integer      => { :name => "int", :limit => 4 },
            :float        => { :name => "float", :limit => 8 },
            :decimal      => { :name => "decimal" },
            :datetime     => { :name => "datetime" },
            :timestamp    => { :name => "datetime" },
            :time         => { :name => native_time_database_type },
            :date         => { :name => native_date_database_type },
            :binary       => { :name => native_binary_database_type },
            :boolean      => { :name => "bit"},
            # These are custom types that may move somewhere else for good schema_dumper.rb hacking to output them.
            :char         => { :name => 'char' },
            :varchar_max  => { :name => 'varchar(max)' },
            :nchar        => { :name => "nchar" },
            :nvarchar     => { :name => "nvarchar", :limit => 255 },
            :nvarchar_max => { :name => "nvarchar(max)" },
            :ntext        => { :name => "ntext" }
          }
        end

        def tables(name = nil)
          info_schema_query do
            select_values "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME <> 'dtproperties'"
          end
        end

        def table_exists?(table_name)
          unquoted_table_name = unqualify_table_name(table_name)
          super || tables.include?(unquoted_table_name) || views.include?(unquoted_table_name)
        end

        def indexes(table_name, name = nil)
          unquoted_table_name = unqualify_table_name(table_name)
          select("EXEC sp_helpindex #{quote_table_name(unquoted_table_name)}",name).inject([]) do |indexes,index|
            index = index.with_indifferent_access
            if index[:index_description] =~ /primary key/
              indexes
            else
              name    = index[:index_name]
              unique  = index[:index_description] =~ /unique/
              columns = index[:index_keys].split(',').map do |column|
                column.strip!
                column.gsub! '(-)', '' if column.ends_with?('(-)')
                column
              end
              indexes << IndexDefinition.new(table_name, name, unique, columns)
            end
          end
        end

        def columns(table_name, name = nil)
          return [] if table_name.blank?
          cache_key = unqualify_table_name(table_name)
          @sqlserver_columns_cache[cache_key] ||= column_definitions(table_name).collect do |ci|
            sqlserver_options = ci.except(:name,:default_value,:type,:null).merge(:database_year=>database_year)
            SQLServerColumn.new ci[:name], ci[:default_value], ci[:type], ci[:null], sqlserver_options
          end
        end

        def create_table(table_name, options = {})
          super
          remove_sqlserver_columns_cache_for(table_name)
        end

        def rename_table(table_name, new_name)
          do_execute "EXEC sp_rename '#{table_name}', '#{new_name}'"
        end

        def drop_table(table_name, options = {})
          super
          remove_sqlserver_columns_cache_for(table_name)
        end

        def add_column(table_name, column_name, type, options = {})
          super
          remove_sqlserver_columns_cache_for(table_name)
        end

        def remove_column(table_name, *column_names)
          raise ArgumentError.new("You must specify at least one column name.  Example: remove_column(:people, :first_name)") if column_names.empty?
          column_names.flatten.each do |column_name|
            remove_check_constraints(table_name, column_name)
            remove_default_constraint(table_name, column_name)
            remove_indexes(table_name, column_name)
            do_execute "ALTER TABLE #{quote_table_name(table_name)} DROP COLUMN #{quote_column_name(column_name)}"
          end
          remove_sqlserver_columns_cache_for(table_name)
        end

        def change_column(table_name, column_name, type, options = {})
          sql_commands = []
          column_object = columns(table_name).detect { |c| c.name.to_s == column_name.to_s }
          change_column_sql = "ALTER TABLE #{quote_table_name(table_name)} ALTER COLUMN #{quote_column_name(column_name)} #{type_to_sql(type, options[:limit], options[:precision], options[:scale])}"
          change_column_sql << " NOT NULL" if options[:null] == false
          sql_commands << change_column_sql
          if options_include_default?(options) || (column_object && column_object.type != type.to_sym)
           	remove_default_constraint(table_name,column_name)
          end
          if options_include_default?(options)
            remove_sqlserver_columns_cache_for(table_name)
            sql_commands << "ALTER TABLE #{quote_table_name(table_name)} ADD CONSTRAINT #{default_constraint_name(table_name,column_name)} DEFAULT #{quote(options[:default])} FOR #{quote_column_name(column_name)}"
          end
          sql_commands.each { |c| do_execute(c) }
          remove_sqlserver_columns_cache_for(table_name)
        end

        def change_column_default(table_name, column_name, default)
          remove_default_constraint(table_name, column_name)
          do_execute "ALTER TABLE #{quote_table_name(table_name)} ADD CONSTRAINT #{default_constraint_name(table_name, column_name)} DEFAULT #{quote(default)} FOR #{quote_column_name(column_name)}"
          remove_sqlserver_columns_cache_for(table_name)
        end

        def rename_column(table_name, column_name, new_column_name)
          detect_column_for!(table_name,column_name)
          do_execute "EXEC sp_rename '#{table_name}.#{column_name}', '#{new_column_name}', 'COLUMN'"
          remove_sqlserver_columns_cache_for(table_name)
        end
        
        def remove_index!(table_name, index_name)
          do_execute "DROP INDEX #{quote_table_name(table_name)}.#{quote_column_name(index_name)}"
        end

        def type_to_sql(type, limit = nil, precision = nil, scale = nil)
          type_limitable = ['string','integer','float','char','nchar','varchar','nvarchar'].include?(type.to_s)
          limit = nil unless type_limitable
          case type.to_s
          when 'integer'
            case limit
              when 1..2       then  'smallint'
              when 3..4, nil  then  'integer'
              when 5..8       then  'bigint'
              else raise(ActiveRecordError, "No integer type has byte size #{limit}. Use a numeric with precision 0 instead.")
            end
          else
            super
          end
        end

        def change_column_null(table_name, column_name, null, default = nil)
          column = detect_column_for!(table_name,column_name)
          unless null || default.nil?
            do_execute("UPDATE #{quote_table_name(table_name)} SET #{quote_column_name(column_name)}=#{quote(default)} WHERE #{quote_column_name(column_name)} IS NULL")
          end
          sql = "ALTER TABLE #{table_name} ALTER COLUMN #{quote_column_name(column_name)} #{type_to_sql column.type, column.limit, column.precision, column.scale}"
          sql << " NOT NULL" unless null
          do_execute sql
        end
        
        # === SQLServer Specific ======================================== #
        
        def views(name = nil)
          @sqlserver_views_cache ||= 
            info_schema_query { select_values("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME NOT IN ('sysconstraints','syssegments')") }
        end
        
        
        protected
        
        # === SQLServer Specific ======================================== #
        
        def column_definitions(table_name)
          db_name = unqualify_db_name(table_name)
          db_name_with_period = "#{db_name}." if db_name
          table_name = unqualify_table_name(table_name)
          sql = %{
            SELECT
            columns.TABLE_NAME as table_name,
            columns.COLUMN_NAME as name,
            columns.DATA_TYPE as type,
            columns.COLUMN_DEFAULT as default_value,
            columns.NUMERIC_SCALE as numeric_scale,
            columns.NUMERIC_PRECISION as numeric_precision,
            CASE
              WHEN columns.DATA_TYPE IN ('nchar','nvarchar') THEN columns.CHARACTER_MAXIMUM_LENGTH
              ELSE COL_LENGTH(columns.TABLE_SCHEMA+'.'+columns.TABLE_NAME, columns.COLUMN_NAME)
            END as length,
            CASE
              WHEN columns.IS_NULLABLE = 'YES' THEN 1
              ELSE NULL
            END as is_nullable,
            CASE
              WHEN COLUMNPROPERTY(OBJECT_ID(columns.TABLE_SCHEMA+'.'+columns.TABLE_NAME), columns.COLUMN_NAME, 'IsIdentity') = 0 THEN NULL
              ELSE 1
            END as is_identity
            FROM #{db_name_with_period}INFORMATION_SCHEMA.COLUMNS columns
            WHERE columns.TABLE_NAME = '#{table_name}'
            ORDER BY columns.ordinal_position
          }.gsub(/[ \t\r\n]+/,' ')
          results = info_schema_query { select(sql,nil) }
          results.collect do |ci|
            ci = ci.symbolize_keys
            ci[:type] = case ci[:type]
                         when /^bit|image|text|ntext|datetime$/
                           ci[:type]
                         when /^numeric|decimal$/i
                           "#{ci[:type]}(#{ci[:numeric_precision]},#{ci[:numeric_scale]})"
                         when /^char|nchar|varchar|nvarchar|varbinary|bigint|int|smallint$/
                           ci[:length].to_i == -1 ? "#{ci[:type]}(max)" : "#{ci[:type]}(#{ci[:length]})"
                         else
                           ci[:type]
                         end
            if ci[:default_value].nil? && views.include?(table_name)
              real_table_name = table_name_or_views_table_name(table_name)
              real_column_name = views_real_column_name(table_name,ci[:name])
              col_default_sql = "SELECT c.COLUMN_DEFAULT FROM #{db_name_with_period}INFORMATION_SCHEMA.COLUMNS c WHERE c.TABLE_NAME = '#{real_table_name}' AND c.COLUMN_NAME = '#{real_column_name}'"
              ci[:default_value] = info_schema_query { select_value(col_default_sql) }
            end
            ci[:default_value] = case ci[:default_value]
                                 when nil, '(null)', '(NULL)'
                                   nil
                                 when /\A\((\w+\(\))\)\Z/
                                   ci[:default_function] = $1
                                   nil
                                 else
                                   match_data = ci[:default_value].match(/\A\(+N?'?(.*?)'?\)+\Z/m)
                                   match_data ? match_data[1] : nil
                                 end
            ci[:null] = ci[:is_nullable].to_i == 1 ; ci.delete(:is_nullable)
            ci
          end
        end
        
        def remove_check_constraints(table_name, column_name)
          constraints = info_schema_query { select_values("SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE where TABLE_NAME = '#{quote_string(table_name)}' and COLUMN_NAME = '#{quote_string(column_name)}'") }
          constraints.each do |constraint|
            do_execute "ALTER TABLE #{quote_table_name(table_name)} DROP CONSTRAINT #{quote_column_name(constraint)}"
          end
        end

        def remove_default_constraint(table_name, column_name)
          select_all("EXEC sp_helpconstraint '#{quote_string(table_name)}','nomsg'").select do |row|
            row['constraint_type'] == "DEFAULT on column #{column_name}"
          end.each do |row|
            do_execute "ALTER TABLE #{quote_table_name(table_name)} DROP CONSTRAINT #{row['constraint_name']}"
          end
        end

        def remove_indexes(table_name, column_name)
          indexes(table_name).select{ |index| index.columns.include?(column_name.to_s) }.each do |index|
            remove_index(table_name, {:name => index.name})
          end
        end
        
        # === SQLServer Specific (Misc Helpers) ========================= #
        
        def info_schema_query
          log_info_schema_queries ? yield : ActiveRecord::Base.silence{ yield }
        end
        
        def unqualify_table_name(table_name)
          table_name.to_s.split('.').last.tr('[]','')
        end

        def unqualify_db_name(table_name)
          table_names = table_name.to_s.split('.')
          table_names.length == 3 ? table_names.first.tr('[]','') : nil
        end
        
        def get_table_name(sql)
          if sql =~ /^\s*insert\s+into\s+([^\(\s]+)\s*|^\s*update\s+([^\(\s]+)\s*/i
            $1 || $2
          elsif sql =~ /from\s+([^\(\s]+)\s*/i
            $1
          else
            nil
          end
        end
        
        def default_constraint_name(table_name, column_name)
          "DF_#{table_name}_#{column_name}"
        end
        
        def detect_column_for!(table_name, column_name)
          unless column = columns(table_name).detect { |c| c.name == column_name.to_s }
            raise ActiveRecordError, "No such column: #{table_name}.#{column_name}"
          end
          column
        end
        
        # === SQLServer Specific (View Reflection) ====================== #
        
        def view_table_name(table_name)
          view_info = view_information(table_name)
          view_info ? get_table_name(view_info['VIEW_DEFINITION']) : table_name
        end
        
        def view_information(table_name)
          table_name = unqualify_table_name(table_name)
          @sqlserver_view_information_cache[table_name] ||= begin
            view_info = info_schema_query { select_one("SELECT * FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = '#{table_name}'") }
            if view_info
              view_info = view_info.with_indifferent_access
              if view_info[:VIEW_DEFINITION].blank? || view_info[:VIEW_DEFINITION].length == 4000
                view_info[:VIEW_DEFINITION] = info_schema_query { select_values("EXEC sp_helptext #{table_name}").join }
              end
            end
            view_info
          end
        end
        
        def table_name_or_views_table_name(table_name)
          unquoted_table_name = unqualify_table_name(table_name)
          views.include?(unquoted_table_name) ? view_table_name(unquoted_table_name) : unquoted_table_name
        end
        
        def views_real_column_name(table_name,column_name)
          view_definition = view_information(table_name)[:VIEW_DEFINITION]
          match_data = view_definition.match(/([\w-]*)\s+as\s+#{column_name}/im)
          match_data ? match_data[1] : column_name
        end
        
        # === SQLServer Specific (Column/View Caches) =================== #
        
        def remove_sqlserver_columns_cache_for(table_name)
          cache_key = unqualify_table_name(table_name)
          @sqlserver_columns_cache[cache_key] = nil
          initialize_sqlserver_caches(false)
        end

        def initialize_sqlserver_caches(reset_columns=true)
          @sqlserver_columns_cache = {} if reset_columns
          @sqlserver_views_cache = nil
          @sqlserver_view_information_cache = {}
        end
        
        # === SQLServer Specific (Identity Inserts) ===================== #

        def query_requires_identity_insert?(sql)
          if insert_sql?(sql)
            table_name = get_table_name(sql)
            id_column = identity_column(table_name)
            id_column && sql =~ /^\s*INSERT[^(]+\([^)]*\b(#{id_column.name})\b,?[^)]*\)/i ? quote_table_name(table_name) : false
          else
            false
          end
        end
        
        def insert_sql?(sql)
          !(sql =~ /^\s*INSERT/i).nil?
        end
        
        def with_identity_insert_enabled(table_name)
          table_name = quote_table_name(table_name_or_views_table_name(table_name))
          set_identity_insert(table_name, true)
          yield
        ensure
          set_identity_insert(table_name, false)
        end

        def set_identity_insert(table_name, enable = true)
          sql = "SET IDENTITY_INSERT #{table_name} #{enable ? 'ON' : 'OFF'}"
          do_execute(sql,'IDENTITY_INSERT')
        rescue Exception => e
          raise ActiveRecordError, "IDENTITY_INSERT could not be turned #{enable ? 'ON' : 'OFF'} for table #{table_name}"
        end

        def identity_column(table_name)
          columns(table_name).detect(&:is_identity?)
        end

      end
    end
  end
end
