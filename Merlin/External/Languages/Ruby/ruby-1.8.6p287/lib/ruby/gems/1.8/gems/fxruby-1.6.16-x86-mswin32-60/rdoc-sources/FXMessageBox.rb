module Fox
  #
  # A Message Box is a convenience class which provides a dialog for
  # very simple common yes/no type interactions with the user.
  # The message box has an optional icon, a title string, and the question
  # which is presented to the user.  It also has up to three buttons which
  # furnish standard responses to the question.
  # Message boxes are usually run modally: the question must be answered
  # before the program may continue.
  #
  # === Message box buttons
  #
  # +MBOX_OK+::				Message box has a only an *Ok* button
  # +MBOX_OK_CANCEL+::			Message box has *Ok* and *Cancel* buttons
  # +MBOX_YES_NO+::			Message box has *Yes* and *No* buttons
  # +MBOX_YES_NO_CANCEL+::		Message box has *Yes*, *No*, and *Cancel* buttons
  # +MBOX_QUIT_CANCEL+::		Message box has *Quit* and *Cancel* buttons
  # +MBOX_QUIT_SAVE_CANCEL+::		Message box has *Quit*, *Save*, and *Cancel* buttons
  # +MBOX_SKIP_SKIPALL_CANCEL+::	Message box has *Skip*, *Skip All* and *Cancel* buttons
  # +MBOX_SAVE_CANCEL_DONTSAVE+::	Message box has *Don't Save*, *Cancel* and *Save* buttons
  #
  # === Return values
  #
  # +MBOX_CLICKED_YES+::	The *Yes* button was clicked
  # +MBOX_CLICKED_NO+::		The *No* button was clicked
  # +MBOX_CLICKED_OK+::		The *Ok* button was clicked
  # +MBOX_CLICKED_CANCEL+::	The *Cancel* button was clicked
  # +MBOX_CLICKED_QUIT+::	The *Quit* button was clicked
  # +MBOX_CLICKED_SAVE+::	The *Save* button was clicked
  # +MBOX_CLICKED_SKIP+::	The *Skip* button was clicked
  # +MBOX_CLICKED_SKIPALL+::	The *Skip All* button was clicked
  # +MBOX_CLICKED_DONTSAVE+:: The *Don't Save* button was clicked (same as +MBOX_CLICKED_NO+)
  #
  class FXMessageBox < FXDialogBox
    #
    # Construct message box with given caption, icon, and message text.
    # If _owner_ is a window, the message box will float over that window.
    # If _owner_ is the application, the message box will be free-floating.
    #
    def initialize(owner, caption, text, ic=nil, opts=0, x=0, y=0) # :yields: theMessageBox
    end

    #
    # Show a modal error message; returns one of the return values listed above.
    # If _owner_ is a window, the message box will float over that window.
    # If _owner_ is the application, the message box will be free-floating.
    #
    def FXMessageBox.error(owner, opts, caption, message); end
  
    #
    # Show a modal warning message; returns one of the return values listed above.
    # If _owner_ is a window, the message box will float over that window.
    # If _owner_ is the application, the message box will be free-floating.
    #
    def FXMessageBox.warning(owner, opts, caption, message); end
  
    #
    # Show a modal question dialog; returns one of the return values listed above.
    # If _owner_ is a window, the message box will float over that window.
    # If _owner_ is the application, the message box will be free-floating.
    #
    def FXMessageBox.question(owner, opts, caption, message); end
  
    #
    # Show a modal information dialog; returns one of the return values listed above.
    # If _owner_ is a window, the message box will float over that window.
    # If _owner_ is the application, the message box will be free-floating.
    #
    def FXMessageBox.information(owner, opts, caption, message); end

  end
end

