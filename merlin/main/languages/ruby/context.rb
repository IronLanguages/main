# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

require 'rubygems'
require 'pathname2'
require 'rexml/document'
require 'fileutils'
require 'tempfile'

ENV['HOME'] ||= ENV['USERPROFILE']
SVN_ROOT = Pathname.new 'c:/svn/trunk'
EXCLUDED_EXTENSIONS   = %w[.old .suo .vspscc .vssscc .user .log .pdb .cache .swp]
DEFAULT_ROBOCOPY_OPTIONS = "/XF *#{EXCLUDED_EXTENSIONS.join(' *')} /NP /COPY:DAT /A-:R "

#bogus comment for jredville testing
class Pathname
  def filtered_subdirs(extras = [])
    return [] unless exist?
    filtered_subdirs = subdirs.find_all { |dir| (dir.to_a & (EXCLUDED_DIRECTORIES + extras)) == [] }
    result = filtered_subdirs.map { |dir| self + dir }
    result.unshift self
  end

  def subdirs
    glob("**/*", File::FNM_DOTMATCH).find_all { |path| (self + path).directory? }
  end

  def files
    raise "Cannot call files on a filename path: #{self}" if !directory?
    entries.find_all { |e| !(self + e).directory? }.sort
  end

  def filtered_files
    raise "Cannot call filtered_files on a filename path: #{self}" if !directory?
    files.find_all { |f| !EXCLUDED_EXTENSIONS.include?((self + f).extname) }.map { |f| f.downcase }
  end
end

class String
  def change_tag!(name, value = '')
    change_tag_value!(name, "(.*?)", value)
  end

  def change_tag_value!(name, old_value, new_value)
    pattern = Regexp.new "\<#{name}\>#{old_value}\<\/#{name}\>", Regexp::MULTILINE
    self.gsub! pattern, "\<#{name}\>#{new_value}\<\/#{name}\>"
  end

  def change_configuration!(name, value)
    source = self.clone
    group = "<PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == '#{name}' \">"
    found = false
    self.replace ''
    source.each do |line|
      match = line.match /\<DefineConstants\>/
      index = match.nil? ? -1 : match.begin(0)
      self << (found && index > 0 ? " " * 4 + "<DefineConstants>#{value}</DefineConstants>\n" : line)
      found = true if line.include? group
      found = false if line.include? "</PropertyGroup>"
    end
  end
end

module SvnProvider
  def add(path)
    exec_f "svn add --force #{path}"
  end

  def delete(path)
    exec_f "svn delete --force #{path}"
  end

  def checkout(path)
  end
end

module TfsProvider
  def add(path)
    exec_f "tf add #{path}"
  end

  def delete(path)
    exec_f "tf delete #{path}"
  end

  def checkout(path)
    exec_f "tf checkout #{path}"
  end
end

class Configuration
  class Group
    attr_reader :super_group

    def initialize(name, super_group)
      @name = name
      @switches = {}
      @references = []
      @super_group = super_group
    end

    def switches(config, *args)
      @switches[config] ||= []
      @switches[config] += args
    end

    def references(*refs)
      @references += refs
    end

    def remove_switches(config, *args)
      args.each { |arg| @switches[config].delete arg } unless @switches[config].nil?
    end

    def get_switches(config)
      all = @switches[:all].nil? ? [] : @switches[:all]
      (@switches[config].nil? ? [] : @switches[config]) + all
    end

    def get_references
      @references
    end

    def framework_path(*args, &b)
      @framework_path = (if args.length == 0
        b.call
      elsif args.length == 1
        args.first
      else
        raise 'framework_path must be called with either a path or a block that defines a path'
      end << nil)
    end

    def resolve_framework_path(path)
      @framework_path.each do |candidate|
        raise "cannot resolve path #{path}" if candidate.nil?
        candidate_path = candidate + path
        break candidate_path if candidate_path.file?
      end
    end
  end

  def self.define(&b)
    @master = {}
    instance_eval(&b)
  end

  def self.group(*args, &b)
    name = args.first
    super_group = args.length == 2 ? @master[args.last] : nil
    @master[name] ||= Group.new(name, super_group)
    @master[name].instance_eval(&b)
  end

  def self.get_switches(group, config)
    result = []
    current = @master[group]
    while !current.nil?
      result += current.get_switches(config)
      current = current.super_group
    end
    result
  end

  def self.get_references(group)
    result = []
    current = @master[group]
    while !current.nil?
      result += current.get_references
      current = current.super_group
    end
    result.map { |assembly| resolve_framework_path(group, assembly) }
  end

  def self.resolve_framework_path(group, path)
    @master[group].resolve_framework_path(path)
  end
