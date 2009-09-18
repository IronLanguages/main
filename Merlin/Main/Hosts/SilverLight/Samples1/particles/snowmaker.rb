#Inspired by Kirupa's Flash Snow flakes (still looking for license)
include System::Windows::Shapes
include System::Windows::Controls
include System::Windows::Media

class SnowMaker
  def initialize(container, maker_options={})    
    @options = {:total_flakes=>40, :width=>800, :height=>600}.merge!(maker_options)
    1.upto(@options[:total_flakes].to_i) do |index|
      circle = Ellipse.new(@options.merge!(:index=>index))
      circle.Fill = SolidColorBrush.new(Colors.White)
      circle.Stroke = SolidColorBrush.new(Colors.White)
      container.children.add(circle)
      yield(circle) if block_given?
    end
  end
end

#monkeypatch Ellipse into a SnowFlake. BECAUSE WE CAN!!!! :) 
class System::Windows::Shapes::Ellipse 
  def initialize(boundaries={})
    @options = {:height=>800, :width=>600}.merge!(boundaries)
    @@random ||= System::Random.new()
    @canvas_width = @options[:width].to_i
    @canvas_height = @options[:height].to_i
    @x = 1
    create()
  end

  def create  
    @x_speed = (@@random.next_double/20.0)
    @y_speed = (0.01 + @@random.next_double * 2.0)
    @radius = @@random.next_double
    @scale = (0.01 + @@random.next_double * 2.0)
    Canvas.set_top(self, @@random.next(@canvas_height))
    Canvas.set_left(self, @@random.next(@canvas_width))

    @y = Canvas.get_top(self)

    self.width = (5 * @scale)
    self.height = (5 * @scale)
    self.opacity = 0.1 + @@random.next_double

    CompositionTarget.Rendering{|s, e| move(s, e) }
  end

  def move(sender, eventargs)
    @x += @x_speed
    @y += @y_speed

    Canvas.set_top(self, @y)
    Canvas.set_left(self, (Canvas.get_left(self) + @radius*Math.cos(@x)).to_f)

    if (Canvas.get_top(self) > @canvas_height):
      Canvas.set_top(self, 0)
      @y = Canvas.get_top(self)
    end
  end
end

SnowMaker.new(me.find_name('backyard'))
