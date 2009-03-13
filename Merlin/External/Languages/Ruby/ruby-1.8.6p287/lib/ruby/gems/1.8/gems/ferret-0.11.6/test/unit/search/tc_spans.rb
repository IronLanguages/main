require File.dirname(__FILE__) + "/../../test_helper"


class SpansBasicTest < Test::Unit::TestCase
  include Ferret::Search
  include Ferret::Store
  include Ferret::Index
  include Ferret::Search::Spans
  include Ferret::Analysis

  def setup()
    @dir = RAMDirectory.new
    iw = IndexWriter.new(:dir => @dir,
                         :analyzer => WhiteSpaceAnalyzer.new(),
                         :create => true)
    [
      "start finish one two three four five six seven",
      "start one finish two three four five six seven",
      "start one two finish three four five six seven flip",
      "start one two three finish four five six seven",
      "start one two three four finish five six seven",
      "start one two three four five finish six seven",
      "start one two three four five six finish seven eight",
      "start one two three four five six seven finish eight nine",
      "start one two three four five six finish seven eight",
      "start one two three four five finish six seven",
      "start one two three four finish five six seven",
      "start one two three finish four five six seven",
      "start one two finish three four five six seven flop",
      "start one finish two three four five six seven",
      "start finish one two three four five six seven",
      "start start  one two three four five six seven",
      "finish start one two three four five six seven",
      "finish one start two three four five six seven toot",
      "finish one two start three four five six seven",
      "finish one two three start four five six seven",
      "finish one two three four start five six seven",
      "finish one two three four five start six seven",
      "finish one two three four five six start seven eight",
      "finish one two three four five six seven start eight nine",
      "finish one two three four five six start seven eight",
      "finish one two three four five start six seven",
      "finish one two three four start five six seven",
      "finish one two three start four five six seven",
      "finish one two start three four five six seven",
      "finish one start two three four five six seven",
      "finish start one two three four five six seven"
    ].each { |line| iw << {:field => line} }

    iw.close()

    @searcher = Searcher.new(@dir)
  end

  def teardown()
    @searcher.close
    @dir.close
  end

  def number_split(i)
    if (i < 10)
      return "<#{i}>"
    elsif (i < 100)
      return "<#{((i/10)*10)}> <#{i%10}>"
    else
      return "<#{((i/100)*100)}> <#{(((i%100)/10)*10)}> <#{i%10}>"
    end
  end

  def check_hits(query, expected, test_explain = false, top=nil)
    top_docs = @searcher.search(query, {:limit => expected.length + 1})
    assert_equal(expected.length, top_docs.hits.size)
    assert_equal(top, top_docs.hits[0].doc) if top
    assert_equal(expected.length, top_docs.total_hits)
    top_docs.hits.each do |hit|
      assert(expected.include?(hit.doc),
             "#{hit.doc} was found unexpectedly")
      if test_explain
        assert(hit.score.approx_eql?(@searcher.explain(query, hit.doc).score), 
          "Scores(#{hit.score} != " +
          "#{@searcher.explain(query, hit.doc).score})")
      end
    end
  end

  def test_span_term_query()
    tq = SpanTermQuery.new(:field, "nine")
    check_hits(tq, [7,23], true)
    tq = SpanTermQuery.new(:field, "eight")
    check_hits(tq, [6,7,8,22,23,24])
  end
 
  def test_span_multi_term_query()
    tq = SpanMultiTermQuery.new(:field, ["eight", "nine"])
    check_hits(tq, [6,7,8,22,23,24], true)
    tq = SpanMultiTermQuery.new(:field, ["flip", "flop", "toot", "nine"])
    check_hits(tq, [2,7,12,17,23])
  end

  def test_span_prefix_query()
    tq = SpanPrefixQuery.new(:field, "fl")
    check_hits(tq, [2, 12], true)
  end

  def test_span_near_query()
    tq1 = SpanTermQuery.new(:field, "start")
    tq2 = SpanTermQuery.new(:field, "finish")
    q = SpanNearQuery.new(:clauses => [tq1, tq2], :in_order => true)
    check_hits(q, [0,14], true)
    q = SpanNearQuery.new()
    q << tq1 << tq2
    check_hits(q, [0,14,16,30], true)
    q = SpanNearQuery.new(:clauses => [tq1, tq2],
                          :slop => 1, :in_order => true)
    check_hits(q, [0,1,13,14])
    q = SpanNearQuery.new(:clauses => [tq1, tq2], :slop => 1)
    check_hits(q, [0,1,13,14,16,17,29,30])
    q = SpanNearQuery.new(:clauses => [tq1, tq2],
                          :slop => 4, :in_order => true)
    check_hits(q, [0,1,2,3,4,10,11,12,13,14])
    q = SpanNearQuery.new(:clauses => [tq1, tq2], :slop => 4)
    check_hits(q, [0,1,2,3,4,10,11,12,13,14,16,17,18,19,20,26,27,28,29,30])
    q = SpanNearQuery.new(:clauses => [
                          SpanPrefixQuery.new(:field, 'se'),
                          SpanPrefixQuery.new(:field, 'fl')], :slop => 0)
    check_hits(q, [2, 12], true)
  end

  def test_span_not_query()
    tq1 = SpanTermQuery.new(:field, "start")
    tq2 = SpanTermQuery.new(:field, "finish")
    tq3 = SpanTermQuery.new(:field, "two")
    tq4 = SpanTermQuery.new(:field, "five")
    nearq1 = SpanNearQuery.new(:clauses => [tq1, tq2],
                               :slop => 4, :in_order => true)
    nearq2 = SpanNearQuery.new(:clauses => [tq3, tq4],
                               :slop => 4, :in_order => true)
    q = SpanNotQuery.new(nearq1, nearq2)
    check_hits(q, [0,1,13,14], true)
    nearq1 = SpanNearQuery.new(:clauses => [tq1, tq2], :slop => 4)
    q = SpanNotQuery.new(nearq1, nearq2)
    check_hits(q, [0,1,13,14,16,17,29,30])
    nearq1 = SpanNearQuery.new(:clauses => [tq1, tq3],
                               :slop => 4, :in_order => true)
    nearq2 = SpanNearQuery.new(:clauses => [tq2, tq4], :slop => 8)
    q = SpanNotQuery.new(nearq1, nearq2)
    check_hits(q, [2,3,4,5,6,7,8,9,10,11,12,15])
  end

  def test_span_first_query()
    finish_first = [16,17,18,19,20,21,22,23,24,25,26,27,28,29,30]
    tq = SpanTermQuery.new(:field, "finish")
    q = SpanFirstQuery.new(tq, 1)
    check_hits(q, finish_first, true)
    q = SpanFirstQuery.new(tq, 5)
    check_hits(q, [0,1,2,3,11,12,13,14]+finish_first, false)
  end

  def test_span_or_query_query()
    tq1 = SpanTermQuery.new(:field, "start")
    tq2 = SpanTermQuery.new(:field, "finish")
    tq3 = SpanTermQuery.new(:field, "five")
    nearq1 = SpanNearQuery.new(:clauses => [tq1, tq2], :slop => 1,
                               :in_order => true)
    nearq2 = SpanNearQuery.new(:clauses => [tq2, tq3], :slop => 0)
    q = SpanOrQuery.new([nearq1, nearq2])
    check_hits(q, [0,1,4,5,9,10,13,14], false)
    nearq1 = SpanNearQuery.new(:clauses => [tq1, tq2], :slop => 0)
    nearq2 = SpanNearQuery.new(:clauses => [tq2, tq3], :slop => 1)
    q = SpanOrQuery.new([nearq1, nearq2])
    check_hits(q, [0,3,4,5,6,8,9,10,11,14,16,30], false)
  end

  def test_span_prefix_query_max_terms
    @dir = RAMDirectory.new
    iw = IndexWriter.new(:dir => @dir,
                         :analyzer => WhiteSpaceAnalyzer.new())
    2000.times { |i| iw << {:field => "prefix#{i} term#{i}"} }
    iw.close()
    @searcher = Searcher.new(@dir)

    pq = SpanPrefixQuery.new(:field, "prefix")
    tq = SpanTermQuery.new(:field, "term1500")
    q = SpanNearQuery.new(:clauses => [pq, tq], :in_order => true)
    check_hits(q, [], false)
    pq = SpanPrefixQuery.new(:field, "prefix", 2000)
    q = SpanNearQuery.new(:clauses => [pq, tq], :in_order => true)
    check_hits(q, [1500], false)
  end
end
