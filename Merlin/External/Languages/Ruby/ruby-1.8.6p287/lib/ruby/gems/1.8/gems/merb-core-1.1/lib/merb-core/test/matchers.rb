module Merb::Test::Rspec; end

require "merb-core/test/matchers/controller_matchers"
require "merb-core/test/matchers/route_matchers"
require "merb-core/test/matchers/request_matchers"

Merb::Test::ControllerHelper.send(:include, Merb::Test::Rspec::ControllerMatchers)
Merb::Test::RouteHelper.send(:include, Merb::Test::Rspec::RouteMatchers)

if defined?(::Webrat)
  module Merb::Test::ViewHelper
    include ::Webrat::Matchers
    include ::Webrat::HaveTagMatcher
  end
end