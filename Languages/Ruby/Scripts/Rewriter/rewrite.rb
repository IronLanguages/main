require 'fileutils'
require 'tmpdir'
require 'mscorlib'
require 'corapi.dll'

SymStore = System::Diagnostics::SymbolStore
CorSymStore = Microsoft::Samples::Debugging::CorSymbolStore

def usage
  STDERR.puts 'Usage: rewrite [/cd:{assembly-dir}] {assembly-path}+ [@response-file] [/include:{included-il}] [/config:(DEBUG|RELEASE)] [/key:{snk-file}] [/out:{dir}]'
  exit(-1) 
end

dir = ""
ASSEMBLIES = []
while ARGV.size > 0
  option = ARGV.shift.strip
  if option[0] == ?#
    # skip
  elsif option[0] == ?@
    ARGV.insert(0, *File.readlines(option[1..-1]))
  elsif option =~ /^\/([a-z]+)(\:(.*))?/i
    case $1.upcase
      when "CD": dir = $3
      when "INCLUDE": INCLUDE = $3
      when "CONFIG": OPTIMIZE = $3.upcase.include?('RELEASE')
      when "KEY": SIGN_KEY = $3 if File.exist?($3)
      when "OUT": OUT = File.expand_path($3)
    else
      usage
    end
  else
    ASSEMBLIES << File.expand_path(File.join(dir, option)).strip
  end
end

usage if ASSEMBLIES.size == 0

DLR_ROOT = File.expand_path(File.join(File.dirname(__FILE__), "../../../.."))

#TODO: Mono?
ILDASM = File.join(DLR_ROOT, "Utilities/IL/ildasm.exe")
ILASM = File.join(ENV["FrameworkDir"], ENV["FrameworkVersion"], "ilasm.exe")

FileUtils.mkdir_p OUT if defined?(OUT) and not File.exist?(OUT)

(ASSEMBLIES + [ILDASM, ILASM]).each do |path|
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

class Rewriter
  def rewrite_file(src_path, dst_path, assembly)
    @symbol_reader = CorSymStore::SymbolAccess.get_reader_for_file(assembly)
    
    File.open(dst_path, "w+") do |dst|
      @dst = dst
      
      File.open(src_path, "r") do |src|
        while true
          line = src.gets
          break if line.nil?
          new_line = rewrite_line(line)
          dst.puts(new_line) unless new_line.nil?
        end  
      end
      
      if defined? INCLUDE
        dst.puts
        dst.puts %Q{#include "#{INCLUDE}"}
      end
    end
  end
  
  def rewrite_line(line)
    case line 
      #.method /*06000001*/ ...
      when %r{^\s*\.method \/\*([0-9A-Za-z]{8})\*\/}:
        method = @symbol_reader.GetMethod(SymStore::SymbolToken.new($1.hex));
        
        if method.nil?
          @offsets = nil
        else
          get_sequence_points(method)
        end
        
        @last_doc_url = nil
        
        line
    
      #IL_000a: ...
      when %r{(^\s*)IL_([0-9A-Za-z]{4})\:.*}:
        offset = $2.hex
        if @offsets and not (i = @offsets.index(offset)).nil?
          url = (@last_doc_url == @docs[i].URL) ? '' : @docs[i].URL
          @dst.puts("#{$1}.line #{@startRow[i]},#{@endRow[i]} : #{@startColumn[i]},#{@endColumn[i]} '#{url}'")
          @last_doc_url = @docs[i].URL
        end
        
        line
    
      else
        line
    end     
  end
  
  def get_sequence_points(method)
    count = method.sequence_point_count
    @offsets = System::Array[Fixnum].new(count)
    @docs = System::Array[SymStore::ISymbolDocument].new(count)
    @startColumn = @offsets.clone
    @endColumn = @offsets.clone
    @startRow = @offsets.clone
    @endRow = @offsets.clone
    
    method.get_sequence_points(@offsets, @docs, @startRow, @startColumn, @endRow, @endColumn);
  end
end
  
ASSEMBLIES.each do |assembly|
  puts

  assembly_filename = File.basename(assembly)
  assembly_dir = File.dirname(assembly)
  output_dir = OUT || assembly_dir
    
  using_tempdir do |temp_dir|
    puts temp_dir if $DEBUG
    Dir.chdir temp_dir
    
    il_file = assembly_filename + '.il'
    new_il_file = assembly_filename + '.new.il'
    res_file = assembly_filename + '.res'
    ext = File.extname(assembly_filename)
    pdb_file = File.basename(assembly_filename, ext) + '.pdb';
    
    puts "Disassembling '#{assembly}' ..."
    
    # don't use /LINENUM since ILDASM might crash
    run q(ILDASM), q(assembly), q("/OUT=#{il_file}"), "/TEXT /TOKENS"
    
    puts "Rewriting '#{il_file}' ..."
    
    Rewriter.new.rewrite_file(il_file, new_il_file, assembly)
    
    puts "Assembling '#{assembly_filename}' ..."
    
    run q(ILASM), q(new_il_file), q("/RESOURCE=#{res_file}"), q("/OUTPUT=#{assembly_filename}"), "/QUIET /#{ext[1..-1]}", 
      OPTIMIZE ? "/OPTIMIZE /FOLD /PDB" : "/DEBUG",
      defined?(SIGN_KEY) ? q("/KEY=#{SIGN_KEY}") : nil
  
    FileUtils.mv assembly_filename, output_dir
    FileUtils.mv pdb_file, output_dir
  end  
end

