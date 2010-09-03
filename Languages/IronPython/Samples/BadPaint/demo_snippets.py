#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

#------------------------------------------------------------------------------
#--INSERTS RANDOM SQUARES

from System.Windows.Controls import Canvas
from System.Windows.Shapes import *
from System.Windows.Media import *
from System import Random
rand = Random()

for i in xrange(100):
    rect = Rectangle(Width=20, Height=20, Fill=Brushes.Blue)
    Application.Painting.Children.Add(rect)
    Canvas.SetLeft(rect, rand.Next(Application.Painting.ActualWidth))
    Canvas.SetTop(rect, rand.Next(Application.Painting.ActualHeight))


#------------------------------------------------------------------------------
#--INSERT A CIRCLE CONSISTING OF SQUARES OF DIFFERENT COLORS

# setup circle
from System.Windows.Controls import Canvas
from System.Windows.Shapes import *
from System.Windows.Media import *
import math

dim = min(Application.Painting.ActualWidth - 20, Application.Painting.ActualHeight - 20)/2
for i, color in zip(xrange(0, 360, 10), dir(Brushes)):
    rect = Rectangle(Width=20, Height=20, Fill=getattr(Brushes, color))
    Application.Painting.Children.Add(rect)
    Canvas.SetTop(rect, dim * math.sin(i * math.pi*2/360) + dim)
    Canvas.SetLeft(rect, dim * math.cos(i * math.pi*2/360)+ dim)

#--IRONRUBY CODE TO SPIN THE CIRCLE ABOVE
# rotate
#Canvas = System::Windows::Controls::Canvas
#def callback
#    self.application.painting.children.each do |child|
#        top, left = Canvas.get_top(child), Canvas.get_left(child)
#        run = (left - dim) / dim
#        rise = (top - dim) / dim
#        angle = Math.atan2 rise, run
#        angle += Math::PI / 100
#        Canvas.set_top child, dim * Math.sin(angle) + dim
#        Canvas.set_left child, dim * Math.cos(angle)+ dim
#    end
#end