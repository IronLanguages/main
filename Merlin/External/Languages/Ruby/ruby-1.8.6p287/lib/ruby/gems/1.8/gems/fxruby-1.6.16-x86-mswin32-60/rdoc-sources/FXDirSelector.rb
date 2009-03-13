module Fox
  #
  # The FXDirSelector widget is the reusable mega-widget component which
  # is the core of the FXDirDialog.  The function of the directory selector widget
  # is very similar to the file selector widget, except that the directory selector widget
  # displays a tree-structured view of the file system, and thereby makes up and down
  # navigation through the file system significantly easier.
  #
  # === Message identifiers
  #
  # +ID_DIRNAME+::	x
  # +ID_DIRLIST+::	x
  # +ID_HOME+::		x
  # +ID_WORK+::		x
  # +ID_DIRECTORY_UP+::	x
  # +ID_BOOKMARK+::	x
  # +ID_VISIT+::	x
  # +ID_NEW+::		x
  # +ID_DELETE+::	x
  # +ID_MOVE+::		x
  # +ID_COPY+::		x
  # +ID_LINK+::		x
  #
  class FXDirSelector < FXPacker
  
    # The "Accept" button [FXButton]
    attr_reader :acceptButton
    
    # The "Cancel" button [FXButton]
    attr_reader :cancelButton
    
    # Directory [String]
    attr_accessor :directory
    
    # Wildcard matching mode, some combination of file matching flags [Integer]
    attr_accessor :matchMode
    
    # Directory list style [Integer]
    attr_accessor :dirBoxStyle

    # Return an initialized FXDirSelector instance
    def initialize(p, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0) # :yields: theDirSelector
    end

    # Return +true+ if showing files as well as directories
    def filesShown?; end
    
    #
    # If _state_ is +true+, the directory selector will show files as well as
    # directories; otherwise, it will only show directories.
    #
    def filesShown=(state); end
  
    # Return +true+ if showing hidden files and directories
    def hiddenFilesShown?; end
    
    #
    # If _state_ is +true+, the directory selector will show hidden files and
    # directories; otherwise, it won't.
    #
    def hiddenFilesShown=(state); end
  end
end

