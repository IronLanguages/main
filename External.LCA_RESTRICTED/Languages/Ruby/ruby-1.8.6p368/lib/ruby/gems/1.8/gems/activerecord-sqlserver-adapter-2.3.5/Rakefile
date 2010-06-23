require 'rake'
require 'rake/testtask'
require 'rake/rdoctask'


namespace :sqlserver do
  
  namespace :test do
    
    ['odbc','adonet'].each do |mode|

      Rake::TestTask.new(mode) do |t|
        t.libs << "test"
        t.libs << "test/connections/native_sqlserver#{mode == 'adonet' ? '' : "_#{mode}"}"
        t.libs << "../../../rails/activerecord/test/"
        t.test_files = (
          Dir.glob("test/cases/**/*_test_sqlserver.rb").sort + 
          Dir.glob("../../../rails/activerecord/test/**/*_test.rb").sort )
        t.verbose = true
      end
      
    end

    desc 'Test with unicode types enabled, uses ODBC mode.'
    task :unicode_types do
      ENV['ENABLE_DEFAULT_UNICODE_TYPES'] = 'true'
      test = Rake::Task['sqlserver:test:odbc']
      test.invoke
    end
    
  end
  
end


desc 'Default runs tests for the adapters ODBC mode.'
task :test do
  test = Rake::Task['sqlserver:test:odbc']
  test.invoke
end

