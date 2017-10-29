cd "`dirname \"$0\"`"
sgaf_dir="../Assets/sgaf/"
plugins_dir="../Assets/Plugins/"

mkdir -pv ${plugins_dir}
cp -R ${sgaf_dir}ThirdParty/Plugins/* ${plugins_dir}
mono ${sgaf_dir}Tools/sgaf_template/SGAFTemplateGenerator.exe ../ GenAll sgaf_config.json