require File.dirname(__FILE__) + "/../test_helper"
require File.dirname(__FILE__) + "/number_to_spoken.rb"
require 'thread'

class IndexThreadSafetyTest < Test::Unit::TestCase
  include Ferret::Index

  INDEX_DIR = File.expand_path(File.join(File.dirname(__FILE__), "index"))
  ITERATIONS = 100
  NUM_THREADS = 3
  ANALYZER = Ferret::Analysis::StandardAnalyzer.new()

  def setup
    index = Index.new(:path => INDEX_DIR,
                      :create => true,
                      :analyzer => ANALYZER,
                      :default_field => :content)
    index.close
  end

  def indexing_thread()
    index = Index.new(:path => INDEX_DIR,
                      :analyzer => ANALYZER,
                      :default_field => :content)

    ITERATIONS.times do
      choice = rand()

      if choice > 0.98
        do_optimize(index)
      elsif choice > 0.7
        do_delete_doc(index)
      elsif choice > 0.5
        do_search(index)
      else
        do_add_doc(index)
      end
      index.commit
    end
  end 

  def do_optimize(index)
    puts "Optimizing the index"
    index.optimize
  end

  def do_delete_doc(index)
    return if index.size == 0
    doc_num = rand(index.size)
    puts "Deleting #{doc_num} from index which has#{index.has_deletions? ? "" : " no"} deletions"
    puts "document was already deleted" if (index.deleted?(doc_num))
    index.delete(doc_num)
  end

  def do_add_doc(index)
    n = rand(0xFFFFFFFF)
    d = {:id => n, :content => n.to_spoken}
    puts("Adding #{n}")
    index << d
  end
  
  def do_search(index)
    n = rand(0xFFFFFFFF)
    puts("Searching for #{n}")
    hits = index.search_each(n.to_spoken, :num_docs => 3) do |d, s|
      puts "Hit for #{n}: #{index[d][:id]} - #{s}"
    end
    puts("Searched for #{n}: total = #{hits}")
  end

  def test_threading
    threads = []
    NUM_THREADS.times do
      threads << Thread.new { indexing_thread }
    end

    threads.each {|t| t.join}
  end
end
