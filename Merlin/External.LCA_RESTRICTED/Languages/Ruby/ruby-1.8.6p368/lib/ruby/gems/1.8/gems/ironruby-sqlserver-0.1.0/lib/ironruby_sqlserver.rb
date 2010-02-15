class IronrubySqlserver
  VERSION = '0.1.0'
end

require 'rubygems'
gem 'activerecord'
gem 'ironruby-dbi'
gem 'activerecord-sqlserver-adapter'

require 'dbi'
require 'active_record'
require 'active_record/connection_adapters/sqlserver_adapter'
require 'activerecord-sqlserver-adapter/adonet_patch'
