require 'rubygems'
gem 'activerecord', '=2.3.5'

ENV['RAILS_ENV'] = 'production'
require "config/environment"

use Rails::Rack::LogTailer
use Rails::Rack::Static
run ActionController::Dispatcher.new
