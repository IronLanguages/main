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

from System import *
from System import Windows
from System.Windows import Media
from System.Windows import Shapes

from dialogutil import InfoPopup
from filestruct import DiskFile, DiskDir

### Draw a graphic representation of a directory usage with vertical rectangles
class VerticalRectanglesRenderer(object):
	def __init__(self, canvas):
		self.popup = InfoPopup()
		self.brushFiles = []
		self.canvas = canvas
		self.canvas.MouseEnter += self.on_MouseEnter
		self.canvas.MouseLeave += self.on_MouseLeave
		self.canvas.MouseMove += self.on_MouseMove
	
	# Modify already present shapes on the canvas to fit in the new canvas size
	def UpdateDimensions(self):
		# Leave 1/8 of the canvas clear each side
		startX = self.canvas.ActualWidth / 8.0
		width = startX * 6.0
		# Leave 1/20 of the canvase clear at top and bottom
		startY = self.canvas.ActualHeight / 20.0
		endY = startY * 19.0
		gHeight = endY - startY
		
		lastY = startY
		for shape in self.canvas.Children:
			rHeight = float(shape.Tag.size) / shape.Tag.parent.size * gHeight
			shape.Data.Rect = Windows.Rect(startX, lastY, width , rHeight)
			lastY += rHeight
		
	# Create the graphic using RectangleGeometries and ajust DiskFile brush color (in the file list)
	def UpdateFiles(self, diskDir):
		# Leave 1/8 of the canvas clear each side
		startX = self.canvas.ActualWidth / 8.0
		width = startX * 6.0
		# Leave 1/20 of the canvase clear at top and bottom
		startY = self.canvas.ActualHeight / 20.0
		endY = startY * 19.0
		gHeight = endY - startY

		# Clear previous files icons
		for f in self.brushFiles:
			f.drawing.Brush = None
		self.brushFiles = []
		
		lastY = startY
		self.canvas.Children.Clear()
		for f in diskDir.files:
			# Create a Path shape
			p = Shapes.Path()
			p.Fill = GetRandomBrush()
			p.Stroke = Media.Brushes.Black
			p.StrokeThickness = 1.25
			self.canvas.Children.Add(p)

			# Adjust the file icon brush in the list
			f.drawing.Brush = p.Fill
			self.brushFiles.append(f)
			
			# Create the rectangle
			r = Media.RectangleGeometry()
			r.RadiusX = 2.5
			r.RadiusY = 2.5
			
			# Set the size of the rectangle
			rHeight = float(f.size) / diskDir.size * gHeight
			r.Rect = Windows.Rect(startX, lastY, width , rHeight)
			lastY += rHeight
			p.Data = r
			p.MouseEnter += self.on_RectMouseEnter
			# Set the DiskFile as the shape's Tag
			p.Tag = f
			
	# On mouse enter, show the info popup
	def on_MouseEnter(self, sender, eventArgs):
		self.popup.Show()
		
	# On mouse leave, hide the info popup
	def on_MouseLeave(self, sender, eventArgs):
		self.popup.Hide()

	# When the mouse enter a file rectangle, update the info popup data
	def on_RectMouseEnter(self, sender, eventArgs):
		fileName = eventArgs.Source.Tag.name
		fileSize = eventArgs.Source.Tag.GetSize() / 1048576.0
		parentSize = eventArgs.Source.Tag.parent.GetSize() / 1048576.0
		percent = 100.0 * fileSize / parentSize
		if isinstance(eventArgs.Source.Tag, DiskDir):
			dirTag = ' \t[dir]'
		else:
			dirTag = ''
		
		self.popup.ClearText()
		self.popup.AddBoldText(fileName)
		self.popup.AddText(
			"%(dirtag)s\n%(size).2f MiB out of %(parentSize).2f MiB\n%(percent).2f %% of parent directory" 
			% {'dirtag' : dirTag, 'size' : fileSize, 'parentSize' : parentSize, 'percent' : percent}
			)
		
	# On mouse move, move the info popup around
	def on_MouseMove(self, sender, eventArgs):
		# Use Windows Forms to get screen mouse position
		pos = Windows.Forms.Cursor.Position
		self.popup.SetPosition(pos.X + 15, pos.Y + 20)


rnd = Random()
colorProps = []
# Keep a list of all static brushes defined in Media.Brushes, and return a random one
def GetRandomBrush():
	global rnd, colorProps
	# If list is empty, directly store Property object in the list
	if len(colorProps) == 0:
		colorProps = list(Type.GetProperties(Media.Brushes))
	# Pick a random Brush property and return its value
	return colorProps.pop( rnd.Next() % len(colorProps) ).GetValue(None, None)

