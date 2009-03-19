require File.dirname(__FILE__) + "/../../test_helper"


class IndexWriterTest < Test::Unit::TestCase
  include Ferret::Index
  include Ferret::Analysis

  def setup()
    @dir = Ferret::Store::RAMDirectory.new
    fis = FieldInfos.new()
    fis.create_index(@dir)
  end

  def teardown()
    @dir.close()
  end

  def test_initialize
    wlock = @dir.make_lock(IndexWriter::WRITE_LOCK_NAME)
    clock = @dir.make_lock(IndexWriter::COMMIT_LOCK_NAME)
    assert(! wlock.locked?)
    assert(! clock.locked?)
    iw = IndexWriter.new(:dir => @dir, :create => true)
    assert(@dir.exists?("segments"))
    assert(wlock.locked?)
    iw.close()
    assert(@dir.exists?("segments"))
    assert(! wlock.locked?)
    assert(! clock.locked?)
  end

  def test_add_document
    iw = IndexWriter.new(:dir => @dir,
                         :analyzer => StandardAnalyzer.new(),
                         :create => true)
    iw << {:title => "first doc", :content => ["contents of", "first doc"]}
    assert_equal(1, iw.doc_count)
    iw << ["contents of", "second doc"]
    assert_equal(2, iw.doc_count)
    iw << "contents of third doc"
    assert_equal(3, iw.doc_count)
    iw.close()
  end

  def test_add_documents_fuzzy
    iw = IndexWriter.new(:dir => @dir,
                         :analyzer => StandardAnalyzer.new())
    iw.merge_factor = 3
    iw.max_buffered_docs = 3

    # add 100 documents
    100.times do
      doc = random_doc()
      iw.add_document(doc)
    end
    assert_equal(100, iw.doc_count)
    iw.close()
  end

  private

  WORDS = [
    "desirous", "hollowness's", "camp's", "Senegal", "broadcaster's",
    "pecking", "Provence", "paternalism", "premonition", "Dumbo's",
    "Darlene's", "Elbert's", "substrate", "Camille", "Menkalinan", "Cooper",
    "decamps", "abatement's", "bindings", "scrubby", "subset", "ancestor's",
    "pelagic", "abscissa", "loofah's", "gleans", "boudoir", "disappointingly",
    "guardianship's", "settlers", "Mylar", "timetable's", "parabolic",
    "madams", "bootlegger's", "monotonically", "gage", "Karyn's", "deposed",
    "boozy", "swordfish's", "Chevron", "Victrola", "Tameka", "impels",
    "carrels", "salami's", "celibate", "resistance's", "duration",
    "abscissae", "Kilroy's", "corrosive", "flight's", "flapper", "scare",
    "peppiest", "Pygmies", "Menzies", "wrist's", "enumerable", "housecoats",
    "Khwarizmi's", "stampeding", "hungering", "steeping", "Yemenis",
    "entangles", "solver", "mishapping", "Rand's", "ninety", "Boris",
    "impedimenta", "predators", "ridge", "wretchedness's", "crapping", "Head",
    "Edwards", "Claude's", "geodesics", "verities", "botch", "Short's",
    "vellum's", "coruscates", "hydrogenates", "Haas's", "deceitfulness",
    "cohort's", "Cepheus", "totes", "Cortez's", "napalm", "fruitcake",
    "coordinated", "Coulomb", "desperation", "behoves", "contractor's",
    "vacationed", "Wanamaker's", "leotard", "filtrated", "cringes", "Lugosi",
    "sheath's", "orb", "jawed", "Isidro", "geophysics", "persons", "Asians",
    "booze's", "eight's", "backslappers", "hankered", "dos", "helpings",
    "tough", "interlarding", "gouger", "inflect", "Juneau's", "hay's",
    "sardining", "spays", "Brandi", "depressant", "space", "assess",
    "reappearance's", "Eli's", "Cote", "Enoch", "chants", "ruffianing",
    "moralised", "unsuccessfully", "or", "Maryland's", "mildest", "unsafer",
    "dutiful", "Pribilof", "teas", "vagued", "microbiologists", "hedgerow",
    "speller's", "conservators", "catharsis", "drawbacks", "whooshed",
    "unlawful", "revolve", "craftsmanship", "destabilise", "Margarito",
    "Asgard's", "spawn's", "Annabel's", "canonicals", "buttermilk",
    "exaltation's", "pothole", "reprints", "approximately", "homage",
    "Wassermann's", "Atlantic's", "exacerbated", "Huerta", "keypunching",
    "engagements", "dilate", "ponchos", "Helvetius", "Krakatoa", "basket's",
    "stepmother", "schlock's", "drippings", "cardiology's", "northwesterly",
    "cruddier", "poesies", "rustproof", "climb", "miscalled", "Belgians",
    "Iago", "brownout", "nurseries", "hooliganism's", "concourse's",
    "advocate", "sunrise's", "hyper", "octopus's", "erecting",
    "counterattacking", "redesign", "studies", "nitrating", "milestone",
    "bawls", "Nereid", "inferring", "Ontario's", "annexed", "treasury",
    "cosmogony's", "scandalised", "shindig's", "detention's",
    "Lollobrigida's", "eradicating", "magpie", "supertankers", "Adventist's",
    "dozes", "Artaxerxes", "accumulate", "dankest", "telephony", "flows",
    "Srivijaya's", "fourteen's", "antonym", "rancid", "briefing's",
    "theologian", "Jacuzzi", "gracing", "chameleon's", "Brittney's",
    "Pullmans", "Robitussin's", "jitterier", "mayonnaise's", "fort",
    "closeouts", "amatory", "Drew's", "cockfight", "pyre", "Laura's",
    "Bradley's", "obstructionists", "interventions", "tenderness's",
    "loadstones", "castigation's", "undercut", "volubly", "meditated",
    "Ypsilanti", "Jannie's", "tams", "drummer's", "inaugurations", "mawing",
    "Anglophile", "Sherpa", "footholds", "Gonzalo", "removers",
    "customisation", "procurement's", "allured", "grimaced", "captaining",
    "liberates", "grandeur's", "Windsor", "screwdrivers", "Flynn's",
    "extortionists", "carnivorous", "thinned", "panhandlers", "trust's",
    "bemoaned", "untwisted", "cantors", "rectifies", "speculation",
    "niacin's", "soppy", "condom", "halberd", "Leadbelly", "vocation's",
    "tanners", "chanticleer", "secretariats", "Ecuador's", "suppurated",
    "users", "slag's", "atrocity's", "pillar", "sleeveless", "bulldozers",
    "turners", "hemline", "astounded", "rosaries", "Mallarmé", "crucifies",
    "Maidenform", "contribution", "evolve", "chemicals", "uteri",
    "expostulation", "roamers", "daiquiris", "arraignment", "ribs", "King's",
    "Persepolis", "arsenic's", "blindfolds", "bloodsucker's", "restocks",
    "falconry", "Olympia's", "Colosseum's", "vigils", "Louie's",
    "unwillingly", "sealed", "potatoes", "Argentine", "audit's", "outworn",
    "boggles", "likely", "alleging", "Tinkerbell", "redistribution's",
    "Normandy", "Cortes", "porter's", "buntings", "cornucopias", "rosewoods",
    "shelf's", "airdrops", "summits", "Rosalyn", "redecorating", "twirlers",
    "monsters", "directed", "semiautomatics", "Foch", "Hobart", "mutilates",
    "Wilma's", "ornamenting", "Clifford's", "pyromania", "Strasbourg",
    "bleeders", "additions", "super", "effortlessly", "piecing", "vacations",
    "gybes", "warranted", "Ting", "her", "histrionic", "marshaled", "spore's",
    "villainy's", "brat", "confusion", "amphitheatre's", "adjourns",
    "guzzled", "Visayans", "rogue's", "morsels", "candlestick", "flaks",
    "Waterbury", "pulp's", "endorser's", "postdoc", "coffining", "swallowing",
    "Wrangell", "Marcie's", "Marley", "untapped", "fear's", "Kant",
    "pursuit's", "normally", "jackals", "orals", "Paramaribo's", "Marilyn's",
    "Diem's", "narrower", "medicinally", "chickweed's", "pretentiousness",
    "Lardner", "baritone's", "purrs", "Pam's", "pestles", "Philip's",
    "Titania", "eccentrics", "Albion's", "greed's", "raggediest",
    "importations", "Truman", "incentives", "typified", "incurred",
    "bandstands", "Minnie's", "pleasant", "Sandy's", "perplexities",
    "crease's", "obliques", "backstop", "Nair's", "perusing", "Quixote's",
    "sicknesses", "vapour's", "butte", "lariats", "disfavours", "McGuffey",
    "paediatric", "filtered", "whiff's", "gunboats", "devolved",
    "extravaganza's", "organism", "giggling", "citadel's", "counterbalances",
    "executrixes", "Cathay", "marshmallow's", "iniquitous", "Katmai", "Siva",
    "welled", "impertinence's", "plunger", "rice", "forgers", "Larousse",
    "pollution's", "medium", "residue's", "rumbas", "Odis", "arrogant",
    "Jasper's", "panged", "doubted", "vistaing", "decibel's", "modulus's",
    "chickpea's", "mugger's", "potentates", "sequesters", "academy's",
    "Turk's", "pharmacology's", "defogger", "clomp", "soulless", "elastic",
    "la's", "shards", "unfortunate", "counterclaim's", "objections", "towel",
    "converged", "z", "ionisation", "stirrups", "antiquarians", "constructor",
    "virtuosity's", "Göteborg", "centigramme's", "translators", "dalliance's",
    "us", "bullfight", "drawer's", "nonconformist", "handcrafts", "Magritte",
    "tulle", "plant's", "routine", "colour's", "latency's", "repertoire's",
    "photocopies", "catalyse", "ashrams", "lagging", "flapjack's",
    "ayatollahs", "decentest", "pitted", "conformity", "jack", "batsman",
    "electrifies", "Unitarians", "obtain", "medicates", "tumour's",
    "nutritionally", "haystack", "bustles", "slut", "satirising", "birettas",
    "starring", "Kubrick's", "flogs", "chequering", "Menkalinan's",
    "Barbados's", "Bioko", "swinish", "hades", "perjured", "timing's",
    "cocaine", "ejecting", "rationalises", "dilettante's", "umping",
    "capsized", "frogmen", "matt", "prostituting", "bola's", "devolution's",
    "poxing", "Maritza's", "snob's", "scoped", "Costco", "feral", "sirocco",
    "rebating", "truculence", "junkier", "nabs", "elicit", "allegiance",
    "care", "arteriosclerosis's", "nonproliferation's", "doxologies",
    "disconsolate", "bodega", "designers", "Rembrandt", "apostasies",
    "garrulousness", "Hertzsprung's", "hayseeds", "noncooperation's",
    "resentment", "cuticles", "sandboxes", "gimmicks", "magnolia",
    "invalidity's", "pulverised", "Tinkerbell's", "hypoglycemics",
    "gunboat's", "workbench's", "fleetingly's", "sportsman's", "trots",
    "decomposes", "discrepancies", "owls", "obscener", "organic", "stoutness",
    "councillor's", "Philippine's", "Aline", "coarsening", "suffocated",
    "infighting's", "peculiarity", "roof's", "premier", "sucked", "churl",
    "remounts", "intends", "wiles", "unfold", "unperturbed", "wainscotings",
    "restfuller", "ashtray's", "wader's", "decanters", "gild", "tandems",
    "spooked", "galling", "annuity's", "opacity", "clamour's", "flaccid",
    "caroming", "savvying", "mammalian's", "toadstool's", "doohickey", "jibs",
    "conquests", "dishes", "effusively", "distinctions", "curly", "Peckinpah",
    "whining", "quasar", "sponge", "infrequent", "Novembers", "cowling",
    "poem's", "muzzles", "Sufi", "authoritarians", "prompts", "Gavin's",
    "morphology's", "shenanigan", "narrated", "rapprochement", "Heine",
    "propane's", "addition", "prefect's", "pining", "dwindles",
    "compulsiveness's", "objectors", "trudging", "segregates", "language",
    "enthralled", "explosiveness", "toeing", "drainers", "Merrimack's",
    "smarten", "bigwig's", "embroiders", "Medicaids", "grammar's", "behest's",
    "chiseled", "equalled", "factual", "Casablanca's", "dams",
    "disillusioned", "turtleneck", "Baden", "provinces", "bushwhacked", "fey",
    "Yangtze", "loan's", "decent", "strobe", "challenger's", "hometown",
    "Neal", "Ernestine's", "magnetises", "minute", "patrol", "Starbucks",
    "Bernstein", "signal", "interplanetary", "tweak", "archdeacon",
    "untoward", "transducer", "azaleas", "levied", "worlds", "talks",
    "Tancred", "hairsplitting's", "edibility's", "confab", "rosetted",
    "Spanish", "Americanisation", "Charley", "realm's", "incongruities",
    "chinstraps", "dollhouses", "binocular", "popgun", "physiotherapy's",
    "knave's", "angelically", "heartbreaking", "clarions", "bespeaks",
    "pivotal", "Zosma", "ungrammatical", "dilution", "tidily", "Dejesus's",
    "taller", "pennyweight's", "freshman", "Jamestown", "chiefer", "amen",
    "attiring", "appurtenance's", "opiates", "mottoes", "towellings", "ashen",
    "font's", "spoors", "pupil", "groom's", "skimpy", "achieves",
    "intolerance's", "ardour's", "exorcist", "bottoming", "snag's",
    "Frenches", "hysteric's", "ladyfinger's", "differences", "seed",
    "clubfoot's", "glades", "Elton's", "jargon", "Waldo", "grinning",
    "coherence's", "winos", "turnround", "appended", "Ethelred's", "delete",
    "steadfastness's", "miss", "thermoplastic", "depraves", "unctuous",
    "reanimates", "transfusing", "protects", "Babbage's", "foists", "inn",
    "etched", "sanctimoniously", "idling", "timepiece", "holistic",
    "waterside", "ulna's", "swindled", "employables", "zebra", "nieces",
    "pertained", "usages", "vamp's", "Larry's", "cooler's", "holographs",
    "clewing", "stubborning", "peaked", "underfeeds", "marshmallows",
    "agreeable", "beards", "Slovenia's", "nitroglycerin", "palls", "impurer",
    "armours", "stomachaches", "notification's", "Dixieland's", "crozier's",
    "neurotic", "kudos", "Tania's", "M", "soundtrack's", "territory's",
    "sped", "house's", "divisibility", "ingress's", "pummelled", "Isabel",
    "Dewitt", "seemly", "hutched", "calliope", "lengthwise", "flubs",
    "Moldavia's", "Mercia", "McBride's", "Lenten", "pulverise", "football",
    "oligarchy", "Max", "scribbler", "acclimatize", "brainwashes",
    "apprenticed", "benevolences", "two", "Wodehouse", "crew's", "massacre",
    "proportionals", "Jewishness's", "instep's", "emissary", "folder",
    "nonentity's", "convinced", "caption", "kangarooed", "dogie",
    "vagabonding", "auction's", "appraising", "antimony", "part's",
    "longitude's", "inconsiderateness's", "pawning", "serer", "solos",
    "histories", "mushy", "parturition", "munched", "oregano", "inanest",
    "dryness", "kitchenware", "unexpected", "covens", "cheesecakes",
    "stakeout's", "Pulaski's", "Yoknapatawpha's", "pinhead", "drifted",
    "guzzler's", "funking", "sou'wester", "oesophagus's", "highbrow",
    "contralto", "meningitis", "Mazzini", "raggedest", "vaginas", "misfiring",
    "margaritas", "wedder", "pointed", "slicked", "garlanded", "comeuppances",
    "vassals", "Sui", "Concord", "bozos", "Garry's", "Maribel's", "epileptic",
    "Jehoshaphat's", "revolutionary's", "kneecaps", "songbird", "actively",
    "Meredith", "toddler", "distrusting", "fuchsias", "perusal", "instills",
    "deathbed", "sunspot's", "spatula's", "Muscovy", "humaniser", "Keats",
    "regrets", "deflect", "theories", "nonpluses", "populating", "leniency's",
    "penicillin's", "gaol's", "borough", "moose's", "dogmata",
    "transcendentally", "supposition's", "nursed", "Gagarin's", "honest",
    "Chandrasekhar's", "mudslinger's", "parable", "bonged", "Wyeth's",
    "Ochoa's", "Grenoble", "steamy", "halter's", "rotisserie's", "pagoda's",
    "wallaby's", "Yank", "pretzel", "rapist's", "estrange", "hectored",
    "Puebla's", "conniver", "creditor's", "dole's", "Fotomat", "patents",
    "heckling", "thickener", "etches", "yogi", "hemstitched", "obverses",
    "Lipizzaner", "divert", "Strong's", "sagest", "Alabama", "He", "Carrie's",
    "obligation's", "verity's", "outed", "Rhee", "bluffed", "codas",
    "crèche's", "unpalatable", "dilettanti", "vestment", "purse's",
    "inflammation's", "bookmarked", "doing's", "whinnying", "impersonators",
    "Theiler", "scurried", "resistor", "southerners", "Anacreon",
    "reconstruction's", "footage", "trespassing", "Kafka", "bottling",
    "stays", "Gretzky", "overburdening", "princesses", "weathercock's",
    "atolls", "cheerier", "packet", "surrenders", "teacup", "Sabik's",
    "undecidable", "lollygagged", "pawl's", "anaesthesiology", "sublimely",
    "contortionists", "motorcades", "Maureen", "lamasery", "yourselves",
    "Creighton", "poliomyelitis's", "civil", "outmanoeuvre", "lauded",
    "closeness", "Humboldt's", "pretzels", "ungrudging", "blackguard's",
    "sickles", "typo", "narcotics", "linesman", "psychotics", "pictured",
    "deviltry", "Yahtzee", "Lovelace's", "cerebra", "airiness's", "bewitch",
    "how", "motherland's", "crate's", "Keenan's", "turnstile's",
    "pedometer's", "carted", "slipping", "fallow", "Canadian", "ladybird's",
    "thump", "shopper's", "enters", "scowls", "nematode", "focused",
    "Riley's", "grainiest", "novas", "snuffled", "leftovers", "deify",
    "Samoan", "pruning", "contenting", "Khachaturian's", "triads",
    "genealogies", "psalmist", "shaming", "appropriated", "ignominies",
    "Beadle's", "MHz", "peerages", "facile", "Seoul", "Janna's", "jig's",
    "mousiness's", "funnier", "delimiter", "watermark", "sheik's", "Reasoner",
    "ipecac's", "curdles", "wronged", "Segovia's", "solders", "Dunne's",
    "contractor", "awards", "hostels", "pinkie's", "Herzl", "misplace",
    "shuttle", "innovative", "vestries", "cosmoses", "trikes", "Casandra's",
    "hokier", "carouser's", "summerhouses", "renascence", "decomposed",
    "Balzac's", "outlast", "shod", "squalling", "smugging", "weighing",
    "omega's", "selects", "fleetingly", "Finland", "petted", "disrespects",
    "fetter", "confound", "brads", "Bosnia's", "preposition's", "guy's",
    "different", "tracts", "paediatrics's", "polygon", "eyetooth's", "Aesop",
    "pentagons", "professions", "homeowner", "looter's", "intimidated",
    "lustre's", "loneliness", "catnapped", "counties", "pailful",
    "Christendom's", "Barents", "penis", "Mumford's", "Nigel", "éclairs",
    "splats", "diabolical", "popularly", "quart", "abjected", "Rasalgethi",
    "camel's", "inimical", "overweening", "distention's", "Advil", "casement",
    "seamier", "avaricious", "sierra's", "caparison's", "moldered", "Cortez",
    "handmaid's", "disappointment", "billowed", "overpopulated", "outsets",
    "ray", "smoother", "overkill", "somber", "tiller's", "zigzag", "adviser",
    "absorption's", "sturdily", "hairy", "bloodmobile", "investiture's",
    "creature", "ripeness's", "Jonathon", "arborvitae's", "skulduggery",
    "bog", "skeleton's", "Kit's", "Panamas", "Ashlee's", "jazzy", "snit",
    "divisive", "caribous", "permuting", "frankest", "annotated", "oak's",
    "meg's", "Gill", "burrito", "dormancy's", "offings", "Nike",
    "outnumbered", "skater's", "Portugal", "deficit", "Cannon's", "pockmark",
    "sediment's", "mailbox", "innuendoed", "retire", "wolfhound's",
    "nicotine's", "brigade's", "mettle's", "softhearted", "hooey's",
    "abdication", "Orval", "Jaime", "ship", "hyphenations", "sectarians",
    "Alabaman", "tagging", "ultras", "schizoids", "medicines", "undersized",
    "Gray", "maternity's", "bandaging", "scooping", "coercion's", "serapes",
    "celebrate", "Listerine's", "throve", "crypt's", "nearsighted",
    "metallurgists", "Delicious", "cotton's", "yoked", "cogitates",
    "underage", "cigarette's", "hallways", "Cointreau", "ma'am", "spacing's",
    "foresight", "parkway's", "Edwardian", "mediator", "Turner", "Derrida's",
    "motorist's", "hobo", "equivalences", "sophism", "peeping", "telescoped",
    "overproduce", "ductility", "Leblanc", "refractory", "passé", "decodes",
    "womanising", "flax's", "pond's", "infrequency", "talkativeness's",
    "settlement's", "Prince", "bating", "multimillionaire", "Schultz",
    "premiss", "quackery", "bathhouse", "Leno's", "Monday's", "Hung's",
    "undaunted", "bewaring", "tension's", "Chile's", "Rostand's", "platoons",
    "rodeo's", "Dionne", "Dyson's", "gingivitis's", "fewer",
    "electromagnetism's", "scrubbier", "ensconced", "wretcheder", "mica's",
    "expectorant", "snapper's", "chastised", "habitation", "spry", "bathing",
    "stealth's", "champagnes", "baleful", "fencing's", "threaded", "codicils",
    "disgraced", "redcaps", "addends", "Olivier", "clasped", "Gwendolyn",
    "foment", "angularity's", "strenuously", "gorilla", "misbehaved",
    "surplus's", "newsier", "positioned", "bloodmobiles", "circumstantials",
    "person's", "varicose", "Calliope", "plethora", "Olmsted",
    "reconciliation", "Brendan's", "beset", "totters", "sailors",
    "parliamentarians", "Whitaker", "hilts", "pummelling", "academician's",
    "ruse", "discreeter", "appetisingly", "perfections", "anus", "overrode",
    "pedantry's", "possessed", "germs", "unscrews", "expired",
    "semitrailer's", "Cupid's", "nonsmoker", "Marathon", "secs", "Hopkins",
    "freeing", "libelled", "furious", "staccatos", "electroencephalogram's",
    "malingerer's", "impulses", "briars", "Tran", "hilltops", "sulks",
    "quailed", "fads", "retrenches", "spouted", "outtake", "puncture's",
    "rats", "kibitzed", "berets", "omnivorous", "flange", "Mons", "glints",
    "mansards", "thou", "cuing", "suspected", "Kaiser's", "savvier", "skits",
    "interdict's", "Booker", "Rubinstein", "Tm's", "crossing's", "dewlap",
    "guarantor's", "edification's", "joyfullest", "crossed", "chowdering",
    "sillier", "reloading", "commodity's", "bodkins", "conduced", "coughs",
    "nucleus's", "sixtieth", "proverbially", "comprehensive", "ineluctably",
    "patrolmen", "resuscitating", "carpetbag's", "Darrin's", "Yeager",
    "Bataan's", "spoonsful", "proceeds", "wrongdoer", "Karroo", "heart",
    "poison", "typifying", "endowment's", "aquanauts", "deaconesses",
    "homosexuality", "Maxine", "haunching", "centred", "Peking's",
    "toothiest", "growers", "firebombs", "throbs", "Downy", "contribution's",
    "sago's", "Cole", "Knoxville", "leftmost", "Nell's", "Baffin", "barrings",
    "contagions", "disencumbers", "countdown", "quintuple", "perihelion",
    "creationism's", "actioning", "admiralty", "Mt's", "durability's",
    "sewer's", "replicas", "oxide", "ripened", "Pisces's", "Cinerama's",
    "catheters", "oppressive", "roosting", "foggiest", "properly", "Kareem",
    "Ollie", "minuted", "vehicles", "eel", "remunerates", "swashbuckler's",
    "remunerative", "sanguining", "Belem's", "forlornly", "rudders",
    "officialdom", "countertenors", "Upton", "whoop", "animations", "arouses",
    "millionths", "videocassette", "fledgling", "shake", "exterminated",
    "Cain's", "trendiest", "wariest", "torpedoes", "airmails", "Cameron's",
    "discord's", "spitefulness's", "thudded", "menaced", "takeovers",
    "solicited", "wallpapers", "economic", "cache", "rechargeable", "gongs",
    "droning", "exemption", "Alaskans", "toothed", "snifter", "Stephens",
    "prejudge", "doctor's", "bobolinks", "rotates", "valuation's", "narrator",
    "weaning", "uncle", "shelter", "destitution's", "Edgardo's", "gauge",
    "Nice", "Adolf's", "rheumatics", "inheritances", "undesirables",
    "Eileen's", "flyweight's", "scope", "possessiveness", "tipsily",
    "effulgence", "rematch", "Baltic", "unsteadiest", "rodeos", "gloaming's",
    "ringers", "randomised", "commissars", "destroyer's", "router",
    "disengaging", "it's", "Albert", "rampantly", "varmint", "Adkins",
    "chevron", "insomniac", "bobsledded", "masochist's", "chronometers",
    "compaction", "Mauro", "sidled", "Highlander's", "snail's", "syllabifies",
    "application's", "symmetrical", "blacking", "accent's", "sentimentalists",
    "sonatas", "profanities", "sloping", "Araby", "percolate", "repeated",
    "youthfulness's", "Loyola", "deliriously", "matriarch's", "tailors",
    "rerouting", "hairpin", "dispersal", "endowment", "disquieting", "swat",
    "neckerchieves", "wrinkles", "amoebas", "Darcy", "orthodontics's",
    "milder", "sneezing", "prescience's", "pads", "wrought", "perspicuity's",
    "materialist", "pull", "laundryman's", "lazily", "protractor's", "Vic",
    "photocopier", "guardrooms", "cablecasting", "confirms", "excretions",
    "combatant", "counterfeiters", "periwig", "genteelest", "router's",
    "springy", "procreated", "syphon", "parent's", "bigwigs", "rebelled",
    "milkmaids", "McGee's", "seaworthier", "Bellatrix's", "tenement",
    "embryologists", "Vaselining", "burrow's", "tonnage's", "Petty's",
    "chancels", "scouring", "mouser", "recompensed", "guarding", "editor",
    "raster", "bourgeoisie's", "interpolating", "skinflint's", "transport",
    "bullfinch", "needlessly", "withholds", "counterclockwise", "panicking",
    "Ahriman", "flambeing", "contrary", "heartstrings", "whittled", "crib's",
    "highlighter", "extroverted", "Martinique's", "racquets", "Maldivian",
    "physiognomy", "Hammarskjold", "massage", "shingling", "neighbourhood",
    "boobed", "vulture", "intercontinental", "cobblers", "peddlers",
    "forthrightly", "germicide", "raindrop's", "fir's", "decaffeinates",
    "wobblier", "abnegated", "cruiser's", "satiety", "trilled", "impending",
    "gulf", "mountebank", "beltway", "reappointment", "cinematographer",
    "pylon", "penthouses", "morally", "installs", "Walsh's", "drawstring",
    "circus's", "Khayyam's", "Myrtle's", "ventrals", "category's",
    "opportunistic", "grovelling", "warier", "upchuck", "hairdresser's",
    "Montanans", "jobber", "dazzle", "encirclement's", "muffin's", "coronets",
    "focus's", "footfall's", "subjunctives", "late", "pedagogued",
    "dignitaries", "content", "blockbusters", "reminiscent", "mayor",
    "specifier", "extinction", "nutshell's", "catbird's", "bundle",
    "gracefulness", "exceed", "estranges", "chancy", "bankrupted", "Avery",
    "Barnett", "succulence", "stacking", "ensnare", "truck", "embargo",
    "persecutes", "translation's", "muskrat's", "illumines", "undercoat's",
    "fleecier", "brick", "qualities", "imprecision", "reprisals", "discounts",
    "harmonics", "Mann's", "terrorism", "interminable", "Santiago's",
    "deepness", "tramples", "golder", "voyeurism's", "tent", "particle's",
    "minuend", "waxwings", "knobby", "trustee", "funnily", "hotheadedness's",
    "Kristin", "what", "bite", "murmur's", "pustule's", "weeknights",
    "rocked", "athlete", "ventilates", "impresses", "daguerreotyping",
    "Gross", "gambols", "villa", "maraud", "disapproval", "apostrophe's",
    "sheaf", "noisemaker's", "autonomy's", "massing", "daemon's", "Thackeray",
    "fermenting", "whammy", "philosophise", "empathy", "calamities",
    "sunbathe", "Qom", "yahoo's", "coxcomb's", "move", "school's",
    "rainmakers", "shipwreck", "potbelly's", "courageously", "current",
    "Aleut", "treaties", "U", "always", "Bosch", "impregnating", "bud's",
    "carat", "centrists", "acquaintance's", "convoy's", "chichis",
    "restraint's", "Cosby", "factotums", "handshaking", "paragon's",
    "mileages", "Tammie", "cartoonists", "lemmas", "lowliness's", "onion's",
    "E's", "Bible", "Cranmer", "fob's", "minks", "overstocking", "Willamette",
    "needle's", "scuppers", "Carborundum", "upwardly", "tallies", "aptitude",
    "synod", "nasturtium's", "Pensacola", "snappish", "merino", "sups",
    "fingerboard's", "prodigy's", "narcissism's", "substantial", "lug",
    "establishing", "Vergil's", "patrimonies", "shorted", "forestation",
    "undeniable", "Katmandu", "lamination", "trollop's", "odd", "stanza",
    "paraplegic", "melanin", "Rico", "foreman", "stereotypes", "affinity's",
    "cleansing", "sautéing", "epochs", "crooners", "manicured", "undisclosed",
    "propel", "usage", "Alioth's", "Aurelia's", "peruse", "Vassar's",
    "Demosthenes's", "Brazos", "supermarket", "scribbles", "Jekyll's",
    "discomfort's", "mastiffs", "ballasting", "Figueroa", "turnstiles",
    "convince", "Shelton's", "Gustavo", "shunting", "Fujitsu's", "fining's",
    "hippos", "dam's", "expressionists", "peewee", "troop's"
  ]
  WORDS_SIZE = WORDS.size

  def random_word
    return WORDS[rand(WORDS_SIZE)]
  end

  def random_sentence(max_len)
    sentence = ""
    (1 + rand(max_len)).times { sentence << " " << random_word }
    return sentence
  end

  def random_doc(max_fields = 10, max_elements = 10, max_len = 100)
    doc = {}
    (1 + rand(max_fields)).times do
      field = random_word.intern
      elem_count = rand(max_elements) + 1
      if (elem_count == 1)
        doc[field] = random_sentence(max_len)
      else
        doc[field] = []
        elem_count.times { doc[field] << random_sentence(max_len)}
      end
    end
    return doc
  end
end
