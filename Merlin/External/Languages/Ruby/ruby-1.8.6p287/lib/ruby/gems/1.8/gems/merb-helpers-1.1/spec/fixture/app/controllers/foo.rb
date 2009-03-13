class Foo < Application

  def index
    "index"
  end

  def bar
    render
  end

  def renders_tag
    render
  end

  def raise_conflict
    raise Conflict
  end
  
  def raise_not_acceptable
    raise NotAcceptable
  end
  
end  
