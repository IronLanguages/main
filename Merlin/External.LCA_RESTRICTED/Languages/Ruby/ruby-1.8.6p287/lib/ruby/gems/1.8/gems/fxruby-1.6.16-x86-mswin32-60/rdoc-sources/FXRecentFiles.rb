module Fox
  #
  # The recent files object manages a most recently used file list by
  # means of the standard system registry.
  # When connected to a widget, like a menu command, the recent files object
  # updates the menu commands label to the associated recent file name; when
  # the menu command is invoked, the recent file object sends its target a
  # +SEL_COMMAND+ message with the message data set to the associated file name
  # (a String).
  # When adding or removing file names, the recent files object automatically
  # updates the system registry to record these changes.
  #
  # === Events
  #
  # The following messages are sent by FXRecentFiles to its target:
  #
  # +SEL_COMMAND+::
  #   sent when one of the recent files in this list is selected,
  #   usually as a result of being selected from a menu command.
  #   The message data is a String containing the name of the file.
  #
  # === Message identifiers
  #
  # <tt>ID_CLEAR</tt>::		Clear the list of files
  # <tt>ID_ANYFILES</tt>::	x
  # <tt>ID_FILE_1</tt>::	x
  # <tt>ID_FILE_2</tt>::	x
  # <tt>ID_FILE_3</tt>::	x
  # <tt>ID_FILE_4</tt>::	x
  # <tt>ID_FILE_5</tt>::	x
  # <tt>ID_FILE_6</tt>::	x
  # <tt>ID_FILE_7</tt>::	x
  # <tt>ID_FILE_8</tt>::	x
  # <tt>ID_FILE_9</tt>::	x
  # <tt>ID_FILE_10</tt>::	x
  #
  class FXRecentFiles < FXObject
    # Application associated with this recent files group [FXApp]
    attr_reader :app

    # Maximum number of files to track [Integer]
    attr_accessor :maxFiles
    
    # Group name [String]
    attr_accessor :groupName
    
    # Message target [FXObject]
    attr_accessor :target
    
    # Message identifier [Integer]
    attr_accessor :selector
    
    #
    # Construct a new FXRecentFiles group, using the global application instance.
    #
    def initialize # :yields: theRecentFiles
    end
  
    # Make new recent files group with default groupname
    def initialize(a) # :yields: theRecentFiles
    end
    
    # Make new recent files group with groupname gp
    def initialize(a, gp, target=nil, selector=0) # :yields: theRecentFiles
    end

    # Append a file to the end of the list.
    def appendFile(filename); end
  
    # Remove a file from the list.
    def removeFile(filename); end
  
    # Clear the list of files.
    def clear(); end
  end
end

