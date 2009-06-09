module Merb
  module Generators

    class AppGenerator < NamedGenerator

      def initialize(*args)
        Merb.disable(:initfile)
        super
      end

    end
  end
end
