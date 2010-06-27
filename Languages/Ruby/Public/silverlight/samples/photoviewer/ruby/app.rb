require "silverlight"
require 'system-json'
require 'render'

class App < SilverlightApplication
  def initialize
    @url = "http://api.flickr.com/services/rest"
    @options = {
      :method => "flickr.photos.search",
      :format => "json",
      :nojsoncallback => "1",
      :api_key => "6dba7971b2abf352b9dcd48a2e5a5921",
      :sort => "relevance",
      :per_page => "30"
    }
    document.submit_search.onclick do |s, e|
      create(document.keyword.value, 1)
    end
  end

  def create(keyword, page)
    @options[:tags] = keyword
    @options[:page] = page
    request
  end

  def request
    make_url
    request = Net::WebClient.new
    request.download_string_completed do |sender, args|
      @response = args.result
      show
    end
    document.images_loading.style[:display] = "inline"
    request.download_string_async Uri.new(@url)
  end

  def make_url
    first, separator = true, '?'
    @options.each do |key, value|
      separator = "&" unless first
      @url += "#{separator}#{key}=#{value}"
      first = false
    end
  end

  def show
    @flickr = System::Json::JsonValue.parse(@response)
    load 'system-json.rb'
    render
    if document.overlay && document.lightbox
      document.overlay.parent.remove_child document.overlay
      document.lightbox.parent.remove_child document.lightbox
    end
    HtmlPage.window.eval("initLightbox()")
  end

  def render
    @render = Render.new(@flickr, @options[:tags], @options[:page])
    document.search_images[:innerHTML] = @render.generate_photos
    document.search_links[:innerHTML] = @render.generate_pages
    @render.hook_page_events('search_links')
    document.images_loading.style[:display] = "none"
    document.search_results.style[:display] = "block"
  end

end

$app = App.new
