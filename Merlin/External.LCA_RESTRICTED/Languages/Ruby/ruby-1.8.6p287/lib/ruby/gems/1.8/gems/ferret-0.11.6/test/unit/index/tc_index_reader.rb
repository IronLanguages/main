require File.dirname(__FILE__) + "/../../test_helper"

module IndexReaderCommon

  include Ferret::Index
  include Ferret::Analysis

  def test_index_reader
    do_test_get_field_names()

    do_test_term_enum()

    do_test_term_doc_enum()
 
    do_test_term_vectors()

    do_test_get_doc()
  end

  def do_test_get_field_names()
    field_names = @ir.field_names

    assert(field_names.include?(:body))
    assert(field_names.include?(:changing_field))
    assert(field_names.include?(:author))
    assert(field_names.include?(:title))
    assert(field_names.include?(:text))
    assert(field_names.include?(:year))
  end

  def do_test_term_enum()
    te = @ir.terms(:author)

    assert_equal('[{"term":"Leo","frequency":1},{"term":"Tolstoy","frequency":1}]', te.to_json);
    te.field = :author
    assert_equal('[["Leo",1],["Tolstoy",1]]', te.to_json(:fast));
    te.field = :author

    assert(te.next?)
    assert_equal("Leo", te.term)
    assert_equal(1, te.doc_freq)
    assert(te.next?)
    assert_equal("Tolstoy", te.term)
    assert_equal(1, te.doc_freq)
    assert(! te.next?)

    te.field = :body
    assert(te.next?)
    assert_equal("And", te.term)
    assert_equal(1, te.doc_freq)

    assert(te.skip_to("Not"))
    assert_equal("Not", te.term)
    assert_equal(1, te.doc_freq)
    assert(te.next?)
    assert_equal("Random", te.term)
    assert_equal(16, te.doc_freq)

    te.field = :text
    assert(te.skip_to("which"))
    assert("which", te.term)
    assert_equal(1, te.doc_freq)
    assert(! te.next?)

    te.field = :title
    assert(te.next?)
    assert_equal("War And Peace", te.term)
    assert_equal(1, te.doc_freq)
    assert(!te.next?)

    expected = %w{is 1 more 1 not 1 skip 42 stored 1 text 1 which 1}
    te = @ir.terms(:text)
    te.each do |term, doc_freq|
      assert_equal(expected.shift, term)
      assert_equal(expected.shift.to_i, doc_freq)
    end

    te = @ir.terms_from(:body, "Not")
    assert_equal("Not", te.term)
    assert_equal(1, te.doc_freq)
    assert(te.next?)
    assert_equal("Random", te.term)
    assert_equal(16, te.doc_freq)
  end

  def do_test_term_doc_enum()

    assert_equal(IndexTestHelper::INDEX_TEST_DOCS.size, @ir.num_docs())
    assert_equal(IndexTestHelper::INDEX_TEST_DOCS.size, @ir.max_doc())

    assert_equal(4, @ir.doc_freq(:body, "Wally"))

    tde = @ir.term_docs_for(:body, "Wally")

    [
      [ 0, 1],
      [ 5, 1],
      [18, 3],
      [20, 6]
    ].each do |doc, freq|
      assert(tde.next?)
      assert_equal(doc, tde.doc())
      assert_equal(freq, tde.freq())
    end
    assert(! tde.next?)

    tde = @ir.term_docs_for(:body, "Wally")
    assert_equal('[{"document":0,"frequency":1},{"document":5,"frequency":1},{"document":18,"frequency":3},{"document":20,"frequency":6}]', tde.to_json)
    tde = @ir.term_docs_for(:body, "Wally")
    assert_equal('[[0,1],[5,1],[18,3],[20,6]]', tde.to_json(:fast))

    do_test_term_docpos_enum_skip_to(tde)

    # test term positions
    tde = @ir.term_positions_for(:body, "read")
    [
      [false,  1, 1, [3]],
      [false,  2, 2, [1, 4]],
      [false,  6, 4, [3, 4]],
      [false,  9, 3, [0, 4]],
      [ true, 16, 2, [2]],
      [ true, 21, 6, [3, 4, 5, 8, 9, 10]]
    ].each do |skip, doc, freq, positions|
      if skip
        assert(tde.skip_to(doc))
      else 
        assert(tde.next?)
      end
      assert_equal(doc, tde.doc())
      assert_equal(freq, tde.freq())
      positions.each {|pos| assert_equal(pos, tde.next_position())}
    end

    assert_nil(tde.next_position())
    assert(! tde.next?)

    tde = @ir.term_positions_for(:body, "read")
    assert_equal('[' +
       '{"document":1,"frequency":1,"positions":[3]},' +
       '{"document":2,"frequency":2,"positions":[1,4]},' +
       '{"document":6,"frequency":4,"positions":[3,4,5,6]},' +
       '{"document":9,"frequency":3,"positions":[0,4,13]},' +
       '{"document":10,"frequency":1,"positions":[1]},' +
       '{"document":16,"frequency":2,"positions":[2,3]},' +
       '{"document":17,"frequency":1,"positions":[2]},' +
       '{"document":20,"frequency":1,"positions":[21]},' +
       '{"document":21,"frequency":6,"positions":[3,4,5,8,9,10]}]',
       tde.to_json())
    tde = @ir.term_positions_for(:body, "read")
    assert_equal('[' +
       '[1,1,[3]],' +
       '[2,2,[1,4]],' +
       '[6,4,[3,4,5,6]],' +
       '[9,3,[0,4,13]],' +
       '[10,1,[1]],' +
       '[16,2,[2,3]],' +
       '[17,1,[2]],' +
       '[20,1,[21]],' +
       '[21,6,[3,4,5,8,9,10]]]',
       tde.to_json(:fast))

    tde = @ir.term_positions_for(:body, "read")

    do_test_term_docpos_enum_skip_to(tde)
  end

  def do_test_term_docpos_enum_skip_to(tde)
    tde.seek(:text, "skip")

    [
      [10, 22],
      [44, 44],
      [60, 60],
      [62, 62],
      [63, 63],
    ].each do |skip_doc, doc_and_freq|
      assert(tde.skip_to(skip_doc))
      assert_equal(doc_and_freq, tde.doc())
      assert_equal(doc_and_freq, tde.freq())
    end


    assert(! tde.skip_to(IndexTestHelper::INDEX_TEST_DOC_COUNT))
    assert(! tde.skip_to(IndexTestHelper::INDEX_TEST_DOC_COUNT))
    assert(! tde.skip_to(IndexTestHelper::INDEX_TEST_DOC_COUNT + 100))

    tde.seek(:text, "skip")
    assert(! tde.skip_to(IndexTestHelper::INDEX_TEST_DOC_COUNT))
  end

  def do_test_term_vectors()
    expected_tv = TermVector.new(:body,
      [
        TVTerm.new("word1", [2, 4, 7]),
        TVTerm.new("word2", [3]),
        TVTerm.new("word3", [0, 5, 8, 9]),
        TVTerm.new("word4", [1, 6])
      ],
      [*(0...10)].collect {|i| TVOffsets.new(i*6, (i+1)*6 - 1)})

    tv = @ir.term_vector(3, :body)

    assert_equal(expected_tv, tv)

    tvs = @ir.term_vectors(3)
    assert_equal(3, tvs.size)

    assert_equal(expected_tv, tvs[:body])
    
    tv = tvs[:author]
    assert_equal(:author, tv.field)
    assert_equal([TVTerm.new("Leo", [0]), TVTerm.new("Tolstoy", [1])], tv.terms)
    assert(tv.offsets.nil?)


    tv = tvs[:title]
    assert_equal(:title, tv.field)
    assert_equal([TVTerm.new("War And Peace", nil)], tv.terms)
    assert_equal([TVOffsets.new(0, 13)], tv.offsets)
  end
   
  def do_test_get_doc()
    doc = @ir.get_document(3)
    [:author, :body, :title, :year].each {|fn| assert(doc.fields.include?(fn))}
    assert_equal(4, doc.fields.size)
    assert_equal(0, doc.size)
    assert_equal([], doc.keys)

    assert_equal("Leo Tolstoy", doc[:author])
    assert_equal("word3 word4 word1 word2 word1 word3 word4 word1 word3 word3",
                 doc[:body])
    assert_equal("War And Peace", doc[:title])
    assert_equal("1865", doc[:year])
    assert_nil(doc[:text])

    assert_equal(4, doc.size)
    [:author, :body, :title, :year].each {|fn| assert(doc.keys.include?(fn))}
    assert_equal([@ir[0].load, @ir[1].load, @ir[2].load], @ir[0, 3].collect {|d| d.load})
    assert_equal([@ir[61].load, @ir[62].load, @ir[63].load], @ir[61, 100].collect {|d| d.load})
    assert_equal([@ir[0].load, @ir[1].load, @ir[2].load], @ir[0..2].collect {|d| d.load})
    assert_equal([@ir[61].load, @ir[62].load, @ir[63].load], @ir[61..100].collect {|d| d.load})
    assert_equal(@ir[-60], @ir[4])
  end

  def test_ir_norms()
    @ir.set_norm(3, :title, 1)
    @ir.set_norm(3, :body, 12)
    @ir.set_norm(3, :author, 145)
    @ir.set_norm(3, :year, 31)
    @ir.set_norm(3, :text, 202)
    @ir.set_norm(25, :text, 20)
    @ir.set_norm(50, :text, 200)
    @ir.set_norm(63, :text, 155)

    norms = @ir.norms(:text)

    assert_equal(202, norms[ 3])
    assert_equal( 20, norms[25])
    assert_equal(200, norms[50])
    assert_equal(155, norms[63])

    norms = @ir.norms(:title)
    assert_equal(1, norms[3])

    norms = @ir.norms(:body)
    assert_equal(12, norms[3])

    norms = @ir.norms(:author)
    assert_equal(145, norms[3])

    norms = @ir.norms(:year)
    # TODO: this returns two possible results depending on whether it is 
    # a multi reader or a segment reader. If it is a multi reader it will
    # always return an empty set of norms, otherwise it will return nil. 
    # I'm not sure what to do here just yet or if this is even an issue.
    #assert(norms.nil?) 

    norms = " " * 164
    @ir.get_norms_into(:text, norms, 100)
    assert_equal(202, norms[103])
    assert_equal( 20, norms[125])
    assert_equal(200, norms[150])
    assert_equal(155, norms[163])

    @ir.commit()

    iw_optimize()

    ir2 = ir_new()

    norms = " " * 164
    ir2.get_norms_into(:text, norms, 100)
    assert_equal(202, norms[103])
    assert_equal( 20, norms[125])
    assert_equal(200, norms[150])
    assert_equal(155, norms[163])
    ir2.close()
  end

  def test_ir_delete()
    doc_count = IndexTestHelper::INDEX_TEST_DOCS.size
    @ir.delete(1000) # non existant doc_num
    assert(! @ir.has_deletions?())
    assert_equal(doc_count, @ir.max_doc())
    assert_equal(doc_count, @ir.num_docs())
    assert(! @ir.deleted?(10))

    [
      [10,            doc_count - 1],
      [10,            doc_count - 1],
      [doc_count - 1, doc_count - 2],
      [doc_count - 2, doc_count - 3],
    ].each do |del_num, num_docs| 
      @ir.delete(del_num)
      assert(@ir.has_deletions?())
      assert_equal(doc_count, @ir.max_doc())
      assert_equal(num_docs, @ir.num_docs())
      assert(@ir.deleted?(del_num))
    end

    @ir.undelete_all()
    assert(! @ir.has_deletions?())
    assert_equal(doc_count, @ir.max_doc())
    assert_equal(doc_count, @ir.num_docs())
    assert(! @ir.deleted?(10))
    assert(! @ir.deleted?(doc_count - 2))
    assert(! @ir.deleted?(doc_count - 1))

    del_list = [10, 20, 30, 40, 50, doc_count - 1]

    del_list.each {|doc_num| @ir.delete(doc_num)}
    assert(@ir.has_deletions?())
    assert_equal(doc_count, @ir.max_doc())
    assert_equal(doc_count - del_list.size, @ir.num_docs())
    del_list.each {|doc_num| assert(@ir.deleted?(doc_num))}

    ir2 = ir_new()
    assert(! ir2.has_deletions?())
    assert_equal(doc_count, ir2.max_doc())
    assert_equal(doc_count, ir2.num_docs())

    @ir.commit()

    assert(! ir2.has_deletions?())
    assert_equal(doc_count, ir2.max_doc())
    assert_equal(doc_count, ir2.num_docs())

    ir2.close
    ir2 = ir_new()
    assert(ir2.has_deletions?())
    assert_equal(doc_count, ir2.max_doc())
    assert_equal(doc_count - 6, ir2.num_docs())
    del_list.each {|doc_num| assert(ir2.deleted?(doc_num))}

    ir2.undelete_all()
    assert(! ir2.has_deletions?())
    assert_equal(doc_count, ir2.max_doc())
    assert_equal(doc_count, ir2.num_docs())
    del_list.each {|doc_num| assert(! ir2.deleted?(doc_num))}

    del_list.each {|doc_num| assert(@ir.deleted?(doc_num))}

    ir2.commit()

    del_list.each {|doc_num| assert(@ir.deleted?(doc_num))}

    del_list.each {|doc_num| ir2.delete(doc_num)}
    ir2.commit()

    iw_optimize()

    ir3 = ir_new()

    assert(!ir3.has_deletions?())
    assert_equal(doc_count - 6, ir3.max_doc())
    assert_equal(doc_count - 6, ir3.num_docs())

    ir2.close()
    ir3.close()
  end

  def test_latest
    assert(@ir.latest?)
    ir2 = ir_new()
    assert(ir2.latest?)

    ir2.delete(0)
    ir2.commit()
    assert(ir2.latest?)
    assert(!@ir.latest?)

    ir2.close()
  end
