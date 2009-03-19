module DataMapper
  ##
  #
  # DataMapper's DependencyQueue is used to store callbacks for classes which
  # may or may not be loaded already.
  #
  class DependencyQueue
    def initialize
      @dependencies = {}
    end

    def add(class_name, &callback)
      @dependencies[class_name] ||= []
      @dependencies[class_name] << callback
      resolve!
    end

    def resolve!
      @dependencies.each do |class_name, callbacks|
        begin
          klass = Object.find_const(class_name)
          callbacks.each do |callback|
            callback.call(klass)
          end
          callbacks.clear
        rescue NameError
        end
      end
    end

  end # class DependencyQueue
end # module DataMapper
