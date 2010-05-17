require 'time'
require 'bigdecimal'

module DBI
    #
    # Interface to convert SQL result sets to native Ruby types.
    #
    # Type is used to convert result sets, which differ from bound variables
    # (which generally go in the opposite direction). For those, see
    # DBI::TypeUtil#convert and DBI::TypeUtil#register_conversion.
    #
    # Type objects have a simple interface: they implement a +parse+ method
    # which takes the result from the DBD and attempts to convert it to the
    # native type. In the event that they do not do this successfully, they are
    # expected to return the object in its original form.
    #
    # As a result, many of the built-in Type classes fallback to simpler forms:
    # Float falls back to Integer, Integer to Varchar, etc. It's questionable
    # at this point if it's desirable to do this, but testing has so far proven
    # it a non-issue.
    #
    # To reiterate, it is *never acceptable* to return +nil+ or some other
    # placeholder when an object will not successfully parse. Return the object
    # handed to you.
    #
    # Types must also handle +nil+ as a result to parse. In this case, the
    # advisable solution is to just let the +nil+ pass through, as it's usually
    # indicative of a SQL NULL result.
    #
    # DBI::Row handles delegation of these objects as a converter for the
    # results. Typically, the type object is a class inferred from
    # DBI::TypeUtil#type_name_to_module ran against the ColumnInfo field
    # +type_name+. However, the the +dbi_type+ field can be used in its place
    # to directly associate a Type object with the column in the DBD, and
    # end-users can leverage StatementHandle#bind_coltype to manually tweak
    # this transformation.
    #
    # As stated before, Type objects are objects. These objects may be Modules
    # or Classes (and typically are), but there is no reason a traditional
    # constructed object with a +parse+ method cannot be returned; in fact, it
    # is used in a few spots to emulate complex types such as PostgreSQL
    # arrays. Look into the +dbi_type+ ColumnInfo field to pass these types
    # around.
    #
    module Type
        #
        # Represents a SQL NULL.
        #
        class Null
            def self.parse(obj)
                return nil if obj.to_s.match(/^null$/i)
                return obj
            end
        end

        #
        # Represents a SQL char or varchar. General fallback class.
        #
        class Varchar 
            def self.parse(obj)
                return obj unless obj
                return obj.to_s if obj.respond_to? :to_s
                return obj.to_str if obj.respond_to? :to_str
                return obj
            end
        end

        #
        # Represents a whole number. Falls back to Varchar.
        #
        class Integer < Varchar
            def self.parse(obj)
                return nil if Null.parse(obj).nil?
                return obj.to_i if obj.respond_to? :to_i
                super 
            end
        end

        #
        # Represents a decimal number with floating-point precision. Falls back
        # to Integer.
        #
        class Float < Integer
            def self.parse(obj)
                return nil if Null.parse(obj).nil?
                return obj.to_f if obj.respond_to? :to_f
                super
            end
        end

        #
        # Represents a Decimal with real precision (BigDecimal). Falls back to
        # Float.
        #
        class Decimal < Float
            def self.parse(obj)
                BigDecimal.new(obj) rescue super
            end
        end

        #
        # Represents a SQL TIMESTAMP and returns DateTime. Falls back to Null.
        #
        class Timestamp < Null
            def self.parse(obj)
                obj = super
                return obj unless obj
                case obj.class
                when ::DateTime
                    return obj
                when ::Date
                    return ::DateTime.parse(obj.to_s)
                when ::Time
                    return ::DateTime.parse(obj.to_s)
                else
                    return ::DateTime.parse(obj.to_s)   if obj.respond_to? :to_s
                    return ::DateTime.parse(obj.to_str) if obj.respond_to? :to_str
                    return obj
                end
            end
        end

        #
        # Represents a SQL BOOLEAN. Returns true/false. Falls back to Null.
        #
        class Boolean < Null
            def self.parse(obj)
                obj = super

                return nil if obj.nil?

                if obj == false or obj.kind_of? FalseClass
                    return false
                elsif obj.kind_of? TrueClass
                    return true
                else
                    case obj
                    when 't'
                        return true
                    when 'f'
                        return false
                    end

                    if obj.respond_to? :to_i
                        if obj.to_i == 0
                            return false
                        else
                            return true
                        end
                    else
                        # punt
                        return nil
                    end
                end
            end
        end
    end
end
