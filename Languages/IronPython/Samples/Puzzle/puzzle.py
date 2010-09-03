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
clr.AddReferenceByPartialName("System.Windows.Forms")
clr.AddReferenceByPartialName("System.Drawing")

from System.Windows.Forms import *
from System.Drawing import *

from quadkey import *
from loadpanel import *
from tiles import *
from copyright import *

class Puzzle(Form):

    def __init__(self):

        ### CREATE CONSTANTS ###
        self.TOP_BORDER = 20
        self.SIDE_BORDER = 10
        self.BOTTOM_BORDER = 60

        ### CREATE INSTANCE VARIABLES ###
        self.gridDimension = 3
        self.normalTileDimension = 128
        self.tileDimension = self.normalTileDimension
        self.grid = []
        for i in range(0, self.gridDimension):
            self.grid += [[]]
            for j in range(0, self.gridDimension):
                self.grid[i] += [None]
        self.gameInProgress = False
        self.clearCacheOnExit = False
        self.exitFromMenu = False
        
        self.savedSettings = LoadAllSavedSettings()
        self.savedPuzzles = self.savedSettings[0]
        self.previewState = self.savedSettings[1]
        
        LoadCache()
        if self.savedSettings[2] == True:
            TurnOnCache()
        else:
            TurnOffCache()
        
        ### SET UP MENU ###
        self.CreateMenu()

        ### SET UP PANELS ###
        self.CreatePanels()

        ### SET PROPERTIES OF THE FORM ###
        self.Text = "Puzzle"
        self.BackColor = Color.White
        w = self.gridDimension * self.tileDimension + 2 * self.SIDE_BORDER + self.menu.Size.Width
        h = self.gridDimension * self.tileDimension + self.TOP_BORDER + self.BOTTOM_BORDER
        self.ClientSize = Size(w, h)
        self.FormClosing += self.formClosing
        self.FormBorderStyle = FormBorderStyle.Fixed3D
        self.MaximizeBox = False
        self.KeyPreview = True

        ### PLACE WLL BANNER ###
        self.wllPicture = PictureBox()
        self.wllPicture.SizeMode = PictureBoxSizeMode.AutoSize
        import nt
        try:
            self.wllPicture.Image = Image.FromFile(nt.getcwd() + "\\banner.bmp")
        except:
            self.wllPicture.Image = None
        self.wllPicture.Location = Point(self.ClientSize.Width - self.wllPicture.Size.Width, self.ClientSize.Height - self.wllPicture.Size.Height)
        self.Controls.Add(self.wllPicture)
        
        try:
            self.Icon = Icon(nt.getcwd() + "\\icon.ico")
        except:
            print "Error loading icon.ico"
        
        ### SET UP KEYBOARD HANDLER ###
        self.KeyDown += self.keyDown

        ### SET DEFAULT GAME ###
        #self.SetBoard('a', 47.55, -122.322, 11, 3)
        self.SetBoard('a', 45.05, 14.26, 5, 3)


    def CreateMenu(self):

        self.menu = MenuStrip()
        self.menu.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow
        self.menu.BackColor = Color.White
        self.menu.Dock = DockStyle.None
        self.menu.Location = Point(0, self.TOP_BORDER)
        self.Controls.Add(self.menu)

        self.menuPlay = self.CreateMenuItem("play!")
        self.menuCreate = self.CreateMenuItem("create")
        self.menuLoad = self.CreateMenuItem("load")
        self.menuOptions = self.CreateMenuItem("options")
        self.menuAbout = self.CreateMenuItem("about")
        self.menuExit = self.CreateMenuItem("exit")
        
        self.menuPlay.BackColor = Color.CornflowerBlue
        self.menuPlay.ForeColor = Color.White

    def CreateMenuItem(self, text):
        item = ToolStripMenuItem()
        item.Text = text
        item.Size = Size(100, 100)
        item.Font = Font("Verdana", 8, FontStyle.Bold)
        item.Click += self.menuItemClicked
        self.menu.Items.Add(item)
        return item
        
    def CreateTile(self, i, j, type = None, quadkey = None):
        size = Size(self.tileDimension, self.tileDimension)
        tile = Tile(i, j, size, type, quadkey)
        tile.Location = self.GetLocation(i, j)
        self.panelBoard.Controls.Add(tile)
        tile.MouseDown += self.mouseDown
        tile.MouseUp += self.mouseUp
        tile.MouseMove += self.mouseMove
        self.grid[i][j] = tile
        return tile


    def ResetBoardForNewGame(self, dimension):
    
        ### clear all tiles
        self.panelBoard.Controls.Clear()

        ### reset grid of tiles
        self.gridDimension = dimension
        self.grid = []
        for i in range(0, self.gridDimension):
            self.grid += [[]]
            for j in range(0, self.gridDimension):
                self.grid[i] += [None]
        
        self.tileDimension = self.normalTileDimension
        if dimension == 4:
            self.tileDimension = 0.75 * self.tileDimension
            
        self.gameInProgress = False
        
    def CreateShuffleLink(self):
        self.linkShuffle = LinkLabel()
        self.linkShuffle.Font = Font("Verdana", 12, FontStyle.Bold);
        self.linkShuffle.LinkColor = Color.CornflowerBlue
        self.linkShuffle.Text = "Shuffle\nto\nStart"
        self.linkShuffle.TextAlign = ContentAlignment.MiddleCenter
        self.linkShuffle.Size = Size(self.tileDimension, self.tileDimension)
        self.linkShuffle.LinkBehavior = LinkBehavior.HoverUnderline
        self.linkShuffle.TabStop = False
        self.linkShuffle.Location = self.GetLocation(self.gridDimension - 1, self.gridDimension - 1)
        self.linkShuffle.Click += self.StartGame
        self.panelBoard.Controls.Add(self.linkShuffle)
        
    def PopulateCaptionPlay(self):
        self.panelBoard.Controls.Add(self.captionPlay)
        self.captionPlay.Text = GetCaption(self.currentGameState[0], self.currentGameState[3], (self.currentGameState[1] + 1, self.currentGameState[2] + 1))
        self.RelocateCaptionPlay(None, None)
        self.captionPlay.BringToFront()
    
    def PopulateCaptionCreate(self):
        self.captionCreate.Text = GetCaption(self.comboType.SelectedItem[0].lower(), self.scrollZoom.Value, self.previewTile[1][1].tile)
        self.RelocateCaptionCreate(None, None)
        self.captionCreate.BringToFront()

    def SetBoard(self, type, lat, lon, level, dimension):

        self.ResetBoardForNewGame(dimension)

        centerTile = LLToTile(lat, lon, level)
        
        self.currentGameState = [type, centerTile[0]-1, centerTile[1]-1, level, dimension]

        self.CreateTile(0, 0, type, TileToQuadkey((centerTile[0]-1, centerTile[1]-1), level))
        self.CreateTile(0, 1, type, TileToQuadkey((centerTile[0], centerTile[1]-1), level))
        self.CreateTile(0, 2, type, TileToQuadkey((centerTile[0]+1, centerTile[1]-1), level))
        self.CreateTile(1, 0, type, TileToQuadkey((centerTile[0]-1, centerTile[1]), level))
        self.CreateTile(1, 1, type, TileToQuadkey(centerTile, level))
        self.CreateTile(1, 2, type, TileToQuadkey((centerTile[0]+1, centerTile[1]), level))
        self.CreateTile(2, 0, type, TileToQuadkey((centerTile[0]-1, centerTile[1]+1), level))
        self.CreateTile(2, 1, type, TileToQuadkey((centerTile[0], centerTile[1]+1), level))

        if dimension == 4:
            self.CreateTile(0, 3, type, TileToQuadkey((centerTile[0]+2, centerTile[1]-1), level))
            self.CreateTile(1, 3, type, TileToQuadkey((centerTile[0]+2, centerTile[1]), level))
            self.CreateTile(2, 2, type, TileToQuadkey((centerTile[0]+1, centerTile[1]+1), level))
            self.CreateTile(2, 3, type, TileToQuadkey((centerTile[0]+2, centerTile[1]+1), level))
            self.CreateTile(3, 0, type, TileToQuadkey((centerTile[0]-1, centerTile[1]+2), level))
            self.CreateTile(3, 1, type, TileToQuadkey((centerTile[0], centerTile[1]+2), level))
            self.CreateTile(3, 2, type, TileToQuadkey((centerTile[0]+1, centerTile[1]+2), level))
            
        self.CreateShuffleLink()
        self.PopulateCaptionPlay()
        
    def SetBoardFromCreate(self):
    
        dimension = 3
        if self.radio4x4.Checked is True:
            dimension = 4
        self.ResetBoardForNewGame(dimension)
        
        if self.comboType.SelectedItem == "Road":
            type = "r"
        elif self.comboType.SelectedItem == "Aerial":
            type = "a"
        elif self.comboType.SelectedItem == "Hybrid":
            type = "h"
            
        level = self.scrollZoom.Value
        
        centerTile = self.previewTile[0][0].tile
        
        self.SetBoardByTile(type, centerTile, level, dimension)
        
    def SetBoardByTile(self, type, tile, level, dimension):
    
        self.ResetBoardForNewGame(dimension)
        
        self.currentGameState = [type, tile[0], tile[1], level, dimension]
        
        for i in range(0, dimension):
            for j in range(0, dimension):
                if i != (dimension - 1) or j != (dimension - 1):
                    self.CreateTile(i, j, type, TileToQuadkey((tile[0]+j, tile[1]+i), level))
        
        self.CreateShuffleLink()
        self.PopulateCaptionPlay()


    def StartGame(self, sender, e):
        self.gameInProgress = True
        self.Shuffle()
        self.linkShuffle.Visible = False
        
    def LoadGame(self, sender, e):
    
        idx = self.panelGames.selectedIndex
        if idx < 0 or idx >= len(self.panelGames.buttons):
            return
            
        if self.gameInProgress is True:
            dr = MessageBox.Show("Quit current game?", "", MessageBoxButtons.YesNo)
            if dr == DialogResult.No:
                return
        
        puzzle = self.savedPuzzles[idx]
        self.menuItemClicked(self.menuPlay, None)
        self.SetBoardByTile(puzzle[1], (puzzle[3], puzzle[2]), puzzle[4], puzzle[5])
        
    def ExitGame(self):
    
        if self.gameInProgress is True:
            dr = MessageBox.Show("Quit current game?", "", MessageBoxButtons.YesNo)
            if dr == DialogResult.No:
                return False
        
        if self.clearCacheOnExit is True:
            ClearCache()
        
        self.SaveSettings()
        
        self.exitFromMenu = True
        self.Close()
        
    def Shuffle(self):
    
        allTiles = []
        for i in range(0, self.gridDimension):
            for j in range(0, self.gridDimension):
                if not self.grid[i][j] is None:
                    allTiles += [self.grid[i][j]]
        allTiles += [None]    # dummy tile, used for empty space
        
        from System import Random
        rand = Random()
        for i in range(0, self.gridDimension):
            for j in range(0, self.gridDimension):
                idx = rand.Next(len(allTiles))
                if allTiles[idx] is None:
                    self.grid[i][j] = None
                else:
                    self.grid[i][j] = allTiles[idx]
                    self.grid[i][j].currentRow = i
                    self.grid[i][j].currentCol = j
                    self.grid[i][j].Location = self.GetLocation(i, j)
                allTiles = allTiles[:idx] + allTiles[idx+1:]

    def GetLocation(self, i, j):
        x = j * self.tileDimension
        y = i * self.tileDimension
        return Point(x, y)
        
    def CheckGrid(self):
        for i in range(0, self.gridDimension):
            for j in range(0, self.gridDimension):
                if not self.grid[i][j] is None:
                    if not self.grid[i][j].isCorrect():
                        return False
        self.gameInProgress = False
        self.linkShuffle.Enabled = False
        self.linkShuffle.Text = "Nicely\ndone!"
        self.linkShuffle.Visible = True
        return True

    def PopulatePreviewTiles(self):
        x = self.previewState[0]
        y = self.previewState[1]
        level = self.previewState[3]
        type = self.comboType.SelectedItem
        
        for i in range(0, 4):
            for j in range(0, 4):
                tile = (x+j, y+i)
                quadkey = TileToQuadkey(tile, level)
                self.previewTile[i][j].quadkey = quadkey
                self.previewTile[i][j].tile = tile
                self.previewTile[i][j].Image = GetImage(type[0].lower(), quadkey)
        
        self.PopulateCaptionCreate()
    
    def SaveSettings(self):
        import clr
        clr.AddReference("System.Xml")
        from System.Xml import XmlDocument
        import nt
        
        xmldoc = XmlDocument()
        try:
            xmldoc.Load(nt.getcwd() + "\load.xml")
        except:
            print "Error reading load.xml"
            return
            
        nodelist = xmldoc.GetElementsByTagName("Cache")
        a = xmldoc.CreateAttribute("allow")
        if self.checkCache.Checked is True:
            a.Value = "true"
        else:
            a.Value = "false"
        nodelist.Item(0).Attributes.Append(a)
        
        nodelist = xmldoc.GetElementsByTagName("TopLeftPreviewTile")
        a = xmldoc.CreateAttribute("x")
        a.Value = str(self.previewTile[0][0].tile[0])
        nodelist.Item(0).Attributes.Append(a)
        a = xmldoc.CreateAttribute("y")
        a.Value = str(self.previewTile[0][0].tile[1])
        nodelist.Item(0).Attributes.Append(a)
        a = xmldoc.CreateAttribute("dimension")
        if self.radio3x3.Checked is True:
            a.Value = "3"
        else:
            a.Value = "4"
        nodelist.Item(0).Attributes.Append(a)
        a = xmldoc.CreateAttribute("level")
        a.Value = str(self.scrollZoom.Value)
        nodelist.Item(0).Attributes.Append(a)
        
        try:
            xmldoc.Save(nt.getcwd() + "\load.xml")
        except:
            print "Error writing load.xml"
    
    def SaveCurrentGame(self, caption):
        import clr
        clr.AddReference("System.Xml")
        from System.Xml import *
        import nt
        
        xmldoc = XmlDocument()
        try:
            xmldoc.Load(nt.getcwd() + "\load.xml")
        except:
            print "Error reading load.xml"
            return
            
        nodeSavedGames = xmldoc.GetElementsByTagName("SavedGames")
        nodeNewGame = xmldoc.CreateElement("Game")
        a = xmldoc.CreateAttribute("caption")
        a.Value = caption
        nodeNewGame.Attributes.Append(a)
        a = xmldoc.CreateAttribute("type")
        a.Value = self.currentGameState[0]
        nodeNewGame.Attributes.Append(a)
        a = xmldoc.CreateAttribute("x")
        a.Value = str(self.currentGameState[1])
        nodeNewGame.Attributes.Append(a)
        a = xmldoc.CreateAttribute("y")
        a.Value = str(self.currentGameState[2])
        nodeNewGame.Attributes.Append(a)
        a = xmldoc.CreateAttribute("level")
        a.Value = str(self.currentGameState[3])
        nodeNewGame.Attributes.Append(a)
        a = xmldoc.CreateAttribute("dimension")
        a.Value = str(self.currentGameState[4])
        nodeNewGame.Attributes.Append(a)
        nodeSavedGames[0].AppendChild(nodeNewGame)
        
        try:
            xmldoc.Save(nt.getcwd() + "\load.xml")
        except:
            print "Error writing load.xml"
        
    ### GUI HEAVY LIFTING ###
    
    def CreatePanels(self):
        
        self.panelBoard = Panel()
        self.panelBoard.Size = Size(self.gridDimension * self.tileDimension, self.gridDimension * self.tileDimension)
        self.panelBoard.Location = Point(self.menu.Size.Width + self.SIDE_BORDER, self.TOP_BORDER)
        self.panelBoard.BackColor = Color.White
        self.Controls.Add(self.panelBoard)
        
        self.panelCreate = Panel()
        self.panelCreate.Size = Size(self.gridDimension * self.tileDimension, self.gridDimension * self.tileDimension)
        self.panelCreate.Location = Point(self.menu.Size.Width + self.SIDE_BORDER, self.TOP_BORDER)
        self.panelCreate.BackColor = Color.LightSteelBlue
        self.Controls.Add(self.panelCreate)
        
        self.panelLoad = Panel()
        self.panelLoad.Size = Size(self.gridDimension * self.tileDimension, self.gridDimension * self.tileDimension)
        self.panelLoad.Location = Point(self.menu.Size.Width + self.SIDE_BORDER, self.TOP_BORDER)
        self.panelLoad.BackColor = Color.LightSteelBlue
        self.Controls.Add(self.panelLoad)
        
        self.panelOptions = Panel()
        self.panelOptions.Size = Size(self.gridDimension * self.tileDimension, self.gridDimension * self.tileDimension)
        self.panelOptions.Location = Point(self.menu.Size.Width + self.SIDE_BORDER, self.TOP_BORDER)
        self.panelOptions.BackColor = Color.LightSteelBlue
        self.Controls.Add(self.panelOptions)
        
        self.panelAbout = Panel()
        self.panelAbout.Size = Size(self.gridDimension * self.tileDimension, self.gridDimension * self.tileDimension)
        self.panelAbout.Location = Point(self.menu.Size.Width + self.SIDE_BORDER, self.TOP_BORDER)
        self.panelAbout.BackColor = Color.LightSteelBlue
        self.Controls.Add(self.panelAbout)
        
        self.panels = [self.panelBoard, self.panelCreate, self.panelLoad, self.panelOptions, self.panelAbout]
        
        ### create labels for captions
        
        self.captionPlay = Label()
        self.captionCreate = Label()
        self.captionPlay.AutoSize = self.captionCreate.AutoSize = True
        self.captionPlay.Font = self.captionCreate.Font = Font("Arial", 9)
        self.captionPlay.BackColor = self.captionCreate.BackColor = Color.White #Color.CornflowerBlue
        self.captionPlay.ForeColor = self.captionCreate.ForeColor = Color.Black #Color.White
        self.panelBoard.Controls.Add(self.captionPlay)
        self.panelCreate.Controls.Add(self.captionCreate)
        
        ### populate Create panel
        
        self.buttonDown = Button()
        self.buttonDown.BackColor = Color.White
        self.buttonDown.Anchor = AnchorStyles.Bottom
        self.buttonDown.FlatStyle = FlatStyle.Flat
        self.buttonDown.Size = Size(25, 25)
        x = self.panelCreate.Size.Width / 2 - self.buttonDown.Size.Width / 2
        y = self.panelCreate.Size.Height - self.buttonDown.Size.Height - 5
        self.buttonDown.Location = Point(x, y)
        self.buttonDown.MouseDown += self.arrowButtonMouseDown
        self.buttonDown.MouseUp += self.arrowButtonMouseUp
        self.buttonDown.MouseDown += self.previewMoveDown
        self.panelCreate.Controls.Add(self.buttonDown)
        
        self.buttonUp = Button()
        self.buttonUp.BackColor = Color.White
        self.buttonUp.Anchor = AnchorStyles.Bottom
        self.buttonUp.FlatStyle = FlatStyle.Flat
        self.buttonUp.Size = Size(25, 25)
        y = self.buttonDown.Location.Y - self.buttonDown.Height - 2
        self.buttonUp.Location = Point(x, y)
        self.buttonUp.MouseDown += self.arrowButtonMouseDown
        self.buttonUp.MouseUp += self.arrowButtonMouseUp
        self.buttonUp.MouseDown += self.previewMoveUp
        self.panelCreate.Controls.Add(self.buttonUp)
        
        self.buttonLeft = Button()
        self.buttonLeft.BackColor = Color.White
        self.buttonLeft.Anchor = AnchorStyles.Bottom
        self.buttonLeft.FlatStyle = FlatStyle.Flat
        self.buttonLeft.Size = Size(25, 25)
        x = self.buttonDown.Location.X - self.buttonDown.Size.Width - 2
        y = self.buttonDown.Location.Y - 0.5 * self.buttonDown.Size.Height
        self.buttonLeft.Location = Point(x, y)
        self.buttonLeft.MouseDown += self.arrowButtonMouseDown
        self.buttonLeft.MouseUp += self.arrowButtonMouseUp
        self.buttonLeft.MouseDown += self.previewMoveLeft
        self.panelCreate.Controls.Add(self.buttonLeft)
        
        self.buttonRight = Button()
        self.buttonRight.BackColor = Color.White
        self.buttonRight.Anchor = AnchorStyles.Bottom
        self.buttonRight.FlatStyle = FlatStyle.Flat
        self.buttonRight.Size = Size(25, 25)
        x = self.buttonDown.Location.X + self.buttonDown.Size.Width + 2
        self.buttonRight.Location = Point(x, y)
        self.buttonRight.MouseDown += self.arrowButtonMouseDown
        self.buttonRight.MouseUp += self.arrowButtonMouseUp
        self.buttonRight.MouseDown += self.previewMoveRight
        self.panelCreate.Controls.Add(self.buttonRight)
        
        self.scrollZoom = HScrollBar()
        self.scrollZoom.Anchor = AnchorStyles.Bottom
        w = 135
        self.scrollZoom.Size = Size(w, 25)
        self.scrollZoom.Location = Point(9, self.buttonLeft.Location.Y)
        self.scrollZoom.Minimum = 3
        self.scrollZoom.Maximum = 19
        self.scrollZoom.LargeChange = 1
        self.panelCreate.Controls.Add(self.scrollZoom)
        
        self.comboType = ComboBox()
        self.comboType.Font = Font("Verdana", 8)
        self.comboType.Size = Size(83, 21)
        x = self.scrollZoom.Location.X
        y = self.scrollZoom.Location.Y - self.comboType.Size.Height - 3
        self.comboType.Location = Point(x, y)
        self.comboType.Anchor = AnchorStyles.Bottom
        self.comboType.Items.Add("Road")
        self.comboType.Items.Add("Aerial")
        self.comboType.Items.Add("Hybrid")
        self.comboType.SelectedIndex = 1
        self.comboType.SelectedIndexChanged += self.previewTypeChanged
        self.panelCreate.Controls.Add(self.comboType)
        
        self.radio3x3 = RadioButton()
        self.radio3x3.BackColor = Color.White
        self.radio3x3.Anchor = AnchorStyles.Bottom
        self.radio3x3.Font = Font("Verdana", 8)
        self.radio3x3.Text = "3x3"
        x = self.buttonRight.Location.X + self.buttonRight.Size.Width + 15
        y = self.buttonUp.Location.Y + 9
        self.radio3x3.Location = Point(x, y)
        self.radio3x3.Size = Size(46, 17)
        self.radio3x3.Checked = True
        self.radio3x3.CheckedChanged += self.previewGridSizeChanged
        self.panelCreate.Controls.Add(self.radio3x3)
        
        self.radio4x4 = RadioButton()
        self.radio4x4.BackColor = Color.White
        self.radio4x4.Anchor = AnchorStyles.Bottom
        self.radio4x4.Font = Font("Verdana", 8)
        self.radio4x4.Text = "4x4"
        y = self.buttonDown.Location.Y + 1
        self.radio4x4.Location = Point(x, y)
        self.radio4x4.Size = Size(46, 17)
        self.radio4x4.CheckedChanged += self.previewGridSizeChanged
        self.panelCreate.Controls.Add(self.radio4x4)
        
        self.buttonCreate = Button()
        self.buttonCreate.BackColor = Color.White
        self.buttonCreate.Anchor = AnchorStyles.Bottom
        x = self.radio3x3.Location.X + self.radio3x3.Size.Width + 10
        y = self.scrollZoom.Location.Y
        self.buttonCreate.Location = Point(x, y)
        self.buttonCreate.Size = Size(71, 25)
        self.buttonCreate.Font = Font("Verdana", 10)
        self.buttonCreate.Text = "Create"
        self.buttonCreate.Click += self.createNewGame
        self.panelCreate.Controls.Add(self.buttonCreate)
        
        
        self.previewTile = [[],[],[],[]]
        
        # TODO: error handling
        if self.previewState[2] == 3:
            tileDim = self.normalTileDimension
        else:
            tileDim = 0.75 * self.normalTileDimension
        
        self.scrollZoom.Value = self.previewState[3]
        
        for i in range(0, 3):
            for j in range(0, 3):
                self.previewTile[i] += [RichTile(tileDim)]
                x = j * tileDim
                y = i * tileDim
                self.previewTile[i][j].Location = Point(x, y)
                self.panelCreate.Controls.Add(self.previewTile[i][j])
        
        i = 3
        y = 3 * 0.75 * self.normalTileDimension
        for j in range(0, 4):
            self.previewTile[i] += [RichTile(0.75 * self.normalTileDimension)]
            x = j * 0.75 * self.normalTileDimension
            self.previewTile[i][j].Location = Point(x, y)
            self.previewTile[i][j].BackColor = Color.Black
            self.panelCreate.Controls.Add(self.previewTile[i][j])
            if self.previewState[2] == 3:
                self.previewTile[i][j].Visible = False
                
        
        j = 3
        for i in range(0, 3):
            self.previewTile[i] += [RichTile(0.75 * self.normalTileDimension)]
            y = i * 0.75 * self.normalTileDimension
            self.previewTile[i][j].Location = Point(x, y)
            self.previewTile[i][j].BackColor = Color.Black
            self.panelCreate.Controls.Add(self.previewTile[i][j])
            if self.previewState[2] == 3:
                self.previewTile[i][j].Visible = False
        
        if self.previewState[2] == 4:
            self.radio4x4.Checked = True
        
        self.PopulatePreviewTiles()    
        
        # attach event after create gui
        self.scrollZoom.ValueChanged += self.scrollZoomChanged
        
        ### populate Load panel
        
        self.panelGames = LoadGamePanel()
        self.panelGames.Location = Point(10, 10)
        self.panelGames.Size = Size(270, self.panelLoad.Size.Height - 23 - 20)
        self.panelGames.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom
        self.panelLoad.Controls.Add(self.panelGames)
        
        for game in self.savedPuzzles:
            b = SavedGameButton(game[0], game[2], game[3], game[1], game[4], game[5])
            self.panelGames.AddButton(b)
        
        self.buttonLoad = Button()
        self.buttonLoad.BackColor = Color.White
        self.buttonLoad.Text = "Load Puzzle"
        self.buttonLoad.Font = Font("Verdana", 10)
        self.buttonLoad.Size = Size(100, 23)
        x = self.panelGames.Location.X
        y = self.panelLoad.Size.Height - self.buttonLoad.Size.Height - 10
        self.buttonLoad.Location = Point(x, y)
        self.buttonLoad.Anchor = AnchorStyles.Left | AnchorStyles.Bottom
        self.buttonLoad.Click += self.LoadGame
        self.panelLoad.Controls.Add(self.buttonLoad)
                
        ### populate Options panel
        
        self.labelTileSize = Label()
        self.labelTileSize.AutoSize = True
        self.labelTileSize.Font = Font("Verdana", 10, FontStyle.Bold)
        self.labelTileSize.Text = "Tile Size"
        self.labelTileSize.Location = Point(25, 15)
        self.panelOptions.Controls.Add(self.labelTileSize)
        
        self.radio50 = RadioButton()
        self.radio50.Font = Font("Verdana", 10)
        self.radio50.Text = "50%"
        self.radio50.Size = Size(55, 20)
        x = self.labelTileSize.Location.X + self.labelTileSize.Size.Width + 15
        y = self.labelTileSize.Location.Y
        self.radio50.Location = Point(x, y)
        self.radio50.Checked = True
        self.radio50.CheckedChanged += self.tileSizeChanged
        self.panelOptions.Controls.Add(self.radio50)
        
        self.radio75 = RadioButton()
        self.radio75.Font = Font("Verdana", 10)
        self.radio75.Text = "75%"
        self.radio75.Size = Size(55, 20)
        x = self.radio50.Location.X + self.radio50.Size.Width + 2
        self.radio75.Location = Point(x, y)
        self.radio75.CheckedChanged += self.tileSizeChanged
        self.panelOptions.Controls.Add(self.radio75)
        
        self.radio100 = RadioButton()
        self.radio100.Font = Font("Verdana", 10)
        self.radio100.Text = "100%"
        self.radio100.Size = Size(65, 20)
        x = self.radio75.Location.X + self.radio75.Size.Width + 2
        self.radio100.Location = Point(x, y)
        self.radio100.CheckedChanged += self.tileSizeChanged
        self.panelOptions.Controls.Add(self.radio100)
        
        self.checkCache = CheckBox()
        self.checkCache.AutoSize = True
        self.checkCache.Font = Font("Verdana", 10, FontStyle.Bold)
        self.checkCache.Text = "Allow caching"
        x = self.labelTileSize.Location.X
        y = self.labelTileSize.Location.Y + self.labelTileSize.Size.Height + 20
        self.checkCache.Location = Point(x, y)
        if self.savedSettings[2] == True:
            self.checkCache.Checked = True
        self.checkCache.CheckedChanged += self.toggleCache
        self.panelOptions.Controls.Add(self.checkCache)
        
        self.buttonClearCache = Button()
        self.buttonClearCache.BackColor = Color.White
        self.buttonClearCache.AutoSize = True
        self.buttonClearCache.Font = Font("Verdana", 10)
        self.buttonClearCache.Text = "Clear Cache"
        x = self.checkCache.Location.X + self.checkCache.Size.Width + 10
        y -= 4
        self.buttonClearCache.Location = Point(x, y)
        self.buttonClearCache.Click += self.buttonClearCacheClicked
        self.panelOptions.Controls.Add(self.buttonClearCache)
        
        ### populate About panel
        
        text = Label()
        text.Text = "Ravi Chugh"
        text.Text += "\nrkc@seas.upenn.edu"
        text.Text += "\n"
        text.Text += "\nwritten in IronPython on .NET 2.0"
        text.Text += "\ntiles served by Virtual Earth"
        text.Text += "\n"
        text.Text += "\nVirtual Earth terms of use:"
        text.Text += "\nwww.microsoft.com/virtualearth/control/terms.mspx"
        text.Text += "\n"
        text.Text += "\nimage providers:"
        text.Text += "\nlocal.live.com/Help/en-us/Credits.htm"
        text.TextAlign = ContentAlignment.MiddleCenter
        text.Size = self.panelAbout.Size
        text.Font = Font("Verdana", 10)
        text.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right
        self.panelAbout.Controls.Add(text)
        
        
        
        
    def ResizeEverything(self, tileDim):
    
        self.tileDimension = self.normalTileDimension = tileDim
        if self.gridDimension == 4:
            self.tileDimension = 0.75 * self.tileDimension
        
        w = self.gridDimension * self.tileDimension + 2 * self.SIDE_BORDER + self.menu.Size.Width
        h = self.gridDimension * self.tileDimension + self.TOP_BORDER + self.BOTTOM_BORDER
        self.ClientSize = Size(w, h)
        self.wllPicture.Location = Point(self.ClientSize.Width - self.wllPicture.Size.Width, self.ClientSize.Height - self.wllPicture.Size.Height)
        
        for panel in self.panels:
            w = self.gridDimension * self.tileDimension
            panel.Size = Size(w, w)
            
        for i in range(0, self.gridDimension):
            for j in range(0, self.gridDimension):
                if not self.grid[i][j] is None:
                    self.grid[i][j].Size = Size(self.tileDimension, self.tileDimension)
                    self.grid[i][j].Location = self.GetLocation(i, j)
        
        ### resize preview tiles
        
        if self.previewState[2] == 3:
            tileDim = self.normalTileDimension
        else:
            tileDim = 0.75 * self.normalTileDimension
            
        for i in range(0, 3):
            for j in range(0, 3):
                x = j * tileDim
                y = i * tileDim
                self.previewTile[i][j].Location = Point(x, y)
                self.previewTile[i][j].Size = Size(tileDim, tileDim)
        
        i = 3
        y = 3 * 0.75 * self.normalTileDimension
        for j in range(0, 4):
            x = j * 0.75 * self.normalTileDimension
            self.previewTile[i][j].Location = Point(x, y)
            self.previewTile[i][j].Size = Size(tileDim, tileDim)            
        
        j = 3
        for i in range(0, 3):
            self.previewTile[i] += [RichTile(0.75 * self.normalTileDimension)]
            y = i * 0.75 * self.normalTileDimension
            self.previewTile[i][j].Location = Point(x, y)
            self.previewTile[i][j].Size = Size(tileDim, tileDim)
        
        self.linkShuffle.Size = Size(self.tileDimension, self.tileDimension)
        self.linkShuffle.Location = self.GetLocation(self.gridDimension - 1, self.gridDimension - 1)
        
        self.RelocateCaptionPlay(None, None)
        self.RelocateCaptionCreate(None, None)    
    
    def RelocateCaptionPlay(self, sender, e):
        self.captionPlay.Location = Point(self.panelBoard.Size.Width - self.captionPlay.Size.Width, 0)
        
    def RelocateCaptionCreate(self, sender, e):
        self.captionCreate.Location = Point(self.panelCreate.Size.Width - self.captionCreate.Size.Width, 0)
        
    ### EVENT HANDLERS ###
    
    def formClosing(self, sender, e):
    
        if self.exitFromMenu is True:
            return
    
        if self.gameInProgress is True:
            dr = MessageBox.Show("Quit current game?", "", MessageBoxButtons.YesNo)
            if dr == DialogResult.No:
                e.Cancel = True
                return
        
        if self.clearCacheOnExit is True:
            ClearCache()
        
        self.SaveSettings()
    
    def menuItemClicked(self, sender, e):
    
        if sender is self.menuExit:
            self.ExitGame()
            return
    
        for i in range(0, self.menu.Items.Count):
            self.menu.Items[i].BackColor = Color.White
            self.menu.Items[i].ForeColor = Color.Black
        sender.BackColor = Color.CornflowerBlue
        sender.ForeColor = Color.White
        
        for i in range(0, len(self.panels)):
            self.panels[i].Visible = False
        
        if sender is self.menuPlay:    self.panels[0].Visible = True
        elif sender is self.menuCreate:    self.panels[1].Visible = True
        elif sender is self.menuLoad: self.panels[2].Visible = True
        elif sender is self.menuOptions: self.panels[3].Visible = True
        elif sender is self.menuAbout: self.panels[4].Visible = True

    def mouseDown(self, sender, e):
    
        if self.gameInProgress is False:
            return

        x = sender.currentRow
        y = sender.currentCol

        if x > 0 and self.grid[x-1][y] is None:
            sender.direction = "up"
        elif x < self.gridDimension - 1 and self.grid[x+1][y] is None:
            sender.direction = "down"
        elif y > 0 and self.grid[x][y-1] is None:
            sender.direction = "left"
        elif y < self.gridDimension - 1 and self.grid[x][y+1] is None:
            sender.direction = "right"

        if not sender.direction is None:
            sender.beingDragged = True
            sender.initialClick = e.Location

    def mouseUp(self, sender, e):

        if not sender.initialClick is None and e.X is sender.initialClick.X and e.Y is sender.initialClick.Y:
            if sender.direction is "right":
                self.slideRight(sender)
            elif sender.direction is "left":
                self.slideLeft(sender)
            elif sender.direction is "up":
                self.slideUp(sender)
            else:
                self.slideDown(sender)

        elif sender.direction is "right":
            if sender.Location.X < (sender.currentCol + 0.5) * self.tileDimension:
                self.snapBack(sender)
            else:
                self.slideRight(sender)

        elif sender.direction is "left":
            if sender.Location.X > (sender.currentCol - 0.5) * self.tileDimension:
                sender.Location = Point((sender.currentCol) * self.tileDimension, sender.Location.Y)
            else:
                self.slideLeft(sender)

        elif sender.direction is "up":
            if sender.Location.Y > (sender.currentRow - 0.5) * self.tileDimension:
                sender.Location = Point(sender.Location.X, sender.currentRow * self.tileDimension)
            else:
                self.slideUp(sender)

        elif sender.direction is "down":
            if sender.Location.Y < (sender.currentRow + 0.5) * self.tileDimension:
                sender.Location = Point(sender.Location.X, sender.currentRow * self.tileDimension)
            else:
                self.slideDown(sender)

        sender.direction = None
        sender.beingDragged = False
        sender.initialClick = None

    def mouseMove(self, sender, e):

        x = sender.Location.X
        y = sender.Location.Y
        if sender.beingDragged:
            if sender.direction is "right":
                dX = e.X - sender.initialClick.X
                if x + dX >= (sender.currentCol) * self.tileDimension:
                    if x + dX <= (sender.currentCol + 1) * self.tileDimension:
                        x += dX
            elif sender.direction is "left":
                dX = e.X - sender.initialClick.X
                if x + dX >= (sender.currentCol - 1) * self.tileDimension:
                    if x + dX <= (sender.currentCol) * self.tileDimension:
                        x += dX
            elif sender.direction is "up":
                dY = e.Y - sender.initialClick.Y
                if y + dY >= (sender.currentRow - 1) * self.tileDimension:
                    if y + dY <= (sender.currentRow) * self.tileDimension:
                        y = sender.Location.Y + e.Y - sender.initialClick.Y
            elif sender.direction is "down":
                dY = e.Y - sender.initialClick.Y
                if y + dY >= (sender.currentRow) * self.tileDimension:
                    if y + dY <= (sender.currentRow + 1) * self.tileDimension:
                        y += dY
            sender.Location = Point(x, y)

    def snapBack(self, sender):
        sender.Location = Point(sender.currentCol * self.tileDimension, sender.currentRow * self.tileDimension)

    def slideLeft(self, sender):
        self.grid[sender.currentRow][sender.currentCol] = None
        sender.currentCol -= 1
        self.grid[sender.currentRow][sender.currentCol] = sender
        sender.Location = Point(sender.currentCol * self.tileDimension, sender.currentRow * self.tileDimension)
        self.CheckGrid()

    def slideRight(self, sender):
        self.grid[sender.currentRow][sender.currentCol] = None
        sender.currentCol += 1
        self.grid[sender.currentRow][sender.currentCol] = sender
        sender.Location = Point(sender.currentCol * self.tileDimension, sender.currentRow * self.tileDimension)
        self.CheckGrid()

    def slideUp(self, sender):
        self.grid[sender.currentRow][sender.currentCol] = None
        sender.currentRow -= 1
        self.grid[sender.currentRow][sender.currentCol] = sender
        sender.Location = Point(sender.currentCol * self.tileDimension, sender.currentRow * self.tileDimension)
        self.CheckGrid()

    def slideDown(self, sender):
        self.grid[sender.currentRow][sender.currentCol] = None
        sender.currentRow += 1
        self.grid[sender.currentRow][sender.currentCol] = sender
        sender.Location = Point(sender.currentCol * self.tileDimension, sender.currentRow * self.tileDimension)
        self.CheckGrid()

    def keyDown(self, sender, e):
        if self.panels[0].Visible is True:
            self.keyDownForGame(sender, e)
        elif self.panels[1].Visible is True:
            self.keyDownForCreate(sender, e)
    
    def keyDownForGame(self, sender, e):
    
        if self.gameInProgress is False:
            return
            
        if e.KeyCode == Keys.Left:
            for i in range(0, self.gridDimension):
                for j in range(0, self.gridDimension - 1):
                    if self.grid[i][j] is None:
                        self.slideLeft(self.grid[i][j+1])
                        return
        elif e.KeyCode == Keys.Right:
            for i in range(0, self.gridDimension):
                for j in range(1, self.gridDimension):
                    if self.grid[i][j] is None:
                        self.slideRight(self.grid[i][j-1])
                        return
        elif e.KeyCode == Keys.Up:
            for i in range(0, self.gridDimension - 1):
                for j in range(0, self.gridDimension):
                    if self.grid[i][j] is None:
                        self.slideUp(self.grid[i+1][j])
                        return
        elif e.KeyCode == Keys.Down:
            for i in range(1, self.gridDimension):
                for j in range(0, self.gridDimension):
                    if self.grid[i][j] is None:
                        self.slideDown(self.grid[i-1][j])
                        return
    
    def keyDownForCreate(self, sender, e):
    
        e.SuppressKeyPress = True
        e.Handled = True
        
        if e.KeyCode == Keys.Left:
            self.previewMoveLeft()
        elif e.KeyCode == Keys.Right:
            self.previewMoveRight()
        elif e.KeyCode == Keys.Up:
            self.previewMoveUp()
        elif e.KeyCode == Keys.Down:
            self.previewMoveDown()
        
        

    def arrowButtonMouseDown(self, sender, e):
        sender.BackColor = Color.SteelBlue

    def arrowButtonMouseUp(self, sender, e):
        sender.BackColor = Color.White
        
    def tileSizeChanged(self, sender, e):
    
        # two CheckedChanged events are fired when choosing another radio button
        # so ignore one
        if sender.Checked is False:
            return
            
        if self.radio50.Checked:
            self.ResizeEverything(0.50 * 256)
        elif self.radio75.Checked:
            self.ResizeEverything(0.75 * 256)
        elif self.radio100.Checked:
            self.ResizeEverything(256)

    def previewTypeChanged(self, sender, e):
        
        type = self.comboType.SelectedItem[0].lower()
        for i in range(0, 4):
            for j in range(0, 4):
                self.previewTile[i][j].Image = GetImage(type, self.previewTile[i][j].quadkey)
        self.PopulateCaptionCreate()
        
    def previewGridSizeChanged(self, sender, e):
        
        if sender is False:
            return
            
        if sender is self.radio3x3:
            tileDim = self.normalTileDimension
        else:
            tileDim = 0.75 * self.normalTileDimension
            i = 3
            for j in range(0, 4):
                self.previewTile[i][j].Visible = True
            j = 3
            for i in range(0, 3):
                self.previewTile[i][j].Visible = True
        
        for i in range(0, 3):
            for j in range(0, 3):
                self.previewTile[i][j].Size = Size(tileDim, tileDim)
                x = j * tileDim
                y = i * tileDim
                self.previewTile[i][j].Location = Point(x, y)
        
        if self.radio3x3 is sender:
            self.previewState[2] = 3
        else:
            self.previewState[2] = 4
                
    def scrollZoomChanged(self, sender, e):
        
        oldLevel = self.previewState[3]
        newLevel = sender.Value
        deltaLevel = newLevel - oldLevel
        if deltaLevel > 0:
            #self.previewState[0] = self.previewState[0] * 2 + int(0.5 * (MaxTiles(newLevel) - MaxTiles(oldLevel)))
            #self.previewState[1] = self.previewState[1] * 2 + int(0.5 * (MaxTiles(newLevel) - MaxTiles(oldLevel)))
            self.previewState[0] = (self.previewState[0] + 1) * 2 * deltaLevel - 1
            self.previewState[1] = (self.previewState[1] + 1) * 2 * deltaLevel - 1
        elif deltaLevel < 0:
            self.previewState[0] = (self.previewState[0]) / (-2 * deltaLevel)
            self.previewState[1] = (self.previewState[1]) / (-2 * deltaLevel)
        self.previewState[0] = self.previewState[0] / MaxTiles(newLevel)
        self.previewState[1] = self.previewState[1] / MaxTiles(newLevel)
        self.previewState[3] = newLevel
        self.PopulatePreviewTiles()
        
    def previewMoveRight(self, sender = None, e = None):
        for i in range(0, 4):
            for j in range(0, 3):
                self.previewTile[i][j].Image = self.previewTile[i][j+1].Image
                self.previewTile[i][j].quadkey = self.previewTile[i][j+1].quadkey
                self.previewTile[i][j].tile = self.previewTile[i][j+1].tile
        type = self.comboType.SelectedItem
        for i in range(0, 4):
            tile = (self.previewTile[i][2].tile[0] + 1, self.previewTile[i][2].tile[1])
            quadkey = TileToQuadkey(tile, self.previewState[3])
            self.previewTile[i][3].Image = GetImage(type[0].lower(), quadkey)
            self.previewTile[i][3].quadkey = quadkey
            self.previewTile[i][3].tile = tile
            
    def previewMoveLeft(self, sender = None, e = None):
        for i in range(0, 4):
            for j in range(3, 0, -1):
                self.previewTile[i][j].Image = self.previewTile[i][j-1].Image
                self.previewTile[i][j].quadkey = self.previewTile[i][j-1].quadkey
                self.previewTile[i][j].tile = self.previewTile[i][j-1].tile
        type = self.comboType.SelectedItem
        for i in range(0, 4):
            tile = (self.previewTile[i][1].tile[0] - 1, self.previewTile[i][1].tile[1])
            quadkey = TileToQuadkey(tile, self.previewState[3])
            self.previewTile[i][0].Image = GetImage(type[0].lower(), quadkey)
            self.previewTile[i][0].quadkey = quadkey
            self.previewTile[i][0].tile = tile
            
    def previewMoveDown(self, sender = None, e = None):
        for i in range(0, 3):
            for j in range(0, 4):
                self.previewTile[i][j].Image = self.previewTile[i+1][j].Image
                self.previewTile[i][j].quadkey = self.previewTile[i+1][j].quadkey
                self.previewTile[i][j].tile = self.previewTile[i+1][j].tile
        type = self.comboType.SelectedItem
        for j in range(0, 4):
            tile = (self.previewTile[3][j].tile[0], self.previewTile[3][j].tile[1] + 1)
            quadkey = TileToQuadkey(tile, self.previewState[3])
            self.previewTile[3][j].Image = GetImage(type[0].lower(), quadkey)
            self.previewTile[3][j].quadkey = quadkey
            self.previewTile[3][j].tile = tile
    
    def previewMoveUp(self, sender = None, e = None):
        for i in range(3, 0, -1):
            for j in range(0, 4):
                self.previewTile[i][j].Image = self.previewTile[i-1][j].Image
                self.previewTile[i][j].quadkey = self.previewTile[i-1][j].quadkey
                self.previewTile[i][j].tile = self.previewTile[i-1][j].tile
        type = self.comboType.SelectedItem
        for j in range(0, 4):
            tile = (self.previewTile[0][j].tile[0], self.previewTile[0][j].tile[1] - 1)
            quadkey = TileToQuadkey(tile, self.previewState[3])
            self.previewTile[0][j].Image = GetImage(type[0].lower(), quadkey)
            self.previewTile[0][j].quadkey = quadkey
            self.previewTile[0][j].tile = tile
            
    def toggleCache(self, sender, e):
        if self.checkCache.Checked is True:
            MessageBox.Show("Take a look at the Caching section of the tutorial")
            TurnOnCache()
        else:
            TurnOffCache()
    
    def buttonClearCacheClicked(self, sender, e):        
        self.clearCacheOnExit = True
    
    def createNewGame(self, sender, e):
        if self.gameInProgress is True:
            dr = MessageBox.Show("Quit current game?", "", MessageBoxButtons.YesNo)
            if dr == DialogResult.No:
                return
        self.SetBoardFromCreate()
        self.menuItemClicked(self.menuPlay, None)
        

def Main():
    Application.Run(Puzzle())


if __name__ == "__main__":
    Main()
else:
    import winforms
