begin
  require 'rubygems/package_task'
rescue LoadError
  # rake/gempackagetask will go away some day
  require 'rake/gempackagetask'
  Gem::PackageTask = Rake::GemPackageTask
end

##
# Package plugin for hoe.
#
# === Tasks Provided:
#
# install_gem::        Install the package as a gem.
# release::            Package and upload the release to rubyforge.

module Hoe::Package
  ##
  # Optional: Should package create a tarball? [default: true]

  attr_accessor :need_tar

  ##
  # Optional: Should package create a zipfile? [default: false]

  attr_accessor :need_zip

  ##
  # Initialize variables for plugin.

  def initialize_package
    self.need_tar ||= true
    self.need_zip ||= false
  end

  ##
  # Define tasks for plugin.

  def define_package_tasks
    Gem::PackageTask.new spec do |pkg|
      pkg.need_tar = @need_tar
      pkg.need_zip = @need_zip
    end

    desc 'Install the package as a gem.'
    task :install_gem => [:clean, :package, :check_extra_deps] do
      install_gem Dir['pkg/*.gem'].first
    end

    desc 'Package and upload the release.'
    task :release => [:prerelease, :release_to, :postrelease]

    # no doco, invisible hook
    task :prerelease do
      abort "Fix your version before you release" if
        spec.version.version =~ /borked/
    end

    # no doco, invisible hook
    task :release_to => :release_to_rubyforge

    # no doco, invisible hook
    task :postrelease

    desc "Sanity checks for release"
    task :release_sanity do
      v = ENV["VERSION"] or abort "Must supply VERSION=x.y.z"
      abort "Versions don't match #{v} vs #{version}" if v != version
    end

    desc 'Release to rubyforge.'
    task :release_to_rubyforge => [:clean, :package, :release_sanity] do
      rf = RubyForge.new.configure
      puts "Logging in"
      rf.login

      c = rf.userconfig
      c["release_notes"]   = description if description
      c["release_changes"] = changes     if changes
      c["preformatted"]    = true

      pkg   = "pkg/#{name}-#{version}"
      files = [(@need_tar ? "#{pkg}.tgz" : nil),
               (@need_zip ? "#{pkg}.zip" : nil),
               Dir["#{pkg}*.gem"]].flatten.compact

      puts "Releasing #{name} v. #{version}"
      rf.add_release rubyforge_name, name, version, *files
    end
  end

  ##
  # Install the named gem.

  def install_gem name, version = nil
    gem_cmd = Gem.default_exec_format % 'gem'
    sudo    = 'sudo '                  unless Hoe::WINDOZE
    local   = '--local'                unless version
    version = "--version '#{version}'" if     version
    sh "#{sudo}#{gem_cmd} install #{local} #{name} #{version}"
  end
end
