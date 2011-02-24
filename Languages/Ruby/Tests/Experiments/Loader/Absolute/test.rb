$:.clear

$".clear
puts x = File.join(ENV["DLR_ROOT"], "Languages/Ruby/Experimental/Loader/Absolute/a")
require x

$".clear
puts x = File.join(ENV["DLR_ROOT"], "Languages/Ruby/Experimental/Loader/Absolute/a.rb")
require x