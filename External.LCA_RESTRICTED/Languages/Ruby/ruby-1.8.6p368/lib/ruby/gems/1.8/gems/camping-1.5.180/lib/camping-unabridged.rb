# == About camping.rb
#
# Camping comes with two versions of its source code.  The code contained in
# lib/camping.rb is compressed, stripped of whitespace, using compact algorithms
# to keep it tight.  The unspoken rule is that camping.rb should be flowed with
# no more than 80 characters per line and must not exceed four kilobytes.
#
# On the other hand, lib/camping-unabridged.rb contains the same code, laid out
# nicely with piles of documentation everywhere.  This documentation is entirely
# generated from lib/camping-unabridged.rb using RDoc and our "flipbook" template
# found in the extras directory of any camping distribution.
#
# == Requirements
#
# Camping requires at least Ruby 1.8.2.
#
# Camping depends on the following libraries.  If you install through RubyGems,
# these will be automatically installed for you.
#
# * ActiveRecord, used in your models.
#   ActiveRecord is an object-to-relational database mapper with adapters
#   for SQLite3, MySQL, PostgreSQL, SQL Server and more.
# * Markaby, used in your views to describe HTML in plain Ruby.
# * MetAid, a few metaprogramming methods which Camping uses.
# * Tempfile, for storing file uploads.
#
# Camping also works well with Mongrel, the swift Ruby web server.
# http://rubyforge.org/projects/mongrel  Mongrel comes with examples
# in its <tt>examples/camping</tt> directory. 
#
%w[active_support markaby tempfile uri].each { |lib| require lib }

