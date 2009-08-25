module Merb
  class Authentication
  
    class << self
    
      # Adds any customizations just before the after_app_loads boot loader
      # so that plugins can offer default customization.  This will still allow
      # for a user to overwrite any customizations if required in the after_app_loads
      # block
      # @plugin
      def customize_default(&block)
        default_customizations << block
        default_customizations
      end
    
      # Gets the list of declared customizations
      def default_customizations
        @custom_default_blocks ||= []
      end
    
    end
  
  end # Merb::Authentication
end # Merb