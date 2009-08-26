##########################################################################
# test_pathname_win.rb
#
# MS Windows test suite for the Pathname class. To test explicitly
# against the C extension pass the letter 'c' as an argument. You should
# use the 'rake test' task to run this test suite.
###########################################################################
require 'facade'
require 'pathname2'
require 'test/unit'

class MyPathname < Pathname; end

class TC_Pathname_MSWin < Test::Unit::TestCase

   def setup
      @fpath = Pathname.new("C:/Program Files/Windows NT/Accessories")
      @bpath = Pathname.new("C:\\Program Files\\Windows NT\\Accessories")
      @dpath = Pathname.new("C:\\Program Files\\File[5].txt")
      @spath = Pathname.new("C:\\PROGRA~1\\WINDOW~1\\ACCESS~1")
      @upath = Pathname.new("\\\\foo\\bar\\baz")
      @npath = Pathname.new("foo\\bar\\baz")
      @rpath = Pathname.new("Z:\\")
      @xpath = Pathname.new("\\\\foo\\bar")
      @ypath = Pathname.new("\\\\foo")
      @zpath = Pathname.new("\\\\")
      @epath = Pathname.new("")
      @ppath = Pathname.new("C:\\foo\\bar\\")
      @cpath = Pathname.new("C:\\foo\\..\\bar\\.\\baz")
      @tpath = Pathname.new("C:\\foo\\bar")

      @url_path = Pathname.new("file:///C:/Documents%20and%20Settings")
      @cur_path = Pathname.new(Dir.pwd)
      
      @mypath = MyPathname.new("C:\\Program Files")
      
      @abs_array = []
      @rel_array = []
      @unc_array = []
   end
   
   def test_aref_with_range
      assert_equal("C:\\Program Files", @fpath[0..1])
      assert_equal("C:\\Program Files\\Windows NT", @fpath[0..2])
      assert_equal("Program Files\\Windows NT", @fpath[1..2])
      assert_equal(@fpath, @fpath[0..-1])
   end
   
   def test_aref_with_index_and_length
      assert_equal("C:", @fpath[0,1])
      assert_equal("C:\\Program Files", @fpath[0,2])
      assert_equal("Program Files\\Windows NT", @fpath[1,2])
   end
   
   def test_aref_with_index
      assert_equal("C:", @fpath[0])
      assert_equal("Program Files", @fpath[1])
      assert_equal("Accessories", @fpath[-1])
      assert_equal(nil, @fpath[10])
   end

   def test_version
      assert_equal('1.6.2', Pathname::VERSION)
   end

   # Convenience method for test_plus
   def assert_pathname_plus(a, b, c)
      a = Pathname.new(a)
      b = Pathname.new(b)
      c = Pathname.new(c)
      assert_equal(a, b + c)
   end

   # Convenience method for test_spaceship operator
   def assert_pathname_cmp(int, s1, s2)
      p1 = Pathname.new(s1)
      p2 = Pathname.new(s2)
      result = p1 <=> p2
      assert_equal(int, result)
   end
   
   # Convenience method for test_relative_path_from
   def assert_relpath(result, dest, base)
      assert_equal(result, Pathname.new(dest).relative_path_from(base))
   end

   # Convenience method for test_relative_path_from_expected_errors
   def assert_relative_path_error(to, from)
      assert_raise(ArgumentError) {
         Pathname.new(to).relative_path_from(from)
      }
   end
   
   def test_file_urls
      assert_equal("C:\\Documents and Settings", @url_path)
      assert_raises(Pathname::Error){ Pathname.new('http://rubyforge.org') }
   end

   def test_realpath
      assert_respond_to(@fpath, :realpath)
      assert_equal(@cur_path, Pathname.new('.').realpath)
      assert_raises(Errno::ENOENT){ Pathname.new('../bogus').realpath }
   end
   
   def test_relative_path_from
      assert_relpath("..\\a", "a", "b")
      assert_relpath("..\\a", "a", "b\\")
      assert_relpath("..\\a", "a\\", "b")
      assert_relpath("..\\a", "a\\", "b\\")
      assert_relpath("..\\a", "c:\\a", "c:\\b")
      assert_relpath("..\\a", "c:\\a", "c:\\b\\")
      assert_relpath("..\\a", "c:\\a\\", "c:\\b")
      assert_relpath("..\\a", "c:\\a\\", "c:\\b\\")
      
      assert_relpath("..\\b", "a\\b", "a\\c")
      assert_relpath("..\\a", "..\\a", "..\\b")

      assert_relpath("a", "a", ".")
      assert_relpath("..", ".", "a")

      assert_relpath(".", ".", ".")
      assert_relpath(".", "..", "..")
      assert_relpath("..", "..", ".")

      assert_relpath("c\\d", "c:\\a\\b\\c\\d", "c:\\a\\b")
      assert_relpath("..\\..", "c:\\a\\b", "c:\\a\\b\\c\\d")
      assert_relpath("..\\..\\..\\..\\e", "c:\\e", "c:\\a\\b\\c\\d")
      assert_relpath("..\\b\\c", "a\\b\\c", "a\\d")

      assert_relpath("..\\a", "c:\\..\\a", "c:\\b")
      #assert_relpath("..\\..\\a", "..\\a", "b") # fails
      assert_relpath(".", "c:\\a\\..\\..\\b", "c:\\b")
      assert_relpath("..", "a\\..", "a")
      assert_relpath(".", "a\\..\\b", "b")

      assert_relpath("a", "a", "b\\..")
      assert_relpath("b\\c", "b\\c", "b\\..")
   end

   def test_relative_path_from_expected_errors
      assert_relative_path_error("c:\\", ".")
      assert_relative_path_error(".", "c:\\")
      assert_relative_path_error("a", "..")
      assert_relative_path_error(".", "..")
      assert_relative_path_error("C:\\Temp", "D:\\Temp")
      assert_relative_path_error("\\\\Server\\Temp", "D:\\Temp")
   end
   
   # Convenience method to verify that the receiver was not modified
   # except perhaps slashes
   def assert_non_destructive      
      assert_equal("C:\\Program Files\\Windows NT\\Accessories", @fpath)
      assert_equal("C:\\Program Files\\Windows NT\\Accessories", @bpath)
      assert_equal("C:\\Program Files\\File[5].txt", @dpath)
      assert_equal("C:\\PROGRA~1\\WINDOW~1\\ACCESS~1", @spath)
      assert_equal("\\\\foo\\bar\\baz", @upath)
      assert_equal("foo\\bar\\baz", @npath)
      assert_equal("Z:\\", @rpath)
      assert_equal("\\\\foo\\bar", @xpath)
      assert_equal("\\\\foo", @ypath)
      assert_equal("\\\\", @zpath)
      assert_equal("", @epath)
      assert_equal("C:\\foo\\bar\\", @ppath)
      assert_equal("C:\\foo\\..\\bar\\.\\baz", @cpath)
   end

   def test_parent
      assert_respond_to(@bpath, :parent)
      assert_equal("C:\\Program Files\\Windows NT", @bpath.parent)
      assert_equal("foo\\bar", @npath.parent)
      assert_equal("Z:\\", @rpath.parent)
   end
   
   def test_short_path
      assert_respond_to(@bpath, :short_path)
      assert_nothing_raised{ @bpath.short_path }      
      assert_kind_of(Pathname, @bpath.short_path)
      assert_equal("C:\\PROGRA~1\\WINDOW~1\\ACCESS~1", @bpath.short_path)
      
      assert_equal("C:\\Program Files\\Windows NT\\Accessories", @bpath)
   end
   
   def test_long_path
      assert_respond_to(@spath, :long_path)
      assert_nothing_raised{ @spath.long_path }     
      assert_kind_of(Pathname, @spath.long_path)
      
      assert_equal(
         "C:\\Program Files\\Windows NT\\Accessories",
         @spath.long_path
      )
      
      assert_equal("C:\\PROGRA~1\\WINDOW~1\\ACCESS~1", @spath)
   end
   
   def test_undecorate
      assert_respond_to(@dpath, :undecorate)
      assert_nothing_raised{ @dpath.undecorate }
      assert_kind_of(Pathname, @dpath.undecorate)
      
      assert_equal('C:\Program Files\File.txt', @dpath.undecorate)    
      assert_equal('C:\Path\File', Pathname.new('C:\Path\File').undecorate)
      assert_equal('C:\Path\File', Pathname.new('C:\Path\File[12]').undecorate)
      assert_equal('C:\Path\[3].txt',
         Pathname.new('C:\Path\[3].txt').undecorate
      )
      assert_equal('\\foo\bar.txt',Pathname.new('\\foo\bar[5].txt').undecorate)
      assert_equal('\\foo\bar', Pathname.new('\\foo\bar[5]').undecorate)
      assert_equal('\\foo\bar', Pathname.new('\\foo\bar').undecorate)
      
      assert_equal("C:\\Program Files\\File[5].txt", @dpath)
   end
   
   def test_undecorate_bang
      assert_respond_to(@dpath, :undecorate!)
      assert_nothing_raised{ @dpath.undecorate! }
      assert_kind_of(Pathname, @dpath.undecorate!)
      
      assert_equal('C:\Program Files\File.txt', @dpath.undecorate!)    
      assert_equal('C:\Path\File', Pathname.new('C:\Path\File').undecorate!)
      assert_equal('C:\Path\File', Pathname.new('C:\Path\File[12]').undecorate!)
      assert_equal('C:\Path\[3].txt',
         Pathname.new('C:\Path\[3].txt').undecorate!
      )
      assert_equal('\\foo\bar.txt',Pathname.new('\\foo\bar[5].txt').undecorate!)
      assert_equal('\\foo\bar', Pathname.new('\\foo\bar[5]').undecorate!)
      assert_equal('\\foo\bar', Pathname.new('\\foo\bar').undecorate!)
      
      assert_equal('C:\Program Files\File.txt', @dpath)
   end
   
   def test_unc
      assert_respond_to(@upath, :unc?)
      assert_nothing_raised{ @upath.unc? }
      
      assert_equal(true, @upath.unc?)
      assert_equal(true, @xpath.unc?)
      assert_equal(true, @ypath.unc?)
      assert_equal(true, @zpath.unc?)
      assert_equal(false, @fpath.unc?)
      assert_equal(false, @bpath.unc?)
      assert_equal(false, @dpath.unc?)
      assert_equal(false, @spath.unc?)
      assert_equal(false, @epath.unc?)

      # Arguably a bug in the PathIsUNC() function since drive letters
      # are, in fact, a legal part of a UNC path (for historical reasons).
      assert_equal(false, Pathname.new("C:\\\\foo\\bar\\baz").unc?)
      
      assert_non_destructive
   end
   
   def test_pstrip
      assert_respond_to(@ppath, :pstrip)
      assert_nothing_raised{ @ppath.pstrip }
      assert_nothing_raised{ @fpath.pstrip }
      assert_kind_of(Pathname, @ppath.pstrip)

      assert_equal('C:\foo', Pathname.new("C:\\foo\\").pstrip)
      assert_equal('C:\foo', Pathname.new("C:\\foo").pstrip)
      assert_equal("", Pathname.new("").pstrip)
      
      assert_equal("C:\\foo\\bar\\", @ppath)
   end
   
   def test_pstrip_bang
      assert_respond_to(@ppath, :pstrip!)
      assert_nothing_raised{ @ppath.pstrip! }
      assert_nothing_raised{ @fpath.pstrip! }
      assert_kind_of(Pathname, @ppath.pstrip!)

      assert_equal('C:\foo', Pathname.new("C:\\foo\\").pstrip!)
      assert_equal('C:\foo', Pathname.new("C:\\foo").pstrip!)
      assert_equal("", Pathname.new("").pstrip!)
      
      assert_equal("C:\\foo\\bar", @ppath)
   end
   
   def test_exists
      assert_respond_to(@fpath, :exists?)
      assert_nothing_raised{ @fpath.exists? }
      assert_equal(true, Pathname.new("C:\\").exists?)
      assert_equal(false, Pathname.new("X:\\foo\\bar\\baz").exists?)
   end
 
   def test_each
      array = []
      
      assert_respond_to(@fpath, :each)
      assert_nothing_raised{ @fpath.each{ |e| array.push(e) } }
      assert_equal(["C:", "Program Files", "Windows NT", "Accessories"], array)
   end
  
   def test_descend
      assert_respond_to(@bpath, :descend)
      assert_nothing_raised{ @bpath.descend{} }
      
      @bpath.descend{ |path| @abs_array.push(path) }
      @npath.descend{ |path| @rel_array.push(path) }
      @upath.descend{ |path| @unc_array.push(path) }
      
      assert_equal("C:", @abs_array[0])
      assert_equal("C:\\Program Files", @abs_array[1])
      assert_equal("C:\\Program Files\\Windows NT", @abs_array[2])
      assert_equal("C:\\Program Files\\Windows NT\\Accessories", @abs_array[3])
      
      assert_equal("foo", @rel_array[0])
      assert_equal("foo\\bar", @rel_array[1])
      assert_equal("foo\\bar\\baz", @rel_array[2])
      
      assert_equal("\\\\foo\\bar", @unc_array[0])
      assert_equal("\\\\foo\\bar\\baz", @unc_array[1])
      
      assert_non_destructive      
   end
    
   def test_ascend
      assert_respond_to(@bpath, :ascend)
      assert_nothing_raised{ @bpath.ascend{} }
      
      @bpath.ascend{ |path| @abs_array.push(path) }
      @npath.ascend{ |path| @rel_array.push(path) }
      @upath.ascend{ |path| @unc_array.push(path) }
      
      assert_equal("C:\\Program Files\\Windows NT\\Accessories", @abs_array[0])
      assert_equal("C:\\Program Files\\Windows NT", @abs_array[1])
      assert_equal("C:\\Program Files", @abs_array[2])
      assert_equal("C:", @abs_array[3])
      assert_equal(4, @abs_array.length)
      
      assert_equal("foo\\bar\\baz", @rel_array[0])
      assert_equal("foo\\bar", @rel_array[1])     
      assert_equal("foo", @rel_array[2])
      assert_equal(3, @rel_array.length)
      
      assert_equal("\\\\foo\\bar\\baz", @unc_array[0])
      assert_equal("\\\\foo\\bar", @unc_array[1])
      assert_equal(2, @unc_array.length)
      
      assert_non_destructive     
   end
  
   def test_immutability
      path = "C:\\Program Files\\foo\\bar".freeze
      assert_equal(true, path.frozen?)
      assert_nothing_raised{ Pathname.new(path) }
      assert_nothing_raised{ Pathname.new(path).root }
   end
   
   def test_plus_operator
      # Standard stuff
      assert_pathname_plus("C:\\a\\b", "C:\\a", "b")
      assert_pathname_plus("C:\\b", "a", "C:\\b")
      assert_pathname_plus("a\\b", "a", "b")
      assert_pathname_plus("C:\\b", "C:\\a", "..\\b")
      assert_pathname_plus("C:\\a\\b", "C:\\a\\.", "\\b")
      assert_pathname_plus("C:\\a\\b.txt", "C:\\a", "b.txt")
      
      # UNC paths
      assert_pathname_plus("\\\\foo\\bar", "\\\\foo", "bar")
      assert_pathname_plus("\\\\foo", "\\\\", "foo")
      assert_pathname_plus("\\\\", "\\\\", "")
      assert_pathname_plus("\\\\foo\\baz", "\\\\foo\\bar", "\\..\\baz")
      assert_pathname_plus("\\\\", "\\\\", "..\\..\\..\\..")

      # Pathname + String
      assert_nothing_raised{ @tpath + "bar" }
      assert_equal('C:\foo\bar\baz', @tpath + 'baz')
      assert_equal('C:\foo\bar', @tpath)

      # Ensure neither left nor right operand are modified
      assert_nothing_raised{ @tpath + @npath }
      assert_equal('C:\foo\bar\foo\bar\baz', @tpath + @npath)
      assert_equal('C:\foo\bar', @tpath)
      assert_equal('foo\bar\baz', @npath)
   end
  
   def test_clean
      assert_respond_to(@cpath, :clean)
      assert_nothing_raised{ @cpath.clean }
      assert_kind_of(Pathname, @cpath.clean)

      # Our preset stuff
      assert_equal("C:\\Program Files\\Windows NT\\Accessories", @fpath.clean)
      assert_equal("C:\\Program Files\\Windows NT\\Accessories", @bpath.clean)
      assert_equal("\\\\foo\\bar\\baz", @upath.clean)
      assert_equal("foo\\bar\\baz", @npath.clean)
      assert_equal("Z:\\", @rpath.clean)
      assert_equal("\\\\foo\\bar", @xpath.clean)
      assert_equal("\\\\foo", @ypath.clean)
      assert_equal("\\\\", @zpath.clean)
      assert_equal("", @epath.clean)
      assert_equal("C:\\bar\\baz", @cpath.clean)

      # Standard stuff
      assert_equal("C:\\a\\c", Pathname.new("C:\\a\\.\\b\\..\\c").clean)
      assert_equal("C:\\a", Pathname.new("C:\\.\\a").clean)
      assert_equal("C:\\a\\b", Pathname.new("C:\\a\\.\\b").clean)
      assert_equal("C:\\b", Pathname.new("C:\\a\\..\\b").clean)
      assert_equal("C:\\a", Pathname.new("C:\\a\\.").clean)
      assert_equal("C:\\d", Pathname.new("C:\\..\\..\\..\\d").clean)
      assert_equal("C:\\a\\", Pathname.new("C:\\a\\").clean)
      
      # Edge cases
      assert_equal("\\", Pathname.new(".").clean)
      assert_equal("\\", Pathname.new("..").clean)

      assert_non_destructive
   end

   def test_clean_bang
      assert_respond_to(@cpath, :clean!)
      assert_nothing_raised{ @cpath.clean! }
      assert_kind_of(Pathname, @cpath.clean!)

      # Our preset stuff
      assert_equal("C:\\Program Files\\Windows NT\\Accessories", @fpath.clean!)
      assert_equal("C:\\Program Files\\Windows NT\\Accessories", @bpath.clean!)
      assert_equal("\\\\foo\\bar\\baz", @upath.clean!)
      assert_equal("foo\\bar\\baz", @npath.clean!)
      assert_equal("Z:\\", @rpath.clean!)
      assert_equal("\\\\foo\\bar", @xpath.clean!)
      assert_equal("\\\\foo", @ypath.clean!)
      assert_equal("\\\\", @zpath.clean!)
      assert_equal("", @epath.clean!)
      assert_equal("C:\\bar\\baz", @cpath.clean!)

      # Standard stuff
      assert_equal("C:\\a\\c", Pathname.new("C:\\a\\.\\b\\..\\c").clean!)
      assert_equal("C:\\a", Pathname.new("C:\\.\\a").clean!)
      assert_equal("C:\\a\\b", Pathname.new("C:\\a\\.\\b").clean!)
      assert_equal("C:\\b", Pathname.new("C:\\a\\..\\b").clean!)
      assert_equal("C:\\a", Pathname.new("C:\\a\\.").clean!)
      assert_equal("C:\\d", Pathname.new("C:\\..\\..\\..\\d").clean!)
      assert_equal("C:\\a\\", Pathname.new("C:\\a\\").clean!)
      
      # Edge cases
      assert_equal("\\", Pathname.new(".").clean!)
      assert_equal("\\", Pathname.new("..").clean!)

      assert_equal("C:\\bar\\baz", @cpath)
   end

   def test_absolute
      assert_equal(true, @fpath.absolute?)
      assert_equal(true, @bpath.absolute?)
      assert_equal(true, @upath.absolute?)
      assert_equal(false, @npath.absolute?)
      assert_equal(true, @rpath.absolute?)
      assert_equal(true, @xpath.absolute?)
      assert_equal(true, @ypath.absolute?)
      assert_equal(true, @zpath.absolute?)
      assert_equal(false, @epath.absolute?)
      
      assert_non_destructive
   end
   
   def test_relative
      assert_equal(false, @fpath.relative?)
      assert_equal(false, @bpath.relative?)
      assert_equal(false, @upath.relative?)
      assert_equal(true, @npath.relative?)
      assert_equal(false, @rpath.relative?)
      assert_equal(false, @xpath.relative?)
      assert_equal(false, @ypath.relative?)
      assert_equal(false, @zpath.relative?)
      assert_equal(true, @epath.relative?)
      
      assert_non_destructive
   end
   
   def test_root
      assert_equal("C:\\", @fpath.root)     
      assert_equal("C:\\", @bpath.root)
      assert_equal("\\\\foo\\bar", @upath.root)
      assert_equal(".", @npath.root)
      assert_equal("Z:\\", @rpath.root)
      assert_equal("\\\\foo\\bar", @xpath.root)
      assert_equal("\\\\foo", @ypath.root)
      assert_equal("\\\\", @zpath.root)
      assert_equal(".", @epath.root)
      
      # Edge cases
      assert_equal(".", Pathname.new("..").root)
      assert_equal(".", Pathname.new(".").root)
      
      assert_non_destructive
   end
   
   def test_drive_number
      assert_equal(2, @fpath.drive_number)
      assert_equal(2, @bpath.drive_number)
      assert_equal(nil, @upath.drive_number)
      assert_equal(nil, @npath.drive_number)
      assert_equal(25, @rpath.drive_number)
      assert_equal(nil, @xpath.drive_number)
      assert_equal(nil, @ypath.drive_number)
      assert_equal(nil, @zpath.drive_number)
      assert_equal(nil, @epath.drive_number)
      
      # Edge cases
      assert_equal(nil, Pathname.new("..").drive_number)
      assert_equal(nil, Pathname.new(".").drive_number)
      
      assert_non_destructive
   end
        
   def test_to_a
      expected = ["C:", "Program Files", "Windows NT", "Accessories"]
      assert_equal(expected, @fpath.to_a)
      assert_equal(expected, @bpath.to_a)
      assert_equal(["foo","bar","baz"], @upath.to_a)
      assert_equal(["foo","bar","baz"], @npath.to_a)
      assert_equal(["Z:"], @rpath.to_a)
      assert_equal(["foo","bar"], @xpath.to_a)
      assert_equal(["foo"], @ypath.to_a)
      assert_equal([], @zpath.to_a)
      assert_equal([], @epath.to_a)
      assert_equal(["C:", "foo", "bar"], @ppath.to_a)
      
      assert_non_destructive
   end
 
   def test_is_root
      assert_equal(false, @fpath.root?)
      assert_equal(false, @bpath.root?)
      assert_equal(false, @upath.root?)
      assert_equal(false, @npath.root?)
      assert_equal(true, @rpath.root?)
      assert_equal(true, @xpath.root?)
      assert_equal(true, @ypath.root?)
      assert_equal(true, @zpath.root?)
      assert_equal(false, @epath.root?)
      assert_equal(false, @ppath.root?)
      
      assert_non_destructive
   end
  
   # These are the methods from IO we have to explicitly define since
   # they aren't handled by Facade.
   def test_facade_io
      assert_respond_to(@fpath, :foreach)
      assert_respond_to(@fpath, :read)
      assert_respond_to(@fpath, :readlines)
      assert_respond_to(@fpath, :sysopen)
   end

   def test_facade_file
      File.methods(false).each{ |method|
         assert_respond_to(@fpath, method.to_sym)
      }
   end

   def test_facade_dir
      Dir.methods(false).each{ |method|
         assert_respond_to(@fpath, method.to_sym)
      }
   end
   
   def test_facade_fileutils
      methods = FileUtils.public_instance_methods
      methods -= File.methods(false)
      methods -= Dir.methods(false)
      methods.delete_if{ |m| m =~ /stream/ }
      methods.delete_if{ |m| m =~ /^ln/ }
      methods.delete("identical?")

      methods.each{ |method|
         assert_respond_to(@fpath, method.to_sym)
      }
   end
   
   def test_facade_find
      assert_respond_to(@fpath, :find)
      assert_nothing_raised{ @fpath.find{} }

      Pathname.new(Dir.pwd).find{ |f|
         Find.prune if f.match("CVS")
         assert_kind_of(Pathname, f)
      }
   end

   def test_children
      assert_respond_to(@cur_path, :children)
      assert_nothing_raised{ @cur_path.children }
      assert_kind_of(Array, @cur_path.children)

      # Delete Eclipse related files
      children = @cur_path.children
      children.delete_if{ |e| File.basename(e) == "CVS" }
      children.delete_if{ |e| File.basename(e) == ".cvsignore" }
      children.delete_if{ |e| File.basename(e) == ".project" }
      children.delete_if{ |e| File.basename(e) == ".loadpath" }

      assert_equal(
         [
            Dir.pwd + "/CHANGES",
            Dir.pwd + "/examples",
            Dir.pwd + "/ext",
            Dir.pwd + "/lib",
            Dir.pwd + "/MANIFEST",
            Dir.pwd + "/pathname2.gemspec",
            Dir.pwd + "/Rakefile",
            Dir.pwd + "/README",
            Dir.pwd + "/test"
         ].map{ |e| e.tr("/", "\\") },
         children
      )

      # Delete Eclipse related files
      children = @cur_path.children(false)
      children.delete("CVS")
      children.delete(".cvsignore")
      children.delete(".project")
      children.delete(".loadpath")
      
      assert_equal(
         [
            "CHANGES", "examples", "ext", "lib", "MANIFEST",
            "pathname2.gemspec", "Rakefile", "README", "test"
         ],
         children
      )
   end

   # Ensures that subclasses return the subclass as the class, not a hard
   # coded Pathname.
   def test_subclasses
      assert_kind_of(MyPathname, @mypath)
      assert_kind_of(MyPathname, @mypath + MyPathname.new('foo'))
      assert_kind_of(MyPathname, @mypath.realpath)
      assert_kind_of(MyPathname, @mypath.children.first)
   end

   # Test to ensure that the pn{ } shortcut works
   #
   def test_kernel_method
      assert_respond_to(Kernel, :pn)
      assert_nothing_raised{ pn{'c:\foo'} }
      assert_kind_of(Pathname, pn{'c:\foo'})
      assert_equal('c:\foo', pn{'c:\foo'})
   end
  
   def teardown
      @fpath = nil
      @bpath = nil
      @dpath = nil
      @spath = nil
      @upath = nil
      @npath = nil
      @rpath = nil
      @xpath = nil
      @ypath = nil
      @zpath = nil
      @epath = nil
      @ppath = nil
      @cpath = nil
      @tpath = nil

      @cur_path = nil
      
      @abs_array.clear
      @rel_array.clear
      @unc_array.clear
   end
end
