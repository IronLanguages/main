require 'rubygems'
require 'pathname'

gem 'dm-core', '~>0.9.11'
require 'dm-core'

require Pathname(__FILE__).dirname.expand_path / 'dm-is-versioned' / 'is' / 'versioned.rb'

# Include the plugin in Resource
module DataMapper
  module Model
    include DataMapper::Is::Versioned
  end # module Model
end # module DataMapper
