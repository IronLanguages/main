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
EXCLUDED_EXTENSIONS   = %w[.old .suo .vspscc .vssscc .user .log .pdb .cache .swp]
EXCLUDED_FILES = "dirs.proj makefile sources .gitignore"
DEFAULT_ROBOCOPY_OPTIONS = "/XF *#{EXCLUDED_EXTENSIONS.join(' *')} #{EXCLUDED_FILES} /XD .git /NP /COPY:DAT /A-:R "
CURRENT_DIR = File.dirname(File.expand_path(__FILE__))

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

  
  def configuration
    ENV['configuration'] ? ENV['configuration'].to_sym : :debug
  end

  def platform
    ENV['platform'].nil? ? :windows : ENV['platform'].to_sym
  end

  def clr
    unless ENV['mono']
      ENV['clr'] ? ENV['clr'].to_sym : :desktop
    else
      :mono
    end
  end

  def build_path
    project_root + Pathname.new(File.join("Bin", "#{clr == :desktop ? configuration : "#{clr}_#{configuration}"}"))
  end

  def project_root
    Pathname.new(File.expand_path(File.join(CURRENT_DIR, "..",".."))) 
  end
end

class CSProjCompiler
  include IronRubyUtils
  def initialize(&b)
    @targets = {}
    instance_eval(&b)
  end

  def dir(target)
    Pathname.new(@targets[target][:dir])
  end

  def compile(name)
    banner name.to_s
    args = @targets[name.to_sym]
    working_dir = File.expand_path(args[:dir])
    build_dir = build_path

    Dir.chdir(working_dir) do |p|
      cs_args = ["out:\"#{build_dir + args[:output]}\""]
      cs_args += references(args[:references], working_dir).map { |ref| "r:\"#{ref}\"" }
      cs_args += compiler_switches
      cs_args += args[:switches] if args[:switches]

      if args[:resources]
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


  def resgen(base_path, resource_map)
    base_path = Pathname.new(base_path)
    resource_map.each_pair do |input, output|
      exec %Q{resgen "#{base_path + input.dup}" "#{build_path + output}"}
    end
  end


  def resolve_framework_path(file)
    Configuration.resolve_framework_path(clr, file)
  end


  def compiler_switches
    Configuration.get_switches(clr, configuration)
  end

  def references(refs, working_dir)
    references = Configuration.get_references(clr)
    refs.each do |ref|
      references << if ref =~ /^\!/
        resolve_framework_path(ref[1..-1])
      else
        (build_path + ref).relative_path_from(working_dir)
      end
    end if refs
    references
  end

  
  def clean
    FileUtils.rm_rf build_path
    FileUtils.mkdir_p build_path
  end

  def transform_config(source_path, target_path, paths)
    file = File.new source_path
    doc = Document.new file

    # replace LibraryPaths
    options = XPath.each(doc, '/configuration/microsoft.scripting/options/set') do |node|
      if node.attributes['language'] == 'Ruby' && node.attributes['option'] == 'LibraryPaths'
        node.attributes['value'] = paths
      end
    end

    File.open(target_path, 'w+') do |f|
      f.write doc.to_s
    end
  end

  def transform_config_file(configuration, source_path, target_build_path)
    # signing is on for IronRuby in Merlin, off for SVN and Binary
    layout = {'Merlin' => { :LibraryPaths => '..\..\Languages\Ruby\libs;..\..\..\External.LCA_RESTRICTED\Languages\Ruby\Ruby-1.8.6p287\lib\ruby\site_ruby\1.8;..\..\..\External.LCA_RESTRICTED\Languages\Ruby\Ruby-1.8.6p287\lib\ruby\site_ruby;..\..\..\External.LCA_RESTRICTED\Languages\Ruby\Ruby-1.8.6p287\lib\ruby\1.8' }, 
              'Binary' => { :LibraryPaths => '..\lib\IronRuby;..\lib\ruby\site_ruby\1.8;..\lib\ruby\site_ruby;..\lib\ruby\1.8' } }
    
    transform_config source_path, target_build_path, layout[configuration][:LibraryPaths]
  end

  def move_config
    source = project_root + "Config\\Unsigned\\app.config"
    config_file = build_path + "ir.exe.config"
    transform_config_file('Merlin', source, config_file)
  end

  def method_missing(name, *args)
    @targets[name.to_sym] = *args
  end
end

IronRubyCompiler = CSProjCompiler.new do
  dlr_core :references => ['!System.dll','!System.Configuration.dll','Microsoft.Scripting.ExtensionAttribute.dll'], 
           :switches   => ['target:library', 'define:MICROSOFT_SCRIPTING_CORE'],
           :output     => 'Microsoft.Scripting.Core.dll',
           :csproj     => 'Microsoft.Scripting.Core.csproj',
           :dir        => '../../../../ndp/fx/src/Core/Microsoft/Scripting'

  dlr_extension :references => ['!System.dll'],
                :switches   => ['target:library'],
                :output     => 'Microsoft.Scripting.ExtensionAttribute.dll',
                :csproj     => 'Microsoft.Scripting.ExtensionAttribute.csproj',
                :dir        => '../../../../ndp/fx/src/Core/Microsoft/Scripting'
  dlr_libs  :references => ['Microsoft.Scripting.Core.dll', '!System.Xml.dll', '!System.dll', '!System.Configuration.dll', 'Microsoft.Scripting.ExtensionAttribute.dll','!System.Runtime.Remoting.dll'],
            :switches   => ['target:library'], 
            :resources  => {Pathname.new('Math') + 'MathResources.resx' => Pathname.new('Microsoft.Scripting.Math.MathResources.resources')}, 
            :output     => 'Microsoft.Scripting.dll', 
            :csproj     => 'Microsoft.Scripting.csproj',
            :dir        => '../../Runtime/Microsoft.Scripting'
  dlr_com :references   => ['Microsoft.Scripting.Core.dll', '!System.Xml.dll', '!System.dll', 'Microsoft.Scripting.ExtensionAttribute.dll'],
          :switches     => ['target:library', 'unsafe'],
          :output       => 'Microsoft.Dynamic.dll',
          :dir          => '../../../../ndp/fx/src/Dynamic/System/Dynamic'
  generator :references => ['Microsoft.Scripting.Core.dll', 'Microsoft.Scripting.dll','Microsoft.Scripting.ExtensionAttribute.dll', 'IronRuby.dll', '!System.dll'],
            :output     => 'ClassInitGenerator.exe',
            :dir        => './ClassInitGenerator'
  ironruby :references => ['Microsoft.Scripting.Core.dll', 'Microsoft.Scripting.dll', 'Microsoft.Scripting.ExtensionAttribute.dll','!System.dll', '!System.Configuration.dll'],
           :switches   => ['target:library'],
           :output     => 'IronRuby.dll',
           :dir        => './Ruby',
           :csproj     => 'Ruby.csproj'
  libraries :references => ['Microsoft.Scripting.Core.dll', 'Microsoft.Scripting.dll', 'Microsoft.Scripting.ExtensionAttribute.dll', 'IronRuby.dll', '!System.dll'],
            :switches   => ['target:library'],
            :output     => 'IronRuby.Libraries.dll',
            :dir        => 'Libraries.LCA_RESTRICTED',
            :csproj     => 'IronRuby.Libraries.csproj'
  console :references => ['Microsoft.Scripting.Core.dll','Microsoft.Scripting.dll','IronRuby.dll'],
          :output     => 'ir.exe',
          :dir        => './Console',
          :csproj     => 'Ruby.Console.csproj'
  test_runner :references => ['Microsoft.Scripting.Core.dll', 'Microsoft.Scripting.dll', 'IronRuby.dll', 'IronRuby.Libraries.dll', '!System.dll', '!System.Windows.Forms.dll'],
              :output     => 'IronRuby.Tests.exe',
              :dir        => './IronRuby.Tests',
              :csproj     => 'IronRuby.Tests.csproj'
  scanner :references => ['Microsoft.Scripting.Core.dll', 'Microsoft.Scripting.dll', 'IronRuby.dll', 'IronRuby.Libraries.dll', '!System.Core.dll'],
          :output     => 'IronRuby.Libraries.Scanner.exe',
          :dir        => './IronRuby.Libraries.Scanner'
  yaml :references => ['Microsoft.Scripting.Core.dll', 'Microsoft.Scripting.dll', 'IronRuby.dll', 'IronRuby.Libraries.dll', '!System.dll'],
       :switches   => ['target:library'],
       :output     => 'IronRuby.Libraries.Yaml.dll',
       :dir        => '../../../External.LCA_RESTRICTED/Languages/IronRuby/Yaml/IronRuby.Libraries.Yaml',
       :csproj     => 'IronRuby.Libraries.Yaml.csproj'
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
    cmd = %Q{"#{UserEnvironment.mri_binary}" "#{UserEnvironment.mspec}/bin/mspec" ci -t #{ruby} -fm -V -B "#{UserEnvironment.config}" "#{UserEnvironment.rubyspec}/core/#{klass}"}
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
    cmd = %Q{#{UserEnvironment.mri_binary} "#{UserEnvironment.mspec}/bin/mspec" ci -t #{ruby} -fs -V -B "#{UserEnvironment.config}" "#{UserEnvironment.rubyspec}/core/#{klass}"}
    cmd += "/#{method}_spec.rb" unless method.nil?
    system cmd
  end

  def test(ruby, klass, method = nil)
    cmd = %Q{#{UserEnvironment.mri_binary} "#{UserEnvironment.mspec}/bin/mspec" run -t #{ruby} -Gcritical -V -fs -B "#{UserEnvironment.config}" "#{UserEnvironment.rubyspec}/core/#{klass}"}
    cmd += "/#{method}_spec.rb" unless method.nil?
    system cmd
  end

  def baseline(ruby, klass, method = nil)
    cmd = %Q{#{UserEnvironment.mri_binary} "#{UserEnvironment.mspec}/bin/mspec" tag -t #{ruby} -fs -V -Gcritical -B "#{UserEnvironment.config}" "#{UserEnvironment.rubyspec}/core/#{klass}"}
    cmd << "/#{method}_spec.rb" unless method.nil?
    system cmd
  end

  def generate_critical_tags
    lines = []
    return unless File.exist? "#{UserEnvironment.tags}\\critical_tags.txt"
    File.open("#{UserEnvironment.tags}\\critical_tags.txt", 'r') do |f|
      f.readlines.each do |line|
        file,tag,*desc = line.chomp.split(":")
        fulltag = tag << ":" << desc.join(":")
        filepath = "#{UserEnvironment.tags}/#{file}"
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
      next if dir.empty?
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
