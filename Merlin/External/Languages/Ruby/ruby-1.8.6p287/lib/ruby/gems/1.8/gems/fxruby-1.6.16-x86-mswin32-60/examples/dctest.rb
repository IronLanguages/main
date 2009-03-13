require 'fox16'

include Fox

$bitmap_bits = [
   0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x03, 0x00, 0x00, 0x00,
   0x00, 0x00, 0x00, 0xc0, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa0,
   0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x11, 0x00, 0x00, 0x00,
   0x00, 0x00, 0x00, 0x88, 0x21, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x84,
   0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x82, 0x81, 0x00, 0x00, 0x00,
   0x00, 0x00, 0x00, 0x81, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x80, 0x80,
   0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x40, 0x80, 0x01, 0x04, 0x00, 0x00,
   0x00, 0x00, 0x20, 0x80, 0x01, 0x08, 0x00, 0x00, 0x00, 0x00, 0x10, 0x80,
   0x01, 0x10, 0x00, 0x00, 0x00, 0x00, 0x08, 0x80, 0x01, 0x20, 0x00, 0x00,
   0x00, 0x00, 0x04, 0x80, 0x01, 0x40, 0x00, 0x00, 0x00, 0x00, 0x02, 0x80,
   0x01, 0x80, 0x00, 0x00, 0x00, 0x00, 0x01, 0x80, 0x01, 0x00, 0x01, 0x00,
   0x00, 0x80, 0x00, 0x80, 0x01, 0x00, 0x02, 0x00, 0x00, 0x40, 0x00, 0x80,
   0x01, 0x00, 0x04, 0x00, 0x00, 0x20, 0x00, 0x80, 0x01, 0x00, 0x08, 0x00,
   0x00, 0x10, 0x00, 0x80, 0x01, 0x00, 0x10, 0x00, 0x00, 0x08, 0x00, 0x80,
   0x01, 0x00, 0x20, 0x00, 0x00, 0x04, 0x00, 0x80, 0x01, 0x00, 0x40, 0x00,
   0x00, 0x02, 0x00, 0x80, 0x01, 0x00, 0x80, 0x00, 0x00, 0x01, 0x00, 0x80,
   0x01, 0x00, 0x00, 0x01, 0x80, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0x02,
   0x40, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0x04, 0x20, 0x00, 0x00, 0x80,
   0x01, 0x00, 0x00, 0x08, 0x10, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0x10,
   0x08, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0x20, 0x04, 0x00, 0x00, 0x80,
   0x01, 0x00, 0x00, 0x40, 0x02, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0x80,
   0x01, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0x80,
   0x01, 0x00, 0x00, 0x40, 0x02, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0x20,
   0x04, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0x10, 0x08, 0x00, 0x00, 0x80,
   0x01, 0x00, 0x00, 0x08, 0x10, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0x04,
   0x20, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0x02, 0x40, 0x00, 0x00, 0x80,
   0x01, 0x00, 0x00, 0x01, 0x80, 0x00, 0x00, 0x80, 0x01, 0x00, 0x80, 0x00,
   0x00, 0x01, 0x00, 0x80, 0x01, 0x00, 0x40, 0x00, 0x00, 0x02, 0x00, 0x80,
   0x01, 0x00, 0x20, 0x00, 0x00, 0x04, 0x00, 0x80, 0x01, 0x00, 0x10, 0x00,
   0x00, 0x08, 0x00, 0x80, 0x01, 0x00, 0x08, 0x00, 0x00, 0x10, 0x00, 0x80,
   0x01, 0x00, 0x04, 0x00, 0x00, 0x20, 0x00, 0x80, 0x01, 0x00, 0x02, 0x00,
   0x00, 0x40, 0x00, 0x80, 0x01, 0x00, 0x01, 0x00, 0x00, 0x80, 0x00, 0x80,
   0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x01, 0x80, 0xff, 0xff, 0x00, 0x00,
   0x00, 0x00, 0x02, 0x80, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x04, 0x80,
   0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x08, 0x80, 0xff, 0xff, 0x00, 0x00,
   0x00, 0x00, 0x10, 0x80, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x20, 0x80,
   0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x40, 0x80, 0xff, 0xff, 0x00, 0x00,
   0x00, 0x00, 0x80, 0x80, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x81,
   0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x82, 0xff, 0xff, 0x00, 0x00,
   0x00, 0x00, 0x00, 0x84, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x88,
   0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0xff, 0xff, 0x00, 0x00,
   0x00, 0x00, 0x00, 0xa0, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0xc0,
   0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff]
   
