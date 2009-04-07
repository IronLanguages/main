require File.dirname(__FILE__) + "/../../test_helper"

class IndexTest < Test::Unit::TestCase
  include Ferret::Index
  include Ferret::Search
  include Ferret::Analysis
  include Ferret::Store

  def setup()
  end

  def teardown()
  end

  def check_results(index, query, expected)
    cnt = 0
    #puts "#{query} - #{expected.inspect}"
    #puts index.size
    index.search_each(query) do |doc, score|
      #puts "doc-#{doc} score=#{score}"
      assert_not_nil(expected.index(doc), "doc #{doc} found but not expected")
      cnt += 1
    end
    assert_equal(expected.length, cnt)
  end

  def do_test_index_with_array(index)
    [
      ["one two"],
      ["one", "three"],
      ["two"],
      ["one", "four"],
      ["one two"],
      ["two", "three", "four"],
      ["one"],
      ["two", "three", "four", "five"]
    ].each {|doc| index << doc }
    assert_equal(8, index.size)
    q = "one"
    check_results(index, q, [0, 1, 3, 4, 6])
    q = "one AND two"
    check_results(index, q, [0, 4])
    q = "one OR five"
    check_results(index, q, [0, 1, 3, 4, 6, 7])
    assert_equal(%w{two three four five}, index.doc(7)[:xxx])
  end

  def do_test_index_with_hash(index)
    data = [
      {:xxx => "one two"},
      {:xxx => "one", :field2 => "three"},
      {:xxx => "two"},
      {:xxx => "one", :field2 => "four"},
      {:xxx => "one two"},
      {:xxx => "two", :field2 => "three", :field3 => "four"},
      {:xxx => "one"},
      {:xxx => "two", :field2 => "three", :field3 => "five"}
    ]
    data.each {|doc| index << doc }
    q = "one AND two"
    check_results(index, q, [0, 4])
    q = "one OR five"
    check_results(index, q, [0, 1, 3, 4, 6])
    q = "one OR field3:five"
    check_results(index, q, [0, 1, 3, 4, 6, 7])
    assert_equal("four", index[5]["field3"])
    q = "field3:f*"
    check_results(index, q, [5, 7])
    q = "*:(one AND NOT three)"
    check_results(index, q, [0, 3, 4, 6])
    q = "*:(one AND (NOT three))"
    check_results(index, q, [0, 3, 4, 6])
    q = "two AND field3:f*"
    check_results(index, q, [5, 7])
    assert_equal("five", index.doc(7)["field3"])
    assert_equal("two", index.doc(7)[:xxx])
  end

  def do_test_index_with_doc_array(index)
    data = [
      {:xxx => "one two multi", :id => "myid"},
      {:xxx => "one", :field2 => "three multi"},
      {:xxx => "two"},
      {:xxx => "one", :field2 => "four"},
      {:xxx => "one two"},
      {:xxx => "two", :field2 => "three", :field3 => "four"},
      {:xxx => "one multi2", :id => "hello"},
      {:xxx => "two", :field2 => "this three multi2", :field3 => "five multi"}
    ]
    data.each {|doc| index << doc }
    q = "one AND two"
    check_results(index, q, [0, 4])
    q = "one OR five"
    check_results(index, q, [0, 1, 3, 4, 6])
    q = "one OR field3:five"
    check_results(index, q, [0, 1, 3, 4, 6, 7])
    q = "two AND (field3:f*)"
    check_results(index, q, [5, 7])
    q = "*:(multi OR multi2)"
    check_results(index, q, [0, 1, 6, 7])
    q = "field2|field3:(multi OR multi2)"
    check_results(index, q, [1, 7])
    doc = index[5]
    assert_equal("three", index[5]["field2"])
    assert(!index.has_deletions?)
    assert(!index.deleted?(5))
    assert_equal(8, index.size)
    index.delete(5)
    assert(index.has_deletions?)
    assert(index.deleted?(5))
    assert_equal(7, index.size)
    q = "two AND (field3:f*)"
    check_results(index, q, [7])
    
    doc.load
    doc[:field2] = "dave"
    index << doc
    check_results(index, q, [7, 8])
    check_results(index, "*:this", [])
    assert_equal(8, index.size)
    assert_equal("dave", index[8][:field2])
    index.optimize
    check_results(index, q, [6, 7])
    assert_equal("dave", index[7][:field2])
    index.query_delete("field2:three")
    assert(index.deleted?(1))
    assert(index.deleted?(6))
    assert(! index.deleted?(7))
    assert_equal("one multi2", index["hello"][:xxx])
    assert_equal("one two multi", index["myid"][:xxx])
    index.delete("myid")
    assert(index.deleted?(0))
  end

  def test_ram_index
    index = Ferret::I.new(:default_input_field => :xxx)
    do_test_index_with_array(index)
    index.close

    index = Index.new(:default_field => :xxx)
    do_test_index_with_hash(index)
    index.close

    index = Index.new(:default_field => :xxx, :id_field => :id)
    do_test_index_with_doc_array(index)
    index.close
  end

  def test_fs_index
    fs_path = File.expand_path(File.join(File.dirname(__FILE__), '../../temp/fsdir'))

    Dir[File.join(fs_path, "*")].each {|path| begin File.delete(path) rescue nil end}
    assert_raise(Ferret::FileNotFoundError) do
      Index.new(:path => fs_path,
                :create_if_missing => false,
                :default_field => :xxx)
    end

    index = Index.new(:path => fs_path, :default_input_field => :xxx)
    do_test_index_with_array(index)
    index.close

    Dir[File.join(fs_path, "*")].each {|path| begin File.delete(path) rescue nil end}
    index = Index.new(:path => fs_path, :default_field => :xxx)
    do_test_index_with_hash(index)
    index.close

    Dir[File.join(fs_path, "*")].each {|path| begin File.delete(path) rescue nil end}
    index = Index.new(:path => fs_path,
                      :default_field => :xxx,
                      :id_field => "id")
    do_test_index_with_doc_array(index)
    index.close
  end

  def test_fs_index_is_persistant
    fs_path = File.expand_path(File.join(File.dirname(__FILE__), '../../temp/fsdir'))
    index = Index.new(:path => fs_path, :default_field => :xxx, :create => true)

    [
      {:xxx => "one two", :id => "me"},
      {:xxx => "one", :field2 => "three"},
      {:xxx => "two"},
      {:xxx => "one", :field2 => "four"},
      {:xxx => "one two"},
      {:xxx => "two", :field2 => "three", :field3 => "four"},
      {:xxx => "one"},
      {:xxx => "two", :field2 => "three", :field3 => "five"}
    ].each {|doc| index << doc }
    assert_equal(8, index.size)
    index.close

    index = Index.new(:path => fs_path, :create_if_missing => false)
    assert_equal(8, index.size)
    assert_equal("four", index[5]["field3"])
    index.close
  end

  def test_key_used_for_id_field
    fs_path = File.expand_path(File.join(File.dirname(__FILE__), '../../temp/fsdir'))

    index = Index.new(:path => fs_path, :key => :my_id, :create => true)
    [
      {:my_id => "three", :id => "me"},
      {:my_id => "one", :field2 => "three"},
      {:my_id => "two"},
      {:my_id => "one", :field2 => "four"},
      {:my_id => "three"},
      {:my_id => "two", :field2 => "three", :field3 => "four"},
      {:my_id => "one"},
      {:my_id => "two", :field2 => "three", :field3 => "five"}
    ].each {|doc| index << doc }
    index.optimize
    assert_equal(3, index.size)
    assert_equal("three", index["two"][:field2])
    index.close
  end

  def test_merging_indexes
    index1 = Index.new(:default_field => :f)
    index2 = Index.new(:default_field => :f)
    index3 = Index.new(:default_field => :f)

    [
      {:f => "zero"},
      {:f => "one"},
      {:f => "two"}
    ].each {|doc| index1 << doc }
    [
      {:f => "three"},
      {:f => "four"},
      {:f => "five"}
    ].each {|doc| index2 << doc }
    [
      {:f => "six"},
      {:f => "seven"},
      {:f => "eight"}
    ].each {|doc| index3 << doc }

    index = Index.new(:default_field => :f)
    index.add_indexes(index1)
    assert_equal(3, index.size)
    assert_equal("zero", index[0][:f])
    index.add_indexes([index2, index3])
    assert_equal(9, index.size)
    assert_equal("zero", index[0][:f])
    assert_equal("eight", index[8][:f])
    index1.close
    index2.close
    index3.close
    assert_equal("seven", index[7][:f])
    data = [
      {:f => "alpha"},
      {:f => "beta"},
      {:f => "charlie"}
    ]
    dir1 = RAMDirectory.new
    index1 = Index.new(:dir => dir1, :default_field => :f)
    data.each {|doc| index1 << doc }
    index1.flush
    data = [
      {:f => "delta"},
      {:f => "echo"},
      {:f => "foxtrot"}
    ]
    dir2 = RAMDirectory.new
    index2 = Index.new(:dir => dir2, :default_field => :f)
    data.each {|doc| index2 << doc }
    index2.flush
    data = [
      {:f => "golf"},
      {:f => "india"},
      {:f => "juliet"}
    ]
    dir3 = RAMDirectory.new
    index3 = Index.new(:dir => dir3, :default_field => :f)
    data.each {|doc| index3 << doc }
    index3.flush

    index.add_indexes(dir1)
    assert_equal(12, index.size)
    assert_equal("alpha", index[9][:f])
    index.add_indexes([dir2, dir3])
    assert_equal(18, index.size)
    assert_equal("juliet", index[17][:f])
    index1.close
    dir1.close
    index2.close
    dir2.close
    index3.close
    dir3.close
    assert_equal("golf", index[15][:f])
    index.close
  end

  def test_persist_index
    data = [
      {:f => "zero"},
      {:f => "one"},
      {:f => "two"}
    ]
    index = Index.new(:default_field => :f)
    data.each {|doc| index << doc }
    fs_path = File.expand_path(File.join(File.dirname(__FILE__), '../../temp/fsdir'))

    index.persist(fs_path, true)
    assert_equal(3, index.size)
    assert_equal("zero", index[0][:f])
    index.close

    index = Index.new(:path => fs_path)
    assert_equal(3, index.size)
    assert_equal("zero", index[0][:f])
    index.close


    data = [
      {:f => "romeo"},
      {:f => "sierra"},
      {:f => "tango"}
    ]
    index = Index.new(:default_field => :f)
    data.each {|doc| index << doc }
    assert_equal(3, index.size)
    assert_equal("romeo", index[0][:f])
    dir = FSDirectory.new(fs_path, false)
    index.persist(dir)
    assert_equal(6, index.size)
    assert_equal("zero", index[0][:f])
    assert_equal("romeo", index[3][:f])
    index.close

    index = Index.new(:path => fs_path)
    assert_equal(6, index.size)
    assert_equal("zero", index[0][:f])
    assert_equal("romeo", index[3][:f])
    index.close
  end

  def test_auto_update_when_externally_modified()
    fs_path = File.expand_path(File.join(File.dirname(__FILE__), '../../temp/fsdir'))
    index = Index.new(:path => fs_path, :default_field => :f, :create => true)
    index << "document 1"
    assert_equal(1, index.size)

    index2 = Index.new(:path => fs_path, :default_field => :f)
    assert_equal(1, index2.size)
    index2 << "document 2"
    assert_equal(2, index2.size)
    assert_equal(2, index.size)
    top_docs = index.search("content3")

    assert_equal(0, top_docs.hits.size)

    iw = IndexWriter.new(:path => fs_path, :analyzer => WhiteSpaceAnalyzer.new)
    iw << {:f, "content3"}
    iw.close()

    top_docs = index.search("content3")
    assert_equal(1, top_docs.hits.size)
    assert_equal(3, index.size)
    assert_equal("content3", index[2][:f])
    index2.close
    index.close
  end

  def test_delete
    index = Index.new(:analyzer => WhiteSpaceAnalyzer.new)
    data = [
      {:id => 0, :cat => "/cat1/subcat1"},
      {:id => 1, :cat => "/cat1/subcat2"},
      {:id => 2, :cat => "/cat1/subcat2"},
      {:id => 3, :cat => "/cat1/subcat3"},
      {:id => 4, :cat => "/cat1/subcat4"},
      {:id => 5, :cat => "/cat2/subcat1"},
      {:id => 6, :cat => "/cat2/subcat2"},
      {:id => 7, :cat => "/cat2/subcat3"},
      {:id => 8, :cat => "/cat2/subcat4"},
      {:id => 9, :cat => "/cat2/subcat5"},
    ].each {|doc| index << doc }
    assert_equal(10, index.size)
    assert_equal(1, index.search("id:9").total_hits)
    index.delete(9)
    assert_equal(9, index.size)
    assert_equal(0, index.search("id:9").total_hits)
    assert_equal(1, index.search("id:8").total_hits)
    index.delete("8")
    assert_equal(8, index.size)
    assert_equal(0, index.search("id:8").total_hits)
    assert_equal(5, index.search("cat:/cat1*").total_hits)
    index.query_delete("cat:/cat1*")
    assert_equal(3, index.size)
    assert_equal(0, index.search("cat:/cat1*").total_hits)
    index.close
  end

  def test_update
    index = Index.new(:analyzer => WhiteSpaceAnalyzer.new,
                      :default_input_field => :content,
                      :id_field => :id)
    data = [
      {:id => 0, :cat => "/cat1/subcat1", :content => "content0"},
      {:id => 1, :cat => "/cat1/subcat2", :content => "content1"},
      {:id => 2, :cat => "/cat1/subcat2", :content => "content2"},
      {:id => 3, :cat => "/cat1/subcat3", :content => "content3"},
      {:id => 4, :cat => "/cat1/subcat4", :content => "content4"},
      {:id => 5, :cat => "/cat2/subcat1", :content => "content5"},
      {:id => 6, :cat => "/cat2/subcat2", :content => "content6"},
      {:id => 7, :cat => "/cat2/subcat3", :content => "content7"},
      {:id => 8, :cat => "/cat2/subcat4", :content => "content8"},
      {:id => 9, :cat => "/cat2/subcat5", :content => "content9"},
    ].each { |doc| index << doc }
    assert_equal(10, index.size)
    assert_equal("content5", index["5"][:content])
    index.query_update("id:5", {:content => "content five"})
    assert_equal("content five", index["5"][:content])
    assert_equal(nil, index["5"][:extra_content])
    index.update("5", {:id => "5",
                       :cat => "/cat1/subcat6",
                       :content => "high five",
                       :extra_content => "hello"})
    assert_equal("hello", index["5"][:extra_content])
    assert_equal("high five", index["5"][:content])
    assert_equal("/cat1/subcat6", index["5"][:cat])
    assert_equal("content9", index["9"][:content])
    index.query_update("content:content9", {:content => "content nine"})
    assert_equal("content nine", index["9"][:content])
    assert_equal("content0", index["0"][:content])
    assert_equal(nil, index["0"][:extra_content])
    document = index[0].load
    document[:content] = "content zero"
    document[:extra_content] = "extra content"
    index.update(0, document)
    assert_equal("content zero", index["0"][:content])
    assert_equal("extra content", index["0"][:extra_content])
    assert_equal(nil, index["1"][:tag])
    assert_equal(nil, index["2"][:tag])
    assert_equal(nil, index["3"][:tag])
    assert_equal(nil, index["4"][:tag])
    index.query_update("id:<5 AND cat:>=/cat1/subcat2", {:tag => "cool"})
    assert_equal("cool", index["1"][:tag])
    assert_equal("cool", index["2"][:tag])
    assert_equal("cool", index["3"][:tag])
    assert_equal("cool", index["4"][:tag])
    assert_equal(4, index.search("tag:cool").total_hits)
    index.close
  end

  def test_index_key
    data = [
      {:id => 0, :val => "one"},
      {:id => 0, :val => "two"},
      {:id => 1, :val => "three"},
      {:id => 1, :val => "four"},
    ]
    index = Index.new(:analyzer => WhiteSpaceAnalyzer.new,
                      :key => :id)
    data.each { |doc| index << doc }
    assert_equal(2, index.size)
    assert_equal("two", index["0"][:val])
    assert_equal("four", index["1"][:val])
    index.close
  end

  def test_index_multi_key
    index = Index.new(:analyzer => WhiteSpaceAnalyzer.new,
                      :key => [:id, :table])
    data = [
      {:id => 0, :table => "product", :product => "tent"},
      {:id => 0, :table => "location", :location => "first floor"},
      {:id => 0, :table => "product", :product => "super tent"},
      {:id => 0, :table => "location", :location => "second floor"},
      {:id => 1, :table => "product", :product => "backback"},
      {:id => 1, :table => "location", :location => "second floor"},
      {:id => 1, :table => "location", :location => "first floor"},
      {:id => 1, :table => "product", :product => "rucksack"},
      {:id => 1, :table => "product", :product => "backpack"}
    ].each { |doc| index << doc }
    index.optimize
    assert_equal(4, index.size)
    assert_equal("super tent", index[0][:product])
    assert_equal("second floor", index[1][:location])
    assert_equal("backpack", index[3][:product])
    assert_equal("first floor", index[2][:location])
    index.close
  end

  def test_index_multi_key_untokenized
    field_infos = FieldInfos.new(:term_vector => :no)
    field_infos.add_field(:id, :index => :untokenized)
    field_infos.add_field(:table, :index => :untokenized)

    index = Index.new(:analyzer => Analyzer.new,
                      :key => [:id, :table],
                      :field_infos => field_infos)
    data = [
      {:id => 0, :table => "Product", :product => "tent"},
      {:id => 0, :table => "location", :location => "first floor"},
      {:id => 0, :table => "Product", :product => "super tent"},
      {:id => 0, :table => "location", :location => "second floor"},
      {:id => 1, :table => "Product", :product => "backback"},
      {:id => 1, :table => "location", :location => "second floor"},
      {:id => 1, :table => "location", :location => "first floor"},
      {:id => 1, :table => "Product", :product => "rucksack"},
      {:id => 1, :table => "Product", :product => "backpack"}
    ].each {|doc| index << doc}

    assert_equal(4, index.size)
    index.optimize
    assert_equal("super tent", index[0][:product])
    assert_equal("second floor", index[1][:location])
    assert_equal("backpack", index[3][:product])
    assert_equal("first floor", index[2][:location])
    index.close
  end

  def test_sortby_date
    index = Index.new(:analyzer => WhiteSpaceAnalyzer.new)

    data = [
      {:content => "one", :date => "20051023"},
      {:content => "two", :date => "19530315"},
      {:content => "three four", :date => "19390912"},
      {:content => "one", :date => "19770905"},
      {:content => "two", :date => "19810831"},
      {:content => "three", :date => "19790531"},
      {:content => "one", :date => "19770725"},
      {:content => "two", :date => "19751226"},
      {:content => "four", :date => "19390912"}
    ].each {|doc| index << doc}

    sf_date = SortField.new("date", {:type => :integer})
    #top_docs = index.search("one", :sort => [sf_date, SortField::SCORE])
    top_docs = index.search("one", :sort => Sort.new("date"))
    assert_equal(3, top_docs.total_hits)
    assert_equal("19770725", index[top_docs.hits[0].doc][:date])
    assert_equal("19770905", index[top_docs.hits[1].doc][:date])
    assert_equal("20051023", index[top_docs.hits[2].doc][:date])
    top_docs = index.search("one two three four",
                            :sort => [sf_date, SortField::SCORE])

    assert_equal("19390912", index[top_docs.hits[0].doc][:date])
    assert_equal("three four", index[top_docs.hits[0].doc][:content])
    assert_equal("19390912", index[top_docs.hits[1].doc][:date])
    assert_equal("four", index[top_docs.hits[1].doc][:content])
    assert_equal("19530315", index[top_docs.hits[2].doc][:date])

    top_docs = index.search("one two three four",
                            :sort => [:date, :content])
    assert_equal("19390912", index[top_docs.hits[0].doc][:date])
    assert_equal("four", index[top_docs.hits[0].doc][:content])
    assert_equal("19390912", index[top_docs.hits[1].doc][:date])
    assert_equal("three four", index[top_docs.hits[1].doc][:content])
    assert_equal("19530315", index[top_docs.hits[2].doc][:date])

    index.close
  end

  def test_auto_flush
    fs_path = File.expand_path(File.join(File.dirname(__FILE__), '../../temp/fsdir'))
    Dir[File.join(fs_path, "*")].each {|path| begin File.delete(path) rescue nil end}

    data = %q(one two three four five six seven eight nine ten eleven twelve)
    index1 = Index.new(:path => fs_path, :auto_flush => true, :key => :id)
    index1 << "zero"
    index2 = Index.new(:path => fs_path, :auto_flush => true)
    begin
      data.each do |datum|
        index1 << {:id => datum[0], :content => datum}
        index2 << {:id => datum[0], :content => datum}
      end
      5.times do |i|
        index1.delete(i)
        index2.delete(i + 5)
      end
      index1.optimize
      index2 << "thirteen"
    rescue Exception => e
      assert(false, "This should not cause an error when auto flush has been set")
    end
    index1.close
    index2.close
  end

  def test_doc_specific_analyzer
    index = Index.new
    index.add_document("abc", Ferret::Analysis::Analyzer.new)
    assert_equal(1, index.size)
  end

  def test_adding_empty_term_vectors
    index = Index.new(:field_infos => FieldInfos.new(:term_vector => :no))

    # Note: Adding keywords to either field1 or field2 gets rid of the error

    index << {:field1, ''}
    index << {:field2, ''}
    index << {:field3, 'foo bar baz'}

    index.flush
    index.close
  end

  def test_stopwords
    field_infos = FieldInfos.new(:store => :no, :term_vector => :no)
    field_infos.add_field(:id, :store => :yes, :index => :untokenized)

    i = Ferret::Index::Index.new(:or_default => false, :default_search_field => '*')
 
    # adding this additional field to the document leads to failure below
    # comment out this statement and all tests pass:
    i << {:id => 1, :content => "Move or shake"}

    hits = i.search 'move nothere shake'
    assert_equal 0, hits.total_hits
    hits = i.search 'move shake'
    assert_equal 1, hits.total_hits
    hits = i.search 'move or shake'
    assert_equal 1, hits.total_hits # fails when id field is present
  end

  def test_threading
    path = File.expand_path(File.join(File.dirname(__FILE__), '../../temp/fsdir'))
    index = Ferret::Index::Index.new(:path => path, :create => true)

    100.times do |i|
      buf = ''
      doc = {}
      doc[:id] = i
      doc[:foo] = "foo #{i}"
      index << doc
    end

    threads = []

    4.times do
      threads << Thread.new(index) do |index|
        result = index.search('id:42')
        assert_equal(1, result.total_hits)
      end
    end

    threads.each{|t| t.join }
  end 

  def test_wildcard
    i = nil
    Ferret::I.new do |i|
      i << "one"
      assert_equal(1, i.search("*").total_hits)
      i << "two"
      assert_equal(2, i.search("*").total_hits)
      i << {:content => "three"}
      assert_equal(3, i.search("*").total_hits)
      assert_equal(3, i.search("id:*").total_hits)
      assert_equal(2, i.search('id:?*').total_hits)
    end
    assert_raise(StandardError) {i.close} 
  end

  def check_highlight(index, q, excerpt_length, num_excerpts, expected, field = :field)
    highlights = index.highlight(q, 0,
                                 :excerpt_length => excerpt_length,
                                 :num_excerpts => num_excerpts,
                                 :field => field)
    assert_equal(expected, highlights)
    highlights = index.highlight(q, 1,
                                 :excerpt_length => excerpt_length,
                                 :num_excerpts => num_excerpts,
                                 :field => field)
    assert_equal(expected, highlights)
  end

  def test_highlighter()
    index = Ferret::I.new(:default_field => :field,
                          :default_input_field => :field,
                          :analyzer => Ferret::Analysis::WhiteSpaceAnalyzer.new)
    [
      "the words we are searching for are one and two also " +
      "sometimes looking for them as a phrase like this; one " +
      "two lets see how it goes",
      [
        "the words we",
        "are searching",
        "for are one",
        "and two also",
        "sometimes looking",
        "for them as a",
        "phrase like this;",
        "one two lets see",
        "how it goes"
      ]
    ].each {|doc| index << doc }
    
    check_highlight(index, "one", 10, 1, ["...are <b>one</b>..."])
    check_highlight(index, "one", 10, 2,
                    ["...are <b>one</b>...","...this; <b>one</b>..."])
    check_highlight(index, "one", 10, 3,
                    ["the words...","...are <b>one</b>...","...this; <b>one</b>..."])
    check_highlight(index, "one", 10, 4,
                    ["the words we are...","...are <b>one</b>...","...this; <b>one</b>..."])
    check_highlight(index, "one", 10, 5,
                    ["the words we are searching for are <b>one</b>...","...this; <b>one</b>..."])
    check_highlight(index, "one", 10, 20,
                    ["the words we are searching for are <b>one</b> and two also " +
                     "sometimes looking for them as a phrase like this; <b>one</b> " +
                     "two lets see how it goes"])
    check_highlight(index, "one", 200, 1,
                    ["the words we are searching for are <b>one</b> and two also " +
                     "sometimes looking for them as a phrase like this; <b>one</b> " +
                     "two lets see how it goes"])
    check_highlight(index, "(one two)", 15, 2, 
                    ["...<b>one</b> and <b>two</b>...","...this; <b>one</b> <b>two</b>..."])
    check_highlight(index, 'one two "one two"', 15, 2, 
                    ["...<b>one</b> and <b>two</b>...","...this; <b>one two</b>..."])
    check_highlight(index, 'one two "one two"', 15, 1, 
                    ["...this; <b>one two</b>..."])
    check_highlight(index, '"one two"', 15, 1, nil, :not_a_field)
    check_highlight(index, 'wrong_field:one', 15, 1, nil, :wrong_field)
    check_highlight(index, '"the words" "for are one and two" words one two', 10, 1, 
                    ["<b>the words</b>..."])
    check_highlight(index, '"the words" "for are one and two" words one two', 20, 2, 
                    ["<b>the words</b> we are...","...<b>for are one and two</b>..."])
    index.close
  end

  def test_changing_analyzer
    index = Ferret::I.new
    a = Ferret::Analysis::WhiteSpaceAnalyzer.new(false)
    index.add_document({:content => "Content With Capitals"}, a)
    tv = index.reader.term_vector(0, :content)
    assert_equal("Capitals", tv.terms[0].text)
    index.close
  end

  def test_top_doc_to_json
    index = Ferret::I.new
    [
      {:f1 => "one"},
      {:f2 => ["two",2,2.0]},
      {:f3 => 3},
      {:f4 => 4.0},
      {:f5 => "five", :funny => '"' * 10_000}
    ].each {|doc| index << doc}
    json_str = index.search("one two 3 4.0 five",
                            :sort => Ferret::Search::Sort::INDEX_ORDER).to_json
    assert(json_str == '[{"f1":"one"},{"f2":["two","2","2.0"]},{"f3":"3"},{"f4":"4.0"},{"f5":"five","funny":"' + '\'"\'' * 10_000 + '"}]' ||
           json_str == '[{"f1":"one"},{"f2":["two","2","2.0"]},{"f3":"3"},{"f4":"4.0"},{"funny":"' + '\'"\'' * 10_000 + '","f5":"five"}]')
    assert_equal('[]', index.search("xxx").to_json)
    index.close
  end

  def test_large_query_delete
    index = Ferret::I.new
    20.times do
      index << {:id => 'one'}
      index << {:id => 'two'}
    end
    index.query_delete('id:one')
    assert_equal(20, index.size)
  end
end
