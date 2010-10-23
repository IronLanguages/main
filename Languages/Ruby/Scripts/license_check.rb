str_array = [ 
"/* ****************************************************************************\n",
" *\n",
" * Copyright (c) Microsoft Corporation. \n",
" *\n",
" * This source code is subject to terms and conditions of the Apache License, Version 2.0. A \n",
" * copy of the license can be found in the License.html file at the root of this distribution. If \n",
" * you cannot locate the  Apache License, Version 2.0, please send an email to \n",
" * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound \n",
" * by the terms of the Apache License, Version 2.0.\n",
" *\n",
" * You must not remove this notice, or any other, from this software.\n",
" *\n",
" *\n",
" * ***************************************************************************/\n"]

unicode_str_array = [ 
"\357\273\277/* ****************************************************************************\n",
"\357\273\277 *\n",
"\357\273\277 * Copyright (c) Microsoft Corporation. \n",
"\357\273\277 *\n",
"\357\273\277 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A \n",
"\357\273\277 * copy of the license can be found in the License.html file at the root of this distribution. If \n",
"\357\273\277 * you cannot locate the  Apache License, Version 2.0, please send an email to \n",
"\357\273\277 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound \n",
"\357\273\277 * by the terms of the Apache License, Version 2.0.\n",
"\357\273\277 *\n",
"\357\273\277 * You must not remove this notice, or any other, from this software.\n",
"\357\273\277 *\n",
"\357\273\277 *\n",
"\357\273\277 * ***************************************************************************/\n"]

files = Dir["#{ARGV[0]}/**/*.{cs,rb}"]
files.each do |file|
lines = File.readlines file
  res = true
  
  str_array.each_with_index do |line, i|
    if line == lines[i]
      next
    elsif unicode_str_array[i] == lines[i]
      next
    else
      res = false
      puts "************************************************************"
      puts "FILE: #{file}"
      puts "ACTUAL: #{lines[i]}"
      puts "EXPECTED: #{line}"
      puts "OR: #{unicode_str_array[i]}"
      puts "************************************************************"
      break
    end
  end

  puts "#{file} OK" unless res
end
