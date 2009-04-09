#!/usr/bin/env ruby

# Purpose: Determine performance curves for various methods of pushing
#          triangles and quads through the OpenGL pipeline

# Copyright (c) 2004-2006, Geoff Broadwell; this script is released
# as open source and may be distributed and modified under the terms
# of either the Artistic License or the GNU General Public License,
# in the same manner as Perl itself.  These licenses should have been
# distributed to you as part of your Perl distribution, and can be
# read using `perldoc perlartistic` and `perldoc perlgpl` respectively.

# Conversion to ruby by Jan Dvorak <jan.dvorak@kraxnet.cz>

require 'opengl'
include Gl,Glu,Glut

require 'mathn'

require 'OGLBench'

$VERSION = '0.1.24-ruby-p1'

$test = 0
$run = 0
$done = false
$ready = false

### USER CONFIG

# Primitive sizes (and therefore counts) are integer divisors of
# (A^i * B^j * C^k ...) where good A, B, C, ... are relatively prime;
# this number is used for the draw area height and width and defaults to:
#     2^4 * 3^2 * 5 = 720
# You may also want to get fewer data points across the same range by
# directly using higher powers; for example:
#     16  * 9   * 5 = 720
#
# my @max_powers = (16 => 1, 9 => 1, 5 => 1);
$max_powers = { 2 => 4, 3 => 2, 5 => 1 }.to_a.flatten

# Maximum quads along each axis for known slow versus usually fast tests;
# chosen to be somewhat reasonable for most common settings of @max_powers
# my $max_count_slow = 60;
$max_count_slow = 154
$max_count_fast = 154

# Font to use to label graphs
$font_style = GLUT_BITMAP_HELVETICA_10

### MISC GLOBALS

$conf = $app = $gl_info = nil
$MIN_FRAMES = $MIN_SECONDS = 0
$w = $h = 0
$dls = {}
$vas = {}
$combos = $slow = $fast = 0
$showing_graph = false
$empty_time = $empty_frames = $total = 0
$max = []
$stats = []
$stats_fixed = []

### CODE

def main
	init()
	print "Benchmarks:"

	glutDisplayFunc(method(:cbDraw).to_proc)
	glutIdleFunc(method(:cbDraw).to_proc)
	glutKeyboardFunc(method(:cbKeyPressed).to_proc)
	glutMainLoop()
end

def init
	$stdout.sync = true

	$combos = recurse_combos($max_powers)
	$combos.sort!
	$slow = $combos.select { |a| a <= $max_count_slow }
	$fast = $combos.select { |a| a > $max_count_slow &&
	                             a <= $max_count_slow }

	# Choose drawing area size to match counts
	$h = $w = $combos.last

  default_conf = {
			:title    => 'Triangle Slammer OpenGL Benchmark',
			:geometry => "#{$w}x#{$h}",
			:frames   => 10,
			:seconds  => 1,
	}
  $conf, $app, $gl_info = OGLBench.basic_init(default_conf)


	# Reduce indirections in inner loops
	$MIN_FRAMES, $MIN_SECONDS = $conf[:frames], $conf[:seconds]
	
	# Let user know what's going on
	show_user_message()

	# Change projection to integer-pixel ortho
	glMatrixMode(GL_PROJECTION)
	glOrtho(0, $w, 0, $h, -1, 1)
	glMatrixMode(GL_MODELVIEW)

	# Make sure GL state is consistent for VA and DL creation
	start_frame()
	
	# Create vertex arrays and display lists outside timing loop
	init_vertex_arrays()
	init_display_lists()
	
	# Clean up GL state
	end_frame()
end

def recurse_combos(powers)
	base, max_power, *rest = powers

	return [1] if powers.size<1

	combos = []	
	(0..max_power).each do |power|
		multiplier = base ** power
		recurse_combos(rest).each do |item|
			combos << (item*multiplier)
		end
	end
	combos
end

def show_user_message
	print <<"EOM";
TRISLAM benchmarks several methods of pushing OpenGL primitives,
testing each method with various primitive counts and sizes.
During the benchmark, the test window will start out black, slowly
brightening to white as testing progresses.  Once benchmarking is
complete, the collected data will be dumped in tabular form.

