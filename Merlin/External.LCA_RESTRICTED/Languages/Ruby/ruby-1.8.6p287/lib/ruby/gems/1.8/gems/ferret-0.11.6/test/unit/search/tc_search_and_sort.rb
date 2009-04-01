require File.dirname(__FILE__) + "/../../test_helper"

class SearchAndSortTest < Test::Unit::TestCase
  include Ferret::Search
  include Ferret::Store
  include Ferret::Analysis
  include Ferret::Index

  def setup()
    @dir = RAMDirectory.new()
    iw = IndexWriter.new(:dir => @dir,
                         :analyzer => WhiteSpaceAnalyzer.new(),
                         :create => true, :min_merge_docs => 3)
    [                                                                      # len mod
      {:x => "findall", :string => "a", :int => "6", :float => "0.01"},    #  4   0
      {:x => "findall", :string => "c", :int => "5", :float => "0.1"},     #  3   3
      {:x => "findall", :string => "e", :int => "2", :float => "0.001"},   #  5   1
      {:x => "findall", :string => "g", :int => "1", :float => "1.0"},     #  3   3
      {:x => "findall", :string => nil, :int => "3", :float => "0.0001"},  #  6   2
      {:x => "findall", :string => "",  :int => "4", :float => "10.0"},    #  4   0
      {:x => "findall", :string => "h", :int => "5", :float => "0.00001"}, #  7   3
      {:x => "findall", :string => "f", :int => "2", :float => "100.0"},   #  5   1
      {:x => "findall", :string => "d", :int => "3", :float => "1000.0"},  #  6   2
      {:x => "findall", :string => "b", :int => "4", :float => "0.000001"} #  8   0
    ].each do |doc|
      doc.extend(Ferret::BoostMixin)
      doc.boost = doc[:float].to_f
      iw << doc
    end
    iw.close
  end

  def teardown()
    @dir.close()
  end

  def do_test_top_docs(is, query, expected, sort = nil)
    top_docs = is.search(query, {:sort => sort})
    top_docs.total_hits.times do |i|
      assert_equal(expected[i], top_docs.hits[i].doc)
    end

    # test sorting works for smaller ranged query
    offset = 3
    limit = 3
    top_docs = is.search(query, {:sort => sort,
                                 :offset => offset,
                                 :limit => limit})
    limit.times do |i|
      assert_equal(expected[offset + i], top_docs.hits[i].doc)
    end
  end

  def test_sort_field_to_s()
    assert_equal("<SCORE>", SortField::SCORE.to_s);
    sf = SortField.new("MyScore",
                       {:type => :score,
                        :reverse => true})
    assert_equal("MyScore:<SCORE>!", sf.to_s)
    assert_equal("<DOC>", SortField::DOC_ID.to_s);
    sf = SortField.new("MyDoc",
                       {:type => :doc_id,
                        :reverse => true})
    assert_equal("MyDoc:<DOC>!", sf.to_s)
    sf = SortField.new(:date,
                       {:type => :integer})
    assert_equal("date:<integer>", sf.to_s)
    sf = SortField.new(:date,
                       {:type => :integer,
                        :reverse => true})
    assert_equal("date:<integer>!", sf.to_s)
    sf = SortField.new(:price,
                       {:type => :float})
    assert_equal("price:<float>", sf.to_s)
    sf = SortField.new(:price,
                       {:type => :float,
                        :reverse => true})
    assert_equal("price:<float>!", sf.to_s)
    sf = SortField.new(:content,
                       {:type => :string})
    assert_equal("content:<string>", sf.to_s)
    sf = SortField.new(:content,
                       {:type => :string,
                        :reverse => true})
    assert_equal("content:<string>!", sf.to_s)
    sf = SortField.new(:auto_field,
                       {:type => :auto})
    assert_equal("auto_field:<auto>", sf.to_s)
    sf = SortField.new(:auto_field,
                       {:type => :auto,
                        :reverse => true})
    assert_equal("auto_field:<auto>!", sf.to_s)
  end
  
  def test_sort_to_s()
    sort = Sort.new
    assert_equal("Sort[<SCORE>, <DOC>]", sort.to_s)
    sf = SortField.new(:auto_field,
                       {:type => :auto,
                        :reverse => true})
    sort = Sort.new([sf, SortField::SCORE, SortField::DOC_ID])
    assert_equal("Sort[auto_field:<auto>!, <SCORE>, <DOC>]", sort.to_s)
    sort = Sort.new([:one, :two, SortField::DOC_ID])
    assert_equal("Sort[one:<auto>, two:<auto>, <DOC>]", sort.to_s)
    sort = Sort.new([:one, :two])
    assert_equal("Sort[one:<auto>, two:<auto>, <DOC>]", sort.to_s)
  end


  def test_sorts()
    is = Searcher.new(@dir)
    q = TermQuery.new(:x, "findall")
    do_test_top_docs(is, q, [8,7,5,3,1,0,2,4,6,9])
    do_test_top_docs(is, q, [8,7,5,3,1,0,2,4,6,9], Sort::RELEVANCE)
    do_test_top_docs(is, q, [8,7,5,3,1,0,2,4,6,9], [SortField::SCORE])
    do_test_top_docs(is, q, [0,1,2,3,4,5,6,7,8,9], Sort::INDEX_ORDER)
    do_test_top_docs(is, q, [0,1,2,3,4,5,6,7,8,9], [SortField::DOC_ID])

    ## int
    sf_int = SortField.new(:int, {:type => :integer, :reverse => true})
    do_test_top_docs(is, q, [0,1,6,5,9,4,8,2,7,3], [sf_int])
    do_test_top_docs(is, q, [0,1,6,5,9,4,8,2,7,3], "int DESC")
    do_test_top_docs(is, q, [0,1,6,5,9,8,4,7,2,3], [sf_int, SortField::SCORE])
    do_test_top_docs(is, q, [0,1,6,5,9,8,4,7,2,3], "int DESC, SCORE")
    sf_int = SortField.new(:int, {:type => :integer})
    do_test_top_docs(is, q, [3,2,7,4,8,5,9,1,6,0], [sf_int])
    do_test_top_docs(is, q, [3,2,7,4,8,5,9,1,6,0], "int")

    ## byte
    do_test_top_docs(is, q, [3,2,7,4,8,5,9,1,6,0],
                     SortField.new(:int, :type => :byte))
    do_test_top_docs(is, q, [0,1,6,5,9,4,8,2,7,3],
                     [SortField.new(:int, :type => :byte, :reverse => true)])
    

    ## float
    sf_float = SortField.new(:float, {:type => :float, :reverse => true})
    do_test_top_docs(is, q, [8,7,5,3,1,0,2,4,6,9],
                     Sort.new([sf_float, SortField::SCORE]))
    do_test_top_docs(is, q, [8,7,5,3,1,0,2,4,6,9], "float DESC, SCORE")
    sf_float = SortField.new(:float, {:type => :float})
    do_test_top_docs(is, q, [9,6,4,2,0,1,3,5,7,8],
                     Sort.new([sf_float, SortField::SCORE]))
    do_test_top_docs(is, q, [9,6,4,2,0,1,3,5,7,8], "float, SCORE")

    ## str
    sf_str = SortField.new(:string, {:type => :string})
    do_test_top_docs(is, q, [0,9,1,8,2,7,3,6,5,4], [sf_str, SortField::SCORE])
    do_test_top_docs(is, q, [0,9,1,8,2,7,3,6,4,5], "string")

    ## auto
    do_test_top_docs(is, q, [0,9,1,8,2,7,3,6,4,5], Sort.new(:string))
    do_test_top_docs(is, q, [3,2,7,4,8,5,9,1,6,0], Sort.new([:int]))
    do_test_top_docs(is, q, [9,6,4,2,0,1,3,5,7,8], Sort.new(:float))
    do_test_top_docs(is, q, [9,6,4,2,0,1,3,5,7,8], :float)
    do_test_top_docs(is, q, [8,7,5,3,1,0,2,4,6,9], Sort.new(:float, true))
    do_test_top_docs(is, q, [0,6,1,5,9,4,8,7,2,3], Sort.new([:int, :string], true))
    do_test_top_docs(is, q, [0,6,1,5,9,4,8,7,2,3], "int DESC, string DESC")
    do_test_top_docs(is, q, [3,2,7,8,4,9,5,1,6,0], Sort.new([:int, :string]))
    do_test_top_docs(is, q, [3,2,7,8,4,9,5,1,6,0], [:int, :string])
    do_test_top_docs(is, q, [3,2,7,8,4,9,5,1,6,0], "int, string")
  end

  #LENGTH = SortField::SortType.new("length", lambda{|str| str.length})
  #LENGTH_MODULO = SortField::SortType.new("length_mod", lambda{|str| str.length},
  #                                        lambda{|i, j| (i%4) <=> (j%4)})
  #def test_special_sorts
  #  is = IndexSearcher.new(@dir)
  #  q = TermQuery.new(Term.new(:x, "findall"))
  #  sf = SortField.new(:float, {:type => LENGTH, :reverse => true})
  #  do_test_top_docs(is, q, [9,6,4,8,2,7,0,5,1,3], [sf])
  #  sf = SortField.new(:float, {:type => LENGTH_MODULO, :reverse => true})
  #  do_test_top_docs(is, q, [1,3,6,4,8,2,7,0,5,9], [sf])
  #  sf = SortField.new(:float, {:type => LENGTH,
  #                               :reverse => true,
  #                               :comparator => lambda{|i,j| (j%4) <=> (i%4)}})
  #  do_test_top_docs(is, q, [0,5,9,2,7,4,8,1,3,6], [sf])
  #end
end
