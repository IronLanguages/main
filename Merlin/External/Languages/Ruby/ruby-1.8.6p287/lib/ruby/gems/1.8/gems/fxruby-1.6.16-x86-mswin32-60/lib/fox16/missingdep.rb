# This is a little helper function used by some of the examples to report
# missing dependencies information on startup. It's especially useful for the
# Windows distribution since people will often start the examples by double-
# clicking on an icon instead of running from the command line.

def missingDependency(msg)
  app = Fox::FXApp.new("Dummy", "FoxTest")
  app.init(ARGV)
  mainWindow = Fox::FXMainWindow.new(app, "")
  app.create
  Fox::FXMessageBox.error(mainWindow, Fox::MBOX_OK, "Dependencies Missing", msg)
  raise SystemExit
end

