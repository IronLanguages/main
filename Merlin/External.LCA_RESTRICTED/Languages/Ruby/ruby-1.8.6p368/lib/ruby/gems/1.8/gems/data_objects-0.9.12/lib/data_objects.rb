require 'rubygems'

gem 'extlib', '~>0.9.11'
require 'extlib'

require File.expand_path(File.join(File.dirname(__FILE__), 'data_objects', 'version'))
require File.expand_path(File.join(File.dirname(__FILE__), 'data_objects', 'logger'))
require File.expand_path(File.join(File.dirname(__FILE__), 'data_objects', 'connection'))
require File.expand_path(File.join(File.dirname(__FILE__), 'data_objects', 'uri'))
require File.expand_path(File.join(File.dirname(__FILE__), 'data_objects', 'transaction'))
require File.expand_path(File.join(File.dirname(__FILE__), 'data_objects', 'command'))
require File.expand_path(File.join(File.dirname(__FILE__), 'data_objects', 'result'))
require File.expand_path(File.join(File.dirname(__FILE__), 'data_objects', 'reader'))
require File.expand_path(File.join(File.dirname(__FILE__), 'data_objects', 'quoting'))


module DataObjects
  class LengthMismatchError < StandardError; end
end
