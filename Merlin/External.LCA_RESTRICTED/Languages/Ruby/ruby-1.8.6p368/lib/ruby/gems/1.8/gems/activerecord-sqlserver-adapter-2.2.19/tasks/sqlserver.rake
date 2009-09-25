
namespace :sqlserver do
  
  ['sqlserver','sqlserver_odbc'].each do |adapter|
    
    Rake::TestTask.new("test_#{adapter}") do |t|
      t.libs << "test" 
      t.libs << "test/connections/native_#{adapter}"
      t.libs << "../../../rails/activerecord/test/"
      t.test_files = (
        Dir.glob("test/cases/**/*_test_sqlserver.rb").sort + 
        Dir.glob("../../../rails/activerecord/test/**/*_test.rb").sort )
      t.verbose = true
    end
    
    namespace adapter do
      task :test => "test_#{adapter}"
    end

  end
  
  desc 'Test with unicode types enabled.'
  task :test_unicode_types do
    ENV['ENABLE_DEFAULT_UNICODE_TYPES'] = 'true'
    test = Rake::Task['test_sqlserver_odbc']
    test.invoke
  end
  
  
end

