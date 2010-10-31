class FileStat
  def self.method_missing(meth, file)
    File.lstat(file).send(meth)
  end
end

module FileStatSpecs
  def self.null_device
    platform_is_not :windows do
      return '/dev/null'
    end
    platform_is :windows do
      return 'nul'
    end
  end
end
