module Fox
  #
  # An icon source is an object that loads an icon of any type.
  # It exists purely for convenience, to make loading icons
  # simpler by concentrating the knowledge of the supported
  # icon formats in a single place.
  # Needless to say, this class is subclassable, allowing users
  # to add additional icon types and make them available to
  # all widgets which deal with icons.
  # Note, the icons are loaded, but _not_ created (realized) yet;
  # this allows users to manipulate the pixel data prior to
  # realizing the icons.
  #
  class FXIconSource < FXObject
    #
    # Construct an icon source, given a reference to the application object.
    #
    def initialize(app) # :yields: theIconSource
    end
    
    #
    # Load an icon from the file _filename_. By default, the file extension is
    # stripped and used as the icon _type_; if an explicit icon type is forced,
    # then that type is used and the extension is ignored.
    # For example, loadIcon("icon", "gif") will try to load a CompuServe GIF
    # file, since the filename does not give any clue as to the type of the
    # icon.
    # Returns a reference to the icon.
    #
    def loadIconFile(filename, type=nil); end

    #
    # Load an icon of a given type (e.g. "gif") from reswrapped data.
    # Returns +nil+ if there's some error loading the icon. (The optional
    # _type_ parameter is actually mandatory at the time of this writing; future
    # versions will attempt to inspect the first few bytes of the stream
    # to divine the icon format if the parameter is omitted).
    # Returns a reference to the icon.
    #
    def loadIconData(pixels, type=nil); end

    #
    # Load an icon of a given type (e.g. "gif") from an already open stream.
    # Returns +nil+ if there's some error loading the icon. (The optional
    # _type_ parameter is actually mandatory at the time of this writing; future
    # versions will attempt to inspect the first few bytes of the stream
    # to divine the icon format if the parameter is omitted).
    # Returns a reference to the icon.
    #
    def loadIconStream(store, type=nil); end

    #
    # Load an image from the file filename. By default, the file extension is
    # stripped and used as the image type; if an explicit image type is forced,
    # then that type is used and the extension is ignored.
    # For example, loadImage("image","gif") will try to load a CompuServe GIF
    # file, since the filename does not give any clue as to the type of the
    # image.
    #
    def loadImageFile(filename, type=nil); end

    #
    # Load an image of a given type (e.g. "gif") from reswrapped data.
    # Returns +nil+ if there's some error loading the icon. (The optional
    # parameter is actually mandatory at the time of this writing; future
    # versions will attempt to inspect the first few bytes of the stream
    # to divine the icon format if the parameter is omitted).
    #
    def loadImageData(pixels, type=nil); end
  
    #
    # Load an image of a given type (e.g. "gif") from an already open stream.
    # Returns +nil+ if there's some error loading the image. (The optional
    # parameter is actually mandatory at the time of this writing; future
    # versions will attempt to inspect the first few bytes of the stream
    # to divine the image format if the parameter is omitted).
    #
    def loadImageStream(store, type=nil); end

    # Load icon and scale it such that its dimensions do not exceed given size
    def loadScaledIconFile(filename, size=32, qual=0, type=nil); end

    # Load icon and scale it such that its dimensions do not exceed given size
    def loadScaledIconData(pixels, size=32, qual=0, type=nil); end

    # Load icon and scale it such that its dimensions do not exceed given size
    def loadScaledIconStream(store, size=32, qual=0, type=nil); end

    # Load image and scale it such that its dimensions do not exceed given size
    def loadScaledImageFile(filename, size=32, qual=0, type=nil); end

    # Load image and scale it such that its dimensions do not exceed given size
    def loadScaledImageData(pixels, size=32, qual=0, type=nil); end

    # Load image and scale it such that its dimensions do not exceed given size
    def loadScaledImageStream(store, size=32, qual=0, type=nil); end
  end
end


