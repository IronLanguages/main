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

from fmsynthlib import *

import clr
clr.AddReferenceByPartialName("System.Windows.Forms")
clr.AddReferenceByPartialName("System.Drawing")
from System.Windows.Forms import *
from System.Drawing import *

# keep the form controls global, for convenience
tv = TreeView()
tbFreq = TextBox()
tbRatio = TextBox()
sbAmp = HScrollBar()
sbVolume = VScrollBar()
lblAmp = Label(Text = "1.00")
chkRatio = CheckBox()
cmbType = ComboBox()
tbBaseFreq = TextBox()
piano = PictureBox()
lblGen = Label(Text = "Generating wave data...")
bp = BufferPlayer()

# called when a wave source changes
# update the wave source description and clear the buffers
def wsChanged():
	if tv.SelectedNode != None:
		tv.SelectedNode.Text = tv.SelectedNode.Tag.toString()
	bp.clearBuffers()

# called when the selected wave source changes
# update all of the form controls with the new wave source
def nodeSelected(s,e):
	if tv.SelectedNode == None:
		return
	ws = tv.SelectedNode.Tag
	tbFreq.Text = int(ws.freq).ToString()
	sbAmp.Value = int(ws.ampl * 100)
	chkRatio.Checked = ws.useRatio
	tbRatio.Text = ws.ratio.ToString()
	cmbType.Text = ws.sourceType()

# enable/disable a wave source
def nodeChecked(s,e):
	e.Node.Tag.enabled = e.Node.Checked
	wsChanged()

# change the freq of a wave source
def changeFreq(s,e):
	if tv.SelectedNode == None:
		return
	try:
		if tv.SelectedNode.Tag.freq == float(tbFreq.Text):
			return
		tv.SelectedNode.Tag.freq = float(tbFreq.Text)
	except:
		pass
	wsChanged()

# change the ampl of a wave source
def changeAmp(s,e):
	if tv.SelectedNode == None:
		return
	lblAmp.Text = tv.SelectedNode.Tag.ampl.ToString()
	if tv.SelectedNode.Tag.ampl == sbAmp.Value / 100.0:
		return
	tv.SelectedNode.Tag.ampl = sbAmp.Value / 100.0
	lblAmp.Text = tv.SelectedNode.Tag.ampl.ToString()
	wsChanged()

# change the global volume
def changeVolume(s,e):
	bp.setVolume(-sbVolume.Value)

# change the type of a wave source
# by constructing a new wave source and copying the old source's values
def changeWaveType(s,e):
	if tv.SelectedNode == None:
		return
	
	if tv.SelectedNode.Tag.sourceType() == cmbType.Text:
		return
	
	if tv.SelectedNode.Parent != None:
		tv.SelectedNode.Parent.Tag.removeMod(tv.SelectedNode.Tag)
	else:
		bp.removeWaveSource(tv.SelectedNode.Tag)
		
	if cmbType.Text == "sine":
		tv.SelectedNode.Tag = SineSource().copy(tv.SelectedNode.Tag)
	elif cmbType.Text == "square":
		tv.SelectedNode.Tag = SquareSource().copy(tv.SelectedNode.Tag)
	elif cmbType.Text == "sawtooth":
		tv.SelectedNode.Tag = SawSource().copy(tv.SelectedNode.Tag)
	elif cmbType.Text == "triangle":
		tv.SelectedNode.Tag = TriangleSource().copy(tv.SelectedNode.Tag)

	if tv.SelectedNode.Parent != None:
		tv.SelectedNode.Parent.Tag.addMod(tv.SelectedNode.Tag)
	else:
		bp.addWaveSource(tv.SelectedNode.Tag)

	wsChanged()

# change the ratio of a wave source
def changeRatio(s,e):
	if tv.SelectedNode == None:
		return
	try:
		if tv.SelectedNode.Tag.ratio == float(tbRatio.Text):
			return
		tv.SelectedNode.Tag.ratio = float(tbRatio.Text)
	except:
		pass
	wsChanged()

# change whether a wave source uses freq or ratio
def changeUseRatio(s,e):
	if chkRatio.Checked:
		tbFreq.Enabled = False
		tbRatio.Enabled = True		
	else:
		tbFreq.Enabled = True
		tbRatio.Enabled = False
	if tv.SelectedNode == None:
		return
	if tv.SelectedNode.Tag.useRatio == chkRatio.Checked:
		return
	tv.SelectedNode.Tag.useRatio = chkRatio.Checked
	wsChanged()

# remove a wave source
def removeNode(b,e):
	if tv.SelectedNode == None:
		return
	parent = tv.SelectedNode.Parent
	if parent == None:
		bp.removeWaveSource(tv.SelectedNode.Tag)
		tv.Nodes.Remove(tv.SelectedNode)
	else:
		parent.Tag.removeMod(tv.SelectedNode.Tag)
		tv.Nodes.Remove(tv.SelectedNode)
	wsChanged()