The configuration for this series of tests will be as follows:

EOM

	OGLBench.show_basic_config($conf, $gl_info, $VERSION)
	
	puts "standard runs:    #{$slow.join(' ')}"
	puts "extra fast runs:  #{$fast.join(' ')}"
	puts '-' * 79
end


def init_vertex_arrays
	print "Init vertex arrays:"

	$va_types.keys.sort.each do |type|
		print " #{type}"
		($slow + $fast).each do |count|
			data = $va_types[type].call(count, $w / count.to_f)
			va = data.pack("f*")
			$vas["#{type}_#{count}"] = va
		end
	end	

	print ".\n";
end

def init_display_lists
	print "Init display lists:"

	num_lists = $dl_types.size * ($slow + $fast).size
	current = glGenLists(num_lists)

	$dl_types.keys.sort.each do |type|
		print " #{type}"
		($slow + $fast).each do |count|
			$dls["#{type}_#{count}"] = current
			glNewList(current, GL_COMPILE)
			$dl_types[type].call(count, $w / count.to_f)
			glEndList()
			current += 1
		end
	end    
	puts "."
end

def benchmark
	if ($test >= $tests.size)
		print ".\n" if (!$done)
		$done = true
		return
	end
	name,draw,stats,class_ = $tests[$test]

	counts = class_ == 'single' ? [1] : (class_ == 'slow' ? $slow : $slow + $fast)

	if ($run == 0)
		print " #{name}";

		# After printing current test name, busy wait for a second
		# so that the terminal can catch up and not do work while
		# the GL timing is in progress
		a = Time.now
		while 1 > (Time.now - a) do end
	end

	count = counts[$run]
  size  = $w / count

	OGLBench.fade_to_white(($test + ($run.to_f / counts.size)) / $tests.size)

	run_done = 0
  frames   = 0
  start    = Time.now

  while (run_done==0)
		start_frame()
    draw.call(count, size.to_f)
    end_frame()
   
    frames += 1
    run_done = 1 if ($MIN_FRAMES <= frames &&
             $MIN_SECONDS <= Time.now - start)
	end
	glFinish()
  end_ = Time.now
  time = end_ - start
    
	$stats << [name,count,time,frames] + stats.call(count,size.to_f)

	$run += 1
	if ($run >= counts.size)
		$test += 1
		$run = 0
	end
end

def cleanup
	fixup_stats()
	show_stats()
	draw_stats()
end

def start_frame
	glClear(GL_COLOR_BUFFER_BIT |
	        GL_DEPTH_BUFFER_BIT)
end

def end_frame
	glFinish()
end

def fixup_stats
	empty = $stats.shift
    
	$empty_time   = empty[2]
  $empty_frames = empty[3]
  empty_tpf = $empty_time.to_f / $empty_frames

	$total = ['totl,','avg'] + [0] * 12
	$max = ['max','max'] + [0] * 12

	$stats.each do |stat|
		name, count, time, frames, pixpf, prmpf, tpf, vpf = stat

		# Subtract out empty loop time, and loop if negative result
		# $time -= $empty_tpf * $frames;
		if (time <= 0)
			stat += [0] * 5
			next
		end
    
		# Calc "work", the geometric mean of pixels and vertices
		workpf = Math::sqrt(pixpf * vpf)
    
		# Calc fps
		fps = frames / time
    
		# Calc other perf stats
		pixps = pixpf  * fps
		prmps = prmpf  * fps
		tps   = tpf    * fps
		vps   = vpf    * fps
		wps   = workpf * fps  
		
		# Add them to stat row
		stat += [fps, pixps, prmps, tps, vps, wps]
    
		# Convert per frame counts to totals
		(4..7).each { |i| stat[i] *= frames }

		# Update running totals
		(2..7).each { |i| $total[i] += stat[i] }
			
		# Update running maximums
		(2..13).each do |i|
			$max[i] = stat[i] if $max[i] < stat[i]
		end
	
		$stats_fixed << stat
	end

	# Calc averages for totals line
	(8..13).each { |i| $total[i] = $total[i-5] / $total[2].to_f }

	$ready = true
	$stats = $stats_fixed
end


