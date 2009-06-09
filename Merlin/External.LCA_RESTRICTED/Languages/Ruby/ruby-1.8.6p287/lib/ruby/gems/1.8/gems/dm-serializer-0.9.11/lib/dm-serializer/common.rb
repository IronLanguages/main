module DataMapper
  module Serialize
    # Returns propreties to serialize based on :only or :exclude arrays, if provided
    # :only takes precendence over :exclude
    #
    # @return <Array> properties that need to be serialized
    def properties_to_serialize(options)
      only_properties     = Array(options[:only])
      excluded_properties = Array(options[:exclude])

      self.class.properties(repository.name).reject do |p|
        if only_properties.include? p.name
          false
        else
          excluded_properties.include?(p.name) || !(only_properties.empty? || only_properties.include?(p.name))
        end
      end
    end
  end

  module Resource
    include Serialize
  end
end
