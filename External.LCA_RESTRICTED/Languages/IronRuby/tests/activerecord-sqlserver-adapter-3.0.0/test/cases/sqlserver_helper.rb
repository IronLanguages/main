
SQLSERVER_TEST_ROOT       = File.expand_path(File.join(File.dirname(__FILE__),'..'))
SQLSERVER_ASSETS_ROOT     = File.expand_path(File.join(SQLSERVER_TEST_ROOT,'assets'))
SQLSERVER_FIXTURES_ROOT   = File.expand_path(File.join(SQLSERVER_TEST_ROOT,'fixtures'))
SQLSERVER_MIGRATIONS_ROOT = File.expand_path(File.join(SQLSERVER_TEST_ROOT,'migrations'))
SQLSERVER_SCHEMA_ROOT     = File.expand_path(File.join(SQLSERVER_TEST_ROOT,'schema'))
ACTIVERECORD_TEST_ROOT    = File.expand_path(File.join(ENV['RAILS_SOURCE'],'activerecord','test'))

require 'rubygems'
require 'bundler'
# TODO(IronRuby): removed due to different test layout
# Bundler.setup
require 'shoulda'
require 'mocha'
begin ; require 'ruby-debug' ; rescue LoadError ; end
[ File.expand_path(File.join(File.dirname(__FILE__),'..','..','test')),
  File.expand_path(File.join(File.dirname(__FILE__),'..','..','test','connections','native_sqlserver_odbc')),
  File.expand_path(File.join(ENV['RAILS_SOURCE'],'activerecord','test'))
].each{ |lib| $:.unshift(lib) unless $:.include?(lib) } if ENV['TM_DIRECTORY']
require 'cases/helper'
require 'models/topic'
require 'active_record/version'

GC.copy_on_write_friendly = true if GC.respond_to?(:copy_on_write_friendly?)

ActiveRecord::Migration.verbose = false

# Defining our classes in one place as well as soem core tests that need coercing date/time types.

class TableWithRealColumn < ActiveRecord::Base; end
class FkTestHasFk < ActiveRecord::Base ; end
class FkTestHasPk < ActiveRecord::Base ; end
class NumericData < ActiveRecord::Base ; self.table_name = 'numeric_data' ; end
class CustomersView < ActiveRecord::Base ; self.table_name = 'customers_view' ; end
class StringDefaultsView < ActiveRecord::Base ; self.table_name = 'string_defaults_view' ; end
class StringDefaultsBigView < ActiveRecord::Base ; self.table_name = 'string_defaults_big_view' ; end
class SqlServerUnicode < ActiveRecord::Base ; end
class SqlServerString < ActiveRecord::Base ; end
class SqlServerChronic < ActiveRecord::Base
  coerce_sqlserver_date :date
  coerce_sqlserver_time :time
  default_timezone = :utc
end
class Topic < ActiveRecord::Base
  coerce_sqlserver_date :last_read
  coerce_sqlserver_time :bonus_time
end
class Person < ActiveRecord::Base
  coerce_sqlserver_date :favorite_day
end

# A module that we can include in classes where we want to override an active record test.

module SqlserverCoercedTest
  def self.included(base)
    base.extend ClassMethods
  end
  module ClassMethods
    def coerced_tests
      self.const_get(:COERCED_TESTS) rescue nil
    end
    def method_added(method)
      if coerced_tests && coerced_tests.include?(method)
        undef_method(method) rescue nil
        STDOUT.puts("Undefined coerced test: #{self.name}##{method}")
      end
    end
  end
end

# Set weather to test unicode string defaults or not. Used from rake task.

if ENV['ENABLE_DEFAULT_UNICODE_TYPES'] != 'false'
  puts "With enabled unicode string types"
  ActiveRecord::ConnectionAdapters::SQLServerAdapter.enable_default_unicode_types = true
end

# Our changes/additions to ActiveRecord test helpers specific for SQL Server.

ActiveRecord::Base.connection.class.class_eval do
  IGNORED_SQL << %r|SELECT SCOPE_IDENTITY| << %r{INFORMATION_SCHEMA\.(TABLES|VIEWS|COLUMNS)}
  IGNORED_SQL << %r|SELECT @@IDENTITY| << %r|SELECT @@ROWCOUNT| << %r|SELECT @@version| << %r|SELECT @@TRANCOUNT|
end

ActiveRecord::ConnectionAdapters::SQLServerAdapter.class_eval do
  def raw_select_with_query_record(sql, name=nil, options={})
    $queries_executed ||= []
    $queries_executed << sql unless IGNORED_SQL.any? { |r| sql =~ r }
    raw_select_without_query_record(sql,name,options)
  end
  alias_method_chain :raw_select, :query_record
end

module ActiveRecord 
  class TestCase < ActiveSupport::TestCase
    class << self
      def connection_mode_odbc? ; ActiveRecord::Base.connection.send(:connection_mode) == :odbc ; end
      def sqlserver_2005? ; ActiveRecord::Base.connection.sqlserver_2005? ; end
      def sqlserver_2008? ; ActiveRecord::Base.connection.sqlserver_2008? ; end
      def ruby_19? ; RUBY_VERSION >= '1.9' ; end
      def quote_values_as_utf8? ; ActiveRecord::Base.connection.quote_value_as_utf8?('') ; end
    end
    def assert_sql(*patterns_to_match)
      $queries_executed = []
      yield
    ensure
      failed_patterns = []
      patterns_to_match.each do |pattern|
        failed_patterns << pattern unless $queries_executed.any?{ |sql| pattern === sql }
      end
      assert failed_patterns.empty?, "Query pattern(s) #{failed_patterns.map(&:inspect).join(', ')} not found in:\n#{$queries_executed.inspect}"
    end
    def connection_mode_odbc? ; self.class.connection_mode_odbc? ; end
    def sqlserver_2005? ; self.class.sqlserver_2005? ; end
    def sqlserver_2008? ; self.class.sqlserver_2008? ; end
    def ruby_19? ; self.class.ruby_19? ; end
    def quote_values_as_utf8? ; self.class.quote_values_as_utf8? ; end
    def with_enable_default_unicode_types?
      ActiveRecord::ConnectionAdapters::SQLServerAdapter.enable_default_unicode_types.is_a?(TrueClass)
    end
    def with_enable_default_unicode_types(setting)
      old_setting = ActiveRecord::ConnectionAdapters::SQLServerAdapter.enable_default_unicode_types
      old_text = ActiveRecord::ConnectionAdapters::SQLServerAdapter.native_text_database_type
      old_string = ActiveRecord::ConnectionAdapters::SQLServerAdapter.native_string_database_type
      ActiveRecord::ConnectionAdapters::SQLServerAdapter.enable_default_unicode_types = setting
      ActiveRecord::ConnectionAdapters::SQLServerAdapter.native_text_database_type = nil
      ActiveRecord::ConnectionAdapters::SQLServerAdapter.native_string_database_type = nil
      yield
    ensure
      ActiveRecord::ConnectionAdapters::SQLServerAdapter.enable_default_unicode_types = old_setting
      ActiveRecord::ConnectionAdapters::SQLServerAdapter.native_text_database_type = old_text
      ActiveRecord::ConnectionAdapters::SQLServerAdapter.native_string_database_type = old_string
    end
  end
end


