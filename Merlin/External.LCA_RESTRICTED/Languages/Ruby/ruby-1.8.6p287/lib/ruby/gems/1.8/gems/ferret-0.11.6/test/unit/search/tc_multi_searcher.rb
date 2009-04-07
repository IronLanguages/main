require File.dirname(__FILE__) + "/../../test_helper"
require File.join(File.dirname(__FILE__), "tc_index_searcher.rb")

# make sure a MultiSearcher searching only one index
# passes all the Searcher tests
class SimpleMultiSearcherTest < SearcherTest
  alias :old_setup :setup 
  def setup()
    old_setup
    @searcher = MultiSearcher.new([Searcher.new(@dir)])
  end
end


# checks query results of a multisearcher searching two indexes
# against those of a single indexsearcher searching the same
# set of documents
class MultiSearcherTest < Test::Unit::TestCase
  include Ferret::Search
  include Ferret::Store
  include Ferret::Analysis
  include Ferret::Index

  include SearcherTests

  DOCUMENTS1 = [
    {"date" => "20050930", :field => "word1",
      "cat" => "cat1/"},
    {"date" => "20051001", :field => "word1 word2 the quick brown fox",
      "cat" => "cat1/sub1"},
    {"date" => "20051002", :field => "word1 word3",
      "cat" => "cat1/sub1/subsub1"},
    {"date" => "20051003", :field => "word1 word3",
      "cat" => "cat1/sub2"},
    {"date" => "20051004", :field => "word1 word2",
      "cat" => "cat1/sub2/subsub2"},
    {"date" => "20051005", :field => "word1",
      "cat" => "cat2/sub1"},
    {"date" => "20051006", :field => "word1 word3",
      "cat" => "cat2/sub1"},
    {"date" => "20051007", :field => "word1",
      "cat" => "cat2/sub1"},
    {"date" => "20051008", :field => "word1 word2 word3 the fast brown fox",
      "cat" => "cat2/sub1"}
  ]

  DOCUMENTS2 = [
    {"date" => "20051009", :field => "word1",
      "cat" => "cat3/sub1"},
    {"date" => "20051010", :field => "word1",
      "cat" => "cat3/sub1"},
    {"date" => "20051011", :field => "word1 word3 the quick red fox",
      "cat" => "cat3/sub1"},
    {"date" => "20051012", :field => "word1",
      "cat" => "cat3/sub1"},
    {"date" => "20051013", :field => "word1",
      "cat" => "cat1/sub2"},
    {"date" => "20051014", :field => "word1 word3 the quick hairy fox",
      "cat" => "cat1/sub1"},
    {"date" => "20051015", :field => "word1",
      "cat" => "cat1/sub2/subsub1"},
    {"date" => "20051016",
      :field => "word1 the quick fox is brown and hairy and a little red",
      "cat" => "cat1/sub1/subsub2"},
    {"date" => "20051017", :field => "word1 the brown fox is quick and red",
      "cat" => "cat1/"}
  ]

  def setup()
    # create MultiSearcher from two seperate searchers
    dir1 = RAMDirectory.new()
    iw1 = IndexWriter.new(:dir => dir1,
                          :analyzer => WhiteSpaceAnalyzer.new(),
                          :create => true)
    DOCUMENTS1.each { |doc| iw1 << doc }
    iw1.close()
    
    dir2 = RAMDirectory.new()
    iw2 = IndexWriter.new(:dir => dir2,
                          :analyzer => WhiteSpaceAnalyzer.new(),
                          :create => true)
    DOCUMENTS2.each { |doc| iw2 << doc }
    iw2.close()
    @searcher = Ferret::Search::MultiSearcher.new([Searcher.new(dir1),
                                                   Searcher.new(dir2)])

    # create single searcher
    dir = RAMDirectory.new
    iw = IndexWriter.new(:dir => dir,
                         :analyzer => WhiteSpaceAnalyzer.new(),
                         :create => true)
    DOCUMENTS1.each { |doc| iw << doc }
    DOCUMENTS2.each { |doc| iw << doc }
    iw.close
    @single = Searcher.new(dir)

    #@query_parser = Ferret::QueryParser.new([:date, :field, :cat], :analyzer => WhiteSpaceAnalyzer.new())
  end

  def teardown()
    @searcher.close
    @single.close
  end

  def check_hits(query, ignore1, ignore2 = nil, ignore3 = nil)
    multi_docs = @searcher.search(query)
    single_docs = @single.search(query)
    assert_equal(single_docs.hits.size, multi_docs.hits.size, 'hit count')
    assert_equal(single_docs.total_hits, multi_docs.total_hits, 'hit count')
    
    multi_docs.hits.each_with_index { |sd, id|
      assert_equal(single_docs.hits[id].doc, sd.doc)
      assert(single_docs.hits[id].score.approx_eql?(sd.score), 
             "#{single_docs.hits[id]} != #{sd.score}")
    }
  end

  def test_get_doc()
    assert_equal(18, @searcher.max_doc)
    assert_equal("20050930", @searcher.get_document(0)[:date])
    assert_equal("cat1/sub2/subsub2", @searcher[4][:cat])
    assert_equal("20051012", @searcher.get_document(12)[:date])
    assert_equal(18, @single.max_doc)
    assert_equal("20050930", @single.get_document(0)[:date])
    assert_equal("cat1/sub2/subsub2", @single[4][:cat])
    assert_equal("20051012", @single.get_document(12)[:date])
  end
end
