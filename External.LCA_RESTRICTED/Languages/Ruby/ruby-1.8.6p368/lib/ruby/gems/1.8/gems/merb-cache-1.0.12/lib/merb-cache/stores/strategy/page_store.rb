module Merb::Cache
  # Store well suited for page caching.
  class PageStore < AbstractStrategyStore
    def writable?(dispatch, parameters = {}, conditions = {})
      if Merb::Controller === dispatch && dispatch.request.method == :get &&
          !dispatch.request.uri.nil? && !dispatch.request.uri.empty? &&
          !conditions.has_key?(:if) && !conditions.has_key?(:unless) &&
          query_string_present?(dispatch)
        @stores.any? {|s| s.writable?(normalize(dispatch), parameters, conditions)}
      else
        false
      end
    end

    def read(dispatch, parameters = {})
      nil
    end

    def write(dispatch, data = nil, parameters = {}, conditions = {})
      if writable?(dispatch, parameters, conditions)
        @stores.capture_first {|s| s.write(normalize(dispatch), data || dispatch.body, {}, conditions)}
      end
    end

    def write_all(dispatch, data = nil, parameters = {}, conditions = {})
      if writable?(dispatch, parameters, conditions)
        @stores.map {|s| s.write_all(normalize(dispatch), data || dispatch.body, {}, conditions)}.all?
      end
    end

    def fetch(dispatch, parameters = {}, conditions = {}, &blk)
      if writable?(dispatch, parameters, conditions)
        read(dispatch, parameters) || @stores.capture_first {|s| s.fetch(normalize(dispatch), data || dispatch.body, {}, conditions, &blk)}
      end
    end

    def exists?(dispatch, parameters = {})
      if writable?(dispatch, parameters)
        @stores.capture_first {|s| s.exists?(normalize(dispatch), {})}
      end
    end

    def delete(dispatch, parameters = {})
      if writable?(dispatch, parameters)
        @stores.map {|s| s.delete(normalize(dispatch), {})}.any?
      end
    end

    def delete_all!
      @stores.map {|s| s.delete_all!}.all?
    end

    def normalize(dispatch)
      key = dispatch.request.uri.split('?').first
      key << "index" if key =~ /\/$/
      key << ".#{dispatch.content_type}" unless key =~ /\.\w{2,6}/
      key
    end

    def query_string_present?(dispatch)
      dispatch.request.env["REQUEST_URI"] == dispatch.request.uri
    end
  end
end