def show_stats
	basic = ["Name","Cnt","Time"]
	raw =   ["Frms","Mpix","Kprim","Ktri","Kvert"]
	calc = raw
	scale = [ 0,   6,    3,    3,   3,    ] * 2
	header = basic + raw + calc
	scale.map! {|n| 10 ** n }
	
	g_form = "%9s%-*s %s\n"
	h_form = '%-5s%3s %6s'   + ' %5s' * raw.size + ' ' + ' %5s' * calc.size + "\n"
	format = '%-5s%3s %6.3f' + ' %5d' * raw.size + ' ' + ' %5d' * calc.size + "\n"

	printf(g_form, '', 6 * 5 + 8, 'MEASURED', 'PER SECOND')
  printf(h_form, *header)
  printf(format, "empty", 1, $empty_time, $empty_frames, *([0] * 9))
    #

	($stats + [$total]).each do |stat|
		st = stat.clone()
		(0..scale.size-1).each do |i|
			st[i + 3] /= scale[i]
		end
		printf(format,*st)
	end
end


def make_quads_va(count,size)
	data = []
	(0 .. count-1).each do |y|
		(0 .. count-1).each do |x|
			data << x * size        << y * size + size
			data << x * size        << y * size
			data << x * size + size << y * size
			data << x * size + size << y * size + size
		end
	end
	data
end

def make_tris_va(count,size)
	data = []
	(0 .. count-1).each do |y|
		(0 .. count-1).each do |x|
			data << x * size        <<  y * size + size
			data << x * size        <<  y * size
			data << x * size + size <<  y * size + size
	
			data << x * size + size <<  y * size + size
			data << x * size        <<  y * size
			data << x * size + size <<  y * size
		end
	end
	data
end

def make_qs_va(count,size)
	data = []
	(0 .. count-1).each do |y|
		(0 .. count).each do |x|
			data << x * size << y * size + size
			data << x * size << y * size
    end
	end
	data
end

def make_ts_va(count,size)
	data = []
	(0 .. count-1).each do |y|
		(0 .. count).each do |x|
			data << x * size << y * size + size
			data << x * size << y * size
    end
	end
	data
end

def draw_qs(count,size)
	(0 .. count-1).each do |y|
		glBegin(GL_QUAD_STRIP)
		(0 .. count).each do |x|
			glVertex2f(x * size, y * size + size)
			glVertex2f(x * size, y * size )
		end
		glEnd()
	end
end

def draw_ts(count,size)
	(0 .. count-1).each do |y|
		glBegin(GL_TRIANGLE_STRIP)
		(0 .. count).each do |x|
			glVertex2f(x * size, y * size + size)
			glVertex2f(x * size, y * size )
		end
		glEnd()
	end
end

def draw_qs_va(count,size)
	va  = $vas["qs_#{count}"]
	row = 2 * (count + 1)

	glEnableClientState(GL_VERTEX_ARRAY)
	glVertexPointer(2, GL_FLOAT, 0, va)
	(0 .. count-1).each do |y|
		glDrawArrays(GL_QUAD_STRIP, y * row, row)
	end
	glDisableClientState(GL_VERTEX_ARRAY)
end

def draw_ts_va(count,size)
	va  = $vas["ts_#{count}"]
	row = 2 * (count + 1)

	glEnableClientState(GL_VERTEX_ARRAY)
	glVertexPointer(2, GL_FLOAT, 0, va)
	(0 .. count-1).each do |y|
		glDrawArrays(GL_TRIANGLE_STRIP, y * row, row)
	end
	glDisableClientState(GL_VERTEX_ARRAY)
end

def draw_tris(count,size)
	glBegin(GL_TRIANGLES)
	(0 .. count-1).each do |y|
		(0 .. count-1).each do |x|
			glVertex2f(x * size       , y * size + size)
			glVertex2f(x * size       , y * size       )
			glVertex2f(x * size + size, y * size + size)
	
			glVertex2f(x * size + size, y * size + size)
			glVertex2f(x * size       , y * size       )
			glVertex2f(x * size + size, y * size       )
		end
	end
	glEnd
end

def stats_tris(count,size)
	length = size * count
	area   = length * length
	prims  = 2 * count * count
	tris   =     prims
	verts  = 3 * prims
	
	[area, prims, tris, verts]
