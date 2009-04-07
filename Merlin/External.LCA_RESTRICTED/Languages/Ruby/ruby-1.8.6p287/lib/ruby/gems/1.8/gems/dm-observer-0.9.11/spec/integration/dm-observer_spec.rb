require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::Observer do
  before :all do
    class ::Adam
      include DataMapper::Resource

      property :id, Integer, :serial => true
      property :name, String
      attr_accessor :done

      def falling?
        @falling
      end

      def dig_a_hole_to_china
        @done = true
      end

      def drink
        @happy = true
      end

      def happy?
        @happy
      end

      def self.unite
        [Adam.new(:name => "Adam 1"), Adam.new(:name => "Adam 2")]
      end

    end
    Adam.auto_migrate!

    module ::Alcohol
      class Beer
        include DataMapper::Resource

        property :id, Integer, :serial => true
        property :name, String

        def drink
          @empty = true
        end

        def empty?
          @empty
        end

      end
    end
    Alcohol::Beer.auto_migrate!


    class ::AdamObserver
      include DataMapper::Observer

      observe Adam

      before :save do
        @falling = true
      end

      before :dig_a_hole_to_china do
        throw :halt
      end

      after_class_method :unite do
        raise "Call for help!"
      end

    end

    class ::DrinkingObserver
      include DataMapper::Observer

      observe Adam, Alcohol::Beer

      after :drink do
        @refrigerated = true
      end

    end

  end

  before(:each) do
    @adam = Adam.new
    @beer = Alcohol::Beer.new
  end

  it "should assign a callback" do
    @adam.should_not be_falling
    @adam.name = "Adam French"
    @adam.save
    @adam.should be_falling
  end

  it "should be able to trigger an abort" do
     @adam.dig_a_hole_to_china
     @adam.done.should be_nil
  end

  it "observe should add a class to the neighborhood watch" do
    AdamObserver.should have(1).observing
    AdamObserver.observing.first.should == Adam
  end

  it "observe should add more than one class to the neighborhood watch" do
    DrinkingObserver.should have(2).observing
    DrinkingObserver.observing.first.should == Adam
    DrinkingObserver.observing[1].should == Alcohol::Beer
  end

  it "should observe multiple classes with the same method name" do
    @adam.should_not be_happy
    @beer.should_not be_empty
    @adam.drink
    @beer.drink
    @adam.should be_happy
    @beer.should be_empty
  end

  it "should wrap class methods" do
    lambda {Adam.unite}.should raise_error('Call for help!')
  end

end
