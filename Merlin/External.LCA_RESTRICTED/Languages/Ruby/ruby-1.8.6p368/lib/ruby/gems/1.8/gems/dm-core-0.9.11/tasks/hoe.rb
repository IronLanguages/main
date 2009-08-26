require 'hoe'

@config_file = "~/.rubyforge/user-config.yml"
@config = nil
RUBYFORGE_USERNAME = "unknown"
def rubyforge_username
  unless @config
    begin
      @config = YAML.load(File.read(File.expand_path(@config_file)))
    rescue
      puts <<-EOS
ERROR: No rubyforge config file found: #{@config_file}
Run 'rubyforge setup' to prepare your env for access to Rubyforge
 - See http://newgem.rubyforge.org/rubyforge.html for more details
      EOS
      exit
    end
  end
  RUBYFORGE_USERNAME.replace @config["username"]
end

# Remove hoe dependency
class Hoe
  def extra_dev_deps
    @extra_dev_deps.reject! { |dep| dep[0] == "hoe" }
    @extra_dev_deps
  end
end

hoe = Hoe.new(GEM_NAME, GEM_VERSION) do |p|

  p.developer(AUTHOR, EMAIL)

  p.description = PROJECT_DESCRIPTION
  p.summary = PROJECT_SUMMARY
  p.url = PROJECT_URL

  p.rubyforge_name = PROJECT_NAME if PROJECT_NAME

  p.clean_globs |= ["{coverage,doc,log,tmp}", "**/*.{log,db}", "profile_results.*", "**/.DS_Store"]

  GEM_DEPENDENCIES.each do |dep|
    p.extra_deps << dep
  end

end