end

def draw_empty(count,size)
end

def stats_empty(count,size)
   [0,0,0,0]
end

def draw_quads(count,size)
	glBegin(GL_QUADS)
	(0 .. count-1).each do |y|
		(0 .. count-1).each do |x|
			glVertex2f(x * size       , y * size + size)
			glVertex2f(x * size       , y * size       )
			glVertex2f(x * size + size, y * size       )
			glVertex2f(x * size + size, y * size + size)
		end
	end
	glEnd
end

def stats_quads(count,size)
	length = size * count
	area   = length * length
	prims  = count * count
	tris   = 2 * prims
	verts  = 4 * prims

	[area, prims, tris, verts]
end

def stats_ts(count,size)
	length = size * count
	area   = length * length
	prims  = count
	tris   = 2 *  count      * prims
	verts  = 2 * (count + 1) * prims
	
	[area, prims, tris, verts]
end

def stats_qs(count,size)
	length = size * count
	area   = length * length
	prims  = count
	tris   = 2 *  count      * prims
	verts  = 2 * (count + 1) * prims
	
	[area, prims, tris, verts]
end

def draw_ts_dl(count,size)
	glCallList($dls["ts_#{count}"]);
end

def draw_qs_dl(count,size)
  glCallList($dls["qs_#{count}"]);
end

def draw_tris_va(count,size)
	va = $vas["t_#{count}"]
	
	glVertexPointer(2, GL_FLOAT, 0, va)
	
	glEnableClientState(GL_VERTEX_ARRAY)
	glDrawArrays(GL_TRIANGLES, 0, 6 * count * count)
	glDisableClientState(GL_VERTEX_ARRAY)
end

def draw_quads_va(count,size)
	va = $vas["q_#{count}"]
	
	glVertexPointer(2, GL_FLOAT, 0, va)
	
	glEnableClientState(GL_VERTEX_ARRAY)
	glDrawArrays(GL_QUADS, 0, 4 * count * count)
	glDisableClientState(GL_VERTEX_ARRAY)
end


def draw_ts_va_dl(count,size)
	va = $vas["ts_#{count}"]
	
	glVertexPointer(2, GL_FLOAT, 0, va)
	
	glEnableClientState(GL_VERTEX_ARRAY)
	glCallList($dls["tsv_#{count}"])
	glDisableClientState(GL_VERTEX_ARRAY)
end

def draw_qs_va_dl(count,size)
	va = $vas["qs_#{count}"]
	
	glVertexPointer(2, GL_FLOAT, 0, va)
	
	glEnableClientState(GL_VERTEX_ARRAY)
	glCallList($dls["qsv_#{count}"])
	glDisableClientState(GL_VERTEX_ARRAY)
end


