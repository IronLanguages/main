if defined?(JSON::Pure::Parser::STRING)
  class JSON::Pure::Parser
    if JSON::Pure::Parser::STRING.source.include?('\\[\x20-\xff]')
      remove_const(:STRING)
      STRING = /" ((?:[^\x0-\x1f"\\] |
                \\["\\\/bfnrt] |
                \\u[0-9a-fA-F]{4} |
                \\[\x20-\x21\x23-\x2e\x30-\x5b\x5d-\x61\x63-\x65\x67-\x6d\x6f-\x71\x73\x75-\xff])*)
               "/nx
      warn("You are running an outdated an vulnerable version of JSON::Pure. Merb has fixed the vulnerability, but " \
           "you should upgrade to the latest version of JSON::Pure or use the json gem")
    end
  end
end