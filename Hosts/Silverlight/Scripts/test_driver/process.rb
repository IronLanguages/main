begin
  require 'test_driver/logger'
rescue LoadError
end

class ProcessWrapper
  include TestLogger if defined? TestLogger

  def actual_process
    @process
  end

  def __exist?(path)
    File.exist?(path)
  end

  def __start(path, args = nil)
    __stop
    @process = ::System::Diagnostics::Process.new
    @process.start_info.use_shell_execute = false
    @process.start_info.arguments = args if args
    @process.start_info.file_name = path.gsub('/', "\\")
    @process.start_info.create_no_window = false
    # # BUG: don't redirect output, as either Chiron or Firefox 
    # # slow down drastically
    # @process.start_info.redirect_standard_output = true
    # @process.start_info.redirect_standard_error = true
    @process.start
    @process_id = @process.id
  end

  def __stop
    if @process && !@process.has_exited
      begin
        @process.close_main_window
        @process.kill unless @process.has_exited
      rescue System::InvalidOperationException
        @process.terminate unless @process.has_exited
      ensure
        @process.dispose
        @process.close
        @process = nil
        __ensure_stop
      end
    end
  end
  
  def __ensure_stop
    if `tasklist`.split("\n").grep(/#{@process_id}/).size > 0
      `TASKKILL /PID #{@process_id} /T`
      @process_id = nil
    end
  end
  
  def __wait_for_exit
    @process.has_exited || @process.wait_for_exit(10_000)
  end
end