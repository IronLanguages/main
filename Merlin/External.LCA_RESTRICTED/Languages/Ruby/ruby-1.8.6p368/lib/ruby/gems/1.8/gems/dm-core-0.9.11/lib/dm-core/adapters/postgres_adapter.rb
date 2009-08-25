gem 'do_postgres', '~>0.9.11'
require 'do_postgres'

module DataMapper
  module Adapters
    class PostgresAdapter < DataObjectsAdapter
      module SQL
        private

        def supports_returning?
          true
        end
      end #module SQL

      include SQL

      # TODO: move to dm-more/dm-migrations (if possible)
      module Migration
        # TODO: move to dm-more/dm-migrations (if possible)
        def storage_exists?(storage_name)
          statement = <<-SQL.compress_lines
            SELECT COUNT(*)
            FROM "information_schema"."tables"
            WHERE "table_type" = 'BASE TABLE'
            AND "table_schema" = current_schema()
            AND "table_name" = ?
          SQL

          query(statement, storage_name).first > 0
        end

        # TODO: move to dm-more/dm-migrations (if possible)
        def field_exists?(storage_name, column_name)
          statement = <<-SQL.compress_lines
            SELECT COUNT(*)
            FROM "information_schema"."columns"
            WHERE "table_schema" = current_schema()
            AND "table_name" = ?
            AND "column_name" = ?
          SQL

          query(statement, storage_name, column_name).first > 0
        end

        # TODO: move to dm-more/dm-migrations
        def upgrade_model_storage(repository, model)
          add_sequences(repository, model)
          super
        end

        # TODO: move to dm-more/dm-migrations
        def create_model_storage(repository, model)
          add_sequences(repository, model)
          without_notices { super }
        end

        # TODO: move to dm-more/dm-migrations
        def destroy_model_storage(repository, model)
          return true unless storage_exists?(model.storage_name(repository.name))
          success = without_notices { super }
          model.properties(repository.name).each do |property|
            drop_sequence(repository, property) if property.serial?
          end
          success
        end

        protected

        # TODO: move to dm-more/dm-migrations
        def create_sequence(repository, property)
          return if sequence_exists?(repository, property)
          execute(create_sequence_statement(repository, property))
        end

        # TODO: move to dm-more/dm-migrations
        def drop_sequence(repository, property)
          without_notices { execute(drop_sequence_statement(repository, property)) }
        end

        module SQL
          private

          # TODO: move to dm-more/dm-migrations
          def drop_table_statement(repository, model)
            "DROP TABLE #{quote_table_name(model.storage_name(repository.name))}"
          end

          # TODO: move to dm-more/dm-migrations
          def without_notices
            # execute the block with NOTICE messages disabled
            begin
              execute('SET client_min_messages = warning')
              yield
            ensure
              execute('RESET client_min_messages')
            end
          end

          # TODO: move to dm-more/dm-migrations
          def add_sequences(repository, model)
            model.properties(repository.name).each do |property|
              create_sequence(repository, property) if property.serial?
            end
          end

          # TODO: move to dm-more/dm-migrations
          def sequence_name(repository, property)
            "#{property.model.storage_name(repository.name)}_#{property.field(repository.name)}_seq"
          end

          # TODO: move to dm-more/dm-migrations
          def sequence_exists?(repository, property)
            statement = <<-EOS.compress_lines
              SELECT COUNT(*)
              FROM "information_schema"."sequences"
              WHERE "sequence_name" = ?
              AND "sequence_schema" = current_schema()
            EOS

            query(statement, sequence_name(repository, property)).first > 0
          end

          # TODO: move to dm-more/dm-migrations
          def create_sequence_statement(repository, property)
            "CREATE SEQUENCE #{quote_column_name(sequence_name(repository, property))}"
          end

          # TODO: move to dm-more/dm-migrations
          def drop_sequence_statement(repository, property)
            "DROP SEQUENCE IF EXISTS #{quote_column_name(sequence_name(repository, property))}"
          end

          # TODO: move to dm-more/dm-migrations
          def property_schema_statement(schema)
            statement = super

            if schema.has_key?(:sequence_name)
              statement << " DEFAULT nextval('#{schema[:sequence_name]}') NOT NULL"
            end

            statement
          end

          # TODO: move to dm-more/dm-migrations
          def property_schema_hash(repository, property)
            schema = super

            if property.serial?
              schema.delete(:default)  # the sequence will be the default
              schema[:sequence_name] = sequence_name(repository, property)
            end

            # TODO: see if TypeMap can be updated to set specific attributes to nil
            # for different adapters.  precision/scale are perfect examples for
            # Postgres floats

            # Postgres does not support precision and scale for Float
            if property.primitive == Float
              schema.delete(:precision)
              schema.delete(:scale)
            end

            schema
          end
        end # module SQL

        include SQL

        module ClassMethods
          # TypeMap for PostgreSQL databases.
          #
          # @return <DataMapper::TypeMap> default TypeMap for PostgreSQL databases.
          #
          # TODO: move to dm-more/dm-migrations
          def type_map
            @type_map ||= TypeMap.new(super) do |tm|
              tm.map(DateTime).to('TIMESTAMP')
              tm.map(Integer).to('INT4')
              tm.map(Float).to('FLOAT8')
            end
          end
        end # module ClassMethods
      end # module Migration

      include Migration
      extend Migration::ClassMethods
    end # class PostgresAdapter
  end # module Adapters
end # module DataMapper
