include System::IO

class DiskFile
  attr_reader :size, :name, :path, :parent
  attr_accessor :drawing
  def initialize(name, size, parent)
    @name = name
    @size = size
    @parent = parent
    @drawing = nil
    if parent
      @path = File.join(parent.path, @name)
    else
      @path = @name
    end
  end

  def <=>(other)
    @size <=> other.size
  end
end

class DiskDir < DiskFile
  attr_reader :files
  def initialize(name, size, parent)
    super
    @path += '\\'
    @files = []
    @loaded = false
  end

  def load
    unless @loaded
      @files = []
      @size = 0
      begin
        dir_info = DirectoryInfo.new(@path)

        dir_info.get_files.each do |a_file|
          @files << DiskFile.new(a_file.name, a_file.length, self)
          @size += a_file.length
        end

        dir_info.get_directories.each do |a_dir| 
          dir_size = self.class.get_dir_size(a_dir) 
          @files << DiskDir.new(a_dir.Name, dir_size, self)
          @size += dir_size
        end

        @files.sort!
        @files.reverse!
        @loaded = true
      rescue Exception => e
        $stderr.write("Error [#{e.message}s]")
        $stderr.flush
        @files = []
      end
    end
  end

  def size
    @size ||= get_dir_size( DirectoryInfo.new(@path))
    @size
  end

  def self.get_dir_size(dirinfo)
    size = 0
    begin
      dirinfo.get_files.each do |afile|
        size += afile.length
      end

      dirinfo.get_directories.each do |adir|
        size += get_dir_size(adir)
      end
    rescue Exception => e
      $stderr.write("Warning [#{e.message}s]")
      $stderr.flush
    end
    size
  end
end
