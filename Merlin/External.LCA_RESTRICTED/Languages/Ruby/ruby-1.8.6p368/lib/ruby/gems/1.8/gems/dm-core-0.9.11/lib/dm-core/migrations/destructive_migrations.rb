# TODO: move to dm-more/dm-migrations

module DataMapper
  module DestructiveMigrations
    def self.included(model)
      DestructiveMigrator.models << model
    end
  end # module DestructiveMigrations

  class DestructiveMigrator < Migrator
    def self.migrate(repository_name)
      models.each do |model|
        model.auto_migrate!
      end
    end
  end # class DestructiveMigrator
end # module DataMapper
