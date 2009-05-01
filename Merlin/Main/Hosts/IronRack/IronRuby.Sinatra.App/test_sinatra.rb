gets
require 'rubygems'
require 'sinatra'

get '/' do
  @msg1 = 'Hello'
  @msg2 = 'World'
  erb '<%= @msg1 %>, <b><%= @msg2 %></b>'
end
