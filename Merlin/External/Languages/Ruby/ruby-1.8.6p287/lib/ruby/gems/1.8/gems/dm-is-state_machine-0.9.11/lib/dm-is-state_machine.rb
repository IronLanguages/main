# Needed to import datamapper and other gems
require 'rubygems'
require 'pathname'

# Add all external dependencies for the plugin here
gem 'dm-core', '~>0.9.11'
require 'dm-core'

# Require plugin-files
require Pathname(__FILE__).dirname.expand_path / 'dm-is-state_machine' / 'is' / 'state_machine'
require Pathname(__FILE__).dirname.expand_path / 'dm-is-state_machine' / 'is' / 'data' / 'event'
require Pathname(__FILE__).dirname.expand_path / 'dm-is-state_machine' / 'is' / 'data' / 'machine'
require Pathname(__FILE__).dirname.expand_path / 'dm-is-state_machine' / 'is' / 'data' / 'state'
require Pathname(__FILE__).dirname.expand_path / 'dm-is-state_machine' / 'is' / 'dsl' / 'event_dsl'
require Pathname(__FILE__).dirname.expand_path / 'dm-is-state_machine' / 'is' / 'dsl' / 'state_dsl'

# Include the plugin in Resource
module DataMapper
  module Model
    include DataMapper::Is::StateMachine
  end # module Model
end # module DataMapper

# An alternative way to do the same thing as above:
# DataMapper::Model.append_extensions DataMapper::Is::StateMachine
