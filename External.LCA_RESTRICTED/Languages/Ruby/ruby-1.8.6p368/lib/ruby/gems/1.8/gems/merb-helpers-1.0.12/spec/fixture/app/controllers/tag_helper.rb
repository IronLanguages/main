class TagHelper < Merb::Controller
  def tag_with_content
    @content = "Astral Projection ~ Dancing Galaxy"
    
    render
  end

  def tag_with_content_in_the_block
    render
  end

  def nested_tags
    @content = "Astral Projection ~ In the Mix"
    
    render
  end

  def tag_with_attributes
    render
  end
end
