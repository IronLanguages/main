require 'uri'
require 'net/http'
require 'test_driver/process'

class ChironHelper < ProcessWrapper
  def initialize(uri)
    @uri = uri
  end

  def start
    log.info "Starting Chiron"
    TestDriver::ITERATIONS.times do |i|
      start_helper
      sleep 5
      if running?
        log.debug 'Chiron started'
        return true
      elsif i == (ITERATIONS - 1)
        log.fatal 'Tried starting on Chiron on many ports, aborting'
        exit(1)
      else
        log.debug 'Timeout: trying on another port'
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
    log.info "Stopping Chiron"
    if actual_process
      __stop
      log.debug "Chiron stopped"
    else
      log.warn "Chiron not running"
    end
  end

  def start_helper
    build_config, path = TestConfig.build_config
    log.debug "Starting web server with \"#{build_config}\" configuration "
    chiron = File.join(path, "Chiron.exe")
    chiron_args = "/d:#{TestConfig.current.tests_dir} /w:#{@uri.port}"
    #f = File.open('./chiron.log', 'w')
    #$p = Process.create 'app_name' => chiron, 'startup_info' => {'stdout' => f}
    __start chiron, chiron_args
  end
  
end