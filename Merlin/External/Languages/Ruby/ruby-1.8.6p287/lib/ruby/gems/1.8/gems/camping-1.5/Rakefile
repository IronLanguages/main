require 'rake'
require 'rake/clean'
require 'rake/gempackagetask'
require 'rake/rdoctask'
require 'fileutils'
include FileUtils

NAME = "camping"
REV = File.read(".svn/entries")[/committed-rev="(\d+)"/, 1] rescue nil
VERS = ENV['VERSION'] || ("1.5" + (REV ? ".#{REV}" : ""))
CLEAN.include ['**/.*.sw?', '*.gem', '.config']
RDOC_OPTS = ['--quiet', '--title', "Camping, the Documentation",
    "--opname", "index.html",
    "--line-numbers", 
    "--main", "README",
    "--inline-source"]

desc "Packages up Camping."
task :default => [:package]
task :package => [:clean]

task :doc => [:before_doc, :rdoc, :after_doc]

task :before_doc do 
    mv "lib/camping.rb", "lib/camping-mural.rb"
    mv "lib/camping-unabridged.rb", "lib/camping.rb"
end

Rake::RDocTask.new do |rdoc|
    rdoc.rdoc_dir = 'doc/rdoc'
    rdoc.options += RDOC_OPTS
    rdoc.template = "extras/flipbook_rdoc.rb"
    rdoc.main = "README"
    rdoc.title = "Camping, the Documentation"
    rdoc.rdoc_files.add ['README', 'CHANGELOG', 'COPYING', 'lib/camping.rb', 'lib/camping/*.rb']
end

task :after_doc do
    mv "lib/camping.rb", "lib/camping-unabridged.rb"
    mv "lib/camping-mural.rb", "lib/camping.rb"
    cp "extras/Camping.gif", "doc/rdoc/"
    cp "extras/permalink.gif", "doc/rdoc/"
    sh %{scp -r doc/rdoc/* #{ENV['USER']}@rubyforge.org:/var/www/gforge-projects/camping/}
end

spec =
    Gem::Specification.new do |s|
        s.name = NAME
        s.version = VERS
        s.platform = Gem::Platform::RUBY
        s.has_rdoc = true
        s.extra_rdoc_files = ["README", "CHANGELOG", "COPYING"]
        s.rdoc_options += RDOC_OPTS + ['--exclude', '^(examples|extras)\/', '--exclude', 'lib/camping.rb']
        s.summary = "minature rails for stay-at-home moms"
        s.description = s.summary
        s.author = "why the lucky stiff"
        s.email = 'why@ruby-lang.org'
        s.homepage = 'http://code.whytheluckystiff.net/camping/'
        s.executables = ['camping']

        s.add_dependency('activesupport', '>=1.3.1')
        s.add_dependency('markaby', '>=0.5')
        s.add_dependency('metaid')
        s.required_ruby_version = '>= 1.8.2'

        s.files = %w(COPYING README Rakefile) +
          Dir.glob("{bin,doc,test,lib,extras}/**/*") + 
          Dir.glob("ext/**/*.{h,c,rb}") +
          Dir.glob("examples/**/*.rb") +
          Dir.glob("tools/*.rb")
        
        s.require_path = "lib"
        # s.extensions = FileList["ext/**/extconf.rb"].to_a
        s.bindir = "bin"
    end

Rake::GemPackageTask.new(spec) do |p|
    p.need_tar = true
    p.gem_spec = spec
end

task :install do
  sh %{rake package}
  sh %{sudo gem install pkg/#{NAME}-#{VERS}}
end

task :uninstall => [:clean] do
  sh %{sudo gem uninstall #{NAME}}
end
