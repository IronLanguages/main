desc "Project network tools - sub-commands : web [user], list, fetch, commits"
flags :after => "Only show commits after a certain date"
flags :before => "Only show commits before a certain date"
flags :shas => "Only show shas"
flags :project => "Filter commits on a certain project"
flags :author => "Filter commits on a email address of author"
flags :applies => "Filter commits to patches that apply cleanly"
flags :noapply => "Filter commits to patches that do not apply cleanly"
flags :nocache => "Do not use the cached network data"
flags :cache => "Use the network data even if it's expired"
flags :sort => "How to sort : date(*), branch, author"
flags :common => "Show common branch point"
flags :thisbranch => "Look at branches that match the current one"
flags :limit => "Only look through the first X heads - useful for really large projects"
command :network do |command, user|
  return if !helper.project
  user ||= helper.owner

  case command
  when 'web'
    helper.open helper.network_page_for(user)
  when 'list'
    members = helper.get_network_members(user, options)
    members.each do |hsh|
      puts hsh["owner"]["login"]
    end
  when 'fetch'
    # fetch each remote we don't have
    data = helper.get_network_data(user, options)
    data['users'].each do |hsh|
      u = hsh['name']
      GitHub.invoke(:track, u) unless helper.tracking?(u)
      puts "fetching #{u}"
      GitHub.invoke(:fetch_all, u)
    end
  when 'commits'
    # show commits we don't have yet

    $stderr.puts 'gathering heads'
    cherry = []

    if helper.cache_commits_data(options)
      ids = []
      data = helper.get_network_data(user, options)
      data['users'].each do |hsh|
        u = hsh['name']
        if options[:thisbranch]
          user_ids = hsh['heads'].map { |a| a['id'] if a['name'] == helper.current_branch }.compact
        else
          user_ids = hsh['heads'].map { |a| a['id'] }
        end
        user_ids.each do |id|
          if !helper.has_commit?(id) && helper.cache_expired?
            GitHub.invoke(:track, u) unless helper.tracking?(u)
            puts "fetching #{u}"
            GitHub.invoke(:fetch_all, u)
          end
        end
        ids += user_ids
      end
      ids.uniq!

      $stderr.puts 'has heads'

      # check that we have all these shas locally
      local_heads = helper.local_heads
      local_heads_not = local_heads.map { |a| "^#{a}"}
      looking_for = (ids - local_heads) + local_heads_not
      commits = helper.get_commits(looking_for)

      $stderr.puts 'ID SIZE:' + ids.size.to_s

      ignores = helper.ignore_sha_array

      ids.each do |id|
        next if ignores[id] || !commits.assoc(id)
        cherries = helper.get_cherry(id)
        cherries = helper.remove_ignored(cherries, ignores)
        cherry += cherries
        helper.ignore_shas([id]) if cherries.size == 0
        $stderr.puts "checking head #{id} : #{cherry.size.to_s}"
        break if options[:limit] && cherry.size > options[:limit].to_i
      end
    end

    if cherry.size > 0 || !helper.cache_commits_data(options)
      helper.print_network_cherry_help if !options[:shas]

      if helper.cache_commits_data(options)
        $stderr.puts "caching..."
        $stderr.puts "commits: " + cherry.size.to_s
        our_commits = cherry.map { |item| c = commits.assoc(item[1]); [item, c] if c }
        our_commits.delete_if { |item| item == nil }
        helper.cache_commits(our_commits)
      else
        $stderr.puts "using cached..."
        our_commits = helper.commits_cache
      end

      helper.print_commits(our_commits, options)
    else
      puts "no unapplied commits"
    end
  else
    helper.print_network_help
  end
end

desc "Ignore a SHA (from 'github network commits')"
command :ignore do |sha|
  commits = helper.resolve_commits(sha)
  helper.ignore_shas(commits)             # add to .git/ignore-shas file
end
