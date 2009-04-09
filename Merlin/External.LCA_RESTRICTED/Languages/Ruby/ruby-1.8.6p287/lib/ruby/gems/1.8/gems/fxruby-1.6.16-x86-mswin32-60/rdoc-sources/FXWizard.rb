module Fox
  #
  # An FXWizard widget guides the user through a number of panels
  # in a predefined sequence; each step must be completed before
  # moving on to the next step.
  # For example, an FXWizard may be used to install software components,
  # and ask various questions at each step in the installation.
  #
  # === Message identifiers
  #
  # +ID_NEXT+::		Move to the next panel in the wizard
  # +ID_BACK+::		Move to the previous panel in the wizard
  #
  class FXWizard < FXDialogBox
  
    # The button frame [FXHorizontalFrame]
    attr_reader :buttonFrame
    
    # The "Advance" button [FXButton]
    attr_reader :advanceButton
    
    # The "Retreat" button [FXButton]
    attr_reader :retreatButton
    
    # The "Finish" button [FXButton]
    attr_reader :finishButton
    
    # The "Cancel" button [FXButton]
    attr_reader :cancelButton
    
    # The container used as parent for the sub-panels [FXSwitcher]
    attr_reader :container
    
    # The image being displayed [FXImage]
    attr_accessor :image
    
    #
    # Return an initialized FXWizard instance.
    # If _owner_ is a window, the dialog box will float over that window.
    # If _owner_ is the application, the dialog box will be free-floating.
    #
    def initialize(owner, name, image, opts=DECOR_TITLE|DECOR_BORDER|DECOR_RESIZE, x=0, y=0, width=0, height=0, padLeft=10, padRight=10, padTop=10, padBottom=10, hSpacing=10, vSpacing=10) # :yields: theWizard
    end

    # Return the number of panels.
    def numPanels; end

    #
    # Bring the child window at _index_ to the top.
    # Raises IndexError if _index_ is out of bounds.
    #
    def currentPanel=(index); end

    #
    # Return the index of the child window currently on top.
    #
    def currentPanel; end
  end
end
