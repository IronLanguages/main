module Merb
  # The AssetsMixin module provides a number of helper methods to views for
  # linking to assets and other pages, dealing with JavaScript and CSS.
  module AssetsMixin
    include Merb::Assets::AssetHelpers
    # :section: Accessing Assets
    # Merb provides views with convenience methods for links images and other
    # assets.


    # ==== Parameters
    # none
    #
    # ==== Returns
    # html<String>
    #
    # ==== Examples
    #  We want all possible matches in the FileSys up to the action name
    #     Given:  controller_name = "namespace/controller"
    #             action_name     = "action"
    #  If all files are present should generate link/script tags for:
    #     namespace.(css|js)
    #     namespace/controller.(css|js)
    #     namespace/controller/action.(css|js)
    #
    def auto_link
      html    = ""
      prefix  = ""
      (controller_name / action_name).split("/").each do |path|
        path = prefix + path

        css_path  = path + ".css"
        if File.exists? Merb.root / "public" / "stylesheets" / css_path
          html << %{<link rel="stylesheet" type="text/css" href="/stylesheets/#{css_path}" /> }
        end

        js_path   = path + ".js"
        if File.exists? Merb.root / "public" / "javascripts" / js_path
          html << %{<script type="text/javascript" language="javascript" src="/javascripts/#{js_path}"></script>}
        end

        #Update the prefix for the next iteration
        prefix += path / ""
      end

      #Return the generated HTML
      html
    end


    # ==== Parameters
    # name<~to_s>:: The text of the link.
    # url<~to_s>:: The URL to link to. Defaults to an empty string.
    # opts<Hash>:: Additional HTML options for the link.
    #
    # ==== Examples
    #   link_to("The Merb home page", "http://www.merbivore.com/")
    #     # => <a href="http://www.merbivore.com/">The Merb home page</a>
    #
    #   link_to("The Ruby home page", "http://www.ruby-lang.org", {'class' => 'special', 'target' => 'blank'})
    #     # => <a href="http://www.ruby-lang.org" class="special" target="blank">The Ruby home page</a>
    #
    #   link_to p.title, "/blog/show/#{p.id}"
    #     # => <a href="/blog/show/13">The Entry Title</a>
    #
    def link_to(name, url='', opts={})
      opts[:href] ||= url
      %{<a #{ opts.to_xml_attributes }>#{name}</a>}
    end

    # ==== Parameters
    # img<~to_s>:: The image path.
    # opts<Hash>:: Additional options for the image tag (see below).
    #
    # ==== Options (opts)
    # :path<String>::
    #   Sets the path prefix for the image. Defaults to "/images/" or whatever
    #   is specified in Merb::Config. This is ignored if img is an absolute
    #   path or full URL.
    #
    # All other options set HTML attributes on the tag.
    #
    # ==== Examples
    #   image_tag('foo.gif')
    #   # => <img src='/images/foo.gif' />
    #
    #   image_tag('foo.gif', :class => 'bar')
    #   # => <img src='/images/foo.gif' class='bar' />
    #
    #   image_tag('foo.gif', :path => '/files/')
    #   # => <img src='/files/foo.gif' />
    #
    #   image_tag('http://test.com/foo.gif')
    #   # => <img src="http://test.com/foo.gif">
    #
    #   image_tag('charts', :path => '/dynamic/')
    #   or
    #   image_tag('/dynamic/charts')
    #   # => <img src="/dynamic/charts">
    def image_tag(img, opts={})
      if img[0].chr == '/'
        opts[:src] = img
      else
        opts[:path] ||=
          if img =~ %r{^https?://}
            ''
          else
            if Merb::Config[:path_prefix]
              Merb::Config[:path_prefix] + '/images/'
            else
              '/images/'
            end
          end
        opts[:src] ||= opts.delete(:path) + img
      end
      random = opts.delete(:reload) || Merb::Config[:reload_templates]
      opts[:src] += opts[:src].include?('?') ? "&#{random_query_string}" : "?#{random_query_string}" if random
      %{<img #{ opts.to_xml_attributes } />}
    end

    # :section: JavaScript related functions
    #

    # ==== Parameters
    # javascript<String>:: Text to escape for use in JavaScript.
    #
    # ==== Examples
    #   escape_js("'Lorem ipsum!' -- Some guy")
    #     # => "\\'Lorem ipsum!\\' -- Some guy"
    #
    #   escape_js("Please keep text\nlines as skinny\nas possible.")
    #     # => "Please keep text\\nlines as skinny\\nas possible."
    def escape_js(javascript)
      (javascript || '').gsub('\\','\0\0').gsub(/\r\n|\n|\r/, "\\n").gsub(/["']/) { |m| "\\#{m}" }
    end

    # ==== Parameters
    # data<Object>::
    #   Object to translate into JSON. If the object does not respond to
    #   :to_json, then data.inspect.to_json is returned instead.
    #
    # ==== Examples
    #   js({'user' => 'Lewis', 'page' => 'home'})
    #    # => "{\"user\":\"Lewis\",\"page\":\"home\"}"
    #
    #   js([ 1, 2, {"a"=>3.141}, false, true, nil, 4..10 ])
    #     # => "[1,2,{\"a\":3.141},false,true,null,\"4..10\"]"
    def js(data)
      if data.respond_to? :to_json
        data.to_json
      else
        data.inspect.to_json
      end
    end

    # :section: External JavaScript and Stylesheets
    #
    # You can use require_js(:prototype) or require_css(:shinystyles)
    # from any view or layout, and the scripts will only be included once
    # in the head of the final page. To get this effect, the head of your
    # layout you will need to include a call to include_required_js and
    # include_required_css.
    #
    # ==== Examples
    #   # File: app/views/layouts/application.html.erb
    #
    #   <html>
    #     <head>
    #       <%= include_required_js %>
    #       <%= include_required_css %>
    #     </head>
    #     <body>
    #       <%= catch_content :layout %>
    #     </body>
    #   </html>
    #
    #   # File: app/views/whatever/_part1.herb
    #
    #   <% require_js  'this' -%>
    #   <% require_css 'that', 'another_one' -%>
    #
    #   # File: app/views/whatever/_part2.herb
    #
    #   <% require_js 'this', 'something_else' -%>
    #   <% require_css 'that' -%>
    #
    #   # File: app/views/whatever/index.herb
    #
    #   <%= partial(:part1) %>
    #   <%= partial(:part2) %>
    #
    #   # Will generate the following in the final page...
    #   <html>
    #     <head>
    #       <script src="/javascripts/this.js" type="text/javascript"></script>
    #       <script src="/javascripts/something_else.js" type="text/javascript"></script>
    #       <link href="/stylesheets/that.css" media="all" rel="Stylesheet" type="text/css"/>
    #       <link href="/stylesheets/another_one.css" media="all" rel="Stylesheet" type="text/css"/>
    #     </head>
    #     .
    #     .
    #     .
    #   </html>
    #
    # See each method's documentation for more information.

    # :section: Bundling Asset Files
    #
    # The key to making a fast web application is to reduce both the amount of
    # data transfered and the number of client-server interactions. While having
    # many small, module Javascript or stylesheet files aids in the development
    # process, your web application will benefit from bundling those assets in
    # the production environment.
    #
    # An asset bundle is a set of asset files which are combined into a single
    # file. This reduces the number of requests required to render a page, and
    # can reduce the amount of data transfer required if you're using gzip
    # encoding.
    #
    # Asset bundling is always enabled in production mode, and can be optionally
    # enabled in all environments by setting the <tt>:bundle_assets</tt> value
    # in <tt>config/merb.yml</tt> to +true+.
    #
    # ==== Examples
    #
    # In the development environment, this:
    #
    #   js_include_tag :prototype, :lowpro, :bundle => true
    #
    # will produce two <script> elements. In the production mode, however, the
    # two files will be concatenated in the order given into a single file,
    # <tt>all.js</tt>, in the <tt>public/javascripts</tt> directory.
    #
    # To specify a different bundle name:
    #
    #   css_include_tag :typography, :whitespace, :bundle => :base
    #   css_include_tag :header, :footer, :bundle => "content"
    #   css_include_tag :lightbox, :images, :bundle => "lb.css"
    #
    # (<tt>base.css</tt>, <tt>content.css</tt>, and <tt>lb.css</tt> will all be
    # created in the <tt>public/stylesheets</tt> directory.)
    #
    # == Callbacks
    #
    # To use a Javascript or CSS compressor, like JSMin or YUI Compressor:
    #
    #   Merb::Assets::JavascriptAssetBundler.add_callback do |filename|
    #     system("/usr/local/bin/yui-compress #{filename}")
    #   end
    #
    #   Merb::Assets::StylesheetAssetBundler.add_callback do |filename|
    #     system("/usr/local/bin/css-min #{filename}")
    #   end
    #
    # These blocks will be run after a bundle is created.
    #
    # == Bundling Required Assets
    #
    # Combining the +require_css+ and +require_js+ helpers with bundling can be
    # problematic. You may want to separate out the common assets for your
    # application -- Javascript frameworks, common CSS, etc. -- and bundle those
    # in a "base" bundle. Then, for each section of your site, bundle the
    # required assets into a section-specific bundle.
    #
    # <b>N.B.: If you bundle an inconsistent set of assets with the same name,
    # you will have inconsistent results. Be thorough and test often.</b>
    #
    # ==== Example
    #
    # In your application layout:
    #
    #   js_include_tag :prototype, :lowpro, :bundle => :base
    #
    # In your controller layout:
    #
    #   require_js :bundle => :posts

    # The require_js method can be used to require any JavaScript file anywhere
    # in your templates. Regardless of how many times a single script is
    # included with require_js, Merb will only include it once in the header.
    #
    # ==== Parameters
    # *js<~to_s>:: JavaScript files to include.
    #
    # ==== Examples
    #   <% require_js 'jquery' %>
    #   # A subsequent call to include_required_js will render...
    #   # => <script src="/javascripts/jquery.js" type="text/javascript"></script>
    #
    #   <% require_js 'jquery', 'effects' %>
    #   # A subsequent call to include_required_js will render...
    #   # => <script src="/javascripts/jquery.js" type="text/javascript"></script>
    #   #    <script src="/javascripts/effects.js" type="text/javascript"></script>
    #
    def require_js(*js)
      @required_js ||= []
      @required_js << js
    end
    
    # All javascript files to include, without duplicates.
    #
    # ==== Parameters
    # options<Hash>:: Default options to pass to js_include_tag.
    def required_js(options = {})
      extract_required_files(@required_js, options)
    end

    # The require_css method can be used to require any CSS file anywhere in
    # your templates. Regardless of how many times a single stylesheet is
    # included with require_css, Merb will only include it once in the header.
    #
    # ==== Parameters
    # *css<~to_s>:: CSS files to include.
    #
    # ==== Examples
    #   <% require_css('style') %>
    #   # A subsequent call to include_required_css will render...
    #   # => <link href="/stylesheets/style.css" media="all" rel="Stylesheet" type="text/css"/>
    #
    #   <% require_css('style', 'ie-specific') %>
    #   # A subsequent call to include_required_css will render...
    #   # => <link href="/stylesheets/style.css" media="all" rel="Stylesheet" type="text/css"/>
    #   #    <link href="/stylesheets/ie-specific.css" media="all" rel="Stylesheet" type="text/css"/>
    #
    def require_css(*css)
      @required_css ||= []
      @required_css << css
    end
    
    # All css files to include, without duplicates.
    #
    # ==== Parameters
    # options<Hash>:: Default options to pass to css_include_tag.
    def required_css(options = {})
      extract_required_files(@required_css, options)
    end

    # A method used in the layout of an application to create +<script>+ tags
    # to include JavaScripts required in in templates and subtemplates using
    # require_js.
    #
    # ==== Parameters
    # options<Hash>:: Options to pass to js_include_tag.
    #
    # ==== Options
    # :bundle<~to_s>::
    #   The name of the bundle the scripts should be combined into.
    #
    # ==== Returns
    # String:: The JavaScript tag.
    #
    # ==== Examples
    #   # my_action.herb has a call to require_js 'jquery'
    #   # File: layout/application.html.erb
    #   include_required_js
    #   # => <script src="/javascripts/jquery.js" type="text/javascript"></script>
    #
    #   # my_action.herb has a call to require_js 'jquery', 'effects', 'validation'
    #   # File: layout/application.html.erb
    #   include_required_js
    #   # => <script src="/javascripts/jquery.js" type="text/javascript"></script>
    #   #    <script src="/javascripts/effects.js" type="text/javascript"></script>
    #   #    <script src="/javascripts/validation.js" type="text/javascript"></script>
    #
    def include_required_js(options = {})
      required_js(options).map { |req_js| js_include_tag(*req_js) }.join
    end

    # A method used in the layout of an application to create +<link>+ tags for
    # CSS stylesheets required in in templates and subtemplates using
    # require_css.
    #
    # ==== Parameters
    # options<Hash>:: Options to pass to css_include_tag.
    #
    # ==== Returns
    # String:: The CSS tag.
    #
    # ==== Options
    # :bundle<~to_s>::
    #   The name of the bundle the stylesheets should be combined into.
    # :media<~to_s>::
    #   The media attribute for the generated link element. Defaults to :all.
    #
    # ==== Examples
    #   # my_action.herb has a call to require_css 'style'
    #   # File: layout/application.html.erb
    #   include_required_css
    #   # => <link href="/stylesheets/style.css" media="all" rel="Stylesheet" type="text/css"/>
    #
    #   # my_action.herb has a call to require_css 'style', 'ie-specific'
    #   # File: layout/application.html.erb
    #   include_required_css
    #   # => <link href="/stylesheets/style.css" media="all" rel="Stylesheet" type="text/css"/>
    #   #    <link href="/stylesheets/ie-specific.css" media="all" rel="Stylesheet" type="text/css"/>
    #
    def include_required_css(options = {})
      required_css(options).map { |req_js| css_include_tag(*req_js) }.join
    end

    # ==== Parameters
    # *scripts::
    #   The scripts to include. If the last element is a Hash, it will be used
    #   as options (see below). If ".js" is left out from the script names, it
    #   will be added to them.
    #
    # ==== Options
    # :bundle<~to_s>::
    #   The name of the bundle the scripts should be combined into.
    #
    # ==== Returns
    # String:: The JavaScript include tag(s).
    #
    # ==== Examples
    #   js_include_tag 'jquery'
    #   # => <script src="/javascripts/jquery.js" type="text/javascript"></script>
    #
    #   js_include_tag 'moofx.js', 'upload'
    #   # => <script src="/javascripts/moofx.js" type="text/javascript"></script>
    #   #    <script src="/javascripts/upload.js" type="text/javascript"></script>
    #
    #   js_include_tag :effects
    #   # => <script src="/javascripts/effects.js" type="text/javascript"></script>
    #
    #   js_include_tag :jquery, :validation
    #   # => <script src="/javascripts/jquery.js" type="text/javascript"></script>
    #   #    <script src="/javascripts/validation.js" type="text/javascript"></script>
    #
    def js_include_tag(*scripts)
      options = scripts.last.is_a?(Hash) ? scripts.pop : {}
      return nil if scripts.empty?
      
      reload = options[:reload] || Merb::Config[:reload_templates]

      if (bundle_name = options[:bundle]) && Merb::Assets.bundle? && scripts.size > 1
        bundler = Merb::Assets::JavascriptAssetBundler.new(bundle_name, *scripts)
        bundled_asset = bundler.bundle!
        return js_include_tag(bundled_asset)
      end

      tags = ""

      for script in scripts
        src = asset_path(:javascript, script)
        src += src.include?('?') ? "&#{random_query_string}" : "?#{random_query_string}" if reload
        attrs = {
          :src => src,
          :type => "text/javascript"
        }
        tags << %Q{<script #{attrs.to_xml_attributes}></script>}
      end

      return tags
    end

    # ==== Parameters
    # *stylesheets::
    #   The stylesheets to include. If the last element is a Hash, it will be
    #   used as options (see below). If ".css" is left out from the stylesheet
    #   names, it will be added to them.
    #
    # ==== Options
    # :bundle<~to_s>::
    #   The name of the bundle the stylesheets should be combined into.
    # :media<~to_s>::
    #   The media attribute for the generated link element. Defaults to :all.
    #
    # ==== Returns
    # String:: The CSS include tag(s).
    #
    # ==== Examples
    #   css_include_tag 'style'
    #   # => <link href="/stylesheets/style.css" media="all" rel="Stylesheet" type="text/css" charset="utf-8" />
    #
    #   css_include_tag 'style.css', 'layout'
    #   # => <link href="/stylesheets/style.css" media="all" rel="Stylesheet" type="text/css" charset="utf-8" />
    #   #    <link href="/stylesheets/layout.css" media="all" rel="Stylesheet" type="text/css" charset="utf-8" />
    #
    #   css_include_tag :menu
    #   # => <link href="/stylesheets/menu.css" media="all" rel="Stylesheet" type="text/css" charset="utf-8" />
    #
    #   css_include_tag :style, :screen
    #   # => <link href="/stylesheets/style.css" media="all" rel="Stylesheet" type="text/css" charset="utf-8" />
    #   #    <link href="/stylesheets/screen.css" media="all" rel="Stylesheet" type="text/css" charset="utf-8" />
    #
    #  css_include_tag :style, :media => :print
    #  # => <link href="/stylesheets/style.css" media="print" rel="Stylesheet" type="text/css" charset="utf-8" />
    #
    #  css_include_tag :style, :charset => 'iso-8859-1'
    #  # => <link href="/stylesheets/style.css" media="print" rel="Stylesheet" type="text/css" charset="iso-8859-1" />
    def css_include_tag(*stylesheets)
      options = stylesheets.last.is_a?(Hash) ? stylesheets.pop : {}
      return nil if stylesheets.empty?

      reload = options[:reload] || Merb::Config[:reload_templates]

      if (bundle_name = options[:bundle]) && Merb::Assets.bundle? && stylesheets.size > 1
        bundler = Merb::Assets::StylesheetAssetBundler.new(bundle_name, *stylesheets)
        bundled_asset = bundler.bundle!
        return css_include_tag(bundled_asset)
      end

      tags = ""

      for stylesheet in stylesheets
        href = asset_path(:stylesheet, stylesheet)
        href += href.include?('?') ? "&#{random_query_string}" : "?#{random_query_string}" if reload
        attrs = {
          :href => href,
          :type => "text/css",
          :rel => "Stylesheet",
          :charset => options[:charset] || 'utf-8',
          :media => options[:media] || :all
        }
        tags << %Q{<link #{attrs.to_xml_attributes} />}
      end

      return tags
    end

    # ==== Parameters
    # *assets::
    #   The assets to include. These should be the full paths to any static served file
    #
    # ==== Returns
    # Array:: Full unique paths to assets OR
    # String:: if only a single path is requested
    # ==== Examples
    #  uniq_path("/javascripts/my.js","/javascripts/my.css")
    #  #=> ["http://assets2.my-awesome-domain.com/javascripts/my.js", "http://assets1.my-awesome-domain.com/javascripts/my.css"]
    #
    #  uniq_path(["/javascripts/my.js","/stylesheets/my.css"])
    #  #=> ["http://assets2.my-awesome-domain.com/javascripts/my.js", "http://assets1.my-awesome-domain.com/stylesheets/my.css"]
    #
    #  uniq_path(%w(/javascripts/my.js /stylesheets/my.css))
    #  #=> ["http://assets2.my-awesome-domain.com/javascripts/my.js", "http://assets1.my-awesome-domain.com/stylesheets/my.css"]
    #
    #  uniq_path('/stylesheets/somearbitrary.css')
    #  #=> "http://assets3.my-awesome-domain.com/stylesheets/somearbitrary.css"
    #
    #  uniq_path('/images/hostsexypicture.jpg')
    #  #=>"http://assets1.my-awesome-domain.com/images/hostsexypicture.jpg"
    def uniq_path(*assets)
      paths = []
      assets.collect.flatten.each do |filename|
        paths.push(Merb::Assets::UniqueAssetPath.build(filename))
      end
      paths.length > 1 ? paths : paths.first
    end

    # ==== Parameters
    # *assets::
    #   Creates unique paths for javascript files (prepends "/javascripts" and appends ".js")
    #
    # ==== Returns
    # Array:: Full unique paths to assets OR
    # String:: if only a single path is requested
    # ==== Examples
    #  uniq_js_path("my")
    #  #=> "http://assets2.my-awesome-domain.com/javascripts/my.js"
    #
    #  uniq_js_path(["admin/secrets","home/signup"])
    #  #=> ["http://assets2.my-awesome-domain.com/javascripts/admin/secrets.js",
    #         "http://assets1.my-awesome-domain.com/javascripts/home/signup.js"]
    def uniq_js_path(*assets)
      paths = []
      assets.collect.flatten.each do |filename|
        paths.push(Merb::Assets::UniqueAssetPath.build(asset_path(:javascript,filename)))
      end
      paths.length > 1 ? paths : paths.first
    end

    # ==== Parameters
    # *assets::
    #   Creates unique paths for stylesheet files (prepends "/stylesheets" and appends ".css")
    #
    # ==== Returns
    # Array:: Full unique paths to assets OR
    # String:: if only a single path is requested
    # ==== Examples
    #  uniq_css_path("my")
    #  #=> "http://assets2.my-awesome-domain.com/stylesheets/my.css"
    #
    #  uniq_css_path(["admin/secrets","home/signup"])
    #  #=> ["http://assets2.my-awesome-domain.com/stylesheets/admin/secrets.css",
    #         "http://assets1.my-awesome-domain.com/stylesheets/home/signup.css"]
    def uniq_css_path(*assets)
      paths = []
      assets.collect.flatten.each do |filename|
        paths.push(Merb::Assets::UniqueAssetPath.build(asset_path(:stylesheet,filename)))
      end
      paths.length > 1 ? paths : paths.first
    end

    # ==== Parameters
    # *assets::
    #   As js_include_tag but has unique path
    #
    # ==== Returns
    # Array:: Full unique paths to assets OR
    # String:: if only a single path is requested
    # ==== Examples
    #  uniq_js_tag("my")
    #  #=> <script type="text/javascript" src="http://assets2.my-awesome-domain.com/javascripts/my.js"></script>
    def uniq_js_tag(*assets)
      js_include_tag(*uniq_js_path(assets))
    end

    # ==== Parameters
    # *assets::
    #   As uniq_css_tag but has unique path
    #
    # ==== Returns
    # Array:: Full unique paths to assets OR
    # String:: if only a single path is requested
    # ==== Examples
    #  uniq_css_tag("my")
    #  #=> <link href="http://assets2.my-awesome-domain.com/stylesheets/my.css" type="text/css" />
    def uniq_css_tag(*assets)
      css_include_tag(*uniq_css_path(assets))
    end
    
    private 
    
    # Helper method to filter out duplicate files.
    #
    # ==== Parameters
    # options<Hash>:: Options to pass to include tag methods.
    def extract_required_files(files, options = {})
      return [] if files.nil? || files.empty?
      seen = []
      files.inject([]) do |extracted, req_js|
        include_files, include_options = if req_js.last.is_a?(Hash)
          [req_js[0..-2], options.merge(req_js.last)]
        else
          [req_js, options]
        end
        seen += (includes = include_files - seen)
        extracted << (includes + [include_options]) unless includes.empty?
        extracted
      end
    end
    
    def random_query_string
      Time.now.strftime("%m%d%H%M%S#{rand(99)}")
    end
    
  end
end