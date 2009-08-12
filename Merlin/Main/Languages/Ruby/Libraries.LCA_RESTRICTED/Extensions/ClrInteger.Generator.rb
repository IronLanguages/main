require '../../Scripts/CodeGenerator.rb'

def range_check_fixnum(type)
  if type == "Int64"
    "true"
  elsif type == "UInt64"
    "value >= 0"
  elsif type == "UInt32"
    "value >= 0"
  elsif type == "UInt16"
    "value >= 0 && value <= $Self.MaxValue"
  else
    "value >= $Self.MinValue && value <= $Self.MaxValue"
  end  
end

generate(__FILE__) do
  template = DATA.read
  types = ["Byte", "SByte", "Int16", "UInt16", "UInt32", "Int64", "UInt64"]
  
  result = ""
  types.each do |type|
    result += template.dup.
      gsub!('$RangeCheckFixnum', range_check_fixnum(type)).
      gsub!('$Self', type)
      
    result += "\n"
  end
  
  result
end

__END__
public static partial class $SelfOps {
    [RubyMethod("size")]
    public static int Size($Self self) {
        return sizeof($Self);
    }

    [RubyConstructor]
    [RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
    public static $Self InducedFrom(RubyClass/*!*/ self, [DefaultProtocol]int value) {
        if ($RangeCheckFixnum) {
            return ($Self)value;
        }
        throw RubyExceptions.CreateRangeError(String.Format("Integer {0} out of range of {1}", value, self.Name));
    }
    
    [RubyConstructor]
    [RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
    public static $Self InducedFrom(RubyClass/*!*/ self, [NotNull]BigInteger/*!*/ value) {
        if (value >= $Self.MinValue && value <= $Self.MaxValue) {
            return ($Self)value;
        }
        throw RubyExceptions.CreateRangeError(String.Format("Integer {0} out of range of {1}", value, self.Name));
    }

    [RubyConstructor]
    [RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
    public static $Self InducedFrom(RubyClass/*!*/ self, double value) {
        if (value >= $Self.MinValue && value <= $Self.MaxValue) {
            return ($Self)value;
        }
        throw RubyExceptions.CreateRangeError(String.Format("Float {0} out of range of {1}", value, self.Name));
    }
    
    [RubyMethod("inspect")]
    public static MutableString/*!*/ Inspect(object/*!*/ self) {
        return MutableString.CreateMutable(RubyEncoding.Binary).Append(self.ToString()).Append(" ($Self)");
    }
}
