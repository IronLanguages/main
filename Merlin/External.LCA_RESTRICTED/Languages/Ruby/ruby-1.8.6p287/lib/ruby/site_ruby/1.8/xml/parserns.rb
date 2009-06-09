## -*- Ruby -*-
## XML::ParserNS
## namespaces-aware version of XML::Parser (experimental)
## 2002 by yoshidam

require 'xml/parser'

module XML
  class InternalParserNS < Parser
    XMLNS = 'http://www.w3.org/XML/1998/namespace'
    attr_reader :ns

    def self.new(parserNS, *args)
      nssep = nil
      if args.length == 2 && !args[0].is_a?(Parser)
        nssep = args[1]
        args = args.shift
      end
      obj = super(*args)
      obj.__init__(parserNS, nssep)
      obj
    end

    def __init__(parserNS, nssep)
      @ns = []
      @parserNS = parserNS
      @nssep = nssep
    end


    def parse(*args)
      if block_given?
        super do |nodetype, name, args, parser|
          case nodetype
          when START_ELEM
            ns, args = getNSAttrs(args)
            @ns.push(ns)
            if @nssep
              if @parserNS.respond_to?(:startNamespaceDecl)
                ns.each do |prefix, uri|
                  yield(START_NAMESPACE_DECL, prefix, uri, parser)
                end
              end

              prefix, uri, localpart = resolveElementQName(name)
              name = uri + @nssep + name if uri
              attrs = {}
              args.each do |k, v|
                prefix, uri, localpart = resolveAttributeQName(k)
                k = uri + @nssep + k if uri
                attrs[k] = v
              end
              args = attrs
            end
            yield(nodetype, name, args, parser)
          when END_ELEM
            if @nssep
              prefix, uri, localpart = resolveElementQName(name)
              name = uri + @nssep + name if uri
            end
            yield(nodetype, name, args, parser)
            ns = @ns.pop
            if @nssep and @parserNS.respond_to?(:endNamespaceDecl)
              ns.to_a.reverse.each do |prefix, uri|
                yield(END_NAMESPACE_DECL, prefix, nil, parser)
              end
            end
          else
            yield(nodetype, name, args, parser)
          end
        end
      else
        super
      end
    end

    def getNamespaces
      @ns[-1]
    end

    def getNSURI(prefix)
      return XMLNS if prefix == 'xml'
      @ns.reverse_each do |n|
        return n[prefix] if n.include?(prefix)
      end
      nil
    end

    def resolveElementQName(qname)
      qname =~ /^((\S+):)?(\S+)$/u
      prefix, localpart = $2, $3
      uri = getNSURI(prefix)
      [prefix, uri, localpart]
    end

    def resolveAttributeQName(qname)
      qname =~ /^((\S+):)?(\S+)$/u
      prefix, localpart = $2, $3
      uri = nil
      uri = getNSURI(prefix) if !prefix.nil?
      [prefix, uri, localpart]
    end

    def getSpecifiedAttributes
      ret = super
#      attrs = {}
#      ret.each do |k, v|
#        next if k =~ /^xmlns/u
#        if @nssep
#          prefix, uri, localpart = resolveAttributeQName(k)
#          k = uri.to_s + @nssep + k
#        end
#        attrs[k] = v
#      end
      attrs = []
      ret.each do |k|
        next if k =~ /^xmlns:|^xmlns$/u
        if @nssep
          prefix, uri, localpart = resolveAttributeQName(k)
          k = uri.to_s + @nssep + k
        end
        attrs.push(k)
      end
      attrs
    end


    private

    def getNSAttrs(args, eliminateNSDecl = false)
      ns = {}
      newargs = {}
      args.each do |n, v|
        prefix, localpart = n.split(':')
        if prefix == 'xmlns'
          ns[localpart] = v
          next if eliminateNSDecl
        end
        newargs[n] = v
      end
      [ns, newargs]
    end


    def startElement(name, args)
      ns, args = getNSAttrs(args)
      @ns.push(ns)
      if @nssep and @parserNS.respond_to?(:startNamespaceDecl)
        ns.each do |prefix, uri|
          @parserNS.startNamespaceDecl(prefix, uri)
        end
      end
      if @parserNS.respond_to?(:startElement)
        if @nssep
          prefix, uri, localpart = resolveElementQName(name)
          name = uri + @nssep + name if uri
          attrs = {}
          args.each do |k, v|
            prefix, uri, localpart = resolveAttributeQName(k)
            k = uri + @nssep + k if uri
            attrs[k] = v
          end
          args = attrs
        end
        @parserNS.startElement(name, args)
      end
    end

    def endElement(name)
      if @parserNS.respond_to?(:endElement)
        if @nssep
          prefix, uri, localpart = resolveElementQName(name)
          name = uri + @nssep + name if uri
        end
        @parserNS.endElement(name)
      end
      ns = @ns.pop
      if @nssep and @parserNS.respond_to?(:endNamespaceDecl)
        ns.to_a.reverse.each do |prefix, uri|
          @parserNS.endNamespaceDecl(prefix)
        end
      end
    end
  end


  class ParserNS
    EVENT_HANDLERS = [
      :character,
      :processingInstruction,
      :unparsedEntityDecl,
      :notationDecl,
      :externalEntityRef,
      :comment,
      :startCdata,
      :endCdata,
      :startNamespaceDecl,
      :endNamespaceDecl,
      :startDoctypeDecl,
      :endDoctypeDecl,
      :default,
      :defaultExpand,
      :unknownEncoding,
      :notStandalone,
      :elementDecl,
      :attlistDecl,
      :xmlDecl,
      :entityDecl,
      :externalParsedEntityDecl,
      :internalParsedEntityDecl]

    def initialize(*args)
      @parser = InternalParserNS.new(self, *args)
    end

    def parse(*args, &block)
      EVENT_HANDLERS.each do |m|
        if self.respond_to?(m)
          eval "def @parser.#{m}(*args); @parserNS.#{m}(*args); end"
        end
      end
      @parser.parse(*args, &block)
    end

    def setReturnNSTriplet(do_nst); end

    def method_missing(name, *args)
      if @parser.respond_to?(name)
        @parser.send(name, *args)
      else
        raise NameError.new("undefined method `#{name.id2name}' " +
                            "for #{self.inspect}")
      end
    end
  end
end
