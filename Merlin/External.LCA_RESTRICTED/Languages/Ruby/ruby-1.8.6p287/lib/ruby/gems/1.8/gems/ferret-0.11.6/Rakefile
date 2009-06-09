$:. << 'lib'
# Some parts of this Rakefile where taken from Jim Weirich's Rakefile for
# Rake. Other parts where taken from the David Heinemeier Hansson's Rails
# Rakefile. Both are under MIT-LICENSE. Thanks to both for their excellent
# projects.

require 'rake'
require 'rake/testtask'
require 'rake/rdoctask'
require 'rake/clean'
require 'ferret_version'

begin
  require 'rubygems'
  require 'rake/gempackagetask'
rescue Exception
  nil
end

CURRENT_VERSION = Ferret::VERSION
if ENV['REL']
  PKG_VERSION = ENV['REL']
else
  PKG_VERSION = CURRENT_VERSION
end

def announce(msg='')
  STDERR.puts msg
end

EXT = "ferret_ext.so"
EXT_SRC = FileList["../c/src/*.[c]", "../c/include/*.h",
                   "../c/lib/libstemmer_c/src_c/*.[ch]",
                   "../c/lib/libstemmer_c/runtime/*.[ch]",
                   "../c/lib/libstemmer_c/libstemmer/*.[ch]",
                   "../c/lib/libstemmer_c/include/libstemmer.h"]
EXT_SRC.exclude('../**/ind.[ch]')

EXT_SRC_DEST = EXT_SRC.map {|fn| File.join("ext", File.basename(fn))}
SRC = (FileList["ext/*.[ch]"] + EXT_SRC_DEST).uniq

CLEAN.include(FileList['**/*.o', '**/*.obj', 'InstalledFiles',
                       '.config', 'ext/cferret.c'])
CLOBBER.include(FileList['**/*.so'], 'ext/Makefile', EXT_SRC_DEST)
POLISH = Rake::FileList.new.include(FileList['**/*.so'], 'ext/Makefile')

desc "Clean specifically for the release."
task :polish => [:clean] do
  POLISH.each { |fn| rm_r fn rescue nil }
end

desc "Run tests with Valgrind"
task :valgrind do
  sh "valgrind --gen-suppressions=yes --suppressions=ferret_valgrind.supp " +
     "--leak-check=yes --show-reachable=yes -v ruby test/test_all.rb"
  #sh "valgrind --suppressions=ferret_valgrind.supp " +
  #   "--leak-check=yes --show-reachable=yes -v ruby test/unit/index/tc_index_reader.rb"
  #valgrind --gen-suppressions=yes --suppressions=ferret_valgrind.supp --leak-check=yes --show-reachable=yes -v ruby test/test_all.rb
end

task :default => :test_all
#task :default => :ext do
#  sh "ruby test/unit/index/tc_index.rb"
#end

desc "Run all tests"
task :test_all => [ :test_units ]

desc "Generate API documentation"
task :doc => [ :appdoc ]

desc "run unit tests in test/unit"
Rake::TestTask.new("test_units" => :ext) do |t|
  t.libs << "test/unit"
  t.pattern = 'test/unit/t[cs]_*.rb'
  #t.pattern = 'test/unit/search/tc_index_searcher.rb'
  t.verbose = true
end

desc "Generate documentation for the application"
rd = Rake::RDocTask.new("appdoc") do |rdoc|
  rdoc.rdoc_dir = 'doc/api'
  rdoc.title    = "Ferret Search Library Documentation"
  rdoc.options << '--line-numbers'
  rdoc.options << '--inline-source'
  rdoc.options << '--charset=utf-8'
  rdoc.rdoc_files.include('README')
  rdoc.rdoc_files.include('TODO')
  rdoc.rdoc_files.include('TUTORIAL')
  rdoc.rdoc_files.include('MIT-LICENSE')
  rdoc.rdoc_files.include('lib/**/*.rb')
  rdoc.rdoc_files.include('ext/r_*.c')
  rdoc.rdoc_files.include('ext/ferret.c')
