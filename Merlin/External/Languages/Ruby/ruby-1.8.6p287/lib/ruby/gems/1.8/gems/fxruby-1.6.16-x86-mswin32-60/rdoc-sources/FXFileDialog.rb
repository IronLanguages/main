module Fox
  #
  # File selection dialog
  #
  # Each pattern in the _patternList_ comprises an optional name, followed by
  # a pattern in parentheses. The patterns are separated by newlines.
  # For example,
  #
  #  fileDialog.patternList = ["*",
  #                            "*.cpp,*.cc",
  #                            "*.hpp,*.hh,*.h"
  #                           ]
  #
  # and
  #
  #  fileDialog.patternList = ["All Files (*)",
  #                            "C++ Sources (*.cpp,*.cc)",
  #                            "C++ Headers (*.hpp,*.hh,*.h)"
  #                           ]
  #
  # will set the same three patterns, but the former shows no pattern names.
  #
  # === Wildcard matching modes
  #
  # +FILEMATCH_FILE_NAME+::		No wildcard can ever match "/" (or "\","/" under Windows).
  # +FILEMATCH_NOESCAPE+::		Backslashes don't quote special chars ("\" is treated as "\").
  # +FILEMATCH_PERIOD+::		Leading "." is matched only explicitly (Useful to match hidden files on Unix).
  # +FILEMATCH_LEADING_DIR+::	Ignore "/..." after a match.
  # +FILEMATCH_CASEFOLD+::		Compare without regard to case.
  #
  # Note that under Windows, +FILEMATCH_NOESCAPE+ must be passed.
  #
  class FXFileDialog < FXDialogBox

    # File name [String]
    attr_accessor :filename
    
    # List of selected filenames [Array]
    attr_reader :filenames
    
    # File pattern [String]
    attr_accessor :pattern
    
    # Current pattern number [Integer]
    attr_accessor :currentPattern
    
    # Directory [String]
    attr_accessor :directory
    
    # Inter-item spacing (in pixels) [Integer]
    attr_accessor :itemSpace
    
    # File list style [Integer]
    attr_accessor :fileBoxStyle
    
    # File selection mode [Integer]
    attr_accessor :selectMode
    
    # Wildcard matching mode [Integer]
    attr_accessor :matchMode
    
    # Image size for preview images [Integer]
    attr_accessor :imageSize

    # Construct a file dialog box
    def initialize(owner, name, opts=0, x=0, y=0, width=500, height=300) # :yields: theFileDialog
    end

    #
    # Change the list of file patterns shown in the file dialog.
    # The _patterns_ argument is an array of strings, and each string
    # represents a different file pattern. A pattern consists of an
    # optional name, followed by a pattern in parentheses. For example,
    # this code:
    #
    #   patterns = [ "*", "*.cpp,*.cc", "*.hpp,*.hh,*.h" ]
    #   aFileSelector.setPatternList(patterns)
    #
    # and this code:
    #
    #   patterns = [ "All Files (*)", "C++ Sources (*.cpp,*.cc)", "C++ Headers (*.hpp,*.hh,*.h)" ]
    #   aFileSelector.setPatternList(patterns)
    #
    # will both set the same three patterns, but the former shows no pattern names.
    #
    # For compatibility with the FOX C++ library API of the same name, #setPatternList
    # also accepts a _patterns_ value that is a single string, with each pattern
    # separated by newline characters, e.g.
    #
    #   patterns = "All Files (*)\nC++ Sources (*.cpp,*.cc)\nC++ Headers (*.hpp,*.hh,*.h)"
    #   aFileSelector.setPatternList(patterns)
    #
    def setPatternList(patterns); end

    #
    # Returns the list of patterns (an Array of Strings).
    #
    def getPatternList(); end

    # Get pattern text for given pattern number
    def getPatternText(patno); end

    # Change pattern text for pattern number
    def setPatternText(patno, text); end
  
    #
    # Return number of patterns
    #
    def numPatterns; end

    #
    # Change whether this file dialog allows pattern entry or not.
    #
    def allowsPatternEntry=(allowed); end
    
    #
    # Return +true+ if this file dialog allows pattern entry
    #
    def allowsPatternEntry? ; end

    # Set visibility of the read-only button, where _shown_ is either +true+ or +false+
    def showReadOnly=(shown); end
    
    # Return +true+ if read-only button is shown
    def readOnlyShown?; end
    
    # Return +true+ if showing hidden files and directories
    def hiddenFilesShown?; end
    
    #
    # If _state_ is +true+, the file dialog will show hidden files and
    # directories; otherwise, it won't.
    #
    def hiddenFilesShown=(state); end

    #
    # If _shown_ is +true+, the file dialog will show preview images;
    # otherwise it won't.
    #
    def imagesShown=(shown); end
    
    #
    # Return +true+ if the file dialog is showing preview images.
    #
    def imagesShown? ; end
    
    # Set initial state of read-only button, where _state_ is either +true+ or +false+
    def readOnly=(state); end
  
    # Return +true+ if read-only
    def readOnly?; end
  
    # Return +true+ if navigation allowed.
    def navigationAllowed?; end

    # Set navigation to allowed (+true+) or disallowed (+false+)
    def navigationAllowed=(allowed); end

    #
    # Display a dialog box that allows the user to select a single existing file name
    # for opening.
    # Returns the selected file name (a String).
    #
    # ==== Parameters:
    #
    # +owner+::		the owner window for the dialog box [FXWindow]
    # +caption+::	the caption for the dialog box [String]
    # +path+::		the initial filename [String]
    # +patterns+::	the pattern list [String]
    # +initial+::	the initial pattern to be used (an index into the pattern list) [Integer]
    #
    def FXFileDialog.getOpenFilename(owner, caption, path, patterns="*", initial=0); end
  
    #
    # Display a dialog box that allows the user to select multiple existing file names
    # for opening.
    # Returns an array of the selected file names (an array of strings).
    #
    # ==== Parameters:
    #
    # +owner+::		the owner window for the dialog box [FXWindow]
    # +caption+::	the caption for the dialog box [String]
    # +path+::		the initial filename [String]
    # +patterns+::	the pattern list [String]
    # +initial+::	the initial pattern to be used (an index into the pattern list) [Integer]
    #
    def FXFileDialog.getOpenFilenames(owner, caption, path, patterns="*", initial=0); end
  
    #
    # Display a dialog box that allows the user to select an existing file name, or
    # enter a new file name, for saving.
    # Returns the save file name (a String).
    #
    # ==== Parameters:
    #
    # +owner+::		the owner window for the dialog box [FXWindow]
    # +caption+::	the caption for the dialog box [String]
    # +path+::		the initial filename [String]
    # +patterns+::	the pattern list [String]
    # +initial+::	the initial pattern to be used (an index into the pattern list) [Integer]
    #
    def FXFileDialog.getSaveFilename(owner, caption, path, patterns="*", initial=0); end
  
    #
    # Display a dialog box that allows the user to select a directory.
    # Returns the directory name (a String).
    #
    # ==== Parameters:
    #
    # +owner+::		the owner window for the dialog box [FXWindow]
    # +caption+::	the caption for the dialog box [String]
    # +path+::		the initial directory path [String]
    #
    def FXFileDialog.getOpenDirectory(owner, caption, path); end
  end
end

