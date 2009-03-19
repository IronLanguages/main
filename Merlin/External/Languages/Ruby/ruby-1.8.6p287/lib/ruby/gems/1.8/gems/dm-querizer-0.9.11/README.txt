= dm-querizer

DataMapper plugin that provides a short rubyish query-syntax.

When this plugin is loaded you can supply blocks to #all and #first. Ordinary hashes
still work, so you do not lose any functionality.

Old: User.all( :name => 'john', :age.gt => 20 )
New: User.all{ name == 'john' && age > 20 }

Old: User.all( :age.gte => 35, :name.like => 'mark%' )
New: User.all{ age >= 35 && name =~ 'mark%' }

You can also use ';' instead of '&&' for even shorter queries.

User.all{ name == 'john' && age > 20 }
User.all{ name == 'john'; age > 20 }

The plugin is still very much experimental. != is not working (and might never work).
Instead you can use '~';

Old: User.all( :name.not => 'mark' )
New: User.all{ name ~ 'mark' }
