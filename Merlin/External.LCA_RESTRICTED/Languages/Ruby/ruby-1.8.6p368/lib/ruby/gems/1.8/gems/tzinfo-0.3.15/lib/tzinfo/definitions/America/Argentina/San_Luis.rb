require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module America
      module Argentina
        module San_Luis
          include TimezoneDefinition
          
          timezone 'America/Argentina/San_Luis' do |tz|
            tz.offset :o0, -15924, 0, :LMT
            tz.offset :o1, -15408, 0, :CMT
            tz.offset :o2, -14400, 0, :ART
            tz.offset :o3, -14400, 3600, :ARST
            tz.offset :o4, -10800, 0, :ART
            tz.offset :o5, -10800, 3600, :ARST
            tz.offset :o6, -14400, 0, :WART
            tz.offset :o7, -14400, 3600, :WARST
            
            tz.transition 1894, 10, :o1, 17374555327, 7200
            tz.transition 1920, 5, :o2, 1453467407, 600
            tz.transition 1930, 12, :o3, 7278935, 3
            tz.transition 1931, 4, :o2, 19411461, 8
            tz.transition 1931, 10, :o3, 7279889, 3
            tz.transition 1932, 3, :o2, 19414141, 8
            tz.transition 1932, 11, :o3, 7281038, 3
            tz.transition 1933, 3, :o2, 19417061, 8
            tz.transition 1933, 11, :o3, 7282133, 3
            tz.transition 1934, 3, :o2, 19419981, 8
            tz.transition 1934, 11, :o3, 7283228, 3
            tz.transition 1935, 3, :o2, 19422901, 8
            tz.transition 1935, 11, :o3, 7284323, 3
            tz.transition 1936, 3, :o2, 19425829, 8
            tz.transition 1936, 11, :o3, 7285421, 3
            tz.transition 1937, 3, :o2, 19428749, 8
            tz.transition 1937, 11, :o3, 7286516, 3
            tz.transition 1938, 3, :o2, 19431669, 8
            tz.transition 1938, 11, :o3, 7287611, 3
            tz.transition 1939, 3, :o2, 19434589, 8
            tz.transition 1939, 11, :o3, 7288706, 3
            tz.transition 1940, 3, :o2, 19437517, 8
            tz.transition 1940, 7, :o3, 7289435, 3
            tz.transition 1941, 6, :o2, 19441285, 8
            tz.transition 1941, 10, :o3, 7290848, 3
            tz.transition 1943, 8, :o2, 19447501, 8
            tz.transition 1943, 10, :o3, 7293038, 3
            tz.transition 1946, 3, :o2, 19455045, 8
            tz.transition 1946, 10, :o3, 7296284, 3
            tz.transition 1963, 10, :o2, 19506429, 8
            tz.transition 1963, 12, :o3, 7315136, 3
            tz.transition 1964, 3, :o2, 19507645, 8
            tz.transition 1964, 10, :o3, 7316051, 3
            tz.transition 1965, 3, :o2, 19510565, 8
            tz.transition 1965, 10, :o3, 7317146, 3
            tz.transition 1966, 3, :o2, 19513485, 8
            tz.transition 1966, 10, :o3, 7318241, 3
            tz.transition 1967, 4, :o2, 19516661, 8
            tz.transition 1967, 10, :o3, 7319294, 3
            tz.transition 1968, 4, :o2, 19519629, 8
            tz.transition 1968, 10, :o3, 7320407, 3
            tz.transition 1969, 4, :o2, 19522541, 8
            tz.transition 1969, 10, :o4, 7321499, 3
            tz.transition 1974, 1, :o5, 128142000
            tz.transition 1974, 5, :o4, 136605600
            tz.transition 1988, 12, :o5, 596948400
            tz.transition 1989, 3, :o4, 605066400
            tz.transition 1989, 10, :o5, 624423600
            tz.transition 1990, 3, :o6, 637380000
            tz.transition 1990, 10, :o7, 655963200
            tz.transition 1991, 3, :o6, 667796400
            tz.transition 1991, 6, :o4, 675748800
            tz.transition 1999, 10, :o7, 938919600
            tz.transition 2000, 3, :o4, 952052400
            tz.transition 2004, 5, :o6, 1085972400
            tz.transition 2004, 7, :o4, 1090728000
            tz.transition 2007, 12, :o5, 1198983600
            tz.transition 2008, 1, :o7, 1200880800
            tz.transition 2008, 3, :o6, 1205031600
            tz.transition 2008, 10, :o7, 1223784000
            tz.transition 2009, 3, :o6, 1236481200
            tz.transition 2009, 10, :o7, 1255233600
            tz.transition 2010, 3, :o6, 1268535600
            tz.transition 2010, 10, :o7, 1286683200
            tz.transition 2011, 3, :o6, 1299985200
            tz.transition 2011, 10, :o7, 1318132800
            tz.transition 2012, 3, :o6, 1331434800
            tz.transition 2012, 10, :o7, 1350187200
            tz.transition 2013, 3, :o6, 1362884400
            tz.transition 2013, 10, :o7, 1381636800
            tz.transition 2014, 3, :o6, 1394334000
            tz.transition 2014, 10, :o7, 1413086400
            tz.transition 2015, 3, :o6, 1425783600
            tz.transition 2015, 10, :o7, 1444536000
            tz.transition 2016, 3, :o6, 1457838000
            tz.transition 2016, 10, :o7, 1475985600
            tz.transition 2017, 3, :o6, 1489287600
            tz.transition 2017, 10, :o7, 1507435200
            tz.transition 2018, 3, :o6, 1520737200
            tz.transition 2018, 10, :o7, 1539489600
            tz.transition 2019, 3, :o6, 1552186800
            tz.transition 2019, 10, :o7, 1570939200
            tz.transition 2020, 3, :o6, 1583636400
            tz.transition 2020, 10, :o7, 1602388800
            tz.transition 2021, 3, :o6, 1615690800
            tz.transition 2021, 10, :o7, 1633838400
            tz.transition 2022, 3, :o6, 1647140400
            tz.transition 2022, 10, :o7, 1665288000
            tz.transition 2023, 3, :o6, 1678590000
            tz.transition 2023, 10, :o7, 1696737600
            tz.transition 2024, 3, :o6, 1710039600
            tz.transition 2024, 10, :o7, 1728792000
            tz.transition 2025, 3, :o6, 1741489200
            tz.transition 2025, 10, :o7, 1760241600
            tz.transition 2026, 3, :o6, 1772938800
            tz.transition 2026, 10, :o7, 1791691200
            tz.transition 2027, 3, :o6, 1804993200
            tz.transition 2027, 10, :o7, 1823140800
            tz.transition 2028, 3, :o6, 1836442800
            tz.transition 2028, 10, :o7, 1854590400
            tz.transition 2029, 3, :o6, 1867892400
            tz.transition 2029, 10, :o7, 1886644800
            tz.transition 2030, 3, :o6, 1899342000
            tz.transition 2030, 10, :o7, 1918094400
            tz.transition 2031, 3, :o6, 1930791600
            tz.transition 2031, 10, :o7, 1949544000
            tz.transition 2032, 3, :o6, 1962846000
            tz.transition 2032, 10, :o7, 1980993600
            tz.transition 2033, 3, :o6, 1994295600
            tz.transition 2033, 10, :o7, 2012443200
            tz.transition 2034, 3, :o6, 2025745200
            tz.transition 2034, 10, :o7, 2043892800
            tz.transition 2035, 3, :o6, 2057194800
            tz.transition 2035, 10, :o7, 2075947200
            tz.transition 2036, 3, :o6, 2088644400
            tz.transition 2036, 10, :o7, 2107396800
            tz.transition 2037, 3, :o6, 2120094000
            tz.transition 2037, 10, :o7, 2138846400
            tz.transition 2038, 3, :o6, 19723973, 8
            tz.transition 2038, 10, :o7, 7397120, 3
            tz.transition 2039, 3, :o6, 19726885, 8
            tz.transition 2039, 10, :o7, 7398212, 3
            tz.transition 2040, 3, :o6, 19729797, 8
            tz.transition 2040, 10, :o7, 7399325, 3
            tz.transition 2041, 3, :o6, 19732709, 8
            tz.transition 2041, 10, :o7, 7400417, 3
            tz.transition 2042, 3, :o6, 19735621, 8
            tz.transition 2042, 10, :o7, 7401509, 3
            tz.transition 2043, 3, :o6, 19738533, 8
            tz.transition 2043, 10, :o7, 7402601, 3
            tz.transition 2044, 3, :o6, 19741501, 8
            tz.transition 2044, 10, :o7, 7403693, 3
            tz.transition 2045, 3, :o6, 19744413, 8
            tz.transition 2045, 10, :o7, 7404785, 3
            tz.transition 2046, 3, :o6, 19747325, 8
            tz.transition 2046, 10, :o7, 7405898, 3
            tz.transition 2047, 3, :o6, 19750237, 8
            tz.transition 2047, 10, :o7, 7406990, 3
            tz.transition 2048, 3, :o6, 19753149, 8
            tz.transition 2048, 10, :o7, 7408082, 3
            tz.transition 2049, 3, :o6, 19756117, 8
            tz.transition 2049, 10, :o7, 7409174, 3
            tz.transition 2050, 3, :o6, 19759029, 8
          end
        end
      end
    end
  end
end
