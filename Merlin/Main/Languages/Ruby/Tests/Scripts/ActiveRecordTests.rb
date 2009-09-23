require "rubygems"
gem "activerecord", "= 2.3.3"
gem "activesupport", "= 2.3.3"
gem "activerecord-sqlserver-adapter", "= 2.2.19"

sqlserver_adapter_root_dir = File.expand_path '../External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.2.19', ENV['MERLIN_ROOT']
activerecord_tests_dir = File.expand_path '../External.LCA_RESTRICTED/Languages/IronRuby/RailsTests-2.3.3/activerecord/test', ENV['MERLIN_ROOT']
$LOAD_PATH << sqlserver_adapter_root_dir + '/test'
$LOAD_PATH << activerecord_tests_dir

if defined? RUBY_ENGINE
  abort if RUBY_ENGINE != "ironruby"
  # Use unsift since activerecord-sqlserver-adapter installs 0.4.1 whereas ironruby-dbi currently depends on 0.4.0
  $LOAD_PATH.unshift "c:/github/ironruby-dbi/src/lib"
  $LOAD_PATH << File.expand_path('Languages/Ruby/Tests/Scripts/adonet_sqlserver', ENV['MERLIN_ROOT'])
else
  DBI_0_2_2 = 'c:/bugs/rails/dbi-0.2.2'
  $LOAD_PATH << "#{DBI_0_2_2}/lib"
  require "#{DBI_0_2_2}/lib/dbi.rb"
  require "#{DBI_0_2_2}/lib/dbd/ado.rb"
  $LOAD_PATH << File.expand_path('Languages/Ruby/Tests/Scripts/native_sqlserver', ENV['MERLIN_ROOT'])
end

require 'active_record'
require 'active_record/test_case'
require 'active_record/fixtures'
require 'activerecord-sqlserver-adapter'

# Monkey patch SQLServerAdapter. This should be moved into SQLServerAdapter itself in activerecord-sqlserver-adapter\lib\active_record\connection_adapters\sqlserver_adapter.rb
module ActiveRecord
  class Base
    def self.sqlserver_connection(config) #:nodoc:
      config.symbolize_keys!
      mode        = config[:mode] ? config[:mode].to_s.upcase : 'ADO'
      username    = config[:username] ? config[:username].to_s : 'sa'
      password    = config[:password] ? config[:password].to_s : ''
      database    = config[:database]
      host        = config[:host] ? config[:host].to_s : 'localhost'
      integrated_security = config[:integrated_security]

      if mode == "ODBC"
        raise ArgumentError, "Missing DSN. Argument ':dsn' must be set in order for this adapter to work." unless config.has_key?(:dsn)
        dsn       = config[:dsn]
        driver_url = "DBI:ODBC:#{dsn}"
        connection_options = [driver_url, username, password]
      elsif mode == "ADO"
        raise ArgumentError, "Missing Database. Argument ':database' must be set in order for this adapter to work." unless config.has_key?(:database)
        if integrated_security
          connection_options = ["DBI:ADO:Provider=SQLOLEDB;Data Source=#{host};Initial Catalog=#{database};Integrated Security=SSPI"]
        else
          driver_url = "DBI:ADO:Provider=SQLOLEDB;Data Source=#{host};Initial Catalog=#{database};User ID=#{username};Password=#{password};"
          connection_options = [driver_url, username, password]
        end
      elsif mode == "ADONET"
        raise ArgumentError, "Missing Database. Argument ':database' must be set in order for this adapter to work." unless config.has_key?(:database)
        if integrated_security
          connection_options = ["DBI:MSSQL:server=#{host};initial catalog=#{database};integrated security=true"]
        else
          connection_options = ["DBI:MSSQL:server=#{host};initial catalog=#{database};user id=#{username};password=#{password}"]
        end
      else
        raise "Unknown mode #{mode}"
      end
      ConnectionAdapters::SQLServerAdapter.new(logger, connection_options)
    end
  end
end

def ensure_test_databases
  if false
    warn "Could not create test databases"
  else
    # TODO
    return true
end

ensure_test_databases

# Load helper files
require "#{sqlserver_adapter_root_dir}/test/cases/aaaa_create_tables_test_sqlserver"
# Overwrite ACTIVERECORD_TEST_ROOT since aaaa_create_tables_test_sqlserver assumes a specific folder layout
ACTIVERECORD_TEST_ROOT = activerecord_tests_dir

test_files = Dir.glob("#{sqlserver_adapter_root_dir}/test/cases/*_test_sqlserver.rb").sort
# Rails ActiveRecord tests
test_files += Dir.glob("#{activerecord_tests_dir}/**/*_test.rb")

# Do some sanity checks
abort("Did not find enough tests files...") unless test_files.size > 85

# Note that the tests are registered using Kernel#at_exit, and will run during shutdown
# The "require" statement just registers the tests for being run later...
test_files.each { |f| 
  begin
    require f
  rescue NameError => e # TODO - This should not be needed ideally...
    abort if not /eager_association_test_sqlserver/ =~ f
    warn "Error while loading #{f}: #{e}"
  end
}

# Disable failing tests by monkey-patching the test method to be a nop

# The first set of tags is non-deterministic test failures

class DynamicFinderMatchTest
  # If this test executes, all subsequent tests start failing during setup with an exception
  # saying "The server failed to resume the transaction", presumably because
  # teardown did not happen properly for this test, and the active transaction was not aborted.
  def test_exists() end
end

# The following list of test tags is generated by doing:
#   require "generate_test-unit_tags"
