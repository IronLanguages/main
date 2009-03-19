require File.dirname(__FILE__) + "/../test_helper"
require File.join(File.dirname(__FILE__), "number_to_spoken.rb")
require 'thread'

class ThreadSafetyTest
  include Ferret::Index
  include Ferret::Search
  include Ferret::Store
  include Ferret

  def initialize(options)
    @options = options
  end

  INDEX_DIR = File.expand_path(File.join(File.dirname(__FILE__), "index"))
  ANALYZER = Ferret::Analysis::WhiteSpaceAnalyzer.new()
  ITERATIONS = 1000
  QUERY_PARSER = Ferret::QueryParser.new(:analyzer => ANALYZER,
                                         :default_field => 'contents')
  @@searcher = nil
  
  def run_index_thread(writer)
    reopen_interval = 30 + rand(60)

    use_compound_file = false
    
    (400*ITERATIONS).times do |i|
      n = rand(0xFFFFFFFF)
      d = {:id => n.to_s, :contents => n.to_spoken}
      puts("Adding #{n}")
      
      # Switch between single and multiple file segments
      use_compound_file = (rand < 0.5)
      writer.use_compound_file = use_compound_file
      
      writer << d

      if (i % reopen_interval == 0) 
        writer.close()
        writer = IndexWriter.new(:path => INDEX_DIR, :analyzer => ANALYZER)
      end
    end
    
    writer.close()
  rescue => e
    puts e
    puts e.backtrace
    raise e
  end

  def run_search_thread(use_global)
    reopen_interval = 10 + rand(20)

    unless use_global
      searcher = Searcher.new(INDEX_DIR)
    end

    (50*ITERATIONS).times do |i|
      search_for(rand(0xFFFFFFFF), (searcher.nil? ? @@searcher : searcher))
      if (i%reopen_interval == 0) 
        if (searcher == nil) 
          @@searcher = Searcher.new(INDEX_DIR)
        else 
          searcher.close()
          searcher = Searcher.new(INDEX_DIR)
        end
      end
    end
  rescue => e
    puts e
    puts e.backtrace
    raise e
  end

  def search_for(n, searcher)
    puts("Searching for #{n}")
    topdocs = searcher.search(QUERY_PARSER.parse(n.to_spoken), :limit => 3)
    puts("Search for #{n}: total = #{topdocs.total_hits}")
    topdocs.hits.each do |hit|
      puts "Hit for #{n}: #{searcher.reader[hit.doc]["id"]} - #{hit.score}"
    end
  end

  def run_test_threads
    threads = []
    unless @options[:read_only]
      writer = IndexWriter.new(:path => INDEX_DIR, :analyzer => ANALYZER,
                               :create => !@options[:add])
      
      threads << Thread.new { run_index_thread(writer) }
      sleep(1)
    end
      
    threads << Thread.new { run_search_thread(false)}

    @@searcher = Searcher.new(INDEX_DIR) 
    threads << Thread.new { run_search_thread(true)}

    threads << Thread.new { run_search_thread(true)}

    threads.each {|t| t.join}
  end
end


if $0 == __FILE__
  require 'optparse'

  OPTIONS = {
    :all        => false,
    :read_only  => false,
  }

  ARGV.options do |opts|
    script_name = File.basename($0)
    opts.banner = "Usage: ruby #{script_name} [options]"

    opts.separator ""

    opts.on("-r", "--read-only", "Read Only.") { OPTIONS[:all] = true }
    opts.on("-a", "--all", "All.") { OPTIONS[:read_only] = true }

    opts.separator ""

    opts.on("-h", "--help",
            "Show this help message.") { puts opts; exit }

    opts.parse!
  end

  tst = ThreadSafetyTest.new(OPTIONS)
  tst.run_test_threads
end
