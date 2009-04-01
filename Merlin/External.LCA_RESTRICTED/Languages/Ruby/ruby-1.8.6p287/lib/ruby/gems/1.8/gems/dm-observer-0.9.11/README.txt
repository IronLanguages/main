== README

DataMapper::Observer allows you to add callback hooks to many models. This is
similar to observers in ActiveRecord.

Example:

class Adam
  include DataMapper::Resource

  property :id, Integer, :serial => true
  property :name, String
end

class AdamObserver
  include DataMapper::Observer

  observe Adam

  before :save do
    # log message
  end

  before :get_drunk do
    # eat something
  end

  after_class_method :unite do
    raise "Call for help!"
  end

end
