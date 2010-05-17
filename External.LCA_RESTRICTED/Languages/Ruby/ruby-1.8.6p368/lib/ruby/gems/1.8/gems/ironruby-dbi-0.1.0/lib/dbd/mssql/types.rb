module DBI::DBD::MSSQL::Type

    #
    # Represents a Decimal with real precision (BigDecimal). Falls back to
    # Float.
    #
    class Decimal < DBI::Type::Float
        def self.parse(obj)
            BigDecimal.new(obj.to_s) rescue super
        end
    end

    #
    # Represents a SQL NULL.
    #
    class Null
        def self.parse(obj)
            return nil if obj.is_a?(System::DBNull) or obj.to_s.match(/^null$/i)
            return obj
        end
    end

    #
    # Custom handling for TIMESTAMP and DATETIME types in MySQL. See DBI::Type
    # for more information.
    #
    class Timestamp < DBI::Type::Null
        def self.parse(obj)
            obj = super
            return obj unless obj

            case obj.class
            when ::DateTime
                return obj
            when ::String
                return ::DateTime.strptime(obj, "%Y-%m-%d %H:%M:%S")
            else
                return ::DateTime.parse(obj.to_s)   if obj.respond_to? :to_s
                return ::DateTime.parse(obj.to_str) if obj.respond_to? :to_str
                return obj
            end
        end
    end

    #
    # Custom handling for DATE types in MySQL. See DBI::Type for more
    # information.
    #
    class Date < DBI::Type::Null
        def self.parse(obj)
            obj = super
            return obj unless obj

            case obj.class
            when ::Date
                return obj
            when ::String
                return ::Date.strptime(obj, "%Y-%m-%d")
            else
                return ::Date.parse(obj.to_s)   if obj.respond_to? :to_s
                return ::Date.parse(obj.to_str) if obj.respond_to? :to_str
                return obj
            end
        end
    end
end
