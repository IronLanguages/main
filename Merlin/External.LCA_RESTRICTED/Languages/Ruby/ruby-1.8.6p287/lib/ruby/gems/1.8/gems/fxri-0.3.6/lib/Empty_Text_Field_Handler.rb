# Copyright (c) 2004, 2005 Martin Ankerl
# Shows a grey text in an FXTextField if the user did not enter any input. This is a nice way to
# give the user more information about what to enter into a text field, without the need of additional
# space in the GUI.
class Empty_Text_Field_Handler

  # Create a new handler for the specified textfield, with the given text. From now on you have to use the
  # created object to get and set text, not the textfield or this handler would come out of sync
  def initialize(textField, myText)
    @textField = textField
    @myText = myText
    @isEmpty = true
    onTextFieldFocusOut
    # create connections
    @textField.connect(SEL_FOCUSIN, method(:onTextFieldFocusIn))
    @textField.connect(SEL_FOCUSOUT, method(:onTextFieldFocusOut))
  end

  # Check if textfield is empty (no user input).
  def empty?
    @isEmpty
  end

  # Set new text for the textfield
  def text=(newText)
    onTextFieldFocusIn
    @textField.text = newText.to_s
    onTextFieldFocusOut
  end

  # Get the textfield's text, if the user has entered something.
  def text
    if empty? && !@inside
      ""
    else
      @textField.text
    end
  end

  # Set focus to the textfield.
  def setFocus
    @textField.setFocus
  end

  private

  def onTextFieldFocusIn(*args)
    @inside = true
    return if !@isEmpty
    @textField.textColor = FXColor::Black
    @textField.text = ""
  end

  def onTextFieldFocusOut(*args)
    @inside = false
    @textField.killSelection
    @isEmpty = @textField.text == ""
    return if !@isEmpty
    @textField.textColor = FXColor::DarkGrey
    @textField.text = @myText
    @isEmpty = true
  end
end
