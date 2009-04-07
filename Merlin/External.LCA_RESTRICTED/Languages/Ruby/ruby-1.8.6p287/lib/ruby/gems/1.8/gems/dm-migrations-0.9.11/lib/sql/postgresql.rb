module SQL
  module Postgresql

    def supports_schema_transactions?
      true
    end

    def table(table_name)
      SQL::Postgresql::Table.new(self, table_name)
    end

    def recreate_database
      execute "DROP SCHEMA IF EXISTS test CASCADE"
      execute "CREATE SCHEMA test"
      execute "SET search_path TO test"
    end

    def supports_serial?
      true
    end

    def property_schema_statement(schema)
      if supports_serial? && schema[:serial]
        statement = "#{schema[:quote_column_name]} serial PRIMARY KEY"
      else
        statement = super
        if schema.has_key?(:sequence_name)
          statement << " DEFAULT nextval('#{schema[:sequence_name]}') NOT NULL"
        end
        statement
      end
      statement
    end

    def create_table_statement(quoted_table_name)
      "CREATE TABLE #{quoted_table_name}"
    end

    class Table < SQL::Table
      def initialize(adapter, table_name)
        @adapter, @name = adapter, table_name
        @columns = []
        adapter.query_table(table_name).each do |col_struct|
          @columns << SQL::Postgresql::Column.new(col_struct)
        end

        query_column_constraints
      end

      def query_column_constraints
        @adapter.query(
          "SELECT * FROM information_schema.table_constraints WHERE table_name='#{@name}' AND table_schema=current_schema()"
        ).each do |table_constraint|
          @adapter.query(
            "SELECT * FROM information_schema.constraint_column_usage WHERE constraint_name='#{table_constraint.constraint_name}' AND table_schema=current_schema()"
          ).each do |constrained_column|
            @columns.each do |column|
              if column.name == constrained_column.column_name
                case table_constraint.constraint_type
                when "UNIQUE"       then column.unique = true
                when "PRIMARY KEY"  then column.primary_key = true
                end
              end
            end
          end
        end

      end

    end

    class Column < SQL::Column
      def initialize(col_struct)
        @name, @type, @default_value = col_struct.column_name, col_struct.data_type, col_struct.column_default

        @not_null = col_struct.is_nullable != "YES"
      end

    end

  end
end
