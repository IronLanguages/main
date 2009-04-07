module My
  module Nested
    module Remixable

      module Rating

        def self.included(base)
          base.extend ClassMethods
        end

        include DataMapper::Resource

        is :remixable

        # properties

        property :id,      Integer, :serial => true

        property :user_id, Integer, :nullable => false
        property :rating,  Integer, :nullable => false, :default => 0

        module ClassMethods

          # total rating for all rateable instances of this type
          def total_rating
            rating_sum = self.sum(:rating).to_f
            rating_count = self.count.to_f
            rating_count > 0 ? rating_sum / rating_count : 0
          end

        end

      end

    end
  end
end
