#!/usr/bin/env ruby

require 'fox16'
require 'tranexp'

include Fox

TRANSLATIONS = {
  "x" => "y"
}

class Babelfish < FXMainWindow

  def initialize(app)
    # Invoke base class initialize first
    super(app, "Babelfish", :opts => DECOR_ALL, :width => 600, :height => 400)

    @translator = Tranexp::Http.new
    
    # Controls area along the bottom
    controlsFrame = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X)
    FXLabel.new(controlsFrame, "Translate from:")
    @fromCombo = FXComboBox.new(controlsFrame, 15, :opts => COMBOBOX_STATIC|FRAME_SUNKEN|FRAME_THICK)
    @fromCombo.numVisible = 6
    FXLabel.new(controlsFrame, " to:")
    @toCombo = FXComboBox.new(controlsFrame, 15, :opts => COMBOBOX_STATIC|FRAME_SUNKEN|FRAME_THICK)
    @toCombo.numVisible = 6
    Tranexp::Codes.constants.each do |lang|
      @fromCombo.appendItem(lang)
      @toCombo.appendItem(lang)
    end
    btn = FXButton.new(controlsFrame, "Translate", :opts => BUTTON_NORMAL|LAYOUT_RIGHT)
    btn.connect(SEL_COMMAND) do
      from = @fromCombo.getItemText(@fromCombo.currentItem)
      to = @toCombo.getItemText(@toCombo.currentItem)
      getApp().beginWaitCursor() do
        @translatedText.text = @translator.translate(@sourceText.text, from, to)
      end
    end

    mainFrame = FXVerticalFrame.new(self,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y|PACK_UNIFORM_HEIGHT)

    # Source text area in a sunken frame
    topFrame = FXVerticalFrame.new(mainFrame, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(topFrame, "Source Text:", :opts => LAYOUT_FILL_X)
    sunkenFrame = FXHorizontalFrame.new(topFrame,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    @sourceText = FXText.new(sunkenFrame, :opts => LAYOUT_FILL_X|LAYOUT_FILL_Y)

    # Translated text output
    bottomFrame = FXVerticalFrame.new(mainFrame, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(bottomFrame, "Translated text:", nil, LAYOUT_FILL_X)
    sunkenFrame = FXHorizontalFrame.new(bottomFrame,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    @translatedText = FXText.new(sunkenFrame, :opts => TEXT_READONLY|LAYOUT_FILL_X|LAYOUT_FILL_Y)
  end
  
  def create
    super
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  FXApp.new("Babelfish", "FoxTest") do |app|
    Babelfish.new(app)
    app.create
    app.run
  end
end