# add a new carrier wave source
def addCarrier(b,e):
	n = tv.Nodes.Add("Carrier")
	n.Tag = SineSource()
	n.Tag.isCarrier = True
	bp.addWaveSource(n.Tag)
	tv.SelectedNode = n
	n.Text = n.Tag.toString()
	n.Checked = True
	wsChanged()

# add a new modulator wave source
def addModulator(b,e):
	if tv.SelectedNode == None:
		return
	n = tv.SelectedNode.Nodes.Add("Modulator")
	n.Tag = SineSource()
	n.Tag.useRatio = True
	tv.SelectedNode.Tag.addMod(n.Tag)
	tv.SelectedNode.Expand()
	n.Text = n.Tag.toString()
	n.Checked = True
	wsChanged()

# play a buffer at current base frequency
def play(b,e):
	try:
		basefreq = float(tbBaseFreq.Text)
	except:
		return
	if bp.isPlaying(basefreq):
		return
	lblGen.Visible = True
	lblGen.Refresh()
	bp.play(basefreq)
	paint(None,None)
	lblGen.Visible = False

# draw the last generated waveform
def paint(s,e):	
	g = f.CreateGraphics()
	g.Clear(f.BackColor)
	lasty = bp.drawdata[0]
	for x,y in enumerate(bp.drawdata):
		g.DrawLine(Pen(Color.Black),x+300,lasty+210,x+301,y+210)
		lasty = y

# stop the buffer at the current base frequency
def stop(b,e):
	try:
		basefreq = float(tbBaseFreq.Text)
	except:
		return
	bp.stop(basefreq)

# stop all of the buffers
def stopall(b,e):
	bp.stopall()

# recursively build the nodes for the TreeView control
def buildTree(n):
	for mod in n.Tag.mods:
		c = n.Nodes.Add("Modulator")
		c.Tag = mod
		c.Text = c.Tag.toString()
		c.Checked = c.Tag.enabled
		buildTree(c)

# load a synth from an XML file
def load(b,e):
	ofd = OpenFileDialog()
	ofd.Filter = "XML files (*.xml)|*.xml"
	ofd.InitialDirectory = r"C:\Oscon\Demos\fmsynth\synths"
	if ofd.ShowDialog() != DialogResult.OK:
		return
	
	tv.Nodes.Clear()
	bp.readXML(ofd.FileName)
	for ws in bp.sources:
		n = tv.Nodes.Add("Carrier")
		n.Tag = ws
		n.Text = n.Tag.toString()
		n.Checked = n.Tag.enabled
		buildTree(n)
	tv.SelectedNode = n

# save a synth to an XML file
def save(b,e):
	sfd = SaveFileDialog()
	sfd.Filter = "XML files (*.xml)|*.xml"
	if sfd.ShowDialog() != DialogResult.OK:
		return
	
	bp.writeXML(sfd.FileName)

# given coordinates on the piano, determine which key was clicked
def findPianoNote(x,y):
	if y <= 40 and (x%20 <= 6 or x%20 >= 14):
		blackspaces = [24,19,14,7,2]
		note = int((x-14)/20)*2+1
		for i,n in enumerate(blackspaces):
			if note >= n+5-i:
				note -= 1
		if note > 0 and note not in blackspaces:
			return note
	whiteskips = [1,4,6,9,11,13,16,18,21,23]
	note = int(x/20)
	for n in whiteskips:
		if note >= n:
			note += 1
	return note

# convert a piano note number to a frequency
def noteToFreq(note):
	return 220*pow(2,float(note)/12)

# called when the piano is clicked
def pianoClick(p,e):
	tbBaseFreq.Text = noteToFreq(findPianoNote(e.X,e.Y)).ToString()
	if e.Button == MouseButtons.Left:
		stopall(None,None)	
	play(None,None)

