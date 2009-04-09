#!/usr/bin/env ruby

# Purpose: Factor out and clean up common code from OpenGL benchmarks

# Copyright (c) 2006, Geoff Broadwell; this module is released
# as open source and may be distributed and modified under the terms
# of either the Artistic License or the GNU General Public License,
# in the same manner as Perl itself.  These licenses should have been
# distributed to you as part of your Perl distribution, and can be
# read using `perldoc perlartistic` and `perldoc perlgpl` respectively.

# Conversion to ruby by Jan Dvorak <jan.dvorak@kraxnet.cz>

module OGLBench

require 'opengl'
include Gl,Glu,Glut

require 'getoptlong'

$VERSION = '0.1.4'

# Cached config and state, for simpler API
$CACHED = {}

# All resolutions listed in landscape orientation,
# even for natively portrait devices such as handhelds.
# Also, 'q' is annoyingly used as a prefix to mean both
# 'quarter' and 'quad'.
$KNOWN_RES = {
    'qqvga'     => [  160,  120, 'Quarter Quarter VGA' ],
    'palm'      => [  160,  160, 'Original Palm OS Devices' ],
    'eigthvga'  => [  240,  160, '1/8 VGA' ],
    'vcrntsc'   => [  240,  480, 'VCR NTSC' ],
    'vcrpal'    => [  240,  576, 'VCR PAL' ],
    'qcif'      => [  176,  144, 'Quarter CIF' ],
    'gears'     => [  300,  300, 'OpenGL Gears Benchmark' ],
    'glxgears'  => [  300,  300, 'OpenGL Gears Benchmark' ],
    'cga'       => [  320,  200, 'IBM PC CGA (Color Graphics Adapter)' ],
    'atarist16' => [  320,  200, 'Atari ST 16 Color' ],
    'qvga'      => [  320,  240, 'Quarter VGA' ],
    'modex'     => [  320,  240, 'VGA Mode X' ],
    'pocketpc'  => [  320,  240, 'Common Pocket PCs' ],
    'clie'      => [  320,  320, 'Sony Clie (Palm OS)' ],
    'vcdntsc'   => [  352,  240, 'VCD (Video CD) NTSC' ],
    'vcdpal'    => [  352,  288, 'VCD (Video CD) PAL' ],
    'cif'       => [  352,  288, 'CIF (Common Image Format)' ],
    'tweakvga'  => [  360,  480, 'Highest 256-color mode for VGA monitors' ],
    'svhsntsc'  => [  400,  480, 'S-VHS NTSC' ],
    'svhspal'   => [  400,  576, 'S-VHS PAL' ],
    'tungsten'  => [  480,  320, 'Tungsten (Palm OS)' ],
    'bwmac9'    => [  512,  342, 'Black & White 9" Mac' ],
    'maclc'     => [  512,  384, 'Mac LC' ],
    'ldntsc'    => [  560,  480, 'Laserdisc NTSC' ],
    'ldpal'     => [  560,  576, 'Laserdisc PAL' ],
    'atarist4'  => [  640,  200, 'Atari ST 4 Color' ],
    'ega'       => [  640,  350, 'IBM PC-AT EGA (Extended Graphics Adapter)' ],
    'atarist2'  => [  640,  400, 'Atari ST 2 Color' ],
    'qcga'      => [  640,  400, 'Quad CGA' ],
    'vga400'    => [  640,  400, 'VGA 400 Line' ],
    'pgc'       => [  640,  480, 'Professional Graphics Controller' ],
    'mcga'      => [  640,  480, 'IBM PS/2 MultiColor Graphics Adapter' ],
    'vga'       => [  640,  480, 'IBM PS/2 VGA (Video Graphics Array)' ],
    'edtv1'     => [  640,  480, 'EDTV 1' ],
    'edtv2'     => [  704,  480, 'EDTV 2' ],
    'hgc'       => [  720,  348, 'Hercules Graphics Controller' ],
    'mda'       => [  720,  350, 'IBM PC MDA (Monochrome Display Adapter)' ],
    'lisa'      => [  720,  360, 'Apple Lisa' ],
    'dv525'     => [  720,  480, 'Digital Video 525/60 (D-1 NTSC, DVD NTSC)' ],
    'd1ntsc'    => [  720,  480, 'Digital Video 525/60 (D-1 NTSC, DVD NTSC)' ],
    'dvdntsc'   => [  720,  480, 'Digital Video 525/60 (D-1 NTSC, DVD NTSC)' ],
    'sp525'     => [  720,  540, 'Digital Video 525/60 (D-1 NTSC Square Pix)' ],
    'spd1ntsc'  => [  720,  540, 'Digital Video 525/60 (D-1 NTSC Square Pix)' ],
    'dv625'     => [  720,  576, 'Digital Video 625/50 (PAL, DVD PAL)' ],
    'dvpal'     => [  720,  576, 'Digital Video 625/50 (PAL, DVD PAL)' ],
    'dvdpal'    => [  720,  576, 'Digital Video 625/50 (PAL, DVD PAL)' ],
    'ns525'     => [  768,  483, 'Non-Standard DV 525/60' ],
    'sp625'     => [  768,  576, 'Square Pixel DV 625/50' ],
    'wga'       => [  800,  480, 'Wide VGA' ],
    'svga'      => [  800,  600, 'Super VGA' ],
    'edtv3'     => [  852,  480, 'EDTV 3' ],
    'ws480'     => [  854,  480, 'Wide Screen 480 Line' ],
    'ns625'     => [  948,  576, 'Non-Standard DV 625/60' ],
    'aacsict'   => [  960,  540, 'AACS Image Constraint Token, Degraded 1080' ],
    'ws576'     => [ 1024,  576, 'Wide Screen 576 Line' ],
    '8514'      => [ 1024,  768, 'IBM 8514/A' ],
    '8514a'     => [ 1024,  768, 'IBM 8514/A' ],
    'evga'      => [ 1024,  768, 'VESA Extended VGA' ],
    'xga'       => [ 1024,  768, 'IBM XGA (eXtended Graphics Array)' ],
    'nextcube'  => [ 1120,  832, 'NeXT Cube' ],
    'apple1mp'  => [ 1152,  864, 'Apple "1 Megapixel"' ],
    'xga+'      => [ 1152,  864, 'XGA+' ],
    'olpcmono'  => [ 1200,  900, 'One Laptop Per Child Mono Mode (true res)' ],
    'olpc'      => [ 1200,  900, 'One Laptop Per Child Mono Mode (true res)' ],
    '720i'      => [ 1280,  720, 'HDTV 720 Line Interlaced' ],
    '720p'      => [ 1280,  720, 'HDTV 720 Line Progressive' ],
    'hd720'     => [ 1280,  720, 'HDTV 720 Line' ],
    'xga43'     => [ 1280,  960, '4:3 XGA Alternative' ],
    'xga960'    => [ 1280,  960, '4:3 XGA Alternative' ],
    'sxga'      => [ 1280, 1024, 'Super XGA' ],
    'wxga'      => [ 1366,  768, 'Wide XGA' ],
    'wxga768'   => [ 1366,  768, 'Wide XGA' ],
    'wsxga'     => [ 1440,  900, 'Wide Super XGA (Low Variant)' ],
    'wsxga1'    => [ 1440,  900, 'Wide Super XGA (Low Variant)' ],
    'wxga+'     => [ 1440,  900, 'Wide XGA+' ],
    'sxga+'     => [ 1440, 1050, 'Super XGA+' ],
    'wsxga2'    => [ 1600, 1024, 'Wide Super XGA (High Variant)' ],
    'uxga'      => [ 1600, 1200, 'Ultra XGA' ],
    'wsxga+'    => [ 1680, 1050, 'Wide Super XGA+' ],
    'academy2k' => [ 1828, 1332, 'Digital Film Academy 1.375:1 @ 2K' ],
    '1080i'     => [ 1920, 1080, 'HDTV 1080 Line Interlaced' ],
    '1080p'     => [ 1920, 1080, 'HDTV 1080 Line Progressive' ],
    'hd1080'    => [ 1920, 1080, 'HDTV 1080 Line' ],
    'wuxga'     => [ 1920, 1200, 'Wide Ultra XGA' ],
    'dc2k1'     => [ 1998, 1080, 'Digital Film Digital Cinema 1.85:1 @ 2K ' ],
    'dc2k2'     => [ 2048,  858, 'Digital Film Digital Cinema 2.39:1 @ 2K ' ],
    'eurohd'    => [ 2048, 1152, 'European HDTV' ],
    'qxga'      => [ 2048, 1536, 'Quad XGA' ],
    'wqxga'     => [ 2560, 1600, 'Wide Quad XGA' ],
    'qsxga'     => [ 2560, 2048, 'Quad Super XGA' ],
    'wqsxga'    => [ 3200, 2048, 'Wide Quad Super XGA' ],
    'quxga'     => [ 3200, 2400, 'Quad Ultra XGA' ],
    'academy4k' => [ 3656, 2664, 'Digital Film Academy 1.375:1 @ 4K' ],
    'wquxga'    => [ 3840, 2400, 'Wide Quad Ultra XGA' ],
    'dc4k1'     => [ 3996, 2160, 'Digital Film Digital Cinema 1.85:1 @ 4K ' ],
    'dc4k2'     => [ 4096, 1714, 'Digital Film Digital Cinema 2.39:1 @ 4K ' ],
    'hxga'      => [ 4096, 3072, 'Hexadecatuple XGA' ],
    'whxga'     => [ 5120, 3200, 'Wide Hexadecatuple XGA' ],
    'hsxga'     => [ 5120, 4096, 'Hexadecatuple Super XGA' ],
    'whsxga'    => [ 6400, 4096, 'Wide Hexadecatuple Super XGA' ],
    'huxga'     => [ 6400, 4800, 'Hexadecatuple Ultra XGA' ],
    'whuxga'    => [ 7680, 4800, 'Wide Hexadecatuple Ultra XGA' ],
}