def draw_stats
	return if (!$ready)

	# Graph config
	x_off     = 10
	y_off     = 10
	tick_size = 3
	val_space = 50
	key_size  = 20
	x_scale   = ($w - 4 * x_off) / (2 * ($fast.last || $slow.last))
	key_scale = ($h - 4 * y_off) / (2 * $tests.size)

	# Get a fresh black frame for graphing
	glClearColor(0, 0, 0, 1)
	start_frame()

	# Use antialiased lines
	glEnable(GL_BLEND)
	glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA)
	glEnable(GL_LINE_SMOOTH)
	glHint(GL_LINE_SMOOTH_HINT, GL_NICEST)

	# Draw axis ticks
	glColor3f(1, 1, 1);
	glBegin(GL_LINES);

	([0] + $slow + $fast).each do |count|
		x_tick = count * x_scale + x_off
		glVertex2f(x_tick, y_off)
		glVertex2f(x_tick, y_off - tick_size)
		glVertex2f(x_tick, y_off + $h / 2)
		glVertex2f(x_tick, y_off + $h / 2 - tick_size)
		glVertex2f(x_tick + $w / 2, y_off + $h / 2)
		glVertex2f(x_tick + $w / 2, y_off + $h / 2 - tick_size)
	end
	glEnd

	x_tick = x_off + 3
	val_max = (($h / 2 - 2 * y_off) / val_space).to_i

	# Work
	(0..val_max).each do |value|
		y_tick = value * val_space + y_off

		glBegin(GL_LINES)
			glVertex2f(x_off,             y_tick)
			glVertex2f(x_off - tick_size, y_tick)
		glEnd
	end

	# Pixels
	value = 0
	val_max = $max[9] / mag_scale($max[9])
	y_scale = ($h - 4 * y_off) / (2 * val_max)
	val_inc = tick_inc(val_max,5)

	while (value < val_max)
		y_tick = (value * y_scale)  + y_off

		glBegin(GL_LINES)
		glVertex2f(x_off,              y_tick + $h / 2)
		glVertex2f(x_off - tick_size, y_tick + $h / 2)
		glEnd
		OGLBench.draw_string($font_style, value.to_s, x_tick, y_tick + $h / 2) if (value!=0)
		value += val_inc
	end

	# Vertices
	value = 0
	val_max = $max[12] / mag_scale($max[12])
	y_scale = ($h - 4 * y_off) / (2 * val_max)
	val_inc = tick_inc(val_max,5)
	while (value < val_max)
		y_tick = (value * y_scale)  + y_off

		glBegin(GL_LINES)
		glVertex2f(x_off + $w / 2,              y_tick + $h / 2)
		glVertex2f(x_off + $w / 2 - tick_size,  y_tick + $h / 2)
		glEnd

		OGLBench.draw_string($font_style, value.to_s, x_tick + $w / 2, y_tick + $h / 2) if (value!=0)
		value += val_inc
	end

	# Draw axes
	glBegin(GL_LINE_STRIP)
	glVertex2f(x_off,          $h / 2 - y_off)
	glVertex2f(x_off,          y_off)
	glVertex2f($w / 2 - x_off, y_off)
	glEnd
	glBegin(GL_LINE_STRIP)
	glVertex2f(x_off,          $h - y_off)
	glVertex2f(x_off,          $h / 2 + y_off)
	glVertex2f($w / 2 - x_off, $h / 2 + y_off)
	glEnd
	glBegin(GL_LINE_STRIP)
	glVertex2f($w / 2 + x_off, $h - y_off)
	glVertex2f($w / 2 + x_off, $h / 2 + y_off)
	glVertex2f($w - x_off,     $h / 2 + y_off)
	glEnd

  # Draw color key
	(0..$tests.size - 1).each do |num|
		test = $tests[num]
		name,color,stipple = [test[0]] + test[-2,2]
		glEnable(GL_LINE_STIPPLE)
		glLineStipple(3, stipple)

		glBegin(GL_LINES)
		glColor3fv(color)
		glVertex2f(x_off + $w / 2,            y_off + num * key_scale)
		glVertex2f(x_off + $w / 2 + key_size, y_off + num * key_scale)
		glEnd()

		glDisable(GL_LINE_STIPPLE)

		OGLBench.draw_string($font_style, name, x_off + $w / 2 + key_size * 2,  y_off + num * key_scale)
	end

	# Draw performance graph lines

	# Pixels per second
	draw_one_stat(x_off, y_off + $h / 2, y_off, x_scale, 9)
	glColor3f(1, 1, 1)

	OGLBench.draw_string($font_style, mag_char($max[9]) + " Pixels/Sec", $w / 4, $h - 2 * y_off)

	# Vertices per second
	draw_one_stat(x_off + $w / 2, y_off + $h / 2, y_off, x_scale, 12)
	glColor3f(1, 1, 1)
	OGLBench.draw_string($font_style, mag_char($max[12]) + " Vertices/Sec", 3 * $w / 4, $h - 2 * y_off)

	# "Work" per second, the geometric mean of pixels and vertices
	draw_one_stat(x_off, y_off, y_off, x_scale, 13)
	glColor3f(1, 1, 1)

	OGLBench.draw_string($font_style, "Work/Sec", $w / 4, $h / 2 - 2 * y_off)

	# Show our graph
	end_frame();
	$showing_graph = true
end

