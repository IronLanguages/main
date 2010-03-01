require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Africa
      module Tunis
        include TimezoneDefinition
        
        timezone 'Africa/Tunis' do |tz|
          tz.offset :o0, 2444, 0, :LMT
          tz.offset :o1, 561, 0, :PMT
          tz.offset :o2, 3600, 0, :CET
          tz.offset :o3, 3600, 3600, :CEST
          
          tz.transition 1881, 5, :o1, 52017389389, 21600
          tz.transition 1911, 3, :o2, 69670267013, 28800
          tz.transition 1939, 4, :o3, 29152433, 12
          tz.transition 1939, 11, :o2, 29155037, 12
          tz.transition 1940, 2, :o3, 29156225, 12
          tz.transition 1941, 10, :o2, 29163281, 12
          tz.transition 1942, 3, :o3, 58330259, 24
          tz.transition 1942, 11, :o2, 58335973, 24
          tz.transition 1943, 3, :o3, 58339501, 24
          tz.transition 1943, 4, :o2, 4861663, 2
          tz.transition 1943, 4, :o3, 58340149, 24
          tz.transition 1943, 10, :o2, 4862003, 2
          tz.transition 1944, 4, :o3, 58348405, 24
          tz.transition 1944, 10, :o2, 29176457, 12
          tz.transition 1945, 4, :o3, 58357141, 24
          tz.transition 1945, 9, :o2, 29180573, 12
          tz.transition 1977, 4, :o3, 231202800
          tz.transition 1977, 9, :o2, 243903600
          tz.transition 1978, 4, :o3, 262825200
          tz.transition 1978, 9, :o2, 276044400
          tz.transition 1988, 5, :o3, 581122800
          tz.transition 1988, 9, :o2, 591145200
          tz.transition 1989, 3, :o3, 606870000
          tz.transition 1989, 9, :o2, 622594800
          tz.transition 1990, 4, :o3, 641516400
          tz.transition 1990, 9, :o2, 654649200
          tz.transition 2005, 4, :o3, 1114902000
          tz.transition 2005, 9, :o2, 1128038400
          tz.transition 2006, 3, :o3, 1143334800
          tz.transition 2006, 10, :o2, 1162083600
          tz.transition 2007, 3, :o3, 1174784400
          tz.transition 2007, 10, :o2, 1193533200
          tz.transition 2008, 3, :o3, 1206838800
          tz.transition 2008, 10, :o2, 1224982800
          tz.transition 2010, 3, :o3, 1269738000
          tz.transition 2010, 10, :o2, 1288486800
          tz.transition 2011, 3, :o3, 1301187600
          tz.transition 2011, 10, :o2, 1319936400
          tz.transition 2012, 3, :o3, 1332637200
          tz.transition 2012, 10, :o2, 1351386000
          tz.transition 2013, 3, :o3, 1364691600
          tz.transition 2013, 10, :o2, 1382835600
          tz.transition 2014, 3, :o3, 1396141200
          tz.transition 2014, 10, :o2, 1414285200
          tz.transition 2015, 3, :o3, 1427590800
          tz.transition 2015, 10, :o2, 1445734800
          tz.transition 2016, 3, :o3, 1459040400
          tz.transition 2016, 10, :o2, 1477789200
          tz.transition 2017, 3, :o3, 1490490000
          tz.transition 2017, 10, :o2, 1509238800
          tz.transition 2018, 3, :o3, 1521939600
          tz.transition 2018, 10, :o2, 1540688400
          tz.transition 2019, 3, :o3, 1553994000
          tz.transition 2019, 10, :o2, 1572138000
          tz.transition 2020, 3, :o3, 1585443600
          tz.transition 2020, 10, :o2, 1603587600
          tz.transition 2021, 3, :o3, 1616893200
          tz.transition 2021, 10, :o2, 1635642000
          tz.transition 2022, 3, :o3, 1648342800
          tz.transition 2022, 10, :o2, 1667091600
          tz.transition 2023, 3, :o3, 1679792400
          tz.transition 2023, 10, :o2, 1698541200
          tz.transition 2024, 3, :o3, 1711846800
          tz.transition 2024, 10, :o2, 1729990800
          tz.transition 2025, 3, :o3, 1743296400
          tz.transition 2025, 10, :o2, 1761440400
          tz.transition 2026, 3, :o3, 1774746000
          tz.transition 2026, 10, :o2, 1792890000
          tz.transition 2027, 3, :o3, 1806195600
          tz.transition 2027, 10, :o2, 1824944400
          tz.transition 2028, 3, :o3, 1837645200
          tz.transition 2028, 10, :o2, 1856394000
          tz.transition 2029, 3, :o3, 1869094800
          tz.transition 2029, 10, :o2, 1887843600
          tz.transition 2030, 3, :o3, 1901149200
          tz.transition 2030, 10, :o2, 1919293200
          tz.transition 2031, 3, :o3, 1932598800
          tz.transition 2031, 10, :o2, 1950742800
          tz.transition 2032, 3, :o3, 1964048400
          tz.transition 2032, 10, :o2, 1982797200
          tz.transition 2033, 3, :o3, 1995498000
          tz.transition 2033, 10, :o2, 2014246800
          tz.transition 2034, 3, :o3, 2026947600
          tz.transition 2034, 10, :o2, 2045696400
          tz.transition 2035, 3, :o3, 2058397200
          tz.transition 2035, 10, :o2, 2077146000
          tz.transition 2036, 3, :o3, 2090451600
          tz.transition 2036, 10, :o2, 2108595600
          tz.transition 2037, 3, :o3, 2121901200
          tz.transition 2037, 10, :o2, 2140045200
          tz.transition 2038, 3, :o3, 59172253, 24
          tz.transition 2038, 10, :o2, 59177461, 24
          tz.transition 2039, 3, :o3, 59180989, 24
          tz.transition 2039, 10, :o2, 59186197, 24
          tz.transition 2040, 3, :o3, 59189725, 24
          tz.transition 2040, 10, :o2, 59194933, 24
          tz.transition 2041, 3, :o3, 59198629, 24
          tz.transition 2041, 10, :o2, 59203669, 24
          tz.transition 2042, 3, :o3, 59207365, 24
          tz.transition 2042, 10, :o2, 59212405, 24
          tz.transition 2043, 3, :o3, 59216101, 24
          tz.transition 2043, 10, :o2, 59221141, 24
          tz.transition 2044, 3, :o3, 59224837, 24
          tz.transition 2044, 10, :o2, 59230045, 24
          tz.transition 2045, 3, :o3, 59233573, 24
          tz.transition 2045, 10, :o2, 59238781, 24
          tz.transition 2046, 3, :o3, 59242309, 24
          tz.transition 2046, 10, :o2, 59247517, 24
          tz.transition 2047, 3, :o3, 59251213, 24
          tz.transition 2047, 10, :o2, 59256253, 24
          tz.transition 2048, 3, :o3, 59259949, 24
          tz.transition 2048, 10, :o2, 59264989, 24
          tz.transition 2049, 3, :o3, 59268685, 24
          tz.transition 2049, 10, :o2, 59273893, 24
          tz.transition 2050, 3, :o3, 59277421, 24
          tz.transition 2050, 10, :o2, 59282629, 24
        end
      end
    end
  end
end
