module DataMapper
  module Constraints
    module PostgresAdapter
      module SQL
        include DataMapper::Constraints::DataObjectsAdapter::SQL

        private

        ##
        # Postgres specific query to determine if a constraint exists
        #
        # @param table_name [Symbol] name of table to check constraint on
        #
        # @param constraint_name [~String] name of constraint to check for
        #
        # @return [Boolean]
        #
        # @api private
        def constraint_exists?(table_name, constraint_name)
          statement = <<-EOS.compress_lines
            SELECT COUNT(*)
            FROM "information_schema"."table_constraints"
            WHERE "table_schema" = current_schema()
            AND "table_name" = ?
            AND "constraint_name" = ?
          EOS
          query(statement, table_name, constraint_name).first > 0
        end
      end
    end
  end
end