end

EXT_SRC.each do |fn|
  dest_fn = File.join("ext", File.basename(fn))
  file dest_fn => fn do |t|
    begin
      raise "copy for release" if ENV["REL"]
      ln_s File.join("..", fn), dest_fn
    rescue Exception => e
      cp File.expand_path(fn), dest_fn
    end

    if fn =~ /stemmer/
      # flatten the directory structure for lib_stemmer
      open(dest_fn) do |in_f|
        open(dest_fn + ".out", "w") do |out_f|
          in_f.each {|line| out_f.write(line.sub(/(#include ["<])[.a-z_\/]*\//) {"#{$1}"})}
        end
      end
      mv dest_fn + ".out", dest_fn
    end
  end
end if File.exists?("../c")

desc "Build the extension"
task :ext => ["ext/#{EXT}"] + SRC do
  rm_f 'ext/mem_pool.*'
  rm_f 'ext/defines.h'
end

file "ext/#{EXT}" => ["ext/Makefile"] do
  cp "ext/inc/lang.h", "ext/lang.h"
  cp "ext/inc/threading.h", "ext/threading.h"
  cd "ext"
  if (/mswin/ =~ RUBY_PLATFORM) and ENV['make'].nil?
    begin
      sh "nmake"
    rescue Exception => e
      puts
      puts "**********************************************************************"
      puts "You may need to call VCVARS32.BAT to set the environment variables."
      puts '  "f:\Program Files\Microsoft Visual Studio\VC98\Bin\VCVARS32.BAT"'
      puts "**********************************************************************"
      puts
      raise e
    end
  else
    sh "make"
  end
  cd ".."
end

file "ext/lang.h" => ["ext/inc/lang.h"] do
  rm_f "ext/lang.h"
  cp "ext/inc/lang.h", "ext/lang.h"
end

file "ext/threading.h" => ["ext/inc/threading.h"] do
  rm_f "ext/threading.h"
  cp "ext/inc/threading.h", "ext/threading.h"
end

file "ext/Makefile" => SRC do
  cd "ext"
  `ruby extconf.rb`
  cd ".."
end

# Make Parsers ---------------------------------------------------------------

RACC_SRC = FileList["lib/**/*.y"]
RACC_OUT = RACC_SRC.collect { |fn| fn.sub(/\.y$/, '.tab.rb') }

task :parsers => RACC_OUT
rule(/\.tab\.rb$/ => [proc {|tn| tn.sub(/\.tab\.rb$/, '.y')}]) do |t|
  sh "racc #{t.source}" 
end

# Create Packages ------------------------------------------------------------

PKG_FILES = FileList[
  'setup.rb',
  '[-A-Z]*',
  'ext/**/*.[ch]', 
  'lib/**/*.rb', 
  'lib/**/*.rhtml', 
  'lib/**/*.css', 
  'lib/**/*.js', 
  'test/**/*.rb',
  'test/**/wordfile',
  'rake_utils/**/*.rb',
  'Rakefile'
]
PKG_FILES.exclude('**/*.o')
PKG_FILES.exclude('**/Makefile')
PKG_FILES.exclude('ext/ferret_ext.so')


if ! defined?(Gem)
  puts "Package Target requires RubyGEMs"
else
  spec = Gem::Specification.new do |s|
    
    #### Basic information.
    s.name = 'ferret'
    s.version = PKG_VERSION
    s.summary = "Ruby indexing library."
    s.description = <<-EOF
      Ferret is a port of the Java Lucene project. It is a powerful
      indexing and search library.
    EOF

    #### Dependencies and requirements.
    s.add_dependency('rake')
    s.files = PKG_FILES.to_a
    s.extensions << "ext/extconf.rb"
    s.require_path = 'lib'
    s.autorequire = 'ferret'
    s.bindir = 'bin'
    s.executables = ['ferret-browser']
    s.default_executable = 'ferret-browser'

    #### Author and project details.
    s.author = "David Balmain"
    s.email = "dbalmain@gmail.com"
    s.homepage = "http://ferret.davebalmain.com/trac"
    s.rubyforge_project = "ferret"

    s.has_rdoc = true
    s.extra_rdoc_files = rd.rdoc_files.reject { |fn| fn =~ /\.rb$/ }.to_a
    s.rdoc_options <<
      '--title' <<  'Ferret -- Ruby Indexer' <<
      '--main' << 'README' << '--line-numbers' <<
      'TUTORIAL' << 'TODO'

    if RUBY_PLATFORM =~ /mswin/
      s.files = PKG_FILES.to_a + ["ext/#{EXT}"]
      s.extensions.clear
      s.platform = Gem::Platform::WIN32
    end
  end

  package_task = Rake::GemPackageTask.new(spec) do |pkg|
    unless RUBY_PLATFORM =~ /mswin/
      pkg.need_zip = true
      pkg.need_tar = true
    end
  end
end

# Support Tasks ------------------------------------------------------

desc "Look for TODO and FIXME tags in the code"
task :todo do
  FileList['**/*.rb'].egrep /#.*(FIXME|TODO|TBD)/
end
# --------------------------------------------------------------------
# Creating a release

desc "Make a new release"
task :release => [
  :prerelease,
  :polish,
  :test_all,
  :update_version,
  :package,
  :tag] do
  announce 
  announce "**************************************************************"
  announce "* Release #{PKG_VERSION} Complete."
  announce "* Packages ready to upload."
  announce "**************************************************************"
  announce 
end

# Validate that everything is ready to go for a release.
task :prerelease do
  announce 
  announce "**************************************************************"
  announce "* Making RubyGem Release #{PKG_VERSION}"
  announce "* (current version #{CURRENT_VERSION})"
  announce "**************************************************************"
  announce  

  # Is a release number supplied?
  unless ENV['REL']
    fail "Usage: rake release REL=x.y.z [REUSE=tag_suffix]"
  end

  # Is the release different than the current release.
  # (or is REUSE set?)
  if PKG_VERSION == CURRENT_VERSION && ! ENV['REUSE']
    fail "Current version is #{PKG_VERSION}, must specify REUSE=tag_suffix to reuse version"
  end

  # Are all source files checked in?
  data = `svn -q --ignore-externals status`
  unless data =~ /^$/
    fail "'svn -q status' is not clean ... do you have unchecked-in files?"
  end
  
  announce "No outstanding checkins found ... OK"
end

def reversion(fn)
  open(fn) do |ferret_in|
    open(fn + ".new", "w") do |ferret_out|
      ferret_in.each do |line|
        if line =~ /^  VERSION\s*=\s*/
          ferret_out.puts "  VERSION = '#{PKG_VERSION}'"
        else
          ferret_out.puts line
        end
      end
    end
  end
  mv fn + ".new", fn
end

task :update_version => [:prerelease] do
  if PKG_VERSION == CURRENT_VERSION
    announce "No version change ... skipping version update"
  else
    announce "Updating Ferret version to #{PKG_VERSION}"
    reversion("lib/ferret_version.rb")
    if ENV['RELTEST']
      announce "Release Task Testing, skipping commiting of new version"
    else
      sh %{svn ci -m "Updated to version #{PKG_VERSION}" lib/ferret_version.rb}
    end
  end
end

desc "Tag all the SVN files with the latest release number (REL=x.y.z)"
task :tag => [:prerelease] do
  reltag = "REL-#{PKG_VERSION}"
  reltag << ENV['REUSE'] if ENV['REUSE']
  announce "Tagging SVN with [#{reltag}]"
  if ENV['RELTEST']
    announce "Release Task Testing, skipping SVN tagging. Would do the following;"
    announce %{svn copy -m "creating release #{reltag}" svn://www.davebalmain.com/ferret/trunk svn://www.davebalmain.com/ferret/tags/#{reltag}}
  else
    sh %{svn copy -m "creating release #{reltag}" svn://www.davebalmain.com/ferret/trunk svn://www.davebalmain.com/ferret/tags/#{reltag}}
  end
end
