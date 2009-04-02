##########################################################################
# tc_sound.rb
#
# Test suite for the win32-sound package. You should run this test case
# via the 'rake test' task.
##########################################################################
puts "You may hear some funny noises - don't panic"
sleep 1

require "test/unit"
require "win32/sound"
include Win32

class TC_Sound < Test::Unit::TestCase
   def setup
      @wav = "c:\\windows\\media\\chimes.wav"
   end
   
   def test_version
      assert_equal("0.4.1", Sound::VERSION)
   end
   
   def test_beep
      assert_respond_to(Sound, :beep)
      assert_nothing_raised{ Sound.beep(55,100) }
      
      assert_raises(Sound::Error){ Sound.beep(0,100) }
      assert_raises(ArgumentError){ Sound.beep }
      assert_raises(ArgumentError){ Sound.beep(500) }
      assert_raises(ArgumentError){ Sound.beep(500,500,5) }
   end
   
   def test_devices
      assert_respond_to(Sound, :devices)
      assert_nothing_raised{ Sound.devices }
      
      assert_kind_of(Array,Sound.devices)
   end
   
   def test_stop
      assert_respond_to(Sound, :stop)
      assert_nothing_raised{ Sound.stop }
      assert_nothing_raised{ Sound.stop(true) }
   end
   
   def test_get_volume
      assert_respond_to(Sound, :wave_volume)
      assert_respond_to(Sound, :get_wave_volume)
      assert_nothing_raised{ Sound.get_wave_volume }
      
      assert_kind_of(Array, Sound.get_wave_volume)
      assert_equal(2, Sound.get_wave_volume.length)
   end

   def test_set_volume
      assert_respond_to(Sound, :set_wave_volume)
      assert_nothing_raised{ Sound.set_wave_volume(30000) } # About half
      assert_nothing_raised{ Sound.set_wave_volume(30000, 30000) }
   end
   
   def test_play
      assert_respond_to(Sound, :play)
      assert_nothing_raised{ Sound.play(@wav) }
      assert_nothing_raised{ Sound.play("SystemAsterisk", Sound::ALIAS) }
   end
   
   def test_expected_errors
      assert_raises(Sound::Error){ Sound.beep(-1, 1) }
   end
   
   def test_constants
      assert_not_nil(Sound::ALIAS)
      assert_not_nil(Sound::APPLICATION)
      assert_not_nil(Sound::ASYNC)
      assert_not_nil(Sound::FILENAME)
      assert_not_nil(Sound::LOOP)
      assert_not_nil(Sound::MEMORY)
      assert_not_nil(Sound::NODEFAULT)
      assert_not_nil(Sound::NOSTOP)
      assert_not_nil(Sound::NOWAIT)
      assert_not_nil(Sound::PURGE)
      assert_not_nil(Sound::SYNC)
   end
   
   def teardown
      @wav = nil
   end
end
