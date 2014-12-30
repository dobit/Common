using System;
using System.Collections.Generic;
using System.Text;

namespace LFNet.Configuration
{
    /// <summary>
    /// 所有的配置类必须继承该类
    /// Id 是自增的
    /// </summary>
    public  abstract class BaseConfigInfo
    {
        private int m_Id;
        public int Id
        {
            get { return m_Id; }
            set
            {
                if(m_Id==0)
                {
                    m_Id = value;
                }else
                {
                    new Exception("Id 值不能更改");
                }
            }
        }

        public string Name { get; set; }
    }

    /// <summary>
    /// 以Id为主键的配置文件
    /// </summary>
    public abstract class IdConfigInfo
    {
        private int m_Id;
        public int Id
        {
            get { return m_Id; }
            set
            {
                if (m_Id == 0)
                {
                    m_Id = value;
                }
                else
                {
                    new Exception("Id 值不能更改");
                }
            }
        }
    }

    /// <summary>
    /// 以name为主键的配置文件
    /// </summary>
    public abstract class NameConfigInfo
    {
        public string Name { get; set; }
    }

    public class IdNameConfigInfo:IdConfigInfo
    {
        public string Name { get; set; }

    }

    public class SortConfigInfo:IdNameConfigInfo
    {
        public int Sort { get; set; }

    }

    public class ParentConfigInfo : IdNameConfigInfo
    {
        public int ParentId { get; set; }

    }

    /// <summary>
    /// 可排序的
    /// </summary>
    public  class SortParentConfigInfo:ParentConfigInfo
    {
        public int Sort { get; set; }

    }
}
