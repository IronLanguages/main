# Workaround for bug 1972 - ENV['TZ'] is read only on startup on Windows
def can_modify_env_tz?()
  extended_on :ironruby do
    return true
  end

  platform_is_not :windows do
    return true
  end

  ruby_bug "1972", "1.9.2" do
    return true
  end
  
  false
end

def with_timezone(name, offset = nil, daylight_saving_zone = "")
  zone = name.dup

  if (offset)
    # TZ convention is backwards
    offset = -offset

    zone << offset.to_s
    zone << ":00:00"
  end
  zone << daylight_saving_zone

  if can_modify_env_tz?
    old = ENV["TZ"]
    ENV["TZ"] = zone
  
    begin
      yield
    ensure
      ENV["TZ"] = old
    end
  else
    yield
    # skip the test
    #true.should be_true
  end
end

# Returns the given time in the same format as returned by
# Time.at(seconds).inspect on MRI 1.8
def localtime_18(seconds)
  platform_is :os => [:darwin, :bsd] do
    return `LC_ALL=C /bin/date -r #{seconds} +'%a %b %d %H:%M:%S %z %Y'`.chomp
  end

  platform_is :os => :linux do
    return `LC_ALL=C date -d @#{seconds} +'%a %b %d %H:%M:%S %z %Y'`.chomp
  end

  return `LC_ALL=C date -j -f "%s" #{seconds} "+%a %b %d %H:%M:%S %z %Y"`.chomp
end

# Returns the given time in the same format as returned by
# Time.at(seconds).inspect on MRI 1.9
def localtime_19(seconds)
  platform_is :os => [:darwin, :bsd] do
    return `LC_ALL=C /bin/date -r #{seconds} +'%F %H:%M:%S %z'`.chomp
  end

  platform_is :os => :linux do
    return `LC_ALL=C date -d @#{seconds} +'%F %H:%M:%S %z'`.chomp
  end

  return `LC_ALL=C date -j -f "%s" #{seconds} "+%F %H:%M:%S %z"`.chomp
end
