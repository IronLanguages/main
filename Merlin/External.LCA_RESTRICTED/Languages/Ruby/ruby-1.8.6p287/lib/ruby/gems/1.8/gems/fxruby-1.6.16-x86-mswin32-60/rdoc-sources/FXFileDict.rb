module Fox
  #
  # Registers stuff to know about the extension
  #
  class FXFileAssoc
    # Command to execute [String]
    attr_accessor :command
    
    # Full extension name [String]
    attr_accessor :extension
    
    # Mime type name [String]
    attr_accessor :mimetype
    
    # Big normal icon [FXIcon]
    attr_accessor :bigicon
    
    # Big open icon [FXIcon]
    attr_accessor :bigiconopen
    
    # Mini normal icon [FXIcon]
    attr_accessor :miniicon
    
    # Mini open icon [FXIcon]
    attr_accessor :miniiconopen
    
    # Registered drag type [FXDragType]
    attr_accessor :dragtype
    
    # Flags [Integer]
    attr_accessor :flags
    
    # Returns an initialized FXFileAssoc instance
    def initialize; end
  end

  #
  # The File Association dictionary associates a file extension with a File
  # Association record which contains command name, mime type, icons, and other
  # information about the file type.  The icons referenced by the file association
  # are managed by the Icon Dictionary; this guarantees that each icon is loaded
  # only once into memory.
  # The associations are determined by the information by the FOX Registry settings;
  # each entry under the FILETYPES registry section comprises the command line,
  # extension name, large icon, small icon, and mime type:
  #
  #   command ';' extension ';' bigicon [ ':' bigiconopen ] ';' icon [ ':' iconopen ] ';' mime
  #
  # For example, the binding for "jpg" could be:
  #
  #   xv %s &;JPEG Image;bigimage.xpm;miniimage.xpm;image/jpeg
  #
  # The association for a file name is determined by first looking at the entire
  # file name, then at the whole extension, and then at sub-extensions.
  # For example, "name.tar.gz", "tar.gz", and "gz" can each be given a different
  # file association.  Directory names may also be given associations; there is
  # no command-line association for a directory, however.  The association for a
  # directory is found by first checking the whole pathname, then checking the
  # pathname less the first component, and so on.  So, "/usr/local/include",
  # "/local/include", and "/include" can each be given their own file associations.
  # If the above lookup procedure has not found a file association, the system
  # uses a fallback associations: for files, the fallback association is determined
  # by the binding "defaultfilebinding".  For directories, the "defaultdirbinding"
  # is used, and for executables the "defaultexecbinding" is used.
  #
  class FXFileDict < FXDict
  
    # Settings database [FXSettings]
    attr_accessor :settings
  
    # Current icon search path [FXIconDict]
    attr_accessor :iconDict

    # Current icon search path [String]
    attr_accessor :iconPath
    
    # Return the registry key used to find fallback executable icons.
    def FXFileDict.defaultExecBinding(); end
  
    # Return the registry key used to find fallback directory icons.
    def FXFileDict.defaultDirBinding(); end

    # Return the registry key used to find fallback document icons.
    def FXFileDict.defaultFileBinding(); end
  
    #
    # Construct a dictionary that maps file extensions to file associations.
    # If _db_ is not +nil+, the specified settings database is used as a
    # source for the bindings.
    # Otherwise, the application registry settings are used.
    #
    # ==== Parameters:
    #
    # +app+:	Application [FXApp]
    # +db+::	Settings database [FXSettings]
    #
    def initialize(app, db=nil); end
  
    #
    # Replace file association for the specified extension;
    # returns a reference to the file association.
    #
    # ==== Parameters:
    #
    # +ext+::	Extension [String]
    # +str+::	String [String]
    #
    def replace(ext, str); end
  
    #
    # Remove file association for the specified extension
    # and return a reference to it.
    #
    def remove(ext); end
  
    #
    # Find file association from registry for the specified key.
    #
    def find(ext); end

    # Returns a reference to the FXFileAssoc instance...
    def findFileBinding(pathname); end

    # Returns a reference to the FXFileAssoc instance...
    def findDirBinding(pathname); end

    # Returns a reference to the FXFileAssoc instance...
    def findExecBinding(pathname); end
  end
end

