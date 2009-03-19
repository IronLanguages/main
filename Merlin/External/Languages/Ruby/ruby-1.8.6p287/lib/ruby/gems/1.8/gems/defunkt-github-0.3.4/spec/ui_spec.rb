require File.dirname(__FILE__) + '/spec_helper'

describe "github" do
  # -- home --
  specify "home should open the project home page" do
    running :home do
      setup_url_for
      @helper.should_receive(:open).once.with("https://github.com/user/project/tree/master")
    end
  end

  specify "home defunkt should open the home page of defunkt's fork" do
    running :home, "defunkt" do
      setup_url_for
      @helper.should_receive(:open).once.with("https://github.com/defunkt/project/tree/master")
    end
  end

  # -- browse --
  specify "browse should open the project home page with the current branch" do
    running :browse do
      setup_url_for
      setup_user_and_branch("user", "test-branch")
      @helper.should_receive(:open).once.with("https://github.com/user/project/tree/test-branch")
    end
  end

  specify "browse pending should open the project home page with the 'pending' branch" do
    running :browse, "pending" do
      setup_url_for
      setup_user_and_branch("user", "test-branch")
      @helper.should_receive(:open).once.with("https://github.com/user/project/tree/pending")
    end
  end

  specify "browse defunkt pending should open the home page of defunkt's fork with the 'pending' branch" do
    running :browse, "defunkt", "pending" do
      setup_url_for
      @helper.should_receive(:open).once.with("https://github.com/defunkt/project/tree/pending")
    end
  end

  specify "browse defunkt/pending should open the home page of defunkt's fork with the 'pending' branch" do
    running :browse, "defunkt/pending" do
      setup_url_for
      @helper.should_receive(:open).once.with("https://github.com/defunkt/project/tree/pending")
    end
  end

  # -- network --
  specify "network should open the network page for this repo" do
    running :network, 'web' do
      setup_url_for
      @helper.should_receive(:open).once.with("https://github.com/user/project/network")
    end
  end

  specify "network defunkt should open the network page for defunkt's fork" do
    running :network, 'web', "defunkt" do
      setup_url_for
      @helper.should_receive(:open).once.with("https://github.com/defunkt/project/network")
    end
  end

  # -- info --
  specify "info should show info for this project" do
    running :info do
      setup_url_for
      setup_remote(:origin, :user => "user", :ssh => true)
      setup_remote(:defunkt)
      setup_remote(:external, :url => "home:/path/to/project.git")
      stdout.should == <<-EOF
== Info for project
You are user
Currently tracking:
 - user (as origin)
 - defunkt (as defunkt)
 - home:/path/to/project.git (as external)
