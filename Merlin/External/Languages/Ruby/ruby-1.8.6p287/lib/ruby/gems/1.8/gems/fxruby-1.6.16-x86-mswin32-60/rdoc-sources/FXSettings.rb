module Fox
  #
  # The Settings class manages a key-value database.  This is normally used as
  # part of Registry, but can also be used separately in applications that need
  # to maintain a key-value database in a file of their own.
  # String values can contain any character, and will be escaped when written
  # to the file.
  #
  class FXSettings < FXDict
    #
    # Return an initialized FXSettings instance.
    #
    def initialize # :yields: theSettings
    end

    #
    # Parse a file containing a settings database.
    # Returns true on success, false otherwise.
    #
    def parseFile(filename, mark); end

    #
    # Unparse settings database into given file.
    # Returns true on success, false otherwise.
    #
    def unparseFile(filename) ; end

    #
    # Obtain the string dictionary (an FXStringDict instance) for the requested section number.
    #
    # ==== Parameters:
    #
    # +pos+::	the section number of interest [Integer]
    #
    def data(pos) ; end

    #
    # Find a section given its name.
    # Returns the section (an FXStringDict instance) if found,
    # otherwise returns nil.
    #
    # ==== Parameters:
    #
    # +section+::	the section name of interest [String]
    #
    def find(section) ; end

    #
    # Iterate over sections (where each section is a dictionary).
    #
    def each_section # :yields: aStringDict
    end
    
    #
    # Read a string registry entry from the specified _section_ and _key_.
    # If no value is found, the _default_ value is returned.
    #
    # ==== Parameters:
    #
    # +section+::	the section name [String]
    # +key+::		the key for the setting of interest [String]
    # +default+::	the default value to return if _key_ is not found [String]
    #
    def readStringEntry(section, key, default="") ; end

    #
    # Read an integer registry entry from the specified _section_ and _key_.
    # If no value is found, the _default_ value is returned.
    #
    # ==== Parameters:
    #
    # +section+::	the section name [String]
    # +key+::		the key for the setting of interest [String]
    # +default+::	the default value to return if _key_ is not found [Integer]
    #
    def readIntEntry(section, key, default=0) ; end

    #
    # Read an unsigned integer registry entry from the specified _section_ and _key_.
    # If no value is found, the _default_ value is returned.
    #
    # ==== Parameters:
    #
    # +section+::	the section name [String]
    # +key+::		the key for the setting of interest [String]
    # +default+::	the default value to return if _key_ is not found [Integer]
    #
    def readUnsignedEntry(section, key, default=0) ; end

    #
    # Read a double-precision floating point registry entry from the specified _section_ and _key_.
    # If no value is found, the _default_ value is returned.
    #
    # ==== Parameters:
    #
    # +section+::	the section name [String]
    # +key+::		the key for the setting of interest [String]
    # +default+::	the default value to return if _key_ is not found [Float]
    #
    def readRealEntry(section, key, default=0.0) ; end

    #
    # Read a color value registry entry from the specified _section_ and _key_.
    # If no value is found, the _default_ value is returned.
    #
    # ==== Parameters:
    #
    # +section+::	the section name [String]
    # +key+::		the key for the setting of interest [String]
    # +default+::	the default value to return if _key_ is not found [FXColor]
    #
    def readColorEntry(section, key, default=0) ; end

    #
    # Read a boolean valued registry entry from the specified _section_ and _key_.
    # If no value is found, the _default_ value is returned.
    #
    # ==== Parameters:
    #
    # +section+::	the section name [String]
    # +key+::		the key for the setting of interest [String]
    # +default+::	the default value to return if _key_ is not found [true or false]
    #
    def readBoolEntry(section, key, default=false) ; end

    #
    # Write a string registry _value_ to the specified _section_ and _key_.
    # Returns true on success, false otherwise.
    #
    # ==== Parameters:
    #
    # +section+::	the section name [String]
    # +key+::		the key for this setting [String]
    # +value+::		the value for this setting [String]
    #
    def writeStringEntry(section, key, value) ; end

    #
    # Write an integer registry _value_ to the specified _section_ and _key_.
    # Returns true on success, false otherwise.
    #
    # ==== Parameters:
    #
    # +section+::	the section name [String]
    # +key+::		the key for this setting [String]
    # +value+::		the value for this setting [Integer]
    #
    def writeIntEntry(section, key, value) ; end

    #
    # Write an unsigned integer registry _value_ to the specified _section_ and _key_.
    # Returns true on success, false otherwise.
    #
    # ==== Parameters:
    #
    # +section+::	the section name [String]
    # +key+::		the key for this setting [String]
    # +value+::		the value for this setting [Integer]
    #
    def writeUnsignedEntry(section, key, value) ; end

    #
    # Write a double-precision floating point registry _value_ to the specified _section_ and _key_.
    # Returns true on success, false otherwise.
    #
    # ==== Parameters:
    #
    # +section+::	the section name [String]
    # +key+::		the key for this setting [String]
    # +value+::		the value for this setting [Float]
    #
    def writeRealEntry(section, key, value) ; end

    #
    # Write a color registry _value_ to the specified _section_ and _key_.
    # Returns true on success, false otherwise.
    #
    # ==== Parameters:
    #
    # +section+::	the section name [String]
    # +key+::		the key for this setting [String]
    # +value+::		the value for this setting [FXColor]
    #
    def writeColorEntry(section, key, value) ; end

    #
    # Write a boolean registry _value_ to the specified _section_ and _key_.
    # Returns true on success, false otherwise.
    #
    # ==== Parameters:
    #
    # +section+::	the section name [String]
    # +key+::		the key for this setting [String]
    # +value+::		the value for this setting [true or false]
    #
    def writeBoolEntry(section, key, value) ; end

    #
    # Delete the registry entry for the specified _section_ and _key_.
    # Returns true on success, false otherwise.
    #
    # ==== Parameters:
    #
    # +section+::	the section containing the key to be deleted [String]
    # +key+::		the key to be deleted [String]
    #
    def deleteEntry(section, key) ; end

    #
    # Returns +true+ if a registry entry exists for the specified _section_ and _key_.
    #
    # ==== Parameters:
    #
    # +section+::	the section containing the key of interest [String]
    # +key+::		the key of interest [String]
    #
    def existingEntry?(section, key) ; end

    #
    # Delete an entire section from this settings database.
    # Returns true on success, false otherwise.
    #
    # ==== Parameters:
    #
    # +section+::	the name of the section to be deleted [String]
    #
    def deleteSection(section) ; end

    #
    # Returns +true+ if the named _section_ exists.
    #
    # ==== Parameters:
    #
    # +section+::	the name of the section of interest [String]
    #
    def existingSection?(section) ; end

    #
    # Mark as changed.
    #
    def modified=(mdfy=true) ; end

    #
    # Returns +true+ if this settings object has been modified.
    #
    def modified? ; end
  end
end
