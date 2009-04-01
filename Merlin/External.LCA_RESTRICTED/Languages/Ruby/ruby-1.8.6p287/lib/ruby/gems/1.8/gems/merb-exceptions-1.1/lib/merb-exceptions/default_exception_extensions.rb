module MerbExceptions
  module DefaultExceptionExtensions
    def self.included(mod)
      mod.class_eval do
        after :notify_of_exceptions
      end
    end
  end
end