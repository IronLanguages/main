module Fox
  #
  # File selection widget
  #
  # === File selection modes
  #
  # +SELECTFILE_ANY+::		A single file, existing or not (to save to)
  # +SELECTFILE_EXISTING+::	An existing file (to load)
  # +SELECTFILE_MULTIPLE+::	Multiple existing files
  # +SELECTFILE_MULTIPLE_ALL+::	Multiple existing files or directories, but not '.' and '..'
  # +SELECTFILE_DIRECTORY+::	Existing directory, including '.' or '..'
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
  # === Message identifiers
  #
  # +ID_FILEFILTER+::		x
  # +ID_ACCEPT+::		x
  # +ID_FILELIST+::		x
  # +ID_DIRECTORY_UP+::		x
  # +ID_DIRTREE+::		x
  # +ID_NORMAL_SIZE+::		x
  # +ID_MEDIUM_SIZE+::		x
  # +ID_GIANT_SIZE+::		x
  # +ID_HOME+::			x
  # +ID_WORK+::			x
  # +ID_BOOKMARK+::		x
  # +ID_BOOKMENU+::		x
  # +ID_VISIT+::		x
  # +ID_NEW+::			x
  # +ID_DELETE+::		x
  # +ID_MOVE+::			x
  # +ID_COPY+::			x
  # +ID_LINK+::			x
  #
  class FXFileSelector < FXPacker
  
    # The "Accept" button [FXButton]
    attr_reader :acceptButton
    
    # The "Cancel" button [FXButton]
    attr_reader :cancelButton

    # File name [String]
    attr_accessor :filename
    
    # File pattern [String]
    attr_accessor :pattern
    
    # Directory [String]
    attr_accessor :directory
    
    # Current pattern number [Integer]
    attr_accessor :currentPattern

    # Inter-item spacing (in pixels) [Integer]
    attr_accessor :itemSpace
  
    # Change file list style [Integer]
    attr_accessor :fileBoxStyle
  
    # Change file selection mode [Integer]
    attr_accessor :selectMode
    
    # Wildcard matching mode [Integer]
    attr_accessor :matchMode
  
    # Image size for preview images [Integer]
    attr_accessor :imageSize

    #
    # Return an initialized FXFileSelector instance.
    #
    def initialize(p, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0) # :yields: theFileSelector
    end
  
    #
    # Returns an array of the selected file names.
    #
    def filenames; end
  
    #
    # Change the list of file patterns shown in the file selector.
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
  
    # Return number of patterns.
    def numPatterns; end

    # Show read-only button.
    def readOnlyShown=(shown); end
    
    # Return +true+ if the read-only button is shown.
    def readOnlyShown?; end
    
    # Set state of read-only button.
    def readOnly=(state); end
  
    # Return +true+ if in read-only mode.
    def readOnly?; end

    # Return +true+ if showing hidden files and directories
    def hiddenFilesShown?; end
    
    #
    # If _state_ is +true+, the file selector will show hidden files and
    # directories; otherwise, it won't.
    #
    def hiddenFilesShown=(state); end

    #
    # If _shown_ is +true+, the file selector will show preview images;
    # otherwise it won't.
    #
    def imagesShown=(shown); end
    
    #
    # Return +true+ if the file selector is showing preview images.
    #
    def imagesShown? ; end

    # Return +true+ if navigation allowed.
    def navigationAllowed?; end

    # Set navigation to allowed (+true+) or disallowed (+false+)
    def navigationAllowed=(allowed); end

    #
    # Given filename pattern of the form "GIF Format (*.gif)",
    # returns the pattern only, i.e. "*.gif" in this case.
    # If the parentheses are not found then returns the entire
    # input pattern.
    #
    def FXFileSelector.patternFromText(pattern) ; end
    
    #
    # Given a pattern of the form "*.gif,*.GIF", return
    # the first extension of the pattern, i.e. "gif" in this
    # example. Returns empty string if it doesn't work out.
    #
    def FXFileSelector.extensionFromPattern(pattern) ; end
  end
end

