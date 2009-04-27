require 'rubygems'
require 'rack'

$app = Rack::Builder.new do
  # config.ru
  require 'myapp'
  run MyApp.new
end

if __FILE__ == $0
  load 'run.rb'
end