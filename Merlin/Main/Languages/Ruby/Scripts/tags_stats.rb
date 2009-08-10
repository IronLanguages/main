require "pathname"
require "optparse"

class TagsStats

  def init_options(args)
    @root_tags_folder = File.expand_path "."
    @min = 7
    @highlight = 100
    
    OptionParser.new do |opts|
      opts.banner = "Usage: tags_stats.rb <root_tags_folder> -files -"

      opts.separator ""
      opts.separator "Specific options:"

      opts.on("-t", "--tags [TAGSFOLDER]", "the root tags folder to analyze") do |t|
        @root_tags_folder = File.expand_path t
      end

      opts.on("-m", "--minimum [MIN]", Integer, "minimum threshold of tags for an entry to be printed") do |min|
        @min = min
      end
      opts.on("-h", "--highlight [HIGHLIGHT]", Integer, "minimum threshold of tags for an entry to be printed") do |h|
        @highlight = h
      end
    end.parse!
  end

  def initialize(args)
    init_options(args)
    @root_tags_pathname = Pathname.new @root_tags_folder
    @indent_level = 0
  end

  def print_lines(lines, path)
    rel_path = Pathname.new(path).relative_path_from(@root_tags_pathname)
    if lines > @min
      highlight = " " + ((lines > @highlight) ? ("<" * 30) : "")
      puts ("   " * @indent_level) + "#{lines} #{rel_path}" + highlight
    end
  end
    
  def scan_tags_file(tags_file)
    lines = File.readlines(tags_file)
    lines = lines.delete_if { |line| line.match(/\w/) == nil }
    print_lines lines.size, tags_file
    lines.size
  end
  
  def walk_dir(tags_folder)
    @indent_level += 1
    
    lines = 0
    Dir.new(tags_folder).each do |f| 
      next if f == "." or f == ".."
      path = File.expand_path f, tags_folder
      if File.directory? path
        lines += walk_dir(path)
        next
      end
      lines += scan_tags_file path
    end
    
    @indent_level -= 1

    print_lines lines, tags_folder
    lines
  end
  
  def stats
    walk_dir @root_tags_folder
  end
end

if $0 == __FILE__
  TagsStats.new(ARGV).stats
end