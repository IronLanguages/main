require 'rubygems'
require 'sinatra'

require 'test_sinatra'

set :run, false
set :environment, :production

run Sinatra::Application
