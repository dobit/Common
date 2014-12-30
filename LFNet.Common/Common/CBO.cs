using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace LFNet.Common
{
    /// <summary>
    /// 将Data,xml,Object之间的转换类
    /// 内有缓存，存储在静态对象
    /// </summary>
    public class CBO
    {
        #region Const

        private const string DefaultCacheByProperty = "ModuleID";
        private const int DefaultCacheTimeOut = 20;
        private const string DefaultPrimaryKey = "ItemID";
        private const string ObjectMapCacheKey = "ObjectMap_";
        //private const string schemaCacheKey = "Schema_";

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the object from reader.
        /// </summary>
        /// <param name="objType">Type of the obj.</param>
        /// <param name="dr">The dr.</param>
        /// <param name="closeReader">if set to <c>true</c> [close reader].</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static object CreateObjectFromReader(Type objType, IDataReader dr, bool closeReader)
        {
            object objObject = null;
            bool isSuccess = Null.NullBoolean;
            bool canRead = true;
            if (closeReader)
            {
                canRead = false;
                if (dr.Read())
                {
                    canRead = true;
                }
            }
            try
            {
                if (canRead)
                {
                    objObject = CreateObject(objType, false);
                    FillObjectFromReader(objObject, dr);
                }
                isSuccess = true;
            }
            finally
            {
                if ((!isSuccess))
                    closeReader = true;
                CloseDataReader(dr, closeReader);
            }
            return objObject;
        }

        /// <summary>
        /// Fills the dictionary from reader.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="keyField">The key field.</param>
        /// <param name="dr">The dr.</param>
        /// <param name="objDictionary">The obj dictionary.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static IDictionary<TKey, TValue> FillDictionaryFromReader<TKey, TValue>(string keyField, IDataReader dr,
                                                                                        IDictionary<TKey, TValue>
                                                                                            objDictionary)
        {
            TValue objObject;
            TKey keyValue = default(TKey);
            try
            {
                while (dr.Read())
                {
                    objObject = (TValue) CreateObjectFromReader(typeof (TValue), dr, false);
                    if (keyField == "KeyID" && objObject is IHydratable)
                    {
                        keyValue = (TKey) Null.SetNull(((IHydratable) objObject).KeyID, keyValue);
                    }
                    else
                    {
                        if (typeof (TKey).Name == "Int32" && dr[keyField].GetType().Name == "Decimal")
                        {
                            keyValue = (TKey) Null.SetNull(dr[keyField], keyValue);
                        }
                        else if (typeof (TKey).Name.ToLower() == "string" &&
                                 dr[keyField].GetType().Name.ToLower() == "dbnull")
                        {
                            keyValue = (TKey) Null.SetNull(dr[keyField], "");
                        }
                        else
                        {
                            keyValue = (TKey) Null.SetNull(dr[keyField], keyValue);
                        }
                    }
                    if (objObject != null)
                    {
                        objDictionary[keyValue] = objObject;
                    }
                }
            }
            finally
            {
                CloseDataReader(dr, true);
            }
            return objDictionary;
        }

        /// <summary>
        /// Fills the list from reader.
        /// </summary>
        /// <param name="objType">Type of the obj.</param>
        /// <param name="dr">The dr.</param>
        /// <param name="objList">The obj list.</param>
        /// <param name="closeReader">if set to <c>true</c> [close reader].</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static IList FillListFromReader(Type objType, IDataReader dr, IList objList, bool closeReader)
        {
            object objObject;
            bool isSuccess = Null.NullBoolean;
            try
            {
                while (dr.Read())
                {
                    objObject = CreateObjectFromReader(objType, dr, false);
                    objList.Add(objObject);
                }
                isSuccess = true;
            }
            finally
            {
                if ((!isSuccess))
                    closeReader = true;
                CloseDataReader(dr, closeReader);
            }
            return objList;
        }

        /// <summary>
        /// Fills the list from reader.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="dr">The dr.</param>
        /// <param name="objList">The obj list.</param>
        /// <param name="closeReader">if set to <c>true</c> [close reader].</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static IList<TItem> FillListFromReader<TItem>(IDataReader dr, IList<TItem> objList, bool closeReader)
        {
            TItem objObject;
            bool isSuccess = Null.NullBoolean;
            try
            {
                while (dr.Read())
                {
                    objObject = (TItem) CreateObjectFromReader(typeof (TItem), dr, false);
                    objList.Add(objObject);
                }
                isSuccess = true;
            }
            finally
            {
                if ((!isSuccess))
                    closeReader = true;
                CloseDataReader(dr, closeReader);
            }
            return objList;
        }

        /// <summary>
        /// Fills the object from reader.
        /// </summary>
        /// <param name="objObject">The obj object.</param>
        /// <param name="dr">The dr.</param>
        /// <remarks></remarks>
        private static void FillObjectFromReader(object objObject, IDataReader dr)
        {
            if (objObject is IHydratable)
            {
                var objHydratable = objObject as IHydratable;
                objHydratable.Fill(dr);
            }
            else
            {
                HydrateObject(objObject, dr);
            }
        }

        /// <summary>
        /// Hydrates the object.
        /// </summary>
        /// <param name="hydratedObject">The hydrated object.</param>
        /// <param name="dr">The dr.</param>
        /// <remarks></remarks>
        private static void HydrateObject(object hydratedObject, IDataReader dr)
        {
            PropertyInfo objPropertyInfo = null;
            Type propType = null;
            object coloumnValue;
            Type objDataType;
            int intIndex;
            ObjectMappingInfo objMappingInfo = GetObjectMapping(hydratedObject.GetType());

            for (intIndex = 0; intIndex <= dr.FieldCount - 1; intIndex++)
            {
                if (objMappingInfo.Properties.TryGetValue(dr.GetName(intIndex).ToUpperInvariant(), out objPropertyInfo))
                {
                    propType = objPropertyInfo.PropertyType;
                    if (objPropertyInfo.CanWrite)
                    {
                        coloumnValue = dr.GetValue(intIndex);
                        objDataType = coloumnValue.GetType();
                        if (coloumnValue == DBNull.Value)
                        {
                            objPropertyInfo.SetValue(hydratedObject, Null.SetNull(objPropertyInfo), null);
                        }
                        else if (propType.Equals(objDataType))
                        {
                            objPropertyInfo.SetValue(hydratedObject, coloumnValue, null);
                        }
                        else
                        {
                            if (propType.BaseType.Equals(typeof (Enum)))
                            {
                                if (Regex.IsMatch(coloumnValue.ToString(), "^\\d+$"))
                                {
                                    objPropertyInfo.SetValue(hydratedObject,
                                                             Enum.ToObject(propType, Convert.ToInt32(coloumnValue)),
                                                             null);
                                }
                                else
                                {
                                    objPropertyInfo.SetValue(hydratedObject, Enum.ToObject(propType, coloumnValue), null);
                                }
                            }
                            else if (propType == typeof (Guid))
                            {
                                objPropertyInfo.SetValue(hydratedObject,
                                                         Convert.ChangeType(new Guid(coloumnValue.ToString()), propType),
                                                         null);
                            }
                            else if (propType == typeof (Version))
                            {
                                objPropertyInfo.SetValue(hydratedObject, new Version(coloumnValue.ToString()), null);
                            }
                            else if (coloumnValue is IConvertible && propType.Name != "Nullable`1")
                            {
                                
                                objPropertyInfo.SetValue(hydratedObject, Convert.ChangeType(coloumnValue, propType),
                                                         null);
                            }
                            else if (coloumnValue is IConvertible && propType.Name == "Nullable`1")
                            {
                                NullableConverter converter = new NullableConverter(propType);
                                objPropertyInfo.SetValue(hydratedObject,converter.ConvertFrom(coloumnValue.ToString()),
                                                         null);
                            }
                            else
                            {
                                // try explicit conversion
                                objPropertyInfo.SetValue(hydratedObject, coloumnValue, null);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the cache by property.
        /// </summary>
        /// <param name="objType">Type of the obj.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static string GetCacheByProperty(Type objType)
        {
            string cacheProperty = DefaultCacheByProperty;
            return cacheProperty;
        }

        /// <summary>
        /// Gets the cache time out multiplier.
        /// </summary>
        /// <param name="objType">Type of the obj.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static int GetCacheTimeOutMultiplier(Type objType)
        {
            int cacheTimeOut = DefaultCacheTimeOut;
            return cacheTimeOut;
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <param name="objProperty">The obj property.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static string GetColumnName(PropertyInfo objProperty)
        {
            string columnName = objProperty.Name;
            return columnName;
        }

        /// <summary>
        /// Gets the object mapping.
        /// </summary>
        /// <param name="objType">Type of the obj.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static ObjectMappingInfo GetObjectMapping(Type objType)
        {
            string cacheKey = ObjectMapCacheKey + objType.FullName;
            ObjectMappingInfo objMap = DataCache.GetCache(cacheKey);
            if (objMap == null)
            {
                objMap = new ObjectMappingInfo
                             {
                                 ObjectType = objType.FullName,
                                 PrimaryKey = GetPrimaryKey(objType),
                                 TableName = GetTableName(objType, "")
                             };
                foreach (PropertyInfo objProperty in objType.GetProperties())
                {
                    objMap.Properties.Add(objProperty.Name.ToUpperInvariant(), objProperty);
                    objMap.ColumnNames.Add(objProperty.Name.ToUpperInvariant(), GetColumnName(objProperty));
                }
                DataCache.SetCache(cacheKey, objMap);
            }
            return objMap;
        }

        /// <summary>
        /// Gets the primary key.
        /// </summary>
        /// <param name="objType">Type of the obj.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static string GetPrimaryKey(Type objType)
        {
            string primaryKey = DefaultPrimaryKey;
            return primaryKey;
        }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <param name="objType">Type of the obj.</param>
        /// <param name="objectQualifier">The object qualifier.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static string GetTableName(Type objType, string objectQualifier)
        {
            string tableName = string.Empty;
            if (string.IsNullOrEmpty(tableName))
            {
                tableName = objType.Name;
                if (tableName.EndsWith("Info"))
                {
                    tableName.Replace("Info", string.Empty);
                }
            }

            tableName = objectQualifier + tableName;
            return tableName;
        }

        #endregion

        /// <summary>
        /// 对象复制
        /// </summary>
        /// <param name="objObject">The obj object.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static object CloneObject(object objObject)
        {
            Type objType = objObject.GetType();
            object objNewObject = Activator.CreateInstance(objType);
            ObjectMappingInfo objMappingInfo = GetObjectMapping(objType);
            foreach (var kvp in objMappingInfo.Properties)
            {
                PropertyInfo objProperty = kvp.Value;
                if (objProperty.CanWrite)
                {
                    var objPropertyClone = objProperty.GetValue(objObject, null) as ICloneable;
                    if (objPropertyClone == null)
                    {
                        objProperty.SetValue(objNewObject, objProperty.GetValue(objObject, null), null);
                    }
                    else
                    {
                        objProperty.SetValue(objNewObject, objPropertyClone.Clone(), null);
                    }
                    var enumerable = objProperty.GetValue(objObject, null) as IEnumerable;
                    if (enumerable != null)
                    {
                        var list = objProperty.GetValue(objNewObject, null) as IList;
                        if (list != null)
                        {
                            foreach (object obj in enumerable)
                            {
                                list.Add(CloneObject(obj));
                            }
                        }
                        var dic = objProperty.GetValue(objNewObject, null) as IDictionary;
                        if (dic != null)
                        {
                            foreach (DictionaryEntry de in enumerable)
                            {
                                dic.Add(de.Key, CloneObject(de.Value));
                            }
                        }
                    }
                }
            }
            return objNewObject;
        }

        /// <summary>
        /// 关闭IDataReader对象
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <param name="closeReader">是否关闭</param>
        /// <remarks></remarks>
        public static void CloseDataReader(IDataReader dr, bool closeReader)
        {
            if (dr != null && closeReader)
            {
                dr.Close();
            }
        }

        /// <summary>
        /// 创建一个新对象
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns></returns>
        /// <remarks></remarks>
        public static TObject CreateObject<TObject>()
        {
            return (TObject) CreateObject(typeof (TObject), false);
        }

        /// <summary>
        /// 创建一个新对象
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="initialise">是否用默认值空值初始化</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static TObject CreateObject<TObject>(bool initialise)
        {
            return (TObject) CreateObject(typeof (TObject), initialise);
        }

        /// <summary>
        /// 根据类型初始化对象
        /// </summary>
        /// <param name="objType">Type of the obj.</param>
        /// <param name="initialise">是否用默认值初始化字段</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static object CreateObject(Type objType, bool initialise)
        {
            object objObject = Activator.CreateInstance(objType);
            if (initialise)
            {
                InitializeObject(objObject);
            }
            return objObject;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TObject">类型</typeparam>
        /// <param name="fileName">文件路径</param>
        /// <returns></returns>
        public static TObject DeserializeObject<TObject>(string fileName)
        {
            return DeserializeObject<TObject>(XmlReader.Create(new FileStream(fileName, FileMode.Open, FileAccess.Read)));
        }

        /// <summary>
        /// 从xml中反序列化对象
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="document"></param>
        /// <returns></returns>
        public static TObject DeserializeObject<TObject>(XmlDocument document)
        {
            return DeserializeObject<TObject>(XmlReader.Create(new StringReader(document.OuterXml)));
        }

        /// <summary>
        /// 从流中反序列化对象
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static TObject DeserializeObject<TObject>(Stream stream)
        {
            return DeserializeObject<TObject>(XmlReader.Create(stream));
        }

        /// <summary>
        /// 从TextReader中反序列化
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static TObject DeserializeObject<TObject>(TextReader reader)
        {
            return DeserializeObject<TObject>(XmlReader.Create(reader));
        }

        /// <summary>
        /// 从xmlReader反序列化
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static TObject DeserializeObject<TObject>(XmlReader reader)
        {
            var objObject = CreateObject<TObject>(true);
            var xmlSerializableObject = objObject as IXmlSerializable;
            if (xmlSerializableObject == null)
            {
                var serializer = new XmlSerializer(objObject.GetType());
                objObject = (TObject) serializer.Deserialize(reader);
            }
            else
            {
                xmlSerializableObject.ReadXml(reader);
            }
            return objObject;
        }

        /// <summary>
        /// ArrayList
        /// </summary>
        /// <param name="dr">IDataReader</param>
        /// <param name="objType"></param>
        /// <returns></returns>
        public static ArrayList FillCollection(IDataReader dr, Type objType)
        {
            return (ArrayList) FillListFromReader(objType, dr, new ArrayList(), true);
        }

        /// <summary>
        /// Fills the collection.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <param name="objType">Type of the obj.</param>
        /// <param name="closeReader">if set to <c>true</c> [close reader].</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static ArrayList FillCollection(IDataReader dr, Type objType, bool closeReader)
        {
            return (ArrayList) FillListFromReader(objType, dr, new ArrayList(), closeReader);
        }

        /// <summary>
        /// Fills the collection.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <param name="objType">Type of the obj.</param>
        /// <param name="objToFill">The obj to fill.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static IList FillCollection(IDataReader dr, Type objType, ref IList objToFill)
        {
            return FillListFromReader(objType, dr, objToFill, true);
        }

        /// <summary>
        /// Fills the collection.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<TItem> FillCollection<TItem>(IDataReader dr)
        {
            return (List<TItem>) FillListFromReader(dr, new List<TItem>(), true);
        }

        /// <summary>
        /// Fills the collection.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="dr">The dr.</param>
        /// <param name="objToFill">The obj to fill.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static IList<TItem> FillCollection<TItem>(IDataReader dr, ref IList<TItem> objToFill)
        {
            return FillListFromReader(dr, objToFill, true);
        }

        /// <summary>
        /// Fills the collection.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="dr">The dr.</param>
        /// <param name="objToFill">The obj to fill.</param>
        /// <param name="closeReader">if set to <c>true</c> [close reader].</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static IList<TItem> FillCollection<TItem>(IDataReader dr, IList<TItem> objToFill, bool closeReader)
        {
            return FillListFromReader(dr, objToFill, closeReader);
        }

        /// <summary>
        /// Fills the collection.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <param name="objType">Type of the obj.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static ArrayList FillCollection(IDataReader dr, ref Type objType, ref int totalRecords)
        {
            var objFillCollection = (ArrayList) FillListFromReader(objType, dr, new ArrayList(), false);
            try
            {
                if (dr.NextResult())
                {
                    totalRecords = Utils.GetTotalRecords(ref dr);
                }
            }
                //catch (Exception exc)
                //{
                //    Exceptions.LogException(exc);
                //}
            finally
            {
                CloseDataReader(dr, true);
            }
            return objFillCollection;
        }

        /// <summary>
        /// Fills the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr">The dr.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<T> FillCollection<T>(IDataReader dr, ref int totalRecords)
        {
            IList<T> objFillCollection = FillCollection(dr, new List<T>(), false);
            try
            {
                if (dr.NextResult())
                {
                    totalRecords = Utils.GetTotalRecords(ref dr);
                }
            }
                //catch (Exception exc)
                //{
                //    Exceptions.LogException(exc);
                //}
            finally
            {
                CloseDataReader(dr, true);
            }
            return (List<T>) objFillCollection;
        }

        /// <summary>
        /// Fills the dictionary.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static IDictionary<int, TItem> FillDictionary<TItem>(IDataReader dr) where TItem : IHydratable
        {
            return FillDictionaryFromReader("KeyID", dr, new Dictionary<int, TItem>());
        }

        /// <summary>
        /// Fills the dictionary.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="dr">The dr.</param>
        /// <param name="objToFill">The obj to fill.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static IDictionary<int, TItem> FillDictionary<TItem>(IDataReader dr,
                                                                    ref IDictionary<int, TItem> objToFill)
            where TItem : IHydratable
        {
            return FillDictionaryFromReader("KeyID", dr, objToFill);
        }

        /// <summary>
        /// Fills the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="keyField">The key field.</param>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Dictionary<TKey, TValue> FillDictionary<TKey, TValue>(string keyField, IDataReader dr)
        {
            return (Dictionary<TKey, TValue>) FillDictionaryFromReader(keyField, dr, new Dictionary<TKey, TValue>());
        }

        /// <summary>
        /// Fills the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="keyField">The key field.</param>
        /// <param name="dr">The dr.</param>
        /// <param name="objDictionary">The obj dictionary.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Dictionary<TKey, TValue> FillDictionary<TKey, TValue>(string keyField, IDataReader dr,
                                                                            IDictionary<TKey, TValue> objDictionary)
        {
            return (Dictionary<TKey, TValue>) FillDictionaryFromReader(keyField, dr, objDictionary);
        }

        /// <summary>
        /// Fills the object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static TObject FillObject<TObject>(IDataReader dr)
        {
            return (TObject) CreateObjectFromReader(typeof (TObject), dr, true);
        }

        /// <summary>
        /// Fills the object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="dr">The dr.</param>
        /// <param name="closeReader">if set to <c>true</c> [close reader].</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static TObject FillObject<TObject>(IDataReader dr, bool closeReader)
        {
            return (TObject) CreateObjectFromReader(typeof (TObject), dr, closeReader);
        }

        /// <summary>
        /// Fills the object.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <param name="objType">Type of the obj.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static object FillObject(IDataReader dr, Type objType)
        {
            return CreateObjectFromReader(objType, dr, true);
        }

        /// <summary>
        /// Fills the object.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <param name="objType">Type of the obj.</param>
        /// <param name="closeReader">if set to <c>true</c> [close reader].</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static object FillObject(IDataReader dr, Type objType, bool closeReader)
        {
            return CreateObjectFromReader(objType, dr, closeReader);
        }

        /// <summary>
        /// Fills the queryable.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static IQueryable<TItem> FillQueryable<TItem>(IDataReader dr)
        {
            return FillListFromReader(dr, new List<TItem>(), true).AsQueryable();
        }

        /// <summary>
        /// Fills the sorted list.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="keyField">The key field.</param>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static SortedList<TKey, TValue> FillSortedList<TKey, TValue>(string keyField, IDataReader dr)
        {
            return (SortedList<TKey, TValue>) FillDictionaryFromReader(keyField, dr, new SortedList<TKey, TValue>());
        }


        //public static TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired)
        //{
        //    return DataCache.GetCachedData<TObject>(cacheItemArgs, cacheItemExpired);
        //}
        //public static TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired, bool saveInDictionary)
        //{
        //    return DataCache.GetCachedData<TObject>(cacheItemArgs, cacheItemExpired, saveInDictionary);
        //}
        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Dictionary<string, PropertyInfo> GetProperties<TObject>()
        {
            return GetObjectMapping(typeof (TObject)).Properties;
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="objType">Type of the obj.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Dictionary<string, PropertyInfo> GetProperties(Type objType)
        {
            return GetObjectMapping(objType).Properties;
        }

        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <param name="objObject">The obj object.</param>
        /// <remarks></remarks>
        public static void InitializeObject(object objObject)
        {
            foreach (PropertyInfo objPropertyInfo in GetObjectMapping(objObject.GetType()).Properties.Values)
            {
                if (objPropertyInfo.CanWrite)
                {
                    objPropertyInfo.SetValue(objObject, Null.SetNull(objPropertyInfo), null);
                }
            }
        }

        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <param name="objObject">The obj object.</param>
        /// <param name="objType">Type of the obj.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static object InitializeObject(object objObject, Type objType)
        {
            foreach (PropertyInfo objPropertyInfo in GetObjectMapping(objType).Properties.Values)
            {
                if (objPropertyInfo.CanWrite)
                {
                    objPropertyInfo.SetValue(objObject, Null.SetNull(objPropertyInfo), null);
                }
            }
            return objObject;
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="objObject">The obj object.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <remarks></remarks>
        public static void SerializeObject(object objObject, string fileName)
        {
            using (
                XmlWriter writer = XmlWriter.Create(fileName, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment)))
            {
                SerializeObject(objObject, writer);
                writer.Flush();
            }
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="objObject">The obj object.</param>
        /// <param name="document">The document.</param>
        /// <remarks></remarks>
        public static void SerializeObject(object objObject, XmlDocument document)
        {
            var sb = new StringBuilder();
            SerializeObject(objObject, XmlWriter.Create(sb, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Document)));
            document.LoadXml(sb.ToString());
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="objObject">The obj object.</param>
        /// <param name="stream">The stream.</param>
        /// <remarks></remarks>
        public static void SerializeObject(object objObject, Stream stream)
        {
            using (XmlWriter writer = XmlWriter.Create(stream, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment))
                )
            {
                SerializeObject(objObject, writer);
                writer.Flush();
            }
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="objObject">The obj object.</param>
        /// <param name="textWriter">The text writer.</param>
        /// <remarks></remarks>
        public static void SerializeObject(object objObject, TextWriter textWriter)
        {
            using (
                XmlWriter writer = XmlWriter.Create(textWriter, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment))
                )
            {
                SerializeObject(objObject, writer);
                writer.Flush();
            }
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="objObject">The obj object.</param>
        /// <param name="writer">The writer.</param>
        /// <remarks></remarks>
        public static void SerializeObject(object objObject, XmlWriter writer)
        {
            var xmlSerializableObject = objObject as IXmlSerializable;
            if (xmlSerializableObject == null)
            {
                var serializer = new XmlSerializer(objObject.GetType());
                serializer.Serialize(writer, objObject);
            }
            else
            {
                xmlSerializableObject.WriteXml(writer);
            }
        }

        /// <summary>
        /// Deserializes the settings.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="rootNode">The root node.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <remarks></remarks>
        public static void DeserializeSettings(IDictionary dictionary, XmlNode rootNode, string elementName)
        {
            foreach (XmlNode settingNode in rootNode.SelectNodes(elementName))
            {
                string sKey = XmlUtils.GetNodeValue(settingNode.CreateNavigator(), "settingname");
                string sValue = XmlUtils.GetNodeValue(settingNode.CreateNavigator(), "settingvalue");

                dictionary[sKey] = sValue;
            }
        }

        /// <summary>
        /// Iterates items in a IDictionary object and generates XML nodes
        /// </summary>
        /// <param name="dictionary">The IDictionary to iterate</param>
        /// <param name="document">The XML document the node should be added to</param>
        /// <param name="targetPath">The target path.</param>
        /// <param name="elementName">The name of the new element created</param>
        /// <remarks></remarks>
        public static void SerializeSettings(IDictionary dictionary, XmlDocument document, string targetPath,
                                             string elementName)
        {
            string sOuterElementName = elementName + "s";
            string sInnerElementName = elementName;
            XmlNode nodeSetting;
            XmlNode nodeSettings;
            XmlNode nodeSettingName;
            XmlNode nodeSettingValue;

            XmlNode targetNode = document.SelectSingleNode(targetPath);

            if (targetNode != null)
            {
                nodeSettings = targetNode.AppendChild(document.CreateElement(sOuterElementName));
                foreach (object sKey in dictionary.Keys)
                {
                    nodeSetting = nodeSettings.AppendChild(document.CreateElement(sInnerElementName));

                    nodeSettingName = nodeSetting.AppendChild(document.CreateElement("settingname"));
                    nodeSettingName.InnerText = sKey.ToString();

                    nodeSettingValue = nodeSetting.AppendChild(document.CreateElement("settingvalue"));
                    nodeSettingValue.InnerText = dictionary[sKey].ToString();
                }
            }
            else
            {
                throw new ArgumentException("Invalid Target Path");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    internal static class DataCache
    {
        private static readonly Dictionary<string, ObjectMappingInfo> Cache =
            new Dictionary<string, ObjectMappingInfo>();

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static ObjectMappingInfo GetCache(string cacheKey)
        {
            if (Cache.ContainsKey(cacheKey))
                return Cache[cacheKey];
            return null;
        }

        /// <summary>
        /// Sets the cache.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="objMap">The obj map.</param>
        /// <remarks></remarks>
        internal static void SetCache(string cacheKey, ObjectMappingInfo objMap)
        {
            if (Cache.ContainsKey(cacheKey))
                Cache[cacheKey] = objMap;
            else
                Cache.Add(cacheKey, objMap);
        }


        /// <summary>
        /// Clears the cache.
        /// </summary>
        /// <remarks></remarks>
        internal static void ClearCache()
        {
            Cache.Clear();
        }
    }
}