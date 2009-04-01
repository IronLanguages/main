module Fox
  #
  # Directory item
  #
  class FXDirItem < FXTreeItem
    # File associations [FXFileAssoc]
    attr_reader :assoc

    # File size [Integer]
    attr_reader :size
    
    # File time [Integer]
    attr_reader :date

    # Returns an initialized FXDirItem instance
    def initialize(text, oi=nil, ci=nil, data=nil) # :yields: theDirItem
    end
    
    # Return +true+ if this is a directory
    def directory?; end

    # Return +true+ if this is an executable
    def executable?; end
    
    # Return +true+ if this is a symbolic link
    def symlink?; end
    
    # Return +true+ if this is a character device
    def chardev?; end
    
    # Return +true+ if this is a block device
    def blockdev?; end
    
    # Return +true+ if this is a FIFO (a named pipe)
    def fifo?; end
    
    # Return +true+ if this is a socket
    def socket?; end
  end
  
  #
  # An FXDirList widget provides a tree-structured view of the file system.
  # It automatically updates itself periodically by re-scanning the file system
  # for any changes.  As it scans the displayed directories and files, it automatically
  # determines the icons to be displayed by consulting the file-associations registry
  # settings.  A number of messages can be sent to the FXDirList to control the
  # filter pattern, sorting order, case sensitivity, and hidden file display mode.
  # The Directory list widget supports drags and drops of files.
  #
  # === Events
  #
  # +SEL_CLOSED+::
  #   sent when a folder item is closed; the message data is a reference to the FXDirItem that was closed
  # +SEL_OPENED+::
  #   sent when a folder item is opened; the message data is a reference to the FXDirItem that was opened
  #
  # === Directory List options
  #
  # +DIRLIST_SHOWFILES+::	Show files as well as directories
  # +DIRLIST_SHOWHIDDEN+::	Show hidden files or directories
  # +DIRLIST_NO_OWN_ASSOC+::	Do not create associations for files
  #
  # === Message identifiers
  #
  # +ID_REFRESH+::		x
  # +ID_SHOW_FILES+::		x
  # +ID_HIDE_FILES+::		x
  # +ID_TOGGLE_FILES+::		x
  # +ID_SHOW_HIDDEN+::		x
  # +ID_HIDE_HIDDEN+::		x
  # +ID_TOGGLE_HIDDEN+::	x
  # +ID_SET_PATTERN+::		x
  # +ID_SORT_REVERSE+::		x
  #
  class FXDirList < FXTreeList

    # Current file [String]
    attr_accessor :currentFile
    
    # Current directory [String]
    attr_accessor :directory
    
    # Wildcard pattern [String]
    attr_accessor :pattern
    
    # Wildcard matching mode, some combination of file matching flags [Integer]
    attr_accessor :matchMode
    
    # File associations [FXFileDict]
    attr_accessor :associations

    # Returns an initialized FXDirList instance
    def initialize(p, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0) # :yields: theDirList
    end
  
    #
    # Scan the directories and update the items if needed, or if _force_ is +true+.
    #
    def scan(force=true); end

    # Return +true+ if item is a directory
    def itemDirectory?(anItem); end
    
    # Return +true+ if item is a file
    def itemFile?(anItem); end
    
    # Return +true+ if item is executable
    def itemExecutable?(anItem); end
    
    #
    # Set current file.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the list's
    # message target to indicate that the current item has changed.
    #
    def setCurrentFile(file, notify=false); end
  
    #
    # Set current directory.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the list's
    # message target to indicate that the current item has changed.
    #
    def setDirectory(path, notify=false); end

    # Return absolute pathname of item
    def itemPathname(anItem); end
    
    # Return the item from the absolute pathname
    def pathnameItem(path); end

    # Return +true+ if showing files as well as directories
    def filesShown?; end
    
    #
    # If _state_ is +true+, the directory list will show files as well as
    # directories; otherwise, it will only show directories.
    #
    def filesShown=(state); end
  
    # Return +true+ if showing hidden files and directories
    def hiddenFilesShown?; end
    
    #
    # If _state_ is +true+, the directory list will show hidden files and
    # directories; otherwise, it won't.
    #
    def hiddenFilesShown=(state); end
  end
end

