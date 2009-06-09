#############################################################################
# tc_file_path.rb
#
# Test case for the path related methods of win32-file. You should run this
# test via the 'rake test' or 'rake test_path' task.
#############################################################################
require 'test/unit'
require 'win32/file'

class TC_Win32_File_Path < Test::Unit::TestCase
   def setup
      @dir  = File.dirname(File.expand_path(__FILE__))
      @long_file  = File.join(@dir, 'sometestfile.txt')      
      @short_file = File.join(@dir, 'SOMETE~1.TXT')
   end
   
   def test_basename
      assert_respond_to(File, :basename)
      assert_nothing_raised{ File.basename("C:\\foo") }
      assert_kind_of(String, File.basename("C:\\foo"))
      
      # Standard paths
      assert_equal("baz.txt", File.basename("C:\\foo\\bar\\baz.txt"))
      assert_equal("baz", File.basename("C:\\foo\\bar\\baz.txt", ".txt"))
      assert_equal("baz.txt", File.basename("C:\\foo\\bar\\baz.txt", ".zip"))
      assert_equal("bar", File.basename("C:\\foo\\bar"))
      assert_equal("bar", File.basename("C:\\foo\\bar\\"))
      assert_equal("foo", File.basename("C:\\foo"))
      assert_equal("C:\\", File.basename("C:\\"))
      
      # UNC paths
      assert_equal("baz.txt", File.basename("\\\\foo\\bar\\baz.txt"))
      assert_equal("baz", File.basename("\\\\foo\\bar\\baz"))
      assert_equal("\\\\foo", File.basename("\\\\foo"))
      assert_equal("\\\\foo\\bar", File.basename("\\\\foo\\bar"))
      
      # Unix style paths
      assert_equal("bar", File.basename("/foo/bar"))
      assert_equal("bar.txt", File.basename("/foo/bar.txt"))
      assert_equal("bar.txt", File.basename("bar.txt"))
      assert_equal("bar", File.basename("/bar"))
      assert_equal("bar", File.basename("/bar/"))
      assert_equal("baz", File.basename("//foo/bar/baz"))
      assert_equal("//foo", File.basename("//foo"))
      assert_equal("//foo/bar", File.basename("//foo/bar"))
      
      # Forward slashes
      assert_equal("bar", File.basename("C:/foo/bar"))
      assert_equal("bar", File.basename("C:/foo/bar/"))
      assert_equal("foo", File.basename("C:/foo"))
      assert_equal("C:/", File.basename("C:/"))
      assert_equal("bar", File.basename("C:/foo/bar\\\\"))
         
      # Edge cases
      assert_equal("", File.basename(""))
      assert_equal(".", File.basename("."))
      assert_equal("..", File.basename(".."))
      assert_equal("foo", File.basename("//foo/"))
      
      # Suffixes
      assert_equal("bar", File.basename("bar.txt", ".txt"))
      assert_equal("bar", File.basename("/foo/bar.txt", ".txt"))
      assert_equal("bar.txt", File.basename("bar.txt", ".exe"))
      assert_equal("bar.txt", File.basename("bar.txt.exe", ".exe"))
      assert_equal("bar.txt.exe", File.basename("bar.txt.exe", ".txt"))
      assert_equal("bar", File.basename("bar.txt", ".*"))
      assert_equal("bar.txt", File.basename("bar.txt.exe", ".*"))
      
      # Ensure original path not modified
      path = "C:\\foo\\bar"
      assert_nothing_raised{ File.basename(path) }
      assert_equal("C:\\foo\\bar", path)    
   end
   
   def test_dirname
      assert_respond_to(File, :dirname)
      assert_nothing_raised{ File.dirname("C:\\foo") }
      assert_kind_of(String, File.dirname("C:\\foo"))
      
      # Standard Paths
      assert_equal("C:\\foo", File.dirname("C:\\foo\\bar.txt"))
      assert_equal("C:\\foo", File.dirname("C:\\foo\\bar"))
      assert_equal("C:\\", File.dirname("C:\\foo"))
      assert_equal("C:\\", File.dirname("C:\\"))
      assert_equal(".", File.dirname("foo"))
      
      # UNC paths
      assert_equal("\\\\foo\\bar", File.dirname("\\\\foo\\bar\\baz"))
      assert_equal("\\\\foo\\bar", File.dirname("\\\\foo\\bar"))
      assert_equal("\\\\foo", File.dirname("\\\\foo"))
      assert_equal("\\\\", File.dirname("\\\\"))
    
      # Forward slashes
      assert_equal("C:/foo", File.dirname("C:/foo/bar.txt"))
      assert_equal("C:/foo", File.dirname("C:/foo/bar"))
      assert_equal("C:/", File.dirname("C:/foo"))
      assert_equal("C:/", File.dirname("C:/"))
      assert_equal("//foo/bar", File.dirname("//foo/bar/baz"))
      assert_equal("//foo/bar", File.dirname("//foo/bar"))
      assert_equal("//foo", File.dirname("//foo"))
      assert_equal("//", File.dirname("//"))
      assert_equal(".", File.dirname("./foo"))
      assert_equal("./foo", File.dirname("./foo/bar"))
      
      # Edge cases
      assert_equal(".", File.dirname(""))
      assert_equal(".", File.dirname("."))
      assert_equal(".", File.dirname("."))
      assert_equal(".", File.dirname("./"))
      assert_raises(TypeError){ File.dirname(nil) }
      
      # Ensure original path not modified
      path = "C:\\foo\\bar"
      assert_nothing_raised{ File.dirname(path) }
      assert_equal("C:\\foo\\bar", path)
   end
   
   def test_split
      assert_respond_to(File, :split)
      assert_nothing_raised{ File.split("C:\\foo\\bar") }
      assert_kind_of(Array, File.split("C:\\foo\\bar"))
      
      # Standard Paths
      assert_equal(["C:\\foo", "bar"], File.split("C:\\foo\\bar"))     
      assert_equal([".", "foo"], File.split("foo"))
      
      # Forward slashes
      assert_equal(["C:/foo", "bar"], File.split("C:/foo/bar"))
      assert_equal([".", "foo"], File.split("foo"))
      
      # Unix paths
      assert_equal(["/foo","bar"], File.split("/foo/bar"))
      assert_equal(["/", "foo"], File.split("/foo"))
      assert_equal([".", "foo"], File.split("foo"))
      
      # UNC paths
      assert_equal(["\\\\foo\\bar", "baz"], File.split("\\\\foo\\bar\\baz"))
      assert_equal(["\\\\foo\\bar", ""], File.split("\\\\foo\\bar"))
      assert_equal(["\\\\foo", ""], File.split("\\\\foo"))
      assert_equal(["\\\\", ""], File.split("\\\\"))

      # Edge cases
      assert_equal(["C:\\", ""], File.split("C:\\"))
      assert_equal(["", ""], File.split(""))
      
      # Ensure original path not modified
      path = "C:\\foo\\bar"
      assert_nothing_raised{ File.split(path) }
      assert_equal("C:\\foo\\bar", path)
   end
   
   def test_long_path
      assert_respond_to(File, :long_path)
      assert_equal('sometestfile.txt', File.long_path(@short_file))
      assert_equal('SOMETE~1.TXT', File.basename(@short_file))
   end
   
   def test_short_path
      assert_respond_to(File, :short_path)
      assert_equal('SOMETE~1.TXT', File.short_path(@long_file))
      assert_equal('sometestfile.txt', File.basename(@long_file))
   end
   
   def teardown
      @short_file = nil
      @long_file  = nil
      @dir        = nil
   end
end