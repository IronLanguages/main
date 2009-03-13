require "rubygems"

# Add the local gems dir if found within the app root; any dependencies loaded
# hereafter will try to load from the local gems before loading system gems.
if (local_gem_dir = File.join(File.dirname(__FILE__), '..', 'gems')) && $BUNDLE.nil?
  $BUNDLE = true; Gem.clear_paths; Gem.path.unshift(local_gem_dir)
end

require "merb-core"

# TODO: Boot Merb, via the Test Rack adapter
Merb.start :environment => (ENV['MERB_ENV'] || 'test'),
           :merb_root  => File.join(File.dirname(__FILE__), ".." )


class Test::Unit::TestCase
  include Merb::Test::RequestHelper
  # Add more helper methods to be used by all tests here...
end