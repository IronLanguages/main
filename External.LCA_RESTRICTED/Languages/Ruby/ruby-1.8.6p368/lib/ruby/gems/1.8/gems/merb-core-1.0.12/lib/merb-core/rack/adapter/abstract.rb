module Merb
  module System
    class PortablePoller
      def initialize(pid)
        @pid = pid
      end
      # Memory usage in kilobytes (resident set size)
      def memory
        ps_int('rss')
      end
      
      # Percentage memory usage
      def percent_memory
        ps_float('%mem')
      end
      
      # Percentage CPU usage
      def percent_cpu
        ps_float('%cpu')
      end
      
      private
      
      def ps_int(keyword)
        `ps -o #{keyword}= -p #{@pid}`.to_i
      end
      
      def ps_float(keyword)
        `ps -o #{keyword}= -p #{@pid}`.to_f
      end
      
      def ps_string(keyword)
        `ps -o #{keyword}= -p #{@pid}`.strip
      end
      
      def time_string_to_seconds(text)
        _, minutes, seconds, useconds = *text.match(/(\d+):(\d{2}).(\d{2})/)
        (minutes.to_i * 60) + seconds.to_i
      end
    end
  end
end

module Merb
  module Rack
    class AbstractAdapter

      # This method is designed to be overridden in a rack adapter.  It 
      # will be called to start a server created with the new_server method.
      # This is called from the AbstractAdapter start method.
      #
      # :api: plugin
      # @overridable
      def self.start_server
        raise NotImplemented
      end
      
      # This method is designed to be overridden in a rack adapter.  It will
      # be called to create a new instance of the server for the adapter to 
      # start.  The adapter should attempt to bind to a port at this point. 
      # This is called from the AbstractAdapter start method.
      #
      # ==== Parameters
      # port<Integer>:: The port the server should listen on
      #
      # :api: plugin
      # @overridable
      def self.new_server(port)
        raise NotImplemented
      end
            
      # This method is designed to be overridden in a rack adapter.  It will
      # be called to stop the adapter server.  
      #
      # ==== Parameters
      # status<Integer>:: The exit status the adapter should exit with. 
      #
      # ==== Returns
      # Boolean:: True if the server was properly stopped.  
      #
      # :api: plugin
      # @overridable
      def self.stop(status)
        raise NotImplemented
      end

      # Spawn a new worker process at a port.
      #
      # ==== Parameters
      # port<Integer>:: The port to start the worker process on. 
      #
      # :api: private
      def self.spawn_worker(port)
        worker_pid = Kernel.fork
        start_at_port(port, @opts) unless worker_pid

        # If we have a worker_pid, we're in the parent.
        throw(:new_worker) unless worker_pid

        @pids[port] = worker_pid
        $WORKERS = @pids.values
      end

      # The main start method for bootloaders that support forking.
      # This method launches the adapters which inherit using the 
      # new_server and start_server methods.  This method should not
      # be overridden in adapters which want to fork.  
      #
      # ==== Parameters
      # opts<Hash>:: A hash of options
      #   socket: the socket to bind to
      #   port: the port to bind to
      #   cluster: the number 
      #
      # :api: private
      def self.start(opts={})
        @opts = opts
        $WORKERS ||= []
        parent = nil

        @pids = {}
        port = (opts[:socket] || opts[:port]).to_i
        max_port = Merb::Config[:cluster] ? Merb::Config[:cluster] - 1 : 0

        # If we only have a single merb, just start it up and dispense with
        # the spawner/worker setup.
        if max_port == 0
          start_at_port(port)
          return
        end

        $0 = process_title(:spawner, port)

        # For each port, spawn a new worker. The parent will continue in
        # the loop, while the worker will throw :new_worker and be booted
        # out of the loop.
        catch(:new_worker) do
          0.upto(max_port) do |i|
            parent = spawn_worker(port + i)
          end
        end

        # If we're in a worker, we're done. Otherwise, we've completed
        # setting up workers and now need to watch them.
        return unless parent

        # For each worker, set up a thread in the spawner to watch it
        0.upto(max_port) do |i|
          Thread.new do
            catch(:new_worker) do
              loop do
                pid, status = @pids[port + i], nil
                poller = Merb::System::PortablePoller.new(pid)
                begin
                  tick = 1
                  loop do                    
                    # Watch for the pid to exit.
                    _, status = Process.wait2(pid, Process::WNOHANG)
                    break if status
                    
                    if (tick % 120 == 0) && Merb::Config[:max_memory] && poller.memory > Merb::Config[:max_memory]
                      tick = 1
                      Process.kill("INT", pid)
                      if (Process.kill(0, pid) rescue false)
                        sleep Merb::Config[:hang_time] || 5
                        Process.kill(9, pid)
                        Process.wait2(pid) if (Process.kill(0, pid) rescue false)
                      end
                      
                      status = Struct.new(:exitstatus).new(nil)
                      break
                    end
                    tick += 1
                    sleep 0.25
                  end

                  # If the pid doesn't exist, we want to silently exit instead of
                  # raising here.
                rescue SystemCallError => e
                ensure
                  # If there was no worker with that PID, the status was non-0
                  # (we send back a status of 128 when ABRT is called on a 
                  # worker, and Merb.fatal! exits with a status of 1), or if
                  # Merb is in the process of exiting, *then* don't respawn.
                  # Note that processes killed with kill -9 will return no
                  # exitstatus, and we respawn them.
                  if !status || 
                    (status.exitstatus && status.exitstatus != 0) || 
                    Merb.exiting then
                    Thread.exit
                  end
                end

                # Otherwise, respawn the worker, and watch it again.
                spawn_worker(port + i)
              end
            end
          end
        end

        # The spawner process will make it here, and when it does, it should just 
        # sleep so it can pick up ctrl-c if it's in console mode.
        sleep

      end

      # Fork a server on the specified port and start the app.
      #
      # ==== Parameters
      # port<Integer>:: The port to start the server on
      # opts<Hash>:: The hash of options, defaults to the @opts 
      #   instance variable.  
      #
      # :api: private
      def self.start_at_port(port, opts = @opts)
        at_exit do
          Merb::Server.remove_pid(port)
        end

        Merb::Worker.start unless Merb.testing?

        # If Merb is daemonized, trap INT. If it's not daemonized,
        # we let the master process' ctrl-c control the cluster
        # of workers.
        if Merb::Config[:daemonize]
          Merb.trap('INT') do
            Merb.exiting = true
            stop
            Merb.logger.warn! "Exiting port #{port}\n"
            exit_process
          end
          # If it was not fork_for_class_load, we already set up
          # ctrl-c handlers in the master thread.
        elsif Merb::Config[:fork_for_class_load]
          if Merb::Config[:console_trap]
            Merb::Server.add_irb_trap
          end
        end

        # In daemonized mode or not, support HUPing the process to
        # restart it.
        Merb.trap('HUP') do
          Merb.exiting = true
          stop
          Merb.logger.warn! "Exiting port #{port} on #{Process.pid}\n"
          exit_process
        end

        # ABRTing the process will kill it, and it will not be respawned.
        Merb.trap('ABRT') do
          Merb.exiting = true
          stopped = stop(128)
          Merb.logger.warn! "Exiting port #{port}\n" if stopped
          exit_process(128)
        end

        # Each worker gets its own `ps' name.
        $0 = process_title(:worker, port)

        # Store the PID for this worker
        Merb::Server.store_pid(port)

        Merb::Config[:log_delimiter] = "#{process_title(:worker, port)} ~ "

        Merb.reset_logger!
        Merb.logger.warn!("Starting #{self.name.split("::").last} at port #{port}")

        # If we can't connect to the port, keep trying until we can. Print
        # a warning about this once. Try every 0.25s.
        printed_warning = false
        loop do
          begin
            # Call the adapter's new_server method, which should attempt
            # to bind to a port.
            new_server(port)
          rescue Errno::EADDRINUSE => e
            if Merb::Config[:bind_fail_fatal]
              Merb.fatal! "Could not bind to #{port}. It was already in use", e
            end
            
            unless printed_warning
              Merb.logger.warn! "Port #{port} is in use, " \
                "Waiting for it to become available."
              printed_warning = true
            end

            sleep 0.25
            next
          end
          break
        end

        Merb.logger.warn! "Successfully bound to port #{port}"

        Merb::Server.change_privilege

        # Call the adapter's start_server method.
        start_server
      end

      # Exit the process with the specified status.  
      #
      # ==== Parameters
      # status<Integer>:: The exit code of the process.
      # 
      # :api: private
      def self.exit_process(status = 0)
        exit(status)
      end

      # Set the process title.
      #
      # ==== Parameters
      # whoami<Symbol>:: Either :spawner for the master process or :worker for any of the worker
      #   processes. 
      # port<Integer>:: The base port that the app is running on. 
      #
      # :api: private
      def self.process_title(whoami, port)
        name = Merb::Config[:name]
        app  = "merb#{" : #{name}" if (name && name != "merb")}"
        max_port  = Merb::Config[:cluster] ? (Merb::Config[:cluster] - 1) : 0
        numbers   = ((whoami != :worker) && (max_port > 0)) ? "#{port}..#{port + max_port}" : port
        file      = Merb::Config[:socket_file] % port if Merb::Config[:socket_file]
        
        listening_on = if Merb::Config[:socket]
          "socket#{'s' if max_port > 0 && whoami != :worker} #{numbers} "\
          "#{file ? file : "#{Merb.log_path}/#{name}.#{port}.sock"}"
        else
          "port#{'s' if max_port > 0 && whoami != :worker} #{port}"
        end
        "#{app} : #{whoami} (#{listening_on})"
      end
    end
  end
end
