require 'fileutils'
require 'open-uri'

##
# multiruby_setup is a script to help you manage multiruby.
#
# usage: multiruby_setup [-h|cmd|spec...]
#
#   cmds:
#
#     -h, --help, help = show this help.
#     build            = build and install everything. used internally.
#     clean            = clean scm build dirs and remove non-scm build dirs.
#     list             = print installed versions.
#     rm:$version      = remove a particular version.
#     rubygems:merge   = symlink all rubygem dirs to one dir.
#     tags             = list all tags from svn.
#     update           = update svn builds.
#     update:rubygems  = update rubygems and nuke install dirs.
#
#   specs:
#
#     the_usual              = alias for latest versions from tar + rubygems
#     mri:svn:current        = alias for mri:svn:releases and mri:svn:branches.
#     mri:svn:releases       = alias for supported releases of mri ruby.
#     mri:svn:branches       = alias for active branches of mri ruby.
#     mri:svn:branch:$branch = install a specific $branch of mri from svn.
#     mri:svn:tag:$tag       = install a specific $tag of mri from svn.
#     mri:tar:$version       = install a specific $version of mri from tarball.
#     rbx:ln:$dir            = symlink your rbx $dir
#     rbx:git:current        = install rbx from git
#
#   environment variables:
#
#     GEM_URL  = url for rubygems tarballs
#     MRI_SVN  = url for MRI SVN
#     RBX_GIT  = url for rubinius git
#     RUBY_URL = url for MRI tarballs
#     VERSIONS = what versions to install
#
#     RUBYOPT is cleared on installs.
#
# NOTES:
#
# * you can add a symlink to your rubinius build into ~/.multiruby/install
# * I need patches/maintainers for other implementations.
#
module Multiruby
  def self.env name, fallback; ENV[name] || fallback; end # :nodoc:

  TAGS     = %w(    1_8_6 1_8_7 1_9_1)
  BRANCHES = %w(1_8 1_8_6 1_8_7 trunk)

  VERSIONS = env('VERSIONS', TAGS.join(":").gsub(/_/, '.')).split(/:/)
  MRI_SVN  = env 'MRI_SVN',  'http://svn.ruby-lang.org/repos/ruby'
  RBX_GIT  = env 'RBX_GIT',  'git://github.com/evanphx/rubinius.git'
  RUBY_URL = env 'RUBY_URL', 'http://ftp.ruby-lang.org/pub/ruby'
  GEM_URL  = env 'GEM_URL',  'http://files.rubyforge.vm.bytemark.co.uk/rubygems'

  HELP = []

  File.readlines(__FILE__).each do |line|
    next unless line =~ /^#( |$)/
    HELP << line.sub(/^# ?/, '')
  end

  def self.build_and_install
    ENV.delete 'RUBYOPT'

    root_dir = self.root_dir
    versions = []

    Dir.chdir root_dir do
      self.setup_dirs

      rubygems = Dir["versions/rubygems*.tgz"]
      abort "You should delete all but one rubygem tarball" if rubygems.size > 1
      rubygem_tarball = File.expand_path rubygems.last rescue nil

      Dir.chdir "build" do
        Dir["../versions/*"].sort.each do |tarball|
          next if tarball =~ /rubygems/

          build_dir = File.basename tarball, ".tar.gz"
          version = build_dir.sub(/^ruby-?/, '')
          versions << version
          inst_dir = "#{root_dir}/install/#{version}"

          unless test ?d, inst_dir then
            unless test ?d, build_dir then
              if test ?d, tarball then
                dir = File.basename tarball
                FileUtils.ln_sf "../versions/#{dir}", "../build/#{dir}"
              else
                puts "creating #{inst_dir}"
                Dir.mkdir inst_dir
                run "tar zxf #{tarball}"
              end
            end
            Dir.chdir build_dir do
              puts "building and installing #{version}"
              if test ?f, "configure.in" then
                gnu_utils_build inst_dir
              elsif test ?f, "Rakefile" then
                rake_build inst_dir
              else
                raise "dunno how to build"
              end

              if rubygem_tarball and version !~ /1[._-]9|mri_trunk|rubinius/ then
                rubygems = File.basename rubygem_tarball, ".tgz"
                run "tar zxf #{rubygem_tarball}" unless test ?d, rubygems

                Dir.chdir rubygems do
                  run "../ruby ./setup.rb --no-rdoc --no-ri", "../log.rubygems"
                end
              end
            end
          end
        end
      end
    end

    versions
  end

  def self.clean
    self.each_scm_build_dir do |style|
      case style
      when :svn, :git then
        if File.exist? "Rakefile" then
          run "rake clean"
        elsif File.exist? "Makefile" then
          run "make clean"
        end
      else
        FileUtils.rm_rf Dir.pwd
      end
    end
  end

  def self.each_scm_build_dir
    Multiruby.in_build_dir do
      Dir["*"].each do |dir|
        next unless File.directory? dir
        Dir.chdir dir do
          if File.exist?(".svn") || File.exist?(".git") then
            scm = File.exist?(".svn") ? :svn : :git
            yield scm
          else
            yield :none
          end
        end
      end
    end
  end

  def self.extract_latest_version url, matching=nil
    file = URI.parse(url).read
    versions = file.scan(/href="(ruby.*tar.gz)"/).flatten.reject { |s|
      s =~ /preview|-rc\d/
    }.sort_by { |s|
      s.split(/\D+/).map { |i| i.to_i }
    }.flatten
    versions = versions.grep(/#{Regexp.escape(matching)}/) if matching
    versions.last
  end

  def self.fetch_tar v
    in_versions_dir do
      warn "  Determining latest version for #{v}"
      ver = v[/\d+\.\d+/]
      base = extract_latest_version("#{RUBY_URL}/#{ver}/", v)
      abort "Could not determine release for #{v}" unless base
      url = File.join RUBY_URL, ver, base
      unless File.file? base then
        warn "    Fetching #{base} via HTTP... this might take a while."
        open(url) do |f|
          File.open base, 'w' do |out|
            out.write f.read
          end
        end
      end
    end
  end

  def self.git_clone url, dir
    Multiruby.in_versions_dir do
      Multiruby.run "git clone #{url} #{dir}" unless File.directory? dir
      FileUtils.ln_sf "../versions/#{dir}", "../build/#{dir}"
    end
  end

  def self.gnu_utils_build inst_dir
    run "autoconf" unless test ?f, "configure"
    run "./configure --enable-shared --prefix #{inst_dir}", "log.configure" unless
      test ?f, "Makefile"
    run "(nice make -j4; nice make)", "log.build"
    run "make install", "log.install"
  end

  def self.help
    puts HELP.join
  end

  def self.in_build_dir
    Dir.chdir File.join(self.root_dir, "build") do
      yield
    end
  end

  def self.in_install_dir
    Dir.chdir File.join(self.root_dir, "install") do
      yield
    end
  end

  def self.in_root_dir
    Dir.chdir self.root_dir do
      yield
    end
  end

  def self.in_tmp_dir
    Dir.chdir File.join(self.root_dir, "tmp") do
      yield
    end
  end

  def self.in_versions_dir
    Dir.chdir File.join(self.root_dir, "versions") do
      yield
    end
  end

  def self.list
    puts "Known versions:"
    in_install_dir do
      Dir["*"].sort.each do |d|
        puts "  #{d}"
      end
    end
  end

  def self.merge_rubygems
    in_install_dir do
      gems = Dir["*/lib/ruby/gems"]

      unless test ?d, "../gems" then
        FileUtils.mv gems.first, ".."
      end

      gems.each do |d|
        FileUtils.rm_rf d
        FileUtils.ln_sf "../../../../gems", d
      end
    end
  end

  def self.mri_latest_tag v
    Multiruby.tags.grep(/#{v}/).last
  end

  def self.rake_build inst_dir
    run "rake", "log.build"
    FileUtils.ln_sf "../build/#{File.basename Dir.pwd}", inst_dir
  end

  def self.rbx_ln dir
    dir = File.expand_path dir
    Multiruby.in_versions_dir do
      FileUtils.ln_sf dir, "rubinius"
      FileUtils.ln_sf "../versions/rubinius", "../install/rubinius"
    end
  end

  def self.rm name
    Multiruby.in_root_dir do
      FileUtils.rm_rf Dir["*/#{name}"]
      f = "versions/ruby-#{name}.tar.gz"
      File.unlink f if test ?f, f
    end
  end

  def self.root_dir
    root_dir = File.expand_path(ENV['MULTIRUBY'] ||
                                File.join(ENV['HOME'], ".multiruby"))

    unless test ?d, root_dir then
      puts "creating #{root_dir}"
      Dir.mkdir root_dir, 0700
    end

    root_dir
  end

  def self.run base_cmd, log = nil
    cmd = base_cmd
    cmd += " > #{log} 2>&1" if log
    puts "Running command: #{cmd}"
    raise "ERROR: Command failed with exit code #{$?}" unless system cmd
  end

  def self.setup_dirs download = true
    %w(build install versions tmp).each do |dir|
      unless test ?d, dir then
        puts "creating #{dir}"
        Dir.mkdir dir
        if dir == "versions" && download then
          warn "  Downloading initial ruby tarballs to ~/.multiruby/versions:"
          VERSIONS.each do |v|
            self.fetch_tar v
          end
          warn "  ...done"
          warn "  Put other ruby tarballs in ~/.multiruby/versions to use them."
        end
      end
    end
  end

  def self.svn_co url, dir
    Multiruby.in_versions_dir do
      Multiruby.run "svn co #{url} #{dir}" unless File.directory? dir
      FileUtils.ln_sf "../versions/#{dir}", "../build/#{dir}"
    end
  end

  def self.tags
    tags = nil
    Multiruby.in_tmp_dir do
      cache = "svn.tag.cache"
      File.unlink cache if Time.now - File.mtime(cache) > 3600 rescue nil

      File.open cache, "w" do |f|
        f.write `svn ls #{MRI_SVN}/tags/`
      end unless File.exist? cache

      tags = File.read(cache).split(/\n/).grep(/^v/).reject {|s| s =~ /preview/}
    end

    tags = tags.sort_by { |s| s.scan(/\d+/).map { |s| s.to_i } }
  end

  def self.update
    # TODO:
    # update will look at the dir name and act accordingly rel_.* will
    # figure out latest tag on that name and svn sw to it trunk and
    # others will just svn update

    clean = []

    self.each_scm_build_dir do |style|
      dir = File.basename(Dir.pwd)
      warn dir

      case style
      when :svn then
        case dir
        when /mri_\d/ then
          system "svn cleanup" # just in case
          svn_up = `svn up`
          in_build_dir do
            if svn_up =~ /^[ADUCG] / then
              clean << dir
            else
              warn "  no update"
            end
            FileUtils.ln_sf "../build/#{dir}", "../versions/#{dir}"
          end
        when /mri_rel_(.+)/ then
          ver = $1
          url = `svn info`[/^URL: (.*)/, 1]
          latest = self.mri_latest_tag(ver).chomp('/')
          new_url = File.join(File.dirname(url), latest)
          if new_url != url then
            run "svn sw #{new_url}"
            clean << dir
          else
            warn "  no update"
          end
        else
          warn "  update in this svn dir not supported yet: #{dir}"
        end
      when :git then
        case dir
        when /rubinius/ then
          run "rake git:update build" # minor cheat by building here
        else
          warn "  update in this git dir not supported yet: #{dir}"
        end
      else
        warn "  update in non-svn dir not supported yet: #{dir}"
      end
    end

    in_install_dir do
      clean.each do |dir|
        warn "removing install/#{dir}"
        FileUtils.rm_rf dir
      end
    end
  end

  def self.update_rubygems
    warn "  Determining latest version for rubygems"
    html = URI.parse(GEM_URL).read

    versions = html.scan(/href="rubygems-update-(\d+(?:\.\d+)+).gem/i).flatten
    latest = versions.sort_by { |s| s.scan(/\d+/).map { |s| s.to_i } }.last

    Multiruby.in_versions_dir do
      file = "rubygems-#{latest}.tgz"
      unless File.file? file then
        warn "    Fetching rubygems-#{latest}.tgz via HTTP."
        File.unlink(*Dir["rubygems*"])
        File.open file, 'w' do |f|
          f.write URI.parse(GEM_URL+"/"+file).read
        end
      end
    end

    Multiruby.in_install_dir do
      FileUtils.rm_rf Dir["*"]
    end
  end
end
