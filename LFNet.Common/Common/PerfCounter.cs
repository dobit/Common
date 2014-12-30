using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace LFNet.Common
{
    internal class PerfCounter
    {
        #region ˽�б���

        private readonly long _freq;
        private long _startTime, _stopTime;

        #endregion

        #region ˽�з���

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        #endregion

        #region ���췽��

        /// <summary>
        /// �������ʵ��
        /// </summary>
        /// <param name="startTimer">�Ƿ�����ִ��Start()����</param>
        public PerfCounter(bool startTimer)
        {
            _startTime = 0;
            _stopTime = 0;

            if (QueryPerformanceFrequency(out _freq) == false)
            {
                // ��֧�ָ߾��ȼ�ʱ
                throw new Win32Exception();
            }

            if (startTimer)
                Start();
        }

        #endregion

        #region ���з���

        /// <summary>
        /// ֹͣ����
        /// </summary>
        public void Stop()
        {
            QueryPerformanceCounter(out _stopTime);
        }

        /// <summary>
        /// ��ʼ��ʱ
        /// </summary>
        public void Start()
        {
            Thread.Sleep(0);
            QueryPerformanceCounter(out _startTime);
        }

        #endregion

        #region ����

        /// <summary>
        /// ��������ʱ��
        /// </summary>
        /// <value>����ʱ��</value>
        public double Duration
        {
            get { return (_stopTime - _startTime)/(double) _freq; }
        }

        #endregion
    }
}