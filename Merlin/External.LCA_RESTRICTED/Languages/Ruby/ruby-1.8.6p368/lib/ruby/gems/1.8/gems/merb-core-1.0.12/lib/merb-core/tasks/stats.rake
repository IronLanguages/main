def show_line(name, stats, color = nil)
  ce = color ? "\033[0m" : ""
  puts  "| #{color}#{name.to_s.capitalize.ljust(20)}#{ce} " + 
        "| #{color}#{stats[:lines].to_s.rjust(7)}#{ce} " +
        "| #{color}#{stats[:loc].to_s.rjust(7)}#{ce} " +
        "| #{color}#{stats[:classes].to_s.rjust(7)}#{ce} " +
        "| #{color}#{stats[:modules].to_s.rjust(7)}#{ce} " +
        "| #{color}#{stats[:methods].to_s.rjust(7)}#{ce} |"
  puts separator
end

def separator
  '+----------------------+---------+---------+---------+---------+---------+'
end

def check_dir(dir)
  Dir.foreach(dir) do |file_name|
    if File.stat(dir / file_name).directory? and (/^\./ !~ file_name)
      check_dir(dir / file_name)
    end

    if file_name =~ /.*\.rb$/
      File.open(dir / file_name).each_line do |line|
        @stats[:lines]    += 1
        @stats[:loc]      += 1 unless line =~ /^\s*$/ || line =~ /^\s*#/
        @stats[:classes]  += 1 if line =~ /class [A-Z]/
        @stats[:modules]  += 1 if line =~ /module [A-Z]/
        @stats[:methods]  += 1 if line =~ /def [a-z]/
      end
    end
  end
end

desc "Lines of code statistics"
task :stats do
  STATISTICS_DIRS = {
    :controllers  => 'app/controllers',
    :helpers      => 'app/helpers',
    :models       => 'app/models',
    :lib          => 'lib',
    :spec         => 'spec'
  }.reject {|name, dir| !File.exist?(dir) }
  EMPTY_STATS = { :lines => 0, :loc => 0, :classes => 0, :modules => 0, :methods => 0 }
 
  @all = {}
  total = EMPTY_STATS.clone
  ce = "\033[0m"
  cb = "\033[35m"
  cg = "\033[4;32m"
  cr = "\033[31m"
 
  puts separator
  puts "| #{cg}Name#{ce}                 | #{cg}Lines#{ce}   | #{cg}LOC#{ce}     | #{cg}Classes#{ce} | #{cg}Modules#{ce} | #{cg}Methods#{ce} |"
  puts separator
 
  STATISTICS_DIRS.each_pair do |name, dir| 
    @stats = EMPTY_STATS.clone
    check_dir(dir)
    @all[name] = @stats
    show_line(name, @stats)
    @stats.each_pair { |type, count| total[type] += count }
  end
 
  show_line('Total', total, cr)
 
  code_loc = [:controllers, :helpers, :models].inject(0) { |sum, e| sum += @all[e][:loc] }
  test_loc = @all[:spec][:loc]
 
  puts "   Code LOC: #{cb}#{code_loc}#{ce}     Test LOC: #{cb}#{test_loc}#{ce}     Code to test radio:  #{cb}1:%0.2f#{ce}" % (test_loc.to_f / code_loc.to_f)
  puts
end
