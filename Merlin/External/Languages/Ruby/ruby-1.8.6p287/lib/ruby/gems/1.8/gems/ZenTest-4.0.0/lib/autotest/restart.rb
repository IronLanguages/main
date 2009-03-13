module Autotest::Restart
  Autotest.add_hook :updated do |at, *args|
    if args.flatten.include? ".autotest" then
      warn "Detected change to .autotest, restarting"
      cmd = "autotest"
      cmd << " -v" if $v
      
      exec cmd
    end
  end
end