$bitmap_bits = $bitmap_bits.pack("c*")
   
$blit_modes = {
  BLT_CLR => "Clear\tBLT_CLR",
  BLT_SRC_AND_DST => "And\tBLT_SRC_AND_DST",
  BLT_SRC_AND_NOT_DST => "AndRev\tBLT_SRC_AND_NOT_DST",
  BLT_SRC => "Copy\tBLT_SRC",
  BLT_NOT_SRC_AND_DST => "AndInv\tBLT_NOT_SRC_AND_DST",
  BLT_DST=> "NoOp\tBLT_DST",
  BLT_SRC_XOR_DST => "Xor\tBLT_SRC_XOR_DST",
  BLT_SRC_OR_DST => "Or\tBLT_SRC_OR_DST",
  BLT_NOT_SRC_AND_NOT_DST => "Nor\tBLT_NOT_SRC_AND_NOT_DST",
  BLT_NOT_SRC_XOR_DST => "Equiv\tBLT_NOT_SRC_XOR_DST",
  BLT_NOT_DST => "Invert\tBLT_NOT_DST",
  BLT_SRC_OR_NOT_DST => "OrRev\tBLT_SRC_OR_NOT_DST",
  BLT_NOT_SRC => "CopyInv\tBLT_NOT_SRC",
  BLT_NOT_SRC_OR_DST => "OrInv\tBLT_NOT_SRC_OR_DST",
  BLT_NOT_SRC_OR_NOT_DST => "Nand\tBLT_NOT_SRC_OR_NOT_DST",
  BLT_SET => "Set\tBLT_SET"
}

$fill_styles = {
  FILL_SOLID => "FILL_SOLID",
  FILL_TILED => "FILL_TILED",
  FILL_STIPPLED => "FILL_STIPPLED",
  FILL_OPAQUESTIPPLED => "FILL_OPAQUESTIPPLED"
}

$stipples = {
  STIPPLE_NONE => "NONE\tSTIPPLE_NONE",
  STIPPLE_BLACK => "BLACK\tSTIPPLE_BLACK",
  STIPPLE_WHITE => "WHITE\tSTIPPLE_WHITE",
  STIPPLE_0 => "0\tSTIPPLE_0",
  STIPPLE_1 => "1\tSTIPPLE_1",
  STIPPLE_2 => "2\tSTIPPLE_2",
  STIPPLE_3 => "3\tSTIPPLE_3",
  STIPPLE_4 => "4\tSTIPPLE_4",
  STIPPLE_5 => "5\tSTIPPLE_5",
  STIPPLE_6 => "6\tSTIPPLE_6",
  STIPPLE_7 => "7\tSTIPPLE_7",
  STIPPLE_8 => "8\tSTIPPLE_8",
  STIPPLE_9 => "9\tSTIPPLE_9",
  STIPPLE_10 => "10\tSTIPPLE_10",
  STIPPLE_11 => "11\tSTIPPLE_11",
  STIPPLE_12 => "12\tSTIPPLE_12",
  STIPPLE_13 => "13\tSTIPPLE_13",
  STIPPLE_14 => "14\tSTIPPLE_14",
  STIPPLE_15 => "15\tSTIPPLE_15",
  STIPPLE_16 => "16\tSTIPPLE_16",
  STIPPLE_HORZ => "HORZ\tSTIPPLE_HORZ",
  STIPPLE_VERT => "VERT\tSTIPPLE_VERT",
  STIPPLE_CROSS => "CROSS\tSTIPPLE_CROSS",
  STIPPLE_DIAG => "DIAG\tSTIPPLE_DIAG",
  STIPPLE_REVDIAG => "REVDIAG\tSTIPPLE_REVDIAG",
  STIPPLE_CROSSDIAG => "CROSSDIAG\tSTIPPLE_CROSSDIAG",
}

