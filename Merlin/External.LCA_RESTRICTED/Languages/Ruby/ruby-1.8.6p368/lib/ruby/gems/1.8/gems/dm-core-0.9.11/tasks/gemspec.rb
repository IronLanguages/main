desc "Generate gemspec"
task :gemspec do |x|
  # Clean up extraneous files before checking manifest
  %x[rake clean]

  # Check the manifest before generating the gemspec
  manifest = %x[rake check_manifest]
  manifest.gsub!("(in /usr/local/projects/dm/dm-core)\n", "")

  unless manifest.empty?
    print "\n", "#"*68, "\n"
    print <<-EOS
  Manifest.txt is not up-to-date. Please review the changes below.
  If the changes are correct, run 'rake check_manifest | patch'
  and then run this command again.
EOS
    print "#"*68, "\n\n"
    puts manifest
  else
    %x[rake debug_gem > #{GEM_NAME}.gemspec]
    puts "Successfully created gemspec for #{GEM_NAME}!"
  end
end
