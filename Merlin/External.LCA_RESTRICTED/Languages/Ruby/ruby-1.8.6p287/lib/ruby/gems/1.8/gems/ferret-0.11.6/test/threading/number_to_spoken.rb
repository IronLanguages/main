# Author: Matthew D Moss
#
# Writtern for ruby quiz #25
#
class JapaneseTranslator
    # My knowledge of counting Japanese is limited, so this may not
    # be entirely correct; in particular, I don't know what rules
    # to follow after 'hyaku man' (1,000,000).
    # I also combine a digit with its group, such as 'gohyaku' rather
    # than 'go hyaku'; I just like reading it better that way.

    DIGITS = %w(zero ichi ni san yon go roku nana hachi kyu)
    GROUPS = %w(nothingtoseeheremovealong ju hyaku sen)
    MAN = 10000

    def to_spoken(val)
        case val <=> 0
        when -1
            '- ' + to_spoken(-val)
        when 0
            DIGITS[0]
        else
            group(val, 0)
        end
    end

    private

    def group(val, level)
        if val >= MAN
            group(val / MAN, 0) + 'man ' + group(val % MAN, 0)
        else
            case val
            when 0
                ''
            when 1
                level == 0 ? DIGITS[val] : GROUPS[level]
            when 2...10
                DIGITS[val] + (GROUPS[level] if level > 0).to_s
            else
                group(val / 10, level+1) + ' ' + group(val % 10, level)
            end
        end
    end
end


class USEnglishTranslator
    # Formal, US English. Optional 'and'. Will not produce things
    # such as 'twelve hundred' but rather 'one thousand two hundred'.
    # The use of 'and' is incomplete; it is sometimes missed.

    DIGITS = %w(zero one two three four five six seven eight nine)
    TEENS  = %w(ten eleven twelve thirteen fourteen fifteen sixteen
                seventeen eighteen nineteen)
    TENS   = %w(hello world twenty thirty forty fifty sixty seventy
                eighty ninety)
    GROUPS = %w(thousand million billion trillion quadrillion
                quintillion sextillion septillion octillion nonillion
                decillion)
    K = 1000

    def initialize(conjunction = true)
        @conjunction = conjunction
    end

    def to_spoken(val)
        case val <=> 0
        when -1
            'negative ' + to_spoken(-val)
        when 0
            DIGITS[0]
        else
            group(val, 0).flatten.join(' ')
        end
    end

    private

    def group(val, level)
        x = group(val / K, level + 1) << GROUPS[level] if val >= K
        x.to_a << under_1000(val % K, level)
    end

    def under_1000(val, level)
        x = [DIGITS[val / 100]] << 'hundred' if val >= 100
        x.to_a << under_100(val % 100, (level == 0 and not x.nil?))
    end

    def under_100(val, junction)
        x = [('and' if @conjunction and junction)]    # wyf?
        case val
        when 0
            []
        when 1...10
            x << DIGITS[val]
        when 10...20
            x << TEENS[val - 10]
        else
            d = val % 10
            x << (TENS[val / 10] + ('-' + DIGITS[d] if d != 0).to_s)
        end
    end
end


class Integer
    def to_spoken(translator = USEnglishTranslator.new)
        translator.to_spoken(self).squeeze(' ').strip
    end
end

if $0 == __FILE__
    SAMPLES = [ 0, 1, 2, 5, 10, 11, 14, 18, 20, 21, 29, 33, 42, 50, 87, 99,
                100, 101, 110, 167, 199, 200, 201, 276, 300, 314, 500, 610,
                1000, 1039, 1347, 2309, 3098, 23501, 32767, 70000, 5480283,
                2435489238, 234100090000, -42, -2001 ]

    TRANSLATORS = { 'US English' => USEnglishTranslator.new,
                    'Japanese'   => JapaneseTranslator.new }


    # main
    TRANSLATORS.each do |lang, translator|
        puts
        puts lang
        puts '-' * lang.length
        SAMPLES.each do |val|
            puts "%12d => %s" % [val, val.to_spoken(translator)]
        end
    end
end
