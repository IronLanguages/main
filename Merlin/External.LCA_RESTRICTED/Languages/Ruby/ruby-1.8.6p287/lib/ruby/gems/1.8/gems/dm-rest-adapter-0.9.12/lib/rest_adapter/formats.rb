module DataMapperRest
  # Absolutely simple format class, extend later if needed
  class Format
    attr_accessor :extension, :mime

    def initialize(type)
      @extension = type
      @mime      = "application/#{type}"
    end

    def header
      {'Content-Type' => @mime}
    end

  end
end
