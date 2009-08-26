gem 'do_sqlite3', '~>0.9.11'
require 'do_sqlite3'

module DataMapper
  module Adapters
    class Sqlite3Adapter < DataObjectsAdapter
      module SQL
        private

        def quote_column_value(column_value)
          case column_value
            when TrueClass  then quote_column_value('t')
            when FalseClass then quote_column_value('f')
            else
              super
          end
        end
      end # module SQL

      include SQL

      # TODO: move to dm-more/dm-migrations (if possible)
      module Migration
        # TODO: move to dm-more/dm-migrations (if possible)
        def storage_exists?(storage_name)
          query_table(storage_name).size > 0
        end

        # TODO: move to dm-more/dm-migrations (if possible)
        def field_exists?(storage_name, column_name)
          query_table(storage_name).any? do |row|
            row.name == column_name
          end
        end

        private

        # TODO: move to dm-more/dm-migrations (if possible)
        def query_table(table_name)
          query('PRAGMA table_info(?)', table_name)
        end

        module SQL
#          private  ## This cannot be private for current migrations

          # TODO: move to dm-more/dm-migrations
          def supports_serial?
            sqlite_version >= '3.1.0'
          end

          # TODO: move to dm-more/dm-migrations
          def create_table_statement(repository, model)
            statement = <<-EOS.compress_lines
              CREATE TABLE #{quote_table_name(model.storage_name(repository.name))}
              (#{model.properties_with_subclasses(repository.name).map { |p| property_schema_statement(property_schema_hash(repository, p)) } * ', '}
            EOS

            # skip adding the primary key if one of the columns is serial.  In
            # SQLite the serial column must be the primary key, so it has already
            # been defined
            unless model.properties(repository.name).any? { |p| p.serial? }
              if (key = model.properties(repository.name).key).any?
                statement << ", PRIMARY KEY(#{key.map { |p| quote_column_name(p.field(repository.name)) } * ', '})"
              end
            end

            statement << ')'
            statement
          end

          # TODO: move to dm-more/dm-migrations
          def property_schema_statement(schema)
            statement = super
            statement << ' PRIMARY KEY AUTOINCREMENT' if supports_serial? && schema[:serial?]
            statement
          end

          # TODO: move to dm-more/dm-migrations
          def sqlite_version
            @sqlite_version ||= query('SELECT sqlite_version(*)').first
          end
        end # module SQL

        include SQL

        module ClassMethods
          # TypeMap for SQLite 3 databases.
          #
          # @return <DataMapper::TypeMap> default TypeMap for SQLite 3 databases.
          #
          # TODO: move to dm-more/dm-migrations
          def type_map
            @type_map ||= TypeMap.new(super) do |tm|
              tm.map(Integer).to('INTEGER')
              tm.map(Class).to('VARCHAR')
            end
          end
        end # module ClassMethods
      end # module Migration

      include Migration
      extend Migration::ClassMethods
    end # class Sqlite3Adapter
  end # module Adapters
end # module DataMapper
