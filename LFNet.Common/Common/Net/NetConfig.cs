using LFNet.Configuration;

namespace LFNet.Common.Net
{
    public class NetConfig:BaseConfig<NetConfig>
    {
        private string _tempPath;//= System.IO.Path.GetTempPath();

        /// <summary>
        /// 临时目录
        /// </summary>
        public string TempPath
        {
            get
            {
                if (string.IsNullOrEmpty(_tempPath))
                    _tempPath=global::System.IO.Path.GetTempPath();
                return _tempPath;
            }
            set { _tempPath = value; }
        }

        /// <summary>
        /// 缓存文件
        /// </summary>
        public bool UseCache { get; set; }

        private bool _checkUpdate=true;

        /// <summary>
        /// 检查更新，即发送 http请求检查服务器是否更新 默认去服务器请求
        /// </summary>
        public bool CheckUpdate
        {
            get { return _checkUpdate; }
            set { _checkUpdate = value; }
        }

        private int _memorySize=512*1024;//512K

        /// <summary>
        /// 内存文件大小，当http响应文件大于这个值时将写入文件中
        /// </summary>
        public int MemoryFileSize
        {
            get { return _memorySize; }
            set { _memorySize = value; }
        }
    }
}