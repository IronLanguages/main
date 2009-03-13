module DataMapper
  module Is
    module NestedSet

      ##
      # docs in the works
      #
      def is_nested_set(options={})
        options = { :child_key => [:parent_id], :scope => [] }.merge(options)

        extend  DataMapper::Is::NestedSet::ClassMethods
        include DataMapper::Is::NestedSet::InstanceMethods

        @nested_set_scope = options[:scope]
        @nested_set_parent = options[:child_key]

        property :lft, Integer, :writer => :private
        property :rgt, Integer, :writer => :private

        # a temporary fix. I need to filter. now I just use parent.children in self_and_siblings, which could
        # be cut down to 1 instead of 2 queries. this would be the other way, but seems hackish:
        # options[:child_key].each{|pname| property(pname, Integer) unless properties.detect{|p| p.name == pname}}

        belongs_to :parent,   :class_name => self.name, :child_key => options[:child_key], :order => [:lft.asc]
        has n,     :children, :class_name => self.name, :child_key => options[:child_key], :order => [:lft.asc]

        before :create do
          if !self.parent
            # TODO must change for nested sets
            self.root ? self.move_without_saving(:into => self.root) : self.move_without_saving(:to => 1)
          elsif self.parent && !self.lft
            self.move_without_saving(:into => self.parent)
          end
        end

        before :update do
          if self.nested_set_scope != self.original_nested_set_scope
            # TODO detach from old list first. many edge-cases here, need good testing
            self.lft,self.rgt = nil,nil
            #puts "#{self.root.inspect} - #{[self.nested_set_scope,self.original_nested_set_scope].inspect}"
            self.root ? self.move_without_saving(:into => self.root) : self.move_without_saving(:to => 1)
          elsif (self.parent && !self.lft) || (self.parent != self.ancestor)
            # if the parent is set, we try to move this into that parent, otherwise move into root.
            self.parent ? self.move_without_saving(:into => self.parent) : self.move_without_saving(:into => self.class.root)
          end
        end

        before :destroy do
          self.send(:detach)
        end

        after_class_method :inherited do |retval,target|
          target.instance_variable_set(:@nested_set_scope, @nested_set_scope.dup)
          target.instance_variable_set(:@nested_set_parent, @nested_set_parent.dup)
        end

      end

      module ClassMethods
        attr_reader :nested_set_scope, :nested_set_parent

        def adjust_gap!(scoped_set,at,adjustment)
          scoped_set.all(:rgt.gt => at).adjust!({:rgt => adjustment},true)
          scoped_set.all(:lft.gt => at).adjust!({:lft => adjustment},true)
        end

        ##
        # get the root of the tree. if sets are scoped, this will return false
        #
        def root
          # TODO scoping
          # what should this return if there is a scope? always false, or node if there is only one?
          roots.length > 1 ? false : first(nested_set_parent.zip([]).to_hash)
        end

        ##
        # not implemented
        #
        def roots
          # TODO scoping
          # TODO supply filtering-option?
          all(nested_set_parent.zip([]).to_hash)
        end

        ##
        #
        #
        def leaves
          # TODO scoping, how should it act?
          # TODO supply filtering-option?
          all(:conditions => ["rgt=lft+1"], :order => [:lft.asc])
        end

        ##
        # rebuilds the parent/child relationships (parent_id) from nested set (left/right values)
        #
        def rebuild_tree_from_set
          all.each do |node|
            node.parent = node.ancestor
            node.save
          end
        end

        ##
        # rebuilds the nested set using parent/child relationships and a chosen order
        #
        def rebuild_set_from_tree(order=nil)
          # TODO pending
        end
      end

      module InstanceMethods

        ##
        #
        # @private
        def nested_set_scope
          self.model.base_model.nested_set_scope.map{|p| [p,attribute_get(p)]}.to_hash
        end

        ##
        #
        # @private
        def original_nested_set_scope
          # TODO commit
          self.model.base_model.nested_set_scope.map{|p| [p, original_values.key?(p) ? original_values[p] : attribute_get(p)]}.to_hash
        end

        ##
        # the whole nested set this node belongs to. served flat like a pancake!
        #
        def nested_set
          # TODO add option for serving it as a nested array
          self.model.base_model.all(nested_set_scope.merge(:order => [:lft.asc]))
        end

        ##
        # move self / node to a position in the set. position can _only_ be changed through this
        #
        # @example [Usage]
        #   * node.move :higher           # moves node higher unless it is at the top of parent
        #   * node.move :lower            # moves node lower unless it is at the bottom of parent
        #   * node.move :below => other   # moves this node below other resource in the set
        #   * node.move :into => other    # same as setting a parent-relationship
        #
        # @param vector <Symbol, Hash> A symbol, or a key-value pair that describes the requested movement
        #
        # @option :higher<Symbol> move node higher
        # @option :highest<Symbol> move node to the top of the list (within its parent)
        # @option :lower<Symbol> move node lower
        # @option :lowest<Symbol> move node to the bottom of the list (within its parent)
        # @option :indent<Symbol> move node into sibling above
        # @option :outdent<Symbol> move node out below its current parent
        # @option :into<Resource> move node into another node
        # @option :above<Resource> move node above other node
        # @option :below<Resource> move node below other node
        # @option :to<Integer> move node to a specific location in the nested set
        #
        # @return <FalseClass> returns false if it cannot move to the position, or if it is already there
        # @raise <RecursiveNestingError> if node is asked to position itself into one of its descendants
        # @raise <UnableToPositionError> if node is unable to calculate a new position for the element
        # @see move_without_saving
        def move(vector)
          move_without_saving(vector) && save
        end

        ##
        # @see move
        def move_without_saving(vector)
          if vector.is_a? Hash then action,object = vector.keys[0],vector.values[0] else action = vector end

          changed_scope = nested_set_scope != original_nested_set_scope

          position = case action
            when :higher  then left_sibling  ? left_sibling.lft    : nil # : "already at the top"
            when :highest then ancestor      ? ancestor.lft+1      : nil # : "is root, or has no parent"
            when :lower   then right_sibling ? right_sibling.rgt+1 : nil # : "already at the bottom"
            when :lowest  then ancestor      ? ancestor.rgt        : nil # : "is root, or has no parent"
            when :indent  then left_sibling  ? left_sibling.rgt    : nil # : "cannot find a sibling to indent into"
            when :outdent then ancestor      ? ancestor.rgt+1      : nil # : "is root, or has no parent"
            when :into    then object        ? object.rgt          : nil # : "supply an object"
            when :above   then object        ? object.lft          : nil # : "supply an object"
            when :below   then object        ? object.rgt+1        : nil # : "supply an object"
            when :to      then object        ? object.to_i         : nil # : "supply a number"
          end

          ##
          # raising an error whenever it couldnt move seems a bit harsh. want to return self for nesting.
          # if anyone has a good idea about how it should react when it cant set a valid position,
          # don't hesitate to find me in #datamapper, or send me an email at sindre -a- identu -dot- no
          #
          # raise UnableToPositionError unless position.is_a?(Integer) && position > 0
          return false if !position || position < 1
          # return false if you are trying to move this into another scope
          return false if [:into, :above,:below].include?(action) && nested_set_scope != object.nested_set_scope
          # if node is already in the requested position
          if self.lft == position || self.rgt == position - 1
            self.parent = self.ancestor # must set this again, because it might have been changed by the user before move.
            return false
          end


          DataMapper::Transaction.new(self.repository) do |transaction|

            ##
            # if this node is already positioned we need to move it, and close the gap it leaves behind etc
            # otherwise we only need to open a gap in the set, and smash that buggar in
            #
            if self.lft && self.rgt
              # raise exception if node is trying to move into one of its descendants (infinate loop, spacetime will warp)
              raise RecursiveNestingError if position > self.lft && position < self.rgt
              # find out how wide this node is, as we need to make a gap large enough for it to fit in
              gap = self.rgt - self.lft + 1

              # make a gap at position, that is as wide as this node
              self.model.base_model.adjust_gap!(nested_set,position-1,gap)

              # offset this node (and all its descendants) to the right position
              self.reload_attributes(:lft,:rgt)
              old_position = self.lft
              offset = position - old_position

              nested_set.all(:rgt => self.lft..self.rgt).adjust!({:lft => offset, :rgt => offset},true)
              # close the gap this movement left behind.
              self.model.base_model.adjust_gap!(nested_set,old_position,-gap)
              self.reload_attributes(:lft,:rgt)
            else
              # make a gap where the new node can be inserted
              self.model.base_model.adjust_gap!(nested_set,position-1,2)
              # set the position fields
              self.lft, self.rgt = position, position + 1
            end
            self.parent = self.ancestor
          end
        end

        ##
        # get the level of this node, where 0 is root. temporary solution
        #
        # @return <Integer>
        def level
          # TODO make a level-property that is cached and intelligently adjusted when saving objects
          ancestors.length
        end

        ##
        # get all ancestors of this node, up to (and including) self
        #
        # @return <Collection>
        def self_and_ancestors
          nested_set.all(:lft.lte => lft, :rgt.gte => rgt)
        end

        ##
        # get all ancestors of this node
        #
        # @return <Collection> collection of all parents, with root as first item
        # @see #self_and_ancestors
        def ancestors
          nested_set.all(:lft.lt => lft, :rgt.gt => rgt)
          #self_and_ancestors.reject{|r| r.key == self.key } # because identitymap is not used in console
        end

        ##
        # get the parent of this node. Same as #parent, but finds it from lft/rgt instead of parent-key
        #
        # @return <Resource, NilClass> returns the parent-object, or nil if this is root/detached
        def ancestor
          ancestors.reverse.first
        end

        ##
        # get the root this node belongs to. this will atm always be the same as Resource.root, but has a
        # meaning when scoped sets is implemented
        #
        # @return <Resource, NilClass>
        def root
          nested_set.first
        end

        ##
        # check if this node is a root
        #
        def root?
          !parent && !new_record?
        end

        ##
        # get all descendants of this node, including self
        #
        # @return <Collection> flat collection, sorted according to nested_set positions
        def self_and_descendants
          # TODO supply filtering-option?
          nested_set.all(:lft => lft..rgt)
        end

        ##
        # get all descendants of this node
        #
        # @return <Collection> flat collection, sorted according to nested_set positions
        # @see #self_and_descendants
        def descendants
          # TODO add argument for returning as a nested array.
          # TODO supply filtering-option?
          nested_set.all(:lft => (lft+1)..(rgt-1))
        end

        ##
        # get all descendants of this node that does not have any children
        #
        # @return <Collection>
        def leaves
          # TODO supply filtering-option?
          nested_set.all(:lft => (lft+1)..rgt, :conditions=>["rgt=lft+1"])
        end

        ##
        # check if this node is a leaf (does not have subnodes).
        # use this instead ofdescendants.empty?
        #
        # @par
        def leaf?
          rgt-lft == 1
        end

        ##
        # get all siblings of this node, and include self
        #
        # @return <Collection>
        def self_and_siblings
          parent ? parent.children : [self]
        end

        ##
        # get all siblings of this node
        #
        # @return <Collection>
        # @see #self_and_siblings
        def siblings
          # TODO find a way to return this as a collection?
          # TODO supply filtering-option?
          self_and_siblings.reject{|r| r.key == self.key } # because identitymap is not used in console
        end

        ##
        # get sibling to the left of/above this node in the nested tree
        #
        # @return <Resource, NilClass> the resource to the left, or nil if self is leftmost
        # @see #self_and_siblings
        def left_sibling
          self_and_siblings.find{|v| v.rgt == lft-1}
        end

        ##
        # get sibling to the right of/above this node in the nested tree
        #
        # @return <Resource, NilClass> the resource to the right, or nil if self is rightmost
        # @see #self_and_siblings
        def right_sibling
          self_and_siblings.find{|v| v.lft == rgt+1}
        end

       private
        def detach
          offset = self.lft - self.rgt - 1
          self.model.base_model.adjust_gap!(nested_set,self.rgt,offset)
          self.lft,self.rgt = nil,nil
        end

      end

      class UnableToPositionError < StandardError; end
      class RecursiveNestingError < StandardError; end

      Model.send(:include, self)
    end # NestedSet
  end # Is
end # DataMapper
