require 'pathname'
require 'rubygems'

__DIR__ = File.expand_path(File.dirname(__FILE__))
$LOAD_PATH.unshift(__DIR__) unless $LOAD_PATH.include?(__DIR__)

# for Pathname /
require File.expand_path(File.join(__DIR__, 'extlib', 'pathname'))

dir = Pathname(__FILE__).dirname.expand_path / 'extlib'

require dir / 'class.rb'
require dir / 'object'
require dir / 'object_space'

require dir / 'array'
require dir / 'string'
require dir / 'symbol'
require dir / 'hash'
require dir / 'mash'
require dir / 'virtual_file'
require dir / 'logger'
require dir / 'time'
require dir / 'datetime'

require dir / 'assertions'
require dir / 'blank'
require dir / 'boolean'
require dir / 'inflection'
require dir / 'lazy_array'
require dir / 'module'
require dir / 'nil'
require dir / 'numeric'
require dir / 'blank'
require dir / 'simple_set'
require dir / 'struct'
require dir / 'symbol'

Extlib.autoload('Hook', (dir / 'hook').to_s)
Extlib.autoload('Pooling', (dir / 'pooling').to_s)

module Extlib

  def self.exiting= bool
    if bool && Extlib.const_defined?('Pooling')
      if Extlib::Pooling.scavenger?
        Extlib::Pooling.scavenger.wakeup
      end
    end
    @exiting = true
  end

  def self.exiting
    @exiting
  end

end
