module EnvSpecs
  def self.with_temp_ENV()
    orig = ENV.to_hash
    comspec = ENV['COMSPEC']
    sysroot = ENV['SYSTEMROOT']
    begin
      ENV.clear
      # Probably a CLR bug - the child process blows up while loading ir.exe assembly if SYSTEMROOT is not set:
      ENV['SYSTEMROOT'] = sysroot
      yield
    ensure
      ENV.replace orig
    end
  end  
end
