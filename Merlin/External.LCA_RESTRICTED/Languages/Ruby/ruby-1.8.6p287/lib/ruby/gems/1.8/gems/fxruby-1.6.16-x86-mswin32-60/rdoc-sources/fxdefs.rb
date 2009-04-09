module Fox
  # Make an unsigned int out of two unsigned shorts
  def Fox.MKUINT(lo, hi); end
  
  # Return the message type for a selector
  def Fox.FXSELTYPE(sel); end
  
  # Return the message identifier for a selector
  def Fox.FXSELID(sel); end
  
  # Construct an FXColor value from its red, green and blue components
  def Fox.FXRGB(r, g, b); end
  
  # Construct an FXColor value from its red, green, blue and alpha (transparency) components
  def Fox.FXRGBA(r, g, b, a); end
  
  # Return the red value from an FXColor value
  def Fox.FXREDVAL(color); end
  
  # Return the red value from an FXColor value
  def Fox.FXGREENVAL(color); end

  # Return the red value from an FXColor value
  def Fox.FXBLUEVAL(color); end

  # Return the red value from an FXColor value
  def Fox.FXALPHAVAL(color); end

  #
  # Return the specified component value for this FXColor value,
  # where _component_ is either 0, 1, 2 or 3.
  #
  def Fox.FXRGBACOMPVAL(color, component); end

  #
  # Return a "hot key" code value that represents the accelerator key
  # described in the string _str_. The string can contain some combination
  # of the modifiers _Ctrl_, _Alt_ and _Shift_, plus the key of interest.
  # For example, to get the accelerator key for Ctrl+Alt+F7, you'd use:
  #
  #   hotKey = fxparseAccel("Ctrl+Alt+F7")
  #
  def Fox.fxparseAccel(str); end
  
  #
  # Return a hot key value that represents the hot key described in
  # the string _str_. This method is less flexible than the similar
  # Fox.fxparseAccel, and is mainly used internally for parsing the
  # labels for FXButton and FXMenuCommand widgets. For example, this:
  #
  #   fxparseHotKey("&File")
  #
  # returns the equivalent of:
  #
  #   fxparseAccel("Alt+F")
  #
  def Fox.fxparseHotKey(s); end
  
  # Locate hot key underline offset from begin of string
  def Fox.fxfindhotkeyoffset(s); end
  
  # Get highlight color
  def Fox.makeHiliteColor(clr); end
  
  # Get shadow color
  def Fox.makeShadowColor(clr); end
  
  #
  # Return the RGB value for this color name.
  #
  def Fox.fxcolorfromname(colorName); end
  
  #
  # Return the name of the closest color to the input RGB value.
  #
  def Fox.fxnamefromcolor(color); end
  
  # Convert RGB to HSV
  def Fox.fxrgb_to_hsv(r, g, b); end
  
  # Convert HSV to RGB
  def Fox.fxhsv_to_rgb(h, s, v); end
  
  # Return the version number that the FOX library has been compiled with, as a String (e.g. "1.0.34"). 
  def Fox.fxversion(); end
  
  # Controls tracing level
  def Fox.fxTraceLevel; end
end

