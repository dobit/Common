using System.Diagnostics;

namespace LFNet.Common
{
    internal class MyProcess
    {
        public static ExitInfo Excute(string fileName, string arguments, string workingDirectory)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            //ָ�������ļ���
            processStartInfo.FileName = fileName;
            //ָ���������ļ�ʱ���������
            processStartInfo.Arguments = arguments;
            //ָ����������ģʽ������
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //ָ��ѹ���󵽴�·��
            processStartInfo.WorkingDirectory = workingDirectory;

            ExitInfo exitInfo=new ExitInfo();
           
            //�������̶���
            Process process = new Process();
            //ָ�����̶���������Ϣ����
            process.StartInfo = processStartInfo;
         
            //��������
            process.Start();
            //ָ��������������Ϊֹ
            process.WaitForExit();
           
            exitInfo.Error = process.StandardError.ReadToEnd();
            exitInfo.ExitCode = process.ExitCode;
            exitInfo.Result = process.StandardOutput.ReadToEnd();
            return exitInfo;
        }
        public class  ExitInfo
        {
            public int ExitCode;
            public string Result;
            public string Error;
        }
    }
}