require 'fileutils'
require 'tmpdir'

def usage
  STDERR.puts 'Usage: rewrite {assembly-path} [/include:{included-il}] [/config:(DEBUG|RELEASE)] [/sign:{key}]'
  exit(-1) 
end

ARGV.each_with_index do |option,index|
  if option =~ /\/([a-z]+)(\:(.*))?/i
    case $1.upcase
      when "INCLUDE": INCLUDE = $3
      when "CONFIG": OPTIMIZE = $3.upcase.include?('RELEASE')
      when "SIGN": SIGN_KEY = $3
    else
      usage
    end
  elsif index == 0
    ASSEMBLY = File.expand_path(option)
  end
end

usage if not defined? ASSEMBLY

#TODO: Mono?
ASSEMBLY_FILENAME = File.basename(ASSEMBLY)
ASSEMBLY_DIR = File.dirname(ASSEMBLY)
MERLIN_ROOT = File.expand_path(File.join(File.dirname(__FILE__), "../../.."))
ILDASM = File.join(MERLIN_ROOT, "Utilities/IL/ildasm.exe")
ILASM = File.join(ENV["FrameworkDir"], ENV["FrameworkVersion"], "ilasm.exe")

[ASSEMBLY, ILDASM, ILASM].each do |path|
  next if File.exist?(path)
  STDERR.puts "File #{path} not found"
  exit(-2)
end

def run exe, *args
  command = "#{exe} #{args.join(' ')}"
  puts command if $DEBUG
  output = `#{command}`
  if $?.exitstatus != 0
    STDERR.puts command
    STDERR.puts
    STDERR.puts output
    exit($?.exitstatus)
  end  
end

def using_tempdir 
  temp = Dir.tmpdir
  i = $$
  begin
    begin
      dir = File.join(temp, "IR.#{Time.now.strftime("%Y.%m.%d-%I.%M.%S")}.#{i}")
    end while File.exists? dir
    Dir.mkdir dir
  rescue SystemCallError
    retry if File.exists? dir
    raise
  end 
  begin
    yield dir
  ensure
    FileUtils.rm_rf dir unless $DEBUG
  end
end

def q str
  '"' + str + '"'
end

def rewrite path
  if defined? INCLUDE
    File.open(path, "a") do |il|
      il.puts
      il.puts %Q{#include "#{INCLUDE}"}
    end
  end  
end

using_tempdir do |temp_dir|
  puts temp_dir if $DEBUG
  Dir.chdir temp_dir
  
  il_file = ASSEMBLY_FILENAME + '.il'
  res_file = ASSEMBLY_FILENAME + '.res'
  ext = File.extname(ASSEMBLY_FILENAME)
  pdb_file = File.basename(ASSEMBLY_FILENAME, ext) + '.pdb';
  
  run q(ILDASM), q(ASSEMBLY), q("/OUT=#{il_file}"), "/TEXT /LINENUM"
  
  rewrite il_file
  
  run q(ILASM), q(il_file), q("/RESOURCE=#{res_file}"), q("/OUTPUT=#{ASSEMBLY_FILENAME}"), "/QUIET /#{ext[1..-1]}", 
    OPTIMIZE ? "/OPTIMIZE /FOLD /PDB" : "/DEBUG",
    defined?(SIGN_KEY) ? q("/KEY=#{SIGN_KEY}") : nil
  
  FileUtils.mv ASSEMBLY_FILENAME, ASSEMBLY_DIR
  FileUtils.mv pdb_file, ASSEMBLY_DIR
end



