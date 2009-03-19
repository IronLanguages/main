module Fox
  #
  # File item
  #
  class FXFileItem < FXIconItem
  
    # The file association object for this item [FXFileAssoc]
    attr_reader :assoc
    
    # The file size for this item [Integer]
    attr_reader :size
    
    # Date for this item [Time]
    attr_reader :date

    # Returns an initialized FXFileItem instance
    def initialize(text, bi=nil, mi=nil, ptr=nil) # :yields: theFileItem
    end
  
    # Return +true+ if this is a file item
    def file?; end
  
    # Return +true+ if this is a directory item
    def directory?; end
  
    # Return +true+ if this is a share item
    def share?; end

    # Return +true+ if this is an executable item
    def executable?; end
  
    # Return +true+ if this is a symbolic link item
    def symlink?; end
  
    # Return +true+ if this is a character device item
    def chardev?; end
  
    # Return +true+ if this is a block device item
    def blockdev?; end
  
    # Return +true+ if this is an FIFO item
    def fifo?; end
  
    # Return +true+ if this is a socket
    def socket?; end
  end

  #
  # A File List widget provides an icon rich view of the file system.
  # It automatically updates itself periodically by re-scanning the file system
  # for any changes.  As it scans the displayed directory, it automatically
  # determines the icons to be displayed by consulting the file associations registry
  # settings.  A number of messages can be sent to the File List to control the
  # filter pattern, sort category, sorting order, case sensitivity, and hidden file
  # display mode.
  # The File list widget supports drags and drops of files.
  #
  # === File List options
  #
  # +FILELIST_SHOWHIDDEN+::	Show hidden files or directories
  # +FILELIST_SHOWDIRS+::	Show only directories
  # +FILELIST_SHOWFILES+::	Show only files
  # +FILELIST_SHOWIMAGES+::	Show preview of images
  # +FILELIST_NO_OWN_ASSOC+::	Do not create associations for files
  # +FILELIST_NO_PARENT+::	Suppress display of '.' and '..'
  #
  # === Message identifiers
  #
  # +ID_SORT_BY_NAME+::		Sort by name
  # +ID_SORT_BY_TYPE+::		Sort by type
  # +ID_SORT_BY_SIZE+::		Sort by size
  # +ID_SORT_BY_TIME+::		Sort by access time
  # +ID_SORT_BY_USER+::		Sort by user name
  # +ID_SORT_BY_GROUP+::	Sort by group name
  # +ID_SORT_REVERSE+::		Reverse sort order
  # +ID_DIRECTORY_UP+::		Move up one directory
  # +ID_SET_PATTERN+::		Set match pattern
  # +ID_SET_DIRECTORY+::	Set directory
  # +ID_SHOW_HIDDEN+::		Show hidden files
  # +ID_HIDE_HIDDEN+::		Hide hidden files
  # +ID_TOGGLE_HIDDEN+::	Toggle visibility of hidden files
  # +ID_REFRESHTIMER+:: 	x
  # +ID_OPENTIMER+:: 		x
  # +ID_TOGGLE_IMAGES+::	Toggle display of images
  # +ID_REFRESH+::		Refresh immediately
  #
  class FXFileList < FXIconList
  
    # Current file [String]
    attr_accessor :currentFile
    
    # Current directory [String]
    attr_accessor :directory
    
    # Wildcard matching pattern [String]
    attr_accessor :pattern
    
    # Wildcard matching mode [Integer]
    attr_accessor :matchMode
    
    # File associations [FXFileDict]
    attr_accessor :associations
    
    # Image size for preview images [Integer]
    attr_accessor :imageSize

    # Construct a file list
    def initialize(p, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0) # :yields: theFileList
    end
    
    #
    # Set the current file to _filename_.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the
    # file list's message target after the current item has changed.
    # If this change causes the selected item(s) to change (because
    # the file list is operating in browse-select mode), +SEL_SELECTED+ and
    # +SEL_DESELECTED+ may be sent to the message target as well.
    #
    def setCurrentFile(filename, notify=false); end
  
    #
    # Scan the current directory and update the items if needed, or if _force_ is +true+.
    #
    def scan(force=true); end

    #
    # Return +true+ if item is a directory.
    # Raises IndexError if _index_ is out of bounds.
    #
    def itemDirectory?(index); end
  
    #
    # Return +true+ if item is a share.
    # Raises IndexError if _index_ is out of bounds.
    #
    def itemShare?(index); end

    #
    # Return +true+ if item is a file.
    # Raises IndexError if _index_ is out of bounds.
    #
    def itemFile?(index); end
  
    #
    # Return +true+ if item is executable.
    # Raises IndexError if _index_ is out of bounds.
    #
    def itemExecutable?(index); end
  
    #
    # Return name of item at index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def itemFilename(index); end
  
    #
    # Return full pathname of item at index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def itemPathname(index); end
    
    #
    # Return file association of item at index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def itemAssoc(index); end
  
    # Return +true+ if showing hidden files.
    def hiddenFilesShown?; end
    
    # Show or hide hidden files.
    def hiddenFilesShown=(shown); end
    
    # Return +true+ if showing directories only.
    def onlyDirectoriesShown?; end
    
    # Show directories only.
    def onlyDirectoriesShown=(shown); end

    # Return +true+ if showing files only.
    def onlyFilesShown?; end
    
    # Show files only.
    def onlyFilesShown=(shown); end

    #
    # If _shown_ is +true+, the file list will show preview images;
    # otherwise it won't.
    #
    def imagesShown=(shown); end
    
    #
    # Return +true+ if the file list is showing preview images.
    #
    def imagesShown? ; end
    
    #
    # Return +true+ if parent directories are shown.
    #
    def parentDirsShown? ; end
    
    #
    # Set whether parent directories are shown to +true+ or +false+.
    #
    def parentDirsShown=(shown); end
  end
end