def OGLBench.w_h_from_geometry(geom) 
	geometry = geom.downcase

	return $~[1,2] if geometry =~ /^(\d+)x(\d+)$/

  dims = $KNOWN_RES[geometry] || [0, 0]
	dims[0,2]
end

def OGLBench.show_known_geometries 
	puts "Known geometries:"

	# convert the hash to array, sort by resolution and iterate
	$KNOWN_RES.sort {|a,b| a[1][0,2] <=> b[1][0,2] }.each do |row|
		name, res = row
		x,y,fullname = res
		printf "%-10s  %4d x %4d  %s\n", name, x, y, fullname
	end
end

def OGLBench.show_usage(conf = $CACHED[:conf])
	usage = conf[:usage]

	if (not conf[:extra_usage].empty?)
		conf[:_USAGE_LABEL_GENERAL] = "\nGENERAL OPTIONS:"
		usage = "#{conf[:usage]}\nOTHER OPTIONS:\n#{conf[:extra_usage]}"
	else
		conf[:_USAGE_LABEL_GENERAL] = ''
	end

	usage.gsub!(/\$(\w+)/) do conf[$1.to_sym] end

	print usage
end

def OGLBench.show_basic_config(conf,gl_info,version)
    print <<CONFIG
#{conf[:title]}, version #{version}, with Ruby #{RUBY_VERSION}

