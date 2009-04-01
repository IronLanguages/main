= Quick Introduction to Ferret

The simplest way to use Ferret is through the Ferret::Index::Index class.
This is now aliased by Ferret::I for quick and easy access. Start by including
the Ferret module.

  require 'ferret'
  include Ferret

=== Creating an index

To create an in memory index is very simple;
  
  index = Index::Index.new()

To create a persistent index;

  index = Index::Index.new(:path => '/path/to/index')

Both of these methods create new Indexes with the StandardAnalyzer. An
analyzer is what you use to divide the input data up into tokens which you can
search for later. If you'd like to use a different analyzer you can specify it
here, eg;

  index = Index::Index.new(:path => '/path/to/index',
                           :analyzer => Analysis::WhiteSpaceAnalyzer.new)

For more options when creating an Index refer to Ferret::Index::Index.

=== Adding Documents

To add a document you can simply add a string or an array of strings. This will
store all the strings in the "" (ie empty string) field (unless you specify the
default field when you create the index).

  index << "This is a new document to be indexed"
  index << ["And here", "is another", "new document", "to be indexed"]

But these are pretty simple documents. If this is all you want to index you
could probably just use SimpleSearch. So let's give our documents some fields;

  index << {:title => "Programming Ruby", :content => "blah blah blah"}
  index << {:title => "Programming Ruby", :content => "yada yada yada"}

Note the way that all field-names are Symbols. Although Strings will work, 
this is a best-practice in Ferret. Or if you are indexing data stored in a
database, you'll probably want to store the id;

  index << {:id => row.id, :title => row.title, :date => row.date}

So far we have been storing and tokenizing all of the input data along with
term vectors. If we want to change this we need to change the way we setup the
index. You must create a FieldInfos object describing the index:

  field_infos = FieldInfos.new(:store => :no,
                               :index => :untokenized_omit_norms,
                               :term_vector => :no)

The values that you set FieldInfos to have will be used by default by all
fields. If you want to change the properties for specific fields, you need to
add a FieldInfo to field_infos.

  field_infos.add_field(:title, :store => :yes, :index => :yes, :boost => 10.0)
  field_infos.add_field(:content, :store => :yes,
                                  :index => :yes,
                                  :term_vector => :with_positions_offsets)

If you need to add a field to an already open index you do so like this:

  index.field_infos.add_field(:new_field, :store => :yes)

=== Searching

Now that we have data in our index, how do we actually use this index to
search the data? The Index offers two search methods, Index#search and
Index#search_each. The first method returns a Ferret::Index::TopDocs object.
The second we'll show here. Lets say we wanted to find all documents with the
phrase "quick brown fox" in the content field. We'd write;

  index.search_each('content:"quick brown fox"') do |id, score|
    puts "Document #{id} found with a score of #{score}"
  end

But "fast" has a pretty similar meaning to "quick" and we don't mind if the
fox is a little red. Also, the phrase could be in the title so we'll search
there as well. So we could expand our search like this;

  index.search_each('title|content:"quick|fast brown|red fox"') do |id, score|
    puts "Document #{id} found with a score of #{score}"
  end

What if we want to find all documents entered on or after 5th of September,
2005 with the words "ruby" or "rails" in any field. We could type something like;

  index.search_each('date:( >= 20050905) *:(ruby OR rails)') do |id, score|
    puts "Document #{index[id][:title]} found with a score of #{score}"
  end

Ferret has quite a complex query language. To find out more about Ferret's
query language, see Ferret::QueryParser. You can also construct even more
complex queries like Ferret::Search::Spans by hand. See Ferret::Search::Query
for more information.

=== Highlighting

Ferret now has a super-fast highlighting method. See
Ferret::Index::Index#highlight. Here is an example of how you would use it
when printing to the console:

  index.search_each('date:( >= 20050905) content:(ruby OR rails)') do |id, score|
    puts "Document #{index[id][:title]} found with a score of #{score}"
    highlights = index.highlight("content:(ruby OR rails)", 0,
                                 :field => :content,
                                 :pre_tag = "\033[36m",
                                 :post_tag = "\033[m")
    puts highlights
  end

