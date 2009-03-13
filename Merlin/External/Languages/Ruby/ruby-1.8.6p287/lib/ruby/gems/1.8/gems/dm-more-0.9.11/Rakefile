require 'pathname'
require 'spec/rake/spectask'
require 'rake/rdoctask'
require 'fileutils'
require 'lib/dm-more/version'
include FileUtils

## ORDER IS IMPORTANT
# gems may depend on other member gems of dm-more
GEM_PATHS = %w[
  dm-adjust
  dm-serializer
  dm-validations
  dm-types
  adapters/dm-couchdb-adapter
  adapters/dm-ferret-adapter
  adapters/dm-rest-adapter
  dm-aggregates
  dm-ar-finders
  dm-cli
  dm-constraints
  dm-is-list
  dm-is-nested_set
  dm-is-remixable
  dm-is-searchable
  dm-is-state_machine
  dm-is-tree
  dm-is-versioned
  dm-is-viewable
  dm-migrations
  dm-observer
  dm-querizer
  dm-shorthand
  dm-sweatshop
  dm-tags
  dm-timestamps
].freeze

gems = GEM_PATHS.map { |p| File.basename(p) }

ROOT    = Pathname(__FILE__).dirname.expand_path
JRUBY   = RUBY_PLATFORM =~ /java/
WINDOWS = Gem.win_platform?
SUDO    = (WINDOWS || JRUBY) ? '' : ('sudo' unless ENV['SUDOLESS'])

AUTHOR = 'Sam Smoot'
EMAIL  = 'ssmoot [a] gmail [d] com'
GEM_NAME = 'dm-more'
GEM_VERSION = DataMapper::More::VERSION
GEM_DEPENDENCIES = [['dm-core', "~>#{GEM_VERSION}"], *gems.map { |g| [g, "~>#{GEM_VERSION}"] }]
GEM_CLEAN = %w[ **/.DS_Store} *.db doc/rdoc .config **/{coverage,log,pkg} cache lib/dm-more.rb ]
GEM_EXTRAS = { :has_rdoc => false }

PROJECT_NAME = 'datamapper'
PROJECT_URL  = 'http://github.com/sam/dm-more/tree/master'
PROJECT_DESCRIPTION = 'Faster, Better, Simpler.'
PROJECT_SUMMARY = 'An Object/Relational Mapper for Ruby'

Pathname.glob(ROOT.join('tasks/**/*.rb').to_s).each { |f| require f }

def sudo_gem(cmd)
  sh "#{SUDO} #{RUBY} -S gem #{cmd}", :verbose => false
end

desc "Install #{GEM_NAME} #{GEM_VERSION}"
task :install => [ :install_gems, :package ] do
  sudo_gem "install --local pkg/#{GEM_NAME}-#{GEM_VERSION} --no-update-sources"
end

desc "Uninstall #{GEM_NAME} #{GEM_VERSION}"
task :uninstall => [ :uninstall_gems, :clobber ] do
  sudo_gem "uninstall #{GEM_NAME} -v#{GEM_VERSION} -Ix"
end

def rake(cmd)
  sh "#{RUBY} -S rake #{cmd}", :verbose => false
end

desc "Build #{GEM_NAME} #{GEM_VERSION}"
task :build_gems do
  GEM_PATHS.each do |dir|
    Dir.chdir(dir){ rake 'gem' }
  end
end

desc 'Install the dm-more gems'
task :install_gems => :build_gems do
  GEM_PATHS.each do |dir|
    Dir.chdir(dir){ rake 'install; true' }
  end
end

desc 'Uninstall the dm-more gems'
task :uninstall_gems do
  GEM_PATHS.each do |dir|
    Dir.chdir(dir){ rake 'uninstall; true' }
  end
end

task :package => %w[ lib/dm-more.rb ]

task 'lib/dm-more.rb' do
  mkdir_p 'lib'
  File.open('lib/dm-more.rb', 'w+') do |file|
    file.puts '### AUTOMATICALLY GENERATED.  DO NOT EDIT.'
    (gems - %w[ dm-gen ]).each do |gem|
      lib = if '-adapter' == gem[-8..-1]
        gem.split('-')[1..-1].join('_')
      else
        gem
      end
      file.puts "require '#{lib}'"
    end
  end
end

task :bundle => [ :package, :build_gems ] do
  mkdir_p 'bundle'
  cp "pkg/dm-more-#{GEM_VERSION}.gem", 'bundle'
  GEM_PATHS.each do |gem|
    File.open("#{gem}/Rakefile") do |rakefile|
      rakefile.read.detect {|l| l =~ /^VERSION\s*=\s*'(.*)'$/ }
      cp "#{gem}/pkg/#{File.basename(gem)}-#{$1}.gem", 'bundle'
    end
  end
end

# NOTE: this task must be named release_all, and not release
desc "Release #{GEM_NAME} #{GEM_VERSION}"
task :release_all do
  sh "rake release VERSION=#{GEM_VERSION}"
  GEM_PATHS.each do |dir|
    Dir.chdir(dir) { rake "release VERSION=#{GEM_VERSION}" }
  end
end

%w[ ci spec clean clobber check_manifest ].each do |command|
  task command do
    GEM_PATHS.each do |gem_name|
      Dir.chdir(gem_name){ rake "#{command}; true" }
    end
  end
end

task :update_manifest do
  GEM_PATHS.each do |gem_name|
    Dir.chdir(gem_name){ rake 'check_manifest | patch; true' }
  end
end

namespace :dm do
  desc 'Run specifications'
  task :specs do
    Spec::Rake::SpecTask.new(:spec) do |t|
      Dir['**/Rakefile'].each do |rakefile|
        # don't run in the top level dir or in the pkg dir
        unless rakefile == 'Rakefile' || rakefile =~ /^pkg/
          # running chdir in a block runs the task in specified dir, then returns to previous dir.
          Dir.chdir(File.join(File.dirname(__FILE__), File.dirname(rakefile))) do
            raise "Broken specs in #{rakefile}" unless system 'rake'
          end
        end
      end
    end
  end
end

task :default => :spec
