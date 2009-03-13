require 'pathname'
require 'rubygems'

ROOT    = Pathname(__FILE__).dirname.expand_path
JRUBY   = RUBY_PLATFORM =~ /java/
WINDOWS = Gem.win_platform?
SUDO    = (WINDOWS || JRUBY) ? '' : ('sudo' unless ENV['SUDOLESS'])

require ROOT + 'lib/dm-is-list/is/version'

AUTHOR = 'Sindre Aarsaether'
EMAIL  = 'sindre [a] identu [d] no'
GEM_NAME = 'dm-is-list'
GEM_VERSION = DataMapper::Is::List::VERSION
GEM_DEPENDENCIES = [['dm-core', "~>#{GEM_VERSION}"], ['dm-adjust', "~>#{GEM_VERSION}"]]
GEM_CLEAN = %w[ log pkg coverage ]
GEM_EXTRAS = { :has_rdoc => true, :extra_rdoc_files => %w[ README.txt LICENSE TODO History.txt ] }

PROJECT_NAME = 'datamapper'
PROJECT_URL  = "http://github.com/sam/dm-more/tree/master/#{GEM_NAME}"
PROJECT_DESCRIPTION = PROJECT_SUMMARY = 'DataMapper plugin for creating and organizing lists'

[ ROOT, ROOT.parent ].each do |dir|
  Pathname.glob(dir.join('tasks/**/*.rb').to_s).each { |f| require f }
end
