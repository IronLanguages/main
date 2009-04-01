= dm-is-tree

DataMapper plugin allowing the creation of tree structures from data models.

Example:

class Category
  include DataMapper::Resource

  property :id, Serial
  property :parent_id, Integer
  property :name, String

  is :tree, :order => :name
end

root
  +- child
    +- grandchild1
    +- grandchild2

root = Category.create(:name => "root")
child = root.children.create(:name => "child")
grandchild1 = child1.children.create(:name => "grandchild1")
grandchild2 = child2.children.create(:name => "grandchild2")

root.parent  # => nil
child.parent  # => root
root.children  # => [child]
root.children.first.children.first  # => grandchild1
Category.first_root  # => root
Category.roots  # => [root]

Original Author:: Timothy Bennett (http://lanaer.com)
Current Maintainer:: Garrett Heaver (http://www.linkedin.com/pub/dir/garrett/heaver)
