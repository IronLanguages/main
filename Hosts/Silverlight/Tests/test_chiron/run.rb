require 'helper'
load_constants

Object.send :remove_const, :'ARGV'
ARGV = Dir["#{File.dirname(__FILE__)}/spec_*.rb"]

require 'rubygems'
gem 'rspec'

load Gem.bin_path('rspec', 'spec')