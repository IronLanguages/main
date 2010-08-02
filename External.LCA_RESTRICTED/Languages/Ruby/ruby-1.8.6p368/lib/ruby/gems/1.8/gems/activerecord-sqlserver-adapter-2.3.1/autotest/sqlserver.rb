require 'autotest'
require 'active_support'

class Autotest::Sqlserver < Autotest

  def initialize
    super
    
    odbc_mode = true
    
    clear_mappings
    
    self.libs = [
      "lib",
      "test",
      "test/connections/native_sqlserver#{odbc_mode ? '_odbc' : ''}",
      "../../../rails/activerecord/test/"
    ].join(File::PATH_SEPARATOR)
    
    @test_sqlserver_file_match = %r%^test/cases/.*_test_sqlserver\.rb$%
    
    add_exception %r%^\./(?:autotest)%
    add_exception %r%^\./(.*LICENSE|debug.log|README.*|CHANGELOG.*)$%i
    
    # Any *_test_sqlserver file saved runs that file
    self.add_mapping @test_sqlserver_file_match do |filename, matchs|
      filename
    end
    
    # If any the adapter changes
    # the test directory, ofcourse having _test.rb at the end, will run that test. 
    self.add_mapping(%r%^lib/(.*)\.rb$%) do |filename, matchs|
      files_matching @test_sqlserver_file_match
    end
    
    # If any core file like the test helper, connections, fixtures, migratinos,
    # and other support files, then run all matching *_test_sqlserver files.
    add_mapping %r%^test/(cases/(sqlserver_helper)\.rb|connections|fixtures|migrations|schema/.*)% do
      files_matching @test_sqlserver_file_match
    end
    
  end
  
  # Have to use a custom reorder method since the normal :alpha for Autotest would put the 
  # files with ../ in the path before others.
  def reorder(files_to_test)
    ar_tests, sqlsvr_tests = files_to_test.partition { |k,v| k.starts_with?('../../../') }
    ar_tests.sort! { |a,b| a[0] <=> b[0] }
    sqlsvr_tests.sort! { |a,b| a[0] <=> b[0] }
    sqlsvr_tests + ar_tests
  end
  
end

