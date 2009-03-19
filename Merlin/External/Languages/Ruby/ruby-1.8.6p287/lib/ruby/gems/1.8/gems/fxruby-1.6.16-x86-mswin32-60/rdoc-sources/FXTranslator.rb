module Fox
  #
  # A translator translates a message to another language.
  #
  class FXTranslator
    
    # The application associated with this translator [FXApp]
    attr_reader :app
    
    #
    # Return a new translator for the application _a_ (an FXApp instance).
    #
    def initialize(a); end
    
    #
    # Translate a message.
    #
    def tr(context, message, hint=nil); end
    
    #
    # Change the text codec used to decode the messages embedded in the
    # source.
    #
    def textCodec=(codec); end
    
    #
    # Return a reference to the text codec.
    #
    def textCodec; end
  end
end

