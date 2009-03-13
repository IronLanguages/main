module Merb::Cache
  # Store well suited for action caching.
  class ActionStore < AbstractStrategyStore
    def writable?(dispatch, parameters = {}, conditions = {})
      case dispatch
      when Merb::Controller
        @stores.any?{|s| s.writable?(normalize(dispatch), parameters, conditions)}
      else false
      end
    end

    def read(dispatch, parameters = {})
      if writable?(dispatch, parameters)
        @stores.capture_first {|s| s.read(normalize(dispatch), parameters)}
      end
    end

    def write(dispatch, data = nil, parameters = {}, conditions = {})
      if writable?(dispatch, parameters, conditions)
        @stores.capture_first {|s| s.write(normalize(dispatch), data || dispatch.body, parameters, conditions)}
      end
    end

    def write_all(dispatch, data = nil, parameters = {}, conditions = {})
      if writable?(dispatch, parameters, conditions)
        @stores.map {|s| s.write_all(normalize(dispatch), data || dispatch.body, parameters, conditions)}.all?
      end
    end

    def fetch(dispatch, parameters = {}, conditions = {}, &blk)
      if writable?(dispatch, parameters, conditions)
        read(dispatch, parameters) || @stores.capture_first {|s| s.fetch(normalize(dispatch), data || dispatch.body, parameters, conditions, &blk)}
      end
    end

    def exists?(dispatch, parameters = {})
      if writable?(dispatch, parameters)
        @stores.capture_first {|s| s.exists?(normalize(dispatch), parameters)}
      end
    end

    def delete(dispatch, parameters = {})
      if writable?(dispatch, parameters)
        @stores.map {|s| s.delete(normalize(dispatch), parameters)}.any?
      end
    end

    def delete_all!
      @stores.map {|s| s.delete_all!}.all?
    end

    def normalize(dispatch)
      "#{dispatch.class.name}##{dispatch.action_name}" unless dispatch.class.name.blank? || dispatch.action_name.blank?
    end
  end
end
