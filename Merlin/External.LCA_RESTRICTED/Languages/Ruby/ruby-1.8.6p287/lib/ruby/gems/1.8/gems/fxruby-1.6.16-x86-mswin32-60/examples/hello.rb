#!/usr/bin/env ruby

require 'fox16'

include Fox

application = FXApp.new("Hello", "FoxTest")
main = FXMainWindow.new(application, "Hello", nil, nil, DECOR_ALL)
FXButton.new(main, "&Hello, World!", nil, application, FXApp::ID_QUIT)
application.create()
main.show(PLACEMENT_SCREEN)
application.run()
