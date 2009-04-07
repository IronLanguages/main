$:.unshift File.join(File.dirname(__FILE__), '..', 'lib')
require "rubygems"
require "merb-core"
require "merb-mailer"

Merb::Config.use do |c|
  c[:session_store] = :memory
end


Merb.start :environment => 'test'
