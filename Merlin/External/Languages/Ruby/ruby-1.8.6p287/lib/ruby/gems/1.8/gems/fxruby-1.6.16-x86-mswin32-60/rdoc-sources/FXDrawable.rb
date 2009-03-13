module Fox
  #
  # FXDrawable is an abstract base class for any surface that can be
  # drawn upon, such as an FXWindow or an FXImage.
  #
  class FXDrawable < FXId
  
    # Width of drawable, in pixels [Integer]
    attr_reader	:width
    
    # Height of drawable, in pixels [Integer]
    attr_reader	:height
    
    # Visual [FXVisual]
    attr_accessor :visual

    #
    # Resize drawable to the specified width and height.
    #
    # ==== Parameters:
    #
    # +width+::	new drawable width, in pixels [Integer]
    # +height+::	new drawable height, in pixels [Integer]
    #
    def resize(w, h); end
  end
end
