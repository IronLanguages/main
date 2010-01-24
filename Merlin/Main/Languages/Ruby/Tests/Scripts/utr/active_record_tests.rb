class UnitTestSetup
  def initialize
    @name = "ActiveRecord"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'test-unit', "= 2.0.5"
    gem 'activerecord', "= 2.3.5"
    gem 'activesupport', "= 2.3.5"
    gem 'activerecord-sqlserver-adapter', "= 2.3.5"    require 'ironruby_sqlserver' if UnitTestRunner.ironruby?
  end

  def ensure_test_databases
    if false
      warn "Could not create test databases"
    else
      # TODO - http://support.microsoft.com/kb/307283
      return true
    end

  def gather_files
    sqlserver_adapter_root_dir = File.expand_path '../External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/gems/activerecord-sqlserver-adapter-2.2.19', ENV['MERLIN_ROOT']
    activerecord_tests_dir = File.expand_path '../External.LCA_RESTRICTED/Languages/IronRuby/tests/RailsTests-2.3.5/activerecord/test', ENV['MERLIN_ROOT']
    $LOAD_PATH << sqlserver_adapter_root_dir + '/test'
    $LOAD_PATH << activerecord_tests_dir

    if UnitTestRunner.ironruby?
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

    ensure_test_databases

    # Load helper files
    require "#{sqlserver_adapter_root_dir}/test/cases/aaaa_create_tables_test_sqlserver"
    # Overwrite ACTIVERECORD_TEST_ROOT since aaaa_create_tables_test_sqlserver assumes a specific folder layout
    ACTIVERECORD_TEST_ROOT = activerecord_tests_dir

    @all_test_files = Dir.glob("#{sqlserver_adapter_root_dir}/test/cases/*_test_sqlserver.rb").sort
    # Rails ActiveRecord tests
    @all_test_files += Dir.glob("#{activerecord_tests_dir}/**/*_test.rb")
  end
  
  def require_tests
    # Note that the tests are registered using Kernel#at_exit, and will run during shutdown
    # The "require" statement just registers the tests for being run later...
    @all_test_files.each { |f| 
      begin
        require f
      rescue NameError => e # TODO - This should not be needed ideally...
        abort if not /eager_association_test_sqlserver/ =~ f
        warn "Error while loading #{f}: #{e}"
      end
    }
  end
  
  def sanity
    # Do some sanity checks
    sanity_size(85)
  end

  def disable_critical_failures
    # If this test executes, all subsequent tests start failing during setup with an exception
    # saying "The server failed to resume the transaction", presumably because
    # teardown did not happen properly for this test, and the active transaction was not aborted.
    disable DynamicFinderMatchTest, :test_exists
  end  
end
