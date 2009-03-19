#
# Splashscreen component, contributed by David Naseby.
#

require 'fox16/colors'

module Fox
  #
  # The FXSplashScreen window...
  #
  class FXSplashScreen < FXDialogBox

    #
    # Return an initialized FXSplashScreen instance.
    #
    # ==== Parameters:
    #
    # +owner+::	Owner window for this dialog box [FXWindow]
    # +title+::	Title string for this dialog box [String]
    # +text+::	   Message text for this dialog box [String]
    # +action+::	The action
    #
    def initialize( owner, title, text, action )
	   # Initialize the base class first
      super(owner, title)

      # Store the action block
      @action = action
    
      # Construct the window contents
      FXVerticalFrame.new( self ) do |frame|
        text.each_line do |line|
          FXLabel.new( frame, line.strip )
        end
        FXLabel.new( frame, "Click OK to continue (this may take a few moments)...." )
        @status = FXLabel.new( frame, " " )
        @accept = FXButton.new( frame, "&OK", nil, self, ID_ACCEPT,
          FRAME_RAISED|FRAME_THICK|LAYOUT_RIGHT|LAYOUT_CENTER_Y)
        @accept.enabled = false
      end
    end

    def execute(placement = PLACEMENT_OWNER)
      Thread.new do
        sleep 1
        @action.call method( :update_status )
        update_status "Completed"
        @accept.enabled = true
      end
      super
    end
  
    def update_status(msg)
      @status.text = msg
    end
  end
end

if $0 == __FILE__
  class FakeSite # :nodoc:
    def open( &status )
      yield ">>>>> Opening Site" if block_given?
      sleep 5
      yield "site open" if block_given?
    end
  end
  fake_site = FakeSite.new
  
  include Fox

  FXApp.new( "Test SplashScreen" ) do |theApp|
    FXMainWindow.new( theApp, "Hello" ) do |mainWin|
      FXButton.new( mainWin, "Show Splash" ).connect( SEL_COMMAND ) do
        lv = FXSplashScreen.new( mainWin, "Opening Site", "Welcome to Sitebuilder!\nOpening the site.\n",  lambda{ |proc| fake_site.open( &proc ) } )
        lv.execute
      end
      mainWin.show
    end
    theApp.create
    theApp.run
  end
end
