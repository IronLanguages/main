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

import clr
clr.AddReference("Microsoft.DirectX.DirectSound")
clr.AddReference("System.Xml")
from Microsoft.DirectX.DirectSound import *
from System import *
from System.Xml import *
from math import *

# create a global DirectSound device
dev = Device()

# given a wave source type, create a wave source
# used when reading in XML files
def createWS(wstype):
	ws = None
	if wstype == "sine":
		ws = SineSource()
	elif wstype == "square":
		ws = SquareSource()
	elif wstype == "sawtooth":
		ws = SawSource()
	elif wstype == "triangle":
		ws = TriangleSource()
	return ws

# Euclid's algorithm
# returns the greatest common divisor of p and q	
def gcd(p,q):
	if p < 1.0:
		return 1.0
	if abs(q) < 0.01:
		return p
	return gcd(q,p % q)

# returns the greatest common divisor of a list of numbers	
def gcdlist(l):
	p = l.pop()
	while len(l) != 0:
		q = l.pop()
		p = gcd(p,q)
	return p

# the base class of all wave types
class WaveSource:
	
	# read a wave source from an XML file
	def readXML(self,nodes):
		for node in nodes:
			ws = createWS(node.Name)
			if ws != None:
				ws.readXML(node.ChildNodes)
				self.mods.append(ws)
			elif node.Name == "freq":
				self.freq = float(node.FirstChild.Value)
			elif node.Name == "ampl":
				self.ampl = float(node.FirstChild.Value)
			elif node.Name == "isCarrier":
				self.isCarrier = (node.FirstChild.Value == "True")
			elif node.Name == "ratio":
				self.ratio = float(node.FirstChild.Value)
			elif node.Name == "useRatio":
				self.useRatio = (node.FirstChild.Value == "True")
			elif node.Name == "enabled":
				self.enabled = (node.FirstChild.Value == "True")				
	
	# write a wave source to an XML file
	def writeXML(self,tw):
		tw.WriteStartElement(self.sourceType())
		tw.WriteElementString("freq",str(self.freq))
		tw.WriteElementString("ampl",str(self.ampl))
		tw.WriteElementString("isCarrier",str(self.isCarrier))
		tw.WriteElementString("ratio",str(self.ratio))
		tw.WriteElementString("useRatio",str(self.useRatio))
		tw.WriteElementString("enabled",str(self.enabled))
		for mod in self.mods:
			mod.writeXML(tw)
		tw.WriteEndElement()
	
	# a base WaveSource should never be instantiated
	def sourceType(self):
		return "none"
	
	# this is the description string that appears in the TreeView control
	def toString(self):
		if self.isCarrier:
			s = "Carrier ("
		else:
			s = "Modulator ("
		s += self.sourceType() + " "
		if self.useRatio:
			s += str(self.ratio) + ":1"
		else:
			s += str(self.freq) + "Hz" 
		s += " ampl=" + str(self.ampl) + ")"
		return s
	
	# add a modulator to this wave source
	def addMod(self,mod):
		self.mods.append(mod)
	
	# remove a modulator from this wave source
	def removeMod(self,mod):
		self.mods.remove(mod)
	
	# generate the wave data for all of our modulators
	def prepModData(self,samples,inc,sps):
		self.moddata = []
		for mod in self.mods:
			if mod.enabled:
				self.moddata.append(mod.genData(samples,inc,sps))
	
	# combine and return the sum of the modulators at sample i
	def addModData(self,i):
		v = 1
		for md in self.moddata:
			v = v + md[i]
		return v
		
	# calculates the frequencies for all modulator ratio sources
	# and returns a list of all the frequencies
	def findAllFreq(self):
		for mod in self.mods:
			if mod.useRatio:
				mod.freq = self.freq * mod.ratio
		freqs = []
		for ws in self.mods:
			freqs += ws.findAllFreq()
		return [self.freq] + freqs
	
	# given a data array with one cycle filled in
	# copy that cycle to fill the rest of the buffer
	def fillData(self,cycleLen,samples,data):
		i = cycleLen
		while i < samples:
			if cycleLen > samples - i:
				len = samples - i
			else:
				len = cycleLen
			Array.Copy(data,0,data,int(i),int(len))
			i += cycleLen
	
	# initialize to default values
	def __init__(self):
		self.freq = 440
		self.ampl = 0.8
		self.mods = []
		self.isCarrier = False
		self.ratio = 1
		self.useRatio = True
		self.enabled = True
	
	# copy another wave source's values to this wave source
	# useful when changing wave type
	def copy(self,ws):
		self.freq = ws.freq
		self.ampl = ws.ampl
		self.mods = ws.mods
		self.isCarrier = ws.isCarrier
		self.ratio = ws.ratio
		self.useRatio = ws.useRatio
		self.enabled = ws.enabled
		return self

