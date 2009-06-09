= dm-rest-adapter

A DataMapper adapter for REST Web Services

== Usage

DM Rest Adapter requires the use of a model which is the same name as the resource you are using. For example, if you have a resource named "posts" you will create a standard datamapper object called post.rb in app/models. The only difference in this model is you will need to define the rest adapter for the model. The following is an example of a post model, where the host settings point to the app you are running the resource on. In addition I have included a basic auth login which will be used if your resource requires auth:

DataMapper.setup(:default, {
 :adapter  => 'rest',
 :format   => 'xml',
 :host     => 'localhost',
 :port     => 4000,
 :login    => 'user',
 :password => 'verys3crit'
})

class Post

  include DataMapper::Resource

  property :id, Serial
  property :title, String
  property :body,  Text

end


If you notice this looks exactly like a normal datmapper model. Every property you define will map itself with the xml returned or posted from/to the resource.

== Code

Now for some code examples. DM Rest Adapter uses the same methods as datamapper including during creation.

Post.first => returns the object from the resouce
Post.get(1) => returns the object from the resource
p = Post.new(:title => "My awesome blog post", :body => "I really have nothing to say...")
p.save => saves the resource on the remote

== Caveat

Posts do not honor RESTful HTTP status codes. I might fix this...

== TODO:

Nested resources
Put verb actions
