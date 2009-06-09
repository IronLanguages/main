###############################
#
# contrib/inifile.rb
#
# These modules/classes are contributed by Yukimi_Sake-san.
# Distributed at http://vruby.sourceforge.net/index.html
#
###############################


class Inifile < Hash
=begin
===Inifile
It makes inifile like Window's in working directory or absolute path.

     [section]
     key=value
     ...

===Methods
---deletekey(section,key)
   Delete ((|key|)) and associated value in ((|section|)).
---erasesection(section)
   Erase ((|section|)) and associated keys and values.
---read( section, key, [default=""])
   Read value associated ((|key|)) in ((|section|)).
   If ((|section|)) or ((|key|)) not exists ,value uses ((|default|)).
---write( section, key, value)
   Write ((|value|)) in assiciated ((|key|)) and ((|section|)).
   If ((|section|)) or ((|key|)) not exists , make them.
---frash
   Write into ((|inifilename|)) of instance.
=end
  def initialize(iniFilename) #privatemethod. Use "new(inFilename)" instead.
    sec={}
    section=""
    if File.dirname(iniFilename) == "."
      @fn = Dir.getwd + "/" + iniFilename
    else
      @fn = iniFilename
    end
    if File.exist?(@fn) then
      f=open @fn
      f.each do |t|
        if t =~ /\[.+\]/ then
          section= t.sub(/\[(.+)\]/,'\1').strip
          sec={section=>{}}
          self.update sec
        elsif t =~/.+=/ then
          a=t.split(/=/)
          val={a[0].strip=>a[1].strip}
          self[section].update(val)
        end
      end
      f.close
    end
  end

  def deletekey(section,key)
    self[section.strip].delete(key)
  end

  def erasesection(section)
    self.delete(section)
  end

  def read( section, key, default="")
    if self[section] && r=self[section][key] then r else default end
  end

  def write( section, key, value)
    self.update({section.strip=>{}}) if self[section.strip] == nil
    self[section.strip].update({key.strip => (value.to_s).strip})
  end

  def flash
    f=open @fn,"w"
    self.each do |k,v|
      f.write'['+k+']' +"\n"
      v.each do |k1,v1|
         f.write k1+"="+v1.to_s+"\n"
      end
    end
    f.close
  end

end

=begin sample
require 'inifile'

ini =  Inifile.new ("test.ini") # generate instance and read if
                                # targetfile exists

p "writing as method"
ini.write "section1","key11",11
ini.write "section1","key12",12
ini.write "section2","key21",21
ini.write "section2","key22",22
ini.write "section3","key31",31
ini.write "section3","key32",32
p ini.read "section1","key11",101
p ini.read("section1","key13",103)
p ini.read("section2","key22",202)
p ini.read("section2","key23",203)
p ini.read("section3","key32",302)
p ini.read("section3","key33",303)
p "writing as Hash"
p ini["section1"]["key11"]
p ini["section2"]
#ini.deletekey("section1","key12")
#ini.erasesection("section1")

ini.flash  # now update inifile

=end
