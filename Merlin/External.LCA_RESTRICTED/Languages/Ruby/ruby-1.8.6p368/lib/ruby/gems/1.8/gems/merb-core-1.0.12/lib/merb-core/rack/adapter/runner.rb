module Merb
  
  module Rack
    
    class Runner
      # ==== Parameters
      # opts<Hash>:: Options for the runner (see below).
      #
      # ==== Options (opts)
      # :runner_code<String>:: The code to run.
      #
      # ==== Notes
      # If opts[:runner_code] matches a filename, that file will be read and
      # the contents executed. Otherwise the code will be executed directly.
      #
      # :api: plugin
      def self.start(opts={})
        Merb::Server.change_privilege
        if opts[:runner_code]
          if File.exists?(opts[:runner_code])
            eval(File.read(opts[:runner_code]), TOPLEVEL_BINDING, __FILE__, __LINE__)
          else
            eval(opts[:runner_code], TOPLEVEL_BINDING, __FILE__, __LINE__)
          end
          exit
        end  
      end
    end
  end
end
