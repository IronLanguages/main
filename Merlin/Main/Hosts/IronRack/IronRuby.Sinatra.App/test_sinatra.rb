require 'rubygems'
require 'sinatra'

get '/' do
  "Hello, <b>World</b> at #{Time.now}"
end