EOF
    end
  end

  # -- track --
  specify "track defunkt should track a new remote for defunkt" do
    running :track, "defunkt" do
      setup_url_for
      @helper.should_receive(:tracking?).with("defunkt").once.and_return(false)
      @command.should_receive(:git).with("remote add defunkt git://github.com/defunkt/project.git").once
    end
  end

  specify "track --private defunkt should track a new remote for defunkt using ssh" do
    running :track, "--private", "defunkt" do
      setup_url_for
      @helper.should_receive(:tracking?).with("defunkt").and_return(false)
      @command.should_receive(:git).with("remote add defunkt git@github.com:defunkt/project.git")
    end
  end

  specify "track --ssh defunkt should be equivalent to track --private defunkt" do
    running :track, "--ssh", "defunkt" do
      setup_url_for
      @helper.should_receive(:tracking?).with("defunkt").and_return(false)
      @command.should_receive(:git).with("remote add defunkt git@github.com:defunkt/project.git")
    end
  end

  specify "track defunkt should die if the defunkt remote exists" do
    running :track, "defunkt" do
      setup_url_for
      @helper.should_receive(:tracking?).with("defunkt").once.and_return(true)
      @command.should_receive(:die).with("Already tracking defunkt").and_return { raise "Died" }
      self.should raise_error("Died")
    end
  end

  specify "track should die with no args" do
    running :track do
      @command.should_receive(:die).with("Specify a user to track").and_return { raise "Died" }
      self.should raise_error("Died")
    end
  end

  specify "track should accept user/project syntax" do
    running :track, "defunkt/github-gem.git" do
      setup_url_for
      @helper.should_receive(:tracking?).with("defunkt").and_return false
      @command.should_receive(:git).with("remote add defunkt git://github.com/defunkt/github-gem.git")
    end
  end

  specify "track defunkt/github-gem.git should function with no origin remote" do
    running :track, "defunkt/github-gem.git" do
      @helper.stub!(:url_for).with(:origin).and_return ""
      @helper.stub!(:tracking?).and_return false
      @command.should_receive(:git).with("remote add defunkt git://github.com/defunkt/github-gem.git")
      self.should_not raise_error(SystemExit)
      stderr.should_not =~ /^Error/
    end
  end

  specify "track origin defunkt/github-gem should track defunkt/github-gem as the origin remote" do
    running :track, "origin", "defunkt/github-gem" do
      @helper.stub!(:url_for).with(:origin).and_return ""
      @helper.stub!(:tracking?).and_return false
      @command.should_receive(:git).with("remote add origin git://github.com/defunkt/github-gem.git")
      stderr.should_not =~ /^Error/
    end
  end

  specify "track --private origin defunkt/github-gem should track defunkt/github-gem as the origin remote using ssh" do
    running :track, "--private", "origin", "defunkt/github-gem" do
      @helper.stub!(:url_for).with(:origin).and_return ""
      @helper.stub!(:tracking?).and_return false
      @command.should_receive(:git).with("remote add origin git@github.com:defunkt/github-gem.git")
      stderr.should_not =~ /^Error/
    end
  end

  # -- fetch --
  specify "fetch should die with no args" do
    running :fetch do
      @command.should_receive(:die).with("Specify a user to pull from").and_return { raise "Died "}
      self.should raise_error("Died")
    end
  end

  specify "pull defunkt should start tracking defunkt if they're not already tracked" do
    running :pull, "defunkt" do
      mock_members 'defunkt'
      setup_remote(:origin, :user => "user", :ssh => true)
      setup_remote(:external, :url => "home:/path/to/project.git")
      GitHub.should_receive(:invoke).with(:track, "defunkt").and_return { raise "Tracked" }
      self.should raise_error("Tracked")
    end
  end

  specify "pull defunkt should create defunkt/master and pull from the defunkt remote" do
    running :pull, "defunkt" do
      mock_members 'defunkt'
      setup_remote(:defunkt)
      @helper.should_receive(:branch_dirty?).and_return false
      @command.should_receive(:git).with("update-ref refs/heads/defunkt/master HEAD").ordered
      @command.should_receive(:git).with("checkout defunkt/master").ordered
      @command.should_receive(:git_exec).with("fetch defunkt master").ordered
      stdout.should == "Switching to defunkt/master\n"
    end
  end

  specify "pull defunkt should switch to pre-existing defunkt/master and pull from the defunkt remote" do
    running :pull, "defunkt" do
      mock_members 'defunkt'
      setup_remote(:defunkt)
      @helper.should_receive(:branch_dirty?).and_return true
      @command.should_receive(:die).with("Unable to switch branches, your current branch has uncommitted changes").and_return { raise "Died" }
      self.should raise_error("Died")
    end
  end

  specify "fetch defunkt/wip should create defunkt/wip and fetch from wip branch on defunkt remote" do
    running :fetch, "defunkt/wip" do
      setup_remote(:defunkt, :remote_branches => ["master", "wip"])
      @helper.should_receive(:branch_dirty?).and_return false
      @command.should_receive(:git).with("update-ref refs/heads/defunkt/wip HEAD").ordered
      @command.should_receive(:git).with("checkout defunkt/wip").ordered
      @command.should_receive(:git_exec).with("fetch defunkt wip").ordered
      stdout.should == "Switching to defunkt/wip\n"
    end
  end

  specify "fetch --merge defunkt should fetch from defunkt remote into current branch" do
    running :fetch, "--merge", "defunkt" do
      setup_remote(:defunkt)
      @helper.should_receive(:branch_dirty?).and_return false
      @command.should_receive(:git_exec).with("fetch defunkt master")
    end
  end

  # -- fetch --
  specify "fetch should die with no args" do
    running :fetch do
      @command.should_receive(:die).with("Specify a user to fetch from").and_return { raise "Died" }
      self.should raise_error("Died")
    end
  end

  specify "fetch defunkt should start tracking defunkt if they're not already tracked" do
    running :fetch, "defunkt" do
      setup_remote(:origin, :user => "user", :ssh => true)
      setup_remote(:external, :url => "home:/path/to/project.git")
      GitHub.should_receive(:invoke).with(:track, "defunkt").and_return { raise "Tracked" }
      self.should raise_error("Tracked")
    end
  end

  specify "fetch defunkt should create defunkt/master and fetch from the defunkt remote" do
    running :fetch, "defunkt" do
      setup_remote(:defunkt)
      @helper.should_receive(:branch_dirty?).and_return false
      @command.should_receive(:git).with("update-ref refs/heads/defunkt/master HEAD").ordered
      @command.should_receive(:git).with("checkout defunkt/master").ordered
      @command.should_receive(:git_exec).with("fetch defunkt master").ordered
      stdout.should == "Switching to defunkt/master\n"
    end
  end

  specify "pull defunkt wip should create defunkt/wip and pull from wip branch on defunkt remote" do
    running :pull, "defunkt", "wip" do
      mock_members 'defunkt'
      setup_remote(:defunkt)
      @helper.should_receive(:branch_dirty?).and_return true
      @command.should_receive(:die).with("Unable to switch branches, your current branch has uncommitted changes").and_return { raise "Died" }
      self.should raise_error("Died")
    end
  end

  specify "pull defunkt/wip should switch to pre-existing defunkt/wip and pull from wip branch on defunkt remote" do
    running :pull, "defunkt/wip" do
      mock_members 'defunkt'
      setup_remote(:defunkt)
      @command.should_receive(:git).with("checkout -b defunkt/wip").ordered.and_return do
        mock("checkout -b defunkt/wip").tap { |m| m.should_receive(:error?) { true } }
      end
      @command.should_receive(:git).with("checkout defunkt/wip").ordered
      @command.should_receive(:git_exec).with("fetch defunkt wip").ordered
      stdout.should == "Switching to defunkt/wip\n"
    end
  end

  specify "pull --merge defunkt should pull from defunkt remote into current branch" do
    running :pull, "--merge", "defunkt" do
      mock_members 'defunkt'
      setup_remote(:defunkt)
      @helper.should_receive(:branch_dirty?).and_return false
      @command.should_receive(:git_exec).with("fetch defunkt master")
    end
  end

  specify "pull falls through for non-recognized commands" do
    running :pull, 'remote' do
      mock_members 'defunkt'
      @command.should_receive(:git_exec).with("pull remote")
    end
  end

  specify "pull passes along args when falling through" do
    running :pull, 'remote', '--stat' do
      mock_members 'defunkt'
      @command.should_receive(:git_exec).with("pull remote --stat")
    end
  end

  # -- clone --
  specify "clone should die with no args" do
    running :clone do
      @command.should_receive(:die).with("Specify a user to pull from").and_return { raise "Died" }
      self.should raise_error("Died")
    end
  end

  specify "clone should fall through with just one arg" do
    running :clone, "git://git.kernel.org/linux.git" do
      @command.should_receive(:git_exec).with("clone git://git.kernel.org/linux.git")
    end
  end

  specify "clone defunkt github-gem should clone the repo" do
    running :clone, "defunkt", "github-gem" do
      @command.should_receive(:git_exec).with("clone git://github.com/defunkt/github-gem.git")
    end
  end

  specify "clone defunkt/github-gem should clone the repo" do
    running :clone, "defunkt/github-gem" do
      @command.should_receive(:git_exec).with("clone git://github.com/defunkt/github-gem.git")
    end
  end

  specify "clone --ssh defunkt github-gem should clone the repo using the private URL" do
    running :clone, "--ssh", "defunkt", "github-gem" do
      @command.should_receive(:git_exec).with("clone git@github.com:defunkt/github-gem.git")
    end
  end

  specify "clone defunkt github-gem repo should clone the repo into the dir 'repo'" do
    running :clone, "defunkt", "github-gem", "repo" do
      @command.should_receive(:git_exec).with("clone git://github.com/defunkt/github-gem.git repo")
    end
  end

  specify "clone defunkt/github-gem repo should clone the repo into the dir 'repo'" do
    running :clone, "defunkt/github-gem", "repo" do
      @command.should_receive(:git_exec).with("clone git://github.com/defunkt/github-gem.git repo")
    end
  end

  specify "clone --ssh defunkt github-gem repo should clone the repo using the private URL into the dir 'repo'" do
    running :clone, "--ssh", "defunkt", "github-gem", "repo" do
      @command.should_receive(:git_exec).with("clone git@github.com:defunkt/github-gem.git repo")
    end
  end

  specify "clone defunkt/github-gem repo should clone the repo into the dir 'repo'" do
    running :clone, "defunkt/github-gem", "repo" do
      @command.should_receive(:git_exec).with("clone git://github.com/defunkt/github-gem.git repo")
    end
  end

  # -- pull-request --
  specify "pull-request should die with no args" do
    running :'pull-request' do
      setup_url_for
      @command.should_receive(:die).with("Specify a user for the pull request").and_return { raise "Died" }
      self.should raise_error("Died")
    end
  end

  specify "pull-request user should track user if untracked" do
    running :'pull-request', "user" do
      setup_url_for
      setup_remote :origin, :user => "kballard"
      setup_remote :defunkt
      GitHub.should_receive(:invoke).with(:track, "user").and_return { raise "Tracked" }
      self.should raise_error("Tracked")
    end
  end

  specify "pull-request user/branch should generate a pull request" do
    running :'pull-request', "user/branch" do
      setup_url_for
      setup_remote :origin, :user => "kballard"
      setup_remote :user
      @command.should_receive(:git_exec).with("request-pull user/branch origin")
    end
  end

  specify "pull-request user should generate a pull request with branch master" do
    running :'pull-request', "user" do
      setup_url_for
      setup_remote :origin, :user => "kballard"
      setup_remote :user
      @command.should_receive(:git_exec).with("request-pull user/master origin")
    end
  end

  specify "pull-request user branch should generate a pull request" do
    running:'pull-request', "user", "branch" do
      setup_url_for
      setup_remote :origin, :user => "kballard"
      setup_remote :user
      @command.should_receive(:git_exec).with("request-pull user/branch origin")
    end
  end

  # -- fallthrough --
  specify "should fall through to actual git commands" do
    running :commit do
      @command.should_receive(:git_exec).with("commit")
    end
  end

  specify "should pass along arguments when falling through" do
    running :commit, '-a', '-m', 'yo mama' do
      @command.should_receive(:git_exec).with("commit -a -m 'yo mama'")
    end
  end

  # -- default --
  specify "should print the default message" do
    running :default do
      GitHub.should_receive(:descriptions).any_number_of_times.and_return({
        "home" => "Open the home page",
        "track" => "Track a new repo",
        "browse" => "Browse the github page for this branch",
        "command" => "description"
      })
      GitHub.should_receive(:flag_descriptions).any_number_of_times.and_return({
        "home" => {:flag => "Flag description"},
        "track" => {:flag1 => "Flag one", :flag2 => "Flag two"},
        "browse" => {},
        "command" => {}
      })
      @command.should_receive(:puts).with("Usage: github command <space separated arguments>", '').ordered
      @command.should_receive(:puts).with("Available commands:", '').ordered
      @command.should_receive(:puts).with("  home    => Open the home page")
      @command.should_receive(:puts).with("           --flag: Flag description")
      @command.should_receive(:puts).with("  track   => Track a new repo")
      @command.should_receive(:puts).with("           --flag1: Flag one")
      @command.should_receive(:puts).with("           --flag2: Flag two")
      @command.should_receive(:puts).with("  browse  => Browse the github page for this branch")
      @command.should_receive(:puts).with("  command => description")
      @command.should_receive(:puts).with()
    end
  end

  # -----------------

  def running(cmd, *args, &block)
    Runner.new(self, cmd, *args, &block).run
  end

  class Runner
    include SetupMethods

    def initialize(parent, cmd, *args, &block)
      @cmd_name = cmd.to_s
      @command = GitHub.find_command(cmd)
      @helper = @command.helper
      @args = args
      @block = block
      @parent = parent
    end

    def run
      self.instance_eval &@block
      mock_remotes unless @remotes.nil?
      GitHub.should_receive(:load).with("commands.rb")
      GitHub.should_receive(:load).with("helpers.rb")
      args = @args.clone
      GitHub.parse_options(args) # strip out the flags
      GitHub.should_receive(:invoke).with(@cmd_name, *args).and_return do
        GitHub.send(GitHub.send(:__mock_proxy).send(:munge, :invoke), @cmd_name, *args)
      end
      invoke = lambda { GitHub.activate([@cmd_name, *@args]) }
      if @expected_result
        expectation, result = @expected_result
        case result
        when Spec::Matchers::RaiseError, Spec::Matchers::Change, Spec::Matchers::ThrowSymbol
          invoke.send expectation, result
        else
          invoke.call.send expectation, result
        end
      else
        invoke.call
      end
      @stdout_mock.invoke unless @stdout_mock.nil?
      @stderr_mock.invoke unless @stderr_mock.nil?
    end

    def setup_remote(remote, options = {:user => nil, :project => "project", :remote_branches => nil})
      @remotes ||= {}
      @remote_branches ||= {}
      user = options[:user] || remote
      project = options[:project]
      ssh = options[:ssh]
      url = options[:url]
      remote_branches = options[:remote_branches] || ["master"]
      if url
        @remotes[remote.to_sym] = url
      elsif ssh
        @remotes[remote.to_sym] = "git@github.com:#{user}/#{project}.git"
      else
        @remotes[remote.to_sym] = "git://github.com/#{user}/#{project}.git"
      end

      @remote_branches[remote.to_sym] = (@remote_branches[remote.to_sym] || Array.new) | remote_branches
      @helper.should_receive(:remote_branch?).any_number_of_times.and_return do |remote, branch|
        @remote_branches.fetch(remote.to_sym,[]).include?(branch)
      end
    end

    def mock_remotes()
      @helper.should_receive(:remotes).any_number_of_times.and_return(@remotes)
    end

    def mock_members(members)
      @helper.should_receive(:network_members).any_number_of_times.and_return(members)
    end

    def should(result)
      @expected_result = [:should, result]
    end

    def should_not(result)
      @expected_result = [:should_not, result]
    end

    def stdout
      if @stdout_mock.nil?
        output = ""
        @stdout_mock = DeferredMock.new(output)
        $stdout.should_receive(:write).any_number_of_times do |str|
          output << str
        end
      end
      @stdout_mock
    end

    def stderr
      if @stderr_mock.nil?
        output = ""
        @stderr_mock = DeferredMock.new(output)
        $stderr.should_receive(:write).any_number_of_times do |str|
          output << str
        end
      end
      @stderr_mock
    end

    class DeferredMock
      def initialize(obj = nil)
        @obj = obj
        @calls = []
        @expectations = []
      end

      attr_reader :obj

      def invoke(obj = nil)
        obj ||= @obj
        @calls.each do |sym, args|
          obj.send sym, *args
        end
        @expectations.each do |exp|
          exp.invoke
        end
      end

      def should(*args)
        if args.empty?
          exp = Expectation.new(self, :should)
          @expectations << exp
          exp
        else
          @calls << [:should, args]
        end
      end

      def should_not(*args)
        if args.empty?
          exp = Expectation.new(self, :should_not)
          @expectations << exp
          exp
        else
          @calls << [:should_not, args]
        end
      end

      class Expectation
        def initialize(mock, call)
          @mock = mock
          @call = call
          @calls = []
        end

        undef_method *(instance_methods.map { |x| x.to_sym } - [:__id__, :__send__])

        def invoke
          @calls.each do |sym, args|
            (@mock.obj.send @call).send sym, *args
          end
        end

        def method_missing(sym, *args)
          @calls << [sym, args]
        end
      end
    end

    def method_missing(sym, *args)
      @parent.send sym, *args
    end
  end
end
