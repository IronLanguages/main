require 'pathname'
require 'rubygems'

gem 'dm-core', '~>0.9.11'
require 'dm-core'

require Pathname(__FILE__).dirname.expand_path / 'dm-is-remixable' / 'is' / 'remixable'

module DataMapper
  module Model
    include DataMapper::Is::Remixable
  end # module Model
end # module DataMapper
