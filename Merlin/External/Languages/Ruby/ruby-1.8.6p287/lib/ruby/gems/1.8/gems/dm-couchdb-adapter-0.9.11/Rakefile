require 'pathname'
require 'rubygems'

ROOT    = Pathname(__FILE__).dirname.expand_path
JRUBY   = RUBY_PLATFORM =~ /java/
WINDOWS = Gem.win_platform?
SUDO    = (WINDOWS || JRUBY) ? '' : ('sudo' unless ENV['SUDOLESS'])

require ROOT + 'lib/couchdb_adapter/version'

AUTHOR = 'Bernerd Schaefer'
EMAIL  = 'bernerd [a] wieck [d] com'
GEM_NAME = 'dm-couchdb-adapter'
GEM_VERSION = DataMapper::CouchDBAdapter::VERSION
GEM_DEPENDENCIES = [['dm-core', "~>#{GEM_VERSION}"], ['mime-types', '~>1.15']]
GEM_CLEAN = %w[ log pkg coverage ]
GEM_EXTRAS = { :has_rdoc => true, :extra_rdoc_files => %w[ README.txt LICENSE TODO History.txt ] }

PROJECT_NAME = 'datamapper'
PROJECT_URL  = "http://github.com/sam/dm-more/tree/master/adapters/#{GEM_NAME}"
PROJECT_DESCRIPTION = PROJECT_SUMMARY = 'CouchDB Adapter for DataMapper'

[ ROOT, ROOT.parent.parent ].each do |dir|
  Pathname.glob(dir.join('tasks/**/*.rb').to_s).each { |f| require f }
end
