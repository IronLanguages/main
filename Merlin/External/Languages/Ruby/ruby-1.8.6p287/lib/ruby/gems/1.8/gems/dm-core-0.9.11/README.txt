
:include:QUICKLINKS

= Why DataMapper?

== Open Development

DataMapper sports a very accessible code-base and a welcoming community.
Outside contributions and feedback are welcome and encouraged, especially
constructive criticism. Make your voice heard! Submit a
ticket[http://wm.lighthouseapp.com/projects/4819-datamapper/overview] or
patch[http://wm.lighthouseapp.com/projects/4819-datamapper/overview], speak up
on our mailing-list[http://groups.google.com/group/datamapper/], chat with us
on irc[irc://irc.freenode.net/#datamapper], write a spec, get it reviewed, ask
for commit rights. It's as easy as that to become a contributor.

== Identity Map

One row in the data-store should equal one object reference. Pretty simple idea.
Pretty profound impact. If you run the following code in ActiveRecord you'll
see all <tt>false</tt> results. Do the same in DataMapper and it's
<tt>true</tt> all the way down.

  @parent = Tree.find(:first, :conditions => ['name = ?', 'bob'])

  @parent.children.each do |child|
    puts @parent.object_id == child.parent.object_id
  end

This makes DataMapper faster and allocate less resources to get things done.

== Dirty Tracking

When you save a model back to your data-store, DataMapper will only write
the fields that actually changed. So it plays well with others. You can
use it in an Integration data-store without worrying that your application will
be a bad actor causing trouble for all of your other processes.

You can also configure which strategy you'd like to use to track dirtiness.

== Eager Loading

Ready for something amazing? The following example executes only two queries.

  zoos = Zoo.all
  first = zoos.first
  first.exhibits  # Loads the exhibits for all the Zoo objects in the zoos variable.

Pretty impressive huh? The idea is that you aren't going to load a set of
objects and use only an association in just one of them. This should hold up
pretty well against a 99% rule. When you don't want it to work like this, just
load the item you want in it's own set. So the DataMapper thinks ahead. We
like to call it "performant by default". This feature single-handedly wipes
out the "N+1 Query Problem". No need to specify an <tt>include</tt> option in
your finders.

== Laziness Can Be A Virtue

Text fields are expensive in data-stores. They're generally stored in a
different place than the rest of your data. So instead of a fast sequential
read from your hard-drive, your data-store server has to hop around all over the
place to get what it needs. Since ActiveRecord returns everything by default,
adding a text field to a table slows everything down drastically, across the
board.

Not so with the DataMapper. Text fields are treated like in-row associations
by default, meaning they only load when you need them. If you want more
control you can enable or disable this feature for any field (not just
text-fields) by passing a @lazy@ option to your field mapping with a value of
<tt>true</tt> or <tt>false</tt>.

  class Animal
    include DataMapper::Resource
    property :name, String
    property :notes, Text, :lazy => false
  end

Plus, lazy-loading of text fields happens automatically and intelligently when
working with associations.  The following only issues 2 queries to load up all
of the notes fields on each animal:

  animals = Animal.all
  animals.each do |pet|
    pet.notes
  end

== Plays Well With Others

In ActiveRecord, all your fields are mapped, whether you want them or not.
This slows things down. In the DataMapper you define your mappings in your
model. So instead of an _ALTER TABLE ADD field_ in your data-store, you simply
add a <tt>property :name, :string</tt> to your model. DRY. No schema.rb. No
migration files to conflict or die without reverting changes. Your model
drives the data-store, not the other way around.

Unless of course you want to map to a legacy data-store. Raise your hand if you
like seeing a method called <tt>col2Name</tt> on your model just because
that's what it's called in an old data-store you can't afford to change right
now? In DataMapper you control the mappings:

  class Fruit
    include DataMapper::Resource
    storage_names[:repo] = 'frt'
    property :name, String, :field => 'col2Name'
  end

== All Ruby, All The Time

It's great that ActiveRecord allows you to write SQL when you need to, but
should we have to so often?

DataMapper supports issuing your own query, but it also provides more helpers
and a unique hash-based condition syntax to cover more of the use-cases where
issuing your own SQL would have been the only way to go. For example, any
finder option that's non-standard is considered a condition. So you can write
<tt>Zoo.all(:name => 'Dallas')</tt> and DataMapper will look for zoos with the
name of 'Dallas'.

It's just a little thing, but it's so much nicer than writing
<tt>Zoo.find(:all, :conditions => ['name = ?', 'Dallas'])</tt>. What if you
need other comparisons though? Try these:

  Zoo.first(:name => 'Galveston')

  # 'gt' means greater-than. We also do 'lt'.
  Person.all(:age.gt => 30)

  # 'gte' means greather-than-or-equal-to. We also do 'lte'.
  Person.all(:age.gte => 30)

  Person.all(:name.not => 'bob')

  # If the value of a pair is an Array, we do an IN-clause for you.
  Person.all(:name.like => 'S%', :id => [1, 2, 3, 4, 5])

  # An alias for Zoo.find(11)
  Zoo[11]

  # Does a NOT IN () clause for you.
  Person.all(:name.not => ['bob','rick','steve'])

See? Fewer SQL fragments dirtying your Ruby code. And that's just a few of the
nice syntax tweaks DataMapper delivers out of the box...
