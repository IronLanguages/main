#############################################################################
# tc_sapi5.rb
#
# Test suite for the sapi5 class.  For now, we're simply going to test that
# the constructors work without incident.  There are simply too many methods
# for me to test.  If the constructor works, the methods should be
# available.
#
# I may eventually split each class into its own test file.
#
# You should run this test case via the 'rake test' Rake task.
#############################################################################
require "win32/sapi5"
require "test/unit"
include Win32

class TC_Win32_SAPI5 < Test::Unit::TestCase
   def setup
   end
   
   def test_version
      assert_equal("0.1.4", SAPI5::VERSION)
   end
   
   def test_SpAudioFormat
      assert_nothing_raised{ SpAudioFormat.new }
   end
   
   def test_SpCustomStream
      assert_nothing_raised{ SpCustomStream.new }
   end
   
   def test_SpFileStream
      assert_nothing_raised{ SpFileStream.new }
   end
   
   def test_SpInProcRecoContext
      assert_nothing_raised{ SpInProcRecoContext.new }
   end
   
   def test_SpInProcRecognizer
      assert_nothing_raised{ SpInProcRecognizer.new }
   end
   
   def test_SpLexicon
      assert_nothing_raised{ SpLexicon.new }
   end
   
   def test_SpMemoryStream
      assert_nothing_raised{ SpMemoryStream.new }
   end
   
   def test_SpMMAudioIn
      assert_nothing_raised{ SpMMAudioIn.new }
   end
   
   def test_SpMMAudioOut
      assert_nothing_raised{ SpMMAudioOut.new }
   end
   
   def test_SpObjectToken
      assert_nothing_raised{ SpObjectToken.new }
   end
   
   def test_SpObjectTokenCategory
      assert_nothing_raised{ SpObjectTokenCategory.new }
   end
   
   def test_SpPhoneConverter
      assert_nothing_raised{ SpPhoneConverter.new }
   end
   
   def test_SpPhraseInfoBuilder
      assert_nothing_raised{ SpPhraseInfoBuilder.new }
   end
   
   def test_SpSharedRecoContext
      assert_nothing_raised{ SpSharedRecoContext.new }
   end
   
   def test_SpSharedRecognizer
      assert_nothing_raised{ SpSharedRecognizer.new }
   end
   
   def test_SpTextSelectionInformation
      assert_nothing_raised{ SpTextSelectionInformation.new }
   end
   
   def test_SpUnCompressedLexicon
      assert_nothing_raised{ SpUnCompressedLexicon.new }
   end
   
   def test_SpVoice
      assert_nothing_raised{ SpVoice.new }
   end
   
   def test_SpWaveFormatEx
      assert_nothing_raised{ SpWaveFormatEx.new }
   end
   
   def teardown
   end
end