window size:      #{conf[:width]} x #{conf[:height]}
full screen:      #{conf[:fs]}
rgba bits:        #{gl_info[:r]} #{gl_info[:g]} #{gl_info[:b]} #{gl_info[:a]}
depth bits:       #{gl_info[:d]}
min frames/test:  #{conf[:frames]}
min seconds/test: #{conf[:seconds]}
CONFIG
end

def OGLBench.friendly_booleans(conf)
	booleans = conf[:booleans].update(conf[:extra_booleans])

	booleans.each_pair do	|logical,readable|
		conf[readable] = (conf[logical] ? 'yes' : 'no')
	end
end

def OGLBench.basic_init(extra_conf = nil,extra_options = nil)
    usage = <<'USAGE';
$title

usage: $0 [options...]
$_USAGE_LABEL_GENERAL
  -f  |--frames=[NNN]          Set minimum frame count [$frames]
  -s  |--seconds=[NNN]         Set minimum time        [$seconds seconds]
  --fs|--fullscreen            Try to use full screen  [$fs]
  -g  |--geometry=[WWW]x[HHH]  Set viewport size       [$width x $height]
  -g  |--geometry=<name>       Set viewport size via well-known name
  -k  |--known-geometries      Show list of known geometry names
  -h  |-?|--help               Show this help message
