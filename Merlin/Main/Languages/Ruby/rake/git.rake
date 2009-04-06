namespace :git do
  desc "smoke test to run before pushing"
  task :smoketest => ["compile"] do
    Dir.chdir(File.dirname(__FILE__) + '/../../../bin/debug') do
      unless `ir -e "require 'yaml';puts(YAML.dump(1+1))"`.chomp == "--- 2"
        raise "Smoke test failed"
      end
    end
  end

  desc "commit git changes" 
  task :commit => ["git:smoketest"] do
    message = ENV['MESSAGE'] || "syncing to head of tfs"
    Dir.chdir(File.join(ENV['MERLIN_ROOT'], "..", "..")) do
      puts "git add ."
      system "git add ."
      puts "git commit -a -m \"#{message}\""
      system "git commit -a -m \"#{message}\""
    end
  end
end
