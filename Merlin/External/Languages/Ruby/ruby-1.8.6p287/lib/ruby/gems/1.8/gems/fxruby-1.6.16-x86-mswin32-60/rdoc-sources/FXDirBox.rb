module Fox
  #
  # A Directory Box widget allows the user to select parts of a file path.
  # First, it is filled with a string comprising a file path, like "/a/b/c".
  # Then, the user can select "/a/b/c", "/a/b", "/a", and "/" from the drop-down
  # list.  The entries in the drop-down list are automatically provided with icons
  # by consulting the file-associations registry settings.
  # The Directory Box sends <tt>SEL_CHANGED</tt> and <tt>SEL_COMMAND</tt> messages, with the string
  # containing the full path to the selected item.
  #
  # === Options
  #
  # +DIRBOX_NO_OWN_ASSOC+::	do not create associations for files
  #
  # === Events
  #
  # The following messages are sent by FXDirBox to its target:
  #
  # +SEL_CHANGED+::     sent when the current item changes; the message data is the new current directory.
  # +SEL_COMMAND+::     sent when the current item changes; the message data is the new current directory.
  #
  class FXDirBox < FXTreeListBox

    # Current directory [String]
    attr_accessor :directory

    # File associations [FXFileDict]
    attr_accessor :associations

    # Return an initialized FXDirBox instance.
    def initialize(p, target=nil, selector=0, opts=FRAME_SUNKEN|FRAME_THICK|TREELISTBOX_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theDirBox
    end

    #
    # Set current directory
    #
    def setDirectory(pathname); end
    
    #
    # Return current directory
    #
    def getDirectory(); end
    
    #
    # Change file associations, where _assoc_ is an FXFileDict instance.
    #
    def setAssociations(assoc); end
    
    #
    # Return file associations (an FXFileDict instance).
    #
    def getAssociations(); end
  end
end

