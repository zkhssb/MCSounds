# MCSounds

![image.png](https://ncstatic.clewm.net/free/2023/0613/10/3331b15e3093fba98e067303db99a737.png)

> .NET7.0 控制台应用程序  
> 用来获取 Minecraft 音频文件的程序,需要[FFmpeg]("https://github.com/BtbN/FFmpeg-Builds/releases")的支持  
> 原理是在 minecraft.fandom.com 搜索并寻找页面的音频  
> 仅支持 Windows 平台 (Linux 没测试过,不知道环境变量是否通用)

## 如何使用

- 1.下载并解压 FFmpeg
  ![image.png](https://ncstatic.clewm.net/free/2023/0613/10/f30241768ff42b08beed9b38f1143a27.png)
- 2.添加 FFmpeg 的 Bin 目录到系统环境变量(PATH)
  ![image.png](https://ncstatic.clewm.net/free/2023/0613/10/a4aeb119bf236dc2bbba2245ea59567a.png)
- 3.打开程序开始使用

### 教程

<details>
  <summary>1.开始</summary>

输入要搜索的文本  
比如我现在要搜索村民的音效,我就搜索`村民`  
![image.png](https://ncstatic.clewm.net/free/2023/0613/10/9024db5a93a44ba62ab691d6f138792a.png)

  </details>

<details>
  <summary>2.选择搜索结果</summary>

操作如下

- 嗯? 输入数字就好了
- 按下"."退出

![使用数字](https://ncstatic.clewm.net/free/2023/0613/10/70c84715eebd0bb49eb932dd9102fb47.png)

</details>

<details>
  <summary>3.选择音效</summary>

操作如下

- 下(向下)
- 上(向上)
- 回车(确认)
- 退格(返回)

![控制](https://ncstatic.clewm.net/free/2023/0613/10/28f41520f235d5d5871373ebe3104f4b.png)

</details>

<details>
  <summary>4.播放或保存</summary>

使用回车选中一个音频以后 可以选择听或者保存
![image.png](https://ncstatic.clewm.net/free/2023/0613/11/67414394868aa54b719c3c40b6e45204.png)

<details>
  <summary>预览音频 (使用FFPlay播放)</summary>
  
  直接输入数字ID可以预览音频 (使用FFPlay播放)
![image.png](https://ncstatic.clewm.net/free/2023/0613/10/77d5f361e52304eec2691066573044af.png)
</details>

<details>
  <summary>下载音频</summary>
  
  输入 s \<id\> \<id\> \<id\> .... 可以下载多个ID   
  如 输入 ```s 0 1 2``` 就可以下载 0 1 2号音频

![image.png](https://ncstatic.clewm.net/free/2023/0613/10/99d581abba93626f9db47cf5e4e1fbf6.png)

会使用 ffmpeg 自动转换为 MP3 格式

如果不喜欢可以修改
Program.ShowSound

```C#
// FFmpegUtil.SaveSound(downSound.Value, savePath, "mp3")
FFmpegUtil.SaveSound(downSound.Value, savePath, null)
```

![image.png](https://ncstatic.clewm.net/free/2023/0613/10/0be8fca0760127bd4fca8fffdc5497ca.png)

</details>

</details>

<details>
  <summary>番外(选择Fandom语言)</summary>

写之前想到了多语言支持(但是想来想去可能也没啥人会用到这个工具,动态切换就咕咕咕了)  
这个功能可以在 Program 里初始化 FandomClient 里设置

```C#
//private static FandomClient client = new(FandomLanguage.Zh);
private static FandomClient client = new(FandomLanguage.En);
```

当然,切换什么语言就得用什么语言来搜索

![image.png](https://ncstatic.clewm.net/free/2023/0613/10/719e087fbeb7d6aa056b200a5120091b.png)

![image.png](https://ncstatic.clewm.net/free/2023/0613/10/a454a4e861a5074c3e823e6a8758f0e3.png)

![image.png](https://ncstatic.clewm.net/free/2023/0613/10/2281b934a7fd60ff4e6c28647c603046.png)

</details>

## 使用到的开源库

| 名称           | 描述            | 地址                                                           |
| -------------- | --------------- | -------------------------------------------------------------- |
| RestSharp      | Http 客户端实现 | [Github]("https://github.com/restsharp/RestSharp")             |
| AngleSharp     | Html 代码解析   | [Github]("https://github.com/AngleSharp/AngleSharp")           |
| ColoredConsole | 控制台彩色输出  | [Github]("https://github.com/colored-console/colored-console") |
