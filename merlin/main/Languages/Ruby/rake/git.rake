desc "Merge TFS into Git"
task :to_git => [:happy] do
  include IronRubyUtils

  if File.exist?("#{ENV["MERLIN_ROOT"]}\\Languages\\IronPython")
    banner("Error")
    rake_output_message <<-EOL
    This command must be run from an enlistment that only has the required ruby sources. To create one you can run:
    
    tf workspace /new /s:http://vstfdevdiv:8080 /template:rubysync;REDMOND\\jdeville %COMPUTERNAME% /noprompt

    where %COMPUTERNAME% is the name of your enlisted computer
    EOL

    abort
  end
  # TODO: generalize target
  target = File.expand_path("~\\projects\\ironruby")
  banner "Removing extra files from the enlistment"
  system "#{ENV["MERLIN_ROOT"]}\\External\\Tools\\tfpt.exe treeclean -delete" 

  banner "Getting the latest TFS sources"
  system "tf get /overwrite"

  banner "Copying sources"
  copy_dir(File.expand_path("#{ENV["MERLIN_ROOT"]}\\..\\.."), target,"/E")

  banner "Git Status:"
  Dir.chdir(target) do
    system "git status"
  end
end
