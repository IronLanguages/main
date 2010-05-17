require 'system-json.rb'

class Render
  def initialize(flickr, tags, current_page)
    @flickr = flickr
    @tags = tags
    @current_page = current_page
  end

  def generate_photos
    if @flickr.stat == "ok" && @flickr.photos.total.to_i > 0 
      tag(:div, :class => 'images') do
        @flickr.photos.photo.collect{ |p| photo(p) }.join
      end
    else
      "No images found!"
    end
  end

  def photo(p)
    source = "http://farm#{p.farm.to_i}.static.flickr.com/#{p.server}/#{p.photo_id}_#{p.secret}"
    thumb = "#{source}_s.jpg"
    img = "#{source}.jpg"
    tag(:div, :class => 'image') do
      tag(:a, { 
        :href  => "#{img}", 
        :title => "&lt;a href=&quot;http://www.flickr.com/photos/#{p.owner}/#{p.photo_id}&quot; target=&quot;_blank&quot;&gt;#{p.title}&lt;/a&gt;",
        :rel   => "lightbox[#{@tags}]"
      }) do
        tag(:img, :src => "#{thumb}")
      end
    end
  end

  def generate_pages
    render = ""
    if @flickr.photos.total.to_i > 0
      num_pages = @flickr.photos.pages > 10 ? 10 : @flickr.photos.pages.to_i
      num_pages.times { |i| render += page(i + 1) } if num_pages > 1
    end
    render
  end

  def page(i)
    tag(:a, :href => 'javascript:void(0)', :id => "#{i}") { "#{i}" }
  end

  def hook_page_events(div)
    $app.document.get_element_by_id(div.to_s.to_clr_string).children.each do |child|
      if child.id.to_s.to_i == @current_page
        child.css_class = "active" 
      else
        child.onclick { |s, args| $app.create(@tags, child.id.to_s.to_i) }
      end
    end
  end

private

  def tag(name, options, &block)
    output = ""
    output << "<#{name}"
    keyvalue = options.collect { |key, value| "#{key}=\"#{value}\"" }
    output << " #{keyvalue.join(" ")}" if keyvalue.size > 0
    if block 
      output << ">"
      output << yield 
      output << "</#{name}>"
    else
      output << " />"
    end
    output
  end

end
