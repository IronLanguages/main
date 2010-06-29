def load_rspec
  require 'rubygems'
  require 'spec'
end
  
def load_constants
  $BUILD ||= "Silverlight3#{ARGV.first || "Debug"}"
  $BUILD_PATH ||= File.join(File.expand_path(ENV['DLR_ROOT']), "bin", $BUILD)
  $CHIRON ||= File.join($BUILD_PATH, "Chiron.exe")
  $DIR ||= File.join(File.expand_path(File.dirname(__FILE__)), "fixtures")
end
  
def load_all
  load_constants
  load_rspec
end

module ChironSpecHelper 
  def chiron(*a)
    c = Chiron.new(*a)
    c.start
    sleep 1
    yield c if block_given?
    c.kill
    c.output
  end
  
  def http_get(uri, path)
    require 'net/http'
    require 'uri'
    url = URI.parse(uri)
    Net::HTTP.start(url.host, url.port) {|http| http.get(path) }
  end
  
  class Chiron
    attr_reader :output, :process
  
    def initialize(*flags)
      @arguments = 
        flags.inject([]) do |fs, f|
          fs << if f.kind_of?(Hash)
            f.inject([]) do |args, (flag, value)|
              args << "-#{flag}#{":#{value}" if value}"
            end.join(' ')
          else
            "-#{f}"
          end
        end.join(' ')
    end
  
    def start
      @process = System::Diagnostics::Process.new
      @process.start_info.file_name = $CHIRON
      @process.start_info.use_shell_execute = false
      @process.start_info.redirect_standard_output = true
      @process.start_info.working_directory = $DIR.gsub('/', '\\')
      @process.start_info.arguments = @arguments
      @output = ''
      @process.output_data_received do |s, e| 
        @output << "#{e.data}\n" if e.data
      end
      @process.start
      @process.begin_output_read_line
      nil
    end
    
    def kill
      unless @process.wait_for_exit(1)
        @process.kill
        unless @process.wait_for_exit(10)
          raise "Chiron failed to quit"
        end
      end
      @process.close
    end
  end
end