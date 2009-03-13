namespace :haml do
  desc "Compiles all sass files into CSS"
  task :compile_sass do
    gem 'haml'
    require 'sass'
    puts "*** Updating stylesheets"
    Sass::Plugin.options = Merb::Config[:sass] if Merb::Config[:sass]
    Sass::Plugin.update_stylesheets
    puts "*** Done"      
  end
end