And if you want to highlight a whole document, set :excerpt_length to :all:

  puts index.highlight(query, doc_id,
                       :field => :content,
                       :pre_tag = "\033[36m",
                       :post_tag = "\033[m",
                       :excerpt_length => :all)

=== Accessing Documents

You may have noticed that when we run a search we only get the document id
back. By itself this isn't much use to us. Getting the data from the index is
very straightforward. For example if we want the :title field form the 3rd
document type;
  
  index[2][:title]

Documents are lazy loading so if you try this:

  puts index[2]

You will always get an empty Hash. To load all fields, call the load method:

  puts index[2].load

NOTE: documents are indexed from 0. You can also use array-like index
parameters to access index. For example

  index[1..4]
  index[10, 10]
  index[-5]

The default field is :id (although you can change this with index's
:default_create_field parameter);

  index << "This is a document"
  index[0][:id]

Let's go back to the database example above. If we store all of our documents
with an id then we can access that field using the id. As long as we called
our id field :id we can do this

  index["89721347"]["title"]

Pretty simple huh? You should note though that if there are more then one
document with the same *id* or *key* then only the first one will be returned
so it is probably better that you ensure the key is unique somehow. By setting
Index's :key attribute to :id, Ferret will do this automatically for you. It
can even handle multiple field primary keys. For example, you could set to
:key to [:id, :model] and Ferret would keep the documents unique for that pair
of fields.

=== Modifying and Deleting Documents

What if we want to change the data in the index. Ferret doesn't actually let
you change the data once it is in the index. But you can delete documents so
the standard way to modify data is to delete it and re-add it again with the
modifications made. It is important to note that when doing this the documents
will get a new document number so you should be careful not to use a document
number after the document has been deleted. Here is an example of modifying a
document;

  index << {:title => "Programing Rbuy", :content => "blah blah blah"}
  doc_num = nil
  index.search_each('title:"Programing Rbuy"') {|id, score| doc_id = id}
  return unless doc_id
  doc = index[doc_id]
  index.delete(doc_id)

  # modify doc. It is just a Hash after all
  doc[:title] = "Programming Ruby"

  index << doc

If you set the :key parameter as described in the last section there is no
need to delete the document. It will be automatically deleted when you add
another document with the same key.

Also, we can use the id field, as above, to delete documents. This time though
every document that matches the id will be deleted. Again, it is probably a
good idea if you somehow ensure that your *ids* are kept unique.

  id = "23453422"
  index.delete(id)

=== Onwards

This is just a small sampling of what Ferret allows you to do.  Ferret, like
Lucene, is designed to be extended, and allows you to construct your own query
types, analyzers, and so on. Going onwards you should check out the following
documentation:

* Ferret::Analysis: for more information on how the data is processed when it
  is tokenized. There are a number of things you can do with your data such as
  adding stop lists or perhaps a porter stemmer. There are also a number of
  analyzers already available and it is almost trivial to create a new one
  with a simple regular expression.

* Ferret::Search: for more information on querying the index. There are a
  number of already available queries and it's unlikely you'll need to create
  your own. You may however want to take advantage of the sorting or filtering
  abilities of Ferret to present your data the best way you see fit.

* Ferret::QueryParser: if you want to find out more about what you can do with
  Ferret's Query Parser, this is the place to look. The query parser is one
  area that could use a bit of work so please send your suggestions.

* Ferret::Index: for more advanced access to the index you'll probably want to
  use the Ferret::Index::IndexWriter and Ferret::Index::IndexReader. This is
  the place to look for more information on them.

* Ferret::Store: This is the module used to access the actual index storage
  and won't be of much interest to most people.
