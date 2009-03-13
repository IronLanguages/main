#!/bin/bash

# set -xv

trap "exit 1" 1 2 3 15

DIRS="-I../../metaruby/dev/tests/builtin -I../../metaruby/dev/tests -Ilib"
for d in $(ls -d ../../*/dev); do
    DIRS="-I$d $DIRS"
done

if [ -f rb.bad.txt ]; then
    mv rb.bad.txt rb.files.txt
else
    find ../../*/dev /usr/local/lib/ruby/1.8/ -name \*.rb > rb.files.txt
fi

total_count=$(wc -l rb.files.txt | awk '{print $1}')
curr_count=0
for f in $(cat rb.files.txt); do
    curr_count=$(($curr_count + 1))
    if GEM_SKIP=ParseTree ruby $DIRS ./bin/parse_tree_show -q $f > /dev/null 2> rb.err.txt < /dev/null; then
	echo $f >> rb.good.txt
	status=pass
    else
	echo $f >> rb.bad.txt	
	status=fail
    fi
    fname=`basename $f`
    printf "%4d/%4d: %s %s\n" $curr_count $total_count $status $fname
done
