require File.expand_path(File.join(File.dirname(__FILE__), '..', '..', 'spec_helper'))
require 'pp'
describe "OneToMany" do
  before(:all) do
    class ::Team
      include DataMapper::Resource

      def self.default_repository_name; ADAPTER end

      property :id, Serial
      property :name, String
      property :class_type, Discriminator

      has n, :players
    end

    class ::BaseballTeam < Team
    end

    class ::Player
      include DataMapper::Resource

      def self.default_repository_name; ADAPTER end

      property :id, Serial
      property :name, String

      belongs_to :team
    end

    [Team, Player].each { |k| k.auto_migrate!(ADAPTER) }

    Team.create(:name => "Cowboys")
    BaseballTeam.create(:name => "Giants")
  end

  describe "(saved parent, saved child)" do
    before(:each) do
      @dc_united = Team.create
      @emilio = Player.create(:team => @dc_united)
    end

    it "child association should return parent" do
      @emilio.team.should == @dc_united
    end

    it "parent association should return children" do
      @dc_united.players.should == [@emilio]
    end
  end

  describe "(saved parent, unsaved child)" do
    before(:each) do
      @dc_united = Team.create
      @emilio = Player.new(:team => @dc_united)
    end

    it "child association should return parent" do
      @emilio.team.should == @dc_united
    end

    it "parent association should return children" do
      pending("DataMapper does not yet support in-memory associations") do
        @dc_united.players.should == [@emilio]
      end
    end
  end

  describe "(unsaved parent, saved child)" do
    before(:each) do
      @dc_united = Team.new
      @emilio = Player.create(:team => @dc_united)
    end

    it "child association should return parent" do
      @emilio.team.should == @dc_united
    end

    it "parent association should return children" do
      @dc_united.players.should == [@emilio]
    end

    it "should return true to child.save" do
      @emilio.should_not be_a_new_record
      @emilio.save.should be_true
    end
  end

  describe "(unsaved parent, unsaved child)" do
    before(:each) do
      @dc_united = Team.new
      @emilio = Player.new(:team => @dc_united)
    end

    it "child association should return parent" do
      @emilio.team.should == @dc_united
    end

    it "parent association should return children" do
      pending("DataMapper does not yet support in-memory associations") do
        @dc_united.players.should == [@emilio]
      end
    end
  end

  describe "parent initialized child" do
    before(:each) do
      @ajax = Team.create
      @vandesar = @ajax.players.new
      @vandesar.save
    end

    it "child association should return parent" do
      @vandesar.team.should == @ajax
    end

    it "parent association should return children" do
      @ajax.players.should == [@vandesar]
    end
  end

  it "unsaved parent model should accept array of hashes for association" do
    players = [{ :name => "Brett Favre" }, { :name => "Reggie White" }]

    team = Team.new(:name => "Packers", :players => players)
    team.players.zip(players) do |player, values|
      player.should be_an_instance_of(Player)
      values.each { |k, v| player.send(k).should == v }
    end

    players = team.players
    team.save

    repository(ADAPTER) do
      Team.get(3).players.should == players
    end
  end

  it "saved parent model should accept array of hashes for association" do
    players = [{ :name => "Troy Aikman" }, { :name => "Chad Hennings" }]

    team = Team.get(1)
    team.players = players
    team.players.zip(players) do |player, values|
      player.should be_an_instance_of(Player)
      values.each { |k, v| player.send(k).should == v }
    end

    players = team.players
    team.save

    repository(ADAPTER) do
      Team.get(1).players.should == players
    end
  end

  describe "STI" do
    before(:all) do
      repository(ADAPTER) do
        @player = Player.create(:name => "Barry Bonds", :team => BaseballTeam.first)
      end
    end

    it "should work for child.parent" do
      repository(ADAPTER) do
        @player.team.should == BaseballTeam.first
      end
    end

    it "should work for parent.children" do
      repository(ADAPTER) do
        team = BaseballTeam.first

        team.players.size.should > 0
        team.players.each{|p| p.should be_an_instance_of(Player)}
      end
    end
  end

  describe "alone" do
    it "should work for parent.children without any parents in IM" do
      repository(ADAPTER) do
        team = BaseballTeam.first
        team.players.each{|p| p.should be_an_instance_of(Player)}
      end
    end
  end
end
