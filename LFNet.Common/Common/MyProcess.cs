using System.Diagnostics;

namespace LFNet.Common
{
    internal class MyProcess
    {
        public static ExitInfo Excute(string fileName, string arguments, string workingDirectory)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            //指定启动文件名
            processStartInfo.FileName = fileName;
            //指定启动该文件时的命令、参数
            processStartInfo.Arguments = arguments;
            //指定启动窗口模式：隐藏
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //指定压缩后到达路径
            processStartInfo.WorkingDirectory = workingDirectory;

            ExitInfo exitInfo=new ExitInfo();
           
            //创建进程对象
            Process process = new Process();
            //指定进程对象启动信息对象
            process.StartInfo = processStartInfo;
         
            //启动进程
            process.Start();
            //指定进程自行退行为止
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