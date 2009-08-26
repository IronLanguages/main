class FakeModel
  
  attr_accessor :vin, :make, :model
  
  def self.columns
    [FakeColumn.new(:foo, :string), 
     FakeColumn.new(:foobad, :string),
     FakeColumn.new(:desc, :string),
     FakeColumn.new(:bar, :integer), 
     FakeColumn.new(:barbad, :integer),      
     FakeColumn.new(:baz, :boolean),
     FakeColumn.new(:bazbad, :boolean),
     FakeColumn.new(:bat, :boolean),
     FakeColumn.new(:batbad, :boolean)
     ]     
  end
  
  def valid?
    false
  end
  
  def new_record?
    false
  end
  
  def errors
    FakeErrors.new(self)
  end
  
  def foo
    "foowee"
  end
  alias_method :foobad, :foo
  
  def bar
    7
  end
  alias_method :barbad, :bar
  
  def baz
    true
  end
  alias_method :bazbad, :baz
  
  def bat
    false
  end
  alias_method :batbad, :bat
  
  def nothing
    nil
  end
  
  def to_s
    'fake_model'
  end
end
