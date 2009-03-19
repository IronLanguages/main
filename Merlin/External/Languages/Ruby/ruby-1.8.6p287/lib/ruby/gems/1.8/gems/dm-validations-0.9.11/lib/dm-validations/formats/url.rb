module DataMapper
  module Validate
    module Format
      module Url

        def self.included(base)
          DataMapper::Validate::FormatValidator::FORMATS.merge!(
            :url => [ Url, lambda { |field, value| '%s is not a valid URL'.t(value) }]
          )
        end

        Url = begin
          # Regex from http://www.igvita.com/2006/09/07/validating-url-in-ruby-on-rails/
          /(^$)|(^(http|https):\/\/[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(([0-9]{1,5})?\/.*)?$)/ix
        end

      end # module Url
    end # module Format
  end # module Validate
end # module DataMapper
