require 'pathname'
require 'rubygems'

gem 'rspec', '~>1.1.11'
require 'spec'

require Pathname(__FILE__).dirname.parent.expand_path + 'lib/dm-migrations'
require Pathname(__FILE__).dirname.parent.expand_path + 'lib/migration_runner'

ADAPTERS = []
def load_driver(name, default_uri)
  begin
    DataMapper.setup(name, default_uri)
    DataMapper::Repository.adapters[:default] =  DataMapper::Repository.adapters[name]
    ADAPTERS << name
    true
  rescue LoadError => e
    warn "Could not load do_#{name}: #{e}"
    false
  end
end

#ENV['ADAPTER'] ||= 'sqlite3'

load_driver(:sqlite3,  'sqlite3::memory:')
load_driver(:mysql,    'mysql://localhost/dm_core_test')
load_driver(:postgres, 'postgres://postgres@localhost/dm_core_test')
