module SQL
  class TableModifier
    attr_accessor :table_name, :opts, :statements, :adapter

    def initialize(adapter, table_name, opts = {}, &block)
      @adapter = adapter
      @table_name = table_name.to_s
      @opts = (opts)

      @statements = []

      self.instance_eval &block
    end

    def add_column(name, type, opts = {})
      column = SQL::TableCreator::Column.new(@adapter, name, type, opts)
      @statements << "ALTER TABLE #{quoted_table_name} ADD COLUMN #{column.to_sql}"
    end

    def drop_column(name)
      # raise NotImplemented for SQLite3. Can't ALTER TABLE, need to copy table.
      # We'd have to inspect it, and we can't, since we aren't executing any queries yet.
      # TODO instead of building the SQL queries when executing the block, create AddColumn,
      # AlterColumn and DropColumn objects that get #to_sql'd
      if name.is_a?(Array)
        name.each{ |n| drop_column(n) }
      else
        @statements << "ALTER TABLE #{quoted_table_name} DROP COLUMN #{quote_column_name(name)}"
      end
    end
    alias drop_columns drop_column

    def rename_column(name, new_name, opts = {})
      # raise NotImplemented for SQLite3
      @statements << "ALTER TABLE #{quoted_table_name} RENAME COLUMN #{quote_column_name(name)} TO #{quote_column_name(new_name)}"
    end

    def change_column(name, type, opts = {})
      # raise NotImplemented for SQLite3
      @statements << "ALTER TABLE #{quoted_table_name} ALTER COLUMN #{quote_column_name(name)} TYPE #{type}"
    end

    def quote_column_name(name)
      @adapter.send(:quote_column_name, name.to_s)
    end

    def quoted_table_name
      @adapter.send(:quote_table_name, table_name)
    end

  end

end
