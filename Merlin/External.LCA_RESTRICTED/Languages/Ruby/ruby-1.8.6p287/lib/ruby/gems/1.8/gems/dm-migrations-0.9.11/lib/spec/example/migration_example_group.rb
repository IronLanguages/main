require 'spec'

require File.dirname(__FILE__) + '/../matchers/migration_matchers'

module Spec
  module Example
    class MigrationExampleGroup < Spec::Example::ExampleGroup
      include Spec::Matchers::Migration

      before(:all) do
        if this_migration.adapter.supports_schema_transactions?
          run_prereq_migrations
        end
      end

      before(:each) do
        if ! this_migration.adapter.supports_schema_transactions?
          run_prereq_migrations
        else
          this_migration.adapter.begin_transaction
        end
      end

      after(:each) do
        if this_migration.adapter.supports_schema_transactions?
          this_migration.adapter.rollback_transaction
        end
      end

      after(:all) do
        this_migration.adapter.recreate_database
      end

      def run_prereq_migrations
        "running n-1 migrations"
        all_databases.each do |db|
          db.adapter.recreate_database
        end
        @@migrations.sort.each do |migration|
          break if migration.name.to_s == migration_name.to_s
          migration.perform_up
        end
      end

      def run_migration
        this_migration.perform_up
      end

      def migration_name
        @migration_name ||= self.class.instance_variable_get("@description_text").to_s
      end

      def all_databases
        @@migrations.map { |m| m.database }.uniq
      end

      def this_migration
        @@migrations.select { |m| m.name.to_s == migration_name }.first
      end

      def query(sql)
        this_migration.adapter.query(sql)
      end

      def table(table_name)
        this_migration.adapter.table(table_name)
      end

      Spec::Example::ExampleGroupFactory.register(:migration, self)

    end
  end
end