end

class MultiReaderTest < Test::Unit::TestCase
  include IndexReaderCommon

  def ir_new
    IndexReader.new(@dir)
  end

  def iw_optimize
    iw = IndexWriter.new(:dir => @dir, :analyzer => WhiteSpaceAnalyzer.new())
    iw.optimize()
    iw.close()
  end

  def setup
    @dir = Ferret::Store::RAMDirectory.new()

    iw = IndexWriter.new(:dir => @dir,
                         :analyzer => WhiteSpaceAnalyzer.new(),
                         :create => true,
                         :field_infos => IndexTestHelper::INDEX_TEST_FIS,
                         :max_buffered_docs => 15)
    IndexTestHelper::INDEX_TEST_DOCS.each {|doc| iw << doc}

    # we mustn't optimize here so that MultiReader is used.
    #iw.optimize() unless self.class == MultiReaderTest
    iw.close()
    @ir = ir_new()
  end

  def teardown()
    @ir.close()
    @dir.close()
  end
end

class SegmentReaderTest < MultiReaderTest
end

class MultiExternalReaderTest < Test::Unit::TestCase
  include IndexReaderCommon

  def ir_new
    readers = @dirs.collect {|dir| IndexReader.new(dir) }
    IndexReader.new(readers)
  end

  def iw_optimize
    @dirs.each do |dir|
      iw = IndexWriter.new(:dir => dir, :analyzer => WhiteSpaceAnalyzer.new())
      iw.optimize()
      iw.close()
    end
  end

  def setup()
    @dirs = []
    
    [
      [0, 10],
      [10, 30],
      [30, IndexTestHelper::INDEX_TEST_DOCS.size]
    ].each do |start, finish|
      dir = Ferret::Store::RAMDirectory.new()
      @dirs << dir

      iw = IndexWriter.new(:dir => dir,
                           :analyzer => WhiteSpaceAnalyzer.new(),
                           :create => true,
                           :field_infos => IndexTestHelper::INDEX_TEST_FIS)
      (start...finish).each do |doc_id|
        iw << IndexTestHelper::INDEX_TEST_DOCS[doc_id]
      end
      iw.close()
    end
    @ir = ir_new
  end

  def teardown()
    @ir.close()
    @dirs.each {|dir| dir.close}
  end
