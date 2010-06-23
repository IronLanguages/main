#Object.send(:remove_const, :'ARGV')
#ARGV = Dir["#{File.dirname(__FILE__)}/spec_*.rb"]

require 'test/ispec'
require 'rubygems'
gem 'test-spec'

load Gem.bin_path('test-spec', 'specrb')