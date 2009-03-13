require 'test/unit'

require 'fox16'
require 'ftools'

include Fox

class TC_FXDCPrint < Test::Unit::TestCase
private
  def printJob
    job = FXPrinter.new
    job.name = "output.ps"
    job.firstpage = 1
    job.lastpage = 1
    job.currentpage = 1
    job.frompage = 1
    job.topage = 1
    job.mediasize = MEDIA_USLETTER
    job.mediawidth = 612.0
    job.mediaheight = 792.0
    job.leftmargin = 72.0
    job.rightmargin = 72.0
    job.topmargin = 72.0
    job.bottommargin = 72.0
    job.numcopies = 1
    job.flags = PRINT_DEST_FILE
    job
  end
  
  def hexdump(ios)
    count = 0
    ios.each_byte do |byte|
      print sprintf("%02x ", byte)
      count = count + 1
      if count%8 == 0
        print "\n"
      end
    end
  end

  # Convert DOS line endings (CR+LF) to Unix (LF)
  def crlf_to_lf(text)
    text.gsub(/\x0d\x0a/, "\x0a")
  end

  def assert_same_file_contents(expected, actual)
    expected_contents, actual_contents = nil, nil
    File.open(expected, 'rb') { |f| expected_contents = f.read }
    File.open(actual, 'rb')   { |f| actual_contents = crlf_to_lf(f.read) }
    assert_equal(expected_contents, actual_contents)
  end
  
public
  def setup
    if FXApp.instance.nil?
      @app = FXApp.new('TC_FXDCPrint', 'FXRuby')
      @app.init([])
    else
      @app = FXApp.instance
    end
    @dc = FXDCPrint.new(@app)
  end

  def test_beginPrint
    @dc.beginPrint(printJob)
    @dc.endPrint
    assert_same_file_contents(File.join(File.dirname(__FILE__), "blankpage.ps"), printJob.name)
  end

  def test_beginPrint_with_block
    @dc.beginPrint(printJob) do |theDC|
      assert_same(@dc, theDC)
    end
    assert_same_file_contents(File.join(File.dirname(__FILE__), "blankpage.ps"), printJob.name)
  end

  def test_beginPage
    @dc.beginPrint(printJob)
    @dc.beginPage(1)
    @dc.drawText(100, 100, "Howdy!")
    @dc.endPage
    @dc.endPrint
#   assert_same_file_contents("howdypage.ps", printJob.name)
  end

  def test_beginPage_with_block
    @dc.beginPrint(printJob) do |theDC|
      assert_same(@dc, theDC)
      theDC.beginPage(1) do |xDC|
        assert_same(theDC, xDC)
        xDC.drawText(100, 100, "Howdy!")
      end
    end
#   assert_same_file_contents("howdypage.ps", printJob.name)
  end
  
  def teardown
    if File.exists?("output.ps")
      File.rm_f("output.ps")
    end
  end
end
