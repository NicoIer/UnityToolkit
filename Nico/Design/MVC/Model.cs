using System;
using System.Collections.Generic;

namespace Nico.MVC
{
    public interface IModel
    {
    }

    public static class ModelManager
    {
        private static readonly Dictionary<Type, IModel> _modelDic = new Dictionary<Type, IModel>();

        public static T Get<T>() where T : IModel, new()
        {
            if (!_modelDic.ContainsKey(typeof(T)))
            {
                _modelDic.Add(typeof(T), new T());
            }

            return (T)_modelDic[typeof(T)];
        }

        public static void Register<T>(T model) where T : IModel
        {
            if (!_modelDic.ContainsKey(typeof(T)))
            {
                _modelDic.Add(typeof(T), model);
            }
        }
    }
}