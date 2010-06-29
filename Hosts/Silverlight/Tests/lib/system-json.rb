l = lambda { require 'System.Json' }
begin
  l.call
rescue LoadError
  l.call
end

module System::Json  
  def self.parse(str)
    JsonValue.parse(str)
  end
  
  class JsonValue
    def method_missing(index)
      item = self.item(index.to_s)
      super if item.nil?
      case item.json_type
        when JsonType.string:  item.to_string.to_s.split("\"").last
        when JsonType.number:  item.to_string.to_s.to_f
        when JsonType.boolean: System::Boolean.parse(item)
        else item
      end
    end

    def inspect
      to_string.to_s
    end
  end
end

class Object
  def to_json_object
    System::Json::JsonPrimitive.new self.to_s.to_clr_string
  end
  
  def to_json
    to_json_object.to_string
  end
end

class Hash
  def to_json_object
    self.inject(System::Json::JsonObject.new) do |json, (k, v)|
      json[k.to_s] = v.to_json_object
      json
    end
  end
end

class Numeric
  def to_json_object
    System::Json::JsonPrimitive.new self.to_f
  end
end

class String
  def to_json_object
    System::Json::JsonPrimitive.new self.to_s.to_clr_string
  end
end

class Array
  def to_json_object
    self.inject(System::Json::JsonArray.new) do |json, i|
      json.add i.to_json_object
      json
    end
  end
end

class TrueClass
  def to_json_object
    System::Json::JsonPrimitive.new true
  end
end

class FalseClass
  def to_json_object
    System::Json::JsonPrimitive.new false
  end
end