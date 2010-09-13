require 'cases/sqlserver_helper'
require 'ruby-prof'
require 'memprof'

class ActiveRecord::TestCase
  
  
  protected
  
  def mem_profile(*args)
    Memprof.track do
      yield
    end
  end
  
  def ruby_profile(name)
    result = RubyProf.profile { yield }
    [:flat,:graph,:html].each do |printer|
      save_ruby_prof_report(result, name, printer)
    end
  end
  
  def save_ruby_prof_report(result, name, printer)
    ptr = case printer
          when :flat  then RubyProf::FlatPrinter
          when :graph then RubyProf::GraphPrinter
          when :html  then RubyProf::GraphHtmlPrinter
          end
    file_name = printer == :html ? "#{name}_graph.html" : "#{name}_#{printer}.txt"
    file_path = File.join(SQLSERVER_TEST_ROOT, 'profile', 'output', file_name)
    File.open(file_path,'w') do |file|
      printer == :html ? ptr.new(result).print(file) : ptr.new(result).print(file,0)
    end
  end
  
end

# RubyProf::FlatPrinter         Creates a flat report in text format 
# RubyProf::GraphPrinter        Creates a call graph report in text format 
# RubyProf::GraphHtmlPrinter    Creates a call graph report in HTML (separate files per thread) 

# RubyProf::PROCESS_TIME        process time
# RubyProf::WALL_TIME           wall time
# RubyProf::CPU_TIME            cpu time
# RubyProf::ALLOCATIONS         object allocations
# RubyProf::MEMORY              memory usage
# RubyProf::GC_RUNS             garbage collections runs
# RubyProf::GC_TIME             garbage collection time

# RubyProf.measure_mode = RubyProf::PROCESS_TIME
# RubyProf.measure_mode = RubyProf::WALL_TIME
# RubyProf.measure_mode = RubyProf::CPU_TIME
# RubyProf.measure_mode = RubyProf::ALLOCATIONS
# RubyProf.measure_mode = RubyProf::MEMORY
# RubyProf.measure_mode = RubyProf::GC_RUNS
# RubyProf.measure_mode = RubyProf::GC_TIME


