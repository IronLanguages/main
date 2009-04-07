= dm-is-searchable

* http://datamapper.org
* git://github.com/sam/dm-more.git

== Description

A DataMapper search plugin api to search for resources from one repository and
load from another.

Typically a full text search adapter that can only produce partial resources is
searched and the resulting resource collection is then loaded from your default
repository.

== Synopsis

=== Resources

  require "rubygems"
  require "dm-core"
  require "dm-is-searchable"

  DataMapper.setup(:default, :adapter => 'in_memory')

  # Your search adapter configuration. See your search adapters setup options.
  DataMapper.setup(:search, :adapter => 'your_adapter')

  class Cow
    include DataMapper::Resource
    property :name, String, :key => true
    property :likes, String
    property :cowpats, Integer

    is :searchabe # This defaults to repository(:search), you could also do
    # is :searchable, :repository => :some_searchable_repository
  end

  class Chicken
    include DataMapper::Resource
    property :name,  String, :key => true
    property :likes, String
    property :eggs, Integer

    is :searchable
    repository(:search) do
      # We only want to be able to search by name.
      properties(:search).clear
      property :name, String
    end
  end

=== Searching

Mixing in is-searchable by default defines a single class method in your
resource with the signature of:

  # ==== Parameters
  # search_options<Hash>::
  #   DM::Query conditions to pass to the searchable repository. Unless you
  #   explicitly defined a searchable repository this is repository(:search).
  #
  # options<Hash>::
  #   Optionsal DM::Query conditions to pass to the repository holding the full
  #   resource. Without a scoped search this is repository(:default).
  #
  # ==== Returns
  # DM::Collection:: Zero or more DM::Resource objects.
  MyModel#search(search_options = {}, options = {})

A basic full text search for cows called 'Pete', 'Peter', 'Pedro' (dependant on
the search adapter) would look like:

  puts Cow.search(:name => 'pete').inspect
  #=> [<Cow name="peter" cowpats="12" ...>, <Cow name='pete' cowpats="1024" ...>, ...]

Adding extra conditions to apply to the default repository allows you to do
interesting things to the search adapter results. For example conditions on
properties not available in your search adapter or unsupported operators:

  # Unsupported #gt operator in search adapter.
  puts Cow.search({:name => 'pete'}, {:cowpats.gt => 1000}).inspect
  #=> [<Cow name="pete" cowpats="1024" ...>]

  # Unknown property in search adapter.
  puts Chicken.search({:name => 'steve'}, {:eggs => (100..200)}).inspect
  #=> [<Chicken name="steve" eggs="120" ...>]

=== Adapter

Like all DM adapters a custom search adapter implements the
DM::AbstractAdapter interface.

The key differences from a typical adapter are:

==== DM::AbstractAdapter#read_many

An Array of Hashes in the form of <tt>[{:id => 12}, {:id => 53}]</tt> will
work just fine. In fact so long as the following snippet would run on the
returned value you are free to return whatever you like.

  ids = read_many_result.collect{|doc| doc[:id]} #=> Array of ID's.

==== DM::AbstractAdapter#read_one

No need to DM::Model#load here. Just return your partial resource as a Hash
in the form of <tt>{:id => 12}</tt>. Like <tt>#read_many</tt> the returned
value really only needs to respond to <tt>#[:id]</tt>.

  read_one_result[:id] #=> The resource ID to load.


== Compatible Search Adapters

=== Ferret
Gem:: dm-ferret-adapter
Git:: git://github.com/sam/dm-more.git # dm-more/adapters/dm-ferret-adapter

=== Sphinx
Gem:: dm-sphinx-adapter
Git:: git://github.com/shanna/dm-sphinx-adapter.git
