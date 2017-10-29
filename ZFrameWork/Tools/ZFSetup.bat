set zf_dir="..\Assets\ZFrame\"
set plugins_dir="..\Assets\Plugins\"

mkdir %plugins_dir%
copy %zf_dir%ThirdParty\Plugins\* %plugins_dir%
%zf_dir%Tools\ZFTemplate\ZFTemplateGenerator.exe ../ GenAll ZFConfig.json
pause