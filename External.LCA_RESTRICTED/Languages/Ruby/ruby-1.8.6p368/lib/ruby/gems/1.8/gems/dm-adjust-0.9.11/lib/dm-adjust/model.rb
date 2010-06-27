module DataMapper
  module Model

    def adjust(attributes = {}, reload = false)
      raise NotImplementedError, 'adjust *with* validations has not be written yet, try adjust!'
    end

    ##
    # increment or decrement attributes on all objects in a resource
    #
    # @example [Usage]
    #   * People.adjust(:salary => +1000)
    #   * Children.adjust(:allowance => -100)
    #
    # @param attributes <Hash> A hash of attributes to adjust, and their adjustment
    # @public
    def adjust!(attributes,reload=false)
      all.adjust!(attributes,reload)
    end
  end # Model
end # DataMapper
