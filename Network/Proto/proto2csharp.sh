# 递归的搜索当前目录下和下一级目录下的所有proto文件，并且生成对应的C#文件(不会递归)
protoRoot="./"
# Assets/Plugins/Unity-Toolkits/Network/Protobuf/
# Protobuf/protoc.exe
protocPath="../Protobuf/protoc"


# 反引号：告诉shell将其中的命令使用命令输出结果代替
# 使用ls命令遍历文件及文件夹(TopOnly，没有路径)
# shellcheck disable=SC2045
for name in $(ls $protoRoot)
do
	# 如果是文件夹并且存在
	# test -d 判断是否为文件夹并且存在   test -f 是否为文件并且存在
	if test -d $protoRoot/"$name"
	then
		# 遍历文件夹下的proto文件
		for filePath in "$protoRoot"/"$name"/*.proto
		do
			$protocPath --proto_path=$protoRoot/"$name" --csharp_out=$protoRoot/"$name" "$filePath"
			done
	else
    # 判断是否为proto文件
    if [[ $name =~ .proto$ ]]
    then
      $protocPath --proto_path=$protoRoot --csharp_out=$protoRoot "$name"
    fi
	fi
done
