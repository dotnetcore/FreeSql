using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;

namespace FreeSql.Extensions.DynamicEntity
{
    /// <summary>
    /// 动态创建实体类型
    /// </summary>
    public class DynamicCompileBuilder
    {
        private string _className = string.Empty;
        private Attribute[] _tableAttributes = null;
        private List<DynamicPropertyInfo> _properties = new List<DynamicPropertyInfo>();
        private Type _superClass = null;

        /// <summary>
        /// 配置Class
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="attributes">类标记的特性[Table(Name = "xxx")] [Index(xxxx)]</param>
        /// <returns></returns>
        public DynamicCompileBuilder Class(string className, params Attribute[] attributes)
        {
            _className = className;
            _tableAttributes = attributes;
            return this;
        }

        /// <summary>
        /// 配置属性
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="propertyType">属性类型</param>
        /// <param name="attributes">属性标记的特性-支持多个</param>
        /// <returns></returns>
        public DynamicCompileBuilder Property(string propertyName, Type propertyType, params Attribute[] attributes)
        {
            _properties.Add(new DynamicPropertyInfo()
            {
                PropertyName = propertyName,
                PropertyType = propertyType,
                Attributes = attributes
            });
            return this;
        }

        /// <summary>
        /// 配置父类
        /// </summary>
        /// <param name="superClass">父类类型</param>
        /// <returns></returns>
        public DynamicCompileBuilder Extend(Type superClass)
        {
            _superClass = superClass;
            return this;
        }

        private void SetTableAttribute(ref TypeBuilder typeBuilder)
        {
            if (_tableAttributes == null) return;

            foreach (var tableAttribute in _tableAttributes)
            {
                var propertyValues = new ArrayList();

                if (tableAttribute == null) continue;

                var classCtorInfo = tableAttribute.GetType().GetConstructor(new Type[] { });

                var propertyInfos = tableAttribute.GetType().GetProperties().Where(p => p.CanWrite == true).ToArray();

                foreach (var propertyInfo in propertyInfos)
                    propertyValues.Add(propertyInfo.GetValue(tableAttribute));

                //可能存在有参构造
                if (classCtorInfo == null)
                {
                    var constructorTypes = propertyInfos.Select(p => p.PropertyType);
                    classCtorInfo = tableAttribute.GetType().GetConstructor(constructorTypes.ToArray());
                    var customAttributeBuilder = new CustomAttributeBuilder(classCtorInfo, propertyValues.ToArray());
                    typeBuilder.SetCustomAttribute(customAttributeBuilder);
                }
                else
                {
                    var customAttributeBuilder = new CustomAttributeBuilder(classCtorInfo, new object[0], propertyInfos,
                        propertyValues.ToArray());
                    typeBuilder.SetCustomAttribute(customAttributeBuilder);
                }
            }
        }

        private void SetPropertys(ref TypeBuilder typeBuilder)
        {
            foreach (var pinfo in _properties)
            {
                if (pinfo == null)
                    continue;
                var propertyName = pinfo.PropertyName;
                var propertyType = pinfo.PropertyType;
                //设置字段
                var field = typeBuilder.DefineField($"_{FirstCharToLower(propertyName)}", propertyType,
                    FieldAttributes.Private);
                var firstCharToUpper = FirstCharToUpper(propertyName);
                //设置属性方法
                var methodGet = typeBuilder.DefineMethod($"Get{firstCharToUpper}", MethodAttributes.Public,
                    propertyType, null);
                var methodSet = typeBuilder.DefineMethod($"Set{firstCharToUpper}", MethodAttributes.Public, null,
                    new Type[] { propertyType });

                var ilOfGet = methodGet.GetILGenerator();
                ilOfGet.Emit(OpCodes.Ldarg_0);
                ilOfGet.Emit(OpCodes.Ldfld, field);
                ilOfGet.Emit(OpCodes.Ret);

                var ilOfSet = methodSet.GetILGenerator();
                ilOfSet.Emit(OpCodes.Ldarg_0);
                ilOfSet.Emit(OpCodes.Ldarg_1);
                ilOfSet.Emit(OpCodes.Stfld, field);
                ilOfSet.Emit(OpCodes.Ret);

                //设置属性
                var propertyBuilder =
                    typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);
                propertyBuilder.SetGetMethod(methodGet);
                propertyBuilder.SetSetMethod(methodSet);

                foreach (var pinfoAttribute in pinfo.Attributes)
                {
                    //设置特性
                    SetPropertyAttribute(ref propertyBuilder, pinfoAttribute);
                }
            }
        }

        private void SetPropertyAttribute<T>(ref PropertyBuilder propertyBuilder, T tAttribute)
        {
            if (tAttribute == null) return;

            var propertyInfos = tAttribute.GetType().GetProperties().Where(p => p.CanWrite == true).ToArray();
            var constructor = tAttribute.GetType().GetConstructor(new Type[] { });
            var propertyValues = new ArrayList();
            foreach (var propertyInfo in propertyInfos)
                propertyValues.Add(propertyInfo.GetValue(tAttribute));

            var customAttributeBuilder =
                new CustomAttributeBuilder(constructor, new object[0], propertyInfos, propertyValues.ToArray());
            propertyBuilder.SetCustomAttribute(customAttributeBuilder);
        }

        /// <summary>
        /// Emit动态创建出Class - Type
        /// </summary>
        /// <returns></returns>
        public Type Build()
        {
            //初始化AssemblyName的一个实例
            var assemblyName = new AssemblyName("FreeSql.DynamicCompileBuilder");
            //设置程序集的名称
            var defineDynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            //动态在程序集内创建一个模块
            var defineDynamicModule =
                defineDynamicAssembly.DefineDynamicModule("FreeSql.DynamicCompileBuilder.Dynamics");
            //动态的在模块内创建一个类
            var typeBuilder =
                defineDynamicModule.DefineType(_className, TypeAttributes.Public | TypeAttributes.Class, _superClass);

            //设置TableAttribute
            SetTableAttribute(ref typeBuilder);

            //设置属性
            SetPropertys(ref typeBuilder);

            //创建类的Type对象
            return typeBuilder.CreateType();
        }

        /// <summary>
        /// 首字母小写
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            string str = input.First().ToString().ToLower() + input.Substring(1);
            return str;
        }

        /// <summary>
        /// 首字母大写
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            string str = input.First().ToString().ToUpper() + input.Substring(1);
            return str;
        }

        private static string Md5Encryption(string inputStr)
        {
            var result = string.Empty;
            //32位大写
            using (var md5 = MD5.Create())
            {
                var resultBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(inputStr));
                result = BitConverter.ToString(resultBytes);
            }

            return result;
        }

        class DynamicPropertyInfo
        {
            public string PropertyName { get; set; } = string.Empty;
            public Type PropertyType { get; set; }
            public Attribute[] Attributes { get; set; }
        }
    }
}