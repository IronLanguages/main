require 'rack'

module Rack #:nodoc:
  module Mount #:nodoc:
    autoload :GeneratableRegexp, 'rack/mount/generatable_regexp'
    autoload :Mixover, 'rack/mount/mixover'
    autoload :Multimap, 'rack/mount/multimap'
    autoload :Prefix, 'rack/mount/prefix'
    autoload :RegexpWithNamedGroups, 'rack/mount/regexp_with_named_groups'
    autoload :Route, 'rack/mount/route'
    autoload :RouteSet, 'rack/mount/route_set'
    autoload :RoutingError, 'rack/mount/exceptions'
    autoload :Strexp, 'rack/mount/strexp'
    autoload :Utils, 'rack/mount/utils'

    module Analysis #:nodoc:
      autoload :Frequency, 'rack/mount/analysis/frequency'
      autoload :Histogram, 'rack/mount/analysis/histogram'
      autoload :Splitting, 'rack/mount/analysis/splitting'
    end

    module Generation #:nodoc:
      autoload :Route, 'rack/mount/generation/route'
      autoload :RouteSet, 'rack/mount/generation/route_set'
    end

    module Recognition #:nodoc:
      autoload :CodeGeneration, 'rack/mount/recognition/code_generation'
      autoload :Route, 'rack/mount/recognition/route'
      autoload :RouteSet, 'rack/mount/recognition/route_set'
    end
  end
end
