Dir[File.dirname(__FILE__) + "/../../../../../**/*.csproj"].each do |csproj|
  next if csproj =~ /Build/
  puts csproj
  content = File.open(csproj, 'r'){|f| f.read}
  newc = content.gsub('<HintPath>$(SilverlightPath)', "<HintPath>C:\\Program Files\\Microsoft Silverlight\\\\3.0.40723.0")
  if (content == newc)
    puts "no change"
  else
    puts "change"
  end
  File.open(csproj, 'w'){|f| f.write(newc)}
end
