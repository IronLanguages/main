class FakeDMModel
  def self.properties
    [FakeColumn.new(:baz, TrueClass),
     FakeColumn.new(:bat, TrueClass)
    ]
  end
  
  def new_record?
    false
  end
  
  def errors
    FakeErrors.new(self)
  end
  
  def baz?
    true
  end
  alias baz baz?
  
  def bat?
    false
  end
  alias bat bat?
end
