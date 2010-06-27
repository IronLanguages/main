begin
  require 'nested_multimap'
rescue LoadError
  $: << File.expand_path(File.join(File.dirname(__FILE__), 'vendor/multimap'))
  require 'nested_multimap'
end

module Rack::Mount
  class Multimap < NestedMultimap #:nodoc:
    def self.[](*args)
      map = super
      map.instance_variable_set('@fuzz', {})
      map
    end

    def initialize(default = [])
      @fuzz = {}
      super
    end

    def initialize_copy(original)
      @fuzz = original.instance_variable_get('@fuzz').dup
      super
    end

    def store(*args)
      keys  = args.dup
      value = keys.pop
      key   = keys.shift

      raise ArgumentError, 'wrong number of arguments (1 for 2)' unless value

      unless key.respond_to?(:=~)
        raise ArgumentError, "unsupported key: #{args.first.inspect}"
      end

      if key.is_a?(Regexp)
        @fuzz[value] = key
        if keys.empty?
          @hash.each_pair { |k, l| l << value if k =~ key }
          self.default << value
        else
          @hash.each_pair { |k, _|
            if k =~ key
              args[0] = k
              super(*args)
            end
          }

          self.default = self.class.new(default) unless default.is_a?(self.class)
          default[*keys.dup] = value
        end
      else
        super(*args)
      end
    end
    alias_method :[]=, :store

    def freeze
      @fuzz.clear
      @fuzz = nil
      super
    end

    undef :index, :invert

    def height
      containers_with_default.max { |a, b| a.length <=> b.length }.length
    end

    def average_height
      lengths = containers_with_default.map { |e| e.length }
      lengths.inject(0) { |sum, len| sum += len }.to_f / lengths.size
    end

    protected
      def update_container(key) #:nodoc:
        super do |container|
          if container.is_a?(self.class)
            container.each_container_with_default do |c|
              c.delete_if do |value|
                (requirement = @fuzz[value]) && key !~ requirement
              end
            end
          else
            container.delete_if do |value|
              (requirement = @fuzz[value]) && key !~ requirement
            end
          end
          yield container
        end
      end
  end
end
