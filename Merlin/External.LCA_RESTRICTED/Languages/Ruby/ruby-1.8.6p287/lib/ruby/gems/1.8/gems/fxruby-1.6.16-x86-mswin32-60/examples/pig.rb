#
# This is a FOX version of Thomas and Hunt's timeless classic,
# the Pig It! example (from the "Ruby/Tk" chapter of "Programming
# Ruby".
#

require 'fox16'

include Fox

class PigBox < FXMainWindow
  def pig(word)
    leadingCap = word =~ /^A-Z/
    word.downcase!
    res = case word
      when /^aeiouy/
        word+"way"
      when /^([^aeiouy]+)(.*)/
        $2+$1+"ay"
      else
        word
    end
    leadingCap ? res.capitalize : res
  end

  def showPig
    @text.value = @text.value.split.collect{|w| pig(w)}.join(" ")
  end

  def initialize(app)
    # Initialize base class
    super(app, "Pig")
    
    @text = FXDataTarget.new("")
    
    top = FXVerticalFrame.new(self, LAYOUT_FILL_X|LAYOUT_FILL_Y) do |theFrame|
      theFrame.padLeft = 10
      theFrame.padRight = 10
      theFrame.padBottom = 10
      theFrame.padTop = 10
      theFrame.vSpacing = 20
    end
    
    p = proc { showPig }
    
    FXLabel.new(top, 'Enter Text:') do |theLabel|
      theLabel.layoutHints = LAYOUT_FILL_X
    end
    
    FXTextField.new(top, 20, @text, FXDataTarget::ID_VALUE) do |theTextField|
      theTextField.layoutHints = LAYOUT_FILL_X
      theTextField.setFocus()
    end
    
    FXButton.new(top, 'Pig It') do |pigButton|
      pigButton.connect(SEL_COMMAND, p)
      pigButton.layoutHints = LAYOUT_CENTER_X
    end
    
    FXButton.new(top, 'Exit') do |exitButton|
      exitButton.connect(SEL_COMMAND) { exit }
      exitButton.layoutHints = LAYOUT_CENTER_X
    end
  end
  
  def create
    super
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  app = FXApp.new("Pig It", "FXRuby")
  PigBox.new(app)
  app.create
  app.run
end
