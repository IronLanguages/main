module Content
  class Dialect
    include DataMapper::Resource

    property :id,   Serial
    property :name, String
    property :code, String
  end

  class Locale
    include DataMapper::Resource

    property :id,   Serial
    property :name, String
  end
end