end

Configuration.define do
  group(:common) {
    switches :all, 'nologo', 'noconfig', 'nowarn:1591,1701,1702', 'errorreport:prompt', 'warn:4', 'warnaserror-'
    switches :debug, 'define:"DEBUG;TRACE;MICROSOFT_SCRIPTING_CORE"', 'debug+', 'debug:full', 'optimize-'
    switches :release, 'define:TRACE', 'optimize+'
    references 'System.dll'
  }
  group(:desktop, :common) {
    references 'System.Configuration.dll'
  }
  group(:silverlight, :common) {
    switches :all, 'define:SILVERLIGHT', 'nostdlib+', 'platform:AnyCPU'
    references 'mscorlib.dll'
  }
  if ENV['mono'].nil?
    group(:desktop) {
      framework_path [Pathname.new(ENV['windir'].dup) + 'Microsoft.NET/Framework/v2.0.50727',
                      Pathname.new('c:\program files\reference assemblies\microsoft\framework\v3.5')]
    }
    group(:silverlight) {
      framework_path [Pathname.new('c:/program files/microsoft silverlight/2.0.30523.6')]
    }
  else
    group(:mono) {
      framework_path {
        libdir = IO.popen('pkg-config --variable=libdir mono').read.strip
        [Pathname.new(libdir) + 'mono' + '2.0']
      }
      switches :all, 'noconfig'
      remove_switches ['warnaserror+']
    }
  end
end

