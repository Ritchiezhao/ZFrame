cd $(dirname $0)
sgaf_dir="../Assets/sgaf/"
mono ${sgaf_dir}Tools/sgaf_template/SGAFTemplateGenerator.exe ../ GenAll sgaf_config.json
