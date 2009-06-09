if defined?(Merb::Plugins)
  dependency 'dm-core'

  require File.dirname(__FILE__) / "merb" / "orms" / "data_mapper" / "connection"
  require File.dirname(__FILE__) / "merb" / "session" / "data_mapper_session"
  Merb::Plugins.add_rakefiles "merb_datamapper" / "merbtasks"

  # conditionally assign things, so as not to override previously set options.
  # This is most relevent for :use_repository_block, which is used later in this file.
  unless Merb::Plugins.config[:merb_datamapper].has_key?(:use_repository_block)
    Merb::Plugins.config[:merb_datamapper][:use_repository_block] = true
  end

  unless Merb::Plugins.config[:merb_datamapper].has_key?(:session_storage_name)
    Merb::Plugins.config[:merb_datamapper][:session_storage_name] = 'sessions'
  end

  unless Merb::Plugins.config[:merb_datamapper].has_key?(:session_repository_name)
    Merb::Plugins.config[:merb_datamapper][:session_repository_name] = :default
  end


  class Merb::Orms::DataMapper::Connect < Merb::BootLoader
    after BeforeAppLoads

    def self.run
      Merb.logger.verbose! "Merb::Orms::DataMapper::Connect block."

      # check for the presence of database.yml
      if File.file?(Merb.dir_for(:config) / "database.yml")
        # if we have it, connect
        Merb::Orms::DataMapper.connect
      else
        # assume we'll be told at some point
        Merb.logger.info "No database.yml file found in #{Merb.dir_for(:config)}, assuming database connection(s) established in the environment file in #{Merb.dir_for(:config)}/environments"
      end

      # if we use a datamapper session store, require it.
      Merb.logger.verbose! "Checking if we need to use DataMapper sessions"
      if Merb::Config.session_stores.include?(:datamapper)
        Merb.logger.verbose! "Using DataMapper sessions"
        require File.dirname(__FILE__) / "merb" / "session" / "data_mapper_session"
      end

      # take advantage of the fact #id returns the key of the model, unless #id is a property
      Merb::Router.root_behavior = Merb::Router.root_behavior.identify(DataMapper::Resource => :id)

      Merb.logger.verbose! "Merb::Orms::DataMapper::Connect complete"
    end
  end

  class Merb::Orms::DataMapper::Associations < Merb::BootLoader
    after LoadClasses

    def self.run
      Merb.logger.verbose! 'Merb::Orms::DataMapper::Associations block'

      # make sure all relationships are initialized after loading
      descendants = DataMapper::Resource.descendants.dup
      descendants.dup.each do |model|
        descendants.merge(model.descendants) if model.respond_to?(:descendants)
      end
      descendants.each do |model|
        model.relationships.each_value { |r| r.child_key }
      end

      Merb.logger.verbose! 'Merb::Orms::DataMapper::Associations complete'
    end
  end

  if Merb::Plugins.config[:merb_datamapper][:use_repository_block]
    # wrap action in repository block to enable identity map
    class Application < Merb::Controller
      override! :_call_action
      def _call_action(*)
        repository do |r|
          Merb.logger.debug "In repository block #{r.name}"
          super
        end
      end
    end
  end

  generators = File.join(File.dirname(__FILE__), 'generators')
  Merb.add_generators generators / 'data_mapper_model'
  Merb.add_generators generators / 'data_mapper_resource_controller'
  Merb.add_generators generators / 'data_mapper_migration'
  
  # Override bug in DM::Timestamps
  Merb::BootLoader.after_app_loads do
    module DataMapper
      module Timestamp
        private
        
        def set_timestamps
          return unless dirty? || new_record?
          TIMESTAMP_PROPERTIES.each do |name,(_type,proc)|
            if model.properties.has_property?(name)
              model.properties[name].set(self, proc.call(self, model.properties[name])) unless attribute_dirty?(name)
            end
          end
        end
      end
    end
  end
end
