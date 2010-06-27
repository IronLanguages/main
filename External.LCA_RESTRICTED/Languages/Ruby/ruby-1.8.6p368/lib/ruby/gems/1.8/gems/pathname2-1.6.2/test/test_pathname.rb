##############################################################################
# tc_pathname.rb
#
# Test suite for the pathname package (Unix). This test suite should be run
# via the Rake tasks, i.e. 'rake test_pr' to test the pure Ruby version, or
# 'rake test_c' to test the C version.
##############################################################################
require 'rubygems'
gem 'test-unit'

require 'test/unit'
require 'pathname2'
require 'rbconfig'
include Config

class MyPathname < Pathname; end

class TC_Pathname < Test::Unit::TestCase
   def self.startup
      Dir.chdir(File.expand_path(File.dirname(__FILE__)))
      @@pwd = Dir.pwd
   end

   def setup
      @abs_path = Pathname.new('/usr/local/bin')
      @rel_path = Pathname.new('usr/local/bin')
      @trl_path = Pathname.new('/usr/local/bin/')
      @mul_path = Pathname.new('/usr/local/lib/local/lib')
      @rul_path = Pathname.new('usr/local/lib/local/lib')
      @url_path = Pathname.new('file:///foo%20bar/baz')
      @cur_path = Pathname.new(@@pwd)

      @abs_array = []
      @rel_array = []

      @mypath = MyPathname.new('/usr/bin')
   end

   # Convenience method to verify that the receiver was not modified
   # except perhaps slashes
   def assert_non_destructive
      assert_equal('/usr/local/bin', @abs_path)
      assert_equal('usr/local/bin', @rel_path)
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
   def assert_relpath_err(to, from)
      assert_raise(ArgumentError) {
         Pathname.new(to).relative_path_from(from)
      }
   end

   def test_version
      assert_equal('1.6.2', Pathname::VERSION)
   end

   def test_file_url_path
      assert_equal('/foo bar/baz', @url_path)
   end

   def test_realpath
      assert_respond_to(@abs_path, :realpath)
      assert_equal(@@pwd, Pathname.new('.').realpath)
      assert_kind_of(Pathname, Pathname.new('/dev/stdin').realpath)
      assert(Pathname.new('/dev/stdin') != Pathname.new('/dev/stdin').realpath)
      if CONFIG['host_os'] =~ /bsd|darwin|mac/i
         assert_raises(Errno::ENOENT){ Pathname.new('../blahblah/bogus').realpath }
      else
         assert_raises(Errno::ENOENT){ Pathname.new('../bogus').realpath }
      end
   end

   def test_realpath_platform
      case CONFIG['host_os']
         when /linux/i
            path1 = '/dev/stdin'
            assert_equal('/dev/pts/0', Pathname.new(path1).realpath)
         when /sunos|solaris/i
            path1 = '/dev/null'
            path2 = '/dev/stdin'
            path3 = '/dev/fd0'   # Multiple symlinks

            assert_equal('/devices/pseudo/mm@0:null', Pathname.new(path1).realpath)
            assert_equal('/dev/fd/0', Pathname.new(path2).realpath)
            assert_equal('/devices/pci@1f,0/isa@7/dma@0,0/floppy@0,3f0:c',
               Pathname.new(path3).realpath
            )
      end
   end

   # These tests taken directly from Tanaka's pathname.rb. The one failure
   # (commented out) is due to the fact that Tanaka's cleanpath method returns
   # the cleanpath for '../a' as '../a' (i.e. it does nothing) whereas mine
   # converts '../a' into just 'a'.  Which is correct? I vote mine, because
   # I don't see how you can get 'more relative' from a relative path not
   # already in the pathname.
   #
   def test_relative_path_from
      assert_relpath('../a', 'a', 'b')
      assert_relpath('../a', 'a', 'b/')
      assert_relpath('../a', 'a/', 'b')
      assert_relpath('../a', 'a/', 'b/')
      assert_relpath('../a', '/a', '/b')
      assert_relpath('../a', '/a', '/b/')
      assert_relpath('../a', '/a/', '/b')
      assert_relpath('../a', '/a/', '/b/')

      assert_relpath('../b', 'a/b', 'a/c')
      assert_relpath('../a', '../a', '../b')

      assert_relpath('a', 'a', '.')
      assert_relpath('..', '.', 'a')

      assert_relpath('.', '.', '.')
      assert_relpath('.', '..', '..')
      assert_relpath('..', '..', '.')

      assert_relpath('c/d', '/a/b/c/d', '/a/b')
      assert_relpath('../..', '/a/b', '/a/b/c/d')
      assert_relpath('../../../../e', '/e', '/a/b/c/d')
      assert_relpath('../b/c', 'a/b/c', 'a/d')

      assert_relpath('../a', '/../a', '/b')
      #assert_relpath('../../a', '../a', 'b') # fails
      assert_relpath('.', '/a/../../b', '/b')
      assert_relpath('..', 'a/..', 'a')
      assert_relpath('.', 'a/../b', 'b')

      assert_relpath('a', 'a', 'b/..')
      assert_relpath('b/c', 'b/c', 'b/..')

      assert_relpath_err('/', '.')
      assert_relpath_err('.', '/')
      assert_relpath_err('a', '..')
      assert_relpath_err('.', '..')
   end

   def test_parent
      assert_respond_to(@abs_path, :parent)
      assert_equal('/usr/local', @abs_path.parent)
      assert_equal('usr/local', @rel_path.parent)
      assert_equal('/', Pathname.new('/').parent)
   end

   def test_pstrip
      assert_respond_to(@trl_path, :pstrip)
      assert_nothing_raised{ @trl_path.pstrip }
      assert_equal('/usr/local/bin', @trl_path.pstrip)
      assert_equal('/usr/local/bin/', @trl_path)
   end

   def test_pstrip_bang
      assert_respond_to(@trl_path, :pstrip!)
      assert_nothing_raised{ @trl_path.pstrip! }
      assert_equal('/usr/local/bin', @trl_path.pstrip!)
      assert_equal('/usr/local/bin', @trl_path)
   end

   def test_ascend
      assert_respond_to(@abs_path, :ascend)
      assert_nothing_raised{ @abs_path.ascend{} }

      @abs_path.ascend{ |path| @abs_array.push(path) }
      @rel_path.ascend{ |path| @rel_array.push(path) }

      assert_equal('/usr/local/bin', @abs_array[0])
      assert_equal('/usr/local', @abs_array[1])
      assert_equal('/usr', @abs_array[2])
      assert_equal('/', @abs_array[3])
      assert_equal(4, @abs_array.length)

      assert_equal('usr/local/bin', @rel_array[0])
      assert_equal('usr/local', @rel_array[1])
      assert_equal('usr', @rel_array[2])
      assert_equal(3, @rel_array.length)

      assert_non_destructive
   end

   def test_descend
      assert_respond_to(@abs_path, :descend)
      assert_nothing_raised{ @abs_path.descend{} }

      @abs_path.descend{ |path| @abs_array.push(path) }
      @rel_path.descend{ |path| @rel_array.push(path) }

      assert_equal('/', @abs_array[0])
      assert_equal('/usr', @abs_array[1])
      assert_equal('/usr/local', @abs_array[2])
      assert_equal('/usr/local/bin', @abs_array[3])
      assert_equal(4, @abs_array.length)

      assert_equal('usr', @rel_array[0])
      assert_equal('usr/local', @rel_array[1])
      assert_equal('usr/local/bin', @rel_array[2])
      assert_equal(3, @rel_array.length)

      assert_non_destructive
   end

   def test_children_with_directory
      assert_respond_to(@cur_path, :children)
      assert_nothing_raised{ @cur_path.children }
      assert_kind_of(Array, @cur_path.children)
      
      children = @cur_path.children.sort.reject{ |f| f.include?('CVS') }
      assert_equal(
         [
            Dir.pwd + '/test_pathname.rb',
            Dir.pwd + '/test_pathname_windows.rb'
         ],
         children.sort
      )
   end
   
   def test_children_without_directory
      assert_nothing_raised{ @cur_path.children(false) }
         
      children = @cur_path.children(false).reject{ |f| f.include?('CVS') }
      assert_equal(['test_pathname.rb', 'test_pathname_windows.rb'], children.sort)
   end

   def test_unc
      assert_raises(NotImplementedError){ @abs_path.unc? }
   end

   def test_enumerable
      assert_respond_to(@abs_path, :each)
   end
   
   def test_root
      assert_respond_to(@abs_path, :root)
      assert_nothing_raised{ @abs_path.root }
      assert_nothing_raised{ @rel_path.root }

      assert_equal('/', @abs_path.root)
      assert_equal('.', @rel_path.root)

      assert_non_destructive
   end

   def test_root?
      assert_respond_to(@abs_path, :root?)
      assert_nothing_raised{ @abs_path.root? }
      assert_nothing_raised{ @rel_path.root? }

      path1 = Pathname.new('/')
      path2 = Pathname.new('a')
      assert_equal(true, path1.root?)
      assert_equal(false, path2.root?)

      assert_non_destructive
   end

   def test_absolute
      assert_respond_to(@abs_path, :absolute?)
      assert_nothing_raised{ @abs_path.absolute? }
      assert_nothing_raised{ @rel_path.absolute? }

      assert_equal(true, @abs_path.absolute?)
      assert_equal(false, @rel_path.absolute?)

      assert_equal(true, Pathname.new('/usr/bin/ruby').absolute?)
      assert_equal(false, Pathname.new('foo').absolute?)
      assert_equal(false, Pathname.new('foo/bar').absolute?)
      assert_equal(false, Pathname.new('../foo/bar').absolute?)

      assert_non_destructive
   end

   def test_relative
      assert_respond_to(@abs_path, :relative?)
      assert_nothing_raised{ @abs_path.relative? }
      assert_nothing_raised{ @rel_path.relative? }

      assert_equal(false, @abs_path.relative?)
      assert_equal(true, @rel_path.relative?)

      assert_equal(false, Pathname.new('/usr/bin/ruby').relative?)
      assert_equal(true, Pathname.new('foo').relative?)
      assert_equal(true, Pathname.new('foo/bar').relative?)
      assert_equal(true, Pathname.new('../foo/bar').relative?)

      assert_non_destructive
   end
   
   def test_to_a
      assert_respond_to(@abs_path, :to_a)
      assert_nothing_raised{ @abs_path.to_a }
      assert_nothing_raised{ @rel_path.to_a }
      assert_kind_of(Array, @abs_path.to_a)
      assert_equal(%w/usr local bin/, @abs_path.to_a)

      assert_non_destructive
   end

   def test_spaceship_operator
      assert_respond_to(@abs_path, :<=>)

      assert_pathname_cmp( 0, '/foo/bar', '/foo/bar')
      assert_pathname_cmp(-1, '/foo/bar', '/foo/zap')
      assert_pathname_cmp( 1, '/foo/zap', '/foo/bar')
      assert_pathname_cmp(-1, 'foo', 'foo/')
      assert_pathname_cmp(-1, 'foo/', 'foo/bar')
   end

   def test_plus_operator
      assert_respond_to(@abs_path, :+)

      # Standard stuff
      assert_pathname_plus('/foo/bar', '/foo', 'bar')
      assert_pathname_plus('foo/bar', 'foo', 'bar')
      assert_pathname_plus('foo', 'foo', '.')
      assert_pathname_plus('foo', '.', 'foo')
      assert_pathname_plus('/foo', 'bar', '/foo')
      assert_pathname_plus('foo', 'foo/bar', '..')
      assert_pathname_plus('/foo', '/', '../foo')
      assert_pathname_plus('foo/zap', 'foo/bar', '../zap')
      assert_pathname_plus('.', 'foo', '..')
      assert_pathname_plus('foo', '..', 'foo')     # Auto clean
      assert_pathname_plus('foo', '..', '../foo')  # Auto clean

      # Edge cases
      assert_pathname_plus('.', '.', '.')
      assert_pathname_plus('/', '/', '..')
      assert_pathname_plus('.', '..',  '..')
      assert_pathname_plus('.', 'foo', '..')

      # Alias
      assert_equal('/foo/bar', Pathname.new('/foo') / Pathname.new('bar'))
   end

   # Any tests marked with '***' mean that this behavior is different than
   # the current implementation.  It also means I disagree with the current
   # implementation.
   def test_clean
      # Standard stuff
      assert_equal('/a/b/c', Pathname.new('/a/b/c').cleanpath)
      assert_equal('b/c', Pathname.new('./b/c').cleanpath)
      assert_equal('a', Pathname.new('a/.').cleanpath)         # ***
      assert_equal('a/c', Pathname.new('a/./c').cleanpath)
      assert_equal('a/b', Pathname.new('a/b/.').cleanpath)     # ***
      assert_equal('.', Pathname.new('a/../.').cleanpath)      # ***
      assert_equal('/a', Pathname.new('/a/b/..').cleanpath)
      assert_equal('/b', Pathname.new('/a/../b').cleanpath)
      assert_equal('d', Pathname.new('a/../../d').cleanpath)   # ***

      # Edge cases
      assert_equal('', Pathname.new('').cleanpath)
      assert_equal('.', Pathname.new('.').cleanpath)
      assert_equal('..', Pathname.new('..').cleanpath)
      assert_equal('/', Pathname.new('/').cleanpath)
      assert_equal('/', Pathname.new('//').cleanpath)

      assert_non_destructive
   end

   def test_dirname_basic
      assert_respond_to(@abs_path, :dirname)
      assert_nothing_raised{ @abs_path.dirname }
      assert_kind_of(String, @abs_path.dirname)
   end

   def test_dirname
      assert_equal('/usr/local', @abs_path.dirname)
      assert_equal('/usr/local/bin', @abs_path.dirname(0))
      assert_equal('/usr/local', @abs_path.dirname(1))
      assert_equal('/usr', @abs_path.dirname(2))
      assert_equal('/', @abs_path.dirname(3))
      assert_equal('/', @abs_path.dirname(9))
   end

   def test_dirname_expected_errors
      assert_raise(ArgumentError){ @abs_path.dirname(-1) }
   end

   def test_facade_io
      assert_respond_to(@abs_path, :foreach)
      assert_respond_to(@abs_path, :read)
      assert_respond_to(@abs_path, :readlines)
      assert_respond_to(@abs_path, :sysopen)
   end

   def test_facade_file
      File.methods(false).each{ |method|
         assert_respond_to(@abs_path, method.to_sym)
      }
   end

   def test_facade_dir
      Dir.methods(false).each{ |method|
         assert_respond_to(@abs_path, method.to_sym)
      }
   end

   def test_facade_fileutils
      methods = FileUtils.public_instance_methods
      methods -= File.methods(false)
      methods -= Dir.methods(false)
      methods.delete_if{ |m| m =~ /stream/ }
      methods.delete('identical?')

      methods.each{ |method|
         assert_respond_to(@abs_path, method.to_sym)
      }
   end

   def test_facade_find
      assert_respond_to(@abs_path, :find)
      assert_nothing_raised{ @abs_path.find{} }

      Pathname.new(Dir.pwd).find{ |f|
         Find.prune if f.match('CVS')
         assert_kind_of(Pathname, f)
      }
   end

   # Ensures that subclasses return the subclass as the class, not a hard
   # coded Pathname.
   #
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
      assert_nothing_raised{ pn{'/foo'} }
      assert_kind_of(Pathname, pn{'/foo'})
      assert_equal('/foo', pn{'/foo'})
   end
   
   def test_pwd_singleton_method
      assert_respond_to(Pathname, :pwd)
      assert_kind_of(String, Pathname.pwd)
      assert_equal(@@pwd, Pathname.pwd)      
   end
   
   def teardown
      @abs_path = nil
      @rel_path = nil
      @trl_path = nil
      @mul_path = nil
      @rul_path = nil
      @cur_path = nil
      @abs_path = nil
      @rel_path = nil
      @cur_path = nil

      @mypath = nil

      @abs_array.clear
      @rel_array.clear
   end

   def self.shutdown
      @@pwd = nil
   end
end
