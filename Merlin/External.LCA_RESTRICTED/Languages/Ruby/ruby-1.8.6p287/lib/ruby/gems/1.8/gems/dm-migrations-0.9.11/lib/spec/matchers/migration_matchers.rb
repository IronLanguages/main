
module Spec
  module Matchers
    module Migration

      def have_table(table_name)
        HaveTableMatcher.new(table_name)
      end

      def have_column(column_name)
        HaveColumnMatcher.new(column_name)
      end

      def permit_null
        NullableColumnMatcher.new
      end

      def be_primary_key
        PrimaryKeyMatcher.new
      end

      class HaveTableMatcher

        attr_accessor :table_name, :repository

        def initialize(table_name)
          @table_name = table_name
        end

        def matches?(repository)
          repository.adapter.storage_exists?(table_name)
        end

        def failure_message
          %(expected #{repository} to have table '#{table_name}')
        end

        def negative_failure_message
          %(expected #{repository} to not have table '#{table_name}')
        end

      end

      class HaveColumnMatcher

        attr_accessor :table, :column_name

        def initialize(column_name)
          @column_name = column_name
        end

        def matches?(table)
          @table = table
          table.columns.map { |c| c.name }.include?(column_name.to_s)
        end

        def failure_message
          %(expected #{table} to have column '#{column_name}')
        end

        def negative_failure_message
          %(expected #{table} to not have column '#{column_name}')
        end

      end

      class NullableColumnMatcher

        attr_accessor :column

        def matches?(column)
          @column = column
          ! column.not_null
        end

        def failure_message
          %(expected #{column.name} to permit NULL)
        end

        def negative_failure_message
          %(expected #{column.name} to be NOT NULL)
        end

      end

      class PrimaryKeyMatcher

        attr_accessor :column

        def matches?(column)
          @column = column
          column.primary_key
        end

        def failure_message
          %(expected #{column.name} to be PRIMARY KEY)
        end

        def negative_failure_message
          %(expected #{column.name} to not be PRIMARY KEY)
        end

      end

    end
  end
end
