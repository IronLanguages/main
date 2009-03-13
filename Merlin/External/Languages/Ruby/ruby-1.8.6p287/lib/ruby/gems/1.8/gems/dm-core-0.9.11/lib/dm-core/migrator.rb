# TODO: move to dm-more/dm-migrations

module DataMapper
  class Migrator
    def self.subclasses
      @@subclasses ||= []
    end

    def self.subclasses=(obj)
      @@subclasses = obj
    end

    def self.inherited(klass)
      subclasses << klass

      class << klass
        def models
          @models ||= []
        end
      end
    end

    def self.migrate(repository_name)
      subclasses.collect do |migrator|
        migrator.migrate(repository_name)
      end.flatten
    end
  end # class Migrator
end # module DataMapper