end

class MultiExternalReaderDirTest < Test::Unit::TestCase
  include IndexReaderCommon

  def ir_new
    IndexReader.new(@dirs)
  end

  def iw_optimize
    @dirs.each do |dir|
      iw = IndexWriter.new(:dir => dir, :analyzer => WhiteSpaceAnalyzer.new())
      iw.optimize()
      iw.close()
    end
  end

  def setup()
    @dirs = []
    
    [
      [0, 10],
      [10, 30],
      [30, IndexTestHelper::INDEX_TEST_DOCS.size]
    ].each do |start, finish|
      dir = Ferret::Store::RAMDirectory.new()
      @dirs << dir

      iw = IndexWriter.new(:dir => dir,
                           :analyzer => WhiteSpaceAnalyzer.new(),
                           :create => true,
                           :field_infos => IndexTestHelper::INDEX_TEST_FIS)
      (start...finish).each do |doc_id|
        iw << IndexTestHelper::INDEX_TEST_DOCS[doc_id]
      end
      iw.close()
    end
    @ir = ir_new
  end

  def teardown()
    @ir.close()
    @dirs.each {|dir| dir.close}
  end
end

class MultiExternalReaderPathTest < Test::Unit::TestCase
  include IndexReaderCommon

  def ir_new
    IndexReader.new(@paths)
  end

  def iw_optimize
    @paths.each do |path|
      iw = IndexWriter.new(:path => path, :analyzer => WhiteSpaceAnalyzer.new())
      iw.optimize()
      iw.close()
    end
  end

  def setup()
    base_dir = File.expand_path(File.join(File.dirname(__FILE__),
                       '../../temp/multidir'))
    FileUtils.mkdir_p(base_dir)
    @paths = [
      File.join(base_dir, "i1"),
      File.join(base_dir, "i2"),
      File.join(base_dir, "i3")
    ]
    
    [
      [0, 10],
      [10, 30],
      [30, IndexTestHelper::INDEX_TEST_DOCS.size]
    ].each_with_index do |(start, finish), i|
      path = @paths[i]

      iw = IndexWriter.new(:path => path,
                           :analyzer => WhiteSpaceAnalyzer.new(),
                           :create => true,
                           :field_infos => IndexTestHelper::INDEX_TEST_FIS)
      (start...finish).each do |doc_id|
        iw << IndexTestHelper::INDEX_TEST_DOCS[doc_id]
      end
      iw.close()
    end
    @ir = ir_new
  end

  def teardown()
    @ir.close()
  end