module IronRubyUtils
  def banner(message)
    rake_output_message "-" * 79
    rake_output_message message
    rake_output_message "-" * 79
  end

  def copy_dir(source_dir, target_dir, options = '')
    log_filename = Pathname.new(Dir.tmpdir) + "rake_transform.log"
    exec_f %Q{robocopy "#{source_dir}" "#{target_dir}" #{DEFAULT_ROBOCOPY_OPTIONS} "/LOG+:#{log_filename}" #{options}}
  end
  # Low-level command helpers

  def is_test?
    ENV['test']
  end
  
  def mono?
    ENV['mono']
  end
  
  def rake_version
    cmd = mono? ? "rake" : "rake.bat"
    `#{cmd} --version`.chomp.gsub(/.*?(\d\.\d\.\d)/, '\1')
  end

  def exec(cmd)
    if is_test?
      rake_output_message ">> #{cmd}"
    else
      sh cmd
    end
  end

  def exec_net(cmd)
    if ENV['mono'].nil?
      exec cmd
    else
      exec "mono #{cmd}"
    end
  end

  def exec_f(cmd)
    begin
      exec(cmd)
    rescue
    end
  end
end

class ProjectContext
  Mapping = Struct.new(:merlin_path, :svn_path, :recurse)
  class CommandContext
    include IronRubyUtils


    # context is either :source or :target. project_context is reference
    # to ProjectContext subclass class

    def initialize(context, project_context)
      if context == :source || context == :target
        @project_context = project_context
        @context = context
      else
        raise "CommandContext#initialize(context) must be :source or :target"
      end
    end

    def get_mapping(name)
      mapping = @project_context.resolve(name)
      raise "cannot find #{name} in ProjectContext" if mapping.nil?
      mapping
    end

    # There are three things to consider when getting the source directory path
    # 1) The context (source or destination) we are running in
    # 2) The name of the mapping that we want to look up the source path in
    # 3) Whether we are running in MERLIN_ROOT or SVN_ROOT

    def get_source_dir(name)
      mapping = get_mapping name
      context_path = @project_context.source
      context_path + (@project_context.is_merlin? ? mapping.merlin_path : mapping.svn_path)
    end

    # Getting the target directory path is the same as source except for the
    # reversal of logic around whether to get svn_path or merlin_path based on
    # is_merlin? status

    def get_target_dir(name)
      mapping = get_mapping name
      context_path = @project_context.target
      context_path + (@project_context.is_merlin? ? mapping.svn_path : mapping.merlin_path)
    end

    def get_relative_target_dir(name)
      mapping = get_mapping name
      @project_context.is_merlin? ? mapping.svn_path : mapping.merlin_path
    end

    def is_recursive?(name)
      get_mapping(name).recurse
    end

    def copy_to_temp_dir(name, temp_dir, extras = [])
      IronRuby.source_context do
        source = get_source_dir(name)
        target = get_relative_target_dir(name)

        if is_recursive? name
          source_dirs = source.filtered_subdirs(extras)
          source_dirs.each { |dir| copy_dir dir, temp_dir + target + dir.relative_path_from(source) }
        else
          copy_dir source, temp_dir + target
        end
      end
    end

    def del(name, *paths)
      dir = name.is_a?(Symbol) ? get_source_dir(name) : name
      Dir.chdir(dir) { paths.each { |path| exec_f %Q{del "#{path}"} } }
    end

    def chdir(env, &b)
      dir = env.is_a?(Symbol) ? get_source_dir(env) : env
      Dir.chdir(dir) { instance_eval(&b) }
    end

    def rd(name)
      path = name.is_a?(Symbol) ? get_source_dir(name) : name
      FileUtils.rm_rf path
    end

    def mkdir(name)
      path = name.is_a?(Symbol) ? get_source_dir(name) : name
      FileUtils.mkdir_p path
    end

    def generate_temp_dir
      layout = Pathname.new(Dir.tmpdir) + 'layout'
      del   Pathname.new(Dir.tmpdir), 'rake_transform.log'
      rd    layout
      mkdir layout
      layout
    end

    # Source transformation related methods

    def diff_directories(temp_dir)
      source_dirs, target_dirs = [], []

      nodes = [:root, :gppg, :dlr_core, :dlr_libs, :dlr_com, :ironruby, :libraries, :tests, :console, :generator, :test_runner, :scanner, :yaml, :stdlibs, :ironlibs]
      nodes.each do |node|
        if is_recursive? node
          source_dirs += (temp_dir + get_relative_target_dir(node)).filtered_subdirs.map { |d| d.relative_path_from(temp_dir).downcase }

          # Target directory may not exist, so we only add if we find it there
          if get_target_dir(node).directory?
            target_dirs += get_target_dir(node).filtered_subdirs.map { |d| d.relative_path_from(@project_context.target).downcase }
          end
        else
          # This is also an unusual case - since there is a 1:1 mapping by
          # definition in a non-recursive directory mapping, this will be
          # flagged only as a change candidate and not an add or a delete.
          source_dirs << get_relative_target_dir(node).downcase

          # Target directory may not exist, so we only add if we find it there
          target_dirs << get_relative_target_dir(node).downcase if get_target_dir(node).directory?
        end
      end

      added             = source_dirs - target_dirs
      removed           = target_dirs - source_dirs
      change_candidates = source_dirs & target_dirs

      return added, removed, change_candidates
    end

    def push_to_target(temp_dir)
      rake_output_message "\n#{'=' * 78}\nApplying source changes to target source repository\n\n"
      rake_output_message "Computing directory structure changes ...\n"

      added, removed, change_candidates = diff_directories(temp_dir)

      dest = @project_context.target

      rake_output_message "Adding new directories to target source control\n"
      added.each do |dir|
        copy_dir(temp_dir + dir, dest + dir)
        add(dest + dir)
      end

      rake_output_message "Deleting directories from target source control\n"
      removed.each do |dir|
        rd dest + dir
        delete dest + dir
      end

      rake_output_message "Copying files in changed directories to target source control\n"
      change_candidates.each do |dir|
        src_file_list = (temp_dir + dir).filtered_files
        dest_file_list = (dest + dir).filtered_files

        added             = src_file_list - dest_file_list
        removed           = dest_file_list - src_file_list
        change_candidates = src_file_list & dest_file_list

        added.each do |file|
          copy temp_dir + dir + file, dest + dir + file
          add dest + dir + file
        end

        removed.each do |file|
          delete dest + dir + file
        end

        change_candidates.each do |file|
          source_file = temp_dir + dir + file
          dest_file = dest + dir + file

          if !compare_file(source_file, dest_file)
            checkout dest_file
            copy source_file, dest_file
          end
        end
      end
    end

    # Compiler-related methods

    def resgen(base_path, resource_map)
      resource_map.each_pair do |input, output|
        exec %Q{resgen "#{base_path + input.dup}" "#{build_path + output}"}
      end
    end

    def configuration
      ENV['configuration'].nil? ? :debug : ENV['configuration'].to_sym
    end

    def clr
      if ENV['mono'].nil?
        ENV['clr'].nil? ? :desktop : ENV['clr'].to_sym
      else
        :mono
      end
    end

    def platform
      ENV['platform'].nil? ? :windows : ENV['platform'].to_sym
    end

    def resolve_framework_path(file)
      Configuration.resolve_framework_path(clr, file)
    end

    def build_path
      get_source_dir(:build) + "#{clr == :desktop ? configuration : "#{clr}_#{configuration}"}"
    end

    def compiler_switches
      Configuration.get_switches(clr, configuration)
    end

    def references(refs, working_dir)
      references = Configuration.get_references(clr)
      refs.each do |ref|
        references << if ref =~ /^\!/
          resolve_framework_path(ref[1..ref.length])
        else
          (build_path + ref).relative_path_from(working_dir)
        end
      end unless refs.nil?
      references
    end

    def get_case_sensitive_path(pathname)
      elements = pathname.split '\\'
      result = Pathname.new '.'
      elements.each do |element|
        entry = result.entries.find { |p| p.downcase == element.downcase }
        result = result + entry
      end
      result.to_s
    end

    def get_compile_path_list(csproj)
      csproj ||= '*.csproj' 
      cs_proj_files = Dir[csproj]
      if cs_proj_files.length == 1
        doc = REXML::Document.new(File.open(cs_proj_files.first))
        result = doc.elements.collect("/Project/ItemGroup/Compile") { |c| c.attributes['Include'] }
        result.delete_if { |e| e =~ /(Silverlight\\SilverlightVersion.cs|System\\Dynamic\\Parameterized.System.Dynamic.cs)/ }
        result.map! { |p| get_case_sensitive_path(p) } if ENV['mono']
        result
      else
        raise ArgumentError.new("Found more than one .csproj file in directory! #{cs_proj_files.join(", ")}")
      end
    end

    def compile(name, args)
      banner name.to_s
      working_dir = get_source_dir(name)
      build_dir = build_path

      Dir.chdir(working_dir) do |p|
        cs_args = ["out:\"#{build_dir + args[:output]}\""]
        cs_args += references(args[:references], working_dir).map { |ref| "r:\"#{ref}\"" }
        cs_args += compiler_switches
        cs_args += args[:switches] unless args[:switches].nil?

        unless args[:resources].nil?
          resgen working_dir, args[:resources]
          args[:resources].each_value { |res| cs_args << "resource:\"#{build_path + res}\"" }
        end
        cmd = ''
        cmd << CS_COMPILER
        if cs_args.include?('noconfig')
          cs_args.delete('noconfig')
          cmd << " /noconfig"
        end
        temp = Tempfile.new(name.to_s)
        cs_args.each { |opt| temp << ' /' + opt + "\n"}
        options = get_compile_path_list(args[:csproj]).join("\n")
        temp.puts options
        temp.close
        
        cmd << " @" << temp.path
        exec cmd
      end
    end

    # Project transformation methods

    def replace_output_path(contents, old, new)
      old,new = new, old unless IronRuby.is_merlin?
      contents.gsub! Regexp.new(Regexp.escape("<OutputPath>#{old}</OutputPath>"), Regexp::IGNORECASE), "<OutputPath>#{new}</OutputPath>"
    end

    def replace_doc_path(contents, old, new)
      old,new = new, old unless IronRuby.is_merlin?
      contents.gsub! Regexp.new(Regexp.escape("<DocumentationFile>#{old}</DocumentationFile>"), Regexp::IGNORECASE), "<DocumentationFile>#{new}</DocumentationFile>"
    end

    def replace_key_path(contents, old, new)
      contents.gsub! Regexp.new(Regexp.escape("<AssemblyOriginatorKeyFile>#{old}</AssemblyOriginatorKeyFile>"), Regexp::IGNORECASE), "<AssemblyOriginatorKeyFile>#{new}</AssemblyOriginatorKeyFile>"
    end

    def replace_import_project(contents, old, new)
      old,new = new, old unless IronRuby.is_merlin?
      contents.gsub! Regexp.new(Regexp.escape("<Import Project=\"#{old}\" />"), Regexp::IGNORECASE), "<Import Project=\"#{new}\" />"
    end

    def replace_post_build_event(contents, old, new)
      old,new = new, old unless IronRuby.is_merlin?
      contents.gsub! Regexp.new(Regexp.escape("<PostBuildEvent>#{old}</PostBuildEvent>"), Regexp::IGNORECASE), "<PostBuildEvent>#{new}</PostBuildEvent>"
    end

    def replace_app_config_path(contents, old, new)
      old,new = new, old unless IronRuby.is_merlin?
      contents.gsub! Regexp.new(Regexp.escape(%Q{<None Include="#{old}" />}), Regexp::IGNORECASE), %Q{<None Include="#{new}" />}
    end

    def transform_project(name, project)
      path = get_target_dir(name) + project
      rake_output_message "Transforming: #{path}"
      contents = path.read

      # Extract the project name from .csproj filename
      project_name = /(.*)\.csproj/.match(project)[1]

      if @project_context.is_merlin?
        contents.change_tag! 'SccProjectName'
        contents.change_tag! 'SccLocalPath'
        contents.change_tag! 'SccAuxPath'
        contents.change_tag! 'SccProvider'

        contents.change_tag! 'DelaySign', 'false'
        contents.change_tag! 'SignAssembly', 'false'

        contents.change_configuration! 'Debug|AnyCPU', 'TRACE;DEBUG'
        contents.change_configuration! 'Release|AnyCPU', 'TRACE'
        contents.change_configuration! 'Silverlight Debug|AnyCPU', 'TRACE;DEBUG;SILVERLIGHT'
        contents.change_configuration! 'Silverlight Release|AnyCPU', 'TRACE;SILVERLIGHT'

      else
        contents.change_tag! 'SccProjectName', 'SAK'
        contents.change_tag! 'SccLocalPath', 'SAK'
        contents.change_tag! 'SccAuxPath', 'SAK'
        contents.change_tag! 'SccProvider', 'SAK'

        contents.change_tag! 'DelaySign', 'true'
        contents.change_tag! 'SignAssembly', 'true'

        contents.change_configuration! 'Debug|AnyCPU', 'TRACE;DEBUG;SIGNED'
        contents.change_configuration! 'Release|AnyCPU', 'TRACE;SIGNED'
        contents.change_configuration! 'Silverlight Debug|AnyCPU', 'TRACE;DEBUG;SILVERLIGHT'
        contents.change_configuration! 'Silverlight Release|AnyCPU', 'TRACE;SILVERLIGHT'

        unless block_given?
          replace_key_path    contents, '..\..\RubyTestKey.snk', '..\..\..\MSSharedLibKey.snk'
        end
      end
      if block_given?
        yield contents
      else
        replace_output_path contents, '..\..\..\Bin\Debug\\', '..\..\build\debug\\'
        replace_output_path contents, '..\..\..\Bin\Release\\', '..\..\build\release\\'
        replace_output_path contents, '..\..\..\Bin\Silverlight Debug\\', '..\..\build\silverlight debug\\'
        replace_output_path contents, '..\..\..\Bin\Silverlight Release\\', '..\..\build\silverlight release\\'
      end
      path.open('w+') { |f| f.write contents }
    end
  end

  # The Rakefile must always be found in the root directory of the source tree.

  # If ENV['MERLIN_ROOT'] is defined, then we know that we are running on
  # a machine with an enlistment in the MERLIN repository. This will enable
  # features that require a source context and a destination context (such as
  # pushing to / from MERLIN). Otherwise, destination context will always be
  # nil and we will throw on an attempt to do operations that require a
  # push.

  private
  def self.init_context
    @rakefile_dir = Pathname.new(File.dirname(File.expand_path(__FILE__)).downcase)

    if ENV['MERLIN_ROOT'].nil?
      # Initialize the context for an external contributor who builds from
      # a non-MERLIN command prompt
      @source = @rakefile_dir
      @target = nil
    else
      # Initialize @source and @target to point to the right places based
      # on whether we are within MERLIN_ROOT or SVN_ROOT
      @merlin_root = Pathname.new(ENV['MERLIN_ROOT'].downcase) + '../../' # hack for changes in TFS layout
      @ruby_root = @merlin_root + 'merlin/main/languages/ruby'

      if @rakefile_dir == @ruby_root
        @source = @merlin_root
        @target = SVN_ROOT
      elsif @rakefile_dir == SVN_ROOT
        @source = @rakefile_dir
        @target = @merlin_root
      else
        raise <<-EOF
          Rakefile is at #{@rakefile_dir}. This is neither the SVN_ROOT nor
          the MERLIN_ROOT. Possible causes of this are running from a
          non-MERLIN command prompt (where MERLIN_ROOT environment variable
          is defined) or if the SVN_ROOT constant in the Rakefile does not
          point to where you downloaded the SVN repository for IronRuby.
        EOF
      end
    end

    @map = {}
    @initialized = true
  end

  def self.make_pathname(path)
    elements = path.split '/'
    raise "must be an an array with at least one element: #{elements}" if elements.length < 1
    result = Pathname.new elements.first
    (1..elements.length-1).each { |i| result += elements[i] }
    result
  end

  public
  def self.map(name, args)
    init_context unless @initialized
    @map[name] = Mapping.new(make_pathname(args[:merlin]), make_pathname(args[:svn]), (args[:recurse].nil? ? true : args[:recurse]))
  end

  def self.resolve(name)
    @map[name]
  end

  def self.is_merlin?
    @merlin_root == @source
  end

  def self.source_context(&b)
    context = CommandContext.new(:source, self)
    context.extend(is_merlin? ? SvnProvider : TfsProvider)
    context.instance_eval(&b)
    context
  end

  def self.target_context(&b)
    if @target.nil?
      raise <<-EOF
      Cannot invoke commands against target_context if you are not running in
      a MERLIN_ROOT context. External folks should never see this error as they
      should never be running commands that require moving things between
      different contexts.
      EOF
    else
      # Note that this is a bit unusual - the source control commands in the
      # target are identical to the source control commands for the source. This
      # is due to the semantics of the operation. The source is always
      # authoritative in these kinds of push scenarios, so you'll never want to
      # mutate the source repository, only the target repository.
      context = CommandContext.new(:target, self)
      context.extend(is_merlin? ? SvnProvider : TfsProvider)
      context.instance_eval(&b)
      context
    end
  end

  def self.source
    @source
  end

  def self.target
    @target
  end
end

class IronRuby < ProjectContext
  map :root, :merlin => 'merlin/main/languages/ruby', :svn => '.', :recurse => false
  map :gppg, :merlin => 'merlin/main/utilities/gppg', :svn => 'bin', :recurse => false
  map :dlr_core, :merlin => 'ndp/fx/src/core/microsoft/scripting', :svn => 'src/microsoft.scripting.core'
  map :dlr_libs, :merlin => 'merlin/main/runtime/microsoft.scripting', :svn => 'src/microsoft.scripting'
  map :dlr_com, :merlin => 'ndp/fx/src/dynamic/system/dynamic', :svn => 'src/dynamic'
  map :ironruby, :merlin => 'merlin/main/languages/ruby/ruby', :svn => 'src/ironruby'
  map :libraries, :merlin => 'merlin/main/languages/ruby/libraries.lca_restricted', :svn => 'src/IronRuby.Libraries'
  map :yaml, :merlin => 'merlin/external/languages/ironruby/yaml/ironruby.libraries.yaml', :svn => 'src/yaml'
  map :tests, :merlin => 'merlin/main/languages/ruby/tests', :svn => 'tests/ironruby'
  map :console, :merlin => 'merlin/main/languages/ruby/console', :svn => 'utils/ironruby.console'
  map :generator, :merlin => 'merlin/main/languages/ruby/classinitgenerator', :svn => 'utils/ironruby.classinitgenerator'
  map :test_runner, :merlin => 'merlin/main/languages/ruby/ironruby.tests', :svn => 'utils/IronRuby.Tests'
  map :scanner, :merlin => 'merlin/main/languages/ruby/utils/ironruby.libraries.scanner', :svn => 'utils/ironruby.libraries.scanner'
  map :build, :merlin => 'merlin/main/bin', :svn => 'build'
  map :stdlibs, :merlin => 'merlin/external/languages/ruby/redist-libs', :svn => 'lib'
  map :ironlibs, :merlin => 'merlin/main/languages/ruby/libs', :svn => 'lib/IronRuby'
  map :lang_root, :merlin => 'merlin/main', :svn => '.'
end

# Spec runner helpers

class MSpecRunner
  attr_accessor :files, :examples, :expectations, :failures, :errors, :summaries
  attr_reader :tempdir

  SUMMARY_PARSER = /(\d+) files?, (\d+) examples?, (\d+) expectations?, (\d+) failures?, (\d+) errors?/

  def initialize
    @files = 0
    @examples = 0
    @expectations = 0
    @failures = 0
    @errors = 0
    @summaries = []
    @tempdir = Dir.tmpdir
  end

  def regression(ruby, klass, method = nil)
    cmd = %Q{"#{UserEnvironment.mri_binary}" "#{UserEnvironment.mspec}/bin/mspec" ci -t #{ruby} -fm -V -B "#{UserEnvironment.config}" "#{UserEnvironment.rubyspec}/1.8/core/#{klass}"}
    cmd += "/#{method}_spec.rb" unless method.nil?
    cmd += " > #{tempdir}/out.txt"
    system cmd
    File.open("#{tempdir}/out.txt", 'r') do |f|
      lines = f.readlines
      lines.each do |line|
        if SUMMARY_PARSER =~ line
          @files += $1.to_i
          @examples += $2.to_i
          @expectations += $3.to_i
          @failures += $4.to_i
          @errors += $5.to_i
          @summaries << "#{klass}: #{$1.to_i} files, #{$2.to_i} examples, #{$3.to_i} expectations, #{$4.to_i} failures, #{$5.to_i} errors"
        end
      end
    end
  end

  def all_core(method, ruby)
    send method, ruby, '*'
  end

  def why_regression(ruby, klass, method = nil)
    cmd = %Q{#{UserEnvironment.mri_binary} "#{UserEnvironment.mspec}/bin/mspec" ci -t #{ruby} -fs -V -B "#{UserEnvironment.config}" "#{UserEnvironment.rubyspec}/1.8/core/#{klass}"}
    cmd += "/#{method}_spec.rb" unless method.nil?
    system cmd
  end

  def test(ruby, klass, method = nil)
    cmd = %Q{#{UserEnvironment.mri_binary} "#{UserEnvironment.mspec}/bin/mspec" run -t #{ruby} -Gcritical -V -fs -B "#{UserEnvironment.config}" "#{UserEnvironment.rubyspec}/1.8/core/#{klass}"}
    cmd += "/#{method}_spec.rb" unless method.nil?
    system cmd
  end

  def baseline(ruby, klass, method = nil)
    cmd = %Q{#{UserEnvironment.mri_binary} "#{UserEnvironment.mspec}/bin/mspec" tag -t #{ruby} -fs -V -Gcritical -B "#{UserEnvironment.config}" "#{UserEnvironment.rubyspec}/1.8/core/#{klass}"}
    cmd << "/#{method}_spec.rb" unless method.nil?
    system cmd
  end

  def generate_critical_tags
    lines = []
    return unless File.exist? "#{UserEnvironment.tags}\\critical_tags.txt"
    File.open("#{UserEnvironment.tags}\\critical_tags.txt", 'r') do |f|
      f.readlines.each do |line|
        file,_,tag,*desc = line.chomp.split(":")
        fulltag = tag << ":" << desc.join(":")
        filepath = "#{UserEnvironment.tags}/1.8/#{file}"
        filedir = File.dirname(filepath)
        FileUtils.mkdir_p(filedir) unless File.exist?(filedir)
        FileUtils.touch(filepath) unless File.exist?(filepath)
        File.open(filepath, File::RDWR) do |tagfile|
          if tagfile.readlines.grep(Regexp.new(Regexp.escape(tag))).empty?
            tagfile.puts fulltag.strip
          end
        end
      end
    end
  end

  def report
    summaries.each { |s| puts s }
    puts "\nSummary:\n"
    puts "#{summaries.length} types, #{files} files, #{examples} examples, #{expectations} expectations, #{failures} failures, #{errors} errors"
  end
end

class UserEnvironment

  # Find path to named executable

  def self.find_executable(executable)
    executable.downcase!
    result = []
    search_path = ENV['PATH'].split(File::PATH_SEPARATOR)
    search_path.each do |dir|
      path = Pathname.new(dir)
      file_path = path + executable
      result << file_path.dirname if file_path.file?
      file_path = path + (executable + '.exe')
      result << file_path.dirname if file_path.file?
    end
    result
  end

  def self.mri_binary
    self.mri + '/bin/ruby.exe'
  end

  def self.method_missing(sym, *args)
    name = sym.to_s
    if name =~ /\?$/
      File.exist?(self.send(name[0..-2]))
    elsif self.constants.include?(name.upcase)
      File.expand_path(UserEnvironment.const_get(name.upcase))
    else
      raise NoMethodError.new("undefined method '#{name}' for #{self}", name, args)
    end
  end

  def initialize
    path_to_config = ENV['HOME'] + '/.irconfig.rb'
    load path_to_config if File.exist? path_to_config

    unless defined?(UserEnvironment::MRI)
      ruby_exe_paths = UserEnvironment.find_executable 'ruby'
      unless ruby_exe_paths.empty?
        UserEnvironment.const_set(:MRI, Pathname.new(ruby_exe_paths.first + '\..\\'))
      else
        raise ArgumentError.new("Could not find ruby.exe on your path")
      end
    end
    UserEnvironment.const_set(:TAGS, "#{ENV['HOME']}\\dev\\ironruby-tags".gsub('\\', '/')) unless defined?(UserEnvironment::TAGS)
    UserEnvironment.const_set(:RUBYSPEC, "#{ENV['HOME']}\\dev\\rubyspec".gsub('\\', '/')) unless defined?(UserEnvironment::RUBYSPEC)
    UserEnvironment.const_set(:MSPEC, "#{ENV['HOME']}\\dev\\mspec".gsub('\\', '/')) unless defined?(UserEnvironment::MSPEC)
    UserEnvironment.const_set(:CONFIG, "#{ENV['HOME']}\\dev\\default.mspec".gsub('\\', '/')) unless defined?(UserEnvironment::CONFIG)
  end
end

UE = UserEnvironment.new
