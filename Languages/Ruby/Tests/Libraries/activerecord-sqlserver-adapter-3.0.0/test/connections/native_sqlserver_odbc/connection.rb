print "Using SQLServer via ODBC\n"
require_dependency 'models/course'
require 'logger'

ActiveRecord::Base.logger = Logger.new(File.expand_path(File.join(SQLSERVER_TEST_ROOT,'debug.log')))
ActiveRecord::Base.logger.level = 0

ActiveRecord::Base.configurations = {
  'arunit' => {
    :adapter  => 'sqlserver',
    :mode     => 'ODBC',
    :host     => 'localhost',
    :username => 'rails',
    :dsn      => ENV['ACTIVERECORD_UNITTEST_DSN'] || 'activerecord_unittest',
    :database => 'activerecord_unittest'
  },
  'arunit2' => {
    :adapter  => 'sqlserver',
    :mode     => 'ODBC',
    :host     => 'localhost',
    :username => 'rails',
    :dsn      => ENV['ACTIVERECORD_UNITTEST2_DSN'] || 'activerecord_unittest2',
    :database => 'activerecord_unittest2'
  }
}

ActiveRecord::Base.establish_connection 'arunit'
Course.establish_connection 'arunit2'
