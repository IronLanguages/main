module FileSpecs
  def self.with_upper_case_folders
    tmp_dir = tmp("expand_path")
    FileUtils.rm_rf(tmp_dir) if File.exist?(tmp_dir)
    aaa = File.join(tmp_dir, "AAA")
    begin
      FileUtils.mkdir_p(aaa + "/BBB/CCC/DDD")
      yield tmp_dir
    ensure
      FileUtils.rm_rf(tmp_dir) if File.exist? tmp_dir
    end
  end

  def self.non_existent_drive
    @non_existent_drive ||= ('a'..'z').each do|letter|
      drive = "#{letter}:"
      break drive unless File.exists?(drive + "/")
    end
    flunk unless @non_existent_drive # could not find non-existent drive
    @non_existent_drive
  end
end
