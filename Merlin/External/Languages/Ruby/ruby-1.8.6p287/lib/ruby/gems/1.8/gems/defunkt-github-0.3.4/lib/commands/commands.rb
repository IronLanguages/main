desc "Open this repo's master branch in a web browser."
command :home do |user|
  if helper.project
    helper.open helper.homepage_for(user || helper.owner, 'master')
  end
end

desc "Automatically set configuration info, or pass args to specify."
usage "github config [my_username] [my_repo_name]"
command :config do |user, repo|
  user ||= ENV['USER']
  repo ||= File.basename(FileUtils.pwd)
  git "config --global github.user #{user}"
  git "config github.repo #{repo}"
  puts "Configured with github.user #{user}, github.repo #{repo}"
end

desc "Open this repo in a web browser."
usage "github browse [user] [branch]"
command :browse do |user, branch|
  if helper.project
    # if one arg given, treat it as a branch name
    # unless it maches user/branch, then split it
    # if two args given, treat as user branch
    # if no args given, use defaults
    user, branch = user.split("/", 2) if branch.nil? unless user.nil?
    branch = user and user = nil if branch.nil?
    user ||= helper.branch_user
    branch ||= helper.branch_name
    helper.open helper.homepage_for(user, branch)
  end
end


desc "Info about this project."
command :info do
  puts "== Info for #{helper.project}"
  puts "You are #{helper.owner}"
  puts "Currently tracking:"
  helper.tracking.sort { |(a,),(b,)| a == helper.origin ? -1 : b == helper.origin ? 1 : a.to_s <=> b.to_s }.each do |(name,user_or_url)|
    puts " - #{user_or_url} (as #{name})"
  end
end

desc "Track another user's repository."
usage "github track remote [user]"
usage "github track remote [user/repo]"
usage "github track [user]"
usage "github track [user/repo]"
flags :private => "Use git@github.com: instead of git://github.com/."
flags :ssh => 'Equivalent to --private'
command :track do |remote, user|
  # track remote user
  # track remote user/repo
  # track user
  # track user/repo
  user, remote = remote, nil if user.nil?
  die "Specify a user to track" if user.nil?
  user, repo = user.split("/", 2)
  die "Already tracking #{user}" if helper.tracking?(user)
  repo = @helper.project if repo.nil?
  repo.chomp!(".git")
  remote ||= user

  if options[:private] || options[:ssh]
    git "remote add #{remote} #{helper.private_url_for_user_and_repo(user, repo)}"
  else
    git "remote add #{remote} #{helper.public_url_for_user_and_repo(user, repo)}"
  end
end

desc "Fetch all refs from a user"
command :fetch_all do |user|
  GitHub.invoke(:track, user) unless helper.tracking?(user)
  git "fetch #{user}"
end

desc "Fetch from a remote to a local branch."
command :fetch do |user, branch|
  die "Specify a user to pull from" if user.nil?
  user, branch = user.split("/", 2) if branch.nil?
  branch ||= 'master'
  GitHub.invoke(:track, user) unless helper.tracking?(user)

  die "Unknown branch (#{branch}) specified" unless helper.remote_branch?(user, branch)
  die "Unable to switch branches, your current branch has uncommitted changes" if helper.branch_dirty?

  puts "Fetching #{user}/#{branch}"
  git "fetch #{user} #{branch}:refs/remotes/#{user}/#{branch}"
  git "update-ref refs/heads/#{user}/#{branch} refs/remotes/#{user}/#{branch}"
  git_exec "checkout #{user}/#{branch}"
end

desc "Pull from a remote."
usage "github pull [user] [branch]"
flags :merge => "Automatically merge remote's changes into your master."
command :pull do |user, branch|
  die "Specify a user to pull from" if user.nil?
  user, branch = user.split("/", 2) if branch.nil?

  if !helper.network_members.include?(user)
    git_exec "#{helper.argv.join(' ')}".strip
  end

  branch ||= 'master'
  GitHub.invoke(:track, user) unless helper.tracking?(user)

  if options[:merge]
    git_exec "pull #{user} #{branch}"
  else
    puts "Switching to #{user}-#{branch}"
    git "fetch #{user}"
    git_exec "checkout -b #{user}/#{branch} #{user}/#{branch}"
  end
end

desc "Clone a repo. Uses ssh if current user is "
usage "github clone [user] [repo] [dir]"
flags :ssh => "Clone using the git@github.com style url."
command :clone do |user, repo, dir|
  die "Specify a user to pull from" if user.nil?
  if user.include?('/') && !user.include?('@') && !user.include?(':')
    die "Expected user/repo dir, given extra argument" if dir
    (user, repo), dir = [user.split('/', 2), repo]
  end

  if options[:ssh] || current_user?(user)
    git_exec "clone git@github.com:#{user}/#{repo}.git" + (dir ? " #{dir}" : "")
  elsif repo
    git_exec "clone git://github.com/#{user}/#{repo}.git" + (dir ? " #{dir}" : "")
  else
    git_exec "#{helper.argv.join(' ')}".strip
  end
end

desc "Generate the text for a pull request."
usage "github pull-request [user] [branch]"
command 'pull-request' do |user, branch|
  if helper.project
    die "Specify a user for the pull request" if user.nil?
    user, branch = user.split('/', 2) if branch.nil?
    branch ||= 'master'
    GitHub.invoke(:track, user) unless helper.tracking?(user)

    git_exec "request-pull #{user}/#{branch} #{helper.origin}"
  end
end

desc "Create a new, empty GitHub repository"
usage "github create [repo]"
flags :markdown => 'Create README.markdown'
flags :mdown => 'Create README.mdown'
flags :textile => 'Create README.textile'
flags :rdoc => 'Create README.rdoc'
flags :rst => 'Create README.rst'
command :create do |repo|
  sh "curl -F 'repository[name]=#{repo}' -F 'login=#{github_user}' -F 'token=#{github_token}' http://github.com/repositories"
  mkdir repo
  cd repo
  git "init"
  extension = options.keys.first
  touch extension ? "README.#{extension}" : "README"
  git "add *"
  git "commit -m 'First commit!'"
  git "remote add origin git@github.com:#{github_user}/#{repo}.git"
  git_exec "push origin master"
end

desc "Forks a GitHub repository"
usage "github fork [user]/[repo]"
command :fork do |user, repo|
  if repo.nil?
    user, repo = user.split('/')
  end

  sh "curl -F 'login=#{github_user}' -F 'token=#{github_token}' http://github.com/#{user}/#{repo}/fork"
  puts "Giving GitHub a moment to create the fork..."
  sleep 3
  git_exec "clone git@github.com:#{github_user}/#{repo}.git"
end

desc "Create a new GitHub repository from the current local repository"
command 'create-from-local' do
  cwd = sh "pwd"
  repo = File.basename(cwd)
  is_repo = !git("status").match(/fatal/)
  raise "Not a git repository. Use gh create instead" unless is_repo
  sh "curl -F 'repository[name]=#{repo}' -F 'login=#{github_user}' -F 'token=#{github_token}' http://github.com/repositories"
  git "remote add origin git@github.com:#{github_user}/#{repo}.git"
  git_exec "push origin master"
end
