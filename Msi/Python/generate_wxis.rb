require File.join(this_dir = File.dirname(__FILE__), "../harvest.rb")

def stdlib_files
  stdlib_proj = File.join(ENV["DLR_ROOT"], "Languages/IronPython/StdLib/StdLib.pyproj")

  files = []
  File.foreach(stdlib_proj) do |line|
    if /\<\s*Content\s*Include\s*=\s*['"]\$\(StdLibPath\)\\([^'"]*)['"]\s*\/>/ =~ line
      files << $1
    end
  end
  files
end

Harvester.new.harvest stdlib_files, File.join(this_dir, "Msm/StdLib.wxi"), File.basename(__FILE__), "$(var.StdLibPath)"
