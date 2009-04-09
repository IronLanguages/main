gem 'dm-core', '~>0.9.11'
require 'dm-core'

module DataMapper
  module Resource
    class << self
      alias_method :_dm_shorthand_included, :included

      def included(target)
        _dm_shorthand_included(target)

        parentname, basename = target.name.split /::(?=\w+$)/

        if basename
          parent = parentname.split(/::/).inject(Kernel) { |mod, str| mod.const_get(str) }
        else
          basename = parentname
          parent = Kernel
        end

        eval(<<-EOS, binding, __FILE__, __LINE__)
          class << parent
            def #{basename}(repository_name)
              class_cache[repository_name] ||= begin
                klass = Class.new(#{target.name})
                klass.instance_eval do
                  (class << self; self; end).send(:define_method, :_repository_name) do
                    repository_name
                  end

                  private

                  def default_repository_name
                    @repository_name ||= _repository_name
                  end
                end

                klass
              end
            end

            private

            def class_cache
              @class_cache ||= {}
            end
          end
        EOS
      end
    end
  end
end
