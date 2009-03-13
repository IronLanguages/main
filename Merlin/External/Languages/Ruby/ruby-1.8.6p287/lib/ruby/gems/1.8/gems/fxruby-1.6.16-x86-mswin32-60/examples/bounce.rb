require 'fox16'

include Fox

# How long to pause between updates (in milliseconds)
ANIMATION_TIME = 20

class Ball

  attr_reader :color
  attr_reader :center
  attr_reader :radius
  attr_reader :dir
  attr_reader :x, :y
  attr_reader :w, :h
  attr_accessor :worldWidth
  attr_accessor :worldHeight

  # Returns an initialized ball
  def initialize(r)
    @radius = r
    @w = 2*@radius
    @h = 2*@radius
    @center = FXPoint.new(50, 50)
    @x = @center.x - @radius
    @y = @center.y - @radius
    @color = FXRGB(255, 0, 0) # red
    @dir = FXPoint.new(-1, 0)
    setWorldSize(1000, 1000)
  end
  
  # Draw the ball into this device context
  def draw(dc)
    dc.setForeground(color)
    dc.fillArc(x, y, w, h, 0, 64*90)
    dc.fillArc(x, y, w, h, 64*90, 64*180)
    dc.fillArc(x, y, w, h, 64*180, 64*270)
    dc.fillArc(x, y, w, h, 64*270, 64*360)
  end
  
  def bounce
    @dir = -@dir
  end
  
  def collision?
    (x < 0) || (x+w > worldWidth) || (y < 0) || (y+h > worldHeight)
  end

  def setWorldSize(ww, wh)
    @worldWidth = ww
    @worldHeight = wh
  end
    
  def move(units)
    dx = dir.x*units
    dy = dir.y*units
    center.x += dx
    center.y += dy
    @x += dx
    @y += dy
    if collision?
      bounce
      move(units)
    end
  end
end

class BounceWindow < FXMainWindow

  include Responder
  
  def initialize(app)
    # Initialize base class first
    super(app, "Bounce", :opts => DECOR_ALL, :width => 400, :height => 300)
    
    # Set up the canvas
    @canvas = FXCanvas.new(self, :opts => LAYOUT_FILL_X|LAYOUT_FILL_Y)
    
    # Set up the back buffer
    @backBuffer = FXImage.new(app, nil, IMAGE_KEEP)
    
    # Handle expose events (by blitting the image to the canvas)
    @canvas.connect(SEL_PAINT) { |sender, sel, evt|
      FXDCWindow.new(sender, evt) { |dc|
        dc.drawImage(@backBuffer, 0, 0)
      }
    }
    
    # Handle resize events
    @canvas.connect(SEL_CONFIGURE) { |sender, sel, evt|
      @backBuffer.create unless @backBuffer.created?
      @backBuffer.resize(sender.width, sender.height)
      @ball.setWorldSize(sender.width, sender.height)
      drawScene(@backBuffer)
    }
    
    @ball = Ball.new(20)
  end
  
  #
  # Draws the scene into the back buffer
  #
  def drawScene(drawable)
    FXDCWindow.new(drawable) { |dc|
      dc.setForeground(FXRGB(255, 255, 255))
      dc.fillRectangle(0, 0, drawable.width, drawable.height)
      @ball.draw(dc)
    }
  end
  
  def updateCanvas
    @ball.move(10)
    drawScene(@backBuffer)
    @canvas.update
  end
  
  #
  # Handle timeout events
  #
  def onTimeout(sender, sel, ptr)
    # Move the ball and re-draw the scene
    updateCanvas
    
    # Re-register the timeout
    getApp().addTimeout(ANIMATION_TIME, method(:onTimeout))
    
    # Done
    return 1
  end
  
  #
  # Create server-side resources
  #
  def create
    # Create base class
    super
    
    # Create the image used as the back-buffer
    @backBuffer.create
    
    # Draw the initial scene into the back-buffer
    drawScene(@backBuffer)
    
    # Register the timer used for animation
    getApp().addTimeout(ANIMATION_TIME, method(:onTimeout))
    
    # Show the main window
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  FXApp.new("Bounce", "FXRuby") do |theApp|
    BounceWindow.new(theApp)
    theApp.create
    theApp.run
  end
end

