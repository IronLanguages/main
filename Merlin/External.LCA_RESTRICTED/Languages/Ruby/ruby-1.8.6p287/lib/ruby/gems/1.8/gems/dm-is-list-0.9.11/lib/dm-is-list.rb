require 'pathname'
require 'rubygems'

gem 'dm-core', '~>0.9.11'
require 'dm-core'

#gem 'dm-adjust', '~>0.9.11'
require 'dm-adjust'

require Pathname(__FILE__).dirname.expand_path / 'dm-is-list' / 'is' / 'list'

DataMapper::Model.append_extensions DataMapper::Is::List
