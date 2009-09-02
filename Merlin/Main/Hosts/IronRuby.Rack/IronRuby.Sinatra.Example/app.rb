require 'rubygems'
require 'sinatra'

get '/' do
  @msg1 = 'Hello'
  @msg2 = 'World'
  erb 'IronRuby running Sinatra says "<%= @msg1 %>, <b><%= @msg2 %></b>" at <%= Time.now %>'
end

get '/foo' do
  "hi"
end