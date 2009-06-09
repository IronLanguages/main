require File.dirname(__FILE__) + '/spec_helper'

class HelperRunner
  def initialize(parent, name)
    @parent = parent
    @name = name
  end

  def run(&block)
    self.instance_eval(&block)
  end

  def it(str, &block)
    @parent.send :it, "#{@name} #{str}", &block
  end
  alias specify it
end

describe GitHub::Helper do
  include SetupMethods

  def self.helper(name, &block)
    HelperRunner.new(self, name).run(&block)
  end

  before(:each) do
    @helper = GitHub::Helper.new
  end

  helper :owner do
    it "should return repo owner" do
      setup_url_for :origin, "hacker"
      @helper.owner.should == "hacker"
    end
  end

  helper :private_url_for do
    it "should return an ssh-style url" do
      setup_url_for :origin, "user", "merb-core"
      @helper.private_url_for("wycats").should == "git@github.com:wycats/merb-core.git"
    end
  end

  helper :private_url_for_user_and_repo do
    it "should return an ssh-style url" do
      @helper.should_not_receive(:project)
      @helper.private_url_for_user_and_repo("defunkt", "github-gem").should == "git@github.com:defunkt/github-gem.git"
    end
  end

  helper :public_url_for do
    it "should return a git:// URL" do
      setup_url_for :origin, "user", "merb-core"
      @helper.public_url_for("wycats").should == "git://github.com/wycats/merb-core.git"
    end
  end

  helper :public_url_for_user_and_repo do
    it "should return a git:// URL" do
      @helper.should_not_receive(:project)
      @helper.public_url_for_user_and_repo("defunkt", "github-gem").should == "git://github.com/defunkt/github-gem.git"
    end
  end

  helper :project do
    it "should return project-awesome" do
      setup_url_for :origin, "user", "project-awesome"
      @helper.project.should == "project-awesome"
    end

    it "should exit due to missing origin" do
      @helper.should_receive(:url_for).twice.with(:origin).and_return("")
      STDERR.should_receive(:puts).with("Error: missing remote 'origin'")
      lambda { @helper.project }.should raise_error(SystemExit)
    end

    it "should exit due to non-github origin" do
      @helper.should_receive(:url_for).twice.with(:origin).and_return("home:path/to/repo.git")
      STDERR.should_receive(:puts).with("Error: remote 'origin' is not a github URL")
      lambda { @helper.project }.should raise_error(SystemExit)
    end
  end

  helper :repo_for do
    it "should return mephisto.git" do
      setup_url_for :mojombo, "mojombo", "mephisto"
      @helper.repo_for(:mojombo).should == "mephisto.git"
    end
  end

  helper :user_and_repo_from do
    it "should parse a git:// url" do
      @helper.user_and_repo_from("git://github.com/defunkt/github.git").should == ["defunkt", "github.git"]
    end

    it "should parse a ssh-based url" do
      @helper.user_and_repo_from("git@github.com:mojombo/god.git").should == ["mojombo", "god.git"]
    end

    it "should parse a non-standard ssh-based url" do
      @helper.user_and_repo_from("ssh://git@github.com:mojombo/god.git").should == ["mojombo", "god.git"]
      @helper.user_and_repo_from("github.com:mojombo/god.git").should == ["mojombo", "god.git"]
      @helper.user_and_repo_from("ssh://github.com:mojombo/god.git").should == ["mojombo", "god.git"]
    end

    it "should return nothing for other urls" do
      @helper.user_and_repo_from("home:path/to/repo.git").should == nil
    end

    it "should return nothing for invalid git:// urls" do
      @helper.user_and_repo_from("git://github.com/foo").should == nil
    end

    it "should return nothing for invalid ssh-based urls" do
      @helper.user_and_repo_from("git@github.com:kballard").should == nil
      @helper.user_and_repo_from("git@github.com:kballard/test/repo.git").should == nil
      @helper.user_and_repo_from("ssh://git@github.com:kballard").should == nil
      @helper.user_and_repo_from("github.com:kballard").should == nil
      @helper.user_and_repo_from("ssh://github.com:kballard").should == nil
    end
  end

  helper :user_for do
    it "should return defunkt" do
      setup_url_for :origin, "defunkt"
      @helper.user_for(:origin).should == "defunkt"
    end
  end

  helper :url_for do
    it "should call out to the shell" do
      @helper.should_receive(:`).with("git config --get remote.origin.url").and_return "git://github.com/user/project.git\n"
      @helper.url_for(:origin).should == "git://github.com/user/project.git"
    end
  end

  helper :remotes do
    it "should return a list of remotes" do
      @helper.should_receive(:`).with('git config --get-regexp \'^remote\.(.+)\.url$\'').and_return <<-EOF
