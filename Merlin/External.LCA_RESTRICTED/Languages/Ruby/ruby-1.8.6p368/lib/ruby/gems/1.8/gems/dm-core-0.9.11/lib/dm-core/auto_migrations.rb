# TODO: move to dm-more/dm-migrations

module DataMapper
  class AutoMigrator
    ##
    # Destructively automigrates the data-store to match the model.
    # First migrates all models down and then up.
    # REPEAT: THIS IS DESTRUCTIVE
    #
    # @param Symbol repository_name the repository to be migrated
    def self.auto_migrate(repository_name = nil, *descendants)
      auto_migrate_down(repository_name, *descendants)
      auto_migrate_up(repository_name, *descendants)
    end

    ##
    # Destructively automigrates the data-store down
    # REPEAT: THIS IS DESTRUCTIVE
    #
    # @param Symbol repository_name the repository to be migrated
    # @calls DataMapper::Resource#auto_migrate_down!
    # @api private
    def self.auto_migrate_down(repository_name = nil, *descendants)
      descendants = DataMapper::Resource.descendants.to_a if descendants.empty?
      descendants.reverse.each do |model|
        model.auto_migrate_down!(repository_name)
      end
    end

    ##
    # Automigrates the data-store up
    #
    # @param Symbol repository_name the repository to be migrated
    # @calls DataMapper::Resource#auto_migrate_up!
    # @api private
    def self.auto_migrate_up(repository_name = nil, *descendants)
      descendants = DataMapper::Resource.descendants.to_a if descendants.empty?
      descendants.each do |model|
        model.auto_migrate_up!(repository_name)
      end
    end

    ##
    # Safely migrates the data-store to match the model
    # preserving data already in the data-store
    #
    # @param Symbol repository_name the repository to be migrated
    # @calls DataMapper::Resource#auto_upgrade!
    def self.auto_upgrade(repository_name = nil)
      DataMapper::Resource.descendants.each do |model|
        model.auto_upgrade!(repository_name)
      end
    end
  end # class AutoMigrator

  module AutoMigrations
    ##
    # Destructively automigrates the data-store to match the model
    # REPEAT: THIS IS DESTRUCTIVE
    #
    # @param Symbol repository_name the repository to be migrated
    def auto_migrate!(repository_name = self.repository_name)
      auto_migrate_down!(repository_name)
      auto_migrate_up!(repository_name)
    end

    ##
    # Destructively migrates the data-store down, which basically
    # deletes all the models.
    # REPEAT: THIS IS DESTRUCTIVE
    #
    # @param Symbol repository_name the repository to be migrated
    # @api private
    def auto_migrate_down!(repository_name = self.repository_name)
      # repository_name ||= default_repository_name
      repository(repository_name) do |r|
        r.adapter.destroy_model_storage(r, self.base_model)
      end
    end

    ##
    # Auto migrates the data-store to match the model
    #
    # @param Symbol repository_name the repository to be migrated
    # @api private
    def auto_migrate_up!(repository_name = self.repository_name)
      repository(repository_name) do |r|
        r.adapter.create_model_storage(r, self.base_model)
      end
    end

    ##
    # Safely migrates the data-store to match the model
    # preserving data already in the data-store
    #
    # @param Symbol repository_name the repository to be migrated
    def auto_upgrade!(repository_name = self.repository_name)
      repository(repository_name) do |r|
        r.adapter.upgrade_model_storage(r, self)
      end
    end

    Model.send(:include, self)
  end # module AutoMigrations
end # module DataMapper
