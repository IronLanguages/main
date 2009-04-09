module DataMapper
  module Constraints
    module MysqlAdapter
      module SQL
        include DataMapper::Constraints::DataObjectsAdapter::SQL

        private

        ##
        # MySQL specific query to determine to drop a foreign key
        #
        # @param table_name [Symbol] name of table to check constraint on
        #
        # @param constraint_name [~String] name of constraint to check for
        #
        # @return [String] SQL DDL to destroy a constraint
        #
        # @api private
        def destroy_constraints_statement(table_name, constraint_name)
          <<-EOS.compress_lines
            ALTER TABLE #{quote_table_name(table_name)}
            DROP FOREIGN KEY #{quote_constraint_name(constraint_name)}
          EOS
        end

        ##
        # MySQL specific query to determine if a constraint exists
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
            FROM `information_schema`.`table_constraints`
            WHERE `constraint_type` = 'FOREIGN KEY'
            AND `table_schema` = ?
            AND `table_name` = ?
            AND `constraint_name` = ?
          EOS
          query(statement, db_name, table_name, constraint_name).first > 0
        end
      end
    end
  end
end
