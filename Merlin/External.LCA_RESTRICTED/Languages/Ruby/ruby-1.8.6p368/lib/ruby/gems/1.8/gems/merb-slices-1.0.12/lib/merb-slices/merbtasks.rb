require "merb-core/tasks/merb_rake_helper"

desc "Show information on application slices"
task :slices => [ "slices:list" ]

namespace :slices do

  desc "Get a suitable slices environment up"
  task :env do
    Merb::Slices.register_slices_from_search_path!
  end

  desc "List known slices for the current application"
  task :list => [:env] do
    puts "Application slices:\n"
    Merb::Slices.each_slice do |slice|
      puts "#{slice.name} (v. #{slice.version}) - #{slice.description}\n"
    end    
  end
  
  desc "Install a slice into the local gems dir (GEM=your-slice)"
  task :install_as_gem do
    if ENV['GEM']
      ENV['GEM_DIR'] ||= File.join(Merb.root, 'gems')
      options = { :install_dir => ENV['GEM_DIR'], 
                  :cache => true, :ignore_dependencies => true }
      options[:version] = ENV['VERSION'] if ENV['VERSION']
      Merb::RakeHelper.install_package(ENV['GEM'], options)
    else
      puts "No slice GEM specified"
    end
  end
  
end