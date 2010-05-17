require 'etc'

module Merb

  # Server encapsulates the management of Merb daemons.
  class Server
    class << self

      # Start a Merb server, in either foreground, daemonized or cluster mode.
      #
      # ==== Parameters
      # port<~to_i>::
      #   The port to which the first server instance should bind to.
      #   Subsequent server instances bind to the immediately following ports.
      # cluster<~to_i>::
      #   Number of servers to run in a cluster.
      #
      # ==== Alternatives
      # If cluster is left out, then one process will be started. This process
      # will be daemonized if Merb::Config[:daemonize] is true.
      #
      # :api: private
      def start(port, cluster=nil)

        @port = port
        @cluster = cluster

        if Merb::Config[:daemonize]
          pidfile = pid_file(port)
          pid = File.read(pidfile).chomp.to_i if File.exist?(pidfile)

          unless alive?(@port)
            remove_pid_file(@port)
            Merb.logger.warn! "Daemonizing..." if Merb::Config[:verbose]
            daemonize(@port)
          else
            Merb.fatal! "Merb is already running on port #{port}.\n" \
              "\e[0m   \e[1;31;47mpid file: \e[34;47m#{pidfile}" \
              "\e[1;31;47m, process id is \e[34;47m#{pid}."
          end
        else
          bootup
        end
      end

      # ==== Parameters
      # port<~to_s>:: The port to check for Merb instances on.
      #
      # ==== Returns
      # Boolean::
      #   True if Merb is running on the specified port.
      #
      # :api: private
      def alive?(port)
        pidfile = pid_file(port)
        pid     = pid_in_file(pidfile)
        Process.kill(0, pid)
        true
      rescue Errno::ESRCH, Errno::ENOENT
        false
      rescue Errno::EACCES => e
        Merb.fatal!("You don't have access to the PID file at #{pidfile}: #{e.message}")
      end

      # :api: private
      def pid_in_file(pidfile)
        File.read(pidfile).chomp.to_i
      end

      # ==== Parameters
      # port<~to_s>:: The port of the Merb process to kill.
      # sig<~to_s>:: The signal to send to the process, the default is 9 - SIGKILL.
      #
      # No    Name         Default Action       Description
      # 1     SIGHUP       terminate process    terminal line hangup
      # 2     SIGINT       terminate process    interrupt program
      # 3     SIGQUIT      create core image    quit program
      # 4     SIGILL       create core image    illegal instruction
      # 9     SIGKILL      terminate process    kill program
      # 15    SIGTERM      terminate process    software termination signal
      # 30    SIGUSR1      terminate process    User defined signal 1
      # 31    SIGUSR2      terminate process    User defined signal 2
      #
      # ==== Alternatives
      # If you pass "all" as the port, the signal will be sent to all Merb processes.
      #
      # :api: private
      def kill(port, sig = "INT")
        if sig.is_a?(Integer)
          sig = Signal.list.invert[sig]
        end
        
        Merb::BootLoader::BuildFramework.run

        # If we kill the master, then the workers should be reaped also.
        if %w(main master all).include?(port)
          # If a graceful exit is requested then send INT to the master process.
          #
          # Otherwise read pids from pid files and try to kill each process in turn.
          kill_pid(sig, pid_file("main")) if sig == "INT"
        else
          kill_pid(sig, pid_file(port))
        end
      end

      # Sends the provided signal to the process pointed at by the provided pid file.
      # :api: private
      def kill_pid(sig, file)
        begin
          pid = pid_in_file(file)
          Merb.logger.fatal! "Killing pid #{pid} with #{sig}"
          Process.kill(sig, pid)
          FileUtils.rm(file) if File.exist?(file)
        rescue Errno::EINVAL
          Merb.logger.fatal! "Failed to kill PID #{pid} with #{sig}: '#{sig}' is an invalid " \
            "or unsupported signal number."
        rescue Errno::EPERM
          Merb.logger.fatal! "Failed to kill PID #{pid} with #{sig}: Insufficient permissions."
        rescue Errno::ESRCH
          FileUtils.rm file
          Merb.logger.fatal! "Failed to kill PID #{pid} with #{sig}: Process is " \
            "deceased or zombie."
        rescue Errno::EACCES => e
          Merb.logger.fatal! e.message
        rescue Errno::ENOENT => e
          # This should not cause abnormal exit, which is why 
          # we do not use Merb.fatal but instead just log with max level.
          Merb.logger.fatal! "Could not find a PID file at #{file}. " \
            "Most likely the process is no longer running and the pid file was not cleaned up."
        rescue Exception => e
          if !e.is_a?(SystemExit)
            Merb.logger.fatal! "Failed to kill PID #{pid.inspect} with #{sig.inspect}: #{e.message}"
          end
        end
      end

      # ==== Parameters
      # port<~to_s>:: The port of the Merb process to daemonize.
      #
      # :api: private
      def daemonize(port)
        Merb.logger.warn! "About to fork..." if Merb::Config[:verbose]
        fork do
          Process.setsid
          exit if fork
          Merb.logger.warn! "In #{Process.pid}" if Merb.logger
          File.umask 0000
          STDIN.reopen "/dev/null"
          STDOUT.reopen "/dev/null", "a"
          STDERR.reopen STDOUT
          begin
            Dir.chdir Merb::Config[:merb_root]
          rescue Errno::EACCES => e
            Merb.fatal! "You specified Merb root as #{Merb::Config[:merb_root]}, " \
              "yet the current user does not have access to it. ", e
          end
          at_exit { remove_pid_file(port) }
          Merb::Config[:port] = port
          bootup
        end
      rescue NotImplementedError => e
        Merb.fatal! "Daemonized mode is not supported on your platform. ", e
      end

      # Starts up Merb by running the bootloader and starting the adapter.
      #
      # :api: private
      def bootup
        Merb.trap("TERM") { shutdown }

        Merb.logger.warn! "Running bootloaders..." if Merb::Config[:verbose]
        BootLoader.run
        Merb.logger.warn! "Starting Rack adapter..." if Merb::Config[:verbose]
        Merb.adapter.start(Merb::Config.to_hash)
      end

      # Shut down Merb, reap any workers if necessary.
      #
      # :api: private
      def shutdown(status = 0)
        # reap_workers does exit but may not be called...
        Merb::BootLoader::LoadClasses.reap_workers(status) if Merb::Config[:fork_for_class_load]
        # which is why we exit explicitly here
        exit(status)
      end

      # Change process user/group to those specified in Merb::Config.
      #
      # :api: private
      def change_privilege
        if Merb::Config[:user] && Merb::Config[:group]
          Merb.logger.verbose! "About to change privilege to group " \
            "#{Merb::Config[:group]} and user #{Merb::Config[:user]}"
          _change_privilege(Merb::Config[:user], Merb::Config[:group])
        elsif Merb::Config[:user]
          Merb.logger.verbose! "About to change privilege to user " \
            "#{Merb::Config[:user]}"
          _change_privilege(Merb::Config[:user])
        else
          return true
        end
      end

      # Removes a PID file used by the server from the filesystem.
      # This uses :pid_file options from configuration when provided
      # or merb.<port/socket>.pid in log directory by default.
      #
      # ==== Parameters
      # port<~to_s>::
      #   The port of the Merb process to whom the the PID file belongs to.
      #
      # ==== Alternatives
      # If Merb::Config[:pid_file] has been specified, that will be used
      # instead of the port/socket based PID file.
      #
      # :api: private
      def remove_pid_file(port)
        pidfile = pid_file(port)
        if File.exist?(pidfile)
          Merb.logger.warn! "Removing pid file #{pidfile} (port/socket: #{port})..."
          FileUtils.rm(pidfile)
        end
      end

      # Stores a PID file on the filesystem.
      # This uses :pid_file options from configuration when provided
      # or merb.<port>.pid in log directory by default.
      #
      # ==== Parameters
      # port<~to_s>::
      #   The port of the Merb process to whom the the PID file belongs to.
      #
      # ==== Alternatives
      # If Merb::Config[:pid_file] has been specified, that will be used
      # instead of the port/socket based PID file.
      #
      # :api: private
      def store_pid(port)
        store_details(port)
      end

      # Delete the pidfile for the specified port/socket.
      #
      # :api: private
      def remove_pid(port)
        FileUtils.rm(pid_file(port)) if File.file?(pid_file(port))
      end

      # Stores a PID file on the filesystem.
      # This uses :pid_file options from configuration when provided
      # or merb.<port/socket>.pid in log directory by default.
      #
      # ==== Parameters
      # port<~to_s>::
      #   The port of the Merb process to whom the the PID file belongs to.
      #
      # ==== Alternatives
      # If Merb::Config[:pid_file] has been specified, that will be used
      # instead of the port/socket based PID file.
      #
      # :api: private
      def store_details(port = nil)
        file = pid_file(port)
        begin
          FileUtils.mkdir_p(File.dirname(file))
        rescue Errno::EACCES => e
          Merb.fatal! "Failed to store Merb logs in #{File.dirname(file)}, " \
            "permission denied. ", e
        end
        Merb.logger.warn! "Storing pid #{Process.pid} file to #{file}..." if Merb::Config[:verbose]
        begin
          File.open(file, 'w'){ |f| f.write(Process.pid.to_s) }
        rescue Errno::EACCES => e
          Merb.fatal! "Failed to access #{file}, permission denied.", e
        end
      end

      # Gets the pid file for the specified port/socket.
      #
      # ==== Parameters
      # port<~to_s>::
      #   The port/socket of the Merb process to whom the the PID file belongs to.
      #
      # ==== Returns
      # String::
      #   Location of pid file for specified port. If clustered and pid_file option
      #   is specified, it adds the port/socket value to the path.
      #
      # :api: private
      def pid_file(port)
        pidfile = Merb::Config[:pid_file] || (Merb.log_path / "merb.%s.pid")
        pidfile % port
      end

      # Get a list of the pid files.
      #
      # ==== Returns
      # Array::
      #   List of pid file paths. If not running clustered, the array contains a single path.
      #
      # :api: private
      def pid_files
        if Merb::Config[:pid_file]
          if Merb::Config[:cluster]
            Dir[Merb::Config[:pid_file] % "*"]
          else
            [ Merb::Config[:pid_file] ]
          end
        else
          Dir[Merb.log_path / "merb.*.pid"]
        end
       end

      # Change privileges of the process to the specified user and group.
      #
      # ==== Parameters
      # user<String>:: The user to change the process to.
      # group<String>:: The group to change the process to.
      #
      # ==== Alternatives
      # If group is left out, the user will be used as the group.
      # 
      # :api: private
      def _change_privilege(user, group=user)
        Merb.logger.warn! "Changing privileges to #{user}:#{group}"

        uid, gid = Process.euid, Process.egid

        begin
          target_uid = Etc.getpwnam(user).uid
        rescue ArgumentError => e
          Merb.fatal!("Failed to change to user #{user}, does the user exist?", e)
          return false
        end

        begin
          target_gid = Etc.getgrnam(group).gid
        rescue ArgumentError => e
          Merb.fatal!("Failed to change to group #{group}, does the group exist?", e)
          return false
        end

        if (uid != target_uid) || (gid != target_gid)
          # Change process ownership
          Process.initgroups(user, target_gid)
          Process::GID.change_privilege(target_gid)
          Process::UID.change_privilege(target_uid)
        end
        true
      rescue Errno::EPERM => e
        Merb.fatal! "Permission denied for changing user:group to #{user}:#{group}.", e
        false
      end
      
      # Add trap to enter IRB on SIGINT. Process exit if second SIGINT is received.
      #
      # :api: private
      def add_irb_trap
        Merb.trap("INT") do
          if @interrupted
            Merb.logger.warn! "Interrupt received a second time, exiting!\n"
            exit
          end

          @interrupted = true
          Merb.logger.warn! "Interrupt a second time to quit."
          Kernel.sleep 1.5
          ARGV.clear # Avoid passing args to IRB

          if @irb.nil?
            require "irb"
            IRB.setup(nil)
            @irb = IRB::Irb.new(nil)
            IRB.conf[:MAIN_CONTEXT] = @irb.context
          end

          Merb.trap(:INT) { @irb.signal_handle }
          catch(:IRB_EXIT) { @irb.eval_input }

          Merb.logger.warn! "Exiting from IRB mode back into server mode."
          @interrupted = false
          add_irb_trap
        end
      end
    end
  end
end
