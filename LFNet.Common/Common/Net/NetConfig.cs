using LFNet.Configuration;

namespace LFNet.Common.Net
{
    public class NetConfig:BaseConfig<NetConfig>
    {
        private string _tempPath;//= System.IO.Path.GetTempPath();

        /// <summary>
        /// ��ʱĿ¼
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
        /// �����ļ�
        /// </summary>
        public bool UseCache { get; set; }

        private bool _checkUpdate=true;

        /// <summary>
        /// �����£������� http������������Ƿ���� Ĭ��ȥ����������
        /// </summary>
        public bool CheckUpdate
        {
            get { return _checkUpdate; }
            set { _checkUpdate = value; }
        }

        private int _memorySize=512*1024;//512K

        /// <summary>
        /// �ڴ��ļ���С����http��Ӧ�ļ��������ֵʱ��д���ļ���
        /// </summary>
        public int MemoryFileSize
        {
            get { return _memorySize; }
            set { _memorySize = value; }
        }
    }
}