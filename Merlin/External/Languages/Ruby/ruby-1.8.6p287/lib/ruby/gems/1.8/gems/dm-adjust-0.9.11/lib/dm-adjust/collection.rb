module DataMapper
  class Collection

    def adjust(attributes = {}, reload = false)
      raise NotImplementedError, 'adjust *with* validations has not be written yet, try adjust!'
    end

    ##
    # increment or decrement attributes on a collection
    #
    # @example [Usage]
    #   * People.all.adjust(:salary => +1000)
    #   * Children.all(:age.gte => 18).adjust(:allowance => -100)
    #
    # @param attributes <Hash> A hash of attributes to adjust, and their adjustment
    # @param reload <FalseClass,TrueClass> If true, affected objects will be reloaded
    #
    # @public
    def adjust!(attributes = {}, reload = false)
      return true if attributes.empty?

      adjust_attributes = {}

      model.properties(repository.name).slice(*attributes.keys).each do |property|
        adjust_attributes[property] = attributes[property.name] if property
      end

      each { |r| attributes.each_pair{|a,v| r.attribute_set(a,r.send(a) + v) }; r.save } if loaded?

      # if none of the attributes that are adjusted is part of the collection-query
      # there is no need to load the collection (it will not change after adjustment)
      # if the query contains a raw sql-string, we cannot (truly) know, and must load.
      altered = query.conditions.detect{|c| adjust_attributes.include?(c[1]) || c[0] == :raw }

      identity_map   = repository.identity_map(model)
      key_properties = model.key(repository.name)

      if identity_map.any? && reload
        reload_query = key_properties.zip(identity_map.keys.transpose).to_hash

        if altered
          keys = all(reload_query.merge(:fields => key_properties)).map { |r| r.key }
          reload_query = model.key(repository.name).zip(keys.transpose).to_hash
        end
      end

      repository.adjust(adjust_attributes,scoped_query)

      # Reload affected objects in identity-map. if collection was affected, dont use the scope.
      (altered ? model : self).all(reload_query).reload(:fields => attributes.keys) if reload_query && reload_query.any?

      # if preload was set to false, and collection was affected by updates,
      # something is now officially borked. We'll try the best we can (still many cases this is borked for)
      query.conditions.each do |c|
        if adjustment = adjust_attributes[c[1]]
          case c[2]
            when Numeric then c[2] += adjustment
            when Range   then c[2] = (c[2].first+adjustment)..(c[2].last+adjustment)
          end if adjustment = adjust_attributes[c[1]]
        end
      end if altered

      true
    end
  end # Collection
end # DataMapper
