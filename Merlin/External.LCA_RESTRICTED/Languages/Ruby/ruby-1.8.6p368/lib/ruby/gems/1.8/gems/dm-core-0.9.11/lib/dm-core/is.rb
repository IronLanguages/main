module DataMapper
  module Is

    def is(plugin, *pars, &block)
      generator_method = "is_#{plugin}".to_sym

      if self.respond_to?(generator_method)
        self.send(generator_method, *pars, &block)
      else
        raise PluginNotFoundError, "could not find plugin named #{plugin}"
      end
    end

    Model.send(:include, self)
  end # module Is
end # module DataMapper
