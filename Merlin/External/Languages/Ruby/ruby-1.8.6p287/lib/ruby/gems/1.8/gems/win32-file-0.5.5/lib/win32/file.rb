require 'windows/security'
require 'windows/limits'
require 'win32/file/stat'

class File
   # Some of these are courtesy of win32-file-stat
   include Windows::Error
   include Windows::File
   include Windows::Security
   include Windows::Limits
   include Windows::DeviceIO
   extend Windows::Error
   extend Windows::File
   extend Windows::Path
   extend Windows::Security
   extend Windows::MSVCRT::Buffer
   extend Windows::Limits
   
   VERSION  = '0.5.5'
   MAX_PATH = 260

   # Abbreviated attribute constants for convenience
   ARCHIVE    = FILE_ATTRIBUTE_ARCHIVE
   COMPRESSED = FILE_ATTRIBUTE_COMPRESSED
   HIDDEN     = FILE_ATTRIBUTE_HIDDEN
   NORMAL     = FILE_ATTRIBUTE_NORMAL
   OFFLINE    = FILE_ATTRIBUTE_OFFLINE
   READONLY   = FILE_ATTRIBUTE_READONLY
   SYSTEM     = FILE_ATTRIBUTE_SYSTEM
   TEMPORARY  = FILE_ATTRIBUTE_TEMPORARY
   INDEXED    = 0x0002000
   CONTENT_INDEXED = 0x0002000

   # Custom Security rights  
   FULL = STANDARD_RIGHTS_ALL | FILE_READ_DATA | FILE_WRITE_DATA |
      FILE_APPEND_DATA | FILE_READ_EA | FILE_WRITE_EA | FILE_EXECUTE |
      FILE_DELETE_CHILD | FILE_READ_ATTRIBUTES | FILE_WRITE_ATTRIBUTES

   CHANGE = FILE_GENERIC_WRITE | FILE_GENERIC_READ | FILE_EXECUTE | DELETE
   READ   = FILE_GENERIC_READ | FILE_EXECUTE
   ADD    = 0x001201bf
   
   SECURITY_RIGHTS = {
      'FULL'    => FULL,
      'DELETE'  => DELETE, 
      'READ'    => READ, 
      'CHANGE'  => CHANGE,
      'ADD'     => ADD
   }

   ### Class Methods

   class << self
      # Strictly for making this code -w clean. They are removed later.
      alias basename_orig basename
      alias blockdev_orig blockdev?
      alias chardev_orig chardev?
      alias dirname_orig dirname
      alias lstat_orig lstat
      alias size_orig size
      alias split_orig split
      alias stat_orig stat

      ## Security
      
      # Sets the file permissions for the given file name.  The 'permissions'
      # argument is a hash with an account name as the key, and the various
      # permission constants as possible values.  The possible constant values
      # are:
      #
      # * FILE_READ_DATA
      # * FILE_WRITE_DATA
      # * FILE_APPEND_DATA
      # * FILE_READ_EA
      # * FILE_WRITE_EA
      # * FILE_EXECUTE
      # * FILE_DELETE_CHILD
      # * FILE_READ_ATTRIBUTES
      # * FILE_WRITE_ATTRIBUTES
      # * STANDARD_RIGHTS_ALL
      # * FULL
      # * READ
      # * ADD
      # * CHANGE
      # * DELETE
      # * READ_CONTROL
      # * WRITE_DAC
      # * WRITE_OWNER
      # * SYNCHRONIZE
      # * STANDARD_RIGHTS_REQUIRED
      # * STANDARD_RIGHTS_READ
      # * STANDARD_RIGHTS_WRITE
      # * STANDARD_RIGHTS_EXECUTE
      # * STANDARD_RIGHTS_ALL
      # * SPECIFIC_RIGHTS_ALL
      # * ACCESS_SYSTEM_SECURITY
      # * MAXIMUM_ALLOWED
      # * GENERIC_READ
      # * GENERIC_WRITE
      # * GENERIC_EXECUTE
      # * GENERIC_ALL
      #
      def set_permissions(file, perms)
         raise TypeError unless perms.kind_of?(Hash)

         account_rights = 0
         sec_desc = 0.chr * SECURITY_DESCRIPTOR_MIN_LENGTH

         unless InitializeSecurityDescriptor(sec_desc, 1)
            raise ArgumentError, get_last_error
         end

         cb_acl = 1024
         cb_sid = 1024

         acl_new = 0.chr * cb_acl

         unless InitializeAcl(acl_new, cb_acl, ACL_REVISION2)
            raise ArgumentError, get_last_error
         end

         sid      = 0.chr * cb_sid
         snu_type = 0.chr * cb_sid

         all_ace = 0.chr * ALLOW_ACE_LENGTH
         all_ace_ptr = memset(all_ace, 0, 0) # address of all_ace

         # all_ace_ptr->Header.AceType = ACCESS_ALLOWED_ACE_TYPE
         all_ace[0] = 0

         perms.each{ |account, mask|
            next if mask.nil?
           
            cch_domain = [80].pack('L')
            cb_sid     = [1024].pack('L')
            domain_buf = 0.chr * 80

            server, account = account.split("\\")

            if ['BUILTIN', 'NT AUTHORITY'].include?(server.upcase)
               server = nil
            end

            val = LookupAccountName(
               server,
               account,
               sid,
               cb_sid,
               domain_buf,
               cch_domain,
               snu_type
            )

            if val == 0
               raise ArgumentError, get_last_error
            end

            size = [0,0,0,0,0].pack('CCSLL').length # sizeof(ACCESS_ALLOWED_ACE)
            
            val = CopySid(
               ALLOW_ACE_LENGTH - size,
               all_ace_ptr + 8,  # address of all_ace_ptr->SidStart
               sid
            )

            if val == 0
               raise ArgumentError, get_last_error
            end

            if (GENERIC_ALL & mask).nonzero?
               account_rights = GENERIC_ALL & mask
            elsif (GENERIC_RIGHTS_CHK & mask).nonzero?
               account_rights = GENERIC_RIGHTS_MASK & mask
            end

            # all_ace_ptr->Header.AceFlags = INHERIT_ONLY_ACE|OBJECT_INHERIT_ACE
            all_ace[1] = INHERIT_ONLY_ACE | OBJECT_INHERIT_ACE

            2.times{
               if account_rights != 0
                  all_ace[2,2] = [12 - 4 + GetLengthSid(sid)].pack('S')
                  all_ace[4,4] = [account_rights].pack('L')

                  val = AddAce(
                     acl_new,
                     ACL_REVISION2,
                     MAXDWORD,
                     all_ace_ptr,
                     all_ace[2,2].unpack('S').first
                  )

                  if val == 0
                     raise ArgumentError, get_last_error
                  end

                  # all_ace_ptr->Header.AceFlags = CONTAINER_INHERIT_ACE
                  all_ace[1] = CONTAINER_INHERIT_ACE
               else
                  # all_ace_ptr->Header.AceFlags = 0
                  all_ace[1] = 0
               end

               account_rights = REST_RIGHTS_MASK & mask
            }
         }

         unless SetSecurityDescriptorDacl(sec_desc, 1, acl_new, 0)
            raise ArgumentError, get_last_error
         end

         unless SetFileSecurity(file, DACL_SECURITY_INFORMATION, sec_desc)
            raise ArgumentError, get_last_error
         end

         self
      end
      
      # Returns an array of human-readable strings that correspond to the
      # permission flags.
      #
      def securities(mask)
         sec_array = []
         if mask == 0
            sec_array.push('NONE')
         else
            if (mask & FULL) ^ FULL == 0
               sec_array.push('FULL')
            else
               SECURITY_RIGHTS.each{ |string, numeric|
                  if (numeric & mask) ^ numeric == 0
                     sec_array.push(string)
                  end
               }
            end
         end
         sec_array
      end

      # Returns a hash describing the current file permissions for the given
      # file.  The account name is the key, and the value is an integer
      # representing an or'd value that corresponds to the security
      # permissions for that file.
      #
      # To get a human readable version of the permissions, pass the value to
      # the +File.securities+ method.
      #
      def get_permissions(file, host=nil)
         current_length = 0
         length_needed  = [0].pack('L')
         sec_buf = ''

         loop do
            bool = GetFileSecurity(
               file,
               DACL_SECURITY_INFORMATION,
               sec_buf,
               sec_buf.length,
               length_needed
            )

            if bool == 0 && GetLastError() != ERROR_INSUFFICIENT_BUFFER
               raise ArgumentError, get_last_error
            end
            
            break if sec_buf.length >= length_needed.unpack('L').first
            sec_buf += ' ' * length_needed.unpack('L').first
         end

         control  = [0].pack('L')
         revision = [0].pack('L')

         unless GetSecurityDescriptorControl(sec_buf, control, revision)
            raise ArgumentError, get_last_error
         end

         # No DACL exists
         if (control.unpack('L').first & SE_DACL_PRESENT) == 0
            raise ArgumentError, 'No DACL present: explicit deny all'
         end

         dacl_present   = [0].pack('L')
         dacl_defaulted = [0].pack('L')
         dacl_ptr       = [0].pack('L')

         val = GetSecurityDescriptorDacl(
            sec_buf,
            dacl_present,
            dacl_ptr,
            dacl_defaulted
         )

         if val == 0
            raise ArgumentError, get_last_error
         end

         acl_buf = 0.chr * 8 # byte, byte, word, word, word (struct ACL)
         memcpy(acl_buf, dacl_ptr.unpack('L').first, acl_buf.size)

         if acl_buf.unpack('CCSSS').first == 0
            raise ArgumentError, 'DACL is NULL: implicit access grant'
         end

         ace_ptr   = [0].pack('L')
         ace_count = acl_buf.unpack('CCSSS')[3]

         perms_hash = {}
         0.upto(ace_count - 1){ |i|
            unless GetAce(dacl_ptr.unpack('L').first, i, ace_ptr)
               next
            end

            ace_buf = 0.chr * 12 # ACE_HEADER, dword, dword (ACCESS_ALLOWED_ACE)
            memcpy(ace_buf, ace_ptr.unpack('L').first, ace_buf.size)

            if ace_buf.unpack('CCS').first == ACCESS_ALLOWED_ACE_TYPE
               name        = 0.chr * MAX_PATH
               name_size   = [name.size].pack('L')
               domain      = 0.chr * MAX_PATH
               domain_size = [domain.size].pack('L')
               snu_ptr     = 0.chr * 4
               
               val = LookupAccountSid(
                  host,
                  ace_ptr.unpack('L').first + 8, # address of ace_ptr->SidStart
                  name,
                  name_size,
                  domain,
                  domain_size,
                  snu_ptr
               )
               
               if val == 0
                  raise ArgumentError, get_last_error
               end
               
               name   = name[0..name_size.unpack('L').first].split(0.chr)[0]
               domain = domain[0..domain_size.unpack('L').first].split(0.chr)[0]
               mask   = ace_buf.unpack('LLL')[1]
               
               unless domain.nil? || domain.empty?
                  name = domain + '\\' + name
               end
               
               perms_hash[name] = mask
            end
         }
         perms_hash
      end
      
      ## Encryption
      
      # Encrypts a file or directory. All data streams in a file are encrypted.
      # All new files created in an encrypted directory are encrypted.
      #
      # The caller must have the FILE_READ_DATA, FILE_WRITE_DATA,
      # FILE_READ_ATTRIBUTES, FILE_WRITE_ATTRIBUTES, and SYNCHRONIZE access
      # rights.
      # 
      # Requires exclusive access to the file being encrypted, and will fail if
      # another process is using the file.  If the file is compressed,
      # EncryptFile will decompress the file before encrypting it.
      # 
      # Windows 2000 or later only.
      #
      def encrypt(file)
         unless EncryptFile(file)
            raise ArgumentError, get_last_error
         end
         self
      end
      
      # Decrypts an encrypted file or directory.
      #  
      # The caller must have the FILE_READ_DATA, FILE_WRITE_DATA,
      # FILE_READ_ATTRIBUTES, FILE_WRITE_ATTRIBUTES, and SYNCHRONIZE access
      # rights.
      # 
      # Requires exclusive access to the file being decrypted, and will fail if
      # another process is using the file. If the file is not encrypted an error
      # is NOT raised.
      # 
      # Windows 2000 or later only.
      #
      def decrypt(file)
         unless DecryptFile(file, 0)
            raise ArgumentError, get_last_error
         end
         self
      end

      ## Path methods
      
      # Returns the last component of the filename given in +filename+.  If
      # +suffix+ is given and present at the end of +filename+, it is removed.
      # Any extension can be removed by giving an extension of ".*".
      #
      # This was reimplemented because the current version does not handle UNC
      # paths properly, i.e. it should not return anything less than the root.
      # In all other respects it is identical to the current implementation.
      #
      # Examples:
      #
      #    File.basename("C:\\foo\\bar.txt")         -> "bar.txt"
      #    File.basename("C:\\foo\\bar.txt", ".txt") -> "bar"
      #    File.basename("\\\\foo\\bar")             -> "\\\\foo\\bar"
      #
      def basename(file, suffix = nil)
         fpath = false
         file = file.dup # Don't modify original string
         
         # We have to convert forward slashes to backslashes for the Windows
         # functions to work properly.
         if file.include?('/')
            file.tr!('/', '\\')
            fpath = true
         end
             
         # Return an empty or root path as-is.
         if file.empty? || PathIsRoot(file)
            file.tr!("\\", '/') if fpath
            return file
         end
         
         PathStripPath(file) # Gives us the basename
         
         if suffix
            if suffix == '.*'
               PathRemoveExtension(file)
            else
               if PathFindExtension(file) == suffix
                  PathRemoveExtension(file)
               end
            end
         end
         
         file = file.split(0.chr).first
         
         # Trim trailing slashes
         while file[-1].chr == "\\"
            file.chop!
         end
         
         # Return forward slashes if that's how the path was passed in.
         if fpath
            file.tr!("\\", '/')
         end
         
         file
      end

      # Returns all components of the filename given in +filename+ except the
      # last one.
      # 
      # This was reimplemented because the current version does not handle UNC
      # paths properly, i.e. it should not return anything less than the root.
      # In all other respects it is identical to the current implementation.
      #
      # Examples:
      #
      #    File.dirname("C:\\foo\\bar\\baz.txt") -> "C:\\foo\\bar"
      #    File.dirname("\\\\foo\\bar")          -> "\\\\foo\\bar"
      #
      def dirname(file)       
         fpath = false
         file = file.dup
         
         if file.include?('/')
            file.tr!('/', "\\")
            fpath = true
         end
         
         if PathIsRoot(file)
            file.tr!("\\", '/') if fpath
            return file
         end
         
         PathRemoveFileSpec(file)
         file = file.split(0.chr).first
         
         # Empty paths, short relative paths
         if file.nil? || (file && file.empty?)
            return '.'
         end
         
         PathRemoveBackslash(file)
         
         file.tr!("\\", '/') if fpath
         file
      end

      # Returns +file+ in long format. For example, if 'SOMEFI~1.TXT'
      # was the argument provided, and the short representation for
      # 'somefile.txt', then this method would return 'somefile.txt'.
      #
      # Note that certain file system optimizations may prevent this method
      # from working as expected.  In that case, you will get back the file
      # name in 8.3 format.
      #
      def long_path(file)
         buf = 0.chr * MAX_PATH
         if GetLongPathName(file, buf, buf.size) == 0
            raise ArgumentError, get_last_error
         end
         File.basename(buf.split(0.chr).first.strip)
      end

      # Returns 'file_name' in 8.3 format. For example, 'c:\documentation.doc'
      # would be returned as 'c:\docume~1.doc'.
      #
      def short_path(file)
         buf = 0.chr * MAX_PATH
         if GetShortPathName(file, buf, buf.size) == 0
            raise ArgumentError, get_last_error
         end
         File.basename(buf.split(0.chr).first.strip)
      end

      # Splits the given string into a directory and a file component and
      # returns them in a two element array.  This was reimplemented because
      # the current version does not handle UNC paths properly.
      # 
      def split(file)
         array = []
         
         if file.empty? || PathIsRoot(file)
            array.push(file, '')
         else
            array.push(File.dirname(file), File.basename(file))
         end
         array
      end

      ## Stat methods
      
      # Returns a File::Stat object, as defined in the win32-file-stat package.
      # 
      def stat(file)
         File::Stat.new(file)
      end
      
      # Identical to File.stat on Windows.
      # 
      def lstat(file)
         File::Stat.new(file)
      end
      
      # Returns the file system's block size.
      #
      def blksize(file)
         File::Stat.new(file).blksize
      end

      # Returns whether or not +file+ is a block device. For MS Windows this
      # means a removable drive, cdrom or ramdisk.
      #
      def blockdev?(file)
         File::Stat.new(file).blockdev?
      end

      # Returns true if the file is a character device.  This replaces the
      # current Ruby implementation which always returns false.
      #
      def chardev?(file)
         File::Stat.new(file).chardev?
      end

      # Returns the size of the file in bytes.
      #
      # This was reimplemented because the current version does not handle file
      # sizes greater than 2gb.
      #
      def size(file)
         File::Stat.new(file).size
      end

      # We no longer need the aliases, so remove them
      remove_method(:basename_orig, :blockdev_orig, :chardev_orig)
      remove_method(:dirname_orig, :lstat_orig, :size_orig)
      remove_method(:split_orig, :stat_orig)
   end # class << self

   ## Attribute methods

   # Returns true if the file or directory is an archive file. Applications
   # use this attribute to mark files for backup or removal.
   # 
   def self.archive?(file)
      File::Stat.new(file).archive?
   end
   
   # Returns true if the file or directory is compressed. For a file, this
   # means that all of the data in the file is compressed. For a directory,
   # this means that compression is the default for newly created files and
   # subdirectories.
   #
   def self.compressed?(file)
      File::Stat.new(file).compressed?
   end

   # Returns true if the file or directory is encrypted. For a file, this
   # means that all data in the file is encrypted. For a directory, this
   # means that encryption is the default for newly created files and
   # subdirectories.
   #
   def self.encrypted?(file)
      File::Stat.new(file).encrypted?
   end

   # Returns true if the file or directory is hidden. It is not included
   # in an ordinary directory listing.
   # 
   def self.hidden?(file)
      File::Stat.new(file).hidden?
   end
   
   # Returns true if the file or directory is indexed by the content indexing
   # service.
   #
   def self.indexed?(file)
      File::Stat.new(file).indexed?
   end

   # Returns true if the file or directory has no other attributes set.
   #
   def self.normal?(file)
      File::Stat.new(file).normal?
   end
   
   # Returns true if the data of the file is not immediately available. This
   # attribute indicates that the file data has been physically moved to
   # offline storage. This attribute is used by Remote Storage, the
   # hierarchical storage management software. Applications should not
   # arbitrarily change this attribute.
   #
   def self.offline?(file)
      File::Stat.new(file).offline?
   end

   # Returns true if The file or directory is read-only. Applications can
   # read the file but cannot write to it or delete it. In the case of a
   # directory, applications cannot delete it.
   #
   def self.readonly?(file)
      File::Stat.new(file).readonly?
   end
   
   

   # Returns true if the file or directory has an associated reparse point. A
   # reparse point is a collection of user defined data associated with a file
   # or directory.  For more on reparse points, search
   # http://msdn.microsoft.com.
   #
   def self.reparse_point?(file)
      File::Stat.new(file).reparse_point?
   end

   # Returns true if the file is a sparse file.  A sparse file is a file in
   # which much of the data is zeros, typically image files.  See
   # http://msdn.microsoft.com for more details.
   #
   def self.sparse?(file)
      File::Stat.new(file).sparse?
   end

   # Returns true if the file or directory is part of the operating system
   # or is used exclusively by the operating system.
   #
   def self.system?(file)
      File::Stat.new(file).system?
   end

   # Returns true if the file is being used for temporary storage.
   #
   # File systems avoid writing data back to mass storage if sufficient cache
   # memory is available, because often the application deletes the temporary
   # file shortly after the handle is closed. In that case, the system can
   # entirely avoid writing the data. Otherwise, the data will be written after
   # the handle is closed.
   #
   def self.temporary?(file)
      File::Stat.new(file).temporary?
   end
   
   # Returns an array of strings indicating the attributes for that file.  The
   # possible values are:
   # 
   # archive
   # compressed
   # directory
   # encrypted
   # hidden
   # indexed
   # normal
   # offline
   # readonly
   # reparse_point
   # sparse
   # system
   # temporary
   #
   def self.attributes(file)
      attributes = GetFileAttributes(file)
      arr = []
      
      if attributes == INVALID_FILE_ATTRIBUTES
         raise ArgumentError, get_last_error
      end
      
      arr.push('archive') if archive?(file)
      arr.push('compressed') if compressed?(file)
      arr.push('directory') if directory?(file)
      arr.push('encrypted') if encrypted?(file)
      arr.push('hidden') if hidden?(file)
      arr.push('indexed') if indexed?(file)
      arr.push('normal') if normal?(file)
      arr.push('offline') if offline?(file)
      arr.push('readonly') if readonly?(file)
      arr.push('reparse_point') if reparse_point?(file)
      arr.push('sparse') if sparse?(file)
      arr.push('system') if system?(file)
      arr.push('temporary') if temporary?(file)
      
      arr
   end
   
   # Sets the file attributes based on the given (numeric) +flags+.  This does
   # not remove existing attributes, it merely adds to them.
   #
   def self.set_attributes(file, flags)
      attributes = GetFileAttributes(file)
       
      if attributes == INVALID_FILE_ATTRIBUTES
         raise ArgumentError, get_last_error
      end
      
      attributes |= flags
      
      if SetFileAttributes(file, attributes) == 0
         raise ArgumentError, get_last_error
      end
      
      self
   end
   
   # Removes the file attributes based on the given (numeric) +flags+.
   #
   def self.remove_attributes(file, flags)
      attributes = GetFileAttributes(file)
       
      if attributes == INVALID_FILE_ATTRIBUTES
         raise ArgumentError, get_last_error
      end
      
      attributes &= ~flags
      
      if SetFileAttributes(file, attributes) == 0
         raise ArgumentError, get_last_error
      end
      
      self
   end
   
   # Instance methods
   
   def stat
      File::Stat.new(self.path)
   end
   
   # Sets whether or not the file is an archive file.
   #
   def archive=(bool)
      attributes = GetFileAttributes(self.path)
     
      if attributes == INVALID_FILE_ATTRIBUTES
         raise ArgumentError, get_last_error
      end
      
      if bool
         attributes |= FILE_ATTRIBUTE_ARCHIVE;
      else
         attributes &= ~FILE_ATTRIBUTE_ARCHIVE;
      end
      
      if SetFileAttributes(self.path, attributes) == 0
         raise ArgumentError, get_last_error
      end

      self
   end
   
   # Sets whether or not the file is a compressed file.
   #
   def compressed=(bool)     
      in_buf = bool ? COMPRESSION_FORMAT_DEFAULT : COMPRESSION_FORMAT_NONE
      in_buf = [in_buf].pack('L')
      bytes  = [0].pack('L')
      
      handle = CreateFile(
         self.path,
         FILE_READ_DATA | FILE_WRITE_DATA,
         FILE_SHARE_READ | FILE_SHARE_WRITE,
         0,
         OPEN_EXISTING,
         0,
         0
      )
      
      if handle == INVALID_HANDLE_VALUE
         raise ArgumentError, get_last_error
      end
      
      val = DeviceIoControl(
         handle,
         FSCTL_SET_COMPRESSION(),
         in_buf,
         in_buf.length,
         0,
         0,
         bytes,
         0
      )
      
      if val == 0
         raise ArgumentError, get_last_error
      end
      
      self
   end
   
   # Sets the hidden attribute to true or false.  Setting this attribute to
   # true means that the file is not included in an ordinary directory listing.
   #
   def hidden=(bool)     
      attributes = GetFileAttributes(self.path)
      
      if attributes == INVALID_FILE_ATTRIBUTES
         raise ArgumentError, get_last_error
      end
      
      if bool
         attributes |= FILE_ATTRIBUTE_HIDDEN;
      else
         attributes &= ~FILE_ATTRIBUTE_HIDDEN;
      end
      
      if SetFileAttributes(self.path, attributes) == 0
         raise ArgumentError, get_last_error
      end
      self
   end
   
   # Sets the 'indexed' attribute to true or false.  Setting this to
   # false means that the file will not be indexed by the content indexing
   # service.
   #
   def indexed=(bool)
      attributes = GetFileAttributes(self.path)
      
      if attributes == INVALID_FILE_ATTRIBUTES
         raise ArgumentError, get_last_error
      end
      
      if bool
         attributes &= ~FILE_ATTRIBUTE_NOT_CONTENT_INDEXED;
      else
         attributes |= FILE_ATTRIBUTE_NOT_CONTENT_INDEXED;
      end
      
      if SetFileAttributes(self.path, attributes) == 0
         raise ArgumentError, get_last_error
      end

      self
   end
   
   alias :content_indexed= :indexed=
   
   # Sets the normal attribute.  Note that only 'true' is a valid argument,
   # which has the effect of removing most other attributes.  Attempting to
   # pass any value except true will raise an ArgumentError.
   #
   def normal=(bool)
      unless bool
         raise ArgumentError, "only 'true' may be passed as an argument"
      end

      if SetFileAttributes(self.path, FILE_ATTRIBUTE_NORMAL) == 0
         raise ArgumentError, get_last_error
      end

      self
   end
   
   # Sets whether or not a file is online or not.  Setting this to false means
	# that the data of the file is not immediately available. This attribute
	# indicates that the file data has been physically moved to offline storage.
	# This attribute is used by Remote Storage, the hierarchical storage
	# management software.
   # 
	# Applications should not arbitrarily change this attribute.
   #
   def offline=(bool)
      attributes = GetFileAttributes(self.path)

      if attributes == INVALID_FILE_ATTRIBUTES
         raise ArgumentError, get_last_error
      end
      
      if bool
         attributes |= FILE_ATTRIBUTE_OFFLINE;
      else
         attributes &= ~FILE_ATTRIBUTE_OFFLINE;
      end
      
      if SetFileAttributes(self.path, attributes) == 0
         raise ArgumentError, get_last_error
      end

      self
   end
   
   # Sets the readonly attribute.  If set to true the the file or directory is
   # readonly. Applications can read the file but cannot write to it or delete
   # it. In the case of a directory, applications cannot delete it.
   # 
   def readonly=(bool)
      attributes = GetFileAttributes(self.path)
      
      if attributes == INVALID_FILE_ATTRIBUTES
         raise ArgumentError, get_last_error
      end
      
      if bool
         attributes |= FILE_ATTRIBUTE_READONLY;
      else
         attributes &= ~FILE_ATTRIBUTE_READONLY;
      end
      
      if SetFileAttributes(self.path, attributes) == 0
         raise ArgumentError, get_last_error
      end

      self
   end
   
   # Sets the file to a sparse (usually image) file.  Note that you cannot
   # remove the sparse property from a file.
   #
   def sparse=(bool)     
      unless bool
         warn 'cannot remove sparse property from a file - operation ignored'
         return
      end
      
      bytes = [0].pack('L')
      
      handle = CreateFile(
         self.path,
         FILE_READ_DATA | FILE_WRITE_DATA,
         FILE_SHARE_READ | FILE_SHARE_WRITE,
         0,
         OPEN_EXISTING,
         FSCTL_SET_SPARSE(),
         0
      )
      
      if handle == INVALID_HANDLE_VALUE
         raise ArgumentError, get_last_error
      end
      
      val = DeviceIoControl(
         handle,
         FSCTL_SET_SPARSE(),
         0,
         0,
         0,
         0,
         bytes,
         0
      )
      
      if val == 0
         raise ArgumentError, get_last_error
      end
      
      self
   end
   
   # Set whether or not the file is a system file.  A system file is a file
	# that is part of the operating system or is used exclusively by it.
   #
   def system=(bool)
      attributes = GetFileAttributes(self.path)
      
      if attributes == INVALID_FILE_ATTRIBUTES
         raise ArgumentError, get_last_error
      end
      
      if bool
         attributes |= FILE_ATTRIBUTE_SYSTEM;
      else
         attributes &= ~FILE_ATTRIBUTE_SYSTEM;
      end
      
      if SetFileAttributes(self.path, attributes) == 0
         raise ArgumentError, get_last_error
      end

      self
   end
   
   # Sets whether or not the file is being used for temporary storage.
   # 
   # File systems avoid writing data back to mass storage if sufficient cache
   # memory is available, because often the application deletes the temporary
   # file shortly after the handle is closed. In that case, the system can
   # entirely avoid writing the data. Otherwise, the data will be written
   # after the handle is closed.
   # 
   def temporary=(bool)
      attributes = GetFileAttributes(self.path)

      if attributes == INVALID_FILE_ATTRIBUTES
         raise ArgumentError, get_last_error
      end
      
      if bool
         attributes |= FILE_ATTRIBUTE_TEMPORARY;
      else
         attributes &= ~FILE_ATTRIBUTE_TEMPORARY;
      end
      
      if SetFileAttributes(self.path, attributes) == 0
         raise ArgumentError, get_last_error
      end

      self
   end
   
   # Singleton aliases, mostly for backwards compatibility
   class << self
      alias :read_only? :readonly?
      alias :content_indexed? :indexed?
      alias :set_attr :set_attributes
      alias :unset_attr :remove_attributes
   end
end
