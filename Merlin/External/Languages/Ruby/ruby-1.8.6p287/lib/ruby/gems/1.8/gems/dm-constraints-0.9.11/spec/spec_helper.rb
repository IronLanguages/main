require 'pathname'
require 'rubygems'

gem 'rspec', '~>1.1.11'
require 'spec'

gem 'dm-core', '~>0.9.11'
require 'dm-core'

ADAPTERS = []
def load_driver(name, default_uri)
  begin
    DataMapper.setup(name, ENV["#{name.to_s.upcase}_SPEC_URI"] || default_uri)
    DataMapper::Repository.adapters[:default] = DataMapper::Repository.adapters[name]
    ADAPTERS << name
  rescue LoadError => e
    warn "Could not load do_#{name}: #{e}"
    false
  end
end

load_driver(:postgres, 'postgres://postgres@localhost/dm_core_test')
load_driver(:mysql,    'mysql://localhost/dm_core_test')

require Pathname(__FILE__).dirname.expand_path.parent + 'lib/dm-constraints'
