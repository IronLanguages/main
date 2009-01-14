include IronRubyUtils

desc "Merge TFS into Git"
task :to_git, [:repo] => [:happy] do |t, args|
  Rake::Task["git:ensure_repo"].invoke(args.repo)

  #TODO: expand this to check for more than just IPy
  if !is_test? and File.exist?("#{ENV["MERLIN_ROOT"]}\\Languages\\IronPython")
    banner("Error")
    rake_output_message <<-EOL
    This command must be run from an enlistment that only has the required ruby sources. To create one you can run:
    
    tf workspace /new /s:http://vstfdevdiv:8080 /template:rubysync;REDMOND\\jdeville %COMPUTERNAME% /noprompt

    where %COMPUTERNAME% is the name of your enlisted computer
    EOL

    abort
  end

  update_tfs

  banner "Copying sources"
  copy_dir(File.expand_path("#{ENV["MERLIN_ROOT"]}\\..\\.."), @target,"/MIR")

  banner "Git Status:"
  logged_chdir(@target) do
    checked_system "git status"
  end
end

desc "Merge Git into TFS"
task :from_git, [:repo, :remotes] => [:happy] do |t,args|
  Rake::Task["git:ensure_repo"].invoke(args.repo, args.remotes)  
  Rake::Task["git:import"].invoke(args.repo)
  last_git_shelvesets = checked_backtick("tf shelvesets").grep(/^gitimport/).last
  number = last_git_shelvesets && last_git_shelvesets.match(/^gitimport.*?(\d\d?\d?)/)[1]
  shelveset = "gitimport#{number.to_i + 1}"
  checked_system("tf shelve #{shelveset} /comment:\"Import Git into TFS, remotes from: #{args.remotes}\"")
  checked_system("snap submit -c:#{shelveset}")
end

namespace :git do
  desc "create a git repository"
  task :ensure_repo, [:repo, :remotes] do |t, args|
    @ran ||= false
    unless @ran
      args.with_defaults(:repo => "~\\projects\\ironruby", :remotes => "")

      @target = File.expand_path(args.repo)
      @url = "git@github.com:ironruby/ironruby.git"
      
      if File.directory?(File.join(@target, ".git")) 
        logged_chdir(@target) do
          banner "Updating target repository at #{@target}"
          checked_system "git pull origin master"
        end
      else
        banner "Creating target repository at #{@target}"
        checked_system "git clone #{@url} #{@target}"
      end
      
      logged_chdir(@target) do
        args.remotes.split(";").each do |remote|
          checked_system "git remote add #{remote} git://github.com/#{remote}/ironruby.git"
          checked_system "git pull #{remote} master"
        end
      end unless args.remotes.empty?
      @ran = true
    end
  end

  desc "import git repository into TFS repository"
  task :import, [:repo] do |t, args|
    Rake::Task["git:ensure_repo"].invoke(args.repo)  
    merlin = File.expand_path(ENV["MERLIN_ROOT"])
    update_tfs

    status = ''
    logged_chdir(merlin) do
      banner "Ensuring TFS doesn't have pending changes"
      status = checked_backtick("tf status")
      unless is_test?
        status.chomp!
        abort "TFS has active changes" unless status =~ /There are no pending changes\./
      end
      
      banner "Copying from #{@target} to #{merlin}\\..\\.."
      copy_dir(@target, "#{merlin}\\..\\..", "/MIR")
      checked_system("tfpt online /adds /deletes /diff /noprompt /recursive")
      checked_system("tfpt uu /noget")
    end
  end
end

def update_tfs
  logged_chdir(ENV["MERLIN_ROOT"]) do
    banner "Removing extra files from the enlistment"
    checked_system "#{ENV["MERLIN_ROOT"]}\\External\\Tools\\tfpt.exe treeclean -delete" 

    banner "Getting the latest TFS sources"
    checked_system "tf get /overwrite"
  end
end

def checked_system(cmd)
  result = checked_exec("system", cmd)
  result.nil? ? false : result
end

def checked_backtick(cmd)
  result = checked_exec("`", cmd)
  result.nil? ? "false" : result
end

def checked_exec(type, cmd)
  res = nil
  if is_test?
    rake_output_message "#{type} call to #{cmd}"
  else
    res = send type, cmd
    unless $?.exitstatus == 0
      abort cmd + " failed"
    end
  end
  return res
end

def logged_chdir(location, &block)
  puts "CHDIR: #{location}" if is_test?
  Dir.chdir(location, &block)
  puts "Back in #{Dir.pwd}" if is_test?
end