def draw_one_stat(x_loc,y_loc,y_off,x_scale,num)
	max     = $max[num]
	y_scale = ($h - 4 * y_off) / (2 * max)
	colors = {}
	$tests.each do |test| colors[test[0]] = test[-2] end

	stipple = {}
	$tests.each do |test| stipple[test[0]] = test[-1] end

	last = ''
	
	glEnable(GL_LINE_STIPPLE)
	glBegin(GL_LINE_STRIP)
	$stats.each_with_index do |stat,run|
		name,count, st = stat[0,2] + [stat[num]]

		if name != last
			glEnd
			glLineStipple(3, stipple[name])
			glBegin(GL_LINE_STRIP)
			last = name
		end

		glColor3fv(colors[name])
		glVertex2f(count * x_scale + x_loc, st * y_scale + y_loc)
	end
	glEnd
	glDisable(GL_LINE_STIPPLE)
end


def kilo_mag(num)
	mag = (Math::log(num) / Math::log(10)).to_i
	(mag / 3)
end

def mag_char(num)
	['','K','M','G','T','P','E','Z','Y'][kilo_mag(num)]
end

def mag_scale(num)
	10 ** (3*kilo_mag(num))
end

def tick_inc(max,parts = 5)
	return (max / parts.to_f) if (max < 1)

	mag = (Math::log(max) / Math::log(10)).to_i
	scl = (10 ** (mag - 1))
	inc = max / (scl * parts)
	
	if (inc > 7.5)
		inc = 10
	elsif (inc > 3.5)
		inc = 5
	elsif (inc > 1.5)
		inc = 2
	else
		inc = 1
	end	
	(inc * scl.to_f)
end

# State engine
def cbDraw
	if (!$done)
		benchmark()
	elsif (!$ready)
		cleanup()
	else
		sleep(1)
		draw_stats()
	end
end

# Keyboard handler
def cbKeyPressed(key,x,y)
  if (key == ?\e or key == ?q)
		glutDestroyWindow($app)
		exit(0)
	end
	if ($done && key == ?r)
		draw_stats()
	end
end


### METHODS TO BENCHMARK

$va_types = {
	"q"   => method(:make_quads_va),
	"t"   => method(:make_tris_va),
	"qs"  => method(:make_qs_va),
	"ts"  => method(:make_ts_va),
}

$dl_types = {
	"qs"  => method(:draw_qs),
  "ts"  => method(:draw_ts),
  "qsv" => method(:draw_qs_va),
	"tsv" => method(:draw_ts_va),
}

$tests = [
	# Nick    Draw Routine          Stats Calc           Type     Graph Color
#	["empty",method(:draw_empty),   method(:stats_empty),'single',[1 ,  1,  1], 0xFFFF],
	["t"    ,method(:draw_tris),    method(:stats_tris) ,'slow',  [1 ,  0,  0], 0xAAAA],
	["q"    ,method(:draw_quads),   method(:stats_quads),'slow',  [1 ,0.5,  0], 0xAAAA],
	["ts"   ,method(:draw_ts),      method(:stats_ts),   'slow',  [1 ,  1,  0], 0xAAAA],
	["qs"   ,method(:draw_qs),      method(:stats_qs),   'slow',  [0 ,  1,  0], 0xAAAA],
	["tsd"  ,method(:draw_ts_dl),   method(:stats_ts),   'fast',  [0 ,  1,  1], 0xAAAA],
	["qsd"  ,method(:draw_qs_dl),   method(:stats_qs),   'fast',  [0 ,  0,  1], 0xAAAA],
	["tv"   ,method(:draw_tris_va), method(:stats_tris), 'fast',  [0.8,  0, 0], 0xFFFF],
	["qv"   ,method(:draw_quads_va),method(:stats_quads),'fast',  [0.8,0.4, 0], 0xFFFF],
	["tsv"  ,method(:draw_ts_va),   method(:stats_ts),   'fast',  [0.8,0.8, 0], 0xFFFF],
	["qsv"  ,method(:draw_qs_va),   method(:stats_qs),   'fast',  [0 ,0.8,  0], 0xFFFF],
	["tsvd" ,method(:draw_ts_va_dl),method(:stats_ts),   'fast',  [0 ,0.8,0.8], 0xFFFF],
	["qsvd" ,method(:draw_qs_va_dl),method(:stats_qs),   'fast',  [0 ,  0,0.8], 0xFFFF],
]

# Start from main function ()
main()

