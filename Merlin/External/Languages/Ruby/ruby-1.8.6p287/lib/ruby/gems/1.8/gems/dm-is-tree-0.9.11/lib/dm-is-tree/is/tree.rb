module DataMapper
  module Is
    module Tree
      def self.included(base)
        base.extend(ClassMethods)
      end

      # An extension to DataMapper to easily allow the creation of tree
      # structures from your DataMapper models.
      # This requires a foreign key property for your model, which by default
      # would be called :parent_id.
      #
      #   Example:
      #
      #   class Category
      #     include DataMapper::Resource
      #
      #     property :id, Integer
      #     property :parent_id, Integer
      #     property :name, String
      #
      #     is :tree, :order => :name
      #   end
      #
      #    root
      #      +- child
      #          +- grandchild1
      #          +- grandchild2
      #
      #   root = Category.create(:name => "root")
      #   child = root.children.create(:name => "child")
      #   grandchild1 = child1.children.create(:name => "grandchild1")
      #   grandchild2 = child2.children.create(:name => "grandchild2")
      #
      #   root.parent  # => nil
      #   child.parent  # => root
      #   root.children  # => [child]
      #   root.children.first.children.first  # => grandchild1
      #   Category.first_root  # => root
      #   Category.roots  # => [root]
      #
      # The following instance methods are added:
      # * <tt>children</tt> - Returns all nodes with the current node as their parent, in the order specified by
      #   <tt>:order</tt> (<tt>[grandchild1, grandchild2]</tt> when called on <tt>child</tt>)
      # * <tt>parent</tt> - Returns the node referenced by the foreign key (<tt>:parent_id</tt> by
      #   default) (<tt>root</tt> when called on <tt>child</tt>)
      # * <tt>siblings</tt> - Returns all the children of the parent, excluding the current node
      #   (<tt>[grandchild2]</tt> when called on <tt>grandchild1</tt>)
      # * <tt>generation</tt> - Returns all the children of the parent, including the current node (<tt>
      #   [grandchild1, grandchild2]</tt> when called on <tt>grandchild1</tt>)
      # * <tt>ancestors</tt> - Returns all the ancestors of the current node (<tt>[root, child1]</tt>
      #   when called on <tt>grandchild2</tt>)
      # * <tt>root</tt> - Returns the root of the current node (<tt>root</tt> when called on <tt>grandchild2</tt>)
      #
      # Original Author:: Timothy Bennett (http://lanaer.com)
      # Current Maintainer:: Garrett Heaver (http://www.linkedin.com/pub/dir/garrett/heaver)

      # Configuration options are:
      #
      # * <tt>child_key</tt> - specifies the column name to use for tracking of the tree (default: +parent_id+)
      def is_tree(options = {})
        options = { :class_name => name, :child_key => :parent_id }.merge(options) if Hash === options
        @tree_options = options

        include DataMapper::Is::Tree::InstanceMethods
        extend  DataMapper::Is::Tree::ClassMethods

        assc_options = { :class_name => options[:class_name], :child_key => Array(options[:child_key]) }
        has_n_options = options[:order] ? { :order => Array(options[:order]) }.merge(assc_options) : assc_options

        belongs_to :parent, assc_options
        has n, :children, has_n_options

        class << self
          alias_method :root, :first_root # for people used to the ActiveRecord acts_as_tree
        end
      end

      def is_a_tree(options = {})
        warn('#is_a_tree is depreciated. use #is :tree instead.')
        is :tree, options
      end
      alias_method :can_has_tree, :is_tree # just for fun ;)

      module ClassMethods
        attr_reader :tree_options

        def roots
          options = { tree_options[:child_key] => nil }
          options = { :order => Array(tree_options[:order]) }.merge(options) if tree_options[:order]
          all options
        end

        def first_root
          options = { tree_options[:child_key] => nil }
          options = { :order => Array(tree_options[:order]) }.merge(options) if tree_options[:order]
          first options
        end
      end

      module InstanceMethods

        # Returns list of ancestors, starting with the root.
        #
        #   grandchild1.ancestors # => [root, child]
        def ancestors
          node, nodes = self, []
          nodes << node = node.parent while node.parent
          nodes.reverse
        end

        # Returns the root node of the current node’s tree.
        #
        #   grandchild1.root # => root
        def root
          node = self
          node = node.parent while node.parent
          node
        end
        alias_method :first_root, :root

        # Returns all siblings of the current node.
        #
        #   grandchild1.siblings # => [grandchild2]
        def siblings
          generation - [self]
        end

        # Returns all children of the current node’s parent.
        #
        #   grandchild1.generation # => [grandchild1, grandchild2]
        def generation
          parent ? parent.children : self.class.roots
        end
        alias_method :self_and_siblings, :generation # for those used to the ActiveRecord acts_as_tree

      end

      Model.send(:include, self)
    end # Tree
  end # Is
end # DataMapper
