require File.dirname(__FILE__) + "/../../test_helper"
require File.dirname(__FILE__) + "/tm_searcher"

class SearcherTest < Test::Unit::TestCase
  include Ferret::Search
  include Ferret::Store
  include Ferret::Analysis
  include Ferret::Index

  include SearcherTests

  def setup()
    @dir = RAMDirectory.new()
    iw = IndexWriter.new(:dir => @dir,
                         :analyzer => WhiteSpaceAnalyzer.new(),
                         :create => true)
    @documents = IndexTestHelper::SEARCH_TEST_DOCS
    @documents.each { |doc| iw << doc }
    iw.close()
    @searcher = Searcher.new(@dir)
  end

  def teardown()
    @searcher.close
    @dir.close()
  end

  def get_docs(hits)
    docs = []
    hits.each do |hit|
      docs << hit.doc
    end
    docs
  end

  def check_hits(query, expected, top=nil, total_hits=nil)
    options = {}
    options[:limit] = expected.size + 1 if (expected.size > 10) 
    top_docs = @searcher.search(query, options)
    assert_equal(expected.length, top_docs.hits.size)
    assert_equal(top, top_docs.hits[0].doc) if top
    if total_hits
      assert_equal(total_hits, top_docs.total_hits)
    else
      assert_equal(expected.length, top_docs.total_hits)
    end
    top_docs.hits.each do |score_doc|
      assert(expected.include?(score_doc.doc),
             "#{score_doc.doc} was found unexpectedly")
      assert(score_doc.score.approx_eql?(@searcher.explain(query, score_doc.doc).score), 
        "Scores(#{score_doc.score} != #{@searcher.explain(query, score_doc.doc).score})")
    end
  end

  def test_get_doc()
    assert_equal(18, @searcher.max_doc)
    assert_equal("20050930", @searcher.get_document(0)[:date])
    assert_equal("cat1/sub2/subsub2", @searcher.get_document(4)[:category])
    assert_equal("20051012", @searcher.get_document(12)[:date])
  end
end
