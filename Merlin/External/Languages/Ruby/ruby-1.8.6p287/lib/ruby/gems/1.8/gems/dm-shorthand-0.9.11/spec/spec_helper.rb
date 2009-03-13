require 'pathname'
require 'rubygems'

gem 'rspec', '~>1.1.11'
require 'spec'

require Pathname(__FILE__).dirname.parent.expand_path + 'lib/dm-shorthand'
