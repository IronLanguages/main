require 'pathname'
require 'rubygems'

gem 'rspec', '~>1.1.11'
require 'spec'

SPEC_ROOT = Pathname(__FILE__).dirname.expand_path
require SPEC_ROOT.parent + 'lib/dm-core'

# Load the various helpers for the spec suite
Dir[(DataMapper.root / 'spec' / 'lib' / '*.rb').to_s].each do |file|
  require file
end

# setup mock adapters
DataMapper.setup(:default2, "sqlite3::memory:")

[ :mock, :legacy, :west_coast, :east_coast ].each do |repository_name|
  DataMapper.setup(repository_name, "mock://localhost/#{repository_name}")
end

# These environment variables will override the default connection string:
#   MYSQL_SPEC_URI
#   POSTGRES_SPEC_URI
#   SQLITE3_SPEC_URI
#
# For example, in the bash shell, you might use:
#   export MYSQL_SPEC_URI="mysql://localhost/dm_core_test?socket=/opt/local/var/run/mysql5/mysqld.sock"
#
def setup_adapter(name, default_uri)
  begin
    adapter = DataMapper.setup(name, ENV["#{name.to_s.upcase}_SPEC_URI"] || default_uri)

    if name.to_s == ENV['ADAPTER']
      Object.const_set('ADAPTER', ENV['ADAPTER'].to_sym)
      DataMapper::Repository.adapters[:default] = adapter
    end

    true
  rescue Exception => e
    if name.to_s == ENV['ADAPTER']
      Object.const_set('ADAPTER', nil)
      warn "Could not load #{name} adapter: #{e}"
    end
    false
  end
end

ENV['ADAPTER'] ||= 'sqlite3'

HAS_SQLITE3  = setup_adapter(:sqlite3,  'sqlite3::memory:')
HAS_MYSQL    = setup_adapter(:mysql,    'mysql://localhost/dm_core_test')
HAS_POSTGRES = setup_adapter(:postgres, 'postgres://postgres@localhost/dm_core_test')

DataMapper::Logger.new(nil, :debug)

# ----------------------------------------------------------------------
# --- Do not declare new models unless absolutely necessary. Instead ---
# --- pick a metaphor and use those models. If you do need new       ---
# --- models, define them according to the metaphor being used.      ---
# ----------------------------------------------------------------------

Spec::Runner.configure do |config|
  config.before(:each) do
    # load_models_for_metaphor :vehicles
  end
end

# ----------------------------------------------------------------------
# --- All these models are going to be removed. Don't use them!!!    ---
# ----------------------------------------------------------------------

class Article
  include DataMapper::Resource

  property :id,         Serial
  property :blog_id,    Integer
  property :created_at, DateTime
  property :author,     String
  property :title,      String
end

class Comment
  include DataMapper::Resource

  property :id,         Serial # blah
end

class NormalClass
  # should not include DataMapper::Resource
end
