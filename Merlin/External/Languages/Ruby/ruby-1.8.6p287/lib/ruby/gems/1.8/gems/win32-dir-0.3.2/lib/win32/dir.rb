require 'windows/directory'
require 'windows/shell'
require 'windows/file'
require 'windows/error'
require 'windows/device_io'
require 'windows/unicode'
require 'windows/directory'
require 'windows/handle'
require 'windows/path'

class Dir
   include Windows::Directory
   include Windows::Shell
   include Windows::Error
   include Windows::File
   include Windows::DeviceIO
   extend Windows::Directory
   extend Windows::Shell
   extend Windows::File
   extend Windows::Error
   extend Windows::DeviceIO
   extend Windows::Unicode
   extend Windows::Handle
   extend Windows::Path
   
   VERSION = '0.3.2'
   
   # Dynamically set each of the CSIDL_ constants
   constants.grep(/CSIDL/).each{ |constant|
      path   = 0.chr * 260
      nconst = constant.split('CSIDL_').last
      
      if SHGetFolderPath(0, const_get(constant), 0, 1, path) != 0
         path = nil
      else 
         path.strip!
      end
      
      Dir.const_set(nconst, path)    
   }
   
   # Creates the symlink +to+, linked to the existing directory +from+.  If the
	# +to+ directory already exists, it must be empty or an error is raised.
	# 
   def self.create_junction(to, from)
      # Normalize the paths
      to.tr!('/', "\\")
      from.tr!('/', "\\")
      
      to_path    = 0.chr * 260
      from_path  = 0.chr * 260
      buf_target = 0.chr * 260
      
      if GetFullPathName(from, from_path.size, from_path, 0) == 0
         raise StandardError, 'GetFullPathName() failed: ' + get_last_error
      end
      
      if GetFullPathName(to, to_path.size, to_path, 0) == 0
         raise StandardError, 'GetFullPathName() failed: ' + get_last_error
      end
      
      to_path   = to_path.split(0.chr).first
      from_path = from_path.split(0.chr).first
      
      # You can create a junction to a directory that already exists, so
      # long as it's empty.
      rv = CreateDirectory(to_path, 0)
      if rv == 0 && rv != ERROR_ALREADY_EXISTS
         raise StandardError, 'CreateDirectory() failed: ' + get_last_error
      end
      
      handle = CreateFile(
         to_path,
         GENERIC_READ | GENERIC_WRITE, 
         0,
         0,
         OPEN_EXISTING, 
         FILE_FLAG_OPEN_REPARSE_POINT | FILE_FLAG_BACKUP_SEMANTICS,
         0
      )
      
      if handle == INVALID_HANDLE_VALUE
         raise StandardError, 'CreateFile() failed: ' + get_last_error
      end 
      
      buf_target  = buf_target.split(0.chr).first
      buf_target  = "\\??\\" << from_path     
      length      = buf_target.size * 2 # sizeof(WCHAR)
      wide_string = multi_to_wide(buf_target)
      
      # REPARSE_JDATA_BUFFER
      rdb = [
         "0xA0000003L".hex,      # ReparseTag (IO_REPARSE_TAG_MOUNT_POINT)
         wide_string.size + 12,  # ReparseDataLength
         0,                      # Reserved
         0,                      # SubstituteNameOffset
         wide_string.size,       # SubstituteNameLength
         wide_string.size + 2,   # PrintNameOffset
         0,                      # PrintNameLength
         wide_string             # PathBuffer
      ].pack('LSSSSSSa' + (wide_string.size + 4).to_s)

      bytes = [0].pack('L')

      bool = DeviceIoControl(
         handle,
         CTL_CODE(FILE_DEVICE_FILE_SYSTEM, 41, METHOD_BUFFERED, FILE_ANY_ACCESS),
         rdb,
         rdb.size,
         0,
         0,
         bytes,
         0
      )
      
      unless bool
         error = 'DeviceIoControl() failed: ' + get_last_error
         RemoveDirectory(to)
         CloseHandle(handle)        
         raise error
      end
      
      CloseHandle(handle)
      
      self  
   end
   
   # Returns whether or not +path+ is empty.  Returns false if +path+ is not
   # a directory, or contains any files other than '.' or '..'.
   # 
   def self.empty?(path)
      PathIsDirectoryEmpty(path)
   end
   
   # Returns whether or not +path+ is a junction.
   # 
   def self.junction?(path)
      bool   = true
      attrib = GetFileAttributes(path)
      
      bool = false if attrib == INVALID_FILE_ATTRIBUTES
      bool = false if attrib & FILE_ATTRIBUTE_DIRECTORY == 0
      bool = false if attrib & FILE_ATTRIBUTE_REPARSE_POINT == 0
      
      bool
   end 
   
   # Class level aliases
   #
   class << self
      alias reparse_dir? junction?
   end
end