#Inspired by Kirupa's Flash Snow flakes (still looking for license)

import clr
from System import *
from System.Windows import *
from System.Windows.Controls import *
from System.Windows.Shapes import *

class Snowmaker:
    def __init__(self, container, total_flakes=3, width=640, height=480):
        for i in range(total_flakes):
            container.Children.Add(Flake((width, height)))

class Flake():
    """I'm a pretty snow flake. I'll float and be your friend. """

    def __init__(self, boundaries):
        self.create(boundaries)

    def create(self, canvas_size=(1,1)):
        self.random = Random()
        self.canvas_size = canvas_size

        self.falling_speed = self.random.NextDouble()/20
        self.floating_speed = 0.01 + self.random.NextDouble() * 2
        self.radius = self.random.NextDouble()
        self.scale = 0.01 + self.random.NextDouble() * 2
        self.set_top()
        self.set_left()
        self.y = Canvas.GetTop(self)
        self.x = Canvas.GetLeft(self)

        self.Children.Add(Ellipse(Fill=SolidColorBrush(Colors.While), Stroke=SolidColorBrush(Colors.Red))
        self.Width = 5 * self.scale
        self.Height = 5 * self.scale
        self.Opacity = 0.1 + self.random.NextDouble()

        CompositionTarget.Rendering += self.move

    def set_top(self):
        Canvas.SetTop(self, self.random.Next(self.canvas_size[1]))

    def set_left(self):
        Canvas.SetLeft(self, self.random.Next(self.canvas_size[0]))

    def move(self, sender, eventargs):
        self.x += self.falling_speed
        self.y += self.floating_speed

        Canvas.SetTop(self, self.y)
        Canvas.SetLeft(self, Canvas.GetLeft(self) + self.radius*Math.Cos(self.x))

        if(Canvas.GetTop(self) > self.canvas_size[1]):
            Canvas.SetTop(self, -self.ActualHeight-self.Height)
            self.y = Canvas.GetTop(self)

Snowmaker(me.backyard)