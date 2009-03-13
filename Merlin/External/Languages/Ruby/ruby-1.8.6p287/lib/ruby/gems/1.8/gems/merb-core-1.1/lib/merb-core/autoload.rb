require 'merb-core/core_ext'
require "merb-core/controller/exceptions"
require "merb-core/controller/mixins/responder"
require "merb-core/controller/mixins/render"
require "merb-core/controller/mixins/authentication"
require "merb-core/controller/mixins/conditional_get"
require "merb-core/controller/mixins/controller"
require "merb-core/controller/abstract_controller"
require "merb-core/controller/template"
require "merb-core/controller/merb_controller"
require "merb-core/bootloader"
require "merb-core/config"
require "merb-core/constants"
require "merb-core/dispatch/dispatcher"
require "merb-core/plugins"
require "merb-core/rack"
require "merb-core/dispatch/request"
require "merb-core/dispatch/request_parsers.rb"
require "merb-core/dispatch/router"
require "merb-core/dispatch/worker"

module Merb
  autoload :Test, "merb-core/test"
end

# Require this rather than autoloading it so we can be sure the default template
# gets registered

module Merb
  module InlineTemplates; end
end
