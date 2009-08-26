gem 'do_mysql', '~>0.9.11'
require 'do_mysql'

module DataMapper
  module Adapters
    # Options:
    # host, user, password, database (path), socket(uri query string), port
    class MysqlAdapter < DataObjectsAdapter
      module SQL
        private

        def supports_default_values?
          false
        end

        def quote_table_name(table_name)
          "`#{table_name.gsub('`', '``')}`"
        end

        def quote_column_name(column_name)
          "`#{column_name.gsub('`', '``')}`"
        end

        def quote_column_value(column_value)
          case column_value
            when TrueClass  then quote_column_value(1)
            when FalseClass then quote_column_value(0)
            else
              super
          end
        end
      end #module SQL

      include SQL

      # TODO: move to dm-more/dm-migrations
      module Migration
        # TODO: move to dm-more/dm-migrations (if possible)
        def storage_exists?(storage_name)
          statement = <<-EOS.compress_lines
            SELECT COUNT(*)
            FROM `information_schema`.`tables`
            WHERE `table_type` = 'BASE TABLE'
            AND `table_schema` = ?
            AND `table_name` = ?
          EOS

          query(statement, db_name, storage_name).first > 0
        end

        # TODO: move to dm-more/dm-migrations (if possible)
        def field_exists?(storage_name, field_name)
          statement = <<-EOS.compress_lines
            SELECT COUNT(*)
            FROM `information_schema`.`columns`
            WHERE `table_schema` = ?
            AND `table_name` = ?
            AND `column_name` = ?
          EOS

          query(statement, db_name, storage_name, field_name).first > 0
        end

        private

        # TODO: move to dm-more/dm-migrations (if possible)
        def db_name
          @uri.path.split('/').last
        end

        module SQL
          private

          # TODO: move to dm-more/dm-migrations
          def supports_serial?
            true
          end

          # TODO: move to dm-more/dm-migrations
          def create_table_statement(repository, model)
            "#{super} ENGINE = InnoDB CHARACTER SET #{character_set} COLLATE #{collation}"
          end

          # TODO: move to dm-more/dm-migrations
          def property_schema_hash(property, model)
            schema = super
            schema.delete(:default) if schema[:primitive] == 'TEXT'
            schema
          end

          # TODO: move to dm-more/dm-migrations
          def property_schema_statement(schema)
            statement = super
            statement << ' AUTO_INCREMENT' if supports_serial? && schema[:serial?]
            statement
          end

          # TODO: move to dm-more/dm-migrations
          def character_set
            @character_set ||= show_variable('character_set_connection') || 'utf8'
          end

          # TODO: move to dm-more/dm-migrations
          def collation
            @collation ||= show_variable('collation_connection') || 'utf8_general_ci'
          end

          # TODO: move to dm-more/dm-migrations
          def show_variable(name)
            query('SHOW VARIABLES WHERE `variable_name` = ?', name).first.value rescue nil
          end
        end # module SQL

        include SQL

        module ClassMethods
          # TypeMap for MySql databases.
          #
          # @return <DataMapper::TypeMap> default TypeMap for MySql databases.
          #
          # TODO: move to dm-more/dm-migrations
          def type_map
            @type_map ||= TypeMap.new(super) do |tm|
              tm.map(Integer).to('INT').with(:size => 11)
              tm.map(TrueClass).to('TINYINT').with(:size => 1)  # TODO: map this to a BIT or CHAR(0) field?
              tm.map(Object).to('TEXT')
            end
          end
        end # module ClassMethods
      end # module Migration

      include Migration
      extend Migration::ClassMethods
    end # class MysqlAdapter
  end # module Adapters
end # module DataMapper
