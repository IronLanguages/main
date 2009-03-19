# TODO: move to dm-more/dm-transactions

module DataMapper
  class Transaction

    attr_reader :transaction_primitives, :adapters, :state

    #
    # Create a new DataMapper::Transaction
    #
    # @see DataMapper::Transaction#link
    #
    # In fact, it just calls #link with the given arguments at the end of the
    # constructor.
    #
    def initialize(*things)
      @transaction_primitives = {}
      @state = :none
      @adapters = {}
      link(*things)
      commit { |*block_args| yield(*block_args) } if block_given?
    end

    #
    # Associate this Transaction with some things.
    #
    # @param things<any number of Object>  the things you want this Transaction
    #   associated with
    # @details [things a Transaction may be associatied with]
    #   DataMapper::Adapters::AbstractAdapter subclasses will be added as
    #     adapters as is.
    #   Arrays will have their elements added.
    #   DataMapper::Repositories will have their @adapters added.
    #   DataMapper::Resource subclasses will have all the repositories of all
    #     their properties added.
    #   DataMapper::Resource instances will have all repositories of all their
    #     properties added.
    # @param block<Block> a block (taking one argument, the Transaction) to execute
    #   within this transaction. The transaction will begin and commit around
    #   the block, and rollback if an exception is raised.
    #
    def link(*things)
      raise "Illegal state for link: #{@state}" unless @state == :none
      things.each do |thing|
        if thing.is_a?(Array)
          link(*thing)
        elsif thing.is_a?(DataMapper::Adapters::AbstractAdapter)
          @adapters[thing] = :none
        elsif thing.is_a?(DataMapper::Repository)
          link(thing.adapter)
        elsif thing.is_a?(Class) && thing.ancestors.include?(DataMapper::Resource)
          link(*thing.repositories)
        elsif thing.is_a?(DataMapper::Resource)
          link(thing.model)
        else
          raise "Unknown argument to #{self}#link: #{thing.inspect}"
        end
      end
      return commit { |*block_args| yield(*block_args) } if block_given?
      return self
    end

    #
    # Begin the transaction
    #
    # Before #begin is called, the transaction is not valid and can not be used.
    #
    def begin
      raise "Illegal state for begin: #{@state}" unless @state == :none
      each_adapter(:connect_adapter, [:log_fatal_transaction_breakage])
      each_adapter(:begin_adapter, [:rollback_and_close_adapter_if_begin, :close_adapter_if_none])
      @state = :begin
    end

    #
    # Commit the transaction
    #
    # @param block<Block>   a block (taking the one argument, the Transaction) to
    #   execute within this transaction. The transaction will begin and commit
    #   around the block, and roll back if an exception is raised.
    #
    # @note
    #   If no block is given, it will simply commit any changes made since the
    #   Transaction did #begin.
    #
    def commit
      if block_given?
        raise "Illegal state for commit with block: #{@state}" unless @state == :none
        begin
          self.begin
          rval = within { |*block_args| yield(*block_args) }
          self.commit if @state == :begin
          return rval
        rescue Exception => e
          self.rollback if @state == :begin
          raise e
        end
      else
        raise "Illegal state for commit without block: #{@state}" unless @state == :begin
        each_adapter(:prepare_adapter, [:rollback_and_close_adapter_if_begin, :rollback_prepared_and_close_adapter_if_prepare])
        each_adapter(:commit_adapter, [:log_fatal_transaction_breakage])
        each_adapter(:close_adapter, [:log_fatal_transaction_breakage])
        @state = :commit
      end
    end

    #
    # Rollback the transaction
    #
    # Will undo all changes made during the transaction.
    #
    def rollback
      raise "Illegal state for rollback: #{@state}" unless @state == :begin
      each_adapter(:rollback_adapter_if_begin, [:rollback_and_close_adapter_if_begin, :close_adapter_if_none])
      each_adapter(:rollback_prepared_adapter_if_prepare, [:rollback_prepared_and_close_adapter_if_begin, :close_adapter_if_none])
      each_adapter(:close_adapter_if_open, [:log_fatal_transaction_breakage])
      @state = :rollback
    end

    #
    # Execute a block within this Transaction.
    #
    # @param block<Block> the block of code to execute.
    #
    # @note
    #   No #begin, #commit or #rollback is performed in #within, but this
    #   Transaction will pushed on the per thread stack of transactions for each
    #   adapter it is associated with, and it will ensures that it will pop the
    #   Transaction away again after the block is finished.
    #
    def within
      raise "No block provided" unless block_given?
      raise "Illegal state for within: #{@state}" unless @state == :begin
      @adapters.each do |adapter, state|
        adapter.push_transaction(self)
      end
      begin
        return yield(self)
      ensure
        @adapters.each do |adapter, state|
          adapter.pop_transaction
        end
      end
    end

    def method_missing(meth, *args, &block)
      if args.size == 1 && args.first.is_a?(DataMapper::Adapters::AbstractAdapter)
        if (match = meth.to_s.match(/^(.*)_if_(none|begin|prepare|rollback|commit)$/))
          if self.respond_to?(match[1], true)
            self.send(match[1], args.first) if state_for(args.first).to_s == match[2]
          else
            super
          end
        elsif (match = meth.to_s.match(/^(.*)_unless_(none|begin|prepare|rollback|commit)$/))
          if self.respond_to?(match[1], true)
            self.send(match[1], args.first) unless state_for(args.first).to_s == match[2]
          else
            super
          end
        else
          super
        end
      else
        super
      end
    end

    def primitive_for(adapter)
      raise "Unknown adapter #{adapter}" unless @adapters.include?(adapter)
      raise "No primitive for #{adapter}" unless @transaction_primitives.include?(adapter)
      @transaction_primitives[adapter]
    end

    private

    def validate_primitive(primitive)
      [:close, :begin, :prepare, :rollback, :rollback_prepared, :commit].each do |meth|
        raise "Invalid primitive #{primitive}: doesnt respond_to?(#{meth.inspect})" unless primitive.respond_to?(meth)
      end
      return primitive
    end

    def each_adapter(method, on_fail)
      begin
        @adapters.each do |adapter, state|
          self.send(method, adapter)
        end
      rescue Exception => e
        @adapters.each do |adapter, state|
          on_fail.each do |fail_handler|
            begin
              self.send(fail_handler, adapter)
            rescue Exception => e2
              DataMapper.logger.fatal("#{self}#each_adapter(#{method.inspect}, #{on_fail.inspect}) failed with #{e.inspect}: #{e.backtrace.join("\n")} - and when sending #{fail_handler} to #{adapter} we failed again with #{e2.inspect}: #{e2.backtrace.join("\n")}")
            end
          end
        end
        raise e
      end
    end

    def state_for(adapter)
      raise "Unknown adapter #{adapter}" unless @adapters.include?(adapter)
      @adapters[adapter]
    end

    def do_adapter(adapter, what, prerequisite)
      raise "No primitive for #{adapter}" unless @transaction_primitives.include?(adapter)
      raise "Illegal state for #{what}: #{state_for(adapter)}" unless state_for(adapter) == prerequisite
      DataMapper.logger.debug("#{adapter.name}: #{what}")
      @transaction_primitives[adapter].send(what)
      @adapters[adapter] = what
    end

    def log_fatal_transaction_breakage(adapter)
      DataMapper.logger.fatal("#{self} experienced a totally broken transaction execution. Presenting member #{adapter.inspect}.")
    end

    def connect_adapter(adapter)
      raise "Already a primitive for adapter #{adapter}" unless @transaction_primitives[adapter].nil?
      @transaction_primitives[adapter] = validate_primitive(adapter.transaction_primitive)
    end

    def close_adapter_if_open(adapter)
      if @transaction_primitives.include?(adapter)
        close_adapter(adapter)
      end
    end

    def close_adapter(adapter)
      raise "No primitive for adapter" unless @transaction_primitives.include?(adapter)
      @transaction_primitives[adapter].close
      @transaction_primitives.delete(adapter)
    end

    def begin_adapter(adapter)
      do_adapter(adapter, :begin, :none)
    end

    def prepare_adapter(adapter)
      do_adapter(adapter, :prepare, :begin);
    end

    def commit_adapter(adapter)
      do_adapter(adapter, :commit, :prepare)
    end

    def rollback_adapter(adapter)
      do_adapter(adapter, :rollback, :begin)
    end

    def rollback_prepared_adapter(adapter)
      do_adapter(adapter, :rollback_prepared, :prepare)
    end

    def rollback_prepared_and_close_adapter(adapter)
      rollback_prepared_adapter(adapter)
      close_adapter(adapter)
    end

    def rollback_and_close_adapter(adapter)
      rollback_adapter(adapter)
      close_adapter(adapter)
    end

  end # class Transaction
end # module DataMapper
