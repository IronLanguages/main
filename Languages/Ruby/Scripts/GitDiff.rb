require "ftools"

path_of_file_in_branch = ARGV[0]
old_file = ARGV[1]
new_file = ARGV[2]
old_files_dir = ARGV[3]
new_files_dir = ARGV[4]

file_name = File.basename(path_of_file_in_branch)

File.copy(old_file, old_files_dir + "\\" + file_name) if old_file != "/dev/null"
File.copy(new_file, new_files_dir + "\\" + file_name) if new_file != "/dev/null"