$LOAD_PATH.unshift("#{File.dirname(__FILE__)}/../lib") if __FILE__ == $0

require 'text/format'
require 'test/unit'

class TestText__Format < Test::Unit::TestCase
  attr_accessor :format_o

  GETTYSBURG = <<-'EOS'
  Four score and seven years ago our fathers brought forth on this
  continent a new nation, conceived in liberty and dedicated to the
  proposition that all men are created equal. Now we are engaged in
  a great civil war, testing whether that nation or any nation so
  conceived and so dedicated can long endure. We are met on a great
  battlefield of that war. We have come to dedicate a portion of
  that field as a final resting-place for those who here gave their
  lives that that nation might live. It is altogether fitting and
  proper that we should do this. But in a larger sense, we cannot
  dedicate, we cannot consecrate, we cannot hallow this ground.
  The brave men, living and dead who struggled here have consecrated
  it far above our poor power to add or detract. The world will
  little note nor long remember what we say here, but it can never
  forget what they did here. It is for us the living rather to be
  dedicated here to the unfinished work which they who fought here
  have thus far so nobly advanced. It is rather for us to be here
  dedicated to the great task remaining before us--that from these
  honored dead we take increased devotion to that cause for which
  they gave the last full measure of devotion--that we here highly
  resolve that these dead shall not have died in vain, that this
  nation under God shall have a new birth of freedom, and that
  government of the people, by the people, for the people shall
  not perish from the earth.

          -- Pres. Abraham Lincoln, 19 November 1863
  EOS

  FIVE_COL = "Four \nscore\nand s\neven \nyears\nago o\nur fa\nthers\nbroug\nht fo\nrth o\nn thi\ns con\ntinen\nt a n\new na\ntion,\nconce\nived \nin li\nberty\nand d\nedica\nted t\no the\npropo\nsitio\nn tha\nt all\nmen a\nre cr\neated\nequal\n. Now\nwe ar\ne eng\naged \nin a \ngreat\ncivil\nwar, \ntesti\nng wh\nether\nthat \nnatio\nn or \nany n\nation\nso co\nnceiv\ned an\nd so \ndedic\nated \ncan l\nong e\nndure\n. We \nare m\net on\na gre\nat ba\nttlef\nield \nof th\nat wa\nr. We\nhave \ncome \nto de\ndicat\ne a p\nortio\nn of \nthat \nfield\nas a \nfinal\nresti\nng-pl\nace f\nor th\nose w\nho he\nre ga\nve th\neir l\nives \nthat \nthat \nnatio\nn mig\nht li\nve. I\nt is \naltog\nether\nfitti\nng an\nd pro\nper t\nhat w\ne sho\nuld d\no thi\ns. Bu\nt in \na lar\nger s\nense,\nwe ca\nnnot \ndedic\nate, \nwe ca\nnnot \nconse\ncrate\n, we \ncanno\nt hal\nlow t\nhis g\nround\n. The\nbrave\nmen, \nlivin\ng and\ndead \nwho s\ntrugg\nled h\nere h\nave c\nonsec\nrated\nit fa\nr abo\nve ou\nr poo\nr pow\ner to\nadd o\nr det\nract.\nThe w\norld \nwill \nlittl\ne not\ne nor\nlong \nremem\nber w\nhat w\ne say\nhere,\nbut i\nt can\nnever\nforge\nt wha\nt the\ny did\nhere.\nIt is\nfor u\ns the\nlivin\ng rat\nher t\no be \ndedic\nated \nhere \nto th\ne unf\ninish\ned wo\nrk wh\nich t\nhey w\nho fo\nught \nhere \nhave \nthus \nfar s\no nob\nly ad\nvance\nd. It\nis ra\nther \nfor u\ns to \nbe he\nre de\ndicat\ned to\nthe g\nreat \ntask \nremai\nning \nbefor\ne us-\n-that\nfrom \nthese\nhonor\ned de\nad we\ntake \nincre\nased \ndevot\nion t\no tha\nt cau\nse fo\nr whi\nch th\ney ga\nve th\ne las\nt ful\nl mea\nsure \nof de\nvotio\nn--th\nat we\nhere \nhighl\ny res\nolve \nthat \nthese\ndead \nshall\nnot h\nave d\nied i\nn vai\nn, th\nat th\nis na\ntion \nunder\nGod s\nhall \nhave \na new\nbirth\nof fr\needom\n, and\nthat \ngover\nnment\nof th\ne peo\nple, \nby th\ne peo\nple, \nfor t\nhe pe\nople \nshall\nnot p\nerish\nfrom \nthe e\narth.\n-- Pr\nes. A\nbraha\nm Lin\ncoln,\n19 No\nvembe\nr 186\n3    \n"

  FIVE_CNF = "Four \nscore\nand s\neven \nyears\nago o\nur f\\\nathe\\\nrs b\\\nroug\\\nht f\\\north \non t\\\nhis c\nonti\\\nnent \na new\nnati\\\non, c\nonce\\\nived \nin l\\\niber\\\nty a\\\nnd d\\\nedic\\\nated \nto t\\\nhe p\\\nropo\\\nsiti\\\non t\\\nhat a\nll m\\\nen a\\\nre c\\\nreat\\\ned e\\\nqual.\nNow w\ne are\nenga\\\nged i\nn a g\nreat \ncivil\nwar, \ntest\\\ning w\nheth\\\ner t\\\nhat n\nation\nor a\\\nny n\\\nation\nso c\\\nonce\\\nived \nand s\no de\\\ndica\\\nted c\nan l\\\nong e\nndur\\\ne. We\nare m\net on\na gr\\\neat b\nattl\\\nefie\\\nld of\nthat \nwar. \nWe h\\\nave c\nome t\no de\\\ndica\\\nte a \nport\\\nion o\nf th\\\nat f\\\nield \nas a \nfinal\nrest\\\ning-\\\nplace\nfor t\nhose \nwho h\nere g\nave t\nheir \nlives\nthat \nthat \nnati\\\non m\\\night \nlive.\nIt is\nalto\\\ngeth\\\ner f\\\nitti\\\nng a\\\nnd p\\\nroper\nthat \nwe s\\\nhould\ndo t\\\nhis. \nBut i\nn a l\narger\nsens\\\ne, we\ncann\\\not d\\\nedic\\\nate, \nwe c\\\nannot\ncons\\\necra\\\nte, w\ne ca\\\nnnot \nhall\\\now t\\\nhis g\nroun\\\nd. T\\\nhe b\\\nrave \nmen, \nlivi\\\nng a\\\nnd d\\\nead w\nho s\\\ntrug\\\ngled \nhere \nhave \ncons\\\necra\\\nted i\nt far\nabove\nour p\noor p\nower \nto a\\\ndd or\ndetr\\\nact. \nThe w\norld \nwill \nlitt\\\nle n\\\note n\nor l\\\nong r\nemem\\\nber w\nhat w\ne say\nhere,\nbut i\nt can\nnever\nforg\\\net w\\\nhat t\nhey d\nid h\\\nere. \nIt is\nfor u\ns the\nlivi\\\nng r\\\nather\nto be\ndedi\\\ncated\nhere \nto t\\\nhe u\\\nnfin\\\nished\nwork \nwhich\nthey \nwho f\nought\nhere \nhave \nthus \nfar s\no no\\\nbly a\ndvan\\\nced. \nIt is\nrath\\\ner f\\\nor us\nto be\nhere \ndedi\\\ncated\nto t\\\nhe g\\\nreat \ntask \nrema\\\nining\nbefo\\\nre u\\\ns--t\\\nhat f\nrom t\nhese \nhono\\\nred d\nead w\ne ta\\\nke i\\\nncre\\\nased \ndevo\\\ntion \nto t\\\nhat c\nause \nfor w\nhich \nthey \ngave \nthe l\nast f\null m\neasu\\\nre of\ndevo\\\ntion\\\n--th\\\nat we\nhere \nhigh\\\nly r\\\nesol\\\nve t\\\nhat t\nhese \ndead \nshall\nnot h\nave d\nied i\nn va\\\nin, t\nhat t\nhis n\nation\nunder\nGod s\nhall \nhave \na new\nbirth\nof f\\\nreed\\\nom, a\nnd t\\\nhat g\nover\\\nnment\nof t\\\nhe p\\\neopl\\\ne, by\nthe p\neopl\\\ne, f\\\nor t\\\nhe p\\\neople\nshall\nnot p\nerish\nfrom \nthe e\narth.\n-- P\\\nres. \nAbra\\\nham L\ninco\\\nln, 1\n9 No\\\nvemb\\\ner 1\\\n863  \n"

  FIVE_CNT = "Four \nscore\nand  \nseven\nyears\nago  \nour f\nathe\\\nrs b\\\nroug\\\nht f\\\north \non t\\\nhis c\nonti\\\nnent \na new\nnati\\\non, c\nonce\\\nived \nin l\\\niber\\\nty a\\\nnd d\\\nedic\\\nated \nto t\\\nhe p\\\nropo\\\nsiti\\\non t\\\nhat  \nall  \nmen  \nare c\nreat\\\ned e\\\nqual.\nNow  \nwe a\\\nre e\\\nngag\\\ned in\na gr\\\neat  \ncivil\nwar, \ntest\\\ning w\nheth\\\ner t\\\nhat n\nation\nor a\\\nny n\\\nation\nso c\\\nonce\\\nived \nand  \nso d\\\nedic\\\nated \ncan  \nlong \nendu\\\nre.  \nWe a\\\nre m\\\net on\na gr\\\neat b\nattl\\\nefie\\\nld of\nthat \nwar. \nWe h\\\nave  \ncome \nto d\\\nedic\\\nate a\nport\\\nion  \nof t\\\nhat  \nfield\nas a \nfinal\nrest\\\ning-\\\nplace\nfor  \nthose\nwho  \nhere \ngave \ntheir\nlives\nthat \nthat \nnati\\\non m\\\night \nlive.\nIt is\nalto\\\ngeth\\\ner f\\\nitti\\\nng a\\\nnd p\\\nroper\nthat \nwe s\\\nhould\ndo t\\\nhis. \nBut  \nin a \nlarg\\\ner s\\\nense,\nwe c\\\nannot\ndedi\\\ncate,\nwe c\\\nannot\ncons\\\necra\\\nte,  \nwe c\\\nannot\nhall\\\now t\\\nhis g\nroun\\\nd. T\\\nhe b\\\nrave \nmen, \nlivi\\\nng a\\\nnd d\\\nead  \nwho s\ntrug\\\ngled \nhere \nhave \ncons\\\necra\\\nted  \nit f\\\nar a\\\nbove \nour  \npoor \npower\nto a\\\ndd or\ndetr\\\nact. \nThe  \nworld\nwill \nlitt\\\nle n\\\note  \nnor  \nlong \nreme\\\nmber \nwhat \nwe s\\\nay h\\\nere, \nbut  \nit c\\\nan n\\\never \nforg\\\net w\\\nhat  \nthey \ndid  \nhere.\nIt is\nfor  \nus t\\\nhe l\\\niving\nrath\\\ner to\nbe d\\\nedic\\\nated \nhere \nto t\\\nhe u\\\nnfin\\\nished\nwork \nwhich\nthey \nwho f\nought\nhere \nhave \nthus \nfar  \nso n\\\nobly \nadva\\\nnced.\nIt is\nrath\\\ner f\\\nor us\nto be\nhere \ndedi\\\ncated\nto t\\\nhe g\\\nreat \ntask \nrema\\\nining\nbefo\\\nre u\\\ns--t\\\nhat  \nfrom \nthese\nhono\\\nred  \ndead \nwe t\\\nake i\nncre\\\nased \ndevo\\\ntion \nto t\\\nhat  \ncause\nfor  \nwhich\nthey \ngave \nthe  \nlast \nfull \nmeas\\\nure  \nof d\\\nevot\\\nion-\\\n-that\nwe h\\\nere h\nighly\nreso\\\nlve  \nthat \nthese\ndead \nshall\nnot  \nhave \ndied \nin v\\\nain, \nthat \nthis \nnati\\\non u\\\nnder \nGod  \nshall\nhave \na new\nbirth\nof f\\\nreed\\\nom,  \nand  \nthat \ngove\\\nrnme\\\nnt of\nthe p\neopl\\\ne, by\nthe p\neopl\\\ne, f\\\nor t\\\nhe p\\\neople\nshall\nnot p\nerish\nfrom \nthe e\narth.\n-- P\\\nres. \nAbra\\\nham L\ninco\\\nln,  \n19 N\\\novem\\\nber  \n1863 \n"

    # Tests both abbreviations and abbreviations=
  def test_abbreviations
    abbr = ["    Pres. Abraham Lincoln\n", "    Pres.  Abraham Lincoln\n"]

    @format_o = Text::Format.new
    assert_equal([], @format_o.abbreviations)

    @format_o.abbreviations = [ 'foo', 'bar' ]
    assert_equal([ 'foo', 'bar' ], @format_o.abbreviations)
    assert_equal(abbr[0], @format_o.format(abbr[0]))

    @format_o.extra_space = true
    assert_equal(abbr[1], @format_o.format(abbr[0]))

    @format_o.abbreviations = [ "Pres" ]
    assert_equal([ "Pres" ], @format_o.abbreviations)
    assert_equal(abbr[0], @format_o.format(abbr[0]))

    @format_o.extra_space = false
    assert_equal(abbr[0], @format_o.format(abbr[0]))
  end

    # Tests both body_indent and body_indent=
  def test_body_indent
    @format_o = Text::Format.new
    assert_equal(0, @format_o.body_indent)

    @format_o.body_indent = 7
    assert_equal(7, @format_o.body_indent)

    @format_o.body_indent = -3
    assert_equal(3, @format_o.body_indent)

    @format_o.body_indent = "9"
    assert_equal(9, @format_o.body_indent)

    @format_o.body_indent = "-2"
    assert_equal(2, @format_o.body_indent)
    assert_match(/^  [^ ]/, @format_o.format(GETTYSBURG).split("\n")[1])
  end

    # Tests both columns and columns=
  def test_columns
    @format_o = Text::Format.new
    assert_equal(72, @format_o.columns)

    @format_o.columns = 7
    assert_equal(7, @format_o.columns)

    @format_o.columns = -3
    assert_equal(3, @format_o.columns)

    @format_o.columns = "9"
    assert_equal(9, @format_o.columns)

    @format_o.columns = "-2"
    assert_equal(2, @format_o.columns)

    @format_o.columns = 40
    assert_equal(40, @format_o.columns)
    assert_match(/this continent$/,
                  @format_o.format(GETTYSBURG).split("\n")[1])
  end

    # Tests both extra_space and extra_space=
  def test_extra_space
    @format_o = Text::Format.new
    assert_equal(false, @format_o.extra_space)

    @format_o.extra_space = true
    assert_equal(true, @format_o.extra_space)
  end

    # Tests both first_indent and first_indent=
  def test_first_indent
    @format_o = Text::Format.new
    assert_equal(4, @format_o.first_indent)

    @format_o.first_indent = 7
    assert_equal(7, @format_o.first_indent)

    @format_o.first_indent = -3
    assert_equal(3, @format_o.first_indent)

    @format_o.first_indent = "9"
    assert_equal(9, @format_o.first_indent)

    @format_o.first_indent = "-2"
    assert_equal(2, @format_o.first_indent)
    assert_match(/^  [^ ]/, @format_o.format(GETTYSBURG).split("\n")[0])
  end

  def test_format_style
    @format_o = Text::Format.new
    assert_equal(Text::Format::LEFT_ALIGN, @format_o.format_style)
    assert_match(/^November 1863$/, @format_o.format(GETTYSBURG).split("\n")[-1])

    @format_o.format_style = Text::Format::RIGHT_ALIGN
    assert_equal(Text::Format::RIGHT_ALIGN, @format_o.format_style)
    assert_match(/^ +November 1863$/, @format_o.format(GETTYSBURG).split("\n")[-1])

    @format_o.format_style = Text::Format::RIGHT_FILL
    assert_equal(Text::Format::RIGHT_FILL, @format_o.format_style)
    assert_match(/^November 1863 +$/, @format_o.format(GETTYSBURG).split("\n")[-1])

    @format_o.format_style = Text::Format::JUSTIFY
    assert_equal(Text::Format::JUSTIFY, @format_o.format_style)
    assert_match(/^of freedom, and that government of the people, by the  people,  for  the$/, @format_o.format(GETTYSBURG).split("\n")[-3])
    assert_raises(ArgumentError) { @format_o.format_style = 33 }
  end

  def test_tag_paragraph
    @format_o = Text::Format.new
    assert_equal(false, @format_o.tag_paragraph)

    @format_o.tag_paragraph = true
    assert_equal(true, @format_o.tag_paragraph)
    assert_not_equal(@format_o.paragraphs([GETTYSBURG, GETTYSBURG]),
                      Text::Format.new.paragraphs([GETTYSBURG, GETTYSBURG]))
  end

  def test_tag_text
    @format_o = Text::Format.new
    assert_equal([], @format_o.tag_text)
    assert_equal(@format_o.format(GETTYSBURG),
                  Text::Format.new.format(GETTYSBURG))

    @format_o.tag_paragraph = true
    @format_o.tag_text = ["Gettysburg Address", "---"]

    assert_not_equal(@format_o.format(GETTYSBURG), Text::Format.new.format(GETTYSBURG))
    assert_not_equal(@format_o.paragraphs([GETTYSBURG, GETTYSBURG]), Text::Format.new.paragraphs([GETTYSBURG, GETTYSBURG]))
    assert_not_equal(@format_o.paragraphs([GETTYSBURG, GETTYSBURG, GETTYSBURG]), Text::Format.new.paragraphs([GETTYSBURG, GETTYSBURG, GETTYSBURG]))
  end

  def test_justify?
    @format_o = Text::Format.new
    assert_equal(false, @format_o.justify?)

    @format_o.format_style = Text::Format::RIGHT_ALIGN
    assert_equal(false, @format_o.justify?)

    @format_o.format_style = Text::Format::RIGHT_FILL
    assert_equal(false, @format_o.justify?)

    @format_o.format_style = Text::Format::JUSTIFY
    assert_equal(true, @format_o.justify?)
      # The format testing is done in _format_style
  end

  def test_left_align?
    @format_o = Text::Format.new
    assert_equal(true, @format_o.left_align?)

    @format_o.format_style = Text::Format::RIGHT_ALIGN
    assert_equal(false, @format_o.left_align?)

    @format_o.format_style = Text::Format::RIGHT_FILL
    assert_equal(false, @format_o.left_align?)

    @format_o.format_style = Text::Format::JUSTIFY
    assert_equal(false, @format_o.left_align?)
      # The format testing is done in _format_style
  end

  def test_left_margin
    @format_o = Text::Format.new
    assert_equal(0, @format_o.left_margin)

    @format_o.left_margin = -3
    assert_equal(3, @format_o.left_margin)

    @format_o.left_margin = "9"
    assert_equal(9, @format_o.left_margin)

    @format_o.left_margin = "-2"
    assert_equal(2, @format_o.left_margin)

    @format_o.left_margin = 7
    assert_equal(7, @format_o.left_margin)

    ft = @format_o.format(GETTYSBURG).split("\n")
    assert_match(/^ {11}Four score/, ft[0])
    assert_match(/^ {7}November/, ft[-1])
  end

  def test_hard_margins
    @format_o = Text::Format.new
    assert_equal(false, @format_o.hard_margins)

    @format_o.hard_margins = true
    @format_o.columns = 5
    @format_o.first_indent = 0
    @format_o.format_style = Text::Format::RIGHT_FILL
    assert_equal(true, @format_o.hard_margins)
    assert_equal(FIVE_COL, @format_o.format(GETTYSBURG))

    @format_o.split_rules |= Text::Format::SPLIT_CONTINUATION
    assert_equal(Text::Format::SPLIT_CONTINUATION_FIXED, @format_o.split_rules)
    assert_equal(FIVE_CNF, @format_o.format(GETTYSBURG))

    @format_o.split_rules = Text::Format::SPLIT_CONTINUATION
    assert_equal(Text::Format::SPLIT_CONTINUATION, @format_o.split_rules)
    assert_equal(FIVE_CNT, @format_o.format(GETTYSBURG))
  end

    # Tests both nobreak and nobreak_regex, since one is only useful
    # with the other.
  def test_nobreak
    @format_o = Text::Format.new
    assert_equal(false, @format_o.nobreak)
    assert_equal(true, @format_o.nobreak_regex.empty?)

    @format_o.nobreak = true
    @format_o.nobreak_regex = { %r{this} => %r{continent} }
    @format_o.columns = 77
    assert_equal(true, @format_o.nobreak)
    assert_equal({ %r{this} => %r{continent} }, @format_o.nobreak_regex)
    assert_match(/^this continent/,
                  @format_o.format(GETTYSBURG).split("\n")[1])
  end

  def test_right_align?
    @format_o = Text::Format.new
    assert_equal(false, @format_o.right_align?)

    @format_o.format_style = Text::Format::RIGHT_ALIGN
    assert_equal(true, @format_o.right_align?)

    @format_o.format_style = Text::Format::RIGHT_FILL
    assert_equal(false, @format_o.right_align?)

    @format_o.format_style = Text::Format::JUSTIFY
    assert_equal(false, @format_o.right_align?)
      # The format testing is done in _format_style
  end

  def test_right_fill?
    @format_o = Text::Format.new
    assert_equal(false, @format_o.right_fill?)

    @format_o.format_style = Text::Format::RIGHT_ALIGN
    assert_equal(false, @format_o.right_fill?)

    @format_o.format_style = Text::Format::RIGHT_FILL
    assert_equal(true, @format_o.right_fill?)

    @format_o.format_style = Text::Format::JUSTIFY
    assert_equal(false, @format_o.right_fill?)
      # The format testing is done in _format_style
  end

  def test_right_margin
    @format_o = Text::Format.new
    assert_equal(0, @format_o.right_margin)

    @format_o.right_margin = -3
    assert_equal(3, @format_o.right_margin)

    @format_o.right_margin = "9"
    assert_equal(9, @format_o.right_margin)

    @format_o.right_margin = "-2"
    assert_equal(2, @format_o.right_margin)

    @format_o.right_margin = 7
    assert_equal(7, @format_o.right_margin)

    ft = @format_o.format(GETTYSBURG).split("\n")
    assert_match(/^ {4}Four score.*forth on$/, ft[0])
    assert_match(/^November/, ft[-1])
  end

  def test_tabstop
    @format_o = Text::Format.new
    assert_equal(8, @format_o.tabstop)

    @format_o.tabstop = 7
    assert_equal(7, @format_o.tabstop)

    @format_o.tabstop = -3
    assert_equal(3, @format_o.tabstop)

    @format_o.tabstop = "9"
    assert_equal(9, @format_o.tabstop)

    @format_o.tabstop = "-2"
    assert_equal(2, @format_o.tabstop)
  end

  def test_text
    @format_o = Text::Format.new
    assert_equal([], @format_o.text)

    @format_o.text = "Test Text"
    assert_equal("Test Text", @format_o.text)

    @format_o.text = ["Line 1", "Line 2"]
    assert_equal(["Line 1", "Line 2"], @format_o.text)
  end

  def test_new
    @format_o = Text::Format.new { |fo| fo.text = "Test 1, 2, 3" }
    assert_equal("Test 1, 2, 3", @format_o.text)

    @format_o = Text::Format.new(:columns => 79)
    assert_equal(79, @format_o.columns)

    @format_o = Text::Format.new(:columns => 80) { |fo| fo.text = "Test 4, 5, 6" }
    assert_equal("Test 4, 5, 6", @format_o.text)
    assert_equal(80, @format_o.columns)

    @format_o = Text::Format.new(:text => "Test A, B, C")
    assert_equal("Test A, B, C", @format_o.text)

    @format_o = Text::Format.new(:text => "Test X, Y, Z") { |fo| fo.columns = -5 }
    assert_equal("Test X, Y, Z", @format_o.text)
    assert_equal(5, @format_o.columns)
  end

  def test_center
    @format_o = Text::Format.new

    ct = @format_o.center(GETTYSBURG.split("\n")).split("\n")
    assert_match(/^    Four score and seven years ago our fathers brought forth on this/, ct[0])
    assert_match(/^                       not perish from the earth./, ct[-3])
  end

  def test_expand
    @format_o = Text::Format.new
    assert_equal("          ", @format_o.expand("\t  "))

    @format_o.tabstop = 4
    assert_equal("      ", @format_o.expand("\t  "))
  end

  def test_unexpand
    @format_o = Text::Format.new
    assert_equal("\t  ", @format_o.unexpand("          "))

    @format_o.tabstop = 4
    assert_equal("\t  ", @format_o.unexpand("      "))
  end

  def test_space_only
    assert_equal("", Text::Format.new.format(" "))
    assert_equal("", Text::Format.new.format("\n"))
    assert_equal("", Text::Format.new.format("        "))
    assert_equal("", Text::Format.new.format("    \n"))
    assert_equal("", Text::Format.new.paragraphs("\n"))
    assert_equal("", Text::Format.new.paragraphs(" "))
    assert_equal("", Text::Format.new.paragraphs("        "))
    assert_equal("", Text::Format.new.paragraphs("    \n"))
    assert_equal("", Text::Format.new.paragraphs(["\n"]))
    assert_equal("", Text::Format.new.paragraphs([" "]))
    assert_equal("", Text::Format.new.paragraphs(["        "]))
    assert_equal("", Text::Format.new.paragraphs(["    \n"]))
  end

  def test_splendiferous
    h = nil
    test = "This is a splendiferous test"
    @format_o = Text::Format.new(:columns => 6, :left_margin => 0, :indent => 0, :first_indent => 0)
    assert_match(/^splendiferous$/, @format_o.format(test))

    @format_o.hard_margins = true
    assert_match(/^lendif$/, @format_o.format(test))

    h = Object.new

    @format_o.split_rules = Text::Format::SPLIT_HYPHENATION
    class << h
      def hyphenate_to(word, size)
        return [nil, word] if size < 2
        [word[0 ... size], word[size .. -1]]
      end
    end
    @format_o.hyphenator = h
    assert_match(/^ferous$/, @format_o.format(test))

    h = Object.new

    class << h
      def hyphenate_to(word, size, formatter)
        return [nil, word] if word.size < formatter.columns
        [word[0 ... size], word[size .. -1]]
      end
    end
    @format_o.hyphenator = h
    assert_match(/^ferous$/, @format_o.format(test))
  end

  def test_encephelogram
    hy = nil

    begin
      require 'text/hyphen'
      hy = Text::Hyphen.new
    rescue LoadError
      begin
        require 'rubygems'
        require 'text/hyphen'
        hy = Text::Hyphen.new
      rescue LoadError
        begin
          require 'tex/hyphen'
          hy = TeX::Hyphen.new
        rescue LoadError
          print 'S'
          return true
        end
      end
    end

    tx = "something pancakes electroencephalogram"
    fo = Text::Format.new(:body_indent  => 15,
                          :columns      => 30,
                          :hard_margins => true,
                          :split_rules  => Text::Format::SPLIT_HYPHENATION,
                          :hyphenator   => hy,
                          :text         => tx)
    res = fo.paragraphs
    exp = <<-EOS
    something pancakes elec-
               troencephalo-
               gram
    EOS
    exp.chomp!

    assert_equal(exp, res)
  end
end
