require 'rubygems'
require 'sinatra'

get '/' do
  @msg = 'Hello, World'
  erb '<b><%= @msg %></b>'
end