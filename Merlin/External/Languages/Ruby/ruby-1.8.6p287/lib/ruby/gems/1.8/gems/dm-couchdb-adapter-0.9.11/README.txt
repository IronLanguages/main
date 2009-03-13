This is a datamapper adapter to couchdb.

NOTE: some functionality and their specs are based on functionality that is in
edge couch but not in stable.  If you want everything to work, use edge.
Otherwise, your milage may vary.  Good luck and let me know about any bugs.

== Setup
Install with the rest of the dm-more package, using:
  gem install dm-more

Setting up:
  The easiest way is to pass a full url, here is an example:
  "couchdb://localhost:5984/my_app_development"

  You can break it out like this:
  "#{adapter}://#{host}:#{port}/#{database}"
  - adapter should be :couchdb
  - database (should be the name of your database)
  - host (probably localhost)
  - port should be specified (couchdb defaults to port 5984)

If you haven't you'll need to create this database.
The easiest way is with curl in the terminal, like so:
'curl -X PUT localhost:5984/my_app_development'
You should use the same address here as you did to connect (just leave out the 'couchdb://' part)

Now, if you want to have a model stored in couch you can just use:
include DataMapper::CouchResource
instead of the normal:
include DataMapper::Resource

This adds the following reserved properties (which have special meaning in Couch, so don't overwrite them):
property :id, String, :key => true, :field => '_id'
property :rev, String, :field => '_rev'
property :attachments, DataMapper::Types::JsonObject, :field => '_attachments'

If you want the model to use your couch repository by default, be sure to also add the following(replacing :couch with your repository name):
def self.default_repository_name
  :couch
end

You should now be able to use resources and their properties and have them stored to couchdb.
NOTE: 'couchdb_type' is a reserved property, used to map documents to their ruby models.

== Views
Special consideration has been made to work with CouchDB views.
You should do ALL queries you'll be repeating this way, doing 'User.all(:something => 'this)' will work, but it is much slower and more inefficient than running views you already created.
You define them in the model with the view function and use Model.auto_migrate! to add the views for that Model to the database, or DataMapper.auto_migrate! to add the views for all models to the database.

An example class with views:

class User
  include DataMapper::Resource

  property :name, String
  view(:by_name_only_this_model) {{ "map" => "function(doc) { if (doc.couchdb_type == 'User') { emit(doc.name, doc); } }" }}
  view(:by_name_with_descendants) {{ "map" => "function(doc) { if (#{couchdb_types_condition}) { emit(doc.name, doc); } }" }}
end

couchdb_types_condition builds a condition for you if you want a view that checks to see if the couchdb_type of the record is that of the current model or any of its descendants, just load your models and run Model.couchdb_types_condition and copy/paste the output as the condition in the models view.  I will be making this smoother/cleaner, as I need to reimplement view handling.

You could then call User.by_name to get a listing of users ordered by name, or pass a key to try and find a specific user by their name, ie User.by_name(:key => 'username').

# TODO: add details about other view options

== Example
For a working example of this functionality checkout muddle, my merb based tumblelog, which uses this adapter to save its posts, at:
http://github.com/geemus/muddle
