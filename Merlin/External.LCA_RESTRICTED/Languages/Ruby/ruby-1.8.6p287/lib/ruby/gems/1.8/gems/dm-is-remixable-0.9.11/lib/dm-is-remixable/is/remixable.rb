# reopen sam/extlib/lib/extlib/object.rb
class Object

  def full_const_defined?(name)
    !!full_const_get(name) rescue false
  end

end

Extlib::Inflection.rule 'ess', 'esses'

module DataMapper
  module Is
    module Remixable

      #==============================INCLUSION METHODS==============================#

      # Adds remixer methods to DataMapper::Resource
      def self.included(base)
        base.send(:include,RemixerClassMethods)
        base.send(:include,RemixerInstanceMethods)
      end

      # - is_remixable
      # ==== Description
      #   Adds RemixeeClassMethods and RemixeeInstanceMethods to any model that is: remixable
      # ==== Examples
      # class User #Remixer
      #   remixes Commentable
      #   remixes Vote
      # end
      #
      # module Commentable #Remixable
      #   include DataMapper::Resource
      #
      #   is :remixable,
      #     :suffix => "comment"
      # end
      #
      # module Vote #Remixable
      #   include DataMapper::Resource
      #
      #   is :remixable
      #
      # ==== Notes
      #   These options are just available for whatever reason your Remixable Module name
      #   might not be what you'd like to see the table name and property accessor named.
      #   These are just configurable defaults, upon remixing the class_name and accessor there
      #   take precedence over the defaults set here
      # ==== Options
      #   :suffix   <String>
      #             Table suffix, defaults to YourModule.name.downcase.singular
      #             Yields table name of remixer_suffix; ie user_comments, user_votes
      def is_remixable(options={})
        extend  DataMapper::Is::Remixable::RemixeeClassMethods
        include DataMapper::Is::Remixable::RemixeeInstanceMethods
        @is_remixable = true

        # support clean suffixes for nested modules
        default_suffix = Extlib::Inflection.demodulize(self.name).singular.snake_case
        suffix(options.delete(:suffix) || default_suffix)
      end


      #==============================CLASS METHODS==============================#

      # - RemixerClassMethods
      # ==== Description
      #   Methods available to all DataMapper::Resources
      module RemixerClassMethods
        def self.included(base);end;

        def is_remixable?
          @is_remixable ||= false
        end

        # - remixables
        # ==== Description
        #   Returns a hash of the remixables used by this class
        # ==== Returns
        #   <Hash> Remixable Class Name => Remixed Class Name
        def remixables
          @remixables
        end

        # - remix
        # ==== Description
        #   Remixes a Remixable Module
        # ==== Parameters
        #   cardinality <~Fixnum> 1, n, x ...
        #   remixable   <Symbol> plural of remixable; i.e. Comment => :comments
        #   options     <Hash>   options hash
        #                       :class_name <String> Remixed Model name (Also creates a storage_name as tableize(:class_name))
        #                                   This is the class that will be created from the Remixable Module
        #                                   The storage_name can be changed via 'enhance' in the class that is remixing
        #                                   Default: self.name.downcase + "_" + remixable.suffix.pluralize
        #                       :as         <String> Alters the name that the remixable items will be available through, this WILL NOT
        #                                   create the standard accessor
        #                                   Default: tableize(:class_name)
        #                       :for|:on    <String> Class name to join to through Remixable
        #                                   This will create a M:M relationship THROUGH the remixable, rather than
        #                                   a 1:M with the remixable
        #                       :via        <String> changes the name of the second id in a unary relationship
        #                                   see example below; only used when remixing a module between the same class twice
        #                                   ie: self.class.to_s == options[:for||:on]
        #                       :unique     <Boolean> Only works with :for|:on; creates a unique composite key
        #                                   over the two table id's
        # ==== Examples
        # Given: User (Class), Addressable (Module)
        #
        #   One-To-Many; Class-To-Remixable
        #
        #   remix n, :addressables,
        #     :class_name => "UserAddress",
        #     :as         => "addresses"
        #
        #   Tables: users, user_addresses
        #   Classes: User, UserAddress
        #     User.user_addresses << UserAddress.new => Raise No Method Exception since it was alias with :as
        #     User.addresses << UserAddress.new
        #   --------------------------------------------
        #   --------------------------------------------
        #
        # Given: User (Class), Video (Class), Commentable (Module)
        #
        #   Many-To-Many; Class-To-Class through RemixableIntermediate (Video allows Commentable for User)
        #
        #   Video.remix n, :commentables
        #     :for        => 'User'    #:for & :on have same effect, just a choice of wording...
        #   --------------------------------------------
        #   --------------------------------------------
        #
        # Given: User (Class), User (Class), Commentable (Module)
        #
        #   Many-To-Many Unary relationship between User & User through comments
        #   User.remix n, :commentables, :as => "comments", :for => 'User', :via => "commentor"
        #   => This would create user_id and commentor_id as the
        #
        def remix(cardinality, remixable, options={})
          #A map for remixable names to Remixed Models
          @remixables = {} if @remixables.nil?

          # Allow nested modules to be remixable to better support using dm-is-remixable in gems
          # Example (from my upcoming dm-is-rateable gem)
          # remix n, "DataMapper::Is::Rateable::Rating", :as => :ratings
          remixable_module = Object.full_const_get(Extlib::Inflection.classify(remixable))

          unless remixable_module.is_remixable?
            raise Exception, "#{remixable_module} is not remixable"
          end

          #Merge defaults/options
          options = {
            :as         => nil,
            :class_name => Extlib::Inflection.classify(self.name.snake_case + "_" + remixable_module.suffix.pluralize),
            :for        => nil,
            :on         => nil,
            :unique     => false,
            :via        => nil
          }.merge(options)

          #Make sure the class hasn't been remixed already
          unless Object.full_const_defined?(Extlib::Inflection.classify(options[:class_name]))

            #Storage name of our remixed model
            options[:table_name] = Extlib::Inflection.tableize(options[:class_name])

            #Other model to mix with in case of M:M through Remixable
            options[:other_model] = options[:for] || options[:on]

            DataMapper.logger.info "Generating Remixed Model: #{options[:class_name]}"
            model = generate_remixed_model(remixable_module, options)

            # map the remixable to the remixed model
            # since this will be used from 'enhance api' i think it makes perfect sense to
            # always refer to a remixable by its demodulized snake_cased constant name
            remixable_key = Extlib::Inflection.demodulize(remixable_module.name).snake_case.to_sym
            populate_remixables_mapping(model, options.merge(:remixable_key => remixable_key))

            # attach RemixerClassMethods and RemixerInstanceMethods to remixer if defined by remixee
            if Object.full_const_defined? "#{remixable_module}::RemixerClassMethods"
              extend Object.full_const_get("#{remixable_module}::RemixerClassMethods")
            end

            if Object.full_const_defined? "#{remixable_module}::RemixerInstanceMethods"
              include Object.full_const_get("#{remixable_module}::RemixerInstanceMethods")
            end

            #Create relationships between Remixer and remixed class
            if options[:other_model]
              # M:M Class-To-Class w/ Remixable Module as intermediate table
              # has n and belongs_to (or One-To-Many)
              remix_many_to_many cardinality, model, options
            else
              # 1:M Class-To-Remixable
              # has n and belongs_to (or One-To-Many)
              remix_one_to_many cardinality, model, options
            end
          else
            DataMapper.logger.warn "#{__FILE__}:#{__LINE__} warning: already remixed constant #{options[:class_name]}"
          end
        end

        # - enhance
        # ==== Description
        #   Enhance a remix; allows nesting remixables, adding columns & functions to a remixed resource
        # ==== Parameters
        #   remixable <Symbol> Name of remixable to enhance (plural or singular name of is :remixable module)
        #   model_class <Class, symbol, String> Name of the remixable generated Model Class.
        #   block     <Proc>    Enhancements to perform
        # ==== Examples
        #   When you have one remixable:
        #
        #   class Video
        #     include DataMapper::Resource
        #     remix Comment
        #
        #     enhance :comments do
        #       remix n, :votes        #This would result in something like YouTubes Voting comments up/down
        #
        #       property :updated_at, DateTime
        #
        #       def backwards; self.test.reverse; end;
        #     end
        #
        #  When you remixe the same remixable modules twice:
        #
        #   class Article
        #     include DataMapper::Resource
        #     remix n, :taggings, :for => User, :class_name => "UserArticleTagging"
        #     remix n, :taggings, :for => Bot,  :class_name => "BotArticleTagging"
        #
        #     enhance :taggings, "UserArticleTagging" do
        #       property :updated_at, DateTime
        #       belongs_to :user
        #       belongs_to :tag
        #     end
        #
        #     enhance :taggings, "BotArticleTagging" do
        #       belongs_to :bot
        #       belongs_to :tag
        #     end
        def enhance(remixable,remixable_model=nil, &block)
          # always use innermost singular snake_cased constant name
          remixable_name = remixable.to_s.singular.snake_case.to_sym
          class_name = if remixable_model.nil?
            @remixables[remixable_name].keys.first
          else
            Extlib::Inflection.demodulize(remixable_model.to_s).snake_case.to_sym
          end

          model = @remixables[remixable_name][class_name][:model] unless @remixables[remixable_name][class_name].nil?

          unless model.nil?
            model.class_eval &block
          else
            raise Exception, "#{remixable} must be remixed with :class_name option as #{remixable_model} before it can be enhanced"
          end
        end

        private

        # - populate_remixables_mapping
        # ==== Description
        #   Populates the Hash of remixables with information about the remixable
        # ==== Parameters
        #   remixable
        #   options <Hash> options hash
        def populate_remixables_mapping(remixable_model, options)
          key = options[:remixable_key]
          accessor_name = options[:as] ? options[:as] : options[:table_name]
          @remixables[key] ||= {}
          model_key = Extlib::Inflection.demodulize(remixable_model.to_s).snake_case.to_sym
          @remixables[key][model_key] ||= {}
          @remixables[key][model_key][:reader] ||= accessor_name.to_sym
          @remixables[key][model_key][:writer] ||= "#{accessor_name}=".to_sym
          @remixables[key][model_key][:model] ||= remixable_model
        end

        # - remix_one_to_many
        # ==== Description
        #   creates a one to many relationship Class has many of remixed model
        # ==== Parameters
        #   cardinality <Fixnum> cardinality of relationship
        #   model       <Class> remixed model that 'self' is relating to
        #   options     <Hash> options hash
        def remix_one_to_many(cardinality, model, options)
          self.has cardinality, (options[:as] || options[:table_name]).to_sym, :class_name => model.name
          model.property Extlib::Inflection.foreign_key(self.name).intern, Integer, :nullable => false
          model.belongs_to belongs_to_name(self.name)
        end

        # - remix_many_to_many
        # ==== Description
        #   creates a many to many relationship between two DataMapper models THROUGH a Remixable module
        # ==== Parameters
        #   cardinality <Fixnum> cardinality of relationship
        #   model       <Class> remixed model that 'self' is relating through
        #   options     <Hash> options hash
        def remix_many_to_many(cardinality, model, options)
          options[:other_model] = Object.full_const_get(Extlib::Inflection.classify(options[:other_model]))
          
          #TODO if options[:unique] the two *_id's need to be a unique composite key, maybe even
          # attach a validates_is_unique if the validator is included.
          puts " ~ options[:unique] is not yet supported" if options[:unique]

          # Is M:M between two different classes or the same class
          unless self.name == options[:other_model].name
            self.has cardinality, (options[:as] || options[:table_name]).to_sym, :class_name => model.name
            options[:other_model].has cardinality, options[:table_name].intern

            model.belongs_to belongs_to_name(self.name)
            model.belongs_to belongs_to_name(options[:other_model].name)
          else
            raise Exception, "options[:via] must be specified when Remixing a module between two of the same class" unless options[:via]

            self.has cardinality, (options[:as] || options[:table_name]).to_sym, :class_name => model.name
            model.belongs_to belongs_to_name(self.name)
            model.belongs_to options[:via].intern, :class_name => options[:other_model].name, :child_key => ["#{options[:via]}_id".intern]
          end
        end

        # - generate_remixed_model
        # ==== Description
        #   Generates a Remixed Model Class from a Remixable Module and options
        # ==== Parameters
        #   remixable <Module> module that is being remixed
        #   options   <Hash> options hash
        # ==== Returns
        #   <Class> remixed model
        def generate_remixed_model(remixable,options)
          #Create Remixed Model
          klass = Class.new Object do
            include DataMapper::Resource
          end

          #Give remixed model a name and create its constant
          model = Object.full_const_set(options[:class_name], klass)

          #Get instance methods & validators
          model.send(:include,remixable)

          #port the properties over...
          remixable.properties.each do |prop|
            model.property(prop.name, prop.type, prop.options)
          end

          # attach remixed model access to RemixeeClassMethods and RemixeeInstanceMethods if defined
          if Object.full_const_defined? "#{remixable}::RemixeeClassMethods"
            model.send :extend, Object.full_const_get("#{remixable}::RemixeeClassMethods")
          end

          if Object.full_const_defined? "#{remixable}::RemixeeInstanceMethods"
            model.send :include, Object.full_const_get("#{remixable}::RemixeeInstanceMethods")
          end

          model
        end

        def belongs_to_name(class_name)
          class_name.to_const_path.gsub(/\//, '_').to_sym
        end

      end # RemixerClassMethods

      # - RemixeeClassMethods
      # ==== Description
      #   Methods available to any model that is :remixable
      module RemixeeClassMethods
        # - suffix
        # ==== Description
        #   modifies the storage name suffix, which is by default based on the Remixable Module name
        # ==== Parameters
        #   suffix <String> storage name suffix to use (singular)
        def suffix(sfx=nil)
          @suffix = sfx unless sfx.nil?
          @suffix
        end

        # Squash auto_migrate!
        # model.auto_migrate! never gets called directly from dm-core/auto_migrations.rb
        # The models are explicitly migrated down and up again.
        def auto_migrate_up!(args=nil)
          DataMapper.logger.warn("Skipping auto_migrate_up! for remixable module (#{self.name})")
        end

        def auto_migrate_down!(args=nil)
          DataMapper.logger.warn("Skipping auto_migrate_down! for remixable module (#{self.name})")
        end

        #Squash auto_upgrade!
        def auto_upgrade!(args=nil)
          DataMapper.logger.warn("Skipping auto_upgrade! for remixable module (#{self.name})")
        end
      end # RemixeeClassMethods


      #==============================INSTANCE METHODS==============================#

      module RemixeeInstanceMethods
        def self.included(base);end;
      end # RemixeeInstanceMethods

      module RemixerInstanceMethods
        def self.included(base);end;
      end # RemixerInstanceMethods

    end # Example
  end # Is
end # DataMapper
