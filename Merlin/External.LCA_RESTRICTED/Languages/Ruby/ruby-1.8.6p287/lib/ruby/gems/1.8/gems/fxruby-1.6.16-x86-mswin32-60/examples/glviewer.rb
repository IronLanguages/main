#!/usr/bin/env ruby

require 'fox16'
require 'fox16/responder'
begin
  require 'fox16/glshapes'
rescue LoadError
  require 'fox16/missingdep'
  MSG = <<EOM
  Sorry, this example depends on the OpenGL extension. Please
  check the Ruby Application Archives for an appropriate
  download site.
EOM
  missingDependency(MSG)
end

include Fox

class TabBook < FXTabBook
  def createAnglesPage(panels, mdiclient)
    # Angles tab
    FXTabItem.new(panels,
      "Angles\tCamera Angles\tSwitch to camera angles panel.")

    # Angles page
    angles = FXMatrix.new(panels, 3,
      :opts => FRAME_THICK|FRAME_RAISED|MATRIX_BY_COLUMNS|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT,
      :padding => 10)

    FXLabel.new(angles, "X:")
    FXTextField.new(angles, 6,
      mdiclient, FXGLViewer::ID_ROLL,
      TEXTFIELD_INTEGER|JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK)
    x_dial = FXDial.new(angles,
      mdiclient, FXGLViewer::ID_DIAL_X,
      :opts => FRAME_SUNKEN|FRAME_THICK|DIAL_CYCLIC|DIAL_HORIZONTAL|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|LAYOUT_CENTER_Y,
      :width => 160, :height => 14, :padding => 0)
    x_dial.tipText = "Rotate about X"
    x_dial.notchOffset = 900

    FXLabel.new(angles, "Y:")
    FXTextField.new(angles, 6,
      mdiclient, FXGLViewer::ID_PITCH,
      TEXTFIELD_INTEGER|JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK)
    y_dial = FXDial.new(angles,
      mdiclient, FXGLViewer::ID_DIAL_Y,
      :opts => FRAME_SUNKEN|FRAME_THICK|DIAL_CYCLIC|DIAL_HORIZONTAL|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|LAYOUT_CENTER_Y,
      :width => 160, :height => 14, :padding => 0)
    y_dial.tipText = "Rotate about Y"
    y_dial.notchOffset = 900

    FXLabel.new(angles, "Z:")
    FXTextField.new(angles, 6,
      mdiclient, FXGLViewer::ID_YAW,
      TEXTFIELD_INTEGER|JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK)
    z_dial = FXDial.new(angles,
      mdiclient, FXGLViewer::ID_DIAL_Z,
      :opts => FRAME_SUNKEN|FRAME_THICK|DIAL_CYCLIC|DIAL_HORIZONTAL|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|LAYOUT_CENTER_Y,
      :width => 160, :height => 14, :padding => 0)
    z_dial.tipText = "Rotate about Z"
    z_dial.notchOffset = 900

    FXLabel.new(angles, "FOV:")
    fov = FXTextField.new(angles, 5, mdiclient, FXGLViewer::ID_FOV,
      JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK)
    FXFrame.new(angles, :opts => FRAME_NONE)
    fov.tipText = "Field of view"

    FXLabel.new(angles, "Zoom:")
    zz = FXTextField.new(angles, 5, mdiclient, FXGLViewer::ID_ZOOM,
      JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK)
    FXFrame.new(angles, :opts => FRAME_NONE)
    zz.tipText = "Zooming"

    FXLabel.new(angles, "Scale X:")
    FXTextField.new(angles, 5, mdiclient, FXGLViewer::ID_SCALE_X,
      JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK)
    FXFrame.new(angles, :opts => FRAME_NONE)
    FXLabel.new(angles, "Scale Y:")
    FXTextField.new(angles, 5, mdiclient, FXGLViewer::ID_SCALE_Y,
      JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK)
    FXFrame.new(angles, :opts => FRAME_NONE)
    FXLabel.new(angles, "Scale Z:")
    FXTextField.new(angles, 5, mdiclient, FXGLViewer::ID_SCALE_Z,
      JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK)
    FXFrame.new(angles, :opts => FRAME_NONE)
  end

  def createColorsPage(panels, mdiclient)
    # Colors tab
    FXTabItem.new(panels, "Colors\tColors\tSwitch to color panel.")

    # Colors page
    colors = FXMatrix.new(panels, 2,
      MATRIX_BY_COLUMNS|FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_Y|LAYOUT_CENTER_X|LAYOUT_TOP|LAYOUT_LEFT,
      :padding => 10)
    FXLabel.new(colors, "Background:", nil,
      LAYOUT_RIGHT|LAYOUT_CENTER_Y|JUSTIFY_RIGHT)
    FXColorWell.new(colors, 0, mdiclient, FXGLViewer::ID_BACK_COLOR,
      COLORWELL_OPAQUEONLY|LAYOUT_TOP|LAYOUT_LEFT, :padding => 0)
    FXLabel.new(colors, "Ambient:", nil,
      LAYOUT_RIGHT|LAYOUT_CENTER_Y|JUSTIFY_RIGHT)
    FXColorWell.new(colors, 0, mdiclient, FXGLViewer::ID_AMBIENT_COLOR,
      COLORWELL_OPAQUEONLY|LAYOUT_TOP|LAYOUT_LEFT, :padding => 0)

    FXLabel.new(colors, "Light Amb:", nil,
      LAYOUT_RIGHT|LAYOUT_CENTER_Y|JUSTIFY_RIGHT)
    FXColorWell.new(colors, 0, mdiclient, FXGLViewer::ID_LIGHT_AMBIENT,
      COLORWELL_OPAQUEONLY|LAYOUT_TOP|LAYOUT_LEFT, :padding => 0)
    FXLabel.new(colors, "Light Diff:", nil,
      LAYOUT_RIGHT|LAYOUT_CENTER_Y|JUSTIFY_RIGHT)
    FXColorWell.new(colors, 0, mdiclient, FXGLViewer::ID_LIGHT_DIFFUSE,
      COLORWELL_OPAQUEONLY|LAYOUT_TOP|LAYOUT_LEFT, :padding => 0)
    FXLabel.new(colors, "Light Spec:", nil,
      LAYOUT_RIGHT|LAYOUT_CENTER_Y|JUSTIFY_RIGHT)
    FXColorWell.new(colors, 0, mdiclient, FXGLViewer::ID_LIGHT_SPECULAR,
      COLORWELL_OPAQUEONLY|LAYOUT_TOP|LAYOUT_LEFT, :padding => 0)
  end

  def createSwitchesPage(panels, mdiclient)
    # Settings tab
    FXTabItem.new(panels, "Settings\tSettings\tSwitche to settings panel.")

    # Settings page
    settings = FXVerticalFrame.new(panels,
      FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_Y|LAYOUT_CENTER_X|LAYOUT_TOP|LAYOUT_LEFT,
      :padding => 10)
    FXCheckButton.new(settings, "Lighting", mdiclient, FXGLViewer::ID_LIGHTING,
      ICON_BEFORE_TEXT)
    FXCheckButton.new(settings, "Fog", mdiclient, FXGLViewer::ID_FOG,
      ICON_BEFORE_TEXT)
    FXCheckButton.new(settings, "Dither", mdiclient, FXGLViewer::ID_DITHER,
      ICON_BEFORE_TEXT)
    FXCheckButton.new(settings, "Turbo", mdiclient, FXGLViewer::ID_TURBO,
      ICON_BEFORE_TEXT)
  end

  def initialize(frame, mdiclient)
    super(frame)
    createAnglesPage(self, mdiclient)
    createColorsPage(self, mdiclient)
    createSwitchesPage(self, mdiclient)
  end
