require File.dirname(__FILE__) + "/../../test_helper"

class QueryParserTest < Test::Unit::TestCase
  include Ferret::Analysis

  def test_strings()
    parser = Ferret::QueryParser.new(:default_field => "xxx",
                                     :fields => ["xxx", "field", "f1", "f2"],
                                     :tokenized_fields => ["xxx", "f1", "f2"])
    pairs = [
      ['', ''],
      ['*:word', 'word field:word f1:word f2:word'],
      ['word', 'word'],
      ['field:word', 'field:word'],
      ['"word1 word2 word#"', '"word1 word2 word"'],
      ['"word1 %%% word3"', '"word1 <> word3"~1'],
      ['field:"one two three"', 'field:"one two three"'],
      ['field:"one %%% three"', 'field:"one %%% three"'],
      ['f1:"one %%% three"', 'f1:"one <> three"~1'],
      ['field:"one <> three"', 'field:"one <> three"'],
      ['field:"one <> three <>"', 'field:"one <> three"'],
      ['field:"one <> <> <> three <>"', 'field:"one <> <> <> three"'],
      ['field:"one <> 222 <> three|four|five <>"', 'field:"one <> 222 <> three|four|five"'],
      ['field:"on1|tw2 THREE|four|five six|seven"', 'field:"on1|tw2 THREE|four|five six|seven"'],
      ['field:"testing|trucks"', 'field:"testing|trucks"'],
      ['[aaa bbb]', '[aaa bbb]'],
      ['{aaa bbb]', '{aaa bbb]'],
      ['field:[aaa bbb}', 'field:[aaa bbb}'],
      ['{aaa bbb}', '{aaa bbb}'],
      ['{aaa>', '{aaa>'],
      ['[aaa>', '[aaa>'],
      ['field:<a\ aa}', 'field:<a aa}'],
      ['<aaa]', '<aaa]'],
      ['>aaa', '{aaa>'],
      ['>=aaa', '[aaa>'],
      ['<aaa', '<aaa}'],
      ['[A>', '[a>'],
      ['field:<=aaa', 'field:<aaa]'],
      ['REQ one REQ two', '+one +two'],
      ['REQ one two', '+one two'],
      ['one REQ two', 'one +two'],
      ['+one +two', '+one +two'],
      ['+one two', '+one two'],
      ['one +two', 'one +two'],
      ['-one -two', '-one -two'],
      ['-one two', '-one two'],
      ['one -two', 'one -two'],
      ['!one !two', '-one -two'],
      ['!one two', '-one two'],
      ['one !two', 'one -two'],
      ['NOT one NOT two', '-one -two'],
      ['NOT one two', '-one two'],
      ['one NOT two', 'one -two'],
      ['NOT two', '-two +*'],
      ['one two', 'one two'],
      ['one OR two', 'one two'],
      ['one AND two', '+one +two'],
      ['one two AND three', 'one two +three'],
      ['one two OR three', 'one two three'],
      ['one (two AND three)', 'one (+two +three)'],
      ['one AND (two OR three)', '+one +(two three)'],
      ['field:(one AND (two OR three))', '+field:one +(field:two field:three)'],
      ['one AND (two OR [aaa vvv})', '+one +(two [aaa vvv})'],
      ['one AND (f1:two OR f2:three) AND four', '+one +(f1:two f2:three) +four'],
      ['one^1.23', 'one^1.23'],
      ['(one AND two)^100.23', '(+one +two)^100.23'],
      ['field:(one AND two)^100.23', '(+field:one +field:two)^100.23'],
      ['field:(one AND [aaa bbb]^23.3)^100.23', '(+field:one +field:[aaa bbb]^23.3)^100.23'],
      ['(REQ field:"one two three")^23', 'field:"one two three"^23.0'],
      ['asdf~0.2', 'asdf~0.2'],
      ['field:asdf~0.2', 'field:asdf~0.2'],
      ['asdf~0.2^100.0', 'asdf~0.2^100.0'],
      ['field:asdf~0.2^0.1', 'field:asdf~0.2^0.1'],
      ['field:"asdf <> asdf|asdf"~4', 'field:"asdf <> asdf|asdf"~4'],
      ['"one two three four five"~5', '"one two three four five"~5'],
      ['ab?de', 'ab?de'],
      ['ab*de', 'ab*de'],
      ['asdf?*?asd*dsf?asfd*asdf?', 'asdf?*?asd*dsf?asfd*asdf?'],
      ['field:a* AND field:(b*)', '+field:a* +field:b*'],
      ['field:abc~ AND field:(b*)', '+field:abc~ +field:b*'],
      ['asdf?*?asd*dsf?asfd*asdf?^20.0', 'asdf?*?asd*dsf?asfd*asdf?^20.0'],

      ['*:xxx', 'xxx field:xxx f1:xxx f2:xxx'],
      ['f1|f2:xxx', 'f1:xxx f2:xxx'],

      ['*:asd~0.2', 'asd~0.2 field:asd~0.2 f1:asd~0.2 f2:asd~0.2'],
      ['f1|f2:asd~0.2', 'f1:asd~0.2 f2:asd~0.2'],

      ['*:a?d*^20.0', '(a?d* field:a?d* f1:a?d* f2:a?d*)^20.0'],
      ['f1|f2:a?d*^20.0', '(f1:a?d* f2:a?d*)^20.0'],

      ['*:"asdf <> xxx|yyy"', '"asdf <> xxx|yyy" field:"asdf <> xxx|yyy" f1:"asdf <> xxx|yyy" f2:"asdf <> xxx|yyy"'],
      ['f1|f2:"asdf <> xxx|yyy"', 'f1:"asdf <> xxx|yyy" f2:"asdf <> xxx|yyy"'],
      ['f1|f2:"asdf <> do|yyy"', 'f1:"asdf <> yyy" f2:"asdf <> yyy"'],
      ['f1|f2:"do|cat"', 'f1:cat f2:cat'],

      ['*:[bbb xxx]', '[bbb xxx] field:[bbb xxx] f1:[bbb xxx] f2:[bbb xxx]'],
      ['f1|f2:[bbb xxx]', 'f1:[bbb xxx] f2:[bbb xxx]'],

      ['*:(xxx AND bbb)', '+(xxx field:xxx f1:xxx f2:xxx) +(bbb field:bbb f1:bbb f2:bbb)'],
      ['f1|f2:(xxx AND bbb)', '+(f1:xxx f2:xxx) +(f1:bbb f2:bbb)'],
      ['asdf?*?asd*dsf?asfd*asdf?^20.0', 'asdf?*?asd*dsf?asfd*asdf?^20.0'],
      ['"onewordphrase"', 'onewordphrase'],
      ["who'd", "who'd"]
    ]
      
    pairs.each do |query_str, expected|
      assert_equal(expected, parser.parse(query_str).to_s("xxx"))
    end
  end

  def test_qp_with_standard_analyzer()
    parser = Ferret::QueryParser.new(:default_field => "xxx",
                                     :fields => ["xxx", "key"],
                                     :analyzer => StandardAnalyzer.new)
    pairs = [
      ['key:1234', 'key:1234'],
      ['key:(1234 and Dave)', 'key:1234 key:dave'],
      ['key:(1234)', 'key:1234'],
      ['and the but they with', '']
    ]
      
    pairs.each do |query_str, expected|
      assert_equal(expected, parser.parse(query_str).to_s("xxx"))
    end

  end

  def test_qp_changing_fields()
    parser = Ferret::QueryParser.new(:default_field => "xxx",
                                     :fields => ["xxx", "key"],
                                     :analyzer => WhiteSpaceAnalyzer.new)
    assert_equal('word key:word', parser.parse("*:word").to_s("xxx"))

    parser.fields = ["xxx", "one", "two", "three"]
    assert_equal('word one:word two:word three:word',
                 parser.parse("*:word").to_s("xxx"))
    assert_equal('three:word four:word',
                 parser.parse("three:word four:word").to_s("xxx"))
  end

  def test_qp_allow_any_field()
    parser = Ferret::QueryParser.new(:default_field => "xxx",
                                     :fields => ["xxx", "key"],
                                     :analyzer => WhiteSpaceAnalyzer.new,
                                     :validate_fields => true)

    assert_equal('key:word',
                 parser.parse("key:word song:word").to_s("xxx"))
    assert_equal('word key:word', parser.parse("*:word").to_s("xxx"))


    parser = Ferret::QueryParser.new(:default_field => "xxx",
                                     :fields => ["xxx", "key"],
                                     :analyzer => WhiteSpaceAnalyzer.new)

    assert_equal('key:word song:word',
                 parser.parse("key:word song:word").to_s("xxx"))
    assert_equal('word key:word', parser.parse("*:word").to_s("xxx"))
  end
  
  def do_test_query_parse_exception_raised(str)
    parser = Ferret::QueryParser.new(:default_field => "xxx",
                                     :fields => ["f1", "f2", "f3"],
                                     :handle_parse_errors => false)
    assert_raise(Ferret::QueryParser::QueryParseException,
                 str + " should have failed") do
      parser.parse(str)
    end
  end

  def test_or_default
    parser = Ferret::QueryParser.new(:default_field => :*,
                                     :fields => [:x, :y],
                                     :or_default => false,
                                     :analyzer => StandardAnalyzer.new)
    pairs = [
      ['word', 'x:word y:word'],
      ['word1 word2', '+(x:word1 y:word1) +(x:word2 y:word2)']
    ]
      
    pairs.each do |query_str, expected|
      assert_equal(expected, parser.parse(query_str).to_s(""))
    end
  end

  def test_prefix_query
    parser = Ferret::QueryParser.new(:default_field => "xxx",
                                     :fields => ["xxx"],
                                     :analyzer => StandardAnalyzer.new)
    assert_equal(Ferret::Search::PrefixQuery, parser.parse("asdg*").class)
    assert_equal(Ferret::Search::WildcardQuery, parser.parse("a?dg*").class)
    assert_equal(Ferret::Search::WildcardQuery, parser.parse("a*dg*").class)
    assert_equal(Ferret::Search::WildcardQuery, parser.parse("adg*c").class)
  end
  
  def test_bad_queries
    parser = Ferret::QueryParser.new(:default_field => "xxx",
                                     :fields => ["f1", "f2"])

    pairs = [
      ['::*word', 'word'],
      ['::*&)(*^&*(', ''],
      ['::*&one)(*two(*&"', '"one two"~1'],
      [':', ''],
      ['[, ]', ''],
      ['{, }', ''],
      ['!', ''],
      ['+', ''],
      ['~', ''],
      ['^', ''],
      ['-', ''],
      ['|', ''],
      ['<, >', ''],
      ['=', ''],
      ['<script>', 'script']
    ]
      
    pairs.each do |query_str, expected|
      do_test_query_parse_exception_raised(query_str)
      assert_equal(expected, parser.parse(query_str).to_s("xxx"))
    end
  end

  def test_use_keywords_switch
    analyzer = LetterAnalyzer.new
    parser = Ferret::QueryParser.new(:analyzer => analyzer,
                                     :default_field => "xxx")
    assert_equal("+www (+xxx +yyy) -zzz",
                 parser.parse("REQ www (xxx AND yyy) OR NOT zzz").to_s("xxx"))

    parser = Ferret::QueryParser.new(:analyzer => analyzer,
                                     :default_field => "xxx",
                                     :use_keywords => false)
    assert_equal("req www (xxx and yyy) or not zzz",
                 parser.parse("REQ www (xxx AND yyy) OR NOT zzz").to_s("xxx"))
  end
end
