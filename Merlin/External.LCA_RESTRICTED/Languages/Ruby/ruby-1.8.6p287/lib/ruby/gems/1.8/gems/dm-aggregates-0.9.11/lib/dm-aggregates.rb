require 'rubygems'

dir = Pathname(__FILE__).dirname.expand_path + 'dm-aggregates'

require dir + 'version'
gem 'dm-core', '~>0.9.11'
require 'dm-core'


require dir + 'aggregate_functions'
require dir + 'model'
require dir + 'repository'
require dir + 'collection'
require dir + 'adapters' + 'data_objects_adapter'
require dir + 'support' + 'symbol'
