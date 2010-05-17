require File.dirname(__FILE__) + '/spec_helper'
include Merb::AssetsMixin

describe "Accessing Assets" do
  it "should create link to name with absolute url" do
    link_to("The Merb home page", "http://www.merbivore.com/").should ==
      "<a href=\"http://www.merbivore.com/\">The Merb home page</a>"
  end

  it "should create link to name with relative url" do
    link_to("The Entry Title", "/blog/show/13").should ==
      "<a href=\"/blog/show/13\">The Entry Title</a>"
  end

  it "should create link with attributes" do
    link_to("The Ruby home page", "http://www.ruby-lang.org", {'class' => 'special', 'target' => 'blank'}).should match(%r{class="special"})
    link_to("The Ruby home page", "http://www.ruby-lang.org", {'class' => 'special', 'target' => 'blank'}).should match(%r{target="blank"})
  end

  it "should create link with explicit href" do
    link_to("The Foo home page", "http://not.foo.example.com/", :href => "http://foo.example.com").should ==
      "<a href=\"http://foo.example.com\">The Foo home page</a>"
  end

  it "should create image tag with absolute url" do
    image_tag('http://example.com/foo.gif').should ==
      "<img src=\"http://example.com/foo.gif\" />"
  end

  it "should create image tag with relative url" do
    image_tag('foo.gif').should ==
      "<img src=\"/images/foo.gif\" />"
  end

  it "should create image tag with class" do
    result = image_tag('foo.gif', :class => 'bar')
    result.should match(%r{<img .*? />})
    result.should match(%r{src="/images/foo.gif"})
    result.should match(/class="bar"/)
  end

  it "should create image tag with specified path" do
    image_tag('foo.gif', :path => '/files/').should ==
      "<img src=\"/files/foo.gif\" />"
  end

  it "should create image tag without extension" do
    image_tag('/dynamic/charts').should ==
      "<img src=\"/dynamic/charts\" />"
  end

  it "should create image tag without extension and with specified path" do
    image_tag('charts', :path => '/dynamic/').should ==
      "<img src=\"/dynamic/charts\" />"
  end
  
  it "should create image tag with a random query string" do
    result = image_tag('foo.gif', :reload => true)
    result.should match(%r{<img src="/images/foo.gif\?\d+" />})
  end

end

describe "JavaScript related functions" do
  it "should escape js having quotes" do
    escape_js("'Lorem ipsum!' -- Some guy").should ==
      "\\'Lorem ipsum!\\' -- Some guy"
  end

  it "should escape js having new lines" do
    escape_js("Please keep text\nlines as skinny\nas possible.").should ==
      "Please keep text\\nlines as skinny\\nas possible."
  end

  it "should convert objects that respond to to_json to json" do
    js({'user' => 'Lewis', 'page' => 'home'}).should ==
      "{\"user\":\"Lewis\",\"page\":\"home\"}"
  end

  it "should convert objects using inspect that don't respond to_json to json" do
    js([ 1, 2, {"a"=>3.141}, false, true, nil, 4..10 ]).should ==
      "[1,2,{\"a\":3.141},false,true,null,\"4..10\"]"
  end
end