end

class IndexReaderTest < Test::Unit::TestCase
  include Ferret::Index
  include Ferret::Analysis

  def setup()
    @dir = Ferret::Store::RAMDirectory.new()
  end

  def teardown()
    @dir.close()
  end

  def test_ir_multivalue_fields()
    @fs_dpath = File.expand_path(File.join(File.dirname(__FILE__),
                                           '../../temp/fsdir'))
    @fs_dir = Ferret::Store::FSDirectory.new(@fs_dpath, true)

    iw = IndexWriter.new(:dir => @fs_dir,
                         :analyzer => WhiteSpaceAnalyzer.new(),
                         :create => true)
    doc = {
      :tag => ["Ruby", "C", "Lucene", "Ferret"],
      :body => "this is the body Document Field",
      :title => "this is the title DocField",
      :author => "this is the author field"
    }
    iw << doc

    iw.close()

    @dir = Ferret::Store::RAMDirectory.new(@fs_dir)
    ir = IndexReader.new(@dir)
    assert_equal(doc, ir.get_document(0).load)
    ir.close
  end

  def do_test_term_vectors(ir)
    expected_tv = TermVector.new(:body,
      [
        TVTerm.new("word1", [2, 4, 7]),
        TVTerm.new("word2", [3]),
        TVTerm.new("word3", [0, 5, 8, 9]),
        TVTerm.new("word4", [1, 6])
      ],
      [*(0...10)].collect {|i| TVOffsets.new(i*6, (i+1)*6 - 1)})

    tv = ir.term_vector(3, :body)

    assert_equal(expected_tv, tv)

    tvs = ir.term_vectors(3)
    assert_equal(3, tvs.size)

    assert_equal(expected_tv, tvs[:body])
    
    tv = tvs[:author]
    assert_equal(:author, tv.field)
    assert_equal([TVTerm.new("Leo", [0]), TVTerm.new("Tolstoy", [1])], tv.terms)
    assert(tv.offsets.nil?)


    tv = tvs[:title]
    assert_equal(:title, tv.field)
    assert_equal([TVTerm.new("War And Peace", nil)], tv.terms)
    assert_equal([TVOffsets.new(0, 13)], tv.offsets)
  end

  def do_test_ir_read_while_optimizing(dir)
    iw = IndexWriter.new(:dir => dir,
                         :analyzer => WhiteSpaceAnalyzer.new(),
                         :create => true,
                         :field_infos => IndexTestHelper::INDEX_TEST_FIS)

    IndexTestHelper::INDEX_TEST_DOCS.each {|doc| iw << doc}

    iw.close()

    ir = IndexReader.new(dir)
    do_test_term_vectors(ir)
    
    iw = IndexWriter.new(:dir => dir, :analyzer => WhiteSpaceAnalyzer.new())
    iw.optimize()
    iw.close()

    do_test_term_vectors(ir)

    ir.close()
  end

  def test_ir_read_while_optimizing()
    do_test_ir_read_while_optimizing(@dir)
  end

  def test_ir_read_while_optimizing_on_disk()
    dpath = File.expand_path(File.join(File.dirname(__FILE__),
                       '../../temp/fsdir'))
    fs_dir = Ferret::Store::FSDirectory.new(dpath, true)
    do_test_ir_read_while_optimizing(fs_dir)
    fs_dir.close()
  end

  def test_latest()
    dpath = File.expand_path(File.join(File.dirname(__FILE__),
                       '../../temp/fsdir'))
    fs_dir = Ferret::Store::FSDirectory.new(dpath, true)

    iw = IndexWriter.new(:dir => fs_dir,
                         :analyzer => WhiteSpaceAnalyzer.new(),
                         :create => true)
    iw << {:field => "content"}
    iw.close()

    ir = IndexReader.new(fs_dir)
    assert(ir.latest?)

    iw = IndexWriter.new(:dir => fs_dir, :analyzer => WhiteSpaceAnalyzer.new())
    iw << {:field => "content2"}
    iw.close()

    assert(!ir.latest?)

    ir.close()
    ir = IndexReader.new(fs_dir)
    assert(ir.latest?)
    ir.close()
  end
end

