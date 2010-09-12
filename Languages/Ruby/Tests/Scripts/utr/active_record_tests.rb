class UnitTestSetup
  def initialize
    @name = "ActiveRecord"
    super
  end
  
  def require_files
    require 'rubygems'
    gem 'test-unit', "= #{TestUnitVersion}"
    gem 'activerecord', "= #{RailsVersion}"
    gem 'activesupport', "= #{RailsVersion}"
    gem 'activerecord-sqlserver-adapter', "= #{SqlServerAdapterVersion}"
  end

  def ensure_database_exists(name)
    conn = ActiveRecord::Base.sqlserver_connection({
      :mode => 'ADONET',
      :adapter => 'sqlserver',
      :host => ENV['COMPUTERNAME'] + "\\SQLEXPRESS",
      :integrated_security => 'true',
      :database => ''
    })
    begin
      conn.execute "CREATE DATABASE #{name}"
      return
    rescue => e
      if e.message =~ /already exists/
        return
      end
    end
    
    warn "Could not create test databases #{name}"
    exit 0
  end

  def gather_files
    gather_rails_files
    
    # remove all adapter tests:
    @all_test_files.delete_if { |path| path.include?("cases/adapters") }
     
    if UnitTestRunner.ironruby?
      $LOAD_PATH << File.expand_path('Languages/Ruby/Tests/Scripts/adonet_sqlserver', ENV['DLR_ROOT'])
    else
      $LOAD_PATH << File.expand_path('Languages/Ruby/Tests/Scripts/native_sqlserver', ENV['DLR_ROOT'])
    end
    
    require 'active_record'
    require 'active_record/test_case'
    require 'active_record/fixtures'
    require 'active_record/connection_adapters/sqlserver_adapter'

    ensure_database_exists "activerecord_unittest"
    ensure_database_exists "activerecord_unittest2"
    
    sqlserver_adapter_root_dir = File.expand_path("External.LCA_RESTRICTED/Languages/IronRuby/tests/activerecord-sqlserver-adapter-#{SqlServerAdapterVersion}", ENV['DLR_ROOT'])
    $LOAD_PATH << "#{sqlserver_adapter_root_dir}/test"
    ENV['RAILS_SOURCE'] = RAILS_TEST_DIR 
    
    @all_test_files += Dir.glob("#{sqlserver_adapter_root_dir}/test/cases/*_test_sqlserver.rb").sort
  end

  def sanity
    # Do some sanity checks
    sanity_size(85)
  end
  
  def disable_tests
    disable_by_name %w{
      test_add_limit_offset_should_sanitize_sql_injection_for_limit_with_comas(AdapterTest)
      test_add_limit_offset_should_sanitize_sql_injection_for_limit_without_comas(AdapterTest)
      
    }
  end
end
