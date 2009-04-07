class Pathname
  # Append path segments and expand to absolute path
  #
  #   file = Pathname(Dir.pwd) / "subdir1" / :subdir2 / "filename.ext"
  #
  # @param [Pathname, String, #to_s] path path segment to concatenate with receiver
  #
  # @return [Pathname]
  #   receiver with _path_ appended and expanded to an absolute path
  #
  # @api public
  def /(path)
    (self + path).expand_path
  end
end # class Pathname
