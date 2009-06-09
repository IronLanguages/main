# Needed to import datamapper and other gems
require 'rubygems'
require 'pathname'

# Add all external dependencies for the plugin here
gem 'dm-core', '~>0.9.11'
require 'dm-core'

dir = Pathname(__FILE__).dirname.expand_path / 'dm-querizer'

require dir / 'querizer'
require dir / 'model'
require dir / 'collection'
