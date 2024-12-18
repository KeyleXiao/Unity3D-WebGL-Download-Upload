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