#/usr/bin/env ruby

require 'fox16'

include Fox

class ButtonWindow < FXMainWindow

  def initialize(app)
    # Invoke base class initialize first
    super(app, "Button Test", :opts => DECOR_ALL, :x => 100, :y => 100)

    # Create a tooltip
    FXToolTip.new(self.getApp())

    # Status bar
    statusbar = FXStatusBar.new(self,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|STATUSBAR_WITH_DRAGCORNER)

    # Controls on the right
    controls = FXVerticalFrame.new(self,
      LAYOUT_SIDE_RIGHT|LAYOUT_FILL_Y|PACK_UNIFORM_WIDTH)

    # Separator
    FXVerticalSeparator.new(self,
      LAYOUT_SIDE_RIGHT|LAYOUT_FILL_Y|SEPARATOR_GROOVE)

    # Contents
    contents = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_LEFT|FRAME_NONE|LAYOUT_FILL_X|LAYOUT_FILL_Y|PACK_UNIFORM_WIDTH,
      :padding => 20)

    # Construct icon from a PNG file on disk
    bigpenguin = loadIcon("bigpenguin.png")

    # The button
    @button = FXButton.new(contents,
      "&This is a multi-line label on\na button to show off the full\n" +
      "capabilities of the button object.\t" +
      "It also has a tooltip\n(which, by the way, can be multi-line also).\t" +
      "Here's a helpful message for the status line.",
      bigpenguin,
      :opts => FRAME_RAISED|FRAME_THICK|LAYOUT_CENTER_X|LAYOUT_CENTER_Y|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT,
      :width => 300, :height => 200)

    checkButton = FXCheckButton.new(controls, "Toolbar Style\tCool \"poppy\" style buttons")
    checkButton.connect(SEL_COMMAND) do |sender, sel, checked|
      if checked
        @button.buttonStyle |= BUTTON_TOOLBAR
        @button.frameStyle = FRAME_RAISED
      else
        @button.buttonStyle &= ~BUTTON_TOOLBAR
        @button.frameStyle = FRAME_RAISED|FRAME_THICK
      end
    end

    group1 = FXGroupBox.new(controls, "Horizontal Placement",
      GROUPBOX_TITLE_CENTER|FRAME_RIDGE)
    @group1_dt = FXDataTarget.new(2)
    @group1_dt.connect(SEL_COMMAND) do
      case @group1_dt.value
        when 0
          @button.iconPosition = (@button.iconPosition|ICON_BEFORE_TEXT) & ~ICON_AFTER_TEXT
        when 1
          @button.iconPosition = (@button.iconPosition|ICON_AFTER_TEXT) & ~ICON_BEFORE_TEXT
        when 2
          @button.iconPosition = (@button.iconPosition & ~ICON_AFTER_TEXT) & ~ICON_BEFORE_TEXT
      end
    end
    FXRadioButton.new(group1, "Before Text", @group1_dt, FXDataTarget::ID_OPTION)
    FXRadioButton.new(group1, "After Text",  @group1_dt, FXDataTarget::ID_OPTION + 1)
    FXRadioButton.new(group1, "Centered",    @group1_dt, FXDataTarget::ID_OPTION + 2)

    group2 = FXGroupBox.new(controls, "Vertical Placement",
      GROUPBOX_TITLE_CENTER|FRAME_RIDGE)
    @group2_dt = FXDataTarget.new(2)
    @group2_dt.connect(SEL_COMMAND) do
      case @group2_dt.value
        when 0
          @button.iconPosition = (@button.iconPosition|ICON_ABOVE_TEXT) & ~ICON_BELOW_TEXT
        when 1
          @button.iconPosition = (@button.iconPosition|ICON_BELOW_TEXT) & ~ICON_ABOVE_TEXT
        when 2
          @button.iconPosition = (@button.iconPosition & ~ICON_ABOVE_TEXT) & ~ICON_BELOW_TEXT
      end
    end
    FXRadioButton.new(group2, "Above Text", @group2_dt, FXDataTarget::ID_OPTION)
    FXRadioButton.new(group2, "Below Text", @group2_dt, FXDataTarget::ID_OPTION + 1)
    FXRadioButton.new(group2, "Centered",   @group2_dt, FXDataTarget::ID_OPTION + 2)

    group3 = FXGroupBox.new(controls, "Horizontal Justification",
      GROUPBOX_TITLE_CENTER|FRAME_RIDGE)
    @group3_dt = FXDataTarget.new(0)
    @group3_dt.connect(SEL_COMMAND) do
      case @group3_dt.value
        when 0
          @button.justify &= ~JUSTIFY_HZ_APART
        when 1
          @button.justify = (@button.justify & ~JUSTIFY_HZ_APART) | JUSTIFY_LEFT
        when 2
          @button.justify = (@button.justify & ~JUSTIFY_HZ_APART) | JUSTIFY_RIGHT
        when 3
          @button.justify |= JUSTIFY_HZ_APART
       end
    end
    FXRadioButton.new(group3, "Center", @group3_dt, FXDataTarget::ID_OPTION)
    FXRadioButton.new(group3, "Left",   @group3_dt, FXDataTarget::ID_OPTION + 1)
    FXRadioButton.new(group3, "Right",  @group3_dt, FXDataTarget::ID_OPTION + 2)
    FXRadioButton.new(group3, "Apart",  @group3_dt, FXDataTarget::ID_OPTION + 3)

    group4 = FXGroupBox.new(controls, "Vertical Justification",
      GROUPBOX_TITLE_CENTER|FRAME_RIDGE)
    @group4_dt = FXDataTarget.new(0)
    @group4_dt.connect(SEL_COMMAND) do
      case @group4_dt.value
        when 0
          @button.justify &= ~JUSTIFY_VT_APART
        when 1
          @button.justify = (@button.justify & ~JUSTIFY_VT_APART) | JUSTIFY_TOP
        when 2
          @button.justify = (@button.justify & ~JUSTIFY_VT_APART) | JUSTIFY_BOTTOM
        when 3
          @button.justify |= JUSTIFY_VT_APART
      end
    end
    FXRadioButton.new(group4, "Center", @group4_dt, FXDataTarget::ID_OPTION)
    FXRadioButton.new(group4, "Top",    @group4_dt, FXDataTarget::ID_OPTION + 1)
    FXRadioButton.new(group4, "Bottom", @group4_dt, FXDataTarget::ID_OPTION + 2)
    FXRadioButton.new(group4, "Apart",  @group4_dt, FXDataTarget::ID_OPTION + 3)

    quitButton = FXButton.new(controls, "&Quit", :opts => FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_X)
    quitButton.connect(SEL_COMMAND) { getApp().exit(0) }
  end

  # Load the named icon from a file
  def loadIcon(filename)
    begin
      filename = File.join("icons", filename)
      icon = nil
      File.open(filename, "rb") do |f|
        icon = FXPNGIcon.new(getApp(), f.read)
      end
      icon
    rescue
      raise RuntimeError, "Couldn't load icon: #{filename}"
    end
  end

  def create
    super
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  # Construct an application
  application = FXApp.new("Button", "FoxTest")

  # Construct the main window
  ButtonWindow.new(application)

  # Create the application
  application.create

  # Run it
  application.run
end

