module Merb
  class Worker
    
    # :api: private
    attr_accessor :thread
    
    class << self
      # ==== Returns
      # Merb::Worker:: instance of a worker.
      # 
      # :api: private
      def start
        @worker ||= new
        Merb.at_exit do 
          if Merb::Dispatcher.work_queue.empty?
            @worker.thread.abort_on_exception = false
            @worker.thread.raise
          else
            @worker.thread.join
          end
        end
        @worker
      end      
    end
    
    # Creates a new worker thread that loops over the work queue.
    # 
    # :api: private
    def initialize
      @thread = Thread.new do
        loop do
          process_queue
          break if Merb::Dispatcher.work_queue.empty? && Merb.exiting
        end
      end
    end
    
    # Processes tasks in the Merb::Dispatcher.work_queue.
    # 
    # :api: private
    def process_queue
      begin
        while blk = Merb::Dispatcher.work_queue.pop
           # we've been blocking on the queue waiting for an item sleeping.
           # when someone pushes an item it wakes up this thread so we 
           # immediately pass execution to the scheduler so we don't 
           # accidentally run this block before the action finishes 
           # it's own processing
          Thread.pass
          blk.call
          break if Merb::Dispatcher.work_queue.empty? && Merb.exiting
        end
      rescue Exception => e
        Merb.logger.warn! %Q!Worker Thread Crashed with Exception:\n#{Merb.exception(e)}\nRestarting Worker Thread!
        retry
      end
    end
    
  end
end
