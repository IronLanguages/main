module Fox
  #
  # The Icon Dictionary manages a collection of icons.  The icons are referenced
  # by their file name.  When first encountering a new file name, the icon is
  # located by searching the icon search path for the icon file.  If found, the
  # services of the icon source object are used to load the icon from the file.
  # A custom icon source may be installed to furnish support for additonal 
  # image file formats.
  # Once the icon is loaded, an association between the icon name and the icon
  # is entered into the icon dictionary.  Subsequent searches for an icon with
  # this name will be satisfied from the cached value.
  # The lifetype of the icons is managed by the icon dictionary, and thus all
  # icons will be deleted when the dictionary is deleted.
  #
  class FXIconDict < FXDict

    # Return the default icon search path (as a string)
    def FXIconDict.defaultIconPath; end

    #
    # Construct icon dictionary, and set initial search path; also
    # creates a default icon source object.
    #
    def initialize(app, path=FXIconDict.defaultIconPath);

    # Change icon source to _src_ (an FXIconSource instance).
    def iconSource=(src); end

    # Return icon source
    def iconSource; end

    # Set icon search path
    def iconPath=(path); end

    # Return current icon search path
    def iconPath; end

    # Insert unique icon loaded from _filename_ into dictionary, and return a reference to the icon (an FXIcon instance).
    def insert(filename); end

    # Remove icon from dictionary; returns a reference to the icon.
    def remove(name); end

    # Find icon by name and return a reference to it.
    def find(name); end
  end
end

