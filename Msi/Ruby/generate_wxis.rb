require File.join(this_dir = File.dirname(__FILE__), "../harvest.rb")

WXIs = [
  ["Msm/Samples.wxi",      "Languages/Ruby/Samples"],
  ["Msm/IronRubyLibs.wxi", "Languages/Ruby/Libs"],
  ["Msm/RubyLibs.wxi",     "External.LCA_RESTRICTED/Languages/Ruby/redist-libs/ruby"],
]

WXIs.each do |wxi, dir|
  Harvester.new.harvest dir, File.join(this_dir, wxi), File.basename(__FILE__), "$(var.DlrRoot)"
end