# a sine wave source
class SineSource(WaveSource):
	def sourceType(self):
		return "sine"
		
	def genData(self,samples,inc,sps):
		self.prepModData(samples,inc,sps)
		data = Array.CreateInstance(float,samples)
		cycleLen = float(sps)/gcdlist(self.findAllFreq())
		c = 0
		for i in range(int(cycleLen)):
			data[i] = self.ampl * sin(c * 2 * pi)
			c = c + inc * self.freq * self.addModData(i)
		self.fillData(cycleLen,samples,data)
		return data

# a square wave source
class SquareSource(WaveSource):
	def sourceType(self):
		return "square"
		
	def genData(self,samples,inc,sps):
		self.prepModData(samples,inc,sps)
		data = Array.CreateInstance(float,samples)
		cycleLen = float(sps)/gcdlist(self.findAllFreq())
		p = 1.0
		c = 0
		for i in range(int(cycleLen)):
			data[i] = p * self.ampl
			c = c + 2 * inc * self.freq * self.addModData(i)
			if int(c) % 2 == 0:
				p = 1.0
			else:
				p = -1.0
		self.fillData(cycleLen,samples,data)
		return data

# a sawtooth wave source
class SawSource(WaveSource):
	def sourceType(self):
		return "sawtooth"
		
	def genData(self,samples,inc,sps):
		self.prepModData(samples,inc,sps)
		data = Array.CreateInstance(float,samples)
		cycleLen = float(sps)/gcdlist(self.findAllFreq())
		c = -1.0
		for i in range(int(cycleLen)):
			data[i] = self.ampl * c
			c = c + 2 * inc * self.freq * self.addModData(i)
			if c > 1:
				c = c - 2
		self.fillData(cycleLen,samples,data)
		return data

# a triangle wave source
class TriangleSource(WaveSource):
	def sourceType(self):
		return "triangle"
		
	def genData(self,samples,inc,sps):
		self.prepModData(samples,inc,sps)
		data = Array.CreateInstance(float,samples)
		cycleLen = float(sps)/gcdlist(self.findAllFreq())
		m = 2
		c = 0
		for i in range(int(cycleLen)):
			data[i] = self.ampl * c
			c = c + m * 2 * inc * self.freq * self.addModData(i)
			if c > 1:
				c -= 2 * (c - 1.0)
				m = -2
			elif c < -1:
				c -= 2 * (c + 1.0)
				m = 2
		self.fillData(cycleLen,samples,data)
		return data