# == Camping 
#
# The camping module contains three modules for separating your application:
#
# * Camping::Models for your database interaction classes, all derived from ActiveRecord::Base.
# * Camping::Controllers for storing controller classes, which map URLs to code.
# * Camping::Views for storing methods which generate HTML.
#
# Of use to you is also one module for storing helpful additional methods:
#
# * Camping::Helpers which can be used in controllers and views.
#
# == The Camping Server
#
# How do you run Camping apps?  Oh, uh... The Camping Server!
#
# The Camping Server is, firstly and thusly, a set of rules.  At the very least, The Camping Server must:
#
# * Load all Camping apps in a directory.
# * Load new apps that appear in that directory.
# * Mount those apps according to their filename. (e.g. blog.rb is mounted at /blog.)
# * Run each app's <tt>create</tt> method upon startup.
# * Reload the app if its modification time changes.
# * Reload the app if it requires any files under the same directory and one of their modification times changes.
# * Support the X-Sendfile header. 
#
# In fact, Camping comes with its own little The Camping Server.
#
# At a command prompt, run: <tt>camping examples/</tt> and the entire <tt>examples/</tt> directory will be served.
#
# Configurations also exist for Apache and Lighttpd.  See http://code.whytheluckystiff.net/camping/wiki/TheCampingServer.
#
# == The <tt>create</tt> method
#
# Many postambles will check for your application's <tt>create</tt> method and will run it
# when the web server starts up.  This is a good place to check for database tables and create
# those tables to save users of your application from needing to manually set them up.
#
#   def Blog.create
#     unless Blog::Models::Post.table_exists?
#       ActiveRecord::Schema.define do
#         create_table :blog_posts, :force => true do |t|
#           t.column :id,       :integer, :null => false
#           t.column :user_id,  :integer, :null => false
#           t.column :title,    :string,  :limit => 255
#           t.column :body,     :text
#         end
#       end
#     end
#   end 
#
# For more tips, see http://code.whytheluckystiff.net/camping/wiki/GiveUsTheCreateMethod.
module Camping
  # Stores an +Array+ of all Camping applications modules.  Modules are added
  # automatically by +Camping.goes+.
  #
  #   Camping.goes :Blog
  #   Camping.goes :Tepee
  #   Camping::Apps # => [Blog, Tepee]
  # 
  Apps = []
  C = self
  S = IO.read(__FILE__).sub(/^  S = I.+$/,'')
  P="Cam\ping Problem!"

  H = HashWithIndifferentAccess
  # An object-like Hash, based on ActiveSupport's HashWithIndifferentAccess.
  # All Camping query string and cookie variables are loaded as this.
  # 
  # To access the query string, for instance, use the <tt>@input</tt> variable.
  #
  #   module Blog::Models
  #     class Index < R '/'
  #       def get
  #         if page = @input.page.to_i > 0
  #           page -= 1
  #         end
  #         @posts = Post.find :all, :offset => page * 20, :limit => 20
  #         render :index
  #       end
  #     end
  #   end
  #
  # In the above example if you visit <tt>/?page=2</tt>, you'll get the second
  # page of twenty posts.  You can also use <tt>@input[:page]</tt> or <tt>@input['page']</tt>
  # to get the value for the <tt>page</tt> query variable.
  #
  # Use the <tt>@cookies</tt> variable in the same fashion to access cookie variables.
  # Also, the <tt>@env</tt> variable is an H containing the HTTP headers and server info.
  class H
    # Gets or sets keys in the hash.
    #
    #   @cookies.my_favorite = :macadamian
    #   @cookies.my_favorite
    #   => :macadamian
    #
    def method_missing(m,*a)
        m.to_s=~/=$/?self[$`]=a[0]:a==[]?self[m]:raise(NoMethodError,"#{m}")
    end
    alias_method :u, :regular_update
  end

  # Helpers contains methods available in your controllers and views.  You may add
  # methods of your own to this module, including many helper methods from Rails.
  # This is analogous to Rails' <tt>ApplicationHelper</tt> module.
  #
  # == Using ActionPack Helpers
  #
  # If you'd like to include helpers from Rails' modules, you'll need to look up the
  # helper module in the Rails documentation at http://api.rubyonrails.org/.
  #
  # For example, if you look up the <tt>ActionView::Helpers::FormHelper</tt> class,
  # you'll find that it's loaded from the <tt>action_view/helpers/form_helper.rb</tt>
  # file.  You'll need to have the ActionPack gem installed for this to work.
  #
  #   require 'action_view/helpers/form_helper.rb'
  #
  #   # This example is unfinished.. soon..
  #
  module Helpers
    # From inside your controllers and views, you will often need to figure out
    # the route used to get to a certain controller +c+.  Pass the controller class
    # and any arguments into the R method, a string containing the route will be
    # returned to you.
    #
    # Assuming you have a specific route in an edit controller:
    #
    #   class Edit < R '/edit/(\d+)'
    #
    # A specific route to the Edit controller can be built with:
    #
    #   R(Edit, 1)
    #
    # Which outputs: <tt>/edit/1</tt>.
    #
    # You may also pass in a model object and the ID of the object will be used.
    #
    # If a controller has many routes, the route will be selected if it is the
    # first in the routing list to have the right number of arguments.
    #
    # == Using R in the View
    #
    # Keep in mind that this route doesn't include the root path.
    # You will need to use <tt>/</tt> (the slash method above) in your controllers.
    # Or, go ahead and use the Helpers#URL method to build a complete URL for a route.
    #
    # However, in your views, the :href, :src and :action attributes automatically
    # pass through the slash method, so you are encouraged to use <tt>R</tt> or
    # <tt>URL</tt> in your views.
    #
    #  module Blog::Views
    #    def menu
    #      div.menu! do
    #        a 'Home', :href => URL()
    #        a 'Profile', :href => "/profile"
    #        a 'Logout', :href => R(Logout)
    #        a 'Google', :href => 'http://google.com'
    #      end
    #    end
    #  end
    #
    # Let's say the above example takes place inside an application mounted at
    # <tt>http://localhost:3301/frodo</tt> and that a controller named <tt>Logout</tt>
    # is assigned to route <tt>/logout</tt>.  The HTML will come out as:
    #
    #   <div id="menu">
    #     <a href="//localhost:3301/frodo/">Home</a>
    #     <a href="/frodo/profile">Profile</a>
    #     <a href="/frodo/logout">Logout</a>
    #     <a href="http://google.com">Google</a>
    #   </div>
    #
    def R(c,*g)
      p,h=/\(.+?\)/,g.grep(Hash)
      (g-=h).inject(c.urls.find{|x|x.scan(p).size==g.size}.dup){|s,a|
        s.sub p,C.escape((a[a.class.primary_key]rescue a))
      }+(h.any?? "?"+h[0].map{|x|x.map{|z|C.escape z}*"="}*"&": "")
    end

    # Shows AR validation errors for the object passed. 
    # There is no output if there are no errors.
    #
    # An example might look like:
    #
    #   errors_for @post
    #
    # Might (depending on actual data) render something like this in Markaby:
    #
    #   ul.errors do
    #     li "Body can't be empty"
    #     li "Title must be unique"
    #   end
    #
    # Add a simple ul.errors {color:red; font-weight:bold;} CSS rule and you
    # have built-in, usable error checking in only one line of code. :-)
    #
    # See AR validation documentation for details on validations.
    def errors_for(o); ul.errors { o.errors.each_full { |er| li er } } if o.errors.any?; end
    # Simply builds a complete path from a path +p+ within the app.  If your application is 
    # mounted at <tt>/blog</tt>:
    #
    #   self / "/view/1"    #=> "/blog/view/1"
    #   self / "styles.css" #=> "styles.css"
    #   self / R(Edit, 1)   #=> "/blog/edit/1"
    #
    def /(p); p[/^\//]?@root+p:p end
    # Builds a URL route to a controller or a path, returning a URI object.
    # This way you'll get the hostname and the port number, a complete URL.
    # No scheme is given (http or https).
    #
    # You can use this to grab URLs for controllers using the R-style syntax.
    # So, if your application is mounted at <tt>http://test.ing/blog/</tt>
    # and you have a View controller which routes as <tt>R '/view/(\d+)'</tt>:
    #
    #   URL(View, @post.id)    #=> #<URL://test.ing/blog/view/12>
    #
    # Or you can use the direct path:
    #
    #   self.URL               #=> #<URL://test.ing/blog/>
    #   self.URL + "view/12"   #=> #<URL://test.ing/blog/view/12>
    #   URL("/view/12")        #=> #<URL://test.ing/blog/view/12>
    #
    # Since no scheme is given, you will need to add the scheme yourself:
    #
    #   "http" + URL("/view/12")   #=> "http://test.ing/blog/view/12"
    #
    # It's okay to pass URL strings through this method as well:
    #
    #   URL("http://google.com")  #=> #<URI:http://google.com>
    #
    # Any string which doesn't begin with a slash will pass through
    # unscathed.
    def URL c='/',*a
      c = R(c, *a) if c.respond_to? :urls
      c = self/c
      c = "//"+@env.HTTP_HOST+c if c[/^\//]
      URI(c)
    end
  end

  # Camping::Base is built into each controller by way of the generic routing
  # class Camping::R.  In some ways, this class is trying to do too much, but
  # it saves code for all the glue to stay in one place.
  #
  # Forgivable, considering that it's only really a handful of methods and accessors.
  #
  # == Treating controller methods like Response objects
  #
  # Camping originally came with a barebones Response object, but it's often much more readable
  # to just use your controller as the response.
  #
  # Go ahead and alter the status, cookies, headers and body instance variables as you
  # see fit in order to customize the response.
  #
  #   module Camping::Controllers
  #     class SoftLink
  #       def get
  #         redirect "/"
  #       end
  #     end
  #   end
  #
  # Is equivalent to:
  #
  #   module Camping::Controllers
  #     class SoftLink
  #       def get
  #         @status = 302
  #         @headers['Location'] = "/"
  #       end
  #     end
  #   end
  #
  module Base
    include Helpers
    attr_accessor :input, :cookies, :env, :headers, :body, :status, :root
    Z = "\r\n"

    # Display a view, calling it by its method name +m+.  If a <tt>layout</tt>
    # method is found in Camping::Views, it will be used to wrap the HTML.
    #
    #   module Camping::Controllers
    #     class Show
    #       def get
    #         @posts = Post.find :all
    #         render :index
    #       end
    #     end
    #   end
    #
    def render(m); end; undef_method :render

    # Any stray method calls will be passed to Markaby.  This means you can reply
    # with HTML directly from your controller for quick debugging.
    #
    #   module Camping::Controllers
    #     class Info
    #       def get; code @env.inspect end
    #     end
    #   end
    #
    # If you have a <tt>layout</tt> method in Camping::Views, it will be used to
    # wrap the HTML.
    def method_missing(*a,&b)
      a.shift if a[0]==:render
      m=Mab.new({},self)
      s=m.capture{send(*a,&b)}
      s=m.capture{send(:layout){s}} if /^_/!~a[0].to_s and m.respond_to?:layout
      s
    end

    # Formulate a redirect response: a 302 status with <tt>Location</tt> header
    # and a blank body.  Uses Helpers#URL to build the location from a controller
    # route or path.
    #
    # So, given a root of <tt>http://localhost:3301/articles</tt>:
    #
    #   redirect "view/12"    # redirects to "//localhost:3301/articles/view/12"
    #   redirect View, 12     # redirects to "//localhost:3301/articles/view/12"
    #
    # <b>NOTE:</b> This method doesn't magically exit your methods and redirect.
    # You'll need to <tt>return redirect(...)</tt> if this isn't the last statement
    # in your code.
    def redirect(*a)
      r(302,'','Location'=>URL(*a))
    end

    # A quick means of setting this controller's status, body and headers.
    # Used internally by Camping, but... by all means...
    #
    #   r(302, '', 'Location' => self / "/view/12")
    #
    # Is equivalent to:
    #
    #   redirect "/view/12"
    #
    def r(s, b, h = {}); @status = s; @headers.merge!(h); @body = b; end

    # Turn a controller into an array.  This is designed to be used to pipe
    # controllers into the <tt>r</tt> method.  A great way to forward your
    # requests!
    #
    #   class Read < '/(\d+)'
    #     def get(id)
    #       Post.find(id)
    #     rescue
    #       r *Blog.get(:NotFound, @env.REQUEST_URI)
    #     end
    #   end
    #
    def to_a;[@status, @body, @headers] end

    def initialize(r, e, m) #:nodoc:
      e = H[e.to_hash]
      @status, @method, @env, @headers, @root = 200, m.downcase, e, 
          {'Content-Type'=>'text/html'}, e.SCRIPT_NAME.sub(/\/$/,'')
      @k = C.kp(e.HTTP_COOKIE)
      qs = C.qsp(e.QUERY_STRING)
      @in = r
      if %r|\Amultipart/form-data.*boundary=\"?([^\";,]+)|n.match(e.CONTENT_TYPE)
        b = /(?:\r?\n|\A)#{Regexp::quote("--#$1")}(?:--)?\r$/
        until @in.eof?
          fh=H[]
          for l in @in
            case l
            when Z: break
            when /^Content-Disposition: form-data;/
              fh.u H[*$'.scan(/(?:\s(\w+)="([^"]+)")/).flatten]
            when /^Content-Type: (.+?)(\r$|\Z)/m
              puts "=> fh[type] = #$1"
              fh[:type] = $1
            end
          end
          fn=fh[:name]
          o=if fh[:filename]
            o=fh[:tempfile]=Tempfile.new(:C)
            o.binmode
          else
            fh=""
          end
          while l=@in.read(16384)
            if l=~b
              o<<$`.chomp
              @in.seek(-$'.size,IO::SEEK_CUR)
              break
            end
            o<<l
          end
          C.qsp(fn,'&;',fh,qs) if fn
          fh[:tempfile].rewind if fh.is_a?H
        end
      elsif @method == "post"
        qs.merge!(C.qsp(@in.read))
      end
      @cookies, @input = @k.dup, qs.dup
    end

    # All requests pass through this method before going to the controller.  Some magic
    # in Camping can be performed by overriding this method.
    #
    # See http://code.whytheluckystiff.net/camping/wiki/BeforeAndAfterOverrides for more
    # on before and after overrides with Camping.
    def service(*a)
      @body = send(@method, *a) if respond_to? @method
      @headers['Set-Cookie'] = @cookies.map { |k,v| "#{k}=#{C.escape(v)}; path=#{self/"/"}" if v != @k[k] } - [nil]
      self
    end

    # Used by the web server to convert the current request to a string.  If you need to
    # alter the way Camping builds HTTP headers, consider overriding this method.
    def to_s
      a=[]
      @headers.map{|k,v|[*v].map{|x|a<<"#{k}: #{x}"}}
      "Status: #{@status}#{Z+a*Z+Z*2+@body}"
    end

  end

  # Controllers is a module for placing classes which handle URLs.  This is done
  # by defining a route to each class using the Controllers::R method.
  #
  #   module Camping::Controllers
  #     class Edit < R '/edit/(\d+)'
  #       def get; end
  #       def post; end
  #     end
  #   end
  #
  # If no route is set, Camping will guess the route from the class name.
  # The rule is very simple: the route becomes a slash followed by the lowercased
  # class name.  See Controllers::D for the complete rules of dispatch.
  #
  # == Special classes
  #
  # There are two special classes used for handling 404 and 500 errors.  The
  # NotFound class handles URLs not found.  The ServerError class handles exceptions
  # uncaught by your application.
  module Controllers
    @r = []
    class << self
      def r #:nodoc:
        @r
      end
      # Add routes to a controller class by piling them into the R method.
      #
      #   module Camping::Controllers
      #     class Edit < R '/edit/(\d+)', '/new'
      #       def get(id)
      #         if id   # edit
      #         else    # new
      #         end
      #       end
      #     end
      #   end
      #
      # You will need to use routes in either of these cases:
      #
      # * You want to assign multiple routes to a controller.
      # * You want your controller to receive arguments.
      #
      # Most of the time the rules inferred by dispatch method Controllers::D will get you
      # by just fine.
      def R *u
        r=@r
        Class.new {
          meta_def(:urls){u}
          meta_def(:inherited){|x|r<<x}
        }
      end

      # Dispatch routes to controller classes.
      # For each class, routes are checked for a match based on their order in the routing list
      # given to Controllers::R.  If no routes were given, the dispatcher uses a slash followed
      # by the name of the controller lowercased.
      #
      # Controllers are searched in this order:
      #
      # # Classes without routes, since they refer to a very specific URL.
      # # Classes with routes are searched in order of their creation.
      #
      # So, define your catch-all controllers last.
      def D(path)
        r.map { |k|
          k.urls.map { |x|
            return k, $~[1..-1] if path =~ /^#{x}\/?$/
          }
        }
        [NotFound, [path]]
      end

      # The route maker, this is called by Camping internally, you shouldn't need to call it.
      #
      # Still, it's worth know what this method does.  Since Ruby doesn't keep track of class
      # creation order, we're keeping an internal list of the controllers which inherit from R().
      # This method goes through and adds all the remaining routes to the beginning of the list
      # and ensures all the controllers have the right mixins.
      #
      # Anyway, if you are calling the URI dispatcher from outside of a Camping server, you'll
      # definitely need to call this at least once to set things up.
      def M
        def M #:nodoc:
        end
        constants.map { |c|
          k=const_get(c)
          k.send :include,C,Base,Models
          r[0,0]=k if !r.include?k
          k.meta_def(:urls){["/#{c.downcase}"]}if !k.respond_to?:urls
        }
      end
    end

    # The NotFound class is a special controller class for handling 404 errors, in case you'd
    # like to alter the appearance of the 404.  The path is passed in as +p+.
    #
    #   module Camping::Controllers
    #     class NotFound
    #       def get(p)
    #         @status = 404
    #         div do
    #           h1 'Camping Problem!'
    #           h2 "#{p} not found"
    #         end
    #       end
    #     end
    #   end
    #
    class NotFound < R()
      def get(p)
        r(404, Mab.new{h1(P);h2("#{p} not found")})
      end
    end

    # The ServerError class is a special controller class for handling many (but not all) 500 errors.
    # If there is a parse error in Camping or in your application's source code, it will not be caught
    # by Camping.  The controller class +k+ and request method +m+ (GET, POST, etc.) where the error
    # took place are passed in, along with the Exception +e+ which can be mined for useful info.
    #
    #   module Camping::Controllers
    #     class ServerError
    #       def get(k,m,e)
    #         @status = 500
    #         div do
    #           h1 'Camping Problem!'
    #           h2 "in #{k}.#{m}"
    #           h3 "#{e.class} #{e.message}:"
    #           ul do
    #             e.backtrace.each do |bt|
    #               li bt
    #             end
    #           end
    #         end
    #       end
    #     end
    #   end
    #
    class ServerError < R()
      def get(k,m,e)
        r(500, Mab.new { 
          h1(P)
          h2 "#{k}.#{m}"
          h3 "#{e.class} #{e.message}:"
          ul { e.backtrace.each { |bt| li bt } }
        }.to_s)
      end
    end
  end
  X = Controllers

  class << self
    # When you are running many applications, you may want to create independent
    # modules for each Camping application.  Namespaces for each.  Camping::goes
    # defines a toplevel constant with the whole MVC rack inside.
    #
    #   require 'camping'
    #   Camping.goes :Blog
    #
    #   module Blog::Controllers; ... end
    #   module Blog::Models;      ... end
    #   module Blog::Views;       ... end
    #
    def goes(m)
      eval S.gsub(/Camping/,m.to_s).gsub("A\pps = []","Cam\ping::Apps<<self"), TOPLEVEL_BINDING
    end

    # URL escapes a string.
    #
    #   Camping.escape("I'd go to the museum straightway!")  
    #     #=> "I%27d+go+to+the+museum+straightway%21"
    #
    def escape(s); s.to_s.gsub(/[^ \w.-]+/n){'%'+($&.unpack('H2'*$&.size)*'%').upcase}.tr(' ', '+') end

    # Unescapes a URL-encoded string.
    #
    #   Camping.un("I%27d+go+to+the+museum+straightway%21") 
    #     #=> "I'd go to the museum straightway!"
    #
    def un(s); s.tr('+', ' ').gsub(/%([\da-f]{2})/in){[$1].pack('H*')} end

    # Parses a query string into an Camping::H object.
    #
    #   input = Camping.qsp("name=Philarp+Tremain&hair=sandy+blonde")
    #   input.name
    #     #=> "Philarp Tremaine"
    #
    # Also parses out the Hash-like syntax used in PHP and Rails and builds
    # nested hashes from it.
    #
    #   input = Camping.qsp("post[id]=1&post[user]=_why")
    #     #=> {'post' => {'id' => '1', 'user' => '_why'}}
    #
    def qsp(qs, d='&;', y=nil, z=H[])
        m = proc {|_,o,n|o.u(n,&m)rescue([*o]<<n)}
        (qs||'').
            split(/[#{d}] */n).
            inject((b,z=z,H[])[0]) { |h,p| k, v=un(p).split('=',2)
                h.u(k.split(/[\]\[]+/).reverse.
                    inject(y||v) { |x,i| H[i,x] },&m)
            } 
    end

    # Parses a string of cookies from the <tt>Cookie</tt> header.
    def kp(s); c = qsp(s, ';,'); end

    # Fields a request through Camping.  For traditional CGI applications, the method can be
    # executed without arguments.
    #
    #   if __FILE__ == $0
    #     Camping::Models::Base.establish_connection :adapter => 'sqlite3',
    #         :database => 'blog3.db'
    #     Camping::Models::Base.logger = Logger.new('camping.log')
    #     puts Camping.run
    #   end
    #
    # The Camping controller returned from <tt>run</tt> has a <tt>to_s</tt> method in case you
    # are running from CGI or want to output the full HTTP output.  In the above example, <tt>puts</tt>
    # will call <tt>to_s</tt> for you.
    #
    # For FastCGI and Webrick-loaded applications, you will need to use a request loop, with <tt>run</tt>
    # at the center, passing in the read +r+ and write +w+ streams.  You will also need to mimick or
    # pass in the <tt>ENV</tt> replacement as part of your wrapper.
    #
    # See Camping::FastCGI and Camping::WEBrick for examples.
    #
    def run(r=$stdin,e=ENV)
      X.M
      k,a=X.D un("/#{e['PATH_INFO']}".gsub(/\/+/,'/'))
      k.new(r,e,(m=e['REQUEST_METHOD']||"GET")).Y.service *a
    rescue Object=>x
      X::ServerError.new(r,e,'get').service(k,m,x)
    end

    # The Camping scriptable dispatcher.  Any unhandled method call to the app module will
    # be sent to a controller class, specified as an argument.
    #
    #   Blog.get(:Index)
    #   #=> #<Blog::Controllers::Index ... >
    #
    # The controller object contains all the @cookies, @body, @headers, etc. formulated by
    # the response.
    #
    # You can also feed environment variables and query variables as a hash, the final
    # argument.
    #
    #   Blog.post(:Login, :input => {'username' => 'admin', 'password' => 'camping'})
    #   #=> #<Blog::Controllers::Login @user=... >
    #
    #   Blog.get(:Info, :env => {:HTTP_HOST => 'wagon'})
    #   #=> #<Blog::Controllers::Info @env={'HTTP_HOST'=>'wagon'} ...>
    #
    def method_missing(m, c, *a)
      X.M
      k = X.const_get(c).new(StringIO.new,
             H['HTTP_HOST','','SCRIPT_NAME','','HTTP_COOKIE',''],m.to_s)
      H.new(a.pop).each { |e,f| k.send("#{e}=",f) } if Hash === a[-1]
      k.service *a
    end
  end

  # Models is an empty Ruby module for housing model classes derived
  # from ActiveRecord::Base.  As a shortcut, you may derive from Base
  # which is an alias for ActiveRecord::Base.
  #
  #   module Camping::Models
  #     class Post < Base; belongs_to :user end
  #     class User < Base; has_many :posts end
  #   end
  #
  # == Where Models are Used
  #
  # Models are used in your controller classes.  However, if your model class
  # name conflicts with a controller class name, you will need to refer to it
  # using the Models module.
  #
  #   module Camping::Controllers
  #     class Post < R '/post/(\d+)'
  #       def get(post_id)
  #         @post = Models::Post.find post_id
  #         render :index
  #       end
  #     end
  #   end
  #
  # Models cannot be referred to in Views at this time.
  module Models
      autoload :Base,'camping/db'
      def Y;self;end
  end

  # Views is an empty module for storing methods which create HTML.  The HTML is described
  # using the Markaby language.
  #
  # == Using the layout method
  #
  # If your Views module has a <tt>layout</tt> method defined, it will be called with a block
  # which will insert content from your view.
  module Views; include Controllers, Helpers end
  
  # The Mab class wraps Markaby, allowing it to run methods from Camping::Views
  # and also to replace :href, :action and :src attributes in tags by prefixing the root
  # path.
  class Mab < Markaby::Builder
      include Views
      def tag!(*g,&b)
          h=g[-1]
          [:href,:action,:src].each{|a|(h[a]=self/h[a])rescue 0}
          super 
      end
  end
end

