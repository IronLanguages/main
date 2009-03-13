begin
  require "ruby-prof"
rescue LoadError => e
  Merb.fatal! "You need ruby-prof installed to use the profiler middleware", e
end

module Merb
  module Rack
    class Profiler < Merb::Rack::Middleware

      # :api: private
      def initialize(app, types = [RubyProf::ALLOCATIONS, RubyProf::PROCESS_TIME])
        super(app)
        @types = types
      end

      # :api: plugin
      def call(env)
        measure_names = { RubyProf::ALLOCATIONS => 'allocations', 
          RubyProf::PROCESS_TIME => 'time', RubyProf::MEMORY => "memory" }

        ret = nil

        GC.disable
        @types.each do |type|
          next if type.nil?
          
          if GC.respond_to?(:enable_stats)
            GC.enable_stats || GC.clear_stats
          end

          RubyProf.measure_mode = type
          RubyProf.start
          100.times do
            ret = super
          end
          result = RubyProf.stop
          printer = RubyProf::CallTreePrinter.new(result)
          path = "merb_profile_results" / env["PATH_INFO"]
          FileUtils.mkdir_p(path)
          printer.print(
            File.open(
              "#{path}/callgrind.out.#{measure_names[RubyProf::measure_mode]}",     
              'w'))

          GC.disable_stats if GC.respond_to?(:disable_stats)
        end
        GC.enable
        ret
      end

      
    end
  end
end