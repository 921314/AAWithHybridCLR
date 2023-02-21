# AAWithHybridCLR
Addressables+HybridCLR的例子
### 例子说明
因为官方例子是基于asset bundle的，因为项目用到AA，所以建了这么个例子。
Addressables + HybridCLR。windows，web，和微信小游戏平台**热更代码+资源**测试通过。

### 运行环境
- Unity版本：2021.3
- 安装Addressables插件，PM管理器安装，1.20版本
- 安装HybirdCLR插件（>= 1.1.20），地址：https://focus-creative-games.github.io/hybridclr/install/

### 资源更新步骤
1. dll生成步骤可以看官方文档
2. 步是编译DLL之后，修改脚本把dll.bytes复制到Res/Dlls/目录下。(手动也行)
3. 然后添加dlls到AAGroup。
4. 打包目标平台资源，web需要在本地架一个服务器，用nginx，把文件放在web目录也行。

### 注意点
1. 因为工程太大了，所以HybridCLR插件不上传了。需要自行安装。
2. 微信小游戏平台，需要修改打包脚本，把模式设置为Fast，而不是默认的。注释掉也行
3. 因为hash文件和json文件，update打包后，都是同样命名的，如果在cdn中获取，会有缓存，可以刷新文件处理，另外也可以修改Addressabes的请求地址，加个后缀，如 url + "?时间戳"
