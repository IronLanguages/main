module Fox
  #
  # OpenGL shape object.
  #
  # === Shape drawing options
  #
  # +SURFACE_SINGLESIDED+::	Single-sided [both sides same]
  # +SURFACE_DUALSIDED+::	Dual-sided surface
  # +SHADING_NONE+::		No light source
  # +SHADING_SMOOTH+::		Smooth shaded
  # +SHADING_FLAT+::		Flag shaded
  # +FACECULLING_OFF+::		No face culling
  # +FACECULLING_ON+::		Cull backward facing surfaces
  # +STYLE_SURFACE+::		Draw filled surfaces
  # +STYLE_WIREFRAME+::		Draw wire frame
  # +STYLE_POINTS+::		Draw as points
  # +STYLE_BOUNDBOX+::		Draw bounding box
  #
  # === Message identifiers
  #
  # +ID_SHADEOFF+::		x
  # +ID_SHADEON+::		x
  # +ID_SHADESMOOTH+::		x
  # +ID_TOGGLE_SIDED+::		x
  # +ID_TOGGLE_CULLING+::	x
  # +ID_STYLE_POINTS+::		x
  # +ID_STYLE_WIREFRAME+::	x
  # +ID_STYLE_SURFACE+::	x
  # +ID_STYLE_BOUNDINGBOX+::	x
  # +ID_FRONT_MATERIAL+::	x
  # +ID_BACK_MATERIAL+::	x
  #
  class FXGLShape < FXGLObject
  
    # Tool tip message for this shape [String]
    attr_accessor :tipText

    # Position [FXVec3f]
    attr_accessor :position
    
    #
    # Draws the shape in this GL viewer.
    #
    def drawshape(viewer); end

    #
    # Construct with specified origin, options and front and back materials.
    #
    def initialize(x, y, z, opts, front=nil, back=nil) # :yields: theGLShape
    end

    #
    # Set the material for specified side, where _side_ = 0 or 1
    # and _mtl_ is an FXMaterial instance.
    #
    def setMaterial(side, mtl); end

    #
    # Get the material for specified side (where _side_ = 0 or 1).
    #
    def getMaterial(side); end
    
    #
    # Set the range (an FXRangef instance) for this shape.
    #
    def setRange(box); end
  end
end

