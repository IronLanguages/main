require 'codeplex'

def setup
  ait = CodePlex::AdvancedIssueTracker.new
  ait.reset
  ait.type = 'issue'
  ait.sort_by_update_date_dsc
  ait.show_fifty
  ait
end

def run
  puts "> Fetching fixed and closed work items from codeplex ..."
  ait = setup
  ait.fetch_workitems :type => 'fixed'
  ait.fetch_workitems :type => 'closed'
  puts '> [DONE]'

  puts '=' * 80
  ait.report(Date.parse('2009-07-29'))
  puts '=' * 80
  
  ait.done
end

if __FILE__ == $0
  run
end

