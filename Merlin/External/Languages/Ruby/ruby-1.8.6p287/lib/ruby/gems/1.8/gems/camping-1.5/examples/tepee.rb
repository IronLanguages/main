#!/usr/bin/ruby
$:.unshift File.dirname(__FILE__) + "/../../lib"
%w(rubygems redcloth camping acts_as_versioned).each { |lib| require lib }

Camping.goes :Tepee

module Tepee::Models

  class Page < Base
    PAGE_LINK = /\[\[([^\]|]*)[|]?([^\]]*)\]\]/
    validates_uniqueness_of :title
    before_save { |r| r.title = r.title.underscore }
    acts_as_versioned
  end

  class CreateTepee < V 1.0
    def self.up
      create_table :tepee_pages, :force => true do |t|
        t.column :title, :string, :limit => 255
        t.column :body, :text
      end
      Page.create_versioned_table
      Page.reset_column_information
    end
    def self.down
      drop_table :tepee_pages
      Page.drop_versioned_table
    end
  end

end

module Tepee::Controllers
  class Index < R '/'
    def get
      redirect Show, 'home'
    end
  end

  class Show < R '/(\w+)', '/(\w+)/(\d+)'
    def get page_name, version = nil
      redirect(Edit, page_name, 1) and return unless @page = Page.find_by_title(page_name)
      @version = (version.nil? or version == @page.version.to_s) ? @page : @page.versions.find_by_version(version)
      render :show
    end
  end

  class Edit < R '/(\w+)/edit', '/(\w+)/(\d+)/edit' 
    def get page_name, version = nil
      @page = Page.find_or_create_by_title(page_name)
      @page = @page.versions.find_by_version(version) unless version.nil? or version == @page.version.to_s
      render :edit
    end
    
    def post page_name
      Page.find_or_create_by_title(page_name).update_attributes :body => input.post_body and redirect Show, page_name
    end
  end

  class Versions < R '/(\w+)/versions'
    def get page_name
      @page = Page.find_or_create_by_title(page_name)
      @versions = @page.versions
      render :versions
    end
  end

  class List < R '/all/list'
    def get
      @pages = Page.find :all, :order => 'title'
      render :list
    end
  end

  class Stylesheet < R '/css/tepee.css'
    def get
@headers['Content-Type'] = 'text/css'
File.read(__FILE__).gsub(/.*__END__/m, '')
    end
  end
end

module Tepee::Views
  def layout
    html do
      head do
        title 'test'
        link :href=>R(Stylesheet), :rel=>'stylesheet', :type=>'text/css' 
      end
      style <<-END, :type => 'text/css'
        body {
          font-family: verdana, arial, sans-serif;
        }
        h1, h2, h3, h4, h5 {
          font-weight: normal;
        }
        p.actions a {
          margin-right: 6px;
        }
      END
      body do
        p do
          small do
            span "welcome to " ; a 'tepee', :href => "http://code.whytheluckystiff.net/svn/camping/trunk/examples/tepee.rb"
            span '. go ' ;       a 'home',  :href => R(Show, 'home')
            span '. list all ' ; a 'pages', :href => R(List)
          end
        end
        div.content do
          self << yield
        end
      end
    end
  end

  def show
    h1 @page.title
    div { _markup @version.body }
    p.actions do 
      _button 'edit',      :href => R(Edit, @version.title, @version.version) 
      _button 'back',      :href => R(Show, @version.title, @version.version-1) unless @version.version == 1 
      _button 'next',      :href => R(Show, @version.title, @version.version+1) unless @version.version == @page.version 
      _button 'current',   :href => R(Show, @version.title)                     unless @version.version == @page.version 
      _button 'versions',  :href => R(Versions, @page.title) 
    end
  end

  def edit
    h1 @page.title 
    form :method => 'post', :action => R(Edit, @page.title) do
      p do
        textarea @page.body, :name => 'post_body', :rows => 50, :cols => 100
      end
      input :type => 'submit', :value=>'change' 
    end
    _button 'cancel', :href => R(Show, @page.title, @page.version) 
    a 'syntax', :href => 'http://hobix.com/textile/', :target=>'_blank' 
  end

  def list
    h1 'all pages'
    ul { @pages.each { |p| li { a p.title, :href => R(Show, p.title) } } }
  end

  def versions
    h1 @page.title
    ul do
      @versions.each do |page|
        li do
          span page.version
          _button 'show',   :href => R(Show, page.title, page.version)
          _button 'edit',   :href => R(Edit, page.title, page.version)
        end
      end
    end
  end

  def _button(text, options={})
    form :method=>:get, :action=>options[:href] do
      input :type=>'submit', :name=>'submit', :value=>text
    end
  end

  def _markup body
    return '' if body.blank?
    body.gsub!(Tepee::Models::Page::PAGE_LINK) do
      page = title = $1
      title = $2 unless $2.empty?
      page = page.gsub /\W/, '_'
      if Tepee::Models::Page.find(:all, :select => 'title').collect { |p| p.title }.include?(page)
        %Q{<a href="#{self/R(Show, page)}">#{title}</a>}
      else
        %Q{<span>#{title}<a href="#{self/R(Edit, page, 1)}">?</a></span>}
      end
    end
    RedCloth.new(body, [ :hard_breaks ]).to_html
  end
end

def Tepee.create
  Tepee::Models.create_schema :assume => (Tepee::Models::Page.table_exists? ? 1.0 : 0.0)
end
__END__
/** focus **/
/*
a:hover:active {
  color: #10bae0;
}

a:not(:hover):active {
  color: #0000ff;
}

*:focus {
  -moz-outline: 2px solid #10bae0 !important;
  -moz-outline-offset: 1px !important;
  -moz-outline-radius: 3px !important;
}

button:focus,
input[type="reset"]:focus,
input[type="button"]:focus,
input[type="submit"]:focus,
input[type="file"] > input[type="button"]:focus {
  -moz-outline-radius: 5px !important;
}

button:focus::-moz-focus-inner {
  border-color: transparent !important;
}

button::-moz-focus-inner,
input[type="reset"]::-moz-focus-inner,
input[type="button"]::-moz-focus-inner,
input[type="submit"]::-moz-focus-inner,
input[type="file"] > input[type="button"]::-moz-focus-inner {
  border: 1px dotted transparent !important;
}
textarea:focus, button:focus, select:focus, input:focus {
  -moz-outline-offset: -1px !important;
}
input[type="radio"]:focus {
  -moz-outline-radius: 12px;
  -moz-outline-offset: 0px !important;
}
a:focus {
  -moz-outline-offset: 0px !important;
}
*/
form { display: inline; }

/** Gradient **/
small, pre, textarea, textfield, button, input, select {
   color: #4B4B4C !important;
   background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAIAAAAeCAMAAAAxfD/2AAAABGdBTUEAAK/INwWK6QAAABl0RVh0U29mdHdhcmUAQWRvYmUgSW1hZ2VSZWFkeXHJZTwAAAAtUExURfT09PLy8vHx8fv7+/j4+PX19fn5+fr6+vf39/z8/Pb29vPz8/39/f7+/v///0c8Y4oAAAA5SURBVHjaXMZJDgAgCMDAuouA/3+uHPRiMmlKzmhCFRorLOakVnpnDEpBBDHM8ODs/bz372+PAAMAXIQCfD6uIDsAAAAASUVORK5CYII=) !important;
   background-color: #FFF !important;
   background-repeat: repeat-x !important;
   border: 1px solid #CCC !important;
}

button, input { margin: 3px; }

