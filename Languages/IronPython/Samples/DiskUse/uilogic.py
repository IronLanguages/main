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

from System import Windows
from System.Windows import Controls
from System.Windows import Media
from System.Windows import Shapes
from System.Windows import Forms

from filestruct import DiskFile, DiskDir
from renderers import VerticalRectanglesRenderer
from dialogutil import LoadXaml


### Main class of the application, manage main window and different components
class IronDiskUsage(object):
	def __init__(self, app):
		self.app = app
		
		# Create window from Xaml
		self.window = LoadXaml("mainWindow.xaml")
		self.window.Closing += self.on_Closing
		self.windowTitle = self.window.Title
		
		# File list
		self.flKeeper = FileListKeeper(self)
		
		# Renderer
		usageCanvas = self.window.FindName('usageCanvas')
		self.renderer = VerticalRectanglesRenderer(usageCanvas)
		usageCanvas.SizeChanged += self.on_CanvasSizeChanged
		
		self.selectedFile = None
		self.window.Show()
		if not self.loadRoot():
			self.app.Shutdown()
		else:
			self.flKeeper.ExpandRoot()
		
	def on_Closing(self, sender, eventArgs):
		# To force quit event if a popup is visible
		self.app.Shutdown()
		
	# Keep the root c: DiskFile and add it to the file list
	def loadRoot(self):
		# Ask the user which folder to analyse
		folderDialog = Forms.FolderBrowserDialog()
		folderDialog.ShowNewFolderButton = False
		folderDialog.SelectedPath = "c:\\"
		folderDialog.Description = "Please select the folder to analyse"
		if folderDialog.ShowDialog() != Forms.DialogResult.OK:
			return False
		
		self.fsRoot = DiskDir(folderDialog.SelectedPath, None, None)
		self.flKeeper.Refresh(self.fsRoot)
		return True
		
	# Update renderer on resize
	def on_CanvasSizeChanged(self, sender, eventArgs):
		self.renderer.UpdateDimensions()
	
	def setLoading(self, value):
		if value:
			self.window.Title = self.windowTitle + " ...loading"
		else:
			self.window.Title = self.windowTitle
	Loading = property(fset=setLoading)
		

### Manage the file list
class FileListKeeper(object):
	def __init__(self, v):
		self.owner = v
		self.control = v.window.FindName('fileList')
		
	# Create the root item
	def Refresh(self, rootDir):
		self.control.Items.Clear()
		self.AddItem(self.control.Items, self.owner.fsRoot)
		self.control.Items[0].Expanded += self.on_Expanded
		
	# Start analysing the root element of the list
	def ExpandRoot(self):
		self.control.Items[0].IsExpanded = True
		
	# Generate child items
	def GenItems(self, fileItem):
		for diskFile in fileItem.Tag.files:
			self.AddItem(fileItem.Items, diskFile)
	
	# When an item gets expanded, calculate sizes and render the graphic
	def on_Expanded(self, sender, eventArgs):
		tvItem = eventArgs.Source
		if isinstance(tvItem.Tag, DiskDir):
			self.owner.Loading = True
			
			# Create list child items
			tvItem.Items.Clear()
			tvItem.Tag.Load()
			self.GenItems(tvItem)
			
			# Render the graphic
			self.owner.selectedFile = tvItem.Tag
			self.owner.renderer.UpdateFiles(tvItem.Tag)

			self.owner.Loading = False
			
	
	# Add a new TreeViewItem for a DiskFile inside a collection
	def AddItem(self, coll, diskFile):
		item = Controls.TreeViewItem()
		item.Header = self.MakeItemHeader(diskFile)
		item.Tag = diskFile
		if isinstance(diskFile, DiskDir):
			# Add this dummy item to get a "+" button. It will be removed in on_Expanded
			item.Items.Add('dummy')
			item.FontWeight = Windows.FontWeights.Bold
		else:
			item.FontWeight = Windows.FontWeights.Normal
		coll.Add(item)

	# Private method that generate one of those fancy tree view item header
	def MakeItemHeader(self, diskFile):
		# RectangleGeometry...
		r = Media.RectangleGeometry()
		r.Rect = Windows.Rect(0,0,15,15)
		r.RadiusX = 3
		r.RadiusY = 3

		# Inside a GeometryDrawing (and keep it in the DiskFile)...
		g = Media.GeometryDrawing()
		g.Geometry = r
		g.Pen = Media.Pen()
		g.Pen.Brush = Media.Brushes.Black
		# We will set the brush on render
		diskFile.drawing = g
		
		# Inside a DrawingImage...
		d = Media.DrawingImage()
		d.Drawing = g
		
		# Inside an Image...
		i = Controls.Image()
		i.Source = d
		
		# Inside a DockPanel with the name TextBlock beside it
		dock = Controls.DockPanel()
		dock.Children.Add(i)
		t = Controls.TextBlock()
		t.Text = ' ' + diskFile.name
		dock.Children.Add(t)
		return dock


