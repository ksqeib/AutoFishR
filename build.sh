#!/bin/bash
# AutoFishR 打包脚本
# 自动编译并打包所有必要文件

# 默认参数
CONFIGURATION=${1:-Release}
OUTPUT_DIR=${2:-./publish}

echo "====================================="
echo "  AutoFishR 自动打包脚本"
echo "====================================="
echo ""

# 获取项目版本号
VERSION=$(grep '<Version>' AutoFishR.csproj | sed 's/.*<Version>\(.*\)<\/Version>.*/\1/' | tr -d '[:space:]')
if [ -z "$VERSION" ]; then
    VERSION="1.0.0"
    echo "警告: 无法获取版本号，使用默认版本: $VERSION"
else
    echo "项目版本: $VERSION"
fi

# 清理旧的发布文件
echo "清理旧的发布文件..."
if [ -d "$OUTPUT_DIR" ]; then
    rm -rf "$OUTPUT_DIR"
fi
mkdir -p "$OUTPUT_DIR"

# 编译项目
echo "开始编译项目 ($CONFIGURATION)..."
dotnet build -c "$CONFIGURATION"
if [ $? -ne 0 ]; then
    echo "编译失败"
    exit 1
fi
echo "编译成功"
echo ""

# 创建临时打包目录
TEMP_DIR="$OUTPUT_DIR/temp"
PACKAGE_DIR="$TEMP_DIR/AutoFishR"
PLUGIN_DIR="$PACKAGE_DIR/dll"
mkdir -p "$PLUGIN_DIR"

# 复制插件本体
echo "复制插件文件..."
BUILD_PATH="bin/$CONFIGURATION/net9.0"
cp "$BUILD_PATH/AutoFishR.dll" "$PLUGIN_DIR/"
echo "  AutoFishR.dll"

# 复制 YamlDotNet 依赖
YAML_PATH="$BUILD_PATH/YamlDotNet.dll"
if [ ! -f "$YAML_PATH" ]; then
    # 从NuGet包中查找
    NUGET_PATH="$HOME/.nuget/packages/yamldotnet"
    if [ -d "$NUGET_PATH" ]; then
        LATEST_VERSION=$(ls -1 "$NUGET_PATH" | sort -V | tail -1)
        for LIB_PATH in "lib/net9.0" "lib/net8.0" "lib/netstandard2.1"; do
            YAML_PATH="$NUGET_PATH/$LATEST_VERSION/$LIB_PATH/YamlDotNet.dll"
            if [ -f "$YAML_PATH" ]; then
                break
            fi
        done
    fi
fi
if [ -f "$YAML_PATH" ]; then
    cp "$YAML_PATH" "$PLUGIN_DIR/"
    echo "  YamlDotNet.dll"
else
    echo "  未找到 YamlDotNet.dll"
fi

# 复制 resource 文件夹
echo "复制 resource 文件夹..."
if [ -d "resource" ]; then
    cp -r "resource" "$PACKAGE_DIR/"
    echo "  resource/"
fi

# 复制所有 .md 文件
echo "复制文档文件..."
for md_file in *.md; do
    if [ -f "$md_file" ]; then
        cp "$md_file" "$PACKAGE_DIR/"
        echo "  $md_file"
    fi
done

# 创建 ZIP 包
echo ""
echo "创建 ZIP 包..."
ZIP_NAME="AutoFishR_v${VERSION}【自动钓鱼重制版】.zip"
ZIP_PATH="$OUTPUT_DIR/$ZIP_NAME"

# 检测可用的压缩工具
if command -v zip &> /dev/null; then
    # 使用 zip 命令
    cd "$TEMP_DIR"
    zip -r "../$ZIP_NAME" AutoFishR/ -q
    cd - > /dev/null
elif command -v powershell.exe &> /dev/null; then
    # 在Windows上使用PowerShell的Compress-Archive
    TEMP_DIR_WIN=$(cygpath -w "$TEMP_DIR/AutoFishR" 2>/dev/null || wslpath -w "$TEMP_DIR/AutoFishR" 2>/dev/null || echo "$TEMP_DIR/AutoFishR")
    ZIP_PATH_WIN=$(cygpath -w "$ZIP_PATH" 2>/dev/null || wslpath -w "$ZIP_PATH" 2>/dev/null || echo "$ZIP_PATH")
    powershell.exe -Command "Compress-Archive -Path '$TEMP_DIR_WIN' -DestinationPath '$ZIP_PATH_WIN' -Force"
else
    echo "错误: 未找到 zip 或 powershell 命令"
    echo "请安装 zip: apt-get install zip 或 yum install zip"
    rm -rf "$TEMP_DIR"
    exit 1
fi

# 清理临时目录
rm -rf "$TEMP_DIR"

# 获取文件大小
if [ -f "$ZIP_PATH" ]; then
    FILE_SIZE=$(du -h "$ZIP_PATH" | cut -f1)
else
    FILE_SIZE="未知"
fi

echo ""
echo "====================================="
echo "  打包完成"
echo "====================================="
echo ""
echo "输出文件: $ZIP_PATH"
echo "文件大小: $FILE_SIZE"
echo ""