describe "External JavaScript and Stylesheets" do
  it "should require a js file only once" do
    require_js :jquery
    require_js :jquery, :effects
    include_required_js.scan(%r{src="/javascripts/jquery.js"}).should have(1).things
    include_required_js.scan(%r{src="/javascripts/effects.js"}).should have(1).things
  end

  it "should require a css file only once" do
    require_css :style
    require_css :style, 'ie-specific'

    include_required_css.scan(%r{href="/stylesheets/style.css"}).should have(1).things
    include_required_css.scan(%r{href="/stylesheets/ie-specific.css"}).should have(1).things
  end

  it "should require included js" do
    require_js 'jquery', 'effects', 'validation'
    result = include_required_js
    result.scan(/<script/).should have(3).things
    result.should match(%r{src="/javascripts/jquery.js"})
    result.should match(%r{src="/javascripts/effects.js"})
    result.should match(%r{src="/javascripts/validation.js"})
  end

  it "should require included css" do
    require_css 'style', 'ie-specific'
    result = include_required_css
    result.scan(/<link/).should have(2).things
    result.should match(%r{href="/stylesheets/style.css"})
    result.should match(%r{href="/stylesheets/ie-specific.css"})
  end

  it "should require included js from an absolute path" do
    require_js '/other/scripts.js', '/other/utils'
    result = include_required_js
    result.scan(/<script/).should have(2).things
    result.should match(%r{src="/other/scripts.js"})
    result.should match(%r{src="/other/utils.js"})
  end

  it "should require included css from an absolute path" do
    require_css '/styles/theme.css', '/styles/fonts'
    result = include_required_css
    result.scan(/<link/).should have(2).things
    result.should match(%r{href="/styles/theme.css"})
    result.should match(%r{href="/styles/fonts.css"})
  end
  
  it "should accept options for required javascript files" do
    require_js :jquery, :effects, :bundle => 'basics'
    require_js :jquery, :effects, :other
    required_js.should == [[:jquery, :effects, {:bundle=>"basics"}], [:other, {}]]
  end
  
  it "should accept options for required css files" do
    require_css :reset, :fonts, :bundle => 'basics'
    require_css :reset, :fonts, :layout
    required_css.should == [[:reset, :fonts, {:bundle=>"basics"}], [:layout, {}]]
  end

  it "should create a js include tag with the extension specified" do
    js_include_tag('jquery.js').should ==
      "<script type=\"text/javascript\" src=\"/javascripts/jquery.js\"></script>"
  end

  it "should create a js include tag and and the extension" do
    js_include_tag('jquery').should ==
      "<script type=\"text/javascript\" src=\"/javascripts/jquery.js\"></script>"
  end

  it "should create a js include tag for multiple includes" do
    result = js_include_tag('jquery.js', :effects)
    result.scan(/<script/).should have(2).things
    result.should match(%r{/javascripts/jquery.js})
    result.should match(%r{/javascripts/effects.js})
  end

  it "should create a css include tag with the extension specified" do
    result = css_include_tag('style.css')
    result.should match(%r{<link (.*?) />})
    result.should match(/charset="utf-8"/)
    result.should match(%r{type="text/css"})
    result.should match(%r{href="/stylesheets/style.css"})
    result.should match(%r{rel="Stylesheet"})
    result.should match(%r{media="all"})
  end

  it "should create a css include tag and add the extension" do
    result = css_include_tag('style')
    result.should match(%r{<link (.*?) />})
    result.should match(/charset="utf-8"/)
    result.should match(%r{type="text/css"})
    result.should match(%r{href="/stylesheets/style.css"})
    result.should match(%r{rel="Stylesheet"})
    result.should match(%r{media="all"})
  end

  it "should create a css include tag for multiple includes" do
    result = css_include_tag('style.css', :layout)
    result.scan(/<link/).should have(2).things
    result.should match(%r{/stylesheets/style.css})
    result.should match(%r{/stylesheets/layout.css})
  end
   
  it "should create a js include tag with a random query string" do
    Merb::Config[:reload_templates] = true
    result = js_include_tag('jquery.js')
    result.should match(%r{/javascripts/jquery.js\?\d+})
    Merb::Config[:reload_templates] = false
  end

  it "should create a css include tag with a random query string" do
    result = css_include_tag('style.css', :reload => true)
    result.should match(%r{/stylesheets/style.css\?\d+})
  end
  
  it "should create a css include tag with the specified media" do
    css_include_tag('style', :media => :print).should match(%r{media="print"})
  end

  it "should create a css include tag with the specified charset" do
    css_include_tag('style', :charset => 'iso-8859-1').should match(%r{charset="iso-8859-1"})
  end

  it "should return a uniq path for a single asset" do
    uniq_path("/javascripts/my.js").should ==
      "http://assets2.my-awesome-domain.com/javascripts/my.js"
  end

  it "should return a uniq path for multiple assets" do
    uniq_path("/javascripts/my.js","/javascripts/my.css").should ==
      ["http://assets2.my-awesome-domain.com/javascripts/my.js", "http://assets2.my-awesome-domain.com/javascripts/my.css"]
  end

  it "should return a uniq path for multiple assets passed as a single array" do
    uniq_path(["/javascripts/my.js","/javascripts/my.css"]).should ==
      ["http://assets2.my-awesome-domain.com/javascripts/my.js", "http://assets2.my-awesome-domain.com/javascripts/my.css"]
  end

  it "should return a uniq js path for a single js file" do
    uniq_js_path("my").should ==
      "http://assets2.my-awesome-domain.com/javascripts/my.js"
  end

  it "should return a uniq js path for multiple js files" do
    uniq_js_path(["admin/secrets","home/signup"]).should ==
      ["http://assets1.my-awesome-domain.com/javascripts/admin/secrets.js", "http://assets2.my-awesome-domain.com/javascripts/home/signup.js"]
  end

  it "should return a uniq css path for a single css file" do
    uniq_css_path("my").should ==
      "http://assets1.my-awesome-domain.com/stylesheets/my.css"
  end

  it "should return a uniq css path for multiple css files" do
    uniq_css_path(["admin/secrets","home/signup"]).should ==
      ["http://assets4.my-awesome-domain.com/stylesheets/admin/secrets.css", "http://assets1.my-awesome-domain.com/stylesheets/home/signup.css"]
  end

  it "should create a uniq js tag for a single js file" do
    uniq_js_tag("my").should ==
      "<script type=\"text/javascript\" src=\"http://assets2.my-awesome-domain.com/javascripts/my.js\"></script>"
  end

  it "should create a uniq js tag for each js file specified" do
    result = uniq_js_tag("jquery.js", :effects)
    result.scan(/<script/).should have(2).things
    result.should match(%r{/javascripts/jquery.js})
    result.should match(%r{/javascripts/effects.js})
  end

  it "should create a uniq css tag for a single css file" do
    result = uniq_css_tag("my")
    result.should match(%r{<link (.*?) />})
    result.should match(/charset="utf-8"/)
    result.should match(%r{type="text/css"})
    result.should match(%r{http://assets1.my-awesome-domain.com/stylesheets/my.css})
    result.should match(%r{rel="Stylesheet"})
    result.should match(%r{media="all"})
  end

  it "should create a uniq css tag for each css file specified" do
    result = uniq_css_tag("style.css", :layout)
    result.scan(/<link/).should have(2).things
    result.should match(%r{/stylesheets/style.css})
    result.should match(%r{/stylesheets/layout.css})
  end
end