end

class GLViewWindow < FXMainWindow

  include Responder
  
  ID_QUERY_MODE = FXMainWindow::ID_LAST
  ID_GLVIEWER   = ID_QUERY_MODE + 1
  
  # Load the named PNG icon from a file
  def loadIcon(filename)
    begin
      filename = File.join("icons", filename) + ".png"
      icon = nil
      File.open(filename, "rb") do |f|
        icon = FXPNGIcon.new(getApp(), f.read)
      end
      icon
    rescue
      raise RuntimeError, "Couldn't load icon: #{filename}"
    end
  end

  def initialize(app)
    # Initialize base class first
    super(app, "OpenGL Example Application", :opts => DECOR_ALL, :width => 800, :height => 600)

    # Define message identifiers for this class

    # Set up the message map
    FXMAPFUNC(SEL_UPDATE,  GLViewWindow::ID_QUERY_MODE, :onUpdMode)
    FXMAPFUNC(SEL_COMMAND, FXWindow::ID_QUERY_MENU,	:onQueryMenu)

    # Main window icon
    peng = loadIcon("penguin")
    setIcon(peng)

    # The colors dialog
    colordlg = FXColorDialog.new(self, "Color Dialog", DECOR_TITLE|DECOR_BORDER)

    # Menubar
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)

    # Tool bar
    FXHorizontalSeparator.new(self,
      LAYOUT_SIDE_TOP|SEPARATOR_GROOVE|LAYOUT_FILL_X);
    toolbar = FXToolBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X,
      :padLeft => 4, :padRight => 4, :padTop => 0, :padBottom => 0,
      :hSpacing => 0, :vSpacing => 0)

    # Make status bar
    statusbar = FXStatusBar.new(self,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|STATUSBAR_WITH_DRAGCORNER)

    # The good old penguin, what would we be without it?
    FXButton.new(statusbar,
      "\tHello, I'm Tux...\nThe symbol for the Linux Operating System.\nAnd all it stands for.",
      :icon => peng, :opts => LAYOUT_RIGHT)

    # Contents
    frame = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y, :padding => 0, :hSpacing => 0, :vSpacing => 0)

    # Nice sunken box around GL viewer
    box = FXVerticalFrame.new(frame,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y, :padding => 0)

    # MDI Client
    @mdiclient = FXMDIClient.new(box, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    @mdimenu = FXMDIMenu.new(self, @mdiclient)

    # MDI buttons in menu:- note the message ID's!!!!!
    # Normally, MDI commands are simply sensitized or desensitized;
    # Under the menubar, however, they're hidden if the MDI Client is
    # not maximized.  To do this, they must have different ID's.
    FXMDIWindowButton.new(menubar, @mdimenu, @mdiclient,
      FXMDIClient::ID_MDI_MENUWINDOW, LAYOUT_LEFT)
    FXMDIDeleteButton.new(menubar, @mdiclient,
      FXMDIClient::ID_MDI_MENUCLOSE, FRAME_RAISED|LAYOUT_RIGHT)
    FXMDIRestoreButton.new(menubar, @mdiclient,
      FXMDIClient::ID_MDI_MENURESTORE, FRAME_RAISED|LAYOUT_RIGHT)
    FXMDIMinimizeButton.new(menubar, @mdiclient,
      FXMDIClient::ID_MDI_MENUMINIMIZE, FRAME_RAISED|LAYOUT_RIGHT)

    # Icon for MDI Child
    @mdiicon = loadIcon("winapp")

    # Make MDI Window Menu
    @mdimenu = FXMDIMenu.new(self, @mdiclient)

    # Make an MDI Child
    mdichild = FXMDIChild.new(@mdiclient, "FOX GL Viewer", @mdiicon,
      @mdimenu, MDI_NORMAL, 30, 30, 300, 200)
    @count = 1

    # A visual to drag OpenGL in double-buffered mode; note the glvisual is
    # shared between all windows which need the same depths and numbers of
    # buffers. Thus, while the first visual may take some time to initialize,
    # each subsequent window can be created very quickly; we need to determine
    # graphics hardware characteristics only once.
    @glvisual = FXGLVisual.new(getApp(), VISUAL_DOUBLEBUFFER)

    # Make it active
    @mdiclient.setActiveChild(mdichild)

    # Drawing gl canvas
    viewer = FXGLViewer.new(mdichild, @glvisual, self, ID_GLVIEWER,
      LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT)

    # Tab book with switchable panels
    panels = TabBook.new(frame, @mdiclient)

    # Construct these icons
    newdoc = loadIcon("filenew")
    opendoc = loadIcon("fileopen")
    savedoc = loadIcon("filesave")
    saveasdoc = loadIcon("filesaveas")

    # File Menu
    filemenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)
    FXMenuCommand.new(filemenu, "&New...\tCtl-N\tCreate new document.", newdoc)
    openCmd = FXMenuCommand.new(filemenu, "&Open...\tCtl-O\tOpen document file.", opendoc)
    openCmd.connect(SEL_COMMAND, method(:onCmdOpen))
    FXMenuCommand.new(filemenu, "&Save\tCtl-S\tSave document.", savedoc)
    FXMenuCommand.new(filemenu, "Save &As...\t\tSave document to another file.", saveasdoc)
    FXMenuCommand.new(filemenu, "&Print Image...\t\tPrint snapshot image.", nil, @mdiclient, FXGLViewer::ID_PRINT_IMAGE, MENU_AUTOGRAY)
    FXMenuCommand.new(filemenu, "&Print Vector...\t\tPrint geometry.", nil, @mdiclient, FXGLViewer::ID_PRINT_VECTOR, MENU_AUTOGRAY)
    FXMenuCommand.new(filemenu, "&Dump...\t\tDump widgets.", nil, getApp(), FXApp::ID_DUMP)
    FXMenuCommand.new(filemenu, "&Quit\tCtl-Q\tQuit the application.", nil, getApp(), FXApp::ID_QUIT)

    # Edit Menu
    editmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Edit", nil, editmenu)
    FXMenuCommand.new(editmenu, "Undo")
    FXMenuCommand.new(editmenu, "Copy", nil, @mdiclient, FXGLViewer::ID_COPY_SEL, MENU_AUTOGRAY)
    FXMenuCommand.new(editmenu, "Cut", nil, @mdiclient, FXGLViewer::ID_CUT_SEL, MENU_AUTOGRAY)
    FXMenuCommand.new(editmenu, "Paste", nil, @mdiclient, FXGLViewer::ID_PASTE_SEL, MENU_AUTOGRAY)
    FXMenuCommand.new(editmenu, "Delete", nil, @mdiclient, FXGLViewer::ID_DELETE_SEL, MENU_AUTOGRAY)

    # File manipulation
    FXButton.new(toolbar, "\tNew\tCreate new document.", newdoc, nil, 0,
      FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    openBtn = FXButton.new(toolbar, "\tOpen\tOpen document file.", opendoc, nil, 0,
      FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    openBtn.connect(SEL_COMMAND, method(:onCmdOpen))
    FXButton.new(toolbar, "\tSave\tSave document.", savedoc, nil, 0,
      FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXButton.new(toolbar, "\tSave As\tSave document to another file.",
      saveasdoc, nil, 0, FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXButton.new(toolbar, "\tNew Folder\tNo comment", loadIcon("newfolder"),
      nil, 0, FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)

    # Print
    FXFrame.new(toolbar,
      LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT, :width => 4, :height => 20)
    FXButton.new(toolbar, "\tPrint Image\tPrint shapshot image.",
      loadIcon("printicon"), @mdiclient, FXGLViewer::ID_PRINT_IMAGE,
      BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
  
    # Editing
    FXFrame.new(toolbar,
      LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT, :width => 4, :height => 20)
    FXButton.new(toolbar, "\tCut", loadIcon("cut"), @mdiclient,
      FXGLViewer::ID_CUT_SEL, (BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|
      LAYOUT_TOP|LAYOUT_LEFT))
    FXButton.new(toolbar, "\tCopy", loadIcon("copy"), @mdiclient,
      FXGLViewer::ID_COPY_SEL, (BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|
      LAYOUT_TOP|LAYOUT_LEFT))
    FXButton.new(toolbar, "\tPaste", loadIcon("paste"), @mdiclient,
      FXGLViewer::ID_PASTE_SEL, (BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|
      LAYOUT_TOP|LAYOUT_LEFT))
  
    # Projections
    FXFrame.new(toolbar, (LAYOUT_TOP|LAYOUT_LEFT|
      LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT), :width => 8, :height => 20)
    FXButton.new(toolbar, "\tPerspective\tSwitch to perspective projection.",
      loadIcon("perspective"), @mdiclient, FXGLViewer::ID_PERSPECTIVE,
      BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXButton.new(toolbar, "\tParallel\tSwitch to parallel projection.",
      loadIcon("parallel"), @mdiclient, FXGLViewer::ID_PARALLEL,
      BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
  
    # Shading model
    FXFrame.new(toolbar, (LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FIX_WIDTH|
      LAYOUT_FIX_HEIGHT), :width => 8, :height => 20)
    FXButton.new(toolbar, "\tNo shading\tTurn light sources off.",
      loadIcon("nolight"), @mdiclient, FXGLShape::ID_SHADEOFF,
      BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXButton.new(toolbar, "\tFlat shading\tTurn on faceted (flat) shading.",
      loadIcon("light"), @mdiclient, FXGLShape::ID_SHADEON,
      BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXButton.new(toolbar, "\tSmooth shading\tTurn on smooth shading.",
      loadIcon("smoothlight"), @mdiclient, FXGLShape::ID_SHADESMOOTH,
      BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXFrame.new(toolbar, (LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FIX_WIDTH|
      LAYOUT_FIX_HEIGHT), :width => 8, :height => 20)
    FXToggleButton.new(toolbar, "\tToggle Light\tToggle light source.", nil,
      loadIcon("nolight"), loadIcon("light"), nil, 0,
      FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
  
    # View orientation
    FXFrame.new(toolbar, (LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FIX_WIDTH|
      LAYOUT_FIX_HEIGHT), :width => 8, :height => 20)
    FXButton.new(toolbar, "\tFront View\tView objects from the front.",
      loadIcon("frontview"), @mdiclient, FXGLViewer::ID_FRONT,
      BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXButton.new(toolbar, "\tBack View\tView objects from behind.",
      loadIcon("backview"), @mdiclient, FXGLViewer::ID_BACK,
      BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXButton.new(toolbar, "\tLeft View\tView objects from the left.",
      loadIcon("leftview"), @mdiclient, FXGLViewer::ID_LEFT,
      BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXButton.new(toolbar, "\tRight View\tView objects from the right.",
      loadIcon("rightview"), @mdiclient, FXGLViewer::ID_RIGHT,
      BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXButton.new(toolbar, "\tTop View\tView objects from the top.",
      loadIcon("topview"), @mdiclient, FXGLViewer::ID_TOP,
      BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXButton.new(toolbar, "\tBottom View\tView objects from below.",
      loadIcon("bottomview"), @mdiclient, FXGLViewer::ID_BOTTOM,
      BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
  
    # Miscellaneous buttons
    FXFrame.new(toolbar, (LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FIX_WIDTH|
      LAYOUT_FIX_HEIGHT), :width => 8, :height => 20)
    FXButton.new(toolbar, nil, loadIcon("zoom"), nil, 0,
      FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXButton.new(toolbar, "\tColors\tDisplay color dialog.",
      loadIcon("colorpal"), colordlg, FXWindow::ID_SHOW,
      FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXButton.new(toolbar, nil, loadIcon("camera"), nil, 0,
      FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
    FXButton.new(toolbar, nil, loadIcon("foxicon"), nil, 0,
      FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
  
    # Dangerous delete a bit on the side
    FXFrame.new(toolbar, (LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FIX_WIDTH|
      LAYOUT_FIX_HEIGHT), :width => 10, :height => 20)
    FXButton.new(toolbar, "\tDelete\tDelete the selected object.",
      loadIcon("kill"), @mdiclient, FXGLViewer::ID_DELETE_SEL,
      BUTTON_AUTOGRAY|FRAME_THICK|FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT)
  
    # View menu
    viewmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&View", nil, viewmenu)
    FXMenuCommand.new(viewmenu,
      "Parallel\t\tSwitch to parallel projection.", nil,
      @mdiclient, FXGLViewer::ID_PARALLEL, MENU_AUTOGRAY)
    FXMenuCommand.new(viewmenu,
      "Perspective\t\tSwitch to perspective projection.", nil,
      @mdiclient, FXGLViewer::ID_PERSPECTIVE, MENU_AUTOGRAY)
    FXMenuCommand.new(viewmenu, "&Front\tCtl-F\tFront view.", nil,
      @mdiclient, FXGLViewer::ID_FRONT, MENU_AUTOGRAY)
    FXMenuCommand.new(viewmenu, "&Back\tCtl-B\tBack view.", nil,
      @mdiclient, FXGLViewer::ID_BACK, MENU_AUTOGRAY)
    FXMenuCommand.new(viewmenu, "&Left\tCtl-L\tLeft view.", nil,
      @mdiclient, FXGLViewer::ID_LEFT, MENU_AUTOGRAY)
    FXMenuCommand.new(viewmenu, "&Right\tCtl-R\tRight view.", nil,
      @mdiclient, FXGLViewer::ID_RIGHT, MENU_AUTOGRAY)
    FXMenuCommand.new(viewmenu, "&Top\tCtl-T\tTop view.", nil,
      @mdiclient, FXGLViewer::ID_TOP, MENU_AUTOGRAY)
    FXMenuCommand.new(viewmenu, "&Bottom\tCtl-K\tBottom view.", nil,
      @mdiclient, FXGLViewer::ID_BOTTOM, MENU_AUTOGRAY)
    FXMenuCommand.new(viewmenu, "F&it\t\tFit to view.", nil,
      @mdiclient, FXGLViewer::ID_FITVIEW, MENU_AUTOGRAY)
    FXMenuCommand.new(viewmenu,
      "R&eset\tCtl-G\tReset all viewing parameters", nil,
      @mdiclient, FXGLViewer::ID_RESETVIEW, MENU_AUTOGRAY)
    FXMenuCommand.new(viewmenu, "Zoom...\t\tZoom in on area", nil,
      @mdiclient, FXGLViewer::ID_LASSO_ZOOM, MENU_AUTOGRAY)
    FXMenuCommand.new(viewmenu, "Select...\t\tZoom in on area", nil,
      @mdiclient, FXGLViewer::ID_LASSO_SELECT, MENU_AUTOGRAY)
  
    # Rendering menu
    rendermenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Rendering", nil, rendermenu)
    FXMenuCommand.new(rendermenu, "Points\t\tRender as points.", nil,
      @mdiclient, FXGLShape::ID_STYLE_POINTS, MENU_AUTOGRAY)
    FXMenuCommand.new(rendermenu, "Wire Frame\t\tRender as wire frame.", nil,
      @mdiclient, FXGLShape::ID_STYLE_WIREFRAME, MENU_AUTOGRAY)
    FXMenuCommand.new(rendermenu, "Surface \t\tRender solid surface.", nil,
      @mdiclient, FXGLShape::ID_STYLE_SURFACE, MENU_AUTOGRAY)
    FXMenuCommand.new(rendermenu,
      "Bounding Box\t\tRender bounding box only.", nil,
      @mdiclient, FXGLShape::ID_STYLE_BOUNDINGBOX, MENU_AUTOGRAY)
  
    # Window menu
    windowmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar,"&Windows", nil, windowmenu)
    newViewerCmd = FXMenuCommand.new(windowmenu, "New Viewer\t\tCreate new viewer window.")
    newViewerCmd.connect(SEL_COMMAND) do
      mdichild = FXMDIChild.new(@mdiclient, "GL Viewer #{@count}", @mdiicon,
        @mdimenu, MDI_NORMAL, 30, 30, 300, 200)
      view = FXGLViewer.new(mdichild, @glvisual, self, ID_GLVIEWER)
      view.scene = @scene
      mdichild.create
      @count += 1
    end
    FXMenuCommand.new(windowmenu,
      "Tile Horizontally\t\tTile windows horizontally.", nil,
      @mdiclient, FXMDIClient::ID_MDI_TILEHORIZONTAL)
    FXMenuCommand.new(windowmenu,
      "Tile Vertically\t\tTile windows vertically.", nil,
      @mdiclient, FXMDIClient::ID_MDI_TILEVERTICAL)
    FXMenuCommand.new(windowmenu, "Cascade\t\tCascade windows.", nil,
      @mdiclient, FXMDIClient::ID_MDI_CASCADE)
    FXMenuCommand.new(windowmenu, "Toolbar", nil,
      toolbar, FXWindow::ID_TOGGLESHOWN)
    FXMenuCommand.new(windowmenu, "Control panel", nil,
      panels, FXWindow::ID_TOGGLESHOWN)
    FXMenuCommand.new(windowmenu,
      "Delete\t\tDelete current viewer window.", nil,
      @mdiclient, FXMDIClient::ID_MDI_CLOSE)
    sep1 = FXMenuSeparator.new(windowmenu)
    sep1.setTarget(@mdiclient)
    sep1.setSelector(FXMDIClient::ID_MDI_ANY)
    FXMenuCommand.new(windowmenu, nil, nil, @mdiclient, FXMDIClient::ID_MDI_1)
    FXMenuCommand.new(windowmenu, nil, nil, @mdiclient, FXMDIClient::ID_MDI_2)
    FXMenuCommand.new(windowmenu, nil, nil, @mdiclient, FXMDIClient::ID_MDI_3)
    FXMenuCommand.new(windowmenu, nil, nil, @mdiclient, FXMDIClient::ID_MDI_4)
  
    # Help menu
    helpmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Help", nil, helpmenu, LAYOUT_RIGHT)
    aboutCmd = FXMenuCommand.new(helpmenu,
      "&About FOX...\t\tDisplay FOX about panel.")
    aboutCmd.connect(SEL_COMMAND) {
      FXMessageBox.information(self, MBOX_OK, "About FOX",
        "FOX OpenGL Example.\nCopyright (C) 1998 Jeroen van der Zijp")
    }
  
    # Make a tool tip
    FXToolTip.new(getApp(), 0)
  
    # The status bar shows our mode
    statusbar.statusLine.target = self
    statusbar.statusLine.selector = ID_QUERY_MODE
  
    # Make a scene!
    @scene = FXGLGroup.new
    gp2 = FXGLGroup.new
    @scene.append(gp2)
    sphere  = FXGLSphere.new(1.0, 1.0, 0.0, 0.5)
    sphere2 = FXGLSphere.new(0.0, 0.0, 0.0, 0.8)
    sphere.tipText = "Sphere"
    gp2.append(FXGLCube.new(-1.0, 0.0, 0.0,  1.0, 1.0, 1.0))
    gp2.append(FXGLCube.new( 1.0, 0.0, 0.0,  1.0, 1.0, 1.0))
    gp2.append(FXGLCube.new( 0.0,-1.0, 0.0,  1.0, 1.0, 1.0))
    gp2.append(FXGLCube.new( 0.0, 1.0, 0.0,  1.0, 1.0, 1.0))
    gp2.append(FXGLCone.new(1.0,-1.5, 0.0, 1.0, 0.5))
    gp2.append(FXGLCylinder.new(-1.0, 0.5, 0.0, 1.0, 0.5))
    gp2.append(sphere)
    gp2.append(sphere2)
  
    # Add scene to GL viewer
    viewer.scene = @scene
  end

  # Create and show the main window
  def create
    super
    show(PLACEMENT_SCREEN)
  end

  def onCmdOpen(sender, sel, ptr)
    dlg = FXFileDialog.new(self, "Open some file")
    dlg.setPatternList([
      "All Files (*)",
      "C++ Source Files (*.[Cc][Pp][Pp])",
      "C++ Header Files (*.[Hh])",
      "Object Files (*.o)",
      "HTML Files (*.[Hh][Tt][Mm][Ll]"])
    if dlg.execute() != 0
      FXMessageBox.information(self, MBOX_OK, "Huzzah!", "You selected the file: #{dlg.filename}")
    end
    return 1
  end

  def onUpdMode(sender, sel, ptr)
    sender.text = "Ready."
  end

  # When the user right-clicks in the GLViewer background, the viewer first
  # sends a SEL_COMMAND message with identifier FXWindow::ID_QUERY_MENU to
  # the selected GLObject (if any). If that message isn't handled, it tries
  # to send it to the GLViewer's target (which in our case is the main
  # window).
  def onQueryMenu(sender, sel, event)
    pane = FXMenuPane.new(self)
    FXMenuCommand.new(pane, "Parallel\t\tSwitch to parallel projection.",
      nil, sender, FXGLViewer::ID_PARALLEL)
    FXMenuCommand.new(pane, "Perspective\t\tSwitch to perspective projection.",
      nil, sender, FXGLViewer::ID_PERSPECTIVE)
    FXMenuSeparator.new(pane)
    FXMenuCommand.new(pane, "&Front\t\tFront view.", nil,
      sender,FXGLViewer::ID_FRONT)
    FXMenuCommand.new(pane, "&Back\t\tBack view.", nil,
      sender, FXGLViewer::ID_BACK)
    FXMenuCommand.new(pane, "&Left\t\tLeft view.", nil,
      sender, FXGLViewer::ID_LEFT)
    FXMenuCommand.new(pane, "&Right\t\tRight view.", nil,
      sender, FXGLViewer::ID_RIGHT)
    FXMenuCommand.new(pane, "&Top\t\tTop view.", nil,
      sender, FXGLViewer::ID_TOP)
    FXMenuCommand.new(pane, "&Bottom\t\tBottom view.", nil,
      sender, FXGLViewer::ID_BOTTOM)
    FXMenuSeparator.new(pane)
    FXMenuCommand.new(pane, "F&it\t\tFit to view.", nil,
      sender, FXGLViewer::ID_FITVIEW)
    FXMenuCommand.new(pane, "R&eset\t\tReset all viewing parameters", nil,
      sender, FXGLViewer::ID_RESETVIEW)
    pane.create
    pane.popup(nil, event.root_x, event.root_y)
    getApp().runModalWhileShown(pane)
    return 1
  end
end

if __FILE__ == $0
  # Make application
  application = FXApp.new("GLViewer", "FoxTest")
  application.threadsEnabled = false

  # Make window
  GLViewWindow.new(application)

  # Create the application windows
  application.create

  # Run the application
  application.run
end
