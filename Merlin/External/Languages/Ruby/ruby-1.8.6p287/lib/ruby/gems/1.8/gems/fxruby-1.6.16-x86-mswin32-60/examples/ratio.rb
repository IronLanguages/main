#!/usr/bin/env ruby
# ratio.rb  
# Translated from FOX C++ version to Ruby (Dec 2004) by Brett Hallett.
# Demonstrates the use of the FXSpring method to retain size ratios between widgets when form is resized.
#

require 'fox16'
require 'fox16/colors'

include Fox

class MainWindow < FXMainWindow
  
  def initialize(app)
	 # Initialize base class first
    super(app, 'Ratio', :opts => DECOR_ALL,
      :width => 400, :height => 200,
      :padLeft => 8, :padRight => 8, :padTop => 8, :padBottom =>8,
      :hSpacing => 6, :vSpacing => 6)

    # Add quit button and connect it to application
    FXButton.new(self, "&Quit", nil, app, FXApp::ID_QUIT,
      :opts => FRAME_RAISED|FRAME_THICK|LAYOUT_SIDE_BOTTOM|LAYOUT_CENTER_X,
      :padLeft => 20, :padRight => 20, :padTop => 2, :padBottom => 2)

    # Label above it
    FXLabel.new(self,
      "FXSpring can be used to keep widgets at fixed size ratios.\n\nResize the window to see how it behaves!",
      :opts => LAYOUT_SIDE_TOP|LAYOUT_FILL_X)

    # Layout manager to place the springs
    horz = FXHorizontalFrame.new(self, FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y,
    :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0,
    :hSpacing => 0, :vSpacing => 0)

	 # First spring is set to behave normally in Y direction,
	 # but to have a ratio 1 for the X direction
	 FXSpring.new(horz, LAYOUT_FILL_X|LAYOUT_FILL_Y, :relw => 1, :padding => 0) do |spring|
	   FXLabel.new(spring, "1", :opts => LAYOUT_FILL_X|LAYOUT_FILL_Y) do |label|
	     label.backColor = FXColor::Red
      end
    end

    # Second spring has ratio 2 in the X direction
    FXSpring.new(horz, LAYOUT_FILL_X|LAYOUT_FILL_Y, :relw => 2, :padding => 0) do |spring|
      FXLabel.new(spring, "2", :opts => LAYOUT_FILL_X|LAYOUT_FILL_Y) do |label|
        label.backColor = FXColor::Green
      end
    end

    # Third spring has ratio 3 in the X direction
    FXSpring.new(horz, LAYOUT_FILL_X|LAYOUT_FILL_Y, :relw => 3, :padding => 0) do |spring|
      puts "pl, pr, pt, pb = #{spring.padLeft}, #{spring.padRight}, #{spring.padTop}, #{spring.padBottom}"
      FXLabel.new(spring, "3", :opts => LAYOUT_FILL_X|LAYOUT_FILL_Y) do |label|
        label.backColor = FXColor::Blue
      end
    end
  end

  def create
    super 
    show(PLACEMENT_SCREEN) 
  end # create 

end  # class MainWindow 

if __FILE__ == $0
  # Construct an application
  FXApp.new('Smithy', 'Max') do |theApp|

    # Construct the main window
    MainWindow.new(theApp) 

    # Create and show the application windows  
    theApp.create 

    # Run the application
    theApp.run
  end
end
