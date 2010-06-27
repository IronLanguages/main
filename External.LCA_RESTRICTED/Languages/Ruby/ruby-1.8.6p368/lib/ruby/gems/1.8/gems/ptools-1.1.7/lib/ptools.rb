require 'rbconfig'
require 'win32/file' if Config::CONFIG['host_os'].match('mswin')

class File
   PTOOLS_VERSION = '1.1.7'

   # :stopdoc:

   if Config::CONFIG['host_os'].match('mswin')
      IS_WINDOWS = true
      begin
         WIN32EXTS = ENV['PATHEXT'].split(';').map{ |e| e.downcase }
      rescue
         WIN32EXTS = %w/.exe .com .bat/
      end
   else
      IS_WINDOWS = false
   end

   IMAGE_EXT = %w/.bmp .gif .jpg .jpeg .png/

   # :startdoc:

   # Returns whether or not the file is an image. Only JPEG, PNG, BMP and
   # GIF are checked against.
   #
   # This method does some simple read and extension checks. For a version
   # that is more robust, but which depends on a 3rd party C library (and is
   # difficult to build on MS Windows), see the 'filemagic' library, available
   # on the RAA.
   #--
   # Approach used here is based on information found at
   # http://en.wikipedia.org/wiki/Magic_number_(programming) 
   #
   def self.image?(file)
      bool = IMAGE_EXT.include?(File.extname(file).downcase)      # Match ext
      bool = bmp?(file) || jpg?(file) || png?(file) || gif?(file) # Check data
      bool
   end

   # Returns the null device (aka bitbucket) on your platform.  On most
   # Unix-like systems this is '/dev/null', on Windows it's 'NUL', etc.
   #--
   # Based on information from  http://en.wikipedia.org/wiki//dev/null
   #
   def self.null
      case Config::CONFIG['host_os']
         when /mswin/i
            'NUL'
         when /amiga/i
            'NIL:'
         when /openvms/i
            'NL:'
         else
            '/dev/null'
      end
   end

   # Returns whether or not +file+ is a binary file.  Note that this is
   # not guaranteed to be 100% accurate.  It performs a "best guess" based
   # on a simple test of the first +File.blksize+ characters.
   #--
   # Based on code originally provided by Ryan Davis (which, in turn, is
   # based on Perl's -B switch).
   #
   def self.binary?(file)
      s = (File.read(file, File.stat(file).blksize) || "").split(//)
      ((s.size - s.grep(" ".."~").size) / s.size.to_f) > 0.30
   end
   
   # Looks for the first occurrence of +program+ within +path+.
   # 
   # On Windows, it looks for executables ending with the suffixes defined
   # in your PATHEXT environment variable, or '.exe', '.bat' and '.com' if
   # that isn't defined, which you may optionally include in +program+.
   #
   # Returns nil if not found. 
   #
   def self.which(program, path=ENV['PATH'])
      programs = [program]
      
      # If no file extension is provided on Windows, try the WIN32EXT's in turn
      if IS_WINDOWS && File.extname(program).empty?
         unless WIN32EXTS.include?(File.extname(program).downcase)
            WIN32EXTS.each{ |ext|
               programs.push(program + ext)
            }
         end
      end
      
      # Catch the first path found, or nil
      location = catch(:done){
         path.split(File::PATH_SEPARATOR).each{ |dir|
            programs.each{ |prog|
               f = File.join(dir, prog)
               if File.executable?(f) && !File.directory?(f)
                  location = File.join(dir, prog)
                  location.tr!('/', File::ALT_SEPARATOR) if File::ALT_SEPARATOR
                  throw(:done, location)
               end
            }
         }
         nil # Evaluate to nil if not found
      }

      location
   end

   # In block form, yields each +program+ within +path+.  In non-block
   # form, returns an array of each +program+ within +path+.
   #
   # On Windows, it looks for executables ending with the suffixes defined
   # in your PATHEXT environment variable, or '.exe', '.bat' and '.com' if
   # that isn't defined, which you may optionally include in +program+.
   #
   # Returns nil if not found.
   #
   def self.whereis(program, path=ENV['PATH'])
      dirs = []
      programs = [program]
      
      # If no file extension is provided on Windows, try the WIN32EXT's in turn
      if IS_WINDOWS && File.extname(program).empty?
         unless WIN32EXTS.include?(File.extname(program).downcase)
            WIN32EXTS.each{ |ext|
               programs.push(program + ext)
            }
         end
      end
      
      path.split(File::PATH_SEPARATOR).each{ |dir|
         programs.each{ |prog|
            file = File.join(dir,prog)
            file.tr!('/', File::ALT_SEPARATOR) if File::ALT_SEPARATOR
            if File.executable?(file) && !File.directory?(file)
               if block_given?
                  yield file
               else
                  dirs << file
               end
            end
         }
      }
      dirs.empty? ? nil : dirs.uniq
   end

   # In block form, yields the first +num_lines+ from +filename+.  In non-block
   # form, returns an Array of +num_lines+
   #
   def self.head(filename, num_lines=10)
      a = []
      IO.foreach(filename){ |line|
         break if num_lines <= 0
         num_lines -= 1
         if block_given?
            yield line
         else
            a << line
         end
      }
      return a.empty? ? nil : a # Return nil in block form
   end

   # In block form, yields line +from+ up to line +to+.  In non-block form
   # returns an Array of lines from +from+ to +to+.
   #
   def self.middle(filename, from=10, to=20)
      if block_given?
         IO.readlines(filename)[from-1..to-1].each{ |line| yield line }
      else
         IO.readlines(filename)[from-1..to-1]
      end
   end

   # In block form, yields the last +num_lines+ of file +filename+.
   # In non-block form, it returns the lines as an array.
   #
   # Note that this method slurps the entire file, so I don't recommend it
   # for very large files.  Also note that 'tail -f' functionality is not
   # present.
   #
   def self.tail(filename, num_lines=10)
      if block_given?
         IO.readlines(filename).reverse[0..num_lines-1].reverse.each{ |line|
            yield line
         }
      else
         IO.readlines(filename).reverse[0..num_lines-1].reverse
      end
   end

   # Converts a text file from one OS platform format to another, ala
   # 'dos2unix'.  Valid values for 'format', which are case insensitve,
   # include:
   #
   # * MS Windows -> dos, windows, win32, mswin
   # * Unix/BSD   -> unix, linux, bsd
   # * Mac        -> mac, macintosh, apple, osx
   #
   # Note that this method is only valid for an ftype of "file".  Otherwise a
   # TypeError will be raised.  If an invalid format value is received, an
   # ArgumentError is raised.
   #
   def self.nl_convert(filename, newfilename=filename, platform="dos")
      unless File.ftype(filename) == "file"
         raise TypeError, "Only valid for plain text files"
      end

      if platform =~ /dos|windows|win32|mswin/i
         format = "\cM\cJ"
      elsif platform =~ /unix|linux|bsd/i
         format = "\cJ"
      elsif platform =~ /mac|apple|macintosh|osx/i
         format = "\cM"
      else
         raise ArgumentError, "Invalid platform string"
      end

      orig = $\
      $\ = format

      if filename == newfilename
         require 'fileutils'
         require 'tempfile'

         begin
            tf = Tempfile.new('ruby_temp_' + Time.now.to_s)
            tf.open

            IO.foreach(filename){ |line|
               line.chomp!
               tf.print line
            }
         ensure
            tf.close if tf && !tf.closed?
         end
         File.delete(filename)
         FileUtils.cp(tf.path, filename)
      else
         begin
            nf = File.new(newfilename, 'w')  
            IO.foreach(filename){ |line|
               line.chomp!
               nf.print line
            }
         ensure
            nf.close if nf && !nf.closed?
         end
      end

      $\ = orig
      self
   end

   # Changes the access and modification time if present, or creates a 0
   # byte file +filename+ if it doesn't already exist.
   #
   def self.touch(filename)
      if File.exists?(filename)
         time = Time.now
         File.utime(time, time, filename)
      else
         File.open(filename, 'w'){}
      end
      self
   end

   # With no arguments, returns a four element array consisting of the number
   # of bytes, characters, words and lines in filename, respectively.
   #
   # Valid options are 'bytes', 'characters' (or just 'chars'), 'words' and
   # 'lines'.
   #
   def self.wc(filename, option='all')
      option.downcase!
      valid = %w/all bytes characters chars lines words/

      unless valid.include?(option)
         raise ArgumentError, "Invalid option: '#{option}'"
      end

      n = 0
      if option == 'lines'
         IO.foreach(filename){ n += 1 }
         return n
      elsif option == 'bytes'
         File.open(filename){ |f|
            f.each_byte{ n += 1 }
         }
         return n
      elsif option == 'characters' || option == 'chars'
         File.open(filename){ |f|
            while f.getc
               n += 1
            end
         }
         return n
      elsif option == 'words'
         IO.foreach(filename){ |line|
            n += line.split.length
         }
         return n
      else
         bytes,chars,lines,words = 0,0,0,0
         IO.foreach(filename){ |line|
            lines += 1
            words += line.split.length
            chars += line.split('').length
         }
         File.open(filename){ |f|
            while f.getc
               bytes += 1
            end
         }
         return [bytes,chars,words,lines]
      end
   end

   private

   def self.bmp?(file)
      IO.read(file, 3) == "BM6"
   end

   def self.jpg?(file)
      IO.read(file, 10) == "\377\330\377\340\000\020JFIF"
   end

   def self.png?(file)
      IO.read(file, 4) == "\211PNG"
   end

   def self.gif?(file)
      ['GIF89a', 'GIF97a'].include?(IO.read(file, 6))
   end
end
