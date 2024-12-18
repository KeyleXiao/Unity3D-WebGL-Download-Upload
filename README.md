# Unity3D-WebGL-Download-Upload
Unity3D WebGL Download &amp; Upload

Recently, I created a drawing board based on Unity WebGL, which involves reading and writing local resources and completing download operations after processing the data.

It includes both uploading and downloading. Even though they are all local file operations, due to the sandbox design of browsers, we cannot directly perform file operations through the IO class and must communicate with JS to exchange data.

This article will explain how to exchange data between the two and provide verified code that you can directly modify and use. Theoretically, this code can be modified to download any file, such as downloading textures. Here, a memory pointer is passed, so it is not limited to downloading images. The uploading code is similar; what is sent to Unity is a URL, which can also be replaced with any type.

It is worth noting that during the development of WebGL, you need to manually clear the cache every time you redeploy and enter the browser page for testing. Otherwise, some files will be cached, and the test will be incorrect.


最近基于unity webgl 制作了一个绘图板，其中涉及到读写本地资源，并且处理完成数据后要完成下载操作。

其中既包含了上传，也包含下载。即使都是本地的文件操作，但由于浏览器的沙盒设计，我们并不能直接通过IO类进行文件操作，必须通过与js的通信，进行数据交换。

本篇将说明如何将两者数据进行交换，并提供相应的验证过的代码，你可以直接修改并使用下面代码。
理论上来说，这个代码是可以修改为下载其他任意文件的，比如下载贴图这里传的是内存指针，所以不限于下载图片。上传的代码也类似，给unity发送的是一个url，也是替换为任意类型都可以的。

有一点比较值得注意，在WebGL的开发过程中，每次重新部署后进入浏览器页面测试，都需要手动请清理缓存。
否则会出现部分文件被缓存，测试不正确情况。


## File Download
Below is a JS encapsulation library that implements the download and upload of image files.

## 文件下载
下面是个js封装库，并且实现了对图片文件的下载与上传。

```js
mergeInto(LibraryManager.library, {
    DownloadTexture: function (dataPtr, dataLength, fileName) {
        var data = HEAPU8.subarray(dataPtr, dataPtr + dataLength);
        var fileName = UTF8ToString(fileName); 
        var blob = new Blob([data], { type: 'image/png' });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    },
	
    LoadImageFile: function () {
        var fileInput = document.createElement('input');
        fileInput.type = 'file';
        fileInput.accept = ".jpg,.png,.tga"; // 限定文件类型
		fileInput.style.position = 'fixed'; // 固定定位
        fileInput.style.left = '-100vw'; // 定位到屏幕外
        fileInput.onchange = function (e) {
		var url = URL.createObjectURL(fileInput.files[0]);
		window.unityInstance.Module.SendMessage('ImageLoader', 'OnImageLoaded', url);
        };
        document.body.appendChild(fileInput); // 添加到body，使其可以被点击
        fileInput.click(); // 模拟点击
        setTimeout(function() {
            document.body.removeChild(fileInput); // 移除元素
        }, 100); // 稍后移除，确保文件输入有足够的时间打开对话框
    }
});
```

## File Upload
Of course, there are some things to note here:

There are type restrictions on the parameters passed
string, a number, just these two, I once tried to use byte[], or send memory pointers from JS, and it prompted that the parameter type was incorrect.
Here is an excerpt from the documentation.
Where objectName is the name of an object in your scene; methodName is the name of a method in the script, currently attached to that object; value can be a string, a number, or can be empty. For example:

It is easy to obtain the browser's cache files through input, and by using the URL.createObjectURL() method, create a URL that represents the file object or Blob object given in the parameter. This method returns a DOMString that contains a unique URL pointing to the file object or Blob object specified by the parameter, and finally, by loading this URL through WebRequest, we can achieve our goal.

Here, async syntax is used. Note that if you change this part and an error occurs, it probably won't be prompted, and it will be difficult to debug. Haha


## 文件上传
当然这里也有一些值得注意的东西：
### 传入的参数有类型限制
 string, a number 就这俩，之前我一度尝试使用byte[]，或者从js发送内存指针都提示参数类型不正确。
下面是文档摘录。
`Where objectName is the name of an object in your scene; methodName is the name of a method in the script, currently attached to that object; value can be a string, a number, or can be empty. For example:`

在input很容易获取浏览器的缓存文件，通过URL.createObjectURL() 方法，创建一个表示参数中给定文件对象或 Blob 对象的 URL。这个方法返回的是一个 DOMString，它包含了一个唯一的 URL，这个 URL 指向由参数指定的文件对象或 Blob 对象，最后通过WebRequest加载这个URL就可以达到我们的目的。

这里使用了async语法，注意如果你改动这部分报错了，大概不会提示出来，会很难查bug哦。哈哈

![](https://i.vrast.cn/i/2024/12/17/rfanrx.webp)

``` C#
    public async void OnImageLoadedAsync(string url)
    {
        // 使用 UnityWebRequest 来下载图片数据
        UnityWebRequest request = UnityWebRequest.Get(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        request.downloadHandler = texDl;

        // 发送请求
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var imageObject = GameObject.Find("ImageObject");
            var sprite = Sprite.Create(texDl.texture, new Rect(0, 0, texDl.texture.width, texDl.texture.height), new Vector2(0.5f, 0.5f));
            imageObject.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
        }
        else
        {
            Debug.LogError("Error downloading image: " + request.error);
        }

        request.Dispose();
    }

```

## Need to obtain the running instance
The following is an excerpt from the official documentation, which roughly means that if JS wants to call Unity's logic, it needs to find the corresponding created instance.

### 需要获取运行实例
下面摘录自官方文档，大致意思是js要调用unity的逻辑需要找到对应已创建的实例。

```
However, if you are planning to call the internal JavaScript functions from the global scope of the embedding page, you should always assume that there are multiple builds embedded on the page, so you should explicitly specify which build you are referencing to. For example, if your game has been instantiated as:
var gameInstance = UnityLoader.instantiate("gameContainer", "Build/build.json", {onProgress: UnityProgress});
Then you can send a message to the build using gameInstance.SendMessage(), or access the build Module object like this gameInstance.Module.

```

The following code is the generated index.html, where we directly save the Unity instance to the current window window.unityInstance = unityInstance; for global calls.

下面代码是生成后的index.html，我们直接将unity的实例保存到当前window中` window.unityInstance = unityInstance;` 方便全局调用。

``` js
      var script = document.createElement("script");
      script.src = loaderUrl;
      script.onload = () => {
        createUnityInstance(canvas, config, (progress) => {
          document.querySelector("#unity-progress-bar-full").style.width = 100 * progress + "%";
              }).then((unityInstance) => {
		window.unityInstance = unityInstance;
                document.querySelector("#unity-loading-bar").style.display = "none";
                document.querySelector("#unity-fullscreen-button").onclick = () => {
                  unityInstance.SetFullscreen(1);
                };

              }).catch((message) => {
                alert(message);
              });
            };

      document.body.appendChild(script);
```


For reference, see the official documentation 
参考官方文档 https://docs.unity3d.com/2017.3/Documentation/Manual/webgl-interactingwithbrowserscripting.html
