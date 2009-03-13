module Image
  include DataMapper::Resource

  is :remixable

  property :id,           Integer, :key => true, :serial => true
  property :description,  String
  property :path,         String

  # These methods will be available to the class remixing this module
  #   If 'User' remixes 'Images', these methods will be available to a User class
  #
  module RemixerClassMethods
    def test_remixer_class_method
      'CLASS METHOD FOR REMIXER'
    end
  end

  # These methods will be available to instantiated objects of the remixing this module
  #   If 'User' remixes 'Images', these methods will be available to a User object
  #
  module RemixerInstanceMethods
    def test_remixer_instance_method
      'INSTANCE METHOD FOR REMIXER'
    end
  end

  # These methods will be available to the Generated Remixed Class
  #   If 'User' remixes 'Images', these methods will be available to UserImage class
  #
  module RemixeeClassMethods
    def test_remixee_class_method
      'CLASS METHOD FOR REMIXEE'
    end
  end

  # These methods will be available to an instantiated Generated Remixed Class
  #   If 'User' remixes 'Images', these methods will be available to a UserImage object
  #
  module RemixeeInstanceMethods
    def test_remixee_instance_method
      'INSTANCE METHOD FOR REMIXEE'
    end
  end
end
