module DataMapper
  module Is
    ##
    # = Is Versioned
    # The Versioned module will configure a model to be versioned.
    #
    # The is-versioned plugin functions differently from other versioning
    # solutions (such as acts_as_versioned), but can be configured to
    # function like it if you so desire.
    #
    # The biggest difference is that there is not an incrementing 'version'
    # field, but rather, any field of your choosing which will be unique
    # on update.
    #
    # == Setup
    # For simplicity, I will assume that you have loaded dm-timestamps to
    # automatically update your :updated_at field. See versioned_spec for
    # and example of updating the versioned field yourself.
    #
    #   class Story
    #     include DataMapper::Resource
    #     property :id, Serial
    #     property :title, String
    #     property :updated_at, DateTime
    #
    #     is_versioned :on => [:updated_at]
    #   end
    #
    # == Auto Upgrading and Auto Migrating
    #
    #   Story.auto_migrate! # => will run auto_migrate! on Story::Version, too
    #   Story.auto_upgrade! # => will run auto_upgrade! on Story::Version, too
    #
    # == Usage
    #
    #   story = Story.get(1)
    #   story.title = "New Title"
    #   story.save # => Saves this story and creates a new version with the
    #              #    original values.
    #   story.versions.size # => 1
    #
    #   story.title = "A Different New Title"
    #   story.save
    #   story.versions.size # => 2
    #
    # TODO: enable replacing a current version with an old version.
    module Versioned

      def is_versioned(options = {})
        on = options[:on]

        class << self; self end.class_eval do
          define_method :const_missing do |name|
            storage_name = Extlib::Inflection.tableize(self.name + "Version")
            model = DataMapper::Model.new(storage_name)

            if name == :Version
              properties.each do |property|
                options = property.options
                options[:key] = true if property.name == on || options[:serial] == true
                options[:serial] = false
                model.property property.name, property.type, options
              end

              self.const_set("Version", model)
            else
              super(name)
            end
          end
        end

        self.after_class_method :auto_migrate! do
          self::Version.auto_migrate!
        end

        self.after_class_method :auto_upgrade! do
          self::Version.auto_upgrade!
        end

        self.before :attribute_set do |property, value|
          pending_version_attributes[property] ||= self.attribute_get(property)
        end

        self.after :update do |result|
          if result && dirty_attributes.has_key?(properties[on])
            self.class::Version.create(self.attributes.merge(pending_version_attributes))
            self.pending_version_attributes.clear
          end

          result
        end

        include DataMapper::Is::Versioned::InstanceMethods
      end


      module InstanceMethods
        ##
        # Returns a hash of original values to be stored in the
        # versions table when a new version is created. It is
        # cleared after a version model is created.
        #
        # --
        # @return <Hash>
        def pending_version_attributes
          @pending_version_attributes ||= {}
        end

        ##
        # Returns a collection of other versions of this resource.
        # The versions are related on the models keys, and ordered
        # by the version field.
        #
        # --
        # @return <Collection>
        def versions
          query = {}
          version = self.class.const_get("Version")
          self.class.key.zip(self.key) { |property, value| query[property.name] = value }
          query.merge(:order => version.key.collect { |key| key.name.desc })
          version.all(query)
        end
      end

    end # Versioned
  end # Is
end # DataMapper
