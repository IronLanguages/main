module DataMapper

  # Use these modules to establish naming conventions.
  # The default is UnderscoredAndPluralized.
  # You assign a naming convention like so:
  #
  #   repository(:default).adapter.resource_naming_convention = NamingConventions::Resource::Underscored
  #
  # You can also easily assign a custom convention with a Proc:
  #
  #   repository(:default).adapter.resource_naming_convention = lambda do |value|
  #     'tbl' + value.camelize(true)
  #   end
  #
  # Or by simply defining your own module in NamingConventions that responds to
  # ::call.
  #
  # NOTE: It's important to set the convention before accessing your models
  # since the resource_names are cached after first accessed.
  # DataMapper.setup(name, uri) returns the Adapter for convenience, so you can
  # use code like this:
  #
  #   adapter = DataMapper.setup(:default, "mock://localhost/mock")
  #   adapter.resource_naming_convention = DataMapper::NamingConventions::Resource::Underscored
  module NamingConventions

    module Resource

      module UnderscoredAndPluralized
        def self.call(name)
          Extlib::Inflection.pluralize(Extlib::Inflection.underscore(name)).gsub('/','_')
        end
      end # module UnderscoredAndPluralized

      module UnderscoredAndPluralizedWithoutModule
        def self.call(name)
          Extlib::Inflection.pluralize(Extlib::Inflection.underscore(Extlib::Inflection.demodulize(name)))
        end
      end # module UnderscoredAndPluralizedWithoutModule

      module Underscored
        def self.call(name)
          Extlib::Inflection.underscore(name)
        end
      end # module Underscored

      module Yaml
        def self.call(name)
          Extlib::Inflection.pluralize(Extlib::Inflection.underscore(name)) + ".yaml"
        end
      end # module Yaml

    end # module Resource

    module Field

      module UnderscoredAndPluralized
        def self.call(property)
          Extlib::Inflection.pluralize(Extlib::Inflection.underscore(property.name.to_s)).gsub('/','_')
        end
      end # module UnderscoredAndPluralized

      module UnderscoredAndPluralizedWithoutModule
        def self.call(property)
          Extlib::Inflection.pluralize(Extlib::Inflection.underscore(Extlib::Inflection.demodulize(property.name.to_s)))
        end
      end # module UnderscoredAndPluralizedWithoutModule

      module Underscored
        def self.call(property)
          Extlib::Inflection.underscore(property.name.to_s)
        end
      end # module Underscored

      module Yaml
        def self.call(property)
          Extlib::Inflection.pluralize(Extlib::Inflection.underscore(property.name.to_s)) + ".yaml"
        end
      end # module Yaml

    end # module Field

  end # module NamingConventions
end # module DataMapper
