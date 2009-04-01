#!/usr/bin/env ruby

require 'fox16'

include Fox

=begin
This is the "Cadillac" version of the classic "Hello, World" example;
it has not only an icon, but also a tooltip, and an accelerator.
 
Executing an FXIcon's constructor will cause it to deserialize the pixel-
data by associating a memory stream with the data array; the resulting icon
object will contain a pixel-array, which will be converted to an off-screen
X pixmap when the icons create() method is called.  At that point, the
temporary (client-side) pixel storage will be freed.
=end  

# Construct the application object, with application name "Hello2"
# and vendor key "FoxTest". These strings are primarily used for
# interactions with the FOX registry.

application = FXApp.new("Hello2", "FoxTest")

# Construct the main window
main = FXMainWindow.new(application, "Hello", nil, nil, DECOR_ALL)

# Construct a PNG icon that we'll attach to the button. Note that the
# second argument to the constructor just needs to be a byte stream (i.e.
# a string) from some source; here, we're reading the bytes from a file
# on disk.

icon = nil
File.open(File.join("icons", "hello2.png"), "rb") { |f|
  icon = FXPNGIcon.new(application, f.read)
}

# Construct the button as a child of the main window.
FXButton.new(main, "&Hello, World!\tWow, FOX is really cool!\nClick on the icon to quit the application.", icon, application, FXApp::ID_QUIT, ICON_UNDER_TEXT|JUSTIFY_BOTTOM)

# Construct the tooltip object
FXToolTip.new(application)

# Calling create() on the application recursively creates all of its
# owned resources (e.g. the application's windows)
application.create

# Show the main window
main.show(PLACEMENT_SCREEN)

# Kick off the event loop
application.run
