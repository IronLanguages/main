require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module America
      module Asuncion
        include TimezoneDefinition
        
        timezone 'America/Asuncion' do |tz|
          tz.offset :o0, -13840, 0, :LMT
          tz.offset :o1, -13840, 0, :AMT
          tz.offset :o2, -14400, 0, :PYT
          tz.offset :o3, -10800, 0, :PYT
          tz.offset :o4, -14400, 3600, :PYST
          
          tz.transition 1890, 1, :o1, 2604278153, 1080
          tz.transition 1931, 10, :o2, 2620754633, 1080
          tz.transition 1972, 10, :o3, 86760000
          tz.transition 1974, 4, :o2, 134017200
          tz.transition 1975, 10, :o4, 181368000
          tz.transition 1976, 3, :o2, 194497200
          tz.transition 1976, 10, :o4, 212990400
          tz.transition 1977, 3, :o2, 226033200
          tz.transition 1977, 10, :o4, 244526400
          tz.transition 1978, 3, :o2, 257569200
          tz.transition 1978, 10, :o4, 276062400
          tz.transition 1979, 4, :o2, 291783600
          tz.transition 1979, 10, :o4, 307598400
          tz.transition 1980, 4, :o2, 323406000
          tz.transition 1980, 10, :o4, 339220800
          tz.transition 1981, 4, :o2, 354942000
          tz.transition 1981, 10, :o4, 370756800
          tz.transition 1982, 4, :o2, 386478000
          tz.transition 1982, 10, :o4, 402292800
          tz.transition 1983, 4, :o2, 418014000
          tz.transition 1983, 10, :o4, 433828800
          tz.transition 1984, 4, :o2, 449636400
          tz.transition 1984, 10, :o4, 465451200
          tz.transition 1985, 4, :o2, 481172400
          tz.transition 1985, 10, :o4, 496987200
          tz.transition 1986, 4, :o2, 512708400
          tz.transition 1986, 10, :o4, 528523200
          tz.transition 1987, 4, :o2, 544244400
          tz.transition 1987, 10, :o4, 560059200
          tz.transition 1988, 4, :o2, 575866800
          tz.transition 1988, 10, :o4, 591681600
          tz.transition 1989, 4, :o2, 607402800
          tz.transition 1989, 10, :o4, 625032000
          tz.transition 1990, 4, :o2, 638938800
          tz.transition 1990, 10, :o4, 654753600
          tz.transition 1991, 4, :o2, 670474800
          tz.transition 1991, 10, :o4, 686721600
          tz.transition 1992, 3, :o2, 699418800
          tz.transition 1992, 10, :o4, 718257600
          tz.transition 1993, 3, :o2, 733546800
          tz.transition 1993, 10, :o4, 749448000
          tz.transition 1994, 2, :o2, 762318000
          tz.transition 1994, 10, :o4, 780984000
          tz.transition 1995, 2, :o2, 793767600
          tz.transition 1995, 10, :o4, 812520000
          tz.transition 1996, 3, :o2, 825649200
          tz.transition 1996, 10, :o4, 844574400
          tz.transition 1997, 2, :o2, 856666800
          tz.transition 1997, 10, :o4, 876024000
          tz.transition 1998, 3, :o2, 888721200
          tz.transition 1998, 10, :o4, 907473600
          tz.transition 1999, 3, :o2, 920775600
          tz.transition 1999, 10, :o4, 938923200
          tz.transition 2000, 3, :o2, 952225200
          tz.transition 2000, 10, :o4, 970372800
          tz.transition 2001, 3, :o2, 983674800
          tz.transition 2001, 10, :o4, 1002427200
          tz.transition 2002, 4, :o2, 1018148400
          tz.transition 2002, 9, :o4, 1030852800
          tz.transition 2003, 4, :o2, 1049598000
          tz.transition 2003, 9, :o4, 1062907200
          tz.transition 2004, 4, :o2, 1081047600
          tz.transition 2004, 10, :o4, 1097985600
          tz.transition 2005, 3, :o2, 1110682800
          tz.transition 2005, 10, :o4, 1129435200
          tz.transition 2006, 3, :o2, 1142132400
          tz.transition 2006, 10, :o4, 1160884800
          tz.transition 2007, 3, :o2, 1173582000
          tz.transition 2007, 10, :o4, 1192939200
          tz.transition 2008, 3, :o2, 1205031600
          tz.transition 2008, 10, :o4, 1224388800
          tz.transition 2009, 3, :o2, 1236481200
          tz.transition 2009, 10, :o4, 1255838400
          tz.transition 2010, 3, :o2, 1268535600
          tz.transition 2010, 10, :o4, 1287288000
          tz.transition 2011, 3, :o2, 1299985200
          tz.transition 2011, 10, :o4, 1318737600
          tz.transition 2012, 3, :o2, 1331434800
          tz.transition 2012, 10, :o4, 1350792000
          tz.transition 2013, 3, :o2, 1362884400
          tz.transition 2013, 10, :o4, 1382241600
          tz.transition 2014, 3, :o2, 1394334000
          tz.transition 2014, 10, :o4, 1413691200
          tz.transition 2015, 3, :o2, 1425783600
          tz.transition 2015, 10, :o4, 1445140800
          tz.transition 2016, 3, :o2, 1457838000
          tz.transition 2016, 10, :o4, 1476590400
          tz.transition 2017, 3, :o2, 1489287600
          tz.transition 2017, 10, :o4, 1508040000
          tz.transition 2018, 3, :o2, 1520737200
          tz.transition 2018, 10, :o4, 1540094400
          tz.transition 2019, 3, :o2, 1552186800
          tz.transition 2019, 10, :o4, 1571544000
          tz.transition 2020, 3, :o2, 1583636400
          tz.transition 2020, 10, :o4, 1602993600
          tz.transition 2021, 3, :o2, 1615690800
          tz.transition 2021, 10, :o4, 1634443200
          tz.transition 2022, 3, :o2, 1647140400
          tz.transition 2022, 10, :o4, 1665892800
          tz.transition 2023, 3, :o2, 1678590000
          tz.transition 2023, 10, :o4, 1697342400
          tz.transition 2024, 3, :o2, 1710039600
          tz.transition 2024, 10, :o4, 1729396800
          tz.transition 2025, 3, :o2, 1741489200
          tz.transition 2025, 10, :o4, 1760846400
          tz.transition 2026, 3, :o2, 1772938800
          tz.transition 2026, 10, :o4, 1792296000
          tz.transition 2027, 3, :o2, 1804993200
          tz.transition 2027, 10, :o4, 1823745600
          tz.transition 2028, 3, :o2, 1836442800
          tz.transition 2028, 10, :o4, 1855195200
          tz.transition 2029, 3, :o2, 1867892400
          tz.transition 2029, 10, :o4, 1887249600
          tz.transition 2030, 3, :o2, 1899342000
          tz.transition 2030, 10, :o4, 1918699200
          tz.transition 2031, 3, :o2, 1930791600
          tz.transition 2031, 10, :o4, 1950148800
          tz.transition 2032, 3, :o2, 1962846000
          tz.transition 2032, 10, :o4, 1981598400
          tz.transition 2033, 3, :o2, 1994295600
          tz.transition 2033, 10, :o4, 2013048000
          tz.transition 2034, 3, :o2, 2025745200
          tz.transition 2034, 10, :o4, 2044497600
          tz.transition 2035, 3, :o2, 2057194800
          tz.transition 2035, 10, :o4, 2076552000
          tz.transition 2036, 3, :o2, 2088644400
          tz.transition 2036, 10, :o4, 2108001600
          tz.transition 2037, 3, :o2, 2120094000
          tz.transition 2037, 10, :o4, 2139451200
          tz.transition 2038, 3, :o2, 19723973, 8
          tz.transition 2038, 10, :o4, 7397141, 3
          tz.transition 2039, 3, :o2, 19726885, 8
          tz.transition 2039, 10, :o4, 7398233, 3
          tz.transition 2040, 3, :o2, 19729797, 8
          tz.transition 2040, 10, :o4, 7399346, 3
          tz.transition 2041, 3, :o2, 19732709, 8
          tz.transition 2041, 10, :o4, 7400438, 3
          tz.transition 2042, 3, :o2, 19735621, 8
          tz.transition 2042, 10, :o4, 7401530, 3
          tz.transition 2043, 3, :o2, 19738533, 8
          tz.transition 2043, 10, :o4, 7402622, 3
          tz.transition 2044, 3, :o2, 19741501, 8
          tz.transition 2044, 10, :o4, 7403714, 3
          tz.transition 2045, 3, :o2, 19744413, 8
          tz.transition 2045, 10, :o4, 7404806, 3
          tz.transition 2046, 3, :o2, 19747325, 8
          tz.transition 2046, 10, :o4, 7405919, 3
          tz.transition 2047, 3, :o2, 19750237, 8
          tz.transition 2047, 10, :o4, 7407011, 3
          tz.transition 2048, 3, :o2, 19753149, 8
          tz.transition 2048, 10, :o4, 7408103, 3
          tz.transition 2049, 3, :o2, 19756117, 8
          tz.transition 2049, 10, :o4, 7409195, 3
          tz.transition 2050, 3, :o2, 19759029, 8
        end
      end
    end
  end
end
