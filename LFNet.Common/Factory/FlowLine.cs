using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LFNet.Factory
{
    /// <summary>
    /// 流水线
    /// </summary>
    public class FlowLine
    {
        public string Name { get; set; }

        public FlowLine()
        {

        }

        public FlowLine(string name,params IFlowLineWork[] flowLineWorks)
        {
            Name = name;
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            
        }
        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            
        }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// 流水线工作内容
    /// </summary>
    public abstract class FlowLineWork : IFlowLineWork
    {

        public abstract void Accept(object product);

        public abstract object Work(object inProduct);

        public abstract bool IsBusy { get; }
    }

    public abstract class FlowLineWork<TIn, TOut> : FlowLineWork
    {
        public override object Work(object inProduct)
        {
            return Work((TIn) inProduct);
        }

        public abstract TOut Work(TIn inProduct);

    }


    public interface IFlowLineWork
    {
        void Accept(object product);
        object Work(object inProduct);
        bool IsBusy { get; }
    }

    
}
