require 'lib/Recursive_Open_Struct'

$cfg = Recursive_Open_Struct.new

$cfg.app.name = "fxri - Instant Ruby Enlightenment"

$cfg.delayed_loading = false

$cfg.search_paths = ENV['FXRI_SEARCH_PATH'] if ENV.has_key?('FXRI_SEARCH_PATH')

# uses the first font that is available
$cfg.app.font.name = ["Bitstream Vera Sans", "Verdana", "Trebuchet MS", "Tahoma", "Arial"]
$cfg.ri_font = ["Bitstream Vera Sans Mono", "Courier New", "Courier"]
$cfg.app.font.size = 8

$cfg.app.width = 760
$cfg.app.height = 480
$cfg.search_delay = 0.1
$cfg.minimum_letters_per_line = 20
$cfg.packet_list_width = 160
$cfg.irb_height = 300
$cfg.status_line_update_interval = 0.1

$cfg.list.opts = ICONLIST_SINGLESELECT|ICONLIST_DETAILED|LAYOUT_FILL_X|LAYOUT_FILL_Y|ICONLIST_AUTOSIZE

# icons, are automatically loaded from Icon_Loader.
$cfg.icons_path = File.join("lib","icons")
$cfg.icons.klass = "class.png"
$cfg.icons.class_method = "module.png"
$cfg.icons.instance_method = "method.png"

# all texts
$cfg.text.search = "%d / %d entries"
$cfg.text.search_field = "What do you want to know?"
$cfg.text.method_name = "name"

# IRB
$cfg.launch_irb = true

$cfg.text.help = %|This is <b>fxri</b>, a graphical interface to the <em>Ruby</em> documentation. Fxri comes with a search engine with quite a few features. Here are several examples:
'<em>Array</em>': Lists all classes with the name <em>Array</em>. Note that upcase words are treated case sensitive, lowercase words insensitive.
'<em>array sort</em>': Everything that contains both <em>array</em> and <em>sort</em> (case insensitive).
'<em>array -Fox</em>': Everything that contain the name <em>array</em> (case insensitive), but not <em>Fox</em> (case sensitive).
After searching just press <em>down</em> to browse the search results. Press <em>Tab</em> to move back into the search field.
If you have any questions, suggestions, problems, please contact the current maintainer with <b>markus.prinz@qsig.org</b>.
Original author: Martin Ankerl (<b>martin.ankerl@gmail.com</b>).|


# prevent further modifications
$cfg.close
