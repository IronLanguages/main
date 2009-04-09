#!/usr/bin/env ruby

require 'fox16'
begin
  require 'opengl'
rescue LoadError
  require 'fox16/missingdep'
  MSG = <<EOM
  Sorry, this example depends on the OpenGL extension. Please
  check the Ruby Application Archives for an appropriate
  download site.
EOM
  missingDependency(MSG)
end


include Fox

class GLTestWindow < FXMainWindow

  # How often our timer will fire (in milliseconds)
  TIMER_INTERVAL = 100

  # Draws a simple box using the given corners
  def drawBox(xmin, ymin, zmin, xmax, ymax, zmax)
    GL.Begin(GL::TRIANGLE_STRIP)
      GL.Normal(0.0, 0.0, -1.0)
      GL.Vertex(xmin, ymin, zmin)
      GL.Vertex(xmin, ymax, zmin)
      GL.Vertex(xmax, ymin, zmin)
      GL.Vertex(xmax, ymax, zmin)
    GL.End()
  
    GL.Begin(GL::TRIANGLE_STRIP)
      GL.Normal(1.0, 0.0, 0.0)
      GL.Vertex(xmax, ymin, zmin)
      GL.Vertex(xmax, ymax, zmin)
      GL.Vertex(xmax, ymin, zmax)
      GL.Vertex(xmax, ymax, zmax)
    GL.End()
  
    GL.Begin(GL::TRIANGLE_STRIP)
      GL.Normal(0.0, 0.0, 1.0)
      GL.Vertex(xmax, ymin, zmax)
      GL.Vertex(xmax, ymax, zmax)
      GL.Vertex(xmin, ymin, zmax)
      GL.Vertex(xmin, ymax, zmax)
    GL.End()
  
    GL.Begin(GL::TRIANGLE_STRIP)
      GL.Normal(-1.0, 0.0, 0.0)
      GL.Vertex(xmin, ymin, zmax)
      GL.Vertex(xmin, ymax, zmax)
      GL.Vertex(xmin, ymin, zmin)
      GL.Vertex(xmin, ymax, zmin)
    GL.End()
  
    GL.Begin(GL::TRIANGLE_STRIP)
      GL.Normal(0.0, 1.0, 0.0)
      GL.Vertex(xmin, ymax, zmin)
      GL.Vertex(xmin, ymax, zmax)
      GL.Vertex(xmax, ymax, zmin)
      GL.Vertex(xmax, ymax, zmax)
    GL.End()
  
    GL.Begin(GL::TRIANGLE_STRIP)
      GL.Normal(0.0, -1.0, 0.0)
      GL.Vertex(xmax, ymin, zmax)
      GL.Vertex(xmax, ymin, zmin)
      GL.Vertex(xmin, ymin, zmax)
      GL.Vertex(xmin, ymin, zmin)
    GL.End()
  end

  # Draw the GL scene
  def drawScene
    lightPosition = [15.0, 10.0, 5.0, 1.0]
    lightAmbient  = [ 0.1,  0.1, 0.1, 1.0]
    lightDiffuse  = [ 0.9,  0.9, 0.9, 1.0]
    redMaterial   = [ 1.0,  0.0, 0.0, 1.0]
    blueMaterial  = [ 0.0,  0.0, 1.0, 1.0]
  
    width = @glcanvas.width.to_f
    height = @glcanvas.height.to_f
    aspect = width/height
  
    # Make context current
    @glcanvas.makeCurrent()
    
    GL.Viewport(0, 0, @glcanvas.width, @glcanvas.height)
  
    GL.ClearColor(1.0, 1.0, 1.0, 1.0)
    GL.Clear(GL::COLOR_BUFFER_BIT|GL::DEPTH_BUFFER_BIT)
    GL.Enable(GL::DEPTH_TEST)
    
    GL.Disable(GL::DITHER)
  
    GL.MatrixMode(GL::PROJECTION)
    GL.LoadIdentity()
    GLU.Perspective(30.0, aspect, 1.0, 100.0)
  
    GL.MatrixMode(GL::MODELVIEW)
    GL.LoadIdentity()
    GLU.LookAt(5.0, 10.0, 15.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0)
  
    GL.ShadeModel(GL::SMOOTH)
    GL.Light(GL::LIGHT0, GL::POSITION, lightPosition)
    GL.Light(GL::LIGHT0, GL::AMBIENT, lightAmbient)
    GL.Light(GL::LIGHT0, GL::DIFFUSE, lightDiffuse)
    GL.Enable(GL::LIGHT0)
    GL.Enable(GL::LIGHTING)
  
    GL.Material(GL::FRONT, GL::AMBIENT, blueMaterial)
    GL.Material(GL::FRONT, GL::DIFFUSE, blueMaterial)
  
    GL.PushMatrix()
    GL.Rotated(@angle, 0.0, 1.0, 0.0)
    drawBox(-1, -1, -1, 1, 1, 1)
  
    GL.Material(GL::FRONT, GL::AMBIENT, redMaterial)
    GL.Material(GL::FRONT, GL::DIFFUSE, redMaterial)
  
    GL.PushMatrix()
    GL.Translated(0.0, 1.75, 0.0)
    GL.Rotated(@angle, 0.0, 1.0, 0.0)
    drawBox(-0.5, -0.5, -0.5, 0.5, 0.5, 0.5)
    GL.PopMatrix()
  
    GL.PushMatrix()
    GL.Translated(0.0, -1.75, 0.0)
    GL.Rotated(@angle, 0.0, 1.0, 0.0)
    drawBox(-0.5, -0.5, -0.5, 0.5, 0.5, 0.5)
    GL.PopMatrix()
  
    GL.PushMatrix()
    GL.Rotated(90.0, 1.0, 0.0, 0.0)
    GL.Translated(0.0, 1.75, 0.0)
    GL.Rotated(@angle, 0.0, 1.0, 0.0)
    drawBox(-0.5,-0.5,-0.5,0.5,0.5,0.5)
    GL.PopMatrix()
  
    GL.PushMatrix()
    GL.Rotated(90.0, -1.0, 0.0, 0.0)
    GL.Translated(0.0,1.75,0.0)
    GL.Rotated(@angle, 0.0, 1.0, 0.0)
    drawBox(-0.5,-0.5,-0.5,0.5,0.5,0.5)
    GL.PopMatrix()
  
    GL.PushMatrix()
    GL.Rotated(90.0, 0.0, 0.0, 1.0)
    GL.Translated(0.0,1.75,0.0)
    GL.Rotated(@angle, 0.0, 1.0, 0.0)
    drawBox(-0.5,-0.5,-0.5,0.5,0.5,0.5)
    GL.PopMatrix()
  
    GL.PushMatrix()
    GL.Rotated(90.0, 0.0, 0.0, -1.0)
    GL.Translated(0.0,1.75,0.0)
    GL.Rotated(@angle, 0.0, 1.0, 0.0)
    drawBox(-0.5,-0.5,-0.5,0.5,0.5,0.5)
    GL.PopMatrix()
  
    GL.PopMatrix()
  
    # Swap if it is double-buffered
    if @glvisual.isDoubleBuffer
      @glcanvas.swapBuffers
    end
    
    # Make context non-current
    @glcanvas.makeNonCurrent
  end

  def initialize(app)
    # Invoke the base class initializer
    super(app, "OpenGL Test Application", :opts => DECOR_ALL, :width => 800, :height => 600)

    # Construct the main window elements
    frame = FXHorizontalFrame.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    frame.padLeft, frame.padRight = 0, 0
    frame.padTop, frame.padBottom = 0, 0

    # Left pane to contain the glcanvas
    glcanvasFrame = FXVerticalFrame.new(frame,
      LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT)
    glcanvasFrame.padLeft, glcanvasFrame.padRight = 10, 10
    glcanvasFrame.padTop, glcanvasFrame.padBottom = 10, 10
  
    # Label above the glcanvas
    FXLabel.new(glcanvasFrame, "OpenGL Canvas Frame", nil,
      JUSTIFY_CENTER_X|LAYOUT_FILL_X)
  
    # Horizontal divider line
    FXHorizontalSeparator.new(glcanvasFrame, SEPARATOR_GROOVE|LAYOUT_FILL_X)
  
    # Drawing glcanvas
    glpanel = FXVerticalFrame.new(glcanvasFrame, (FRAME_SUNKEN|FRAME_THICK|
      LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT))
    glpanel.padLeft, glpanel.padRight = 0, 0
    glpanel.padTop, glpanel.padBottom = 0, 0
      
    # A visual to draw OpenGL
    @glvisual = FXGLVisual.new(getApp(), VISUAL_DOUBLEBUFFER)
  
    # Drawing glcanvas
    @glcanvas = FXGLCanvas.new(glpanel, @glvisual, :opts => LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT)
    @glcanvas.connect(SEL_PAINT) { drawScene }
    @glcanvas.connect(SEL_CONFIGURE) do
      if @glcanvas.makeCurrent
        GL.Viewport(0, 0, @glcanvas.width, @glcanvas.height)
        @glcanvas.makeNonCurrent
      end
    end
  
    # Right pane for the buttons
    buttonFrame = FXVerticalFrame.new(frame, LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT)
    buttonFrame.padLeft, buttonFrame.padRight = 10, 10
    buttonFrame.padTop, buttonFrame.padBottom = 10, 10
      
    # Label above the buttons
    FXLabel.new(buttonFrame, "Button Frame", nil,
      JUSTIFY_CENTER_X|LAYOUT_FILL_X)
  
    # Horizontal divider line
    FXHorizontalSeparator.new(buttonFrame, SEPARATOR_RIDGE|LAYOUT_FILL_X)
  
    # Spin according to timer
    spinTimerBtn = FXButton.new(buttonFrame, "Spin &Timer\tSpin using interval timers\nNote the app blocks until the interal has elapsed...",nil,nil,0,FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT)
    spinTimerBtn.padLeft, spinTimerBtn.padRight = 10, 10
    spinTimerBtn.padTop, spinTimerBtn.padBottom = 5, 5
    spinTimerBtn.connect(SEL_COMMAND) do
      @spinning = true
      @timer = getApp().addTimeout(TIMER_INTERVAL, :repeat => true) do
        @angle += 2.0
        if @angle > 360.0
          @angle -= 360.0
        end
        drawScene()
      end
    end
    spinTimerBtn.connect(SEL_UPDATE) do |sender, sel, ptr|
      @spinning ? sender.disable : sender.enable
    end

    # Spin according to chore
    spinChoreBtn = FXButton.new(buttonFrame,
      "Spin &Chore\tSpin as fast as possible using chores\nNote even though the
      app is very responsive, it never blocks;\nthere is always something to
      do...", :opts => FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT)
    spinChoreBtn.padLeft, spinChoreBtn.padRight = 10, 10
    spinChoreBtn.padTop, spinChoreBtn.padBottom = 5, 5
    spinChoreBtn.connect(SEL_COMMAND) do
      @spinning = true
      @chore = getApp().addChore(:repeat => true) do
        @angle += 2.0
        if @angle > 360.0
          @angle -= 360.0
        end
        drawScene()
      end
    end
    spinChoreBtn.connect(SEL_UPDATE) do |sender, sel, ptr|
      @spinning ? sender.disable : sender.enable
    end
  
    # Stop spinning
    stopBtn = FXButton.new(buttonFrame,
      "&Stop Spin\tStop this mad spinning, I'm getting dizzy",
      :opts => FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT)
    stopBtn.padLeft, stopBtn.padRight = 10, 10
    stopBtn.padTop, stopBtn.padBottom = 5, 5
    stopBtn.connect(SEL_COMMAND) do
      @spinning = false
      if @timer
        getApp().removeTimeout(@timer)
        @timer = nil
      end
      if @chore
        getApp().removeChore(@chore)
        @chore = nil
      end
    end
    stopBtn.connect(SEL_UPDATE) do |sender, sel, ptr|
      @spinning ? sender.enable : sender.disable
    end
  
    # Exit button
    exitBtn = FXButton.new(buttonFrame, "&Exit\tExit the application", nil,
      getApp(), FXApp::ID_QUIT,
      FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT)
    exitBtn.padLeft, exitBtn.padRight = 10, 10
    exitBtn.padTop, exitBtn.padBottom = 5, 5
      
    # Make a tooltip
    FXToolTip.new(getApp())
  
    # Initialize private variables
    @spinning = false
    @chore = nil
    @timer = nil
    @angle = 0.0
  end

  # Create and initialize
  def create
    super
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  # Construct the application
  application = FXApp.new("GLTest", "FoxTest")
  
  # To ensure that the chores-based spin will run as fast as possible,
  # we can disable the chore in FXRuby's event loop that tries to schedule
  # other threads. This is OK for this program because there aren't any
  # other Ruby threads running.
  
  application.disableThreads

  # Construct the main window
  GLTestWindow.new(application)

  # Create the app's windows
  application.create

  # Run the application
  application.run
end