# manages DirectSound buffers
class BufferPlayer:

	# load a set of wave sources from an XML file
	def readXML(self,filename):
		self.sources = []
		tr = XmlDocument()
		tr.Load(filename)
		for root in tr.ChildNodes:
			if root.Name == "BufferPlayer":
				for node in root.ChildNodes:
					ws = createWS(node.Name)
					if ws != None:
						ws.readXML(node.ChildNodes)
						self.sources.append(ws)
		self.clearBuffers()
	
	# write a set of wave sources to an XML file
	def writeXML(self,filename):
		tw = XmlTextWriter(filename,None)
		tw.Formatting = Formatting.Indented
		tw.WriteStartDocument()
		tw.WriteStartElement("BufferPlayer")
		for ws in self.sources:
			ws.writeXML(tw)
		tw.WriteEndElement()
		tw.WriteEndDocument()
		tw.Close()

	# checks if the buffer at given frequency is playing
	def isPlaying(self,basefreq):
		if basefreq not in self.bufs:
			return False
		return self.bufs[basefreq].Status.Playing
	
	# play a buffer at given frequency
	def play(self,basefreq):
		if len(self.sources) == 0:
			return
		self.addBuffer(basefreq).Play(0, BufferPlayFlags.Looping)
		return self.drawdata
	
	# stop the buffer at given frequency
	def stop(self,basefreq):
		if basefreq not in self.bufs:
			return
		self.bufs[basefreq].Stop()
	
	# stop all of the buffers
	def stopall(self):
		for buf in self.bufs.values():
			buf.Stop()
	
	# add a wave source
	def addWaveSource(self,ws):
		self.sources.append(ws)
	
	# remove a wave source
	def removeWaveSource(self,ws):
		self.sources.remove(ws)
	
	# generate the audio data
	def genWaveSources(self,buf):
		# the number of bytes per audio sample
		bytesPerSample = int(buf.Format.BitsPerSample) / 8
		# total number of samples to be generated
		samples = int(buf.Caps.BufferBytes) / bytesPerSample
		# samples per second
		sps = buf.Format.AverageBytesPerSecond / bytesPerSample
		# inc is duration of a sample, in fractions of a second
		inc = 1.0 / sps
		
		# generate audio data from each wave source
		datas = [ws.genData(samples,inc,sps) for ws in self.sources if ws.enabled]

		# we combine the sources in two passes
		# in the first pass we sum each simultaneous sample together
		# we also find the peak (maxval) of the whole buffer
		mixeddata = Array.CreateInstance(float,samples)
		maxval = 0.0
		for i in range(samples):
			mixeddata[i] = sum([d[i] for d in datas])
			if abs(mixeddata[i]) > maxval:
				maxval = abs(mixeddata[i])
		
		realdata = Array.CreateInstance(UInt16,samples)
		
		# record some of the data in a form suitable for drawing
		self.drawdata = Array.CreateInstance(int,300)
		drawinc = float(samples)/300
		drawnext = 0
		lastfilled = 0
		
		# if the maximum is zero, we have no audio data
		# to avoid division by zero we just return an empty buffer
		if maxval == 0.0:
			Array.Clear(realdata,0,samples)
			buf.Write(0,realdata,LockFlag.EntireBuffer)
			return
		
		# in the second pass we convert the data from floating point to UInt16
		# we also normalize using maxval as we go
		# we also fill in the drawdata
		for i,v in enumerate(mixeddata):
			if v >= 0:
				realdata[i] = int((v/maxval) * 32767)
			else:
				realdata[i] = int((v/maxval) * 32767 + 65536)
			if i > drawnext:
				while lastfilled <= int(i/drawinc):
					self.drawdata[lastfilled] = int(((v/maxval) + 1)*100)
					lastfilled += 1
				drawnext += drawinc
		
		while lastfilled < 300:
			self.drawdata[lastfilled] = self.drawdata[lastfilled-1]
			lastfilled += 1

		# write the final data to the buffer
		buf.Write(0, realdata, LockFlag.EntireBuffer)
	
	# finds the greatest common divisor of all the wave source frequencies
	def findGCDFreq(self,basefreq):
		freqs = []
		for ws in self.sources:
			if ws.useRatio:
				ws.freq = basefreq * ws.ratio
			freqs += ws.findAllFreq()
		return gcdlist(freqs)
	
	# called when a wave source property has changed
	# we have to throw out all cached buffers
	def clearBuffers(self):
		self.stopall()
		self.bufs.clear()
		
	# set the global volume for all buffers
	def setVolume(self,volume):
		self.volume = volume
		for buf in self.bufs.values():
			buf.Volume = volume
	
	# create a new buffer at this frequency if needed
	def addBuffer(self,basefreq):
		# if a buffer at this frequency already exists then return it
		if basefreq in self.bufs:
			return self.bufs[basefreq]
		
		# the format we use is mono 44.1kHz 16-bit audio
		wf = WaveFormat()
		wf.AverageBytesPerSecond = 88200
		wf.BitsPerSample = 16
		wf.BlockAlign = 2
		wf.Channels = 1
		wf.FormatTag = WaveFormatTag.Pcm
		wf.SamplesPerSecond = 44100
		
		bd = BufferDescription(wf)
		# the longer the buffer, the longer it takes to generate
		# so we make the buffer just long enough to contain one whole cycle
		# by finding the greatest common divisor of all wave source frequencies
		bd.BufferBytes = int(wf.AverageBytesPerSecond / self.findGCDFreq(basefreq))
		bd.ControlVolume = True
		
		# create the actual buffer
		buf = SecondaryBuffer(bd, dev)
		# fill the buffer with data
		self.genWaveSources(buf)
		buf.Volume = self.volume
		self.bufs[basefreq] = buf
		return buf
	
	def __init__(self):
		self.sources = []
		self.bufs = {}
		self.volume = -1000
		self.drawdata = Array.CreateInstance(int,300)

# when the program is initialized, set the DirectSound cooperative level
def fmsynth_init(f):
	# Priority gives up the device when the user switches away from the program
	dev.SetCooperativeLevel(f, CooperativeLevel.Priority)