class DCTestWindow < FXMainWindow
  def initialize(app)
    # Initialize base class first
    super(app, "Device Context Test", :opts => DECOR_ALL, :width => 850, :height => 740)
        
    opts = FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y
        
    # Preferred line attributes
    @lineStyle = LINE_SOLID
    @capStyle = CAP_BUTT
    @joinStyle = JOIN_MITER
        
    # Create a tooltip
    tooltip = FXToolTip.new(getApp())

    # Menu bar
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)

    # Separator
    FXHorizontalSeparator.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X|SEPARATOR_GROOVE)

    # Contents
    contents = FXHorizontalFrame.new(self, LAYOUT_SIDE_TOP|FRAME_NONE|LAYOUT_FILL_X|LAYOUT_FILL_Y|PACK_UNIFORM_WIDTH)

    # Controls on right
    controls = FXVerticalFrame.new(contents, LAYOUT_RIGHT|LAYOUT_FILL_Y)

    # Block for BLIT modes
    FXLabel.new(controls, "BLIT Function:", nil, LAYOUT_LEFT)
    blitgrid = FXMatrix.new(controls, 4, FRAME_RIDGE|MATRIX_BY_COLUMNS|LAYOUT_FILL_X|PACK_UNIFORM_WIDTH,
      :padLeft => 2, :padRight => 2, :padTop => 2, :padBottom => 2)
	
    # One button for each mode
    $blit_modes.each do |blit_mode, desc|
      btn = FXButton.new(blitgrid, desc, :opts => BUTTON_TOOLBAR|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_COLUMN)
      btn.userData = blit_mode
      btn.connect(SEL_COMMAND) do |sender, sel, ptr|
        @function = sender.userData
        @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
      end
      btn.connect(SEL_UPDATE) do |sender, sel, ptr|
        if sender.userData == @function
          sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
        else
          sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
        end
      end
    end
	
    # Line dash style
    FXLabel.new(controls, "Line Style:", nil, LAYOUT_LEFT)
    linestyle = FXMatrix.new(controls, 3, FRAME_RIDGE|MATRIX_BY_COLUMNS|LAYOUT_FILL_X,
      :padLeft => 2, :padRight => 2, :padTop => 2, :padBottom => 2)
    lineSolidBtn = FXButton.new(linestyle, "\tLINE_SOLID", loadIcon("solid_line.png", 0, IMAGE_OPAQUE), :opts => BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_COLUMN)
    lineSolidBtn.connect(SEL_COMMAND) do |sender, sel, ptr|
      @lineStyle = LINE_SOLID
      @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
    end
    lineSolidBtn.connect(SEL_UPDATE) do |sender, sel, ptr|
      if @lineStyle == LINE_SOLID
        sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
      else
        sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
      end
    end
    lineOnOffDashBtn = FXButton.new(linestyle, "\tLINE_ONOFF_DASH", loadIcon("onoff_dash.png"), nil, 0, BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_COLUMN)
    lineOnOffDashBtn.connect(SEL_COMMAND) do |sender, sel, ptr|
      @lineStyle = LINE_ONOFF_DASH
      @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
    end
    lineOnOffDashBtn.connect(SEL_UPDATE) do |sender, sel, ptr|
      if @lineStyle == LINE_ONOFF_DASH
        sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
      else
        sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
      end
    end
    lineDoubleDashBtn = FXButton.new(linestyle, "\tLINE_DOUBLE_DASH", loadIcon("double_dash.png", 0,  IMAGE_OPAQUE), nil, 0, BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_COLUMN)
    lineDoubleDashBtn.connect(SEL_COMMAND) do |sender, sel, ptr|
      @lineStyle = LINE_DOUBLE_DASH
      @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
    end
    lineDoubleDashBtn.connect(SEL_UPDATE) do |sender, sel, ptr|
      if @lineStyle == LINE_DOUBLE_DASH
        sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
      else
        sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
      end
    end
    
    # Line cap style
    FXLabel.new(controls, "Cap Style:", nil, LAYOUT_LEFT)
    capstyle = FXMatrix.new(controls, 4, FRAME_RIDGE|MATRIX_BY_COLUMNS|LAYOUT_FILL_X)
    capstyle.padLeft = 2
    capstyle.padRight = 2
    capstyle.padTop = 2
    capstyle.padBottom = 2
    btn = FXButton.new(capstyle, "\tCAP_NOT_LAST", loadIcon("capnotlast.png"), nil, 0, BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_COLUMN)
    btn.connect(SEL_COMMAND) do |sender, sel, ptr|
      @capStyle = CAP_NOT_LAST
      @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
    end
    btn.connect(SEL_UPDATE) do |sender, sel, ptr|
      if @capStyle == CAP_NOT_LAST
        sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
      else
        sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
      end
    end
    btn = FXButton.new(capstyle, "\tCAP_BUTT", loadIcon("capbutt.png"), nil, 0, BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_COLUMN)
    btn.connect(SEL_COMMAND) do |sender, sel, ptr|
      @capStyle = CAP_BUTT
      @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
    end
    btn.connect(SEL_UPDATE) do |sender, sel, ptr|
      if @capStyle == CAP_BUTT
        sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
      else
        sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
      end
    end
    btn = FXButton.new(capstyle, "\tCAP_ROUND", loadIcon("capround.png"), nil, 0, BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_COLUMN)
    btn.connect(SEL_COMMAND) do |sender, sel, ptr|
      @capStyle = CAP_ROUND
      @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
    end
    btn.connect(SEL_UPDATE) do |sender, sel, ptr|
      if @capStyle == CAP_ROUND
        sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
      else
        sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
      end
    end
    btn = FXButton.new(capstyle, "\tCAP_PROJECTING", loadIcon("capproj.png"), nil, 0, BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_COLUMN)
    btn.connect(SEL_COMMAND) do |sender, sel, ptr|
      @capStyle = CAP_PROJECTING
      @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
    end
    btn.connect(SEL_UPDATE) do |sender, sel, ptr|
      if @capStyle == CAP_PROJECTING
        sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
      else
        sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
      end
    end
  	
    # Line join style
    FXLabel.new(controls, "Join Style:", nil, LAYOUT_LEFT)
    joinstyle = FXMatrix.new(controls, 3, FRAME_RIDGE|MATRIX_BY_COLUMNS|LAYOUT_FILL_X)
    joinstyle.padLeft = 2
    joinstyle.padRight = 2
    joinstyle.padTop = 2
    joinstyle.padBottom = 2
    btn = FXButton.new(joinstyle, "\tJOIN_MITER", loadIcon("jmiter.png"), nil, 0, BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_COLUMN)
    btn.connect(SEL_COMMAND) do |sender, sel, ptr|
      @joinStyle = JOIN_MITER
      @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
    end
    btn.connect(SEL_UPDATE) do |sender, sel, ptr|
      if @joinStyle == JOIN_MITER
        sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
      else
        sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
      end
    end
    btn = FXButton.new(joinstyle, "\tJOIN_ROUND", loadIcon("jround.png"), nil, 0, BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_COLUMN)
    btn.connect(SEL_COMMAND) do |sender, sel, ptr|
      @joinStyle = JOIN_ROUND
      @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
    end
    btn.connect(SEL_UPDATE) do |sender, sel, ptr|
      if @joinStyle == JOIN_ROUND
        sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
      else
        sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
      end
    end
    btn = FXButton.new(joinstyle, "\tJOIN_BEVEL", loadIcon("jbevel.png"), nil, 0, BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_COLUMN)
    btn.connect(SEL_COMMAND) do |sender, sel, ptr|
      @joinStyle = JOIN_BEVEL
      @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
    end
    btn.connect(SEL_UPDATE) do |sender, sel, ptr|
      if @joinStyle == JOIN_BEVEL
        sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
      else
        sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
      end
    end
  
    # Colors
    FXLabel.new(controls, "Colors:", nil, LAYOUT_LEFT)
    pairs = FXMatrix.new(controls, 2, FRAME_RIDGE|MATRIX_BY_COLUMNS|LAYOUT_FILL_X)
    pairs.padLeft = 2
    pairs.padRight = 2
    pairs.padTop = 2
    pairs.padBottom = 2
    pairs.hSpacing = 5
    pairs.vSpacing = 5
    
    # Back Color
    FXLabel.new(pairs, "Back Color:")
    @backWell = FXColorWell.new(pairs, FXRGB(0, 0, 255), nil, 0, FRAME_SUNKEN|FRAME_THICK|ICON_AFTER_TEXT|LAYOUT_FILL_X|LAYOUT_FILL_COLUMN)
    @backWell.connect(SEL_COMMAND) do |sender, sel, clr|
      @backcolor = clr
      @linesCanvas.update
      @shapesCanvas.update
      @imagesCanvas.update
    end
    @backWell.connect(SEL_CHANGED) do |sender, sel, clr|
      @backcolor = clr
      @linesCanvas.update
      @shapesCanvas.update
      @imagesCanvas.update
    end
    @backWell.connect(SEL_UPDATE) do |sender, sel, ptr|
      sender.handle(self, MKUINT(ID_SETVALUE, SEL_COMMAND), @backcolor)
    end
  
    # Fore Color
    FXLabel.new(pairs, "Fore Color:")
    @foreWell = FXColorWell.new(pairs, FXRGB(255, 0, 0), nil, 0, FRAME_SUNKEN|FRAME_THICK|ICON_AFTER_TEXT|LAYOUT_FILL_X|LAYOUT_FILL_COLUMN)
    @foreWell.connect(SEL_COMMAND) do |sender, sel, clr|
      @forecolor = clr
      @linesCanvas.update
      @shapesCanvas.update
      @imagesCanvas.update
    end
    @foreWell.connect(SEL_CHANGED) do |sender, sel, clr|
      @forecolor = clr
      @linesCanvas.update
      @shapesCanvas.update
      @imagesCanvas.update
    end
    @foreWell.connect(SEL_UPDATE) do |sender, sel, ptr|
      sender.handle(self, MKUINT(ID_SETVALUE, SEL_COMMAND), @forecolor)
    end
    
    # Erase Color
    FXLabel.new(pairs, "Erase Color:")
    @eraseWell = FXColorWell.new(pairs, FXRGB(255, 255, 255), nil, 0, FRAME_SUNKEN|FRAME_THICK|ICON_AFTER_TEXT|LAYOUT_FILL_X|LAYOUT_FILL_COLUMN)
    @eraseWell.connect(SEL_COMMAND) do |sender, sel, clr|
      @erasecolor = clr
      @linesCanvas.update
      @shapesCanvas.update
      @imagesCanvas.update
    end
    @eraseWell.connect(SEL_CHANGED) do |sender, sel, clr|
      @erasecolor = clr
      @linesCanvas.update
      @shapesCanvas.update
      @imagesCanvas.update
    end
    @eraseWell.connect(SEL_UPDATE) do |sender, sel, ptr|
      sender.handle(self, MKUINT(ID_SETVALUE, SEL_COMMAND), @erasecolor)
    end
    
    # Line width
    linew = FXMatrix.new(controls, 2, FRAME_RIDGE|MATRIX_BY_COLUMNS|LAYOUT_FILL_X)
    linew.padLeft = 2
    linew.padRight = 2
    linew.padTop = 2
    linew.padBottom = 2    
    FXLabel.new(linew, "Line Width:")
    @lineWidthSpinner = FXSpinner.new(linew, 4, nil, 0, SPIN_NORMAL|FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_COLUMN)
    @lineWidthSpinner.connect(SEL_COMMAND) { @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height) }
    @lineWidthSpinner.range = 1..255
    @lineWidthSpinner.value = 1
  
    # Stipple
    stip = FXMatrix.new(controls, 2, FRAME_RIDGE|MATRIX_BY_COLUMNS|LAYOUT_FILL_X)
    stip.padLeft = 2
    stip.padRight = 2
    stip.padTop = 2
    stip.padBottom = 2
    FXLabel.new(stip, "Stipples:")
    pop = FXPopup.new(self)
    
    $stipples.each do |pat, desc|
      opt = FXOption.new(pop, desc, nil, nil, 0, JUSTIFY_HZ_APART|ICON_AFTER_TEXT)
      opt.userData = pat
      opt.connect(SEL_COMMAND) do |sender, sel, ptr|
        @stipple = sender.userData
        @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
      end
    end
    FXOptionMenu.new(stip, pop, LAYOUT_TOP|FRAME_RAISED|FRAME_THICK|JUSTIFY_HZ_APART|ICON_AFTER_TEXT)
  
    # Fill Style
    FXLabel.new(controls, "Fill Style:", nil, LAYOUT_LEFT)
    fillstyle = FXMatrix.new(controls, 2, FRAME_RIDGE|MATRIX_BY_COLUMNS|LAYOUT_FILL_X)
    fillstyle.padLeft = 2
    fillstyle.padRight = 2
    fillstyle.padTop = 2
    fillstyle.padBottom = 2
    $fill_styles.each do |fs, desc|
      btn = FXButton.new(fillstyle, desc, nil, nil, 0, BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_COLUMN)
      btn.userData = fs
      btn.connect(SEL_COMMAND) do |sender, sel, ptr|
        @fillStyle = sender.userData
        @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
      end
      btn.connect(SEL_UPDATE) do |sender, sel, ptr|
        if sender.userData == @fillStyle
          sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
        else
          sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
        end
      end
    end
  
    # Angles for arcs
    FXLabel.new(controls, "Arc angles:", nil, LAYOUT_LEFT)
    arcangles = FXMatrix.new(controls, 3, FRAME_RIDGE|MATRIX_BY_COLUMNS|LAYOUT_FILL_X)
    arcangles.padLeft = 2
    arcangles.padRight = 2
    arcangles.padTop = 2
    arcangles.padBottom = 2
    arcangles.hSpacing = 4
    arcangles.vSpacing = 4

    @ang1 = FXDataTarget.new(0)
    FXLabel.new(arcangles, "Ang1:", nil, LAYOUT_LEFT)
    FXTextField.new(arcangles, 4, @ang1, FXDataTarget::ID_VALUE, TEXTFIELD_INTEGER|JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK)
    sang1 = FXSlider.new(arcangles, @ang1, FXDataTarget::ID_VALUE, LAYOUT_CENTER_Y|LAYOUT_FILL_X|SLIDER_INSIDE_BAR|LAYOUT_FILL_COLUMN)
    sang1.range = -360..360
    
    @ang2 = FXDataTarget.new(90)
    FXLabel.new(arcangles, "Ang2:", nil, LAYOUT_LEFT)
    FXTextField.new(arcangles, 4, @ang2, FXDataTarget::ID_VALUE, TEXTFIELD_INTEGER|JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK)
    sang2 = FXSlider.new(arcangles, @ang2, FXDataTarget::ID_VALUE, LAYOUT_CENTER_Y|LAYOUT_FILL_X|SLIDER_INSIDE_BAR|LAYOUT_FILL_COLUMN)
    sang2.range = -360..360
  
    # Font
    fonts = FXHorizontalFrame.new(controls, FRAME_RIDGE|LAYOUT_FILL_X|PACK_UNIFORM_WIDTH)
    fonts.padLeft = 2
    fonts.padRight = 2
    fonts.padTop = 2
    fonts.padBottom = 2
    btn = FXButton.new(fonts, "Font Dialog...\tChange the text font", nil, nil, 0, BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    btn.connect(SEL_COMMAND, method(:onCmdFont))
  
    # Printing
    printer = FXHorizontalFrame.new(controls, FRAME_RIDGE|LAYOUT_FILL_X|PACK_UNIFORM_WIDTH)
    printer.padLeft = 2
    printer.padRight = 2
    printer.padTop = 2
    printer.padBottom = 2
    btn = FXButton.new(printer, "Print Dialog...\tPrint it out", nil, nil, 0, BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    btn.connect(SEL_COMMAND, method(:onCmdPrint))
  
    # Quit
    quitter = FXHorizontalFrame.new(controls, FRAME_RIDGE|LAYOUT_FILL_X|PACK_UNIFORM_WIDTH)
    quitter.padLeft = 2
    quitter.padRight = 2
    quitter.padTop = 2
    quitter.padBottom = 2
    FXButton.new(quitter, "Bye Bye!\tHasta la vista, baby!", nil, getApp(), FXApp::ID_QUIT, BUTTON_TOOLBAR|JUSTIFY_CENTER_X|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_FILL_Y)
  
    # Switcher
    tabbook = FXTabBook.new(contents, nil, 0, LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_RIGHT)
    
    # First page shows various line styles
    linesTab = FXTabItem.new(tabbook, "&Lines", nil)
    linesPage = FXPacker.new(tabbook, FRAME_THICK|FRAME_RAISED)
    frame = FXHorizontalFrame.new(linesPage, FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    frame.padLeft = 0
    frame.padRight = 0
    frame.padTop = 0
    frame.padBottom = 0
    @linesCanvas = FXCanvas.new(frame, nil, 0, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    @linesCanvas.connect(SEL_PAINT) do |canvas, sel, ev|
      dc = FXDCWindow.new(canvas, ev)
      drawPage(dc, canvas.width, canvas.height)
    end
  
    # Second page shows various shapes
    shapesTab = FXTabItem.new(tabbook, "&Shapes", nil)
    shapesPage = FXPacker.new(tabbook, FRAME_THICK|FRAME_RAISED)
    frame = FXHorizontalFrame.new(shapesPage, FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    frame.padLeft = 0
    frame.padRight = 0
    frame.padTop = 0
    frame.padBottom = 0
    @shapesCanvas = FXCanvas.new(frame, nil, 0, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    @shapesCanvas.connect(SEL_PAINT) do |canvas, sel, ev|
      dc = FXDCWindow.new(canvas, ev)
      dc.foreground = @eraseWell.rgba
      dc.fillRectangle(0, 0, canvas.width, canvas.height)
      
      dc.foreground = @foreWell.rgba
      dc.background = @backWell.rgba
      dc.drawRectangle(5, 5, 50, 50)
      dc.fillRectangle(60, 5, 50, 50)
      
      dc.foreground = @foreWell.rgba
      dc.background = @backWell.rgba
      dc.drawArc(5, 60, 50, 50, 0, 64*90)
      dc.fillArc(60, 60, 50, 50, 64*90, 64*180)
      
      dc.foreground = @foreWell.rgba
      dc.background = @backWell.rgba
      dc.drawBitmap(@bitmap, 115, 5)
    end
    
    # Third page shows images
    imagesTab = FXTabItem.new(tabbook, "&Images", nil)
    imagesPage = FXPacker.new(tabbook, FRAME_THICK|FRAME_RAISED)
    frame = FXHorizontalFrame.new(imagesPage, FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    frame.padLeft = 0
    frame.padRight = 0
    frame.padTop = 0
    frame.padBottom = 0
    @imagesCanvas = FXCanvas.new(frame, nil, 0, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    @imagesCanvas.connect(SEL_PAINT) do |canvas, sel, ev|
      dc = FXDCWindow.new(canvas, ev)
      dc.foreground = @eraseWell.rgba
      dc.fillRectangle(0, 0, canvas.width, canvas.height)
      dc.drawImage(@birdImage, 0, 0)
    end
  
    # File menu
    @filemenu = FXMenuPane.new(self)
    FXMenuCommand.new(@filemenu, "&Print...\tCtl-P").connect(SEL_COMMAND, method(:onCmdPrint))
    FXMenuCommand.new(@filemenu, "&Font...\tCtl-F").connect(SEL_COMMAND, method(:onCmdFont))
    FXMenuCommand.new(@filemenu, "&Quit\tCtl-Q", nil, getApp(), FXApp::ID_QUIT)
    FXMenuTitle.new(menubar, "&File", nil, @filemenu)
    
    @birdImage = FXPNGImage.new(getApp(), File.open("icons/dippy.png", "rb").read)
    @bitmap = FXBitmap.new(getApp(), $bitmap_bits, 0, 64, 64)
    
    @function = BLT_SRC
    @lineStyle = LINE_SOLID
    @capStyle = CAP_BUTT
    @joinStyle = JOIN_MITER
    @fillStyle = FILL_SOLID
    @fillRule = RULE_EVEN_ODD
    @stipple = STIPPLE_NONE
    @forecolor = FXRGB(255, 0, 0) # red
    @backcolor = FXRGB(0, 0, 255) # blue
    @erasecolor = FXRGB(255, 255, 255) # white
    @testFont = FXFont.new(getApp(), "helvetica", 20)
  end
  
  def create
    super
    @birdImage.create
    @testFont.create
    @bitmap.create
    show(PLACEMENT_SCREEN)
  end
  
  def detach
    super
    @birdImage.detach
    @testFont.detach
    @bitmap.detach
  end
  
  def drawPage(dc, w, h)
    dc.foreground = @erasecolor
    dc.fillRectangle(0, 0, w, h)
    
    dc.foreground = @forecolor
    dc.background = @backcolor
    
    dc.lineStyle = @lineStyle
    dc.lineCap = @capStyle
    dc.lineJoin = @joinStyle
    dc.function = @function
    
    dc.stipple = @stipple
    dc.fillStyle = @fillStyle
    dc.lineWidth = @lineWidthSpinner.value
    
    # Here's a single line
    dc.drawLine(20, 200, w - 20, 200)
    
    # Here are some connected lines (to show join styles)
    points = []
    points << FXPoint.new(10, 3*h/4)
    points << FXPoint.new(points[0].x+w/6, h/2)
    points << FXPoint.new(points[1].x+w/6, points[0].y)
    points << FXPoint.new(points[2].x+w/6, points[1].y)
    points << FXPoint.new(points[3].x+w/6, points[0].y)
    points << FXPoint.new(points[4].x+w/6, points[1].y)
    dc.drawLines(points)
    
    dc.font = @testFont
    dc.foreground = @forecolor
    dc.background = @backcolor
    s = "Font: #{@testFont.name}  Size: #{@testFont.size/10}"
    dc.drawText(30, h-70, s)
    dc.drawImageText(30, h-30, s)
    
    dc.foreground = @forecolor
    dc.background = @backcolor
    dc.drawRectangle(20, 20, 200, 100)
    dc.fillRectangle(300, 20, 200, 100)
    
    dc.drawArc(20, 120, 100, 100, 64*@ang1.value, 64*@ang2.value)
    dc.fillArc(300, 120, 100, 100, 64*@ang1.value, 64*@ang2.value)
    
    poly = []
    poly << FXPoint.new(200, 230)
    poly << FXPoint.new(poly[0].x+40, poly[0].y+20)
    poly << FXPoint.new(poly[0].x+30, poly[0].y+60)
    poly << FXPoint.new(poly[0].x-30, poly[0].y+60)
    poly << FXPoint.new(poly[0].x-40, poly[0].y+20)
    dc.fillPolygon(poly)
    
    poly = []
    poly << FXPoint.new(300, 230)
    poly << FXPoint.new(poly[0].x+30, poly[0].y+60)
    poly << FXPoint.new(poly[0].x-40, poly[0].y+20)
    poly << FXPoint.new(poly[0].x+40, poly[0].y+20)
    poly << FXPoint.new(poly[0].x-30, poly[0].y+60)
    dc.fillComplexPolygon(poly)

    poly = []
    poly << FXPoint.new(400, 230)
    poly << FXPoint.new(poly[0].x+30, poly[0].y+60)
    poly << FXPoint.new(poly[0].x-40, poly[0].y+20)
    poly << FXPoint.new(poly[0].x+40, poly[0].y+20)
    poly << FXPoint.new(poly[0].x-30, poly[0].y+60)
    dc.fillRule = RULE_WINDING
    dc.fillComplexPolygon(poly)
    
    concave = []
    concave << FXPoint.new(w-100, h-100)
    concave << FXPoint.new(concave[0].x+40, concave[0].y-20)
    concave << FXPoint.new(concave[0].x   , concave[0].y+40)
    concave << FXPoint.new(concave[0].x-40, concave[0].y-20)
    dc.fillConcavePolygon(concave)
    
    # Draw a pale blue dot ;)
    dc.foreground = FXRGB(128, 128, 255)
    dc.drawPoint(w-20, h-20)
  end
  
  def onCmdFont(sender, sel, ptr)
    fontdlg = FXFontDialog.new(self, "Change Font", DECOR_BORDER|DECOR_TITLE)
    fontdlg.fontSelection = @testFont.fontDesc
    if fontdlg.execute != 0
      @testFont = FXFont.new(getApp(), fontdlg.fontSelection)
      @testFont.create
      @linesCanvas.update(0, 0, @linesCanvas.width, @linesCanvas.height)
    end
    return 1
  end
  
  def onCmdPrint(sender, sel, ptr)
    dlg = FXPrintDialog.new(self, "Print Graphics")
    if dlg.execute != 0
      p = dlg.printer
      pdc = FXDCPrint.new(getApp())
      unless pdc.beginPrint(p)
        FXMessageBox.error(self, MBOX_OK, "Printer Error", "Unable to print")
        return 1
      end
      pdc.beginPage(1)
      drawPage(pdc, 500, 500)
      pdc.endPage
      pdc.endPrint
    end
    return 1
  end
  
  # Load the named icon from a file
  def loadIcon(filename, clr = FXRGB(192, 192, 192), opts = 0)
    begin
      filename = File.join("icons", filename)
      icon = nil
      File.open(filename, "rb") { |f|
        icon = FXPNGIcon.new(getApp(), f.read, clr, opts)
      }
      icon
    rescue
      raise RuntimeError, "Couldn't load icon: #{filename}"
    end
  end
end

if __FILE__ == $0
  application = FXApp.new("DCTest", "FoxTest")
  DCTestWindow.new(application)
  application.create
  application.run
end

