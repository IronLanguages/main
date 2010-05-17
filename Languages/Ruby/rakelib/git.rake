namespace :git do
  desc "commit git changes" 
  task :commit => "test:all" do
    message = ENV['MESSAGE'] || "syncing to head of tfs"
    Dir.chdir(File.join(ENV['DLR_ROOT'], "..", "..")) do
      puts "git add ."
      system "git add ."
      puts "git commit -a -m \"#{message}\""
      system "git commit -a -m \"#{message}\""
    end
  end
end
