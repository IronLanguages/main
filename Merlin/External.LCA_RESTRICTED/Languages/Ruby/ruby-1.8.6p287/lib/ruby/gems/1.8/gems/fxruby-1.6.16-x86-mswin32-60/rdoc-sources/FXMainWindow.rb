module Fox
  #
  # The Main Window is usually the central window of an application.  Applications
  # may have any number of main windows.
  # When a MainWindow is closed, it sends a SEL_CLOSE message to its target; the target
  # should return 0 if there is no objection against proceeding with the close, and
  # return 1 otherwise.
  # After the SEL_CLOSE message has been sent and no objection was raised, the main
  # window will delete itself.
  #
  # === Events
  #
  # The following messages are sent by FXMainWindow to its target:
  #
  # +SEL_CLOSE+::
  #   sent when the user clicks the close button in the upper right-hand
  #   corner of the main window.
  #
  class FXMainWindow < FXTopWindow
    #
    # Construct a main window
    #
    def initialize(app, title, icon=nil, miniIcon=nil, opts=DECOR_ALL, x=0, y=0, width=0, height=0, padLeft=0, padRight=0, padTop=0, padBottom=0, hSpacing=4, vSpacing=4) # :yields: theMainWindow
    end
  end
end
