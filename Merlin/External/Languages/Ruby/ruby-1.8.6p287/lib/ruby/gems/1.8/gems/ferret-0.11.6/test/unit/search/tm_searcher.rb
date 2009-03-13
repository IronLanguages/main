require File.dirname(__FILE__) + "/../../test_helper"

module SearcherTests
  include Ferret::Search

  def test_term_query
    tq = TermQuery.new(:field, "word2")
    tq.boost = 100
    check_hits(tq, [1,4,8])
    #puts @searcher.explain(tq, 1)
    #puts @searcher.explain(tq, 4)
    #puts @searcher.explain(tq, 8)

    tq = TermQuery.new(:field, "2342")
    check_hits(tq, [])

    tq = TermQuery.new(:field, "")
    check_hits(tq, [])

    tq = TermQuery.new(:field, "word1")
    top_docs = @searcher.search(tq)
    assert_equal(@searcher.max_doc, top_docs.total_hits)
    assert_equal(10, top_docs.hits.size)
    top_docs = @searcher.search(tq, {:limit => 20})
    assert_equal(@searcher.max_doc, top_docs.hits.size)

    assert_equal([Ferret::Term.new(:field, "word1")], tq.terms(@searcher))
  end

  def check_docs(query, options, expected=[])
    top_docs = @searcher.search(query, options)
    docs = top_docs.hits
    assert_equal(expected.length, docs.length)
    docs.length.times do |i|
      assert_equal(expected[i], docs[i].doc)
    end
  end

  def test_offset
    tq = TermQuery.new(:field, "word1")
    tq.boost = 100
    top_docs = @searcher.search(tq, {:limit => 100})
    expected = []
    top_docs.hits.each do |sd|
      expected << sd.doc
    end

    assert_raise(ArgumentError) { @searcher.search(tq, {:offset => -1}) }
    assert_raise(ArgumentError) { @searcher.search(tq, {:limit => 0}) }
    assert_raise(ArgumentError) { @searcher.search(tq, {:limit => -1}) }

    check_docs(tq, {:limit => 8, :offset => 0}, expected[0,8])
    check_docs(tq, {:limit => 3, :offset => 1}, expected[1,3])
    check_docs(tq, {:limit => 6, :offset => 2}, expected[2,6])
    check_docs(tq, {:limit => 2, :offset => expected.length}, [])
    check_docs(tq, {:limit => 2, :offset => expected.length + 100}, [])
    check_docs(tq, {:limit => :all}, expected)
    check_docs(tq, {:limit => :all, :offset => 2}, expected[2..-1])
  end

  def test_multi_term_query
    mtq = MultiTermQuery.new(:field, :max_terms => 4, :min_score => 0.5)
    check_hits(mtq, [])
    assert_equal('""', mtq.to_s(:field))
    assert_equal('field:""', mtq.to_s)

    [
      ["brown", 1.0, '"brown"'],
      ["fox",   0.1, '"brown"'],
      ["fox",   0.6, '"fox^0.6|brown"'],
      ["fast", 50.0, '"fox^0.6|brown|fast^50.0"']
    ].each do |term, boost, str|
      mtq.add_term(term, boost)
      assert_equal(str, mtq.to_s(:field))
      assert_equal("field:#{str}", mtq.to_s())
    end

    mtq.boost = 80.1
    assert_equal('field:"fox^0.6|brown|fast^50.0"^80.1', mtq.to_s())
    mtq << "word1"
    assert_equal('field:"fox^0.6|brown|word1|fast^50.0"^80.1', mtq.to_s())
    mtq << "word2"
    assert_equal('field:"brown|word1|word2|fast^50.0"^80.1', mtq.to_s())
    mtq << "word3"
    assert_equal('field:"brown|word1|word2|fast^50.0"^80.1', mtq.to_s())
    
    terms = mtq.terms(@searcher)
    assert(terms.index(Ferret::Term.new(:field, "brown")))
    assert(terms.index(Ferret::Term.new(:field, "word1")))
    assert(terms.index(Ferret::Term.new(:field, "word2")))
    assert(terms.index(Ferret::Term.new(:field, "fast")))
  end

  def test_boolean_query
    bq = BooleanQuery.new()
    tq1 = TermQuery.new(:field, "word1")
    tq2 = TermQuery.new(:field, "word3")
    bq.add_query(tq1, :must)
    bq.add_query(tq2, :must)
    check_hits(bq, [2,3,6,8,11,14], 14)

    tq3 = TermQuery.new(:field, "word2")
    bq.add_query(tq3, :should)
    check_hits(bq, [2,3,6,8,11,14], 8)

    bq = BooleanQuery.new()
    bq.add_query(tq2, :must)
    bq.add_query(tq3, :must_not)
    check_hits(bq, [2,3,6,11,14])

    bq = BooleanQuery.new()
    bq.add_query(tq2, :must_not)
    check_hits(bq, [0,1,4,5,7,9,10,12,13,15,16,17])

    bq = BooleanQuery.new()
    bq.add_query(tq2, :should)
    bq.add_query(tq3, :should)
    check_hits(bq, [1,2,3,4,6,8,11,14])

    bq = BooleanQuery.new()
    bc1 = BooleanQuery::BooleanClause.new(tq2, :should)
    bc2 = BooleanQuery::BooleanClause.new(tq3, :should)
    bq << bc1
    bq << bc2
    check_hits(bq, [1,2,3,4,6,8,11,14])
  end

  def test_phrase_query()
    pq = PhraseQuery.new(:field)
    assert_equal("\"\"", pq.to_s(:field))
    assert_equal("field:\"\"", pq.to_s)

    pq << "quick" << "brown" << "fox"
    check_hits(pq, [1])

    pq = PhraseQuery.new(:field, 1)
    pq << "quick"
    pq.add_term("fox", 2)
    check_hits(pq, [1,11,14,16])

    pq.slop = 0
    check_hits(pq, [1,11,14])

    pq.slop = 1
    check_hits(pq, [1,11,14,16])

    pq.slop = 4
    check_hits(pq, [1,11,14,16,17])
  end

  def test_range_query()
    rq = RangeQuery.new(:date, :lower => "20051006", :upper => "20051010")
    check_hits(rq, [6,7,8,9,10])

    rq = RangeQuery.new(:date, :>= => "20051006", :<= => "20051010")
    check_hits(rq, [6,7,8,9,10])

    rq = RangeQuery.new(:date, :lower => "20051006", :upper => "20051010",
                        :include_lower => false)
    check_hits(rq, [7,8,9,10])

    rq = RangeQuery.new(:date, :> => "20051006", :<= => "20051010")
    check_hits(rq, [7,8,9,10])

    rq = RangeQuery.new(:date, :lower => "20051006", :upper => "20051010",
                        :include_upper => false)
    check_hits(rq, [6,7,8,9])

    rq = RangeQuery.new(:date, :>= => "20051006", :< => "20051010")
    check_hits(rq, [6,7,8,9])

    rq = RangeQuery.new(:date, :lower => "20051006", :upper => "20051010",
                        :include_lower => false, :include_upper => false)
    check_hits(rq, [7,8,9])

    rq = RangeQuery.new(:date, :> => "20051006", :< => "20051010")
    check_hits(rq, [7,8,9])

    rq = RangeQuery.new(:date, :upper => "20051003")
    check_hits(rq, [0,1,2,3])

    rq = RangeQuery.new(:date, :<= => "20051003")
    check_hits(rq, [0,1,2,3])

    rq = RangeQuery.new(:date, :upper => "20051003", :include_upper => false)
    check_hits(rq, [0,1,2])

    rq = RangeQuery.new(:date, :< => "20051003")
    check_hits(rq, [0,1,2])

    rq = RangeQuery.new(:date, :lower => "20051014")
    check_hits(rq, [14,15,16,17])

    rq = RangeQuery.new(:date, :>= => "20051014")
    check_hits(rq, [14,15,16,17])

    rq = RangeQuery.new(:date, :lower => "20051014", :include_lower => false)
    check_hits(rq, [15,16,17])

    rq = RangeQuery.new(:date, :> => "20051014")
    check_hits(rq, [15,16,17])
  end

  def test_prefix_query()
    pq = PrefixQuery.new(:category, "cat1")
    check_hits(pq, [0, 1, 2, 3, 4, 13, 14, 15, 16, 17])

    pq = PrefixQuery.new(:category, "cat1/sub2")
    check_hits(pq, [3, 4, 13, 15])
  end

  def test_wildcard_query()
    wq = WildcardQuery.new(:category, "cat1*")
    check_hits(wq, [0, 1, 2, 3, 4, 13, 14, 15, 16, 17])

    wq = WildcardQuery.new(:category, "cat1*/su??ub2")
    check_hits(wq, [4, 16])

    wq = WildcardQuery.new(:category, "*/sub2*")
    check_hits(wq, [3, 4, 13, 15])
  end

  def test_multi_phrase_query()
    mpq = PhraseQuery.new(:field)
    mpq << ["quick", "fast"]
    mpq << ["brown", "red", "hairy"]
    mpq << "fox"
    check_hits(mpq, [1, 8, 11, 14])

    mpq.slop = 4
    check_hits(mpq, [1, 8, 11, 14, 16, 17])
  end

  def test_highlighter()
    dir = Ferret::Store::RAMDirectory.new
    iw = Ferret::Index::IndexWriter.new(:dir => dir,
                  :analyzer => Ferret::Analysis::WhiteSpaceAnalyzer.new())
    long_text = "big " + "between " * 2000 + 'house'
    [
      {:field => "the words we are searching for are one and two also " +
                 "sometimes looking for them as a phrase like this; one " +
                 "two lets see how it goes"},
      {:long =>  'before ' * 1000 + long_text + ' after' * 1000},
      {:dates => '20070505 20071230 20060920 20081111'},
    ].each {|doc| iw << doc }
    iw.close
    
    searcher = Searcher.new(dir)

    q = TermQuery.new(:field, "one");
    highlights = searcher.highlight(q, 0, :field,
                                    :excerpt_length => 10,
                                    :num_excerpts => 1)
    assert_equal(1, highlights.size)
    assert_equal("...are <b>one</b>...", highlights[0])

    highlights = searcher.highlight(q, 0, :field,
                                    :excerpt_length => 10,
                                    :num_excerpts => 2)
    assert_equal(2, highlights.size)
    assert_equal("...are <b>one</b>...", highlights[0])
    assert_equal("...this; <b>one</b>...", highlights[1])

    highlights = searcher.highlight(q, 0, :field,
                                    :excerpt_length => 10,
                                    :num_excerpts => 3)
    assert_equal(3, highlights.size)
    assert_equal("the words...", highlights[0])
    assert_equal("...are <b>one</b>...", highlights[1])
    assert_equal("...this; <b>one</b>...", highlights[2])

    highlights = searcher.highlight(q, 0, :field,
                                    :excerpt_length => 10,
                                    :num_excerpts => 4)
    assert_equal(3, highlights.size)
    assert_equal("the words we are...", highlights[0])
    assert_equal("...are <b>one</b>...", highlights[1])
    assert_equal("...this; <b>one</b>...", highlights[2])

    highlights = searcher.highlight(q, 0, :field,
                                    :excerpt_length => 10,
                                    :num_excerpts => 5)
    assert_equal(2, highlights.size)
    assert_equal("the words we are searching for are <b>one</b>...", highlights[0])
    assert_equal("...this; <b>one</b>...", highlights[1])

    highlights = searcher.highlight(q, 0, :field,
                                    :excerpt_length => 10,
                                    :num_excerpts => 20)
    assert_equal(1, highlights.size)
    assert_equal("the words we are searching for are <b>one</b> and two also " +
            "sometimes looking for them as a phrase like this; <b>one</b> " +
            "two lets see how it goes", highlights[0])

    highlights = searcher.highlight(q, 0, :field,
                                    :excerpt_length => 1000,
                                    :num_excerpts => 1)
    assert_equal(1, highlights.size)
    assert_equal("the words we are searching for are <b>one</b> and two also " +
            "sometimes looking for them as a phrase like this; <b>one</b> " +
            "two lets see how it goes", highlights[0])

    q = BooleanQuery.new(false)
    q << TermQuery.new(:field, "one")
    q << TermQuery.new(:field, "two")

    highlights = searcher.highlight(q, 0, :field,
                                    :excerpt_length => 15,
                                    :num_excerpts => 2)
    assert_equal(2, highlights.size)
    assert_equal("...<b>one</b> and <b>two</b>...", highlights[0])
    assert_equal("...this; <b>one</b> <b>two</b>...", highlights[1])

    q << (PhraseQuery.new(:field) << "one" << "two")

    highlights = searcher.highlight(q, 0, :field,
                                    :excerpt_length => 15,
                                    :num_excerpts => 2)
    assert_equal(2, highlights.size)
    assert_equal("...<b>one</b> and <b>two</b>...", highlights[0])
    assert_equal("...this; <b>one two</b>...", highlights[1])

    highlights = searcher.highlight(q, 0, :field,
                                    :excerpt_length => 15,
                                    :num_excerpts => 1)
    assert_equal(1, highlights.size)
    # should have a higher priority since it the merger of three matches
    assert_equal("...this; <b>one two</b>...", highlights[0])

    highlights = searcher.highlight(q, 0, :not_a_field,
                                    :excerpt_length => 15,
                                    :num_excerpts => 1)
    assert_nil(highlights)

    q = TermQuery.new(:wrong_field, "one")
    highlights = searcher.highlight(q, 0, :wrong_field,
                                    :excerpt_length => 15,
                                    :num_excerpts => 1)
    assert_nil(highlights)

    q = BooleanQuery.new(false)
    q << (PhraseQuery.new(:field) << "the" << "words")
    q << (PhraseQuery.new(:field) << "for" << "are" << "one" << "and" << "two")
    q << TermQuery.new(:field, "words")
    q << TermQuery.new(:field, "one")
    q << TermQuery.new(:field, "two")

    highlights = searcher.highlight(q, 0, :field,
                                    :excerpt_length => 10,
                                    :num_excerpts => 1)
    assert_equal(1, highlights.size)
    assert_equal("<b>the words</b>...", highlights[0])

    highlights = searcher.highlight(q, 0, :field,
                                    :excerpt_length => 10,
                                    :num_excerpts => 2)
    assert_equal(2, highlights.size)
    assert_equal("<b>the words</b>...", highlights[0])
    assert_equal("...<b>one</b> <b>two</b>...", highlights[1])

    # {:dates => '20070505, 20071230, 20060920, 20081111'},
    [
      [RangeQuery.new(:dates, :>= => '20081111'),
        '20070505 20071230 20060920 <b>20081111</b>'],
      [RangeQuery.new(:dates, :>= => '20070101'),
        '<b>20070505</b> <b>20071230</b> 20060920 <b>20081111</b>'],
      [PrefixQuery.new(:dates, '2007'),
        '<b>20070505</b> <b>20071230</b> 20060920 20081111'],
    ].each do |query, expected|
      assert_equal([expected],
                   searcher.highlight(query, 2, :dates))
    end

    #q = PhraseQuery.new(:long) << 'big' << 'house'
    #q.slop = 4000
    #highlights = searcher.highlight(q, 1, :long,
    #                                :excerpt_length => 400,
    #                                :num_excerpts => 2)
    #assert_equal(1, highlights.size)
    #puts highlights[0]
    #assert_equal("<b>the words</b>...", highlights[0])
    #assert_equal("...<b>one</b> <b>two</b>...", highlights[1])
  end
end
