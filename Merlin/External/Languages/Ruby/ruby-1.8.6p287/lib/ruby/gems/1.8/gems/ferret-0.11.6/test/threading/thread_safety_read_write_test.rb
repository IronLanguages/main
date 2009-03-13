require File.dirname(__FILE__) + "/../test_helper"
require File.dirname(__FILE__) + "/../utils/number_to_spoken.rb"
require 'thread'

class IndexThreadSafetyReadWriteTest < Test::Unit::TestCase
  include Ferret::Index
  include Ferret::Document

  INDEX_DIR = File.expand_path(File.join(File.dirname(__FILE__), "index"))
  ITERATIONS = 10000
  ANALYZER = Ferret::Analysis::Analyzer.new()

  def setup
    @index = Index.new(:path => 'index2',
                       :create => true,
                       :analyzer => ANALYZER,
                       :default_field => 'contents')
  end

  def search_thread()
    ITERATIONS.times do
      do_search()
      sleep(rand(1))
    end
  rescue => e
    puts e
    puts e.backtrace
    @index = nil
    raise e
  end 

  def index_thread()
    ITERATIONS.times do
      do_add_doc()
      sleep(rand(1))
    end
  rescue => e
    puts e
    puts e.backtrace
    @index = nil
    raise e
  end 

  def do_add_doc
    d = Document.new()
    n = rand(0xFFFFFFFF)
    d << Field.new("id", n.to_s, Field::Store::YES, Field::Index::UNTOKENIZED)
    d << Field.new("contents", n.to_spoken, Field::Store::NO, Field::Index::TOKENIZED)
    puts("Adding #{n}")
    begin
      @index << d
    rescue => e
      puts e
      puts e.backtrace
      @index = nil
      raise e
    end
  end
  
  def do_search
    n = rand(0xFFFFFFFF)
    puts("Searching for #{n}")
    hits = @index.search_each(n.to_spoken, :num_docs => 3) do |d, s|
      puts "Hit for #{n}: #{@index[d]["id"]} - #{s}"
    end
    puts("Searched for #{n}: total = #{hits}")
  end

  def test_threading
    threads = []
    threads << Thread.new { search_thread }
    threads << Thread.new { index_thread }

    threads.each { |t| t.join }
  end
end
