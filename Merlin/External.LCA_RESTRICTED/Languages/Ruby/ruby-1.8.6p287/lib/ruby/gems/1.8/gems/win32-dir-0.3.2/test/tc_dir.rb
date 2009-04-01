###########################################################################
# tc_dir.rb
#
# Test suite for the win32-dir package.  Note that some of these tests
# may fail, because some constants are simply not defined, depending on
# your operating system and version of certain DLL files.
#
# You should run this test case via the 'rake test' task.
###########################################################################
require 'test/unit'
require 'win32/dir'
require 'fileutils'

puts "Some tests may fail because some constants aren't defined on your system."
puts "This is not unexpected."

class TC_Win32_Dir < Test::Unit::TestCase
   def setup
      @from       = "from"
      @ascii_to   = "to"
      @unicode_to = "Ελλάσ" # Greek - the word is 'Hellas'
      Dir.mkdir(@from)
   end
   
   def test_version
      assert_equal('0.3.2', Dir::VERSION)
   end
  
   def test_create_junction
      assert_respond_to(Dir, :create_junction)
      assert_nothing_raised{ Dir.create_junction(@ascii_to, @from) }
      assert_nothing_raised{ Dir.create_junction(@unicode_to, @from) }
      
      # If we've gotten this far, make sure files created in the @from
      # directory show up in the linked directories.
      File.open(@from + "\\test.txt", "w+"){ |f| f.puts "Hello World" }
      
      assert_equal(Dir.entries(@from), Dir.entries(@ascii_to))
      assert_equal(Dir.entries(@from), Dir.entries(@unicode_to))
   end
   
   def test_is_junction
      assert_respond_to(Dir, :junction?)
      assert_respond_to(Dir, :reparse_dir?) # alias
      assert_nothing_raised{ Dir.junction?(@from) }
      assert_nothing_raised{ Dir.create_junction(@ascii_to, @from) }
      
      assert_equal(false, Dir.junction?(@from))
      assert_equal(true, Dir.junction?(@ascii_to))
   end
   
   def test_is_empty
      assert_respond_to(Dir, :empty?)
      assert_equal(false, Dir.empty?("C:\\")) # One would hope
      assert_equal(true, Dir.empty?(@from))
   end

   def test_admintools
      assert_not_nil(Dir::ADMINTOOLS, "+IGNORE+")
      assert_kind_of(String, Dir::ADMINTOOLS)
   end
   
   def test_altstartup
      assert_not_nil(Dir::ALTSTARTUP, "+IGNORE+")
      assert_kind_of(String, Dir::ALTSTARTUP)
   end
   
   def test_appdata
      assert_not_nil(Dir::APPDATA, "+IGNORE+")
      assert_kind_of(String, Dir::APPDATA)
   end
   
   def test_bitbucket
      assert_not_nil(Dir::BITBUCKET, "+IGNORE+")
      assert_kind_of(String, Dir::BITBUCKET)
   end
   
   def test_cdburn_area
      assert_not_nil(Dir::CDBURN_AREA, "+IGNORE+")
      assert_kind_of(String, Dir::CDBURN_AREA)
   end
   
   def test_common_admintools
      assert_not_nil(Dir::COMMON_ADMINTOOLS, "+IGNORE+")
      assert_kind_of(String, Dir::COMMON_ADMINTOOLS)
   end
   
   def test_common_altstartup
      assert_not_nil(Dir::COMMON_ALTSTARTUP, "+IGNORE+")
      assert_kind_of(String, Dir::COMMON_ALTSTARTUP)
   end
   
   def test_common_appdata
      assert_not_nil(Dir::COMMON_APPDATA, "+IGNORE+")
      assert_kind_of(String, Dir::COMMON_APPDATA)
   end
   
   def test_common_desktopdirectory
      assert_not_nil(Dir::COMMON_DESKTOPDIRECTORY, "+IGNORE+")
      assert_kind_of(String, Dir::COMMON_DESKTOPDIRECTORY)
   end
   
   def test_common_documents
      assert_not_nil(Dir::COMMON_DOCUMENTS, "+IGNORE+")
      assert_kind_of(String, Dir::COMMON_DOCUMENTS)
   end
   
   def test_common_favorites
      assert_not_nil(Dir::COMMON_FAVORITES, "+IGNORE+")
      assert_kind_of(String, Dir::COMMON_FAVORITES)
   end
   
   def test_common_music
      assert_not_nil(Dir::COMMON_MUSIC, "+IGNORE+")
      assert_kind_of(String, Dir::COMMON_MUSIC)
   end
   
   def test_common_pictures
      assert_not_nil(Dir::COMMON_PICTURES, "+IGNORE+")
      assert_kind_of(String, Dir::COMMON_PICTURES)
   end
   
   def test_common_programs
      assert_not_nil(Dir::COMMON_PROGRAMS, "+IGNORE+")
      assert_kind_of(String, Dir::COMMON_PROGRAMS)
   end
   
   def test_common_startmenu
      assert_not_nil(Dir::COMMON_STARTMENU, "+IGNORE+")
      assert_kind_of(String, Dir::COMMON_STARTMENU)
   end
   
   def test_common_startup
      assert_not_nil(Dir::COMMON_STARTUP, "+IGNORE+")
      assert_kind_of(String, Dir::COMMON_STARTUP)
   end
   
   def test_common_templates
      assert_not_nil(Dir::COMMON_TEMPLATES, "+IGNORE+")
      assert_kind_of(String, Dir::COMMON_TEMPLATES)
   end
   
   def test_common_video
      assert_not_nil(Dir::COMMON_VIDEO, "+IGNORE+")
      assert_kind_of(String, Dir::COMMON_VIDEO)
   end
   
   def test_controls
      assert_not_nil(Dir::CONTROLS, "+IGNORE+")
      assert_kind_of(String, Dir::CONTROLS)
   end
   
   def test_cookies
      assert_not_nil(Dir::COOKIES, "+IGNORE+")
      assert_kind_of(String, Dir::COOKIES)
   end
   
   def test_desktop
      assert_not_nil(Dir::DESKTOP, "+IGNORE+")
      assert_kind_of(String, Dir::DESKTOP)
   end
   
   def test_desktopdirectory
      assert_not_nil(Dir::DESKTOPDIRECTORY, "+IGNORE+")
      assert_kind_of(String, Dir::DESKTOPDIRECTORY)
   end
   
   def test_drives
      assert_not_nil(Dir::DRIVES, "+IGNORE+")
      assert_kind_of(String, Dir::DRIVES)
   end

   def test_favorites
      assert_not_nil(Dir::FAVORITES, "+IGNORE+")
      assert_kind_of(String, Dir::FAVORITES)
   end
   
   def test_fonts
      assert_not_nil(Dir::FONTS, "+IGNORE+")
      assert_kind_of(String, Dir::FONTS)
   end
   
   def test_history
      assert_not_nil(Dir::HISTORY, "+IGNORE+")
      assert_kind_of(String, Dir::HISTORY)
   end
   
   def test_internet
      assert_not_nil(Dir::INTERNET, "+IGNORE+")
      assert_kind_of(String, Dir::INTERNET)
   end
   
   def test_internet_cache
      assert_not_nil(Dir::INTERNET_CACHE, "+IGNORE+")
      assert_kind_of(String, Dir::INTERNET_CACHE)
   end
   
   def test_local_appdata
      assert_not_nil(Dir::LOCAL_APPDATA, "+IGNORE+")
      assert_kind_of(String, Dir::LOCAL_APPDATA)
   end
   
   def test_mydocuments
      assert_not_nil(Dir::MYDOCUMENTS, "+IGNORE+")
      assert_kind_of(String, Dir::MYDOCUMENTS)
   end
   
   def test_local_mymusic
      assert_not_nil(Dir::MYMUSIC, "+IGNORE+")
      assert_kind_of(String, Dir::MYMUSIC)
   end
   
   def test_local_mypictures
      assert_not_nil(Dir::MYPICTURES, "+IGNORE+")
      assert_kind_of(String, Dir::MYPICTURES)
   end
   
   def test_local_myvideo
      assert_not_nil(Dir::MYVIDEO, "+IGNORE+")
      assert_kind_of(String, Dir::MYVIDEO)
   end
   
   def test_nethood
      assert_not_nil(Dir::NETHOOD, "+IGNORE+")
      assert_kind_of(String, Dir::NETHOOD)
   end
   
   def test_network
      assert_not_nil(Dir::NETWORK, "+IGNORE+")
      assert_kind_of(String, Dir::NETWORK)
   end

   def test_personal
      assert_not_nil(Dir::PERSONAL, "+IGNORE+")
      assert_kind_of(String, Dir::PERSONAL)
   end
   
   def test_printers
      assert_not_nil(Dir::PRINTERS, "+IGNORE+")
      assert_kind_of(String, Dir::PRINTERS)
   end
   
   def test_printhood
      assert_not_nil(Dir::PRINTHOOD, "+IGNORE+")
      assert_kind_of(String, Dir::PRINTHOOD)
   end
   
   def test_profile
      assert_not_nil(Dir::PROFILE, "+IGNORE+")
      assert_kind_of(String, Dir::PROFILE)
   end
   
   # Doesn't appear to actually exist
   #def test_profiles
   #   assert_not_nil(Dir::PROFILES)
   #   assert_kind_of(String,Dir::PROFILES)
   #end
   
   def test_program_files
      assert_not_nil(Dir::PROGRAM_FILES, "+IGNORE+")
      assert_kind_of(String, Dir::PROGRAM_FILES)
   end
   
   def test_program_files_common
      assert_not_nil(Dir::PROGRAM_FILES_COMMON, "+IGNORE+")
      assert_kind_of(String, Dir::PROGRAM_FILES_COMMON)
   end
   
   def test_programs
      assert_not_nil(Dir::PROGRAMS, "+IGNORE+")
      assert_kind_of(String, Dir::PROGRAMS)
   end
   
   def test_recent
      assert_not_nil(Dir::RECENT, "+IGNORE+")
      assert_kind_of(String, Dir::RECENT)
   end
   
   def test_sendto
      assert_not_nil(Dir::SENDTO, "+IGNORE+")
      assert_kind_of(String, Dir::SENDTO)
   end
   
   def test_startmenu
      assert_not_nil(Dir::STARTMENU, "+IGNORE+")
      assert_kind_of(String, Dir::STARTMENU)
   end
   
   def test_startup
      assert_not_nil(Dir::STARTUP, "+IGNORE+")
      assert_kind_of(String, Dir::STARTUP)
   end
   
   def test_system
      assert_not_nil(Dir::SYSTEM, "+IGNORE+")
      assert_kind_of(String, Dir::SYSTEM)
   end
   
   def test_templates
      assert_not_nil(Dir::TEMPLATES, "+IGNORE+")
      assert_kind_of(String, Dir::TEMPLATES)
   end

   def test_windows_dir
      assert_not_nil(Dir::WINDOWS, "+IGNORE+")
      assert_kind_of(String, Dir::WINDOWS)
   end

   def teardown
      FileUtils.rm_rf(@ascii_to)
      FileUtils.rm_rf(@unicode_to)
      FileUtils.rm_rf(@from)
   end
end
