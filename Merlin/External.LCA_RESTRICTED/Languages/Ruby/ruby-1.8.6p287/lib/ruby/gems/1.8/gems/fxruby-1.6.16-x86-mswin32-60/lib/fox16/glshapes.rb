require 'opengl'

module Fox

  HANDLE_SIZE = 4.0

  #
  # OpenGL point object
  #
  class FXGLPoint < FXGLObject
  
    # Point position, in model coordinates (a 3-element array)
    attr_accessor :pos
    
    #
    # Returns an initialized FXGLPoint instance.
    # If no arguments are passed to #new, the initial point position is
    # (0.0, 0.0, 0.0). You can specify a different initial position by
    # passing in the x, y and z coordinates individually:
    #
    #     aPoint = FXGLPoint.new(x, y, z)
    #
    # or as a 3-element array:
    #
    #     aPoint = FXGLPoint.new([x, y, z])
    #
    def initialize(*args)
      super()
      if args.length == 0
        @pos = [0.0, 0.0, 0.0]
      elsif args.length == 3
        @pos = [args[0], args[1], args[2]]
      else
        @pos = args[0]
      end
    end
    
    #
    # Return the bounding box (an FXRangef instance) for this point.
    #
    def bounds
      FXRangef.new(@pos[0], @pos[0], @pos[1], @pos[1], @pos[2], @pos[2])
    end
    
    #
    # Draw this point into _viewer_ (an FXGLViewer instance).
    #
    def draw(viewer)
      GL::Color(0.0, 0.0, 1.0)
      GL::PointSize(HANDLE_SIZE)
      GL::Begin(GL::POINTS)
      GL::Vertex(@pos)
      GL::End()
    end
    
    #
    # Perform hit test for this point in _viewer_ (an FXGLViewer instance).
    #
    def hit(viewer)
      GL::Begin(GL::POINTS)
      GL::Vertex(@pos)
      GL::End()
    end
  end

  #
  # OpenGL line object
  #
  class FXGLLine < FXGLObject
  
    # Starting point for line [FXGLPoint]
    attr_accessor :fm
    
    # End point for line [FXGLPoint]
    attr_accessor :to
    
    #
    # Return an initialized FXGLLine instance.
    #
    # If no arguments are passed to #new, the initial starting and ending
    # points for the line are (-0.5, 0.0, 0.0) and (0.5, 0.0, 0.0), respectively.
    # You can specify different initial start and end points by passing in
    # another FXGLLine instance from which to copy the start and end point
    # values, e.g.
    #
    #     aLine = FXGLLine.new(anotherLine)
    #
    # or by passing in the x, y and z coordinates individually:
    #
    #     aLine = FXGLLine.new(x0, y0, z0, x1, y1, z1)
    #
    def initialize(*args)
      super()
      if args.length == 0
	@fm = FXGLPoint.new(-0.5, 0.0, 0.0)
	@to = FXGLPoint.new( 0.5, 0.0, 0.0)
      elsif args.length == 1
	@fm = args[0].fm
	@to = args[0].to
      else
	@fm = FXGLPoint.new(args[0], args[1], args[2])
	@to = FXGLPoint.new(args[3], args[4], args[5])
      end
    end
    
    #
    # Return the bounding box (an FXRangef instance) for this line.
    #
    def bounds
      FXRangef.new([@fm.pos[0], @to.pos[0]].min,
                  [@fm.pos[0], @to.pos[0]].max,
                  [@fm.pos[1], @to.pos[1]].min,
                  [@fm.pos[1], @to.pos[1]].max,
                  [@fm.pos[2], @to.pos[2]].min,
                  [@fm.pos[2], @to.pos[2]].max)
    end
    
    #
    # Draw this line into _viewer_ (an FXGLViewer instance).
    #
    def draw(viewer)
      GL::Color(1.0, 0.0, 0.0)
      GL::PointSize(HANDLE_SIZE)
      GL::Begin(GL::LINES)
      GL::Vertex(@fm.pos)
      GL::Vertex(@to.pos)
      GL::End()
    end
    
    #
    # Perform hit-test for this line in _viewer_ (an FXGLViewer instance).
    #
    def hit(viewer)
      GL::Begin(GL::LINES)
      GL::Vertex(@fm.pos)
      GL::Vertex(@to.pos)
      GL::End()
    end
  end

  #
  # OpenGL cube object
  #
  class FXGLCube < FXGLShape

    # Cube width [Float]
    attr_accessor :width
    
    # Cube height [Float]
    attr_accessor :height
    
    # Cube depth [Float]
    attr_accessor :depth

    #
    # Return an initialized FXGLCube instance.
    #
    # One option is to initialize the cube with a specified origin,
    # width, height and depth:
    #
    #     aCube = FXGLCube.new(x, y, z, w, h, d)
    #
    # If left unspecified, the width (_w_), height (_h_) and depth (_d_)
    # default to 1.0.
    #
    # Another option is to initialize the cube with a specified origin,
    # width, height, depth and material:
    #
    #     aCube = FXGLCube.new(x, y, z, w, h, d, material)
    #
    # where the _material_ is an FXMaterial instance.
    #
    def initialize(*args)
      if args.length == 7
	super(args[0], args[1], args[2], SHADING_SMOOTH|STYLE_SURFACE,
              args[6], args[6])
      else
	super(args[0], args[1], args[2], SHADING_SMOOTH|STYLE_SURFACE)
      end
      @width = args[3] ? args[3] : 1.0
      @height = args[4] ? args[4] : 1.0
      @depth = args[5] ? args[5] : 1.0
      setRange(FXRangef.new(-0.5*@width, 0.5*@width,
                           -0.5*@height, 0.5*@height,
                           -0.5*@depth, 0.5*@depth))
    end

    #
    # Draws this cube into _viewer_ (an FXGLViewer instance).
    #
    def drawshape(viewer)
      xmin, xmax = -0.5*@width, 0.5*@width
      ymin, ymax = -0.5*@height, 0.5*@height
      zmin, zmax = -0.5*@depth, 0.5*@depth

      # Draw low face
      GL::Begin(GL::TRIANGLE_STRIP)
	GL::Normal(0.0, 0.0, -1.0)
	GL::Vertex(xmin, ymin, zmin)
	GL::Vertex(xmin, ymax, zmin)
	GL::Vertex(xmax, ymin, zmin)
	GL::Vertex(xmax, ymax, zmin)
      GL::End()

      # Draw east face
      GL::Begin(GL::TRIANGLE_STRIP)
	GL::Normal(1.0, 0.0, 0.0)
	GL::Vertex(xmax, ymin, zmin)
	GL::Vertex(xmax, ymax, zmin)
	GL::Vertex(xmax, ymin, zmax)
	GL::Vertex(xmax, ymax, zmax)
      GL::End()

      # Draw high face
      GL::Begin(GL::TRIANGLE_STRIP)
	GL::Normal(0.0, 0.0, 1.0)
	GL::Vertex(xmax, ymin, zmax)
	GL::Vertex(xmax, ymax, zmax)
	GL::Vertex(xmin, ymin, zmax)
	GL::Vertex(xmin, ymax, zmax)
      GL::End()

      # Draw west face
      GL::Begin(GL::TRIANGLE_STRIP)
	GL::Normal(-1.0, 0.0, 0.0)
	GL::Vertex(xmin, ymin, zmax)
	GL::Vertex(xmin, ymax, zmax)
	GL::Vertex(xmin, ymin, zmin)
	GL::Vertex(xmin, ymax, zmin)
      GL::End()

      # Draw north face
      GL::Begin(GL::TRIANGLE_STRIP)
	GL::Normal(0.0, 1.0, 0.0)
	GL::Vertex(xmin, ymax, zmin)
	GL::Vertex(xmin, ymax, zmax)
	GL::Vertex(xmax, ymax, zmin)
	GL::Vertex(xmax, ymax, zmax)
      GL::End()

      # Draw south face
      GL::Begin(GL::TRIANGLE_STRIP)
	GL::Normal(0.0, -1.0, 0.0)
	GL::Vertex(xmin, ymin, zmax)
	GL::Vertex(xmin, ymin, zmin)
	GL::Vertex(xmax, ymin, zmax)
	GL::Vertex(xmax, ymin, zmin)
      GL::End()
    end
  end

  #
  # OpenGL cone object
  #
  class FXGLCone < FXGLShape
    # Cone fidelity
    SLICES_NUMBER = 20
    STACKS_NUMBER = 20
    LOOPS = 4

    # Cone height [Float]
    attr_accessor :height
    
    # Cone base radius [Float]
    attr_accessor :radius
    
    # Number of slices (default is 20) [Integer]
    attr_accessor :slices
    
    # Number of stacks (default is 20) [Integer]
    attr_accessor :stacks
    
    # Number of loops (default is 4) [Integer]
    attr_accessor :loops

    #
    # Returns an initialized FXGLCone instance.
    #
    # One option is to initialize the cone with a specified origin,
    # height and radius:
    #
    #     aCone = FXGLCone.new(x, y, z, h, r)
    #
    # If left unspecified, the height (_h_) and radius (_r_) default to 1.0.
    #
    # Another option is to initialize the cone with a specified origin,
    # height, radius and material:
    #
    #     aCone = FXGLCone.new(x, y, z, h, r, material)
    #
    # where the _material_ is an FXMaterial instance.
    #
    def initialize(*args)
      if args.length == 5
	super(args[0], args[1], args[2], SHADING_SMOOTH|STYLE_SURFACE)
      elsif args.length == 6
	super(args[0], args[1], args[2], SHADING_SMOOTH|STYLE_SURFACE,
              args[5], args[5])
      end
      @height = args[3] ? args[3] : 1.0
      @radius = args[4] ? args[4] : 1.0
      @slices = SLICES_NUMBER
      @stacks = STACKS_NUMBER
      @loops  = LOOPS
      setRange(FXRangef.new(-@radius, @radius, 0, @height, -@radius, @radius))
    end
    
    #
    # Draw this cone into _viewer_ (an FXGLViewer instance).
    #
    def drawshape(viewer)
      quad = GLU::NewQuadric()
      GLU::QuadricDrawStyle(quad, GLU::FILL)
      GL::PushMatrix()
      GL::Rotate(-90, 1, 0, 0)
      GLU::Cylinder(quad, @radius, 0, @height, @slices, @stacks)
      GLU::QuadricOrientation(quad, GLU::INSIDE)
      GLU::Disk(quad, 0, @radius, @slices, @loops)
      GLU::DeleteQuadric(quad)
      GL::PopMatrix()
    end
  end

  #
  # OpenGL cylinder object
  #
  class FXGLCylinder < FXGLShape
    # Cylinder fidelity
    SLICES_NUMBER = 20
    STACKS_NUMBER = 20
    LOOPS = 4

    # Cylinder height [Float]
    attr_accessor :height
    
    # Cylinder radius [Float]
    attr_accessor :radius
    
    # Number of slices (default is 20) [Integer]
    attr_accessor :slices
    
    # Number of stacks (default is 20) [Integer]
    attr_accessor :stacks
    
    # Number of loops (default is 4) [Integer]
    attr_accessor :loops

    #
    # Returns an initialized FXGLCylinder instance.
    #
    # One option is to initialize the cylinder with a specified origin,
    # height and radius:
    #
    #     aCylinder = FXGLCylinder.new(x, y, z, h, r)
    #
    # If left unspecified, the height (_h_) and radius (_r_) default to 1.0.
    #
    # Another option is to initialize the cylinder with a specified origin,
    # height, radius and material:
    #
    #     aCylinder = FXGLCylinder.new(x, y, z, h, r, material)
    #
    # where the _material_ is an FXMaterial instance.
    #
    def initialize(*args)
      if args.length == 5
	super(args[0], args[1], args[2], SHADING_SMOOTH|STYLE_SURFACE)
      else
	super(args[0], args[1], args[2], SHADING_SMOOTH|STYLE_SURFACE,
              args[5], args[5])
      end
      @height = args[3] ? args[3] : 1.0
      @radius = args[4] ? args[4] : 1.0
      @slices = SLICES_NUMBER
      @stacks = STACKS_NUMBER
      @loops  = LOOPS
      setRange(FXRangef.new(-@radius, @radius, 0, @height, -@radius, @radius))
    end
    
    #
    # Draw this cylinder into _viewer_ (an FXGLViewer instance).
    #
    def drawshape(viewer)
      quad = GLU::NewQuadric()
      GLU::QuadricDrawStyle(quad, GLU::FILL)
      GL::PushMatrix()
      GL::Rotate(-90, 1, 0, 0)
      GLU::Cylinder(quad, @radius, @radius, @height, @slices, @stacks)
      GLU::QuadricOrientation(quad, GLU::INSIDE)
      GLU::Disk(quad, 0, @radius, @slices, @loops)
      GL::Translate(0, 0, @height)
      GLU::QuadricOrientation(quad, GLU::OUTSIDE)
      GLU::Disk(quad, 0, @radius, @slices, @loops)
      GL::PopMatrix()
      GLU::DeleteQuadric(quad)
    end
  end

  #
  # OpenGL sphere object
  #
  class FXGLSphere < FXGLShape
    # Sphere fidelity
    SLICES_NUMBER = 20
    STACKS_NUMBER = 20

    # Sphere radius [Float]
    attr_accessor :radius
    
    # Number of slices (default is 20) [Integer]
    attr_accessor :slices
    
    # Number of stacks (default is 20) [Integer]
    attr_accessor :stacks

    #
    # Returns an initialized FXGLSphere instance.
    #
    # One option is to initialize the sphere with a specified origin and
    # radius:
    #
    #     aSphere = FXGLSphere.new(x, y, z, r)
    #
    # If left unspecified, the radius (_r_) defaults to 1.0.
    #
    # Another option is to initialize the sphere with a specified origin,
    # radius and material:
    #
    #     aSphere = FXGLSphere.new(x, y, z, r, material)
    #
    # where the _material_ is an FXMaterial instance.
    #
    def initialize(*args)
      if args.length == 4
        super(args[0], args[1], args[2], SHADING_SMOOTH|STYLE_SURFACE)
      else
        super(args[0], args[1], args[2], SHADING_SMOOTH|STYLE_SURFACE,
        args[4], args[4])
      end
      @radius = args[3] ? args[3] : 1.0
      @slices = SLICES_NUMBER
      @stacks = STACKS_NUMBER
      setRange(FXRangef.new(-@radius, @radius, -@radius, @radius, -@radius, @radius))
    end
    
    #
    # Draw this sphere into _viewer_ (an FXGLViewer instance).
    #
    def drawshape(viewer)
      quad = GLU::NewQuadric()
      GLU::QuadricDrawStyle(quad, GLU::FILL)
      GLU::Sphere(quad, @radius, @slices, @stacks)
      GLU::DeleteQuadric(quad)
    end
  end
end