# create all of the controls and add them to the form
def createUI():
	f.Text = "Frequency Modulation Synthesizer"
	f.Width = 640
	f.Height = 480
	f.FormBorderStyle = FormBorderStyle.FixedSingle
	f.MaximizeBox = False
	f.Paint += paint
	
	tv.Width = 294
	tv.Height = 420
	tv.Font = Font(FontFamily.GenericSansSerif, 10)
	tv.HideSelection = False
	tv.CheckBoxes = True
	tv.AfterSelect += nodeSelected
	tv.AfterCheck += nodeChecked
	addCarrier(tv,None)
	f.Controls.Add(tv)

	b = Button()
	b.Text = "Add Carrier"
	b.Height = 30
	b.Top = 420
	b.Width = 98
	b.Click += addCarrier
	f.Controls.Add(b)
	
	b = Button()
	b.Text = "Add Modulator"
	b.Height = 30
	b.Top = 420
	b.Width = 98
	b.Left = 98
	b.Click += addModulator
	f.Controls.Add(b)
	
	b = Button()
	b.Text = "Remove Source"
	b.Height = 30
	b.Top = 420
	b.Left = 196
	b.Width = 98
	b.Click += removeNode
	f.Controls.Add(b)
	
	b = Button()
	b.Text = "Load Synth"
	b.Height = 30
	b.Top = 420
	b.Left = 300
	b.Width = 70
	b.Click += load
	f.Controls.Add(b)
	
	b = Button()
	b.Text = "Save Synth"
	b.Height = 30
	b.Top = 420
	b.Left = 370
	b.Width = 70
	b.Click += save
	f.Controls.Add(b)
	
	l = Label(Text = "Frequency")
	l.AutoSize = True
	l.Left = 300
	l.Top = 8
	f.Controls.Add(l)
	
	tbFreq.Text = "440"
	tbFreq.Left = 360
	tbFreq.Top = 5
	tbFreq.Width = 40
	tbFreq.TextAlign = HorizontalAlignment.Center
	tbFreq.TextChanged += changeFreq
	f.Controls.Add(tbFreq)
	
	l = Label(Text = "Amplitude")
	l.AutoSize = True
	l.Left = 300
	l.Top = 48
	f.Controls.Add(l)
	
	sbAmp.Left = 360
	sbAmp.Top = 45
	sbAmp.Width = 240
	sbAmp.Height = 16
	sbAmp.Maximum = 100 + sbAmp.LargeChange - 1
	sbAmp.Minimum = 0
	sbAmp.Value = 100
	sbAmp.ValueChanged += changeAmp
	f.Controls.Add(sbAmp)
	
	lblAmp.AutoSize = True
	lblAmp.Left = 600
	lblAmp.Top = 46
	f.Controls.Add(lblAmp)
	
	chkRatio.Left = 410
	chkRatio.Top = 3
	chkRatio.Width = 70
	chkRatio.Text = "Use ratio"
	chkRatio.Enabled = True
	chkRatio.CheckedChanged += changeUseRatio
	f.Controls.Add(chkRatio)
	
	tbRatio.Text = "1"
	tbRatio.Left = 480
	tbRatio.Top = 5
	tbRatio.Width = 40
	tbRatio.TextAlign = HorizontalAlignment.Center
	tbRatio.Enabled = False
	tbRatio.TextChanged += changeRatio
	f.Controls.Add(tbRatio)
	
	l = Label(Text = ":1")
	l.AutoSize = True
	l.Left = 520
	l.Top = 8
	f.Controls.Add(l)
	
	cmbType.Left = 550
	cmbType.Top = 3
	cmbType.Width = 80
	cmbType.DropDownStyle = ComboBoxStyle.DropDownList
	cmbType.Items.Add("sine")
	cmbType.Items.Add("square")
	cmbType.Items.Add("sawtooth")
	cmbType.Items.Add("triangle")
	cmbType.SelectedIndex = 0
	cmbType.SelectedValueChanged += changeWaveType
	f.Controls.Add(cmbType)
	
	l = Label(Text = "Test with frequency")
	l.AutoSize = True
	l.Left = 300
	l.Top = 80
	f.Controls.Add(l)
	
	tbBaseFreq.Text = "440"
	tbBaseFreq.Left = 404
	tbBaseFreq.Top = 78
	tbBaseFreq.Width = 40
	tbBaseFreq.TextAlign = HorizontalAlignment.Center
	f.Controls.Add(tbBaseFreq)
	
	b = Button()
	b.Text = "Play"
	b.Height = 24
	b.Top = 76
	b.Left = 450
	b.Width = 50
	b.Click += play
	f.Controls.Add(b)
	
	b = Button()
	b.Text = "Stop"
	b.Height = 24
	b.Top = 76
	b.Left = 500
	b.Width = 50
	b.Click += stop
	f.Controls.Add(b)
	
	b = Button()
	b.Text = "Stop All"
	b.Height = 24
	b.Top = 76
	b.Left = 550
	b.Width = 70
	b.Click += stopall
	f.Controls.Add(b)
	
	l = Label(Text = "Volume")
	l.AutoSize = True
	l.Left = 590
	l.Top = 104
	f.Controls.Add(l)
	
	sbVolume.Left = 604
	sbVolume.Top = 120
	sbVolume.Width = 16
	sbVolume.Height = 296
	sbVolume.Maximum = 8000 + sbVolume.LargeChange - 1
	sbVolume.Minimum = 0
	sbVolume.Value = 1000
	sbVolume.LargeChange = 500
	sbVolume.SmallChange = 100
	sbVolume.ValueChanged += changeVolume
	f.Controls.Add(sbVolume)
	
	piano.Load("piano.bmp")
	piano.Left = 300
	piano.Top = 121
	piano.Width = 301
	piano.Height = 80
	piano.MouseClick += pianoClick
	f.Controls.Add(piano)

	l = Label(Text = "left click on piano for a single note, right click for sustain")
	l.AutoSize = True
	l.Left = 300
	l.Top = 104
	f.Controls.Add(l)

	lblGen.Left = 460
	lblGen.Top = 426
	lblGen.AutoSize = True
	lblGen.Font = Font(FontFamily.GenericSansSerif, 10)
	lblGen.ForeColor = Color.Red
	lblGen.Visible = False
	f.Controls.Add(lblGen)
	
# create the form
f = Form()
# initialize the sound system
fmsynth_init(f)
# create the user interface
createUI()
# start the form
Application.Run(f)
