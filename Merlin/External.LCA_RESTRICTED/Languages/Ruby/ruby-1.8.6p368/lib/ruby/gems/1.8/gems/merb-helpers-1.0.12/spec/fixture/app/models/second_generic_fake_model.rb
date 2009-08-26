class FakeModel2 < FakeModel
  def id
    1
  end
  
  def foo
    "foowee2"
  end
  alias_method :foobad, :foo
  
  def bar
    "barbar"
  end
  
  def new_record?
    true
  end
end
