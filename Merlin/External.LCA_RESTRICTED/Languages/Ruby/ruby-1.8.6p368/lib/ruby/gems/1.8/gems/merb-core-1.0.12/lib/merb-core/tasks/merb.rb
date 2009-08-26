local_gem_path = Gem.path if $BUNDLE
require 'merb-core/tasks/merb_rake_helper'
Gem.path.replace(local_gem_path) if local_gem_path
Dir[File.dirname(__FILE__) / '*.rake'].each { |ext| load ext }
