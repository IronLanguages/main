module Fox
  #
  # Directory selection dialog
  #
  class FXDirDialog < FXDialogBox
  
    # Directory [String]
    attr_accessor :directory
    
    # Wildcard matching mode, some combination of file matching flags [Integer]
    attr_accessor :matchMode
    
    # Directory list style [Integer]
    attr_accessor :dirBoxStyle
    
    # Returns an initialized FXDirDialog instance.
    def initialize(owner, name, opts=0, x=0, y=0, width=500, height=300) # :yields: theDirDialog
    end

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

    #
    # Display a directory dialog with the specified owner window, caption
    # string and initial path string.
    # Return the selected directory name (a string) or +nil+ if the dialog
    # was cancelled.
    #
    def FXDirDialog.getOpenDirectory(owner, caption, path); end
  end
end

