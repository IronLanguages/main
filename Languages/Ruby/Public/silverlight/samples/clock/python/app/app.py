from System.Windows import Application
from System.Windows.Controls import Canvas
from datetime import datetime

class Clock:
  
  def __init__(self):
  	self.scene = Application.Current.LoadRootVisual(Canvas(), "app.xaml")
  
  def fromAngle(self, time, divisor = 5, offset = 0):
    return ((time / (12.0 * divisor)) * 360) + offset + 180

  def toAngle(self, time):
    return self.fromAngle(time) + 360

  def start(self):
    d = datetime.now()

    self.scene.hourAnimation.From    = self.fromAngle(d.hour, 1, d.minute/2)
    self.scene.hourAnimation.To      = self.toAngle(d.hour)
    self.scene.minuteAnimation.From  = self.fromAngle(d.minute)
    self.scene.minuteAnimation.To    = self.toAngle(d.minute)
    self.scene.secondAnimation.From  = self.fromAngle(d.second)
    self.scene.secondAnimation.To    = self.toAngle(d.second)

Clock().start()
