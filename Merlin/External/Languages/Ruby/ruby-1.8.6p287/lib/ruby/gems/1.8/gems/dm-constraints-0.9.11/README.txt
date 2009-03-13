= dm-constraints

Plugin that adds foreign key constraints to associations.
Currently supports only PostgreSQL and MySQL

All constraints are added to the underlying database, but constraining is implemented in
pure ruby.


=== Constraints

 - :protect     returns false on destroy if there are child records
 - :destroy     deletes children if present
 - :destroy!    deletes children directly without instantiating the resource, bypassing any hooks
                Does not support 1:1 Relationships as #destroy! is not supported on Resource in dm-master
 - :set_nil     sets parent id to nil in child associations
                Not valid for M:M relationships as duplicate records could be created (see explanation in specs)
 - :skip        Does nothing with children, results in orphaned records

By default a relationship will PROTECT its children.


=== Cardinality Notes
 * 1:1
  * Applicable constraints: [:set_nil, :skip, :protect, :destroy]

 * 1:M
  * Applicable constraints: [:set_nil, :skip, :protect, :destroy, :destroy!]

 * M:M
  * Applicable constraints: [:skip, :protect, :destroy, :destroy!]


=== Examples

# 1:M Example
class Farmer
  has n, :pigs #equivalent to: has n, :pigs, :constraint => :protect
end

# M:M Example
class Articles
  has n, :tags, :through => Resource, :constraint => :destroy
end
class Tags
  has n, :articles, :through => Resource, :constraint => :destroy
end

# 1:1 Example
class Farmer
  has 1, :beloved_sheep, :constraint => :protect
end
