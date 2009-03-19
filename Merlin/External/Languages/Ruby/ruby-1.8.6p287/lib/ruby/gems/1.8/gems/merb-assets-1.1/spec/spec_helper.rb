$:.unshift File.join(File.dirname(__FILE__), '..', 'lib')
require "rubygems"
require "merb-core"
require "merb-assets"
# require File.dirname(__FILE__) / "controllers" / "action-args"
require "spec"

Merb.start :environment => 'test'

Merb::Plugins.config[:asset_helpers][:max_hosts] = 4
Merb::Plugins.config[:asset_helpers][:asset_domain] = "assets%d"
Merb::Plugins.config[:asset_helpers][:domain] = "my-awesome-domain.com"


Spec::Runner.configure do |config|
  config.include Merb::Test::RequestHelper  
end