USAGE

#  conf = {:usage => usage, :frames => "150"}
#  show_usage(conf)
# FIXME: $0 ?

	conf = {
		:title       => 'Ruby-OpenGL Benchmark',
		:usage       => usage,
		:extra_usage => '',
		"0".to_sym   => $0,
	
		:frames      => 100,
		:seconds     => 10,
		:geometry    => '300x300',
		
		:fullscreen  => false,
		:known       => false,
		:help        => false,
		
		:booleans    => {
				:fullscreen => :fs,
				:known      => :show_known,
				:help       => :show_help,
		},
		:extra_booleans => {},
	}

	conf.update(extra_conf) if extra_conf

	opts = GetoptLong.new(
		[ "--frames", "-f", GetoptLong::REQUIRED_ARGUMENT ],
		[ "--seconds", "-s", GetoptLong::REQUIRED_ARGUMENT ],
		[ "--geometry", "-g", GetoptLong::REQUIRED_ARGUMENT ],
		[ "--fullscreen", "--fs", GetoptLong::NO_ARGUMENT ],
		[ "--known", "-k","--known-geometries", GetoptLong::NO_ARGUMENT ],
		[ "--help", "-h",  "-?", GetoptLong::NO_ARGUMENT ]
	)

	opts.each do |opt, arg|
		name = opt.tr('-','')
		if arg.empty?
			conf[name.to_sym] = true
		else
			conf[name.to_sym] = arg 
		end
	end

	friendly_booleans(conf)


  geometry  = conf[:geometry]
  w,h = w_h_from_geometry(geometry)
	conf[:width] = w.to_i
	conf[:height] = h.to_i

	$stdout.sync = true

	if (conf[:help])
		show_usage(conf)
		exit(0)
	end

	if (conf[:known])
		show_known_geometries()
		exit(0)
	end


	app = init_opengl(conf)
	gl_info = get_gl_info(app)

	[conf, app, gl_info]
end


def OGLBench.init_opengl(conf)
	w,h = conf[:width], conf[:height]

	raise "Could not determine sane width and height from '#{conf[:geometry]}'.\n" 	unless w > 0 && h > 0;

	glutInit()
	glutInitDisplayMode(GLUT_RGB | GLUT_DEPTH)
	glutInitWindowSize(w,h)
	app = glutCreateWindow(conf[:title])
	glViewport(0, 0, w, h)

	glMatrixMode(GL_PROJECTION)
	glLoadIdentity

	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity

	$CACHED[:conf] = conf
	$CACHED[:app] = app

	app
end

def OGLBench.get_gl_info(app = $CACHED[:app])
	gl_info = {}

	# These values are faked
	conf = $CACHED[:conf]
	gl_info[:r] = 8
	gl_info[:g] = 8
	gl_info[:b] = 8
	gl_info[:a] = 0
	gl_info[:d] = 24

	$CACHED[:gl_info] = gl_info

	gl_info
end

def OGLBench.fade_to_white(frac)
	glColor4f(frac, frac, frac, 1)
	glClearColor(frac, frac, frac, 1)
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
	glFinish
end

def OGLBench.draw_string(font_style,str,x,y)
  glRasterPos2i(x,y)
	str.each_byte do |char|
    glutBitmapCharacter(font_style, char)
	end
end

# def init_bitmap_font
# def texture_from_texels

end # end module
	