remote.origin.url git@github.com:kballard/github-gem.git
remote.defunkt.url git://github.com/defunkt/github-gem.git
remote.nex3.url git://github.com/nex3/github-gem.git
      EOF
      @helper.remotes.should == {
        :origin => "git@github.com:kballard/github-gem.git",
        :defunkt => "git://github.com/defunkt/github-gem.git",
        :nex3 => "git://github.com/nex3/github-gem.git"
      }
    end
  end

  helper :remote_branches_for do
    it "should return an empty list because no user was provided" do
      @helper.remote_branches_for(nil).should == nil
    end

    it "should return a list of remote branches for defunkt" do
      @helper.should_receive(:`).with('git ls-remote -h defunkt 2> /dev/null').and_return <<-EOF
fe1f852f3cf719c7cd86147031732f570ad89619	refs/heads/kballard/master
f8a6bb42b0ed43ac7336bfcda246e59a9da949d6	refs/heads/master
624d9c2f742ff24a79353a7e02bf289235c72ff1	refs/heads/restart
      EOF
      @helper.remote_branches_for("defunkt").should == {
        "master"          => "f8a6bb42b0ed43ac7336bfcda246e59a9da949d6",
        "kballard/master" => "fe1f852f3cf719c7cd86147031732f570ad89619",
        "restart"         => "624d9c2f742ff24a79353a7e02bf289235c72ff1"
      }
    end

    it "should return an empty list of remote branches for nex3 and nex4" do
      # the following use-case should never happen as the -h parameter should only return heads on remote branches
      # however, we are testing this particular case to verify how remote_branches_for would respond if random
      # git results
      @helper.should_receive(:`).with('git ls-remote -h nex3 2> /dev/null').and_return <<-EOF
fe1f852f3cf719c7cd86147031732f570ad89619	HEAD
a1a392369e5b7842d01cce965272d4b96c2fd343	refs/tags/v0.1.3
624d9c2f742ff24a79353a7e02bf289235c72ff1	refs/remotes/origin/master
random
	random_again
      EOF
      @helper.remote_branches_for("nex3").should be_empty

      @helper.should_receive(:`).with('git ls-remote -h nex4 2> /dev/null').and_return ""
      @helper.remote_branches_for("nex4").should be_empty
    end
  end

  helper :remote_branch? do
    it "should return whether the branch exists at the remote user" do
      @helper.should_receive(:remote_branches_for).with("defunkt").any_number_of_times.and_return({
        "master"          => "f8a6bb42b0ed43ac7336bfcda246e59a9da949d6",
        "kballard/master" => "fe1f852f3cf719c7cd86147031732f570ad89619",
        "restart"         => "624d9c2f742ff24a79353a7e02bf289235c72ff1"
      })
      @helper.remote_branch?("defunkt", "master").should == true
      @helper.remote_branch?("defunkt", "not_master").should == false
    end
  end

  helper :branch_dirty? do
    it "should return false" do
      @helper.should_receive(:system).with(/^git diff/).and_return(0, 0)
      @helper.branch_dirty?.should == 0
    end

    it "should return true" do
      @helper.should_receive(:system).with(/^git diff/).and_return(1, 1, 0, 1)
      @helper.branch_dirty?.should == 1
      @helper.branch_dirty?.should == 1
    end
  end

  helper :tracking do
    it "should return a list of remote/user_or_url pairs" do
      @helper.should_receive(:remotes).and_return({
        :origin => "git@github.com:kballard/github-gem.git",
        :defunkt => "git://github.com/defunkt/github-gem.git",
        :external => "server:path/to/github-gem.git"
      })
      @helper.tracking.should == {
        :origin => "kballard",
        :defunkt => "defunkt",
        :external => "server:path/to/github-gem.git"
      }
    end
  end

  helper :tracking? do
    it "should return whether the user is tracked" do
      @helper.should_receive(:tracking).any_number_of_times.and_return({
        :origin => "kballard",
        :defunkt => "defunkt",
        :external => "server:path/to/github-gem.git"
      })
      @helper.tracking?("kballard").should == true
      @helper.tracking?("defunkt").should == true
      @helper.tracking?("nex3").should == false
    end
  end

  helper :user_and_branch do
    it "should return owner and branch for unqualified branches" do
      setup_url_for
      @helper.should_receive(:`).with("git rev-parse --symbolic-full-name HEAD").and_return "refs/heads/master"
      @helper.user_and_branch.should == ["user", "master"]
    end

    it "should return user and branch for user/branch-style branches" do
      @helper.should_receive(:`).with("git rev-parse --symbolic-full-name HEAD").and_return "refs/heads/defunkt/wip"
      @helper.user_and_branch.should == ["defunkt", "wip"]
    end
  end

  helper :open do
    it "should launch the URL when Launchy is installed" do
      begin
        require 'launchy'

        @helper.should_receive(:gem).with('launchy')
        Launchy::Browser.next_instance.tap do |browser|
          browser.should_receive(:my_os_family).any_number_of_times.and_return :windows # avoid forking
          if RUBY_PLATFORM =~ /mingw|mswin/
            browser.should_receive(:system).with("start http://www.google.com")
          else
            browser.should_receive(:system).with("/usr/bin/open http://www.google.com")
          end
          @helper.open "http://www.google.com"
        end
      rescue LoadError
        fail "Launchy is required for this spec"
      end
    end

    it "should fail when Launchy is not installed" do
      @helper.should_receive(:gem).with('launchy').and_raise(Gem::LoadError)
      STDERR.should_receive(:puts).with("Sorry, you need to install launchy: `gem install launchy`")
      @helper.open "http://www.google.com"
    end
  end
end
