require 'uri'
require 'net/http'
require 'test_driver/process'

class ChironHelper < ProcessWrapper
  def initialize(uri)
    @uri = uri
  end

  def start
    info "Starting Chiron"
    TestDriver::ITERATIONS.times do |i|
      start_helper
      sleep 5
      if running?
        debug 'Chiron started'
        return true
      elsif i == (ITERATIONS - 1)
        fatal 'Tried starting on Chiron on many ports, aborting'
        exit(1)
      else
        debug 'Timeout: trying on another port'
        @uri.port += 1
      end
    end
  end

  def running?
    # make sure process is running
    return false unless actual_process

    # and make sure a request works
    begin
      req = Net::HTTP.start(@uri.host, @uri.port){|http| http.get('/')}
      return false if req.code.to_i != 200
    rescue Errno::ECONNREFUSED
      return false
    end

    return true
  end

  def stop
    info "Stopping Chiron"
    if actual_process
      __stop
      debug "Chiron stopped"
    else
      warn "Chiron not running"
    end
  end

  def start_helper
    build_config, path = TestConfig.build_config
    debug "Starting web server with \"#{build_config}\" configuration "
    chiron = File.join(path, "Chiron.exe")
    chiron_args = "/d:#{TestConfig.current.tests_dir} /w:#{@uri.port}"
    chiron_args << ' /n /s' unless log.debug?
    __start chiron, chiron_args
  end
  
  def zip_directory(dir_path)
    zip_path = "#{dir_path}.zip"
    build_config, path = TestConfig.build_config
    debug "Generating \"#{zip_path}\" from \"#{dir_path}\" directory."
    chiron = File.join(path, "Chiron.exe")
    chiron_args = "/d:#{to_dos_path dir_path} /x:#{to_dos_path zip_path} /s"
    __start chiron, chiron_args
    unless __wait_for_exit
      raise "Failed to generate \"#{zip_path}\" from \"#{dir_path}\" directory."
    end
  end

private
  def to_dos_path str
    str.gsub('/', '\\')
  end
end