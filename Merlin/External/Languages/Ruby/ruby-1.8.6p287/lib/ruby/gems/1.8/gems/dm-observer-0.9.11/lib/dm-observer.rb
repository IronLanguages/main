module DataMapper
  # Observers allow you to add callback hooks to DataMapper::Resource objects
  # in a separate class. This is great for separating out logic that is not
  # really part of the model, but needs to be triggered by a model, or models.
  module Observer

    def self.included(klass)
      klass.extend(ClassMethods)
    end

    module ClassMethods

      attr_accessor :observing

      def initialize
        self.observing = []
      end

      # Assign an Array of Class names to watch.
      #   observe User, Article, Topic
      def observe(*args)
        # puts "#{self.to_s} observing... #{args.collect{|c| Extlib::Inflection.classify(c.to_s)}.join(', ')}"
        self.observing = args
      end

      def before(sym, &block)
        self.observing.each do |klass|
          klass.before(sym.to_sym, &block)
        end
      end

      def after(sym, &block)
        self.observing.each do |klass|
          klass.after(sym.to_sym, &block)
        end
      end

      def before_class_method(sym, &block)
        self.observing.each do |klass|
          klass.before_class_method(sym.to_sym, &block)
        end
      end

      def after_class_method(sym, &block)
        self.observing.each do |klass|
          klass.after_class_method(sym.to_sym, &block)
        end
      end

    end # ClassMethods

  end # Observer
end # DataMapper

if $0 == __FILE__
  require 'rubygems'

  gem 'dm-core', '~>0.9.11'
  require 'dm-core'

  FileUtils.touch(File.join(Dir.pwd, "migration_test.db"))
  DataMapper.setup(:default, "sqlite3://#{Dir.pwd}/migration_test.db")

  class Foo
    include DataMapper::Resource

    property :id, Integer, :serial => true
    property :bar, Text
  end

  Foo.auto_migrate!

  class FooObserver
    include DataMapper::Observer

    observe :foo

    before :save do
      raise "Hell!" if self.bar.nil?
      puts "hi"
    end

    after :save do
      puts "bye"
    end

  end

  Foo.new(:bar => "hello").save

end
