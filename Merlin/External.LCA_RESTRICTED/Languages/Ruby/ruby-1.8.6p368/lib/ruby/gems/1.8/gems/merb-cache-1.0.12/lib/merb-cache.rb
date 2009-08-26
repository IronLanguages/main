# make sure we're running inside Merb
if defined?(Merb::Plugins)
  require "merb-cache" / "cache"
  require "merb-cache" / "core_ext" / "enumerable"
  require "merb-cache" / "core_ext" / "hash"
  require "merb-cache" / "merb_ext" / "controller"
  require "merb-cache" / "cache_request"

  class Merb::Controller
    include Merb::Cache::CacheMixin
  end
end
