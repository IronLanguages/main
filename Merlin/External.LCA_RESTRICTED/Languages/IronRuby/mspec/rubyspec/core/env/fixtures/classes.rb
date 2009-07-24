module EnvSpecs
  def self.with_temp_ENV()
    orig = ENV.to_hash
    comspec = ENV['COMSPEC']
    begin
      ENV.clear
      yield
    ensure
      ENV.replace orig
    end
  end  
end
