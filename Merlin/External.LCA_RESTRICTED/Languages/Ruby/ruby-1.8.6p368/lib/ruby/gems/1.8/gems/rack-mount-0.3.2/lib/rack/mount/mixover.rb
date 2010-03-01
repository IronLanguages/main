module Rack::Mount
  # A mixin that changes the behavior of +include+. Instead of modules
  # being chained as a superclass, they are mixed into the objects
  # metaclass. This allows mixins to be stacked ontop of the instance
  # methods.
  module Mixover
    module InstanceMethods #:nodoc:
      def dup
        obj = super
        included_modules = (class << self; included_modules; end) - (class << obj; included_modules; end)
        included_modules.reverse.each { |mod| obj.extend(mod) }
        obj
      end
    end

    # Replaces include with a lazy version.
    def include(*mod)
      (@included_modules ||= []).push(*mod)
    end

    def new(*args, &block) #:nodoc:
      obj = allocate
      obj.extend(InstanceMethods)
      (@included_modules ||= []).each { |mod| obj.extend(mod) }
      obj.send(:initialize, *args, &block)
      obj
    end

    # Create a new class without an included module.
    def new_without_module(mod, *args, &block)
      old_included_modules = (@included_modules ||= []).dup
      @included_modules.delete(mod)
      new(*args, &block)
    ensure
      @included_modules = old_included_modules
    end

    # Create a new class temporarily with a module.
    def new_with_module(mod, *args, &block)
      old_included_modules = (@included_modules ||= []).dup
      include(mod)
      new(*args, &block)
    ensure
      @included_modules = old_included_modules
    end
  end
end
