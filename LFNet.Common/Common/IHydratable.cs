using System.Data;

namespace LFNet.Common
{
    /// <summary>
    /// 可以将数据填充到对象的接口
    /// </summary>
    public interface IHydratable
    {
        /// <summary>
        /// 用于填充字典对象
        /// </summary>
        int KeyID { get; set; }

        /// <summary>
        /// 自定义模式将IDataReader填充到当前对象
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <remarks></remarks>
        void Fill(IDataReader dr);
    }
}