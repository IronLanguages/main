module Rack::Mount
  module Recognition
    module CodeGeneration #:nodoc:
      def _expired_recognize(env) #:nodoc:
        raise 'route set not finalized'
      end

      def rehash
        super
        optimize_recognize!
      end

      private
        def expire!
          remove_metaclass_method :recognize

          class << self
            alias_method :recognize, :_expired_recognize
          end

          super
        end

        def optimize_container_iterator(container)
          body = []

          container.each_with_index { |route, i|
            body << "route = self[#{i}]"
            body << 'params = route.defaults.dup'

            conditions = []
            route.conditions.each do |method, condition|
              b = []
              b << "if m = obj.#{method}.match(#{condition.inspect})"
              b << 'matches = m.captures' if route.named_captures[method].any?
              b << 'p = nil' if route.named_captures[method].any?
              b << route.named_captures[method].map { |k, j| "params[#{k.inspect}] = p if p = matches[#{j}]" }.join('; ')
              b << 'true'
              b << 'end'
              conditions << "(#{b.join('; ')})"
            end

            body << <<-RUBY
              if #{conditions.join(' && ')}
                yield route, params
              end
            RUBY
          }

          container.instance_eval(<<-RUBY, __FILE__, __LINE__)
            def optimized_each(obj)
              #{body.join("\n")}
              nil
            end
          RUBY
        end

        def optimize_recognize!
          keys = @recognition_keys.map { |key|
            if key.is_a?(Array)
              key.call_source(:cache, :obj)
            else
              "obj.#{key}"
            end
          }.join(', ')

          remove_metaclass_method :recognize

          instance_eval(<<-RUBY, __FILE__, __LINE__)
            def recognize(obj, &block)
              cache = {}
              container = @recognition_graph[#{keys}]
              optimize_container_iterator(container) unless container.respond_to?(:optimized_each)

              if block_given?
                container.optimized_each(obj) do |route, params|
                  yield route, params
                end
              else
                container.optimized_each(obj) do |route, params|
                  return route, params
                end
              end

              nil
            end
          RUBY
        end

        # method_defined? can't distinguish between instance
        # and meta methods. So we have to rescue if the method
        # has not been defined in the metaclass yet.
        def remove_metaclass_method(symbol)
          metaclass = class << self; self; end
          metaclass.send(:remove_method, symbol)
        rescue NameError => e
          nil
        end
    end
  end
end
