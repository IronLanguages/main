# Log4r can be configured using YAML. This example uses log4r_yaml.yaml

$: << File.join('..','src') # path if log4r is not installed
require 'log4r'
require 'log4r/yamlconfigurator'
# we use various outputters, so require them, otherwise config chokes
require 'log4r/outputter/datefileoutputter'
require 'log4r/outputter/emailoutputter'
include Log4r

cfg = YamlConfigurator # shorthand
cfg['HOME'] = '.'      # the only parameter in the YAML, our HOME directory

# load the YAML file with this
cfg.load_yaml_file('log4r_yaml.yaml')

# Method to log each of the custom levels
def do_logging(log)
  log.deb "This is DEB"
  log.inf "This is INF"
  log.prt "This is PRT"
  log.wrn "This is WRN"
  log.err "This is ERR"
  log.fat "This is FAT"
end

# turn off the email outputter
Outputter['email'].level = OFF
# the other two outputters log to stderr and a timestamped file in ./logs
do_logging( Logger['mylogger'])
