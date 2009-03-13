#!/usr/bin/env ruby

$:.unshift File.dirname(__FILE__) + "/../../lib"
require 'camping'
require 'camping/session'
  
Camping.goes :Blog

module Blog
    include Camping::Session
end

module Blog::Models
    class Post < Base; belongs_to :user; end
    class Comment < Base; belongs_to :user; end
    class User < Base; end

    class CreateTheBasics < V 1.0
      def self.up
        create_table :blog_posts, :force => true do |t|
          t.column :id,       :integer, :null => false
          t.column :user_id,  :integer, :null => false
          t.column :title,    :string,  :limit => 255
          t.column :body,     :text
        end
        create_table :blog_users, :force => true do |t|
          t.column :id,       :integer, :null => false
          t.column :username, :string
          t.column :password, :string
        end
        create_table :blog_comments, :force => true do |t|
          t.column :id,       :integer, :null => false
          t.column :post_id,  :integer, :null => false
          t.column :username, :string
          t.column :body,     :text
        end
        User.create :username => 'admin', :password => 'camping'
      end
      def self.down
        drop_table :blog_posts
        drop_table :blog_users
        drop_table :blog_comments
      end
    end
end

module Blog::Controllers
    class Index < R '/'
        def get
            @posts = Post.find :all
            render :index
        end
    end
     
    class Add
        def get
            unless @state.user_id.blank?
                @user = User.find @state.user_id
                @post = Post.new
            end
            render :add
        end
        def post
            unless @state.user_id.blank?
                post = Post.create :title => input.post_title, :body => input.post_body,
                                   :user_id => @state.user_id
                redirect View, post
            end
        end
    end

    class Info < R '/info/(\d+)', '/info/(\w+)/(\d+)', '/info', '/info/(\d+)/(\d+)/(\d+)/([\w-]+)'
        def get(*args)
            div do
                code args.inspect; br; br
                code @env.inspect; br
                code "Link: #{R(Info, 1, 2)}"
            end
        end
    end

    class View < R '/view/(\d+)'
        def get post_id 
            @post = Post.find post_id
            @comments = Models::Comment.find :all, :conditions => ['post_id = ?', post_id]
            render :view
        end
    end
     
    class Edit < R '/edit/(\d+)', '/edit'
        def get post_id 
            unless @state.user_id.blank?
                @user = User.find @state.user_id
            end
            @post = Post.find post_id
            render :edit
        end
     
        def post
            unless @state.user_id.blank?
                @post = Post.find input.post_id
                @post.update_attributes :title => input.post_title, :body => input.post_body
                redirect View, @post
            end
        end
    end
     
    class Comment
        def post
            Models::Comment.create(:username => input.post_username,
                       :body => input.post_body, :post_id => input.post_id)
            redirect View, input.post_id
        end
    end
     
    class Login
        def post
            @user = User.find :first, :conditions => ['username = ? AND password = ?', input.username, input.password]
     
            if @user
                @login = 'login success !'
                @state.user_id = @user.id
            else
                @login = 'wrong user name or password'
            end
            render :login
        end
    end
     
    class Logout
        def get
            @state.user_id = nil
            render :logout
        end
    end
     
    class Style < R '/styles.css'
        def get
            @headers["Content-Type"] = "text/css; charset=utf-8"
            @body = %{
                body {
                    font-family: Utopia, Georga, serif;
                }
                h1.header {
                    background-color: #fef;
                    margin: 0; padding: 10px;
                }
                div.content {
                    padding: 10px;
                }
            }
        end
    end
end

module Blog::Views

    def layout
      html do
        head do
          title 'blog'
          link :rel => 'stylesheet', :type => 'text/css', 
               :href => '/styles.css', :media => 'screen'
        end
        body do
          h1.header { a 'blog', :href => R(Index) }
          div.content do
            self << yield
          end
        end
      end
    end

    def index
      if @posts.empty?
        p 'No posts found.'
        p { a 'Add', :href => R(Add) }
      else
        for post in @posts
          _post(post)
        end
      end
    end

    def login
      p { b @login }
      p { a 'Continue', :href => R(Add) }
    end

    def logout
      p "You have been logged out."
      p { a 'Continue', :href => R(Index) }
    end

    def add
      if @user
        _form(@post, :action => R(Add))
      else
        _login
      end
    end

    def edit
      if @user
        _form(@post, :action => R(Edit))
      else
        _login
      end
    end

    def view
        _post(@post)

        p "Comment for this post:"
        for c in @comments
          h1 c.username
          p c.body
        end

        form :action => R(Comment), :method => 'post' do
          label 'Name', :for => 'post_username'; br
          input :name => 'post_username', :type => 'text'; br
          label 'Comment', :for => 'post_body'; br
          textarea :name => 'post_body' do; end; br
          input :type => 'hidden', :name => 'post_id', :value => @post.id
          input :type => 'submit'
        end
    end

    # partials
    def _login
      form :action => R(Login), :method => 'post' do
        label 'Username', :for => 'username'; br
        input :name => 'username', :type => 'text'; br

        label 'Password', :for => 'password'; br
        input :name => 'password', :type => 'text'; br

        input :type => 'submit', :name => 'login', :value => 'Login'
      end
    end

    def _post(post)
      h1 post.title
      p post.body
      p do
        [a("Edit", :href => R(Edit, post)), a("View", :href => R(View, post))].join " | "
      end
    end

    def _form(post, opts)
      p { "You are logged in as #{@user.username} | #{a 'Logout', :href => R(Logout)}" }
      form({:method => 'post'}.merge(opts)) do
        label 'Title', :for => 'post_title'; br
        input :name => 'post_title', :type => 'text', 
              :value => post.title; br

        label 'Body', :for => 'post_body'; br
        textarea post.body, :name => 'post_body'; br

        input :type => 'hidden', :name => 'post_id', :value => post.id
        input :type => 'submit'
      end
    end
end
 
def Blog.create
    Camping::Models::Session.create_schema
    Blog::Models.create_schema :assume => (Blog::Models::Post.table_exists? ? 1.0 : 0.0)
end

