require 'rubygems'
gem 'dm-core', '~>0.9.11'
require 'dm-core'
require 'benchmark'
require File.dirname(__FILE__) + '/sql'

module DataMapper
  class DuplicateMigrationNameError < StandardError
    def initialize(migration)
      super("Duplicate Migration Name: '#{migration.name}', version: #{migration.position}")
    end
  end

  class Migration
    include SQL

    attr_accessor :position, :name, :database, :adapter

    def initialize( position, name, opts = {}, &block )
      @position, @name = position, name
      @options = opts

      @database = DataMapper.repository(@options[:database] || :default)
      @adapter = @database.adapter

      case @adapter.class.to_s
      when /Sqlite3/  then @adapter.extend(SQL::Sqlite3)
      when /Mysql/    then @adapter.extend(SQL::Mysql)
      when /Postgres/ then @adapter.extend(SQL::Postgresql)
      else
        raise "Unsupported Migration Adapter #{@adapter.class}"
      end

      @verbose = @options.has_key?(:verbose) ? @options[:verbose] : true

      @up_action   = lambda {}
      @down_action = lambda {}

      instance_eval &block
    end

    # define the actions that should be performed on an up migration
    def up(&block)
      @up_action = block
    end

    # define the actions that should be performed on a down migration
    def down(&block)
      @down_action = block
    end

    # perform the migration by running the code in the #up block
    def perform_up
      result = nil
      if needs_up?
        # TODO: fix this so it only does transactions for databases that support create/drop
        # database.transaction.commit do
          say_with_time "== Performing Up Migration ##{position}: #{name}", 0 do
            result = @up_action.call
          end
          update_migration_info(:up)
        # end
      end
      result
    end

    # un-do the migration by running the code in the #down block
    def perform_down
      result = nil
      if needs_down?
        # TODO: fix this so it only does transactions for databases that support create/drop
        # database.transaction.commit do
          say_with_time "== Performing Down Migration ##{position}: #{name}", 0 do
            result = @down_action.call
          end
          update_migration_info(:down)
        # end
      end
      result
    end

    # execute raw SQL
    def execute(sql, *bind_values)
      say_with_time(sql) do
        @adapter.execute(sql, *bind_values)
      end
    end

    def create_table(table_name, opts = {}, &block)
      execute TableCreator.new(@adapter, table_name, opts, &block).to_sql
    end

    def drop_table(table_name, opts = {})
      execute "DROP TABLE #{@adapter.send(:quote_table_name, table_name.to_s)}"
    end

    def modify_table(table_name, opts = {}, &block)
      TableModifier.new(@adapter, table_name, opts, &block).statements.each do |sql|
        execute(sql)
      end
    end

    def create_index(table_name, *columns_and_options)
      if columns_and_options.last.is_a?(Hash)
        opts = columns_and_options.pop
      else
        opts = {}
      end
      columns = columns_and_options.flatten

      opts[:name] ||= "#{opts[:unique] ? 'unique_' : ''}index_#{table_name}_#{columns.join('_')}"

      execute <<-SQL.compress_lines
        CREATE #{opts[:unique] ? 'UNIQUE ' : '' }INDEX #{quote_column_name(opts[:name])} ON
        #{quote_table_name(table_name)} (#{columns.map { |c| quote_column_name(c) }.join(', ') })
      SQL
    end

    # Orders migrations by position, so we know what order to run them in.
    # First order by postition, then by name, so at least the order is predictable.
    def <=> other
      if self.position == other.position
        self.name.to_s <=> other.name.to_s
      else
        self.position <=> other.position
      end
    end

    # Output some text. Optional indent level
    def say(message, indent = 4)
      write "#{" " * indent} #{message}"
    end

    # Time how long the block takes to run, and output it with the message.
    def say_with_time(message, indent = 2)
      say(message, indent)
      result = nil
      time = Benchmark.measure { result = yield }
      say("-> %.4fs" % time.real, indent)
      result
    end

    # output the given text, but only if verbose mode is on
    def write(text="")
      puts text if @verbose
    end

    # Inserts or removes a row into the `migration_info` table, so we can mark this migration as run, or un-done
    def update_migration_info(direction)
      save, @verbose = @verbose, false

      create_migration_info_table_if_needed

      if direction.to_sym == :up
        execute("INSERT INTO #{migration_info_table} (#{migration_name_column}) VALUES (#{quoted_name})")
      elsif direction.to_sym == :down
        execute("DELETE FROM #{migration_info_table} WHERE #{migration_name_column} = #{quoted_name}")
      end
      @verbose = save
    end

    def create_migration_info_table_if_needed
      save, @verbose = @verbose, false
      unless migration_info_table_exists?
        execute("CREATE TABLE #{migration_info_table} (#{migration_name_column} VARCHAR(255) UNIQUE)")
      end
      @verbose = save
    end

    # Quote the name of the migration for use in SQL
    def quoted_name
      "'#{name}'"
    end

    def migration_info_table_exists?
      adapter.storage_exists?('migration_info')
    end

    # Fetch the record for this migration out of the migration_info table
    def migration_record
      return [] unless migration_info_table_exists?
      @adapter.query("SELECT #{migration_name_column} FROM #{migration_info_table} WHERE #{migration_name_column} = #{quoted_name}")
    end

    # True if the migration needs to be run
    def needs_up?
      return true unless migration_info_table_exists?
      migration_record.empty?
    end

    # True if the migration has already been run
    def needs_down?
      return false unless migration_info_table_exists?
      ! migration_record.empty?
    end

    # Quoted table name, for the adapter
    def migration_info_table
      @migration_info_table ||= quote_table_name('migration_info')
    end

    # Quoted `migration_name` column, for the adapter
    def migration_name_column
      @migration_name_column ||= quote_column_name('migration_name')
    end

    def quote_table_name(table_name)
      # TODO: Fix this for 1.9 - can't use this hack to access a private method
      @adapter.send(:quote_table_name, table_name.to_s)
    end

    def quote_column_name(column_name)
      # TODO: Fix this for 1.9 - can't use this hack to access a private method
      @adapter.send(:quote_column_name, column_name.to_s)
    end
  end
end
