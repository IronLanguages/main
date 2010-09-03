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

from System.IO import DirectoryInfo
from sys import stderr

### DiskFile is the main property class around a file in this app
class DiskFile(object):
	def __init__(self, name, size, parent):
		self.name = name
		self.size = size
		self.parent = parent
		self.drawing = None
		if parent != None:
			self.path = parent.path + self.name
		else:
			self.path = name
	
	# Compare files by their size
	def __cmp__(self, other):
		return cmp(self.size, getattr(other, 'size', None))
		
	def GetSize(self):
		return self.size

### A directory is the same as a file except that it has child files
class DiskDir(DiskFile):
	def __init__(self, name, size, parent):
		DiskFile.__init__(self, name, size, parent)
		self.path += '\\'
		self.files = []
		self.loaded = False
		
	# Load the direct child files (and dirs) in this directory and calculate their size
	def Load(self):
		if not self.loaded:
			self.files = []
			self.size = 0
			
			try:
				dirInfo = DirectoryInfo(self.path)
				for aFile in dirInfo.GetFiles():
					self.files.append( DiskFile(aFile.Name, aFile.Length, self) )
					self.size += aFile.Length
				for aDir in dirInfo.GetDirectories():
					dirSize = GetDirSize(aDir)
					self.files.append( DiskDir(aDir.Name, dirSize, self) )
					self.size += dirSize
				# Sort files by descending size
				self.files.sort(reverse=True)
				self.loaded = True
			except Exception, details:
				print >> stderr, "Error [%(details)s]" % {'details' : details}
				self.files = []
			
	def GetSize(self):
		if self.size != None:
			return self.size
		else:
			self.size = GetDirSize( DirectoryInfo(self.path) )
			return self.size

# Module function that recursively calculate the size of a directory
def GetDirSize(dirinfo):
	size = 0
	try:
		for afile in dirinfo.GetFiles():
			size += afile.Length
		for adir in dirinfo.GetDirectories():
			size += GetDirSize(adir)
	except Exception, details:
		print >> stderr, "Warning [%(details)s]" % {'details' : details}
	return size

